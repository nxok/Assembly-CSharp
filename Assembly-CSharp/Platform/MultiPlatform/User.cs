using System;
using System.Collections;
using System.Collections.Generic;

namespace Platform.MultiPlatform
{
	public class User : IUserClient
	{
		public void Init(IPlatform _owner)
		{
		}

		public EUserStatus UserStatus
		{
			get
			{
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				if (((crossplatformPlatform != null) ? crossplatformPlatform.User : null) == null)
				{
					return PlatformManager.NativePlatform.User.UserStatus;
				}
				return PlatformManager.CrossplatformPlatform.User.UserStatus;
			}
		}

		public event Action<IPlatform> UserLoggedIn
		{
			add
			{
				lock (this)
				{
					PlatformManager.NativePlatform.User.UserLoggedIn += value;
					IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
					if (((crossplatformPlatform != null) ? crossplatformPlatform.User : null) != null)
					{
						PlatformManager.CrossplatformPlatform.User.UserLoggedIn += value;
					}
				}
			}
			remove
			{
				lock (this)
				{
					PlatformManager.NativePlatform.User.UserLoggedIn -= value;
					IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
					if (((crossplatformPlatform != null) ? crossplatformPlatform.User : null) != null)
					{
						PlatformManager.CrossplatformPlatform.User.UserLoggedIn -= value;
					}
				}
			}
		}

		public event UserBlocksChangedCallback UserBlocksChanged
		{
			add
			{
				PlatformManager.NativePlatform.User.UserBlocksChanged += value;
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				IUserClient userClient = ((crossplatformPlatform != null) ? crossplatformPlatform.User : null);
				if (userClient != null)
				{
					userClient.UserBlocksChanged += value;
				}
			}
			remove
			{
				PlatformManager.NativePlatform.User.UserBlocksChanged -= value;
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				IUserClient userClient = ((crossplatformPlatform != null) ? crossplatformPlatform.User : null);
				if (userClient != null)
				{
					userClient.UserBlocksChanged -= value;
				}
			}
		}

		public PlatformUserIdentifierAbs PlatformUserId
		{
			get
			{
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				PlatformUserIdentifierAbs platformUserIdentifierAbs;
				if (crossplatformPlatform == null)
				{
					platformUserIdentifierAbs = null;
				}
				else
				{
					IUserClient user = crossplatformPlatform.User;
					platformUserIdentifierAbs = ((user != null) ? user.PlatformUserId : null);
				}
				return platformUserIdentifierAbs ?? PlatformManager.NativePlatform.User.PlatformUserId;
			}
		}

		public void Login(LoginUserCallback _delegate)
		{
			PlatformManager.NativePlatform.User.Login(delegate(IPlatform _nativePlatform, EApiStatusReason _nativeReason, string _statusReasonAdditionalText)
			{
				if (_nativePlatform.Api.ClientApiStatus != EApiStatus.Ok || _nativePlatform.User.UserStatus != EUserStatus.LoggedIn)
				{
					_delegate(_nativePlatform, _nativeReason, _statusReasonAdditionalText);
					return;
				}
				if (_nativeReason != EApiStatusReason.Ok)
				{
					_delegate(_nativePlatform, _nativeReason, _statusReasonAdditionalText);
					return;
				}
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				if (((crossplatformPlatform != null) ? crossplatformPlatform.User : null) == null)
				{
					_delegate(_nativePlatform, _nativeReason, _statusReasonAdditionalText);
					return;
				}
				PlatformManager.CrossplatformPlatform.User.Login(_delegate);
			});
		}

		public void PlayOffline(LoginUserCallback _delegate)
		{
			PlatformManager.NativePlatform.User.PlayOffline(delegate(IPlatform _nativePlatform, EApiStatusReason _nativeReason, string _statusReasonAdditionalText)
			{
				if (_nativePlatform.Api.ClientApiStatus != EApiStatus.Ok || _nativePlatform.User.UserStatus != EUserStatus.OfflineMode)
				{
					_delegate(_nativePlatform, _nativeReason, _statusReasonAdditionalText);
					return;
				}
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				if (((crossplatformPlatform != null) ? crossplatformPlatform.User : null) == null)
				{
					_delegate(_nativePlatform, _nativeReason, _statusReasonAdditionalText);
					return;
				}
				PlatformManager.CrossplatformPlatform.User.PlayOffline(_delegate);
			});
		}

		public void StartAdvertisePlaying(GameServerInfo _serverInfo)
		{
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			if (crossplatformPlatform != null)
			{
				IUserClient user = crossplatformPlatform.User;
				if (user != null)
				{
					user.StartAdvertisePlaying(_serverInfo);
				}
			}
			PlatformManager.NativePlatform.User.StartAdvertisePlaying(_serverInfo);
		}

		public void StopAdvertisePlaying()
		{
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			if (crossplatformPlatform != null)
			{
				IUserClient user = crossplatformPlatform.User;
				if (user != null)
				{
					user.StopAdvertisePlaying();
				}
			}
			PlatformManager.NativePlatform.User.StopAdvertisePlaying();
		}

		public void GetLoginTicket(Action<bool, byte[], string> _callback)
		{
			throw new NotImplementedException();
		}

		public string GetFriendName(PlatformUserIdentifierAbs _playerId)
		{
			throw new NotImplementedException();
		}

		public bool IsFriend(PlatformUserIdentifierAbs _playerId)
		{
			if (_playerId == null)
			{
				return false;
			}
			IPlatform platform = PlatformManager.InstanceForPlatformIdentifier(_playerId.PlatformIdentifier);
			if (platform == null)
			{
				return false;
			}
			IUserClient user = platform.User;
			return user != null && user.IsFriend(_playerId);
		}

		public bool CanShowProfile(PlatformUserIdentifierAbs _playerId)
		{
			if (!PlatformManager.NativePlatform.User.CanShowProfile(_playerId))
			{
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				bool? flag;
				if (crossplatformPlatform == null)
				{
					flag = null;
				}
				else
				{
					IUserClient user = crossplatformPlatform.User;
					flag = ((user != null) ? new bool?(user.CanShowProfile(_playerId)) : null);
				}
				bool? flag2 = flag;
				return flag2.GetValueOrDefault();
			}
			return true;
		}

		public void ShowProfile(PlatformUserIdentifierAbs _playerId)
		{
			if (PlatformManager.NativePlatform.User.CanShowProfile(_playerId))
			{
				PlatformManager.NativePlatform.User.ShowProfile(_playerId);
				return;
			}
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			bool? flag;
			if (crossplatformPlatform == null)
			{
				flag = null;
			}
			else
			{
				IUserClient user = crossplatformPlatform.User;
				flag = ((user != null) ? new bool?(user.CanShowProfile(_playerId)) : null);
			}
			bool? flag2 = flag;
			if (flag2.GetValueOrDefault())
			{
				PlatformManager.CrossplatformPlatform.User.ShowProfile(_playerId);
			}
		}

		public EUserPerms Permissions
		{
			get
			{
				if (GameManager.IsDedicatedServer)
				{
					return EUserPerms.All;
				}
				EUserPerms permissions = PlatformManager.NativePlatform.User.Permissions;
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				EUserPerms? euserPerms;
				if (crossplatformPlatform == null)
				{
					euserPerms = null;
				}
				else
				{
					IUserClient user = crossplatformPlatform.User;
					euserPerms = ((user != null) ? new EUserPerms?(user.Permissions) : null);
				}
				return permissions & (euserPerms ?? EUserPerms.All);
			}
		}

		public string GetPermissionDenyReason(EUserPerms _perms)
		{
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			string text;
			if (crossplatformPlatform == null)
			{
				text = null;
			}
			else
			{
				IUserClient user = crossplatformPlatform.User;
				text = ((user != null) ? user.GetPermissionDenyReason(_perms) : null);
			}
			string text2 = text;
			if (!string.IsNullOrEmpty(text2))
			{
				return text2;
			}
			string permissionDenyReason = PlatformManager.NativePlatform.User.GetPermissionDenyReason(_perms);
			if (!string.IsNullOrEmpty(permissionDenyReason))
			{
				return permissionDenyReason;
			}
			return null;
		}

		public IEnumerator ResolvePermissions(EUserPerms _perms, bool _canPrompt, CoroutineCancellationToken _cancellationToken = null)
		{
			if (_canPrompt && this.UserStatus != EUserStatus.LoggedIn)
			{
				Log.Out("[MultiPlatform] ResolvePermissions: Attempting Login as we're allowed to prompt.");
				bool loginAttemptDone = false;
				this.Login(delegate(IPlatform platform, EApiStatusReason reason, string text)
				{
					CoroutineCancellationToken cancellationToken2 = _cancellationToken;
					if (cancellationToken2 != null && cancellationToken2.IsCancelled())
					{
						return;
					}
					loginAttemptDone = true;
					EUserStatus userStatus = this.UserStatus;
					((userStatus == EUserStatus.LoggedIn) ? new Action<string>(Log.Out) : new Action<string>(Log.Warning))(string.Format("[MultiPlatform] {0}: Login Attempt Completed. Status: {1}, Platform: {2}, Reason: {3}, Additional Reason: '{4}'.", new object[] { "ResolvePermissions", userStatus, platform, reason, text }));
				});
				while (!loginAttemptDone)
				{
					yield return null;
					CoroutineCancellationToken cancellationToken = _cancellationToken;
					if (cancellationToken != null && cancellationToken.IsCancelled())
					{
						yield break;
					}
				}
			}
			yield return PlatformManager.NativePlatform.User.ResolvePermissions(_perms, _canPrompt, _cancellationToken);
			_perms &= PlatformManager.NativePlatform.User.Permissions;
			if (_perms == (EUserPerms)0)
			{
				yield break;
			}
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			object obj;
			if (crossplatformPlatform == null)
			{
				obj = null;
			}
			else
			{
				IUserClient user = crossplatformPlatform.User;
				obj = ((user != null) ? user.ResolvePermissions(_perms, _canPrompt, _cancellationToken) : null);
			}
			yield return obj;
			yield break;
		}

		public IEnumerator ResolveUserBlocks(IReadOnlyList<IPlatformUserBlockedResults> _results)
		{
			if (GameManager.IsDedicatedServer)
			{
				yield break;
			}
			if (!this.Permissions.HasCommunication())
			{
				PlatformUserIdentifierAbs platformUserId = this.PlatformUserId;
				foreach (IPlatformUserBlockedResults platformUserBlockedResults in _results)
				{
					if (!object.Equals(platformUserId, platformUserBlockedResults.User.PrimaryId))
					{
						platformUserBlockedResults.Block(EBlockType.TextChat);
						platformUserBlockedResults.Block(EBlockType.VoiceChat);
					}
				}
			}
			yield return PlatformManager.NativePlatform.User.ResolveUserBlocks(_results);
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			object obj;
			if (crossplatformPlatform == null)
			{
				obj = null;
			}
			else
			{
				IUserClient user = crossplatformPlatform.User;
				obj = ((user != null) ? user.ResolveUserBlocks(_results) : null);
			}
			yield return obj;
			yield break;
		}

		public EMatchmakingGroup GetMatchmakingGroup()
		{
			IUserClient user = PlatformManager.NativePlatform.User;
			if (user == null)
			{
				return EMatchmakingGroup.Dev;
			}
			return user.GetMatchmakingGroup();
		}

		public void Destroy()
		{
		}
	}
}
