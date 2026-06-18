using System;
using System.Collections.Generic;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WindowSelector : XUiController
{
	public event XUiC_WindowSelector.WindowSelectedDelegate WindowSelected;

	[XuiXmlAttribute("sound_on_open", false)]
	public string SoundOnOpen { get; set; }

	[XuiXmlAttribute("sound_on_close", false)]
	public string SoundOnClose { get; set; }

	public XUiC_ButtonSelectable SelectedButton
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.selectedButton;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.selectedButton != null)
			{
				this.selectedButton.IsSelected = false;
			}
			this.selectedButton = value;
			if (this.selectedButton != null)
			{
				this.selectedButton.IsSelected = true;
			}
			base.RefreshBindings();
		}
	}

	[XuiXmlAttribute("name_based_windows", false)]
	public bool NameBasedWindows
	{
		get
		{
			return this.nameBasedWindows;
		}
		set
		{
			this.nameBasedWindows = value;
		}
	}

	[XuiXmlBinding("open_window_key")]
	public string SelectedName
	{
		get
		{
			XUiC_ButtonSelectable xuiC_ButtonSelectable = this.SelectedButton;
			return ((xuiC_ButtonSelectable != null) ? xuiC_ButtonSelectable.ViewComponent.ID : null) ?? "";
		}
	}

	[XuiXmlBinding("open_window")]
	public string SelectedNameLocalized
	{
		get
		{
			if (this.SelectedButton == null)
			{
				return "";
			}
			return Localization.Get("xui" + this.SelectedButton.ViewComponent.ID, false, null);
		}
	}

	public override void Init()
	{
		base.Init();
		for (int i = 0; i < this.buttons.Length; i++)
		{
			XUiC_ButtonSelectable xuiC_ButtonSelectable = this.buttons[i];
			this.buttonsByName[xuiC_ButtonSelectable.ViewComponent.ID] = xuiC_ButtonSelectable;
		}
		if (this.buttons.Length != 0)
		{
			this.SetSelected(this.buttons[0].ViewComponent.ID);
		}
	}

	[XuiBindEvent("OnPress", "buttons")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnPress(XUiController _sender, int _mouseButton)
	{
		this.tryCloseCurrentWindow(delegate
		{
			this.SetSelected(_sender.ViewComponent.ID);
			this.OpenSelectedWindow();
		});
	}

	public void OpenSelectedWindow()
	{
		if (this.SelectedButton == null)
		{
			return;
		}
		if (!this.SelectedButton.ViewComponent.IsVisible || !this.SelectedButton.ViewComponent.Enabled)
		{
			this.toggleCategory(1);
			return;
		}
		string id = this.SelectedButton.ViewComponent.ID;
		XUiC_WindowSelector.WindowSelectedDelegate windowSelected = this.WindowSelected;
		if (windowSelected != null)
		{
			windowSelected(this, id);
		}
		if (!this.NameBasedWindows)
		{
			return;
		}
		if (!this.xui.playerUI.windowManager.IsWindowOpen(id))
		{
			this.xui.playerUI.windowManager.CloseAllOpenModalWindows(this.windowGroup, false);
			this.xui.playerUI.windowManager.Open(id, true);
		}
	}

	public void SetSelected(string _name)
	{
		XUiC_ButtonSelectable xuiC_ButtonSelectable;
		if (!this.buttonsByName.TryGetValue(_name, out xuiC_ButtonSelectable))
		{
			return;
		}
		this.SelectedButton = xuiC_ButtonSelectable;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.OpenSelectedWindow();
		if (this.xui.DragAndDropWindow != null)
		{
			this.xui.DragAndDropWindow.InMenu = true;
		}
		Manager.PlayInsidePlayerHead(this.SoundOnOpen, -1, 0f, false, false);
	}

	public override void OnClose()
	{
		base.OnClose();
		if (this.xui.DragAndDropWindow != null)
		{
			this.xui.DragAndDropWindow.InMenu = false;
		}
		Manager.PlayInsidePlayerHead(this.SoundOnClose, -1, 0f, false, false);
		if (this.xui.CurrentSelectedEntry != null)
		{
			this.xui.CurrentSelectedEntry.IsSelected = false;
		}
		if (!this.xui.playerUI.isPrimaryUI)
		{
			XUiWindowGroup xuiWindowGroup = this.xui.playerUI.windowManager.GetWindow("toolbelt") as XUiWindowGroup;
			if (xuiWindowGroup != null)
			{
				XUiC_Toolbelt childByType = xuiWindowGroup.Controller.GetChildByType<XUiC_Toolbelt>();
				if (childByType == null)
				{
					return;
				}
				childByType.ClearHoveredItems();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openSelectorAndWindow(string _selectedPage)
	{
		XUiC_FocusedBlockHealth.SetData(this.xui.playerUI, null, 0f);
		if (this.xui.playerUI.windowManager.IsWindowOpen(this.windowGroup) && this.SelectedButton.ViewComponent.ID.EqualsCaseInsensitive(_selectedPage))
		{
			this.xui.playerUI.windowManager.CloseAllOpenModalWindows(null, false);
			if (this.xui.playerUI.windowManager.IsWindowOpen(this.windowGroup))
			{
				this.xui.playerUI.windowManager.Close(this.windowGroup, false);
			}
			return;
		}
		this.SetSelected(_selectedPage);
		if (this.xui.playerUI.windowManager.IsWindowOpen(this.windowGroup))
		{
			this.OpenSelectedWindow();
			return;
		}
		this.xui.playerUI.windowManager.CloseAllOpenModalWindows(null, false);
		this.xui.playerUI.windowManager.Open(this.windowGroup, false);
	}

	public static void OpenSelectorAndWindow(EntityPlayerLocal _localPlayer, string _selectedPage)
	{
		if (_localPlayer.IsDead())
		{
			return;
		}
		LocalPlayerUI.GetUIForPlayer(_localPlayer).xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>().openSelectorAndWindow(_selectedPage);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toggleCategory(int _dir)
	{
		int num = 0;
		while (num < this.buttons.Length && this.buttons[num] != this.SelectedButton)
		{
			num++;
		}
		if (num >= this.buttons.Length)
		{
			num = -1;
		}
		XUiC_ButtonSelectable xuiC_ButtonSelectable;
		do
		{
			num = NGUIMath.RepeatIndex(num + _dir, this.buttons.Length);
			xuiC_ButtonSelectable = this.buttons[num];
		}
		while (!xuiC_ButtonSelectable.ViewComponent.IsVisible || !xuiC_ButtonSelectable.ViewComponent.Enabled);
		this.SetSelected(this.buttons[num].ViewComponent.ID);
		this.buttons[num].ViewComponent.PlayClickSound();
		this.OpenSelectedWindow();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (XUiUtils.HotkeysAllowedFor(this.viewComponent, false))
		{
			PlayerActionsGUI guiactions = this.xui.playerUI.playerInput.GUIActions;
			if (guiactions.WindowPagingLeft.WasReleased)
			{
				ThreadManager.RunTaskAfterFrames(delegate
				{
					this.tryCloseCurrentWindow(delegate
					{
						this.toggleCategory(-1);
					});
				}, 1);
			}
			if (guiactions.WindowPagingRight.WasReleased)
			{
				ThreadManager.RunTaskAfterFrames(delegate
				{
					this.tryCloseCurrentWindow(delegate
					{
						this.toggleCategory(1);
					});
				}, 1);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void tryCloseCurrentWindow(Action _onDone)
	{
		XUiWindowGroup xuiWindowGroup = this.xui.playerUI.windowManager.GetModalWindow() as XUiWindowGroup;
		if (xuiWindowGroup == null)
		{
			this.xui.playerUI.windowManager.Close(this.xui.playerUI.windowManager.GetModalWindow(), false);
			_onDone();
			return;
		}
		IXUiWindowConditionalClosing ixuiWindowConditionalClosing = xuiWindowGroup.Controller as IXUiWindowConditionalClosing;
		if (ixuiWindowConditionalClosing != null)
		{
			ixuiWindowConditionalClosing.TryClose(_onDone, null);
			return;
		}
		foreach (XUiV_Window xuiV_Window in xuiWindowGroup.Windows)
		{
			IXUiWindowConditionalClosing ixuiWindowConditionalClosing2 = xuiV_Window.Controller as IXUiWindowConditionalClosing;
			if (ixuiWindowConditionalClosing2 != null)
			{
				ixuiWindowConditionalClosing2.TryClose(_onDone, null);
				return;
			}
		}
		_onDone();
	}

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ButtonSelectable[] buttons;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, XUiC_ButtonSelectable> buttonsByName = new CaseInsensitiveStringDictionary<XUiC_ButtonSelectable>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ButtonSelectable selectedButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool nameBasedWindows = true;

	public delegate void WindowSelectedDelegate(XUiC_WindowSelector _sender, string _windowId);
}
