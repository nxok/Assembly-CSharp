using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Platform.LAN
{
	public class LANMasterServerAnnouncer : IMasterServerAnnouncer
	{
		public bool GameServerInitialized
		{
			get
			{
				return true;
			}
		}

		public void Init(IPlatform _owner)
		{
		}

		public string GetServerPorts()
		{
			return string.Empty;
		}

		public void AdvertiseServer(Action _onServerRegistered)
		{
			int num = 11000;
			IPAddress multicastGroupIp = LANServerSearchConfig.MulticastGroupIp;
			try
			{
				this.udpClient = new UdpClient(num);
				this.udpClient.JoinMulticastGroup(multicastGroupIp);
				this.receiveHandler = new UdpClientReceiveHandler(this.udpClient);
				this.shouldAnnounce = true;
				this.replyCoroutine = ThreadManager.StartCoroutine(this.LANServerListReplyTask());
				Log.Out(string.Format("[{0}] listening on {1} and multicast group {2}", "LANMasterServerAnnouncer", num, multicastGroupIp));
			}
			catch (SocketException ex)
			{
				Log.Warning(string.Format("[{0}] could not start LAN server search listening on port {1} and multicast group {2}. ErrorCode: {3}, Message: {4}", new object[] { "LANMasterServerAnnouncer", num, multicastGroupIp, ex.ErrorCode, ex.Message }));
			}
			catch (Exception ex2)
			{
				Log.Exception(ex2);
			}
			_onServerRegistered();
		}

		public IEnumerator LANServerListReplyTask()
		{
			while (this.shouldAnnounce)
			{
				if (!this.receiveHandler.BeginReceive())
				{
					Log.Error("[LANMasterServerAnnouncer] could not start receive");
					yield break;
				}
				while (!this.receiveHandler.isComplete)
				{
					yield return null;
				}
				IPEndPoint remoteEP = this.receiveHandler.remoteEP;
				byte[] message = this.receiveHandler.message;
				int length = this.receiveHandler.length;
				if (remoteEP != null && message != null && length == 0)
				{
					this.SendReply(remoteEP);
				}
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SendReply(IPEndPoint _remoteEP)
		{
			try
			{
				int value = SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.GetValue(GameInfoInt.Port);
				int num = 0;
				StreamUtils.Write(LANMasterServerAnnouncer.replyBuffer, value, ref num);
				this.udpClient.Client.SendTo(LANMasterServerAnnouncer.replyBuffer, 0, num, SocketFlags.None, _remoteEP);
			}
			catch (Exception ex)
			{
				Log.Error(string.Format("[{0}] could not send reply to {1}", "LANMasterServerAnnouncer", _remoteEP));
				Log.Exception(ex);
			}
		}

		public void Update()
		{
		}

		public void StopServer()
		{
			this.shouldAnnounce = false;
			if (this.replyCoroutine != null)
			{
				ThreadManager.StopCoroutine(this.replyCoroutine);
			}
			UdpClient udpClient = this.udpClient;
			if (udpClient != null)
			{
				udpClient.Dispose();
			}
			this.udpClient = null;
			this.receiveHandler = null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool shouldAnnounce = true;

		[PublicizedFrom(EAccessModifier.Private)]
		public UdpClient udpClient;

		[PublicizedFrom(EAccessModifier.Private)]
		public UdpClientReceiveHandler receiveHandler;

		[PublicizedFrom(EAccessModifier.Private)]
		public Coroutine replyCoroutine;

		[PublicizedFrom(EAccessModifier.Private)]
		public static byte[] emptyMessage = new byte[0];

		[PublicizedFrom(EAccessModifier.Private)]
		public static byte[] replyBuffer = new byte[4];
	}
}
