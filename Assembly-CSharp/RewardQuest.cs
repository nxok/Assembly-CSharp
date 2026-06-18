using System;
using UnityEngine.Scripting;

[Preserve]
public class RewardQuest : BaseReward
{
	public override void SetupReward()
	{
		base.Description = Localization.Get("RewardQuest_keyword", false, null);
		base.ValueText = QuestClass.GetQuest(base.ID).Name;
		base.Icon = "ui_game_symbol_quest";
	}

	public override void GiveReward(EntityPlayer player)
	{
		Quest quest = QuestClass.CreateQuest(base.ID);
		if (base.OwnerQuest != null)
		{
			quest.PreviousQuest = QuestClass.GetQuest(base.OwnerQuest.ID).Name;
		}
		player.QuestJournal.AddQuest(quest, true);
	}

	public override BaseReward Clone()
	{
		RewardQuest rewardQuest = new RewardQuest();
		base.CopyValues(rewardQuest);
		rewardQuest.IsChainQuest = this.IsChainQuest;
		return rewardQuest;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(RewardQuest.PropQuest))
		{
			base.ID = properties.Values[RewardQuest.PropQuest];
		}
		if (properties.Values.ContainsKey(RewardQuest.PropChainQuest))
		{
			this.IsChainQuest = Convert.ToBoolean(properties.Values[RewardQuest.PropChainQuest]);
		}
	}

	public static string PropQuest = "quest";

	public static string PropChainQuest = "chainquest";

	public bool IsChainQuest = true;
}
