using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdTeleport : ConsoleCmdTeleportsAbs
{
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
		return "Teleport the local player";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n  1. teleport <x> <y> <z> [view direction]\n  2. teleport <x> <z> [view direction]\n  3. teleport <target steam id / player name / entity id>\n  4. teleport offset <inc x> <inc y> <inc z>\nFor 1. and 2.: view direction is an optional specifier to select the direction you want to look into\nafter teleporting. This can be either of n, ne, e, se, s, sw, w, nw or north, northeast, etc.\n1. Teleports the local player to the specified location. Use y = -1 to spawn on ground.\n2. Same as 1 but always spawn on ground.\n3. Teleports to the location of the given player\n4. Teleport the local player to the position calculated by his current position and the given offsets";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "teleport", "tp" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!_senderInfo.IsLocalGame && _senderInfo.RemoteClientInfo == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be used on clients, use \"teleportplayer\" instead for other players / remote clients");
			return;
		}
		EntityPlayer executingEntityPlayer = base.GetExecutingEntityPlayer(_senderInfo);
		if (_params.Count < 1 || _params.Count > 4)
		{
			SdtdConsole instance = SingletonMonoBehaviour<SdtdConsole>.Instance;
			string text = "Wrong number of arguments, expected 1 to 4, found ";
			int count = _params.Count;
			instance.Output(text + count.ToString() + ".");
			return;
		}
		if (_params.Count == 1)
		{
			Vector3 vector;
			if (!base.TryGetDestinationFromPlayer(_params[0], out vector))
			{
				return;
			}
			base.ExecuteTeleport(_senderInfo.RemoteClientInfo, vector, null);
			return;
		}
		else if (_params.Count == 4 && _params[0].EqualsCaseInsensitive("offset"))
		{
			Vector3i vector3i;
			if (!base.TryParseV3i(_params, 1, out vector3i))
			{
				return;
			}
			vector3i += new Vector3i(executingEntityPlayer.position);
			base.ExecuteTeleport(_senderInfo.RemoteClientInfo, vector3i, null);
			return;
		}
		else
		{
			int count;
			bool flag = !int.TryParse(_params[_params.Count - 1], out count);
			if (_params.Count == 2 || (flag && _params.Count == 3))
			{
				_params.Insert(1, "-1");
			}
			Vector3i vector3i;
			if (!base.TryParseV3i(_params, 0, out vector3i))
			{
				return;
			}
			Vector3? vector2 = ((_params.Count == 4) ? base.TryParseViewDirection(_params[3]) : null);
			base.ExecuteTeleport(_senderInfo.RemoteClientInfo, vector3i, vector2);
			return;
		}
	}
}
