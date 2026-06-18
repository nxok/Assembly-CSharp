using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using InControl;
using Steamworks;

namespace Platform.Steam
{
	public class User : IUserClient
	{
		public EUserStatus UserStatus
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		} = EUserStatus.NotAttempted;

		public event Action<IPlatform> UserLoggedIn
		{
			add
			{
				lock (this)
				{
					this.userLoggedIn = (Action<IPlatform>)Delegate.Combine(this.userLoggedIn, value);
					if (this.UserStatus == EUserStatus.LoggedIn)
					{
						value(this.owner);
					}
				}
			}
			remove
			{
				lock (this)
				{
					this.userLoggedIn = (Action<IPlatform>)Delegate.Remove(this.userLoggedIn, value);
				}
			}
		}

		public event UserBlocksChangedCallback UserBlocksChanged
		{
			add
			{
			}
			remove
			{
			}
		}

		public PlatformUserIdentifierAbs PlatformUserId
		{
			get
			{
				return this.platformUserId;
			}
		}

		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
			this.owner.Api.ClientApiInitialized += delegate
			{
				if (!GameManager.IsDedicatedServer && this.m_gameOverlayActivated == null)
				{
					this.m_gameOverlayActivated = Callback<GameOverlayActivated_t>.Create(new Callback<GameOverlayActivated_t>.DispatchDelegate(this.GameOverlayActivated));
				}
			};
		}

		public void Login(LoginUserCallback _delegate)
		{
			if (this.UserStatus == EUserStatus.LoggedIn)
			{
				Log.Out("[Steamworks.NET] Already logged in");
				_delegate(this.owner, EApiStatusReason.Ok, null);
				return;
			}
			if (this.owner.Api.ClientApiStatus == EApiStatus.PermanentError)
			{
				Log.Out("[Steamworks.NET] API could not be loaded.");
				this.UserStatus = EUserStatus.PermanentError;
				_delegate(this.owner, EApiStatusReason.ApiNotLoadable, null);
				return;
			}
			if (this.owner.Api.ClientApiStatus == EApiStatus.TemporaryError)
			{
				this.owner.Api.InitClientApis();
				if (this.owner.Api.ClientApiStatus == EApiStatus.TemporaryError)
				{
					Log.Out("[Steamworks.NET] API could not be initialized - probably Steam not running.");
					this.UserStatus = EUserStatus.TemporaryError;
					_delegate(this.owner, EApiStatusReason.SteamNotRunning, null);
					return;
				}
			}
			if (!SteamApps.BIsSubscribedApp((AppId_t)251570U))
			{
				Log.Out("[Steamworks.NET] User not licensed for app.");
				this.UserStatus = EUserStatus.PermanentError;
				_delegate(this.owner, EApiStatusReason.NoLicense, null);
				return;
			}
			string personaName = SteamFriends.GetPersonaName();
			if (string.IsNullOrEmpty(personaName))
			{
				Log.Out("[Steamworks.NET] Username not found.");
				this.UserStatus = EUserStatus.TemporaryError;
				_delegate(this.owner, EApiStatusReason.NoFriendsName, null);
				return;
			}
			GamePrefs.Set(EnumGamePrefs.PlayerName, personaName);
			this.platformUserId = new UserIdentifierSteam(SteamUser.GetSteamID());
			if (!SteamUser.BLoggedOn())
			{
				this.UserStatus = EUserStatus.OfflineMode;
				Log.Out("[Steamworks.NET] User not logged in.");
				_delegate(this.owner, EApiStatusReason.NotLoggedOn, null);
				return;
			}
			Log.Out("[Steamworks.NET] Login ok.");
			this.UserStatus = EUserStatus.LoggedIn;
			Action<IPlatform> action = this.userLoggedIn;
			if (action != null)
			{
				action(this.owner);
			}
			_delegate(this.owner, EApiStatusReason.Ok, null);
		}

		public void PlayOffline(LoginUserCallback _delegate)
		{
			if (this.UserStatus != EUserStatus.OfflineMode && this.UserStatus != EUserStatus.LoggedIn)
			{
				throw new Exception("Can not explicitly set Steam to offline mode");
			}
			this.UserStatus = EUserStatus.OfflineMode;
			Action<IPlatform> action = this.userLoggedIn;
			if (action != null)
			{
				action(this.owner);
			}
			_delegate(this.owner, EApiStatusReason.Ok, null);
		}

		public void StartAdvertisePlaying(GameServerInfo _serverInfo)
		{
		}

		public void StopAdvertisePlaying()
		{
		}

		public void GetLoginTicket(Action<bool, byte[], string> _callback)
		{
			if (this.requestEncryptedAppTicketCallback == null)
			{
				this.requestEncryptedAppTicketCallback = CallResult<EncryptedAppTicketResponse_t>.Create(new CallResult<EncryptedAppTicketResponse_t>.APIDispatchDelegate(this.EncryptedAppTicketCallback));
			}
			this.encryptedAppTicketCallback = _callback;
			SteamAPICall_t steamAPICall_t = SteamUser.RequestEncryptedAppTicket(null, 0);
			this.requestEncryptedAppTicketCallback.Set(steamAPICall_t, null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void EncryptedAppTicketCallback(EncryptedAppTicketResponse_t _result, bool _ioFailure)
		{
			if (_ioFailure || _result.m_eResult != 1)
			{
				this.<EncryptedAppTicketCallback>g__Callback|24_0(null, "[Steamworks.NET] RequestEncryptedAppTicket failed (result=" + _result.m_eResult.ToStringCached<EResult>() + ")");
				return;
			}
			uint num;
			SteamUser.GetEncryptedAppTicket(null, 0, ref num);
			if (num == 0U || num > 1024U)
			{
				this.<EncryptedAppTicketCallback>g__Callback|24_0(null, string.Format("[Steamworks.NET] Fetching encrypted app ticket size: {0}", num));
				return;
			}
			byte[] array = new byte[num];
			uint num2;
			if (!SteamUser.GetEncryptedAppTicket(array, (int)num, ref num2))
			{
				this.<EncryptedAppTicketCallback>g__Callback|24_0(null, "[Steamworks.NET] Failed fetching encrypted app ticket");
				return;
			}
			if (num2 != num)
			{
				this.<EncryptedAppTicketCallback>g__Callback|24_0(null, string.Format("[Steamworks.NET] Ticket size expected {0} does not match ticket size received {1}", num, num2));
				return;
			}
			this.<EncryptedAppTicketCallback>g__Callback|24_0(array, null);
		}

		public string GetFriendName(PlatformUserIdentifierAbs _playerId)
		{
			UserIdentifierSteam userIdentifierSteam = _playerId as UserIdentifierSteam;
			if (userIdentifierSteam == null)
			{
				return null;
			}
			return SteamFriends.GetFriendPersonaName(new CSteamID(userIdentifierSteam.SteamId));
		}

		public bool IsFriend(PlatformUserIdentifierAbs _playerId)
		{
			UserIdentifierSteam userIdentifierSteam = _playerId as UserIdentifierSteam;
			return userIdentifierSteam != null && this.owner.Api.ClientApiStatus == EApiStatus.Ok && SteamFriends.GetFriendRelationship(new CSteamID(userIdentifierSteam.SteamId)) == 3;
		}

		public EUserPerms Permissions
		{
			get
			{
				return EUserPerms.All;
			}
		}

		public string GetPermissionDenyReason(EUserPerms _perms)
		{
			return null;
		}

		public IEnumerator ResolvePermissions(EUserPerms _perms, bool _canPrompt, CoroutineCancellationToken _cancellationToken = null)
		{
			return Enumerable.Empty<object>().GetEnumerator();
		}

		public void UserAdded(PlatformUserIdentifierAbs _userId, bool _isPrimary)
		{
		}

		public IEnumerator ResolveUserBlocks(IReadOnlyList<IPlatformUserBlockedResults> _results)
		{
			return Enumerable.Empty<object>().GetEnumerator();
		}

		public EMatchmakingGroup GetMatchmakingGroup()
		{
			return EMatchmakingGroup.Retail;
		}

		public void Destroy()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void GameOverlayActivated(GameOverlayActivated_t _val)
		{
			InputManager.Enabled = _val.m_bActive == 0;
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <EncryptedAppTicketCallback>g__Callback|24_0(byte[] _ticket, string _message)
		{
			if (_message != null)
			{
				Log.Error(_message);
			}
			Action<bool, byte[], string> action = this.encryptedAppTicketCallback;
			if (action != null)
			{
				action(_message == null, _ticket, null);
			}
			this.encryptedAppTicketCallback = null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action<IPlatform> userLoggedIn;

		[PublicizedFrom(EAccessModifier.Private)]
		public Callback<GameOverlayActivated_t> m_gameOverlayActivated;

		[PublicizedFrom(EAccessModifier.Private)]
		public UserIdentifierSteam platformUserId;

		[PublicizedFrom(EAccessModifier.Private)]
		public CallResult<EncryptedAppTicketResponse_t> requestEncryptedAppTicketCallback;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action<bool, byte[], string> encryptedAppTicketCallback;
	}
}
