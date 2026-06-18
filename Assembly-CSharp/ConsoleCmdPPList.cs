using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPPList : ConsoleCmdAbstract
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
		return new string[] { "pplist" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		PersistentPlayerList persistentPlayers = GameManager.Instance.persistentPlayers;
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(persistentPlayers.Players.Count.ToString() + " Persistent Player(s)");
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in persistentPlayers.Players)
		{
			SdtdConsole instance = SingletonMonoBehaviour<SdtdConsole>.Instance;
			string text = "   ";
			PlatformUserIdentifierAbs key = keyValuePair.Key;
			instance.Output(text + ((key != null) ? key.ToString() : null) + " -> " + keyValuePair.Value.EntityId.ToString());
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Lists all PersistentPlayer data";
	}
}
