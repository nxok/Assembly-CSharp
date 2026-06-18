using System;
using Audio;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_KeypadWindow : XUiController
{
	public override void Init()
	{
		XUiC_KeypadWindow.ID = this.windowGroup.Id;
		base.Init();
		this.txtPassword = (XUiC_TextInput)base.GetChildById("txtPassword");
		this.txtPassword.OnSubmitHandler += this.TxtPassword_OnSubmitHandler;
		XUiC_SimpleButton xuiC_SimpleButton = (XUiC_SimpleButton)base.GetChildById("btnCancel");
		xuiC_SimpleButton.OnPressed += this.BtnCancel_OnPressed;
		xuiC_SimpleButton.ViewComponent.NavUpTarget = this.txtPassword.ViewComponent;
		XUiC_SimpleButton xuiC_SimpleButton2 = (XUiC_SimpleButton)base.GetChildById("btnOk");
		xuiC_SimpleButton2.OnPressed += this.BtnOk_OnPressed;
		xuiC_SimpleButton2.ViewComponent.NavUpTarget = this.txtPassword.ViewComponent;
		this.txtPassword.ViewComponent.NavDownTarget = xuiC_SimpleButton.ViewComponent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TextInput_OnInputAbortedHandler(XUiController _sender)
	{
		this.xui.playerUI.windowManager.Close(base.WindowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtPassword_OnSubmitHandler(XUiController _sender, string _text)
	{
		this.BtnOk_OnPressed(_sender, -1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOk_OnPressed(XUiController _sender, int _mouseButton)
	{
		string text = this.txtPassword.Text;
		string hashForPassword = this.LockedItem.GetHashForPassword(text);
		if (this.LockedItem.IsOwner(PlatformManager.InternalLocalUserIdentifier) && hashForPassword != this.LockedItem.GetPasswordHash())
		{
			this.LockedItem.SetPasswordHash(hashForPassword, PlatformManager.InternalLocalUserIdentifier);
			if (text.Length == 0)
			{
				GameManager.ShowTooltip(this.xui.playerUI.entityPlayer, "passcodeRemoved", false, false, 0f);
			}
			else
			{
				GameManager.ShowTooltip(this.xui.playerUI.entityPlayer, "passcodeSet", false, false, 0f);
			}
		}
		if (this.LockedItem.CheckPasswordHash(hashForPassword, PlatformManager.InternalLocalUserIdentifier))
		{
			if (this.LockedItem.LocalPlayerIsOwner())
			{
				Manager.PlayInsidePlayerHead("Misc/password_set", -1, 0f, false, false);
			}
			else
			{
				GameManager.ShowTooltip(this.xui.playerUI.entityPlayer, "passcodeAccepted", false, false, 0f);
				Manager.PlayInsidePlayerHead("Misc/password_pass", -1, 0f, false, false);
			}
			this.xui.playerUI.windowManager.Close(base.WindowGroup, false);
			return;
		}
		Manager.PlayInsidePlayerHead("Misc/password_fail", -1, 0f, false, false);
		GameManager.ShowTooltip(this.xui.playerUI.entityPlayer, "passcodeRejected", false, false, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(base.WindowGroup, false);
	}

	public static void Open(LocalPlayerUI _playerUi, ILockable _lockedItem, Action _onClose = null, Func<bool> _interactionCheck = null)
	{
		XUiC_KeypadWindow childByType = _playerUi.xui.FindWindowGroupByName(XUiC_KeypadWindow.ID).GetChildByType<XUiC_KeypadWindow>();
		childByType.LockedItem = _lockedItem;
		childByType.onCloseCallback = _onClose;
		childByType.checkInteraction = _interactionCheck;
		_playerUi.windowManager.Open(XUiC_KeypadWindow.ID, true);
	}

	public override void Update(float _dt)
	{
		if (this.checkInteraction != null && !this.checkInteraction())
		{
			this.xui.playerUI.windowManager.Close(XUiC_KeypadWindow.ID);
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.xui.playerUI.entityPlayer.PlayOneShot("open_sign", false, false, false, null, 1f);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.playerUI.entityPlayer.PlayOneShot("close_sign", false, false, false, null, 1f);
		Action action = this.onCloseCallback;
		if (action != null)
		{
			action();
		}
		this.LockedItem = null;
		this.onCloseCallback = null;
		this.checkInteraction = null;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtPassword;

	[PublicizedFrom(EAccessModifier.Private)]
	public ILockable LockedItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onCloseCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<bool> checkInteraction;
}
