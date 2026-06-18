using System;
using System.Globalization;
using UnityEngine.Scripting;

[Preserve]
public class QuestActionSetCVar : BaseQuestAction
{
	public override void SetupAction()
	{
	}

	public override void PerformAction(Quest ownerQuest)
	{
		ownerQuest.OwnerJournal.OwnerPlayer.Buffs.SetCustomVar(this.ID, StringParsers.ParseFloat(this.Value, 0, -1, NumberStyles.Any), true, CVarOperation.set);
	}

	public override BaseQuestAction Clone()
	{
		QuestActionSetCVar questActionSetCVar = new QuestActionSetCVar();
		base.CopyValues(questActionSetCVar);
		return questActionSetCVar;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(QuestActionSetCVar.PropCVar, ref this.ID);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropCVar = "cvar";
}
