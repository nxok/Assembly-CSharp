using System;

namespace Twitch
{
	public class TwitchSubEventEntry : TwitchEventEntry
	{
		public TwitchSubEventEntry()
		{
			this.RewardsBitPot = true;
		}

		public override bool IsValid(int amount = -1, string name = "", TwitchSubEventEntry.SubTierTypes subTier = TwitchSubEventEntry.SubTierTypes.Any)
		{
			return (this.StartAmount == -1 || amount >= this.StartAmount) && (this.EndAmount == -1 || amount <= this.EndAmount) && (this.SubTier == TwitchSubEventEntry.SubTierTypes.Any || this.SubTier == subTier);
		}

		public override string Description(TwitchEventActionEntry entry)
		{
			return this.EventTitle;
		}

		public static TwitchSubEventEntry.SubTierTypes GetSubTier(string subPlan)
		{
			if (subPlan == "1000")
			{
				return TwitchSubEventEntry.SubTierTypes.Tier1;
			}
			if (subPlan == "2000")
			{
				return TwitchSubEventEntry.SubTierTypes.Tier2;
			}
			if (!(subPlan == "3000"))
			{
				return TwitchSubEventEntry.SubTierTypes.Prime;
			}
			return TwitchSubEventEntry.SubTierTypes.Tier3;
		}

		public TwitchSubEventEntry.SubTierTypes SubTier;

		public enum SubTierTypes
		{
			Any,
			Prime,
			Tier1,
			Tier2,
			Tier3
		}
	}
}
