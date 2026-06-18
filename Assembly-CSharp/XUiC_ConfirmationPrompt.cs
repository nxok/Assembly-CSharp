using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ConfirmationPrompt : XUiController
{
	[XuiXmlBinding("headertext")]
	public string HeaderText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "";

	[XuiXmlBinding("bodytext")]
	public string BodyText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "";

	[XuiXmlBinding("canceltext")]
	public string CancelText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "";

	[XuiXmlBinding("confirmtext")]
	public string ConfirmText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "";

	[XuiXmlBinding("cancelvisible")]
	public bool CancelVisible
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !string.IsNullOrWhiteSpace(this.CancelText);
		}
	}

	[XuiXmlBinding("confirmvisible")]
	public bool ConfirmVisible
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !string.IsNullOrWhiteSpace(this.ConfirmText);
		}
	}

	public bool IsVisible
	{
		get
		{
			return this.viewComponent.IsVisible;
		}
	}

	public override void Init()
	{
		base.Init();
		this.viewComponent.IsVisible = false;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.viewComponent.IsVisible = false;
	}

	[XuiBindEvent("OnPress", "btnConfirm")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirm_OnPress(XUiController _sender, int _mouseButton)
	{
		this.Confirm();
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPress(XUiController _sender, int _mouseButton)
	{
		this.Cancel();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (XUiUtils.HotkeysAllowedFor(this.viewComponent, false) && this.xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
		{
			ThreadManager.RunTaskAfterFrames(new Action(this.Cancel), 1);
		}
		base.handleDirtyUpdateDefault();
	}

	public void ShowPrompt(string _headerText, string _bodyText, string _cancelText, string _confirmText, Action<XUiC_ConfirmationPrompt.Result> _callback)
	{
		this.HeaderText = _headerText;
		this.BodyText = _bodyText;
		this.CancelText = _cancelText;
		this.ConfirmText = _confirmText;
		this.resultHandler = _callback;
		this.viewComponent.IsVisible = true;
		XUiView xuiView = ((!string.IsNullOrWhiteSpace(_cancelText)) ? this.btnCancel.ViewComponent : ((!string.IsNullOrWhiteSpace(_confirmText)) ? this.btnConfirm.ViewComponent : null));
		this.xui.playerUI.CursorController.SetNavigationLockView(this.viewComponent, xuiView);
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closePrompt()
	{
		this.xui.playerUI.CursorController.SetNavigationLockView(null, null);
		this.viewComponent.IsVisible = false;
	}

	public void Confirm()
	{
		if (!this.IsVisible)
		{
			return;
		}
		this.closePrompt();
		Action<XUiC_ConfirmationPrompt.Result> action = this.resultHandler;
		if (action == null)
		{
			return;
		}
		action(XUiC_ConfirmationPrompt.Result.Confirmed);
	}

	public void Cancel()
	{
		if (!this.IsVisible)
		{
			return;
		}
		this.closePrompt();
		Action<XUiC_ConfirmationPrompt.Result> action = this.resultHandler;
		if (action == null)
		{
			return;
		}
		action(XUiC_ConfirmationPrompt.Result.Cancelled);
	}

	[XuiBindComponent("btnPromptCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[XuiBindComponent("btnPromptConfirm", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnConfirm;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<XUiC_ConfirmationPrompt.Result> resultHandler;

	public enum Result
	{
		Cancelled,
		Confirmed
	}
}
