using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdAutoMove : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	public override DeviceFlag AllowedDeviceTypesClient
	{
		get
		{
			return DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "automove" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Player auto movement";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Parameters:\noff - disable\ngototarget - goto the target position\nsettarget - set target to current player position\nclearlookat - disable look at\nsetlookat - set look at to current player position\nline duration loops <x> <y> <z> - move to x y z (or target) over duration with loops (-loops will ping pong)\norbit duration loops <x> <y> <z> - circle around x y z (or target) over duration with loops (-loops will ping pong)\nrelative x z angle - move by x (left/right) and z (forward) and turn by angle per second";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
			return;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (!primaryPlayer)
		{
			return;
		}
		string text = _params[0].ToLower();
		uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
		if (num <= 2872740362U)
		{
			if (num <= 1260422518U)
			{
				if (num <= 400234023U)
				{
					if (num != 361777697U)
					{
						if (num != 400234023U)
						{
							goto IL_052B;
						}
						if (!(text == "line"))
						{
							goto IL_052B;
						}
						goto IL_02E7;
					}
					else
					{
						if (!(text == "cla"))
						{
							goto IL_052B;
						}
						goto IL_02CE;
					}
				}
				else if (num != 890022063U)
				{
					if (num != 1260422518U)
					{
						goto IL_052B;
					}
					if (!(text == "gt"))
					{
						goto IL_052B;
					}
					goto IL_02B3;
				}
				else if (!(text == "0"))
				{
					goto IL_052B;
				}
			}
			else
			{
				if (num <= 1918877446U)
				{
					if (num != 1263673922U)
					{
						if (num != 1918877446U)
						{
							goto IL_052B;
						}
						if (!(text == "settarget"))
						{
							goto IL_052B;
						}
					}
					else if (!(text == "st"))
					{
						goto IL_052B;
					}
					this.targetPos = primaryPlayer.GetPosition();
					return;
				}
				if (num != 2142508988U)
				{
					if (num != 2872740362U)
					{
						goto IL_052B;
					}
					if (!(text == "off"))
					{
						goto IL_052B;
					}
				}
				else
				{
					if (!(text == "clearlookat"))
					{
						goto IL_052B;
					}
					goto IL_02CE;
				}
			}
			primaryPlayer.EnableAutoMove(false);
			return;
			IL_02CE:
			this.lookAtPos = Vector3.zero;
			return;
		}
		if (num > 3926667934U)
		{
			if (num <= 4144776981U)
			{
				if (num != 3946618703U)
				{
					if (num != 4144776981U)
					{
						goto IL_052B;
					}
					if (!(text == "r"))
					{
						goto IL_052B;
					}
				}
				else
				{
					if (!(text == "orbit"))
					{
						goto IL_052B;
					}
					goto IL_03C9;
				}
			}
			else if (num != 4236415261U)
			{
				if (num != 4239927381U)
				{
					goto IL_052B;
				}
				if (!(text == "setlookat"))
				{
					goto IL_052B;
				}
				goto IL_02DA;
			}
			else if (!(text == "relative"))
			{
				goto IL_052B;
			}
			float num2 = 0f;
			if (_params.Count >= 2)
			{
				num2 = this.FloatParse(_params[1]);
			}
			float num3 = 0f;
			if (_params.Count >= 3)
			{
				num3 = this.FloatParse(_params[2]);
			}
			float num4 = 0f;
			if (_params.Count >= 4)
			{
				num4 = this.FloatParse(_params[3]);
			}
			EntityPlayerLocal.AutoMove autoMove = primaryPlayer.EnableAutoMove(true);
			autoMove.SetLookAt(this.lookAtPos);
			autoMove.StartRelative(num2, num3, num4);
			return;
		}
		if (num <= 3308801681U)
		{
			if (num != 3291586103U)
			{
				if (num != 3308801681U)
				{
					goto IL_052B;
				}
				if (!(text == "sla"))
				{
					goto IL_052B;
				}
			}
			else
			{
				if (!(text == "gototarget"))
				{
					goto IL_052B;
				}
				goto IL_02B3;
			}
		}
		else if (num != 3909890315U)
		{
			if (num != 3926667934U)
			{
				goto IL_052B;
			}
			if (!(text == "o"))
			{
				goto IL_052B;
			}
			goto IL_03C9;
		}
		else
		{
			if (!(text == "l"))
			{
				goto IL_052B;
			}
			goto IL_02E7;
		}
		IL_02DA:
		this.lookAtPos = primaryPlayer.GetPosition();
		return;
		IL_03C9:
		if (this.targetPos.sqrMagnitude == 0f && _params.Count < 6)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("orbit target is not set");
			return;
		}
		Vector3 vector = this.targetPos;
		float num5 = 5f;
		if (_params.Count >= 2)
		{
			num5 = this.FloatParse(_params[1]);
		}
		int num6 = 0;
		if (_params.Count >= 3)
		{
			num6 = this.IntParse(_params[2]);
		}
		if (_params.Count >= 4)
		{
			vector.x = this.FloatParse(_params[3]);
		}
		if (_params.Count >= 5)
		{
			vector.y = this.FloatParse(_params[4]);
		}
		if (_params.Count >= 6)
		{
			vector.z = this.FloatParse(_params[5]);
		}
		EntityPlayerLocal.AutoMove autoMove2 = primaryPlayer.EnableAutoMove(true);
		autoMove2.SetLookAt(this.lookAtPos);
		autoMove2.StartOrbit(num5, num6, vector, false, false, null);
		return;
		IL_02B3:
		primaryPlayer.SetPosition(this.targetPos, true);
		return;
		IL_02E7:
		if (this.targetPos.sqrMagnitude == 0f && _params.Count < 6)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("line target is not set");
			return;
		}
		Vector3 vector2 = this.targetPos;
		float num7 = 5f;
		if (_params.Count >= 2)
		{
			num7 = this.FloatParse(_params[1]);
		}
		int num8 = 0;
		if (_params.Count >= 3)
		{
			num8 = this.IntParse(_params[2]);
		}
		if (_params.Count >= 4)
		{
			vector2.x = this.FloatParse(_params[3]);
		}
		if (_params.Count >= 5)
		{
			vector2.y = this.FloatParse(_params[4]);
		}
		if (_params.Count >= 6)
		{
			vector2.z = this.FloatParse(_params[5]);
		}
		EntityPlayerLocal.AutoMove autoMove3 = primaryPlayer.EnableAutoMove(true);
		autoMove3.SetLookAt(this.lookAtPos);
		autoMove3.StartLine(num7, num8, vector2, null);
		return;
		IL_052B:
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("unknown command " + _params[0]);
	}

	public int IntParse(string s)
	{
		int num;
		int.TryParse(s, out num);
		return num;
	}

	public float FloatParse(string s)
	{
		float num;
		float.TryParse(s, out num);
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 targetPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lookAtPos;
}
