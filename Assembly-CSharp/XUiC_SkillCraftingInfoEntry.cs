using System;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkillCraftingInfoEntry : XUiController
{
	public int ListIndex
	{
		set
		{
			if (value != this.listIndex)
			{
				this.listIndex = value;
				this.IsDirty = true;
			}
		}
	}

	public ProgressionClass.DisplayData Data
	{
		get
		{
			return this.data;
		}
		set
		{
			if (value != this.data)
			{
				this.data = value;
				this.IsDirty = true;
			}
		}
	}

	public bool IsSelected
	{
		get
		{
			return this.isSelected;
		}
		set
		{
			this.isSelected = value;
			this.IsDirty = true;
		}
	}

	public int MaxEntriesWithoutPaging
	{
		set
		{
			if (this.maxEntriesWithoutPaging != value)
			{
				this.maxEntriesWithoutPaging = value;
				this.IsDirty = true;
			}
		}
	}

	public int HiddenEntriesWithPaging
	{
		set
		{
			if (this.hiddenEntriesWithPaging != value)
			{
				this.hiddenEntriesWithPaging = value;
				this.IsDirty = true;
			}
		}
	}

	public override void Init()
	{
		base.Init();
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		if (_name == "color_bg_bought")
		{
			this.color_bg_bought = _value;
			return true;
		}
		if (_name == "color_bg_available")
		{
			this.color_bg_available = _value;
			return true;
		}
		if (_name == "color_bg_locked")
		{
			this.color_bg_locked = _value;
			return true;
		}
		if (_name == "color_lbl_available")
		{
			this.color_lbl_available = _value;
			return true;
		}
		if (!(_name == "color_lbl_locked"))
		{
			return base.ParseAttribute(_name, _value);
		}
		this.color_lbl_locked = _value;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		bool flag = this.data != null;
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 2155443558U)
		{
			if (num <= 375292317U)
			{
				if (num <= 94814074U)
				{
					if (num != 12870938U)
					{
						if (num == 94814074U)
						{
							if (_bindingName == "nextqualitytext")
							{
								if (!flag)
								{
									_value = "";
									return true;
								}
								int num2 = this.data.GetQualityLevel(entityPlayer.Progression.GetProgressionValue(this.data.Owner.Name).Level);
								if (num2 < this.data.QualityStarts.Length + 1)
								{
									num2++;
								}
								_value = num2.ToString();
								return true;
							}
						}
					}
					else if (_bindingName == "color_fg")
					{
						_value = this.color_lbl_available;
						if (flag)
						{
							_value = ((this.data.GetQualityLevel(entityPlayer.Progression.GetProgressionValue(this.data.Owner.Name).Level) == 0) ? this.color_lbl_locked : this.color_lbl_available);
						}
						return true;
					}
				}
				else if (num != 306828118U)
				{
					if (num != 372648664U)
					{
						if (num == 375292317U)
						{
							if (_bindingName == "showquality")
							{
								_value = (flag ? (this.data.HasQuality && this.data.GetQualityLevel(entityPlayer.Progression.GetProgressionValue(this.data.Owner.Name).Level) > 0).ToString() : "false");
								return true;
							}
						}
					}
					else if (_bindingName == "show_selected")
					{
						_value = this.isSelected.ToString();
						return true;
					}
				}
				else if (_bindingName == "currentqualitycolor")
				{
					if (!flag)
					{
						_value = "255,255,255,255";
						return true;
					}
					Color32 color = QualityInfo.GetTierColor(this.data.GetQualityLevel(entityPlayer.Progression.GetProgressionValue(this.data.Owner.Name).Level));
					_value = this.durabilitycolorFormatter.Format(color);
					return true;
				}
			}
			else if (num <= 859026113U)
			{
				if (num != 850736759U)
				{
					if (num == 859026113U)
					{
						if (_bindingName == "notcomplete")
						{
							_value = (flag ? (!this.data.IsComplete(entityPlayer.Progression.GetProgressionValue(this.data.Owner.Name).Level)).ToString() : "false");
							return true;
						}
					}
				}
				else if (_bindingName == "nextpoints")
				{
					_value = (flag ? string.Format("{0}/{1}", entityPlayer.Progression.GetProgressionValue(this.data.Owner.Name).Level, this.data.GetNextPoints(entityPlayer.Progression.GetProgressionValue(this.data.Owner.Name).Level).ToString()) : "");
					return true;
				}
			}
			else if (num != 1566407741U)
			{
				if (num != 1701888201U)
				{
					if (num == 2155443558U)
					{
						if (_bindingName == "nothiddenbypager")
						{
							_value = "true";
							return true;
						}
					}
				}
				else if (_bindingName == "itemcolor")
				{
					_value = "255,255,255,255";
					return true;
				}
			}
			else if (_bindingName == "hasentry")
			{
				_value = flag.ToString();
				return true;
			}
		}
		else if (num <= 3708628627U)
		{
			if (num <= 3185987134U)
			{
				if (num != 2842844659U)
				{
					if (num == 3185987134U)
					{
						if (_bindingName == "text")
						{
							_value = (flag ? this.data.GetName(entityPlayer.Progression.GetProgressionValue(this.data.Owner.Name).Level) : "");
							return true;
						}
					}
				}
				else if (_bindingName == "showcomplete")
				{
					_value = (flag ? this.data.IsComplete(entityPlayer.Progression.GetProgressionValue(this.data.Owner.Name).Level).ToString() : "false");
					return true;
				}
			}
			else if (num != 3263401906U)
			{
				if (num != 3503204070U)
				{
					if (num == 3708628627U)
					{
						if (_bindingName == "itemicon")
						{
							_value = (flag ? this.data.GetIcon(entityPlayer.Progression.GetProgressionValue(this.data.Owner.Name).Level) : "");
							return true;
						}
					}
				}
				else if (_bindingName == "color_bg")
				{
					_value = this.color_bg_available;
					if (flag)
					{
						_value = ((this.data.GetQualityLevel(entityPlayer.Progression.GetProgressionValue(this.data.Owner.Name).Level) == 0) ? this.color_bg_locked : this.color_bg_available);
					}
					return true;
				}
			}
			else if (_bindingName == "notcompletequality")
			{
				_value = (flag ? (this.data.HasQuality && !this.data.IsComplete(entityPlayer.Progression.GetProgressionValue(this.data.Owner.Name).Level)).ToString() : "false");
				return true;
			}
		}
		else if (num <= 3933605619U)
		{
			if (num != 3924526227U)
			{
				if (num == 3933605619U)
				{
					if (_bindingName == "notcompletenoquality")
					{
						_value = (flag ? (!this.data.HasQuality && !this.data.IsComplete(entityPlayer.Progression.GetProgressionValue(this.data.Owner.Name).Level)).ToString() : "false");
						return true;
					}
				}
			}
			else if (_bindingName == "showlock")
			{
				_value = (flag ? (this.data.GetQualityLevel(entityPlayer.Progression.GetProgressionValue(this.data.Owner.Name).Level) == 0).ToString() : "false");
				return true;
			}
		}
		else if (num != 4168931239U)
		{
			if (num != 4220565952U)
			{
				if (num == 4277871100U)
				{
					if (_bindingName == "nextqualitycolor")
					{
						if (!flag)
						{
							_value = "";
							return true;
						}
						int num3 = this.data.GetQualityLevel(entityPlayer.Progression.GetProgressionValue(this.data.Owner.Name).Level);
						if (num3 < this.data.QualityStarts.Length + 1)
						{
							num3++;
						}
						_value = this.nextdurabilitycolorFormatter.Format(QualityInfo.GetTierColor(num3));
						return true;
					}
				}
			}
			else if (_bindingName == "currentqualitytext")
			{
				_value = (flag ? this.data.GetQualityLevel(entityPlayer.Progression.GetProgressionValue(this.data.Owner.Name).Level).ToString() : "");
				return true;
			}
		}
		else if (_bindingName == "iconatlas")
		{
			if (!flag)
			{
				_value = "ItemIconAtlas";
			}
			else
			{
				_value = ((entityPlayer.Progression.GetProgressionValue(this.data.Owner.Name).Level >= this.data.QualityStarts[0]) ? "ItemIconAtlas" : "ItemIconAtlasGreyscale");
			}
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public override void Update(float _dt)
	{
		if (this.IsDirty)
		{
			this.IsDirty = false;
			base.RefreshBindings();
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string color_bg_bought;

	[PublicizedFrom(EAccessModifier.Private)]
	public string color_bg_available;

	[PublicizedFrom(EAccessModifier.Private)]
	public string color_bg_locked;

	[PublicizedFrom(EAccessModifier.Private)]
	public string color_lbl_available;

	[PublicizedFrom(EAccessModifier.Private)]
	public string color_lbl_locked;

	[PublicizedFrom(EAccessModifier.Private)]
	public int listIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProgressionClass.DisplayData data;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxEntriesWithoutPaging = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hiddenEntriesWithPaging = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float, bool> attributeSubtractionFormatter = new CachedStringFormatter<string, float, bool>((string _s, float _f, bool _b) => string.Format("{0}: {1}", _s, _f.ToCultureInvariantString("0.#") + (_b ? "%" : "")));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, float> attributeSetValueFormatter = new CachedStringFormatter<string, float>((string _s, float _f) => string.Format("{0}: {1}", _s, _f.ToCultureInvariantString("0.#")));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor durabilitycolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor nextdurabilitycolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringBuilder effectsStringBuilder = new StringBuilder();
}
