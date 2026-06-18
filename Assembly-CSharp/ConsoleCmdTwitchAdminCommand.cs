using System;
using System.Collections.Generic;
using Platform;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdTwitchAdminCommand : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "twitchadmin" };
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
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.getHelp());
			return;
		}
		string text = _params[0];
		if (!(text == "cleanup"))
		{
			if (!(text == "export"))
			{
				if (!(text == "import"))
				{
					if (text == "pointstotal")
					{
						goto IL_0190;
					}
					if (!(text == "reset"))
					{
						return;
					}
					if (_params.Count != 3 || !(_params[1] == "all"))
					{
						return;
					}
					string text2 = _params[2];
					if (text2 == "both")
					{
						TwitchManager.Current.ViewerData.ResetAllPoints();
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Resetting all Twitch PP and SP");
						return;
					}
					if (text2 == "sp")
					{
						TwitchManager.Current.ViewerData.ResetAllSpecialPoints();
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Resetting all Twitch SP");
						return;
					}
					if (!(text2 == "pp"))
					{
						return;
					}
					TwitchManager.Current.ViewerData.ResetAllStandardPoints();
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Resetting all Twitch PP");
					return;
				}
			}
			else
			{
				if (!DeviceCapabilities.CanUserAccessFilesystem())
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Viewer data import/export not supported on this device");
					return;
				}
				if (_params.Count != 2)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Export failed: file path required");
					return;
				}
				try
				{
					string text3 = _params[1];
					TwitchManager.Current.SaveExportViewerData(text3);
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Viewer data exported");
					return;
				}
				catch (Exception ex)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Export failed. " + ex.GetType().Name + ": " + ex.Message);
					return;
				}
			}
			if (!DeviceCapabilities.CanUserAccessFilesystem())
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Viewer data import/export not supported on this device");
				return;
			}
			if (_params.Count != 2)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Import failed: file path required");
				return;
			}
			try
			{
				string text4 = _params[1];
				TwitchManager.Current.LoadExportViewerData(text4);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Viewer data imported");
				return;
			}
			catch (Exception ex2)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Import failed. " + ex2.GetType().Name + ": " + ex2.Message);
				return;
			}
			IL_0190:
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Twitch Viewer Data Point Totals:\n" + TwitchManager.Current.ViewerData.GetPointTotals());
			return;
		}
		TwitchManager.Current.ViewerData.Cleanup();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Twitch Admin Commands";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n  1. twitchadmin cleanup\n  2. twitchadmin export <filepath>\n  3. twitchadmin import <filepath>\n  4. twitchadmin pointtotals\n  5. twitchadmin reset all both\n  6. twitchadmin reset all pp\n  7. twitchadmin reset all sp\n1. Cleans up duplicate Viewer Data.\n2. Export users, points, and credits to a file\n3. Import users, points, and credits from a file, replacing existing data\n4. Prints out the totals for PP, SP, and BitCredit\n5. Clears all PP and SP for all users\n6. Clears all PP for all users\n7. Clears all SP for all users";
	}
}
