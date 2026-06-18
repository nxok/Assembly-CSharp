using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EditingToolsPoiEditor : XUiC_EditingToolsDialogBase
{
	[XuiBindEvent("OnPress", "btnStart")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnStart_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		GameUtils.StartSinglePrefabEditingWithMessage(null);
	}

	public override void Init()
	{
		base.Init();
		XUiC_EditingToolsPoiEditor.ID = this.windowGroup.Id;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (XUiUtils.HotkeysAllowedFor(this.viewComponent ?? this.children[0].ViewComponent, false) && this.xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
		{
			this.BtnStart_OnPressed(null, 0);
		}
	}

	public static string ID = "";

	[XuiBindComponent("btnStart", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnStart;
}
