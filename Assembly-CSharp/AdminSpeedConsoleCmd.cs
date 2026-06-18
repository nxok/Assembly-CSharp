using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class AdminSpeedConsoleCmd : ConsoleCmdAbstract
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

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		this.parameters = _params;
		string text = ((_params.Count == 0) ? "nope" : _params[0].ToLower());
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		float num = 0f;
		if (!float.TryParse(text, out num))
		{
			num = (float)((primaryPlayer.GodModeSpeedModifier == 15f) ? 5 : 15);
		}
		primaryPlayer.GodModeSpeedModifier = num;
		Log.Out("Admin speed: " + num.ToString());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetParam(List<string> _params, int index)
	{
		if (_params == null)
		{
			return null;
		}
		if (index >= _params.Count)
		{
			return null;
		}
		return _params[index];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetParamAsInt(int index)
	{
		if (this.parameters == null)
		{
			return -1;
		}
		if (index >= this.parameters.Count)
		{
			return -1;
		}
		int num = -1;
		int.TryParse(this.parameters[index], out num);
		return num;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			AdminSpeedConsoleCmd.info,
			"as"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return AdminSpeedConsoleCmd.info;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string info = "AdminSpeed";

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> parameters;
}
