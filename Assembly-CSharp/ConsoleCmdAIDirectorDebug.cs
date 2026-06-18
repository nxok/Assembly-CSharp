using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdAIDirectorDebug : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "aiddebug" };
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return true;
		}
	}

	public override DeviceFlag AllowedDeviceTypes
	{
		get
		{
			return DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;
		}
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		AIDirectorConstants.DebugOutput = !AIDirectorConstants.DebugOutput;
		if (AIDirectorConstants.DebugOutput)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("AIDirector debug output is ON.");
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("AIDirector debug output is OFF.");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Toggles AIDirector debug output.";
	}
}
