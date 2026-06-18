using System;
using System.Collections.Generic;
using DynamicMusic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdDMS : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "dms" };
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
		if (_params.Count <= 0)
		{
			Log.Out("a parameter is required to run a dms command. Call 'help dms' to see the list of available parameters.");
			return;
		}
		if (!(_params[0].ToLower() == "state"))
		{
			Log.Out(string.Format("{0} is not a known parameter for 'dms'", _params[0]));
			return;
		}
		Conductor dmsConductor = GameManager.Instance.World.dmsConductor;
		if (dmsConductor != null)
		{
			Log.Out(string.Format("dms exists with current state ${0}", dmsConductor.CurrentSectionType));
			return;
		}
		Log.Out("dms does not currently exist");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Gives control over Dynamic Music functionality.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "No commands available for dms at the moment.";
	}
}
