using System;
using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SkillListWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		this.totalItems = Localization.Get("lblTotalItems", false, null);
		this.pointsAvailable = Localization.Get("xuiPointsAvailable", false, null);
		this.skillsTitle = Localization.Get("xuiSkills", false, null);
		this.booksTitle = Localization.Get("lblCategoryBooks", false, null);
		this.craftingTitle = Localization.Get("xuiCrafting", false, null);
		this.skillList = base.GetChildByType<XUiC_SkillList>();
		XUiController childByType = base.GetChildByType<XUiC_CategoryList>();
		if (childByType != null)
		{
			this.categoryList = (XUiC_CategoryList)childByType;
			this.categoryList.CategoryChanged += this.CategoryList_CategoryChanged;
		}
		this.skillList.CategoryList = this.categoryList;
		this.skillList.SkillListWindow = this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CategoryList_CategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		this.categoryName = _categoryEntry.CategoryDisplayName;
		this.categoryIcon = _categoryEntry.SpriteName;
		this.count = this.skillList.GetActiveCount();
		base.RefreshBindings();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, "igcoSpendPoints", XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
		this.xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts, 0f);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 2672683516U)
		{
			if (num <= 1237242696U)
			{
				if (num != 1050865026U)
				{
					if (num == 1237242696U)
					{
						if (bindingName == "isbook")
						{
							if (this.skillList != null)
							{
								value = (this.skillList.DisplayType == ProgressionClass.DisplayTypes.Book).ToString();
							}
							return true;
						}
					}
				}
				else if (bindingName == "isnormal")
				{
					if (this.skillList != null)
					{
						value = (this.skillList.DisplayType == ProgressionClass.DisplayTypes.Standard).ToString();
					}
					return true;
				}
			}
			else if (num != 1741938684U)
			{
				if (num == 2672683516U)
				{
					if (bindingName == "paging_visible")
					{
						if (this.skillList != null)
						{
							value = (this.skillList.PageCount > 0).ToString();
						}
						return true;
					}
				}
			}
			else if (bindingName == "categoryicon")
			{
				value = this.categoryIcon;
				return true;
			}
		}
		else if (num <= 3877939383U)
		{
			if (num != 3822843618U)
			{
				if (num == 3877939383U)
				{
					if (bindingName == "totalskills")
					{
						value = "";
						if (this.skillList != null)
						{
							value = this.totalSkillsFormatter.Format(this.totalItems, this.skillList.GetActiveCount());
						}
						return true;
					}
				}
			}
			else if (bindingName == "titlename")
			{
				value = "";
				if (this.skillList != null)
				{
					switch (this.skillList.DisplayType)
					{
					case ProgressionClass.DisplayTypes.Standard:
						value = this.skillsTitle;
						break;
					case ProgressionClass.DisplayTypes.Book:
						value = this.booksTitle;
						break;
					case ProgressionClass.DisplayTypes.Crafting:
						value = this.craftingTitle;
						break;
					}
				}
				return true;
			}
		}
		else if (num != 3983894959U)
		{
			if (num == 4035727244U)
			{
				if (bindingName == "skillpointsavailable")
				{
					string text = this.pointsAvailable;
					EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
					if (XUi.IsGameRunning() && entityPlayer != null)
					{
						value = this.skillPointsAvailableFormatter.Format(text, entityPlayer.Progression.SkillPoints);
					}
					return true;
				}
			}
		}
		else if (bindingName == "iscrafting")
		{
			if (this.skillList != null)
			{
				value = (this.skillList.DisplayType == ProgressionClass.DisplayTypes.Crafting).ToString();
			}
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string totalItems = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int count;

	[PublicizedFrom(EAccessModifier.Private)]
	public string categoryName = "Intellect";

	[PublicizedFrom(EAccessModifier.Private)]
	public string categoryIcon = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string pointsAvailable;

	[PublicizedFrom(EAccessModifier.Private)]
	public string skillsTitle = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string booksTitle = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string craftingTitle;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SkillList skillList;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, int> totalSkillsFormatter = new CachedStringFormatter<string, int>((string _s, int _i) => string.Format(_s, _i));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string, int> skillPointsAvailableFormatter = new CachedStringFormatter<string, int>((string _s, int _i) => string.Format("{0} {1}", _s, _i));
}
