using System;
using System.Collections.Generic;
using System.Globalization;
using UniLinq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestListWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		this.questList = base.GetChildByType<XUiC_QuestList>();
		this.questList.QuestListWindow = this;
		this.trackBtn = (XUiV_Button)base.GetChildById("trackBtn").ViewComponent;
		this.trackBtn.Controller.OnPress += this.trackBtn_OnPress;
		this.showOnMapBtn = (XUiV_Button)base.GetChildById("showOnMapBtn").ViewComponent;
		this.showOnMapBtn.Controller.OnPress += this.showOnMapBtn_OnPress;
		this.questRemoveBtn = (XUiV_Button)base.GetChildById("questRemoveBtn").ViewComponent;
		this.questRemoveBtn.Controller.OnPress += this.questRemoveBtn_OnPress;
		this.questShareBtn = (XUiV_Button)base.GetChildById("questShareBtn").ViewComponent;
		this.questShareBtn.Controller.OnPress += this.questShareBtn_OnPress;
		this.buttonSpacing = this.showOnMapBtn.Position.x - this.trackBtn.Position.x;
		this.txtInput = (XUiC_TextInput)base.GetChildById("searchInput");
		if (this.txtInput != null)
		{
			this.txtInput.OnChangeHandler += this.HandleOnChangedHandler;
			this.txtInput.Text = "";
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void questShareBtn_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_QuestEntry selectedEntry = this.questList.SelectedEntry;
		Quest quest = ((selectedEntry != null) ? selectedEntry.Quest : null);
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		PartyQuests.ShareQuestWithParty(quest, entityPlayer, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showOnMapBtn_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.questList.SelectedEntry != null)
		{
			Quest quest = this.questList.SelectedEntry.Quest;
			if (quest.HasPosition)
			{
				XUiC_WindowSelector.OpenSelectorAndWindow(this.xui.playerUI.entityPlayer, "map");
				((XUiC_MapArea)this.xui.GetWindow("mapArea").Controller).PositionMapAt(quest.Position);
				return;
			}
			GameManager.ShowTooltip(this.xui.playerUI.entityPlayer, Localization.Get("ttQuestNoLocation", false, null), false, false, 0f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void trackBtn_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiM_Quest questTracker = this.xui.QuestTracker;
		if (this.questList.SelectedEntry != null)
		{
			Quest quest = this.questList.SelectedEntry.Quest;
			if (quest.Active)
			{
				quest.Tracked = !quest.Tracked;
				this.xui.playerUI.entityPlayer.QuestJournal.TrackedQuest = (quest.Tracked ? quest : null);
				this.xui.playerUI.entityPlayer.QuestJournal.RefreshTracked();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnChangedHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.filterText = _text;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void questRemoveBtn_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.questList.SelectedEntry != null)
		{
			this.xui.playerUI.entityPlayer.QuestJournal.RemoveQuest(this.questList.SelectedEntry.Quest);
			this.questList.SelectedEntry = null;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.filterText = this.filterText.ToLower();
			this.currentItems = (from quest in this.player.QuestJournal.quests
				where this.filterText == "" || QuestClass.GetQuest(quest.ID).Name.ToLower().Contains(this.filterText)
				orderby !quest.Active, quest.FinishTime descending, QuestClass.GetQuest(quest.ID).Name
				select quest).ToList<Quest>();
			this.questList.SetQuestList(this.currentItems);
			this.IsDirty = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.player = this.xui.playerUI.entityPlayer;
		this.player.QuestAccepted += this.QuestJournal_QuestAccepted;
		this.player.QuestRemoved += this.QuestJournal_QuestRemoved;
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.player.QuestAccepted -= this.QuestJournal_QuestAccepted;
		this.player.QuestRemoved -= this.QuestJournal_QuestRemoved;
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		if (_name == "visible_quest_count")
		{
			this.questList.VisibleEntries = StringParsers.ParseSInt32(_value, 0, -1, NumberStyles.Integer);
			return true;
		}
		return base.ParseAttribute(_name, _value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestJournal_QuestAccepted(Quest q)
	{
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestJournal_QuestRemoved(Quest q)
	{
		this.IsDirty = true;
		if (q == this.xui.QuestTracker.TrackedQuest)
		{
			this.xui.QuestTracker.TrackedQuest = null;
		}
	}

	public void ShowTrackButton(bool _show)
	{
		this.trackBtn.IsVisible = _show;
		if (this.showingTrackButton != _show)
		{
			this.showingTrackButton = _show;
			this.trackBtn.Enabled = _show;
			Vector3 localPosition = this.showOnMapBtn.UiTransform.localPosition;
			Vector3 localPosition2 = this.questRemoveBtn.UiTransform.localPosition;
			if (_show)
			{
				this.showOnMapBtn.UiTransform.localPosition = new Vector3(localPosition.x + (float)this.buttonSpacing, localPosition.y, localPosition.z);
				this.questRemoveBtn.UiTransform.localPosition = new Vector3(localPosition2.x + (float)this.buttonSpacing, localPosition2.y, localPosition2.z);
				return;
			}
			this.showOnMapBtn.UiTransform.localPosition = new Vector3(localPosition.x - (float)this.buttonSpacing, localPosition.y, localPosition.z);
			this.questRemoveBtn.UiTransform.localPosition = new Vector3(localPosition2.x - (float)this.buttonSpacing, localPosition2.y, localPosition2.z);
		}
	}

	public void ShowShareQuest(bool _show)
	{
		this.questShareBtn.IsVisible = _show && !PartyQuests.AutoShare;
	}

	public void ShowRemoveQuest(bool _show)
	{
		this.questRemoveBtn.IsVisible = _show;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestListWindow.SearchTypes searchType;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestList questList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button trackBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button questRemoveBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button showOnMapBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button questShareBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Quest> currentItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public string filterText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int buttonSpacing;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showingTrackButton = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum SearchTypes
	{
		All,
		Active,
		Completed
	}
}
