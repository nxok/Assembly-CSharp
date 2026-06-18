using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using UnityEngine.Networking;

public class NetworkServerLiteNetLib : INetworkServer
{
	public NetworkServerLiteNetLib(IProtocolManagerProtocolInterface _protoManager)
	{
		this.protoManager = _protoManager;
	}

	public void Update()
	{
		ClientInfo clientInfo;
		do
		{
			clientInfo = null;
			List<ClientInfo> list = this.dropClientsQueue;
			lock (list)
			{
				int num = this.dropClientsQueue.Count - 1;
				if (num >= 0)
				{
					clientInfo = this.dropClientsQueue[num];
					this.dropClientsQueue.RemoveAt(num);
				}
			}
			if (clientInfo != null)
			{
				this.DropClient(clientInfo, false);
			}
		}
		while (clientInfo != null);
		NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer liteNetLibAuthWrapperServer = this.authWrapper;
		if (liteNetLibAuthWrapperServer == null)
		{
			return;
		}
		liteNetLibAuthWrapperServer.Update();
	}

	public void LateUpdate()
	{
	}

	public NetworkConnectionError StartServer(int _basePort, string _password)
	{
		this.serverPassword = (string.IsNullOrEmpty(_password) ? "" : _password);
		EventBasedNetListener eventBasedNetListener = new EventBasedNetListener();
		this.server = new NetManager(eventBasedNetListener, null, true);
		this.authWrapper = new NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer(this);
		NetworkCommonLiteNetLib.InitConfig(this.server);
		eventBasedNetListener.ConnectionRequestEvent += new EventBasedNetListener.OnConnectionRequest(this.authWrapper.ConnectionRequestCheck);
		eventBasedNetListener.PeerConnectedEvent += new EventBasedNetListener.OnPeerConnected(this.authWrapper.OnPeerConnectedEvent);
		eventBasedNetListener.PeerDisconnectedEvent += new EventBasedNetListener.OnPeerDisconnected(this.authWrapper.OnPeerDisconnectedEvent);
		eventBasedNetListener.NetworkReceiveEvent += new EventBasedNetListener.OnNetworkReceive(this.authWrapper.OnNetworkReceiveEvent);
		eventBasedNetListener.NetworkErrorEvent += delegate(IPEndPoint _endpoint, SocketError _code)
		{
			Log.Error("NET: LiteNetLib: Network error: {0}", new object[] { _code });
		};
		if (this.server.Start(_basePort + 2))
		{
			Log.Out("NET: LiteNetLib server started");
			return NetworkConnectionError.NoError;
		}
		Log.Out("NET: LiteNetLib server could not be started");
		return NetworkConnectionError.CreateSocketOrThreadFailure;
	}

	public void SetServerPassword(string _password)
	{
		this.serverPassword = _password ?? "";
	}

	public void StopServer()
	{
		NetManager netManager = this.server;
		if (netManager != null && netManager.IsRunning)
		{
			List<NetPeer> list = new List<NetPeer>();
			this.server.GetPeersNonAlloc(list, 46);
			for (int i = 0; i < list.Count; i++)
			{
				this.server.DisconnectPeer(list[i], NetworkServerLiteNetLib.disconnectServerShutdown);
			}
			this.server.Stop();
			this.server = null;
			this.authWrapper = null;
		}
		Log.Out("NET: LiteNetLib server stopped");
	}

	public void DropClient(ClientInfo _clientInfo, bool _clientDisconnect)
	{
		this.OnPlayerDisconnected(_clientInfo.litenetPeerConnectId);
		NetPeer peerByConnectId = this.GetPeerByConnectId(_clientInfo.litenetPeerConnectId);
		if (peerByConnectId != null)
		{
			this.server.DisconnectPeer(peerByConnectId, NetworkServerLiteNetLib.disconnectFromClientSide);
		}
	}

	public NetworkError SendData(ClientInfo _cInfo, int _channel, ArrayListMP<byte> _data, bool _reliableDelivery = true)
	{
		NetPeer peerByConnectId = this.GetPeerByConnectId(_cInfo.litenetPeerConnectId);
		if (peerByConnectId == null)
		{
			Log.Warning("NET: LiteNetLib: SendData requested for unknown client {0}", new object[] { _cInfo.ToString() });
			List<ClientInfo> list = this.dropClientsQueue;
			lock (list)
			{
				if (!this.dropClientsQueue.Contains(_cInfo))
				{
					this.dropClientsQueue.Add(_cInfo);
				}
			}
			return NetworkError.WrongConnection;
		}
		_data[0] = (byte)_channel;
		if (ConnectionManager.VerboseNetLogging)
		{
			Log.Out("Sending data to peer {2}: ch={0}, size={1}", new object[]
			{
				_channel,
				_data.Count,
				_cInfo.InternalId.CombinedString
			});
		}
		peerByConnectId.Send(_data.Items, 0, _data.Count, _reliableDelivery ? 2 : 4);
		return NetworkError.Ok;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PeerConnectedEvent(NetPeer _peer)
	{
		Log.Out(string.Format("NET: LiteNetLib: Connect from: {0} / {1}", _peer, _peer.Id));
		ClientInfo clientInfo = new ClientInfo
		{
			litenetPeerConnectId = (long)_peer.Id,
			network = this,
			netConnection = new INetConnection[2]
		};
		for (int i = 0; i < 2; i++)
		{
			INetConnection[] netConnection = clientInfo.netConnection;
			int num = i;
			int num2 = i;
			ClientInfo clientInfo2 = clientInfo;
			INetworkClient networkClient = null;
			int id = _peer.Id;
			netConnection[num] = new NetConnectionSimple(num2, clientInfo2, networkClient, id.ToString(), 1, 0);
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.AddClient(clientInfo);
		SingletonMonoBehaviour<ConnectionManager>.Instance.Net_PlayerConnected(clientInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PeerDisconnectedEvent(NetPeer _peer, DisconnectInfo _info)
	{
		Log.Out(string.Format("NET: LiteNetLib: Client disconnect from: {0} / {1} ({2})", _peer, _peer.Id, _info.Reason.ToStringCached<DisconnectReason>()));
		if (_info.Reason == 1)
		{
			Log.Out(string.Format("NET: LiteNetLib: TimeSinceLastPacket: {0}", _peer.TimeSinceLastPacket));
		}
		ThreadManager.AddSingleTaskMainThread("PlayerDisconnectLiteNetLib", delegate(object _)
		{
			Log.Out(string.Format("NET: LiteNetLib: MT: Client disconnect from: {0} / {1} ({2})", _peer, _peer.Id, _info.Reason.ToStringCached<DisconnectReason>()));
			this.OnPlayerDisconnected((long)_peer.Id);
		}, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NetworkReceiveEvent(NetPeer _peer, NetPacketReader _reader, DeliveryMethod _deliveryMethod)
	{
		ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForLiteNetPeer((long)_peer.Id);
		if (clientInfo == null)
		{
			Log.Out("NET: LiteNetLib: Received package from an unknown client: " + ((_peer != null) ? _peer.ToString() : null));
			return;
		}
		if (_reader.AvailableBytes == 0)
		{
			string text = "NET: LiteNetLib: Received package with zero size from: ";
			ClientInfo clientInfo2 = clientInfo;
			Log.Out(text + ((clientInfo2 != null) ? clientInfo2.ToString() : null));
			return;
		}
		int availableBytes = _reader.AvailableBytes;
		byte[] array = MemoryPools.poolByte.Alloc(availableBytes);
		_reader.GetBytes(array, availableBytes);
		if (ConnectionManager.VerboseNetLogging)
		{
			Log.Out("Received data from peer {2}: ch={0}, size={1}", new object[]
			{
				array[0],
				availableBytes,
				clientInfo.InternalId.CombinedString
			});
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.Net_DataReceivedServer(clientInfo, (int)array[0], array, availableBytes);
	}

	public string GetIP(ClientInfo _cInfo)
	{
		NetPeer peerByConnectId = this.GetPeerByConnectId(_cInfo.litenetPeerConnectId);
		if (peerByConnectId == null)
		{
			Log.Warning("NET: LiteNetLib: IP requested for unknown client {0}", new object[] { _cInfo.ToString() });
			return string.Empty;
		}
		return peerByConnectId.Address.ToString();
	}

	public int GetPing(ClientInfo _cInfo)
	{
		NetPeer peerByConnectId = this.GetPeerByConnectId(_cInfo.litenetPeerConnectId);
		if (peerByConnectId == null)
		{
			Log.Warning("NET: LiteNetLib: Ping requested for unknown client {0}", new object[] { _cInfo.ToString() });
			return -1;
		}
		return peerByConnectId.Ping;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPlayerDisconnected(long _peerConnectId)
	{
		ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForLiteNetPeer(_peerConnectId);
		if (clientInfo != null)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.Net_PlayerDisconnected(clientInfo);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPeer GetPeerByConnectId(long _connectId)
	{
		if (this.server == null)
		{
			return null;
		}
		List<NetPeer> list = this.getPeersList;
		lock (list)
		{
			this.server.GetPeersNonAlloc(this.getPeersList, 46);
			for (int i = 0; i < this.getPeersList.Count; i++)
			{
				if ((long)this.getPeersList[i].Id == _connectId)
				{
					return this.getPeersList[i];
				}
			}
		}
		return null;
	}

	public int GetBadPacketCount(ClientInfo _cInfo)
	{
		NetPeer peerByConnectId = this.GetPeerByConnectId(_cInfo.litenetPeerConnectId);
		if (peerByConnectId != null)
		{
			return peerByConnectId.badPacketCount;
		}
		return 0;
	}

	public string GetServerPorts(int _basePort)
	{
		return (_basePort + 2).ToString() + "/UDP";
	}

	public void SetLatencySimulation(bool _enable, int _minLatency, int _maxLatency)
	{
		if (this.server != null)
		{
			this.server.SimulateLatency = _enable;
			this.server.SimulationMinLatency = _minLatency;
			this.server.SimulationMaxLatency = _maxLatency;
		}
	}

	public void SetPacketLossSimulation(bool _enable, int _chance)
	{
		if (this.server != null)
		{
			this.server.SimulatePacketLoss = _enable;
			this.server.SimulationPacketLossChance = _chance;
		}
	}

	public void EnableStatistics()
	{
		if (this.server != null)
		{
			this.server.EnableStatistics = true;
		}
	}

	public void DisableStatistics()
	{
		if (this.server != null)
		{
			this.server.EnableStatistics = false;
		}
	}

	public string PrintNetworkStatistics()
	{
		if (this.server != null)
		{
			return this.server.Statistics.ToString();
		}
		return "no server!";
	}

	public void ResetNetworkStatistics()
	{
		if (this.server != null)
		{
			this.server.Statistics.Reset();
		}
	}

	public int GetMaximumPacketSize(ClientInfo _cInfo, bool _reliable = false)
	{
		int num = -1;
		NetPeer peerByConnectId = this.GetPeerByConnectId(_cInfo.litenetPeerConnectId);
		if (peerByConnectId != null)
		{
			num = peerByConnectId.GetMaxSinglePacketSize(_reliable ? 2 : 4);
		}
		return num;
	}

	// Note: this type is marked as 'beforefieldinit'.
	[PublicizedFrom(EAccessModifier.Private)]
	static NetworkServerLiteNetLib()
	{
		byte[] array = new byte[2];
		array[0] = 1;
		NetworkServerLiteNetLib.rejectRateLimit = array;
		byte[] array2 = new byte[2];
		array2[0] = 2;
		NetworkServerLiteNetLib.rejectPendingConnection = array2;
		byte[] array3 = new byte[2];
		array3[0] = 3;
		NetworkServerLiteNetLib.disconnectServerShutdown = array3;
		byte[] array4 = new byte[2];
		array4[0] = 4;
		NetworkServerLiteNetLib.disconnectFromClientSide = array4;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] rejectInvalidPassword = new byte[2];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] rejectRateLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] rejectPendingConnection;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] disconnectServerShutdown;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] disconnectFromClientSide;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IProtocolManagerProtocolInterface protoManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public string serverPassword;

	[PublicizedFrom(EAccessModifier.Private)]
	public NetManager server;

	[PublicizedFrom(EAccessModifier.Private)]
	public NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer authWrapper;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ClientInfo> dropClientsQueue = new List<ClientInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<NetPeer> getPeersList = new List<NetPeer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public class LiteNetLibAuthWrapperServer
	{
		public LiteNetLibAuthWrapperServer(NetworkServerLiteNetLib _owner)
		{
			this.owner = _owner;
		}

		public void Update()
		{
			DateTime now = DateTime.Now;
			if (now < this.nextStateChecksRun)
			{
				return;
			}
			this.nextStateChecksRun = now + NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.ConnectionStateCheckInterval;
			Dictionary<int, NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.LNLAuthConnectionState> dictionary = this.authStates;
			lock (dictionary)
			{
				this.clearList.Clear();
				foreach (KeyValuePair<int, NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.LNLAuthConnectionState> keyValuePair in this.authStates)
				{
					int num;
					NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.LNLAuthConnectionState lnlauthConnectionState;
					keyValuePair.Deconstruct(out num, out lnlauthConnectionState);
					int num2 = num;
					NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.LNLAuthConnectionState lnlauthConnectionState2 = lnlauthConnectionState;
					if (lnlauthConnectionState2.State == NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.LNLAuthConnectionState.EState.Authenticating && !(lnlauthConnectionState2.Age <= NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.MaxDurationInAuthState))
					{
						this.clearList.Add(num2);
					}
				}
				foreach (int num3 in this.clearList)
				{
					this.authStates.Remove(num3);
				}
			}
		}

		public void ConnectionRequestCheck(ConnectionRequest _request)
		{
			string text = _request.RemoteEndPoint.Address.ToString();
			DateTime now = DateTime.Now;
			DateTime dateTime;
			this.lastConnectAttemptTimes.TryGetValue(text, out dateTime);
			TimeSpan timeSpan = now - dateTime;
			this.lastConnectAttemptTimes[text] = now;
			if (timeSpan.TotalMilliseconds < 500.0)
			{
				Log.Out("NET: Rejecting connection request from " + text + ": Limiting connect rate from that IP!");
				_request.Reject(NetworkServerLiteNetLib.rejectRateLimit);
				return;
			}
			foreach (ClientInfo clientInfo in SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List)
			{
				if (!clientInfo.loginDone && clientInfo.ip == text)
				{
					Log.Out("NET: Rejecting connection request from " + text + ": A connection attempt from that IP is currently being processed!");
					_request.Reject(NetworkServerLiteNetLib.rejectPendingConnection);
					return;
				}
			}
			if (_request.Data.GetString() != this.owner.serverPassword)
			{
				_request.Reject(NetworkServerLiteNetLib.rejectInvalidPassword);
				return;
			}
			_request.Accept();
		}

		public void OnPeerConnectedEvent(NetPeer _peer)
		{
			Dictionary<int, NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.LNLAuthConnectionState> dictionary = this.authStates;
			NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.LNLAuthConnectionState lnlauthConnectionState;
			lock (dictionary)
			{
				if (this.authStates.TryGetValue(_peer.Id, out lnlauthConnectionState) && ConnectionManager.VerboseNetLogging)
				{
					Log.Out(string.Format("NET: LiteNetLib: New peer {0} with previously used peer id {1}", _peer, _peer.Id));
				}
				lnlauthConnectionState = new NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.LNLAuthConnectionState();
				this.authStates[_peer.Id] = lnlauthConnectionState;
			}
			byte[] array = new byte[17];
			array[0] = 202;
			lnlauthConnectionState.Challenge.WriteToBuffer(array, 1);
			if (ConnectionManager.VerboseNetLogging)
			{
				Log.Out(string.Format("NET: LiteNetLib: Sending challenge {0} to new peer {1} / {2}", lnlauthConnectionState.Challenge, _peer, _peer.Id));
			}
			_peer.Send(array, 0, array.Length, 2);
		}

		public void OnPeerDisconnectedEvent(NetPeer _peer, DisconnectInfo _disconnectInfo)
		{
			Dictionary<int, NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.LNLAuthConnectionState> dictionary = this.authStates;
			NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.LNLAuthConnectionState lnlauthConnectionState;
			lock (dictionary)
			{
				if (!this.authStates.TryGetValue(_peer.Id, out lnlauthConnectionState))
				{
					return;
				}
			}
			dictionary = this.authStates;
			lock (dictionary)
			{
				this.authStates.Remove(_peer.Id);
			}
			if (lnlauthConnectionState.State == NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.LNLAuthConnectionState.EState.Authenticating)
			{
				Log.Out(string.Format("NET: LiteNetLib: Peer disconnected in auth state: {0} / {1}", _peer, _peer.Id));
				return;
			}
			this.owner.PeerDisconnectedEvent(_peer, _disconnectInfo);
		}

		public void OnNetworkReceiveEvent(NetPeer _peer, NetPacketReader _reader, byte _channel, DeliveryMethod _deliveryMethod)
		{
			Dictionary<int, NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.LNLAuthConnectionState> dictionary = this.authStates;
			NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.LNLAuthConnectionState lnlauthConnectionState;
			lock (dictionary)
			{
				if (!this.authStates.TryGetValue(_peer.Id, out lnlauthConnectionState))
				{
					return;
				}
			}
			if (lnlauthConnectionState.State == NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.LNLAuthConnectionState.EState.Authenticating)
			{
				int availableBytes = _reader.AvailableBytes;
				if (availableBytes != 17)
				{
					Log.Out(string.Format("NET: LiteNetLib: Received invalid package (length={0}) from an client in auth state: {1} / {2}", availableBytes, _peer, _peer.Id));
					_peer.Disconnect();
					return;
				}
				byte[] array = MemoryPools.poolByte.Alloc(availableBytes);
				_reader.GetBytes(array, availableBytes);
				if (array[0] != 202)
				{
					Log.Out(string.Format("NET: LiteNetLib: Received invalid package (channel={0}) from an client in auth state: {1} / {2}", array[0], _peer, _peer.Id));
					_peer.Disconnect();
					MemoryPools.poolByte.Free(array);
					return;
				}
				byte[] array2 = new byte[16];
				Array.Copy(array, 1, array2, 0, 16);
				Guid guid = new Guid(array2);
				if (guid != lnlauthConnectionState.Challenge)
				{
					Log.Out(string.Format("NET: LiteNetLib: Received invalid challenge {0} from an client in auth state: {1} / {2}", guid, _peer, _peer.Id));
					_peer.Disconnect();
					MemoryPools.poolByte.Free(array);
					return;
				}
				if (ConnectionManager.VerboseNetLogging)
				{
					Log.Out(string.Format("NET: LiteNetLib: Received correct challenge {0} from new peer {1} / {2}, accepting", guid, _peer, _peer.Id));
				}
				lnlauthConnectionState.State = NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.LNLAuthConnectionState.EState.Connected;
				this.owner.PeerConnectedEvent(_peer);
				MemoryPools.poolByte.Free(array);
				return;
			}
			else
			{
				this.owner.NetworkReceiveEvent(_peer, _reader, _deliveryMethod);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly TimeSpan ConnectionStateCheckInterval = TimeSpan.FromSeconds(10.0);

		[PublicizedFrom(EAccessModifier.Private)]
		public const int ConnectionRateLimitMilliseconds = 500;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly TimeSpan MaxDurationInAuthState = new TimeSpan(0, 0, 0, 10);

		[PublicizedFrom(EAccessModifier.Private)]
		public const int ChallengePackageSize = 17;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly NetworkServerLiteNetLib owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, DateTime> lastConnectAttemptTimes = new Dictionary<string, DateTime>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<int, NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.LNLAuthConnectionState> authStates = new Dictionary<int, NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.LNLAuthConnectionState>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<int> clearList = new List<int>();

		[PublicizedFrom(EAccessModifier.Private)]
		public DateTime nextStateChecksRun;

		[PublicizedFrom(EAccessModifier.Private)]
		public class LNLAuthConnectionState
		{
			public TimeSpan Age
			{
				get
				{
					return DateTime.Now - this.StartTime;
				}
			}

			public NetworkServerLiteNetLib.LiteNetLibAuthWrapperServer.LNLAuthConnectionState.EState State;

			public readonly Guid Challenge = Guid.NewGuid();

			public readonly DateTime StartTime = DateTime.Now;

			public enum EState
			{
				Authenticating,
				Connected
			}
		}
	}
}
