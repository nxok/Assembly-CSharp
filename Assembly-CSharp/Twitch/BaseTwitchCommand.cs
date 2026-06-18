using System;
using System.Collections.Generic;

namespace Twitch
{
	public class BaseTwitchCommand
	{
		public virtual BaseTwitchCommand.PermissionLevels RequiredPermission
		{
			get
			{
				return BaseTwitchCommand.PermissionLevels.Everyone;
			}
		}

		public virtual string[] CommandText
		{
			get
			{
				return null;
			}
		}

		public virtual string[] LocalizedCommandNames
		{
			get
			{
				return null;
			}
		}

		public static void ClearCommandPermissionOverrides()
		{
			BaseTwitchCommand.CommandPermissionOverrides.Clear();
		}

		public static void AddCommandPermissionOverride(string commandName, BaseTwitchCommand.PermissionLevels permissionLevel)
		{
			if (!BaseTwitchCommand.CommandPermissionOverrides.ContainsKey(commandName))
			{
				BaseTwitchCommand.CommandPermissionOverrides.Add(commandName, permissionLevel);
			}
		}

		public static BaseTwitchCommand.PermissionLevels GetPermission(BaseTwitchCommand cmd)
		{
			if (BaseTwitchCommand.CommandPermissionOverrides.ContainsKey(cmd.CommandText[0]))
			{
				return BaseTwitchCommand.CommandPermissionOverrides[cmd.CommandText[0]];
			}
			return cmd.RequiredPermission;
		}

		public BaseTwitchCommand()
		{
			this.SetupCommandTextList();
			if (BaseTwitchCommand.allText == "")
			{
				BaseTwitchCommand.allText = Localization.Get("lblAll", false, null);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetupCommandTextList()
		{
			this.CommandTextList.AddRange(this.CommandText);
			string[] localizedCommandNames = this.LocalizedCommandNames;
			for (int i = 0; i < localizedCommandNames.Length; i++)
			{
				if (!this.CommandTextList.Contains(localizedCommandNames[i]))
				{
					this.CommandTextList.Add(localizedCommandNames[i]);
				}
			}
		}

		public virtual bool CheckAllowed(TwitchIRCClient.TwitchChatMessage message)
		{
			BaseTwitchCommand.PermissionLevels permission = BaseTwitchCommand.GetPermission(this);
			if (permission == BaseTwitchCommand.PermissionLevels.Everyone)
			{
				return true;
			}
			if (permission == BaseTwitchCommand.PermissionLevels.Mod)
			{
				return message.isMod;
			}
			if (permission == BaseTwitchCommand.PermissionLevels.Broadcaster)
			{
				return message.isBroadcaster;
			}
			if (permission == BaseTwitchCommand.PermissionLevels.VIP)
			{
				return message.isVIP;
			}
			return permission == BaseTwitchCommand.PermissionLevels.Sub && message.isSub;
		}

		public virtual void Execute(ViewerEntry entry, TwitchIRCClient.TwitchChatMessage message)
		{
		}

		public virtual void ExecuteConsole(List<string> arguments)
		{
		}

		public List<string> CommandTextList = new List<string>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string allText = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static Dictionary<string, BaseTwitchCommand.PermissionLevels> CommandPermissionOverrides = new Dictionary<string, BaseTwitchCommand.PermissionLevels>();

		public enum PermissionLevels
		{
			Everyone,
			VIP,
			Sub,
			Mod,
			Broadcaster
		}
	}
}
