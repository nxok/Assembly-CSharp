using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayersBlockedListEntryBase : XUiController
{
	public PlatformUserIdentifierAbs PlayerId
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.playerId;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.playerId = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("hasentry")]
	public bool HasEntry
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.PlayerId != null;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
	}

	public virtual void UpdateEntry(PlatformUserIdentifierAbs _playerId)
	{
		BlockedPlayerList.ListEntry playerStateInfo = BlockedPlayerList.Instance.GetPlayerStateInfo(_playerId);
		if (playerStateInfo != null)
		{
			this.PlayerId = _playerId;
			this.PlayerName.UpdatePlayerData(playerStateInfo.PlayerData, true, null);
			return;
		}
		this.Clear();
	}

	public void Clear()
	{
		this.PlayerId = null;
		this.PlayerName.ClearPlayerData();
	}

	[XuiBindEvent("OnPress", "btnReportPlayer")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void reportPlayerPressed(XUiController _sender, int _mouseButton)
	{
		if (PlatformManager.MultiPlatform.PlayerReporting == null || this.PlayerId == null)
		{
			return;
		}
		XUiC_ReportPlayer.Open(BlockedPlayerList.Instance.GetPlayerStateInfo(this.PlayerId).PlayerData, EnumReportCategory.None, "");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs playerId;

	[XuiBindParent(true)]
	public readonly XUiC_BlockedPlayersList BlockList;

	[XuiBindComponent("playerName", true)]
	public readonly XUiC_PlayerName PlayerName;

	[XuiBindComponent("btnReportPlayer", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnReportPlayer;
}
