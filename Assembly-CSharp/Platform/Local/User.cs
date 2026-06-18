using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Local
{
	public class User : IUserClient
	{
		public EUserStatus UserStatus
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public event Action<IPlatform> UserLoggedIn
		{
			add
			{
				lock (this)
				{
					value(this.owner);
				}
			}
			remove
			{
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

		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
			GamePrefs.OnGamePrefChanged += delegate(EnumGamePrefs _pref)
			{
				if (_pref == EnumGamePrefs.PlayerName)
				{
					this.platformUserId = new UserIdentifierLocal(GamePrefs.GetString(EnumGamePrefs.PlayerName));
				}
			};
		}

		public PlatformUserIdentifierAbs PlatformUserId
		{
			get
			{
				return this.platformUserId;
			}
		}

		public void Login(LoginUserCallback _delegate)
		{
			this.platformUserId = new UserIdentifierLocal(GamePrefs.GetString(EnumGamePrefs.PlayerName));
			_delegate(this.owner, EApiStatusReason.Ok, null);
		}

		public void PlayOffline(LoginUserCallback _delegate)
		{
			this.UserStatus = EUserStatus.OfflineMode;
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
			throw new NotImplementedException();
		}

		public string GetFriendName(PlatformUserIdentifierAbs _playerId)
		{
			return null;
		}

		public bool IsFriend(PlatformUserIdentifierAbs _playerId)
		{
			return true;
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

		public void Destroy()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public UserIdentifierLocal platformUserId;
	}
}
