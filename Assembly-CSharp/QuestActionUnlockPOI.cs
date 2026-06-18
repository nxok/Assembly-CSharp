using System;
using UnityEngine.Scripting;

[Preserve]
public class QuestActionUnlockPOI : BaseQuestAction
{
	public override void SetupAction()
	{
	}

	public override void PerformAction(Quest ownerQuest)
	{
		ownerQuest.HandleUnlockPOI(null);
	}

	public override BaseQuestAction Clone()
	{
		QuestActionUnlockPOI questActionUnlockPOI = new QuestActionUnlockPOI();
		base.CopyValues(questActionUnlockPOI);
		return questActionUnlockPOI;
	}
}
