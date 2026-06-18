using System;
using System.Collections.Generic;
using InControl;
using Platform;

public static class GameOptionsControls
{
	public static IEnumerable<ValueTuple<PlayerActionsBase, string>> ActionSetPrefs
	{
		get
		{
			foreach (PlayerActionsBase playerActionsBase in PlatformManager.NativePlatform.Input.ActionSets)
			{
				yield return new ValueTuple<PlayerActionsBase, string>(playerActionsBase, "ActionSet_" + playerActionsBase.Name);
			}
			IEnumerator<PlayerActionsBase> enumerator = null;
			yield break;
			yield break;
		}
	}

	public static void Save()
	{
		foreach (ValueTuple<PlayerActionsBase, string> valueTuple in GameOptionsControls.ActionSetPrefs)
		{
			PlayerActionsBase item = valueTuple.Item1;
			SdPlayerPrefs.SetString(valueTuple.Item2, item.Save());
		}
		SdPlayerPrefs.SetString("Controls", GameOptionsControls.Export());
		SdPlayerPrefs.SetInt("ActionSetsSaved", 0);
		GameOptionsControls.ApplyAllowControllerOption();
	}

	public static void Load()
	{
		bool flag = SdPlayerPrefs.HasKey("ActionSetsSaved");
		if (!flag && SdPlayerPrefs.HasKey("Controls"))
		{
			string @string = SdPlayerPrefs.GetString("Controls", string.Empty);
			PlatformManager.NativePlatform.Input.LoadActionSetsFromStrings(@string.Split(';', StringSplitOptions.None));
			GameOptionsControls.Save();
			GameOptionsControls.ApplyAllowControllerOption();
			GameOptionsControls.RestoreNonBindableControllerActionsToDefaults();
			Log.Out("Legacy controls data converted");
			return;
		}
		if (flag)
		{
			foreach (ValueTuple<PlayerActionsBase, string> valueTuple in GameOptionsControls.ActionSetPrefs)
			{
				PlayerActionsBase item = valueTuple.Item1;
				string item2 = valueTuple.Item2;
				if (!string.IsNullOrEmpty(SdPlayerPrefs.GetString(item2, string.Empty)))
				{
					item.Load(SdPlayerPrefs.GetString(item2));
				}
				else
				{
					Log.Warning("Loading controls: No data for action set " + item.Name);
				}
			}
		}
		GameOptionsControls.ApplyAllowControllerOption();
		GameOptionsControls.RestoreNonBindableControllerActionsToDefaults();
	}

	public static string Export()
	{
		string[] array = new string[PlatformManager.NativePlatform.Input.ActionSets.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = PlatformManager.NativePlatform.Input.ActionSets[i].Save();
		}
		return string.Join(";", array);
	}

	public static void Import(string importString)
	{
		PlatformManager.NativePlatform.Input.LoadActionSetsFromStrings(importString.Split(';', StringSplitOptions.None));
		GameOptionsControls.ApplyAllowControllerOption();
		GameOptionsControls.RestoreNonBindableControllerActionsToDefaults();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ApplyAllowControllerOption()
	{
		bool @bool = GamePrefs.GetBool(EnumGamePrefs.OptionsAllowController);
		for (int i = 0; i < PlatformManager.NativePlatform.Input.ActionSets.Count; i++)
		{
			PlatformManager.NativePlatform.Input.ActionSets[i].Device = (@bool ? null : InputDevice.Null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void RestoreNonBindableControllerActionsToDefaults()
	{
		PlatformManager.NativePlatform.Input.GetActionSetForName("gui").ResetControllerBindings();
		PlatformManager.NativePlatform.Input.GetActionSetForName("permanent").ResetControllerBindings();
	}

	public const string cActionSetSavePrefix = "ActionSet_";

	public const string ActionSetSaveModeFlagPref = "ActionSetsSaved";

	public const string LegacyControlsPref = "Controls";
}
