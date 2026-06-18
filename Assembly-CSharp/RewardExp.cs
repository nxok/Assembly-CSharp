using System;
using UnityEngine.Scripting;

[Preserve]
public class RewardExp : BaseReward
{
	public override void SetupReward()
	{
		base.Description = Localization.Get("RewardExp_keyword", false, null);
		this.SetupValueText();
		base.Icon = "ui_game_symbol_trophy";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupValueText()
	{
		float num = Convert.ToSingle(base.Value) * Progression.XPGain;
		if (num > 214748370f)
		{
			num = 214748370f;
		}
		base.ValueText = num.ToString("{0}");
	}

	public override void GiveReward(EntityPlayer player)
	{
		int num = Convert.ToInt32(base.Value);
		player.Progression.AddLevelExp(num, "_xpFromQuest", Progression.XPTypes.Quest, true, true, -1, null);
	}

	public override BaseReward Clone()
	{
		RewardExp rewardExp = new RewardExp();
		base.CopyValues(rewardExp);
		return rewardExp;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(RewardExp.PropExp))
		{
			base.Value = properties.Values[RewardExp.PropExp];
		}
	}

	public static string PropExp = "xp";

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> questTag = FastTags<TagGroup.Global>.Parse("quest");
}
