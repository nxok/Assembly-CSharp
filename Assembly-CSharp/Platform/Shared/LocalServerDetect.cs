using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platform.Shared
{
	public class LocalServerDetect : IServerListInterface
	{
		public LocalServerDetect()
		{
			if (GameManager.IsDedicatedServer)
			{
				return;
			}
			Application.wantsToQuit += delegate
			{
				this.Disconnect();
				return true;
			};
		}

		public bool IsPrefiltered
		{
			get
			{
				return false;
			}
		}

		public void Init(IPlatform _owner)
		{
			if (GameManager.IsDedicatedServer || this.initDone)
			{
				return;
			}
			this.owner = _owner;
		}

		public void RegisterGameServerFoundCallback(GameServerFoundCallback _serverFound, MaxResultsReachedCallback _maxResultsCallback, ServerSearchErrorCallback _errorCallback)
		{
			this.gameServerFoundCallback = _serverFound;
		}

		public bool IsRefreshing
		{
			get
			{
				return this.isRefreshing;
			}
		}

		public void StartSearch(IList<IServerListInterface.ServerFilter> _activeFilters)
		{
			this.isRefreshing = true;
			if (this.detectCoroutine == null)
			{
				this.detectCoroutine = ThreadManager.StartCoroutine(this.detectLocalServers());
			}
		}

		public void StopSearch()
		{
			this.isRefreshing = false;
			this.detectCoroutine = null;
		}

		public void Disconnect()
		{
			this.isRefreshing = false;
		}

		public void GetSingleServerDetails(GameServerInfo _serverInfo, EServerRelationType _relation, GameServerFoundCallback _callback)
		{
			throw new NotImplementedException();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator detectLocalServers()
		{
			while (this.isRefreshing)
			{
				GameServerInfo gameServerInfo = new GameServerInfo();
				gameServerInfo.SetValue(GameInfoString.IP, "127.0.0.1");
				gameServerInfo.SetValue(GameInfoInt.Port, 26900);
				ServerInformationTcpClient.RequestRules(gameServerInfo, true, new ServerInformationTcpClient.RulesRequestDone(this.callback));
				yield return LocalServerDetect.refreshInterval;
				GameServerInfo gameServerInfo2 = new GameServerInfo();
				gameServerInfo2.SetValue(GameInfoString.IP, "127.0.0.1");
				gameServerInfo2.SetValue(GameInfoInt.Port, 27020);
				ServerInformationTcpClient.RequestRules(gameServerInfo2, true, new ServerInformationTcpClient.RulesRequestDone(this.callback));
				yield return LocalServerDetect.refreshInterval;
			}
			this.detectCoroutine = null;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void callback(bool _success, string _message, GameServerInfo _gsi)
		{
			if (!this.isRefreshing || !_success)
			{
				return;
			}
			_gsi.IsLAN = true;
			GameServerFoundCallback gameServerFoundCallback = this.gameServerFoundCallback;
			if (gameServerFoundCallback == null)
			{
				return;
			}
			gameServerFoundCallback(this.owner, _gsi, EServerRelationType.LAN);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool initDone;

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public GameServerFoundCallback gameServerFoundCallback;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly WaitForSeconds refreshInterval = new WaitForSeconds(3f);

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isRefreshing;

		[PublicizedFrom(EAccessModifier.Private)]
		public Coroutine detectCoroutine;
	}
}
