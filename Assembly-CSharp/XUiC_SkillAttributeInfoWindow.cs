using System;
using System.Collections.Generic;
using System.Globalization;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkillAttributeInfoWindow : XUiC_InfoWindow
{
	public ProgressionValue CurrentSkill
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.xui.SelectedSkill == null || !this.xui.SelectedSkill.ProgressionClass.IsAttribute)
			{
				return null;
			}
			return this.xui.SelectedSkill;
		}
	}

	public int HoveredLevel
	{
		get
		{
			return this.hoveredLevel;
		}
		set
		{
			if (this.hoveredLevel != value)
			{
				this.hoveredLevel = value;
				base.RefreshBindings();
			}
		}
	}

	public override void Init()
	{
		base.Init();
		base.GetChildrenByType<XUiC_SkillAttributeLevel>(this.levelEntries);
		int num = 1;
		foreach (XUiC_SkillAttributeLevel xuiC_SkillAttributeLevel in this.levelEntries)
		{
			xuiC_SkillAttributeLevel.ListIndex = num - 1;
			xuiC_SkillAttributeLevel.Level = num++;
			xuiC_SkillAttributeLevel.HiddenEntriesWithPaging = this.hiddenEntriesWithPaging;
			xuiC_SkillAttributeLevel.MaxEntriesWithoutPaging = this.levelEntries.Count;
			xuiC_SkillAttributeLevel.OnScroll += this.Entry_OnScroll;
			xuiC_SkillAttributeLevel.OnHover += this.Entry_OnHover;
			xuiC_SkillAttributeLevel.btnBuy.Controller.OnHover += this.Entry_OnHover;
		}
		this.actionItemList = base.GetChildByType<XUiC_ItemActionList>();
		this.skillsPerPage = this.levelEntries.Count - this.hiddenEntriesWithPaging;
		this.pager = base.GetChildByType<XUiC_Paging>();
		if (this.pager != null)
		{
			this.pager.OnPageChanged += this.Pager_OnPageChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Entry_OnHover(XUiController _sender, bool _isOver)
	{
		XUiC_SkillAttributeLevel xuiC_SkillAttributeLevel = _sender as XUiC_SkillAttributeLevel;
		if (xuiC_SkillAttributeLevel == null)
		{
			xuiC_SkillAttributeLevel = _sender.Parent as XUiC_SkillAttributeLevel;
		}
		if (_isOver && xuiC_SkillAttributeLevel != null)
		{
			this.HoveredLevel = xuiC_SkillAttributeLevel.Level;
			return;
		}
		this.HoveredLevel = -1;
	}

	public void SkillChanged()
	{
		XUiC_Paging xuiC_Paging = this.pager;
		if (xuiC_Paging != null)
		{
			xuiC_Paging.SetLastPageByElementsAndPageLength((this.CurrentSkill != null && this.CurrentSkill.ProgressionClass.MaxLevel > this.levelEntries.Count) ? (this.CurrentSkill.ProgressionClass.MaxLevel - 1) : 0, this.skillsPerPage);
		}
		XUiC_Paging xuiC_Paging2 = this.pager;
		if (xuiC_Paging2 != null)
		{
			xuiC_Paging2.Reset();
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateSkill()
	{
		if (this.CurrentSkill != null && this.actionItemList != null)
		{
			this.actionItemList.SetCraftingActionList(XUiC_ItemActionList.ItemActionListTypes.Skill, this);
		}
		XUiC_SkillEntry entryForSkill = this.windowGroup.Controller.GetChildByType<XUiC_SkillList>().GetEntryForSkill(this.CurrentSkill);
		XUiC_Paging xuiC_Paging = this.pager;
		int num = ((xuiC_Paging != null) ? xuiC_Paging.GetPage() : 0) * this.skillsPerPage + 1;
		foreach (XUiC_SkillAttributeLevel xuiC_SkillAttributeLevel in this.levelEntries)
		{
			xuiC_SkillAttributeLevel.Level = num++;
			xuiC_SkillAttributeLevel.IsDirty = true;
			if (entryForSkill != null)
			{
				xuiC_SkillAttributeLevel.btnBuy.NavLeftTarget = entryForSkill.ViewComponent;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Pager_OnPageChanged()
	{
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Entry_OnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			XUiC_Paging xuiC_Paging = this.pager;
			if (xuiC_Paging == null)
			{
				return;
			}
			xuiC_Paging.PageDown();
			return;
		}
		else
		{
			XUiC_Paging xuiC_Paging2 = this.pager;
			if (xuiC_Paging2 == null)
			{
				return;
			}
			xuiC_Paging2.PageUp();
			return;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.actionItemList != null)
		{
			this.actionItemList.SetCraftingActionList(XUiC_ItemActionList.ItemActionListTypes.Skill, this);
		}
		this.IsDirty = true;
	}

	public override void Update(float _dt)
	{
		if (this.IsDirty)
		{
			this.IsDirty = false;
			this.UpdateSkill();
			base.RefreshBindings();
		}
		if (base.ViewComponent.IsActiveInHierarchy && this.CurrentSkill != null && !this.xui.playerUI.windowManager.IsInputActive() && ((PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && this.xui.playerUI.playerInput.GUIActions.Inspect.WasPressed) || (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && this.xui.playerUI.playerInput.GUIActions.DPad_Up.WasPressed)))
		{
			foreach (XUiC_SkillAttributeLevel xuiC_SkillAttributeLevel in this.levelEntries)
			{
				if (xuiC_SkillAttributeLevel.CurrentSkill != null && xuiC_SkillAttributeLevel.Level == this.CurrentSkill.Level + 1)
				{
					xuiC_SkillAttributeLevel.btnBuy.Controller.Pressed(-1);
					break;
				}
			}
		}
		base.Update(_dt);
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		if (_name == "hidden_entries_with_paging")
		{
			this.hiddenEntriesWithPaging = StringParsers.ParseSInt32(_value, 0, -1, NumberStyles.Integer);
			foreach (XUiC_SkillAttributeLevel xuiC_SkillAttributeLevel in this.levelEntries)
			{
				if (xuiC_SkillAttributeLevel != null)
				{
					xuiC_SkillAttributeLevel.HiddenEntriesWithPaging = this.hiddenEntriesWithPaging;
				}
			}
			this.skillsPerPage = this.levelEntries.Count - this.hiddenEntriesWithPaging;
			return true;
		}
		return base.ParseAttribute(_name, _value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 1912580562U)
		{
			if (num <= 1275709072U)
			{
				if (num != 443815844U)
				{
					if (num == 1275709072U)
					{
						if (_bindingName == "maxSkillLevel")
						{
							_value = ((this.CurrentSkill != null) ? this.maxSkillLevelFormatter.Format((float)ProgressionClass.GetCalculatedMaxLevel(entityPlayer, this.CurrentSkill)) : "0");
							return true;
						}
					}
				}
				else if (_bindingName == "skillLevel")
				{
					_value = ((this.CurrentSkill != null) ? this.skillLevelFormatter.Format(this.CurrentSkill.GetCalculatedLevel(entityPlayer)) : "0");
					return true;
				}
			}
			else if (num != 1283949528U)
			{
				if (num == 1912580562U)
				{
					if (_bindingName == "buycost")
					{
						_value = "-- PTS";
						if (this.CurrentSkill != null && this.CurrentSkill.Level < this.CurrentSkill.ProgressionClass.MaxLevel)
						{
							if (this.CurrentSkill.ProgressionClass.CurrencyType == ProgressionCurrencyType.SP)
							{
								_value = this.buyCostFormatter.Format(this.CurrentSkill.ProgressionClass.CalculatedCostForLevel(this.CurrentSkill.Level + 1));
							}
							else
							{
								_value = this.expCostFormatter.Format((int)((1f - this.CurrentSkill.PercToNextLevel) * (float)this.CurrentSkill.ProgressionClass.CalculatedCostForLevel(this.CurrentSkill.Level + 1)));
							}
						}
						return true;
					}
				}
			}
			else if (_bindingName == "currentlevel")
			{
				_value = Localization.Get("xuiSkillLevel", false, null);
				return true;
			}
		}
		else if (num <= 3268933568U)
		{
			if (num != 2606420134U)
			{
				if (num == 3268933568U)
				{
					if (_bindingName == "showPaging")
					{
						_value = (this.CurrentSkill != null && this.CurrentSkill.ProgressionClass.MaxLevel > this.levelEntries.Count).ToString();
						return true;
					}
				}
			}
			else if (_bindingName == "groupdescription")
			{
				_value = ((this.CurrentSkill != null) ? Localization.Get(this.CurrentSkill.ProgressionClass.DescKey, false, null) : "");
				return true;
			}
		}
		else if (num != 3504806855U)
		{
			if (num != 4010384093U)
			{
				if (num == 4294521801U)
				{
					if (_bindingName == "detailsdescription")
					{
						if (this.CurrentSkill != null && this.hoveredLevel != -1)
						{
							using (List<MinEffectGroup>.Enumerator enumerator = this.CurrentSkill.ProgressionClass.Effects.EffectGroups.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									MinEffectGroup minEffectGroup = enumerator.Current;
									if (minEffectGroup.EffectDescriptions != null)
									{
										for (int i = 0; i < minEffectGroup.EffectDescriptions.Count; i++)
										{
											if (this.hoveredLevel >= minEffectGroup.EffectDescriptions[i].MinLevel && this.hoveredLevel <= minEffectGroup.EffectDescriptions[i].MaxLevel)
											{
												_value = ((!string.IsNullOrEmpty(minEffectGroup.EffectDescriptions[i].LongDescription)) ? minEffectGroup.EffectDescriptions[i].LongDescription : minEffectGroup.EffectDescriptions[i].Description);
												return true;
											}
										}
									}
								}
								return true;
							}
						}
						_value = "";
						return true;
					}
				}
			}
			else if (_bindingName == "groupicon")
			{
				_value = ((this.CurrentSkill != null) ? this.CurrentSkill.ProgressionClass.Icon : "ui_game_symbol_skills");
				return true;
			}
		}
		else if (_bindingName == "groupname")
		{
			_value = ((this.CurrentSkill != null) ? Localization.Get(this.CurrentSkill.ProgressionClass.NameKey, false, null) : "Skill Info");
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemActionList actionItemList;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hiddenEntriesWithPaging = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_SkillAttributeLevel> levelEntries = new List<XUiC_SkillAttributeLevel>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Private)]
	public int skillsPerPage;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hoveredLevel = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat skillLevelFormatter = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat maxSkillLevelFormatter = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> buyCostFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString() + " " + Localization.Get("xuiSkillPoints", false, null));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> expCostFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString() + " " + Localization.Get("RewardExp_keyword", false, null));
}
