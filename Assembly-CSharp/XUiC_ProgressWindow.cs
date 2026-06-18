using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ProgressWindow : XUiController
{
	public string ProgressText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.lblProgress.Text = value;
		}
	}

	[XuiXmlBinding("use_shadow")]
	public bool UseShadow
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.useShadow;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.useShadow == value)
			{
				return;
			}
			this.IsDirty = true;
			this.useShadow = value;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_ProgressWindow.ID = base.WindowGroup.Id;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.escapeDelegate != null && this.xui.playerUI.playerInput != null && (this.xui.playerUI.playerInput.Menu.WasPressed || this.xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed))
		{
			this.escapeDelegate();
		}
		if (this.IsDirty)
		{
			base.RefreshBindings();
			this.IsDirty = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		((XUiV_Window)base.ViewComponent).ForceVisible(1f);
		this.xui.playerUI.CursorController.SetNavigationTarget(null);
		this.xui.playerUI.CursorController.Locked = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.escapeDelegate = null;
		this.xui.playerUI.CursorController.Locked = false;
	}

	public static bool IsWindowOpen()
	{
		XUiC_ProgressWindow childByType = LocalPlayerUI.primaryUI.xui.FindWindowGroupByName(XUiC_ProgressWindow.ID).GetChildByType<XUiC_ProgressWindow>();
		return childByType != null && childByType.IsOpen;
	}

	public static void Open(LocalPlayerUI _playerUi, string _text, Action _escDelegate = null, bool _modal = true, bool _notEscClosable = true, bool _useShadow = false)
	{
		if (_playerUi != null && _playerUi.xui != null)
		{
			XUiC_ProgressWindow childByType = _playerUi.xui.FindWindowGroupByName(XUiC_ProgressWindow.ID).GetChildByType<XUiC_ProgressWindow>();
			childByType.ProgressText = _text;
			_playerUi.windowManager.Open(XUiC_ProgressWindow.ID, _modal, _notEscClosable);
			childByType.escapeDelegate = _escDelegate;
			childByType.UseShadow = _useShadow;
		}
	}

	public static void Close(LocalPlayerUI _playerUi)
	{
		if (_playerUi != null && _playerUi.xui != null)
		{
			_playerUi.windowManager.Close(XUiC_ProgressWindow.ID);
		}
	}

	public static void SetText(LocalPlayerUI _playerUi, string _text, bool _clearEscDelegate = true)
	{
		if (_playerUi != null && _playerUi.xui != null)
		{
			XUiC_ProgressWindow childByType = _playerUi.xui.FindWindowGroupByName(XUiC_ProgressWindow.ID).GetChildByType<XUiC_ProgressWindow>();
			childByType.ProgressText = _text;
			if (_clearEscDelegate)
			{
				childByType.escapeDelegate = null;
			}
		}
	}

	public static void SetEscDelegate(LocalPlayerUI _playerUi, Action _escapeDelegate)
	{
		_playerUi.xui.FindWindowGroupByName(XUiC_ProgressWindow.ID).GetChildByType<XUiC_ProgressWindow>().escapeDelegate = _escapeDelegate;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useShadow;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action escapeDelegate;

	[XuiBindComponent("lblProgress", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label lblProgress;
}
