using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BhvrAnalyticsServices.Interfaces;
using Platform;
using Services;
using Services.Analytics.Events;
using UnityEngine;

public class ConnectionManager : SingletonMonoBehaviour<ConnectionManager>
{
	public event Action OnDisconnectFromServer;

	public static event ConnectionManager.ClientConnectionAction OnClientAdded;

	public static event ConnectionManager.ClientConnectionAction OnClientDisconnected;

	public bool HasRunningServers
	{
		get
		{
			return this.protocolManager.HasRunningServers;
		}
	}

	public ProtocolManager.NetworkType CurrentMode
	{
		get
		{
			return this.protocolManager.CurrentMode;
		}
	}

	public bool IsServer
	{
		get
		{
			return this.protocolManager.IsServer;
		}
	}

	public bool IsClient
	{
		get
		{
			return this.protocolManager.IsClient;
		}
	}

	public bool IsSinglePlayer
	{
		get
		{
			return this.IsServer && this.ClientCount() == 0;
		}
	}

	public GameServerInfo LastGameServerInfo { get; set; }

	public GameServerInfo LocalServerInfo { get; set; }

	public GameServerInfo CurrentGameServerInfoServerOrClient
	{
		get
		{
			if (this.IsServer)
			{
				return this.LocalServerInfo;
			}
			if (!this.IsClient)
			{
				return null;
			}
			return this.LastGameServerInfo;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void singletonAwake()
	{
		this.windowManager = (GUIWindowManager)Object.FindObjectOfType(typeof(GUIWindowManager));
		if (GameUtils.GetLaunchArgument("debugnet") != null)
		{
			ConnectionManager.VerboseNetLogging = true;
		}
		this.protocolManager = new ProtocolManager();
		GamePrefs.OnGamePrefChanged += this.OnGamePrefChanged;
		NetPackageLogger.Init();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void singletonDestroy()
	{
		base.singletonDestroy();
		GamePrefs.OnGamePrefChanged -= this.OnGamePrefChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGamePrefChanged(EnumGamePrefs _pref)
	{
		if (_pref == EnumGamePrefs.ServerPassword)
		{
			this.protocolManager.SetServerPassword(GamePrefs.GetString(EnumGamePrefs.ServerPassword));
		}
	}

	public void Disconnect()
	{
		if (this.Clients != null)
		{
			for (int i = 0; i < this.Clients.List.Count; i++)
			{
				ClientInfo clientInfo = this.Clients.List[i];
				this.DisconnectClient(clientInfo, true, false);
			}
			this.Clients.Clear();
		}
		if (this.connectionToServer[0] != null)
		{
			IAntiCheatClient antiCheatClient = PlatformManager.MultiPlatform.AntiCheatClient;
			if (antiCheatClient != null)
			{
				antiCheatClient.DisconnectFromServer();
			}
			this.connectionToServer[0].Disconnect(false);
			this.LastGameServerInfo = null;
		}
		INetConnection netConnection = this.connectionToServer[1];
		if (netConnection != null)
		{
			netConnection.Disconnect(false);
		}
		this.connectionToServer[0] = null;
		this.connectionToServer[1] = null;
		if (this.IsConnected && !this.IsServer)
		{
			this.protocolManager.Disconnect();
		}
		this.IsConnected = false;
	}

	public void BeginHeartbeat()
	{
		this.countdownAnalyticsHeartbeat.ResetAndRestart();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openConnectProgressWindow(GameServerInfo _gameServerInfo)
	{
		string text = (GeneratedTextManager.IsFiltered(_gameServerInfo.ServerDisplayName) ? GeneratedTextManager.GetDisplayTextImmediately(_gameServerInfo.ServerDisplayName, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes) : _gameServerInfo.GetValue(GameInfoString.GameHost));
		string text2;
		if (!string.IsNullOrEmpty(text))
		{
			Log.Out("Connecting to server " + text + "...");
			text2 = string.Format(Localization.Get("msgConnectingToServer", false, null), Utils.EscapeBbCodes(text, false, false));
		}
		else
		{
			Log.Out(string.Concat(new string[]
			{
				"Connecting to server ",
				_gameServerInfo.GetValue(GameInfoString.IP),
				":",
				_gameServerInfo.GetValue(GameInfoInt.Port).ToString(),
				"..."
			}));
			text2 = string.Format(Localization.Get("msgConnectingToServer", false, null), _gameServerInfo.GetValue(GameInfoString.IP) + ":" + _gameServerInfo.GetValue(GameInfoInt.Port).ToString());
		}
		text2 = text2 + "\n\n[FFFFFF]" + Utils.GetCancellationMessage();
		XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, text2, delegate
		{
			this.Disconnect();
			LocalPlayerUI.primaryUI.windowManager.Open(XUiC_MainMenu.ID, true);
		}, true, true, false);
	}

	public void Connect(GameServerInfo _gameServerInfo)
	{
		if (PlatformApplicationManager.IsRestartRequired)
		{
			Log.Warning("A restart was pending when attempting to connect to a server.");
			this.Net_ConnectionFailed(Localization.Get("app_restartRequired", false, null));
			return;
		}
		if (!PermissionsManager.IsMultiplayerAllowed())
		{
			this.Net_ConnectionFailed(Localization.Get("xuiConnectFailed_MpNotAllowed", false, null));
			return;
		}
		if (ProfileSDF.CurrentProfileName().Length == 0)
		{
			string[] profiles = ProfileSDF.GetProfiles();
			if (profiles.Length != 0)
			{
				ProfileSDF.SetSelectedProfile(profiles[Random.Range(0, profiles.Length - 1)]);
			}
		}
		this.IsConnected = true;
		this.LastGameServerInfo = _gameServerInfo;
		this.openConnectProgressWindow(_gameServerInfo);
		NetPackageManager.StartClient();
		this.protocolManager.ConnectToServer(_gameServerInfo);
	}

	public void SetConnectionToServer(INetConnection[] _cons)
	{
		this.connectionToServer = _cons;
	}

	public INetConnection[] GetConnectionToServer()
	{
		return this.connectionToServer;
	}

	public void DisconnectFromServer()
	{
		Action onDisconnectFromServer = this.OnDisconnectFromServer;
		if (onDisconnectFromServer != null)
		{
			onDisconnectFromServer();
		}
		this.Disconnect();
		if (GameManager.Instance != null)
		{
			GameManager.Instance.SaveAndCleanupWorld();
		}
		if (GamePrefs.GetInt(EnumGamePrefs.AutopilotMode) > 0)
		{
			Application.Quit();
		}
	}

	public void SendToServer(NetPackage _package, bool _flush = false)
	{
		int channel = _package.Channel;
		if (this.connectionToServer[channel] == null)
		{
			if (this.IsConnected)
			{
				Log.Error("Can not queue package for server: NetConnection null");
			}
			return;
		}
		this.connectionToServer[channel].AddToSendQueue(_package);
		if (_flush)
		{
			this.connectionToServer[channel].FlushSendQueue();
		}
	}

	public NetworkConnectionError StartServers(string _password, bool _offline)
	{
		if (PlatformApplicationManager.IsRestartRequired)
		{
			Log.Warning("A restart was pending when attempting to start servers.");
			return NetworkConnectionError.RestartRequired;
		}
		NetworkConnectionError networkConnectionError = NetworkConnectionError.NoError;
		if (!GameManager.IsDedicatedServer && !_offline)
		{
			if (PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.OfflineMode)
			{
				Log.Out("Can not start servers in online mode because user is in offline mode. Starting server in offline mode.");
				_offline = true;
			}
			else if (!PermissionsManager.IsMultiplayerAllowed() || !PermissionsManager.CanHostMultiplayer())
			{
				Log.Out("Can not start servers in online mode because user does not have multiplayer hosting permissions. Starting in offline mode.");
				_offline = true;
			}
		}
		if (_offline)
		{
			this.protocolManager.StartOfflineServer();
		}
		else
		{
			networkConnectionError = this.protocolManager.StartServers(_password);
		}
		if (networkConnectionError == NetworkConnectionError.NoError)
		{
			GameManager.Instance.StartGame(_offline);
		}
		NetPackageManager.StartServer();
		return networkConnectionError;
	}

	public void MakeServerOffline()
	{
		this.protocolManager.MakeServerOffline();
	}

	public void StopServers()
	{
		Log.Out("[NET] ServerShutdown");
		this.protocolManager.StopServers();
		this.Disconnect();
		if (GameManager.Instance != null)
		{
			GameManager.Instance.SaveAndCleanupWorld();
		}
		if (this.LocalServerInfo != null)
		{
			this.LocalServerInfo.ClearOnChanged();
			this.LocalServerInfo = null;
		}
		NetPackageManager.ResetMappings();
		if (GamePrefs.GetInt(EnumGamePrefs.AutopilotMode) > 0)
		{
			Application.Quit();
		}
	}

	public void ServerReady()
	{
		if (!this.IsConnected)
		{
			this.Clients.Clear();
		}
		this.IsConnected = true;
	}

	public int ClientCount()
	{
		return this.Clients.Count;
	}

	public void AddClient(ClientInfo _cInfo)
	{
		ConnectionManager.ClientConnectionAction onClientAdded = ConnectionManager.OnClientAdded;
		if (onClientAdded != null)
		{
			onClientAdded(_cInfo);
		}
		this.Clients.Add(_cInfo);
		GameSparksCollector.SetMax(GameSparksCollector.GSDataKey.PeakConcurrentClients, null, this.ClientCount(), false, GameSparksCollector.GSDataCollection.SessionTotal);
		GameSparksCollector.SetMax(GameSparksCollector.GSDataKey.PeakConcurrentPlayers, null, this.ClientCount() + (GameManager.IsDedicatedServer ? 0 : 1), false, GameSparksCollector.GSDataCollection.SessionTotal);
	}

	public void DisconnectClient(ClientInfo _cInfo, bool _bShutdown = false, bool _clientDisconnect = false)
	{
		if (!ThreadManager.IsMainThread())
		{
			ThreadManager.AddSingleTaskMainThread("CM.DisconnectClient-" + _cInfo.ClientNumber.ToString(), delegate(object _parameter)
			{
				ValueTuple<ClientInfo, bool, bool> valueTuple = (ValueTuple<ClientInfo, bool, bool>)_parameter;
				ClientInfo item = valueTuple.Item1;
				bool item2 = valueTuple.Item2;
				bool item3 = valueTuple.Item3;
				this.DisconnectClient(item, item2, item3);
			}, new ValueTuple<ClientInfo, bool, bool>(_cInfo, _bShutdown, _clientDisconnect));
			return;
		}
		if (_cInfo == null)
		{
			Log.Error("DisconnectClient: ClientInfo is null");
			return;
		}
		if (!this.Clients.Contains(_cInfo))
		{
			Log.Warning("DisconnectClient: Player " + _cInfo.InternalId.CombinedString + " not found");
			Log.Out("From: " + StackTraceUtility.ExtractStackTrace());
			return;
		}
		ConnectionManager.ClientConnectionAction onClientDisconnected = ConnectionManager.OnClientDisconnected;
		if (onClientDisconnected != null)
		{
			onClientDisconnected(_cInfo);
		}
		ModEvents.SPlayerDisconnectedData splayerDisconnectedData = new ModEvents.SPlayerDisconnectedData(_cInfo, _bShutdown);
		ModEvents.PlayerDisconnected.Invoke(ref splayerDisconnectedData);
		Log.Out(string.Format("Player disconnected: {0}", _cInfo));
		if (_cInfo.latestPlayerData != null)
		{
			PlayerDataFile latestPlayerData = _cInfo.latestPlayerData;
			if (latestPlayerData.bModifiedSinceLastSave)
			{
				latestPlayerData.Save(GameIO.GetPlayerDataDir(), _cInfo.InternalId.CombinedString);
			}
		}
		INetConnection netConnection = _cInfo.netConnection[0];
		if (netConnection != null)
		{
			netConnection.Disconnect(false);
		}
		INetConnection netConnection2 = _cInfo.netConnection[1];
		if (netConnection2 != null)
		{
			netConnection2.Disconnect(false);
		}
		AuthorizationManager.Instance.Disconnect(_cInfo);
		if (!_bShutdown)
		{
			World world = GameManager.Instance.World;
			EntityPlayer entityPlayer = ((EntityAlive)((world != null) ? world.GetEntity(_cInfo.entityId) : null)) as EntityPlayer;
			if (entityPlayer != null)
			{
				entityPlayer.bWillRespawn = false;
				entityPlayer.PartyDisconnect();
				QuestEventManager.Current.HandlePlayerDisconnect(entityPlayer);
				LockManager.Instance.ForceUnlockByPlayer(_cInfo.entityId);
				GameManager.Instance.GameMessage(EnumGameMessages.LeftGame, entityPlayer, null);
				if (GameManager.Instance.World.m_ChunkManager != null)
				{
					GameManager.Instance.World.m_ChunkManager.RemoveChunkObserver(entityPlayer.ChunkObserver);
				}
				GameManager.Instance.World.RemoveEntity(_cInfo.entityId, EnumRemoveEntityReason.Unloaded);
				GameEventManager.Current.HandleForceBossDespawn(entityPlayer);
			}
		}
		else
		{
			World world2 = GameManager.Instance.World;
			EntityAlive entityAlive = (EntityAlive)((world2 != null) ? world2.GetEntity(_cInfo.entityId) : null);
			if (entityAlive != null)
			{
				QuestEventManager.Current.HandlePlayerDisconnect(entityAlive as EntityPlayer);
			}
		}
		if (!_bShutdown)
		{
			this.Clients.Remove(_cInfo);
			_cInfo.network.DropClient(_cInfo, _clientDisconnect);
		}
	}

	public void SetClientEntityId(ClientInfo _cInfo, int _entityId, PlayerDataFile _pdf)
	{
		_cInfo.entityId = _entityId;
		_cInfo.bAttachedToEntity = true;
		_cInfo.latestPlayerData = _pdf;
	}

	public void SendPackage(List<NetPackage> _packages, bool _onlyClientsAttachedToAnEntity = false, int _attachedToEntityId = -1, int _allButAttachedToEntityId = -1, int _entitiesInRangeOfEntity = -1, Vector3? _entitiesInRangeOfWorldPos = null, int _range = 192, bool _onlyClientsNotAttachedToAnEntity = false)
	{
		if (this.Clients == null)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		for (int i = 0; i < _packages.Count; i++)
		{
			_packages[i].RegisterSendQueue();
		}
		for (int j = 0; j < this.Clients.List.Count; j++)
		{
			ClientInfo clientInfo = this.Clients.List[j];
			if (clientInfo.loginDone && (!_onlyClientsAttachedToAnEntity || clientInfo.bAttachedToEntity) && (!_onlyClientsNotAttachedToAnEntity || !clientInfo.bAttachedToEntity) && (_attachedToEntityId == -1 || (clientInfo.bAttachedToEntity && clientInfo.entityId == _attachedToEntityId)) && (_allButAttachedToEntityId == -1 || (clientInfo.bAttachedToEntity && clientInfo.entityId != _allButAttachedToEntityId)) && (_entitiesInRangeOfEntity == -1 || GameManager.Instance.World.IsEntityInRange(_entitiesInRangeOfEntity, clientInfo.entityId, _range)) && (_entitiesInRangeOfWorldPos == null || GameManager.Instance.World.IsEntityInRange(clientInfo.entityId, _entitiesInRangeOfWorldPos.Value, _range)))
			{
				for (int k = 0; k < _packages.Count; k++)
				{
					NetPackage netPackage = _packages[k];
					clientInfo.netConnection[netPackage.Channel].AddToSendQueue(netPackage);
					if (netPackage.Channel == 1)
					{
						flag2 = true;
					}
					else
					{
						flag = true;
					}
					flag3 |= netPackage.FlushQueue;
				}
				if (flag3)
				{
					if (flag)
					{
						clientInfo.netConnection[0].FlushSendQueue();
					}
					if (flag2)
					{
						clientInfo.netConnection[1].FlushSendQueue();
					}
				}
			}
		}
		for (int l = 0; l < _packages.Count; l++)
		{
			_packages[l].SendQueueHandled();
		}
	}

	public void SendPackage(NetPackage _package, bool _onlyClientsAttachedToAnEntity = false, int _attachedToEntityId = -1, int _allButAttachedToEntityId = -1, int _entitiesInRangeOfEntity = -1, Vector3? _entitiesInRangeOfWorldPos = null, int _range = 192, bool _onlyClientsNotAttachedToAnEntity = false)
	{
		if (this.Clients == null)
		{
			return;
		}
		_package.RegisterSendQueue();
		for (int i = 0; i < this.Clients.List.Count; i++)
		{
			ClientInfo clientInfo = this.Clients.List[i];
			if (clientInfo.loginDone && (!_onlyClientsAttachedToAnEntity || clientInfo.bAttachedToEntity) && (!_onlyClientsNotAttachedToAnEntity || !clientInfo.bAttachedToEntity) && (_attachedToEntityId == -1 || (clientInfo.bAttachedToEntity && clientInfo.entityId == _attachedToEntityId)) && (_allButAttachedToEntityId == -1 || (clientInfo.bAttachedToEntity && clientInfo.entityId != _allButAttachedToEntityId)) && (_entitiesInRangeOfEntity == -1 || GameManager.Instance.World.IsEntityInRange(_entitiesInRangeOfEntity, clientInfo.entityId, _range)) && (_entitiesInRangeOfWorldPos == null || GameManager.Instance.World.IsEntityInRange(clientInfo.entityId, _entitiesInRangeOfWorldPos.Value, _range)))
			{
				clientInfo.netConnection[_package.Channel].AddToSendQueue(_package);
				if (_package.FlushQueue)
				{
					clientInfo.netConnection[_package.Channel].FlushSendQueue();
				}
			}
		}
		_package.SendQueueHandled();
	}

	public void FlushClientSendQueues()
	{
		for (int i = 0; i < this.Clients.List.Count; i++)
		{
			ClientInfo clientInfo = this.Clients.List[i];
			clientInfo.netConnection[0].FlushSendQueue();
			clientInfo.netConnection[1].FlushSendQueue();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdatePings()
	{
		for (int i = 0; i < this.Clients.List.Count; i++)
		{
			this.Clients.List[i].UpdatePing();
		}
	}

	public string GetRequiredPortsString()
	{
		return this.protocolManager.GetGamePortsString();
	}

	public void SendToClientsOrServer(NetPackage _package)
	{
		if (!this.IsServer)
		{
			this.SendToServer(_package, false);
			return;
		}
		this.SendPackage(_package, false, -1, -1, -1, null, 192, false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		this.protocolManager.Update();
		if (this.IsServer)
		{
			bool flag = Time.time - this.lastBadPacketCheck > 1f;
			if (flag)
			{
				this.lastBadPacketCheck = Time.time;
			}
			for (int i = 0; i < this.Clients.Count; i++)
			{
				ClientInfo clientInfo = this.Clients.List[i];
				if (!clientInfo.netConnection[0].IsDisconnected())
				{
					if (flag && clientInfo.entityId != -1 && !clientInfo.disconnecting && clientInfo.network.GetBadPacketCount(clientInfo) >= 3)
					{
						GameUtils.KickPlayerForClientInfo(clientInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.BadMTUPackets, 0, default(DateTime), ""));
					}
					else
					{
						this.ProcessPackages(clientInfo.netConnection[0], NetPackageDirection.ToClient, clientInfo);
						if (i < this.Clients.Count)
						{
							this.ProcessPackages(clientInfo.netConnection[1], NetPackageDirection.ToClient, clientInfo);
						}
					}
				}
			}
			this.FlushClientSendQueues();
			if (this.updateClientInfo.HasPassed() && GameManager.Instance.World != null && this.ClientCount() > 0)
			{
				this.UpdatePings();
				this.updateClientInfo.ResetAndRestart();
				this.SendPackage(NetPackageManager.GetPackage<NetPackageClientInfo>().Setup(GameManager.Instance.World, this.Clients.List), true, -1, -1, -1, null, 192, false);
			}
		}
		else
		{
			if (this.connectionToServer[0] != null && !this.connectionToServer[0].IsDisconnected())
			{
				this.ProcessPackages(this.connectionToServer[0], NetPackageDirection.ToServer, null);
				INetConnection netConnection = this.connectionToServer[0];
				if (netConnection != null)
				{
					netConnection.FlushSendQueue();
				}
			}
			if (this.connectionToServer[1] != null && !this.connectionToServer[1].IsDisconnected())
			{
				this.ProcessPackages(this.connectionToServer[1], NetPackageDirection.ToServer, null);
				INetConnection netConnection2 = this.connectionToServer[1];
				if (netConnection2 != null)
				{
					netConnection2.FlushSendQueue();
				}
			}
		}
		if (!GameManager.IsDedicatedServer && this.countdownAnalyticsHeartbeat.HasPassed())
		{
			this.countdownAnalyticsHeartbeat.ResetAndRestart();
			HeartbeatEventData heartbeatEventData = new HeartbeatEventData();
			heartbeatEventData.HeartbeatTimestamp = DateTime.UtcNow.ToString("O");
			GameServerInfo currentGameServerInfoServerOrClient = SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentGameServerInfoServerOrClient;
			heartbeatEventData.ServerId = ((currentGameServerInfoServerOrClient != null) ? currentGameServerInfoServerOrClient.GetValue(GameInfoString.UniqueId) : null);
			HeartbeatEventData heartbeatEventData2 = heartbeatEventData;
			ServiceProvider.Instance.Get<IAnalyticsService>().LogEvent(heartbeatEventData2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		this.protocolManager.LateUpdate();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessPackages(INetConnection _connection, NetPackageDirection _disallowedDirection, ClientInfo _clientInfo = null)
	{
		if (_connection == null)
		{
			Log.Error("ProcessPackages: connection == null");
			return;
		}
		_connection.GetPackages(this.packagesToProcess);
		if (this.packagesToProcess == null)
		{
			Log.Error("ProcessPackages: packages == null");
			return;
		}
		for (int i = 0; i < this.packagesToProcess.Count; i++)
		{
			NetPackage netPackage = this.packagesToProcess[i];
			if (netPackage == null)
			{
				Log.Error(string.Concat(new string[]
				{
					"ProcessPackages: packages [",
					i.ToString(),
					"] == null (packages.Count == ",
					this.packagesToProcess.Count.ToString(),
					")"
				}));
			}
			else if (netPackage.PackageDirection == _disallowedDirection)
			{
				if (_clientInfo == null)
				{
					Log.Warning(string.Format("[NET] Received package {0} which is only allowed to be sent to the server", netPackage));
				}
				else
				{
					Log.Warning(string.Format("[NET] Received package {0} which is only allowed to be sent to clients from client {1}", netPackage, _clientInfo));
				}
			}
			else if (_clientInfo != null && !netPackage.AllowedBeforeAuth && !_clientInfo.loginDone)
			{
				Log.Warning(string.Format("[NET] Received an unexpected package ({0}) before authentication was finished from client {1}", netPackage, _clientInfo));
			}
			else if (netPackage.ShouldProcess(GameManager.Instance.World, GameManager.Instance))
			{
				netPackage.ProcessPackage(GameManager.Instance.World, GameManager.Instance);
				NetPackageManager.FreePackage(netPackage);
			}
			else
			{
				netPackage.HandleSkipped(GameManager.Instance.World, GameManager.Instance);
			}
		}
	}

	public void PlayerAllowed(string _gameInfo, PlatformLobbyId _platformLobbyId, [TupleElementNames(new string[] { "userId", "token" })] ValueTuple<PlatformUserIdentifierAbs, string> _platformUserAndToken, [TupleElementNames(new string[] { "userId", "token" })] ValueTuple<PlatformUserIdentifierAbs, string> _crossplatformUserAndToken)
	{
		ConnectionManager.<>c__DisplayClass73_0 CS$<>8__locals1 = new ConnectionManager.<>c__DisplayClass73_0();
		CS$<>8__locals1.<>4__this = this;
		CS$<>8__locals1._platformUserAndToken = _platformUserAndToken;
		CS$<>8__locals1._crossplatformUserAndToken = _crossplatformUserAndToken;
		CS$<>8__locals1._platformLobbyId = _platformLobbyId;
		if (!this.IsClient)
		{
			return;
		}
		Log.Out("Player allowed");
		if (this.LastGameServerInfo.GetValue(GameInfoBool.IsDedicated))
		{
			ServerInfoCache.Instance.AddHistory(this.LastGameServerInfo);
		}
		this.LastGameServerInfo.GetValue(GameInfoString.IP);
		this.LastGameServerInfo.GetValue(GameInfoInt.Port);
		this.LastGameServerInfo = new GameServerInfo(_gameInfo);
		if ((!LaunchPrefs.AllowJoinConfigModded.Value && this.LastGameServerInfo.GetValue(GameInfoBool.ModdedConfig)) || ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() && this.LastGameServerInfo.GetValue(GameInfoBool.RequiresMod)))
		{
			CS$<>8__locals1.<PlayerAllowed>g__AuthorizerDisconnect|2(Localization.Get("auth_moddedconfigdetected", false, null));
			return;
		}
		PlatformUserIdentifierAbs item = CS$<>8__locals1._platformUserAndToken.Item1;
		if (item != null)
		{
			item.DecodeTicket(CS$<>8__locals1._platformUserAndToken.Item2);
		}
		PlatformUserIdentifierAbs item2 = CS$<>8__locals1._crossplatformUserAndToken.Item1;
		if (item2 != null)
		{
			item2.DecodeTicket(CS$<>8__locals1._crossplatformUserAndToken.Item2);
		}
		CS$<>8__locals1.authorizers = ConnectionManager.<PlayerAllowed>g__GetAuthenticationClients|73_3();
		CS$<>8__locals1.authorizerIndex = -1;
		CS$<>8__locals1.<PlayerAllowed>g__NextAuthorizer|0();
	}

	public void PlayerDenied(string _reason)
	{
		if (this.IsClient)
		{
			this.protocolManager.Disconnect();
			Log.Out("Player denied: " + _reason);
			XUiC_MessageBoxWindowGroup.ShowOk(this.windowManager.playerUI.xui, Localization.Get("mmLblErrorConnectionDeniedTitle", false, null), _reason, null, true, true, false);
		}
	}

	public void ServerConsoleCommand(ClientInfo _cInfo, string _cmd)
	{
		if (GameManager.Instance == null)
		{
			return;
		}
		if (_cmd.Length > 300)
		{
			Log.Warning("Client tried to execute command with {0} characters. First 20: '{1}'", new object[]
			{
				_cmd.Length,
				_cmd.Substring(0, 20)
			});
			return;
		}
		IConsoleCommand command = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommand(_cmd, false);
		if (command == null)
		{
			_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("Unknown command", false));
			return;
		}
		if (!command.CanExecuteForDevice)
		{
			_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup("Command not permitted on the server's device", false));
			return;
		}
		string[] commands = command.GetCommands();
		AdminTools adminTools = GameManager.Instance.adminTools;
		if (adminTools == null || !adminTools.CommandAllowedFor(commands, _cInfo))
		{
			Log.Out(string.Format("Denying command '{0}' from client {1}", _cmd, _cInfo));
			_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(string.Format(Localization.Get("msgServer25", false, null), _cmd, _cInfo.playerName), false));
			return;
		}
		if (command.IsExecuteOnClient)
		{
			Log.Out("Client {0}/{1} executing client side command: {2}", new object[]
			{
				_cInfo.InternalId.CombinedString,
				_cInfo.playerName,
				_cmd
			});
			_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(_cmd, true));
			return;
		}
		List<string> list = SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(_cmd, _cInfo);
		_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>().Setup(list, false));
	}

	public void SendLogin()
	{
		PlatformUserIdentifierAbs platformUserId = PlatformManager.NativePlatform.User.PlatformUserId;
		IAuthenticationClient authenticationClient = PlatformManager.NativePlatform.AuthenticationClient;
		ValueTuple<PlatformUserIdentifierAbs, string> valueTuple = new ValueTuple<PlatformUserIdentifierAbs, string>(platformUserId, (authenticationClient != null) ? authenticationClient.GetAuthTicket() : null);
		IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
		PlatformUserIdentifierAbs platformUserIdentifierAbs = ((crossplatformPlatform != null) ? crossplatformPlatform.User.PlatformUserId : null);
		IPlatform crossplatformPlatform2 = PlatformManager.CrossplatformPlatform;
		ValueTuple<PlatformUserIdentifierAbs, string> valueTuple2 = new ValueTuple<PlatformUserIdentifierAbs, string>(platformUserIdentifierAbs, ((crossplatformPlatform2 != null) ? crossplatformPlatform2.AuthenticationClient.GetAuthTicket() : null) ?? "");
		ulong num = (DiscordManager.Instance.IsReady ? DiscordManager.Instance.LocalUser.ID : 0UL);
		this.SendToServer(NetPackageManager.GetPackage<NetPackagePlayerLogin>().Setup(GamePrefs.GetString(EnumGamePrefs.PlayerName), valueTuple, valueTuple2, Constants.cVersionInformation.LongStringNoBuild, Constants.cVersionInformation.LongStringNoBuild, num), false);
	}

	public void Net_ConnectionFailed(string _message)
	{
		Log.Error("[NET] Connection to server failed: " + _message);
		XUiC_MessageBoxWindowGroup.ShowOk(this.windowManager.playerUI.xui, Localization.Get("mmLblErrorConnectionFailed", false, null), _message, null, true, true, false);
		this.IsConnected = false;
		IAntiCheatClient antiCheatClient = PlatformManager.MultiPlatform.AntiCheatClient;
		if (antiCheatClient == null)
		{
			return;
		}
		antiCheatClient.DisconnectFromServer();
	}

	public void Net_InvalidPassword()
	{
		XUiC_ServerPasswordWindow.OpenPasswordWindow(LocalPlayerUI.primaryUI.xui, true, ServerInfoCache.Instance.GetPassword(this.LastGameServerInfo), true, delegate(string _pwd)
		{
			ServerInfoCache.Instance.SavePassword(this.LastGameServerInfo, _pwd);
			this.Connect(this.LastGameServerInfo);
		}, delegate
		{
			this.windowManager.Open(XUiC_ServerBrowser.ID, true);
			this.Disconnect();
		});
	}

	public void Net_DisconnectedFromServer(string _reason)
	{
		Log.Out("[NET] DisconnectedFromServer: " + _reason);
		this.DisconnectFromServer();
		XUiC_MessageBoxWindowGroup.ShowOk(this.windowManager.playerUI.xui, Localization.Get("mmLblErrorConnectionLost", false, null), _reason, null, true, true, false);
	}

	public void Net_DataReceivedClient(int _channel, byte[] _data, int _size)
	{
		if (this.connectionToServer[_channel] != null)
		{
			this.connectionToServer[_channel].AppendToReaderStream(_data, _size);
		}
	}

	public void Net_DataReceivedServer(ClientInfo _cInfo, int _channel, byte[] _data, int _size)
	{
		if (_cInfo != null)
		{
			INetConnection netConnection = _cInfo.netConnection[_channel];
			if (netConnection == null)
			{
				return;
			}
			netConnection.AppendToReaderStream(_data, _size);
		}
	}

	public void Net_PlayerConnected(ClientInfo _cInfo)
	{
		Log.Out(string.Format("[NET] PlayerConnected {0}", _cInfo));
		_cInfo.netConnection[0].AddToSendQueue(NetPackageManager.GetPackage<NetPackagePackageIds>().Setup());
	}

	public void Net_PlayerDisconnected(ClientInfo _cInfo)
	{
		if (_cInfo != null)
		{
			Log.Out(string.Format("[NET] PlayerDisconnected {0}", _cInfo));
			this.DisconnectClient(_cInfo, false, false);
		}
	}

	public void SetLatencySimulation(bool _enable, int _min, int _max)
	{
		this.protocolManager.SetLatencySimulation(_enable, _min, _max);
	}

	public void SetPacketLossSimulation(bool _enable, int _chance)
	{
		this.protocolManager.SetPacketLossSimulation(_enable, _chance);
	}

	public void EnableNetworkStatistics()
	{
		this.protocolManager.EnableNetworkStatistics();
	}

	public void DisableNetworkStatistics()
	{
		this.protocolManager.DisableNetworkStatistics();
	}

	public string PrintNetworkStatistics()
	{
		return this.protocolManager.PrintNetworkStatistics();
	}

	public void ResetNetworkStatistics()
	{
		this.protocolManager.ResetNetworkStatistics();
		this.protocolManager.DisableNetworkStatistics();
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static IAuthenticationClient[] <PlayerAllowed>g__GetAuthenticationClients|73_3()
	{
		IAuthenticationClient[] array = new IAuthenticationClient[2];
		array[0] = PlatformManager.NativePlatform.AuthenticationClient;
		int num = 1;
		IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
		array[num] = ((crossplatformPlatform != null) ? crossplatformPlatform.AuthenticationClient : null);
		return array.Where((IAuthenticationClient authorizer) => authorizer != null).ToArray<IAuthenticationClient>();
	}

	public const int CHANNELCOUNT = 2;

	public static bool VerboseNetLogging;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GUIWindowManager windowManager;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public INetConnection[] connectionToServer = new INetConnection[2];

	public readonly AntiCheatEncryptionAuthClient AntiCheatEncryptionAuthClient = new AntiCheatEncryptionAuthClient();

	public readonly ClientInfoCollection Clients = new ClientInfoCollection();

	public readonly AntiCheatEncryptionAuthServer AntiCheatEncryptionAuthServer = new AntiCheatEncryptionAuthServer();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastBadPacketCheck;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int badPacketDisconnectThreshold = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ProtocolManager protocolManager;

	public bool IsConnected;

	public int ReceivedBytesThisFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly CountdownTimer updateClientInfo = new CountdownTimer(5f, true);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CountdownTimer countdownAnalyticsHeartbeat = new CountdownTimer(300f, false);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<NetPackage> packagesToProcess = new List<NetPackage>();

	public delegate void ClientConnectionAction(ClientInfo _clientInfo);
}
