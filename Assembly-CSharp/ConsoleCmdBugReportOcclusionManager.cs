using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdBugReportOcclusionManager : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "testoccreport", "toccr" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Test the occlusion manager self reporting to backtrace, requires Backtrace to be enabled at build creation";
	}

	public override bool AllowedInMainMenu
	{
		get
		{
			return false;
		}
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
		{
			return;
		}
		List<string> list;
		if (OcclusionManager.Instance.WriteListToDisk(out list))
		{
			BacktraceUtils.SendErrorReport("OcclusionManagerUsedUpAllEntries", "Occlusion Manager used all entries", list);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("testoccreport: Wrote Files to disk: " + string.Join(",", list));
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("testoccreport: Posted report to backtrace");
		}
	}
}
