using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DialogWindowGroup : XUiController
{
	public Dialog CurrentDialog
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static void Open(XUi _xui, Action _onDialogClosed = null)
	{
		_xui.playerUI.windowManager.Open("dialog", true);
		if (_onDialogClosed != null)
		{
			GUIWindow window = _xui.playerUI.windowManager.GetWindow("dialog");
			window.OnWindowClose = (Action)Delegate.Combine(window.OnWindowClose, _onDialogClosed);
		}
	}

	public override void Init()
	{
		base.Init();
		this.responseWindow = base.GetChildByType<XUiC_DialogResponseList>();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.xui.Dialog.DialogWindowGroup = this;
		this.xui.playerUI.entityPlayer.OverrideFOV = 30f;
		this.xui.playerUI.entityPlayer.OverrideLookAt = this.xui.Dialog.Respondent.getHeadPosition();
		this.xui.playerUI.windowManager.Close("windowpaging");
		this.xui.playerUI.windowManager.Close("toolbelt");
		this.CurrentDialog = Dialog.DialogList[this.xui.Dialog.Respondent.NPCInfo.DialogID];
		this.CurrentDialog.CurrentOwner = this.xui.Dialog.Respondent;
		if (this.xui.Dialog.ReturnStatement == "" || this.CurrentDialog.CurrentStatement == null)
		{
			this.CurrentDialog.RestartDialog(this.xui.playerUI.entityPlayer);
		}
		else if (this.xui.Dialog.ReturnStatement != "")
		{
			this.CurrentDialog.CurrentStatement = this.CurrentDialog.GetStatement(this.xui.Dialog.ReturnStatement);
			this.CurrentDialog.ChildDialog = null;
		}
		this.xui.Dialog.ReturnStatement = "";
		this.responseWindow.CurrentDialog = this.CurrentDialog;
		GameManager.Instance.SetToolTipPause(this.xui.playerUI.nguiWindowManager, true);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.playerUI.windowManager.Close("questOffer");
		this.xui.playerUI.windowManager.Open("toolbelt", false);
		this.xui.Dialog.Respondent = null;
		GameManager.Instance.SetToolTipPause(this.xui.playerUI.nguiWindowManager, false);
		if (!this.xui.Dialog.KeepZoomOnClose)
		{
			this.xui.playerUI.entityPlayer.OverrideFOV = -1f;
		}
	}

	public void RefreshDialog()
	{
		if (this.CurrentDialog.CurrentStatement != null)
		{
			this.responseWindow.Refresh();
			return;
		}
		this.xui.playerUI.windowManager.Close("dialog");
	}

	public void ShowResponseWindow(bool isVisible)
	{
		this.responseWindow.ViewComponent.IsVisible = isVisible;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_DialogResponseList responseWindow;
}
