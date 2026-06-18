using System;

namespace Twitch
{
	public class TwitchLeaderboardEntry
	{
		public TwitchLeaderboardEntry(string username, string usercolor, int kills)
		{
			this.UserName = username;
			this.UserColor = ((usercolor == null) ? "FFFFFF" : usercolor);
			this.Kills = kills;
		}

		public string UserName;

		public string UserColor;

		public int Kills;
	}
}
