using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Platform.LAN
{
	public class LANServerList : IServerListInterface
	{
		public bool IsPrefiltered
		{
			get
			{
				return false;
			}
		}

		public bool IsRefreshing
		{
			get
			{
				return this.shouldRefresh;
			}
		}

		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
		}

		public void RegisterGameServerFoundCallback(GameServerFoundCallback _serverFound, MaxResultsReachedCallback _maxResultsCallback, ServerSearchErrorCallback _sessionSearchErrorCallback)
		{
			this.serverFoundCallback = _serverFound;
		}

		public void GetSingleServerDetails(GameServerInfo _serverInfo, EServerRelationType _relation, GameServerFoundCallback _callback)
		{
			throw new NotImplementedException();
		}

		public void StartSearch(IList<IServerListInterface.ServerFilter> _activeFilters)
		{
			try
			{
				this.shouldRefresh = true;
				this.isPaused = false;
				this.udpClient = new UdpClient(AddressFamily.InterNetwork);
				this.sendHandler = new UdpClientSendHandler(this.udpClient);
				this.receiveHandler = new UdpClientReceiveHandler(this.udpClient);
				this.requestCoroutine = ThreadManager.StartCoroutine(this.LANServerInfoRequestCoroutine());
				this.receiveCoroutine = ThreadManager.StartCoroutine(this.LANServerInfoReceiveCoroutine());
			}
			catch (Exception ex)
			{
				Log.Error("[LANServerList] Could not start LAN server search");
				Log.Exception(ex);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator LANServerInfoRequestCoroutine()
		{
			while (this.shouldRefresh)
			{
				IPEndPoint ipendPoint = new IPEndPoint(LANServerSearchConfig.MulticastGroupIp, 11000);
				if (!this.sendHandler.BeginSend(LANServerList.emptyMessage, 0, ipendPoint))
				{
					yield break;
				}
				while (!this.sendHandler.isComplete)
				{
					yield return null;
				}
				yield return new WaitForSeconds(5f);
				while (this.isPaused)
				{
					yield return null;
				}
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator LANServerInfoReceiveCoroutine()
		{
			while (this.shouldRefresh)
			{
				if (!this.receiveHandler.BeginReceive())
				{
					yield break;
				}
				while (!this.receiveHandler.isComplete)
				{
					yield return null;
				}
				while (this.isPaused)
				{
					yield return null;
				}
				IPEndPoint remoteEP = this.receiveHandler.remoteEP;
				byte[] message = this.receiveHandler.message;
				int length = this.receiveHandler.length;
				if (remoteEP != null && message != null && length == 4)
				{
					int num = 0;
					int num2 = StreamUtils.ReadInt32(message, ref num);
					IPEndPoint ipendPoint = remoteEP;
					ipendPoint.Port = num2;
					this.OnServerFound(ipendPoint);
				}
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnServerFound(IPEndPoint endpoint)
		{
			string text = endpoint.Address.ToString();
			if (!this.cacheControl.IsUpdateRequired(text, endpoint.Port))
			{
				return;
			}
			GameServerInfo gameServerInfo = new GameServerInfo();
			gameServerInfo.SetValue(GameInfoString.IP, endpoint.Address.ToString());
			gameServerInfo.SetValue(GameInfoInt.Port, endpoint.Port);
			ServerInformationTcpClient.RequestRules(gameServerInfo, false, new ServerInformationTcpClient.RulesRequestDone(this.OnRulesRequestDone));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnRulesRequestDone(bool _success, string _message, GameServerInfo _gsi)
		{
			this.cacheControl.SetUpdated(_gsi.GetValue(GameInfoString.IP), _gsi.GetValue(GameInfoInt.Port));
			_gsi.IsLAN = true;
			this.serverFoundCallback(this.owner, _gsi, EServerRelationType.LAN);
		}

		public void StopSearch()
		{
			this.isPaused = true;
			this.cacheControl.Clear();
		}

		public void Disconnect()
		{
			this.StopSearch();
			this.isPaused = false;
			this.shouldRefresh = false;
			if (this.requestCoroutine != null)
			{
				ThreadManager.StopCoroutine(this.requestCoroutine);
			}
			if (this.receiveCoroutine != null)
			{
				ThreadManager.StopCoroutine(this.receiveCoroutine);
			}
			UdpClient udpClient = this.udpClient;
			if (udpClient != null)
			{
				udpClient.Dispose();
			}
			this.udpClient = null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int serverBroadcastIntervalSeconds = 5;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly TimeSpan rulesRefreshInterval = new TimeSpan(0, 2, 0);

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly TimeSpan knownServerTimeout = new TimeSpan(0, 1, 0);

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public GameServerFoundCallback serverFoundCallback;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly IPAddress multicastGroupIp;

		[PublicizedFrom(EAccessModifier.Private)]
		public UdpClient udpClient;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool shouldRefresh;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isPaused;

		[PublicizedFrom(EAccessModifier.Private)]
		public Coroutine requestCoroutine;

		[PublicizedFrom(EAccessModifier.Private)]
		public Coroutine receiveCoroutine;

		[PublicizedFrom(EAccessModifier.Private)]
		public UdpClientSendHandler sendHandler;

		[PublicizedFrom(EAccessModifier.Private)]
		public UdpClientReceiveHandler receiveHandler;

		[PublicizedFrom(EAccessModifier.Private)]
		public LANServerCacheControl cacheControl = new LANServerCacheControl(LANServerList.rulesRefreshInterval, LANServerList.knownServerTimeout);

		[PublicizedFrom(EAccessModifier.Private)]
		public static byte[] emptyMessage = new byte[0];
	}
}
