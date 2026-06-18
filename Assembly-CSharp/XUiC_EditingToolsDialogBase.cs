using System;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_EditingToolsDialogBase : XUiController
{
	[XuiBindEvent("OnPress", "btnBack")]
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Event_BackOnPress(XUiController _sender, int _mouseButton)
	{
		this.close();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.windowGroup.isEscClosable = false;
		this.xui.playerUI.windowManager.Open(XUiC_EditingTools.ID, false);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.playerUI.windowManager.Close(XUiC_EditingTools.ID);
		this.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, true);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.handleDirtyUpdateDefault())
		{
			this.onDirtyUpdate();
		}
		if (XUiUtils.HotkeysAllowedFor(this.viewComponent ?? this.children[0].ViewComponent, false) && this.xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
		{
			this.close();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void onDirtyUpdate()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void close()
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_EditingToolsDialogBase()
	{
	}

	[XuiBindComponent("btnBack", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnBack;
}
