using System;
using System.Collections;
using System.Text;
using System.Threading;
using Steamworks;
using UnityEngine.Networking;

namespace Platform.Steam
{
	public class NetworkClientSteam : IPlatformNetworkClient, INetworkClient
	{
		public bool IsConnected
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return this.serverId != CSteamID.Nil && this.handlerThread != null && (this.protoManager.IsClient || this.connecting) && this.owner.User.UserStatus == EUserStatus.LoggedIn;
			}
		}

		public NetworkClientSteam(IPlatform _owner, IProtocolManagerProtocolInterface _protoManager)
		{
			this.owner = _owner;
			this.protoManager = _protoManager;
			this.owner.Api.ClientApiInitialized += delegate
			{
				if (!GameManager.IsDedicatedServer)
				{
					this.m_P2PSessionConnectFail = Callback<P2PSessionConnectFail_t>.Create(new Callback<P2PSessionConnectFail_t>.DispatchDelegate(this.P2PSessionConnectFail));
				}
			};
		}

		public void Connect(GameServerInfo _gsi)
		{
			this.disconnectEventReceived = false;
			this.anyPacketsSent = false;
			Log.Out("NET: Steam NW trying to connect to: " + _gsi.GetValue(GameInfoString.IP) + ":" + _gsi.GetValue(GameInfoInt.Port).ToString());
			if (string.IsNullOrEmpty(_gsi.GetValue(GameInfoString.SteamID)))
			{
				Log.Out("[Steamworks.NET] NET: Resolving SteamID for IP " + _gsi.GetValue(GameInfoString.IP) + ":" + _gsi.GetValue(GameInfoInt.Port).ToString());
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
			if (string.IsNullOrEmpty(_gsi.GetValue(GameInfoString.SteamID)))
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
				Encoding.UTF8.GetBytes(password, 0, password.Length, arrayListMP.Items, 0);
			}
			else
			{
				arrayListMP = new ArrayListMP<byte>(MemoryPools.poolByte, 1)
				{
					Count = 1
				};
			}
			this.serverId = new CSteamID(ulong.Parse(_gsi.GetValue(GameInfoString.SteamID)));
			string text = "[Steamworks.NET] NET: Connecting to SteamID ";
			CSteamID csteamID = this.serverId;
			Log.Out(text + csteamID.ToString());
			if (this.handlerThread == null)
			{
				this.handlerThread = ThreadManager.StartThread("SteamNetworkingClient", new ThreadManager.ThreadFunctionDelegate(this.threadHandlerMethod), null, null, true, false);
			}
			this.connecting = true;
			this.SendData(50, arrayListMP);
		}

		public void Disconnect()
		{
			this.connecting = false;
			if (this.serverId != CSteamID.Nil)
			{
				this.sendBufs.Clear();
			}
			if (this.handlerThread == null)
			{
				return;
			}
			this.signalThread.Set();
			this.handlerThread.WaitForEnd(30);
			this.handlerThread = null;
			this.serverId = CSteamID.Nil;
		}

		public NetworkError SendData(int _channel, ArrayListMP<byte> _data)
		{
			if (this.IsConnected)
			{
				CSteamID csteamID = this.serverId;
				_data[_data.Count - 1] = (byte)_channel;
				this.sendBufs.Enqueue(new NetworkCommonSteam.SendInfo(csteamID, _data));
				this.signalThread.Set();
			}
			else
			{
				Log.Warning("[Steamworks.NET] NET: Tried to send a package while not connected to a server");
			}
			return NetworkError.Ok;
		}

		public void Update()
		{
			if (!this.IsConnected)
			{
				return;
			}
			if (this.disconnectEventReceived)
			{
				this.OnDisconnectedFromServer();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator connectionFailedLater(string _message)
		{
			yield return null;
			yield return null;
			this.protoManager.ConnectionFailedEv(_message);
			yield break;
		}

		public void LateUpdate()
		{
			this.flushBuffers = true;
			this.signalThread.Set();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnDisconnectedFromServer()
		{
			this.Disconnect();
			this.protoManager.DisconnectedFromServerEv(Localization.Get("netSteamNetworking_ConnectionClosedByServer", false, null));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void P2PSessionConnectFail(P2PSessionConnectFail_t _par)
		{
			if (!this.connecting)
			{
				return;
			}
			this.Disconnect();
			Log.Out("[Steamworks.NET] NET: P2PSessionConnectFail to: " + _par.m_steamIDRemote.m_SteamID.ToString() + " - Error: " + _par.m_eP2PSessionError.ToStringCached<EP2PSessionError>());
			string text = Localization.Get("netSteamNetworkingSessionError_" + _par.m_eP2PSessionError.ToStringCached<EP2PSessionError>(), false, null);
			this.protoManager.ConnectionFailedEv(text);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void threadHandlerMethod(ThreadManager.ThreadInfo _threadinfo)
		{
			while (!_threadinfo.TerminationRequested())
			{
				if (!this.IsConnected)
				{
					this.signalThread.WaitOne(100);
				}
				else
				{
					this.signalThread.WaitOne(6);
					if (this.anyPacketsSent)
					{
						this.CheckConnection();
					}
					this.ReceivePackets();
					while (this.sendBufs.HasData())
					{
						NetworkCommonSteam.SendInfo sendInfo = this.sendBufs.Dequeue();
						if (!SteamNetworking.SendP2PPacket(sendInfo.Recipient, sendInfo.Data.Items, (uint)sendInfo.Data.Count, 3, 0))
						{
							Log.Error("[Steamworks.NET] NET: Could not send package to server");
						}
						else
						{
							this.packetsPendingSend = true;
							this.anyPacketsSent = true;
						}
					}
					if (this.flushBuffers && this.packetsPendingSend)
					{
						this.packetsPendingSend = false;
						this.flushBuffers = false;
						this.FlushBuffer();
					}
				}
			}
			if (this.serverId != CSteamID.Nil)
			{
				SteamNetworking.CloseP2PSessionWithUser(this.serverId);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CheckConnection()
		{
			P2PSessionState_t p2PSessionState_t;
			if (SteamNetworking.GetP2PSessionState(this.serverId, ref p2PSessionState_t))
			{
				if (p2PSessionState_t.m_bConnectionActive != 0 && this.connecting)
				{
					this.connecting = false;
					Log.Out("[Steamworks.NET] NET: Connection established");
					return;
				}
				if (p2PSessionState_t.m_bConnecting == 0 && p2PSessionState_t.m_bConnectionActive == 0 && this.protoManager.IsClient)
				{
					Log.Out("[Steamworks.NET] NET: Connection closed");
					this.disconnectEventReceived = true;
					return;
				}
			}
			else if (this.protoManager.IsClient)
			{
				Log.Out("[Steamworks.NET] NET: Connection closed");
				this.disconnectEventReceived = true;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ReceivePackets()
		{
			uint num;
			CSteamID csteamID;
			bool flag = SteamNetworking.ReadP2PPacket(this.recvBuf, (uint)this.recvBuf.Length, ref num, ref csteamID, 0);
			while (flag)
			{
				if (num > 0U)
				{
					num -= 1U;
					NetworkCommonSteam.ESteamNetChannels esteamNetChannels = (NetworkCommonSteam.ESteamNetChannels)this.recvBuf[(int)num];
					if (esteamNetChannels > NetworkCommonSteam.ESteamNetChannels.NetpackageChannel1)
					{
						if (esteamNetChannels != NetworkCommonSteam.ESteamNetChannels.Authentication)
						{
							if (esteamNetChannels == NetworkCommonSteam.ESteamNetChannels.Ping)
							{
								ArrayListMP<byte> arrayListMP = new ArrayListMP<byte>(MemoryPools.poolByte, (int)(num + 1U));
								Array.Copy(this.recvBuf, arrayListMP.Items, (long)((ulong)num));
								arrayListMP.Count = (int)(num + 1U);
								this.SendData(60, arrayListMP);
							}
						}
						else
						{
							if (this.connecting)
							{
								this.connecting = false;
								Log.Out("[Steamworks.NET] NET: Connection established");
							}
							if (this.recvBuf[0] == 0)
							{
								Log.Out("[Steamworks.NET] NET: Received invalid password package");
								ThreadManager.AddSingleTaskMainThread("SteamNetInvalidPassword", delegate(object _info)
								{
									this.protoManager.InvalidPasswordEv();
								}, null);
							}
							else
							{
								Log.Out("[Steamworks.NET] NET: Password accepted");
								this.OnConnectedToServer();
							}
						}
					}
					else if (num > 0U)
					{
						byte[] array = MemoryPools.poolByte.Alloc((int)num);
						Array.Copy(this.recvBuf, array, (long)((ulong)num));
						SingletonMonoBehaviour<ConnectionManager>.Instance.Net_DataReceivedClient((int)esteamNetChannels, array, (int)num);
					}
				}
				flag = SteamNetworking.ReadP2PPacket(this.recvBuf, (uint)this.recvBuf.Length, ref num, ref csteamID, 0);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void FlushBuffer()
		{
			if (!SteamNetworking.SendP2PPacket(this.serverId, NetworkClientSteam.emptyData, 0U, 2, 0))
			{
				Log.Error("[Steamworks.NET] NET: Could not flush the data buffer");
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnConnectedToServer()
		{
			INetConnection[] array = new INetConnection[2];
			for (int i = 0; i < 2; i++)
			{
				array[i] = new NetConnectionSteam(i, null, this, null);
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
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly IProtocolManagerProtocolInterface protoManager;

		[PublicizedFrom(EAccessModifier.Private)]
		public Callback<P2PSessionConnectFail_t> m_P2PSessionConnectFail;

		[PublicizedFrom(EAccessModifier.Private)]
		public ThreadManager.ThreadInfo handlerThread;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly AutoResetEvent signalThread = new AutoResetEvent(false);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BlockingQueue<NetworkCommonSteam.SendInfo> sendBufs = new BlockingQueue<NetworkCommonSteam.SendInfo>();

		[PublicizedFrom(EAccessModifier.Private)]
		public volatile bool flushBuffers;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool packetsPendingSend;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool anyPacketsSent;

		[PublicizedFrom(EAccessModifier.Private)]
		public CSteamID serverId = CSteamID.Nil;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool connecting;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool disconnectEventReceived;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly byte[] recvBuf = new byte[1048576];

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] emptyData = new byte[0];
	}
}
