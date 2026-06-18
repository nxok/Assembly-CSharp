using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using UnityEngine;
using UnityEngine.Networking;

namespace Platform.EOS
{
	public class NetworkServerEos : IPlatformNetworkServer, INetworkServer
	{
		public bool ServerRunning
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return this.serverStarted;
			}
		}

		public NetworkServerEos(IPlatform _owner, IProtocolManagerProtocolInterface _protoManager)
		{
			this.owner = _owner;
			this.protoManager = _protoManager;
			this.owner.Api.ClientApiInitialized += delegate
			{
				if (!GameManager.IsDedicatedServer)
				{
					EosHelpers.AssertMainThread("P2PS.Init");
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

		public NetworkConnectionError StartServer(int _basePort, string _password)
		{
			if (this.ServerRunning)
			{
				Log.Error("[EOS-P2PS] Server already running");
				return NetworkConnectionError.AlreadyConnectedToServer;
			}
			this.serverPassword = (string.IsNullOrEmpty(_password) ? null : _password);
			this.serverStarted = true;
			Log.Out("[EOS-P2PS] Server started");
			return NetworkConnectionError.NoError;
		}

		public void SetServerPassword(string _password)
		{
			this.serverPassword = (string.IsNullOrEmpty(_password) ? null : _password);
		}

		public void StopServer()
		{
			if (!this.ServerRunning)
			{
				return;
			}
			this.serverStarted = false;
			this.connections.Clear();
			EosHelpers.AssertMainThread("P2PS.Stop");
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
				Log.Error("[EOS-P2PS] Failed closing connections: " + result.ToStringCached<Result>());
			}
			Log.Out("[EOS-P2PS] Server stopped");
		}

		public void DropClient(ClientInfo _clientInfo, bool _clientDisconnect)
		{
			ProductUserId productUserId = ((UserIdentifierEos)_clientInfo.CrossplatformId).ProductUserId;
			ThreadManager.StartCoroutine(this.dropLater(productUserId, 0.2f));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator dropLater(ProductUserId _id, float _delay)
		{
			yield return new WaitForSeconds(_delay);
			if (!this.ServerRunning)
			{
				yield break;
			}
			NetworkServerEos.ConnectionInformation connectionInformation;
			if (this.connections.TryGetValue(_id, out connectionInformation))
			{
				Log.Out("[EOS-P2PS] Dropping client: " + ((_id != null) ? _id.ToString() : null));
				connectionInformation.State = NetworkServerEos.EConnectionState.Disconnected;
				this.OnPlayerDisconnected(_id);
			}
			CloseConnectionOptions closeConnectionOptions = default(CloseConnectionOptions);
			closeConnectionOptions.SocketId = new SocketId?(this.socketId);
			closeConnectionOptions.LocalUserId = this.localUser;
			closeConnectionOptions.RemoteUserId = _id;
			CloseConnectionOptions closeConnectionOptions2 = closeConnectionOptions;
			object lockObject = AntiCheatCommon.LockObject;
			Result result;
			lock (lockObject)
			{
				result = this.ptpInterface.CloseConnection(ref closeConnectionOptions2);
			}
			if (result != null)
			{
				Log.Error(string.Format("[EOS-P2PS] Failed closing connection: {0}: {1}", _id, result.ToStringCached<Result>()));
			}
			yield break;
		}

		public NetworkError SendData(ClientInfo _clientInfo, int _channel, ArrayListMP<byte> _data, bool _reliableDelivery = true)
		{
			if (this.ServerRunning)
			{
				_data[0] = (byte)_channel;
				this.sendBufs.Enqueue(new NetworkCommonEos.SendInfo(_clientInfo, _data));
			}
			else
			{
				Log.Warning("[EOS-P2PS] Tried to send a package to a client while not being a server");
			}
			return NetworkError.Ok;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ConnectionRequestHandler(ref OnIncomingConnectionRequestInfo _data)
		{
			if (!this.ServerRunning)
			{
				return;
			}
			ProductUserId remoteUserId = _data.RemoteUserId;
			UserIdentifierEos userIdentifierEos = new UserIdentifierEos(remoteUserId);
			if (_data.SocketId.Value.SocketName != "Game")
			{
				Log.Warning(string.Concat(new string[]
				{
					"[EOS-P2PS] P2P session request from ",
					userIdentifierEos.ProductUserIdString,
					" with invalid socket name '",
					_data.SocketId.Value.SocketName,
					"'"
				}));
				return;
			}
			Log.Out("[EOS-P2PS] P2PSessionRequest from: " + userIdentifierEos.ProductUserIdString);
			AcceptConnectionOptions acceptConnectionOptions = default(AcceptConnectionOptions);
			acceptConnectionOptions.SocketId = new SocketId?(this.socketId);
			acceptConnectionOptions.LocalUserId = this.localUser;
			acceptConnectionOptions.RemoteUserId = remoteUserId;
			AcceptConnectionOptions acceptConnectionOptions2 = acceptConnectionOptions;
			object lockObject = AntiCheatCommon.LockObject;
			Result result;
			lock (lockObject)
			{
				result = this.ptpInterface.AcceptConnection(ref acceptConnectionOptions2);
			}
			if (result != null)
			{
				Log.Error("[EOS-P2PS] Failed accepting session: " + result.ToStringCached<Result>());
				return;
			}
			this.connections[remoteUserId] = new NetworkServerEos.ConnectionInformation
			{
				State = NetworkServerEos.EConnectionState.Authenticating,
				UserIdentifier = userIdentifierEos
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ConnectionEstablishedHandler(ref OnPeerConnectionEstablishedInfo _data)
		{
			if (!this.ServerRunning)
			{
				return;
			}
			Log.Out(string.Format("[EOS-P2PS] Connection established: {0}", _data.RemoteUserId));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ConnectionClosedHandler(ref OnRemoteConnectionClosedInfo _data)
		{
			if (!this.ServerRunning)
			{
				return;
			}
			ProductUserId remoteUserId = _data.RemoteUserId;
			Log.Out(string.Format("[EOS-P2PS] Connection closed by {0}: ", remoteUserId) + _data.Reason.ToStringCached<ConnectionClosedReason>());
			NetworkServerEos.ConnectionInformation connectionInformation;
			if (!this.connections.TryGetValue(remoteUserId, out connectionInformation) || connectionInformation.State != NetworkServerEos.EConnectionState.Connected)
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
				Log.Error(string.Format("[EOS-P2PS] Failed closing connection to {0}: {1}", remoteUserId, result.ToStringCached<Result>()));
			}
			connectionInformation.State = NetworkServerEos.EConnectionState.Disconnected;
			this.OnPlayerDisconnected(remoteUserId);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void IncomingPacketQueueFullHandler(ref OnIncomingPacketQueueFullInfo _data)
		{
			if (!this.ServerRunning)
			{
				return;
			}
			Log.Error(string.Format("[EOS-P2PS] Packet queue full: Chn={0}, IncSize={1}, Used={2}, Max={3}", new object[] { _data.OverflowPacketChannel, _data.OverflowPacketSizeBytes, _data.PacketQueueCurrentSizeBytes, _data.PacketQueueMaxSizeBytes }));
		}

		public void Update()
		{
			if (!this.ServerRunning)
			{
				return;
			}
			long num = (long)(Time.unscaledTime * 1000f);
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
				uint num2;
				lock (obj)
				{
					byte b;
					result = this.ptpInterface.ReceivePacket(ref receivePacketOptions2, ref productUserId, ref socketId, ref b, this.receiveBuffer, ref num2);
				}
				if (result != null)
				{
					break;
				}
				NetworkServerEos.ConnectionInformation connectionInformation;
				if (!this.connections.TryGetValue(productUserId, out connectionInformation) || connectionInformation.State == NetworkServerEos.EConnectionState.Disconnected)
				{
					string text = "[EOS-P2PS] Received package from an unconnected client: ";
					ProductUserId productUserId2 = productUserId;
					Log.Out(text + ((productUserId2 != null) ? productUserId2.ToString() : null));
				}
				else if (num2 != 0U)
				{
					NetworkCommonEos.ESteamNetChannels esteamNetChannels = (NetworkCommonEos.ESteamNetChannels)this.receiveBuffer.Array[0];
					if (esteamNetChannels > NetworkCommonEos.ESteamNetChannels.NetpackageChannel1)
					{
						if (esteamNetChannels != NetworkCommonEos.ESteamNetChannels.Authentication)
						{
							if (esteamNetChannels != NetworkCommonEos.ESteamNetChannels.Ping)
							{
								string text2 = "[EOS-P2PS] Received package on an unknown channel from: ";
								ProductUserId productUserId3 = productUserId;
								Log.Out(text2 + ((productUserId3 != null) ? productUserId3.ToString() : null));
							}
							else
							{
								this.UpdatePing(productUserId, this.receiveBuffer.Array, num);
							}
						}
						else if (connectionInformation.State == NetworkServerEos.EConnectionState.Authenticating)
						{
							string @string = Encoding.UTF8.GetString(this.receiveBuffer.Array, 1, (int)(num2 - 1U));
							bool flag2 = this.Authenticate(productUserId, @string);
							SendPacketOptions sendPacketOptions = default(SendPacketOptions);
							sendPacketOptions.SocketId = new SocketId?(this.socketId);
							sendPacketOptions.LocalUserId = this.localUser;
							sendPacketOptions.RemoteUserId = productUserId;
							sendPacketOptions.Channel = 0;
							sendPacketOptions.Reliability = 2;
							sendPacketOptions.AllowDelayedDelivery = true;
							sendPacketOptions.Data = (flag2 ? NetworkServerEos.passwordValidPacket : NetworkServerEos.passwordInvalidPacket);
							SendPacketOptions sendPacketOptions2 = sendPacketOptions;
							obj = AntiCheatCommon.LockObject;
							Result result2;
							lock (obj)
							{
								result2 = this.ptpInterface.SendPacket(ref sendPacketOptions2);
							}
							if (result2 != null)
							{
								Log.Error(string.Format("[EOS-P2PS] Could not send package to client {0}: {1}", productUserId, result2.ToStringCached<Result>()));
							}
						}
					}
					else if (connectionInformation.State == NetworkServerEos.EConnectionState.Connected)
					{
						byte[] array = MemoryPools.poolByte.Alloc((int)num2);
						Array.Copy(this.receiveBuffer.Array, array, (long)((ulong)num2));
						ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForUserId(connectionInformation.UserIdentifier);
						SingletonMonoBehaviour<ConnectionManager>.Instance.Net_DataReceivedServer(clientInfo, (int)esteamNetChannels, array, (int)num2);
					}
					else
					{
						string text3 = "[EOS-P2PS] Received package from an unauthenticated client: ";
						ProductUserId productUserId4 = productUserId;
						Log.Out(text3 + ((productUserId4 != null) ? productUserId4.ToString() : null));
					}
				}
			}
			if (result != 18)
			{
				Log.Error("[EOS-P2PS] Error reading packages: " + result.ToStringCached<Result>());
			}
			if (result == 4)
			{
				this.StopServer();
				return;
			}
		}

		public void LateUpdate()
		{
			if (!this.ServerRunning)
			{
				return;
			}
			this.sendBuffers(this.sendBufs, 2);
			this.sendBuffers(this.sendBufsUnreliable, 0);
			Utils.GetBytes((long)(Time.unscaledTime * 1000f), NetworkServerEos.timeData.Array, 1);
			foreach (KeyValuePair<ProductUserId, NetworkServerEos.ConnectionInformation> keyValuePair in this.connections)
			{
				if (keyValuePair.Value.State == NetworkServerEos.EConnectionState.Connected)
				{
					this.FlushBuffer(keyValuePair.Key);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void sendBuffers(BlockingQueue<NetworkCommonEos.SendInfo> _buffers, PacketReliability _queue)
		{
			while (_buffers.HasData())
			{
				NetworkCommonEos.SendInfo sendInfo = _buffers.Dequeue();
				ProductUserId productUserId = ((UserIdentifierEos)sendInfo.Recipient.CrossplatformId).ProductUserId;
				NetworkServerEos.ConnectionInformation connectionInformation;
				if (this.connections.TryGetValue(productUserId, out connectionInformation) && connectionInformation.State == NetworkServerEos.EConnectionState.Connected)
				{
					SendPacketOptions sendPacketOptions = default(SendPacketOptions);
					sendPacketOptions.SocketId = new SocketId?(this.socketId);
					sendPacketOptions.LocalUserId = this.localUser;
					sendPacketOptions.RemoteUserId = productUserId;
					sendPacketOptions.Channel = 0;
					sendPacketOptions.Reliability = _queue;
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
						Log.Error(string.Format("[EOS-P2PS] Could not send package to client {0}: {1}", productUserId, result.ToStringCached<Result>()));
					}
				}
			}
		}

		public string GetIP(ClientInfo _cInfo)
		{
			NetworkServerEos.ConnectionInformation connectionInformation;
			if (!this.connections.TryGetValue(((UserIdentifierEos)_cInfo.CrossplatformId).ProductUserId, out connectionInformation))
			{
				return string.Empty;
			}
			return NetworkUtils.ToAddr(connectionInformation.Ip);
		}

		public int GetPing(ClientInfo _cInfo)
		{
			NetworkServerEos.ConnectionInformation connectionInformation;
			if (!this.connections.TryGetValue(((UserIdentifierEos)_cInfo.CrossplatformId).ProductUserId, out connectionInformation))
			{
				return -1;
			}
			int num = 0;
			for (int i = 0; i < 50; i++)
			{
				num += connectionInformation.Pings[i];
			}
			return num / 50;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnPlayerDisconnected(ProductUserId _id)
		{
			UserIdentifierEos userIdentifierEos = new UserIdentifierEos(_id);
			ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForUserId(userIdentifierEos);
			SingletonMonoBehaviour<ConnectionManager>.Instance.Net_PlayerDisconnected(clientInfo);
		}

		public string GetServerPorts(int _basePort)
		{
			return "";
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool Authenticate(ProductUserId _id, string _password)
		{
			bool flag = string.IsNullOrEmpty(this.serverPassword) || _password == this.serverPassword;
			Log.Out(string.Format("[EOS-P2PS] Received authentication package from {0}: {1} password", _id, flag ? "valid" : "invalid"));
			if (!flag)
			{
				this.connections[_id].State = NetworkServerEos.EConnectionState.Authenticating;
				return false;
			}
			this.connections[_id].State = NetworkServerEos.EConnectionState.Connected;
			this.OnPlayerConnected(_id);
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnPlayerConnected(ProductUserId _id)
		{
			ClientInfo clientInfo = new ClientInfo
			{
				CrossplatformId = new UserIdentifierEos(_id),
				network = this,
				netConnection = new INetConnection[2]
			};
			for (int i = 0; i < 2; i++)
			{
				clientInfo.netConnection[i] = new NetConnectionSimple(i, clientInfo, null, clientInfo.InternalId.CombinedString, 1, 1120);
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.AddClient(clientInfo);
			SingletonMonoBehaviour<ConnectionManager>.Instance.Net_PlayerConnected(clientInfo);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void FlushBuffer(ProductUserId _id)
		{
			SendPacketOptions sendPacketOptions = default(SendPacketOptions);
			sendPacketOptions.SocketId = new SocketId?(this.socketId);
			sendPacketOptions.LocalUserId = this.localUser;
			sendPacketOptions.RemoteUserId = _id;
			sendPacketOptions.Channel = 0;
			sendPacketOptions.Reliability = 2;
			sendPacketOptions.AllowDelayedDelivery = true;
			sendPacketOptions.Data = NetworkServerEos.timeData;
			SendPacketOptions sendPacketOptions2 = sendPacketOptions;
			object lockObject = AntiCheatCommon.LockObject;
			Result result;
			lock (lockObject)
			{
				result = this.ptpInterface.SendPacket(ref sendPacketOptions2);
			}
			if (result != null)
			{
				Log.Error(string.Format("[EOS-P2PS] Could not send ping package to client {0}: {1}", _id, result.ToStringCached<Result>()));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void UpdatePing(ProductUserId _sourceId, byte[] _data, long _curTime)
		{
			long num = BitConverter.ToInt64(_data, 1);
			int num2 = (int)(_curTime - num);
			NetworkServerEos.ConnectionInformation connectionInformation = this.connections[_sourceId];
			connectionInformation.LastPingIndex++;
			if (connectionInformation.LastPingIndex >= 50)
			{
				connectionInformation.LastPingIndex = 0;
			}
			connectionInformation.Pings[connectionInformation.LastPingIndex] = num2;
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

		public int GetMaximumPacketSize(ClientInfo _cInfo, bool _reliable = false)
		{
			return 1170;
		}

		public int GetBadPacketCount(ClientInfo _cInfo)
		{
			return 0;
		}

		// Note: this type is marked as 'beforefieldinit'.
		[PublicizedFrom(EAccessModifier.Private)]
		static NetworkServerEos()
		{
			byte[] array = new byte[2];
			array[0] = 50;
			NetworkServerEos.passwordInvalidPacket = new ArraySegment<byte>(array);
			byte[] array2 = new byte[9];
			array2[0] = 60;
			NetworkServerEos.timeData = new ArraySegment<byte>(array2);
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
		public bool serverStarted;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int PingCount = 50;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<ProductUserId, NetworkServerEos.ConnectionInformation> connections = new Dictionary<ProductUserId, NetworkServerEos.ConnectionInformation>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BlockingQueue<NetworkCommonEos.SendInfo> sendBufs = new BlockingQueue<NetworkCommonEos.SendInfo>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BlockingQueue<NetworkCommonEos.SendInfo> sendBufsUnreliable = new BlockingQueue<NetworkCommonEos.SendInfo>();

		[PublicizedFrom(EAccessModifier.Private)]
		public string serverPassword;

		[PublicizedFrom(EAccessModifier.Private)]
		public ProductUserId localUser;

		[PublicizedFrom(EAccessModifier.Private)]
		public SocketId socketId;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ArraySegment<byte> receiveBuffer = new ArraySegment<byte>(new byte[1170]);

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly ArraySegment<byte> passwordValidPacket = new ArraySegment<byte>(new byte[] { 50, 1 });

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly ArraySegment<byte> passwordInvalidPacket;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly ArraySegment<byte> timeData;

		[PublicizedFrom(EAccessModifier.Private)]
		public enum EConnectionState
		{
			Disconnected,
			Authenticating,
			Connected
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public class ConnectionInformation
		{
			public NetworkServerEos.EConnectionState State;

			public uint Ip;

			public UserIdentifierEos UserIdentifier;

			public int LastPingIndex = -1;

			public readonly int[] Pings = new int[50];
		}
	}
}
