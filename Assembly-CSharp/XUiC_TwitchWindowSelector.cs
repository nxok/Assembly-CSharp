using System;
using System.Collections.Generic;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchWindowSelector : XUiController
{
	public new XUiV_Button Selected
	{
		get
		{
			return this.selected;
		}
		set
		{
			if (this.selected != null)
			{
				this.selected.Selected = false;
			}
			this.selected = value;
			if (this.selected != null)
			{
				this.selected.Selected = true;
				this.HandleSelectedChange();
			}
		}
	}

	public string SelectedName
	{
		get
		{
			if (this.selected == null)
			{
				return "";
			}
			return this.selected.ID;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_TwitchWindowSelector.ID = base.WindowGroup.Id;
		XUiController childById = base.GetChildById("lblWindowName");
		if (childById != null)
		{
			this.lblWindowName = (XUiV_Label)childById.ViewComponent;
		}
		this.categories.Clear();
		for (int i = 0; i < this.children.Count; i++)
		{
			XUiController xuiController = this.children[i];
			if (xuiController.ViewComponent.EventOnPress)
			{
				xuiController.OnPress += this.HandleOnPress;
				this.categories.Add(xuiController.ViewComponent.ID.ToLower());
			}
		}
		this.SetSelected("actions");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnPress(XUiController _sender, int _mouseButton)
	{
		this.Selected = (XUiV_Button)_sender.ViewComponent;
		this.OpenSelectedWindow();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleSelectedChange()
	{
		this.updateWindowTitle();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateWindowTitle()
	{
		this.lblWindowName.Text = ((this.selected != null) ? Localization.Get("TwitchInfo_" + this.Selected.ID, false, null) : "");
	}

	public void OpenSelectedWindow()
	{
		if (this.Selected != null)
		{
			this.updateWindowTitle();
			string id = this.Selected.ID;
			GUIWindowManager windowManager = this.xui.playerUI.windowManager;
			if (id == "Actions")
			{
				this.xui.FindWindowGroupByName("twitchInfo").GetChildByType<XUiC_TwitchEntryListWindow>().SetOpenToActions(XUiC_TwitchWindowSelector.extras);
				XUiC_TwitchWindowSelector.extras = false;
				windowManager.Open("twitchInfo", true);
				return;
			}
			if (id == "Votes")
			{
				this.xui.FindWindowGroupByName("twitchInfo").GetChildByType<XUiC_TwitchEntryListWindow>().SetOpenToVotes();
				windowManager.Open("twitchInfo", true);
				return;
			}
			if (id == "ActionHistory")
			{
				this.xui.FindWindowGroupByName("twitchInfo").GetChildByType<XUiC_TwitchEntryListWindow>().SetOpenToHistory();
				windowManager.Open("twitchInfo", true);
				return;
			}
			if (!(id == "Leaderboard"))
			{
				return;
			}
			this.xui.FindWindowGroupByName("twitchInfo").GetChildByType<XUiC_TwitchEntryListWindow>().SetOpenToLeaderboard();
			windowManager.Open("twitchInfo", true);
		}
	}

	public void SetSelected(string name)
	{
		XUiController childById = base.GetChildById(name.ToLower());
		if (((childById != null) ? childById.ViewComponent : null) is XUiV_Button)
		{
			this.Selected = (XUiV_Button)childById.ViewComponent;
			this.currentCategoryIndex = this.categories.IndexOf(this.Selected.ID.ToLower());
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.OpenSelectedWindow();
		this.xui.DragAndDropWindow.InMenu = true;
		Manager.PlayInsidePlayerHead("open_inventory", -1, 0f, false, false);
		base.RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.DragAndDropWindow.InMenu = false;
		Manager.PlayInsidePlayerHead("close_inventory", -1, 0f, false, false);
		if (this.xui.CurrentSelectedEntry != null)
		{
			this.xui.CurrentSelectedEntry.IsSelected = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openSelectorAndWindow(string _selectedPage)
	{
		_selectedPage = _selectedPage.ToLower();
		XUiC_FocusedBlockHealth.SetData(this.xui.playerUI, null, 0f);
		if (this.xui.playerUI.windowManager.IsWindowOpen("twitchWindowpaging") && this.SelectedName.EqualsCaseInsensitive(_selectedPage) && !this.OverrideClose)
		{
			this.xui.playerUI.windowManager.CloseAllOpenModalWindows(null, false);
			if (this.xui.playerUI.windowManager.IsWindowOpen("twitchWindowpaging"))
			{
				this.xui.playerUI.windowManager.Close("twitchWindowpaging");
				return;
			}
		}
		else
		{
			this.SetSelected(_selectedPage);
			if (this.xui.playerUI.windowManager.IsWindowOpen("twitchWindowpaging"))
			{
				this.OpenSelectedWindow();
				return;
			}
			this.xui.playerUI.windowManager.CloseAllOpenModalWindows(null, false);
			this.xui.playerUI.windowManager.Open("twitchWindowpaging", false);
		}
	}

	public static void OpenSelectorAndWindow(EntityPlayerLocal _localPlayer, string _selectedPage, bool _extras = false)
	{
		if (_localPlayer.IsDead())
		{
			return;
		}
		XUiC_TwitchWindowSelector.extras = _extras;
		LocalPlayerUI.GetUIForPlayer(_localPlayer).xui.FindWindowGroupByName("twitchWindowpaging").GetChildByType<XUiC_TwitchWindowSelector>().openSelectorAndWindow(_selectedPage);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toggleCategory(int _dir)
	{
		int num = NGUIMath.RepeatIndex(this.currentCategoryIndex + _dir, this.categories.Count);
		XUiController childById = base.GetChildById(this.categories[num]);
		if (((childById != null) ? childById.ViewComponent : null) is XUiV_Button)
		{
			if (childById.ViewComponent.IsVisible)
			{
				this.SetSelected(this.categories[num]);
				this.OpenSelectedWindow();
				return;
			}
			this.currentCategoryIndex = num;
			this.toggleCategory(_dir);
		}
	}

	public static void ToggleCategory(EntityPlayerLocal _localPlayer, int _dir)
	{
		LocalPlayerUI.GetUIForPlayer(_localPlayer).xui.FindWindowGroupByName("twitchWindowpaging").GetChildByType<XUiC_TwitchWindowSelector>().toggleCategory(_dir);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		GUIWindowManager windowManager = this.xui.playerUI.windowManager;
		if (this.xui.playerUI.windowManager.IsKeyShortcutsAllowed())
		{
			if (this.xui.playerUI.playerInput.GUIActions.WindowPagingLeft.WasReleased && windowManager.IsWindowOpen(XUiC_TwitchWindowSelector.ID))
			{
				XUiC_TwitchWindowSelector.ToggleCategory(this.xui.playerUI.entityPlayer, -1);
			}
			if (this.xui.playerUI.playerInput.GUIActions.WindowPagingRight.WasReleased && windowManager.IsWindowOpen(XUiC_TwitchWindowSelector.ID))
			{
				XUiC_TwitchWindowSelector.ToggleCategory(this.xui.playerUI.entityPlayer, 1);
			}
		}
		this.OverrideClose = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblWindowName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button selected;

	public static string ID = "";

	public bool OverrideClose;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> categories = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentCategoryIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool extras = false;
}
