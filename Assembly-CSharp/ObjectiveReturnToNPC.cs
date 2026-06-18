using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveReturnToNPC : ObjectiveRandomGoto
{
	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveReturnToTrader_keyword", false, null) + ((base.OwnerQuest.CurrentState == Quest.QuestState.NotStarted) ? "" : ":");
		this.completeWithinRange = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetupIcon()
	{
		this.icon = "ui_game_symbol_quest";
	}

	public override bool PlayObjectiveComplete
	{
		get
		{
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 GetPosition()
	{
		if (base.OwnerQuest.GetPositionData(out this.position, Quest.PositionDataTypes.QuestGiver))
		{
			base.OwnerQuest.Position = this.position;
			this.positionSet = true;
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.QuestGiver, this.NavObjectName, -1);
			base.CurrentValue = 2;
			return this.position;
		}
		base.CurrentValue = 3;
		base.Complete = true;
		base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		this.positionSet = true;
		QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
		this.HiddenObjective = true;
		base.OwnerQuest.RemoveMapObject();
		return Vector3.zero;
	}

	public override void OnStart()
	{
		base.OnStart();
		if (base.OwnerQuest.QuestClass.AddsToTierComplete)
		{
			base.OwnerQuest.OwnerJournal.HandleQuestCompleteToday(base.OwnerQuest);
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveReturnToNPC objectiveReturnToNPC = new ObjectiveReturnToNPC();
		this.CopyValues(objectiveReturnToNPC);
		return objectiveReturnToNPC;
	}
}
