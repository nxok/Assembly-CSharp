using System;
using System.Collections.Generic;

public class QuestTierReward
{
	public void GiveRewards(EntityPlayer player)
	{
		for (int i = 0; i < this.Rewards.Count; i++)
		{
			this.Rewards[i].GiveReward(player);
		}
	}

	public int Tier;

	public List<BaseReward> Rewards = new List<BaseReward>();
}
