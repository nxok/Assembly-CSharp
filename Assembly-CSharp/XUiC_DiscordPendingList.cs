using System;
using System.Collections.Generic;
using Discord.Sdk;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordPendingList : XUiC_List<XUiC_DiscordPendingList.PendingEntry>
{
	public override void Init()
	{
		base.Init();
		DiscordManager.Instance.StatusChanged += this.discordStatusChanged;
		DiscordManager.Instance.RelationshipChanged += this.discordRelationshipChanged;
		DiscordManager.Instance.ActivityInviteReceived += this.discordActivityInviteReceived;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		DiscordManager.Instance.StatusChanged -= this.discordStatusChanged;
		DiscordManager.Instance.RelationshipChanged -= this.discordRelationshipChanged;
		DiscordManager.Instance.ActivityInviteReceived -= this.discordActivityInviteReceived;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordStatusChanged(DiscordManager.EDiscordStatus _status)
	{
		if (_status == DiscordManager.EDiscordStatus.Disconnected)
		{
			this.UpdateList();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordActivityInviteReceived(DiscordManager.DiscordUser _user, bool _cleared, ActivityActionTypes _type)
	{
		this.UpdateList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordRelationshipChanged(DiscordManager.DiscordUser _user)
	{
		this.UpdateList();
	}

	public void UpdateList()
	{
		int page = base.Page;
		this.RebuildList(false);
		base.Page = page;
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		this.users.Clear();
		DiscordManager.Instance.GetUsersWithPendingAction(this.users);
		foreach (DiscordManager.DiscordUser discordUser in this.users)
		{
			if (discordUser.PendingFriendRequest)
			{
				this.allEntries.Add(new XUiC_DiscordPendingList.PendingEntry(discordUser, (discordUser.GameRelationship == 3) ? XUiC_DiscordPendingList.PendingEntry.EEntryType.FriendRequestGame : XUiC_DiscordPendingList.PendingEntry.EEntryType.FriendRequestDiscord));
			}
			if (discordUser.PendingIncomingJoinRequest)
			{
				this.allEntries.Add(new XUiC_DiscordPendingList.PendingEntry(discordUser, XUiC_DiscordPendingList.PendingEntry.EEntryType.JoinRequest));
			}
			if (discordUser.PendingIncomingInvite)
			{
				this.allEntries.Add(new XUiC_DiscordPendingList.PendingEntry(discordUser, XUiC_DiscordPendingList.PendingEntry.EEntryType.Invite));
			}
		}
		this.users.Clear();
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.allEntries.Count == 0)
		{
			this.RebuildList(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<DiscordManager.DiscordUser> users = new HashSet<DiscordManager.DiscordUser>();

	[Preserve]
	public class PendingEntry : XUiListEntry<XUiC_DiscordPendingList.PendingEntry>
	{
		public PendingEntry(DiscordManager.DiscordUser _discordUser, XUiC_DiscordPendingList.PendingEntry.EEntryType _type)
		{
			this.User = _discordUser;
			this.EntryType = _type;
		}

		public override int CompareTo(XUiC_DiscordPendingList.PendingEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			int num = this.EntryType.CompareTo(_otherEntry.EntryType);
			if (num != 0)
			{
				return num;
			}
			return string.Compare(this.User.DisplayName, _otherEntry.User.DisplayName, StringComparison.OrdinalIgnoreCase);
		}

		public override bool MatchesSearch(string _searchString)
		{
			if (string.IsNullOrEmpty(_searchString))
			{
				return true;
			}
			DiscordManager.DiscordUser user = this.User;
			return user != null && user.DisplayName.ContainsCaseInsensitive(_searchString);
		}

		public readonly XUiC_DiscordPendingList.PendingEntry.EEntryType EntryType;

		public readonly DiscordManager.DiscordUser User;

		public enum EEntryType
		{
			JoinRequest,
			Invite,
			FriendRequestGame,
			FriendRequestDiscord
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class DiscordPendingListEntryController : XUiC_List<XUiC_DiscordPendingList.PendingEntry>.XUiC_ListEntry
	{
		public override void Init()
		{
			base.Init();
			XUiController childById = base.GetChildById("avatar");
			XUiV_Texture xuiV_Texture = ((childById != null) ? childById.ViewComponent : null) as XUiV_Texture;
			if (xuiV_Texture != null)
			{
				this.avatarTexture = xuiV_Texture;
			}
			XUiController childById2 = base.GetChildById("btnAccept");
			XUiV_Button xuiV_Button = ((childById2 != null) ? childById2.ViewComponent : null) as XUiV_Button;
			if (xuiV_Button != null)
			{
				xuiV_Button.Controller.OnPress += delegate(XUiController _, int _)
				{
					XUiC_DiscordPendingList.PendingEntry entry = base.GetEntry();
					DiscordManager.DiscordUser user = entry.User;
					switch (entry.EntryType)
					{
					case XUiC_DiscordPendingList.PendingEntry.EEntryType.JoinRequest:
						user.SendInvite();
						return;
					case XUiC_DiscordPendingList.PendingEntry.EEntryType.Invite:
						user.AcceptInvite(null);
						return;
					case XUiC_DiscordPendingList.PendingEntry.EEntryType.FriendRequestGame:
					case XUiC_DiscordPendingList.PendingEntry.EEntryType.FriendRequestDiscord:
						user.SendFriendRequest(entry.EntryType == XUiC_DiscordPendingList.PendingEntry.EEntryType.FriendRequestGame);
						return;
					default:
						throw new ArgumentOutOfRangeException();
					}
				};
			}
			XUiController childById3 = base.GetChildById("btnDecline");
			XUiV_Button xuiV_Button2 = ((childById3 != null) ? childById3.ViewComponent : null) as XUiV_Button;
			if (xuiV_Button2 != null)
			{
				xuiV_Button2.Controller.OnPress += delegate(XUiController _, int _)
				{
					XUiC_DiscordPendingList.PendingEntry entry2 = base.GetEntry();
					DiscordManager.DiscordUser user2 = entry2.User;
					switch (entry2.EntryType)
					{
					case XUiC_DiscordPendingList.PendingEntry.EEntryType.JoinRequest:
						user2.DeclineJoinRequest();
						return;
					case XUiC_DiscordPendingList.PendingEntry.EEntryType.Invite:
						user2.DeclineInvite();
						return;
					case XUiC_DiscordPendingList.PendingEntry.EEntryType.FriendRequestGame:
					case XUiC_DiscordPendingList.PendingEntry.EEntryType.FriendRequestDiscord:
						user2.DeclineFriendRequest(entry2.EntryType == XUiC_DiscordPendingList.PendingEntry.EEntryType.FriendRequestGame);
						return;
					default:
						throw new ArgumentOutOfRangeException();
					}
				};
			}
		}

		public override void SetEntry(XUiC_DiscordPendingList.PendingEntry _data)
		{
			base.SetEntry(_data);
			if (this.avatarTexture == null)
			{
				return;
			}
			XUiV_TextureBased xuiV_TextureBased = this.avatarTexture;
			Texture texture;
			if (_data == null)
			{
				texture = null;
			}
			else
			{
				DiscordManager.DiscordUser user = _data.User;
				texture = ((user != null) ? user.Avatar : null);
			}
			xuiV_TextureBased.Texture = texture;
		}

		[XuiXmlBinding("displayname")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingDisplayName()
		{
			XUiC_DiscordPendingList.PendingEntry entryData = this.entryData;
			string text;
			if (entryData == null)
			{
				text = null;
			}
			else
			{
				DiscordManager.DiscordUser user = entryData.User;
				text = ((user != null) ? user.DisplayName : null);
			}
			return text ?? "";
		}

		[XuiXmlBinding("entry_type")]
		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_DiscordPendingList.PendingEntry.EEntryType bindingEntryType()
		{
			XUiC_DiscordPendingList.PendingEntry entryData = this.entryData;
			if (entryData == null)
			{
				return XUiC_DiscordPendingList.PendingEntry.EEntryType.Invite;
			}
			return entryData.EntryType;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public XUiV_Texture avatarTexture;
	}
}
