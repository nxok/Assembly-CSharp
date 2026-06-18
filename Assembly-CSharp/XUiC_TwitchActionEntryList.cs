using System;
using System.Collections.Generic;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchActionEntryList : XUiController
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
				this.setFirstEntry = true;
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

	public XUiC_TwitchActionEntry SelectedEntry
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
		if (this.entryList[0].Action != null)
		{
			this.SelectedEntry = this.entryList[0];
			this.entryList[0].SelectCursorElement(true, false);
		}
		else
		{
			this.SelectedEntry = null;
			base.WindowGroup.Controller.GetChildById("searchControls").SelectCursorElement(true, false);
		}
		((XUiC_TwitchInfoWindowGroup)base.WindowGroup.Controller).SetEntry(this.selectedEntry);
	}

	public override void Init()
	{
		base.Init();
		XUiC_TwitchInfoWindowGroup xuiC_TwitchInfoWindowGroup = (XUiC_TwitchInfoWindowGroup)base.WindowGroup.Controller;
		XUiController childById = xuiC_TwitchInfoWindowGroup.GetChildByType<XUiC_TwitchEntryDescriptionWindow>().GetChildById("btnDecrease");
		for (int i = 0; i < this.children.Count; i++)
		{
			if (this.children[i] is XUiC_TwitchActionEntry)
			{
				XUiC_TwitchActionEntry xuiC_TwitchActionEntry = (XUiC_TwitchActionEntry)this.children[i];
				xuiC_TwitchActionEntry.Owner = this;
				xuiC_TwitchActionEntry.TwitchInfoUIHandler = xuiC_TwitchInfoWindowGroup;
				xuiC_TwitchActionEntry.OnScroll += this.OnScrollEntry;
				xuiC_TwitchActionEntry.ViewComponent.NavRightTarget = childById.ViewComponent;
				this.entryList.Add(xuiC_TwitchActionEntry);
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
					XUiC_TwitchActionEntry xuiC_TwitchActionEntry = this.entryList[i];
					if (xuiC_TwitchActionEntry != null)
					{
						xuiC_TwitchActionEntry.OnPress -= this.OnPressEntry;
						xuiC_TwitchActionEntry.Selected = false;
						if (num < this.actionList.Count)
						{
							xuiC_TwitchActionEntry.Action = this.actionList[num];
							xuiC_TwitchActionEntry.OnPress += this.OnPressEntry;
							xuiC_TwitchActionEntry.ViewComponent.SoundPlayOnClick = true;
							xuiC_TwitchActionEntry.ViewComponent.IsNavigatable = true;
						}
						else
						{
							xuiC_TwitchActionEntry.Action = null;
							xuiC_TwitchActionEntry.ViewComponent.SoundPlayOnClick = false;
							xuiC_TwitchActionEntry.ViewComponent.IsNavigatable = false;
						}
					}
				}
				XUiC_Paging xuiC_Paging = this.pager;
				if (xuiC_Paging != null)
				{
					xuiC_Paging.SetLastPageByElementsAndPageLength(this.actionList.Count, this.entryList.Count);
				}
			}
			if (this.setFirstEntry)
			{
				this.SetFirstEntry();
				this.setFirstEntry = false;
			}
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressEntry(XUiController _sender, int _mouseButton)
	{
		XUiC_TwitchActionEntry xuiC_TwitchActionEntry = _sender as XUiC_TwitchActionEntry;
		if (xuiC_TwitchActionEntry != null)
		{
			this.SelectedEntry = xuiC_TwitchActionEntry;
			this.SelectedEntry.TwitchInfoUIHandler.SetEntry(this.SelectedEntry);
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

	public void SetTwitchActionList(List<TwitchAction> newActionEntryList, TwitchActionPreset currentPreset)
	{
		this.CurrentPreset = currentPreset;
		this.Page = 0;
		this.actionList = newActionEntryList;
		this.setFirstEntry = true;
		this.IsDirty = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.setFirstEntry = true;
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
	public List<XUiC_TwitchActionEntry> entryList = new List<XUiC_TwitchActionEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchActionEntry selectedEntry;

	public bool setFirstEntry = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchAction> actionList = new List<TwitchAction>();

	public XUiC_TwitchEntryListWindow TwitchEntryListWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	public TwitchActionPreset CurrentPreset;
}
