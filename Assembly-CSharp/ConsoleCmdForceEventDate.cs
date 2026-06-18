using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdForceEventDate : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "ForceEventDate" };
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return true;
		}
	}

	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Specify date for testing event dates";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Current forced date: " + ((EventsFromXml.ForceTestDateTime == DateTime.MinValue) ? "-none-" : EventsFromXml.ForceTestDateTime.ToShortDateString()));
			return;
		}
		string text = _params[0];
		DateTime minValue;
		if (text == "now")
		{
			minValue = DateTime.MinValue;
		}
		else if (!EventsFromXml.TryParseDate(text, out minValue))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Failed parsing date argument, must be in the form 'mm/dd'");
			return;
		}
		EventsFromXml.ForceTestDateTime = minValue;
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Forced date: " + minValue.ToShortDateString());
		foreach (KeyValuePair<string, EventsFromXml.EventDefinition> keyValuePair in EventsFromXml.Events)
		{
			string text2;
			EventsFromXml.EventDefinition eventDefinition;
			keyValuePair.Deconstruct(out text2, out eventDefinition);
			EventsFromXml.EventDefinition eventDefinition2 = eventDefinition;
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Name={0}, Start={1}, End={2}, Active={3}", new object[] { eventDefinition2.Name, eventDefinition2.Start, eventDefinition2.End, eventDefinition2.Active }));
		}
	}
}
