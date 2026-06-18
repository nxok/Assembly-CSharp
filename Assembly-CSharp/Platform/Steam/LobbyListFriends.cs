using System;
using System.Collections.Generic;
using Steamworks;

namespace Platform.Steam
{
	public class LobbyListFriends : LobbyListAbs
	{
		public override void Init(IPlatform _owner)
		{
			this.owner = _owner;
			_owner.Api.ClientApiInitialized += delegate
			{
				if (this.m_lobbyDataUpdate == null && !GameManager.IsDedicatedServer)
				{
					this.m_lobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(new Callback<LobbyDataUpdate_t>.DispatchDelegate(this.Lobby_DataUpdate));
				}
			};
		}

		public override void StopSearch()
		{
			this.currentFriend = -1;
			this.isRefreshing = false;
		}

		public override void StartSearch(IList<IServerListInterface.ServerFilter> _activeFilters)
		{
			if (this.gameServerFoundCallback == null)
			{
				return;
			}
			this.isRefreshing = true;
			this.currentFriend = 0;
			this.queryNextFriend();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void queryNextFriend()
		{
			while (this.currentFriend < SteamFriends.GetFriendCount(65535))
			{
				FriendGameInfo_t friendGameInfo_t;
				if (SteamFriends.GetFriendGamePlayed(SteamFriends.GetFriendByIndex(this.currentFriend, 65535), ref friendGameInfo_t) && friendGameInfo_t.m_steamIDLobby != CSteamID.Nil)
				{
					SteamMatchmaking.RequestLobbyData(friendGameInfo_t.m_steamIDLobby);
					return;
				}
				this.currentFriend++;
			}
			ThreadManager.StartCoroutine(base.restartRefreshCo(2f));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Lobby_DataUpdate(LobbyDataUpdate_t _val)
		{
			CSteamID csteamID;
			csteamID..ctor(_val.m_ulSteamIDLobby);
			if (_val.m_bSuccess == 0)
			{
				return;
			}
			base.ParseLobbyData(csteamID, EServerRelationType.Friends);
			if (this.currentFriend < 0)
			{
				return;
			}
			this.currentFriend++;
			this.queryNextFriend();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Callback<LobbyDataUpdate_t> m_lobbyDataUpdate;

		[PublicizedFrom(EAccessModifier.Private)]
		public int currentFriend;
	}
}
