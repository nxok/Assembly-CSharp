using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestSharedListWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		this.questList = base.GetChildByType<XUiC_QuestSharedList>();
		this.acceptBtn = (XUiV_Button)base.GetChildById("acceptBtn").ViewComponent;
		this.acceptBtn.Controller.OnPress += this.acceptBtn_OnPress;
		this.showOnMapBtn = (XUiV_Button)base.GetChildById("showOnMapBtn").ViewComponent;
		this.showOnMapBtn.Controller.OnPress += this.showOnMapBtn_OnPress;
		this.questRemoveBtn = (XUiV_Button)base.GetChildById("questRemoveBtn").ViewComponent;
		this.questRemoveBtn.Controller.OnPress += this.questRemoveBtn_OnPress;
		this.buttonSpacing = this.showOnMapBtn.Position.x - this.acceptBtn.Position.x;
		this.txtInput = (XUiC_TextInput)base.GetChildById("searchInput");
		if (this.txtInput != null)
		{
			this.txtInput.OnChangeHandler += this.HandleOnChangedHandler;
			this.txtInput.Text = "";
		}
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
	public void acceptBtn_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_QuestEntry selectedEntry = this.questList.SelectedEntry;
		SharedQuestEntry sharedQuestEntry = ((selectedEntry != null) ? selectedEntry.SharedQuestEntry : null);
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		PartyQuests.AcceptSharedQuest(sharedQuestEntry, entityPlayer);
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
			this.xui.playerUI.entityPlayer.QuestJournal.RemoveSharedQuestEntry(this.questList.SelectedEntry.SharedQuestEntry);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.filterText = this.filterText.ToLower();
			this.currentItems = this.player.QuestJournal.sharedQuestEntries.Where((SharedQuestEntry quest) => this.filterText == "" || quest.QuestClass.Name.ToLower().Contains(this.filterText)).ToList<SharedQuestEntry>();
			this.questList.SetSharedQuestList(this.currentItems);
			this.IsDirty = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.player = this.xui.playerUI.entityPlayer;
		this.player.SharedQuestAdded += this.QuestJournal_SharedQuestAdded;
		this.player.SharedQuestRemoved += this.QuestJournal_SharedQuestRemoved;
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.player.SharedQuestAdded -= this.QuestJournal_SharedQuestAdded;
		this.player.SharedQuestRemoved -= this.QuestJournal_SharedQuestRemoved;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestJournal_SharedQuestAdded(SharedQuestEntry entry)
	{
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestJournal_SharedQuestRemoved(SharedQuestEntry entry)
	{
		this.IsDirty = true;
	}

	public void ShowTrackButton(bool _show)
	{
		this.acceptBtn.IsVisible = _show;
		if (this.showingTrackButton != _show)
		{
			this.showingTrackButton = _show;
			this.acceptBtn.Enabled = _show;
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

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestSharedListWindow.SearchTypes searchType;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestSharedList questList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button acceptBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button questRemoveBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button showOnMapBtn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SharedQuestEntry> currentItems;

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
