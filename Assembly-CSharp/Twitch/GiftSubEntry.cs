using System;

namespace Twitch
{
	public class GiftSubEntry
	{
		public GiftSubEntry(string userName, int userID, TwitchSubEventEntry.SubTierTypes tier, int subCount)
		{
			this.UserName = userName;
			this.UserID = userID;
			this.TimeRemaining = 1f;
			this.SubCount = subCount;
			this.Tier = tier;
		}

		public void AddSub()
		{
			this.SubCount++;
			this.TimeRemaining = 1f;
		}

		public bool Update(float deltaTime)
		{
			this.TimeRemaining -= deltaTime;
			return this.TimeRemaining <= 0f;
		}

		public float TimeRemaining = 1f;

		public string UserName = "";

		public int UserID = -1;

		public int SubCount;

		public TwitchSubEventEntry.SubTierTypes Tier = TwitchSubEventEntry.SubTierTypes.Tier1;
	}
}
