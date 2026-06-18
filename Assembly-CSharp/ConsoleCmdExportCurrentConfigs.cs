using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdExportCurrentConfigs : ConsoleCmdAbstract
{
	public override int DefaultPermissionLevel
	{
		get
		{
			return 1000;
		}
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Exports the current game config XMLs";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Exports all game config XMLs as they are currently used (including applied\npatches from mods) to the folder \"Configs\" in the save folder of the game.\nIf run from the main menu it exports the XUi configs for the menu, if run\nfrom a game session will export all others.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "exportcurrentconfigs" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.Instance.World == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No game started, exporting XUi menu and rwgmixer configs");
			string text = Path.Combine(GameIO.GetApplicationTempPath(), "ExportedConfigs");
			if (SdDirectory.Exists(text))
			{
				SdDirectory.Delete(text, true);
			}
			Thread.Sleep(50);
			SdDirectory.CreateDirectory(text);
			string[] array = new string[] { "rwgmixer", "loadingscreen", "XUi_Menu/styles", "XUi_Menu/templates", "XUi_Menu/windows", "XUi_Menu/xui" };
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = array[i];
				XmlFile xml = null;
				ThreadManager.RunCoroutineSync(XmlPatcher.LoadAndPatchConfig(text2, delegate(XmlFile _file)
				{
					xml = _file;
				}));
				string text3 = text + "/" + text2 + ".xml";
				if (text2.IndexOf('/') >= 0)
				{
					string directoryName = Path.GetDirectoryName(text3);
					if (!SdDirectory.Exists(directoryName))
					{
						SdDirectory.CreateDirectory(directoryName);
					}
				}
				xml.SerializeToFile(text3, false, null);
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Configs exports to " + text);
			GameIO.OpenExplorer(text);
			return;
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			string @string = GamePrefs.GetString(EnumGamePrefs.GameGuidClient);
			ConsoleCmdExportCurrentConfigs.DumpLoadedXmls(Path.Combine(GameIO.GetApplicationTempPath(), "SavesLocal", @string, "ConfigsDump"));
			return;
		}
		string text4 = Path.Combine(GameIO.GetSaveGameDir(), "ConfigsDump");
		if (!GameIO.IsRoamingUserDataPath(text4))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Patched XMLs are automatically dumped on game start to a ConfigsDump subdirectory of the save game.");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("In this case you can find the folder at: " + text4);
			GameIO.OpenExplorer(text4);
			return;
		}
		string string2 = GamePrefs.GetString(EnumGamePrefs.GameWorld);
		string string3 = GamePrefs.GetString(EnumGamePrefs.GameName);
		ConsoleCmdExportCurrentConfigs.DumpLoadedXmls(Path.Combine(GameIO.GetApplicationTempPath(), string2, string3, "ConfigsDump"));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void DumpLoadedXmls(string tempPath)
	{
		if (SdDirectory.Exists(tempPath))
		{
			SdDirectory.Delete(tempPath, true);
		}
		SdDirectory.CreateDirectory(tempPath);
		WorldStaticData.SaveXmlsToFolder(tempPath);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Configs exports to " + tempPath);
		GameIO.OpenExplorer(tempPath);
	}
}
