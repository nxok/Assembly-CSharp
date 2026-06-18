using System;
using System.Collections.Generic;
using InControl;
using Platform;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUIWindowManager : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		this.nguiWindowManager = base.GetComponent<NGUIWindowManager>();
		this.playerUI = base.GetComponent<LocalPlayerUI>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnGUI()
	{
		for (int i = 0; i < this.windowsToOpen.Count; i++)
		{
			GUIWindow guiwindow = this.windowsToOpen[i];
			guiwindow.isShowing = true;
			this.openWindows.Add(guiwindow);
			this.topmostWindow = guiwindow;
		}
		this.windowsToOpen.Clear();
		this.modalWindow = null;
		for (int j = 0; j < this.openWindows.Count; j++)
		{
			GUIWindow guiwindow2 = this.openWindows[j];
			if (guiwindow2.isModal)
			{
				this.modalWindow = guiwindow2;
				break;
			}
		}
		this.cursorWindowOpen = false;
		for (int k = 0; k < this.openWindows.Count; k++)
		{
			GUIWindow guiwindow3 = this.openWindows[k];
			if (guiwindow3.isShowing)
			{
				this.cursorWindowOpen |= guiwindow3.alwaysUsesMouseCursor;
				GUI.matrix = Matrix4x4.identity;
				guiwindow3.OnGUI();
			}
		}
		GUI.enabled = true;
		List<GUIWindow> list = this.windowsToRemove;
		list.Clear();
		for (int l = 0; l < this.openWindows.Count; l++)
		{
			GUIWindow guiwindow4 = this.openWindows[l];
			if (!guiwindow4.isShowing)
			{
				list.Add(guiwindow4);
				this.topmostWindow = ((this.openWindows.Count > 0) ? this.openWindows[this.openWindows.Count - 1] : null);
			}
		}
		for (int m = 0; m < list.Count; m++)
		{
			GUIWindow guiwindow5 = list[m];
			this.openWindows.Remove(guiwindow5);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		for (int i = 0; i < this.openWindows.Count; i++)
		{
			this.openWindows[i].Update();
		}
		if (this.lastActionClicked.Count != 0)
		{
			for (int j = 0; j < this.lastActionClicked.Count; j++)
			{
				NGuiAction nguiAction = this.lastActionClicked[j];
				PlayerAction hotkey = nguiAction.GetHotkey();
				if (hotkey != null && hotkey.WasReleased)
				{
					nguiAction.OnRelease();
					this.actionsToClear.Add(nguiAction);
				}
			}
			for (int k = 0; k < this.actionsToClear.Count; k++)
			{
				NGuiAction nguiAction2 = this.actionsToClear[k];
				this.lastActionClicked.Remove(nguiAction2);
			}
			this.actionsToClear.Clear();
		}
		if (this.IsInputActive())
		{
			if (this.playerUI.playerInput != null && this.playerUI.playerInput.PermanentActions.Cancel.WasPressed && UIInput.selection != null)
			{
				UIInput.selection.RemoveFocus();
			}
			return;
		}
		if (!this.IsInputLocked)
		{
			List<NGuiAction> list = this.actionsForGlobalHotkeys;
			list.Clear();
			for (int l = 0; l < this.globalActions.Count; l++)
			{
				PlayerAction hotkey2 = this.globalActions[l].GetHotkey();
				if (hotkey2 != null && (((this.globalActions[l].KeyMode & NGuiAction.EnumKeyMode.FireOnRelease) == NGuiAction.EnumKeyMode.FireOnRelease && hotkey2.WasReleased) || ((this.globalActions[l].KeyMode & NGuiAction.EnumKeyMode.FireOnPress) == NGuiAction.EnumKeyMode.FireOnPress && hotkey2.WasPressed) || ((this.globalActions[l].KeyMode & NGuiAction.EnumKeyMode.FireOnRepeat) == NGuiAction.EnumKeyMode.FireOnRepeat && hotkey2.WasRepeated)))
				{
					list.Add(this.globalActions[l]);
				}
			}
			if (list.Count > 0)
			{
				for (int m = 0; m < list.Count; m++)
				{
					NGuiAction nguiAction3 = list[m];
					nguiAction3.OnClick();
					this.lastActionClicked.Add(nguiAction3);
				}
				list.Clear();
			}
			if (this.playerUI.playerInput != null && this.playerUI.playerInput.PermanentActions.Cancel.WasPressed && !this.IsWindowOpen("popupGroup") && this.modalWindow != null && this.modalWindow.isEscClosable)
			{
				this.CloseAllOpenModalWindows(null, true);
			}
		}
	}

	public bool IsInputActive()
	{
		return (UIInput.selection != null && UIInput.selection.gameObject.activeInHierarchy) || (this.topmostWindow != null && this.topmostWindow.isInputActive) || this.IsUGUIInputActive() || GUIWindowConsole.IsOpen();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsUGUIInputActive()
	{
		EventSystem current = EventSystem.current;
		if (current == null)
		{
			return false;
		}
		GameObject currentSelectedGameObject = current.currentSelectedGameObject;
		if (currentSelectedGameObject == null)
		{
			return false;
		}
		InputField inputField;
		if (currentSelectedGameObject.TryGetComponent<InputField>(ref inputField))
		{
			return inputField.isFocused;
		}
		TMP_InputField tmp_InputField;
		return currentSelectedGameObject.TryGetComponent<TMP_InputField>(ref tmp_InputField) && tmp_InputField.isFocused;
	}

	public bool IsKeyShortcutsAllowed()
	{
		return !this.IsInputLocked && (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard || !this.IsInputActive());
	}

	public void Add(GUIWindow _window)
	{
		this.nameToWindowMap.Add(_window.Id, _window);
		_window.windowManager = this;
		if (this.nguiWindowManager == null)
		{
			this.nguiWindowManager = base.GetComponent<NGUIWindowManager>();
		}
		if (this.playerUI == null)
		{
			this.playerUI = base.GetComponent<LocalPlayerUI>();
		}
		_window.playerUI = this.playerUI;
	}

	public void Remove(GUIWindow _w)
	{
		if (_w.isShowing)
		{
			this.Close(_w, false);
		}
		_w.Cleanup();
		this.nameToWindowMap.Remove(_w.Id);
	}

	public GUIWindow GetWindow(string _windowName)
	{
		GUIWindow guiwindow;
		if (!this.nameToWindowMap.TryGetValue(_windowName, out guiwindow))
		{
			Log.Warning("GUIWindowManager.GetWindow: Window \"{0}\" unknown!", new object[] { _windowName });
			return null;
		}
		return guiwindow;
	}

	public bool TryGetWindow(string _windowName, out GUIWindow _window)
	{
		return this.nameToWindowMap.TryGetValue(_windowName, out _window);
	}

	public T GetWindow<T>(string _windowName) where T : GUIWindow
	{
		return (T)((object)this.nameToWindowMap[_windowName]);
	}

	public void SwitchVisible(string _windowName, bool _modal = true)
	{
		GUIWindow guiwindow;
		if (this.nameToWindowMap.TryGetValue(_windowName, out guiwindow))
		{
			this.SwitchVisible(guiwindow, _modal);
		}
	}

	public void SwitchVisible(GUIWindow _guiWindow, bool _modal = true)
	{
		if ((!_modal || _guiWindow.isModal) && _guiWindow.isShowing)
		{
			this.Close(_guiWindow, false);
			return;
		}
		this.Open(_guiWindow, _modal);
	}

	public bool CloseAllOpenModalWindows(string _exceptWindowName)
	{
		GUIWindow guiwindow;
		this.nameToWindowMap.TryGetValue(_exceptWindowName, out guiwindow);
		return this.CloseAllOpenModalWindows(guiwindow, false);
	}

	public bool CloseAllOpenModalWindows(GUIWindow _exceptWindow = null, bool _fromEsc = false)
	{
		bool flag = false;
		for (int i = 0; i < this.openWindows.Count; i++)
		{
			GUIWindow guiwindow = this.openWindows[i];
			if (guiwindow.isModal && (_exceptWindow == null || _exceptWindow != guiwindow))
			{
				this.Close(guiwindow, _fromEsc);
				flag = true;
			}
		}
		if (this.playerUI.CursorController != null)
		{
			if (this.playerUI.CursorController.navigationTarget != null)
			{
				this.playerUI.CursorController.navigationTarget.Controller.Hovered(false);
			}
			this.playerUI.CursorController.SetNavigationTarget(null);
			this.playerUI.CursorController.SetNavigationLockView(null, null);
		}
		return flag;
	}

	public void Open(string _windowName, bool _bModal)
	{
		GUIWindow guiwindow;
		if (!this.nameToWindowMap.TryGetValue(_windowName, out guiwindow))
		{
			Log.Warning("GUIWindowManager.Open: Window \"{0}\" unknown!", new object[] { _windowName });
			Log.Out("Trace: " + StackTraceUtility.ExtractStackTrace());
			return;
		}
		this.openInternal(guiwindow, _bModal, false);
	}

	public void Open(string _windowName, bool _bModal, bool _bIsNotEscClosable)
	{
		GUIWindow guiwindow;
		if (!this.nameToWindowMap.TryGetValue(_windowName, out guiwindow))
		{
			Log.Warning("GUIWindowManager.Open: Window \"{0}\" unknown!", new object[] { _windowName });
			Log.Out("Trace: " + StackTraceUtility.ExtractStackTrace());
			return;
		}
		this.openInternal(guiwindow, _bModal, _bIsNotEscClosable);
	}

	public void Open(GUIWindow _w, bool _bModal)
	{
		this.openInternal(_w, _bModal, false);
	}

	public void Open(GUIWindow _w, bool _bModal, bool _bIsNotEscClosable)
	{
		this.openInternal(_w, _bModal, _bIsNotEscClosable);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openInternal(GUIWindow _w, bool _bModal, bool _bIsNotEscClosable)
	{
		if (_w == null)
		{
			return;
		}
		if (this.IsFullHUDDisabled())
		{
			return;
		}
		if (this.IsWindowOpen(_w))
		{
			return;
		}
		QuestEventManager.Current.ChangedWindow(_w.Id);
		if (_bModal)
		{
			this.CloseAllOpenModalWindows(null, false);
			int i = 0;
			while (i < this.windowsToOpen.Count)
			{
				GUIWindow guiwindow = this.windowsToOpen[i];
				if (guiwindow.isModal && guiwindow != _w)
				{
					this.windowsToOpen.Remove(guiwindow);
					guiwindow.OnClose();
					if (guiwindow.HasActionSet())
					{
						this.DisableWindowActionSet(guiwindow);
						break;
					}
					break;
				}
				else
				{
					i++;
				}
			}
		}
		_w.isModal = _bModal;
		if (_w.isShowing)
		{
			return;
		}
		_w.windowManager = this;
		_w.isEscClosable = !_bIsNotEscClosable;
		bool flag = _w.isShowing || this.windowsToOpen.Contains(_w);
		if (!this.openWindows.Contains(_w))
		{
			this.windowsToOpen.Add(_w);
		}
		else
		{
			_w.isShowing = true;
		}
		if (!flag)
		{
			_w.OnOpen();
		}
		if (_w.HasActionSet() && !_w.bActionSetEnabled && (_w.isShowing || this.windowsToOpen.Contains(_w)))
		{
			this.EnableWindowActionSet(_w);
		}
	}

	public bool IsWindowOpen(string _wdwID)
	{
		GUIWindow guiwindow;
		return this.nameToWindowMap.TryGetValue(_wdwID, out guiwindow) && this.IsWindowOpen(guiwindow);
	}

	public bool IsWindowOpen(GUIWindow _window)
	{
		return _window != null && (_window.isShowing || this.windowsToOpen.Contains(_window));
	}

	public bool IsModalWindowOpen()
	{
		return this.modalWindow != null;
	}

	public GUIWindow GetModalWindow()
	{
		return this.modalWindow;
	}

	public bool IsCursorWindowOpen()
	{
		return this.cursorWindowOpen;
	}

	public void Close(string _windowName)
	{
		GUIWindow guiwindow;
		if (this.nameToWindowMap.TryGetValue(_windowName, out guiwindow))
		{
			this.Close(guiwindow, false);
		}
	}

	public void Close(GUIWindow _w, bool _fromEsc = false)
	{
		if (_w == null)
		{
			return;
		}
		if (_w.isShowing)
		{
			_w.isShowing = false;
			_w.OnClose();
			if (_fromEsc && !string.IsNullOrEmpty(_w.openWindowOnEsc))
			{
				this.Open(_w.openWindowOnEsc, _w.isModal);
			}
		}
		else if (this.windowsToOpen.Contains(_w))
		{
			this.windowsToOpen.Remove(_w);
			_w.OnClose();
		}
		if (_w.bActionSetEnabled)
		{
			this.DisableWindowActionSet(_w);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DisableWindowActionSet(GUIWindow _w)
	{
		if (_w.playerUI != null && _w.playerUI.ActionSetManager != null)
		{
			_w.playerUI.ActionSetManager.Pop(_w);
			_w.bActionSetEnabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnableWindowActionSet(GUIWindow _w)
	{
		if (_w.playerUI != null && _w.playerUI.ActionSetManager != null)
		{
			_w.playerUI.ActionSetManager.Push(_w);
			_w.bActionSetEnabled = true;
		}
	}

	public void ResetActionSets()
	{
		ActionSetManager actionSetManager = this.playerUI.ActionSetManager;
		if (actionSetManager == null)
		{
			return;
		}
		actionSetManager.Reset();
		actionSetManager.Push(this.playerUI.playerInput);
		foreach (GUIWindow guiwindow in this.openWindows)
		{
			if (guiwindow.isShowing && guiwindow.HasActionSet())
			{
				this.EnableWindowActionSet(guiwindow);
			}
		}
		foreach (GUIWindow guiwindow2 in this.windowsToOpen)
		{
			if (guiwindow2.bActionSetEnabled)
			{
				this.EnableWindowActionSet(guiwindow2);
			}
		}
	}

	public void RemoveGlobalAction(NGuiAction _action)
	{
		this.globalActions.Remove(_action);
	}

	public void AddGlobalAction(NGuiAction _action)
	{
		this.globalActions.Add(_action);
	}

	public bool IsHUDEnabled()
	{
		return this.bHUDEnabled == GUIWindowManager.HudEnabledStates.Enabled;
	}

	public bool IsHUDPartialHidden()
	{
		return this.bHUDEnabled == GUIWindowManager.HudEnabledStates.PartialHide;
	}

	public bool IsFullHUDDisabled()
	{
		return this.bHUDEnabled == GUIWindowManager.HudEnabledStates.FullHide;
	}

	public void ToggleHUDEnabled()
	{
		this.bHUDEnabled = this.bHUDEnabled.CycleEnum(false, true);
		this.SetHUDEnabled(this.bHUDEnabled);
	}

	public void TempHUDDisable()
	{
		this.bTempEnabled = this.bHUDEnabled;
		this.bHUDEnabled = GUIWindowManager.HudEnabledStates.FullHide;
		this.SetHUDEnabled(this.bHUDEnabled);
	}

	public void ReEnableHUD()
	{
		this.bHUDEnabled = this.bTempEnabled;
		this.SetHUDEnabled(this.bHUDEnabled);
	}

	public void SetHUDEnabled(GUIWindowManager.HudEnabledStates _hudState)
	{
		this.bHUDEnabled = _hudState;
		if (_hudState <= GUIWindowManager.HudEnabledStates.PartialHide)
		{
			this.nguiWindowManager.ShowAll(true);
			this.playerUI.xui.transform.gameObject.SetActive(true);
			return;
		}
		if (_hudState != GUIWindowManager.HudEnabledStates.FullHide)
		{
			return;
		}
		this.nguiWindowManager.ShowAll(false);
		this.playerUI.xui.transform.gameObject.SetActive(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<GUIWindow> windowsToOpen = new List<GUIWindow>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<GUIWindow> windowsToRemove = new List<GUIWindow>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<GUIWindow> openWindows = new List<GUIWindow>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly Dictionary<string, GUIWindow> nameToWindowMap = new CaseInsensitiveStringDictionary<GUIWindow>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GUIWindow topmostWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GUIWindow modalWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool cursorWindowOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<NGuiAction> globalActions = new List<NGuiAction>();

	public bool IsInputLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public NGUIWindowManager nguiWindowManager;

	public LocalPlayerUI playerUI;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<NGuiAction> lastActionClicked = new List<NGuiAction>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<NGuiAction> actionsToClear = new List<NGuiAction>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly List<NGuiAction> actionsForGlobalHotkeys = new List<NGuiAction>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GUIWindowManager.HudEnabledStates bHUDEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GUIWindowManager.HudEnabledStates bTempEnabled;

	public enum HudEnabledStates
	{
		Enabled,
		PartialHide,
		FullHide
	}
}
