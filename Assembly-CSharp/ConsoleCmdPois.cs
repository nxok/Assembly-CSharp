using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPois : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "pois" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Switches distant POIs on/off";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Use on or off or only the command to toggle";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		GameObject gameObject = GameObject.Find("/PrefabsLOD");
		if (gameObject != null)
		{
			ConsoleCmdPois.parentGO = gameObject;
		}
		if (ConsoleCmdPois.parentGO == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Distant POIs not active!");
			return;
		}
		if (_params.Count == 0)
		{
			ConsoleCmdPois.parentGO.SetActive(!ConsoleCmdPois.parentGO.activeSelf);
		}
		else if (_params[0] == "on")
		{
			ConsoleCmdPois.parentGO.SetActive(true);
		}
		else if (_params[0] == "off")
		{
			ConsoleCmdPois.parentGO.SetActive(true);
		}
		else
		{
			int num;
			if (int.TryParse(_params[0], out num))
			{
				GameManager.Instance.prefabLODManager.SetPOIDistance(128 * num);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Concat(new string[]
				{
					"Setting to POI chunk distance ",
					num.ToString(),
					" =",
					(128 * num).ToString(),
					"m"
				}));
				return;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown parameter");
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("POIs set to " + (ConsoleCmdPois.parentGO.activeSelf ? "on" : "off"));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject parentGO;
}
