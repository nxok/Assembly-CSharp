using System;

namespace Twitch
{
	public class TwitchActionEntry
	{
		public int ActionCost
		{
			get
			{
				return this.Action.CurrentCost;
			}
		}

		public TwitchActionHistoryEntry SetupHistoryEntry(ViewerEntry viewerEntry)
		{
			string text = ((this.Target != null) ? this.Target.EntityName : "");
			this.HistoryEntry = new TwitchActionHistoryEntry(this.UserName, viewerEntry.UserColor, this.Action, null, null)
			{
				UserID = viewerEntry.UserID,
				PointsSpent = this.Action.CurrentCost,
				Target = text
			};
			this.HistoryEntry.ActionEntry = this;
			return this.HistoryEntry;
		}

		public string UserName;

		public EntityPlayer Target;

		public bool ReadyForRemove;

		public TwitchVoteEntry VoteEntry;

		public TwitchAction Action;

		public bool IsSent;

		public bool ChannelNotify = true;

		public bool IsBitAction;

		public bool IsReRun;

		public bool IsRespawn;

		public int SpecialPointsUsed;

		public int StandardPointsUsed;

		public int BitsUsed;

		public int CreditsUsed;

		public TwitchActionHistoryEntry HistoryEntry;
	}
}
