using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdAIDirectorShowNextWanderingHordeTime : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "shownexthordetime" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.Instance.World.aiDirector != null)
		{
			GameManager.Instance.World.aiDirector.GetComponent<AIDirectorWanderingHordeComponent>().LogTimes();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Displays the wandering horde time";
	}
}
