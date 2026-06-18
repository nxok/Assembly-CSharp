using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabEditorHelp : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_PrefabEditorHelp.ID = base.WindowGroup.Id;
		base.GetChildById("outclick").OnPress += this.Close_OnPress;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Close_OnPress(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	public static string ID = "";
}
