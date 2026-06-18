using System;

namespace Twitch
{
	public class TwitchCreatorGoalEventEntry : BaseTwitchEventEntry
	{
		public override bool IsValid(int amount = -1, string name = "", TwitchSubEventEntry.SubTierTypes subTier = TwitchSubEventEntry.SubTierTypes.Any)
		{
			return name == this.GoalType;
		}

		public string GoalType = "Subs";

		public int RewardAmount = 100;

		public TwitchAction.PointTypes RewardType;
	}
}
