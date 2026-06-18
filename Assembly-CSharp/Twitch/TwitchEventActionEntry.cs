using System;

namespace Twitch
{
	public class TwitchEventActionEntry
	{
		public bool HandleEvent(TwitchManager tm)
		{
			if (this.Event.HandleEvent(this.UserName, tm))
			{
				this.IsSent = true;
				return true;
			}
			return false;
		}

		public string UserName;

		public byte Tier;

		public short Count;

		public bool IsSent;

		public bool IsRetry;

		public bool ReadyForRemove;

		public TwitchActionHistoryEntry HistoryEntry;

		public BaseTwitchEventEntry Event;
	}
}
