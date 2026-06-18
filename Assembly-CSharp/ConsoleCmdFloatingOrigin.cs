using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdFloatingOrigin : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "floatingorigin", "fo" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "";
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

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 1 && _params[0] == "on")
		{
			if (Origin.Instance != null)
			{
				Origin.Instance.isAuto = true;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Set floating origin to on");
			return;
		}
		if (_params.Count == 1 && _params[0] == "off")
		{
			if (Origin.Instance != null)
			{
				Origin.Instance.isAuto = false;
				if (GameManager.Instance.World != null)
				{
					Origin.Instance.Reposition(Vector3.zero);
				}
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Set floating origin to off");
			return;
		}
		if (Origin.Instance != null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Floating origin is " + (Origin.Instance.isAuto ? "on" : "off") + " and is at position " + Origin.position.ToCultureInvariantString());
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No FO instance!");
	}
}
