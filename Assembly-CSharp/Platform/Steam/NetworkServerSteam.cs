using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;

namespace Platform.Steam
{
	public class NetworkServerSteam : IPlatformNetworkServer, INetworkServer
	{
		public bool ServerRunning
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return this.protoManager.IsServer && this.handlerThread != null;
			}
		}

		public bool SteamGameServerRunning
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return this.owner.ServerListAnnouncer.GameServerInitialized;
			}
		}

		public NetworkServerSteam(IPlatform _owner, IProtocolManagerProtocolInterface _protoManager)
		{
			this.owner = _owner;
			this.protoManager = _protoManager;
			this.owner.Api.ClientApiInitialized += delegate
			{
				this.m_P2PSessionRequest = Callback<P2PSessionRequest_t>.CreateGameServer(new Callback<P2PSessionRequest_t>.DispatchDelegate(this.P2PSessionRequest));
			};
		}

		public NetworkConnectionError StartServer(int _basePort, string _password)
		{
			if (this.ServerRunning)
			{
				Log.Error("[Steamworks.NET] NET: Server already running");
				return NetworkConnectionError.AlreadyConnectedToServer;
			}
			this.serverPassword = (string.IsNullOrEmpty(_password) ? null : _password);
			this.handlerThread = ThreadManager.StartThread("SteamNetworkingServer", new ThreadManager.ThreadFunctionDelegate(this.threadHandlerMethod), null, null, true, false);
			Log.Out("[Steamworks.NET] NET: Server started");
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
			this.handlerThread.WaitForEnd(30);
			this.handlerThread = null;
			this.checkConnections.Clear();
			foreach (KeyValuePair<CSteamID, NetworkServerSteam.ConnectionInformation> keyValuePair in this.connections)
			{
				if (keyValuePair.Value.State != NetworkServerSteam.EConnectionState.Disconnected)
				{
					keyValuePair.Value.State = NetworkServerSteam.EConnectionState.Disconnected;
					SteamGameServerNetworking.CloseP2PSessionWithUser(keyValuePair.Key);
				}
			}
			this.connections.Clear();
			this.sendBufs.Clear();
			this.sendBufsUnreliable.Clear();
			this.acceptQueue.Clear();
			this.dropQueue.Clear();
			this.disconnectQueue.Clear();
			Log.Out("[Steamworks.NET] NET: Server stopped");
		}

		public void DropClient(ClientInfo _clientInfo, bool _clientDisconnect)
		{
			CSteamID csteamID;
			csteamID..ctor(((UserIdentifierSteam)_clientInfo.PlatformId).SteamId);
			ThreadManager.StartCoroutine(this.dropLater(csteamID, 0.2f));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator dropLater(CSteamID _id, float _delay)
		{
			yield return new WaitForSeconds(_delay);
			if (!this.ServerRunning)
			{
				yield break;
			}
			NetworkServerSteam.ConnectionInformation connectionInformation;
			if (this.connections.TryGetValue(_id, out connectionInformation))
			{
				string text = "[Steamworks.NET] NET: Dropping client: ";
				CSteamID csteamID = _id;
				Log.Out(text + csteamID.ToString());
				connectionInformation.State = NetworkServerSteam.EConnectionState.Disconnected;
				this.OnPlayerDisconnected(_id);
			}
			this.dropQueue.Enqueue(_id);
			yield break;
		}

		public NetworkError SendData(ClientInfo _clientInfo, int _channel, ArrayListMP<byte> _data, bool reliableDelivery = true)
		{
			if (this.ServerRunning)
			{
				CSteamID csteamID;
				csteamID..ctor(((UserIdentifierSteam)_clientInfo.PlatformId).SteamId);
				_data[_data.Count - 1] = (byte)_channel;
				if (GameManager.unreliableNetPackets && !reliableDelivery && _data.Count <= this.GetMaximumPacketSize(_clientInfo, false))
				{
					this.sendBufsUnreliable.Enqueue(new NetworkCommonSteam.SendInfo(csteamID, _data));
				}
				else
				{
					this.sendBufs.Enqueue(new NetworkCommonSteam.SendInfo(csteamID, _data));
				}
				this.signalThread.Set();
			}
			else
			{
				Log.Warning("[Steamworks.NET] NET: Tried to send a package to a client while not being a server");
			}
			return NetworkError.Ok;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void P2PSessionRequest(P2PSessionRequest_t _par)
		{
			if (!this.ServerRunning)
			{
				return;
			}
			Log.Out("[Steamworks.NET] NET: P2PSessionRequest from: " + _par.m_steamIDRemote.m_SteamID.ToString());
			this.acceptQueue.Enqueue(_par.m_steamIDRemote);
		}

		public void Update()
		{
			if (!this.ServerRunning)
			{
				return;
			}
			while (this.disconnectQueue.HasData())
			{
				CSteamID csteamID = this.disconnectQueue.Dequeue();
				this.OnPlayerDisconnected(csteamID);
			}
		}

		public void LateUpdate()
		{
			this.flushBuffers = true;
			this.signalThread.Set();
		}

		public string GetIP(ClientInfo _cInfo)
		{
			NetworkServerSteam.ConnectionInformation connectionInformation;
			if (!this.connections.TryGetValue(new CSteamID(((UserIdentifierSteam)_cInfo.PlatformId).SteamId), out connectionInformation))
			{
				return string.Empty;
			}
			return NetworkUtils.ToAddr(connectionInformation.Ip);
		}

		public int GetPing(ClientInfo _cInfo)
		{
			NetworkServerSteam.ConnectionInformation connectionInformation;
			if (!this.connections.TryGetValue(new CSteamID(((UserIdentifierSteam)_cInfo.PlatformId).SteamId), out connectionInformation))
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
		public void OnPlayerDisconnected(CSteamID _id)
		{
			UserIdentifierSteam userIdentifierSteam = new UserIdentifierSteam(_id);
			ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForUserId(userIdentifierSteam);
			SingletonMonoBehaviour<ConnectionManager>.Instance.Net_PlayerDisconnected(clientInfo);
		}

		public string GetServerPorts(int _basePort)
		{
			return "";
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void threadHandlerMethod(ThreadManager.ThreadInfo _threadinfo)
		{
			while (!_threadinfo.TerminationRequested())
			{
				this.signalThread.WaitOne(6);
				if (this.ServerRunning && this.SteamGameServerRunning)
				{
					while (this.acceptQueue.HasData())
					{
						CSteamID csteamID = this.acceptQueue.Dequeue();
						UserIdentifierSteam userIdentifierSteam = new UserIdentifierSteam(csteamID);
						this.connections[csteamID] = new NetworkServerSteam.ConnectionInformation
						{
							State = NetworkServerSteam.EConnectionState.Authenticating,
							UserIdentifier = userIdentifierSteam
						};
						SteamGameServerNetworking.AcceptP2PSessionWithUser(csteamID);
					}
					this.CheckConnections();
					this.ReceivePackets();
					while (this.sendBufs.HasData())
					{
						NetworkCommonSteam.SendInfo sendInfo = this.sendBufs.Dequeue();
						CSteamID recipient = sendInfo.Recipient;
						NetworkServerSteam.ConnectionInformation connectionInformation;
						if (this.connections.TryGetValue(recipient, out connectionInformation) && connectionInformation.State == NetworkServerSteam.EConnectionState.Connected)
						{
							if (!SteamGameServerNetworking.SendP2PPacket(recipient, sendInfo.Data.Items, (uint)sendInfo.Data.Count, 3, 0))
							{
								string text = "[Steamworks.NET] NET: Could not send package to client ";
								CSteamID csteamID2 = recipient;
								Log.Error(text + csteamID2.ToString());
							}
							else
							{
								connectionInformation.PacketsPendingSend = true;
							}
						}
					}
					while (this.sendBufsUnreliable.HasData())
					{
						NetworkCommonSteam.SendInfo sendInfo2 = this.sendBufsUnreliable.Dequeue();
						CSteamID recipient2 = sendInfo2.Recipient;
						NetworkServerSteam.ConnectionInformation connectionInformation2;
						if (this.connections.TryGetValue(recipient2, out connectionInformation2) && connectionInformation2.State == NetworkServerSteam.EConnectionState.Connected)
						{
							if (!SteamGameServerNetworking.SendP2PPacket(recipient2, sendInfo2.Data.Items, (uint)sendInfo2.Data.Count, 0, 0))
							{
								string text2 = "[Steamworks.NET] NET: Could not send package to client ";
								CSteamID csteamID2 = recipient2;
								Log.Error(text2 + csteamID2.ToString());
							}
							else
							{
								connectionInformation2.PacketsPendingSend = true;
							}
						}
					}
					if (this.flushBuffers)
					{
						this.flushBuffers = false;
						Utils.GetBytes(this.mswPing.ElapsedMilliseconds, NetworkServerSteam.timeData, 0);
						using (Dictionary<CSteamID, NetworkServerSteam.ConnectionInformation>.Enumerator enumerator = this.connections.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								KeyValuePair<CSteamID, NetworkServerSteam.ConnectionInformation> keyValuePair = enumerator.Current;
								if (keyValuePair.Value.State == NetworkServerSteam.EConnectionState.Connected && keyValuePair.Value.PacketsPendingSend)
								{
									keyValuePair.Value.PacketsPendingSend = false;
									this.FlushBuffer(keyValuePair.Key);
								}
							}
							goto IL_0244;
						}
						goto IL_0233;
					}
					IL_0244:
					if (!this.dropQueue.HasData())
					{
						continue;
					}
					IL_0233:
					SteamGameServerNetworking.CloseP2PSessionWithUser(this.dropQueue.Dequeue());
					goto IL_0244;
				}
				this.signalThread.WaitOne(100);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CheckConnections()
		{
			if (this.checkConnections.Count == 0)
			{
				foreach (KeyValuePair<CSteamID, NetworkServerSteam.ConnectionInformation> keyValuePair in this.connections)
				{
					if (keyValuePair.Value.State != NetworkServerSteam.EConnectionState.Disconnected)
					{
						this.checkConnections.Add(keyValuePair.Key, keyValuePair.Value);
					}
				}
				this.checkPerFrame = (this.checkConnections.Count + 9) / 10;
			}
			int num = 0;
			while (num < this.checkPerFrame && this.checkConnections.Count != 0)
			{
				Dictionary<CSteamID, NetworkServerSteam.ConnectionInformation>.Enumerator enumerator2 = this.checkConnections.GetEnumerator();
				enumerator2.MoveNext();
				KeyValuePair<CSteamID, NetworkServerSteam.ConnectionInformation> keyValuePair2 = enumerator2.Current;
				enumerator2.Dispose();
				if (keyValuePair2.Value.State != NetworkServerSteam.EConnectionState.Disconnected)
				{
					P2PSessionState_t p2PSessionState_t;
					if (SteamGameServerNetworking.GetP2PSessionState(keyValuePair2.Key, ref p2PSessionState_t))
					{
						if (p2PSessionState_t.m_bConnectionActive == 0 && p2PSessionState_t.m_bConnecting == 0)
						{
							Log.Out("[Steamworks.NET] NET: Connection closed: " + keyValuePair2.Key.ToString());
							SteamGameServerNetworking.CloseP2PSessionWithUser(keyValuePair2.Key);
							keyValuePair2.Value.State = NetworkServerSteam.EConnectionState.Disconnected;
							this.disconnectQueue.Enqueue(keyValuePair2.Key);
						}
						else if (keyValuePair2.Value.Ip == 0U)
						{
							keyValuePair2.Value.Ip = p2PSessionState_t.m_nRemoteIP;
						}
					}
					else
					{
						Log.Out("[Steamworks.NET] NET: No connection to client: " + keyValuePair2.Key.ToString());
						SteamGameServerNetworking.CloseP2PSessionWithUser(keyValuePair2.Key);
						keyValuePair2.Value.State = NetworkServerSteam.EConnectionState.Disconnected;
						this.disconnectQueue.Enqueue(keyValuePair2.Key);
					}
				}
				num++;
				this.checkConnections.Remove(keyValuePair2.Key);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ReceivePackets()
		{
			long elapsedMilliseconds = this.mswPing.ElapsedMilliseconds;
			uint num;
			CSteamID csteamID;
			bool flag = SteamGameServerNetworking.ReadP2PPacket(this.recvBuf, (uint)this.recvBuf.Length, ref num, ref csteamID, 0);
			while (flag)
			{
				NetworkServerSteam.ConnectionInformation connectionInformation;
				if (!this.connections.TryGetValue(csteamID, out connectionInformation) || connectionInformation.State == NetworkServerSteam.EConnectionState.Disconnected)
				{
					string text = "[Steamworks.NET] NET: Received package from an unconnected client: ";
					CSteamID csteamID2 = csteamID;
					Log.Out(text + csteamID2.ToString());
				}
				else if (num != 0U)
				{
					num -= 1U;
					NetworkCommonSteam.ESteamNetChannels esteamNetChannels = (NetworkCommonSteam.ESteamNetChannels)this.recvBuf[(int)num];
					if (esteamNetChannels > NetworkCommonSteam.ESteamNetChannels.NetpackageChannel1)
					{
						if (esteamNetChannels != NetworkCommonSteam.ESteamNetChannels.Authentication)
						{
							if (esteamNetChannels != NetworkCommonSteam.ESteamNetChannels.Ping)
							{
								string text2 = "[Steamworks.NET] NET: Received package on an unknown channel from: ";
								CSteamID csteamID2 = csteamID;
								Log.Out(text2 + csteamID2.ToString());
							}
							else
							{
								this.UpdatePing(csteamID, this.recvBuf, elapsedMilliseconds);
							}
						}
						else if (connectionInformation.State == NetworkServerSteam.EConnectionState.Authenticating)
						{
							string @string = Encoding.UTF8.GetString(this.recvBuf, 0, (int)num);
							bool flag2;
							if (this.Authenticate(csteamID, @string))
							{
								flag2 = SteamGameServerNetworking.SendP2PPacket(csteamID, NetworkServerSteam.passwordValidPacket, (uint)NetworkServerSteam.passwordValidPacket.Length, 2, 0);
							}
							else
							{
								flag2 = SteamGameServerNetworking.SendP2PPacket(csteamID, NetworkServerSteam.passwordInvalidPacket, (uint)NetworkServerSteam.passwordInvalidPacket.Length, 2, 0);
							}
							if (!flag2)
							{
								string text3 = "[Steamworks.NET] NET: Could not send package to client ";
								CSteamID csteamID2 = csteamID;
								Log.Error(text3 + csteamID2.ToString());
							}
						}
					}
					else if (connectionInformation.State == NetworkServerSteam.EConnectionState.Connected)
					{
						if (num > 0U)
						{
							byte[] array = MemoryPools.poolByte.Alloc((int)num);
							Array.Copy(this.recvBuf, array, (long)((ulong)num));
							ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForUserId(connectionInformation.UserIdentifier);
							SingletonMonoBehaviour<ConnectionManager>.Instance.Net_DataReceivedServer(clientInfo, (int)esteamNetChannels, array, (int)num);
						}
					}
					else
					{
						string text4 = "[Steamworks.NET] NET: Received package from an unauthenticated client: ";
						CSteamID csteamID2 = csteamID;
						Log.Out(text4 + csteamID2.ToString());
					}
				}
				flag = SteamGameServerNetworking.ReadP2PPacket(this.recvBuf, (uint)this.recvBuf.Length, ref num, ref csteamID, 0);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool Authenticate(CSteamID _id, string _password)
		{
			bool flag = string.IsNullOrEmpty(this.serverPassword) || _password == this.serverPassword;
			Log.Out(string.Concat(new string[]
			{
				"[Steamworks.NET] NET: Received authentication package from ",
				_id.ToString(),
				": ",
				flag ? "valid" : "invalid",
				" password"
			}));
			if (!flag)
			{
				this.connections[_id].State = NetworkServerSteam.EConnectionState.Authenticating;
				return false;
			}
			this.connections[_id].State = NetworkServerSteam.EConnectionState.Connected;
			this.OnPlayerConnected(_id);
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnPlayerConnected(CSteamID _id)
		{
			ClientInfo clientInfo = new ClientInfo
			{
				PlatformId = new UserIdentifierSteam(_id),
				network = this,
				netConnection = new INetConnection[2]
			};
			for (int i = 0; i < 2; i++)
			{
				clientInfo.netConnection[i] = new NetConnectionSteam(i, clientInfo, null, clientInfo.InternalId.CombinedString);
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.AddClient(clientInfo);
			SingletonMonoBehaviour<ConnectionManager>.Instance.Net_PlayerConnected(clientInfo);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void FlushBuffer(CSteamID _id)
		{
			if (!SteamGameServerNetworking.SendP2PPacket(_id, NetworkServerSteam.timeData, (uint)NetworkServerSteam.timeData.Length, 2, 0))
			{
				string text = "[Steamworks.NET] NET: Could not flush the buffer to client ";
				CSteamID csteamID = _id;
				Log.Error(text + csteamID.ToString());
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void UpdatePing(CSteamID _sourceId, byte[] _data, long _curTime)
		{
			long num = BitConverter.ToInt64(_data, 0);
			int num2 = (int)(_curTime - num);
			NetworkServerSteam.ConnectionInformation connectionInformation = this.connections[_sourceId];
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

		public int GetMaximumPacketSize(ClientInfo _cInfo, bool reliable = false)
		{
			return 1200;
		}

		public int GetBadPacketCount(ClientInfo _cInfo)
		{
			return 0;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly IProtocolManagerProtocolInterface protoManager;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int PingCount = 50;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly MicroStopwatch mswPing = new MicroStopwatch();

		[PublicizedFrom(EAccessModifier.Private)]
		public ThreadManager.ThreadInfo handlerThread;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly AutoResetEvent signalThread = new AutoResetEvent(false);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BlockingQueue<NetworkCommonSteam.SendInfo> sendBufs = new BlockingQueue<NetworkCommonSteam.SendInfo>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BlockingQueue<NetworkCommonSteam.SendInfo> sendBufsUnreliable = new BlockingQueue<NetworkCommonSteam.SendInfo>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BlockingQueue<CSteamID> acceptQueue = new BlockingQueue<CSteamID>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BlockingQueue<CSteamID> dropQueue = new BlockingQueue<CSteamID>();

		[PublicizedFrom(EAccessModifier.Private)]
		public volatile bool flushBuffers;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BlockingQueue<CSteamID> disconnectQueue = new BlockingQueue<CSteamID>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<CSteamID, NetworkServerSteam.ConnectionInformation> connections = new Dictionary<CSteamID, NetworkServerSteam.ConnectionInformation>();

		[PublicizedFrom(EAccessModifier.Private)]
		public Callback<P2PSessionRequest_t> m_P2PSessionRequest;

		[PublicizedFrom(EAccessModifier.Private)]
		public string serverPassword;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<CSteamID, NetworkServerSteam.ConnectionInformation> checkConnections = new Dictionary<CSteamID, NetworkServerSteam.ConnectionInformation>();

		[PublicizedFrom(EAccessModifier.Private)]
		public int checkPerFrame;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] passwordValidPacket = new byte[] { 1, 50 };

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] passwordInvalidPacket = new byte[] { 0, 50 };

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly byte[] recvBuf = new byte[1048576];

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] timeData = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 60 };

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
			public NetworkServerSteam.EConnectionState State;

			public uint Ip;

			public bool PacketsPendingSend;

			public UserIdentifierSteam UserIdentifier;

			public int LastPingIndex = -1;

			public readonly int[] Pings = new int[50];
		}
	}
}
