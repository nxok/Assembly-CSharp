using System;

namespace Twitch
{
	public class TwitchMessageEntry
	{
		public TwitchMessageEntry(string msg, string sound)
		{
			this.Message = msg;
			this.Sound = sound;
		}

		public string Message = "";

		public string Sound = "";
	}
}
