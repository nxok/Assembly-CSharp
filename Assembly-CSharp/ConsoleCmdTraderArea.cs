using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdTraderArea : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "traderarea" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "...";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count > 0)
		{
			bool flag = StringParsers.ParseBool(_params[0], 0, -1, true);
			for (int i = 0; i < GameManager.Instance.World.TraderAreas.Count; i++)
			{
				GameManager.Instance.World.TraderAreas[i].SetClosed(GameManager.Instance.World, flag, null, false);
			}
			return;
		}
		for (int j = 0; j < GameManager.Instance.World.TraderAreas.Count; j++)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("TraderArea: Position: {0} - IsClosed: {1}", GameManager.Instance.World.TraderAreas[j].Position, GameManager.Instance.World.TraderAreas[j].IsClosed));
		}
	}
}
