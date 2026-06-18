using System;
using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkillWindowGroup : XUiController
{
	public ProgressionValue CurrentSkill
	{
		get
		{
			return this.currentSkill;
		}
		set
		{
			this.currentSkill = value;
		}
	}

	public override void Init()
	{
		base.Init();
		this.skillList = base.GetChildByType<XUiC_SkillList>();
		this.skillAttributeInfoWindow = base.GetChildByType<XUiC_SkillAttributeInfoWindow>();
		this.skillSkillInfoWindow = base.GetChildByType<XUiC_SkillSkillInfoWindow>();
		this.skillPerkInfoWindow = base.GetChildByType<XUiC_SkillPerkInfoWindow>();
		this.skillBookInfoWindow = base.GetChildByType<XUiC_SkillBookInfoWindow>();
		this.skillCraftingInfoWindow = base.GetChildByType<XUiC_SkillCraftingInfoWindow>();
		XUiController[] childrenByType = base.GetChildrenByType<XUiC_SkillEntry>(null);
		XUiController[] array = childrenByType;
		this.skillEntries = new XUiC_SkillEntry[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			this.skillEntries[i] = (XUiC_SkillEntry)array[i];
			this.skillEntries[i].OnPress += this.XUiC_SkillEntry_OnPress;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CategoryList_CategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		this.skillList.Category = _categoryEntry.CategoryName;
		this.skillList.RefreshSkillList();
		this.skillList.SelectFirstEntry();
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CategoryList_CategoryClickChanged(XUiC_CategoryEntry _categoryEntry)
	{
		this.skillList.Category = _categoryEntry.CategoryName;
		this.skillList.SetFilterText("");
		this.skillList.RefreshSkillList();
		this.skillList.SelectFirstEntry();
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiC_SkillEntry_OnPress(XUiController _sender, int _mouseButton)
	{
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.playerUI.windowManager.Close("windowpaging");
		this.xui.CalloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.CurrentSkill = this.xui.SelectedSkill;
			this.skillAttributeInfoWindow.SkillChanged();
			this.skillSkillInfoWindow.IsDirty = true;
			this.skillPerkInfoWindow.SkillChanged();
			this.skillBookInfoWindow.SkillChanged();
			this.skillCraftingInfoWindow.SkillChanged();
			if (this.skillList.DisplayType == ProgressionClass.DisplayTypes.Book)
			{
				this.skillBookInfoWindow.ViewComponent.IsVisible = true;
			}
			else if (this.skillList.DisplayType == ProgressionClass.DisplayTypes.Crafting)
			{
				this.skillCraftingInfoWindow.ViewComponent.IsVisible = true;
			}
			else if (this.xui.SelectedSkill != null)
			{
				if (this.xui.SelectedSkill.ProgressionClass.IsAttribute)
				{
					this.skillAttributeInfoWindow.ViewComponent.IsVisible = true;
				}
				else if (this.xui.SelectedSkill.ProgressionClass.IsSkill)
				{
					this.skillSkillInfoWindow.ViewComponent.IsVisible = true;
				}
				else
				{
					this.skillPerkInfoWindow.ViewComponent.IsVisible = true;
				}
			}
			this.IsDirty = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.categoryList == null)
		{
			XUiController childByType = base.GetChildByType<XUiC_CategoryList>();
			if (childByType != null)
			{
				this.categoryList = (XUiC_CategoryList)childByType;
				this.categoryList.SetupCategoriesByWorkstation("skills");
				this.categoryList.CategoryChanged += this.CategoryList_CategoryChanged;
				this.categoryList.CategoryClickChanged += this.CategoryList_CategoryClickChanged;
			}
		}
		this.xui.playerUI.windowManager.Open("windowpaging", false);
		this.xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu, 0f);
		XUiC_WindowSelector childByType2 = this.xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>();
		if (childByType2 != null)
		{
			childByType2.SetSelected("skills");
		}
		this.IsDirty = true;
		if (this.categoryList.CurrentCategory == null)
		{
			this.categoryList.SetCategoryToFirst();
		}
		this.skillList.Category = this.categoryList.CurrentCategory.CategoryName;
		this.skillList.RefreshSkillList();
		if (this.xui.SelectedSkill == null)
		{
			this.skillList.SelectFirstEntry();
		}
		else
		{
			this.skillList.SelectedEntry.SelectCursorElement(true, false);
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SkillEntry[] skillEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SkillList skillList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SkillAttributeInfoWindow skillAttributeInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SkillSkillInfoWindow skillSkillInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SkillPerkInfoWindow skillPerkInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SkillBookInfoWindow skillBookInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SkillCraftingInfoWindow skillCraftingInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProgressionValue currentSkill;
}
