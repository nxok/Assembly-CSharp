using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdTeleportPoi : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "tppoi" };
	}

	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Open POI Teleporter window";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!_senderInfo.IsLocalGame)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be used on game clients");
			return;
		}
		if (_params.Count == 0)
		{
			GUIWindowConsole.Close();
			LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.Open(XUiC_PoiTeleportMenu.ID, true);
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Not implemented yet");
	}
}
