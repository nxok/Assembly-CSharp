using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdCCPhysics : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "ccphysics" };
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Enables or disables changes to CCPhysics layer interactions. Reloading the game session may be necessary to fully apply if changed.";
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
			ConsoleCmdCCPhysics.EnableCCPhysicsChanges = !ConsoleCmdCCPhysics.EnableCCPhysicsChanges;
		}
		else
		{
			string text = _params[0].ToLowerInvariant();
			if (!(text == "on"))
			{
				if (!(text == "off"))
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unrecognised subcommand. Use 'on' or 'off' subcommand, or none to toggle.");
					return;
				}
				ConsoleCmdCCPhysics.EnableCCPhysicsChanges = false;
			}
			else
			{
				ConsoleCmdCCPhysics.EnableCCPhysicsChanges = true;
			}
		}
		Physics.IgnoreLayerCollision(20, 15, ConsoleCmdCCPhysics.EnableCCPhysicsChanges);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Physics layer interaction changes " + (ConsoleCmdCCPhysics.EnableCCPhysicsChanges ? "enabled" : "disabled") + ". Reloading the game session may be necessary to fully apply.");
	}

	public static bool ClientVerification = true;

	public static bool AllowSendToPeer = true;

	public static bool AllowReceiveFromPeer = true;

	public static bool DisablePeerAction = false;

	public static bool EnableCCPhysicsChanges = true;
}
