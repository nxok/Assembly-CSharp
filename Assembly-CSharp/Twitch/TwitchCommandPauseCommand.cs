using System;
using System.Collections.Generic;

namespace Twitch
{
	public class TwitchCommandPauseCommand : BaseTwitchCommand
	{
		public override BaseTwitchCommand.PermissionLevels RequiredPermission
		{
			get
			{
				return BaseTwitchCommand.PermissionLevels.Mod;
			}
		}

		public override string[] CommandText
		{
			get
			{
				return new string[] { "#pause" };
			}
		}

		public override string[] LocalizedCommandNames
		{
			get
			{
				return new string[] { Localization.Get("TwitchCommand_Pause", false, null) };
			}
		}

		public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
		{
			if (message.Message.Split(' ', StringSplitOptions.None).Length == 1)
			{
				TwitchManager.Current.SetTwitchActive(false);
			}
		}

		public override void ExecuteConsole(List<string> arguments)
		{
			if (arguments.Count == 1)
			{
				TwitchManager.Current.SetTwitchActive(false);
			}
		}
	}
}
