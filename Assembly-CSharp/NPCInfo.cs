using System;
using System.Collections.Generic;
using MusicUtils.Enums;

public class NPCInfo
{
	public List<QuestEntry> Quests
	{
		get
		{
			QuestList quest = QuestList.GetQuest(this.QuestListName);
			if (quest != null)
			{
				return quest.Quests;
			}
			return null;
		}
	}

	public static void InitStatic()
	{
		NPCInfo.npcInfoList = new Dictionary<string, NPCInfo>();
	}

	public void Init()
	{
		NPCInfo.npcInfoList[this.Id] = this;
	}

	public static void Cleanup()
	{
		NPCInfo.npcInfoList = null;
	}

	public static Dictionary<string, NPCInfo> npcInfoList;

	public string Id;

	public string Name;

	public string Faction;

	public string Portrait;

	public string LocalizationID;

	public string VoiceSet = "";

	public NPCInfo.StanceTypes CurrentStance;

	public SectionType DmsSectionType;

	public byte QuestFaction;

	public string QuestListName = "trader_quests";

	public int TraderID = -1;

	public string DialogID;

	public enum StanceTypes
	{
		None,
		Like,
		Neutral,
		Dislike
	}
}
