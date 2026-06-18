using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdStab : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "stab" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "stability";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Running stability");
		GameManager.Instance.CreateStabilityViewer();
		if (_params.Count == 0)
		{
			GameManager.Instance.stabilityViewer.StartSearch(100);
			return;
		}
		if (_params.Count != 1)
		{
			return;
		}
		if (_params[0].EqualsCaseInsensitive("Clear"))
		{
			GameManager.Instance.ClearStabilityViewer();
			return;
		}
		if (_params[0].EqualsCaseInsensitive("Redo"))
		{
			GameManager.Instance.stabilityViewer.StartSearch(100);
			return;
		}
		int num = 31;
		int.TryParse(_params[0], out num);
		GameManager.Instance.stabilityViewer.StartSearch(num);
	}
}
