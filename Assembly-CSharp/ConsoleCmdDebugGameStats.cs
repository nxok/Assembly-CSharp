using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdDebugGameStats : ConsoleCmdAbstract
{
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
		return new string[] { "debuggamestats" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "GameStats commands";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count >= 1)
		{
			if (_params[0].ToUpper() == "LOG")
			{
				bool flag;
				if (_params.Count >= 2 && bool.TryParse(_params[1], out flag))
				{
					if (flag)
					{
						this.logFile = Path.Join(PlatformApplicationManager.Application.temporaryCachePath, "GameStats.tsv");
						this.stringBuilder = new StringBuilder();
						this.stringBuilder.AppendLine(DebugGameStats.GetHeader('\t'));
						File.AppendAllText(this.logFile, this.stringBuilder.ToString());
						DebugGameStats.StartStatisticsUpdate(new DebugGameStats.StatisticsUpdatedCallback(this.logDebugGameStats));
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Started logging debug stats to " + this.logFile + ".");
						return;
					}
					DebugGameStats.StopStatisticsUpdate();
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Stopped logging debug stats to " + this.logFile + ".");
					return;
				}
			}
			else if (_params[0].ToUpper() == "PRINT")
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(DebugGameStats.GetHeader(','));
				stringBuilder.AppendLine(DebugGameStats.GetCurrentStatsString(','));
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(stringBuilder.ToString());
				return;
			}
		}
		Log.Out("Incorrect params, expected 'log [true|false]' or 'print'");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void logDebugGameStats(Dictionary<string, string> statisticsDictionary)
	{
		this.stringBuilder.Clear();
		foreach (KeyValuePair<string, string> keyValuePair in statisticsDictionary)
		{
			this.stringBuilder.Append(keyValuePair.Value);
			this.stringBuilder.Append('\t');
		}
		this.stringBuilder.AppendLine();
		File.AppendAllText(this.logFile, this.stringBuilder.ToString());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string logFile;

	[PublicizedFrom(EAccessModifier.Private)]
	public StringBuilder stringBuilder;
}
