using System;
using System.Threading.Tasks;
using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestWindowGroup : XUiController
{
	public override void Init()
	{
		base.Init();
		this.objectivesWindow = base.GetChildByType<XUiC_QuestObjectivesWindow>();
		this.rewardsWindow = base.GetChildByType<XUiC_QuestRewardsWindow>();
		this.descriptionWindow = base.GetChildByType<XUiC_QuestDescriptionWindow>();
		XUiC_QuestListWindow childByType = base.GetChildByType<XUiC_QuestListWindow>();
		this.questList = ((childByType != null) ? childByType.GetChildByType<XUiC_QuestList>() : null);
		XUiC_QuestSharedListWindow childByType2 = base.GetChildByType<XUiC_QuestSharedListWindow>();
		this.sharedList = ((childByType2 != null) ? childByType2.GetChildByType<XUiC_QuestSharedList>() : null);
		if (this.questList != null)
		{
			this.questList.SharedList = this.sharedList;
		}
		if (this.sharedList != null)
		{
			this.sharedList.QuestList = this.questList;
		}
	}

	public void SetQuest(XUiC_QuestEntry q)
	{
		this.objectivesWindow.SetQuest(q);
		this.rewardsWindow.SetQuest(q);
		this.descriptionWindow.SetQuest(q);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.xui.playerUI.windowManager.Open("windowpaging", false);
		this.xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu, 0f);
		XUiController xuiController = this.xui.FindWindowGroupByName("windowpaging");
		if (xuiController != null)
		{
			XUiC_WindowSelector childByType = xuiController.GetChildByType<XUiC_WindowSelector>();
			if (childByType != null)
			{
				childByType.SetSelected("quests");
			}
		}
		base.RefreshBindings();
		this.AsyncUISelectionOnOpen();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public async void AsyncUISelectionOnOpen()
	{
		this.xui.playerUI.CursorController.Locked = true;
		for (int i = 0; i < 3; i++)
		{
			await Task.Yield();
		}
		this.xui.playerUI.CursorController.Locked = false;
		if (this.questList.HasQuests())
		{
			this.questList.SelectCursorElement(true, false);
		}
		else if (this.sharedList.HasQuests())
		{
			this.sharedList.SelectCursorElement(true, false);
		}
		else
		{
			base.GetChildById("content").SelectCursorElement(true, false);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.CalloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.xui.playerUI.windowManager.Close("windowpaging");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "questsautoshare")
		{
			_value = PartyQuests.AutoShare.ToString();
			return true;
		}
		if (_bindingName == "questsautoaccept")
		{
			_value = PartyQuests.AutoAccept.ToString();
			return true;
		}
		if (!(_bindingName == "queststier"))
		{
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		if (entityPlayer != null)
		{
			int currentFactionTier = entityPlayer.QuestJournal.GetCurrentFactionTier(1, 0, false);
			_value = string.Format(Localization.Get("xuiQuestTierDescription", false, null), ValueDisplayFormatters.RomanNumber(entityPlayer.QuestJournal.GetCurrentFactionTier(1, 0, false)), entityPlayer.QuestJournal.GetQuestFactionPoints(1), entityPlayer.QuestJournal.GetQuestFactionMax(1, currentFactionTier));
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestList questList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestSharedList sharedList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestObjectivesWindow objectivesWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestRewardsWindow rewardsWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestDescriptionWindow descriptionWindow;
}
