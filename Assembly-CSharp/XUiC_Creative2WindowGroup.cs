using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Creative2WindowGroup : XUiController
{
	public override void OnOpen()
	{
		base.OnOpen();
		this.xui.playerUI.windowManager.Open("windowpaging", false);
		XUiC_WindowSelector childByType = this.xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>();
		if (childByType == null)
		{
			return;
		}
		childByType.SetSelected("creative2");
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.playerUI.windowManager.Close("windowpaging");
	}
}
