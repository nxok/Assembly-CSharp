using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Platform;

public static class SaveDataUtils
{
	public static ISaveDataManager SaveDataManager
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static ISaveDataPrefs SaveDataPrefs
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = SaveDataPrefsUninitialized.INSTANCE;

	public static IEnumerator InitStaticCoroutine()
	{
		if (SaveDataUtils.s_initStatic)
		{
			yield break;
		}
		SaveDataUtils.s_initStatic = true;
		Log.Out("[SaveDataUtils] InitStatic Begin");
		SaveDataUtils.UpdatePaths();
		IPlatformSaveGameProvider platformSaveGameProvider = PlatformManager.MultiPlatform.SaveGameProvider;
		SaveDataUtils.s_isManagementEnabled = platformSaveGameProvider != null;
		if (SaveDataUtils.s_isManagementEnabled)
		{
			while (!platformSaveGameProvider.Status.IsTerminal())
			{
				yield return null;
			}
		}
		if (SaveDataUtils.s_isManagementEnabled)
		{
			SaveDataUtils.s_saveDataManager = new SaveDataManager(platformSaveGameProvider);
		}
		else
		{
			SaveDataUtils.s_saveDataManager = SaveDataManager_Placeholder.Instance;
		}
		SaveDataUtils.SaveDataManager = SaveDataUtils.s_saveDataManager;
		SaveDataUtils.SaveDataManager.Init();
		SdDirectory.CreateDirectory(GameIO.GetUserGameDataDir());
		Log.Out("[SaveDataUtils] UserGameDataDir: " + GameIO.GetUserGameDataDir());
		if (LaunchPrefs.PlayerPrefsFile.Value)
		{
			Log.Out("[SaveDataUtils] SdPlayerPrefs -> SdFile");
			SaveDataUtils.SaveDataPrefs = (SaveDataUtils.s_saveDataPrefs = new SaveDataPrefsFile(GameIO.GetNormalizedPath(Path.Join(GameIO.GetUserGameDataDir(), "prefs.cfg"))));
		}
		else
		{
			Log.Out("[SaveDataUtils] SdPlayerPrefs -> Unity");
			SaveDataUtils.SaveDataPrefs = (SaveDataUtils.s_saveDataPrefs = SaveDataPrefsUnity.INSTANCE);
		}
		Log.Out("[SaveDataUtils] InitStatic Complete");
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void SetSaveDataManagerOverride(ISaveDataManager saveDataManagerOverride)
	{
		if (saveDataManagerOverride == SaveDataUtils.s_saveDataManager)
		{
			Log.Error("SetSaveDataManagerOverride failed: Cannot override default Save Data Manager with itself.");
			return;
		}
		SaveDataUtils.SaveDataManager = saveDataManagerOverride;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ClearSaveDataManagerOverride()
	{
		if (SaveDataUtils.SaveDataManager == SaveDataUtils.s_saveDataManager)
		{
			Log.Error("ClearSaveDataManagerOverride failed: Save Data Manager override was not set or has already been cleared.");
			return;
		}
		SaveDataUtils.SaveDataManager.Cleanup();
		SaveDataUtils.SaveDataManager = null;
		SaveDataUtils.SaveDataManager = SaveDataUtils.s_saveDataManager;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void SetSaveDataPrefsOverride(ISaveDataPrefs saveDataPrefsOverride)
	{
		SaveDataUtils.SaveDataPrefs = saveDataPrefsOverride;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ClearSaveDataPrefsOverride()
	{
		SaveDataUtils.SaveDataPrefs = SaveDataUtils.s_saveDataPrefs;
	}

	public static bool IsManaged(string path)
	{
		if (!SaveDataUtils.s_isManagementEnabled)
		{
			return false;
		}
		if (string.IsNullOrWhiteSpace(path))
		{
			return false;
		}
		string normalizedPath = GameIO.GetNormalizedPath(path);
		return SaveDataUtils.s_managedPathRegexWithoutGroups.IsMatch(normalizedPath);
	}

	public static bool TryGetManagedPath(string path, out SaveDataManagedPath managedPath)
	{
		if (!SaveDataUtils.s_isManagementEnabled)
		{
			managedPath = null;
			return false;
		}
		if (string.IsNullOrWhiteSpace(path))
		{
			managedPath = null;
			return false;
		}
		string normalizedPath = GameIO.GetNormalizedPath(path);
		Match match = SaveDataUtils.s_managedPathRegex.Match(normalizedPath);
		if (match.Success)
		{
			GroupCollection groups = match.Groups;
			string value = groups[1].Value;
			string value2 = groups[2].Value;
			managedPath = new SaveDataManagedPath(value2);
			return true;
		}
		managedPath = null;
		return false;
	}

	public static SaveDataManagedPath GetBackupPath(SaveDataManagedPath restorePath)
	{
		return new SaveDataManagedPath(restorePath.PathRelativeToRoot + ".bup");
	}

	public static SaveDataManagedPath GetRestorePath(SaveDataManagedPath backupPath)
	{
		string pathRelativeToRoot = backupPath.PathRelativeToRoot;
		if (!pathRelativeToRoot.EndsWith(".bup"))
		{
			throw new ArgumentException(string.Format("Expected \"{0}\" to end with \"{1}\".", backupPath, ".bup"));
		}
		return new SaveDataManagedPath(pathRelativeToRoot.AsSpan(0, pathRelativeToRoot.Length - ".bup".Length));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdatePaths()
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder.Append("^(?:");
		stringBuilder2.Append("^(?:");
		string normalizedPath = GameIO.GetNormalizedPath(GameIO.GetUserGameDataDir());
		stringBuilder.Append("(?<1>");
		string text = Regex.Escape(normalizedPath);
		stringBuilder.Append(text);
		stringBuilder2.Append(text);
		stringBuilder.Append(')');
		stringBuilder.Append(')');
		stringBuilder2.Append(')');
		stringBuilder.Append("(?:$|[\\\\/](?<2>.*)$)");
		stringBuilder2.Append("(?:$|[\\\\/])");
		SaveDataUtils.s_managedPathRegex = new Regex(stringBuilder.ToString(), RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
		SaveDataUtils.s_managedPathRegexWithoutGroups = new Regex(stringBuilder.ToString(), RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
		SaveDataUtils.s_saveDataRootPathPrefix = normalizedPath;
	}

	public static void Destroy()
	{
		SdPlayerPrefs.Save();
		SaveDataUtils.SaveDataManager.Cleanup();
		SaveDataUtils.SaveDataPrefs = (SaveDataUtils.s_saveDataPrefs = SaveDataPrefsUninitialized.INSTANCE);
		RoamingPrefs.Destroy();
		SaveDataUtils.SaveDataManager = (SaveDataUtils.s_saveDataManager = null);
		IPlatformSaveGameProvider saveGameProvider = PlatformManager.NativePlatform.SaveGameProvider;
		if (saveGameProvider == null)
		{
			return;
		}
		saveGameProvider.Destroy();
	}

	public const string BACKUP_FILE_EXTENSION = "bup";

	public const string BACKUP_FILE_EXTENSION_WITH_DOT = ".bup";

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_initStatic;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ISaveDataManager s_saveDataManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_isManagementEnabled = false;

	public static string s_saveDataRootPathPrefix;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ISaveDataPrefs s_saveDataPrefs = SaveDataPrefsUninitialized.INSTANCE;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Regex s_managedPathRegex = new Regex("$^", RegexOptions.Compiled);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Regex s_managedPathRegexWithoutGroups = new Regex("$^", RegexOptions.Compiled);
}
