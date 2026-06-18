using System;
using UnityEngine.Scripting;

[Preserve]
public class QuestActionTrackQuest : BaseQuestAction
{
	public override void SetupAction()
	{
	}

	public override void PerformAction(Quest ownerQuest)
	{
		ownerQuest.Tracked = true;
		ownerQuest.OwnerJournal.RefreshTracked();
	}

	public override BaseQuestAction Clone()
	{
		QuestActionTrackQuest questActionTrackQuest = new QuestActionTrackQuest();
		base.CopyValues(questActionTrackQuest);
		return questActionTrackQuest;
	}
}
