using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EditingTools : XUiController
{
	[XuiBindEvent("WindowSelected", "parentSelector")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void onWindowSelected(XUiC_WindowSelector _sender, string _windowId)
	{
		GUIWindowManager windowManager = this.xui.playerUI.windowManager;
		if (!(_windowId == "rwgPreviewer"))
		{
			if (_windowId == "poiEditor")
			{
				windowManager.Open(XUiC_EditingToolsPoiEditor.ID, true);
				return;
			}
			if (!(_windowId == "worldEditor"))
			{
				return;
			}
			windowManager.Open(XUiC_WorldEditor.ID, true);
		}
		else if (!XUiC_WorldGenerationWindow.IsWindowOpen(this.xui))
		{
			XUiC_WorldGenerationWindow.Open(this.xui, XUiC_MainMenu.ID);
			return;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_EditingTools.ID = base.WindowGroup.Id;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.RefreshBindings();
	}

	public static string ID = "";

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_WindowSelector parentSelector;
}
