using System;
using System.Collections.Generic;
using Discord.Sdk;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordFriendsList : XUiC_List<XUiC_DiscordFriendsList.FriendEntry>
{
	public override void Init()
	{
		base.Init();
		DiscordManager.Instance.FriendsListChanged += this.discordFriendsListChanged;
		DiscordManager.Instance.StatusChanged += this.discordStatusChanged;
		for (int i = 0; i < 4; i++)
		{
			XUiC_DiscordFriendsList.sectionsOpened[i] = i != 3;
		}
	}

	[XuiBindEvent("OnPress", "btnDiscordLinkAccount")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDiscordLinkAccount_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_DiscordLogin.Open(delegate
		{
		}, true, false, false, false, false);
		DiscordManager.Instance.AuthManager.LoginDiscordUser();
	}

	public override void Cleanup()
	{
		base.Cleanup();
		DiscordManager.Instance.FriendsListChanged -= this.discordFriendsListChanged;
		DiscordManager.Instance.StatusChanged -= this.discordStatusChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordFriendsListChanged()
	{
		this.UpdateList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordStatusChanged(DiscordManager.EDiscordStatus _obj)
	{
		this.UpdateList();
	}

	public void ToggleSectionVisibility(XUiC_DiscordFriendsList.FriendEntry.ESection _section)
	{
		XUiC_DiscordFriendsList.sectionsOpened[(int)_section] = !XUiC_DiscordFriendsList.sectionsOpened[(int)_section];
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
		Array.Clear(XUiC_DiscordFriendsList.sectionUserCounts, 0, XUiC_DiscordFriendsList.sectionUserCounts.Length);
		this.users.Clear();
		DiscordManager.Instance.GetFriends(this.users);
		DiscordManager.Instance.GetInServer(this.users);
		foreach (DiscordManager.DiscordUser discordUser in this.users)
		{
			XUiC_DiscordFriendsList.FriendEntry friendEntry = new XUiC_DiscordFriendsList.FriendEntry(discordUser);
			this.allEntries.Add(friendEntry);
			XUiC_DiscordFriendsList.sectionUserCounts[(int)friendEntry.SectionType]++;
		}
		this.users.Clear();
		this.allEntries.Add(this.entryHeaderInServer);
		this.allEntries.Add(this.entryHeaderInGame);
		this.allEntries.Add(this.entryHeaderOnline);
		this.allEntries.Add(this.entryHeaderOffline);
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FilterResults(string _textMatch)
	{
		base.FilterResults(_textMatch);
		for (int i = 0; i < 4; i++)
		{
			if (!XUiC_DiscordFriendsList.sectionsOpened[i])
			{
				XUiC_DiscordFriendsList.FriendEntry.ESection sectionType = (XUiC_DiscordFriendsList.FriendEntry.ESection)i;
				this.filteredEntries.RemoveAll((XUiC_DiscordFriendsList.FriendEntry _entry) => _entry.EntryType == XUiC_DiscordFriendsList.FriendEntry.EEntryType.User && _entry.SectionType == sectionType);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "self_activity")
		{
			_value = DiscordManager.Instance.Presence.JoinableActivitySet.ToString();
			return true;
		}
		if (_bindingName == "discord_is_ready")
		{
			_value = DiscordManager.Instance.IsReady.ToString();
			return true;
		}
		if (_bindingName == "discord_supports_full_accounts")
		{
			_value = DiscordManager.SupportsFullAccounts.ToString();
			return true;
		}
		if (!(_bindingName == "discordaccountlinked"))
		{
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		DiscordManager.DiscordUser localUser = DiscordManager.Instance.LocalUser;
		_value = (localUser == null || !localUser.IsProvisionalAccount).ToString();
		return true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.allEntries.Count == 0)
		{
			this.RebuildList(false);
		}
	}

	[XuiBindComponent("btnDiscordLinkAccount", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDiscordLinkAccount;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly bool[] sectionsOpened = new bool[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly int[] sectionUserCounts = new int[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<DiscordManager.DiscordUser> users = new HashSet<DiscordManager.DiscordUser>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_DiscordFriendsList.FriendEntry entryHeaderInServer = new XUiC_DiscordFriendsList.FriendEntry(XUiC_DiscordFriendsList.FriendEntry.ESection.InServer);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_DiscordFriendsList.FriendEntry entryHeaderInGame = new XUiC_DiscordFriendsList.FriendEntry(XUiC_DiscordFriendsList.FriendEntry.ESection.OnlineInGame);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_DiscordFriendsList.FriendEntry entryHeaderOnline = new XUiC_DiscordFriendsList.FriendEntry(XUiC_DiscordFriendsList.FriendEntry.ESection.Online);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_DiscordFriendsList.FriendEntry entryHeaderOffline = new XUiC_DiscordFriendsList.FriendEntry(XUiC_DiscordFriendsList.FriendEntry.ESection.Offline);

	[Preserve]
	public class FriendEntry : XUiListEntry<XUiC_DiscordFriendsList.FriendEntry>
	{
		public XUiC_DiscordFriendsList.FriendEntry.EEntryType EntryType { get; }

		public XUiC_DiscordFriendsList.FriendEntry.ESection SectionType
		{
			get
			{
				if (this.EntryType != XUiC_DiscordFriendsList.FriendEntry.EEntryType.SectionHeader)
				{
					return this.getUserSectionType();
				}
				return this.sectionHeaderType;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_DiscordFriendsList.FriendEntry.ESection getUserSectionType()
		{
			if (this.User == null)
			{
				return XUiC_DiscordFriendsList.FriendEntry.ESection.Offline;
			}
			if (this.User.InSameSession)
			{
				return XUiC_DiscordFriendsList.FriendEntry.ESection.InServer;
			}
			if (this.User.InGame)
			{
				return XUiC_DiscordFriendsList.FriendEntry.ESection.OnlineInGame;
			}
			XUiC_DiscordFriendsList.FriendEntry.ESection esection;
			switch (this.User.DiscordState)
			{
			case 0:
				esection = XUiC_DiscordFriendsList.FriendEntry.ESection.Online;
				break;
			case 1:
				esection = XUiC_DiscordFriendsList.FriendEntry.ESection.Offline;
				break;
			case 2:
				esection = XUiC_DiscordFriendsList.FriendEntry.ESection.Online;
				break;
			case 3:
				esection = XUiC_DiscordFriendsList.FriendEntry.ESection.Online;
				break;
			case 4:
				esection = XUiC_DiscordFriendsList.FriendEntry.ESection.Online;
				break;
			case 5:
				esection = XUiC_DiscordFriendsList.FriendEntry.ESection.Online;
				break;
			case 6:
				esection = XUiC_DiscordFriendsList.FriendEntry.ESection.Online;
				break;
			case 7:
				esection = XUiC_DiscordFriendsList.FriendEntry.ESection.Offline;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return esection;
		}

		public FriendEntry(XUiC_DiscordFriendsList.FriendEntry.ESection _sectionHeaderType)
		{
			this.EntryType = XUiC_DiscordFriendsList.FriendEntry.EEntryType.SectionHeader;
			this.sectionHeaderType = _sectionHeaderType;
		}

		public FriendEntry(DiscordManager.DiscordUser _discordUser)
		{
			this.User = _discordUser;
			this.EntryType = XUiC_DiscordFriendsList.FriendEntry.EEntryType.User;
		}

		public override int CompareTo(XUiC_DiscordFriendsList.FriendEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			int num = this.SectionType.CompareTo(_otherEntry.SectionType);
			if (num != 0)
			{
				return num;
			}
			num = this.EntryType.CompareTo(_otherEntry.EntryType);
			if (num != 0)
			{
				return num;
			}
			if (_otherEntry.User == null)
			{
				return -1;
			}
			if (this.User == null)
			{
				return 1;
			}
			num = -this.User.InSameSession.CompareTo(_otherEntry.User.InSameSession);
			if (num != 0)
			{
				return num;
			}
			num = -this.User.IsFriend.CompareTo(_otherEntry.User.IsFriend);
			if (num != 0)
			{
				return num;
			}
			num = this.User.DiscordState.CompareTo(_otherEntry.User.DiscordState);
			if (num != 0)
			{
				return num;
			}
			return string.Compare(this.User.DisplayName, _otherEntry.User.DisplayName, StringComparison.OrdinalIgnoreCase);
		}

		public override bool MatchesSearch(string _searchString)
		{
			if (this.EntryType == XUiC_DiscordFriendsList.FriendEntry.EEntryType.SectionHeader)
			{
				return true;
			}
			if (string.IsNullOrEmpty(_searchString))
			{
				return true;
			}
			DiscordManager.DiscordUser user = this.User;
			return user != null && user.DisplayName.ContainsCaseInsensitive(_searchString);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_DiscordFriendsList.FriendEntry.ESection sectionHeaderType;

		public readonly DiscordManager.DiscordUser User;

		public enum EEntryType
		{
			SectionHeader,
			User
		}

		public enum ESection
		{
			InServer,
			OnlineInGame,
			Online,
			Offline,
			Count
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class DiscordFriendsListEntryController : XUiC_List<XUiC_DiscordFriendsList.FriendEntry>.XUiC_ListEntry
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
					XUiC_DiscordFriendsList.FriendEntry entry = base.GetEntry();
					if (entry.EntryType == XUiC_DiscordFriendsList.FriendEntry.EEntryType.SectionHeader)
					{
						((XUiC_DiscordFriendsList)this.list).ToggleSectionVisibility(entry.SectionType);
						return;
					}
					bool isPrimaryUI = this.xui.playerUI.isPrimaryUI;
					DiscordManager.DiscordUser user = entry.User;
					XUiC_PopupMenu popupMenuWindow = this.xui.PopupMenuWindow;
					popupMenuWindow.Setup(new Vector2i(0, -26), Localization.Get("xuiDiscordUserActions", false, null));
					if (!isPrimaryUI)
					{
						popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordSendMessage", false, null), "ui_game_symbol_invite", GameManager.Instance.World != null, null, delegate(XUiC_PopupMenuItem.Entry _)
						{
							LocalPlayerUI uiforPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
							uiforPrimaryPlayer.entityPlayer.AimingGun = false;
							uiforPrimaryPlayer.windowManager.Open(XUiC_Chat.ID, true);
							XUiC_Chat.SetChatTarget(uiforPrimaryPlayer.xui, EChatType.Discord, base.GetEntry().User.ID.ToString());
						}));
					}
					popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordSendJoinRequest", false, null), "ui_game_symbol_send_join_request", user.JoinableActivity && !user.InSameSession, null, delegate(XUiC_PopupMenuItem.Entry _)
					{
						user.SendJoinRequest();
					}));
					if (!isPrimaryUI)
					{
						popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordSendInvite", false, null), "ui_game_symbol_send_invite", DiscordManager.Instance.Presence.JoinableActivitySet && !user.InSameSession, null, delegate(XUiC_PopupMenuItem.Entry _)
						{
							user.SendInvite();
						}));
					}
					if (!isPrimaryUI)
					{
						popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordAddGameFriend", false, null), "ui_game_symbol_add_game_friend", !user.IsFriend && user.GameRelationship != 4, null, delegate(XUiC_PopupMenuItem.Entry _)
						{
							user.SendFriendRequest(true);
						}));
						XUiC_PopupMenu xuiC_PopupMenu = popupMenuWindow;
						string text = Localization.Get("xuiDiscordAddDiscordFriend", false, null);
						string text2 = "ui_game_symbol_add_discord_friend";
						bool flag;
						if (user.DiscordRelationship != 1 && user.DiscordRelationship != 4)
						{
							DiscordManager.DiscordUser localUser = DiscordManager.Instance.LocalUser;
							flag = localUser == null || !localUser.IsProvisionalAccount;
						}
						else
						{
							flag = false;
						}
						xuiC_PopupMenu.AddItem(new XUiC_PopupMenuItem.Entry(text, text2, flag, null, delegate(XUiC_PopupMenuItem.Entry _)
						{
							user.SendFriendRequest(false);
						}));
					}
					popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordRemoveFriend", false, null), "ui_game_symbol_x", user.IsFriend, null, delegate(XUiC_PopupMenuItem.Entry _)
					{
						XUiC_MessageBoxWindowGroup.ShowOkCancel(this.xui, Localization.Get("xuiDiscordRemoveFriendConfirmationTitle", false, null), string.Format(Localization.Get("xuiDiscordRemoveFriendConfirmationText", false, null), user.DisplayName), new Action(user.RemoveFriend), null, false, false, false);
					}));
					popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordBlockUser", false, null), "ui_game_symbol_player_block", user.DiscordRelationship != 2, null, delegate(XUiC_PopupMenuItem.Entry _)
					{
						XUiC_MessageBoxWindowGroup.ShowOkCancel(this.xui, Localization.Get("xuiDiscordBlockUserConfirmationTitle", false, null), string.Format(Localization.Get("xuiDiscordBlockUserConfirmationText", false, null), user.DisplayName), new Action(user.BlockUser), null, false, false, false);
					}));
					if (!isPrimaryUI)
					{
						popupMenuWindow.AddItem(new XUiC_PopupMenuItem.Entry(Localization.Get("xuiDiscordUserVolume", false, null), "ui_game_symbol_noise", user.Volume, user.InCurrentVoice, null, delegate(XUiC_PopupMenuItem.Entry _, double _value)
						{
							Log.Out(string.Format("[Discord UI] New output volume for user {0}: {1}", user.DisplayName, _value));
							user.Volume = _value;
						}));
					}
					popupMenuWindow.Show();
				};
			}
		}

		public override void SetEntry(XUiC_DiscordFriendsList.FriendEntry _data)
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
			XUiC_DiscordFriendsList.FriendEntry entryData = this.entryData;
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

		[XuiXmlBinding("discordstate_icon")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingDiscordStateIcon()
		{
			XUiC_DiscordFriendsList.FriendEntry entryData = this.entryData;
			if (((entryData != null) ? entryData.User : null) == null)
			{
				return "";
			}
			string text;
			switch (this.entryData.User.DiscordState)
			{
			case 0:
				text = "discord_status_available";
				break;
			case 1:
				text = "discord_status_offline";
				break;
			case 2:
				text = "";
				break;
			case 3:
				text = "discord_status_idle";
				break;
			case 4:
				text = "discord_status_dnd";
				break;
			case 5:
				text = "";
				break;
			case 6:
				text = "";
				break;
			case 7:
				text = "";
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return text;
		}

		[XuiXmlBinding("statustext")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingStatusText()
		{
			XUiC_DiscordFriendsList.FriendEntry entryData = this.entryData;
			DiscordManager.DiscordUser discordUser = ((entryData != null) ? entryData.User : null);
			if (discordUser == null)
			{
				return "";
			}
			if (discordUser.InSameSession)
			{
				Activity activity = discordUser.Activity;
				string text;
				if (activity == null)
				{
					text = null;
				}
				else
				{
					ActivityAssets activityAssets = activity.Assets();
					text = ((activityAssets != null) ? activityAssets.LargeText() : null);
				}
				string text2 = text;
				string text3 = Localization.Get("xuiDiscordSameSession", false, null);
				if (!string.IsNullOrEmpty(text2))
				{
					return text3 + " - " + text2;
				}
				return text3;
			}
			else
			{
				if (discordUser.InGame)
				{
					return discordUser.Activity.Details() ?? "";
				}
				return discordUser.DiscordStateLocalized;
			}
		}

		[XuiXmlBinding("entry_type")]
		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_DiscordFriendsList.FriendEntry.EEntryType bindingEntryType()
		{
			XUiC_DiscordFriendsList.FriendEntry entryData = this.entryData;
			if (entryData == null)
			{
				return XUiC_DiscordFriendsList.FriendEntry.EEntryType.SectionHeader;
			}
			return entryData.EntryType;
		}

		[XuiXmlBinding("section_type")]
		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_DiscordFriendsList.FriendEntry.ESection bindingSectionType()
		{
			XUiC_DiscordFriendsList.FriendEntry entryData = this.entryData;
			if (entryData == null)
			{
				return XUiC_DiscordFriendsList.FriendEntry.ESection.Offline;
			}
			return entryData.SectionType;
		}

		[XuiXmlBinding("has_joinable_activity")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingHasJoinableActivity()
		{
			XUiC_DiscordFriendsList.FriendEntry entryData = this.entryData;
			bool? flag;
			if (entryData == null)
			{
				flag = null;
			}
			else
			{
				DiscordManager.DiscordUser user = entryData.User;
				flag = ((user != null) ? new bool?(user.JoinableActivity) : null);
			}
			bool? flag2 = flag;
			return flag2.GetValueOrDefault();
		}

		[XuiXmlBinding("section_open")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingSectionOpen()
		{
			return this.entryData == null || XUiC_DiscordFriendsList.sectionsOpened[(int)this.entryData.SectionType];
		}

		[XuiXmlBinding("section_user_count")]
		[PublicizedFrom(EAccessModifier.Private)]
		public int bindingUserCount()
		{
			if (this.entryData != null)
			{
				return XUiC_DiscordFriendsList.sectionUserCounts[(int)this.entryData.SectionType];
			}
			return 0;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public XUiV_Texture avatarTexture;
	}
}
