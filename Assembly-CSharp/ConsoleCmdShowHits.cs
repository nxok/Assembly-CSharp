using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdShowHits : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "showhits" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Show hit entity info";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Commands:\ndamage - toggle damage numbers\nhits <time> <size> - toggle hits";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
			return;
		}
		string text = _params[0].ToLower();
		if (text == "damage")
		{
			DamageText.Enabled = !DamageText.Enabled;
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Damage " + (DamageText.Enabled ? "on" : "off"));
			return;
		}
		if (!(text == "hits"))
		{
			return;
		}
		EntityAlive.ShowDebugDisplayHit = !EntityAlive.ShowDebugDisplayHit;
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Hits " + (EntityAlive.ShowDebugDisplayHit ? "on" : "off"));
		if (_params.Count >= 2)
		{
			EntityAlive.DebugDisplayHitTime = StringParsers.ParseFloat(_params[1], 0, -1, NumberStyles.Any);
		}
		if (_params.Count >= 3)
		{
			EntityAlive.DebugDisplayHitSize = StringParsers.ParseFloat(_params[2], 0, -1, NumberStyles.Any);
		}
		ItemAction.ShowDebugDisplayHit = EntityAlive.ShowDebugDisplayHit;
		ItemAction.DebugDisplayHitTime = EntityAlive.DebugDisplayHitTime;
		ItemAction.DebugDisplayHitSize = EntityAlive.DebugDisplayHitSize;
	}
}
