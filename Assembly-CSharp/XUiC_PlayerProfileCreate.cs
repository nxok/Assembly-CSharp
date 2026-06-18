using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayerProfileCreate : XUiController
{
	[XuiXmlBinding("create_name_valid")]
	public bool CreateNameValid
	{
		get
		{
			return this.createNameValid;
		}
		set
		{
			if (this.createNameValid == value)
			{
				return;
			}
			this.createNameValid = value;
			this.IsDirty = true;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.createProfileName.Text = "";
		this.createProfileName.SelectOrVirtualKeyboard(false);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
		if (this.xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
		{
			this.BtnCancelCreate_OnPressed(this, -1);
		}
		if (this.xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
		{
			this.BtnConfirmCreate_OnPressed(this, -1);
		}
	}

	[XuiBindEvent("OnPress", "createProfileCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancelCreate_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		Action<string> action = this.onClosed;
		if (action == null)
		{
			return;
		}
		action(null);
	}

	[XuiBindEvent("OnPress", "createProfileConfirm")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirmCreate_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.CreateNameValid)
		{
			return;
		}
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		Action<string> action = this.onClosed;
		if (action == null)
		{
			return;
		}
		action(this.createProfileName.Text.Trim());
	}

	[XuiBindEvent("OnSubmitHandler", "createProfileName")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateProfileName_OnSubmitHandler(XUiController _sender, string _text)
	{
		if (!this.CreateNameValid)
		{
			return;
		}
		this.BtnConfirmCreate_OnPressed(this, -1);
	}

	[XuiBindEvent("OnChangeHandler", "createProfileName")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateProfileName_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		string text = _text.Trim();
		this.CreateNameValid = text.Length > 0 && text.IndexOf('.') < 0 && !ProfileSDF.ProfileExists(text);
	}

	public static void Open(XUi _xui, Action<string> _onClosed)
	{
		XUiC_PlayerProfileCreate childByType = _xui.GetChildByType<XUiC_PlayerProfileCreate>();
		childByType.onClosed = _onClosed;
		_xui.playerUI.windowManager.Open(childByType.windowGroup, false);
	}

	[XuiBindComponent("createProfileName", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput createProfileName;

	[XuiBindComponent("btnConfirm", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button createProfileConfirm;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button createProfileCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool createNameValid;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<string> onClosed;
}
