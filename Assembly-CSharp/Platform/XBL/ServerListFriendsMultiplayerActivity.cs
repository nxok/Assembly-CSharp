using System;
using System.Collections.Generic;
using System.Threading;
using Unity.XGamingRuntime;

namespace Platform.XBL
{
	public class ServerListFriendsMultiplayerActivity : IServerListInterface
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
				return this.isLoadingFriendsList || this.activitySearchCount > 0 || this.sessionSearchCount > 0;
			}
		}

		public void Init(IPlatform _owner)
		{
			this.user = (User)_owner.User;
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			this.serverLookup = ((crossplatformPlatform != null) ? crossplatformPlatform.ServerLookupInterface : null);
			if (this.serverLookup == null)
			{
				Log.Error("[XBL] no crossplatform server lookup interface provided, friends session search is not possible");
				return;
			}
		}

		public void RegisterGameServerFoundCallback(GameServerFoundCallback _serverFound, MaxResultsReachedCallback _maxResultsCallback, ServerSearchErrorCallback _sessionSearchErrorCallback)
		{
			if (this.serverLookup == null)
			{
				return;
			}
			this.gameServerFoundCallback = _serverFound;
		}

		public void StartSearch(IList<IServerListInterface.ServerFilter> _activeFilters)
		{
			if (this.serverLookup == null)
			{
				return;
			}
			XblSocialManagerUserGroupHandle xblSocialManagerUserGroupHandle;
			if (!this.user.SocialManager.TryCreateUserGroup(1, 1, new SocialManagerXbl.UserGroupMembersChanged(this.OnlineFriendsListUpdated), out xblSocialManagerUserGroupHandle))
			{
				Log.Error("[XBL] could not create friends user group, friends session search will fail");
				return;
			}
			Log.Out("[XBL] ServerListFriendsMultiplayerActivity starting search");
			this.userGroupHandle = xblSocialManagerUserGroupHandle;
			this.isLoadingFriendsList = true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnlineFriendsListUpdated(ulong[] _users)
		{
			this.isLoadingFriendsList = false;
			if (_users == null || _users.Length == 0)
			{
				return;
			}
			Interlocked.Increment(ref this.activitySearchCount);
			this.user.MultiplayerActivityQueryManager.GetActivityAsync(_users, new MultiplayerActivityQueryManager.OnGetActivityComplete(this.OnActivitiesRetrieved));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnActivitiesRetrieved(ulong[] _searchedXuids, List<XblMultiplayerActivityInfo> _results)
		{
			int num = 0;
			using (List<XblMultiplayerActivityInfo>.Enumerator enumerator = _results.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!string.IsNullOrEmpty(enumerator.Current.ConnectionString))
					{
						num++;
					}
				}
			}
			if (num == 0 || this.gameServerFoundCallback == null)
			{
				Interlocked.Decrement(ref this.activitySearchCount);
				return;
			}
			ThreadManager.AddSingleTaskMainThread("SearchXboxActivitySessions", delegate(object param)
			{
				foreach (XblMultiplayerActivityInfo xblMultiplayerActivityInfo in _results)
				{
					if (xblMultiplayerActivityInfo.JoinRestriction != 1 && !string.IsNullOrEmpty(xblMultiplayerActivityInfo.ConnectionString))
					{
						GameServerInfo gameServerInfo = new GameServerInfo();
						gameServerInfo.SetValue(GameInfoString.UniqueId, xblMultiplayerActivityInfo.ConnectionString);
						Interlocked.Increment(ref this.sessionSearchCount);
						this.serverLookup.GetSingleServerDetails(gameServerInfo, EServerRelationType.Friends, new GameServerFoundCallback(this.OnServerFound));
					}
				}
				Interlocked.Decrement(ref this.activitySearchCount);
			}, null);
		}

		public void OnServerFound(IPlatform _sourcePlatform, GameServerInfo _info, EServerRelationType _source)
		{
			if (_info == null)
			{
				return;
			}
			_info.IsFriends = true;
			GameServerFoundCallback gameServerFoundCallback = this.gameServerFoundCallback;
			if (gameServerFoundCallback != null)
			{
				gameServerFoundCallback(_sourcePlatform, _info, _source);
			}
			Interlocked.Decrement(ref this.sessionSearchCount);
		}

		public void StopSearch()
		{
			if (this.userGroupHandle != null)
			{
				this.user.SocialManager.DestroyUserGroup(this.userGroupHandle);
			}
			this.userGroupHandle = null;
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
		public GameServerFoundCallback gameServerFoundCallback;

		[PublicizedFrom(EAccessModifier.Private)]
		public User user;

		[PublicizedFrom(EAccessModifier.Private)]
		public IServerListInterface serverLookup;

		[PublicizedFrom(EAccessModifier.Private)]
		public XblSocialManagerUserGroupHandle userGroupHandle;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isLoadingFriendsList;

		[PublicizedFrom(EAccessModifier.Private)]
		public int activitySearchCount;

		[PublicizedFrom(EAccessModifier.Private)]
		public int sessionSearchCount;
	}
}
