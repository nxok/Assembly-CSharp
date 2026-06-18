using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdAIDirectorSpawnScouts : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "spawnscouts" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count > 1 && _params.Count != 3)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected none, 1 or 3, found " + _params.Count.ToString() + ".");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" ");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
			return;
		}
		Vector3 vector = default(Vector3);
		if (_params.Count == 0)
		{
			if (!_senderInfo.IsLocalGame && _senderInfo.RemoteClientInfo == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command requires a parameter if not executed by a player.");
				return;
			}
			if (_senderInfo.IsLocalGame)
			{
				vector = GameManager.Instance.World.GetPrimaryPlayer().GetPosition();
			}
			else
			{
				vector = GameManager.Instance.World.Players.dict[_senderInfo.RemoteClientInfo.entityId].GetPosition();
			}
		}
		else if (_params.Count == 1)
		{
			ClientInfo clientInfo = ConsoleHelper.ParseParamIdOrName(_params[0], true, false);
			if (clientInfo == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Playername or entity/steamid id not found.");
				return;
			}
			vector = GameManager.Instance.World.Players.dict[clientInfo.entityId].GetPosition();
		}
		else if (_params.Count == 3)
		{
			int minValue = int.MinValue;
			int minValue2 = int.MinValue;
			int minValue3 = int.MinValue;
			int.TryParse(_params[0], out minValue);
			int.TryParse(_params[1], out minValue2);
			int.TryParse(_params[2], out minValue3);
			if (minValue == -2147483648 || minValue2 == -2147483648 || minValue3 == -2147483648)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("x:" + minValue.ToString());
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("y:" + minValue2.ToString());
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("z:" + minValue3.ToString());
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("At least one of the given coordinates is not a valid integer");
				return;
			}
			vector..ctor((float)minValue, (float)minValue2, (float)minValue3);
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Concat(new string[]
		{
			"Scouts spawning at ",
			vector.x.ToCultureInvariantString(),
			", ",
			vector.y.ToCultureInvariantString(),
			", ",
			vector.z.ToCultureInvariantString()
		}));
		GameManager.Instance.World.aiDirector.GetComponent<AIDirectorChunkEventComponent>().SpawnScouts(vector);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Spawns zombie scouts";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Spawn scouts near a player.Usage:\n   1. spawnscouts\n   2. spawnscouts <player name/steam id/entity id>\n   3. spawnscouts <x> <y> <z>\n1. Will spawn the scouts near the issuing player. Can only be used by a player, not a remote console.\n2. Spawn scouts near the given player.\n3. Spawn scouts at the given coordinates.";
	}
}
