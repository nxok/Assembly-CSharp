using System;
using System.Collections.Generic;
using System.Text;
using Epic.OnlineServices;
using Epic.OnlineServices.Sessions;
using UnityEngine;

namespace Platform.EOS
{
	public class SessionsHost : IMasterServerAnnouncer
	{
		public static string GetMatchmakingGroupTag(EMatchmakingGroup _matchmakingGroup)
		{
			if (_matchmakingGroup == EMatchmakingGroup.CertQA)
			{
				return "CertQA";
			}
			return "<WeDontCare>";
		}

		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
			this.owner.Api.ClientApiInitialized += this.apiInitialized;
		}

		public void Update()
		{
			if (!this.GameServerInitialized)
			{
				if (this.updatesSessionModification != null)
				{
					object lockObject = AntiCheatCommon.LockObject;
					lock (lockObject)
					{
						this.updatesSessionModification.Release();
					}
					this.updatesSessionModification = null;
				}
				return;
			}
			if (this.commitBackendCountdown.HasPassed())
			{
				this.commitBackendCountdown.Reset();
				this.commitBackendCountdown.SetTimeout(30f);
				if (this.updatesSessionModification != null)
				{
					this.commitSessionToBackend(false, this.updatesSessionModification);
					this.updatesSessionModification = null;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void apiInitialized()
		{
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.sessionsInterface = ((Api)this.owner.Api).PlatformInterface.GetSessionsInterface();
			}
		}

		public bool GameServerInitialized
		{
			get
			{
				return this.sessionId != null;
			}
		}

		public string GetServerPorts()
		{
			return string.Empty;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string GetBucketId()
		{
			if (!GameManager.IsDedicatedServer)
			{
				return SessionsHost.GetMatchmakingGroupTag(PlatformManager.MultiPlatform.User.GetMatchmakingGroup());
			}
			string @string = GamePrefs.GetString(EnumGamePrefs.ServerMatchmakingGroup);
			if (!string.IsNullOrEmpty(@string))
			{
				Log.Out("[EOS] using GamePref matchmaking group: " + @string);
				return @string;
			}
			return "<WeDontCare>";
		}

		public void AdvertiseServer(Action _onServerRegistered)
		{
			Log.Out("[EOS] Registering server");
			EosHelpers.AssertMainThread("SeHo.Adv");
			IUserClient user = this.owner.User;
			PlatformUserIdentifierAbs platformUserIdentifierAbs;
			if ((platformUserIdentifierAbs = ((user != null) ? user.PlatformUserId : null)) == null)
			{
				IUserClient userServer = this.owner.UserServer;
				platformUserIdentifierAbs = ((userServer != null) ? userServer.PlatformUserId : null);
			}
			UserIdentifierEos userIdentifierEos = (UserIdentifierEos)platformUserIdentifierAbs;
			if (this.sessionsInterface == null)
			{
				if (_onServerRegistered != null)
				{
					_onServerRegistered();
				}
				return;
			}
			GameServerInfo localServerInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo;
			localServerInfo.SetValue(GameInfoString.CombinedPrimaryId, (userIdentifierEos != null) ? userIdentifierEos.CombinedString : null);
			GameServerInfo gameServerInfo = localServerInfo;
			GameInfoString gameInfoString = GameInfoString.CombinedNativeId;
			IUserClient user2 = PlatformManager.NativePlatform.User;
			string text;
			if (user2 == null)
			{
				text = null;
			}
			else
			{
				PlatformUserIdentifierAbs platformUserId = user2.PlatformUserId;
				text = ((platformUserId != null) ? platformUserId.CombinedString : null);
			}
			gameServerInfo.SetValue(gameInfoString, text);
			string bucketId = this.GetBucketId();
			CreateSessionModificationOptions createSessionModificationOptions = default(CreateSessionModificationOptions);
			createSessionModificationOptions.SessionName = "GameHost";
			createSessionModificationOptions.BucketId = bucketId;
			createSessionModificationOptions.MaxPlayers = (uint)localServerInfo.GetValue(GameInfoInt.MaxPlayers);
			createSessionModificationOptions.LocalUserId = ((userIdentifierEos != null) ? userIdentifierEos.ProductUserId : null);
			createSessionModificationOptions.PresenceEnabled = false;
			createSessionModificationOptions.SanctionsEnabled = this.owner.AntiCheatServer.ServerEacEnabled();
			createSessionModificationOptions.AllowedPlatformIds = EPlayGroupExtensions.GetCurrentlyAllowedPlatformIds();
			CreateSessionModificationOptions createSessionModificationOptions2 = createSessionModificationOptions;
			object obj = AntiCheatCommon.LockObject;
			SessionModification sessionModification;
			Result result;
			lock (obj)
			{
				result = this.sessionsInterface.CreateSessionModification(ref createSessionModificationOptions2, ref sessionModification);
			}
			if (result != null)
			{
				Log.Error("[EOS] Failed creating session modification: " + result.ToStringCached<Result>());
				obj = AntiCheatCommon.LockObject;
				lock (obj)
				{
					if (sessionModification != null)
					{
						sessionModification.Release();
					}
				}
				if (_onServerRegistered != null)
				{
					_onServerRegistered();
				}
				return;
			}
			SessionModificationSetPermissionLevelOptions sessionModificationSetPermissionLevelOptions = default(SessionModificationSetPermissionLevelOptions);
			int value = localServerInfo.GetValue(GameInfoInt.ServerVisibility);
			OnlineSessionPermissionLevel onlineSessionPermissionLevel;
			if (value != 1)
			{
				if (value == 2)
				{
					onlineSessionPermissionLevel = 0;
				}
				else
				{
					onlineSessionPermissionLevel = 1;
				}
			}
			else
			{
				onlineSessionPermissionLevel = 1;
			}
			sessionModificationSetPermissionLevelOptions.PermissionLevel = onlineSessionPermissionLevel;
			SessionModificationSetPermissionLevelOptions sessionModificationSetPermissionLevelOptions2 = sessionModificationSetPermissionLevelOptions;
			obj = AntiCheatCommon.LockObject;
			lock (obj)
			{
				result = sessionModification.SetPermissionLevel(ref sessionModificationSetPermissionLevelOptions2);
			}
			if (result != null)
			{
				Log.Error("[EOS] Failed setting permission level: " + result.ToStringCached<Result>());
				obj = AntiCheatCommon.LockObject;
				lock (obj)
				{
					sessionModification.Release();
				}
				if (_onServerRegistered != null)
				{
					_onServerRegistered();
				}
				return;
			}
			SessionModificationSetJoinInProgressAllowedOptions sessionModificationSetJoinInProgressAllowedOptions = default(SessionModificationSetJoinInProgressAllowedOptions);
			sessionModificationSetJoinInProgressAllowedOptions.AllowJoinInProgress = true;
			SessionModificationSetJoinInProgressAllowedOptions sessionModificationSetJoinInProgressAllowedOptions2 = sessionModificationSetJoinInProgressAllowedOptions;
			obj = AntiCheatCommon.LockObject;
			lock (obj)
			{
				result = sessionModification.SetJoinInProgressAllowed(ref sessionModificationSetJoinInProgressAllowedOptions2);
			}
			if (result != null)
			{
				Log.Error("[EOS] Failed setting join in progress: " + result.ToStringCached<Result>());
				obj = AntiCheatCommon.LockObject;
				lock (obj)
				{
					sessionModification.Release();
				}
				if (_onServerRegistered != null)
				{
					_onServerRegistered();
				}
				return;
			}
			SessionModificationSetInvitesAllowedOptions sessionModificationSetInvitesAllowedOptions = default(SessionModificationSetInvitesAllowedOptions);
			sessionModificationSetInvitesAllowedOptions.InvitesAllowed = false;
			SessionModificationSetInvitesAllowedOptions sessionModificationSetInvitesAllowedOptions2 = sessionModificationSetInvitesAllowedOptions;
			obj = AntiCheatCommon.LockObject;
			lock (obj)
			{
				result = sessionModification.SetInvitesAllowed(ref sessionModificationSetInvitesAllowedOptions2);
			}
			if (result != null)
			{
				Log.Error("[EOS] Failed setting invites allowed: " + result.ToStringCached<Result>());
				obj = AntiCheatCommon.LockObject;
				lock (obj)
				{
					sessionModification.Release();
				}
				if (_onServerRegistered != null)
				{
					_onServerRegistered();
				}
				return;
			}
			if (!this.setBaseAttributes(sessionModification, localServerInfo))
			{
				obj = AntiCheatCommon.LockObject;
				lock (obj)
				{
					sessionModification.Release();
				}
				if (_onServerRegistered != null)
				{
					_onServerRegistered();
				}
				return;
			}
			this.onServerRegistered = _onServerRegistered;
			this.commitSessionToBackend(true, sessionModification);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void sessionRegisteredCallback(ref UpdateSessionCallbackInfo _callbackData)
		{
			if (this.onServerRegistered == null)
			{
				return;
			}
			if (_callbackData.ResultCode != null)
			{
				Log.Error("[EOS] Failed registering session on backend: " + _callbackData.ResultCode.ToStringCached<Result>());
				Log.Warning(string.Format("[EOS] Attribute count: {0}", this.registeredAttributes.Count));
				Action action = this.onServerRegistered;
				if (action != null)
				{
					action();
				}
				this.onServerRegistered = null;
				return;
			}
			this.sessionId = _callbackData.SessionId;
			Log.Out(string.Format("[EOS] Server registered, session: {0}, {1} attributes", this.sessionId, this.registeredAttributes.Count));
			GameServerInfo localServerInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo;
			localServerInfo.OnChangedString += this.updateSessionString;
			localServerInfo.OnChangedInt += this.updateSessionInt;
			localServerInfo.OnChangedBool += this.updateSessionBool;
			localServerInfo.SetValue(GameInfoString.IP, this.getPublicIpFromHostedSession());
			localServerInfo.SetValue(GameInfoString.UniqueId, this.sessionId);
			Action action2 = this.onServerRegistered;
			if (action2 != null)
			{
				action2();
			}
			this.onServerRegistered = null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string getPublicIpFromHostedSession()
		{
			CopyActiveSessionHandleOptions copyActiveSessionHandleOptions = default(CopyActiveSessionHandleOptions);
			copyActiveSessionHandleOptions.SessionName = "GameHost";
			CopyActiveSessionHandleOptions copyActiveSessionHandleOptions2 = copyActiveSessionHandleOptions;
			object obj = AntiCheatCommon.LockObject;
			ActiveSession activeSession;
			Result result;
			lock (obj)
			{
				result = this.sessionsInterface.CopyActiveSessionHandle(ref copyActiveSessionHandleOptions2, ref activeSession);
			}
			if (result != null)
			{
				Log.Error("[EOS] Failed getting active session: " + result.ToStringCached<Result>());
				return null;
			}
			ActiveSessionCopyInfoOptions activeSessionCopyInfoOptions = default(ActiveSessionCopyInfoOptions);
			obj = AntiCheatCommon.LockObject;
			ActiveSessionInfo? activeSessionInfo;
			lock (obj)
			{
				result = activeSession.CopyInfo(ref activeSessionCopyInfoOptions, ref activeSessionInfo);
			}
			if (result != null)
			{
				Log.Error("[EOS] Failed getting active session info: " + result.ToStringCached<Result>());
				obj = AntiCheatCommon.LockObject;
				lock (obj)
				{
					activeSession.Release();
				}
				return null;
			}
			string text = activeSessionInfo.Value.SessionDetails.Value.HostAddress;
			Log.Out("[EOS] Session address: " + Utils.MaskIp(text));
			obj = AntiCheatCommon.LockObject;
			lock (obj)
			{
				activeSession.Release();
			}
			return text;
		}

		public void StopServer()
		{
			EosHelpers.AssertMainThread("SeHo.Stop");
			this.onServerRegistered = null;
			if (!this.GameServerInitialized)
			{
				return;
			}
			Log.Out("[EOS] Unregistering server");
			if (SingletonMonoBehaviour<ConnectionManager>.Instance != null && SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo != null)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.OnChangedString -= this.updateSessionString;
				SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.OnChangedInt -= this.updateSessionInt;
				SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.OnChangedBool -= this.updateSessionBool;
			}
			this.registeredAttributes.Clear();
			DestroySessionOptions destroySessionOptions = default(DestroySessionOptions);
			destroySessionOptions.SessionName = "GameHost";
			DestroySessionOptions destroySessionOptions2 = destroySessionOptions;
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.sessionsInterface.DestroySession(ref destroySessionOptions2, null, delegate(ref DestroySessionCallbackInfo _callbackData)
				{
					if (_callbackData.ResultCode == null)
					{
						Log.Out("[EOS] Server unregistered");
						this.sessionId = null;
						return;
					}
					Log.Error("[EOS] Failed unregistering session on backend: " + _callbackData.ResultCode.ToStringCached<Result>());
				});
			}
		}

		public void RegisterUser(ClientInfo _cInfo)
		{
			EosHelpers.AssertMainThread("SeHo.Reg");
			RegisterPlayersOptions registerPlayersOptions = default(RegisterPlayersOptions);
			registerPlayersOptions.SessionName = "GameHost";
			registerPlayersOptions.PlayersToRegister = new ProductUserId[] { ((UserIdentifierEos)_cInfo.CrossplatformId).ProductUserId };
			RegisterPlayersOptions registerPlayersOptions2 = registerPlayersOptions;
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.sessionsInterface.RegisterPlayers(ref registerPlayersOptions2, null, delegate(ref RegisterPlayersCallbackInfo _callbackData)
				{
					if (_callbackData.ResultCode != null)
					{
						Log.Error("[EOS] Failed registering player in session: " + _callbackData.ResultCode.ToStringCached<Result>());
						return;
					}
					if (_callbackData.SanctionedPlayers != null)
					{
						ProductUserId[] sanctionedPlayers = _callbackData.SanctionedPlayers;
						for (int i = 0; i < sanctionedPlayers.Length; i++)
						{
							if (sanctionedPlayers[i] == ((UserIdentifierEos)_cInfo.CrossplatformId).ProductUserId)
							{
								Log.Out("Player " + _cInfo.playerName + " has a sanction and cannot join the session, kicking player");
								GameUtils.KickPlayerForClientInfo(_cInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.CrossPlatformAuthenticationFailed, 9, default(DateTime), "Sanction"));
							}
						}
					}
				});
			}
		}

		public void UnregisterUser(ClientInfo _cInfo)
		{
			if (((_cInfo != null) ? _cInfo.CrossplatformId : null) == null)
			{
				return;
			}
			EosHelpers.AssertMainThread("SeHo.Free");
			UnregisterPlayersOptions unregisterPlayersOptions = default(UnregisterPlayersOptions);
			unregisterPlayersOptions.SessionName = "GameHost";
			unregisterPlayersOptions.PlayersToUnregister = new ProductUserId[] { ((UserIdentifierEos)_cInfo.CrossplatformId).ProductUserId };
			UnregisterPlayersOptions unregisterPlayersOptions2 = unregisterPlayersOptions;
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.sessionsInterface.UnregisterPlayers(ref unregisterPlayersOptions2, null, delegate(ref UnregisterPlayersCallbackInfo _callbackData)
				{
					if (_callbackData.ResultCode != null)
					{
						Log.Error("[EOS] Failed unregistering player in session: " + _callbackData.ResultCode.ToStringCached<Result>());
						return;
					}
				});
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public SessionModification getSessionModificationHandle()
		{
			UpdateSessionModificationOptions updateSessionModificationOptions = default(UpdateSessionModificationOptions);
			updateSessionModificationOptions.SessionName = "GameHost";
			UpdateSessionModificationOptions updateSessionModificationOptions2 = updateSessionModificationOptions;
			object obj = AntiCheatCommon.LockObject;
			SessionModification sessionModification;
			Result result;
			lock (obj)
			{
				result = this.sessionsInterface.UpdateSessionModification(ref updateSessionModificationOptions2, ref sessionModification);
			}
			if (result != null)
			{
				Log.Error("[EOS] Failed getting session modification: " + result.ToStringCached<Result>());
				obj = AntiCheatCommon.LockObject;
				lock (obj)
				{
					sessionModification.Release();
				}
				sessionModification = null;
			}
			return sessionModification;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool addAttribute(SessionModification _sessionModificationHandle, string _key, string _value)
		{
			if (_value == null)
			{
				_value = "";
			}
			_value = _value + "~$#$~" + _value.ToLowerInvariant();
			AttributeDataValue attributeDataValue = default(AttributeDataValue);
			attributeDataValue.AsUtf8 = _value;
			return this.addAttributeInternal(_sessionModificationHandle, _key, attributeDataValue, _value);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool addAttribute(SessionModification _sessionModificationHandle, string _key, int _value)
		{
			AttributeDataValue attributeDataValue = default(AttributeDataValue);
			attributeDataValue.AsInt64 = new long?((long)_value);
			return this.addAttributeInternal(_sessionModificationHandle, _key, attributeDataValue, _value.ToString());
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool addAttribute(SessionModification _sessionModificationHandle, string _key, bool _value)
		{
			AttributeDataValue attributeDataValue = default(AttributeDataValue);
			attributeDataValue.AsBool = new bool?(_value);
			return this.addAttributeInternal(_sessionModificationHandle, _key, attributeDataValue, _value.ToString());
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool addBoolsAttribute(SessionModification _sessionModificationHandle, string _values)
		{
			string text = "-BoolValues-";
			AttributeDataValue attributeDataValue = default(AttributeDataValue);
			attributeDataValue.AsUtf8 = _values;
			return this.addAttributeInternal(_sessionModificationHandle, text, attributeDataValue, _values);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool addAttributeInternal(SessionModification _sessionModificationHandle, string _key, AttributeDataValue _value, string _valueString)
		{
			SessionModificationAddAttributeOptions sessionModificationAddAttributeOptions = default(SessionModificationAddAttributeOptions);
			sessionModificationAddAttributeOptions.AdvertisementType = 1;
			AttributeData attributeData = default(AttributeData);
			attributeData.Key = _key;
			attributeData.Value = _value;
			sessionModificationAddAttributeOptions.SessionAttribute = new AttributeData?(attributeData);
			SessionModificationAddAttributeOptions sessionModificationAddAttributeOptions2 = sessionModificationAddAttributeOptions;
			object lockObject = AntiCheatCommon.LockObject;
			Result result;
			lock (lockObject)
			{
				result = _sessionModificationHandle.AddAttribute(ref sessionModificationAddAttributeOptions2);
			}
			if (result == null)
			{
				this.registeredAttributes.Add(_key);
				return true;
			}
			Log.Error(string.Format("[EOS] Failed setting {0}th attribute '{1}' to '{2}': {3}", new object[]
			{
				this.registeredAttributes.Count + 1,
				_key,
				_valueString,
				result.ToStringCached<Result>()
			}));
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool setBaseAttributes(SessionModification _sessionModificationHandle, GameServerInfo _gameServerInfo)
		{
			foreach (GameInfoInt gameInfoInt in GameServerInfo.IntInfosInGameTags)
			{
				if (!this.addAttribute(_sessionModificationHandle, gameInfoInt.ToStringCached<GameInfoInt>(), _gameServerInfo.GetValue(gameInfoInt)))
				{
					return false;
				}
			}
			if (!this.addBoolsAttribute(_sessionModificationHandle, this.getBoolsString(_gameServerInfo)))
			{
				return false;
			}
			foreach (GameInfoString gameInfoString in GameServerInfo.SearchableStringInfos)
			{
				if (!this.addAttribute(_sessionModificationHandle, gameInfoString.ToStringCached<GameInfoString>(), _gameServerInfo.GetValue(gameInfoString)))
				{
					return false;
				}
			}
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public SessionModification getUpdateSessionModification()
		{
			if (this.updatesSessionModification == null)
			{
				this.updatesSessionModification = this.getSessionModificationHandle();
			}
			return this.updatesSessionModification;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void updateSessionString(GameServerInfo _gameServerInfo, GameInfoString _gameInfoKey)
		{
			if (!this.GameServerInitialized)
			{
				return;
			}
			if (!GameServerInfo.IsSearchable(_gameInfoKey))
			{
				return;
			}
			if (!this.commitBackendCountdown.IsRunning)
			{
				this.commitBackendCountdown.ResetAndRestart();
			}
			if (_gameInfoKey.ToStringCached<GameInfoString>().EndsWith("ID", StringComparison.OrdinalIgnoreCase))
			{
				this.commitBackendCountdown.SetTimeout(5f);
			}
			this.addAttribute(this.getUpdateSessionModification(), _gameInfoKey.ToStringCached<GameInfoString>(), _gameServerInfo.GetValue(_gameInfoKey));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void updateSessionInt(GameServerInfo _gameServerInfo, GameInfoInt _gameInfoKey)
		{
			if (!this.GameServerInitialized)
			{
				return;
			}
			if (!GameServerInfo.IsSearchable(_gameInfoKey))
			{
				return;
			}
			if (!this.commitBackendCountdown.IsRunning)
			{
				this.commitBackendCountdown.ResetAndRestart();
			}
			this.addAttribute(this.getUpdateSessionModification(), _gameInfoKey.ToStringCached<GameInfoInt>(), _gameServerInfo.GetValue(_gameInfoKey));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void updateSessionBool(GameServerInfo _gameServerInfo, GameInfoBool _gameInfoKey)
		{
			if (!this.GameServerInitialized)
			{
				return;
			}
			if (!GameServerInfo.IsSearchable(_gameInfoKey))
			{
				return;
			}
			if (!this.commitBackendCountdown.IsRunning)
			{
				this.commitBackendCountdown.ResetAndRestart();
			}
			this.addBoolsAttribute(this.getUpdateSessionModification(), this.getBoolsString(_gameServerInfo));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string getBoolsString(GameServerInfo _gameServerInfo)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(',');
			foreach (GameInfoBool gameInfoBool in GameServerInfo.BoolInfosInGameTags)
			{
				stringBuilder.Append(gameInfoBool.ToStringCached<GameInfoBool>());
				stringBuilder.Append('=');
				stringBuilder.Append(_gameServerInfo.GetValue(gameInfoBool) ? '1' : '0');
				stringBuilder.Append(',');
			}
			return stringBuilder.ToString();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void commitSessionToBackend(bool _initialRegistration, SessionModification _sessionModification)
		{
			UpdateSessionOptions updateSessionOptions = default(UpdateSessionOptions);
			updateSessionOptions.SessionModificationHandle = _sessionModification;
			UpdateSessionOptions updateSessionOptions2 = updateSessionOptions;
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.sessionsInterface.UpdateSession(ref updateSessionOptions2, new SessionsHost.SessionModificationCallbackArgs(_sessionModification, _initialRegistration, _initialRegistration ? new OnUpdateSessionCallback(this.sessionRegisteredCallback) : new OnUpdateSessionCallback(this.sessionUpdatedCallback)), new OnUpdateSessionCallback(this.commitSessionCallbackWrapper));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void commitSessionCallbackWrapper(ref UpdateSessionCallbackInfo _callbackData)
		{
			SessionsHost.SessionModificationCallbackArgs sessionModificationCallbackArgs = (SessionsHost.SessionModificationCallbackArgs)_callbackData.ClientData;
			if (_callbackData.ResultCode == 19)
			{
				Log.Warning("[EOS] Failed updating session on backend, will retry");
				return;
			}
			sessionModificationCallbackArgs.SessionModification.Release();
			sessionModificationCallbackArgs.SessionModification = null;
			if (sessionModificationCallbackArgs.IsInitialRegistration || this.GameServerInitialized)
			{
				sessionModificationCallbackArgs.Callback.Invoke(ref _callbackData);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void sessionUpdatedCallback(ref UpdateSessionCallbackInfo _callbackData)
		{
			if (_callbackData.ResultCode != null)
			{
				Log.Error("[EOS] Failed updating session on backend: " + _callbackData.ResultCode.ToStringCached<Result>() + ". From: " + StackTraceUtility.ExtractStackTrace());
				Log.Warning(string.Format("[EOS] Attribute count: {0}", this.registeredAttributes.Count));
				return;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const float sessionUpdateIntervalSecsDefault = 30f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float sessionUpdateIntervalSecsImportant = 5f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const string sessionName = "GameHost";

		public const string DefaultMatchmakingGroupTag = "<WeDontCare>";

		public const string EmptyStringAttributeValue = "##EMPTY##";

		public const string LowerCaseAttributeSeparator = "~$#$~";

		public const string BoolsAttributeName = "-BoolValues-";

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public SessionsInterface sessionsInterface;

		[PublicizedFrom(EAccessModifier.Private)]
		public string sessionId;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly CountdownTimer commitBackendCountdown = new CountdownTimer(30f, false);

		[PublicizedFrom(EAccessModifier.Private)]
		public SessionModification updatesSessionModification;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action onServerRegistered;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<string> registeredAttributes = new HashSet<string>();

		[PublicizedFrom(EAccessModifier.Private)]
		public class SessionModificationCallbackArgs
		{
			public SessionModificationCallbackArgs(SessionModification _sessionModification, bool _isInitialRegistration, OnUpdateSessionCallback _callback)
			{
				this.SessionModification = _sessionModification;
				this.IsInitialRegistration = _isInitialRegistration;
				this.Callback = _callback;
			}

			public SessionModification SessionModification;

			public readonly bool IsInitialRegistration;

			public readonly OnUpdateSessionCallback Callback;
		}
	}
}
