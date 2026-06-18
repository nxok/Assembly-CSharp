using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdMumblePositionalAudio : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "mumblepositionalaudio", "mpa" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Mumble Positional Audio related tools";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "\r\n\t\t\t|Usage:\r\n\t\t\t|  1. mpa enable\r\n\t\t\t|  2. mpa disable\r\n\t\t\t|  3. mpa reinit\r\n\t\t\t".Unindent(true);
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
		if (_params.Count != 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 1, found " + _params.Count.ToString() + ".");
			return;
		}
		if (_params[0].EqualsCaseInsensitive("enable"))
		{
			if (SingletonMonoBehaviour<MumblePositionalAudio>.Instance != null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Positional Audio already enabled");
				return;
			}
			MumblePositionalAudio.Init();
			return;
		}
		else if (_params[0].EqualsCaseInsensitive("disable"))
		{
			if (SingletonMonoBehaviour<MumblePositionalAudio>.Instance == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Positional Audio already disabled");
				return;
			}
			MumblePositionalAudio.Destroy();
			return;
		}
		else if (_params[0].EqualsCaseInsensitive("reinit"))
		{
			if (SingletonMonoBehaviour<MumblePositionalAudio>.Instance == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Positional Audio not enabled");
				return;
			}
			SingletonMonoBehaviour<MumblePositionalAudio>.Instance.ReinitShm();
			return;
		}
		else
		{
			if (!_params[0].EqualsCaseInsensitive("uiversion"))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid subcommand");
				return;
			}
			if (SingletonMonoBehaviour<MumblePositionalAudio>.Instance == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Positional Audio not enabled");
				return;
			}
			SingletonMonoBehaviour<MumblePositionalAudio>.Instance.printUiVersion();
			return;
		}
	}
}
