using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdDebuff : ConsoleCmdAbstract
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
		return new string[] { "debuff" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!_senderInfo.IsLocalGame)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be used on clients, use \"debuffplayer\" instead for other players / remote clients");
			return;
		}
		EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (!(primaryPlayer != null))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No local player found.");
			return;
		}
		if (_params.Count != 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("debuff requires a buff name as the only argument!");
			ConsoleCmdDebuff.PrintActiveBuffNames(primaryPlayer);
			return;
		}
		if (primaryPlayer.Buffs.GetBuff(_params[0]) == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Debuff failed: buff \"" + _params[0] + "\" unknown or not active");
			ConsoleCmdDebuff.PrintActiveBuffNames(primaryPlayer);
			return;
		}
		primaryPlayer.Buffs.RemoveBuff(_params[0], -1, true);
	}

	public static void PrintActiveBuffNames(EntityPlayer _player)
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Active buffs:");
		foreach (BuffValue buffValue in _player.Buffs.ActiveBuffs)
		{
			if (buffValue != null && buffValue.BuffClass != null)
			{
				BuffClass buffClass = buffValue.BuffClass;
				if (buffClass.Name.Equals(buffClass.LocalizedName))
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" - " + buffClass.Name);
				}
				else
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Concat(new string[] { " - ", buffClass.Name, " (", buffClass.LocalizedName, ")" }));
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Removes a buff from the local player";
	}
}
