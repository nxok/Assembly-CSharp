using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestList : XUiController
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
				XUiC_Paging xuiC_Paging = this.pager;
				if (xuiC_Paging != null)
				{
					xuiC_Paging.SetPage(this.page);
				}
				this.IsDirty = true;
			}
		}
	}

	public XUiC_QuestEntry SelectedEntry
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
			XUiC_QuestListWindow questListWindow = this.QuestListWindow;
			bool flag;
			if (this.xui.playerUI.entityPlayer.IsInParty())
			{
				XUiC_QuestEntry xuiC_QuestEntry = this.selectedEntry;
				flag = xuiC_QuestEntry != null && xuiC_QuestEntry.Quest.IsShareable;
			}
			else
			{
				flag = false;
			}
			questListWindow.ShowShareQuest(flag);
			if (this.selectedEntry != null)
			{
				this.selectedEntry.Selected = true;
				if (this.SharedList != null)
				{
					this.SharedList.SelectedEntry = null;
				}
				this.QuestListWindow.ShowRemoveQuest(this.selectedEntry.Quest.QuestClass.AllowRemove);
				return;
			}
			this.QuestListWindow.ShowRemoveQuest(true);
		}
	}

	public int VisibleEntries
	{
		get
		{
			return this.visibleEntries;
		}
		set
		{
			if (value != this.visibleEntries)
			{
				this.IsDirty = true;
				this.visibleEntries = value;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_QuestWindowGroup xuiC_QuestWindowGroup = (XUiC_QuestWindowGroup)base.WindowGroup.Controller;
		for (int i = 0; i < this.children.Count; i++)
		{
			if (this.children[i] is XUiC_QuestEntry)
			{
				XUiC_QuestEntry xuiC_QuestEntry = (XUiC_QuestEntry)this.children[i];
				xuiC_QuestEntry.QuestUIHandler = xuiC_QuestWindowGroup;
				xuiC_QuestEntry.OnScroll += this.OnScrollQuest;
				this.entryList.Add(xuiC_QuestEntry);
			}
		}
		this.pager = base.Parent.GetChildByType<XUiC_Paging>();
		if (this.pager != null)
		{
			this.pager.OnPageChanged += delegate
			{
				this.Page = this.pager.CurrentPageNumber;
			};
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			XUiC_QuestEntry xuiC_QuestEntry = this.selectedEntry;
			Quest quest = ((xuiC_QuestEntry != null) ? xuiC_QuestEntry.Quest : null);
			for (int i = 0; i < this.entryList.Count; i++)
			{
				int num = i + this.visibleEntries * this.page;
				XUiC_QuestEntry xuiC_QuestEntry2 = this.entryList[i];
				xuiC_QuestEntry2.OnPress -= this.OnPressQuest;
				xuiC_QuestEntry2.ViewComponent.IsVisible = i < this.visibleEntries;
				if (num < this.questList.Count && i < this.visibleEntries)
				{
					xuiC_QuestEntry2.Quest = this.questList[num];
					xuiC_QuestEntry2.OnPress += this.OnPressQuest;
					xuiC_QuestEntry2.ViewComponent.SoundPlayOnClick = true;
					xuiC_QuestEntry2.Selected = this.questList[num] == quest;
					if (xuiC_QuestEntry2.Selected)
					{
						this.SelectedEntry = xuiC_QuestEntry2;
					}
				}
				else
				{
					xuiC_QuestEntry2.Quest = null;
					xuiC_QuestEntry2.ViewComponent.SoundPlayOnClick = false;
					xuiC_QuestEntry2.Selected = false;
				}
			}
			XUiC_Paging xuiC_Paging = this.pager;
			if (xuiC_Paging != null)
			{
				xuiC_Paging.SetLastPageByElementsAndPageLength(this.questList.Count, this.visibleEntries);
			}
			if (this.selectedEntry != null && this.selectedEntry.Quest == null)
			{
				this.selectedEntry = null;
			}
			if (this.selectedEntry == null && this.questList.Count > 0)
			{
				this.SelectedEntry = this.entryList[0];
				((XUiC_QuestWindowGroup)base.WindowGroup.Controller).SetQuest(this.selectedEntry);
			}
			else if (this.questList.Count == 0 && this.selectedEntry == null)
			{
				this.SelectedEntry = null;
				((XUiC_QuestWindowGroup)base.WindowGroup.Controller).SetQuest(this.selectedEntry);
			}
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressQuest(XUiController _sender, int _mouseButton)
	{
		XUiC_QuestEntry xuiC_QuestEntry = _sender as XUiC_QuestEntry;
		if (xuiC_QuestEntry != null)
		{
			this.SelectedEntry = xuiC_QuestEntry;
			this.SelectedEntry.QuestUIHandler.SetQuest(this.SelectedEntry);
			if (InputUtils.ShiftKeyPressed)
			{
				Quest quest = xuiC_QuestEntry.Quest;
				if (quest.Active && !quest.Tracked)
				{
					quest.Tracked = !quest.Tracked;
					this.xui.playerUI.entityPlayer.QuestJournal.TrackedQuest = (quest.Tracked ? quest : null);
					this.xui.playerUI.entityPlayer.QuestJournal.RefreshTracked();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnScrollQuest(XUiController _sender, float _delta)
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

	public void SetQuestList(List<Quest> newQuestList)
	{
		this.Page = 0;
		this.questList = newQuestList;
		this.IsDirty = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.player = this.xui.playerUI.entityPlayer;
		this.player.QuestChanged += this.QuestJournal_QuestChanged;
		this.xui.QuestTracker.OnTrackedQuestChanged += this.QuestTracker_OnTrackedQuestChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestTracker_OnTrackedQuestChanged()
	{
		for (int i = 0; i < this.entryList.Count; i++)
		{
			this.entryList[i].IsDirty = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.player.QuestChanged -= this.QuestJournal_QuestChanged;
		this.xui.QuestTracker.OnTrackedQuestChanged -= this.QuestTracker_OnTrackedQuestChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestJournal_QuestChanged(Quest q)
	{
		if (this.selectedEntry != null && this.selectedEntry.Quest == q)
		{
			this.selectedEntry.IsDirty = true;
		}
	}

	public bool HasQuests()
	{
		return this.questList != null && this.questList.Count > 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_QuestEntry> entryList = new List<XUiC_QuestEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestEntry selectedEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Quest> questList = new List<Quest>();

	public XUiC_QuestListWindow QuestListWindow;

	public XUiC_QuestSharedList SharedList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Private)]
	public int visibleEntries;
}
