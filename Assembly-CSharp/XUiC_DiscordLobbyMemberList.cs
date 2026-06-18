using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordLobbyMemberList : XUiC_List<XUiC_DiscordLobbyMemberList.LobbyMember>
{
	public override void Init()
	{
		base.Init();
		DiscordManager.Instance.LobbyMembersChanged += this.InstanceOnLobbyMembersChanged;
		DiscordManager.Instance.CallChanged += this.InstanceOnCallChanged;
		DiscordManager.Instance.CallMembersChanged += this.InstanceOnCallMembersChanged;
		DiscordManager.Instance.VoiceStateChanged += this.InstanceOnVoiceStateChanged;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		DiscordManager.Instance.LobbyMembersChanged -= this.InstanceOnLobbyMembersChanged;
		DiscordManager.Instance.CallChanged -= this.InstanceOnCallChanged;
		DiscordManager.Instance.CallMembersChanged -= this.InstanceOnCallMembersChanged;
		DiscordManager.Instance.VoiceStateChanged -= this.InstanceOnVoiceStateChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InstanceOnLobbyMembersChanged(DiscordManager.LobbyInfo _lobby)
	{
		this.RebuildList(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InstanceOnCallChanged(DiscordManager.CallInfo _newCall)
	{
		this.RebuildList(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InstanceOnCallMembersChanged(DiscordManager.CallInfo _call)
	{
		this.RebuildList(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InstanceOnVoiceStateChanged(bool _self, ulong _userId)
	{
		base.RefreshBindingsSelfAndChildren();
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		this.membersTempCopy.Clear();
		DiscordManager.LobbyInfo activeVoiceLobby = DiscordManager.Instance.ActiveVoiceLobby;
		if (activeVoiceLobby != null)
		{
			activeVoiceLobby.VoiceCall.GetMembers(this.membersTempCopy);
		}
		foreach (ulong num in this.membersTempCopy)
		{
			this.allEntries.Add(new XUiC_DiscordLobbyMemberList.LobbyMember(DiscordManager.Instance.GetUser(num)));
		}
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
	public readonly List<ulong> membersTempCopy = new List<ulong>();

	[Preserve]
	public class LobbyMember : XUiListEntry<XUiC_DiscordLobbyMemberList.LobbyMember>
	{
		public LobbyMember(DiscordManager.DiscordUser _discordUser)
		{
			this.User = _discordUser;
		}

		public override int CompareTo(XUiC_DiscordLobbyMemberList.LobbyMember _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			if (this.User.IsLocalAccount)
			{
				return -1;
			}
			if (_otherEntry.User.IsLocalAccount)
			{
				return 1;
			}
			return string.Compare(this.User.DisplayName, _otherEntry.User.DisplayName, StringComparison.OrdinalIgnoreCase);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.User.DisplayName.ContainsCaseInsensitive(_searchString);
		}

		public readonly DiscordManager.DiscordUser User;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_List<XUiC_DiscordLobbyMemberList.LobbyMember>.XUiC_ListEntry
	{
		[XuiXmlBinding("is_self")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingIsSelf()
		{
			XUiC_DiscordLobbyMemberList.LobbyMember entryData = this.entryData;
			bool? flag;
			if (entryData == null)
			{
				flag = null;
			}
			else
			{
				DiscordManager.DiscordUser user = entryData.User;
				flag = ((user != null) ? new bool?(user.IsLocalAccount) : null);
			}
			bool? flag2 = flag;
			return flag2.GetValueOrDefault();
		}

		[XuiXmlBinding("displayname")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingDisplayName()
		{
			XUiC_DiscordLobbyMemberList.LobbyMember entryData = this.entryData;
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

		[XuiXmlBinding("is_speaking")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingIsSpeaking()
		{
			XUiC_DiscordLobbyMemberList.LobbyMember entryData = this.entryData;
			bool? flag;
			if (entryData == null)
			{
				flag = null;
			}
			else
			{
				DiscordManager.DiscordUser user = entryData.User;
				flag = ((user != null) ? new bool?(user.IsSpeaking) : null);
			}
			bool? flag2 = flag;
			return flag2.GetValueOrDefault();
		}

		[XuiXmlBinding("voice_muted")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingVoiceMuted()
		{
			XUiC_DiscordLobbyMemberList.LobbyMember entryData = this.entryData;
			bool? flag;
			if (entryData == null)
			{
				flag = null;
			}
			else
			{
				DiscordManager.DiscordUser user = entryData.User;
				flag = ((user != null) ? new bool?(user.IsMuted) : null);
			}
			bool? flag2 = flag;
			return flag2.GetValueOrDefault();
		}

		[XuiXmlBinding("output_muted")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingOutputMuted()
		{
			XUiC_DiscordLobbyMemberList.LobbyMember entryData = this.entryData;
			bool? flag;
			if (entryData == null)
			{
				flag = null;
			}
			else
			{
				DiscordManager.DiscordUser user = entryData.User;
				flag = ((user != null) ? new bool?(user.IsDeafened) : null);
			}
			bool? flag2 = flag;
			return flag2.GetValueOrDefault();
		}
	}
}
