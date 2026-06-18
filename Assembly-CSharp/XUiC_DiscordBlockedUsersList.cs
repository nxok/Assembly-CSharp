using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordBlockedUsersList : XUiC_List<XUiC_DiscordBlockedUsersList.Entry>
{
	public override void Init()
	{
		base.Init();
		DiscordManager.Instance.FriendsListChanged += this.discordFriendsListChanged;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		DiscordManager.Instance.FriendsListChanged -= this.discordFriendsListChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordFriendsListChanged()
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
		DiscordManager.Instance.GetBlockedUsers(this.users);
		foreach (DiscordManager.DiscordUser discordUser in this.users)
		{
			XUiC_DiscordBlockedUsersList.Entry entry = new XUiC_DiscordBlockedUsersList.Entry(discordUser);
			this.allEntries.Add(entry);
		}
		this.users.Clear();
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
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
	public class Entry : XUiListEntry<XUiC_DiscordBlockedUsersList.Entry>
	{
		public Entry(DiscordManager.DiscordUser _discordUser)
		{
			this.User = _discordUser;
		}

		public override int CompareTo(XUiC_DiscordBlockedUsersList.Entry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			if (_otherEntry.User == null)
			{
				return -1;
			}
			if (this.User == null)
			{
				return 1;
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

		public readonly DiscordManager.DiscordUser User;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class DiscordBlockedUsersListEntryController : XUiC_List<XUiC_DiscordBlockedUsersList.Entry>.XUiC_ListEntry
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
			if (base.ViewComponent.EventOnPress)
			{
				base.OnPress += delegate(XUiController _, int _)
				{
					XUiC_DiscordBlockedUsersList.Entry entry = base.GetEntry();
					DiscordManager.DiscordUser user = entry.User;
					XUiC_PopupMenu popupMenuWindow = this.xui.PopupMenuWindow;
					popupMenuWindow.Setup(new Vector2i(0, -26), Localization.Get("xuiDiscordUserActions", false, null));
					popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordAddGameFriend", false, null), "ui_game_symbol_add_game_friend", true, null, delegate(XUiC_PopupMenuItem.Entry _)
					{
						user.SendFriendRequest(true);
					}));
					string text = Localization.Get("xuiDiscordAddDiscordFriend", false, null);
					string text2 = "ui_game_symbol_add_discord_friend";
					DiscordManager.DiscordUser localUser = DiscordManager.Instance.LocalUser;
					popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(text, text2, localUser == null || !localUser.IsProvisionalAccount, null, delegate(XUiC_PopupMenuItem.Entry _)
					{
						user.SendFriendRequest(false);
					}));
					popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordUnblockUser", false, null), "ui_game_symbol_modded", true, null, delegate(XUiC_PopupMenuItem.Entry _)
					{
						user.UnblockUser();
					}));
					popupMenuWindow.Show();
				};
			}
		}

		public override void SetEntry(XUiC_DiscordBlockedUsersList.Entry _data)
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
			XUiC_DiscordBlockedUsersList.Entry entryData = this.entryData;
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

		[PublicizedFrom(EAccessModifier.Private)]
		public XUiV_Texture avatarTexture;
	}
}
