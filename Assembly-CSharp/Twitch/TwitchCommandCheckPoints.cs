using System;
using System.Collections.Generic;

namespace Twitch
{
	public class TwitchCommandCheckPoints : BaseTwitchCommand
	{
		public override string[] CommandText
		{
			get
			{
				return new string[] { "#checkpoints", "#cp" };
			}
		}

		public override string[] LocalizedCommandNames
		{
			get
			{
				return new string[]
				{
					Localization.Get("TwitchCommand_CheckPoints1", false, null),
					Localization.Get("TwitchCommand_CheckPoints2", false, null)
				};
			}
		}

		public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
		{
			string[] array = message.Message.Split(' ', StringSplitOptions.None);
			if (array.Length == 2)
			{
				if (message.isMod || message.isBroadcaster)
				{
					string text = array[1];
					if (text.StartsWith("@"))
					{
						text = text.Substring(1).ToLower();
					}
					else
					{
						text = text.ToLower();
					}
					TwitchManager twitchManager = TwitchManager.Current;
					if (twitchManager.ViewerData.HasViewerEntry(text))
					{
						twitchManager.SendChannelPointOutputMessage(text);
						return;
					}
					twitchManager.ircClient.SendChannelMessage(string.Format("[7DTD]: No viewer data for {0}.", array[1]), true);
					return;
				}
			}
			else if (array.Length == 1)
			{
				TwitchManager.Current.SendChannelPointOutputMessage(message.UserName);
			}
		}

		public override void ExecuteConsole(List<string> arguments)
		{
			if (arguments.Count != 2)
			{
				if (arguments.Count == 1)
				{
					TwitchManager.Current.SendChannelPointOutputMessage(TwitchManager.Current.Authentication.userName);
				}
				return;
			}
			string text = arguments[1];
			if (text.StartsWith("@"))
			{
				text = text.Substring(1).ToLower();
			}
			else
			{
				text = text.ToLower();
			}
			TwitchManager twitchManager = TwitchManager.Current;
			if (twitchManager.ViewerData.HasViewerEntry(text))
			{
				twitchManager.SendChannelPointOutputMessage(text);
				return;
			}
			twitchManager.ircClient.SendChannelMessage(string.Format("[7DTD]: No viewer data for {0}.", arguments[1]), true);
		}
	}
}
