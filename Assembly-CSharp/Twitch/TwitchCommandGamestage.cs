using System;
using System.Collections.Generic;

namespace Twitch
{
	public class TwitchCommandGamestage : BaseTwitchCommand
	{
		public override string[] CommandText
		{
			get
			{
				return new string[] { "#gamestage", "#gs" };
			}
		}

		public override string[] LocalizedCommandNames
		{
			get
			{
				return new string[]
				{
					Localization.Get("TwitchCommand_Gamestage1", false, null),
					Localization.Get("TwitchCommand_Gamestage1", false, null)
				};
			}
		}

		public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
		{
			TwitchManager.Current.DisplayGameStage();
		}

		public override void ExecuteConsole(List<string> arguments)
		{
			TwitchManager.Current.DisplayGameStage();
		}
	}
}
