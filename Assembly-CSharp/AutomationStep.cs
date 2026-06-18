using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

[Serializable]
public class AutomationStep
{
	public static AutomationStep LoadGame(string world, string gameName)
	{
		return new AutomationStep
		{
			type = AutomationStep.StepType.LoadGame,
			world = world,
			gameName = gameName
		};
	}

	public static AutomationStep PreparePlayer()
	{
		return new AutomationStep
		{
			type = AutomationStep.StepType.PreparePlayer
		};
	}

	public static AutomationStep Teleport(Vector3 pos, Vector3 look)
	{
		return new AutomationStep
		{
			type = AutomationStep.StepType.Teleport,
			position = pos,
			lookDir = look
		};
	}

	public static AutomationStep Wait(float secs)
	{
		return new AutomationStep
		{
			type = AutomationStep.StepType.Wait,
			duration = secs
		};
	}

	public static AutomationStep MoveLine(Vector3 dest, float dur)
	{
		return new AutomationStep
		{
			type = AutomationStep.StepType.MoveLine,
			position = dest,
			duration = dur
		};
	}

	public static AutomationStep MoveLine(Vector3 start, Vector3 dest, float dur)
	{
		return new AutomationStep
		{
			type = AutomationStep.StepType.MoveLine,
			position = dest,
			positionB = start,
			hasStart = true,
			duration = dur
		};
	}

	public static AutomationStep MovePingPong(Vector3 a, Vector3 b, int count, float legDuration)
	{
		return new AutomationStep
		{
			type = AutomationStep.StepType.MovePingPong,
			position = a,
			positionB = b,
			pingPongCount = Mathf.Max(1, count),
			duration = legDuration
		};
	}

	public static AutomationStep Orbit(Vector3 center, float dur, bool lookForward = true, bool isFlipped = false)
	{
		return new AutomationStep
		{
			type = AutomationStep.StepType.Orbit,
			position = center,
			duration = dur,
			lookForward = lookForward,
			isFlipped = isFlipped
		};
	}

	public static AutomationStep WaitForChunksLoaded()
	{
		return new AutomationStep
		{
			type = AutomationStep.StepType.WaitForChunksLoaded
		};
	}

	public static AutomationStep StartPerfSession(int runs)
	{
		return new AutomationStep
		{
			type = AutomationStep.StepType.StartPerfSession,
			runCount = runs
		};
	}

	public static AutomationStep StopPerfSession()
	{
		return new AutomationStep
		{
			type = AutomationStep.StepType.StopPerfSession
		};
	}

	public static AutomationStep StartPerfCapture(int fps = 30, string prefix = "")
	{
		return new AutomationStep
		{
			type = AutomationStep.StepType.StartPerfCapture,
			targetFps = fps,
			capturePrefix = prefix
		};
	}

	public static AutomationStep StopPerfCapture()
	{
		return new AutomationStep
		{
			type = AutomationStep.StepType.StopPerfCapture
		};
	}

	public static AutomationStep ExitToMenu()
	{
		return new AutomationStep
		{
			type = AutomationStep.StepType.ExitToMenu
		};
	}

	public static AutomationStep Cleanup()
	{
		return new AutomationStep
		{
			type = AutomationStep.StepType.Cleanup
		};
	}

	public static AutomationStep AutomationComplete(string url = "")
	{
		return new AutomationStep
		{
			type = AutomationStep.StepType.AutomationComplete,
			url = url
		};
	}

	public static AutomationStep LogError(string text = "")
	{
		return new AutomationStep
		{
			type = AutomationStep.StepType.LogError,
			text = text
		};
	}

	public static AutomationStep DeleteSave(string world, string gameName)
	{
		return new AutomationStep
		{
			type = AutomationStep.StepType.DeleteSave,
			world = world,
			gameName = gameName
		};
	}

	public static AutomationStep ConsoleCmd(string command)
	{
		return new AutomationStep
		{
			type = AutomationStep.StepType.ConsoleCmd,
			text = command
		};
	}

	public string Describe(int index)
	{
		string text;
		switch (this.type)
		{
		case AutomationStep.StepType.LoadGame:
			text = string.Concat(new string[] { "LoadGame     world='", this.world, "'  game='", this.gameName, "'" });
			break;
		case AutomationStep.StepType.PreparePlayer:
			text = "PreparePlayer  (god + fly + freeze time)";
			break;
		case AutomationStep.StepType.Teleport:
			text = "Teleport     pos=" + AutomationStep.Fmt(this.position) + "  look=" + AutomationStep.Fmt(this.lookDir);
			break;
		case AutomationStep.StepType.Wait:
			text = string.Format("Wait         {0:F2}s", this.duration);
			break;
		case AutomationStep.StepType.MoveLine:
			text = (this.hasStart ? string.Format("MoveLine  {0} -> {1}  over {2:F2}s", AutomationStep.Fmt(this.positionB), AutomationStep.Fmt(this.position), this.duration) : string.Format("MoveLine  →  {0}  over {1:F2}s", AutomationStep.Fmt(this.position), this.duration));
			break;
		case AutomationStep.StepType.Orbit:
			text = string.Format("Orbit     around {0}  for {1:F2}s  lookForward={2}  isFlipped={3}", new object[]
			{
				AutomationStep.Fmt(this.position),
				this.duration,
				this.lookForward,
				this.isFlipped
			});
			break;
		case AutomationStep.StepType.WaitForChunksLoaded:
			text = "WaitForChunksLoaded  (wait until chunks around player are displayed)";
			break;
		case AutomationStep.StepType.StartPerfSession:
			text = string.Format("StartPerfSession  runs={0}", this.runCount);
			break;
		case AutomationStep.StepType.StopPerfSession:
			text = "StopPerfSession";
			break;
		case AutomationStep.StepType.StartPerfCapture:
			text = string.Format("  StartPerfCapture  fps={0}  prefix='{1}'", this.targetFps, this.capturePrefix);
			break;
		case AutomationStep.StepType.StopPerfCapture:
			text = "  StopPerfCapture";
			break;
		case AutomationStep.StepType.ExitToMenu:
			text = "ExitToMenu  (disconnect and return to main menu)";
			break;
		case AutomationStep.StepType.Cleanup:
			text = "Cleanup  (unregister event handlers)";
			break;
		case AutomationStep.StepType.AutomationComplete:
			text = "AutomationComplete  url='" + this.url + "'";
			break;
		case AutomationStep.StepType.LogError:
			text = "LogError text='" + this.text + "'";
			break;
		case AutomationStep.StepType.DeleteSave:
			text = string.Concat(new string[] { "DeleteSave     world='", this.world, "'  game='", this.gameName, "'" });
			break;
		case AutomationStep.StepType.ConsoleCmd:
			text = "ConsoleCmd     command='" + this.text + "'";
			break;
		case AutomationStep.StepType.MovePingPong:
			text = string.Format("MovePingPong  {0} ↔ {1}  ×{2}  ({3:F2}s/leg)", new object[]
			{
				AutomationStep.Fmt(this.position),
				AutomationStep.Fmt(this.positionB),
				this.pingPongCount,
				this.duration
			});
			break;
		default:
			text = string.Format("??? Unknown type: '{0}'", this.type);
			break;
		}
		string text2 = text;
		return string.Format("  [{0:D2}] {1}", index, text2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string Fmt(Vector3 v)
	{
		return string.Format("({0:F1}, {1:F1}, {2:F1})", v.x, v.y, v.z);
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public AutomationStep.StepType type;

	public string world;

	public string gameName;

	public Vector3 position;

	public Vector3 positionB;

	public Vector3 lookDir;

	public float duration;

	public bool lookForward = true;

	public bool isFlipped;

	public bool hasStart;

	public int runCount = 1;

	public int pingPongCount = 1;

	public int targetFps = 30;

	public string capturePrefix = string.Empty;

	public string url = string.Empty;

	public string text = string.Empty;

	public enum StepType
	{
		LoadGame,
		PreparePlayer,
		Teleport,
		Wait,
		MoveLine,
		Orbit,
		WaitForChunksLoaded,
		StartPerfSession,
		StopPerfSession,
		StartPerfCapture,
		StopPerfCapture,
		ExitToMenu,
		Cleanup,
		AutomationComplete,
		LogError,
		DeleteSave,
		ConsoleCmd,
		MovePingPong
	}
}
