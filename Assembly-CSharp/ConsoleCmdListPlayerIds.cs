using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdListPlayerIds : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "listplayerids", "lpi" };
	}

	public override int DefaultPermissionLevel
	{
		get
		{
			return 1000;
		}
	}

	public override DeviceFlag AllowedDeviceTypes
	{
		get
		{
			return DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Lists all players with their IDs for ingame commands";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		World world = GameManager.Instance.World;
		int num = 0;
		foreach (KeyValuePair<int, EntityPlayer> keyValuePair in world.Players.dict)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("{0}. id={1}, {2}", ++num, keyValuePair.Value.entityId, keyValuePair.Value.EntityName));
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Total of " + world.Players.list.Count.ToString() + " in the game");
	}
}
