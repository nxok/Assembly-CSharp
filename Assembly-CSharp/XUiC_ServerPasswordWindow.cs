using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerPasswordWindow : XUiController
{
	[XuiXmlBinding("isinvalidpassword")]
	public bool IsInvalidPassword
	{
		get
		{
			return this.isInvalidPassword;
		}
		set
		{
			this.isInvalidPassword = value;
			this.IsDirty = true;
		}
	}

	[XuiBindEvent("OnSubmitHandler", "txtPassword")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtPassword_OnSubmitHandler(XUiController _sender, string _text)
	{
		this.BtnSubmit_OnPressed(_sender, -1);
	}

	[XuiBindEvent("OnPress", "btnSubmit")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSubmit_OnPressed(XUiController _sender, int _mouseButton)
	{
		Action<string> action = this.onSubmit;
		string text = this.txtPassword.Text;
		this.xui.playerUI.windowManager.Close(base.WindowGroup, false);
		if (action == null)
		{
			return;
		}
		action(text);
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		Action action = this.onCancel;
		this.xui.playerUI.windowManager.Close(base.WindowGroup, false);
		if (action == null)
		{
			return;
		}
		action();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
		if (XUiUtils.HotkeysAllowedFor(this.viewComponent, false))
		{
			if (this.xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
			{
				this.BtnCancel_OnPressed(this, -1);
			}
			if (this.xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
			{
				this.BtnSubmit_OnPressed(this, -1);
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.txtPassword.SetSelected(true, true);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.onCancel = null;
		this.onSubmit = null;
		this.txtPassword.Text = "";
	}

	public static void OpenPasswordWindow(XUi _xuiInstance, bool _badPassword, string _currentPwd, bool _modal, Action<string> _onSubmitDelegate, Action _onCancelDelegate)
	{
		XUiC_ServerPasswordWindow childByType = _xuiInstance.GetChildByType<XUiC_ServerPasswordWindow>();
		childByType.txtPassword.Text = _currentPwd;
		childByType.onSubmit = _onSubmitDelegate;
		childByType.onCancel = _onCancelDelegate;
		childByType.IsInvalidPassword = _badPassword;
		_xuiInstance.playerUI.windowManager.Open(childByType.windowGroup, _modal, true);
	}

	[XuiBindComponent("txtPassword", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtPassword;

	[XuiBindComponent("btnSubmit", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnSubmit;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<string> onSubmit;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInvalidPassword;
}
