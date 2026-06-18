using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.XGamingRuntime;
using Unity.XGamingRuntime.Interop;
using UnityEngine;

namespace Platform.XBL
{
	public class SocialManagerXbl
	{
		public SocialManagerXbl(XUserHandle user)
		{
			int num = SDK.XBL.XblSocialManagerAddLocalUser(user, 0);
			if (!HR.SUCCEEDED(num))
			{
				XblHelpers.LogHR(num, "XblSocialManagerAddLocalUser", false);
				return;
			}
			this.localUser = user;
		}

		public bool TryCreateUserGroup(XblPresenceFilter presenceFilter, XblRelationshipFilter relationshipFilter, SocialManagerXbl.UserGroupMembersChanged callback, out XblSocialManagerUserGroupHandle handle)
		{
			if (this.localUser == null)
			{
				Log.Error("[XBL] Social users lookup not available as the local user was not registered");
				handle = null;
				return false;
			}
			if (callback == null)
			{
				Log.Error("[XBL] TryCreateUserGroup null callback not permitted");
				handle = null;
				return false;
			}
			int num = SDK.XBL.XblSocialManagerCreateSocialUserGroupFromFilters(this.localUser, presenceFilter, relationshipFilter, ref handle);
			if (!HR.SUCCEEDED(num))
			{
				Log.Error(string.Format("[XBL] Failed to create user group for {0} {1}", presenceFilter, relationshipFilter));
				XblHelpers.LogHR(num, "XblSocialManagerCreateSocialUserGroupFromFilters", false);
				handle = null;
				return false;
			}
			this.userGroups.Add(handle, new SocialManagerXbl.UserGroup(handle, callback));
			if (this.updateCoroutine == null)
			{
				this.updateCoroutine = ThreadManager.StartCoroutine(this.UpdateSocialManagerCoroutine());
			}
			return true;
		}

		public void DestroyUserGroup(XblSocialManagerUserGroupHandle handle)
		{
			this.userGroups.Remove(handle);
			int num = SDK.XBL.XblSocialManagerDestroySocialUserGroup(handle);
			if (!HR.SUCCEEDED(num))
			{
				XblHelpers.LogHR(num, "XblSocialManagerCreateSocialUserGroupFromFilters", false);
			}
			if (this.userGroups.Count == 0 && this.updateCoroutine != null)
			{
				ThreadManager.StopCoroutine(this.updateCoroutine);
				this.updateCoroutine = null;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator UpdateSocialManagerCoroutine()
		{
			int num;
			for (;;)
			{
				XblSocialManagerEvent[] array;
				num = SDK.XBL.XblSocialManagerDoWork(ref array);
				if (!HR.SUCCEEDED(num))
				{
					break;
				}
				if (array != null && array.Length != 0)
				{
					if (SocialManagerXbl.IsUpdateRequired(array))
					{
						foreach (SocialManagerXbl.UserGroup userGroup in this.userGroups.Values)
						{
							userGroup.NotifyChanged();
						}
					}
					foreach (XblSocialManagerEvent xblSocialManagerEvent in array)
					{
						if (xblSocialManagerEvent.EventType == 6)
						{
							SocialManagerXbl.UserGroup userGroup2;
							if (!this.userGroups.TryGetValue(xblSocialManagerEvent.LoadedGroup, out userGroup2))
							{
								Log.Error("[XBL] LoadedGroup did not match saved handle");
							}
							else
							{
								userGroup2.isLoaded = true;
								userGroup2.NotifyChanged();
							}
						}
					}
				}
				yield return null;
			}
			XblHelpers.LogHR(num, "XblAchievementsManagerDoWork", false);
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool IsUpdateRequired(XblSocialManagerEvent[] socialEvents)
		{
			for (int i = 0; i < socialEvents.Length; i++)
			{
				switch (socialEvents[i].EventType)
				{
				case 0:
				case 1:
				case 4:
					return true;
				case 2:
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public XUserHandle localUser;

		[PublicizedFrom(EAccessModifier.Private)]
		public Coroutine updateCoroutine;

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<XblSocialManagerUserGroupHandle, SocialManagerXbl.UserGroup> userGroups = new Dictionary<XblSocialManagerUserGroupHandle, SocialManagerXbl.UserGroup>();

		public delegate void UserGroupMembersChanged(ulong[] members);

		[PublicizedFrom(EAccessModifier.Private)]
		public class UserGroup
		{
			public UserGroup(XblSocialManagerUserGroupHandle handle, SocialManagerXbl.UserGroupMembersChanged membersChangedCallback)
			{
				this.handle = handle;
				this.membersChangedCallback = membersChangedCallback;
			}

			public void NotifyChanged()
			{
				if (!this.isLoaded)
				{
					return;
				}
				ulong[] array = this.membersCache;
				int num = SDK.XBL.XblSocialManagerUserGroupGetUsersTrackedByGroup(this.handle, ref this.membersCache);
				if (!HR.SUCCEEDED(num))
				{
					XblHelpers.LogHR(num, "XblSocialManagerUserGroupGetUsersTrackedByGroup", false);
					this.membersCache = null;
					return;
				}
				Array.Sort<ulong>(this.membersCache);
				if (array != null && array.Length == this.membersCache.Length && this.membersCache.SequenceEqual(array))
				{
					Log.Out("[XBL] social manager skipping user group update as member list didn't change");
					return;
				}
				ulong[] array2 = new ulong[this.membersCache.Length];
				Array.Copy(this.membersCache, array2, this.membersCache.Length);
				this.membersChangedCallback(array2);
			}

			public readonly XblSocialManagerUserGroupHandle handle;

			public readonly SocialManagerXbl.UserGroupMembersChanged membersChangedCallback;

			public bool isLoaded;

			[PublicizedFrom(EAccessModifier.Private)]
			public ulong[] membersCache;
		}
	}
}
