using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSpawnScreen : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "SpawnScreen" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Display SpawnScreen";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "SpawnScreen on/off";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		foreach (EntityPlayerLocal entityPlayerLocal in GameManager.Instance.World.GetLocalPlayers())
		{
			if (entityPlayerLocal)
			{
				entityPlayerLocal.spawnInTime = Time.time;
				entityPlayerLocal.bPlayingSpawnIn = true;
				if (_params.Count > 0)
				{
					int num = 0;
					if (int.TryParse(_params[0], out num))
					{
						EntityPlayerLocal.spawnInEffectSpeed = (float)num;
					}
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("SpawnEffect initiated...");
			}
		}
	}
}
