using System;
using System.Collections.Generic;

public class QuestList
{
	public string ID
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestList(string id)
	{
		this.ID = id;
	}

	public static QuestList NewList(string id)
	{
		if (QuestList.s_QuestLists.ContainsKey(id))
		{
			return null;
		}
		QuestList questList = new QuestList(id.ToLower());
		QuestList.s_QuestLists[id] = questList;
		return questList;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static QuestList GetQuest(string questListID)
	{
		if (!QuestList.s_QuestLists.ContainsKey(questListID))
		{
			return null;
		}
		return QuestList.s_QuestLists[questListID];
	}

	public static Dictionary<string, QuestList> s_QuestLists = new CaseInsensitiveStringDictionary<QuestList>();

	public List<QuestEntry> Quests = new List<QuestEntry>();
}
