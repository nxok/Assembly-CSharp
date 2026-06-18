using System;
using System.Collections.Generic;
using Unity.XGamingRuntime;

namespace Platform.XBL
{
	public class FriendsListXbl
	{
		public FriendsListXbl(SocialManagerXbl socialManager)
		{
			this.socialManager = socialManager;
			if (!socialManager.TryCreateUserGroup(7, 1, new SocialManagerXbl.UserGroupMembersChanged(this.OnFriendsListChanged), out this.friendsUserGroup))
			{
				Log.Error("[FriendsListXbl] failed to create friends social manager group");
				this.friendsUserGroup = null;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnFriendsListChanged(ulong[] friends)
		{
			if (this.friendsXuidsTemp == null)
			{
				this.friendsXuidsTemp = new HashSet<ulong>();
			}
			foreach (ulong num in friends)
			{
				this.friendsXuidsTemp.Add(num);
			}
			HashSet<ulong> hashSet = this.friendsXuidsTemp;
			HashSet<ulong> hashSet2 = this.friendXuids;
			this.friendXuids = hashSet;
			this.friendsXuidsTemp = hashSet2;
			HashSet<ulong> hashSet3 = this.friendsXuidsTemp;
			if (hashSet3 != null)
			{
				hashSet3.Clear();
			}
			XblXuidMapper.ResolveUserIdentifiers(this.friendXuids);
		}

		public bool IsFriend(ulong xuid)
		{
			if (this.friendsUserGroup == null)
			{
				Log.Error("[FriendsListXbl] could not check IsFriend, friends user group has not been initialized yet.");
				return false;
			}
			if (this.friendXuids == null)
			{
				Log.Error("[FriendsListXbl] could not check IsFriend, friends list has not been retrieved yet");
				return false;
			}
			return this.friendXuids.Contains(xuid);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly SocialManagerXbl socialManager;

		[PublicizedFrom(EAccessModifier.Private)]
		public XblSocialManagerUserGroupHandle friendsUserGroup;

		[PublicizedFrom(EAccessModifier.Private)]
		public HashSet<ulong> friendXuids;

		[PublicizedFrom(EAccessModifier.Private)]
		public HashSet<ulong> friendsXuidsTemp;
	}
}
