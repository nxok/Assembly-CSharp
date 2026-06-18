using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSpawnEntity : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "spawnentity", "se" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "se playerId entity# <count> - spawn around playerId (0 for local) entity# of count (1 to 100)";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		World world = GameManager.Instance.World;
		int num = 1;
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("players:");
			foreach (KeyValuePair<int, EntityPlayer> keyValuePair in world.Players.dict)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  " + keyValuePair.Key.ToString() + " - " + keyValuePair.Value.EntityName);
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("entity numbers:");
			foreach (KeyValuePair<int, EntityClass> keyValuePair2 in EntityClass.list.Dict)
			{
				if (keyValuePair2.Value.userSpawnType != EntityClass.UserSpawnType.None)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  " + num.ToString() + " - " + keyValuePair2.Value.entityClassName);
					num++;
				}
			}
			return;
		}
		EntityPlayer entityPlayer = null;
		int num2;
		bool flag = int.TryParse(_params[0], out num2);
		if (flag && num2 == 0)
		{
			entityPlayer = world.GetPrimaryPlayer();
		}
		else
		{
			if (!flag)
			{
				num2 = -1;
				for (int i = 0; i < world.Players.list.Count; i++)
				{
					EntityPlayer entityPlayer2 = world.Players.list[i];
					if (entityPlayer2.EntityName.EqualsCaseInsensitive(_params[0]))
					{
						num2 = entityPlayer2.entityId;
						break;
					}
				}
			}
			if (num2 != -1)
			{
				world.Players.dict.TryGetValue(num2, out entityPlayer);
			}
		}
		if (!entityPlayer)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Player '" + _params[0] + "' not found");
			return;
		}
		int num3 = -1;
		int.TryParse(_params[1], out num3);
		int num4 = 1;
		if (_params.Count >= 3)
		{
			num4 = int.Parse(_params[2]);
			num4 = Utils.FastMin(num4, 100);
		}
		num = 1;
		foreach (KeyValuePair<int, EntityClass> keyValuePair3 in EntityClass.list.Dict)
		{
			if (keyValuePair3.Value.userSpawnType != EntityClass.UserSpawnType.None)
			{
				if (num == num3 || keyValuePair3.Value.entityClassName.Equals(_params[1]))
				{
					if (keyValuePair3.Value.entityClassName == "entityJunkDrone")
					{
						num4 = 1;
					}
					for (int j = 0; j < num4; j++)
					{
						int num5;
						int num6;
						int num7;
						if (!world.FindRandomSpawnPointNearPlayer(entityPlayer, 15, out num5, out num6, out num7, 10))
						{
							SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No spawn point found near player!");
							return;
						}
						Entity entity = EntityFactory.CreateEntity(keyValuePair3.Key, new Vector3((float)num5, (float)num6 + 0.3f, (float)num7));
						world.SpawnEntityInWorld(entity);
					}
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Spawned " + keyValuePair3.Value.entityClassName);
					return;
				}
				num++;
			}
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Entity '" + _params[1] + "' not found");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "spawns an entity";
	}
}
