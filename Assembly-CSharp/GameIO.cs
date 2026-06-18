using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Platform;
using UnityEngine;

public static class GameIO
{
	public static SdFileInfo[] GetDirectory(string _path, string _pattern)
	{
		if (!SdDirectory.Exists(_path))
		{
			return Array.Empty<SdFileInfo>();
		}
		return new SdDirectoryInfo(_path).GetFiles(_pattern);
	}

	public static long FileSize(string _filePath)
	{
		SdFileInfo sdFileInfo = new SdFileInfo(_filePath);
		if (!sdFileInfo.Exists)
		{
			return -1L;
		}
		return sdFileInfo.Length;
	}

	public static string GetNormalizedPath(string _path)
	{
		return Path.GetFullPath(_path).TrimEnd(GameIO.pathTrimCharacters);
	}

	public static bool PathsEquals(string _path1, string _path2, bool _ignoreCase)
	{
		return string.Equals(GameIO.GetNormalizedPath(_path1), GameIO.GetNormalizedPath(_path2), _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
	}

	public static string GetFileExtension(string _filename)
	{
		int num;
		if (_filename.Length > 4 && (num = _filename.LastIndexOf('.')) > 0)
		{
			return _filename.Substring(num);
		}
		return _filename;
	}

	public static string RemoveFileExtension(string _filename)
	{
		int num;
		if (_filename.Length > 4 && (num = _filename.LastIndexOf('.')) > 0)
		{
			return _filename.Substring(0, num);
		}
		return _filename;
	}

	public static string RemoveExtension(string _filename, string _extension)
	{
		if (_filename.Length > _extension.Length && _filename.EndsWith(_extension, StringComparison.InvariantCultureIgnoreCase))
		{
			return _filename.Substring(0, _filename.Length - _extension.Length);
		}
		return _filename;
	}

	public static string GetFilenameFromPath(string _filepath)
	{
		int num = _filepath.LastIndexOfAny(GameIO.ResourcePathSeparators);
		if (num >= 0 && num < _filepath.Length)
		{
			_filepath = _filepath.Substring(num + 1);
		}
		return _filepath;
	}

	public static string GetFilenameFromPathWithoutExtension(string _filepath)
	{
		int num = _filepath.LastIndexOfAny(GameIO.ResourcePathSeparators);
		int num2 = _filepath.LastIndexOf('.');
		if (num >= 0 && num2 < num)
		{
			num2 = -1;
		}
		if (num >= 0 && num2 >= 0)
		{
			return _filepath.Substring(num + 1, num2 - num - 1);
		}
		if (num >= 0)
		{
			return _filepath.Substring(num + 1);
		}
		if (num2 >= 0)
		{
			return _filepath.Substring(0, num2);
		}
		return _filepath;
	}

	public static string GetDirectoryFromPath(string _filepath)
	{
		int num = _filepath.LastIndexOf('/');
		if (num > 0 && num < _filepath.Length)
		{
			_filepath = _filepath.Substring(0, num);
		}
		return _filepath;
	}

	public static long GetDirectorySize(string _filepath, bool recursive = true)
	{
		return GameIO.GetDirectorySize(new SdDirectoryInfo(_filepath), recursive);
	}

	public static long GetDirectorySize(SdDirectoryInfo directoryInfo, bool recursive = true)
	{
		long num = 0L;
		if (directoryInfo == null || !directoryInfo.Exists)
		{
			return num;
		}
		foreach (SdFileInfo sdFileInfo in directoryInfo.GetFiles())
		{
			num += sdFileInfo.Length;
		}
		if (recursive)
		{
			foreach (SdDirectoryInfo sdDirectoryInfo in directoryInfo.GetDirectories())
			{
				num += GameIO.GetDirectorySize(sdDirectoryInfo, recursive);
			}
		}
		return num;
	}

	public static string GetGameDir(string _relDir)
	{
		return GameIO.GetApplicationPath() + "/" + _relDir;
	}

	public static string GetApplicationPath()
	{
		if (GameIO.m_ApplicationPath == null)
		{
			string text = GameIO.m_UnityDataPath;
			RuntimePlatform unityRuntimePlatform = GameIO.m_UnityRuntimePlatform;
			if (unityRuntimePlatform <= 25)
			{
				if (unityRuntimePlatform - 1 > 1)
				{
					if (unityRuntimePlatform != 25)
					{
						goto IL_003F;
					}
					goto IL_004B;
				}
			}
			else
			{
				if (unityRuntimePlatform == 38)
				{
					goto IL_004B;
				}
				if (unityRuntimePlatform - 44 > 1)
				{
					goto IL_003F;
				}
			}
			text += "/..";
			goto IL_004B;
			IL_003F:
			text += "/..";
			IL_004B:
			GameIO.m_ApplicationPath = text;
		}
		return GameIO.m_ApplicationPath;
	}

	public static string GetGamePath()
	{
		if (GameIO.m_UnityRuntimePlatform != 1 && GameIO.m_UnityRuntimePlatform != 45)
		{
			return GameIO.m_UnityDataPath + "/..";
		}
		return GameIO.m_UnityDataPath + "/../..";
	}

	public static string GetGameExecutablePath()
	{
		return GameIO.GetGamePath() + "/" + GameIO.GetGameExecutableName();
	}

	public static string GetGameExecutableName()
	{
		RuntimePlatform unityRuntimePlatform = GameIO.m_UnityRuntimePlatform;
		if (unityRuntimePlatform <= 13)
		{
			switch (unityRuntimePlatform)
			{
			case 0:
				return "7DaysToDie.app";
			case 1:
				return "7DaysToDie.app";
			case 2:
				return "7DaysToDie.exe";
			default:
				if (unityRuntimePlatform == 7)
				{
					return "7DaysToDie.exe";
				}
				if (unityRuntimePlatform == 13)
				{
					return "7DaysToDie.x86_64";
				}
				break;
			}
		}
		else
		{
			if (unityRuntimePlatform == 16)
			{
				return "7DaysToDie.x86_64";
			}
			if (unityRuntimePlatform == 27)
			{
				throw new ArgumentException("Platform " + GameIO.m_UnityRuntimePlatform.ToStringCached<RuntimePlatform>() + " currently not supported", "m_UnityRuntimePlatform");
			}
			switch (unityRuntimePlatform)
			{
			case 36:
				throw new ArgumentException("Platform " + GameIO.m_UnityRuntimePlatform.ToStringCached<RuntimePlatform>() + " currently not supported", "m_UnityRuntimePlatform");
			case 37:
				throw new ArgumentException("Platform " + GameIO.m_UnityRuntimePlatform.ToStringCached<RuntimePlatform>() + " currently not supported", "m_UnityRuntimePlatform");
			case 38:
				throw new ArgumentException("Platform " + GameIO.m_UnityRuntimePlatform.ToStringCached<RuntimePlatform>() + " currently not supported", "m_UnityRuntimePlatform");
			}
		}
		throw new ArgumentOutOfRangeException("m_UnityRuntimePlatform");
	}

	public static string GetLauncherExecutablePath()
	{
		return GameIO.GetGamePath() + "/" + GameIO.GetLauncherExecutableName();
	}

	public static string GetLauncherExecutableName()
	{
		RuntimePlatform unityRuntimePlatform = GameIO.m_UnityRuntimePlatform;
		if (unityRuntimePlatform <= 13)
		{
			switch (unityRuntimePlatform)
			{
			case 0:
				return "7dLauncher.app";
			case 1:
				return "7dLauncher.app";
			case 2:
				return "7dLauncher.exe";
			default:
				if (unityRuntimePlatform == 7)
				{
					return "7dLauncher.exe";
				}
				if (unityRuntimePlatform == 13)
				{
					return "7DaysToDie.sh";
				}
				break;
			}
		}
		else
		{
			if (unityRuntimePlatform == 16)
			{
				return "7DaysToDie.sh";
			}
			if (unityRuntimePlatform == 27)
			{
				throw new ArgumentException("Platform " + GameIO.m_UnityRuntimePlatform.ToStringCached<RuntimePlatform>() + " currently not supported", "m_UnityRuntimePlatform");
			}
			switch (unityRuntimePlatform)
			{
			case 36:
				throw new ArgumentException("Platform " + GameIO.m_UnityRuntimePlatform.ToStringCached<RuntimePlatform>() + " currently not supported", "m_UnityRuntimePlatform");
			case 37:
				throw new ArgumentException("Platform " + GameIO.m_UnityRuntimePlatform.ToStringCached<RuntimePlatform>() + " currently not supported", "m_UnityRuntimePlatform");
			case 38:
				throw new ArgumentException("Platform " + GameIO.m_UnityRuntimePlatform.ToStringCached<RuntimePlatform>() + " currently not supported", "m_UnityRuntimePlatform");
			}
		}
		throw new ArgumentOutOfRangeException("m_UnityRuntimePlatform");
	}

	public static string GetApplicationScratchPath()
	{
		if (GameIO.m_ApplicationScratchPath == null)
		{
			RuntimePlatform unityRuntimePlatform = GameIO.m_UnityRuntimePlatform;
			if (unityRuntimePlatform <= 27)
			{
				if (unityRuntimePlatform != 25)
				{
					if (unityRuntimePlatform != 27)
					{
						goto IL_0050;
					}
					GameIO.m_ApplicationScratchPath = "D:";
					goto IL_005A;
				}
			}
			else
			{
				if (unityRuntimePlatform - 36 <= 1)
				{
					GameIO.m_ApplicationScratchPath = "D:";
					goto IL_005A;
				}
				if (unityRuntimePlatform != 38)
				{
					goto IL_0050;
				}
			}
			GameIO.m_ApplicationScratchPath = "/hostapp";
			goto IL_005A;
			IL_0050:
			GameIO.m_ApplicationScratchPath = GameIO.GetApplicationPath();
		}
		IL_005A:
		return GameIO.m_ApplicationScratchPath;
	}

	public static string GetApplicationTempPath()
	{
		return PlatformApplicationManager.Application.temporaryCachePath;
	}

	public static IEnumerator PrecacheFile(string _path, int _doYieldEveryMs = -1, Action<float, long, long> _statusUpdateHandler = null)
	{
		if (!SdFile.Exists(_path))
		{
			Log.Error("File does not exist: " + _path);
			yield break;
		}
		Stream fs;
		try
		{
			fs = SdFile.OpenRead(_path);
		}
		catch (Exception ex)
		{
			Log.Error("Precaching file failed");
			Log.Exception(ex);
			yield break;
		}
		byte[] buf = new byte[16384];
		MicroStopwatch msw = new MicroStopwatch();
		int num;
		do
		{
			if (_doYieldEveryMs > 0 && msw.ElapsedMilliseconds >= (long)_doYieldEveryMs)
			{
				if (_statusUpdateHandler != null)
				{
					_statusUpdateHandler((float)fs.Position / (float)fs.Length, fs.Position, fs.Length);
				}
				yield return null;
				msw.ResetAndRestart();
			}
			try
			{
				num = fs.Read(buf, 0, buf.Length);
			}
			catch (Exception ex2)
			{
				Log.Error("Precaching file failed");
				Log.Exception(ex2);
				try
				{
					fs.Dispose();
				}
				catch (Exception ex3)
				{
					Log.Error("Failed disposing filestream");
					Log.Exception(ex3);
				}
				yield break;
			}
		}
		while (num > 0);
		fs.Dispose();
		yield break;
	}

	public static string GetDocumentPath()
	{
		RuntimePlatform unityRuntimePlatform = GameIO.m_UnityRuntimePlatform;
		if (unityRuntimePlatform <= 13)
		{
			if (unityRuntimePlatform <= 2)
			{
				if (unityRuntimePlatform <= 1)
				{
					goto IL_0083;
				}
				if (unityRuntimePlatform != 2)
				{
					goto IL_00B3;
				}
				goto IL_007B;
			}
			else
			{
				if (unityRuntimePlatform == 7)
				{
					goto IL_007B;
				}
				if (unityRuntimePlatform != 13)
				{
					goto IL_00B3;
				}
			}
		}
		else if (unityRuntimePlatform <= 25)
		{
			if (unityRuntimePlatform != 16)
			{
				if (unityRuntimePlatform != 25)
				{
					goto IL_00B3;
				}
				return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			}
		}
		else
		{
			if (unityRuntimePlatform == 27)
			{
				Debug.LogWarning("XboxOne: Platform Document Path is currently not set");
				return null;
			}
			switch (unityRuntimePlatform)
			{
			case 36:
			case 37:
				return Application.persistentDataPath;
			case 38:
				return "/download0";
			case 39:
			case 40:
			case 41:
			case 42:
				goto IL_00B3;
			case 43:
				break;
			case 44:
				goto IL_007B;
			case 45:
				goto IL_0083;
			default:
				goto IL_00B3;
			}
		}
		return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		IL_007B:
		return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		IL_0083:
		return Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Application Support";
		IL_00B3:
		return null;
	}

	public static string GetDefaultPersistentDataPath()
	{
		return GameIO.GetDocumentPath() + "/" + "7 Days To Die".Replace(" ", "");
	}

	public static void InitializeUserDataPaths(string _rootPathOverride = null)
	{
		string text = GameIO.GetDefaultPersistentDataPath();
		if (!string.IsNullOrEmpty(_rootPathOverride))
		{
			text = GameIO.MakeAbsolutePath(_rootPathOverride);
			Log.Out("Overriding default user data path to " + text);
		}
		if (GameIO.m_lastDeviceLocalRoot != null && GameIO.m_lastDeviceLocalRoot.Equals(text))
		{
			return;
		}
		GameIO.m_lastDeviceLocalRoot = text;
		text = Path.GetFullPath(text);
		string text2 = text + "Roaming";
		string text3 = Regex.Replace(text2, "[\\\\/]", "[\\\\/]", RegexOptions.Singleline | RegexOptions.CultureInvariant);
		GameIO.m_isRoamingPathRegex = new Regex("^(?:" + text3 + ")", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
		GameIO.m_cachedUserDataPaths = new Dictionary<ValueTuple<UserDataStorageType, string>, string>();
		GameIO.<InitializeUserDataPaths>g__PopulateCache|36_0(UserDataStorageType.DeviceLocal, text);
		GameIO.<InitializeUserDataPaths>g__PopulateCache|36_0(UserDataStorageType.Roaming, text2);
	}

	public static bool IsRoamingUserDataPath(string _path)
	{
		if (GameIO.m_isRoamingPathRegex == null)
		{
			throw new Exception("IsRoamingUserDataPath used before InitializeUserDataPaths");
		}
		return GameIO.m_isRoamingPathRegex.IsMatch(_path);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetCachedUserDataPath(UserDataStorageType _storage, string _folder = "")
	{
		if (GameIO.m_cachedUserDataPaths == null)
		{
			throw new Exception("GetCachedUserDataPath used before InitializeUserDataPaths");
		}
		return GameIO.m_cachedUserDataPaths[new ValueTuple<UserDataStorageType, string>(_storage, _folder)];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetDefaultUserGameDataPath(string _folder = "")
	{
		if (PlatformManager.MultiPlatform == null)
		{
			string cachedUserDataPath = GameIO.GetCachedUserDataPath(UserDataStorageType.DeviceLocal, _folder);
			Log.Error("Trying to access default save storage before native platform initialized. Falling back to " + cachedUserDataPath);
			return cachedUserDataPath;
		}
		return GameIO.GetUserGameDataPath(PlatformManager.MultiPlatform.UserDataRoaming.DefaultSaveStorage, _folder);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetUserGameDataPath(UserDataStorageType _storage, string _folder = "")
	{
		if (_storage == UserDataStorageType.DeviceLocal)
		{
			return GameIO.GetCachedUserDataPath(_storage, _folder);
		}
		if (_storage != UserDataStorageType.Roaming)
		{
			throw new Exception(string.Format("Unknown user data storage type {0}", _storage));
		}
		if (!PlatformManager.MultiPlatform.UserDataRoaming.IsSupported)
		{
			string cachedUserDataPath = GameIO.GetCachedUserDataPath(UserDataStorageType.DeviceLocal, _folder);
			Log.Error("Platform does not support save roaming. Falling back to " + cachedUserDataPath);
			return cachedUserDataPath;
		}
		return GameIO.GetCachedUserDataPath(_storage, _folder);
	}

	public static string GetDeviceLocalUserGameDataDir()
	{
		return GameIO.GetUserGameDataPath(UserDataStorageType.DeviceLocal, "");
	}

	public static string GetRoamingUserGameDataDir()
	{
		return GameIO.GetUserGameDataPath(UserDataStorageType.Roaming, "");
	}

	public static string GetUserGameDataDir()
	{
		return GameIO.GetDefaultUserGameDataPath("");
	}

	public static string GetUserGameDataDir(UserDataStorageType _storage)
	{
		return GameIO.GetUserGameDataPath(_storage, "");
	}

	public static string GetSaveGameRootDir()
	{
		return GameIO.GetDefaultUserGameDataPath("Saves");
	}

	public static string GetSaveGameRootDir(UserDataStorageType _storage)
	{
		return GameIO.GetUserGameDataPath(_storage, "Saves");
	}

	public static string GetSaveGameDir(string _worldName, UserDataStorageType _storage)
	{
		return GameIO.GetSaveGameRootDir(_storage) + "/" + _worldName;
	}

	public static string GetSaveGameDir(string _worldName, string _gameName, UserDataStorageType _storage)
	{
		return GameIO.GetSaveGameDir(_worldName, _storage) + "/" + _gameName;
	}

	public static string GetSaveGameDir()
	{
		return GameIO.GetSaveGameDir(GamePrefs.GetString(EnumGamePrefs.GameWorld), GamePrefs.GetString(EnumGamePrefs.GameName), (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType));
	}

	public static string GetSaveGameLocalRootDir(UserDataStorageType _storage)
	{
		return GameIO.GetUserGameDataPath(_storage, "SavesLocal");
	}

	public static void SetSaveGameLocalGuid(string _guid)
	{
		GamePrefs.Set(EnumGamePrefs.GameGuidClient, _guid);
		if (string.IsNullOrEmpty(_guid))
		{
			return;
		}
		if (!PlatformManager.MultiPlatform.UserDataRoaming.SaveRoamingEnabled)
		{
			GamePrefs.Set(EnumGamePrefs.GameSaveStorageType, 0);
			return;
		}
		if (SdDirectory.Exists(GameIO.GetSaveGameLocalDir(UserDataStorageType.DeviceLocal, _guid)))
		{
			GamePrefs.Set(EnumGamePrefs.GameSaveStorageType, 0);
			return;
		}
		GamePrefs.Set(EnumGamePrefs.GameSaveStorageType, 1);
	}

	public static string GetSaveGameLocalDir()
	{
		string @string = GamePrefs.GetString(EnumGamePrefs.GameGuidClient);
		if (string.IsNullOrEmpty(@string))
		{
			throw new Exception("Accessing GetSaveGameLocalDir while GameGuidClient is not yet set!");
		}
		return GameIO.GetSaveGameLocalDir((UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType), @string);
	}

	public static string GetSaveGameLocalDir(UserDataStorageType _storage, string _guid)
	{
		return GameIO.GetSaveGameLocalRootDir(_storage) + "/" + _guid;
	}

	public static string GetPlayerDataDir()
	{
		return Path.Combine(GameIO.GetSaveGameDir(), "Player");
	}

	public static string GetPlayerDataLocalDir()
	{
		return Path.Combine(GameIO.GetSaveGameLocalDir(), "Player");
	}

	public static int GetPlayerSaves(GameIO.FoundSave _foundSave = null, bool includeArchived = false)
	{
		GameIO.<>c__DisplayClass57_0 CS$<>8__locals1;
		CS$<>8__locals1.includeArchived = includeArchived;
		CS$<>8__locals1._foundSave = _foundSave;
		int num = 0;
		if (PlatformManager.MultiPlatform.UserDataRoaming.SaveRoamingEnabled)
		{
			num += GameIO.<GetPlayerSaves>g__SearchSaveDir|57_0(UserDataStorageType.Roaming, ref CS$<>8__locals1);
		}
		return num + GameIO.<GetPlayerSaves>g__SearchSaveDir|57_0(UserDataStorageType.DeviceLocal, ref CS$<>8__locals1);
	}

	public static string GetSaveGameRegionDir()
	{
		return Path.Combine(GameIO.GetSaveGameDir(), "Region");
	}

	public static bool IsWorldGenerated(string _worldName, UserDataStorageType _storage)
	{
		return SdDirectory.Exists(Path.Combine(GameIO.GetUserGameDataDir(_storage), "GeneratedWorlds", _worldName));
	}

	public static bool IsAbsolutePath(string _path)
	{
		RuntimePlatform unityRuntimePlatform = GameIO.m_UnityRuntimePlatform;
		if (unityRuntimePlatform <= 17)
		{
			switch (unityRuntimePlatform)
			{
			case 0:
			case 1:
			case 8:
				break;
			case 2:
			case 7:
				goto IL_00E4;
			case 3:
			case 4:
			case 5:
			case 6:
				goto IL_013B;
			default:
				switch (unityRuntimePlatform)
				{
				case 11:
				case 13:
				case 16:
					break;
				case 12:
				case 14:
				case 15:
				case 17:
					goto IL_013B;
				default:
					goto IL_013B;
				}
				break;
			}
		}
		else if (unityRuntimePlatform != 25)
		{
			if (unityRuntimePlatform == 27)
			{
				goto IL_00E4;
			}
			switch (unityRuntimePlatform)
			{
			case 31:
			case 32:
			case 33:
			case 34:
			case 35:
			case 39:
			case 40:
			case 41:
			case 42:
				goto IL_013B;
			case 36:
			case 37:
			case 44:
				goto IL_00E4;
			case 38:
			case 43:
			case 45:
				break;
			default:
				goto IL_013B;
			}
		}
		return _path[0] == '/' || _path[0] == '\\' || _path.StartsWith("~/") || _path.StartsWith("~\\");
		IL_00E4:
		return _path[1] == ':' && (_path[2] == '/' || _path[2] == '\\') && ((_path[0] >= 'A' && _path[0] <= 'Z') || (_path[0] >= 'a' && _path[0] <= 'z'));
		IL_013B:
		throw new ArgumentOutOfRangeException("_path", _path, "Unsupported platform");
	}

	public static string MakeAbsolutePath(string _path)
	{
		if (GameIO.IsAbsolutePath(_path))
		{
			return _path;
		}
		return GameIO.GetGamePath() + "/" + _path;
	}

	public static string GetOsStylePath(string _path)
	{
		if (GameIO.m_UnityRuntimePlatform != 2 && GameIO.m_UnityRuntimePlatform != 44)
		{
			return _path.Replace("\\", "/");
		}
		return _path.Replace("/", "\\");
	}

	public static void CopyDirectory(string _sourceDirectory, string _targetDirectory)
	{
		SdDirectoryInfo sdDirectoryInfo = new SdDirectoryInfo(_sourceDirectory);
		SdDirectoryInfo sdDirectoryInfo2 = new SdDirectoryInfo(_targetDirectory);
		GameIO.CopyAll(sdDirectoryInfo, sdDirectoryInfo2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CopyAll(SdDirectoryInfo _source, SdDirectoryInfo _target)
	{
		SdDirectory.CreateDirectory(_target.FullName);
		foreach (SdFileInfo sdFileInfo in _source.GetFiles())
		{
			sdFileInfo.CopyTo(Path.Combine(_target.FullName, sdFileInfo.Name), true);
		}
		foreach (SdDirectoryInfo sdDirectoryInfo in _source.GetDirectories())
		{
			SdDirectoryInfo sdDirectoryInfo2 = _target.CreateSubdirectory(sdDirectoryInfo.Name);
			GameIO.CopyAll(sdDirectoryInfo, sdDirectoryInfo2);
		}
	}

	public static void SafeDirectoryCopy(string _sourceDirectory, string _targetDirectory)
	{
		try
		{
			GameIO.CopyDirectory(_sourceDirectory, _targetDirectory);
		}
		catch
		{
			try
			{
				if (SdDirectory.Exists(_targetDirectory))
				{
					SdDirectory.Delete(_targetDirectory, true);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Failed to cleanup target path " + _targetDirectory + " after failed directory move");
				Log.Exception(ex);
			}
			throw;
		}
	}

	public static void SafeDirectoryMove(string _sourceDirectory, string _targetDirectory)
	{
		GameIO.SafeDirectoryCopy(_sourceDirectory, _targetDirectory);
		try
		{
			SdDirectory.Delete(_sourceDirectory, true);
		}
		catch (Exception ex)
		{
			Log.Error("Could not remove source directory " + _sourceDirectory);
			Log.Exception(ex);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OpenExplorerOnLinux(string _path)
	{
		_path = _path.Replace("\\", "/");
		if (SdFile.Exists(_path))
		{
			_path = Path.GetDirectoryName(_path);
		}
		if (_path.IndexOf(' ') >= 0)
		{
			_path = "\"" + _path + "\"";
		}
		try
		{
			Process.Start("xdg-open", _path);
		}
		catch (Exception ex)
		{
			Log.Error("Failed opening file browser:");
			Log.Exception(ex);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OpenExplorerOnMac(string _path)
	{
		_path = _path.Replace("\\", "/");
		bool flag = SdDirectory.Exists(_path);
		if (!_path.StartsWith("\""))
		{
			_path = "\"" + _path;
		}
		if (!_path.EndsWith("\""))
		{
			_path += "\"";
		}
		try
		{
			Process.Start("open", (flag ? "" : "-R ") + _path);
		}
		catch (Exception ex)
		{
			Log.Error("Failed opening Finder:");
			Log.Exception(ex);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OpenExplorerOnWin(string _path)
	{
		_path = _path.Replace("/", "\\");
		bool flag = SdDirectory.Exists(_path);
		try
		{
			Process.Start("explorer.exe", (flag ? "/root,\"" : "/select,\"") + _path + "\"");
		}
		catch (Exception ex)
		{
			Log.Error("Failed opening Explorer:");
			Log.Exception(ex);
		}
	}

	public static void OpenExplorer(string _path)
	{
		RuntimePlatform platform = Application.platform;
		if (platform > 7)
		{
			if (platform != 13 && platform != 16)
			{
				switch (platform)
				{
				case 43:
					break;
				case 44:
					goto IL_0039;
				case 45:
					goto IL_0040;
				default:
					goto IL_004E;
				}
			}
			GameIO.OpenExplorerOnLinux(_path);
			return;
		}
		if (platform <= 1)
		{
			goto IL_0040;
		}
		if (platform != 2 && platform != 7)
		{
			goto IL_004E;
		}
		IL_0039:
		GameIO.OpenExplorerOnWin(_path);
		return;
		IL_0040:
		GameIO.OpenExplorerOnMac(_path);
		return;
		IL_004E:
		Log.Error("Failed opening file browser: Unsupported OS");
	}

	public static string IsRunningAsSnap()
	{
		if (SystemInfo.operatingSystemFamily != 3)
		{
			return null;
		}
		string environmentVariable = Environment.GetEnvironmentVariable("SNAP_NAME");
		if (environmentVariable == null)
		{
			Log.Out("Snap detection: Not running as Snap (no SNAP_NAME environment variable)");
			return null;
		}
		Log.Out("Snap detection: Running as Snap (Snap package: '" + environmentVariable + "')");
		return environmentVariable;
	}

	public static bool IsRunningInSteamRuntime()
	{
		if (SystemInfo.operatingSystemFamily != 3)
		{
			return false;
		}
		string text = "/etc/os-release";
		if (!SdFile.Exists(text))
		{
			Log.Out("SteamRuntime detection: Linux OS file " + text + " does not exist");
			return false;
		}
		foreach (string text2 in SdFile.ReadAllLines(text))
		{
			Match match = GameIO.linuxOsReleaseMatcher.Match(text2);
			if (match.Success)
			{
				string value = match.Groups[1].Value;
				bool flag = value.EqualsCaseInsensitive("steamrt");
				Log.Out(string.Format("SteamRuntime detection: OS ID='{0}', is SteamRT={1}", value, flag));
				return flag;
			}
		}
		Log.Out("SteamRuntime detection: No ID line matched");
		return false;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static string GetPostTerminationAccessiblePath()
	{
		string text = GameIO.GetApplicationScratchPath() + "/" + "7 Days To Die".Replace(" ", "");
		if (!text.EndsWith('/') && !text.EndsWith('\\'))
		{
			text += "/";
		}
		return text;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <InitializeUserDataPaths>g__PopulateCache|36_0(UserDataStorageType _storage, string _rootPath)
	{
		GameIO.m_cachedUserDataPaths.Add(new ValueTuple<UserDataStorageType, string>(_storage, ""), _rootPath);
		GameIO.m_cachedUserDataPaths.Add(new ValueTuple<UserDataStorageType, string>(_storage, "Saves"), Path.Combine(_rootPath, "Saves"));
		GameIO.m_cachedUserDataPaths.Add(new ValueTuple<UserDataStorageType, string>(_storage, "SavesLocal"), Path.Combine(_rootPath, "SavesLocal"));
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static int <GetPlayerSaves>g__SearchSaveDir|57_0(UserDataStorageType storage, ref GameIO.<>c__DisplayClass57_0 A_1)
	{
		int num = 0;
		string saveGameRootDir = GameIO.GetSaveGameRootDir(storage);
		if (!SdDirectory.Exists(saveGameRootDir))
		{
			return 0;
		}
		SdFileSystemInfo[] array = new SdDirectoryInfo(saveGameRootDir).GetDirectories();
		foreach (SdDirectoryInfo sdDirectoryInfo in array)
		{
			string fullName = sdDirectoryInfo.FullName;
			if (SdDirectory.Exists(fullName))
			{
				SdFileSystemInfo[] array2 = new SdDirectoryInfo(fullName).GetDirectories();
				foreach (SdDirectoryInfo sdDirectoryInfo2 in array2)
				{
					if (!sdDirectoryInfo2.Name.Contains("#"))
					{
						bool flag = SdFile.Exists(Path.Combine(sdDirectoryInfo2.FullName, "archived.flag"));
						if (A_1.includeArchived || !flag)
						{
							string text = sdDirectoryInfo2.FullName + "/main.ttw";
							if (SdFile.Exists(text))
							{
								try
								{
									WorldState worldState = new WorldState();
									worldState.Load(text, false, false, false);
									if (worldState.gameVersion != null)
									{
										GameIO.FoundSave foundSave = A_1._foundSave;
										if (foundSave != null)
										{
											foundSave(storage, sdDirectoryInfo2.Name, sdDirectoryInfo.Name, SdFile.GetLastWriteTime(text), worldState, flag);
										}
										num++;
									}
								}
								catch (Exception ex)
								{
									Log.Warning("Error reading header of level '" + text + "'. Ignoring. Msg: " + ex.Message);
								}
							}
						}
					}
				}
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string m_ApplicationScratchPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string XB1ScratchPath = "D:";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PS4ScratchPath = "/hostapp";

	[PublicizedFrom(EAccessModifier.Private)]
	public static RuntimePlatform m_UnityRuntimePlatform = Application.platform;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string m_UnityDataPath = Application.dataPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string m_ApplicationPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] pathTrimCharacters = new char[] { '/', '\\' };

	public static readonly char[] ResourcePathSeparators = new char[] { '/', '\\', '?' };

	[PublicizedFrom(EAccessModifier.Private)]
	public static string m_lastDeviceLocalRoot;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<ValueTuple<UserDataStorageType, string>, string> m_cachedUserDataPaths;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Regex m_isRoamingPathRegex;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex linuxOsReleaseMatcher = new Regex("^ID=['\"]?([^'\"]+)['\"]?$");

	public delegate void FoundSave(UserDataStorageType storage, string saveName, string worldName, DateTime lastSaved, WorldState worldState, bool isArchived);
}
