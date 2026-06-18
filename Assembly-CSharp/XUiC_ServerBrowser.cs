using System;
using System.Collections;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowser : XUiC_PlayGameDialogBase
{
	[XuiXmlBinding("filtersvisible")]
	public bool ShowingFilters
	{
		get
		{
			return this.showingFilters;
		}
		set
		{
			if (value != this.showingFilters)
			{
				this.showingFilters = value;
				if (!this.showingFilters)
				{
					this.filtersHiddenRefreshRequired = true;
				}
				this.IsDirty = true;
			}
		}
	}

	[XuiXmlBinding("filtersbuttonselected")]
	public bool HasActiveFilters
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.anyFilterSet();
		}
	}

	[XuiXmlBinding("selectedlisttype")]
	public XUiC_ServersList.EnumServerLists SelectedListType
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ServersList xuiC_ServersList = this.serversList;
			if (xuiC_ServersList == null)
			{
				return XUiC_ServersList.EnumServerLists.Dedicated;
			}
			return xuiC_ServersList.CurrentServerTypeList;
		}
	}

	[XuiXmlBinding("typededicatedcount")]
	public int ServersCountDedicated
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ServersList xuiC_ServersList = this.serversList;
			if (xuiC_ServersList == null)
			{
				return 0;
			}
			return xuiC_ServersList.GetServerCount(XUiC_ServersList.EnumServerLists.Dedicated);
		}
	}

	[XuiXmlBinding("typepeercount")]
	public int ServersCountPeer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ServersList xuiC_ServersList = this.serversList;
			if (xuiC_ServersList == null)
			{
				return 0;
			}
			return xuiC_ServersList.GetServerCount(XUiC_ServersList.EnumServerLists.Peer);
		}
	}

	[XuiXmlBinding("typefriendscount")]
	public int ServersCountFriends
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ServersList xuiC_ServersList = this.serversList;
			if (xuiC_ServersList == null)
			{
				return 0;
			}
			return xuiC_ServersList.GetServerCount(XUiC_ServersList.EnumServerLists.Friends);
		}
	}

	[XuiXmlBinding("typehistorycount")]
	public int ServersCountHistory
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ServersList xuiC_ServersList = this.serversList;
			if (xuiC_ServersList == null)
			{
				return 0;
			}
			return xuiC_ServersList.GetServerCount(XUiC_ServersList.EnumServerLists.History);
		}
	}

	[XuiXmlBinding("typelancount")]
	public int ServersCountLan
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ServersList xuiC_ServersList = this.serversList;
			if (xuiC_ServersList == null)
			{
				return 0;
			}
			return xuiC_ServersList.GetServerCount(XUiC_ServersList.EnumServerLists.LAN);
		}
	}

	[XuiXmlBinding("isprefilteredsearch")]
	public bool IsPrefilteredSearch
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return ServerListManager.Instance.IsPrefilteredSearch;
		}
	}

	[XuiXmlBinding("limitwarningvisible")]
	public bool SearchLimitReached
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.maxSearchResultsReached;
		}
	}

	[XuiXmlBinding("resultlimit")]
	public int SearchLimit
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.searchResultLimit;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_ServerBrowser.ID = base.WindowGroup.Id;
		using (IEnumerator<XUiC_ServersList.EnumServerLists> enumerator = EnumUtils.Values<XUiC_ServersList.EnumServerLists>().GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XUiC_ServersList.EnumServerLists list = enumerator.Current;
				string text = "type" + list.ToStringCached<XUiC_ServersList.EnumServerLists>();
				XUiController xuiController;
				XUiC_Button xuiC_Button;
				if (base.TryGetChildController<XUiController>(text, out xuiController) && xuiController.TryGetChildController<XUiC_Button>("", out xuiC_Button))
				{
					xuiC_Button.OnPress += delegate(XUiController _, int _)
					{
						this.serversList.SetServerTypeFilter(list);
						this.IsDirty = true;
					};
				}
			}
		}
		this.serverFilters.SetServersList(this.serversList);
		this.setServerListType((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() ? XUiC_ServersList.EnumServerLists.Peer : XUiC_ServersList.EnumServerLists.Dedicated);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setServerListType(XUiC_ServersList.EnumServerLists _type)
	{
		this.serversList.SetServerTypeFilter(_type);
		this.serverInfo.InitializeForListFilter(_type);
		this.IsDirty = true;
	}

	[XuiBindEvent("OnPress", "btnDirectConnect")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDirectConnect_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_ServerBrowserDirectConnect.Open(this.xui, delegate
		{
			this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		}, delegate
		{
			_sender.SelectCursorElement(true, false);
		});
	}

	[XuiBindEvent("OnPress", "btnConnect")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConnect_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_ServerBrowser.<>c__DisplayClass37_0 CS$<>8__locals1 = new XUiC_ServerBrowser.<>c__DisplayClass37_0();
		CS$<>8__locals1.<>4__this = this;
		if (!this.btnConnect.ViewComponent.Enabled)
		{
			return;
		}
		XUiC_ServerBrowser.<>c__DisplayClass37_0 CS$<>8__locals2 = CS$<>8__locals1;
		XUiC_ServersList xuiC_ServersList = this.serversList;
		GameServerInfo gameServerInfo;
		if (xuiC_ServersList == null)
		{
			gameServerInfo = null;
		}
		else
		{
			XUiC_ServersList.ListEntry selectedEntryData = xuiC_ServersList.SelectedEntryData;
			gameServerInfo = ((selectedEntryData != null) ? selectedEntryData.GameServerInfo : null);
		}
		CS$<>8__locals2.gsi = gameServerInfo;
		if (CS$<>8__locals1.gsi == null)
		{
			return;
		}
		XUiC_MultiplayerPrivilegeNotification.ResolvePrivilegesWithDialog(CS$<>8__locals1.gsi.AllowsCrossplay ? (EUserPerms.Multiplayer | EUserPerms.Crossplay) : EUserPerms.Multiplayer, delegate(bool _result)
		{
			if (!_result)
			{
				return;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo = CS$<>8__locals1.gsi;
			IAntiCheatClient antiCheatClient = PlatformManager.MultiPlatform.AntiCheatClient;
			if ((antiCheatClient == null || !antiCheatClient.ClientAntiCheatEnabled()) && CS$<>8__locals1.gsi.EACEnabled)
			{
				XUi xui = CS$<>8__locals1.<>4__this.xui;
				string text = Localization.Get("xuiServerEacNeeded", false, null);
				string text2 = Localization.Get("xuiServerEacNeededText", false, null);
				Action action;
				if ((action = CS$<>8__locals1.<>9__1) == null)
				{
					action = (CS$<>8__locals1.<>9__1 = delegate
					{
						CS$<>8__locals1.<>4__this.btnConnect.SelectCursorElement(true, false);
					});
				}
				XUiC_MessageBoxWindowGroup.ShowOk(xui, text, text2, action, false, false, true);
				return;
			}
			if (CS$<>8__locals1.gsi.GetValue(GameInfoBool.IsPasswordProtected))
			{
				XUiC_ServerPasswordWindow.OpenPasswordWindow(CS$<>8__locals1.<>4__this.xui, false, ServerInfoCache.Instance.GetPassword(CS$<>8__locals1.gsi), false, new Action<string>(CS$<>8__locals1.<>4__this.connectToServer), delegate
				{
				});
				return;
			}
			CS$<>8__locals1.<>4__this.connectToServer("");
		}, -1f, false, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void connectToServer(string _password)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo != null)
		{
			ServerInfoCache.Instance.SavePassword(SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo, _password);
		}
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		SingletonMonoBehaviour<ConnectionManager>.Instance.Connect(SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
	}

	[XuiBindEvent("OnFilterChanged", "serverFilters")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnFilterValueChanged(IServerBrowserFilterControl _sender)
	{
		this.serversList.SetFilter(_sender.GetFilter());
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool anyFilterSet()
	{
		return this.serversList != null && this.serversList.GetActiveFilterCount() > 0;
	}

	[XuiBindEvent("OnPress", "btnShowFilters")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowFiltersButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.showFilters();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showFilters()
	{
		if (ServerListManager.Instance.IsPrefilteredSearch)
		{
			ServerListManager.Instance.StopSearch();
		}
		this.ShowingFilters = true;
		this.serverFilters.GetChildByType<XUiC_TabSelector>().SelectedTabIndex = 0;
		if (!ServerListManager.Instance.IsPrefilteredSearch)
		{
			this.xui.playerUI.CursorController.SetNavigationLockView(this.serverFilters.ViewComponent, this.serverFilters.GetInitialSelectedElement());
		}
	}

	public override void OnOpen()
	{
		this.IsDirty = true;
		base.OnOpen();
		this.windowGroup.isEscClosable = false;
		ServerListManager.Instance.RegisterGameServerFoundCallback(new GameServerFoundCallback(this.onGameServerFoundCallback), new MaxResultsReachedCallback(this.maxResultsCallback), new ServerSearchErrorCallback(this.serverSearchErrorCallback));
		this.btnConnect.ViewComponent.Enabled = false;
		if (ServerListManager.Instance.IsPrefilteredSearch)
		{
			this.showFilters();
			return;
		}
		ServerListManager.Instance.StartSearch(null);
		this.ShowingFilters = false;
		base.GetChildById("btnBack").SelectCursorElement(false, false);
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiC_MultiplayerPrivilegeNotification.Close();
		ServerListManager.Instance.Disconnect();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!PermissionsManager.IsMultiplayerAllowed() && this.xui.playerUI.windowManager.IsWindowOpen(this.windowGroup.Id))
		{
			this.CloseBrowser();
		}
		if (this.xui.playerUI.playerInput == null)
		{
			return;
		}
		if (XUiUtils.HotkeysAllowedFor(this.children[0].ViewComponent, false) && this.xui.playerUI.playerInput.GUIActions.Apply.WasPressed && this.btnConnect.ViewComponent.Enabled)
		{
			this.BtnConnect_OnPressed(null, 0);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void close()
	{
		this.handleBackOrEscape();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void onDirtyUpdate()
	{
		base.onDirtyUpdate();
		if (!this.filtersHiddenRefreshRequired)
		{
			return;
		}
		this.filtersHiddenRefreshRequired = false;
		if (ServerListManager.Instance.IsPrefilteredSearch)
		{
			base.GetChildById("btnBack").SelectCursorElement(true, false);
			return;
		}
		base.GetChildById("btnShowFilters").SelectCursorElement(true, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onGameServerDetailsCallback(bool _success, string _message, GameServerInfo _info)
	{
		if (!_success)
		{
			return;
		}
		this.serversList.SetAllChildrenDirty();
		XUiC_ServersList.ListEntry selectedEntryData = this.serversList.SelectedEntryData;
		if (_info.Equals((selectedEntryData != null) ? selectedEntryData.GameServerInfo : null))
		{
			this.serverInfo.SetServerInfo(_info);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onGameServerFoundCallback(IPlatform _owner, GameServerInfo _gameServerInfo, EServerRelationType _source)
	{
		if (!(_gameServerInfo.AllowsCrossplay ? PermissionsManager.IsCrossplayAllowed() : _gameServerInfo.PlayGroup.IsCurrent()))
		{
			return;
		}
		if (Submission.Enabled && !_gameServerInfo.IsCompatibleVersion)
		{
			return;
		}
		if (_gameServerInfo.IsDedicated)
		{
			this.serversList.AddGameServer(_gameServerInfo, _source);
			this.IsDirty = true;
			return;
		}
		string value = _gameServerInfo.GetValue(GameInfoString.CombinedPrimaryId);
		string value2 = _gameServerInfo.GetValue(GameInfoString.CombinedNativeId);
		if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(value2))
		{
			this.serversList.AddGameServer(_gameServerInfo, _source);
			this.IsDirty = true;
			return;
		}
		PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromCombinedString(value, true);
		PlatformUserIdentifierAbs platformUserIdentifierAbs2 = PlatformUserIdentifierAbs.FromCombinedString(value2, true);
		ThreadManager.StartCoroutine(this.resolveBlocksAndAddServerCoroutine(platformUserIdentifierAbs, platformUserIdentifierAbs2, _gameServerInfo, _source));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator resolveBlocksAndAddServerCoroutine(PlatformUserIdentifierAbs _hostPrimaryId, PlatformUserIdentifierAbs _hostNativeId, GameServerInfo _gameServerInfo, EServerRelationType _source)
	{
		IPlatformUserData userData = PlatformUserManager.GetOrCreate(_hostPrimaryId);
		userData.NativeId = _hostNativeId;
		userData.MarkBlockedStateChanged();
		yield return PlatformUserManager.ResolveUserBlockedCoroutine(userData);
		if (userData.Blocked[EBlockType.Play].IsBlocked())
		{
			yield break;
		}
		this.serversList.AddGameServer(_gameServerInfo, _source);
		this.IsDirty = true;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void maxResultsCallback(IPlatform _sourcePlatform, bool _maxReached, int _limit)
	{
		this.maxSearchResultsReached = _maxReached;
		this.searchResultLimit = _limit;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void serverSearchErrorCallback(string _message)
	{
		XUiC_MessageBoxWindowGroup.ShowOk(this.xui, Localization.Get("xuiServerBrowserSearchErrorTitle", false, null), _message, null, false, false, true);
	}

	[XuiBindEvent("ListEntryDoubleClicked", "serversList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ServersList_OnEntryDoubleClicked(XUiC_List<XUiC_ServersList.ListEntry> _list, XUiC_ServersList.ListEntry _entry)
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && this.serversList.SelectedEntryIndex >= 0)
		{
			this.BtnConnect_OnPressed(this, -1);
		}
	}

	[XuiBindEvent("SelectionChanged", "serversList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ServersList_OnSelectionChanged(XUiC_List<XUiC_ServersList.ListEntry> _list, XUiC_ServersList.ListEntry _previousEntry, XUiC_ServersList.ListEntry _newEntry)
	{
		bool flag = _newEntry != null;
		GameServerInfo gameServerInfo = (flag ? _newEntry.GameServerInfo : null);
		this.btnConnect.ViewComponent.Enabled = flag && gameServerInfo.IsCompatibleVersion;
		this.serverInfo.SetServerInfo(gameServerInfo);
		if (flag)
		{
			ServerInformationTcpClient.RequestRules(gameServerInfo, false, new ServerInformationTcpClient.RulesRequestDone(this.onGameServerDetailsCallback));
		}
	}

	[XuiBindEvent("CountsChanged", "serversList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ServersList_OnCountsChanged()
	{
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleBackOrEscape()
	{
		if (ServerListManager.Instance.IsPrefilteredSearch)
		{
			if (this.ShowingFilters)
			{
				this.CloseBrowser();
				return;
			}
			this.maxSearchResultsReached = false;
			this.serversList.RebuildList(false);
			this.showFilters();
			this.btnConnect.ViewComponent.Enabled = false;
			return;
		}
		else
		{
			if (this.ShowingFilters)
			{
				this.serverFilters.CloseFilters();
				base.GetChildById("btnShowFilters").SelectCursorElement(true, false);
				return;
			}
			this.CloseBrowser();
			return;
		}
	}

	public void CloseBrowser()
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		this.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, true);
	}

	public static string ID = "";

	[XuiBindComponent("btnConnect", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnConnect;

	[XuiBindComponent("btnDirectConnect", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button[] btnDirectConnect;

	[XuiBindComponent("btnShowFilters", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnShowFilters;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ServersList serversList;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ServerFilters serverFilters;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ServerInfo serverInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool maxSearchResultsReached;

	[PublicizedFrom(EAccessModifier.Private)]
	public int searchResultLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showingFilters;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool filtersHiddenRefreshRequired;
}
