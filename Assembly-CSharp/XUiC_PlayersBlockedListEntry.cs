using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayersBlockedListEntry : XUiC_PlayersBlockedListEntryBase
{
	[XuiBindEvent("OnPress", "btnUnblockPlayer")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void unblockPlayerPressed(XUiController _sender, int _mouseButton)
	{
		if (base.PlayerId == null)
		{
			return;
		}
		BlockedPlayerList.ListEntry playerStateInfo = BlockedPlayerList.Instance.GetPlayerStateInfo(base.PlayerId);
		if (playerStateInfo == null || !playerStateInfo.ResolvedOnce)
		{
			return;
		}
		playerStateInfo.SetBlockState(false);
		this.BlockList.IsDirty = true;
	}

	[XuiBindComponent("unblockBtn", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnUnblockPlayer;
}
