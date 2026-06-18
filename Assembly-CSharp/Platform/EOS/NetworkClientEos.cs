using System;
using System.Collections;
using System.Text;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using UnityEngine.Networking;

namespace Platform.EOS
{
	public class NetworkClientEos : IPlatformNetworkClient, INetworkClient
	{
		public bool IsConnected
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return this.serverId != null && (this.protoManager.IsClient || this.connecting) && this.owner.User.UserStatus == EUserStatus.LoggedIn;
			}
		}

		public NetworkClientEos(IPlatform _owner, IProtocolManagerProtocolInterface _protoManager)
		{
			this.owner = _owner;
			this.protoManager = _protoManager;
			this.owner.Api.ClientApiInitialized += delegate
			{
				if (!GameManager.IsDedicatedServer)
				{
					EosHelpers.AssertMainThread("P2P.Init");
					this.localUser = ((UserIdentifierEos)this.owner.User.PlatformUserId).ProductUserId;
					SocketId socketId = default(SocketId);
					socketId.SocketName = "Game";
					this.socketId = socketId;
					object obj = AntiCheatCommon.LockObject;
					lock (obj)
					{
						this.ptpInterface = ((Api)this.owner.Api).PlatformInterface.GetP2PInterface();
					}
					AddNotifyPeerConnectionRequestOptions addNotifyPeerConnectionRequestOptions = default(AddNotifyPeerConnectionRequestOptions);
					addNotifyPeerConnectionRequestOptions.LocalUserId = this.localUser;
					addNotifyPeerConnectionRequestOptions.SocketId = new SocketId?(this.socketId);
					AddNotifyPeerConnectionRequestOptions addNotifyPeerConnectionRequestOptions2 = addNotifyPeerConnectionRequestOptions;
					obj = AntiCheatCommon.LockObject;
					lock (obj)
					{
						this.ptpInterface.AddNotifyPeerConnectionRequest(ref addNotifyPeerConnectionRequestOptions2, null, new OnIncomingConnectionRequestCallback(this.ConnectionRequestHandler));
					}
					AddNotifyPeerConnectionEstablishedOptions addNotifyPeerConnectionEstablishedOptions = default(AddNotifyPeerConnectionEstablishedOptions);
					addNotifyPeerConnectionEstablishedOptions.LocalUserId = this.localUser;
					addNotifyPeerConnectionEstablishedOptions.SocketId = new SocketId?(this.socketId);
					AddNotifyPeerConnectionEstablishedOptions addNotifyPeerConnectionEstablishedOptions2 = addNotifyPeerConnectionEstablishedOptions;
					obj = AntiCheatCommon.LockObject;
					lock (obj)
					{
						this.ptpInterface.AddNotifyPeerConnectionEstablished(ref addNotifyPeerConnectionEstablishedOptions2, null, new OnPeerConnectionEstablishedCallback(this.ConnectionEstablishedHandler));
					}
					AddNotifyPeerConnectionClosedOptions addNotifyPeerConnectionClosedOptions = default(AddNotifyPeerConnectionClosedOptions);
					addNotifyPeerConnectionClosedOptions.LocalUserId = this.localUser;
					addNotifyPeerConnectionClosedOptions.SocketId = new SocketId?(this.socketId);
					AddNotifyPeerConnectionClosedOptions addNotifyPeerConnectionClosedOptions2 = addNotifyPeerConnectionClosedOptions;
					obj = AntiCheatCommon.LockObject;
					lock (obj)
					{
						this.ptpInterface.AddNotifyPeerConnectionClosed(ref addNotifyPeerConnectionClosedOptions2, null, new OnRemoteConnectionClosedCallback(this.ConnectionClosedHandler));
					}
					AddNotifyIncomingPacketQueueFullOptions addNotifyIncomingPacketQueueFullOptions = default(AddNotifyIncomingPacketQueueFullOptions);
					obj = AntiCheatCommon.LockObject;
					lock (obj)
					{
						this.ptpInterface.AddNotifyIncomingPacketQueueFull(ref addNotifyIncomingPacketQueueFullOptions, null, new OnIncomingPacketQueueFullCallback(this.IncomingPacketQueueFullHandler));
					}
				}
			};
		}

		public void Connect(GameServerInfo _gsi)
		{
			this.disconnectEventReceived = false;
			Log.Out("[EOS-P2PC] Trying to connect to: " + _gsi.GetValue(GameInfoString.IP) + ":" + _gsi.GetValue(GameInfoInt.Port).ToString());
			if (string.IsNullOrEmpty(_gsi.GetValue(GameInfoString.CombinedPrimaryId)))
			{
				Log.Out("[EOS-P2PC] Resolving EOS ID for IP " + _gsi.GetValue(GameInfoString.IP) + ":" + _gsi.GetValue(GameInfoInt.Port).ToString());
				ServerInformationTcpClient.RequestRules(_gsi, false, new ServerInformationTcpClient.RulesRequestDone(this.RulesRequestTcpDone));
				this.connecting = true;
				return;
			}
			this.ConnectInternal(_gsi);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void RulesRequestTcpDone(bool _success, string _message, GameServerInfo _gsi)
		{
			if (_success && this.connecting)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo = _gsi;
				this.ConnectInternal(_gsi);
				return;
			}
			this.Disconnect();
			ThreadManager.StartCoroutine(this.connectionFailedLater(_message));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ConnectInternal(GameServerInfo _gsi)
		{
			string value = _gsi.GetValue(GameInfoString.CombinedPrimaryId);
			if (string.IsNullOrEmpty(value))
			{
				Log.Error("Server info does not have a CombinedPrimaryId");
				this.Disconnect();
				ThreadManager.StartCoroutine(this.connectionFailedLater(Localization.Get("netSteamNetworking_NoServerID", false, null)));
				return;
			}
			if (_gsi.AllowsCrossplay && !PermissionsManager.IsCrossplayAllowed())
			{
				this.Disconnect();
				this.protoManager.ConnectionFailedEv(Localization.Get("auth_noCrossplay", false, null));
				return;
			}
			UserIdentifierEos userIdentifierEos = PlatformUserIdentifierAbs.FromCombinedString(value, true) as UserIdentifierEos;
			if (userIdentifierEos == null)
			{
				this.Disconnect();
				ThreadManager.StartCoroutine(this.connectionFailedLater(Localization.Get("netSteamNetworking_NoServerID", false, null)));
				return;
			}
			string password = ServerInfoCache.Instance.GetPassword(_gsi);
			ArrayListMP<byte> arrayListMP;
			if (!string.IsNullOrEmpty(password))
			{
				int byteCount = Encoding.UTF8.GetByteCount(password);
				arrayListMP = new ArrayListMP<byte>(MemoryPools.poolByte, byteCount + 1)
				{
					Count = byteCount + 1
				};
				Encoding.UTF8.GetBytes(password, 0, password.Length, arrayListMP.Items, 1);
			}
			else
			{
				arrayListMP = new ArrayListMP<byte>(MemoryPools.poolByte, 1)
				{
					Count = 1
				};
			}
			EosHelpers.AssertMainThread("P2P.ConInt.PUID");
			this.serverId = userIdentifierEos.ProductUserId;
			string text = "[EOS-P2PC] Connecting to EOS ID ";
			ProductUserId productUserId = this.serverId;
			Log.Out(text + ((productUserId != null) ? productUserId.ToString() : null));
			this.connecting = true;
			this.SendData(50, arrayListMP);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator connectionFailedLater(string _message)
		{
			yield return null;
			yield return null;
			this.protoManager.ConnectionFailedEv(_message);
			yield break;
		}

		public void Disconnect()
		{
			this.connecting = false;
			this.sendBufs.Clear();
			EosHelpers.AssertMainThread("P2P.CloseCons");
			CloseConnectionsOptions closeConnectionsOptions = default(CloseConnectionsOptions);
			closeConnectionsOptions.SocketId = new SocketId?(this.socketId);
			closeConnectionsOptions.LocalUserId = this.localUser;
			CloseConnectionsOptions closeConnectionsOptions2 = closeConnectionsOptions;
			object lockObject = AntiCheatCommon.LockObject;
			Result result;
			lock (lockObject)
			{
				result = this.ptpInterface.CloseConnections(ref closeConnectionsOptions2);
			}
			if (result != null)
			{
				Log.Error("[EOS-P2PC] Failed closing connections: " + result.ToStringCached<Result>());
			}
			this.serverId = null;
		}

		public NetworkError SendData(int _channel, ArrayListMP<byte> _data)
		{
			if (this.IsConnected)
			{
				_data[0] = (byte)_channel;
				this.sendBufs.Enqueue(new NetworkCommonEos.SendInfo(null, _data));
			}
			else
			{
				Log.Warning("[EOS-P2PC] Tried to send a package while not connected to a server");
			}
			return NetworkError.Ok;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ConnectionRequestHandler(ref OnIncomingConnectionRequestInfo _data)
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ConnectionEstablishedHandler(ref OnPeerConnectionEstablishedInfo _data)
		{
			Log.Out(string.Format("[EOS-P2PC] Connection established: {0}", _data.RemoteUserId));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ConnectionClosedHandler(ref OnRemoteConnectionClosedInfo _data)
		{
			ProductUserId remoteUserId = _data.RemoteUserId;
			if (this.connecting)
			{
				this.Disconnect();
				Log.Out(string.Format("[EOS-P2PC] P2PSessionConnectFail to: {0} - Error: {1}", _data.RemoteUserId, _data.Reason.ToStringCached<ConnectionClosedReason>()));
				string text = Localization.Get("netSteamNetworkingSessionError_" + _data.Reason.ToStringCached<ConnectionClosedReason>(), false, null);
				this.protoManager.ConnectionFailedEv(text);
				return;
			}
			if (!this.IsConnected)
			{
				return;
			}
			Log.Out(string.Format("[EOS-P2PC] Connection closed by {0}: ", remoteUserId) + _data.Reason.ToStringCached<ConnectionClosedReason>());
			if (_data.Reason == 1)
			{
				return;
			}
			CloseConnectionOptions closeConnectionOptions = default(CloseConnectionOptions);
			closeConnectionOptions.SocketId = new SocketId?(this.socketId);
			closeConnectionOptions.LocalUserId = this.localUser;
			closeConnectionOptions.RemoteUserId = remoteUserId;
			CloseConnectionOptions closeConnectionOptions2 = closeConnectionOptions;
			object lockObject = AntiCheatCommon.LockObject;
			Result result;
			lock (lockObject)
			{
				result = this.ptpInterface.CloseConnection(ref closeConnectionOptions2);
			}
			if (result != null)
			{
				Log.Error(string.Format("[EOS-P2PC] Failed closing connection to {0}: {1}", remoteUserId, result.ToStringCached<Result>()));
			}
			this.OnDisconnectedFromServer();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void IncomingPacketQueueFullHandler(ref OnIncomingPacketQueueFullInfo _data)
		{
			if (!this.IsConnected)
			{
				return;
			}
			Log.Error(string.Format("[EOS-P2PC] Packet queue full: Chn={0}, IncSize={1}, Used={2}, Max={3}", new object[] { _data.OverflowPacketChannel, _data.OverflowPacketSizeBytes, _data.PacketQueueCurrentSizeBytes, _data.PacketQueueMaxSizeBytes }));
		}

		public void Update()
		{
			if (!this.IsConnected)
			{
				return;
			}
			Result result;
			for (;;)
			{
				ReceivePacketOptions receivePacketOptions = default(ReceivePacketOptions);
				receivePacketOptions.LocalUserId = this.localUser;
				receivePacketOptions.MaxDataSizeBytes = 1170U;
				ReceivePacketOptions receivePacketOptions2 = receivePacketOptions;
				ProductUserId productUserId = new ProductUserId();
				SocketId socketId = default(SocketId);
				object obj = AntiCheatCommon.LockObject;
				uint num;
				lock (obj)
				{
					byte b;
					result = this.ptpInterface.ReceivePacket(ref receivePacketOptions2, ref productUserId, ref socketId, ref b, this.receiveBuffer, ref num);
				}
				if (result != null)
				{
					break;
				}
				if (num > 0U)
				{
					NetworkCommonEos.ESteamNetChannels esteamNetChannels = (NetworkCommonEos.ESteamNetChannels)this.receiveBuffer.Array[0];
					if (esteamNetChannels > NetworkCommonEos.ESteamNetChannels.NetpackageChannel1)
					{
						if (esteamNetChannels != NetworkCommonEos.ESteamNetChannels.Authentication)
						{
							if (esteamNetChannels == NetworkCommonEos.ESteamNetChannels.Ping)
							{
								SendPacketOptions sendPacketOptions = default(SendPacketOptions);
								sendPacketOptions.SocketId = new SocketId?(this.socketId);
								sendPacketOptions.LocalUserId = this.localUser;
								sendPacketOptions.RemoteUserId = this.serverId;
								sendPacketOptions.Channel = 0;
								sendPacketOptions.Reliability = 2;
								sendPacketOptions.AllowDelayedDelivery = true;
								sendPacketOptions.Data = this.receiveBuffer;
								SendPacketOptions sendPacketOptions2 = sendPacketOptions;
								obj = AntiCheatCommon.LockObject;
								Result result2;
								lock (obj)
								{
									result2 = this.ptpInterface.SendPacket(ref sendPacketOptions2);
								}
								if (result2 != null)
								{
									Log.Error("[EOS-P2PC] Could not send ping package to server: " + result2.ToStringCached<Result>());
								}
							}
						}
						else
						{
							if (this.connecting)
							{
								this.connecting = false;
								Log.Out("[EOS-P2PC] Connection established");
							}
							if (this.receiveBuffer.Array[1] == 0)
							{
								Log.Out("[EOS-P2PC] Received invalid password package");
								ThreadManager.AddSingleTaskMainThread("SteamNetInvalidPassword", delegate(object _info)
								{
									this.protoManager.InvalidPasswordEv();
								}, null);
							}
							else
							{
								Log.Out("[EOS-P2PC] Password accepted");
								this.OnConnectedToServer();
							}
						}
					}
					else
					{
						byte[] array = MemoryPools.poolByte.Alloc((int)num);
						Array.Copy(this.receiveBuffer.Array, array, (long)((ulong)num));
						SingletonMonoBehaviour<ConnectionManager>.Instance.Net_DataReceivedClient((int)esteamNetChannels, array, (int)num);
					}
				}
			}
			if (result != 18)
			{
				Log.Error("[EOS-P2PS] Error reading packages: " + result.ToStringCached<Result>());
				return;
			}
		}

		public void LateUpdate()
		{
			if (!this.IsConnected)
			{
				return;
			}
			while (this.sendBufs.HasData())
			{
				NetworkCommonEos.SendInfo sendInfo = this.sendBufs.Dequeue();
				ProductUserId productUserId = this.serverId;
				SendPacketOptions sendPacketOptions = default(SendPacketOptions);
				sendPacketOptions.SocketId = new SocketId?(this.socketId);
				sendPacketOptions.LocalUserId = this.localUser;
				sendPacketOptions.RemoteUserId = productUserId;
				sendPacketOptions.Channel = 0;
				sendPacketOptions.Reliability = 2;
				sendPacketOptions.AllowDelayedDelivery = true;
				sendPacketOptions.Data = new ArraySegment<byte>(sendInfo.Data.Items, 0, sendInfo.Data.Count);
				SendPacketOptions sendPacketOptions2 = sendPacketOptions;
				object lockObject = AntiCheatCommon.LockObject;
				Result result;
				lock (lockObject)
				{
					result = this.ptpInterface.SendPacket(ref sendPacketOptions2);
				}
				if (result != null)
				{
					Log.Error("[EOS-P2PC] Could not send package to server: " + result.ToStringCached<Result>());
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnDisconnectedFromServer()
		{
			this.Disconnect();
			this.protoManager.DisconnectedFromServerEv(Localization.Get("netSteamNetworking_ConnectionClosedByServer", false, null));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnConnectedToServer()
		{
			INetConnection[] array = new INetConnection[2];
			for (int i = 0; i < 2; i++)
			{
				array[i] = new NetConnectionSimple(i, null, this, null, 1, 1120);
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SetConnectionToServer(array);
		}

		public void SetLatencySimulation(bool _enable, int _minLatency, int _maxLatency)
		{
		}

		public void SetPacketLossSimulation(bool _enable, int _chance)
		{
		}

		public void EnableStatistics()
		{
		}

		public void DisableStatistics()
		{
		}

		public string PrintNetworkStatistics()
		{
			return "";
		}

		public void ResetNetworkStatistics()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string socketName = "Game";

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly IProtocolManagerProtocolInterface protoManager;

		[PublicizedFrom(EAccessModifier.Private)]
		public P2PInterface ptpInterface;

		[PublicizedFrom(EAccessModifier.Private)]
		public ProductUserId localUser;

		[PublicizedFrom(EAccessModifier.Private)]
		public SocketId socketId;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BlockingQueue<NetworkCommonEos.SendInfo> sendBufs = new BlockingQueue<NetworkCommonEos.SendInfo>();

		[PublicizedFrom(EAccessModifier.Private)]
		public ProductUserId serverId;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool connecting;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool disconnectEventReceived;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ArraySegment<byte> receiveBuffer = new ArraySegment<byte>(new byte[1170]);
	}
}
