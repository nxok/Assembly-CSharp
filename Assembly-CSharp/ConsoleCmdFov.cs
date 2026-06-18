using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdFov : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "fov" };
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
		return "Camera field of view";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		int num;
		if (_params.Count == 1 && int.TryParse(_params[0], out num))
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV, num);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Set FOV to " + num.ToString());
		}
	}
}
