using System;
using System.Collections.Generic;

namespace Twitch
{
	public class TwitchCommandTeleportBackpack : BaseTwitchCommand
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
				return new string[] { "#tp_backpack", "#teleport_backpack" };
			}
		}

		public override string[] LocalizedCommandNames
		{
			get
			{
				return new string[]
				{
					Localization.Get("TwitchCommand_TeleportBackpack1", false, null),
					Localization.Get("TwitchCommand_TeleportBackpack2", false, null)
				};
			}
		}

		public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
		{
			string[] array = message.Message.Split(' ', StringSplitOptions.None);
			EntityPlayer entityPlayer = TwitchManager.Current.LocalPlayer;
			if (array.Length != 2)
			{
				GameEventManager.Current.HandleAction("action_teleport_backpack", entityPlayer, entityPlayer, false, "", "", false, true, "", null);
				return;
			}
			int num = -1;
			if (StringParsers.TryParseSInt32(array[1], out num) && TwitchManager.Current.LocalPlayer.Party != null)
			{
				entityPlayer = TwitchManager.Current.LocalPlayer.Party.GetMemberAtIndex(num, TwitchManager.Current.LocalPlayer);
				entityPlayer == null;
				return;
			}
		}

		public override void ExecuteConsole(List<string> arguments)
		{
			EntityPlayer entityPlayer = TwitchManager.Current.LocalPlayer;
			if (arguments.Count != 2)
			{
				GameEventManager.Current.HandleAction("action_teleport_backpack", entityPlayer, entityPlayer, false, "", "", false, true, "", null);
				return;
			}
			int num = -1;
			if (StringParsers.TryParseSInt32(arguments[1], out num) && TwitchManager.Current.LocalPlayer.Party != null)
			{
				entityPlayer = TwitchManager.Current.LocalPlayer.Party.GetMemberAtIndex(num, TwitchManager.Current.LocalPlayer);
				entityPlayer == null;
				return;
			}
		}
	}
}
