using System;
using System.Collections.Generic;
using Steamworks;

namespace Platform.Steam
{
	public class LobbyListInternet : LobbyListAbs
	{
		public override void Init(IPlatform _owner)
		{
			this.owner = _owner;
			_owner.Api.ClientApiInitialized += delegate
			{
				if (this.m_RequestLobbies == null && !GameManager.IsDedicatedServer)
				{
					this.m_RequestLobbies = CallResult<LobbyMatchList_t>.Create(new CallResult<LobbyMatchList_t>.APIDispatchDelegate(this.RequestLobbies_CallResult));
				}
			};
		}

		public override void StopSearch()
		{
			if (this.m_RequestLobbies != null && this.m_RequestLobbies.IsActive())
			{
				this.m_RequestLobbies.Cancel();
			}
			this.isRefreshing = false;
		}

		public override void StartSearch(IList<IServerListInterface.ServerFilter> _activeFilters)
		{
			if (this.gameServerFoundCallback == null)
			{
				return;
			}
			SteamMatchmaking.AddRequestLobbyListStringFilter("CompatibilityVersion", Constants.cVersionInformation.LongStringNoBuild, 0);
			SteamAPICall_t steamAPICall_t = SteamMatchmaking.RequestLobbyList();
			this.m_RequestLobbies.Set(steamAPICall_t, null);
			this.isRefreshing = true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void RequestLobbies_CallResult(LobbyMatchList_t _val, bool _ioFailure)
		{
			if (_ioFailure)
			{
				Log.Out("[Steamworks.NET] RequestLobbies failed");
			}
			else
			{
				int num = 0;
				while ((long)num < (long)((ulong)_val.m_nLobbiesMatching))
				{
					base.ParseLobbyData(SteamMatchmaking.GetLobbyByIndex(num), EServerRelationType.Internet);
					num++;
				}
			}
			ThreadManager.StartCoroutine(base.restartRefreshCo(3f));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public CallResult<LobbyMatchList_t> m_RequestLobbies;
	}
}
