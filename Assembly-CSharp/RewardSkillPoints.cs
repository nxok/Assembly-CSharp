using System;
using System.Globalization;
using UnityEngine.Scripting;

[Preserve]
public class RewardSkillPoints : BaseReward
{
	public override void SetupReward()
	{
		string text = Localization.Get("RewardSkillPoint_keyword", false, null);
		base.Description = text;
		base.ValueText = string.Format("+{0}", base.Value);
		base.Icon = "ui_game_symbol_skills";
	}

	public override void GiveReward(EntityPlayer player)
	{
		player.Progression.SkillPoints += StringParsers.ParseSInt32(base.Value, 0, -1, NumberStyles.Integer);
	}

	public override BaseReward Clone()
	{
		RewardSkillPoints rewardSkillPoints = new RewardSkillPoints();
		base.CopyValues(rewardSkillPoints);
		return rewardSkillPoints;
	}

	public override string GetRewardText()
	{
		return string.Format("{0} {1}", base.Description, base.ValueText);
	}
}
