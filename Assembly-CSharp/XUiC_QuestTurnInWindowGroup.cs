using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestTurnInWindowGroup : XUiController
{
	public static void Open(XUi xui, Action _OnCloseAction = null)
	{
		xui.playerUI.windowManager.Open("questTurnIn", true);
		if (_OnCloseAction != null)
		{
			GUIWindow window = xui.playerUI.windowManager.GetWindow("questTurnIn");
			window.OnWindowClose = (Action)Delegate.Combine(window.OnWindowClose, _OnCloseAction);
		}
	}

	public override void Init()
	{
		base.Init();
		this.nonPagingHeaderWindow = base.GetChildByType<XUiC_WindowNonPagingHeader>();
		this.detailsWindow = base.GetChildByType<XUiC_QuestTurnInDetailsWindow>();
		this.rewardsWindow = base.GetChildByType<XUiC_QuestTurnInRewardsWindow>();
	}

	public override void OnOpen()
	{
		if (this.xui.Dialog.Respondent != null)
		{
			this.NPC = this.xui.Dialog.Respondent;
		}
		else
		{
			this.NPC = this.xui.Trader.Trader as EntityTrader;
		}
		this.detailsWindow.NPC = (this.rewardsWindow.NPC = this.NPC);
		XUiC_ItemInfoWindow childByType = this.xui.GetChildByType<XUiC_ItemInfoWindow>();
		this.rewardsWindow.InfoWindow = childByType;
		base.OnOpen();
		this.xui.playerUI.entityPlayer.OverrideFOV = 30f;
		this.xui.playerUI.entityPlayer.OverrideLookAt = this.NPC.getHeadPosition();
		GUIWindowManager windowManager = this.xui.playerUI.windowManager;
		if (this.nonPagingHeaderWindow != null)
		{
			this.nonPagingHeaderWindow.SetHeader("QUEST COMPLETE");
		}
		windowManager.Close("windowpaging");
		this.xui.DragAndDropWindow.InMenu = true;
		EntityTrader entityTrader = this.xui.Dialog.Respondent as EntityTrader;
		if (entityTrader != null)
		{
			entityTrader.SetNextTraderWindow(EntityTrader.TraderWindowState.Dialog);
		}
	}

	public override void OnClose()
	{
		EntityTrader entityTrader = this.xui.Dialog.Respondent as EntityTrader;
		base.OnClose();
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		if (GameManager.Instance.World == null)
		{
			return;
		}
		if (entityTrader == null)
		{
			if (Vector3.Distance(this.xui.Dialog.Respondent.position, entityPlayer.position) > 5f)
			{
				this.xui.Dialog.Respondent = null;
				this.xui.playerUI.entityPlayer.OverrideFOV = -1f;
				return;
			}
			this.xui.playerUI.windowManager.Open("dialog", true);
		}
	}

	public void TryNextComplete()
	{
		Quest nextCompletedQuest = this.xui.playerUI.entityPlayer.QuestJournal.GetNextCompletedQuest(this.xui.Dialog.QuestTurnIn, this.NPC.entityId);
		if (nextCompletedQuest == null)
		{
			this.xui.playerUI.windowManager.CloseAllOpenModalWindows(null, false);
			return;
		}
		this.xui.Dialog.QuestTurnIn = nextCompletedQuest;
		this.detailsWindow.CurrentQuest = (this.rewardsWindow.CurrentQuest = nextCompletedQuest);
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestTurnInDetailsWindow detailsWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestTurnInRewardsWindow rewardsWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	public static string ID = "questTurnIn";

	public EntityNPC NPC;
}
