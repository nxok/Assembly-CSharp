using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdListGameObjects : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "lgo", "listgameobjects" };
	}

	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
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
		return "List all active game objects";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch();
		int num = Object.FindObjectsByType<Object>(0).Length;
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("GOs: {0}, took {1} ms", num, microStopwatch.ElapsedMilliseconds));
	}
}
