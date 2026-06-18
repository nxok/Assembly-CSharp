using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;

namespace Platform.EOS
{
	public abstract class UserBase : IUserClient
	{
		public ConnectInterface ConnectInterface
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return ((Api)this.Owner.Api).ConnectInterface;
			}
		}

		public virtual void Init(IPlatform _owner)
		{
			this.Owner = _owner;
			this.Owner.Api.ClientApiInitialized += this.apiInitialized;
		}

		public EUserStatus UserStatus
		{
			get; [PublicizedFrom(EAccessModifier.Protected)]
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
						value(this.Owner);
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
				return this.PlatformUserIdEos;
			}
		}

		public virtual void Login(LoginUserCallback _delegate)
		{
		}

		public virtual void PlayOffline(LoginUserCallback _delegate)
		{
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
			throw new NotImplementedException();
		}

		public bool IsFriend(PlatformUserIdentifierAbs _playerId)
		{
			return false;
		}

		public abstract EUserPerms Permissions { get; }

		public virtual string GetPermissionDenyReason(EUserPerms _perms)
		{
			return null;
		}

		public virtual IEnumerator ResolvePermissions(EUserPerms _perms, bool _canPrompt, CoroutineCancellationToken _cancellationToken = null)
		{
			yield break;
		}

		public IEnumerator ResolveUserBlocks(IReadOnlyList<IPlatformUserBlockedResults> _results)
		{
			return Enumerable.Empty<object>().GetEnumerator();
		}

		public virtual void Destroy()
		{
			EosHelpers.AssertMainThread("Usr.Destroy");
			this.removeNotifications();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void apiInitialized()
		{
			EosHelpers.AssertMainThread("Usr.Init");
			this.addNotifications();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void addNotifications()
		{
			if (this.ConnectInterface == null)
			{
				return;
			}
			EosHelpers.AssertMainThread("Usr.AddNtfs");
			AddNotifyAuthExpirationOptions addNotifyAuthExpirationOptions = default(AddNotifyAuthExpirationOptions);
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.notifyAuthExpirationHandle = this.ConnectInterface.AddNotifyAuthExpiration(ref addNotifyAuthExpirationOptions, null, new OnAuthExpirationCallback(this.OnAuthExpiration));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void removeNotifications()
		{
			if (this.ConnectInterface == null)
			{
				return;
			}
			EosHelpers.AssertMainThread("Usr.RemNtfs");
			if (this.notifyAuthExpirationHandle != 0UL)
			{
				object lockObject = AntiCheatCommon.LockObject;
				lock (lockObject)
				{
					this.ConnectInterface.RemoveNotifyAuthExpiration(this.notifyAuthExpirationHandle);
				}
				this.notifyAuthExpirationHandle = 0UL;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnAuthExpiration(ref AuthExpirationCallbackInfo _data)
		{
			Log.Out("[EOS] Refreshing Login");
			this.startLogin(null, true);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract void startLogin(LoginUserCallback _delegate, bool _refreshing = false);

		[PublicizedFrom(EAccessModifier.Protected)]
		public void connectLogin(byte[] _byteTicket, string _stringTicket, ExternalCredentialType _externalType, LoginUserCallback _callback, bool _refreshing, string _displayName = null)
		{
			EosHelpers.AssertMainThread("Usr.Log");
			Utf8String utf8String = ((_byteTicket != null) ? Common.ToString(new ArraySegment<byte>(_byteTicket)) : ((_stringTicket != null) ? new Utf8String(_stringTicket) : null));
			LoginOptions loginOptions = default(LoginOptions);
			Credentials credentials = default(Credentials);
			credentials.Token = utf8String;
			credentials.Type = _externalType;
			loginOptions.Credentials = new Credentials?(credentials);
			loginOptions.UserLoginInfo = null;
			LoginOptions loginOptions2 = loginOptions;
			if (_displayName != null)
			{
				UserLoginInfo userLoginInfo = default(UserLoginInfo);
				userLoginInfo.DisplayName = _displayName;
				loginOptions2.UserLoginInfo = new UserLoginInfo?(userLoginInfo);
			}
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.ConnectInterface.Login(ref loginOptions2, null, delegate(ref LoginCallbackInfo _callbackData)
				{
					if (_callbackData.ResultCode == null)
					{
						if (!_refreshing)
						{
							string text = "[EOS] Login succeeded, PUID: ";
							ProductUserId localUserId = _callbackData.LocalUserId;
							Log.Out(text + ((localUserId != null) ? localUserId.ToString() : null));
							this.eosLoggedIn(_callbackData.LocalUserId, _callback);
							return;
						}
						Log.Out("[EOS] Login refreshed");
						return;
					}
					else if (Common.IsOperationComplete(_callbackData.ResultCode))
					{
						if (_callbackData.ResultCode == 3)
						{
							if (!_refreshing)
							{
								this.connectCreateUser(_callbackData.ContinuanceToken, _callback);
								return;
							}
							Log.Error("[EOS] Login refresh failed, invalid user");
							return;
						}
						else
						{
							Log.Warning(string.Format("[EOS] Login {0}failed: {1}", _refreshing ? "refresh " : "", _callbackData.ResultCode));
							Result resultCode = _callbackData.ResultCode;
							if (resultCode != 12)
							{
								if (resultCode != 7006)
								{
									if (resultCode != 2147483647)
									{
										this.UserStatus = EUserStatus.TemporaryError;
										LoginUserCallback callback = _callback;
										if (callback == null)
										{
											return;
										}
										callback(this.Owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached<Result>());
										return;
									}
									else
									{
										this.UserStatus = EUserStatus.OfflineMode;
										LoginUserCallback callback2 = _callback;
										if (callback2 == null)
										{
											return;
										}
										callback2(this.Owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached<Result>());
										return;
									}
								}
								else
								{
									this.UserStatus = EUserStatus.OfflineMode;
									LoginUserCallback callback3 = _callback;
									if (callback3 == null)
									{
										return;
									}
									callback3(this.Owner, EApiStatusReason.ExternalAuthUnavailable, PlatformManager.NativePlatform.PlatformDisplayName);
									return;
								}
							}
							else
							{
								this.UserStatus = EUserStatus.OfflineMode;
								LoginUserCallback callback4 = _callback;
								if (callback4 == null)
								{
									return;
								}
								callback4(this.Owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached<Result>());
								return;
							}
						}
					}
					else
					{
						Log.Error("[EOS] Login " + (_refreshing ? "refresh " : "") + "error: " + _callbackData.ResultCode.ToString());
						this.UserStatus = EUserStatus.PermanentError;
						LoginUserCallback callback5 = _callback;
						if (callback5 == null)
						{
							return;
						}
						callback5(this.Owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached<Result>());
						return;
					}
				});
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract void connectCreateUser(ContinuanceToken _continuanceToken, LoginUserCallback _callback);

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void eosLoggedIn(ProductUserId _puid, LoginUserCallback _callback)
		{
			this.PlatformUserIdEos = new UserIdentifierEos(_puid);
			if (EosHelpers.UserAccountState != EUserAccountState.NewUser)
			{
				EosHelpers.UserAccountState = EUserAccountState.ReturningUser;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void eosLoginDone(LoginUserCallback _callback)
		{
			this.UserStatus = EUserStatus.LoggedIn;
			Action<IPlatform> action = this.userLoggedIn;
			if (action != null)
			{
				action(this.Owner);
			}
			if (_callback != null)
			{
				_callback(this.Owner, EApiStatusReason.Ok, null);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public UserBase()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public IPlatform Owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public ulong notifyAuthExpirationHandle;

		[PublicizedFrom(EAccessModifier.Protected)]
		public Action<IPlatform> userLoggedIn;

		[PublicizedFrom(EAccessModifier.Protected)]
		public UserIdentifierEos PlatformUserIdEos;
	}
}
