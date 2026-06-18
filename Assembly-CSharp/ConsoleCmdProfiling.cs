using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdProfiling : ConsoleCmdAbstract
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
		return new string[] { "profiling" };
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
		if (this.cmdNetwork == null)
		{
			this.cmdNetwork = SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommand("profilenetwork", true) as ConsoleCmdProfileNetwork;
		}
		if (_params.Count != 1 || !_params[0].EqualsCaseInsensitive("stop"))
		{
			if (!Profiler.enabled)
			{
				this.profileNetwork = true;
				int num = 300;
				if (_params.Count > 0 && !int.TryParse(_params[0], out num))
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Not a valid integer for number of frames (\"{0}\")", num));
					return;
				}
				if (num < 10 || num > 3000)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Number of frames needs to be within {0} and {1}", 10, 3000));
					return;
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Enabled profiling for {0} frames (typically 5 - 10 seconds)", num));
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Profiler mem: {0}", Profiler.maxUsedMemory));
				Profiler.logFile = string.Format("{0}/profiling_{1:yyyy-MM-dd_HH-mm-ss}_unity.log", GameIO.GetApplicationPath(), DateTime.Now);
				Profiler.enableBinaryLog = true;
				Profiler.enabled = true;
				ThreadManager.StartCoroutine(this.stopProfilingLater(num));
				if (this.profileNetwork)
				{
					ConsoleCmdProfileNetwork consoleCmdProfileNetwork = this.cmdNetwork;
					if (consoleCmdProfileNetwork == null)
					{
						return;
					}
					consoleCmdProfileNetwork.resetData();
				}
			}
			return;
		}
		if (!Profiler.enabled)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Profiling not running.");
			return;
		}
		this.stopProfiling();
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Profiling stopped.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator stopProfilingLater(int _frames)
	{
		int i = 0;
		while (i < _frames && Profiler.enabled)
		{
			yield return null;
			int num = i;
			i = num + 1;
		}
		if (Profiler.enabled)
		{
			this.stopProfiling();
			Log.Out("Profiling done");
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stopProfiling()
	{
		Profiler.enabled = false;
		Profiler.logFile = null;
		if (this.profileNetwork)
		{
			ConsoleCmdProfileNetwork consoleCmdProfileNetwork = this.cmdNetwork;
			if (consoleCmdProfileNetwork == null)
			{
				return;
			}
			consoleCmdProfileNetwork.doProfileNetwork();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Enable Unity profiling for 300 frames";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ConsoleCmdProfileNetwork cmdNetwork;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool profileNetwork;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int FramesDefault = 300;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int FramesMin = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int FramesMax = 3000;
}
