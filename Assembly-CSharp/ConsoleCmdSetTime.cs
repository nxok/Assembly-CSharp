using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSetTime : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "settime", "st" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Set the current game time";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Set the current game time.\nUsage:\n  1. settime day\n  2. settime night\n  3. settime <time>\n  4. settime <day> <hour> <minute>\n1. sets the time to day 1, 12:00 pm.\n2. sets the time to day 2, 12:00 am.\n3. sets the time to the given value. 1000 is one hour.\n4. sets the time to the given day/hour/minute values.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		ulong num = 0UL;
		if (_params.Count == 1)
		{
			if (_params[0].EqualsCaseInsensitive("day"))
			{
				num = GameUtils.DayTimeToWorldTime(1, 12, 0);
			}
			else if (_params[0].EqualsCaseInsensitive("night"))
			{
				num = GameUtils.DayTimeToWorldTime(2, 0, 0);
			}
			else if (!ulong.TryParse(_params[0], out num))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid value for single argument variant: \"" + _params[0] + "\"");
				return;
			}
		}
		else
		{
			if (_params.Count != 3)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 1 or 3, found " + _params.Count.ToString() + ".");
				return;
			}
			int num2;
			if (!int.TryParse(_params[0], out num2))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[0] + "\" is not a valid integer.");
				return;
			}
			int num3;
			if (!int.TryParse(_params[1], out num3))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[1] + "\" is not a valid integer.");
				return;
			}
			int num4;
			if (!int.TryParse(_params[2], out num4))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("\"" + _params[2] + "\" is not a valid integer.");
				return;
			}
			if (num2 < 1)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Day must be >= 1");
				return;
			}
			if (num3 > 23)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Hour must be <= 23");
				return;
			}
			if (num4 > 59)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Minute must be <= 59");
				return;
			}
			num = GameUtils.DayTimeToWorldTime(num2, num3, num4);
		}
		GameManager.Instance.World.SetTimeJump(num, false);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Set time to " + num.ToString());
	}
}
