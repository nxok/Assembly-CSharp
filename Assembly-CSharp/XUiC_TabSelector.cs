using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TabSelector : XUiController
{
	public event XUiC_TabSelector.TabChangedDelegate OnTabChanged;

	[XuiXmlAttribute("use_page_buttons", false)]
	public bool UsePageButtons
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int SelectedTabIndex
	{
		get
		{
			return this.selectedTabIndex;
		}
		set
		{
			if (!this.enabled)
			{
				return;
			}
			if (value >= this.Tabs.Length)
			{
				value = this.Tabs.Length - 1;
			}
			if (this.selectedTabIndex == value)
			{
				return;
			}
			this.IsDirty = true;
			if (value < 0)
			{
				if (this.SelectedTab != null)
				{
					this.SelectedTab.TabSelected = false;
				}
				this.selectedTabIndex = value;
				this.updateTabVisibility();
				XUiC_TabSelector.TabChangedDelegate onTabChanged = this.OnTabChanged;
				if (onTabChanged == null)
				{
					return;
				}
				onTabChanged(this.selectedTabIndex, this.SelectedTab);
				return;
			}
			else
			{
				if (!this.Tabs[value].TabVisible)
				{
					return;
				}
				if (this.SelectedTab != null)
				{
					this.SelectedTab.TabSelected = false;
				}
				this.selectedTabIndex = value;
				this.SelectedTab.TabSelected = true;
				this.updateTabVisibility();
				XUiC_TabSelector.TabChangedDelegate onTabChanged2 = this.OnTabChanged;
				if (onTabChanged2 != null)
				{
					onTabChanged2(this.selectedTabIndex, this.SelectedTab);
				}
				if (this.xui.playerUI.CursorController.navigationTarget != null && this.xui.playerUI.CursorController.navigationTarget.Controller.IsChildOf(this))
				{
					this.xui.playerUI.CursorController.SetNavigationTarget(null);
				}
				if (this.SelectTabContentsOnChange)
				{
					this.pendingOnChangeSelection = true;
				}
				return;
			}
		}
	}

	public XUiC_TabSelectorTab SelectedTab
	{
		get
		{
			if (this.Tabs == null)
			{
				return null;
			}
			if (this.selectedTabIndex < 0 || this.selectedTabIndex >= this.Tabs.Length)
			{
				return null;
			}
			return this.Tabs[this.selectedTabIndex];
		}
		set
		{
			if (value == this.SelectedTab)
			{
				return;
			}
			int num = Array.IndexOf<XUiC_TabSelectorTab>(this.Tabs, value);
			if (num >= 0)
			{
				this.SelectedTabIndex = num;
			}
		}
	}

	[XuiXmlBinding("selected_tab")]
	public string SelectedTabKey
	{
		get
		{
			XUiC_TabSelectorTab selectedTab = this.SelectedTab;
			return ((selectedTab != null) ? selectedTab.TabKey : null) ?? "";
		}
	}

	public XUiC_TabSelectorButton SelectedTabButton
	{
		get
		{
			XUiC_TabSelectorTab selectedTab = this.SelectedTab;
			if (selectedTab == null)
			{
				return null;
			}
			return selectedTab.TabButton;
		}
	}

	public override void Init()
	{
		base.Init();
		if (this.tabButtons.Length == 0)
		{
			Log.Error("[XUi] TabSelector without any TabSelectorButtons in 'tabsHeader' in windowGroup '" + this.windowGroup.Id + "'");
			return;
		}
		if (this.Tabs.Length == 0)
		{
			Log.Error("[XUi] TabSelector without any TabSelectorTabs in 'tabsContent' in windowGroup '" + this.windowGroup.Id + "'");
			return;
		}
		for (int i = 0; i < this.Tabs.Length; i++)
		{
			if (i >= this.tabButtons.Length)
			{
				Log.Warning(string.Format("More tabs ({0}) than tab buttons ({1}) in windowGroup '{2}'", this.Tabs.Length, this.tabButtons.Length, base.WindowGroup.Id));
				break;
			}
			XUiC_TabSelectorTab xuiC_TabSelectorTab = this.Tabs[i];
			XUiC_TabSelectorButton xuiC_TabSelectorButton = this.tabButtons[i];
			xuiC_TabSelectorButton.Tab = xuiC_TabSelectorTab;
			xuiC_TabSelectorTab.TabButton = xuiC_TabSelectorButton;
		}
		if (this.tabHeaderBackground != null && this.tabSelectorBorder != null)
		{
			this.tabHeaderBackground.Sprite.rightAnchor.target = this.tabSelectorBorder.UiTransform;
			this.tabHeaderBackground.Sprite.rightAnchor.relative = 1f;
		}
		this.Tabs[0].TabSelected = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.updateTabVisibility();
		this.pendingOnOpenSelection = this.selectOnOpen;
		this.pendingOnChangeSelection = false;
		if (this.UsePageButtons)
		{
			this.xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory, 0f);
		}
		base.RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.CalloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuCategory);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateTabVisibility()
	{
		int num = -1;
		for (int i = 0; i < this.tabButtons.Length; i++)
		{
			if (i >= this.Tabs.Length)
			{
				this.tabButtons[i].ViewComponent.IsVisible = false;
			}
			else
			{
				bool tabVisible = this.Tabs[i].TabVisible;
				bool flag = i == this.selectedTabIndex;
				this.tabButtons[i].ViewComponent.IsVisible = tabVisible;
				this.Tabs[i].ViewComponent.IsVisible = flag;
				if (tabVisible)
				{
					num = i;
				}
			}
		}
		if (this.tabHeaderBackground != null)
		{
			UIRect.AnchorPoint leftAnchor = this.tabHeaderBackground.Sprite.leftAnchor;
			if (num >= 0)
			{
				leftAnchor.target = this.tabButtons[num].OuterDimensionsTransform;
				leftAnchor.relative = 1f;
			}
			else
			{
				UIRect.AnchorPoint anchorPoint = leftAnchor;
				XUiV_Sprite xuiV_Sprite = this.tabSelectorBorder;
				anchorPoint.target = ((xuiV_Sprite != null) ? xuiV_Sprite.UiTransform : null);
				leftAnchor.relative = 0f;
			}
			this.tabHeaderBackground.Sprite.ResetAndUpdateAnchors();
		}
		XUiV_Table xuiV_Table = this.headerButtonsTable;
		if (xuiV_Table == null)
		{
			return;
		}
		xuiV_Table.Reposition();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toggleCategory(int _dir, bool _playSound = true)
	{
		int num = this.selectedTabIndex;
		do
		{
			num = NGUIMath.RepeatIndex(num + _dir, this.Tabs.Length);
		}
		while (!this.Tabs[num].TabVisible && num != this.selectedTabIndex);
		this.SelectedTabIndex = num;
		if (!this.SelectedTab.TabVisible)
		{
			this.SelectedTabIndex = -1;
		}
		if (_playSound)
		{
			XUiC_TabSelectorButton selectedTabButton = this.SelectedTabButton;
			if (selectedTabButton == null)
			{
				return;
			}
			selectedTabButton.PlayClickSound();
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		bool flag = XUiUtils.HotkeysAllowedFor(this.viewComponent, false);
		LocalPlayerUI playerUI = this.xui.playerUI;
		PlayerActionsGUI guiactions = playerUI.playerInput.GUIActions;
		GUIWindowManager windowManager = playerUI.windowManager;
		if (flag)
		{
			if (((!this.UsePageButtons && guiactions.WindowPagingLeft.WasReleased) || (this.UsePageButtons && guiactions.PageDown.WasReleased)) && windowManager.IsWindowOpen(this.windowGroup))
			{
				this.toggleCategory(-1, true);
			}
			if (((!this.UsePageButtons && guiactions.WindowPagingRight.WasReleased) || (this.UsePageButtons && guiactions.PageUp.WasReleased)) && windowManager.IsWindowOpen(this.windowGroup))
			{
				this.toggleCategory(1, true);
			}
		}
		XUiC_TabSelectorTab selectedTab = this.SelectedTab;
		if (selectedTab != null)
		{
			if (this.pendingOnOpenSelection)
			{
				selectedTab.SelectCursorElement(true, false);
				this.pendingOnOpenSelection = false;
			}
			if (this.pendingOnChangeSelection)
			{
				selectedTab.SelectCursorElement(true, false);
				this.pendingOnChangeSelection = false;
			}
		}
		if (this.IsDirty)
		{
			this.IsDirty = false;
			base.RefreshBindings();
		}
	}

	public void SetTabCaption(int _index, string _name)
	{
		if (_index >= this.Tabs.Length)
		{
			throw new ArgumentOutOfRangeException("_index");
		}
		this.Tabs[_index].TabHeaderText = _name;
	}

	public XUiC_TabSelectorTab GetTab(int _index)
	{
		if (_index >= this.Tabs.Length)
		{
			throw new ArgumentOutOfRangeException("_index");
		}
		return this.Tabs[_index];
	}

	public void SelectTabByName(string _tabKey)
	{
		foreach (XUiC_TabSelectorTab xuiC_TabSelectorTab in this.Tabs)
		{
			if (!(xuiC_TabSelectorTab.TabKey != _tabKey))
			{
				this.SelectedTab = xuiC_TabSelectorTab;
				return;
			}
		}
	}

	public bool IsSelected(string _tabKey)
	{
		XUiC_TabSelectorTab selectedTab = this.SelectedTab;
		return selectedTab != null && selectedTab.TabKey.Equals(_tabKey);
	}

	public void TabVisibilityChanged(XUiC_TabSelectorTab _tab, bool _visible)
	{
		if (_tab == this.SelectedTab && !_visible)
		{
			this.toggleCategory(-1, false);
		}
		else if (_visible && this.SelectedTabIndex < 0)
		{
			this.SelectedTab = _tab;
		}
		this.updateTabVisibility();
	}

	public void TabButtonClicked(XUiC_TabSelectorButton _tabButton)
	{
		if (!this.enabled)
		{
			return;
		}
		this.SelectedTabIndex = Array.IndexOf<XUiC_TabSelectorButton>(this.tabButtons, _tabButton);
	}

	[XuiXmlAttribute("select_tab_contents_on_open", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeSelectTabContentsOnOpen(string _value)
	{
		if (!string.IsNullOrEmpty(_value))
		{
			this.selectOnOpen = StringParsers.ParseBool(_value, 0, -1, true);
		}
	}

	[XuiXmlAttribute("select_tab_contents_on_change", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeSelectTabContentsOnChange(string _value)
	{
		if (!string.IsNullOrEmpty(_value))
		{
			this.SelectTabContentsOnChange = StringParsers.ParseBool(_value, 0, -1, true);
		}
	}

	[XuiBindComponent("tabsHeader.", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TabSelectorButton[] tabButtons;

	[XuiBindComponent("tabsContents.", true)]
	public readonly XUiC_TabSelectorTab[] Tabs;

	[XuiBindComponent("tabsHeader.", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Table headerButtonsTable;

	[XuiBindComponent("border", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite tabSelectorBorder;

	[XuiBindComponent("backgroundMainTabs", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite tabHeaderBackground;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selectedTabIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool selectOnOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pendingOnOpenSelection;

	public bool SelectTabContentsOnChange = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pendingOnChangeSelection;

	public delegate void TabChangedDelegate(int _tabIndex, XUiC_TabSelectorTab _tab);
}
