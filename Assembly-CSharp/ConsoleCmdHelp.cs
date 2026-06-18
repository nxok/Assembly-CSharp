using System;
using System.Collections.Generic;
using System.Text;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdHelp : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "help" };
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return true;
		}
	}

	public override int DefaultPermissionLevel
	{
		get
		{
			return 1000;
		}
	}

	public override DeviceFlag AllowedDeviceTypes
	{
		get
		{
			return DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Help on console and specific commands";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "\r\n\t\t\t|Usage:\r\n\t\t\t|  1. help\r\n\t\t\t|  2. help * <searchstring>\r\n\t\t\t|  3. help <command name>\r\n\t\t\t|  4. help output\r\n\t\t\t|  5. help outputdetailed\r\n\t\t\t|1. Show general help and list all available commands\r\n\t\t\t|2. List commands where either the name or the description contains the given text\r\n\t\t\t|3. Show help for the given command\r\n\t\t\t|4. Write command list to log file\r\n\t\t\t|5. Write command list with help texts to log file\r\n\t\t\t".Unindent(true);
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		ConsoleCmdHelp.sb.Clear();
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("*** Generic Console Help ***");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("To get further help on a specific topic or command type (without the brackets)");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("    help <topic / command>");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Generic notation of command parameters:");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("   <param name>              Required parameter");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("   <entityId / player name>  Possible types of parameter values");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("   [param name]              Optional parameter");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("");
			ConsoleCmdHelp.sb.Clear();
			ConsoleCmdHelp.BuildStringCommandDescriptions();
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(ConsoleCmdHelp.sb.ToString());
			ConsoleCmdHelp.sb.Clear();
			return;
		}
		Action<List<string>> action = null;
		if (ConsoleCmdHelp.helpTopics.ContainsKey(_params[0]))
		{
			action = ConsoleCmdHelp.helpTopics[_params[0]].Action;
		}
		ConsoleCmdHelp.sb.Clear();
		ConsoleCmdHelp.BuildStringHelpText(_params[0]);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(ConsoleCmdHelp.sb.ToString());
		ConsoleCmdHelp.sb.Clear();
		if (action != null)
		{
			action(_params);
		}
	}

	public static void ValidateNoCommandOverlap()
	{
		IList<IConsoleCommand> commands = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommands();
		for (int i = 0; i < commands.Count; i++)
		{
			foreach (string text in commands[i].GetCommands())
			{
				if (ConsoleCmdHelp.helpTopics.ContainsKey(text))
				{
					Log.Warning("Command with alias \"" + text + "\" conflicts with help topic command");
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void BuildStringCommandDescriptions()
	{
		ConsoleCmdHelp.sb.AppendLine("*** List of Help Topics ***");
		foreach (KeyValuePair<string, ConsoleCmdHelp.HelpTopic> keyValuePair in ConsoleCmdHelp.helpTopics)
		{
			ConsoleCmdHelp.sb.Append(keyValuePair.Key);
			ConsoleCmdHelp.sb.Append(" => ");
			ConsoleCmdHelp.sb.AppendLine(keyValuePair.Value.Description);
		}
		ConsoleCmdHelp.sb.AppendLine("");
		ConsoleCmdHelp.sb.AppendLine("*** List of Commands ***");
		IList<IConsoleCommand> commands = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommands();
		for (int i = 0; i < commands.Count; i++)
		{
			IConsoleCommand consoleCommand = commands[i];
			if (consoleCommand.CanExecuteForDevice && consoleCommand != null)
			{
				foreach (string text in consoleCommand.GetCommands())
				{
					ConsoleCmdHelp.sb.Append(" ");
					ConsoleCmdHelp.sb.Append(text);
				}
				ConsoleCmdHelp.sb.Append(" => ");
				ConsoleCmdHelp.sb.AppendLine(consoleCommand.GetDescription());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void BuildStringHelpText(string key)
	{
		string text = null;
		string text2 = null;
		if (ConsoleCmdHelp.helpTopics.ContainsKey(key))
		{
			text = "Topic: " + key;
			text2 = ConsoleCmdHelp.helpTopics[key].ActionCompleteText;
		}
		else
		{
			IConsoleCommand command = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommand(key, false);
			if (command != null && command.CanExecuteForDevice)
			{
				text = "Command(s): " + string.Join(", ", command.GetCommands());
				text2 = command.GetHelp();
				if (string.IsNullOrEmpty(text2))
				{
					text2 = "No detailed help available.\nDescription: " + command.GetDescription();
				}
			}
		}
		if (text != null)
		{
			ConsoleCmdHelp.sb.AppendLine("*** " + text + " ***");
			foreach (string text3 in text2.Split('\n', StringSplitOptions.None))
			{
				ConsoleCmdHelp.sb.AppendLine(text3);
			}
			return;
		}
		ConsoleCmdHelp.sb.AppendLine("No command or topic found by \"" + key + "\"");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OutputHelp(List<string> _params)
	{
		ConsoleCmdHelp.sb.Clear();
		ConsoleCmdHelp.BuildStringCommandDescriptions();
		Log.Out(ConsoleCmdHelp.sb.ToString());
		ConsoleCmdHelp.sb.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OutputDetailedHelp(List<string> _params)
	{
		ConsoleCmdHelp.sb.Clear();
		ConsoleCmdHelp.sb.AppendLine("*** List of Help Topics ***");
		foreach (KeyValuePair<string, ConsoleCmdHelp.HelpTopic> keyValuePair in ConsoleCmdHelp.helpTopics)
		{
			ConsoleCmdHelp.BuildStringHelpText(keyValuePair.Key);
			ConsoleCmdHelp.sb.AppendLine();
		}
		ConsoleCmdHelp.sb.AppendLine("*** List of Commands ***");
		foreach (IConsoleCommand consoleCommand in SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommands())
		{
			ConsoleCmdHelp.BuildStringHelpText(consoleCommand.GetCommands()[0]);
			ConsoleCmdHelp.sb.AppendLine();
		}
		Log.Out(ConsoleCmdHelp.sb.ToString());
		ConsoleCmdHelp.sb.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SearchHelp(List<string> _params)
	{
		ConsoleCmdHelp.sb.Clear();
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Argument for search mask missing");
			return;
		}
		string text = _params[1];
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("*** List of Commands for \"" + text + "\" ***");
		IList<IConsoleCommand> commands = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommands();
		for (int i = 0; i < commands.Count; i++)
		{
			IConsoleCommand consoleCommand = commands[i];
			string description = consoleCommand.GetDescription();
			bool flag = text == null;
			if (!flag)
			{
				flag = description.ContainsCaseInsensitive(text);
				foreach (string text2 in consoleCommand.GetCommands())
				{
					flag |= text2.ContainsCaseInsensitive(text);
				}
			}
			if (flag)
			{
				foreach (string text3 in consoleCommand.GetCommands())
				{
					ConsoleCmdHelp.sb.Append(" ");
					ConsoleCmdHelp.sb.Append(text3);
				}
				ConsoleCmdHelp.sb.Append(" => ");
				ConsoleCmdHelp.sb.Append(description);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(ConsoleCmdHelp.sb.ToString());
				ConsoleCmdHelp.sb.Length = 0;
			}
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(ConsoleCmdHelp.sb.ToString());
		ConsoleCmdHelp.sb.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static StringBuilder sb = new StringBuilder();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, ConsoleCmdHelp.HelpTopic> helpTopics = new Dictionary<string, ConsoleCmdHelp.HelpTopic>
	{
		{
			"output",
			new ConsoleCmdHelp.HelpTopic("Prints commands to log file", new Action<List<string>>(ConsoleCmdHelp.OutputHelp), "Printed commands to log file")
		},
		{
			"outputdetailed",
			new ConsoleCmdHelp.HelpTopic("Prints commands with details to log file", new Action<List<string>>(ConsoleCmdHelp.OutputDetailedHelp), "Printed commands with details to log file")
		},
		{
			"search",
			new ConsoleCmdHelp.HelpTopic("Search for all commands matching a string", new Action<List<string>>(ConsoleCmdHelp.SearchHelp), "<first argument will be the string to match>")
		},
		{
			"*",
			new ConsoleCmdHelp.HelpTopic("Search for all commands matching a string", new Action<List<string>>(ConsoleCmdHelp.SearchHelp), "<first argument will be the string to match>")
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public struct HelpTopic
	{
		public HelpTopic(string _desc, Action<List<string>> _action, string _actionCompleteText)
		{
			this.Description = _desc;
			this.Action = _action;
			this.ActionCompleteText = _actionCompleteText;
		}

		public string Description;

		public Action<List<string>> Action;

		public string ActionCompleteText;
	}
}
