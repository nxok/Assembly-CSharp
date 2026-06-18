using System;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DiscordLogin : XUiController
{
	[XuiXmlBinding("status_text")]
	public string StatusText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.statusText ?? "";
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.statusText = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("show_settings_button")]
	public bool ShowSettingsButton
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.showSettingsButton;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.showSettingsButton = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("done")]
	public bool IsDone
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.isDone;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.isDone = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("cancellable")]
	public bool Cancellable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.cancellable;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.cancellable = value;
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_DiscordLogin.ID = base.WindowGroup.Id;
	}

	[XuiBindEvent("OnPress", "btnOk")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnOk_OnPress(XUiController _sender, int _mouseButton)
	{
		this.closeAndOpenNextWindow();
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnCancel_OnPress(XUiController _sender, int _mouseButton)
	{
		DiscordManager.Instance.AuthManager.AbortAuth();
	}

	[XuiBindEvent("OnPress", "btnSettings")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnSettings_OnPress(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		XUiC_OptionsAudio childByType = this.xui.GetChildByType<XUiC_OptionsAudio>();
		if (childByType == null)
		{
			return;
		}
		childByType.OpenAtTab("xuiOptionsAudioDiscord");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeAndOpenNextWindow()
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		if (this.onCloseWithOk == null)
		{
			this.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, true);
			return;
		}
		this.onCloseWithOk();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.xui.playerUI.nguiWindowManager.Show(EnumNGUIWindow.Loading, false);
		this.windowGroup.isEscClosable = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		DiscordManager.Instance.UserAuthorizationResult -= this.authResult;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.updateInput();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateInput()
	{
		if (!this.IsDone)
		{
			return;
		}
		PlayerActionsGUI guiactions = this.xui.playerUI.playerInput.GUIActions;
		if (guiactions.Apply.WasReleased || guiactions.Cancel.WasReleased)
		{
			this.closeAndOpenNextWindow();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void authResult(bool _isDone, DiscordManager.EFullAccountLoginResult _fullAccResult, DiscordManager.EProvisionalAccountLoginResult _provisionalAccResult, bool _isExpectedSuccess)
	{
		LocalPlayerUI playerUI = this.xui.playerUI;
		playerUI.nguiWindowManager.Show(EnumNGUIWindow.Loading, false);
		if (!this.windowGroup.isShowing && (_isDone || _fullAccResult == DiscordManager.EFullAccountLoginResult.RequestingAuth))
		{
			if (this.skipOnSuccess && _isExpectedSuccess)
			{
				DiscordManager.Instance.UserAuthorizationResult -= this.authResult;
				this.closeAndOpenNextWindow();
				return;
			}
			XUiC_DiscordLogin.openNow(playerUI, this.openModal);
		}
		this.updateUi(_isDone, _fullAccResult, _provisionalAccResult, _isExpectedSuccess);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateUi(bool _isDone, DiscordManager.EFullAccountLoginResult _fullAccResult, DiscordManager.EProvisionalAccountLoginResult _provisionalAccResult, bool _isExpectedSuccess)
	{
		XUiC_DiscordLogin.<>c__DisplayClass33_0 CS$<>8__locals1;
		CS$<>8__locals1._isDone = _isDone;
		CS$<>8__locals1._fullAccResult = _fullAccResult;
		CS$<>8__locals1._provisionalAccResult = _provisionalAccResult;
		this.IsDone = CS$<>8__locals1._isDone;
		this.Cancellable = CS$<>8__locals1._fullAccResult == DiscordManager.EFullAccountLoginResult.RequestingAuth;
		this.StatusText = XUiC_DiscordLogin.<updateUi>g__BuildText|33_0(ref CS$<>8__locals1);
		base.RefreshBindings();
	}

	public static void Open(Action _onCloseWithOk = null, bool _showSettingsButton = true, bool _waitForResultToShow = false, bool _skipOnSuccess = false, bool _modal = true, bool _cancellable = false)
	{
		XUi xui = LocalPlayerUI.primaryUI.xui;
		LocalPlayerUI playerUI = xui.playerUI;
		XUiC_DiscordLogin childByType = xui.GetWindowGroupById(XUiC_DiscordLogin.ID).Controller.GetChildByType<XUiC_DiscordLogin>();
		DiscordManager.Instance.UserAuthorizationResult += childByType.authResult;
		childByType.StatusText = Localization.Get("xuiDiscordLoginLoggingIn", false, null);
		childByType.IsDone = false;
		childByType.Cancellable = _cancellable;
		childByType.onCloseWithOk = _onCloseWithOk;
		childByType.ShowSettingsButton = _showSettingsButton;
		childByType.skipOnSuccess = _skipOnSuccess;
		childByType.openModal = _modal;
		if (_waitForResultToShow)
		{
			playerUI.nguiWindowManager.SetLabelText(EnumNGUIWindow.Loading, Localization.Get("xuiDiscordLoginProgress", false, null) + "...", false);
			return;
		}
		XUiC_DiscordLogin.openNow(playerUI, _modal);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void openNow(LocalPlayerUI _playerUi, bool _modal = true)
	{
		_playerUi.windowManager.Open(XUiC_DiscordLogin.ID, _modal, true);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static string <updateUi>g__BuildText|33_0(ref XUiC_DiscordLogin.<>c__DisplayClass33_0 A_0)
	{
		if (!A_0._isDone)
		{
			switch (A_0._fullAccResult)
			{
			case DiscordManager.EFullAccountLoginResult.RequestingAuth:
				return Localization.Get("xuiDiscordLoginFullAccountRequestingAuth", false, null);
			case DiscordManager.EFullAccountLoginResult.AuthAccepted:
				return Localization.Get("xuiDiscordLoginFullAccountAuthAccepted", false, null);
			case DiscordManager.EFullAccountLoginResult.AuthCancelled:
				return Localization.Get("xuiDiscordLoginFullAccountAuthCancelledTryingProvisional", false, null);
			case DiscordManager.EFullAccountLoginResult.AuthFailed:
				return Localization.Get("xuiDiscordLoginFullAccountAuthFailedTryingProvisional", false, null);
			}
		}
		if (A_0._fullAccResult == DiscordManager.EFullAccountLoginResult.None)
		{
			return XUiC_DiscordLogin.<updateUi>g__BuildProvisionalOnlyText|33_3(ref A_0);
		}
		if (A_0._fullAccResult == DiscordManager.EFullAccountLoginResult.Success)
		{
			return Localization.Get("xuiDiscordLoginFullAccountSuccess", false, null);
		}
		return XUiC_DiscordLogin.<updateUi>g__BuildFullAccFailureText|33_1(ref A_0);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static string <updateUi>g__BuildFullAccFailureText|33_1(ref XUiC_DiscordLogin.<>c__DisplayClass33_0 A_0)
	{
		string text = Localization.Get("xuiDiscordLoginFullAccount" + A_0._fullAccResult.ToStringCached<DiscordManager.EFullAccountLoginResult>(), false, null);
		if (A_0._provisionalAccResult == DiscordManager.EProvisionalAccountLoginResult.None || A_0._provisionalAccResult == DiscordManager.EProvisionalAccountLoginResult.NotSupported)
		{
			return text;
		}
		return XUiC_DiscordLogin.<updateUi>g__BuildProvisionalFallbackText|33_2(text, ref A_0);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static string <updateUi>g__BuildProvisionalFallbackText|33_2(string _fullAccFailureText, ref XUiC_DiscordLogin.<>c__DisplayClass33_0 A_1)
	{
		string text = Localization.Get("xuiDiscordLoginFallback" + A_1._provisionalAccResult.ToStringCached<DiscordManager.EProvisionalAccountLoginResult>(), false, null);
		return _fullAccFailureText + "\n\n" + text;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static string <updateUi>g__BuildProvisionalOnlyText|33_3(ref XUiC_DiscordLogin.<>c__DisplayClass33_0 A_0)
	{
		return Localization.Get("xuiDiscordLoginProvisional" + A_0._provisionalAccResult.ToStringCached<DiscordManager.EProvisionalAccountLoginResult>(), false, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string ID = "";

	[XuiBindComponent("btnOk", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnOk;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[XuiBindComponent("btnSettings", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnSettings;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onCloseWithOk;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showSettingsButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool skipOnSuccess;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openModal;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDone;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cancellable;

	[PublicizedFrom(EAccessModifier.Private)]
	public string statusText;
}
