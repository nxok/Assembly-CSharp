using System;

namespace Twitch
{
	public class TwitchEventEntry : BaseTwitchEventEntry
	{
		public override bool IsValid(int amount = -1, string name = "", TwitchSubEventEntry.SubTierTypes subTier = TwitchSubEventEntry.SubTierTypes.Any)
		{
			return (this.StartAmount == -1 || amount >= this.StartAmount) && (this.EndAmount == -1 || amount <= this.EndAmount);
		}

		public int StartAmount = -1;

		public int EndAmount = -1;
	}
}
