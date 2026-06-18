using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.XGamingRuntime;
using Unity.XGamingRuntime.Interop;

namespace Platform.XBL
{
	public class User : IUserClient
	{
		[PublicizedFrom(EAccessModifier.Private)]
		static User()
		{
			Dictionary<EBlockType, XblPermission> dictionary = EnumUtils.Values<EBlockType>().ToDictionary((EBlockType blockType) => blockType, delegate(EBlockType blockType)
			{
				XblPermission xblPermission3;
				switch (blockType)
				{
				case EBlockType.TextChat:
					xblPermission3 = 1000;
					break;
				case EBlockType.VoiceChat:
					xblPermission3 = 1002;
					break;
				case EBlockType.Play:
					xblPermission3 = 1014;
					break;
				default:
					throw new NotImplementedException(string.Format("Mapping from {0}.{1} to {2} not implemented!", "EBlockType", blockType, "XblPermission"));
				}
				return xblPermission3;
			});
			User.userBlockedPermissions = dictionary.Values.ToArray<XblPermission>();
			User.xblPermissionToBlockType = new EnumDictionary<XblPermission, EBlockType>();
			foreach (KeyValuePair<EBlockType, XblPermission> keyValuePair in dictionary)
			{
				EBlockType eblockType;
				XblPermission xblPermission;
				keyValuePair.Deconstruct(out eblockType, out xblPermission);
				EBlockType eblockType2 = eblockType;
				XblPermission xblPermission2 = xblPermission;
				User.xblPermissionToBlockType.Add(xblPermission2, eblockType2);
			}
			User.userBlockedAnonymousTypes = new XblAnonymousUserType[] { 2, 1 };
		}

		public bool IsMultiplayerActivityActive
		{
			get
			{
				object activityLock = this.m_activityLock;
				bool isActivityActive;
				lock (activityLock)
				{
					isActivityActive = this.m_isActivityActive;
				}
				return isActivityActive;
			}
		}

		public XUserLocalId LocalID
		{
			get
			{
				return this.m_userLocalId;
			}
		}

		public ulong Xuid
		{
			get
			{
				return this.m_userXuid;
			}
		}

		public XblContextHandle XblContextHandle
		{
			get
			{
				return this.m_contextHandle;
			}
		}

		public XUserHandle UserHandle
		{
			get
			{
				return this.m_userHandle;
			}
		}

		public EUserStatus UserStatus
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		} = EUserStatus.NotAttempted;

		public event Action<XUserHandle> UserHandleReady
		{
			add
			{
				object userHandleLock = this.m_userHandleLock;
				lock (userHandleLock)
				{
					this.m_userHandleReady = (Action<XUserHandle>)Delegate.Combine(this.m_userHandleReady, value);
					if (this.m_userHandle != null)
					{
						value(this.m_userHandle);
					}
				}
			}
			remove
			{
				object userHandleLock = this.m_userHandleLock;
				lock (userHandleLock)
				{
					this.m_userHandleReady = (Action<XUserHandle>)Delegate.Remove(this.m_userHandleReady, value);
				}
			}
		}

		public event Action<IPlatform> UserLoggedIn
		{
			add
			{
				object loginLock = this.m_loginLock;
				lock (loginLock)
				{
					this.m_userLoggedIn = (Action<IPlatform>)Delegate.Combine(this.m_userLoggedIn, value);
					if (this.UserStatus == EUserStatus.LoggedIn)
					{
						value(this.m_owner);
					}
				}
			}
			remove
			{
				object loginLock = this.m_loginLock;
				lock (loginLock)
				{
					this.m_userLoggedIn = (Action<IPlatform>)Delegate.Remove(this.m_userLoggedIn, value);
				}
			}
		}

		public event UserBlocksChangedCallback UserBlocksChanged;

		public PlatformUserIdentifierAbs PlatformUserId
		{
			get
			{
				return this.m_idProvider.Id;
			}
		}

		public string GamerTag
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public SocialManagerXbl SocialManager
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public MultiplayerActivityQueryManager MultiplayerActivityQueryManager
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public XblSandboxHelper SandboxHelper
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		} = new XblSandboxHelper();

		public void Init(IPlatform _owner)
		{
			this.m_owner = _owner;
			this.m_api = _owner.Api;
			this.m_appState = _owner.ApplicationState;
			this.m_api.ClientApiInitialized += this.OnClientApiInitialized;
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			this.m_idProvider = new IdProviderGameCore(this, (crossplatformPlatform != null) ? crossplatformPlatform.User : null);
			XblXuidMapper.XuidMapped += this.OnXuidMapped;
			this.UserLoggedIn += this.SetNameOnPlayerLogin;
			this.m_appState.OnNetworkStateChanged += this.OnNetworkStateChanged;
		}

		public void Destroy()
		{
			this.m_appState.OnNetworkStateChanged -= this.OnNetworkStateChanged;
			if (this.m_userHandle != null)
			{
				SDK.XUserCloseHandle(this.m_userHandle);
				XblHelpers.LogHR(0, "Close User Handle.", false);
				this.m_userHandle = null;
				this.m_userLocalId = null;
			}
			this.m_loginUserCallback = null;
			this.m_api.ClientApiInitialized -= this.OnClientApiInitialized;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetNameOnPlayerLogin(IPlatform platform)
		{
			string text;
			if (HR.FAILED(SDK.XUserGetGamertag(this.UserHandle, 0, ref text)))
			{
				Log.Error("[XBL] Failed to get player's gamertag");
				return;
			}
			GamePrefs.Set(EnumGamePrefs.PlayerName, text);
			this.GamerTag = text;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnClientApiInitialized()
		{
			this.SandboxHelper.RefreshSandboxId();
			SDK.XUserAddAsync(1, new XUserAddCompleted(this.<OnClientApiInitialized>g__XUserAddAsyncSilentCompletionRoutine|77_0));
		}

		public void Login(LoginUserCallback _delegate)
		{
			User.<>c__DisplayClass78_0 CS$<>8__locals1 = new User.<>c__DisplayClass78_0();
			CS$<>8__locals1._delegate = _delegate;
			CS$<>8__locals1.<>4__this = this;
			object loginLock = this.m_loginLock;
			lock (loginLock)
			{
				if (this.m_loginDone)
				{
					this.UserStatus = this.m_loginUserStatus;
					XblHelpers.LogHR((this.m_loginUserStatus == EUserStatus.LoggedIn) ? 0 : (-2147467259), string.Format("Login with cached initial results. User Status: {0}, Reason: {1}, Additional: '{2}'.", this.m_loginUserStatus, this.m_loginUserCallbackReason, this.m_loginUserCallbackReasonAdditional), false);
					CS$<>8__locals1.<Login>g__Callback|0(this.m_owner, this.m_loginUserCallbackReason, this.m_loginUserCallbackReasonAdditional);
				}
				else
				{
					this.m_loginUserCallback = new LoginUserCallback(CS$<>8__locals1.<Login>g__Callback|0);
				}
			}
		}

		public void PlayOffline(LoginUserCallback _delegate)
		{
			if (!this.m_idProvider.LoadOfflineId())
			{
				this.UserStatus = EUserStatus.TemporaryError;
				_delegate(this.m_owner, EApiStatusReason.NoOnlineStart, null);
				return;
			}
			this.UserStatus = EUserStatus.OfflineMode;
			_delegate(this.m_owner, EApiStatusReason.Ok, null);
			Action<IPlatform> userLoggedIn = this.m_userLoggedIn;
			if (userLoggedIn == null)
			{
				return;
			}
			userLoggedIn(this.m_owner);
		}

		public Action IsConnectToServerFromCommandline()
		{
			return null;
		}

		public void StartAdvertisePlaying(GameServerInfo _serverInfo)
		{
			if (_serverInfo == null)
			{
				return;
			}
			object activityLock = this.m_activityLock;
			lock (activityLock)
			{
				this.m_shouldActivityBeActive = true;
				if (_serverInfo != this.m_activityLastServerInfo)
				{
					if (this.m_activityLastServerInfo != null)
					{
						this.UnregisterGameServerInfoEvents(this.m_activityLastServerInfo);
					}
					this.m_activityLastServerInfo = _serverInfo;
					this.RegisterGameServerInfoEvents(_serverInfo);
				}
				this.SetActivity(_serverInfo);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void RegisterGameServerInfoEvents(GameServerInfo _serverInfo)
		{
			_serverInfo.OnChangedString += this.OnServerInfoChangedString;
			_serverInfo.OnChangedInt += this.OnServerInfoChangedInt;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void UnregisterGameServerInfoEvents(GameServerInfo _serverInfo)
		{
			_serverInfo.OnChangedInt -= this.OnServerInfoChangedInt;
			_serverInfo.OnChangedString -= this.OnServerInfoChangedString;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnServerInfoChangedString(GameServerInfo _serverInfo, GameInfoString _key)
		{
			if (_key == GameInfoString.UniqueId)
			{
				this.SetActivity(_serverInfo);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnServerInfoChangedInt(GameServerInfo _serverInfo, GameInfoInt _key)
		{
			if (_key == GameInfoInt.CurrentPlayers || _key == GameInfoInt.ServerVisibility || _key == GameInfoInt.MaxPlayers)
			{
				this.SetActivity(_serverInfo);
			}
		}

		public void StopAdvertisePlaying()
		{
			object activityLock = this.m_activityLock;
			lock (activityLock)
			{
				this.m_shouldActivityBeActive = false;
				if (this.m_activityLastServerInfo != null)
				{
					this.UnregisterGameServerInfoEvents(this.m_activityLastServerInfo);
					this.m_activityLastServerInfo = null;
				}
				this.DeleteActivity();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void DeleteActivity()
		{
			object activityLock = this.m_activityLock;
			lock (activityLock)
			{
				if (!this.m_isActivityActive || this.m_shouldActivityBeActive)
				{
					this.m_shouldRetryDeleteActivity = false;
					return;
				}
				if (this.m_currentlyDeletingActivity)
				{
					this.m_shouldRetryDeleteActivity = true;
					return;
				}
				this.m_shouldRetryDeleteActivity = false;
				this.m_currentlyDeletingActivity = true;
				SDK.XBL.XblMultiplayerActivityDeleteActivityAsync(this.XblContextHandle, new SDK.XBL.XblMultiplayerActivityAsyncOperationCompleted(this.<DeleteActivity>g__CompletionRoutine|87_0));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnNetworkStateChanged(bool state)
		{
			object activityLock = this.m_activityLock;
			lock (activityLock)
			{
				if (this.m_isActivityActive && !this.m_shouldActivityBeActive)
				{
					this.DeleteActivity();
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetActivity(GameServerInfo _serverInfo)
		{
			if (_serverInfo == null)
			{
				return;
			}
			string value = _serverInfo.GetValue(GameInfoString.UniqueId);
			int value2 = _serverInfo.GetValue(GameInfoInt.CurrentPlayers);
			int value3 = _serverInfo.GetValue(GameInfoInt.ServerVisibility);
			int value4 = _serverInfo.GetValue(GameInfoInt.MaxPlayers);
			if (value4 < 2)
			{
				return;
			}
			if (string.IsNullOrEmpty(value))
			{
				return;
			}
			XblMultiplayerActivityInfo xblMultiplayerActivityInfo = new XblMultiplayerActivityInfo();
			xblMultiplayerActivityInfo.ConnectionString = value;
			xblMultiplayerActivityInfo.CurrentPlayers = (uint)value2;
			xblMultiplayerActivityInfo.GroupId = "Dummy";
			XblMultiplayerActivityInfo xblMultiplayerActivityInfo2 = xblMultiplayerActivityInfo;
			XblMultiplayerActivityJoinRestriction xblMultiplayerActivityJoinRestriction;
			if (value3 != 1)
			{
				if (value3 == 2)
				{
					xblMultiplayerActivityJoinRestriction = 0;
				}
				else
				{
					xblMultiplayerActivityJoinRestriction = 1;
				}
			}
			else
			{
				xblMultiplayerActivityJoinRestriction = 2;
			}
			xblMultiplayerActivityInfo2.JoinRestriction = xblMultiplayerActivityJoinRestriction;
			xblMultiplayerActivityInfo.MaxPlayers = (uint)value4;
			xblMultiplayerActivityInfo.Platform = 60;
			xblMultiplayerActivityInfo.Xuid = this.m_idProvider.Id.Xuid;
			XblMultiplayerActivityInfo xblMultiplayerActivityInfo3 = xblMultiplayerActivityInfo;
			object activityLock = this.m_activityLock;
			lock (activityLock)
			{
				if (this.m_shouldActivityBeActive)
				{
					SDK.XBL.XblMultiplayerActivitySetActivityAsync(this.XblContextHandle, xblMultiplayerActivityInfo3, true, new SDK.XBL.XblMultiplayerActivityAsyncOperationCompleted(this.<SetActivity>g__CompletionRoutine|89_0));
				}
			}
		}

		public void GetLoginTicket(Action<bool, byte[], string> _callback)
		{
			User.<>c__DisplayClass93_0 CS$<>8__locals1 = new User.<>c__DisplayClass93_0();
			CS$<>8__locals1._callback = _callback;
			if (this.m_userHandle == null)
			{
				Log.Error("[XBL] Attempting to retrieve XSTS token before acquiring XUserHandle");
				CS$<>8__locals1._callback(false, null, null);
				return;
			}
			SDK.XUserGetTokenAndSignatureUtf16Async(this.m_userHandle, 0, "GET", "https://eos.epicgames.com", User.eosRelyingPartyRequestHeaders, null, new XUserGetTokenAndSignatureUtf16Result(CS$<>8__locals1.<GetLoginTicket>g__CompletionRoutine|0));
		}

		public string GetFriendName(PlatformUserIdentifierAbs _playerId)
		{
			return null;
		}

		public bool IsFriend(PlatformUserIdentifierAbs _playerId)
		{
			ulong xuid = XblXuidMapper.GetXuid(_playerId);
			if (xuid == 0UL)
			{
				return false;
			}
			FriendsListXbl friendsListXbl = this.friendsList;
			return friendsListXbl != null && friendsListXbl.IsFriend(xuid);
		}

		public bool CanShowProfile(PlatformUserIdentifierAbs _playerId)
		{
			return XblXuidMapper.GetXuid(_playerId) > 0UL;
		}

		public void ShowProfile(PlatformUserIdentifierAbs _playerId)
		{
			ulong xuid = XblXuidMapper.GetXuid(_playerId);
			if (xuid != 0UL)
			{
				SDK.XGameUiShowPlayerProfileCardAsync(this.m_userHandle, xuid, delegate(int hr)
				{
					XblHelpers.LogHR(0, "XGameUiShowPlayerProfileCardAsync", false);
					if (HR.FAILED(hr))
					{
						Log.Error("XBL-GXDK: Showing Player Profile Failed.");
						return;
					}
					Log.Out("XBL-GXDK: Showing Player Profile Succeeded.");
				});
			}
		}

		public EUserPerms Permissions
		{
			get
			{
				if (this.m_privilegeHelper == null)
				{
					return (EUserPerms)0;
				}
				EUserPerms euserPerms = (EUserPerms)0;
				if (this.m_privilegeHelper.MultiplayerAllowed.Has())
				{
					euserPerms |= EUserPerms.Multiplayer | EUserPerms.HostMultiplayer;
				}
				if (this.m_privilegeHelper.CommunicationAllowed.Has())
				{
					euserPerms |= EUserPerms.Communication;
				}
				if (this.m_privilegeHelper.CrossPlayAllowed.Has())
				{
					euserPerms |= EUserPerms.Crossplay;
				}
				return euserPerms;
			}
		}

		public string GetPermissionDenyReason(EUserPerms _perms)
		{
			return null;
		}

		public IEnumerator ResolvePermissions(EUserPerms _perms, bool _canPrompt, CoroutineCancellationToken _cancellationToken = null)
		{
			Log.Out(string.Format("[XBL] {0}({1}: [{2}], {3}: {4})", new object[] { "ResolvePermissions", "_perms", _perms, "_canPrompt", _canPrompt }));
			if (this.m_privilegeHelper == null)
			{
				yield break;
			}
			yield return this.m_privilegeHelper.ResolvePermissions(_perms, _canPrompt, _cancellationToken);
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnXuidMapped(IReadOnlyCollection<PlatformUserIdentifierAbs> userIds, ulong xuid)
		{
			UserBlocksChangedCallback userBlocksChanged = this.UserBlocksChanged;
			if (userBlocksChanged == null)
			{
				return;
			}
			userBlocksChanged(userIds);
		}

		public IEnumerator ResolveUserBlocks(IReadOnlyList<IPlatformUserBlockedResults> _results)
		{
			User.<>c__DisplayClass103_0 CS$<>8__locals1 = new User.<>c__DisplayClass103_0();
			CS$<>8__locals1._results = _results;
			CS$<>8__locals1.<>4__this = this;
			this.userBlockedXuidToResultsTemp.Clear();
			this.userBlockedAnonymousResultsTemp.Clear();
			PlatformUserIdentifierAbs platformUserId = this.PlatformUserId;
			foreach (IPlatformUserBlockedResults platformUserBlockedResults in CS$<>8__locals1._results)
			{
				PlatformUserIdentifierAbs nativeId = platformUserBlockedResults.User.NativeId;
				if (!object.Equals(platformUserId, nativeId))
				{
					UserIdentifierXbl userIdentifierXbl = nativeId as UserIdentifierXbl;
					ulong xuid;
					if (userIdentifierXbl == null || (xuid = userIdentifierXbl.Xuid) == 0UL)
					{
						this.userBlockedAnonymousResultsTemp.Add(platformUserBlockedResults);
					}
					else
					{
						this.userBlockedXuidToResultsTemp[xuid] = platformUserBlockedResults;
					}
				}
			}
			CS$<>8__locals1.running = true;
			SDK.XBL.XblPrivacyBatchCheckPermissionAsync(this.XblContextHandle, User.userBlockedPermissions, this.userBlockedXuidToResultsTemp.Keys.ToArray<ulong>(), (this.userBlockedAnonymousResultsTemp.Count > 0) ? User.userBlockedAnonymousTypes : Array.Empty<XblAnonymousUserType>(), new XblPrivacyBatchCheckPermissionCompleted(CS$<>8__locals1.<ResolveUserBlocks>g__CompletionRoutine|0));
			while (CS$<>8__locals1.running)
			{
				yield return null;
			}
			yield break;
		}

		public EMatchmakingGroup GetMatchmakingGroup()
		{
			string sandboxId = this.SandboxHelper.SandboxId;
			if (sandboxId == null)
			{
				EMatchmakingGroup ematchmakingGroup = EMatchmakingGroup.Retail;
				Log.Warning(string.Format("[XBL] {0} no sandbox id. Defaulting to {1}", "GetMatchmakingGroup", ematchmakingGroup));
				return ematchmakingGroup;
			}
			return XblSandboxHelper.SandboxIdToMatchmakingGroup(sandboxId);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <OnClientApiInitialized>g__XUserAddAsyncSilentCompletionRoutine|77_0(int hr, XUserHandle userHandle)
		{
			XblHelpers.LogHR(hr, "XUserAddAsync: AddDefaultUserSilently", false);
			if (HR.SUCCEEDED(hr))
			{
				this.<OnClientApiInitialized>g__XUserAddSucceeded|77_2(userHandle);
				return;
			}
			if (hr == -1994108666)
			{
				this.UserStatus = EUserStatus.TemporaryError;
				SDK.XUserAddAsync(4, new XUserAddCompleted(this.<OnClientApiInitialized>g__XUserAddAsyncAllowingUICompletionRoutine|77_1));
				return;
			}
			this.UserStatus = EUserStatus.TemporaryError;
			SDK.XUserAddAsync(1, new XUserAddCompleted(this.<OnClientApiInitialized>g__XUserAddAsyncSilentCompletionRoutine|77_0));
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <OnClientApiInitialized>g__XUserAddAsyncAllowingUICompletionRoutine|77_1(int hr, XUserHandle userHandle)
		{
			XblHelpers.LogHR(hr, "XUserAddAsync: AddDefaultUserAllowingUI", false);
			if (HR.SUCCEEDED(hr))
			{
				this.<OnClientApiInitialized>g__XUserAddSucceeded|77_2(userHandle);
				return;
			}
			this.UserStatus = EUserStatus.TemporaryError;
			SDK.XUserAddAsync(1, new XUserAddCompleted(this.<OnClientApiInitialized>g__XUserAddAsyncSilentCompletionRoutine|77_0));
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <OnClientApiInitialized>g__XUserAddSucceeded|77_2(XUserHandle userHandle)
		{
			object userHandleLock = this.m_userHandleLock;
			lock (userHandleLock)
			{
				this.m_userHandle = userHandle;
				Action<XUserHandle> userHandleReady = this.m_userHandleReady;
				if (userHandleReady != null)
				{
					userHandleReady(userHandle);
				}
			}
			this.m_privilegeHelper = new UserPrivilegeHelper(this.m_userHandle);
			this.m_privilegeHelper.AllAllowed.ResolveSilent();
			this.SocialManager = new SocialManagerXbl(this.m_userHandle);
			this.friendsList = new FriendsListXbl(this.SocialManager);
			this.<OnClientApiInitialized>g__GetXuid|77_3();
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <OnClientApiInitialized>g__GetXuid|77_3()
		{
			int num = SDK.XUserGetId(this.m_userHandle, ref this.m_userXuid);
			XblHelpers.LogHR(num, "XUserGetId", false);
			if (!HR.FAILED(num))
			{
				this.<OnClientApiInitialized>g__PostLogin|77_6();
				return;
			}
			if (num == -1994108670)
			{
				this.<OnClientApiInitialized>g__ResolveXuidIssue|77_4();
				return;
			}
			this.<OnClientApiInitialized>g__DoLoginUserCallback|77_7(EUserStatus.PermanentError, EApiStatusReason.Other, "Could not obtain User's Xuid.");
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <OnClientApiInitialized>g__ResolveXuidIssue|77_4()
		{
			SDK.XUserResolveIssueWithUiUtf16Async(this.m_userHandle, null, new XUserResolveIssueWithUiUtf16Result(this.<OnClientApiInitialized>g__ResolveXuidIssueCompletionRoutine|77_5));
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <OnClientApiInitialized>g__ResolveXuidIssueCompletionRoutine|77_5(int hrResolve)
		{
			XblHelpers.LogHR(hrResolve, "XUserResolveIssueWithUiUtf16Async", false);
			this.<OnClientApiInitialized>g__GetXuid|77_3();
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <OnClientApiInitialized>g__PostLogin|77_6()
		{
			int num = SDK.XUserGetLocalId(this.m_userHandle, ref this.m_userLocalId);
			XblHelpers.LogHR(num, "XUserGetLocalId", false);
			if (HR.FAILED(num))
			{
				this.<OnClientApiInitialized>g__DoLoginUserCallback|77_7(EUserStatus.PermanentError, EApiStatusReason.Other, "Could not obtain User's Local ID.");
				return;
			}
			int num2 = SDK.XBL.XblContextCreateHandle(this.m_userHandle, ref this.m_contextHandle);
			XblHelpers.LogHR(num2, "Create Xbox Live Context Handle", false);
			if (HR.FAILED(num2))
			{
				this.<OnClientApiInitialized>g__DoLoginUserCallback|77_7(EUserStatus.PermanentError, EApiStatusReason.Other, "Could not obtain Xbox Live Context Handle.");
				return;
			}
			this.MultiplayerActivityQueryManager = new MultiplayerActivityQueryManager(this.m_contextHandle);
			this.<OnClientApiInitialized>g__DoLoginUserCallback|77_7(EUserStatus.LoggedIn, EApiStatusReason.Ok, null);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <OnClientApiInitialized>g__DoLoginUserCallback|77_7(EUserStatus userStatus, EApiStatusReason reason, string reasonAdditional)
		{
			object loginLock = this.m_loginLock;
			lock (loginLock)
			{
				this.UserStatus = userStatus;
				this.m_loginUserStatus = userStatus;
				this.m_loginUserCallbackReason = reason;
				this.m_loginUserCallbackReasonAdditional = reasonAdditional;
				this.m_loginDone = true;
				XblHelpers.LogHR((userStatus == EUserStatus.LoggedIn) ? 0 : (-2147467259), string.Format("Initial Login Callback Done. User Status: {0}, Reason: {1}, Additional: '{2}'.", userStatus, reason, reasonAdditional), false);
				LoginUserCallback loginUserCallback = this.m_loginUserCallback;
				if (loginUserCallback != null)
				{
					loginUserCallback(this.m_owner, reason, reasonAdditional);
				}
				this.m_loginUserCallback = null;
			}
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <DeleteActivity>g__CompletionRoutine|87_0(int _hresult)
		{
			object activityLock = this.m_activityLock;
			lock (activityLock)
			{
				this.m_currentlyDeletingActivity = false;
				if (HR.SUCCEEDED(_hresult))
				{
					if (this.m_isActivityActive)
					{
						Log.Out("[XBL] Activity Deleted");
						this.m_isActivityActive = false;
					}
					this.m_shouldRetryDeleteActivity = false;
					if (this.m_shouldActivityBeActive)
					{
						Log.Out("[XBL] Setting activity since it should be active.");
						this.SetActivity(this.m_activityLastServerInfo);
					}
				}
				else
				{
					XblHelpers.LogHR(_hresult, "Delete Activity", true);
					if (this.m_isActivityActive && !this.m_shouldActivityBeActive)
					{
						if (this.m_shouldRetryDeleteActivity)
						{
							Log.Warning("[XBL] Failed to delete activity. Will retry now because a change (e.g. Network State) happened while deleting the activity.");
							this.m_shouldRetryDeleteActivity = false;
							SDK.XBL.XblMultiplayerActivityDeleteActivityAsync(this.XblContextHandle, new SDK.XBL.XblMultiplayerActivityAsyncOperationCompleted(this.<DeleteActivity>g__CompletionRoutine|87_0));
						}
						else
						{
							Log.Warning("[XBL] Failed to delete activity. Will retry when a change occurs (e.g. Network State).");
						}
					}
				}
			}
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <SetActivity>g__CompletionRoutine|89_0(int _hresult)
		{
			object activityLock = this.m_activityLock;
			lock (activityLock)
			{
				if (HR.SUCCEEDED(_hresult))
				{
					if (!this.m_isActivityActive)
					{
						Log.Out("[XBL] Activity Created");
						this.m_isActivityActive = true;
					}
					if (!this.m_shouldActivityBeActive)
					{
						Log.Out("[XBL] Deleting activity since it should not be active.");
						this.DeleteActivity();
					}
				}
				else
				{
					XblHelpers.LogHR(_hresult, "Set Activity", false);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly EnumDictionary<XblPermission, EBlockType> xblPermissionToBlockType;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly XblPermission[] userBlockedPermissions;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly XblAnonymousUserType[] userBlockedAnonymousTypes;

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform m_owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatformApi m_api;

		[PublicizedFrom(EAccessModifier.Private)]
		public IApplicationStateController m_appState;

		[PublicizedFrom(EAccessModifier.Private)]
		public XUserHandle m_userHandle;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly object m_userHandleLock = new object();

		[PublicizedFrom(EAccessModifier.Private)]
		public Action<XUserHandle> m_userHandleReady;

		[PublicizedFrom(EAccessModifier.Private)]
		public XblContextHandle m_contextHandle;

		[PublicizedFrom(EAccessModifier.Private)]
		public XUserLocalId m_userLocalId;

		[PublicizedFrom(EAccessModifier.Private)]
		public ulong m_userXuid;

		[PublicizedFrom(EAccessModifier.Private)]
		public UserIdentifierXbl m_platformUserId;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly object m_loginLock = new object();

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_loginDone;

		[PublicizedFrom(EAccessModifier.Private)]
		public EUserStatus m_loginUserStatus;

		[PublicizedFrom(EAccessModifier.Private)]
		public LoginUserCallback m_loginUserCallback;

		[PublicizedFrom(EAccessModifier.Private)]
		public EApiStatusReason m_loginUserCallbackReason;

		[PublicizedFrom(EAccessModifier.Private)]
		public string m_loginUserCallbackReasonAdditional;

		[PublicizedFrom(EAccessModifier.Private)]
		public UserPrivilegeHelper m_privilegeHelper;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly object m_activityLock = new object();

		[PublicizedFrom(EAccessModifier.Private)]
		public GameServerInfo m_activityLastServerInfo;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_isActivityActive;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_shouldActivityBeActive;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_shouldRetryDeleteActivity;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_currentlyDeletingActivity;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<ulong, IPlatformUserBlockedResults> userBlockedXuidToResultsTemp = new Dictionary<ulong, IPlatformUserBlockedResults>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<IPlatformUserBlockedResults> userBlockedAnonymousResultsTemp = new List<IPlatformUserBlockedResults>();

		public string SandboxName;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action<IPlatform> m_userLoggedIn;

		[PublicizedFrom(EAccessModifier.Private)]
		public IdProviderGameCore m_idProvider;

		[PublicizedFrom(EAccessModifier.Private)]
		public FriendsListXbl friendsList;

		[PublicizedFrom(EAccessModifier.Private)]
		public const string eosRelyingPartyUrl = "https://eos.epicgames.com";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string eosRelyingPartyHttpMethod = "GET";

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly XUserGetTokenAndSignatureUtf16HttpHeader[] eosRelyingPartyRequestHeaders = new XUserGetTokenAndSignatureUtf16HttpHeader[]
		{
			new XUserGetTokenAndSignatureUtf16HttpHeader
			{
				Name = "X-XBL-Contract-Version",
				Value = "2"
			}
		};
	}
}
