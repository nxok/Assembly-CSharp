using System;
using System.Collections.Generic;
using Platform;

public class XUiWindowGroup : GUIWindow
{
	public bool Initialized
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public XUiWindowGroup(XUi _xui, string _id, XUiWindowGroup.EHasActionSetFor _hasActionSetFor, string _defaultSelectedName, int _stackPanelYOffset, int _stackPanelPadding, bool _openBackpackOnOpen, bool _closeCompassOnOpen)
		: base(_id)
	{
		this.Windows = this.windows.AsReadOnly();
		this.xui = _xui;
		this.playerUI = this.xui.playerUI;
		this.windowManager = this.playerUI.windowManager;
		this.hasActionSetFor = _hasActionSetFor;
		this.defaultSelectedView = _defaultSelectedName;
		if (_stackPanelYOffset != -2147483648)
		{
			this.StackPanelYOffset = _stackPanelYOffset;
		}
		if (_stackPanelPadding != -2147483648)
		{
			this.StackPanelPadding = _stackPanelPadding;
		}
		this.openBackpackOnOpen = _openBackpackOnOpen;
		this.closeCompassOnOpen = _closeCompassOnOpen;
	}

	public void Init()
	{
		if (this.Initialized)
		{
			return;
		}
		this.Controller.AutoBindComponents();
		this.Controller.Init();
		this.Controller.AutoBindEvents();
		foreach (XUiController xuiController in this.Controller.Children)
		{
			if (xuiController.ViewComponent != null)
			{
				this.windows.Add((XUiV_Window)xuiController.ViewComponent);
				xuiController.ViewComponent.IsVisible = false;
			}
		}
		this.windowManager.Add(this);
		this.Initialized = true;
	}

	public override void Cleanup()
	{
		if (this.cleanedUp)
		{
			return;
		}
		this.cleanedUp = true;
		CursorControllerAbs cursorController = this.xui.playerUI.CursorController;
		if (cursorController.navigationTarget != null && cursorController.navigationTarget.Controller.IsChildOf(this.Controller))
		{
			cursorController.SetNavigationTarget(null);
		}
		this.Controller.Cleanup();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.Controller.OnOpen();
		this.applyDefaultSelectedView();
		if (this.closeCompassOnOpen)
		{
			this.windowManager.Close("compass");
		}
		if (this.openBackpackOnOpen && GameManager.Instance != null)
		{
			this.windowManager.Open("backpack", false);
		}
		this.xui.RecenterWindowGroup(this, false);
		bool flag;
		switch (this.hasActionSetFor)
		{
		case XUiWindowGroup.EHasActionSetFor.Both:
			flag = true;
			break;
		case XUiWindowGroup.EHasActionSetFor.OnlyController:
			flag = PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard;
			break;
		case XUiWindowGroup.EHasActionSetFor.OnlyKeyboard:
			flag = PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard;
			break;
		case XUiWindowGroup.EHasActionSetFor.None:
			flag = false;
			break;
		default:
			flag = this.hasActionSetThisOpen;
			break;
		}
		this.hasActionSetThisOpen = flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyDefaultSelectedView()
	{
		if (string.IsNullOrEmpty(this.defaultSelectedView))
		{
			return;
		}
		if (this.defaultSelectedView.StartsWith("bp."))
		{
			XUiC_BackpackWindow.defaultSelectedElement = this.defaultSelectedView.Remove(0, 3);
			return;
		}
		XUiController childById = this.Controller.GetChildById(this.defaultSelectedView);
		if (childById != null)
		{
			childById.SelectCursorElement(true, false);
			return;
		}
		Log.Warning("Could not find selectable element " + this.defaultSelectedView + " in WindowGroup " + this.Id);
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiC_DragAndDropWindow dragAndDropWindow = this.xui.DragAndDropWindow;
		if (dragAndDropWindow != null)
		{
			dragAndDropWindow.PlaceItemBackInInventory();
		}
		this.Controller.OnClose();
		if (this.openBackpackOnOpen && GameManager.Instance != null)
		{
			this.windowManager.Close("backpack");
		}
		if (this.xui.ToolTipWindow != null)
		{
			this.xui.ToolTipWindow.ToolTip = "";
		}
		XUiC_PopupMenu popupMenuWindow = this.xui.PopupMenuWindow;
		if (popupMenuWindow == null)
		{
			return;
		}
		popupMenuWindow.Close();
	}

	public override bool HasActionSet()
	{
		return this.hasActionSetThisOpen;
	}

	public bool HasStackPanelWindows()
	{
		if (this.Controller == null)
		{
			return false;
		}
		using (List<XUiV_Window>.Enumerator enumerator = this.windows.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.IsInStackPanel)
				{
					return true;
				}
			}
		}
		return false;
	}

	public readonly XUi xui;

	public XUiController Controller;

	public readonly bool LeftPanelVAlignTop = true;

	public readonly bool RightPanelVAlignTop = true;

	public readonly int StackPanelYOffset = 457;

	public readonly int StackPanelPadding = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool openBackpackOnOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool closeCompassOnOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string defaultSelectedView;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiWindowGroup.EHasActionSetFor hasActionSetFor;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasActionSetThisOpen = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiV_Window> windows = new List<XUiV_Window>();

	public readonly IReadOnlyList<XUiV_Window> Windows;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cleanedUp;

	public enum EHasActionSetFor
	{
		Both,
		OnlyController,
		OnlyKeyboard,
		None
	}
}
