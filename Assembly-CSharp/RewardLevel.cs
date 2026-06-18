using System;
using UnityEngine.Scripting;

[Preserve]
public class RewardLevel : BaseReward
{
	public override void SetupReward()
	{
		base.Description = string.Format("{0}", Localization.Get("RewardLevel_keyword", false, null));
		base.ValueText = base.Value;
		base.Icon = "ui_game_symbol_trophy";
	}

	public override void GiveReward(EntityPlayer player)
	{
		int num = Convert.ToInt32(base.Value);
		float levelProgressPercentage = player.Progression.GetLevelProgressPercentage();
		for (int i = 0; i < num; i++)
		{
			player.Progression.AddLevelExp(player.Progression.ExpToNextLevel, "_xpOther", Progression.XPTypes.Other, true, true, -1, null);
		}
		player.Progression.AddLevelExp((int)(levelProgressPercentage * (float)player.Progression.GetExpForNextLevel()), "_xpOther", Progression.XPTypes.Other, true, true, -1, null);
	}

	public override BaseReward Clone()
	{
		RewardLevel rewardLevel = new RewardLevel();
		base.CopyValues(rewardLevel);
		return rewardLevel;
	}
}
