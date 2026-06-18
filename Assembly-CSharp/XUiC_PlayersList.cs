using System;
using System.Collections.Generic;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayersList : XUiController
{
	public override void Init()
	{
		base.Init();
		this.playerList = (XUiV_Grid)base.GetChildById("playerList").ViewComponent;
		this.playerEntries = base.GetChildrenByType<XUiC_PlayersListEntry>(null);
		this.playerPager = (XUiC_Paging)base.GetChildById("playerPager");
		this.playerPager.OnPageChanged += this.updatePlayersList;
		this.numberOfPlayers = (XUiV_Label)base.GetChildById("numberOfPlayers").ViewComponent;
		if (Application.isPlaying)
		{
			GameManager.Instance.OnLocalPlayerChanged += this.onLocalPlayerChanged;
			PersistentPlayerList.OnPersistentAllyChangeEvent += this.OnPlayerEventHandler;
		}
		for (int i = 0; i < this.playerEntries.Length; i++)
		{
			this.playerEntries[i].PlayersList = this;
			this.playerEntries[i].IsAlternating = i % 2 == 0;
		}
		if (XUiC_PlayersList.twitchDisabled == "")
		{
			XUiC_PlayersList.twitchDisabled = Localization.Get("xuiTwitchDisabled", false, null);
			XUiC_PlayersList.twitchSafe = Localization.Get("xuiTwitchSafe", false, null);
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		this.onLocalPlayerChanged(null);
		GameManager.Instance.OnLocalPlayerChanged -= this.onLocalPlayerChanged;
		PersistentPlayerList.OnPersistentAllyChangeEvent -= this.OnPlayerEventHandler;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onLocalPlayerChanged(EntityPlayerLocal _localPlayer)
	{
		if (_localPlayer != null)
		{
			return;
		}
		if (this.persistentPlayerList != null)
		{
			this.persistentPlayerList.RemovePlayerEventHandler(new PersistentPlayerData.PlayerEventHandler(this.OnListEventHandler));
			this.persistentPlayerList = null;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!this.bOpened && this.persistentPlayerList == null)
		{
			this.persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
			this.persistentPlayerList.AddPlayerEventHandler(new PersistentPlayerData.PlayerEventHandler(this.OnListEventHandler));
		}
		this.bOpened = true;
		this.AllyCache.CopyFrom(GameManager.Instance.persistentPlayers.Allies);
		this.xui.playerUI.windowManager.Open("windowpaging", false);
		this.xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu, 0f);
		this.playerPager.Reset();
		this.updatePlayersList();
		XUiController xuiController = this.xui.FindWindowGroupByName("windowpaging");
		if (xuiController != null)
		{
			XUiC_WindowSelector childByType = xuiController.GetChildByType<XUiC_WindowSelector>();
			if (childByType != null)
			{
				childByType.SetSelected("players");
			}
		}
		this.windowGroup.isEscClosable = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		BlockedPlayerList instance = BlockedPlayerList.Instance;
		if (instance != null)
		{
			instance.MarkForWrite();
		}
		this.xui.playerUI.windowManager.Close("windowpaging");
		this.xui.CalloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.bOpened = false;
		this.AllyCache.ClearAll();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePlayersList()
	{
		this.sortedPlayerList.Clear();
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		for (int i = 0; i < GameManager.Instance.World.Players.list.Count; i++)
		{
			PersistentPlayerData persistentPlayerData = GameManager.Instance.World.Players.list[i].PersistentPlayerData;
			if (persistentPlayerData != null)
			{
				this.sortedPlayerList.Add(persistentPlayerData);
			}
		}
		foreach (PlatformUserIdentifierAbs platformUserIdentifierAbs in GameManager.Instance.persistentPlayers.Allies.EnumerateAllies(GameManager.Instance.persistentLocalPlayer.PrimaryId))
		{
			PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(platformUserIdentifierAbs);
			if (playerData != null && !(GameManager.Instance.World.GetEntity(playerData.EntityId) != null))
			{
				this.sortedPlayerList.Add(playerData);
			}
		}
		this.sortedPlayerList.Sort(new Comparison<PersistentPlayerData>(XUiC_PlayersList.PlayerComparator));
		this.numberOfPlayers.Text = this.sortedPlayerList.Count.ToString();
		this.playerPager.SetLastPageByElementsAndPageLength(this.sortedPlayerList.Count, this.playerList.Rows);
		GameServerInfo gameServerInfo = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
		bool flag = gameServerInfo != null && gameServerInfo.AllowsCrossplay;
		if (!flag)
		{
			EPlayGroup eplayGroup = DeviceFlag.StandaloneWindows.ToPlayGroup();
			for (int j = 0; j < this.sortedPlayerList.Count; j++)
			{
				EPlayGroup playGroup = this.sortedPlayerList[j].PlayGroup;
				if (playGroup != EPlayGroup.Unknown && playGroup != eplayGroup)
				{
					flag = true;
					break;
				}
			}
		}
		int k;
		for (k = 0; k < this.playerList.Rows; k++)
		{
			if (k >= this.sortedPlayerList.Count)
			{
				break;
			}
			int num = k + this.playerList.Rows * this.playerPager.GetPage();
			if (num >= this.sortedPlayerList.Count)
			{
				break;
			}
			XUiC_PlayersListEntry xuiC_PlayersListEntry = this.playerEntries[k];
			if (xuiC_PlayersListEntry != null)
			{
				PersistentPlayerData persistentPlayerData2 = this.sortedPlayerList[num];
				EntityPlayer entityPlayer2 = GameManager.Instance.World.GetEntity(persistentPlayerData2.EntityId) as EntityPlayer;
				bool flag2 = entityPlayer2 != null && entityPlayer2 != entityPlayer && entityPlayer2.IsInPartyOfLocalPlayer;
				bool flag3;
				if (!(entityPlayer2 == null))
				{
					if (entityPlayer2 != entityPlayer)
					{
						PersistentPlayerData persistentLocalPlayer = GameManager.Instance.persistentLocalPlayer;
						flag3 = persistentLocalPlayer != null && persistentLocalPlayer.IsAlly(persistentPlayerData2);
					}
					else
					{
						flag3 = false;
					}
				}
				else
				{
					flag3 = true;
				}
				bool flag4 = flag3;
				foreach (EBlockType eblockType in EnumUtils.Values<EBlockType>())
				{
					xuiC_PlayersListEntry.playerBlockStateChanged(persistentPlayerData2.PlatformData, eblockType, persistentPlayerData2.PlatformData.Blocked[eblockType].State);
				}
				if (entityPlayer2 != null)
				{
					xuiC_PlayersListEntry.IsOffline = false;
					xuiC_PlayersListEntry.EntityId = entityPlayer2.entityId;
					xuiC_PlayersListEntry.PlayerData = persistentPlayerData2;
					xuiC_PlayersListEntry.ViewComponent.IsVisible = true;
					xuiC_PlayersListEntry.PlayerName.UpdatePlayerData(persistentPlayerData2.PlayerData, flag, persistentPlayerData2.PlayerName.DisplayName);
					xuiC_PlayersListEntry.AdminSprite.IsVisible = entityPlayer2.IsAdmin;
					xuiC_PlayersListEntry.TwitchSprite.IsVisible = entityPlayer2.TwitchEnabled && entityPlayer2.TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.Enabled;
					xuiC_PlayersListEntry.TwitchDisabledSprite.IsVisible = entityPlayer2.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled || entityPlayer2.TwitchSafe;
					xuiC_PlayersListEntry.TwitchDisabledSprite.SpriteName = ((entityPlayer2.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled) ? "ui_game_symbol_twitch_action_disabled" : "ui_game_symbol_brick");
					xuiC_PlayersListEntry.TwitchDisabledSprite.ToolTip = ((entityPlayer2.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled) ? XUiC_PlayersList.twitchDisabled : XUiC_PlayersList.twitchSafe);
					xuiC_PlayersListEntry.ZombieKillsText.Text = entityPlayer2.KilledZombies.ToString();
					xuiC_PlayersListEntry.PlayerKillsText.Text = entityPlayer2.KilledPlayers.ToString();
					xuiC_PlayersListEntry.DeathsText.Text = entityPlayer2.Died.ToString();
					xuiC_PlayersListEntry.LevelText.Text = entityPlayer2.Progression.GetLevel().ToString();
					xuiC_PlayersListEntry.GamestageText.Text = entityPlayer2.gameStage.ToString();
					xuiC_PlayersListEntry.PingText.Text = ((entityPlayer2 == entityPlayer && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) ? "--" : ((entityPlayer2.pingToServer < 0) ? "--" : entityPlayer2.pingToServer.ToString()));
					xuiC_PlayersListEntry.Voice.IsVisible = entityPlayer2 != entityPlayer;
					xuiC_PlayersListEntry.Chat.IsVisible = entityPlayer2 != entityPlayer;
					xuiC_PlayersListEntry.IsFriend = flag4;
					xuiC_PlayersListEntry.ShowOnMapEnabled = (flag4 || flag2) && World.MapEnabled;
					if (flag4 || flag2)
					{
						float magnitude = (entityPlayer2.GetPosition() - entityPlayer.GetPosition()).magnitude;
						xuiC_PlayersListEntry.DistanceToFriend.Text = ValueDisplayFormatters.Distance(magnitude);
					}
					else
					{
						xuiC_PlayersListEntry.DistanceToFriend.Text = "--";
					}
					xuiC_PlayersListEntry.buttonReportPlayer.IsVisible = PlatformManager.MultiPlatform.PlayerReporting != null && entityPlayer2 != entityPlayer;
					xuiC_PlayersListEntry.IsLocalPlayer = entityPlayer2 == entityPlayer;
					if (entityPlayer2 == entityPlayer)
					{
						if (entityPlayer.partyInvites.Contains(entityPlayer2))
						{
							xuiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_Received;
						}
						else if (entityPlayer.IsInParty())
						{
							xuiC_PlayersListEntry.PartyStatus = ((entityPlayer.Party.Leader == entityPlayer) ? XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_InPartyAsLead : XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_InParty);
						}
						else
						{
							xuiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_NoParty;
						}
					}
					else if (entityPlayer.IsInParty())
					{
						bool flag5 = entityPlayer.IsPartyLead();
						if (entityPlayer.Party.MemberList.Contains(entityPlayer2))
						{
							if (flag5)
							{
								xuiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InPartyAsLead;
							}
							else
							{
								xuiC_PlayersListEntry.PartyStatus = (entityPlayer2.IsPartyLead() ? XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InPartyIsLead : XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_InParty);
							}
						}
						else if (entityPlayer2.IsInParty() && entityPlayer2.Party.IsFull())
						{
							xuiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_PartyFullAsLead;
						}
						else
						{
							xuiC_PlayersListEntry.PartyStatus = (flag5 ? XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoPartyAsLead : XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoParty);
						}
					}
					else if (entityPlayer.partyInvites.Contains(entityPlayer2))
					{
						if (entityPlayer2.IsInParty() && entityPlayer2.Party.IsFull())
						{
							xuiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoPartyAsLead;
							entityPlayer.partyInvites.Remove(entityPlayer2);
						}
						else
						{
							xuiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.LocalPlayer_Received;
						}
					}
					else
					{
						xuiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.OtherPlayer_NoPartyAsLead;
					}
				}
				else
				{
					xuiC_PlayersListEntry.IsOffline = true;
					xuiC_PlayersListEntry.EntityId = -1;
					xuiC_PlayersListEntry.PlayerData = persistentPlayerData2;
					xuiC_PlayersListEntry.PlayerName.UpdatePlayerData(persistentPlayerData2.PlayerData, flag, persistentPlayerData2.PlayerName.DisplayName ?? persistentPlayerData2.PrimaryId.CombinedString);
					xuiC_PlayersListEntry.AdminSprite.IsVisible = false;
					xuiC_PlayersListEntry.TwitchSprite.IsVisible = false;
					xuiC_PlayersListEntry.TwitchDisabledSprite.IsVisible = false;
					xuiC_PlayersListEntry.DistanceToFriend.IsVisible = true;
					xuiC_PlayersListEntry.DistanceToFriend.Text = "--";
					xuiC_PlayersListEntry.ZombieKillsText.Text = "--";
					xuiC_PlayersListEntry.PlayerKillsText.Text = "--";
					xuiC_PlayersListEntry.DeathsText.Text = "--";
					xuiC_PlayersListEntry.LevelText.Text = "--";
					xuiC_PlayersListEntry.GamestageText.Text = "--";
					xuiC_PlayersListEntry.PingText.Text = "--";
					xuiC_PlayersListEntry.Voice.IsVisible = false;
					xuiC_PlayersListEntry.Chat.IsVisible = false;
					xuiC_PlayersListEntry.IsOffline = true;
					xuiC_PlayersListEntry.IsLocalPlayer = entityPlayer2 == entityPlayer;
					xuiC_PlayersListEntry.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.Offline;
					xuiC_PlayersListEntry.labelPartyIcon.IsVisible = true;
					xuiC_PlayersListEntry.buttonReportPlayer.IsVisible = true;
					xuiC_PlayersListEntry.buttonShowOnMap.IsVisible = false;
					xuiC_PlayersListEntry.labelShowOnMap.IsVisible = true;
				}
				xuiC_PlayersListEntry.RefreshBindings();
			}
		}
		while (k < this.playerList.Rows)
		{
			XUiC_PlayersListEntry xuiC_PlayersListEntry2 = this.playerEntries[k];
			if (xuiC_PlayersListEntry2 != null)
			{
				xuiC_PlayersListEntry2.EntityId = -1;
				xuiC_PlayersListEntry2.PlayerData = null;
				xuiC_PlayersListEntry2.PlayerName.ClearPlayerData();
				xuiC_PlayersListEntry2.AdminSprite.IsVisible = false;
				xuiC_PlayersListEntry2.TwitchSprite.IsVisible = false;
				xuiC_PlayersListEntry2.TwitchDisabledSprite.IsVisible = false;
				xuiC_PlayersListEntry2.ZombieKillsText.Text = string.Empty;
				xuiC_PlayersListEntry2.PlayerKillsText.Text = string.Empty;
				xuiC_PlayersListEntry2.DeathsText.Text = string.Empty;
				xuiC_PlayersListEntry2.LevelText.Text = string.Empty;
				xuiC_PlayersListEntry2.GamestageText.Text = string.Empty;
				xuiC_PlayersListEntry2.PingText.Text = string.Empty;
				xuiC_PlayersListEntry2.Voice.IsVisible = false;
				xuiC_PlayersListEntry2.Chat.IsVisible = false;
				xuiC_PlayersListEntry2.ShowOnMapEnabled = false;
				xuiC_PlayersListEntry2.DistanceToFriend.IsVisible = false;
				xuiC_PlayersListEntry2.IsLocalPlayer = false;
				xuiC_PlayersListEntry2.PartyStatus = XUiC_PlayersListEntry.EnumPartyStatus.Offline;
				xuiC_PlayersListEntry2.buttonReportPlayer.IsVisible = false;
				xuiC_PlayersListEntry2.labelAllyIcon.IsVisible = false;
				xuiC_PlayersListEntry2.labelPartyIcon.IsVisible = false;
				xuiC_PlayersListEntry2.labelShowOnMap.IsVisible = false;
			}
			k++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnListEventHandler(PersistentPlayerData _ppData, PersistentPlayerData _otherPlayer, EnumPersistentPlayerDataReason _reason)
	{
		this.updatePlayersList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPlayerEventHandler(PlatformUserIdentifierAbs source, PlatformUserIdentifierAbs target, AllyStore.AllyEvent allyEvent)
	{
		if (source.Equals(PlatformManager.InternalLocalUserIdentifier))
		{
			PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(target);
			if (playerData != null)
			{
				EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
				string safeDisplayName = playerData.PlayerName.SafeDisplayName;
				switch (allyEvent)
				{
				case AllyStore.AllyEvent.OutgoingSent:
					GameManager.ShowTooltip(entityPlayer, "friendSentInvite", safeDisplayName, null, null, false, false, 0f);
					break;
				case AllyStore.AllyEvent.OutgoingCanceled:
				case AllyStore.AllyEvent.IncomingDeclined:
				case AllyStore.AllyEvent.AllyRemoved:
					GameManager.ShowTooltip(entityPlayer, "friendRemoved", safeDisplayName, null, null, false, false, 0f);
					break;
				case AllyStore.AllyEvent.IncomingAccepted:
					GameManager.ShowTooltip(entityPlayer, "friendInviteAccepted", safeDisplayName, null, null, false, false, 0f);
					break;
				case AllyStore.AllyEvent.OutgoingAccepted:
					GameManager.ShowTooltip(entityPlayer, "friendInviteAccepted2", safeDisplayName, null, null, false, false, 0f);
					break;
				case AllyStore.AllyEvent.OutgoingDeclined:
					GameManager.ShowTooltip(entityPlayer, "friendInviteDeclined2", safeDisplayName, null, null, false, false, 0f);
					break;
				case AllyStore.AllyEvent.RemovedByAlly:
					GameManager.ShowTooltip(entityPlayer, "friendRemoved2", safeDisplayName, null, null, false, false, 0f);
					break;
				}
			}
		}
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		if (source.Equals(internalLocalUserIdentifier))
		{
			AllyStore allies = GameManager.Instance.persistentPlayers.Allies;
			this.AllyCache.SetStatus(source, target, allies.GetStatus(source, target));
		}
		this.updatePlayersList();
	}

	public void AddInvitePress(int _otherPlayerId)
	{
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_otherPlayerId);
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		this.AllyCache.ApplyTransition(internalLocalUserIdentifier, playerDataFromEntityID.PrimaryId, true);
		GameManager.Instance.persistentPlayers.Allies.AllyUpdateRequest(playerDataFromEntityID.PrimaryId, true);
		this.updatePlayersList();
	}

	public void RemoveInvitePress(PersistentPlayerData _otherPpData)
	{
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		this.AllyCache.ApplyTransition(internalLocalUserIdentifier, _otherPpData.PrimaryId, false);
		GameManager.Instance.persistentPlayers.Allies.AllyUpdateRequest(_otherPpData.PrimaryId, false);
		this.updatePlayersList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int PlayerComparator(PersistentPlayerData a, PersistentPlayerData b)
	{
		EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(a.EntityId) as EntityPlayer;
		EntityPlayer entityPlayer2 = GameManager.Instance.World.GetEntity(b.EntityId) as EntityPlayer;
		if (entityPlayer == null && entityPlayer2 == null)
		{
			return 0;
		}
		if (entityPlayer == null)
		{
			return 1;
		}
		if (entityPlayer2 == null)
		{
			return -1;
		}
		if (entityPlayer is EntityPlayerLocal)
		{
			return -1;
		}
		if (entityPlayer2 is EntityPlayerLocal)
		{
			return 1;
		}
		PersistentPlayerData persistentLocalPlayer = GameManager.Instance.persistentLocalPlayer;
		bool flag = persistentLocalPlayer != null && persistentLocalPlayer.IsAlly(entityPlayer.PersistentPlayerData);
		bool flag2 = persistentLocalPlayer != null && persistentLocalPlayer.IsAlly(entityPlayer2.PersistentPlayerData);
		if (flag == flag2)
		{
			return entityPlayer2.Progression.GetLevel().CompareTo(entityPlayer.Progression.GetLevel());
		}
		if (!flag)
		{
			return 1;
		}
		return -1;
	}

	public void ShowOnMap(int _playerId)
	{
		Entity entity = GameManager.Instance.World.GetEntity(_playerId);
		if (entity == null)
		{
			return;
		}
		XUiC_WindowSelector.OpenSelectorAndWindow(this.xui.playerUI.entityPlayer, "map");
		((XUiC_MapArea)this.xui.GetWindow("mapArea").Controller).PositionMapAt(entity.GetPosition());
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.updateLimiter -= _dt;
		if (base.ViewComponent.IsVisible && this.updateLimiter < 0f)
		{
			this.updateLimiter = 1f;
			this.updatePlayersList();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid playerList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PlayersListEntry[] playerEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging playerPager;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label numberOfPlayers;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite reportHeaderSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public PersistentPlayerList persistentPlayerList;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateLimiter;

	public readonly AllyStore AllyCache = new AllyStore();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PersistentPlayerData> sortedPlayerList = new List<PersistentPlayerData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static string twitchDisabled = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string twitchSafe = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bOpened;
}
