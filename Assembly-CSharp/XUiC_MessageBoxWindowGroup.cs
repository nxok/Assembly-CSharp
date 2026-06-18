using System;
using System.Collections;
using System.Runtime.CompilerServices;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MessageBoxWindowGroup : XUiController
{
	[XuiXmlBinding("msgTitle")]
	public string Title
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "";

	[XuiXmlBinding("msgText")]
	public string Text
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = "";

	public XUiC_MessageBoxWindowGroup.MessageButton[] Buttons
	{
		get
		{
			return this.buttons;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_MessageBoxWindowGroup.id = base.WindowGroup.Id;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.windowGroup.isEscClosable = false;
		this.xui.playerUI.CursorController.Locked = false;
		this.xui.playerUI.CursorController.SetCursorHidden(false);
	}

	public override void OnClose()
	{
		base.OnClose();
		if (!this.openMainMenuOnClose && this.windowGroupEscCloseableBefore)
		{
			GUIWindow modalWindow = this.xui.playerUI.windowManager.GetModalWindow();
			if (modalWindow != null)
			{
				modalWindow.isEscClosable = true;
			}
		}
		if (GameManager.Instance.World == null && this.openMainMenuOnClose)
		{
			ThreadManager.StartCoroutine(PlatformApplicationManager.CheckRestartCoroutine(false));
			this.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, true);
		}
	}

	[XuiBindEvent("OnPress", "fullScreenCollider")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void FullScreenCollider_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.pressButtonOnOutsideClick < 0 || this.pressButtonOnOutsideClick >= this.buttons.Length)
		{
			return;
		}
		XUiC_MessageBoxWindowGroup.MessageButton messageButton = this.buttons[this.pressButtonOnOutsideClick];
		if (!messageButton.IsUsed || !messageButton.Enabled)
		{
			return;
		}
		messageButton.Execute();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showMessage(string _title, string _text, bool _openMainMenuOnClose = true, bool _modal = true, int _buttonOnOutsideClick = -1)
	{
		this.Text = _text;
		this.Title = _title;
		this.pressButtonOnOutsideClick = _buttonOnOutsideClick;
		this.openMainMenuOnClose = _openMainMenuOnClose;
		GUIWindow modalWindow = this.xui.playerUI.windowManager.GetModalWindow();
		if (modalWindow != null)
		{
			this.windowGroupEscCloseableBefore = modalWindow.isEscClosable;
			modalWindow.isEscClosable = false;
		}
		if (this.windowGroup.isShowing)
		{
			this.OnOpen();
			return;
		}
		this.xui.playerUI.windowManager.Open(base.WindowGroup, _modal);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openPage()
	{
		this.urlBoxData.Item1 = null;
		Func<string, bool> item = this.urlBoxData.Item3;
		if (item == null)
		{
			return;
		}
		item(this.urlBoxData.Item2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cancelOpenPage()
	{
		this.urlBoxData.Item1 = null;
	}

	public static void ShowCustom(XUi _xuiInstance, string _title, string _text, Action<XUiC_MessageBoxWindowGroup> _setupCallback, bool _openMainMenuOnClose = true, bool _modal = true, int _buttonOnOutsideClick = -1)
	{
		XUiC_MessageBoxWindowGroup xuiC_MessageBoxWindowGroup = (XUiC_MessageBoxWindowGroup)_xuiInstance.FindWindowGroupByName(XUiC_MessageBoxWindowGroup.id);
		for (int i = 0; i < xuiC_MessageBoxWindowGroup.buttons.Length; i++)
		{
			xuiC_MessageBoxWindowGroup.buttons[i].Clear();
		}
		if (_setupCallback != null)
		{
			_setupCallback(xuiC_MessageBoxWindowGroup);
		}
		xuiC_MessageBoxWindowGroup.showMessage(_title, _text, _openMainMenuOnClose, _modal, -1);
	}

	public static void ShowOk(XUi _xuiInstance, string _title, string _text, Action _onOk = null, bool _openMainMenuOnClose = true, bool _modal = true, bool _confirmOnOutsideClick = false)
	{
		XUiC_MessageBoxWindowGroup xuiC_MessageBoxWindowGroup = (XUiC_MessageBoxWindowGroup)_xuiInstance.FindWindowGroupByName(XUiC_MessageBoxWindowGroup.id);
		for (int i = 0; i < xuiC_MessageBoxWindowGroup.buttons.Length; i++)
		{
			xuiC_MessageBoxWindowGroup.buttons[i].Clear();
		}
		xuiC_MessageBoxWindowGroup.buttons[0].DefaultConfirm("xuiOk", _onOk, true, 0f, 0f, false);
		xuiC_MessageBoxWindowGroup.showMessage(_title, _text, _openMainMenuOnClose, _modal, _confirmOnOutsideClick ? 0 : (-1));
	}

	public static void ShowOkCancel(XUi _xuiInstance, string _title, string _text, Action _onOk = null, Action _onCancel = null, bool _openMainMenuOnClose = true, bool _modal = true, bool _cancelOnOutsideClick = false)
	{
		XUiC_MessageBoxWindowGroup xuiC_MessageBoxWindowGroup = (XUiC_MessageBoxWindowGroup)_xuiInstance.FindWindowGroupByName(XUiC_MessageBoxWindowGroup.id);
		for (int i = 0; i < xuiC_MessageBoxWindowGroup.buttons.Length; i++)
		{
			xuiC_MessageBoxWindowGroup.buttons[i].Clear();
		}
		xuiC_MessageBoxWindowGroup.buttons[0].DefaultConfirm("xuiOk", _onOk, true, 0f, 0f, false);
		xuiC_MessageBoxWindowGroup.buttons[2].DefaultCancel("xuiCancel", _onCancel, true, 0f, 0f, false);
		xuiC_MessageBoxWindowGroup.showMessage(_title, _text, _openMainMenuOnClose, _modal, _cancelOnOutsideClick ? 2 : (-1));
	}

	public static void ShowConfirmCancel(XUi _xuiInstance, string _title, string _text, Action _onConfirm = null, Action _onCancel = null, bool _openMainMenuOnClose = true, bool _modal = true, bool _cancelOnOutsideClick = false)
	{
		XUiC_MessageBoxWindowGroup xuiC_MessageBoxWindowGroup = (XUiC_MessageBoxWindowGroup)_xuiInstance.FindWindowGroupByName(XUiC_MessageBoxWindowGroup.id);
		for (int i = 0; i < xuiC_MessageBoxWindowGroup.buttons.Length; i++)
		{
			xuiC_MessageBoxWindowGroup.buttons[i].Clear();
		}
		xuiC_MessageBoxWindowGroup.buttons[0].DefaultConfirm("btnConfirm", _onConfirm, true, 0f, 0f, false);
		xuiC_MessageBoxWindowGroup.buttons[2].DefaultCancel("xuiCancel", _onCancel, true, 0f, 0f, false);
		xuiC_MessageBoxWindowGroup.showMessage(_title, _text, _openMainMenuOnClose, _modal, _cancelOnOutsideClick ? 2 : (-1));
	}

	public static void ShowUrlConfirmationDialog(XUi _xuiInstance, string _url, bool _modal = false, Func<string, bool> _browserOpenMethod = null, string _title = null, string _text = null, string _displayUrl = null, bool _cancelOnOutsideClick = false)
	{
		XUiC_MessageBoxWindowGroup xuiC_MessageBoxWindowGroup = (XUiC_MessageBoxWindowGroup)_xuiInstance.FindWindowGroupByName(XUiC_MessageBoxWindowGroup.id);
		if (string.IsNullOrEmpty(_url))
		{
			return;
		}
		if (!Utils.IsValidWebUrl(ref _url))
		{
			return;
		}
		if (_displayUrl == null)
		{
			_displayUrl = _url;
		}
		xuiC_MessageBoxWindowGroup.urlBoxData.Item2 = _url;
		xuiC_MessageBoxWindowGroup.urlBoxData.Item3 = _browserOpenMethod ?? ((PlatformManager.NativePlatform.Utils != null) ? new Func<string, bool>(PlatformManager.NativePlatform.Utils.OpenBrowser) : null);
		for (int i = 0; i < xuiC_MessageBoxWindowGroup.buttons.Length; i++)
		{
			xuiC_MessageBoxWindowGroup.buttons[i].Clear();
		}
		xuiC_MessageBoxWindowGroup.buttons[0].DefaultConfirm("lblContextActionOpen", new Action(xuiC_MessageBoxWindowGroup.openPage), true, 0f, 0f, false);
		xuiC_MessageBoxWindowGroup.buttons[2].DefaultCancel("xuiCancel", new Action(xuiC_MessageBoxWindowGroup.cancelOpenPage), true, 0f, 0f, false);
		if (_title == null)
		{
			_title = Localization.Get("xuiOpenUrlConfirmationTitle", false, null);
		}
		if (_text == null)
		{
			_text = Localization.Get("xuiOpenUrlConfirmationText", false, null);
		}
		_text = string.Format(_text, _displayUrl);
		bool flag = false;
		if (xuiC_MessageBoxWindowGroup.windowGroup.isShowing)
		{
			_modal = xuiC_MessageBoxWindowGroup.windowGroup.isModal;
			flag = xuiC_MessageBoxWindowGroup.openMainMenuOnClose;
		}
		xuiC_MessageBoxWindowGroup.showMessage(_title, _text, flag, _modal, _cancelOnOutsideClick ? 2 : (-1));
	}

	public static void ShowNetworkError(XUi _xuiInstance, NetworkConnectionError _error, bool _confirmOnOutsideClick = false)
	{
		string text = _error.ToStringCached<NetworkConnectionError>();
		switch (_error)
		{
		case NetworkConnectionError.InternalDirectConnectFailed:
		case NetworkConnectionError.EmptyConnectTarget:
		case NetworkConnectionError.IncorrectParameters:
		case NetworkConnectionError.AlreadyConnectedToAnotherServer:
			break;
		case NetworkConnectionError.CreateSocketOrThreadFailure:
			text = string.Format(Localization.Get("mmLblErrorSocketFailure", false, null), SingletonMonoBehaviour<ConnectionManager>.Instance.GetRequiredPortsString());
			goto IL_00D1;
		default:
			switch (_error)
			{
			case NetworkConnectionError.ConnectionFailed:
			case NetworkConnectionError.AlreadyConnectedToServer:
			case (NetworkConnectionError)17:
			case NetworkConnectionError.TooManyConnectedPlayers:
			case (NetworkConnectionError)19:
			case (NetworkConnectionError)20:
			case NetworkConnectionError.RSAPublicKeyMismatch:
			case NetworkConnectionError.ConnectionBanned:
				break;
			case NetworkConnectionError.InvalidPassword:
				text = Localization.Get("mmLblErrorWrongPassword", false, null);
				goto IL_00D1;
			default:
				switch (_error)
				{
				case NetworkConnectionError.RestartRequired:
					text = string.Format(Localization.Get("app_restartRequired", false, null), Array.Empty<object>());
					goto IL_00D1;
				}
				break;
			}
			break;
		}
		text = string.Format(Localization.Get("mmLblErrorUnknown", false, null), text);
		IL_00D1:
		XUiC_MessageBoxWindowGroup.ShowOk(_xuiInstance, Localization.Get("mmLblErrorServerInit", false, null), text, null, true, true, _confirmOnOutsideClick);
	}

	public static void Close(XUi _xuiInstance)
	{
		_xuiInstance.playerUI.windowManager.Close(XUiC_MessageBoxWindowGroup.id);
	}

	public static bool IsShowing(XUi _xuiInstance)
	{
		return _xuiInstance.playerUI.windowManager.IsWindowOpen(XUiC_MessageBoxWindowGroup.id);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string id = "";

	[XuiBindComponent("buttons.", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_MessageBoxWindowGroup.MessageButton[] buttons;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_FullScreenCollider fullScreenCollider;

	[PublicizedFrom(EAccessModifier.Private)]
	public int pressButtonOnOutsideClick = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openMainMenuOnClose;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool windowGroupEscCloseableBefore;

	[TupleElementNames(new string[] { "prevModalWindow", "url", "browserOpenMethod" })]
	[PublicizedFrom(EAccessModifier.Private)]
	public ValueTuple<GUIWindow, string, Func<string, bool>> urlBoxData;

	[Preserve]
	public class MessageButton : XUiController
	{
		[XuiXmlBinding("msg_btn_used")]
		public bool IsUsed
		{
			get
			{
				return !string.IsNullOrEmpty(this.Caption);
			}
		}

		[XuiXmlBinding("msg_btn_caption_raw")]
		public string Caption
		{
			get
			{
				return this.caption ?? "";
			}
			set
			{
				if (this.caption == value)
				{
					return;
				}
				this.caption = value;
				this.IsDirty = true;
			}
		}

		[XuiXmlBinding("msg_btn_caption")]
		public string CaptionWithSuffix
		{
			get
			{
				return this.Caption + (this.HoldToConfirm ? (" (" + Localization.Get("xuiHoldButton", false, null) + ")") : string.Empty);
			}
		}

		public PlayerAction Hotkey
		{
			get
			{
				return this.hotkey;
			}
			set
			{
				if (this.hotkey == value)
				{
					return;
				}
				this.hotkey = value;
				this.IsDirty = true;
			}
		}

		[XuiXmlBinding("msg_btn_action")]
		public string HotkeyAction
		{
			get
			{
				PlayerAction playerAction = this.hotkey;
				return ((playerAction != null) ? playerAction.Name : null) ?? "";
			}
		}

		[XuiXmlBinding("msg_btn_hashotkey")]
		public bool HasHotkey
		{
			get
			{
				return this.hotkey != null;
			}
		}

		public Action Callback
		{
			get
			{
				return this.callback;
			}
			set
			{
				this.callback = value;
			}
		}

		[XuiXmlBinding("msg_btn_enabled")]
		public bool Enabled
		{
			get
			{
				return this.enabled;
			}
			set
			{
				if (this.enabled == value)
				{
					return;
				}
				this.enabled = value;
				this.IsDirty = true;
			}
		}

		[XuiXmlBinding("msg_btn_enableddelay")]
		public float EnabledDelay
		{
			get
			{
				return this.enabledDelay;
			}
			set
			{
				if (Mathf.Approximately(this.enabledDelay, value))
				{
					return;
				}
				this.enabledDelay = value;
				this.IsDirty = true;
			}
		}

		[XuiXmlBinding("msg_btn_hold_time")]
		public float HoldTime
		{
			get
			{
				return this.holdTime;
			}
			set
			{
				if (Mathf.Approximately(this.holdTime, value))
				{
					return;
				}
				this.holdTime = value;
				this.IsDirty = true;
			}
		}

		public bool HoldToConfirm
		{
			get
			{
				return this.HoldTime > 0f;
			}
		}

		public void Clear()
		{
			this.Caption = "";
			this.Hotkey = null;
			this.Callback = null;
			this.Enabled = false;
			this.EnabledDelay = 0f;
			this.HoldTime = 0f;
		}

		public void DefaultConfirm(string _captionKey, Action _callback, bool _enabled = true, float _enabledDelay = 0f, float _holdTime = 0f, bool _holdTimeOnKbdMouse = false)
		{
			this.Set(_captionKey, this.xui.playerUI.playerInput.GUIActions.Submit, _callback, _enabled, _enabledDelay, _holdTime, _holdTimeOnKbdMouse);
		}

		public void DefaultCancel(string _captionKey, Action _callback, bool _enabled = true, float _enabledDelay = 0f, float _holdTime = 0f, bool _holdTimeOnKbdMouse = false)
		{
			this.Set(_captionKey, this.xui.playerUI.playerInput.GUIActions.Cancel, _callback, _enabled, _enabledDelay, _holdTime, _holdTimeOnKbdMouse);
		}

		public void Set(string _captionKey, PlayerAction _hotkey, Action _callback, bool _enabled = true, float _enabledDelay = 0f, float _holdTime = 0f, bool _holdTimeOnKbdMouse = false)
		{
			this.Caption = Localization.Get(_captionKey, false, null);
			this.Hotkey = _hotkey;
			this.Callback = _callback;
			this.Enabled = _enabled;
			this.EnabledDelay = _enabledDelay;
			if (!_holdTimeOnKbdMouse && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
			{
				this.HoldTime = 0f;
				return;
			}
			this.HoldTime = _holdTime;
		}

		[XuiBindEvent("OnPress", "button")]
		[PublicizedFrom(EAccessModifier.Private)]
		public void OnButtonPressed(XUiController _sender, int _mouseButton)
		{
			this.Execute();
		}

		public override void Update(float _dt)
		{
			base.Update(_dt);
			if (this.EnabledDelay > 0f)
			{
				this.EnabledDelay -= _dt;
				if (this.EnabledDelay <= 0f)
				{
					this.Enabled = true;
				}
			}
			base.handleDirtyUpdateDefault();
			if (this.hotkey == null || !this.Enabled)
			{
				return;
			}
			if (this.hotkey.WasPressed)
			{
				this.button.MouseUpDown(true);
			}
			if (this.hotkey.WasReleased)
			{
				this.button.MouseUpDown(false);
				this.button.Pressed(-1);
			}
		}

		public void Execute()
		{
			ThreadManager.StartCoroutine(this.executeCallbackLater(this.Callback));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator executeCallbackLater(Action _callback)
		{
			yield return null;
			this.xui.playerUI.windowManager.Close(this.windowGroup, false);
			if (_callback != null)
			{
				_callback();
			}
			yield break;
		}

		[XuiBindComponent(true)]
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_Button button;

		[PublicizedFrom(EAccessModifier.Private)]
		public string caption;

		[PublicizedFrom(EAccessModifier.Private)]
		public PlayerAction hotkey;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action callback;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool enabled;

		[PublicizedFrom(EAccessModifier.Private)]
		public float enabledDelay;

		[PublicizedFrom(EAccessModifier.Private)]
		public float holdTime;
	}
}
