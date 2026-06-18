using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LoginBase : XUiController
{
	public virtual string MsgTitleKey
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return "xuiSteamLogin";
		}
	}

	[XuiXmlBinding("title")]
	public string MsgTitle
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.offendingPlatform != null)
			{
				return string.Format(Localization.Get(this.MsgTitleKey, false, null), this.offendingPlatform.PlatformDisplayName);
			}
			return "";
		}
	}

	[XuiXmlBinding("caption")]
	public string MsgCaption
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.offendingPlatform != null)
			{
				return string.Format(Localization.Get("xuiSteamLoginFailure", false, null), this.offendingPlatform.PlatformDisplayName);
			}
			return "";
		}
	}

	[XuiXmlBinding("reason")]
	public string MsgReason
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.offendingPlatform != null)
			{
				return string.Format(Localization.Get("xuiSteamLoginReason" + this.statusReason.ToStringCached<EApiStatusReason>(), false, null), this.offendingPlatform.PlatformDisplayName, this.statusReasonAdditionalText);
			}
			return "";
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_LoginBase.ID = base.WindowGroup.Id;
	}

	[XuiBindEvent("OnPress", "btnRetry")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRetry_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.btnOffline.ViewComponent.Enabled = false;
		this.btnRetry.ViewComponent.Enabled = false;
		if (this.btnExit != null)
		{
			this.btnExit.ViewComponent.Enabled = false;
		}
		this.offendingPlatform = null;
		this.wantOffline = false;
		base.RefreshBindings();
		_sender.xui.playerUI.nguiWindowManager.SetLabelText(EnumNGUIWindow.Loading, Localization.Get("xuiSteamLoginProgressSignIn", false, null) + "...", false);
		PlatformManager.MultiPlatform.User.Login(new LoginUserCallback(this.updateState));
	}

	[XuiBindEvent("OnPress", "btnOffline")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOffline_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.btnOffline.ViewComponent.Enabled = false;
		this.btnRetry.ViewComponent.Enabled = false;
		if (this.btnExit != null)
		{
			this.btnExit.ViewComponent.Enabled = false;
		}
		this.offendingPlatform = null;
		base.RefreshBindings();
		this.wantOffline = true;
		PlatformManager.MultiPlatform.User.PlayOffline(new LoginUserCallback(this.updateState));
	}

	[XuiBindEvent("OnPress", "btnExit")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnExit_OnPressed(XUiController _sender, int _mouseButton)
	{
		Application.Quit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateState(IPlatform _platform, EApiStatusReason _statusReason, string _statusReasonAdditionalText)
	{
		if (_platform.User.UserStatus == EUserStatus.LoggedIn || (this.wantOffline && _platform.User.UserStatus == EUserStatus.OfflineMode))
		{
			this.xui.playerUI.windowManager.Close(this.windowGroup, false);
			Action action = this.onLoginComplete;
			if (action != null)
			{
				action();
			}
			this.onLoginComplete = null;
			return;
		}
		this.xui.playerUI.windowManager.Open(this.windowGroup, true, true);
		this.btnRetry.ViewComponent.Enabled = _platform.Api.ClientApiStatus != EApiStatus.PermanentError;
		this.btnOffline.ViewComponent.Enabled = _platform.User.UserStatus == EUserStatus.OfflineMode;
		if (this.btnExit != null)
		{
			this.btnExit.ViewComponent.Enabled = true;
		}
		this.offendingPlatform = _platform;
		this.statusReason = _statusReason;
		this.statusReasonAdditionalText = _statusReasonAdditionalText;
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Open(XUi _xuiInstance, IPlatform _platform, EApiStatusReason _statusReason, string _statusReasonAdditionalText, Action _onLoginComplete)
	{
		XUiC_LoginBase childByType = _xuiInstance.FindWindowGroupByName(XUiC_LoginBase.ID).GetChildByType<XUiC_LoginBase>();
		childByType.onLoginComplete = _onLoginComplete;
		childByType.updateState(_platform, _statusReason, _statusReasonAdditionalText);
	}

	public static void Login(XUi _xuiInstance, Action _onLoginComplete)
	{
		_xuiInstance.playerUI.nguiWindowManager.SetLabelText(EnumNGUIWindow.Loading, Localization.Get("xuiSteamLoginProgressSignIn", false, null) + "...", false);
		PlatformManager.MultiPlatform.User.Login(delegate(IPlatform _platform, EApiStatusReason _statusReason, string _statusReasonAdditionalText)
		{
			if (_platform.Api.ClientApiStatus != EApiStatus.Ok)
			{
				XUiC_LoginBase.Open(_xuiInstance, _platform, _statusReason, _statusReasonAdditionalText, _onLoginComplete);
				return;
			}
			if (_platform.User.UserStatus != EUserStatus.LoggedIn || _statusReason != EApiStatusReason.Ok)
			{
				XUiC_LoginBase.Open(_xuiInstance, _platform, _statusReason, _statusReasonAdditionalText, _onLoginComplete);
				return;
			}
			_onLoginComplete();
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string ID = "";

	[XuiBindComponent("btnRetry", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnRetry;

	[XuiBindComponent("btnOffline", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnOffline;

	[XuiBindComponent("btnExit", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnExit;

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatform offendingPlatform;

	[PublicizedFrom(EAccessModifier.Private)]
	public EApiStatusReason statusReason;

	[PublicizedFrom(EAccessModifier.Private)]
	public string statusReasonAdditionalText;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wantOffline;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onLoginComplete;
}
