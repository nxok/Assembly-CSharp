using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsBlockedPlayersList : XUiC_OptionsDialogBase
{
	public override void Init()
	{
		base.Init();
		XUiC_OptionsBlockedPlayersList.ID = base.WindowGroup.Id;
	}

	public override void OnClose()
	{
		base.OnClose();
		BlockedPlayerList instance = BlockedPlayerList.Instance;
		if (instance == null)
		{
			return;
		}
		instance.MarkForWrite();
	}

	public override bool SupportsDefaults
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return false;
		}
	}

	public static string ID = "";

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_BlockedPlayersList blockedPlayersList;
}
