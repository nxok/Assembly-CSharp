using System;
using System.Collections.Generic;
using InControl;

namespace Platform
{
	public abstract class AbsPlatform : IPlatform
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public AbsPlatform()
		{
			Type typeFromHandle = typeof(PlatformFactoryAttribute);
			object[] customAttributes = base.GetType().GetCustomAttributes(typeFromHandle, false);
			if (customAttributes.Length != 1)
			{
				throw new Exception("Platform has no PlatformFactory attribute");
			}
			PlatformFactoryAttribute platformFactoryAttribute = (PlatformFactoryAttribute)customAttributes[0];
			this.PlatformIdentifier = platformFactoryAttribute.TargetPlatform;
		}

		public bool AsServerOnly { get; set; }

		public bool IsCrossplatform { get; set; }

		public string PlatformDisplayName
		{
			get
			{
				return PlatformManager.GetPlatformDisplayName(this.PlatformIdentifier);
			}
		}

		public virtual void Init()
		{
			Log.Out("[Platform] Initializing " + this.PlatformIdentifier.ToString());
			IPlatformApi api = this.Api;
			if (api != null)
			{
				api.Init(this);
			}
			IUserClient user = this.User;
			if (user != null)
			{
				user.Init(this);
			}
			IUserClient userServer = this.UserServer;
			if (userServer != null)
			{
				userServer.Init(this);
			}
			IAuthenticationClient authenticationClient = this.AuthenticationClient;
			if (authenticationClient != null)
			{
				authenticationClient.Init(this);
			}
			IAuthenticationServer authenticationServer = this.AuthenticationServer;
			if (authenticationServer != null)
			{
				authenticationServer.Init(this);
			}
			if (this.ServerListInterfaces != null)
			{
				foreach (IServerListInterface serverListInterface in this.ServerListInterfaces)
				{
					serverListInterface.Init(this);
				}
			}
			IMasterServerAnnouncer serverListAnnouncer = this.ServerListAnnouncer;
			if (serverListAnnouncer != null)
			{
				serverListAnnouncer.Init(this);
			}
			if (this.InviteListeners != null)
			{
				foreach (IJoinSessionGameInviteListener joinSessionGameInviteListener in this.InviteListeners)
				{
					if (joinSessionGameInviteListener != null)
					{
						joinSessionGameInviteListener.Init(this);
					}
				}
			}
			IMultiplayerInvitationDialog multiplayerInvitationDialog = this.MultiplayerInvitationDialog;
			if (multiplayerInvitationDialog != null)
			{
				multiplayerInvitationDialog.Init(this);
			}
			ILobbyHost lobbyHost = this.LobbyHost;
			if (lobbyHost != null)
			{
				lobbyHost.Init(this);
			}
			IPlayerInteractionsRecorder playerInteractionsRecorder = this.PlayerInteractionsRecorder;
			if (playerInteractionsRecorder != null)
			{
				playerInteractionsRecorder.Init(this);
			}
			IGameplayNotifier gameplayNotifier = this.GameplayNotifier;
			if (gameplayNotifier != null)
			{
				gameplayNotifier.Init(this);
			}
			IPartyVoice partyVoice = this.PartyVoice;
			if (partyVoice != null)
			{
				partyVoice.Init(this);
			}
			IUtils utils = this.Utils;
			if (utils != null)
			{
				utils.Init(this);
			}
			IUtils utils2 = this.Utils;
			if (utils2 != null)
			{
				utils2.ClearTempFiles();
			}
			if (this.Utils != null)
			{
				InputManager.OnDeviceDetached += this.Utils.ControllerDisconnected;
			}
			IAntiCheatClient antiCheatClient = this.AntiCheatClient;
			if (antiCheatClient != null)
			{
				antiCheatClient.Init(this);
			}
			IAntiCheatServer antiCheatServer = this.AntiCheatServer;
			if (antiCheatServer != null)
			{
				antiCheatServer.Init(this);
			}
			IPlayerReporting playerReporting = this.PlayerReporting;
			if (playerReporting != null)
			{
				playerReporting.Init(this);
			}
			ITextCensor textCensor = this.TextCensor;
			if (textCensor != null)
			{
				textCensor.Init(this);
			}
			IVirtualKeyboard virtualKeyboard = this.VirtualKeyboard;
			if (virtualKeyboard != null)
			{
				virtualKeyboard.Init(this);
			}
			IPlatformSaveGameProvider saveGameProvider = this.SaveGameProvider;
			if (saveGameProvider != null)
			{
				saveGameProvider.Init(this);
			}
			IUserDataRoaming userDataRoaming = this.UserDataRoaming;
			if (userDataRoaming != null)
			{
				userDataRoaming.Init(this);
			}
			IAchievementManager achievementManager = this.AchievementManager;
			if (achievementManager != null)
			{
				achievementManager.Init(this);
			}
			IRichPresence richPresence = this.RichPresence;
			if (richPresence != null)
			{
				richPresence.Init(this);
			}
			IRemoteFileStorage remoteFileStorage = this.RemoteFileStorage;
			if (remoteFileStorage != null)
			{
				remoteFileStorage.Init(this);
			}
			IRemotePlayerFileStorage remotePlayerFileStorage = this.RemotePlayerFileStorage;
			if (remotePlayerFileStorage != null)
			{
				remotePlayerFileStorage.Init(this);
			}
			if (this.EntitlementValidators != null)
			{
				foreach (IEntitlementValidator entitlementValidator in this.EntitlementValidators)
				{
					if (entitlementValidator != null)
					{
						entitlementValidator.Init(this);
					}
				}
			}
			IApplicationStateController applicationState = this.ApplicationState;
			if (applicationState != null)
			{
				applicationState.Init(this);
			}
			IUserDetailsService userDetailsService = this.UserDetailsService;
			if (userDetailsService != null)
			{
				userDetailsService.Init(this);
			}
			if (!GameManager.IsDedicatedServer && !this.AsServerOnly)
			{
				IPlatformApi api2 = this.Api;
				if (api2 != null)
				{
					api2.InitClientApis();
				}
			}
			if (GameManager.IsDedicatedServer)
			{
				IPlatformApi api3 = this.Api;
				if (api3 == null)
				{
					return;
				}
				api3.InitServerApis();
			}
		}

		public virtual bool HasNetworkingEnabled(IList<string> _disabledProtocolNames)
		{
			string networkProtocolName = this.NetworkProtocolName;
			if (string.IsNullOrEmpty(networkProtocolName))
			{
				return false;
			}
			string text = networkProtocolName.ToLowerInvariant();
			bool flag = GameUtils.GetLaunchArgument("no" + text) == null && !_disabledProtocolNames.Contains(text);
			if (!flag)
			{
				Log.Out("[NET] Disabling protocol: " + networkProtocolName);
			}
			return flag;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual IPlatformNetworkServer instantiateNetworkServer(ProtocolManager _protocolManager)
		{
			return null;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual IPlatformNetworkClient instantiateNetworkClient(ProtocolManager _protocolManager)
		{
			return null;
		}

		public INetworkServer GetNetworkingServer(ProtocolManager _protocolManager)
		{
			IPlatformNetworkServer platformNetworkServer;
			if ((platformNetworkServer = this.NetworkServer) == null)
			{
				platformNetworkServer = (this.NetworkServer = this.instantiateNetworkServer(_protocolManager));
			}
			return platformNetworkServer;
		}

		public INetworkClient GetNetworkingClient(ProtocolManager _protocolManager)
		{
			IPlatformNetworkClient platformNetworkClient;
			if ((platformNetworkClient = this.NetworkClient) == null)
			{
				platformNetworkClient = (this.NetworkClient = this.instantiateNetworkClient(_protocolManager));
			}
			return platformNetworkClient;
		}

		public virtual void UserAdded(PlatformUserIdentifierAbs _id, bool _isPrimary)
		{
		}

		public virtual string[] GetArgumentsForRelaunch()
		{
			return new string[0];
		}

		public abstract void CreateInstances();

		public virtual void Update()
		{
			IPlatformApi api = this.Api;
			if (api != null)
			{
				api.Update();
			}
			IMasterServerAnnouncer serverListAnnouncer = this.ServerListAnnouncer;
			if (serverListAnnouncer != null)
			{
				serverListAnnouncer.Update();
			}
			IAntiCheatServer antiCheatServer = this.AntiCheatServer;
			if (antiCheatServer != null)
			{
				antiCheatServer.Update();
			}
			PlayerInputManager input = this.Input;
			if (input != null)
			{
				input.Update();
			}
			ITextCensor textCensor = this.TextCensor;
			if (textCensor != null)
			{
				textCensor.Update();
			}
			IApplicationStateController applicationState = this.ApplicationState;
			if (applicationState == null)
			{
				return;
			}
			applicationState.Update();
		}

		public void LateUpdate()
		{
		}

		public virtual void Destroy()
		{
			this.RichPresence = null;
			IAchievementManager achievementManager = this.AchievementManager;
			if (achievementManager != null)
			{
				achievementManager.Destroy();
			}
			this.AchievementManager = null;
			this.Input = null;
			IPlatformSaveGameProvider saveGameProvider = this.SaveGameProvider;
			if (saveGameProvider != null)
			{
				saveGameProvider.Destroy();
			}
			this.SaveGameProvider = null;
			this.UserDataRoaming = null;
			IApplicationStateController applicationState = this.ApplicationState;
			if (applicationState != null)
			{
				applicationState.Destroy();
			}
			this.ApplicationState = null;
			IVirtualKeyboard virtualKeyboard = this.VirtualKeyboard;
			if (virtualKeyboard != null)
			{
				virtualKeyboard.Destroy();
			}
			this.VirtualKeyboard = null;
			this.NetworkClient = null;
			this.NetworkServer = null;
			this.PlayerReporting = null;
			this.TextCensor = null;
			IAntiCheatServer antiCheatServer = this.AntiCheatServer;
			if (antiCheatServer != null)
			{
				antiCheatServer.Destroy();
			}
			this.AntiCheatServer = null;
			IAntiCheatClient antiCheatClient = this.AntiCheatClient;
			if (antiCheatClient != null)
			{
				antiCheatClient.Destroy();
			}
			this.AntiCheatClient = null;
			this.Memory = null;
			IUtils utils = this.Utils;
			if (utils != null)
			{
				utils.ClearTempFiles();
			}
			this.Utils = null;
			IPartyVoice partyVoice = this.PartyVoice;
			if (partyVoice != null)
			{
				partyVoice.Destroy();
			}
			this.PartyVoice = null;
			this.LobbyHost = null;
			IPlayerInteractionsRecorder playerInteractionsRecorder = this.PlayerInteractionsRecorder;
			if (playerInteractionsRecorder != null)
			{
				playerInteractionsRecorder.Destroy();
			}
			this.PlayerInteractionsRecorder = null;
			this.InviteListeners = null;
			this.ServerListAnnouncer = null;
			this.ServerListInterfaces = null;
			this.AuthenticationServer = null;
			IAuthenticationClient authenticationClient = this.AuthenticationClient;
			if (authenticationClient != null)
			{
				authenticationClient.Destroy();
			}
			this.AuthenticationClient = null;
			IUserClient userServer = this.UserServer;
			if (userServer != null)
			{
				userServer.Destroy();
			}
			this.UserServer = null;
			IUserClient user = this.User;
			if (user != null)
			{
				user.Destroy();
			}
			this.User = null;
			IPlatformApi api = this.Api;
			if (api != null)
			{
				api.Destroy();
			}
			this.Api = null;
		}

		public EPlatformIdentifier PlatformIdentifier { get; }

		public IPlatformApi Api
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IUserClient User
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IUserClient UserServer
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IAuthenticationClient AuthenticationClient
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IAuthenticationServer AuthenticationServer
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IList<IServerListInterface> ServerListInterfaces
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IServerListInterface ServerLookupInterface
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IMasterServerAnnouncer ServerListAnnouncer
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public ILobbyHost LobbyHost
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IPlayerInteractionsRecorder PlayerInteractionsRecorder
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IGameplayNotifier GameplayNotifier
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IList<IJoinSessionGameInviteListener> InviteListeners
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IMultiplayerInvitationDialog MultiplayerInvitationDialog
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IPartyVoice PartyVoice
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IUtils Utils
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IPlatformMemory Memory
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IAntiCheatClient AntiCheatClient
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IAntiCheatServer AntiCheatServer
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IUserIdentifierMappingService IdMappingService
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IUserDetailsService UserDetailsService
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IPlayerReporting PlayerReporting
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public ITextCensor TextCensor
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IRemoteFileStorage RemoteFileStorage
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IRemotePlayerFileStorage RemotePlayerFileStorage
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IList<IEntitlementValidator> EntitlementValidators
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public PlayerInputManager Input
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IVirtualKeyboard VirtualKeyboard
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IAchievementManager AchievementManager
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IRichPresence RichPresence
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IApplicationStateController ApplicationState
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IPlatformSaveGameProvider SaveGameProvider
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public IUserDataRoaming UserDataRoaming
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
			set;
		}

		public virtual string NetworkProtocolName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public IPlatformNetworkServer NetworkServer;

		[PublicizedFrom(EAccessModifier.Protected)]
		public IPlatformNetworkClient NetworkClient;
	}
}
