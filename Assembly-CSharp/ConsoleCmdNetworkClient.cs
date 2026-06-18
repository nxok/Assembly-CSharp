using System;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdNetworkClient : ConsoleCmdNetworkServer
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
		return new string[] { "networkclient", "netc" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Client side network commands";
	}
}
