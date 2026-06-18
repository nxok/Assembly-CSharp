using System;
using System.Collections.Generic;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchLeaderboardEntryList : XUiController
{
	public int Page
	{
		get
		{
			return this.page;
		}
		set
		{
			if (this.page != value)
			{
				this.page = value;
				this.IsDirty = true;
				XUiC_Paging xuiC_Paging = this.pager;
				if (xuiC_Paging == null)
				{
					return;
				}
				xuiC_Paging.SetPage(this.page);
			}
		}
	}

	public XUiC_TwitchLeaderboardEntry SelectedEntry
	{
		get
		{
			return this.selectedEntry;
		}
		set
		{
			if (this.selectedEntry != null)
			{
				this.selectedEntry.Selected = false;
			}
			this.selectedEntry = value;
			if (this.selectedEntry != null)
			{
				this.selectedEntry.Selected = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetFirstEntry()
	{
		if (this.entryList[0].LeaderboardEntry != null)
		{
			this.SelectedEntry = this.entryList[0];
			this.entryList[0].SelectCursorElement(true, false);
		}
		else
		{
			this.SelectedEntry = null;
			base.WindowGroup.Controller.GetChildById("searchControls").SelectCursorElement(true, false);
		}
		((XUiC_TwitchInfoWindowGroup)base.WindowGroup.Controller).ClearEntries();
	}

	public override void Init()
	{
		base.Init();
		XUiC_TwitchInfoWindowGroup xuiC_TwitchInfoWindowGroup = (XUiC_TwitchInfoWindowGroup)base.WindowGroup.Controller;
		XUiController childById = xuiC_TwitchInfoWindowGroup.GetChildByType<XUiC_TwitchHowToWindow>().GetChildById("leftButton");
		for (int i = 0; i < this.children.Count; i++)
		{
			if (this.children[i] is XUiC_TwitchLeaderboardEntry)
			{
				XUiC_TwitchLeaderboardEntry xuiC_TwitchLeaderboardEntry = (XUiC_TwitchLeaderboardEntry)this.children[i];
				xuiC_TwitchLeaderboardEntry.Owner = this;
				xuiC_TwitchLeaderboardEntry.TwitchInfoUIHandler = xuiC_TwitchInfoWindowGroup;
				xuiC_TwitchLeaderboardEntry.OnScroll += this.OnScrollEntry;
				xuiC_TwitchLeaderboardEntry.ViewComponent.NavRightTarget = childById.ViewComponent;
				this.entryList.Add(xuiC_TwitchLeaderboardEntry);
			}
		}
		this.pager = base.Parent.GetChildByType<XUiC_Paging>();
		if (this.pager != null)
		{
			this.pager.OnPageChanged += delegate
			{
				if (this.viewComponent.IsVisible)
				{
					this.Page = this.pager.CurrentPageNumber;
				}
			};
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			if (this.entryList != null)
			{
				for (int i = 0; i < this.entryList.Count; i++)
				{
					int num = i + this.entryList.Count * this.page;
					XUiC_TwitchLeaderboardEntry xuiC_TwitchLeaderboardEntry = this.entryList[i];
					if (xuiC_TwitchLeaderboardEntry != null)
					{
						xuiC_TwitchLeaderboardEntry.OnPress -= this.OnPressEntry;
						xuiC_TwitchLeaderboardEntry.Selected = false;
						xuiC_TwitchLeaderboardEntry.ViewComponent.SoundPlayOnClick = false;
						if (num < this.leaderboardList.Count)
						{
							xuiC_TwitchLeaderboardEntry.LeaderboardEntry = this.leaderboardList[num];
							xuiC_TwitchLeaderboardEntry.ViewComponent.IsNavigatable = true;
						}
						else
						{
							xuiC_TwitchLeaderboardEntry.LeaderboardEntry = null;
							xuiC_TwitchLeaderboardEntry.ViewComponent.IsNavigatable = false;
						}
					}
				}
				XUiC_Paging xuiC_Paging = this.pager;
				if (xuiC_Paging != null)
				{
					xuiC_Paging.SetLastPageByElementsAndPageLength(this.leaderboardList.Count, this.entryList.Count);
				}
			}
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressEntry(XUiController _sender, int _mouseButton)
	{
		XUiC_TwitchLeaderboardEntry xuiC_TwitchLeaderboardEntry = _sender as XUiC_TwitchLeaderboardEntry;
		if (xuiC_TwitchLeaderboardEntry != null)
		{
			this.SelectedEntry = xuiC_TwitchLeaderboardEntry;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnScrollEntry(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			XUiC_Paging xuiC_Paging = this.pager;
			if (xuiC_Paging == null)
			{
				return;
			}
			xuiC_Paging.PageDown();
			return;
		}
		else
		{
			XUiC_Paging xuiC_Paging2 = this.pager;
			if (xuiC_Paging2 == null)
			{
				return;
			}
			xuiC_Paging2.PageUp();
			return;
		}
	}

	public void SetTwitchLeaderboardList(List<TwitchLeaderboardEntry> newLeaderboardList)
	{
		this.Page = 0;
		this.leaderboardList = newLeaderboardList;
		this.IsDirty = true;
		((XUiC_TwitchInfoWindowGroup)base.WindowGroup.Controller).ClearEntries();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.player = this.xui.playerUI.entityPlayer;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.SelectedEntry = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_TwitchLeaderboardEntry> entryList = new List<XUiC_TwitchLeaderboardEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchLeaderboardEntry selectedEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchLeaderboardEntry> leaderboardList = new List<TwitchLeaderboardEntry>();

	public XUiC_TwitchEntryListWindow TwitchEntryListWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;
}
