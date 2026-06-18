using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdLogLevel : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "loglevel" };
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Telnet/Web only: Select which types of log messages are shown";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Select which types of log messages are shown on the connection\nyou enter this command. By default all log messages are printed\non every connection.\nUsage: loglevel <loglevel name> <true/false>\nLog levels: INF, WRN, ERR, EXC or ALL\nExample: Disable display of WRN messages: loglevel WRN false";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_senderInfo.IsLocalGame || _senderInfo.RemoteClientInfo != null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("This command can only be used through a management interface like Telnet or the Web Panel.");
			return;
		}
		if (_params.Count != 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments.");
			return;
		}
		LogType logType = 1;
		bool flag = false;
		if (_params[0].EqualsCaseInsensitive("all"))
		{
			flag = true;
		}
		else if (_params[0].EqualsCaseInsensitive("inf"))
		{
			logType = 3;
		}
		else if (_params[0].EqualsCaseInsensitive("wrn"))
		{
			logType = 2;
		}
		else if (_params[0].EqualsCaseInsensitive("err"))
		{
			logType = 0;
		}
		else
		{
			if (!_params[0].EqualsCaseInsensitive("exc"))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[0] + "\" is not a valid loglevel name.");
				return;
			}
			logType = 4;
		}
		bool flag2;
		try
		{
			flag2 = ConsoleHelper.ParseParamBool(_params[1], false);
		}
		catch (ArgumentException)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[1] + "\" is not a valid boolean value.");
			return;
		}
		if (flag)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output((flag2 ? "Enabling" : "Disabling") + " all loglevels on this connection.");
			_senderInfo.NetworkConnection.EnableLogLevel(1, flag2);
			_senderInfo.NetworkConnection.EnableLogLevel(0, flag2);
			_senderInfo.NetworkConnection.EnableLogLevel(4, flag2);
			_senderInfo.NetworkConnection.EnableLogLevel(3, flag2);
			_senderInfo.NetworkConnection.EnableLogLevel(2, flag2);
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output((flag2 ? "Enabling" : "Disabling") + " loglevel \"" + logType.ToString() + "\" on this connection.");
		_senderInfo.NetworkConnection.EnableLogLevel(logType, flag2);
	}
}
