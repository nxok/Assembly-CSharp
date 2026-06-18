using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdOverrideServerMaxPlayerCount : ConsoleCmdAbstract
{
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
		return new string[] { "overridemaxplayercount" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Override Max Server Player Count";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		int num;
		if (_params.Count >= 1 && int.TryParse(_params[0], out num))
		{
			GameModeSurvival.OverrideMaxPlayerCount = num;
			Log.Out(string.Format("Survival Max Player Count Override set to {0}", num));
			return;
		}
		Log.Out("Incorrect param, expected an integer for max player count.");
	}
}
