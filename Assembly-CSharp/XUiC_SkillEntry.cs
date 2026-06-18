using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkillEntry : XUiController
{
	public ProgressionValue Skill
	{
		get
		{
			return this.currentSkill;
		}
		set
		{
			this.currentSkill = value;
			base.RefreshBindings();
			this.IsDirty = true;
			this.IsHovered = false;
			base.ViewComponent.IsNavigatable = (base.ViewComponent.IsSnappable = value != null);
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
			if (this.isSelected != value)
			{
				this.IsDirty = true;
				this.isSelected = value;
			}
		}
	}

	public ProgressionClass.DisplayTypes DisplayType
	{
		get
		{
			return this.displayType;
		}
		set
		{
			this.displayType = value;
		}
	}

	public override void Init()
	{
		base.Init();
		if (base.GetChildById("groupName") != null)
		{
			this.groupName = base.GetChildById("groupName").ViewComponent as XUiV_Label;
			if (this.groupName != null)
			{
				this.ogNamePos = this.groupName.UiTransform.localPosition;
			}
		}
		if (base.GetChildById("groupIcon") != null)
		{
			this.groupIcon = base.GetChildById("groupIcon").ViewComponent as XUiV_Sprite;
			if (this.groupIcon != null)
			{
				this.ogIconPos = this.groupIcon.UiTransform.localPosition;
				this.ogIconSize = this.groupIcon.Size;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		base.OnHovered(_isOver);
		if (this.currentSkill != null && (this.currentSkill.ProgressionClass.Type != ProgressionType.Skill || this.DisplayType != ProgressionClass.DisplayTypes.Standard))
		{
			if (this.IsHovered != _isOver)
			{
				this.IsHovered = _isOver;
				base.RefreshBindings();
				return;
			}
		}
		else
		{
			this.IsHovered = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetGroupLevel()
	{
		if (this.displayType == ProgressionClass.DisplayTypes.Standard && this.currentSkill != null)
		{
			if (this.currentSkill.ProgressionClass.Type == ProgressionType.Crafting || this.currentSkill.ProgressionClass.IsPerk)
			{
				return this.craftingGroupLevelFormatter.Format(this.currentSkill.CalculatedLevel(this.xui.playerUI.entityPlayer), this.currentSkill.ProgressionClass.MaxLevel);
			}
			if (this.currentSkill.ProgressionClass.Type != ProgressionType.Skill)
			{
				return this.groupLevelFormatter.Format(this.currentSkill.Level, this.currentSkill.CalculatedLevel(this.xui.playerUI.entityPlayer), this.currentSkill.ProgressionClass.MaxLevel);
			}
		}
		return "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetGroupPointCost()
	{
		if (this.currentSkill != null)
		{
			if (this.currentSkill.ProgressionClass.IsAttribute || this.currentSkill.ProgressionClass.IsPerk)
			{
				if (this.currentSkill.ProgressionClass.CurrencyType != ProgressionCurrencyType.SP)
				{
					return "";
				}
				if (this.currentSkill.CostForNextLevel <= 0)
				{
					return "NA";
				}
				return this.groupPointCostFormatter.Format(this.currentSkill.CostForNextLevel);
			}
			else
			{
				if (this.currentSkill.ProgressionClass.IsBookGroup)
				{
					int num = 0;
					int num2 = 0;
					for (int i = 0; i < this.currentSkill.ProgressionClass.Children.Count; i++)
					{
						num++;
						if (this.xui.playerUI.entityPlayer.Progression.GetProgressionValue(this.currentSkill.ProgressionClass.Children[i].Name).Level == 1)
						{
							num2++;
						}
					}
					num2 = Mathf.Min(num2, num - 1);
					return this.groupLevelFormatter.Format(num2, num2, num - 1);
				}
				if (this.currentSkill.ProgressionClass.IsCrafting)
				{
					return this.craftingGroupLevelFormatter.Format(this.currentSkill.CalculatedLevel(this.xui.playerUI.entityPlayer), this.currentSkill.ProgressionClass.MaxLevel);
				}
			}
		}
		return "";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 2853047009U)
		{
			if (num <= 1209257445U)
			{
				if (num <= 515098247U)
				{
					if (num != 329963356U)
					{
						if (num == 515098247U)
						{
							if (bindingName == "isnothighlighted")
							{
								value = (!this.IsHovered && !this.IsSelected).ToString();
								return true;
							}
						}
					}
					else if (bindingName == "isnotlocked")
					{
						value = ((this.currentSkill != null) ? (this.currentSkill.CalculatedLevel(this.xui.playerUI.entityPlayer) <= this.currentSkill.CalculatedMaxLevel(this.xui.playerUI.entityPlayer)).ToString() : "true");
						return true;
					}
				}
				else if (num != 765459171U)
				{
					if (num == 1209257445U)
					{
						if (bindingName == "grouptypeicon")
						{
							if (this.displayType == ProgressionClass.DisplayTypes.Standard)
							{
								value = ((this.currentSkill != null) ? (this.currentSkill.ProgressionClass.IsPerk ? "ui_game_symbol_perk" : (this.currentSkill.ProgressionClass.IsSkill ? "ui_game_symbol_skills" : (this.currentSkill.ProgressionClass.IsAttribute ? "ui_game_symbol_hammer" : "ui_game_symbol_skills"))) : "");
							}
							return true;
						}
					}
				}
				else if (bindingName == "rowstatecolor")
				{
					value = (this.IsSelected ? "255,255,255,255" : (this.IsHovered ? this.hoverColor : ((this.currentSkill != null && this.currentSkill.ProgressionClass.IsAttribute) ? "160,160,160,255" : this.rowColor)));
					return true;
				}
			}
			else if (num <= 2063064015U)
			{
				if (num != 1656712805U)
				{
					if (num == 2063064015U)
					{
						if (bindingName == "skillpercentthislevel")
						{
							value = ((this.currentSkill != null) ? this.skillPercentThisLevelFormatter.Format(this.currentSkill.PercToNextLevel) : "0");
							return true;
						}
					}
				}
				else if (bindingName == "rowstatesprite")
				{
					value = (this.IsSelected ? "ui_game_select_row" : "menu_empty");
					return true;
				}
			}
			else if (num != 2205902594U)
			{
				if (num != 2648037987U)
				{
					if (num == 2853047009U)
					{
						if (bindingName == "cannotpurchase")
						{
							value = ((this.currentSkill != null && this.currentSkill.ProgressionClass.Type != ProgressionType.Skill && this.currentSkill.ProgressionClass.Type != ProgressionType.BookGroup && this.currentSkill.ProgressionClass.Type != ProgressionType.Crafting) ? (!this.currentSkill.CanPurchase(this.xui.playerUI.entityPlayer, this.currentSkill.Level + 1)).ToString() : "false");
							return true;
						}
					}
				}
				else if (bindingName == "islocked")
				{
					value = ((this.currentSkill != null) ? (this.currentSkill.CalculatedLevel(this.xui.playerUI.entityPlayer) > this.currentSkill.CalculatedMaxLevel(this.xui.playerUI.entityPlayer)).ToString() : "false");
					return true;
				}
			}
			else if (bindingName == "canpurchase")
			{
				if (this.displayType != ProgressionClass.DisplayTypes.Standard)
				{
					value = "true";
				}
				else
				{
					value = ((this.currentSkill != null && this.currentSkill.ProgressionClass.Type != ProgressionType.Skill) ? this.currentSkill.CanPurchase(this.xui.playerUI.entityPlayer, this.currentSkill.Level + 1).ToString() : "false");
				}
				return true;
			}
		}
		else if (num <= 3504806855U)
		{
			if (num <= 3095337586U)
			{
				if (num != 2864211248U)
				{
					if (num == 3095337586U)
					{
						if (bindingName == "grouplevel")
						{
							value = this.GetGroupLevel();
							return true;
						}
					}
				}
				else if (bindingName == "hasskill")
				{
					value = (this.currentSkill != null).ToString();
					return true;
				}
			}
			else if (num != 3380638462U)
			{
				if (num == 3504806855U)
				{
					if (bindingName == "groupname")
					{
						value = ((this.currentSkill != null) ? Localization.Get(this.currentSkill.ProgressionClass.NameKey, false, null) : "");
						return true;
					}
				}
			}
			else if (bindingName == "statuscolor")
			{
				value = ((this.currentSkill != null) ? ((this.currentSkill.CalculatedMaxLevel(this.xui.playerUI.entityPlayer) == 0 && this.currentSkill.ProgressionClass.Type != ProgressionType.Crafting) ? this.disabledColor : this.enabledColor) : this.disabledColor);
				return true;
			}
		}
		else if (num <= 3689766838U)
		{
			if (num != 3677042333U)
			{
				if (num == 3689766838U)
				{
					if (bindingName == "ishighlighted")
					{
						value = (this.IsHovered || this.IsSelected).ToString();
						return true;
					}
				}
			}
			else if (bindingName == "requiredskill")
			{
				string text = "NA";
				if (this.currentSkill != null)
				{
					text = this.currentSkill.ProgressionClass.NameKey;
				}
				value = text;
				return true;
			}
		}
		else if (num != 4010384093U)
		{
			if (num != 4017165219U)
			{
				if (num == 4224950485U)
				{
					if (bindingName == "skillpercentshouldshow")
					{
						value = ((this.currentSkill != null) ? (this.currentSkill.ProgressionClass.Type == ProgressionType.Skill).ToString() : "false");
						return true;
					}
				}
			}
			else if (bindingName == "grouppointcost")
			{
				value = this.GetGroupPointCost();
				return true;
			}
		}
		else if (bindingName == "groupicon")
		{
			value = ((this.currentSkill != null) ? ((this.currentSkill.ProgressionClass.Icon == null || this.currentSkill.ProgressionClass.Icon == "") ? "ui_game_filled_circle" : ((this.currentSkill.ProgressionClass.Icon != null) ? this.currentSkill.ProgressionClass.Icon : "ui_game_symbol_other")) : "");
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public override bool ParseAttribute(string name, string value)
	{
		if (name == "enabled_color")
		{
			this.enabledColor = value;
			return true;
		}
		if (name == "disabled_color")
		{
			this.disabledColor = value;
			return true;
		}
		if (name == "row_color")
		{
			this.rowColor = value;
			return true;
		}
		if (!(name == "hover_color"))
		{
			return base.ParseAttribute(name, value);
		}
		this.hoverColor = value;
		return true;
	}

	public override void Update(float _dt)
	{
		if (this.currentSkill != null)
		{
			if (this.displayType != ProgressionClass.DisplayTypes.Standard)
			{
				this.groupIcon.UiTransform.localPosition = this.ogIconPos;
				this.groupIcon.Size = this.ogIconSize;
			}
			else if (this.currentSkill.ProgressionClass.IsSkill)
			{
				Vector3 vector;
				vector..ctor(32f, -4f, 0f);
				if (this.currentSkill.ProgressionClass.Parent.Hidden)
				{
					vector..ctor(0f, -4f, 0f);
				}
				this.groupIcon.UiTransform.localPosition = this.ogIconPos + vector;
				this.groupIcon.Size = this.ogIconSize;
				base.ViewComponent.IsNavigatable = false;
			}
			else if (this.currentSkill.ProgressionClass.IsPerk)
			{
				Vector3 vector2;
				vector2..ctor(64f, -4f, 0f);
				if (this.currentSkill.ProgressionClass.Parent.Hidden && this.currentSkill.ProgressionClass.Parent.Parent.Hidden)
				{
					vector2..ctor(0f, -4f, 0f);
				}
				else if (this.currentSkill.ProgressionClass.Parent.Hidden || this.currentSkill.ProgressionClass.Parent.Parent.Hidden)
				{
					vector2..ctor(32f, -4f, 0f);
				}
				this.groupIcon.UiTransform.localPosition = this.ogIconPos + vector2;
				this.groupIcon.Size = this.ogIconSize;
				base.ViewComponent.IsNavigatable = true;
			}
			else
			{
				this.groupIcon.UiTransform.localPosition = this.ogIconPos;
				this.groupIcon.Size = this.ogIconSize;
				base.ViewComponent.IsNavigatable = true;
			}
		}
		else
		{
			this.groupIcon.UiTransform.localPosition = this.ogIconPos;
			this.groupIcon.Size = this.ogIconSize;
			base.ViewComponent.IsNavigatable = true;
		}
		if (this.currentSkill != null)
		{
			if (this.displayType != ProgressionClass.DisplayTypes.Standard)
			{
				this.groupName.UiTransform.localPosition = this.ogNamePos;
			}
			else if (this.currentSkill.ProgressionClass.IsSkill)
			{
				Vector3 vector3;
				vector3..ctor(32f, -4f, 0f);
				if (this.currentSkill.ProgressionClass.Parent.Hidden)
				{
					vector3..ctor(0f, -4f, 0f);
				}
				this.groupName.UiTransform.localPosition = this.ogNamePos + vector3;
				base.ViewComponent.IsNavigatable = false;
			}
			else if (this.currentSkill.ProgressionClass.IsPerk)
			{
				Vector3 zero;
				zero..ctor(64f, 0f, 0f);
				if (this.currentSkill.ProgressionClass.Parent.Hidden && this.currentSkill.ProgressionClass.Parent.Parent.Hidden)
				{
					zero = Vector3.zero;
				}
				else if (this.currentSkill.ProgressionClass.Parent.Hidden || this.currentSkill.ProgressionClass.Parent.Parent.Hidden)
				{
					zero..ctor(32f, 0f, 0f);
				}
				this.groupName.UiTransform.localPosition = this.ogNamePos + zero;
				base.ViewComponent.IsNavigatable = true;
			}
			else
			{
				this.groupName.UiTransform.localPosition = this.ogNamePos;
				base.ViewComponent.IsNavigatable = true;
			}
		}
		else
		{
			this.groupName.UiTransform.localPosition = this.ogNamePos;
			base.ViewComponent.IsNavigatable = true;
		}
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.IsDirty = false;
			base.RefreshBindings();
		}
	}

	public override void OnCursorSelected(bool _isActualElement)
	{
		base.OnCursorSelected(_isActualElement);
		this.skillList.SetSelected(this);
		((XUiC_SkillWindowGroup)this.windowGroup.Controller).IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ProgressionValue currentSkill;

	[PublicizedFrom(EAccessModifier.Private)]
	public string enabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string disabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string rowColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string hoverColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label groupName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite groupIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 ogIconPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 ogNamePos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i ogIconSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSelected;

	public bool IsHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProgressionClass.DisplayTypes displayType;

	public XUiC_SkillList skillList;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int, int> groupLevelFormatter = new CachedStringFormatter<int, int, int>(delegate(int _i3, int _i1, int _i2)
	{
		if (_i1 < _i3)
		{
			return "[cc1111]" + _i1.ToString() + "[-]/" + _i2.ToString();
		}
		if (_i1 <= _i3)
		{
			return _i1.ToString() + "/" + _i2.ToString();
		}
		return "[11cc11]" + _i1.ToString() + "[-]/" + _i2.ToString();
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int> craftingGroupLevelFormatter = new CachedStringFormatter<int, int>((int _i1, int _i2) => _i1.ToString() + "/" + _i2.ToString());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> groupPointCostFormatter = new CachedStringFormatter<int>((int _i) => string.Format("{0} {1}", _i, (_i != 1) ? Localization.Get("xuiSkillPoints", false, null) : Localization.Get("xuiSkillPoint", false, null)));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat skillPercentThisLevelFormatter = new CachedStringFormatterFloat(null);
}
