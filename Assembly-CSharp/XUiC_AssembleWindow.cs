using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_AssembleWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		this.btnComplete = base.GetChildById("btnComplete");
		if (this.btnComplete != null)
		{
			this.btnComplete.OnPress += this.BtnComplete_OnPress;
		}
	}

	public virtual ItemStack ItemStack
	{
		get
		{
			return this.itemStack;
		}
		set
		{
			this.itemStack = value;
			if (!this.itemStack.IsEmpty())
			{
				this.itemClass = this.itemStack.itemValue.ItemClass;
				this.itemDisplayEntry = UIDisplayInfoManager.Current.GetDisplayStatsForTag(this.itemClass.DisplayType);
			}
			else
			{
				this.itemClass = null;
			}
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnComplete_OnPress(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.CloseAllOpenModalWindows(null, false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = !this.itemStack.IsEmpty();
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 3191456325U)
		{
			if (num <= 1607128273U)
			{
				if (num <= 619741203U)
				{
					if (num != 546263858U)
					{
						if (num == 619741203U)
						{
							if (bindingName == "itemqualitytitle")
							{
								value = Localization.Get("xuiQuality", false, null);
								return true;
							}
						}
					}
					else if (bindingName == "itemqualityfill")
					{
						value = (flag ? this.qualityfillFormatter.Format(((float)this.itemStack.itemValue.MaxUseTimes - this.itemStack.itemValue.UseTimes) / (float)this.itemStack.itemValue.MaxUseTimes) : "1");
						return true;
					}
				}
				else if (num != 1556795416U)
				{
					if (num != 1573573035U)
					{
						if (num == 1607128273U)
						{
							if (bindingName == "itemstattitle1")
							{
								value = (flag ? this.GetStatTitle(0) : "");
								return true;
							}
						}
					}
					else if (bindingName == "itemstattitle3")
					{
						value = (flag ? this.GetStatTitle(2) : "");
						return true;
					}
				}
				else if (bindingName == "itemstattitle2")
				{
					value = (flag ? this.GetStatTitle(1) : "");
					return true;
				}
			}
			else if (num <= 1640683511U)
			{
				if (num != 1623905892U)
				{
					if (num == 1640683511U)
					{
						if (bindingName == "itemstattitle7")
						{
							value = (flag ? this.GetStatTitle(6) : "");
							return true;
						}
					}
				}
				else if (bindingName == "itemstattitle6")
				{
					value = (flag ? this.GetStatTitle(5) : "");
					return true;
				}
			}
			else if (num != 1657461130U)
			{
				if (num != 1674238749U)
				{
					if (num == 3191456325U)
					{
						if (bindingName == "itemname")
						{
							value = (flag ? this.itemClass.GetLocalizedItemName() : "");
							return true;
						}
					}
				}
				else if (bindingName == "itemstattitle5")
				{
					value = (flag ? this.GetStatTitle(4) : "");
					return true;
				}
			}
			else if (bindingName == "itemstattitle4")
			{
				value = (flag ? this.GetStatTitle(3) : "");
				return true;
			}
		}
		else if (num <= 4140647608U)
		{
			if (num <= 3994216002U)
			{
				if (num != 3708628627U)
				{
					if (num == 3994216002U)
					{
						if (bindingName == "itemqualitycolor")
						{
							value = "255,255,255,255";
							if (flag)
							{
								Color32 color = QualityInfo.GetQualityColor((int)this.itemStack.itemValue.Quality);
								value = this.qualitycolorFormatter.Format(color);
							}
							return true;
						}
					}
				}
				else if (bindingName == "itemicon")
				{
					value = "";
					if (flag)
					{
						value = this.itemStack.itemValue.GetPropertyOverride("CustomIcon", (this.itemClass.CustomIcon != null) ? this.itemClass.CustomIcon.Value : this.itemClass.GetIconName());
					}
					return true;
				}
			}
			else if (num != 4053908414U)
			{
				if (num != 4113438435U)
				{
					if (num == 4140647608U)
					{
						if (bindingName == "itemstat4")
						{
							value = (flag ? this.GetStatValue(3) : "");
							return true;
						}
					}
				}
				else if (bindingName == "itemquality")
				{
					value = (flag ? this.qualityFormatter.Format((int)this.itemStack.itemValue.Quality) : "0");
					return true;
				}
			}
			else if (bindingName == "itemicontint")
			{
				Color32 color2 = Color.white;
				if (this.itemClass != null)
				{
					color2 = this.itemStack.itemValue.ItemClass.GetIconTint(this.itemStack.itemValue);
				}
				value = this.itemicontintcolorFormatter.Format(color2);
				return true;
			}
		}
		else if (num <= 4190980465U)
		{
			if (num != 4157425227U)
			{
				if (num != 4174202846U)
				{
					if (num == 4190980465U)
					{
						if (bindingName == "itemstat7")
						{
							value = (flag ? this.GetStatValue(6) : "");
							return true;
						}
					}
				}
				else if (bindingName == "itemstat6")
				{
					value = (flag ? this.GetStatValue(5) : "");
					return true;
				}
			}
			else if (bindingName == "itemstat5")
			{
				value = (flag ? this.GetStatValue(4) : "");
				return true;
			}
		}
		else if (num != 4224535703U)
		{
			if (num != 4241313322U)
			{
				if (num == 4258090941U)
				{
					if (bindingName == "itemstat3")
					{
						value = (flag ? this.GetStatValue(2) : "");
						return true;
					}
				}
			}
			else if (bindingName == "itemstat2")
			{
				value = (flag ? this.GetStatValue(1) : "");
				return true;
			}
		}
		else if (bindingName == "itemstat1")
		{
			value = (flag ? this.GetStatValue(0) : "");
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetStatTitle(int index)
	{
		if (this.itemDisplayEntry == null || this.itemDisplayEntry.DisplayStats.Count <= index)
		{
			return "";
		}
		if (this.itemDisplayEntry.DisplayStats[index].TitleOverride != null)
		{
			return this.itemDisplayEntry.DisplayStats[index].TitleOverride;
		}
		return UIDisplayInfoManager.Current.GetLocalizedName(this.itemDisplayEntry.DisplayStats[index].StatType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetStatValue(int index)
	{
		if (this.itemDisplayEntry == null || this.itemDisplayEntry.DisplayStats.Count <= index)
		{
			return "";
		}
		DisplayInfoEntry displayInfoEntry = this.itemDisplayEntry.DisplayStats[index];
		return XUiM_ItemStack.GetStatItemValueTextWithModInfo(this.itemStack, this.xui.playerUI.entityPlayer, displayInfoEntry);
	}

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null && GameManager.Instance.World == null)
		{
			return;
		}
		if (this.IsDirty)
		{
			base.RefreshBindings();
			this.IsDirty = false;
		}
		base.Update(_dt);
	}

	public virtual void OnChanged()
	{
		XUiC_AssembleWindowGroup.GetWindowGroup(this.xui).ItemStack = this.ItemStack;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack itemStack = ItemStack.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass itemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnComplete;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor qualitycolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat qualityfillFormatter = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt qualityFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemDisplayEntry itemDisplayEntry;
}
