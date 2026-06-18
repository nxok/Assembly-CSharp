using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSetTargetFps : ConsoleCmdAbstract
{
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
	public override string[] getCommands()
	{
		return new string[] { "settargetfps" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Set the target FPS the game should run at (upper limit)";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Set the target FPS the game should run at (upper limit).\nUsage:\n  1. settargetfps\n  2. settargetfps <fps>\n1. gets the current target FPS.\n2. sets the target FPS to the given integer value, 0 disables the FPS limiter.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			int targetFPS = GameManager.Instance.waitForTargetFPS.TargetFPS;
			if (targetFPS > 0)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Current FPS limit is " + targetFPS.ToString());
				return;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("FPS limiter is currently disabled");
			return;
		}
		else
		{
			if (_params.Count != 1)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 0 or 1, found " + _params.Count.ToString() + ".");
				return;
			}
			int num;
			if (!int.TryParse(_params[0], out num))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[0] + "\" is not a valid integer.");
				return;
			}
			if (num < 0)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("FPS must be >= 0");
				return;
			}
			GameManager.Instance.waitForTargetFPS.TargetFPS = num;
			if (num > 0)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Set FPS limit to " + num.ToString());
				return;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Disabled target FPS limiter");
			return;
		}
	}
}
