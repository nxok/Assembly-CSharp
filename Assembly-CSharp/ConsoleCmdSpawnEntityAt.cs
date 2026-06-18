using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSpawnEntityAt : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "spawnentityat", "sea" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Spawns an entity at a give position";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n  1. spawnentityat\n  2. spawnentityat <entityidx> <x> <y> <z>\n  3. spawnentityat <entityidx> <x> <y> <z> <count>\n  4. spawnentityat <entityidx> <x> <y> <z> <count> <rotX> <rotY> <rotZ>\n  5. spawnentityat <entityidx> <x> <y> <z> <count> <rotX> <rotY> <rotZ> <stepX> <stepY> <stepZ>\n  6. spawnentityat <entityidx> <x> <y> <z> <count> <rotX> <rotY> <rotZ> <stepX> <stepY> <stepZ> <spawnerType>\n1. Lists the known entity class names\n2. Spawns the entity with the given class name at the given coordinates\n3. As 2. but spawns <count> instances of that entity type\n4. As 3. but also specifies the rotation of the spawned entities\n5. As 4. but also specify the step distance between entities\n6. As 5. but also specify the spawner source of the entity (Dynamic, StaticSpawner, Biome)";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Entity class names:");
			int num = 1;
			foreach (KeyValuePair<int, EntityClass> keyValuePair in EntityClass.list.Dict)
			{
				if (keyValuePair.Value.userSpawnType != EntityClass.UserSpawnType.None)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  " + num.ToString() + " - " + keyValuePair.Value.entityClassName);
					num++;
				}
			}
			return;
		}
		if (_params.Count != 4 && _params.Count != 5 && _params.Count != 8 && _params.Count != 11 && _params.Count != 12)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Illegal number of parameters");
			return;
		}
		string text = _params[0];
		float num2;
		if (!StringParsers.TryParseFloat(_params[1], out num2))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("x coordinate is not a valid float");
			return;
		}
		float num3;
		if (!StringParsers.TryParseFloat(_params[2], out num3))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("y coordinate is not a valid float");
			return;
		}
		float num4;
		if (!StringParsers.TryParseFloat(_params[3], out num4))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("z coordinate is not a valid float");
			return;
		}
		Vector3 vector;
		vector..ctor(num2, num3, num4);
		int num5 = 1;
		if (_params.Count > 4 && !int.TryParse(_params[4], out num5))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("count is not a valid integer");
			return;
		}
		Vector3 zero = Vector3.zero;
		if (_params.Count > 5)
		{
			float num6;
			if (!StringParsers.TryParseFloat(_params[5], out num6))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("rotX is not a valid float");
				return;
			}
			float num7;
			if (!StringParsers.TryParseFloat(_params[6], out num7))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("rotY is not a valid float");
				return;
			}
			float num8;
			if (!StringParsers.TryParseFloat(_params[7], out num8))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("rotZ is not a valid float");
				return;
			}
			zero..ctor(num6, num7, num8);
		}
		Vector3 zero2 = Vector3.zero;
		if (_params.Count > 8)
		{
			if (!StringParsers.TryParseFloat(_params[8], out zero2.x))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("rotX is not a valid float");
				return;
			}
			if (!StringParsers.TryParseFloat(_params[9], out zero2.y))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("rotY is not a valid float");
				return;
			}
			if (!StringParsers.TryParseFloat(_params[10], out zero2.z))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("rotZ is not a valid float");
				return;
			}
		}
		EnumSpawnerSource enumSpawnerSource = EnumSpawnerSource.Unknown;
		if (_params.Count > 11)
		{
			try
			{
				enumSpawnerSource = EnumUtils.Parse<EnumSpawnerSource>(_params[11], false);
			}
			catch (ArgumentException)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("spawnerType is not a valid value");
				return;
			}
		}
		foreach (KeyValuePair<int, EntityClass> keyValuePair2 in EntityClass.list.Dict)
		{
			if (keyValuePair2.Value.userSpawnType != EntityClass.UserSpawnType.None && keyValuePair2.Value.entityClassName.EqualsCaseInsensitive(text))
			{
				if (keyValuePair2.Value.entityClassName == "entityJunkDrone")
				{
					num5 = 1;
				}
				vector -= zero2 * ((float)(num5 - 1) * 0.5f);
				for (int i = 0; i < num5; i++)
				{
					Entity entity = EntityFactory.CreateEntity(keyValuePair2.Key, vector, zero);
					entity.SetSpawnerSource(enumSpawnerSource);
					GameManager.Instance.World.SpawnEntityInWorld(entity);
					vector += zero2;
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Spawned " + num5.ToString() + " " + keyValuePair2.Value.entityClassName);
				return;
			}
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Entity class name '" + _params[0] + "' unknown or not allowed to be instantiated");
	}
}
