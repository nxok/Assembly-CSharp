using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdExportPrefab : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { ConsoleCmdExportPrefab.CommandName };
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Exports a prefab from a world area";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n  exportprefab <name> <x1> <y1> <z1> <x2> <y2> <z2> [part]\nExports a prefab with the given name from a box defined by the coordinate pair of two corners\nIf the optional parameter 'part' is 'true' it will export into the 'Parts' subfolder of the prefabs folder";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count < 7 || _params.Count > 8)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Illegal number of parameters");
			return;
		}
		bool flag = false;
		if (_params.Count == 8)
		{
			flag = ConsoleHelper.ParseParamBool(_params[7], true);
		}
		string text = _params[0];
		int num;
		if (!StringParsers.TryParseSInt32(_params[1], out num))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("x1 coordinate is not a valid integer");
			return;
		}
		int num2;
		if (!StringParsers.TryParseSInt32(_params[2], out num2))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("y1 coordinate is not a valid integer");
			return;
		}
		int num3;
		if (!StringParsers.TryParseSInt32(_params[3], out num3))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("z1 coordinate is not a valid integer");
			return;
		}
		Vector3i vector3i = new Vector3i(num, num2, num3);
		if (!StringParsers.TryParseSInt32(_params[4], out num))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("x2 coordinate is not a valid integer");
			return;
		}
		if (!StringParsers.TryParseSInt32(_params[5], out num2))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("y2 coordinate is not a valid integer");
			return;
		}
		if (!StringParsers.TryParseSInt32(_params[6], out num3))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("z2 coordinate is not a valid integer");
			return;
		}
		Vector3i vector3i2 = new Vector3i(num, num2, num3);
		if (Prefab.PrefabExists(text))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("A prefab with the name \"" + text + "\" already exists.");
			return;
		}
		Prefab prefab = new Prefab();
		prefab.location = Prefab.LocationForNewPrefab(text, flag ? "Parts" : null);
		prefab.copyFromWorld(GameManager.Instance.World, vector3i, vector3i2);
		if (prefab.Save(prefab.location, true))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Prefab saved to " + prefab.location.ToString());
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Prefab could not be saved");
	}

	public static string BuildCommandString(string _prefabName, Vector3i _startPos, Vector3i _endPos, bool _asPart)
	{
		return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8}", new object[]
		{
			ConsoleCmdExportPrefab.CommandName,
			_prefabName,
			_startPos.x,
			_startPos.y,
			_startPos.z,
			_endPos.x,
			_endPos.y,
			_endPos.z,
			_asPart
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string CommandName = "exportprefab";
}
