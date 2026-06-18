using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayersRecentListEntry : XUiC_PlayersBlockedListEntryBase
{
	[XuiXmlBinding("lastseenage")]
	public string LastSeenAge
	{
		get
		{
			return this.lastSeenAge ?? "";
		}
		set
		{
			this.lastSeenAge = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("canshowprofile")]
	public bool CanShowProfile
	{
		get
		{
			return this.canShowProfile;
		}
		set
		{
			this.canShowProfile = value;
			this.IsDirty = true;
		}
	}

	public override void UpdateEntry(PlatformUserIdentifierAbs _playerId)
	{
		base.UpdateEntry(_playerId);
		if (base.HasEntry)
		{
			BlockedPlayerList.ListEntry playerStateInfo = BlockedPlayerList.Instance.GetPlayerStateInfo(_playerId);
			this.LastSeenAge = Utils.DescribeTimeSince(DateTime.UtcNow, playerStateInfo.LastSeen);
			this.CanShowProfile = this.PlayerName.CanShowProfile();
		}
	}

	[XuiBindEvent("OnPress", "btnBlockPlayer")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void blockPlayerPressed(XUiController _sender, int _mouseButton)
	{
		if (base.PlayerId == null || this.PlayerName.CanShowProfile())
		{
			return;
		}
		BlockedPlayerList.ListEntry playerStateInfo = BlockedPlayerList.Instance.GetPlayerStateInfo(base.PlayerId);
		if (playerStateInfo != null && playerStateInfo.ResolvedOnce)
		{
			ValueTuple<bool, string> valueTuple = playerStateInfo.SetBlockState(true);
			bool item = valueTuple.Item1;
			string item2 = valueTuple.Item2;
			if (item)
			{
				this.BlockList.IsDirty = true;
				return;
			}
			if (!string.IsNullOrEmpty(item2))
			{
				this.BlockList.DisplayMessage(Localization.Get("xuiBlockedPlayersCantAddHeader", false, null), item2);
			}
		}
	}

	[XuiBindEvent("OnPress", "btnViewProfile")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void bntViewProfilePressed(XUiController _sender, int _mouseButton)
	{
		if (this.PlayerName.CanShowProfile())
		{
			this.PlayerName.ShowProfile();
		}
	}

	[XuiBindComponent("lastSeen", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label lblLastSeenTime;

	[XuiBindComponent("blockBtn", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnBlockPlayer;

	[XuiBindComponent("btnViewProfile", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnViewProfile;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lastSeenAge;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canShowProfile;
}
