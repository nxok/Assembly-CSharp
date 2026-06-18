using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerFilters : XUiController
{
	public event Action<IServerBrowserFilterControl> OnFilterChanged;

	[XuiXmlBinding("results")]
	public int ResultCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlBinding("crossplaytooltip")]
	public string CrossplayTooltip
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return PermissionsManager.GetPermissionDenyReason(EUserPerms.Crossplay, PermissionsManager.PermissionSources.All) ?? Localization.Get("xuiOptionsGeneralCrossplayTooltip", false, null);
		}
	}

	public override void Init()
	{
		base.Init();
		foreach (XUiC_ServerBrowserGamePrefSelectorCombo xuiC_ServerBrowserGamePrefSelectorCombo in base.GetChildrenByType<XUiC_ServerBrowserGamePrefSelectorCombo>(null))
		{
			GameInfoInt gameInfoInt = xuiC_ServerBrowserGamePrefSelectorCombo.GameInfoInt;
			if (gameInfoInt != GameInfoInt.CurrentServerTime)
			{
				if (gameInfoInt != GameInfoInt.AirDropFrequency)
				{
					if (gameInfoInt == GameInfoInt.WorldSize)
					{
						if (PlatformOptimizations.EnforceMaxWorldSizeClient)
						{
							xuiC_ServerBrowserGamePrefSelectorCombo.ValueRangeMin = new int?(0);
							xuiC_ServerBrowserGamePrefSelectorCombo.ValueRangeMax = new int?(PlatformOptimizations.MaxWorldSizeClient);
						}
					}
				}
				else
				{
					xuiC_ServerBrowserGamePrefSelectorCombo.ValuePreDisplayModifierFunc = (int _n) => _n;
				}
			}
			else
			{
				xuiC_ServerBrowserGamePrefSelectorCombo.CustomValuePreFilterModifierFunc = (int _n) => (_n - 1) * 24000;
			}
			if (xuiC_ServerBrowserGamePrefSelectorCombo.GameInfoString == GameInfoString.Region)
			{
				this.regionFilter = xuiC_ServerBrowserGamePrefSelectorCombo;
			}
			GameInfoBool gameInfoBool = xuiC_ServerBrowserGamePrefSelectorCombo.GameInfoBool;
			switch (gameInfoBool)
			{
			case GameInfoBool.EACEnabled:
				this.eacFilter = xuiC_ServerBrowserGamePrefSelectorCombo;
				break;
			case GameInfoBool.SanctionsIgnored:
				this.ignoreSanctionsFilter = xuiC_ServerBrowserGamePrefSelectorCombo;
				break;
			case GameInfoBool.Architecture64:
			case GameInfoBool.StockSettings:
			case GameInfoBool.StockFiles:
				break;
			case GameInfoBool.ModdedConfig:
				this.moddedConfigsFilter = xuiC_ServerBrowserGamePrefSelectorCombo;
				break;
			case GameInfoBool.RequiresMod:
				this.requiresModsFilter = xuiC_ServerBrowserGamePrefSelectorCombo;
				break;
			default:
				if (gameInfoBool == GameInfoBool.AllowCrossplay)
				{
					this.crossplayFilter = xuiC_ServerBrowserGamePrefSelectorCombo;
				}
				break;
			}
			XUiC_ServerBrowserGamePrefSelectorCombo xuiC_ServerBrowserGamePrefSelectorCombo2 = xuiC_ServerBrowserGamePrefSelectorCombo;
			xuiC_ServerBrowserGamePrefSelectorCombo2.OnValueChanged = (Action<IServerBrowserFilterControl>)Delegate.Combine(xuiC_ServerBrowserGamePrefSelectorCombo2.OnValueChanged, new Action<IServerBrowserFilterControl>(this.OnFilterValueChanged));
			this.allFilterControls.Add(xuiC_ServerBrowserGamePrefSelectorCombo);
		}
		foreach (XUiC_ServerBrowserGamePrefString xuiC_ServerBrowserGamePrefString in base.GetChildrenByType<XUiC_ServerBrowserGamePrefString>(null))
		{
			if (xuiC_ServerBrowserGamePrefString.GameInfoString == GameInfoString.Language)
			{
				this.languageFilter = xuiC_ServerBrowserGamePrefString;
			}
			XUiC_ServerBrowserGamePrefString xuiC_ServerBrowserGamePrefString2 = xuiC_ServerBrowserGamePrefString;
			xuiC_ServerBrowserGamePrefString2.OnValueChanged = (Action<IServerBrowserFilterControl>)Delegate.Combine(xuiC_ServerBrowserGamePrefString2.OnValueChanged, new Action<IServerBrowserFilterControl>(this.OnFilterValueChanged));
			this.allFilterControls.Add(xuiC_ServerBrowserGamePrefString);
		}
		base.OnVisiblity += this.OnVisibilityChanged;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		base.OnVisiblity -= this.OnVisibilityChanged;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
		if (XUiUtils.HotkeysAllowedFor(base.ViewComponent, false))
		{
			if (this.xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
			{
				ThreadManager.RunTaskAfterFrames(new Action(this.startShortcutPressed), 1);
			}
			if (this.xui.playerUI.playerInput.GUIActions.Cancel.WasReleased)
			{
				ThreadManager.RunTaskAfterFrames(new Action(this.backShortcutPressed), 1);
			}
		}
	}

	[XuiBindEvent("OnPress", "fullScreenCollider")]
	[XuiBindEvent("OnPress", "btnCloseFilters")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CloseFiltersButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.CloseFilters();
	}

	[XuiBindEvent("OnPress", "btnResetFilters")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetFiltersButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.resetFilters();
	}

	[XuiBindEvent("OnPress", "btnBack")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BackButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.parentBrowser.CloseBrowser();
	}

	[XuiBindEvent("OnPress", "btnStartSearch")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void StartSearchButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.CloseFilters();
		List<IServerListInterface.ServerFilter> list = new List<IServerListInterface.ServerFilter>();
		foreach (IServerBrowserFilterControl serverBrowserFilterControl in this.allFilterControls)
		{
			XUiC_ServersList.UiServerFilter filter = serverBrowserFilterControl.GetFilter();
			if (filter.Type != IServerListInterface.ServerFilter.EServerFilterType.Any)
			{
				list.Add(filter);
			}
		}
		ServerListManager.Instance.StartSearch(list);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void backShortcutPressed()
	{
		if (this.btnBack.ViewComponent.IsActiveInHierarchy)
		{
			this.BackButton_OnPress(null, 0);
			return;
		}
		this.CloseFiltersButton_OnPress(null, 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startShortcutPressed()
	{
		if (this.btnStartSearch.ViewComponent.Enabled)
		{
			this.StartSearchButton_OnPress(null, 0);
			return;
		}
		this.CloseFiltersButton_OnPress(null, 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetFilters()
	{
		foreach (IServerBrowserFilterControl serverBrowserFilterControl in this.allFilterControls)
		{
			serverBrowserFilterControl.Reset();
		}
		if (this.regionFilter != null)
		{
			string text = GamePrefs.GetString(EnumGamePrefs.Region) ?? "";
			this.regionFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(text, null));
		}
		if (this.languageFilter != null)
		{
			string text2 = GamePrefs.GetString(EnumGamePrefs.LanguageBrowser) ?? "";
			this.languageFilter.SetValue(text2);
		}
		this.applyForcedSettings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyForcedSettings()
	{
		bool flag = PermissionsManager.IsCrossplayAllowed();
		this.crossplayFilter.Enabled = flag;
		if (!flag)
		{
			this.crossplayFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(0, null));
		}
		if (!LaunchPrefs.AllowJoinConfigModded.Value)
		{
			this.moddedConfigsFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(0, null));
			this.moddedConfigsFilter.ViewComponent.IsVisible = false;
		}
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			return;
		}
		this.requiresModsFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(0, null));
		this.requiresModsFilter.ViewComponent.IsVisible = false;
		if (!Submission.Enabled)
		{
			return;
		}
		this.eacFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(1, null));
		this.eacFilter.ViewComponent.IsVisible = false;
		this.ignoreSanctionsFilter.SelectEntry(new XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue(0, null));
		this.ignoreSanctionsFilter.ViewComponent.IsVisible = false;
	}

	public void CloseFilters()
	{
		this.windowGroup.Controller.GetChildByType<XUiC_ServerBrowser>().ShowingFilters = false;
		this.xui.playerUI.CursorController.SetNavigationLockView(null, null);
	}

	public void SetServersList(XUiC_ServersList _serversList)
	{
		_serversList.OnFilterResultsChanged += this.ServersList_OnFilterResultsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ServersList_OnFilterResultsChanged(int _count)
	{
		this.ResultCount = _count;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnFilterValueChanged(IServerBrowserFilterControl _sender)
	{
		Action<IServerBrowserFilterControl> onFilterChanged = this.OnFilterChanged;
		if (onFilterChanged != null)
		{
			onFilterChanged(_sender);
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnVisibilityChanged(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
	{
		if (_visibleInScene)
		{
			this.selectInitialElement();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!this.openedBefore)
		{
			this.openedBefore = true;
			this.resetFilters();
		}
		else
		{
			this.applyForcedSettings();
		}
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void selectInitialElement()
	{
		this.GetInitialSelectedElement().Controller.SelectCursorElement(true, false);
	}

	public XUiView GetInitialSelectedElement()
	{
		return base.GetChildById(ServerListManager.Instance.IsPrefilteredSearch ? "btnStartSearch" : "btnResetFilters").ViewComponent;
	}

	public override void OnClose()
	{
		base.OnClose();
		if (this.regionFilter != null)
		{
			XUiC_ServerBrowserGamePrefSelectorCombo.GameOptionValue selection = this.regionFilter.GetSelection();
			if (selection.Type == XUiC_ServerBrowserGamePrefSelectorCombo.EOptionValueType.String)
			{
				GamePrefs.Set(EnumGamePrefs.Region, selection.StringValue);
			}
		}
		if (this.languageFilter != null)
		{
			string value = this.languageFilter.GetValue();
			GamePrefs.Set(EnumGamePrefs.LanguageBrowser, value);
		}
		GamePrefs.Set(EnumGamePrefs.IgnoreEOSSanctions, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<IServerBrowserFilterControl> allFilterControls = new List<IServerBrowserFilterControl>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo regionFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefString languageFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo crossplayFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo moddedConfigsFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo requiresModsFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo eacFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGamePrefSelectorCombo ignoreSanctionsFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openedBefore;

	[XuiBindParent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ServerBrowser parentBrowser;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_FullScreenCollider fullScreenCollider;

	[XuiBindComponent("btnStartSearch", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnStartSearch;

	[XuiBindComponent("btnResetFilters", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnResetFilters;

	[XuiBindComponent("btnBack", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnBack;

	[XuiBindComponent("btnCloseFilters", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCloseFilters;
}
