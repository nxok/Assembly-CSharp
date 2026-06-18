using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSDCS : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Change a player's sex, race, and variant:\n  Usage:\n    sdcs                            : Show current archetype values\n    sdcs <sex|race|variant> <value> : Set the specified value\n  Examples :\n    sdcs sex <male|female>\n    sdcs race <white|black|asian|native>\n    sdcs variant <1|2|3|4>";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "sdcs" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Control entity sex, race, and variant";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.Instance.World.GetLocalPlayers().Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No local players found");
			return;
		}
		EModelSDCS component = GameManager.Instance.World.GetLocalPlayers()[0].GetComponent<EModelSDCS>();
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Concat(new string[]
			{
				"Current Archetype Values:\n  Sex: ",
				component.Archetype.Sex,
				"\n  Race: ",
				component.Archetype.Race,
				"\n  Variant: ",
				component.Archetype.Variant.ToString()
			}));
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
			return;
		}
		ConsoleCmdSDCS.cTypes cTypes;
		if (!Enum.TryParse<ConsoleCmdSDCS.cTypes>(_params[0], true, out cTypes))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid control type");
			return;
		}
		string text = _params[1];
		if (component == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No SDCS model found");
			return;
		}
		switch (cTypes)
		{
		case ConsoleCmdSDCS.cTypes.Sex:
		{
			ConsoleCmdSDCS.sTypes sTypes;
			if (!Enum.TryParse<ConsoleCmdSDCS.sTypes>(_params[1], true, out sTypes))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid sex '" + _params[1] + "'");
				return;
			}
			bool flag = false;
			if (sTypes == ConsoleCmdSDCS.sTypes.Male)
			{
				flag = true;
			}
			component.SetSex(flag);
			return;
		}
		case ConsoleCmdSDCS.cTypes.Race:
		{
			ConsoleCmdSDCS.rTypes rTypes;
			if (!Enum.TryParse<ConsoleCmdSDCS.rTypes>(_params[1], true, out rTypes))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid race '" + _params[1] + "'");
				return;
			}
			component.SetRace(rTypes.ToString());
			return;
		}
		case ConsoleCmdSDCS.cTypes.Variant:
		{
			int num;
			if (!StringParsers.TryParseSInt32(_params[1], out num))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid variant number " + _params[1]);
				return;
			}
			if (num < 1 || num > 4)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Invalid variant number {0}", num));
				return;
			}
			component.SetVariant(num);
			return;
		}
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum cTypes
	{
		Sex,
		Race,
		Variant
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum rTypes
	{
		White,
		Black,
		Asian,
		Native
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum sTypes
	{
		Male,
		Female
	}
}
