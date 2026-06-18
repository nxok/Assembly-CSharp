using System;
using UnityEngine.Scripting;

[Preserve]
public class QuestEntry
{
	public QuestEntry(string questID, float prob, int startStage, int endStage, string cvar)
	{
		this.QuestID = questID;
		this.Prob = prob;
		this.StartStage = startStage;
		this.EndStage = endStage;
		this.QuestClass = QuestClass.GetQuest(this.QuestID);
		if (cvar != "")
		{
			this.CVar = cvar.Split(',', StringSplitOptions.None);
		}
	}

	public bool CheckRequirement(EntityPlayer player)
	{
		if (!QuestJournal.BuriedQuestsEnabled && this.QuestClass.FilterTags.Test_AnySet(QuestEntry.buriedTag))
		{
			return false;
		}
		if (!QuestJournal.POIQuestsEnabled && this.QuestClass.FilterTags.Test_AnySet(QuestEntry.poiTag))
		{
			return false;
		}
		if (this.CVar == null)
		{
			return true;
		}
		for (int i = 0; i < this.CVar.Length; i++)
		{
			if (player.Buffs.GetCustomVar(this.CVar[i]) == 0f)
			{
				return false;
			}
		}
		return true;
	}

	public float Prob = 1f;

	public int StartStage = -1;

	public int EndStage = -1;

	public string QuestID;

	public QuestClass QuestClass;

	public string[] CVar;

	public static FastTags<TagGroup.Global> buriedTag = FastTags<TagGroup.Global>.Parse("buried");

	public static FastTags<TagGroup.Global> poiTag = FastTags<TagGroup.Global>.Parse("poi");
}
