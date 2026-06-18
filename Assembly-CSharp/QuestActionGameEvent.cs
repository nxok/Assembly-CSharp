using System;
using UnityEngine.Scripting;

[Preserve]
public class QuestActionGameEvent : BaseQuestAction
{
	public override void SetupAction()
	{
	}

	public override void PerformAction(Quest ownerQuest)
	{
		GameEventManager.Current.HandleAction(this.ID, ownerQuest.OwnerJournal.OwnerPlayer, ownerQuest.OwnerJournal.OwnerPlayer, false, "", "", false, true, "", null);
	}

	public override BaseQuestAction Clone()
	{
		QuestActionGameEvent questActionGameEvent = new QuestActionGameEvent();
		base.CopyValues(questActionGameEvent);
		return questActionGameEvent;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(QuestActionGameEvent.PropEventName, ref this.ID);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropEventName = "event";
}
