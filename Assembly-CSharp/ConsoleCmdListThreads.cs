using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdListThreads : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "listthreads", "lt" };
	}

	public override int DefaultPermissionLevel
	{
		get
		{
			return 1000;
		}
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return true;
		}
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Threads:");
		int num = 0;
		foreach (KeyValuePair<string, ThreadManager.ThreadInfo> keyValuePair in ThreadManager.ActiveThreads)
		{
			SdtdConsole instance = SingletonMonoBehaviour<SdtdConsole>.Instance;
			int num2;
			num = (num2 = num + 1);
			instance.Output(num2.ToString() + ". " + keyValuePair.Key);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "lists all threads";
	}
}
