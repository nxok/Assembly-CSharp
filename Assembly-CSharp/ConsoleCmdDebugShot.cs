using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdDebugShot : ConsoleCmdAbstract
{
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
		return new string[] { "debugshot", "dbs" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		GUIWindowConsole.Close();
		bool flag = _params.Count > 0 && StringParsers.ParseBool(_params[0], 0, -1, true);
		ThreadManager.StartCoroutine(this.openWindowLater(flag));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator openWindowLater(bool _savePerks)
	{
		yield return null;
		GUIWindowScreenshotText.Open(LocalPlayerUI.primaryUI, _savePerks);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Creates a screenshot with some debug information";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n  debugshot [save perks]\nLets you make a screenshot that will have some generic info\non it and a custom text you can enter. Also stores a list\nof your current perk levels, buffs and cvars in a CSV file\nnext to it if the optional parameter 'save perks' is set to true";
	}
}
