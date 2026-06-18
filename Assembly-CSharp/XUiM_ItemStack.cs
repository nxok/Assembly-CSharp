using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

public class XUiM_ItemStack : XUiModel
{
	public static bool HasItemStats(ItemStack itemStack)
	{
		if (itemStack.itemValue.ItemClass == null)
		{
			return false;
		}
		if (itemStack.itemValue.ItemClass.IsBlock())
		{
			return Block.list[itemStack.itemValue.type].DisplayType != "";
		}
		return itemStack.itemValue.ItemClass.DisplayType != "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string BuffActionStrings(ItemAction itemAction, List<string> stringList)
	{
		if (itemAction.BuffActions == null || itemAction.BuffActions.Count == 0)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < itemAction.BuffActions.Count; i++)
		{
			BuffClass buff = BuffManager.GetBuff(itemAction.BuffActions[i]);
			if (buff != null && !string.IsNullOrEmpty(buff.Name))
			{
				stringList.Add(XUiM_ItemStack.StringFormatHandler(Localization.Get("lblEffect", false, null), string.Format("{0}", buff.Name)));
			}
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string getColoredItemStat(string _title, float _value)
	{
		if (_value > 0f)
		{
			return string.Format("{0}: [00ff00]+{1}[-]", _title, _value.ToCultureInvariantString());
		}
		if (_value < 0f)
		{
			return string.Format("{0}: [ff0000]{1}[-]", _title, _value.ToCultureInvariantString());
		}
		return "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string getColoredItemStatPercentage(string _title, float _value)
	{
		if (_value > 0f)
		{
			return string.Format("{0}: [00ff00]+{1}%[-]", _title, _value.ToCultureInvariantString("0.0"));
		}
		if (_value < 0f)
		{
			return string.Format("{0}: [ff0000]{1}%[-]", _title, _value.ToCultureInvariantString("0.0"));
		}
		return "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string StringFormatHandler(string title, object value)
	{
		return string.Format("{0}: [REPLACE_COLOR]{1}[-]\n", title, value);
	}

	public static string GetStatItemValueTextWithModInfo(ItemStack itemStack, EntityPlayer player, DisplayInfoEntry infoEntry)
	{
		FastTags<TagGroup.Global> fastTags = (infoEntry.TagsSet ? infoEntry.Tags : (XUiM_ItemStack.primaryFastTags | XUiM_ItemStack.physicalDamageFastTags));
		float num = 0f;
		float num2 = 0f;
		MinEventParams.CachedEventParam.ItemValue = itemStack.itemValue;
		MinEventParams.CachedEventParam.Seed = (int)itemStack.itemValue.Seed;
		if (infoEntry.CustomName == "")
		{
			num = EffectManager.GetValue(infoEntry.StatType, itemStack.itemValue, 0f, player, null, fastTags, false, false, false, false, true, 1, false, false);
			num2 = EffectManager.GetValue(infoEntry.StatType, itemStack.itemValue, 0f, player, null, fastTags, false, false, false, false, true, 1, true, false);
		}
		else
		{
			num = XUiM_ItemStack.GetCustomValue(infoEntry, itemStack.itemValue, false);
			num2 = XUiM_ItemStack.GetCustomValue(infoEntry, itemStack.itemValue, true);
		}
		XUiM_ItemStack.degradationMaxMod(infoEntry.StatType, itemStack.itemValue, player, fastTags, false, ref num);
		XUiM_ItemStack.degradationMaxMod(infoEntry.StatType, itemStack.itemValue, player, fastTags, true, ref num2);
		if (((infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent) && Mathf.Floor(num2 * 100f) != Mathf.Floor(num * 100f)) || (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Integer && Mathf.Floor(num2) != Mathf.Floor(num)))
		{
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
			{
				num *= 100f;
				num = Mathf.Floor(num);
				num2 *= 100f;
				num2 = Mathf.Floor(num2);
				if (infoEntry.ShowInverted)
				{
					num -= 100f;
					num2 -= 100f;
				}
				float num3 = num2 - num;
				bool flag = num3 > 0f;
				bool flag2 = (infoEntry.NegativePreferred ? (!flag) : flag);
				string text = ((num2 > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "");
				return string.Concat(new string[]
				{
					text,
					num2.ToString(),
					"% (",
					flag2 ? "[00FF00]" : "[FF0000]",
					flag ? "+" : "",
					num3.ToString(),
					"%[-])"
				});
			}
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1)
			{
				num2 *= 10f;
				num2 = Mathf.Floor(num2);
				num2 /= 10f;
				num *= 10f;
				num = Mathf.Floor(num);
				num /= 10f;
			}
			else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2)
			{
				num2 *= 100f;
				num2 = Mathf.Floor(num2);
				num2 /= 100f;
				num *= 100f;
				num = Mathf.Floor(num);
				num /= 100f;
			}
			else
			{
				num2 = Mathf.Floor(num2);
				num = Mathf.Floor(num);
			}
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
			{
				float num4 = num2 - num;
				bool flag3 = num4 > 0f;
				bool flag4 = (infoEntry.NegativePreferred ? (!flag3) : flag3);
				return string.Concat(new string[]
				{
					XUiM_PlayerBuffs.GetCVarValueAsTimeString(num2),
					" (",
					flag4 ? "[00FF00]" : "[FF0000]",
					flag3 ? "+" : "",
					XUiM_PlayerBuffs.GetCVarValueAsTimeString(num4),
					"[-])"
				});
			}
			if (infoEntry.ShowInverted)
			{
				num -= 1f;
				num2 -= 1f;
			}
			float num5 = num2 - num;
			bool flag5 = num5 > 0f;
			bool flag6 = (infoEntry.NegativePreferred ? (!flag5) : flag5);
			string text2 = ((num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "");
			return string.Concat(new string[]
			{
				text2,
				num2.ToString(),
				" (",
				flag6 ? "[00FF00]" : "[FF0000]",
				flag5 ? "+" : "",
				num5.ToString("0.##"),
				"[-])"
			});
		}
		else
		{
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
			{
				num2 *= 100f;
				num2 = Mathf.Floor(num2);
				if (infoEntry.ShowInverted)
				{
					num2 -= 100f;
				}
				return ((num2 > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "") + num2.ToString("0") + "%";
			}
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1)
			{
				num2 *= 10f;
				num2 = Mathf.Floor(num2);
				num2 /= 10f;
			}
			else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2)
			{
				num2 *= 100f;
				num2 = Mathf.Floor(num2);
				num2 /= 100f;
			}
			else
			{
				num2 = Mathf.Floor(num2);
			}
			if (infoEntry.ShowInverted)
			{
				num2 -= 1f;
			}
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
			{
				return XUiM_PlayerBuffs.GetCVarValueAsTimeString(num2);
			}
			return ((num2 > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "") + num2.ToString("0.##");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void degradationMaxMod(PassiveEffects _statType, ItemValue _itemValue, EntityPlayer _player, FastTags<TagGroup.Global> _tags, bool _useMods, ref float _value)
	{
		if (_statType != PassiveEffects.DegradationMax)
		{
			return;
		}
		_value = EffectManager.GetValue(PassiveEffects.DegradationMax, _itemValue, 0f, _player, null, _tags, false, false, false, false, true, 1, _useMods, false);
		_value = (float)ItemValue.ModMaxUseTimes((int)_value, _itemValue);
	}

	public static string GetStatItemValueTextWithModColoring(ItemValue itemValue, EntityPlayerLocal player, DisplayInfoEntry infoEntry)
	{
		FastTags<TagGroup.Global> fastTags = (infoEntry.TagsSet ? infoEntry.Tags : (XUiM_ItemStack.primaryFastTags | XUiM_ItemStack.physicalDamageFastTags));
		MinEventParams.CachedEventParam.ItemValue = itemValue;
		MinEventParams.CachedEventParam.Seed = (int)itemValue.Seed;
		float num;
		float num2;
		if (infoEntry.CustomName == "")
		{
			num = EffectManager.GetValue(infoEntry.StatType, itemValue, 0f, player, null, fastTags, false, false, false, false, true, 1, false, false);
			num2 = EffectManager.GetValue(infoEntry.StatType, itemValue, 0f, player, null, fastTags, false, false, false, false, true, 1, true, false);
		}
		else
		{
			num = XUiM_ItemStack.GetCustomValue(infoEntry, itemValue, false);
			num2 = XUiM_ItemStack.GetCustomValue(infoEntry, itemValue, true);
		}
		XUiM_ItemStack.degradationMaxMod(infoEntry.StatType, itemValue, player, fastTags, false, ref num);
		XUiM_ItemStack.degradationMaxMod(infoEntry.StatType, itemValue, player, fastTags, true, ref num2);
		if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Bool)
		{
			if (num != num2)
			{
				bool flag = num2 > num;
				return ((infoEntry.NegativePreferred ? (!flag) : flag) ? "[00FF00]" : "[FF0000]") + XUiM_ItemStack.ShowLocalizedBool(Convert.ToBoolean(num2)) + "%[-]";
			}
			return XUiM_ItemStack.ShowLocalizedBool(Convert.ToBoolean(num));
		}
		else
		{
			if (infoEntry.DisplayType != DisplayInfoEntry.DisplayTypes.Time)
			{
				string text2;
				if (((infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent) && Mathf.Floor(num2 * 100f) != Mathf.Floor(num * 100f)) || (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Integer && Mathf.Floor(num2) != Mathf.Floor(num)))
				{
					if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
					{
						num = Mathf.Floor(num * 100f);
						num2 = Mathf.Floor(num2 * 100f);
						if (infoEntry.ShowInverted)
						{
							num -= 100f;
							num2 -= 100f;
						}
						bool flag2 = num2 - num > 0f;
						bool flag3 = (infoEntry.NegativePreferred ? (!flag2) : flag2);
						string text = ((num2 > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "");
						text2 = (flag3 ? "[00FF00]" : "[FF0000]") + text + num2.ToString() + "%[-]";
					}
					else
					{
						if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1)
						{
							num2 = Mathf.Floor(num2 * 10f) / 10f;
							num = Mathf.Floor(num * 10f) / 10f;
						}
						else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2)
						{
							num2 = Mathf.Floor(num2 * 100f) / 100f;
							num = Mathf.Floor(num * 100f) / 100f;
						}
						else
						{
							num2 = Mathf.Floor(num2);
							num = Mathf.Floor(num);
						}
						if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
						{
							bool flag4 = num2 - num > 0f;
							text2 = ((infoEntry.NegativePreferred ? (!flag4) : flag4) ? "[00FF00]" : "[FF0000]") + XUiM_PlayerBuffs.GetCVarValueAsTimeString(num2) + "[-]";
						}
						else
						{
							if (infoEntry.ShowInverted)
							{
								num -= 1f;
								num2 -= 1f;
							}
							bool flag5 = num2 - num > 0f;
							bool flag6 = (infoEntry.NegativePreferred ? (!flag5) : flag5);
							string text3 = ((num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "");
							text2 = (flag6 ? "[00FF00]" : "[FF0000]") + text3 + num2.ToString() + "[-]";
						}
					}
				}
				else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
				{
					num2 = Mathf.Floor(num2 * 100f);
					if (infoEntry.ShowInverted)
					{
						num2 -= 100f;
					}
					text2 = ((num2 > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "") + num2.ToString("0") + "%";
				}
				else
				{
					if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1)
					{
						num2 = Mathf.Floor(num2 * 10f) / 10f;
					}
					else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2)
					{
						num2 = Mathf.Floor(num2 * 100f) / 100f;
					}
					else
					{
						num2 = Mathf.Floor(num2);
					}
					if (infoEntry.ShowInverted)
					{
						num2 -= 1f;
					}
					if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
					{
						text2 = XUiM_PlayerBuffs.GetCVarValueAsTimeString(num2);
					}
					else
					{
						text2 = num2.ToString("0.##");
						if (num2 > 0f && infoEntry.DisplayLeadingPlus)
						{
							text2 = "+" + text2;
						}
					}
				}
				if (itemValue.GetStatPercent(infoEntry.StatType, true) != 1f)
				{
					text2 = "[sp=ui_stat]" + text2;
				}
				if (InputUtils.ShiftKeyPressed || LocalPlayerUI.GetUIForPrimaryPlayer().playerInput.GUIActions.Inspect.IsPressed || player.PlayerUI.windowManager.IsWindowOpen("combine"))
				{
					float statPercent = itemValue.GetStatPercent(infoEntry.StatType, false);
					if (statPercent != 1f)
					{
						text2 += string.Format(" [00f0f0]({0:0.#}%)", (statPercent - 1f) * 100f);
					}
				}
				return text2;
			}
			if (num != num2)
			{
				if (infoEntry.ShowInverted)
				{
					num -= 1f;
					num2 -= 1f;
				}
				bool flag7 = num2 - num > 0f;
				return ((infoEntry.NegativePreferred ? (!flag7) : flag7) ? "[00FF00]" : "[FF0000]") + XUiM_PlayerBuffs.GetCVarValueAsTimeString(num) + "[-])";
			}
			return XUiM_PlayerBuffs.GetCVarValueAsTimeString(num);
		}
	}

	public static string GetStatItemValueTextWithCompareInfo(ItemValue itemValue, ItemValue compareValue, EntityPlayerLocal player, DisplayInfoEntry infoEntry, bool flipCompare = false, bool useMods = true)
	{
		FastTags<TagGroup.Global> fastTags = (infoEntry.TagsSet ? infoEntry.Tags : (XUiM_ItemStack.primaryFastTags | XUiM_ItemStack.physicalDamageFastTags));
		float num = 0f;
		float num2 = 0f;
		if (compareValue.IsEmpty() || compareValue.Equals(itemValue))
		{
			return XUiM_ItemStack.GetStatItemValueTextWithModColoring(itemValue, player, infoEntry);
		}
		if (infoEntry.CustomName == "")
		{
			MinEventParams.CachedEventParam.ItemValue = itemValue;
			MinEventParams.CachedEventParam.Seed = (int)itemValue.Seed;
			num = EffectManager.GetValue(infoEntry.StatType, itemValue, 0f, player, null, fastTags, false, false, false, false, true, 1, useMods, false);
			num2 = EffectManager.GetValue(infoEntry.StatType, compareValue, 0f, player, null, fastTags, false, false, false, false, true, 1, useMods, false);
		}
		else
		{
			num = XUiM_ItemStack.GetCustomValue(infoEntry, itemValue, useMods);
			num2 = XUiM_ItemStack.GetCustomValue(infoEntry, compareValue, useMods);
		}
		XUiM_ItemStack.degradationMaxMod(infoEntry.StatType, itemValue, player, fastTags, useMods, ref num);
		XUiM_ItemStack.degradationMaxMod(infoEntry.StatType, compareValue, player, fastTags, useMods, ref num2);
		if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Bool)
		{
			if (!compareValue.IsEmpty() && num != num2)
			{
				bool flag = num2 > num;
				bool flag2 = (infoEntry.NegativePreferred ? (!flag) : flag);
				return string.Concat(new string[]
				{
					XUiM_ItemStack.ShowLocalizedBool(Convert.ToBoolean(num)),
					" (",
					flag2 ? "[00FF00]" : "[FF0000]",
					XUiM_ItemStack.ShowLocalizedBool(Convert.ToBoolean(num2)),
					"[-])"
				});
			}
			return XUiM_ItemStack.ShowLocalizedBool(Convert.ToBoolean(num));
		}
		else
		{
			if (infoEntry.DisplayType != DisplayInfoEntry.DisplayTypes.Time)
			{
				string text = string.Empty;
				if (!compareValue.IsEmpty() && (((infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent) && Mathf.Floor(num2 * 100f) != Mathf.Floor(num * 100f)) || (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Integer && Mathf.Floor(num2) != Mathf.Floor(num))))
				{
					if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
					{
						num *= 100f;
						num = Mathf.Floor(num);
						num2 *= 100f;
						num2 = Mathf.Floor(num2);
						if (infoEntry.ShowInverted)
						{
							num -= 100f;
							num2 -= 100f;
						}
						float num3 = num2 - num;
						if (flipCompare)
						{
							num3 = num - num2;
						}
						bool flag3 = num3 > 0f;
						bool flag4 = (infoEntry.NegativePreferred ? (!flag3) : flag3);
						string text2 = ((num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "");
						text = string.Concat(new string[]
						{
							text2,
							num.ToString(),
							"% (",
							flag4 ? "[00FF00]" : "[FF0000]",
							flag3 ? "+" : "",
							num3.ToString(),
							"%[-])"
						});
					}
					else
					{
						if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1)
						{
							num2 *= 10f;
							num2 = Mathf.Floor(num2);
							num2 /= 10f;
							num *= 10f;
							num = Mathf.Floor(num);
							num /= 10f;
						}
						else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2)
						{
							num2 *= 100f;
							num2 = Mathf.Floor(num2);
							num2 /= 100f;
							num *= 100f;
							num = Mathf.Floor(num);
							num /= 100f;
						}
						else
						{
							num2 = Mathf.Floor(num2);
							num = Mathf.Floor(num);
						}
						if (infoEntry.ShowInverted)
						{
							num -= 1f;
							num2 -= 1f;
						}
						float num4 = num2 - num;
						if (flipCompare)
						{
							num4 = num - num2;
						}
						bool flag5 = num4 > 0f;
						bool flag6 = (infoEntry.NegativePreferred ? (!flag5) : flag5);
						string text3 = ((num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "");
						text = string.Concat(new string[]
						{
							text3,
							num.ToString(),
							" (",
							flag6 ? "[00FF00]" : "[FF0000]",
							flag5 ? "+" : "",
							num4.ToString("0.##"),
							"[-])"
						});
					}
				}
				else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
				{
					num *= 100f;
					num = Mathf.Floor(num);
					if (infoEntry.ShowInverted)
					{
						num -= 100f;
					}
					text = ((num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "") + num.ToString("0") + "%";
				}
				else
				{
					if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1)
					{
						num *= 10f;
						num = Mathf.Floor(num);
						num /= 10f;
					}
					else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2)
					{
						num *= 100f;
						num = Mathf.Floor(num);
						num /= 100f;
					}
					else
					{
						num = Mathf.Floor(num);
					}
					if (infoEntry.ShowInverted)
					{
						num -= 1f;
					}
					text = ((num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "") + num.ToString("0.##");
				}
				bool flag7 = ItemValue.IsStatLowerBetter(infoEntry.StatType);
				float num5 = itemValue.GetStatPercent(infoEntry.StatType, false);
				num5 = compareValue.GetStatPercent(infoEntry.StatType, false) - num5;
				if (!flag7)
				{
					if (num5 > 0f)
					{
						text += string.Format(" [00f0f0](+{0:0.#}%)", num5 * 100f);
					}
				}
				else if (num5 < 0f)
				{
					text += string.Format(" [00f0f0]({0:0.#}%)", num5 * 100f);
				}
				return text;
			}
			if (!compareValue.IsEmpty() && num != num2)
			{
				if (infoEntry.ShowInverted)
				{
					num -= 1f;
					num2 -= 1f;
				}
				float num6 = num2 - num;
				if (flipCompare)
				{
					num6 = num - num2;
				}
				bool flag8 = num6 > 0f;
				bool flag9 = (infoEntry.NegativePreferred ? (!flag8) : flag8);
				return string.Concat(new string[]
				{
					XUiM_PlayerBuffs.GetCVarValueAsTimeString(num),
					" (",
					flag9 ? "[00FF00]" : "[FF0000]",
					flag8 ? "+" : "-",
					XUiM_PlayerBuffs.GetCVarValueAsTimeString(Mathf.Abs(num6)),
					"[-])"
				});
			}
			return XUiM_PlayerBuffs.GetCVarValueAsTimeString(num);
		}
	}

	public static string ShowLocalizedBool(bool value)
	{
		if (XUiM_ItemStack.localizedTrue == "")
		{
			XUiM_ItemStack.localizedTrue = Localization.Get("statTrue", false, null);
			XUiM_ItemStack.localizedFalse = Localization.Get("statFalse", false, null);
		}
		if (!value)
		{
			return XUiM_ItemStack.localizedFalse;
		}
		return XUiM_ItemStack.localizedTrue;
	}

	public static bool CanCompare(ItemClass item1, ItemClass item2)
	{
		if (item1 == null || item2 == null)
		{
			return false;
		}
		string text = item1.DisplayType;
		string text2 = item2.DisplayType;
		if (item1.IsBlock())
		{
			text = Block.list[item1.Id].DisplayType;
		}
		if (item2.IsBlock())
		{
			text2 = Block.list[item2.Id].DisplayType;
		}
		ItemDisplayEntry displayStatsForTag = UIDisplayInfoManager.Current.GetDisplayStatsForTag(text);
		ItemDisplayEntry displayStatsForTag2 = UIDisplayInfoManager.Current.GetDisplayStatsForTag(text2);
		return displayStatsForTag != null && displayStatsForTag2 != null && displayStatsForTag.DisplayGroup == displayStatsForTag2.DisplayGroup;
	}

	public static float GetCustomValue(DisplayInfoEntry entry, ItemValue itemValue, bool useMods)
	{
		Block block = ((itemValue != null) ? itemValue.ToBlockValue(false).Block : null);
		if (block != null && block.SelectAlternates)
		{
			block = block.GetAltBlockValue(itemValue.Meta).Block ?? block;
		}
		string customName = entry.CustomName;
		uint num = <PrivateImplementationDetails>.ComputeStringHash(customName);
		if (num <= 2273019518U)
		{
			if (num <= 1180197617U)
			{
				if (num != 470662067U)
				{
					if (num != 944097337U)
					{
						if (num == 1180197617U)
						{
							if (customName == "Mass")
							{
								if (block != null)
								{
									return (float)block.blockMaterial.Mass.Value;
								}
								goto IL_0463;
							}
						}
					}
					else if (customName == "RequiredPower")
					{
						BlockPowered blockPowered = block as BlockPowered;
						if (blockPowered != null)
						{
							return (float)blockPowered.RequiredPower;
						}
						goto IL_0463;
					}
				}
				else if (customName == "Explosion.RadiusEntities")
				{
					if (block == null)
					{
						goto IL_0463;
					}
					BlockMine blockMine = block as BlockMine;
					if (blockMine != null)
					{
						return (float)blockMine.Explosion.EntityRadius;
					}
					BlockCompositeTileEntity blockCompositeTileEntity = block as BlockCompositeTileEntity;
					TileEntityFeatureData tileEntityFeatureData;
					if (blockCompositeTileEntity != null && blockCompositeTileEntity.CompositeData.TryGetFeatureData<TEFeatureExplodable>(out tileEntityFeatureData))
					{
						return tileEntityFeatureData.Props.GetFloat("RadiusEntities");
					}
					goto IL_0463;
				}
			}
			else if (num != 1509954053U)
			{
				if (num != 2218964596U)
				{
					if (num == 2273019518U)
					{
						if (customName == "Explosion.RadiusBlocks")
						{
							if (block == null)
							{
								goto IL_0463;
							}
							BlockMine blockMine2 = block as BlockMine;
							if (blockMine2 != null)
							{
								return blockMine2.Explosion.BlockRadius;
							}
							BlockCompositeTileEntity blockCompositeTileEntity2 = block as BlockCompositeTileEntity;
							TileEntityFeatureData tileEntityFeatureData2;
							if (blockCompositeTileEntity2 != null && blockCompositeTileEntity2.CompositeData.TryGetFeatureData<TEFeatureExplodable>(out tileEntityFeatureData2))
							{
								return tileEntityFeatureData2.Props.GetFloat("RadiusBlocks");
							}
							goto IL_0463;
						}
					}
				}
				else if (customName == "Explosion.EntityDamage")
				{
					if (block == null)
					{
						goto IL_0463;
					}
					BlockMine blockMine3 = block as BlockMine;
					if (blockMine3 != null)
					{
						return blockMine3.Explosion.EntityDamage;
					}
					BlockCompositeTileEntity blockCompositeTileEntity3 = block as BlockCompositeTileEntity;
					TileEntityFeatureData tileEntityFeatureData3;
					if (blockCompositeTileEntity3 != null && blockCompositeTileEntity3.CompositeData.TryGetFeatureData<TEFeatureExplodable>(out tileEntityFeatureData3))
					{
						return tileEntityFeatureData3.Props.GetFloat("EntityDamage");
					}
					goto IL_0463;
				}
			}
			else if (customName == "StabilityGlue")
			{
				if (block != null)
				{
					return (float)block.blockMaterial.StabilityGlue;
				}
				goto IL_0463;
			}
		}
		else if (num <= 2523555452U)
		{
			if (num != 2276058132U)
			{
				if (num != 2327184015U)
				{
					if (num == 2523555452U)
					{
						if (customName == "LightOpacity")
						{
							if (block != null)
							{
								return (float)block.lightOpacity;
							}
							goto IL_0463;
						}
					}
				}
				else if (customName == "StabilitySupport")
				{
					if (block == null)
					{
						goto IL_0463;
					}
					if (!block.StabilitySupport)
					{
						return 0f;
					}
					return 1f;
				}
			}
			else if (customName == "MaxDamage")
			{
				if (block != null)
				{
					return (float)block.MaxDamage;
				}
				goto IL_0463;
			}
		}
		else if (num != 2927752580U)
		{
			if (num != 3550496702U)
			{
				if (num == 3706968837U)
				{
					if (customName == "ExplosionResistance")
					{
						if (block != null)
						{
							return block.GetExplosionResistance();
						}
						goto IL_0463;
					}
				}
			}
			else if (customName == "Explosion.BlockDamage")
			{
				if (block == null)
				{
					goto IL_0463;
				}
				BlockMine blockMine4 = block as BlockMine;
				if (blockMine4 != null)
				{
					return blockMine4.Explosion.BlockDamage;
				}
				BlockCompositeTileEntity blockCompositeTileEntity4 = block as BlockCompositeTileEntity;
				TileEntityFeatureData tileEntityFeatureData4;
				if (blockCompositeTileEntity4 != null && blockCompositeTileEntity4.CompositeData.TryGetFeatureData<TEFeatureExplodable>(out tileEntityFeatureData4))
				{
					return tileEntityFeatureData4.Props.GetFloat("BlockDamage");
				}
				goto IL_0463;
			}
		}
		else if (customName == "FertileLevel")
		{
			if (block != null)
			{
				return (float)block.blockMaterial.FertileLevel;
			}
			goto IL_0463;
		}
		float num2 = 0f;
		if (itemValue.ItemClass != null && itemValue.ItemClass.Effects != null && itemValue.ItemClass.Effects.EffectGroups != null)
		{
			num2 = XUiM_ItemStack.GetCustomDisplayValueForItem(itemValue, entry);
			if (useMods)
			{
				for (int i = 0; i < itemValue.Modifications.Length; i++)
				{
					if (itemValue.Modifications[i] != null && itemValue.Modifications[i].ItemClass is ItemClassModifier)
					{
						num2 += XUiM_ItemStack.GetCustomDisplayValueForItem(itemValue.Modifications[i], entry);
					}
				}
			}
		}
		return num2;
		IL_0463:
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float GetCustomDisplayValueForItem(ItemValue itemValue, DisplayInfoEntry entry)
	{
		XUiM_ItemStack.<>c__DisplayClass16_0 CS$<>8__locals1;
		CS$<>8__locals1.entry = entry;
		CS$<>8__locals1.newValue = 0f;
		MinEffectController effects = itemValue.ItemClass.Effects;
		List<MinEffectGroup> list = ((effects != null) ? effects.EffectGroups : null);
		if (list == null)
		{
			return CS$<>8__locals1.newValue;
		}
		for (int i = 0; i < list.Count; i++)
		{
			MinEffectGroup minEffectGroup = list[i];
			MinEventParams.CachedEventParam.ItemValue = itemValue;
			MinEventParams.CachedEventParam.Seed = (int)itemValue.Seed;
			if (minEffectGroup.EffectDisplayValues.ContainsKey(CS$<>8__locals1.entry.CustomName) && minEffectGroup.EffectDisplayValues[CS$<>8__locals1.entry.CustomName].IsValid(MinEventParams.CachedEventParam))
			{
				CS$<>8__locals1.newValue += minEffectGroup.EffectDisplayValues[CS$<>8__locals1.entry.CustomName].GetValue((int)itemValue.Quality);
			}
			foreach (MinEventActionBase minEventActionBase in minEffectGroup.GetTriggeredEffects(MinEventTypes.onSelfPrimaryActionEnd))
			{
				XUiM_ItemStack.<GetCustomDisplayValueForItem>g__AddValueForDisplayIfValid|16_0(minEventActionBase, ref CS$<>8__locals1);
			}
			foreach (MinEventActionBase minEventActionBase2 in minEffectGroup.GetTriggeredEffects(MinEventTypes.onSelfSecondaryActionEnd))
			{
				XUiM_ItemStack.<GetCustomDisplayValueForItem>g__AddValueForDisplayIfValid|16_0(minEventActionBase2, ref CS$<>8__locals1);
			}
		}
		return CS$<>8__locals1.newValue;
	}

	public static bool CheckKnown(EntityPlayerLocal player, ItemClass itemClass, ItemValue itemValue = null)
	{
		string unlocks = itemClass.Unlocks;
		bool flag = false;
		if (unlocks != "")
		{
			if (player.GetCVar(unlocks) == 1f)
			{
				flag = true;
			}
			if (!flag)
			{
				ProgressionValue progressionValue = player.Progression.GetProgressionValue(unlocks);
				if (progressionValue != null)
				{
					if (progressionValue.ProgressionClass.IsCrafting)
					{
						if (progressionValue.Level == progressionValue.ProgressionClass.MaxLevel)
						{
							flag = true;
						}
					}
					else if (progressionValue.Level == 1)
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				Recipe recipe = CraftingManager.GetRecipe(unlocks);
				if (recipe != null && !recipe.scrapable && !recipe.wildcardForgeCategory && recipe.IsUnlocked(player))
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <GetCustomDisplayValueForItem>g__AddValueForDisplayIfValid|16_0(MinEventActionBase actionBase, ref XUiM_ItemStack.<>c__DisplayClass16_0 A_1)
	{
		if (!(actionBase is MinEventActionModifyCVar))
		{
			return;
		}
		if (!actionBase.CanExecute(actionBase.EventType, MinEventParams.CachedEventParam))
		{
			return;
		}
		MinEventActionModifyCVar minEventActionModifyCVar = actionBase as MinEventActionModifyCVar;
		if (minEventActionModifyCVar.cvarName == A_1.entry.CustomName && minEventActionModifyCVar.targetType == MinEventActionTargetedBase.TargetTypes.self)
		{
			A_1.newValue += minEventActionModifyCVar.GetValueForDisplay();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> primaryFastTags = FastTags<TagGroup.Global>.Parse("primary");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> physicalDamageFastTags = FastTags<TagGroup.Global>.Parse("piercing,bashing,slashing,crushing,none,corrosive");

	[PublicizedFrom(EAccessModifier.Private)]
	public static string localizedTrue = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string localizedFalse = "";
}
