using System;

namespace Twitch
{
	public class BaseTwitchEventEntry
	{
		public virtual bool IsValid(int amount = -1, string name = "", TwitchSubEventEntry.SubTierTypes subTier = TwitchSubEventEntry.SubTierTypes.Any)
		{
			return false;
		}

		public virtual string Description(TwitchEventActionEntry entry)
		{
			return string.Format("{0}({1})", this.EventTitle, entry.Count);
		}

		public virtual void HandleInstant(string username, TwitchManager tm)
		{
			if (this.PimpPotAdd > 0)
			{
				tm.AddToPot(this.PimpPotAdd);
			}
			if (this.BitPotAdd > 0)
			{
				tm.AddToBitPot(this.BitPotAdd);
			}
			if (this.CooldownAdd > 0)
			{
				tm.AddCooldownAmount(this.CooldownAdd);
			}
			if (this.PPAmount > 0 || this.SPAmount > 0)
			{
				if (username == "")
				{
					tm.ViewerData.AddPointsAll(this.PPAmount, this.SPAmount, true);
					return;
				}
				ViewerEntry viewerEntry = tm.ViewerData.GetViewerEntry(username);
				viewerEntry.SpecialPoints += (float)this.SPAmount;
				viewerEntry.StandardPoints += (float)this.PPAmount;
			}
		}

		public virtual bool HandleEvent(string username, TwitchManager tm)
		{
			if (this.EventName == "")
			{
				return true;
			}
			if (!this.SafeAllowed && tm.IsSafe)
			{
				return false;
			}
			if (TwitchManager.BossHordeActive)
			{
				return false;
			}
			TwitchManager.CooldownTypes cooldownType = tm.CooldownType;
			if (cooldownType != TwitchManager.CooldownTypes.None)
			{
				if (cooldownType == TwitchManager.CooldownTypes.Startup)
				{
					if (!this.StartingCooldownAllowed)
					{
						return false;
					}
				}
				else if (!this.CooldownAllowed)
				{
					return false;
				}
			}
			if (!this.VoteEventAllowed && tm.VotingManager.VotingIsActive)
			{
				return false;
			}
			if (GameEventManager.Current.HandleAction(this.EventName, tm.LocalPlayer, tm.LocalPlayer, false, username, "event", tm.AllowCrateSharing, false, "", null))
			{
				GameEventManager.Current.HandleGameEventApproved(this.EventName, tm.LocalPlayer.entityId, username, "event");
				return true;
			}
			return false;
		}

		public string EventName = "";

		public string EventTitle = "";

		public bool SafeAllowed = true;

		public bool StartingCooldownAllowed;

		public bool CooldownAllowed = true;

		public bool VoteEventAllowed = true;

		public bool RewardsBitPot;

		public int PPAmount;

		public int SPAmount;

		public int PimpPotAdd;

		public int BitPotAdd;

		public int CooldownAdd;

		public BaseTwitchEventEntry.EventTypes EventType;

		public enum EventTypes
		{
			Bits,
			Subs,
			GiftSubs,
			Raid,
			Charity,
			ChannelPoints,
			HypeTrain,
			CreatorGoal
		}
	}
}
