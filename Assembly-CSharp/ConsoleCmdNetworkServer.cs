using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdNetworkServer : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "networkserver", "nets" };
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Server side network commands";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Commands:\nlatencysim <min> <max> - sets simulation in millisecs (0 min disables)\npacketlosssim <chance> - sets simulation in percent (0 - 50)";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
			return;
		}
		string text = _params[0].ToLower();
		if (text == "ls" || text == "latencysim")
		{
			int num = 0;
			int num2 = 100;
			if (_params.Count >= 2)
			{
				int.TryParse(_params[1], out num);
			}
			if (_params.Count >= 3)
			{
				int.TryParse(_params[2], out num2);
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SetLatencySimulation(num > 0, num, num2);
			return;
		}
		if (!(text == "pls") && !(text == "packetlosssim"))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown command " + text + ".");
			return;
		}
		int num3 = 0;
		if (_params.Count >= 2)
		{
			int.TryParse(_params[1], out num3);
		}
		if (num3 > 50)
		{
			num3 = 50;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SetPacketLossSimulation(num3 > 0, num3);
	}
}
