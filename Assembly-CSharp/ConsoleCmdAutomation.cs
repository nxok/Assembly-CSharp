using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdAutomation : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "automation", "auto" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Automation Script Runner";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Type 'auto' in the console or see source XML doc for full usage.";
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

	public override int DefaultPermissionLevel
	{
		get
		{
			return 1000;
		}
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			this.CmdShow();
			return;
		}
		string text = _params[0].ToLower();
		uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
		if (num <= 993596020U)
		{
			if (num <= 681154065U)
			{
				if (num != 217798785U)
				{
					if (num == 681154065U)
					{
						if (text == "new")
						{
							this.CmdNew(_params);
							return;
						}
					}
				}
				else if (text == "list")
				{
					this.CmdList();
					return;
				}
			}
			else if (num != 718098122U)
			{
				if (num == 993596020U)
				{
					if (text == "add")
					{
						this.CmdAdd(_params);
						return;
					}
				}
			}
			else if (text == "run")
			{
				this.CmdRun(_params);
				return;
			}
		}
		else if (num <= 2840060476U)
		{
			if (num != 2771110649U)
			{
				if (num == 2840060476U)
				{
					if (text == "show")
					{
						this.CmdShow();
						return;
					}
				}
			}
			else if (text == "abort")
			{
				this.CmdAbort();
				return;
			}
		}
		else if (num != 3439296072U)
		{
			if (num != 3683784189U)
			{
				if (num == 3859241449U)
				{
					if (text == "load")
					{
						this.CmdLoad(_params);
						return;
					}
				}
			}
			else if (text == "remove")
			{
				this.CmdRemove(_params);
				return;
			}
		}
		else if (text == "save")
		{
			this.CmdSave(_params);
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown sub-command '" + _params[0] + "'. See 'help auto'.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdNew(List<string> p)
	{
		if (!this.RequireNotRunning())
		{
			return;
		}
		AutomationRunner.Instance.LoadScriptUnchecked(new AutomationScript());
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("New empty script created.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdShow()
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(AutomationRunner.Instance.CurrentScript.Describe());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdRemove(List<string> p)
	{
		if (!this.RequireNotRunning())
		{
			return;
		}
		int num;
		if (p.Count < 2 || !int.TryParse(p[1], out num))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: auto remove <index>");
			return;
		}
		List<AutomationStep> steps = AutomationRunner.Instance.CurrentScript.steps;
		if (num < 0 || num >= steps.Count)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Index {0} out of range (0–{1}).", num, steps.Count - 1));
			return;
		}
		string text = steps[num].Describe(num);
		steps.RemoveAt(num);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Removed step: " + text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdSave(List<string> p)
	{
		if (p.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: auto save <name>");
			return;
		}
		AutomationRunner.Instance.CurrentScript.SaveToFile(p[1]);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Script saved as '" + p[1] + "'.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdLoad(List<string> p)
	{
		if (!this.RequireNotRunning())
		{
			return;
		}
		if (p.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: auto load <name>");
			return;
		}
		AutomationScript automationScript = AutomationScript.LoadFromFile(p[1]);
		if (automationScript == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Failed to load script '" + p[1] + "'. Check log for details.");
			return;
		}
		if (!AutomationRunner.Instance.LoadScript(automationScript))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Script failed validation. Check log for details.");
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(automationScript.Describe());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdList()
	{
		List<string> list = AutomationScript.ListSavedScripts();
		if (list.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No saved scripts found.");
			return;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Saved scripts:\n" + string.Join("\n", list.ConvertAll<string>((string n) => "  " + n)));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdAdd(List<string> p)
	{
		if (!this.RequireNotRunning())
		{
			return;
		}
		if (p.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: auto add <type> [args]");
			return;
		}
		string text = p[1].ToLower();
		uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
		AutomationStep automationStep;
		if (num <= 2324817210U)
		{
			if (num <= 1439362434U)
			{
				if (num <= 719351077U)
				{
					if (num != 51507210U)
					{
						if (num == 719351077U)
						{
							if (text == "waitforchunks")
							{
								automationStep = AutomationStep.WaitForChunksLoaded();
								goto IL_0962;
							}
						}
					}
					else if (text == "startperfcapture")
					{
						int num2;
						automationStep = AutomationStep.StartPerfCapture((p.Count > 2 && int.TryParse(p[2], out num2)) ? num2 : 30, "");
						goto IL_0962;
					}
				}
				else if (num != 1019769052U)
				{
					if (num == 1439362434U)
					{
						if (text == "complete")
						{
							automationStep = AutomationStep.AutomationComplete("");
							goto IL_0962;
						}
					}
				}
				else if (text == "prepare")
				{
					automationStep = AutomationStep.PreparePlayer();
					goto IL_0962;
				}
			}
			else if (num <= 2077554967U)
			{
				if (num != 1837547875U)
				{
					if (num == 2077554967U)
					{
						if (text == "deletesave")
						{
							automationStep = AutomationStep.DeleteSave(p[2], p[3]);
							goto IL_0962;
						}
					}
				}
				else if (text == "loadgame")
				{
					if (p.Count < 4)
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: auto add loadgame <world> <gameName>");
						return;
					}
					automationStep = AutomationStep.LoadGame(p[2], p[3]);
					goto IL_0962;
				}
			}
			else if (num != 2301512864U)
			{
				if (num == 2324817210U)
				{
					if (text == "stopperfsession")
					{
						automationStep = AutomationStep.StopPerfSession();
						goto IL_0962;
					}
				}
			}
			else if (text == "wait")
			{
				float num3;
				if (p.Count < 3 || !float.TryParse(p[2], out num3))
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: auto add wait <secs>");
					return;
				}
				automationStep = AutomationStep.Wait(num3);
				goto IL_0962;
			}
		}
		else if (num <= 3454868101U)
		{
			if (num <= 3254955298U)
			{
				if (num != 2759670435U)
				{
					if (num == 3254955298U)
					{
						if (text == "consolecmd")
						{
							automationStep = AutomationStep.ConsoleCmd(string.Join(" ", p.Skip(2)));
							goto IL_0962;
						}
					}
				}
				else if (text == "pingpong")
				{
					if (!this.RequireWorld())
					{
						return;
					}
					string text2 = ((p.Count > 2) ? p[2].ToLower() : "");
					if (text2 == "p1")
					{
						this._pendingStartPoint = new Vector3?(this.PlayerPos());
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Point A captured: " + ConsoleCmdAutomation.Fmt(this._pendingStartPoint.Value));
						return;
					}
					if (text2 == "clear")
					{
						this._pendingStartPoint = null;
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Pending point A cleared.");
						return;
					}
					if (text2 == "show")
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output((this._pendingStartPoint != null) ? ("Pending point A: " + ConsoleCmdAutomation.Fmt(this._pendingStartPoint.Value)) : "No pending point A.");
						return;
					}
					int num4;
					Vector3 value;
					Vector3 vector;
					float num5;
					if (text2 == "p2")
					{
						if (this._pendingStartPoint == null)
						{
							SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No point A yet. Run 'auto add pingpong p1' first.");
							return;
						}
						if (p.Count < 4 || !int.TryParse(p[3], out num4) || num4 < 1)
						{
							SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: auto add pingpong p2 <count> [secsPerLeg]");
							return;
						}
						value = this._pendingStartPoint.Value;
						vector = this.PlayerPos();
						float num6;
						num5 = ((p.Count >= 5 && float.TryParse(p[4], out num6)) ? num6 : this.DefaultLegDuration(value, vector));
						this._pendingStartPoint = null;
					}
					else
					{
						if (p.Count < 9 || !ConsoleCmdAutomation.TryParseVec3(p, 2, out value) || !ConsoleCmdAutomation.TryParseVec3(p, 5, out vector) || !int.TryParse(p[8], out num4) || num4 < 1)
						{
							SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage:\n  auto add pingpong p1\n  auto add pingpong p2 <count> [secsPerLeg]\n  auto add pingpong <x1> <y1> <z1> <x2> <y2> <z2> <count> [secsPerLeg]\n  auto add pingpong show | clear");
							return;
						}
						float num7;
						num5 = ((p.Count >= 10 && float.TryParse(p[9], out num7)) ? num7 : this.DefaultLegDuration(value, vector));
					}
					if (value == vector)
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Point A and point B are identical — step not added.");
						return;
					}
					automationStep = AutomationStep.MovePingPong(value, vector, num4, num5);
					goto IL_0962;
				}
			}
			else if (num != 3289626814U)
			{
				if (num == 3454868101U)
				{
					if (text == "exit")
					{
						automationStep = AutomationStep.ExitToMenu();
						goto IL_0962;
					}
				}
			}
			else if (text == "teleport")
			{
				Vector3 vector2;
				if (p.Count >= 5 && ConsoleCmdAutomation.TryParseVec3(p, 2, out vector2))
				{
					automationStep = AutomationStep.Teleport(vector2, this.PlayerForward());
					goto IL_0962;
				}
				if (!this.RequireWorld())
				{
					return;
				}
				automationStep = AutomationStep.Teleport(this.PlayerPos(), this.PlayerForward());
				goto IL_0962;
			}
		}
		else if (num <= 3946618703U)
		{
			if (num != 3646812595U)
			{
				if (num == 3946618703U)
				{
					if (text == "orbit")
					{
						Vector3 vector3;
						float num8;
						if (p.Count >= 6 && ConsoleCmdAutomation.TryParseVec3(p, 2, out vector3) && float.TryParse(p[5], out num8))
						{
							bool flag = p.Count < 7 || p[6].ToLower() != "false";
							automationStep = AutomationStep.Orbit(vector3, num8, flag, false);
							goto IL_0962;
						}
						float num9;
						if (p.Count < 3 || !float.TryParse(p[2], out num9))
						{
							SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: auto add orbit <secs>  OR  orbit <x> <y> <z> <secs> [true|false]");
							return;
						}
						if (!this.RequireWorld())
						{
							return;
						}
						automationStep = AutomationStep.Orbit(this.PlayerPos(), num9, true, false);
						goto IL_0962;
					}
				}
			}
			else if (text == "cleanup")
			{
				automationStep = AutomationStep.Cleanup();
				goto IL_0962;
			}
		}
		else if (num != 4025278982U)
		{
			if (num != 4071203716U)
			{
				if (num == 4164204148U)
				{
					if (text == "stopperfcapture")
					{
						automationStep = AutomationStep.StopPerfCapture();
						goto IL_0962;
					}
				}
			}
			else if (text == "startperfsession")
			{
				int num10;
				if (p.Count < 3 || !int.TryParse(p[2], out num10) || num10 < 1)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage: auto add startperfsession <runs>");
					return;
				}
				automationStep = AutomationStep.StartPerfSession(num10);
				goto IL_0962;
			}
		}
		else if (text == "moveline")
		{
			string text3 = ((p.Count > 2) ? p[2].ToLower() : "");
			if (text3 == "p1")
			{
				if (!this.RequireWorld())
				{
					return;
				}
				this._pendingStartPoint = new Vector3?(this.PlayerPos());
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Start point captured: " + ConsoleCmdAutomation.Fmt(this._pendingStartPoint.Value));
				return;
			}
			else
			{
				if (text3 == "clear")
				{
					this._pendingStartPoint = null;
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Pending start point cleared.");
					return;
				}
				if (text3 == "show")
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output((this._pendingStartPoint != null) ? ("Pending start point: " + ConsoleCmdAutomation.Fmt(this._pendingStartPoint.Value)) : "No pending start point.");
					return;
				}
				if (text3 == "p2")
				{
					if (!this.RequireWorld())
					{
						return;
					}
					if (this._pendingStartPoint == null)
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No start point yet. Run 'auto add moveline p1' first.");
						return;
					}
					Vector3 value2 = this._pendingStartPoint.Value;
					Vector3 vector4 = this.PlayerPos();
					float num12;
					float num11 = ((p.Count >= 4 && float.TryParse(p[3], out num12)) ? num12 : this.DefaultLegDuration(value2, vector4));
					if (value2 == vector4)
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Start and end are identical — step not added.");
						return;
					}
					automationStep = AutomationStep.MoveLine(value2, vector4, num11);
					this._pendingStartPoint = null;
					goto IL_0962;
				}
				else
				{
					Vector3 vector5;
					float num13;
					if (p.Count >= 6 && ConsoleCmdAutomation.TryParseVec3(p, 2, out vector5) && float.TryParse(p[5], out num13))
					{
						automationStep = AutomationStep.MoveLine(vector5, num13);
						goto IL_0962;
					}
					Vector3 vector6;
					if (p.Count >= 5 && ConsoleCmdAutomation.TryParseVec3(p, 2, out vector6))
					{
						if (!this.RequireWorld())
						{
							return;
						}
						automationStep = AutomationStep.MoveLine(vector6, this.DefaultLegDuration(this.PlayerPos(), vector6));
						goto IL_0962;
					}
					else
					{
						float num14;
						if (p.Count < 3 || !float.TryParse(p[2], out num14))
						{
							SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Usage:\n  auto add moveline <secs>\n  auto add moveline <x> <y> <z> [secs]\n  auto add moveline p1           (capture current pos as start)\n  auto add moveline p2 [secs]    (current pos is end; requires p1)\n  auto add moveline show | clear");
							return;
						}
						if (!this.RequireWorld())
						{
							return;
						}
						automationStep = AutomationStep.MoveLine(this.PlayerPos(), num14);
						goto IL_0962;
					}
				}
			}
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown step type '" + p[1] + "'. Valid: loadgame, prepare, teleport, wait, waitforchunks, moveline, pingpong, orbit, startperfsession, stopperfsession, startperfcapture, stopperfcapture, exit, cleanup, quit.");
		return;
		IL_0962:
		AutomationScript currentScript = AutomationRunner.Instance.CurrentScript;
		currentScript.steps.Add(automationStep);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Added: " + automationStep.Describe(currentScript.steps.Count - 1));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdRun(List<string> p)
	{
		if (AutomationRunner.Instance.IsRunning)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Already running. Use 'auto abort'.");
			return;
		}
		AutomationRunner.Instance.StartRuns();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmdAbort()
	{
		AutomationRunner.Instance.Abort();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool RequireWorld()
	{
		if (GameManager.Instance.World != null)
		{
			return true;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("World must be loaded to use this command.");
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool RequireNotRunning()
	{
		if (!AutomationRunner.Instance.IsRunning)
		{
			return true;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Cannot edit script while a session is running.");
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float DefaultLegDuration(Vector3 a, Vector3 b)
	{
		return Mathf.Max(0.25f, Vector3.Distance(a, b) / 10f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string Fmt(Vector3 v)
	{
		return string.Format("({0:F1}, {1:F1}, {2:F1})", v.x, v.y, v.z);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 PlayerPos()
	{
		return GameManager.Instance.World.GetPrimaryPlayer().position;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 PlayerForward()
	{
		return GameManager.Instance.World.GetPrimaryPlayer().transform.forward;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool TryParseVec3(List<string> p, int startIndex, out Vector3 v)
	{
		v = Vector3.zero;
		return p.Count >= startIndex + 3 && (float.TryParse(p[startIndex], out v.x) && float.TryParse(p[startIndex + 1], out v.y)) && float.TryParse(p[startIndex + 2], out v.z);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float DefaultFlyMetersPerSec = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3? _pendingStartPoint;
}
