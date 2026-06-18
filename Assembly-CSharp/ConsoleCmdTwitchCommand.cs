using System;
using System.Collections.Generic;
using Platform;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdTwitchCommand : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "twitch" };
	}

	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	public override DeviceFlag AllowedDeviceTypes
	{
		get
		{
			return DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;
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
		if (!TwitchManager.Current.IsReady)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Twitch must be active to use this command!");
		}
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("twitch commands available:");
			for (int i = 0; i < TwitchManager.Current.TwitchCommandList.Count; i++)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("   " + TwitchManager.Current.TwitchCommandList[i].CommandText[0]);
			}
			return;
		}
		if (_params[0] == "refresh")
		{
			GameManager.Instance.StartCoroutine(TwitchManager.Current.Authentication.RefreshToken(0f, true));
			return;
		}
		TwitchManager.Current.HandleConsoleAction(_params);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "usage: twitch <command> <params>";
	}
}
