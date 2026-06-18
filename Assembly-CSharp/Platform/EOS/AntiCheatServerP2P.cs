using System;
using System.IO;
using System.Runtime.CompilerServices;
using Epic.OnlineServices;
using Epic.OnlineServices.AntiCheatClient;
using Epic.OnlineServices.AntiCheatCommon;

namespace Platform.EOS
{
	public class AntiCheatServerP2P : IAntiCheatServer, IAntiCheatEncryption, IEncryptionModule
	{
		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
			this.owner.Api.ClientApiInitialized += this.apiInitialized;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void apiInitialized()
		{
			EosHelpers.AssertMainThread("ACSP2P.Init");
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.antiCheatInterface = ((Api)this.owner.Api).PlatformInterface.GetAntiCheatClientInterface();
			}
			if (this.antiCheatInterface == null)
			{
				Log.Out("[EAC] AntiCheatServerP2P initialized with null interface");
				return;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void AddCallbacks()
		{
			AddNotifyMessageToPeerOptions addNotifyMessageToPeerOptions = default(AddNotifyMessageToPeerOptions);
			object obj = AntiCheatCommon.LockObject;
			lock (obj)
			{
				this.handleMessageToPeerID = this.antiCheatInterface.AddNotifyMessageToPeer(ref addNotifyMessageToPeerOptions, null, new OnMessageToPeerCallback(this.handleMessageToPeer));
			}
			AddNotifyPeerAuthStatusChangedOptions addNotifyPeerAuthStatusChangedOptions = default(AddNotifyPeerAuthStatusChangedOptions);
			obj = AntiCheatCommon.LockObject;
			lock (obj)
			{
				this.handlePeerAuthStateChangeID = this.antiCheatInterface.AddNotifyPeerAuthStatusChanged(ref addNotifyPeerAuthStatusChangedOptions, null, new OnPeerAuthStatusChangedCallback(this.handlePeerAuthStateChange));
			}
			AddNotifyPeerActionRequiredOptions addNotifyPeerActionRequiredOptions = default(AddNotifyPeerActionRequiredOptions);
			obj = AntiCheatCommon.LockObject;
			lock (obj)
			{
				this.handlePeerActionRequiredID = this.antiCheatInterface.AddNotifyPeerActionRequired(ref addNotifyPeerActionRequiredOptions, null, new OnPeerActionRequiredCallback(this.handlePeerActionRequired));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void RemoveCallbacks()
		{
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.antiCheatInterface.RemoveNotifyMessageToPeer(this.handleMessageToPeerID);
				this.antiCheatInterface.RemoveNotifyPeerAuthStatusChanged(this.handlePeerAuthStateChangeID);
				this.antiCheatInterface.RemoveNotifyPeerActionRequired(this.handlePeerActionRequiredID);
			}
		}

		public bool GetHostUserIdAndToken([TupleElementNames(new string[] { "userId", "token" })] out ValueTuple<PlatformUserIdentifierAbs, string> _hostUserIdAndToken)
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
			IPlatform crossplatformPlatform2 = PlatformManager.CrossplatformPlatform;
			string text;
			if (crossplatformPlatform2 == null)
			{
				text = null;
			}
			else
			{
				IAuthenticationClient authenticationClient = crossplatformPlatform2.AuthenticationClient;
				text = ((authenticationClient != null) ? authenticationClient.GetAuthTicket() : null);
			}
			_hostUserIdAndToken = new ValueTuple<PlatformUserIdentifierAbs, string>(platformUserIdentifierAbs, text);
			return true;
		}

		public void Update()
		{
		}

		public bool StartServer(AuthenticationSuccessfulCallbackDelegate _authSuccessfulDelegate, KickPlayerDelegate _kickPlayerDelegate)
		{
			if (this.ServerEacEnabled())
			{
				this.AddCallbacks();
				Log.Out("[EAC] Starting EAC peer to peer server");
				this.authSuccessfulDelegate = _authSuccessfulDelegate;
				this.kickPlayerDelegate = _kickPlayerDelegate;
				ProductUserId productUserId = ((UserIdentifierEos)this.owner.User.PlatformUserId).ProductUserId;
				BeginSessionOptions beginSessionOptions = default(BeginSessionOptions);
				beginSessionOptions.LocalUserId = productUserId;
				beginSessionOptions.Mode = 2;
				BeginSessionOptions beginSessionOptions2 = beginSessionOptions;
				object lockObject = AntiCheatCommon.LockObject;
				Result result;
				lock (lockObject)
				{
					result = this.antiCheatInterface.BeginSession(ref beginSessionOptions2);
				}
				if (result != null)
				{
					Log.Error("[EOS-ACSP2P] Starting module failed: " + result.ToStringCached<Result>());
				}
				else
				{
					this.serverRunning = true;
				}
				return result == 0;
			}
			return true;
		}

		public bool RegisterUser(ClientInfo _client)
		{
			if (!this.serverRunning)
			{
				return false;
			}
			Log.Out(string.Format("[EOS-ACSP2P] Registering user: {0}", _client));
			EosHelpers.AssertMainThread("ACSP2P.Reg");
			RegisterPeerOptions registerPeerOptions = default(RegisterPeerOptions);
			registerPeerOptions.PeerHandle = AntiCheatCommon.ClientInfoToIntPtr(_client);
			registerPeerOptions.ClientPlatform = EosHelpers.DeviceTypeToAntiCheatPlatformMappings[_client.device];
			registerPeerOptions.PeerProductUserId = ((UserIdentifierEos)_client.CrossplatformId).ProductUserId;
			registerPeerOptions.ClientType = (_client.requiresAntiCheat ? 0 : 1);
			registerPeerOptions.IpAddress = _client.ip;
			registerPeerOptions.AuthenticationTimeout = 60U;
			RegisterPeerOptions registerPeerOptions2 = registerPeerOptions;
			object lockObject = AntiCheatCommon.LockObject;
			Result result;
			lock (lockObject)
			{
				result = this.antiCheatInterface.RegisterPeer(ref registerPeerOptions2);
			}
			if (result != null)
			{
				Log.Error("[EOS-ACSP2P] Failed registering user: " + result.ToStringCached<Result>());
				return false;
			}
			if (!_client.requiresAntiCheat)
			{
				this.authSuccessfulDelegate(_client);
			}
			return true;
		}

		public void FreeUser(ClientInfo _client)
		{
			if (!this.serverRunning)
			{
				return;
			}
			EosHelpers.AssertMainThread("ACS.Free");
			Log.Out(string.Format("[EOS-ACSP2P] FreeUser: {0}", _client));
			UnregisterPeerOptions unregisterPeerOptions = default(UnregisterPeerOptions);
			unregisterPeerOptions.PeerHandle = AntiCheatCommon.ClientInfoToIntPtr(_client);
			UnregisterPeerOptions unregisterPeerOptions2 = unregisterPeerOptions;
			object lockObject = AntiCheatCommon.LockObject;
			Result result;
			lock (lockObject)
			{
				result = this.antiCheatInterface.UnregisterPeer(ref unregisterPeerOptions2);
			}
			if (result != null)
			{
				Log.Error("[EOS-ACSP2P] Failed unregistering user: " + result.ToStringCached<Result>());
			}
		}

		public void HandleMessageFromClient(ClientInfo _cInfo, byte[] _data)
		{
			if (!this.serverRunning)
			{
				Log.Warning("[EOS-ACSP2P] Server: Received EAC package but EAC was not initialized");
				return;
			}
			if (AntiCheatCommon.DebugEacVerbose)
			{
				Log.Out(string.Format("[EOS-ACSP2P] PushNetworkMessage (len={0}, from={1})", _data.Length, _cInfo.InternalId));
			}
			ReceiveMessageFromPeerOptions receiveMessageFromPeerOptions = default(ReceiveMessageFromPeerOptions);
			receiveMessageFromPeerOptions.Data = new ArraySegment<byte>(_data);
			receiveMessageFromPeerOptions.PeerHandle = AntiCheatCommon.ClientInfoToIntPtr(_cInfo);
			ReceiveMessageFromPeerOptions receiveMessageFromPeerOptions2 = receiveMessageFromPeerOptions;
			object lockObject = AntiCheatCommon.LockObject;
			Result result;
			lock (lockObject)
			{
				result = this.antiCheatInterface.ReceiveMessageFromPeer(ref receiveMessageFromPeerOptions2);
			}
			if (result != 12008 && result != null)
			{
				Log.Error("[EOS-ACSP2P] Failed handling message: " + result.ToStringCached<Result>());
			}
		}

		public void StopServer()
		{
			if (!this.serverRunning)
			{
				return;
			}
			this.RemoveCallbacks();
			EndSessionOptions endSessionOptions = default(EndSessionOptions);
			object lockObject = AntiCheatCommon.LockObject;
			Result result;
			lock (lockObject)
			{
				result = this.antiCheatInterface.EndSession(ref endSessionOptions);
			}
			if (result != null)
			{
				Log.Error("[EOS-ACSP2P] Stopping module failed: " + result.ToStringCached<Result>());
			}
			this.serverRunning = false;
			this.authSuccessfulDelegate = null;
			this.kickPlayerDelegate = null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void handleMessageToPeer(ref OnMessageToClientCallbackInfo _data)
		{
			if (!this.serverRunning)
			{
				return;
			}
			ClientInfo clientInfo = AntiCheatCommon.IntPtrToClientInfo(_data.ClientHandle, "[EOS-ACSP2P] Got message for unknown client number: {0}");
			if (clientInfo == null)
			{
				Log.Out(string.Format("[EOS-ACSP2P] FreeUser: {0}", _data.ClientHandle));
				UnregisterPeerOptions unregisterPeerOptions = default(UnregisterPeerOptions);
				unregisterPeerOptions.PeerHandle = _data.ClientHandle;
				UnregisterPeerOptions unregisterPeerOptions2 = unregisterPeerOptions;
				object lockObject = AntiCheatCommon.LockObject;
				Result result;
				lock (lockObject)
				{
					result = this.antiCheatInterface.UnregisterPeer(ref unregisterPeerOptions2);
				}
				if (result != null)
				{
					Log.Error("[EOS-ACSP2P] Failed unregistering user: " + result.ToStringCached<Result>());
				}
			}
			if (clientInfo != null)
			{
				clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEAC>().Setup(_data.MessageData.Count, _data.MessageData.Array));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void handlePeerActionRequired(ref OnClientActionRequiredCallbackInfo _data)
		{
			if (!this.serverRunning)
			{
				return;
			}
			ClientInfo clientInfo = AntiCheatCommon.IntPtrToClientInfo(_data.ClientHandle, "[EOS-ACSP2P] Got action for unknown client number: {0}");
			if (clientInfo == null)
			{
				return;
			}
			AntiCheatCommonClientAction clientAction = _data.ClientAction;
			AntiCheatCommonClientActionReason actionReasonCode = _data.ActionReasonCode;
			string text = _data.ActionReasonDetailsString;
			if (clientAction != 1)
			{
				Log.Warning(string.Format("[EOS-ACSP2P] Got invalid action ({0}), reason='{1}', details={2}, client={3}", new object[]
				{
					clientAction.ToStringCached<AntiCheatCommonClientAction>(),
					actionReasonCode.ToStringCached<AntiCheatCommonClientActionReason>(),
					text,
					clientInfo
				}));
				return;
			}
			Log.Out(string.Format("[EOS-ACSP2P] Kicking player. Reason={0}, details='{1}', client={2}", actionReasonCode.ToStringCached<AntiCheatCommonClientActionReason>(), text, clientInfo));
			KickPlayerDelegate kickPlayerDelegate = this.kickPlayerDelegate;
			if (kickPlayerDelegate == null)
			{
				return;
			}
			ClientInfo clientInfo2 = clientInfo;
			GameUtils.EKickReason ekickReason = GameUtils.EKickReason.EosEacViolation;
			int num = actionReasonCode;
			string text2 = text;
			kickPlayerDelegate(clientInfo2, new GameUtils.KickPlayerData(ekickReason, num, default(DateTime), text2));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void handlePeerAuthStateChange(ref OnClientAuthStatusChangedCallbackInfo _data)
		{
			if (!this.serverRunning)
			{
				return;
			}
			ClientInfo clientInfo = AntiCheatCommon.IntPtrToClientInfo(_data.ClientHandle, "[EOS-ACSP2P] Got auth state change for unknown client number: {0}");
			if (_data.ClientAuthStatus == 2)
			{
				Log.Out(string.Format("[EOS-ACSP2P] Remote Auth complete for client number {0}", _data.ClientHandle));
				AuthenticationSuccessfulCallbackDelegate authenticationSuccessfulCallbackDelegate = this.authSuccessfulDelegate;
				if (authenticationSuccessfulCallbackDelegate == null)
				{
					return;
				}
				authenticationSuccessfulCallbackDelegate(clientInfo);
			}
		}

		public void Destroy()
		{
		}

		public bool ServerEacEnabled()
		{
			return this.antiCheatInterface != null && GamePrefs.GetBool(EnumGamePrefs.ServerEACPeerToPeer);
		}

		public bool ServerEacAvailable()
		{
			return this.antiCheatInterface != null;
		}

		public bool EncryptionAvailable()
		{
			return false;
		}

		public bool EncryptStream(ClientInfo _cInfo, MemoryStream _stream)
		{
			throw new NotImplementedException("Encryption is not supported for a Peer to Peer AntiCheatServer.");
		}

		public bool DecryptStream(ClientInfo _cInfo, MemoryStream _stream)
		{
			throw new NotImplementedException("Encryption is not supported for a Peer to Peer AntiCheatServer.");
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public AntiCheatClientInterface antiCheatInterface;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool serverRunning;

		[PublicizedFrom(EAccessModifier.Private)]
		public AuthenticationSuccessfulCallbackDelegate authSuccessfulDelegate;

		[PublicizedFrom(EAccessModifier.Private)]
		public KickPlayerDelegate kickPlayerDelegate;

		[PublicizedFrom(EAccessModifier.Private)]
		public ulong handleMessageToPeerID;

		[PublicizedFrom(EAccessModifier.Private)]
		public ulong handlePeerAuthStateChangeID;

		[PublicizedFrom(EAccessModifier.Private)]
		public ulong handlePeerActionRequiredID;
	}
}
