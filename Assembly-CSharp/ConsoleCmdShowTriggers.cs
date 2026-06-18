using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdShowTriggers : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "showtriggers" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		bool flag = !GameManager.Instance.World.triggerManager.ShowNavObjects;
		GameManager.Instance.World.triggerManager.ShowNavObjects = flag;
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Show Triggers {0}!", new object[] { flag ? "Enabled" : "Disabled" });
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Sets the visibility of the block triggers.";
	}
}
