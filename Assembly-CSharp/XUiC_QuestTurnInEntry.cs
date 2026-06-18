using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestTurnInEntry : XUiC_SelectableEntry
{
	public BaseReward Reward
	{
		get
		{
			return this.reward;
		}
		set
		{
			this.reward = value;
			this.Refresh();
			if (!base.IsSelected)
			{
				this.background.Color = new Color32(64, 64, 64, byte.MaxValue);
			}
		}
	}

	public ItemStack Item
	{
		get
		{
			if (this.reward != null && this.item.IsEmpty())
			{
				if (this.reward is RewardItem)
				{
					this.item = (this.reward as RewardItem).Item;
				}
				else if (this.reward is RewardLootItem)
				{
					this.item = (this.reward as RewardLootItem).Item;
				}
			}
			return this.item;
		}
	}

	public bool Chosen
	{
		get
		{
			return this.chosen;
		}
		set
		{
			this.chosen = value;
			this.RefreshBackground();
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SelectedChanged(bool isSelected)
	{
		this.RefreshBackground();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshBackground()
	{
		if (this.background != null)
		{
			this.background.Color = (base.IsSelected ? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue) : new Color32(64, 64, 64, byte.MaxValue));
			this.background.SpriteName = (base.IsSelected ? "ui_game_select_row" : "menu_empty");
		}
	}

	public void SetBaseReward(BaseReward reward)
	{
		this.Reward = reward;
		this.item = ItemStack.Empty;
		this.IsDirty = true;
		base.RefreshBindings();
		base.ViewComponent.Enabled = reward != null;
		if (reward == null)
		{
			this.background.Color = new Color32(64, 64, 64, byte.MaxValue);
		}
	}

	public override void Init()
	{
		base.Init();
		for (int i = 0; i < this.children.Count; i++)
		{
			XUiView viewComponent = this.children[i].ViewComponent;
			if (viewComponent.ID.EqualsCaseInsensitive("background"))
			{
				this.background = viewComponent as XUiV_Sprite;
			}
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		this.isHovered = _isOver;
		if (this.background != null && this.reward != null && !base.IsSelected)
		{
			if (_isOver)
			{
				this.background.Color = new Color32(96, 96, 96, byte.MaxValue);
			}
			else
			{
				this.background.Color = new Color32(64, 64, 64, byte.MaxValue);
			}
		}
		base.OnHovered(_isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 1388578781U)
		{
			if (num <= 847165955U)
			{
				if (num != 392967384U)
				{
					if (num != 602070729U)
					{
						if (num == 847165955U)
						{
							if (bindingName == "itemtypeicon")
							{
								if (this.reward != null)
								{
									if (this.item != null && !this.item.IsEmpty())
									{
										if (this.item.itemValue.ItemClass.IsBlock())
										{
											value = Block.list[this.item.itemValue.type].ItemTypeIcon;
										}
										else
										{
											if (this.item.itemValue.ItemClass.AltItemTypeIcon != null && this.item.itemValue.ItemClass.Unlocks != "" && XUiM_ItemStack.CheckKnown(this.xui.playerUI.entityPlayer, this.item.itemValue.ItemClass, this.item.itemValue))
											{
												value = this.item.itemValue.ItemClass.AltItemTypeIcon;
												return true;
											}
											value = this.item.itemValue.ItemClass.ItemTypeIcon;
										}
									}
									else
									{
										value = "";
									}
								}
								else
								{
									value = "";
								}
								return true;
							}
						}
					}
					else if (bindingName == "namecolor")
					{
						value = ((this.reward != null && this.chosen) ? this.rewardchosencolorFormatter.Format(this.selectedColor) : this.rewardchosencolorFormatter.Format(this.defaultColor));
						return true;
					}
				}
				else if (bindingName == "hasotherreward")
				{
					if (this.reward != null)
					{
						if (this.item != null && !this.item.IsEmpty())
						{
							value = "false";
						}
						else
						{
							value = "true";
						}
					}
					else
					{
						value = "true";
					}
					return true;
				}
			}
			else if (num <= 1062608009U)
			{
				if (num != 899280930U)
				{
					if (num == 1062608009U)
					{
						if (bindingName == "durabilitycolor")
						{
							if (this.Item != null && !this.Item.IsEmpty() && this.Item.itemValue.ItemClass.ShowQualityBar)
							{
								Color32 color = QualityInfo.GetTierColor((int)this.Item.itemValue.Quality);
								value = this.durabilityColorFormatter.Format(color);
							}
							else
							{
								value = "0,0,0,0";
							}
							return true;
						}
					}
				}
				else if (bindingName == "chosenicon")
				{
					value = ((this.reward != null && this.chosen) ? "ui_game_symbol_check" : "");
					return true;
				}
			}
			else if (num != 1290001761U)
			{
				if (num == 1388578781U)
				{
					if (bindingName == "hasitemtypeicon")
					{
						if (this.reward != null)
						{
							if (this.item != null && !this.item.IsEmpty())
							{
								if (this.item.itemValue.ItemClass.IsBlock())
								{
									value = (Block.list[this.item.itemValue.type].ItemTypeIcon != "").ToString();
								}
								else
								{
									value = (this.item.itemValue.ItemClass.ItemTypeIcon != "").ToString();
								}
							}
							else
							{
								value = "false";
							}
						}
						else
						{
							value = "false";
						}
						return true;
					}
				}
			}
			else if (bindingName == "rewardname")
			{
				value = ((this.reward != null) ? this.reward.GetRewardText() : "");
				return true;
			}
		}
		else if (num <= 2485383123U)
		{
			if (num <= 2048631988U)
			{
				if (num != 1771022199U)
				{
					if (num == 2048631988U)
					{
						if (bindingName == "hasreward")
						{
							value = (this.reward != null).ToString();
							return true;
						}
					}
				}
				else if (bindingName == "rewardicon")
				{
					if (this.reward != null)
					{
						if (this.item != null && !this.item.IsEmpty())
						{
							value = this.item.itemValue.ItemClass.GetIconName();
						}
						else
						{
							value = this.reward.Icon;
						}
					}
					else
					{
						value = "";
					}
					return true;
				}
			}
			else if (num != 2400639434U)
			{
				if (num == 2485383123U)
				{
					if (bindingName == "hasitemreward")
					{
						if (this.reward != null)
						{
							if (this.item != null && !this.item.IsEmpty())
							{
								value = "true";
							}
							else
							{
								value = "false";
							}
						}
						else
						{
							value = "false";
						}
						return true;
					}
				}
			}
			else if (bindingName == "rewardicontint")
			{
				if (this.reward != null)
				{
					if (this.item != null && !this.item.IsEmpty())
					{
						value = this.rewardiconcolorFormatter.Format(this.item.itemValue.ItemClass.GetIconTint(this.item.itemValue));
					}
					else
					{
						value = "255,255,255,255";
					}
				}
				else
				{
					value = "255,255,255,255";
				}
				return true;
			}
		}
		else if (num <= 4049247086U)
		{
			if (num != 2944858628U)
			{
				if (num == 4049247086U)
				{
					if (bindingName == "itemtypeicontint")
					{
						value = "255,255,255,255";
						if (this.item != null && !this.item.IsEmpty() && this.item.itemValue.ItemClass.Unlocks != "" && XUiM_ItemStack.CheckKnown(this.xui.playerUI.entityPlayer, this.item.itemValue.ItemClass, this.item.itemValue))
						{
							value = this.altitemtypeiconcolorFormatter.Format(this.item.itemValue.ItemClass.AltItemTypeIconColor);
						}
						return true;
					}
				}
			}
			else if (bindingName == "hasdurability")
			{
				value = (this.Item != null && !this.Item.IsEmpty() && this.Item.itemValue.ItemClass.ShowQualityBar).ToString();
				return true;
			}
		}
		else if (num != 4107624007U)
		{
			if (num == 4172540779U)
			{
				if (bindingName == "durabilityfill")
				{
					value = ((this.Item != null && !this.Item.IsEmpty()) ? ((this.Item.itemValue.MaxUseTimes == 0) ? "1" : this.durabilityFillFormatter.Format(this.Item.itemValue.PercentUsesLeft)) : "0");
					return true;
				}
			}
		}
		else if (bindingName == "durabilityvalue")
		{
			value = "";
			if (this.Item != null && !this.Item.IsEmpty())
			{
				if (this.Item.itemValue.HasQuality || (!this.Item.IsEmpty() && this.Item.itemValue.ItemClass.HasSubItems))
				{
					value = ((this.Item.itemValue.Quality > 0) ? this.durabilityValueFormatter.Format((int)this.Item.itemValue.Quality) : "-");
				}
				else
				{
					value = "";
				}
			}
			else
			{
				value = "";
			}
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public override bool ParseAttribute(string name, string value)
	{
		bool flag = base.ParseAttribute(name, value);
		if (!flag)
		{
			if (!(name == "default_color"))
			{
				if (!(name == "selected_color"))
				{
					return false;
				}
				this.selectedColor = StringParsers.ParseColor32(value);
			}
			else
			{
				this.defaultColor = StringParsers.ParseColor32(value);
			}
			return true;
		}
		return flag;
	}

	public void Refresh()
	{
		this.IsDirty = true;
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BaseReward reward;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack item = ItemStack.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool chosen;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor recipeicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor hasingredientsstatecolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor unlockstatecolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor durabilityColorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt durabilityValueFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat durabilityFillFormatter = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor rewardiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor rewardchosencolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor altitemtypeiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public Color defaultColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color selectedColor;
}
