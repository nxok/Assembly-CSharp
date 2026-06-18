using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSetGameStat : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "setgamestat", "sgs" };
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return true;
		}
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count != 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Parameters: <game stat> <value>");
			return;
		}
		EnumGameStats enumGameStats;
		try
		{
			enumGameStats = EnumUtils.Parse<EnumGameStats>(_params[0], true);
		}
		catch (Exception)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Error parsing parameter: " + _params[0]);
			return;
		}
		object obj;
		try
		{
			obj = GameStats.Parse(enumGameStats, _params[1]);
		}
		catch (Exception)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Error parsing value: " + _params[1]);
			return;
		}
		GameStats.SetObject(enumGameStats, obj);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(enumGameStats.ToStringCached<EnumGameStats>() + " set to " + ((obj != null) ? obj.ToString() : null));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "sets a game stat";
	}
}
