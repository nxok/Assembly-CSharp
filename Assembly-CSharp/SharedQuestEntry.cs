using System;
using UnityEngine;

public class SharedQuestEntry
{
	public QuestClass QuestClass
	{
		get
		{
			return QuestClass.GetQuest(this.QuestID);
		}
	}

	public SharedQuestEntry(int questCode, string questID, string poiName, Vector3 position, Vector3 size, Vector3 returnPos, int sharedByPlayerID, int questGiverID, QuestJournal questJournal, Quest quest)
	{
		this.QuestCode = questCode;
		this.QuestID = questID;
		this.POIName = poiName;
		this.Position = position;
		this.Size = size;
		this.ReturnPos = returnPos;
		this.SharedByPlayerID = sharedByPlayerID;
		this.QuestGiverID = questGiverID;
		this.Quest = ((quest == null) ? QuestClass.CreateQuest(questID) : quest.Clone());
		this.Quest.OwnerJournal = questJournal;
		this.Quest.SetupSharedQuest();
		this.Quest.SharedOwnerID = sharedByPlayerID;
		this.Quest.QuestGiverID = questGiverID;
		this.Quest.QuestCode = questCode;
		this.Quest.AddSharedLocation(position, size);
		if (!this.Quest.DataVariables.ContainsKey("POIName"))
		{
			this.Quest.DataVariables.Add("POIName", poiName);
		}
	}

	public SharedQuestEntry Clone()
	{
		return new SharedQuestEntry(this.QuestCode, this.QuestID, this.POIName, this.Position, this.Size, this.ReturnPos, this.SharedByPlayerID, this.QuestGiverID, this.Quest.OwnerJournal, this.Quest);
	}

	public int QuestCode;

	public string QuestID;

	public string POIName;

	public Vector3 Position;

	public Vector3 Size;

	public Vector3 ReturnPos;

	public int SharedByPlayerID = -1;

	public int QuestGiverID = -1;

	public Quest Quest;
}
