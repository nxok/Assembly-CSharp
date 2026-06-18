using System;
using System.Collections.Generic;

public class NPCQuestData
{
	public Dictionary<int, NPCQuestData.PlayerQuestData> PlayerQuestList = new Dictionary<int, NPCQuestData.PlayerQuestData>();

	public class PlayerQuestData
	{
		public List<Quest> QuestList
		{
			get
			{
				return this.questList;
			}
			set
			{
				this.questList = value;
				this.LastUpdate = GameManager.Instance.World.GetWorldTime() / 24000UL * 24000UL;
			}
		}

		public PlayerQuestData(List<Quest> questList)
		{
			this.QuestList = questList;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public List<Quest> questList;

		public ulong LastUpdate;
	}
}
