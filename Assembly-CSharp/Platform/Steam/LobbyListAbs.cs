using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace Platform.Steam
{
	public abstract class LobbyListAbs : IServerListInterface
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public LobbyListAbs()
		{
			if (GameManager.IsDedicatedServer)
			{
				return;
			}
			Application.wantsToQuit += this.OnApplicationQuit;
		}

		public bool IsPrefiltered
		{
			get
			{
				return false;
			}
		}

		public abstract void Init(IPlatform _owner);

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

		public abstract void StartSearch(IList<IServerListInterface.ServerFilter> _activeFilters);

		public abstract void StopSearch();

		public virtual void Disconnect()
		{
			this.StopSearch();
			this.gameServerFoundCallback = null;
		}

		public void GetSingleServerDetails(GameServerInfo _serverInfo, EServerRelationType _relation, GameServerFoundCallback _callback)
		{
			throw new NotImplementedException();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual bool OnApplicationQuit()
		{
			this.Disconnect();
			return true;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public IEnumerator restartRefreshCo(float _delay)
		{
			yield return new WaitForSeconds(_delay);
			this.StartSearch(null);
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void ParseLobbyData(CSteamID _lobbyId, EServerRelationType _source)
		{
			if (this.gameServerFoundCallback == null)
			{
				return;
			}
			GameServerInfo gameServerInfo = new GameServerInfo
			{
				IsLobby = true
			};
			int lobbyDataCount = SteamMatchmaking.GetLobbyDataCount(_lobbyId);
			for (int i = 0; i < lobbyDataCount; i++)
			{
				string text;
				string text2;
				if (SteamMatchmaking.GetLobbyDataByIndex(_lobbyId, i, ref text, 100, ref text2, 200))
				{
					gameServerInfo.ParseAny(text, text2);
				}
			}
			if (PlatformManager.CrossplatformPlatform == null)
			{
				gameServerInfo.SetValue(GameInfoString.UniqueId, gameServerInfo.GetValue(GameInfoString.SteamID));
			}
			gameServerInfo.IsFriends = _source == EServerRelationType.Friends;
			this.gameServerFoundCallback(this.owner, gameServerInfo, _source);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Protected)]
		public GameServerFoundCallback gameServerFoundCallback;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool isRefreshing;
	}
}
