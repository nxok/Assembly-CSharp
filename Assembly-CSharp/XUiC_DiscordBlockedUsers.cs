using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordBlockedUsers : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_DiscordBlockedUsers.ID = base.WindowGroup.Id;
		XUiC_Button xuiC_Button = base.GetChildById("btnBack") as XUiC_Button;
		if (xuiC_Button != null)
		{
			xuiC_Button.OnPress += delegate(XUiController _, int _)
			{
				this.xui.playerUI.windowManager.Close(this.windowGroup, false);
			};
		}
	}

	public static string ID = "";
}
