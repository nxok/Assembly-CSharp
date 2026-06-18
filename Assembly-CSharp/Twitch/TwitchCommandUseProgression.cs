using System;
using System.Collections.Generic;

namespace Twitch
{
	public class TwitchCommandUseProgression : BaseTwitchCommand
	{
		public override BaseTwitchCommand.PermissionLevels RequiredPermission
		{
			get
			{
				return BaseTwitchCommand.PermissionLevels.Broadcaster;
			}
		}

		public override string[] CommandText
		{
			get
			{
				return new string[] { "#useprogression" };
			}
		}

		public override string[] LocalizedCommandNames
		{
			get
			{
				return new string[] { Localization.Get("TwitchCommand_UseProgression", false, null) };
			}
		}

		public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
		{
			string[] array = message.Message.Split(' ', StringSplitOptions.None);
			if (array.Length == 2)
			{
				bool flag = false;
				if (bool.TryParse(array[1], out flag))
				{
					TwitchManager.Current.SetUseProgression(flag);
				}
				TwitchManager.Current.SendChannelMessage("[7DTD]: Use Progression Enabled: " + (TwitchManager.Current.UseProgression ? "Yes" : "No"), true);
			}
		}

		public override void ExecuteConsole(List<string> arguments)
		{
			if (arguments.Count == 2)
			{
				bool flag = false;
				if (bool.TryParse(arguments[1], out flag))
				{
					TwitchManager.Current.SetUseProgression(flag);
				}
				TwitchManager.Current.SendChannelMessage("[7DTD]: Use Progression Enabled: " + (TwitchManager.Current.UseProgression ? "Yes" : "No"), true);
			}
		}
	}
}
