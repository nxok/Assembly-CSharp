using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdProfiler : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "profiler" };
	}

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
	public override string getHelp()
	{
		return "";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
			return;
		}
		string text = _params[0];
		if (!(text == "listrawmetrics"))
		{
			if (!(text == "mem"))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
			}
			else
			{
				if (this.memMetrics == null)
				{
					this.InitMemoryMetrics();
				}
				if (_params.Count == 1)
				{
					this.LogPretty(this.memMetrics);
					return;
				}
				if (_params.Count == 2)
				{
					string text2 = _params[1];
					if (text2 == "csv")
					{
						this.LogCsv(this.memMetrics);
						return;
					}
					if (!(text2 == "pretty"))
					{
						return;
					}
					this.LogPretty(this.memMetrics);
					return;
				}
			}
			return;
		}
		Log.Out(ProfilerUtils.GetAvailableMetricsCsv());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitMemoryMetrics()
	{
		if (this.memMetrics != null)
		{
			this.memMetrics.Cleanup();
		}
		this.memMetrics = ProfilerCaptureUtils.CreateMemoryProfiler();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogCsv(ProfilingMetricCapture _metrics)
	{
		ThreadManager.StartCoroutine(this.LogCsvNextFrame(_metrics));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator LogCsvNextFrame(ProfilingMetricCapture _metrics)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		Log.Out(_metrics.GetCsvHeader());
		Log.Out(_metrics.GetLastValueCsv());
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogPretty(ProfilingMetricCapture _metrics)
	{
		ThreadManager.StartCoroutine(this.LogPrettyNextFrame(_metrics));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator LogPrettyNextFrame(ProfilingMetricCapture _metrics)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		Log.Out(_metrics.PrettyPrint());
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Utilities for collection profiling data from a variety of sources";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilingMetricCapture memMetrics;
}
