using System;
using System.Collections.Generic;

namespace Twitch
{
	public class TwitchCommandCommands : BaseTwitchCommand
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
				return new string[] { "#commands" };
			}
		}

		public override string[] LocalizedCommandNames
		{
			get
			{
				return new string[] { Localization.Get("TwitchCommand_Commands", false, null) };
			}
		}

		public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
		{
			TwitchManager.Current.DisplayCommands(message.isBroadcaster, message.isMod, message.isVIP, message.isSub);
		}

		public override void ExecuteConsole(List<string> arguments)
		{
			TwitchManager.Current.DisplayCommands(true, true, true, true);
		}
	}
}
