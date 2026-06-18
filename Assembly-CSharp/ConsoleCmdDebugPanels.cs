using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdDebugPanels : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "debugpanels" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "debugpanels - toggle display, enabling also enables the debug menu\r\ndebugpanels [names] - set these panels active and disable all others, uses the short name of the panel (e.g. debugpanels Ply Ge Sp)";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		GamePrefs.Set(EnumGamePrefs.DebugMenuEnabled, true);
		NGuiWdwDebugPanels nguiWdwDebugPanels = Object.FindAnyObjectByType<NGuiWdwDebugPanels>();
		if (nguiWdwDebugPanels == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Cannot find debug panel controller");
			return;
		}
		if (_params.Count == 0)
		{
			nguiWdwDebugPanels.ToggleDisplay();
			return;
		}
		nguiWdwDebugPanels.ShowGeneralData();
		nguiWdwDebugPanels.SetActivePanels(_params.ToArray());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "allows usage of debug display panels (F3 menu) via command console";
	}
}
