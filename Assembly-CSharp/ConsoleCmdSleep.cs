using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSleep : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "sleep" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Makes the main thread sleep for the given number of seconds (allows decimals)";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		float num = 1f;
		if (_params.Count >= 1 && !StringParsers.TryParseFloat(_params[0], out num))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Argument is not a valid float");
			return;
		}
		Thread.Sleep((int)(num * 1000f));
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Slept for {0} seconds", num));
	}
}
