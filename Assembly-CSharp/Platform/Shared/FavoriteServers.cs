using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platform.Shared
{
	public class FavoriteServers : IServerListInterface
	{
		public FavoriteServers()
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
				this.detectCoroutine = ThreadManager.StartCoroutine(this.detectFavoriteServers());
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
		public IEnumerator detectFavoriteServers()
		{
			while (this.isRefreshing)
			{
				Dictionary<ServerInfoCache.FavoritesHistoryKey, ServerInfoCache.FavoritesHistoryValue>.Enumerator dictEnumerator = ServerInfoCache.Instance.GetFavoriteServersEnumerator();
				bool flag = dictEnumerator.MoveNext();
				while (flag && this.isRefreshing)
				{
					KeyValuePair<ServerInfoCache.FavoritesHistoryKey, ServerInfoCache.FavoritesHistoryValue> keyValuePair = dictEnumerator.Current;
					GameServerInfo gameServerInfo = new GameServerInfo();
					gameServerInfo.SetValue(GameInfoString.IP, keyValuePair.Key.Address);
					gameServerInfo.SetValue(GameInfoInt.Port, keyValuePair.Key.Port);
					gameServerInfo.IsFavorite = keyValuePair.Value.IsFavorite;
					gameServerInfo.LastPlayedLinux = (int)keyValuePair.Value.LastPlayedTime;
					ServerInformationTcpClient.RequestRules(gameServerInfo, true, new ServerInformationTcpClient.RulesRequestDone(this.callback));
					yield return FavoriteServers.serverCheckInterval;
					try
					{
						flag = dictEnumerator.MoveNext();
					}
					catch (InvalidOperationException)
					{
						flag = false;
					}
				}
				dictEnumerator.Dispose();
				yield return FavoriteServers.refreshInterval;
				dictEnumerator = default(Dictionary<ServerInfoCache.FavoritesHistoryKey, ServerInfoCache.FavoritesHistoryValue>.Enumerator);
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
			GameServerFoundCallback gameServerFoundCallback = this.gameServerFoundCallback;
			if (gameServerFoundCallback == null)
			{
				return;
			}
			gameServerFoundCallback(this.owner, _gsi, _gsi.IsFavorite ? EServerRelationType.Favorites : EServerRelationType.History);
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
		public static readonly WaitForSeconds serverCheckInterval = new WaitForSeconds(0.1f);

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isRefreshing;

		[PublicizedFrom(EAccessModifier.Private)]
		public Coroutine detectCoroutine;
	}
}
