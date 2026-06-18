using System;
using System.IO;
using System.Runtime.CompilerServices;
using Epic.OnlineServices;
using Epic.OnlineServices.AntiCheatClient;

namespace Platform.EOS
{
	public class AntiCheatClientManager : IAntiCheatClient, IAntiCheatEncryption, IEncryptionModule
	{
		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
			this.owner.Api.ClientApiInitialized += this.apiInitialized;
			this.antiCheatActive = !AntiCheatCommon.NoEacCmdLine;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void apiInitialized()
		{
			EosHelpers.AssertMainThread("ACC.Init");
			object obj = AntiCheatCommon.LockObject;
			lock (obj)
			{
				this.antiCheatInterface = ((Api)this.owner.Api).PlatformInterface.GetAntiCheatClientInterface();
			}
			if (this.antiCheatInterface == null)
			{
				this.antiCheatActive = false;
				Log.Out("[EOS-ACC] Not started with EAC, anticheat disabled");
				return;
			}
			this.clientServerClient = new AntiCheatClientCS(this.owner, this.antiCheatInterface);
			this.peerToPeerClient = new AntiCheatClientP2P(this.owner, this.antiCheatInterface);
			AddNotifyClientIntegrityViolatedOptions addNotifyClientIntegrityViolatedOptions = default(AddNotifyClientIntegrityViolatedOptions);
			obj = AntiCheatCommon.LockObject;
			lock (obj)
			{
				this.antiCheatInterface.AddNotifyClientIntegrityViolated(ref addNotifyClientIntegrityViolatedOptions, null, new OnClientIntegrityViolatedCallback(this.handleClientIntegrityViolated));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void handleClientIntegrityViolated(ref OnClientIntegrityViolatedCallbackInfo data)
		{
			Log.Warning(string.Format("[EOS-ACCP2P] Client violation: {0}, message: {1}", data.ViolationType.ToStringCached<AntiCheatClientViolationType>(), data.ViolationMessage));
			this.eacViolationMessage = data.ViolationMessage;
			this.eacViolation = true;
			this.antiCheatActive = false;
		}

		public bool GetUnhandledViolationMessage(out string _message)
		{
			if (this.eacViolation && !this.eacViolationHandled)
			{
				_message = this.eacViolationMessage;
				this.eacViolationHandled = true;
				return true;
			}
			_message = "";
			return false;
		}

		public bool ClientAntiCheatEnabled()
		{
			return this.antiCheatActive && !this.eacViolation;
		}

		public void WaitForRemoteAuth(Action onRemoteAuthSkippedOrComplete)
		{
			if (!Submission.Enabled && this.clientMode == AntiCheatClientManager.AntiCheatClientMode.Unknown)
			{
				Action onRemoteAuthSkippedOrComplete2 = onRemoteAuthSkippedOrComplete;
				if (onRemoteAuthSkippedOrComplete2 == null)
				{
					return;
				}
				onRemoteAuthSkippedOrComplete2();
				return;
			}
			else
			{
				if (this.clientMode != AntiCheatClientManager.AntiCheatClientMode.ClientServer)
				{
					AntiCheatClientP2P antiCheatClientP2P = this.peerToPeerClient;
					if (antiCheatClientP2P != null && antiCheatClientP2P.IsServerAntiCheatProtected())
					{
						this.peerToPeerClient.OnRemoteAuthComplete += delegate
						{
							Action onRemoteAuthSkippedOrComplete4 = onRemoteAuthSkippedOrComplete;
							if (onRemoteAuthSkippedOrComplete4 == null)
							{
								return;
							}
							onRemoteAuthSkippedOrComplete4();
						};
						return;
					}
				}
				Action onRemoteAuthSkippedOrComplete3 = onRemoteAuthSkippedOrComplete;
				if (onRemoteAuthSkippedOrComplete3 == null)
				{
					return;
				}
				onRemoteAuthSkippedOrComplete3();
				return;
			}
		}

		public void ConnectToServer([TupleElementNames(new string[] { "userId", "token" })] ValueTuple<PlatformUserIdentifierAbs, string> _hostUserAndToken, Action _onNoAntiCheatOrConnectionComplete, Action<string> _onConnectionFailed)
		{
			if (!this.ClientAntiCheatEnabled())
			{
				Log.Out("[EOS-ACC] Anti cheat not loaded");
				this.connectedToServer = false;
				Action onNoAntiCheatOrConnectionComplete = _onNoAntiCheatOrConnectionComplete;
				if (onNoAntiCheatOrConnectionComplete == null)
				{
					return;
				}
				onNoAntiCheatOrConnectionComplete();
				return;
			}
			else
			{
				if (_hostUserAndToken.Item1 != null)
				{
					this.clientMode = AntiCheatClientManager.AntiCheatClientMode.PeerToPeer;
					this.peerToPeerClient.Activate();
					this.peerToPeerClient.ConnectToServer(_hostUserAndToken, delegate
					{
						Action onNoAntiCheatOrConnectionComplete3 = _onNoAntiCheatOrConnectionComplete;
						if (onNoAntiCheatOrConnectionComplete3 != null)
						{
							onNoAntiCheatOrConnectionComplete3();
						}
						this.connectedToServer = true;
					}, _onConnectionFailed);
					return;
				}
				this.clientMode = AntiCheatClientManager.AntiCheatClientMode.ClientServer;
				if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
				{
					this.clientServerClient.Activate();
					this.clientServerClient.ConnectToServer(delegate
					{
						Action onNoAntiCheatOrConnectionComplete4 = _onNoAntiCheatOrConnectionComplete;
						if (onNoAntiCheatOrConnectionComplete4 != null)
						{
							onNoAntiCheatOrConnectionComplete4();
						}
						this.connectedToServer = true;
					}, _onConnectionFailed);
					return;
				}
				Action onNoAntiCheatOrConnectionComplete2 = _onNoAntiCheatOrConnectionComplete;
				if (onNoAntiCheatOrConnectionComplete2 == null)
				{
					return;
				}
				onNoAntiCheatOrConnectionComplete2();
				return;
			}
		}

		public void HandleMessageFromServer(byte[] _data)
		{
			if (!this.antiCheatActive)
			{
				Log.Warning("[EOS-ACC] Received EAC package but EAC was not initialized");
				return;
			}
			AntiCheatClientManager.AntiCheatClientMode antiCheatClientMode = this.clientMode;
			if (antiCheatClientMode == AntiCheatClientManager.AntiCheatClientMode.ClientServer)
			{
				this.clientServerClient.HandleMessageFromServer(_data);
				return;
			}
			if (antiCheatClientMode != AntiCheatClientManager.AntiCheatClientMode.PeerToPeer)
			{
				Log.Warning("[EOS-ACC] Received EAC package but EAC client mode is unknown.");
				return;
			}
			this.peerToPeerClient.HandleMessageFromPeer(_data);
		}

		public void DisconnectFromServer()
		{
			if (!this.ClientAntiCheatEnabled())
			{
				return;
			}
			if (!this.connectedToServer)
			{
				return;
			}
			AntiCheatClientManager.AntiCheatClientMode antiCheatClientMode = this.clientMode;
			if (antiCheatClientMode != AntiCheatClientManager.AntiCheatClientMode.ClientServer)
			{
				if (antiCheatClientMode != AntiCheatClientManager.AntiCheatClientMode.PeerToPeer)
				{
					Log.Warning("[EOS-ACC] DisconnectFromServer called but EAC client mode is unknown.");
				}
				else
				{
					this.peerToPeerClient.DisconnectFromServer();
				}
			}
			else
			{
				this.clientServerClient.DisconnectFromServer();
			}
			Log.Out("[EOS-ACC] Disconnected from game server");
			this.connectedToServer = false;
		}

		public void Destroy()
		{
		}

		public bool EncryptionAvailable()
		{
			return this.clientMode == AntiCheatClientManager.AntiCheatClientMode.ClientServer;
		}

		public bool EncryptStream(ClientInfo _cInfo, MemoryStream _stream)
		{
			if (this.clientMode != AntiCheatClientManager.AntiCheatClientMode.ClientServer)
			{
				Log.Error("[EOS-ACC] Encryption is not supported in AntiCheatClientMode.PeerToPeer");
				return false;
			}
			return this.clientServerClient.EncryptStream(_cInfo, _stream);
		}

		public bool DecryptStream(ClientInfo _cInfo, MemoryStream _stream)
		{
			if (this.clientMode != AntiCheatClientManager.AntiCheatClientMode.ClientServer)
			{
				Log.Error("[EOS-ACC] Encryption is not supported in AntiCheatClientMode.PeerToPeer");
				return false;
			}
			return this.clientServerClient.DecryptStream(_cInfo, _stream);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public AntiCheatClientInterface antiCheatInterface;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool antiCheatActive;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool eacViolation;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool eacViolationHandled;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool connectedToServer;

		[PublicizedFrom(EAccessModifier.Private)]
		public Utf8String eacViolationMessage;

		[PublicizedFrom(EAccessModifier.Private)]
		public AntiCheatClientManager.AntiCheatClientMode clientMode = AntiCheatClientManager.AntiCheatClientMode.Unknown;

		[PublicizedFrom(EAccessModifier.Private)]
		public AntiCheatClientCS clientServerClient;

		[PublicizedFrom(EAccessModifier.Private)]
		public AntiCheatClientP2P peerToPeerClient;

		[PublicizedFrom(EAccessModifier.Private)]
		public enum AntiCheatClientMode
		{
			ClientServer,
			PeerToPeer,
			Unknown
		}
	}
}
