using System;
using System.Collections.Generic;

namespace Twitch
{
	public class TwitchCommandResetCooldowns : BaseTwitchCommand
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
				return new string[] { "#reset_cooldowns" };
			}
		}

		public override string[] LocalizedCommandNames
		{
			get
			{
				return new string[] { Localization.Get("TwitchCommand_ResetCooldowns", false, null) };
			}
		}

		public override void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
		{
			TwitchManager twitchManager = TwitchManager.Current;
			float currentUnityTime = twitchManager.CurrentUnityTime;
			foreach (string text in TwitchActionManager.TwitchActions.Keys)
			{
				TwitchActionManager.TwitchActions[text].ResetCooldown(currentUnityTime);
			}
			twitchManager.SetupAvailableCommands();
			twitchManager.SendChannelMessage("Action Cooldowns have been reset!", true);
		}

		public override void ExecuteConsole(List<string> arguments)
		{
			if (arguments.Count == 1)
			{
				TwitchManager twitchManager = TwitchManager.Current;
				float currentUnityTime = twitchManager.CurrentUnityTime;
				foreach (string text in TwitchActionManager.TwitchActions.Keys)
				{
					TwitchActionManager.TwitchActions[text].ResetCooldown(currentUnityTime);
				}
				twitchManager.SetupAvailableCommands();
				twitchManager.SendChannelMessage("Action Cooldowns have been reset!", true);
			}
		}
	}
}
