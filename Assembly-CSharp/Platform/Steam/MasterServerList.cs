using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace Platform.Steam
{
	public class MasterServerList : IServerListInterface
	{
		public MasterServerList(EServerRelationType _source)
		{
			if (GameManager.IsDedicatedServer)
			{
				return;
			}
			Application.wantsToQuit += this.OnApplicationQuit;
			this.source = _source;
			this.compatVersionInt = int.Parse(Constants.SteamVersionNr.Replace(".", ""));
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
			this.owner = _owner;
			_owner.Api.ClientApiInitialized += delegate
			{
				if (this.matchmakingServerListResponse == null && !GameManager.IsDedicatedServer)
				{
					this.matchmakingServerListResponse = new ISteamMatchmakingServerListResponse(new ISteamMatchmakingServerListResponse.ServerResponded(this.ServerResponded), new ISteamMatchmakingServerListResponse.ServerFailedToRespond(this.ServerFailedToRespond), new ISteamMatchmakingServerListResponse.RefreshComplete(this.RefreshComplete));
				}
			};
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
			if (this.gameServerFoundCallback == null)
			{
				return;
			}
			if (this.requestHandle != HServerListRequest.Invalid)
			{
				SteamMatchmakingServers.ReleaseRequest(this.requestHandle);
				this.requestHandle = HServerListRequest.Invalid;
			}
			MatchMakingKeyValuePair_t[] array = new MatchMakingKeyValuePair_t[0];
			HServerListRequest hserverListRequest;
			switch (this.source)
			{
			case EServerRelationType.Internet:
				hserverListRequest = SteamMatchmakingServers.RequestInternetServerList((AppId_t)251570U, array, (uint)array.Length, this.matchmakingServerListResponse);
				break;
			case EServerRelationType.LAN:
				hserverListRequest = SteamMatchmakingServers.RequestLANServerList((AppId_t)251570U, this.matchmakingServerListResponse);
				break;
			case EServerRelationType.Friends:
				hserverListRequest = SteamMatchmakingServers.RequestFriendsServerList((AppId_t)251570U, array, (uint)array.Length, this.matchmakingServerListResponse);
				break;
			case EServerRelationType.Favorites:
				hserverListRequest = SteamMatchmakingServers.RequestFavoritesServerList((AppId_t)251570U, array, (uint)array.Length, this.matchmakingServerListResponse);
				break;
			case EServerRelationType.History:
				hserverListRequest = SteamMatchmakingServers.RequestHistoryServerList((AppId_t)251570U, array, (uint)array.Length, this.matchmakingServerListResponse);
				break;
			case EServerRelationType.Spectator:
				hserverListRequest = SteamMatchmakingServers.RequestSpectatorServerList((AppId_t)251570U, array, (uint)array.Length, this.matchmakingServerListResponse);
				break;
			default:
				hserverListRequest = this.requestHandle;
				break;
			}
			this.requestHandle = hserverListRequest;
			this.isRefreshing = true;
		}

		public void StopSearch()
		{
			if (this.requestHandle != HServerListRequest.Invalid)
			{
				SteamMatchmakingServers.ReleaseRequest(this.requestHandle);
				this.requestHandle = HServerListRequest.Invalid;
			}
			this.isRefreshing = false;
		}

		public void Disconnect()
		{
			this.StopSearch();
			this.gameServerFoundCallback = null;
		}

		public void GetSingleServerDetails(GameServerInfo _serverInfo, EServerRelationType _relation, GameServerFoundCallback _callback)
		{
			throw new NotImplementedException();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool OnApplicationQuit()
		{
			this.StopSearch();
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ServerResponded(HServerListRequest _hRequest, int _iServer)
		{
			gameserveritem_t serverDetails = SteamMatchmakingServers.GetServerDetails(_hRequest, _iServer);
			if (serverDetails.m_nServerVersion != this.compatVersionInt && this.source != EServerRelationType.Favorites && this.source != EServerRelationType.History && this.source != EServerRelationType.LAN)
			{
				return;
			}
			GameServerInfo gameServerInfo = new GameServerInfo();
			gameServerInfo.SetValue(GameInfoInt.Ping, serverDetails.m_nPing);
			gameServerInfo.SetValue(GameInfoString.IP, NetworkUtils.ToAddr(serverDetails.m_NetAdr.GetIP()));
			gameServerInfo.SetValue(GameInfoInt.Port, (int)serverDetails.m_NetAdr.GetQueryPort());
			gameServerInfo.SetValue(GameInfoString.SteamID, serverDetails.m_steamID.ToString());
			gameServerInfo.SetValue(GameInfoString.UniqueId, serverDetails.m_steamID.ToString());
			gameServerInfo.SetValue(GameInfoString.LevelName, serverDetails.GetMap());
			gameServerInfo.SetValue(GameInfoInt.CurrentPlayers, serverDetails.m_nPlayers);
			gameServerInfo.SetValue(GameInfoInt.MaxPlayers, serverDetails.m_nMaxPlayers);
			gameServerInfo.SetValue(GameInfoBool.IsPasswordProtected, serverDetails.m_bPassword);
			gameServerInfo.SetValue(GameInfoString.GameHost, serverDetails.GetServerName());
			gameServerInfo.LastPlayedLinux = (int)serverDetails.m_ulTimeLastPlayed;
			switch (this.source)
			{
			case EServerRelationType.LAN:
				gameServerInfo.IsLAN = true;
				break;
			case EServerRelationType.Friends:
				gameServerInfo.IsFriends = true;
				break;
			case EServerRelationType.Favorites:
				gameServerInfo.IsFavorite = true;
				break;
			}
			if (NetworkUtils.ParseGameTags(serverDetails.GetGameTags(), gameServerInfo))
			{
				GameServerFoundCallback gameServerFoundCallback = this.gameServerFoundCallback;
				if (gameServerFoundCallback == null)
				{
					return;
				}
				gameServerFoundCallback(this.owner, gameServerInfo, this.source);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ServerFailedToRespond(HServerListRequest _hRequest, int _iServer)
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void RefreshComplete(HServerListRequest _hRequest, EMatchMakingServerResponse _response)
		{
			ThreadManager.StartCoroutine(this.restartRefreshCo());
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator restartRefreshCo()
		{
			yield return new WaitForSeconds(4f);
			this.StartSearch(null);
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int compatVersionInt;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly EServerRelationType source;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isRefreshing;

		[PublicizedFrom(EAccessModifier.Private)]
		public GameServerFoundCallback gameServerFoundCallback;

		[PublicizedFrom(EAccessModifier.Private)]
		public ISteamMatchmakingServerListResponse matchmakingServerListResponse;

		[PublicizedFrom(EAccessModifier.Private)]
		public HServerListRequest requestHandle = HServerListRequest.Invalid;
	}
}
