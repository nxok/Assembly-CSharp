using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestRewardList : XUiController
{
	public Quest Quest
	{
		get
		{
			return this.quest;
		}
		set
		{
			this.quest = value;
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = base.GetChildrenByType<XUiC_QuestRewardEntry>(null);
		XUiController[] array = childrenByType;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				this.rewardEntries.Add(array[i]);
			}
		}
	}

	public override void Update(float _dt)
	{
		if (this.IsDirty)
		{
			if (this.quest != null)
			{
				List<BaseReward> list = new List<BaseReward>();
				Quest quest2;
				for (Quest quest = this.quest; quest != null; quest = quest2)
				{
					quest2 = null;
					for (int i = 0; i < quest.Rewards.Count; i++)
					{
						if (quest.Rewards[i] is RewardQuest)
						{
							quest2 = QuestClass.CreateQuest(quest.Rewards[i].ID);
						}
						if (!quest.Rewards[i].HiddenReward && quest.Rewards[i].ReceiveStage == BaseReward.ReceiveStages.QuestCompletion && (quest == this.quest || !(quest.Rewards[i] is RewardQuest)))
						{
							list.Add(quest.Rewards[i]);
						}
					}
				}
				int count = this.rewardEntries.Count;
				int count2 = list.Count;
				int num = 0;
				for (int j = 0; j < count; j++)
				{
					if (this.rewardEntries[num] is XUiC_QuestRewardEntry)
					{
						if (j < count2)
						{
							((XUiC_QuestRewardEntry)this.rewardEntries[num]).Reward = list[j];
							((XUiC_QuestRewardEntry)this.rewardEntries[num]).ChainQuest = list[j].OwnerQuest != this.Quest;
							num++;
						}
						else
						{
							((XUiC_QuestRewardEntry)this.rewardEntries[num]).Reward = null;
							num++;
						}
					}
				}
			}
			else
			{
				int count3 = this.rewardEntries.Count;
				for (int k = 0; k < count3; k++)
				{
					if (this.rewardEntries[k] is XUiC_QuestRewardEntry)
					{
						((XUiC_QuestRewardEntry)this.rewardEntries[k]).Reward = null;
					}
				}
			}
			this.IsDirty = false;
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Quest quest;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiController> rewardEntries = new List<XUiController>();
}
