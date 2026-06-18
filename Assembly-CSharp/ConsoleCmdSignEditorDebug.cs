using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSignEditorDebug : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "signeditordebug", "sed" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Toggles visibility of the Sign Editor debug panel. ";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "No params: Toggles visibility of the Sign Editor debug panel. ";
	}

	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	public override int DefaultPermissionLevel
	{
		get
		{
			return 1000;
		}
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		XUiC_SignEditorWindow.ShowDebugPanel = !XUiC_SignEditorWindow.ShowDebugPanel;
		Log.Out("[SignEditor] Set Sign Editor debug panel visibility to " + XUiC_SignEditorWindow.ShowDebugPanel.ToString() + ". Changes take effect the next time a layer is selected in the editor.");
	}
}
