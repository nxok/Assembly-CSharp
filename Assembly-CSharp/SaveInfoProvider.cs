using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Platform;
using Platform.XBL.Save.Storage;
using UnityEngine;

public class SaveInfoProvider
{
	public static SaveInfoProvider Instance
	{
		get
		{
			if (SaveInfoProvider.instance == null)
			{
				SaveInfoProvider.instance = new SaveInfoProvider();
			}
			return SaveInfoProvider.instance;
		}
	}

	public static bool DataLimitEnabled
	{
		get
		{
			return SaveDataUtils.SaveDataManager.ShouldLimitSize();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetWorldEntryKey(string worldName, string fullPath)
	{
		return worldName.ToLowerInvariant() + "_" + GameIO.GetNormalizedPath(fullPath);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetWorldEntryKey(string key)
	{
		return key.ToLowerInvariant();
	}

	public static string GetWorldEntryKey(string worldName, UserDataStorageType userStorageType)
	{
		if (string.IsNullOrEmpty(worldName))
		{
			return null;
		}
		PathAbstractions.SearchDefinition worldsSearchPaths = PathAbstractions.WorldsSearchPaths;
		UserDataStorageType? userDataStorageType = new UserDataStorageType?(userStorageType);
		PathAbstractions.AbstractedLocation location = worldsSearchPaths.GetLocation(worldName, null, userDataStorageType);
		if (location.Type == PathAbstractions.EAbstractedLocationType.None)
		{
			return SaveInfoProvider.GetWorldEntryKey(worldName);
		}
		return SaveInfoProvider.GetWorldEntryKey(worldName, location.FullPath);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetLocalSaveEntryKey(string worldName, string saveName, UserDataStorageType saveStorage)
	{
		return string.Format("{0}/{1}_{2}", worldName, saveName, saveStorage).ToLowerInvariant();
	}

	public IReadOnlyCollection<SaveInfoProvider.WorldEntryInfo> WorldEntryInfos
	{
		get
		{
			this.RefreshIfDirty();
			return this.worldEntryInfos;
		}
	}

	public IReadOnlyCollection<SaveInfoProvider.SaveEntryInfo> SaveEntryInfos
	{
		get
		{
			this.RefreshIfDirty();
			return this.saveEntryInfos;
		}
	}

	public long TotalUsedBytes
	{
		get
		{
			this.RefreshIfDirty();
			return this.totalUsedBytes;
		}
	}

	public long TotalAllowanceBytes
	{
		get
		{
			this.RefreshIfDirty();
			return this.totalAllowanceBytes;
		}
	}

	public long TotalAvailableBytes
	{
		get
		{
			this.RefreshIfDirty();
			return this.totalAllowanceBytes - this.totalUsedBytes;
		}
	}

	public void SetDirty()
	{
		this.isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static long GetPlatformReservedSizeBytes(SaveDataSizes sizes)
	{
		if (PlatformManager.NativePlatform.PlatformIdentifier == EPlatformIdentifier.XBL)
		{
			long num = ((LaunchPrefs.GameCoreSaveStorageProvider.Value == SaveStorageProvider.Files) ? 32L : 16L);
			return 5242880L + num * 1048576L;
		}
		return 5242880L;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshIfDirty()
	{
		if (!this.isDirty)
		{
			return;
		}
		this.isDirty = false;
		this.localSavesSum = 0L;
		this.remoteSavesSum = 0L;
		this.worldsSum = 0L;
		this.worldEntryInfosByWorldKey.Clear();
		this.deletedWorlds.Clear();
		this.conflictedWorlds.Clear();
		this.saveEntryInfosBySaveKey.Clear();
		this.remoteSaveEntryInfosByGuid.Clear();
		this.worldEntryInfos.Clear();
		this.saveEntryInfos.Clear();
		this.ProcessLocalWorlds();
		this.ProcessLocalWorldSaves();
		this.ProcessRemoteWorldSaves();
		this.worldEntryInfos.AddRange(this.worldEntryInfosByWorldKey.Values);
		this.worldEntryInfos.AddRange(this.deletedWorlds.Values);
		this.worldEntryInfos.AddRange(this.conflictedWorlds.Values);
		this.totalUsedBytes = this.localSavesSum + this.remoteSavesSum + this.worldsSum;
		long num = 0L;
		this.worldEntryInfos.Sort();
		foreach (SaveInfoProvider.WorldEntryInfo worldEntryInfo in this.worldEntryInfos)
		{
			worldEntryInfo.BarStartOffset = num;
			if (worldEntryInfo.UsesDataLimit)
			{
				num += worldEntryInfo.WorldDataSize;
			}
			worldEntryInfo.SaveEntryInfos.Sort();
			foreach (SaveInfoProvider.SaveEntryInfo saveEntryInfo in worldEntryInfo.SaveEntryInfos)
			{
				saveEntryInfo.PlayerEntryInfos.Sort();
				if (saveEntryInfo.SizeInfo.UsesDataLimit)
				{
					saveEntryInfo.BarStartOffset = num;
					num += saveEntryInfo.SizeInfo.ReportedSize;
					long num2 = num;
					for (int i = saveEntryInfo.PlayerEntryInfos.Count - 1; i >= 0; i--)
					{
						SaveInfoProvider.PlayerEntryInfo playerEntryInfo = saveEntryInfo.PlayerEntryInfos[i];
						num2 -= playerEntryInfo.Size;
						playerEntryInfo.BarStartOffset = num2;
					}
				}
			}
		}
		if (SaveDataUtils.SaveDataManager.ShouldLimitSize())
		{
			SaveDataUtils.SaveDataManager.UpdateSizes();
			SaveDataSizes sizes = SaveDataUtils.SaveDataManager.GetSizes();
			long platformReservedSizeBytes = SaveInfoProvider.GetPlatformReservedSizeBytes(sizes);
			this.totalAllowanceBytes = sizes.Total - platformReservedSizeBytes;
			return;
		}
		this.totalAllowanceBytes = -1L;
	}

	public void ClearResources()
	{
		this.worldEntryInfosByWorldKey.Clear();
		this.deletedWorlds.Clear();
		this.conflictedWorlds.Clear();
		this.saveEntryInfosBySaveKey.Clear();
		this.remoteSaveEntryInfosByGuid.Clear();
		this.worldEntryInfos.Clear();
		this.saveEntryInfos.Clear();
		this.protectedDirectories.Clear();
		this.isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessLocalWorlds()
	{
		foreach (PathAbstractions.AbstractedLocation abstractedLocation in PathAbstractions.WorldsSearchPaths.GetAvailablePathsList(null, false, null))
		{
			PathAbstractions.EAbstractedLocationType type = abstractedLocation.Type;
			string text;
			if (type != PathAbstractions.EAbstractedLocationType.Mods)
			{
				if (type != PathAbstractions.EAbstractedLocationType.GameData)
				{
					text = Localization.Get("xuiDmGenerated", false, null);
				}
				else
				{
					text = Localization.Get("xuiDmBuiltIn", false, null);
				}
			}
			else
			{
				text = Localization.Get("xuiDmMod", false, null) + ": " + abstractedLocation.ContainingMod.Name;
			}
			string text2 = text;
			bool flag = GameIO.IsWorldGenerated(abstractedLocation.Name, abstractedLocation.StorageType);
			SaveInfoProvider.WorldEntryInfo worldEntryInfo = new SaveInfoProvider.WorldEntryInfo
			{
				WorldKey = SaveInfoProvider.GetWorldEntryKey(abstractedLocation.Name, abstractedLocation.FullPath),
				Name = abstractedLocation.Name,
				Type = text2,
				Location = abstractedLocation,
				Deletable = flag,
				Moveable = flag,
				WorldDataSize = GameIO.GetDirectorySize(abstractedLocation.FullPath, true),
				Version = null,
				HideIfEmpty = (!flag || SaveInfoProvider.HideableWorlds.ContainsCaseInsensitive(abstractedLocation.Name))
			};
			if (worldEntryInfo.UsesDataLimit)
			{
				this.worldsSum += worldEntryInfo.WorldDataSize;
			}
			GameUtils.WorldInfo worldInfo = GameUtils.WorldInfo.LoadWorldInfo(abstractedLocation);
			if (worldInfo != null)
			{
				worldEntryInfo.Version = worldInfo.GameVersionCreated;
				worldEntryInfo.WorldSize = new Vector2i?(worldInfo.WorldSize);
			}
			this.worldEntryInfosByWorldKey[worldEntryInfo.WorldKey] = worldEntryInfo;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessLocalWorldSaves()
	{
		if (PlatformManager.MultiPlatform.UserDataRoaming.SaveRoamingEnabled)
		{
			this.ProcessLocalWorldSaves(UserDataStorageType.Roaming);
		}
		this.ProcessLocalWorldSaves(UserDataStorageType.DeviceLocal);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessLocalWorldSaves(UserDataStorageType storageType)
	{
		string saveGameRootDir = GameIO.GetSaveGameRootDir(storageType);
		if (SdDirectory.Exists(saveGameRootDir))
		{
			SdFileSystemInfo[] array = new SdDirectoryInfo(saveGameRootDir).GetDirectories();
			foreach (SdDirectoryInfo sdDirectoryInfo in array)
			{
				if (SdDirectory.Exists(sdDirectoryInfo.FullName))
				{
					SdDirectoryInfo sdDirectoryInfo2 = new SdDirectoryInfo(sdDirectoryInfo.FullName);
					SdFileSystemInfo[] array2 = sdDirectoryInfo2.GetDirectories();
					SdFileSystemInfo[] array3 = array2;
					if (array3.Length == 0 && sdDirectoryInfo2.GetFiles().Length == 0)
					{
						SdDirectory.Delete(sdDirectoryInfo.FullName);
					}
					else
					{
						string name = sdDirectoryInfo.Name;
						foreach (SdDirectoryInfo sdDirectoryInfo3 in array3)
						{
							SaveInfoProvider.WorldEntryInfo worldEntryInfo = null;
							int num = 0;
							int num2 = 0;
							UserDataStorageType userDataStorageType = UserDataStorageType.DeviceLocal;
							foreach (SaveInfoProvider.WorldEntryInfo worldEntryInfo2 in this.worldEntryInfosByWorldKey.Values)
							{
								if (!worldEntryInfo2.Type.Equals(SaveInfoProvider.RemoteWorldsType) && worldEntryInfo2.Name.Equals(name))
								{
									num++;
									if (worldEntryInfo2.Location.Type == PathAbstractions.EAbstractedLocationType.UserDataPath)
									{
										num2++;
										userDataStorageType = worldEntryInfo2.Location.StorageType;
									}
									worldEntryInfo = worldEntryInfo2;
								}
							}
							if (num > 1)
							{
								string worldEntryKey = SaveInfoProvider.GetWorldEntryKey(name + "_" + SaveInfoProvider.ConflictedWorldsType);
								if (!this.conflictedWorlds.TryGetValue(worldEntryKey, out worldEntryInfo))
								{
									PathAbstractions.AbstractedLocation abstractedLocation = PathAbstractions.AbstractedLocation.None;
									if (num2 <= 1)
									{
										PathAbstractions.SearchDefinition worldsSearchPaths = PathAbstractions.WorldsSearchPaths;
										string text = name;
										UserDataStorageType? userDataStorageType2 = new UserDataStorageType?(userDataStorageType);
										abstractedLocation = worldsSearchPaths.GetLocation(text, null, userDataStorageType2);
									}
									worldEntryInfo = new SaveInfoProvider.WorldEntryInfo
									{
										WorldKey = worldEntryKey,
										Name = name,
										Type = SaveInfoProvider.ConflictedWorldsType,
										Location = abstractedLocation,
										Deletable = false,
										Moveable = false,
										WorldDataSize = 0L,
										Version = null,
										HideIfEmpty = true
									};
									this.conflictedWorlds.Add(worldEntryInfo.WorldKey, worldEntryInfo);
								}
							}
							if (worldEntryInfo == null)
							{
								string worldEntryKey2 = SaveInfoProvider.GetWorldEntryKey(sdDirectoryInfo.Name + "_" + SaveInfoProvider.DeletedWorldsType);
								if (!this.deletedWorlds.TryGetValue(worldEntryKey2, out worldEntryInfo))
								{
									worldEntryInfo = new SaveInfoProvider.WorldEntryInfo
									{
										WorldKey = worldEntryKey2,
										Name = sdDirectoryInfo.Name,
										Type = SaveInfoProvider.DeletedWorldsType,
										Location = PathAbstractions.AbstractedLocation.None,
										Deletable = false,
										Moveable = false,
										WorldDataSize = 0L,
										Version = null,
										HideIfEmpty = true
									};
									this.deletedWorlds.Add(worldEntryInfo.WorldKey, worldEntryInfo);
								}
							}
							this.ProcessSaveEntry(worldEntryInfo, sdDirectoryInfo3, storageType);
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessSaveEntry(SaveInfoProvider.WorldEntryInfo worldEntryInfo, SdDirectoryInfo curSaveFolder, UserDataStorageType storageType)
	{
		string text = curSaveFolder.FullName + "/main.ttw";
		SaveInfoProvider.SaveEntryInfo saveEntryInfo = new SaveInfoProvider.SaveEntryInfo
		{
			Name = curSaveFolder.Name,
			WorldEntry = worldEntryInfo,
			SaveDir = curSaveFolder.FullName,
			StorageType = storageType,
			Version = null
		};
		saveEntryInfo.SizeInfo.BytesOnDisk = GameIO.GetDirectorySize(curSaveFolder, true);
		saveEntryInfo.SizeInfo.IsArchived = SaveInfoProvider.DataLimitEnabled && SdFile.Exists(Path.Combine(curSaveFolder.FullName, "archived.flag"));
		saveEntryInfo.SizeInfo.StorageType = storageType;
		if (SdFile.Exists(text))
		{
			saveEntryInfo.LastSaved = SdFile.GetLastWriteTime(text);
			try
			{
				WorldState worldState = new WorldState();
				worldState.Load(text, false, false, false);
				saveEntryInfo.Version = worldState.gameVersion;
				saveEntryInfo.SizeInfo.BytesReserved = worldState.saveDataLimit;
				if (SaveInfoProvider.DataLimitEnabled && saveEntryInfo.SizeInfo.BytesReserved != -1L && saveEntryInfo.SizeInfo.BytesOnDisk > saveEntryInfo.SizeInfo.BytesReserved)
				{
					Debug.LogError(string.Format("Directory size of save \"{0}\" exceeds serialized save data limit of {1}.", curSaveFolder.FullName, worldState.saveDataLimit));
				}
				goto IL_0181;
			}
			catch (Exception ex)
			{
				Log.Warning("Error reading header of level '" + text + "'. Msg: " + ex.Message);
				goto IL_0181;
			}
		}
		if (curSaveFolder.Name != "WorldEditor" && curSaveFolder.Name != "PrefabEditor")
		{
			Log.Warning(string.Format("Could not find main ttw file for save in directory: {0}", curSaveFolder));
		}
		saveEntryInfo.LastSaved = curSaveFolder.LastWriteTime;
		IL_0181:
		this.saveEntryInfos.Add(saveEntryInfo);
		string localSaveEntryKey = SaveInfoProvider.GetLocalSaveEntryKey(saveEntryInfo.WorldEntry.Name, saveEntryInfo.Name, saveEntryInfo.StorageType);
		this.saveEntryInfosBySaveKey[localSaveEntryKey] = saveEntryInfo;
		worldEntryInfo.SaveEntryInfos.Add(saveEntryInfo);
		worldEntryInfo.SaveDataCount++;
		worldEntryInfo.SaveDataSizeTotal += saveEntryInfo.SizeInfo.ReportedSize;
		if (saveEntryInfo.SizeInfo.UsesDataLimit)
		{
			worldEntryInfo.SaveDataSizeForLimit += saveEntryInfo.SizeInfo.ReportedSize;
			this.localSavesSum += saveEntryInfo.SizeInfo.ReportedSize;
		}
		this.ProcessPlayerEntries(saveEntryInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessPlayerEntries(SaveInfoProvider.SaveEntryInfo saveEntryInfo)
	{
		string text = saveEntryInfo.SaveDir + "/Player";
		if (!SdDirectory.Exists(text))
		{
			return;
		}
		this.fileNameKeysToPlayerInfos.Clear();
		foreach (SdFileInfo sdFileInfo in new SdDirectoryInfo(text).GetFiles())
		{
			int num;
			string text2;
			string text3;
			if ((num = sdFileInfo.Name.IndexOf('.')) != -1)
			{
				text2 = sdFileInfo.Name.Substring(0, num);
				int num2 = sdFileInfo.Name.LastIndexOf('.') + 1;
				text3 = sdFileInfo.Name.Substring(num2, sdFileInfo.Name.Length - num2);
			}
			else
			{
				Debug.LogError("Encountered player save file with no extension.");
				text2 = sdFileInfo.Name;
				text3 = string.Empty;
			}
			DateTime lastWriteTime = SdFile.GetLastWriteTime(sdFileInfo.FullName);
			SaveInfoProvider.PlayerEntryInfo playerEntryInfo;
			if (!this.fileNameKeysToPlayerInfos.TryGetValue(text2, out playerEntryInfo))
			{
				playerEntryInfo = new SaveInfoProvider.PlayerEntryInfo
				{
					Id = text2,
					LastPlayed = lastWriteTime,
					SaveEntry = saveEntryInfo
				};
				PlatformUserIdentifierAbs platformUserIdentifierAbs;
				if (PlatformUserIdentifierAbs.TryFromCombinedString(text2, out platformUserIdentifierAbs))
				{
					playerEntryInfo.PrimaryUserId = platformUserIdentifierAbs;
					playerEntryInfo.CachedName = platformUserIdentifierAbs.ReadablePlatformUserIdentifier;
				}
				else
				{
					Log.Error("Could not associate player save file \"" + sdFileInfo.FullName + "\" with a player id. Combined id string: " + text2);
					playerEntryInfo.CachedName = text2;
				}
				saveEntryInfo.PlayerEntryInfos.Add(playerEntryInfo);
			}
			else if (lastWriteTime > playerEntryInfo.LastPlayed)
			{
				playerEntryInfo.LastPlayed = lastWriteTime;
			}
			playerEntryInfo.Size += sdFileInfo.Length;
			PlayerMetaInfo playerMetaInfo;
			if (string.Equals(text3, "meta", StringComparison.InvariantCultureIgnoreCase) && PlayerMetaInfo.TryRead(sdFileInfo.FullName, out playerMetaInfo))
			{
				if (playerMetaInfo.NativeId != null)
				{
					playerEntryInfo.NativeUserId = playerMetaInfo.NativeId;
				}
				if (playerMetaInfo.Name != null)
				{
					playerEntryInfo.CachedName = playerMetaInfo.Name;
				}
				playerEntryInfo.PlayerLevel = playerMetaInfo.Level;
				playerEntryInfo.DistanceWalked = playerMetaInfo.DistanceWalked;
			}
			this.fileNameKeysToPlayerInfos[text2] = playerEntryInfo;
		}
		foreach (KeyValuePair<string, SaveInfoProvider.PlayerEntryInfo> keyValuePair in this.fileNameKeysToPlayerInfos)
		{
			long directorySize = GameIO.GetDirectorySize(new SdDirectoryInfo(Path.Combine(text, keyValuePair.Key)), true);
			keyValuePair.Value.Size += directorySize;
		}
		this.fileNameKeysToPlayerInfos.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessRemoteWorldSaves()
	{
		if (PlatformManager.MultiPlatform.UserDataRoaming.SaveRoamingEnabled)
		{
			this.ProcessRemoteWorldSaves(UserDataStorageType.Roaming);
		}
		this.ProcessRemoteWorldSaves(UserDataStorageType.DeviceLocal);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessRemoteWorldSaves(UserDataStorageType storageType)
	{
		string saveGameLocalRootDir = GameIO.GetSaveGameLocalRootDir(storageType);
		if (!SdDirectory.Exists(saveGameLocalRootDir))
		{
			return;
		}
		SdFileSystemInfo[] array = new SdDirectoryInfo(saveGameLocalRootDir).GetDirectories();
		foreach (SdDirectoryInfo sdDirectoryInfo in array)
		{
			string text = sdDirectoryInfo.FullName + "/RemoteWorldInfo.xml";
			SaveInfoProvider.SaveEntryInfo saveEntryInfo = new SaveInfoProvider.SaveEntryInfo
			{
				SaveDir = sdDirectoryInfo.FullName,
				StorageType = storageType
			};
			saveEntryInfo.SizeInfo.BytesOnDisk = GameIO.GetDirectorySize(sdDirectoryInfo, true);
			saveEntryInfo.SizeInfo.IsArchived = SaveInfoProvider.DataLimitEnabled && SdFile.Exists(Path.Combine(sdDirectoryInfo.FullName, "archived.flag"));
			saveEntryInfo.SizeInfo.StorageType = storageType;
			PathAbstractions.AbstractedLocation abstractedLocation = PathAbstractions.Contextual.FindDownloadedRemoteWorld(sdDirectoryInfo.FullName);
			RemoteWorldInfo remoteWorldInfo;
			SaveInfoProvider.WorldEntryInfo worldEntryInfo;
			if (RemoteWorldInfo.TryRead(text, out remoteWorldInfo))
			{
				string text2 = ((abstractedLocation.Type != PathAbstractions.EAbstractedLocationType.None) ? SaveInfoProvider.GetWorldEntryKey(remoteWorldInfo.WorldName, abstractedLocation.FullPath) : SaveInfoProvider.GetWorldEntryKey(remoteWorldInfo.WorldName));
				if (!this.worldEntryInfosByWorldKey.TryGetValue(text2, out worldEntryInfo))
				{
					worldEntryInfo = new SaveInfoProvider.WorldEntryInfo
					{
						WorldKey = text2,
						Name = remoteWorldInfo.WorldName,
						Type = SaveInfoProvider.RemoteWorldsType,
						Location = abstractedLocation,
						Deletable = false,
						Moveable = false,
						WorldDataSize = 0L,
						Version = remoteWorldInfo.GameVersion
					};
					this.worldEntryInfosByWorldKey[text2] = worldEntryInfo;
				}
				saveEntryInfo.SizeInfo.BytesReserved = remoteWorldInfo.SaveSize;
				if (SaveInfoProvider.DataLimitEnabled && saveEntryInfo.SizeInfo.BytesReserved != -1L && saveEntryInfo.SizeInfo.BytesOnDisk > saveEntryInfo.SizeInfo.BytesReserved)
				{
					Debug.LogError(string.Format("Directory size of save \"{0}\" exceeds serialized save data size of {1}.", sdDirectoryInfo.FullName, remoteWorldInfo.SaveSize));
				}
				saveEntryInfo.Name = remoteWorldInfo.GameName;
				saveEntryInfo.WorldEntry = worldEntryInfo;
				saveEntryInfo.LastSaved = SdFile.GetLastWriteTime(text);
				saveEntryInfo.Version = remoteWorldInfo.GameVersion;
			}
			else
			{
				string worldEntryKey = SaveInfoProvider.GetWorldEntryKey(SaveInfoProvider.RemoteWorldsLabel);
				if (!this.worldEntryInfosByWorldKey.TryGetValue(worldEntryKey, out worldEntryInfo))
				{
					worldEntryInfo = new SaveInfoProvider.WorldEntryInfo
					{
						WorldKey = worldEntryKey,
						Name = SaveInfoProvider.RemoteWorldsLabel,
						Type = SaveInfoProvider.RemoteWorldsType,
						Location = abstractedLocation,
						Deletable = false,
						Moveable = false,
						WorldDataSize = 0L,
						Version = null
					};
					this.worldEntryInfosByWorldKey[worldEntryKey] = worldEntryInfo;
				}
				saveEntryInfo.Name = sdDirectoryInfo.Name;
				saveEntryInfo.WorldEntry = worldEntryInfo;
				saveEntryInfo.LastSaved = sdDirectoryInfo.LastWriteTime;
				saveEntryInfo.Version = null;
			}
			this.saveEntryInfos.Add(saveEntryInfo);
			this.remoteSaveEntryInfosByGuid[sdDirectoryInfo.Name] = saveEntryInfo;
			worldEntryInfo.SaveEntryInfos.Add(saveEntryInfo);
			worldEntryInfo.SaveDataCount++;
			worldEntryInfo.SaveDataSizeTotal += saveEntryInfo.SizeInfo.ReportedSize;
			if (saveEntryInfo.SizeInfo.UsesDataLimit)
			{
				worldEntryInfo.SaveDataSizeForLimit += saveEntryInfo.SizeInfo.ReportedSize;
				this.remoteSavesSum += saveEntryInfo.SizeInfo.ReportedSize;
			}
		}
	}

	public bool TryGetLocalSaveEntry(string worldName, string saveName, UserDataStorageType storage, out SaveInfoProvider.SaveEntryInfo saveEntryInfo)
	{
		this.RefreshIfDirty();
		string localSaveEntryKey = SaveInfoProvider.GetLocalSaveEntryKey(worldName, saveName, storage);
		return this.saveEntryInfosBySaveKey.TryGetValue(localSaveEntryKey, out saveEntryInfo);
	}

	public bool TryGetRemoteSaveEntry(string guid, out SaveInfoProvider.SaveEntryInfo saveEntryInfo)
	{
		this.RefreshIfDirty();
		return this.remoteSaveEntryInfosByGuid.TryGetValue(guid, out saveEntryInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string NormalizePath(string path)
	{
		return Path.GetFullPath(path);
	}

	public void SetDirectoryProtected(string path, bool isProtected)
	{
		if (isProtected)
		{
			this.protectedDirectories.Add(this.NormalizePath(path));
			return;
		}
		this.protectedDirectories.Remove(this.NormalizePath(path));
	}

	public bool IsDirectoryProtected(string path)
	{
		return this.protectedDirectories.Contains(this.NormalizePath(path));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const long BytesPerMB = 1048576L;

	public static readonly string RemoteWorldsLabel = "[" + Localization.Get("xuiDmRemoteWorlds", false, null) + "] ";

	public static readonly string RemoteWorldsType = Localization.Get("xuiDmRemote", false, null);

	public static readonly string DeletedWorldsType = Localization.Get("xuiDmDeleted", false, null);

	public static readonly string ConflictedWorldsType = Localization.Get("xuiDmConflicted", false, null);

	public static readonly List<string> HideableWorlds = new List<string> { "Empty", "Playtesting" };

	[PublicizedFrom(EAccessModifier.Private)]
	public static SaveInfoProvider instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, SaveInfoProvider.WorldEntryInfo> worldEntryInfosByWorldKey = new Dictionary<string, SaveInfoProvider.WorldEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, SaveInfoProvider.WorldEntryInfo> deletedWorlds = new Dictionary<string, SaveInfoProvider.WorldEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, SaveInfoProvider.WorldEntryInfo> conflictedWorlds = new Dictionary<string, SaveInfoProvider.WorldEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, SaveInfoProvider.SaveEntryInfo> saveEntryInfosBySaveKey = new Dictionary<string, SaveInfoProvider.SaveEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, SaveInfoProvider.SaveEntryInfo> remoteSaveEntryInfosByGuid = new Dictionary<string, SaveInfoProvider.SaveEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SaveInfoProvider.WorldEntryInfo> worldEntryInfos = new List<SaveInfoProvider.WorldEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SaveInfoProvider.SaveEntryInfo> saveEntryInfos = new List<SaveInfoProvider.SaveEntryInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<string> protectedDirectories = new HashSet<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public long localSavesSum;

	[PublicizedFrom(EAccessModifier.Private)]
	public long remoteSavesSum;

	[PublicizedFrom(EAccessModifier.Private)]
	public long worldsSum;

	[PublicizedFrom(EAccessModifier.Private)]
	public long totalUsedBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public long totalAllowanceBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, SaveInfoProvider.PlayerEntryInfo> fileNameKeysToPlayerInfos = new Dictionary<string, SaveInfoProvider.PlayerEntryInfo>();

	public struct SaveSizeInfo
	{
		public long ReportedSize
		{
			get
			{
				if (!this.IsArchived && this.UsesDataLimit)
				{
					return this.MaxSize;
				}
				return this.BytesOnDisk;
			}
		}

		public long MaxSize
		{
			get
			{
				return Math.Max(this.BytesOnDisk, this.BytesReserved);
			}
		}

		public bool UsesDataLimit
		{
			get
			{
				return this.StorageType.UsesDataLimit();
			}
		}

		public bool Archivable
		{
			get
			{
				return this.UsesDataLimit && this.BytesReserved >= this.BytesOnDisk;
			}
		}

		public UserDataStorageType StorageType;

		public long BytesOnDisk;

		public long BytesReserved;

		public bool IsArchived;
	}

	public class WorldEntryInfo : IComparable
	{
		public bool UsesDataLimit
		{
			get
			{
				return this.Location.StorageType.UsesDataLimit();
			}
		}

		public string DisplayName
		{
			get
			{
				if (PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional && this.Location.Type == PathAbstractions.EAbstractedLocationType.UserDataPath)
				{
					return this.Name + " [808080][i](" + this.Location.StorageType.LocalizedName() + ")[/i][-]";
				}
				return this.Name;
			}
		}

		public bool ShouldBeMovedWithSave(UserDataStorageType targetSaveStorage)
		{
			return targetSaveStorage.UsesDataLimit() && this.Moveable && targetSaveStorage != this.Location.StorageType;
		}

		public int CompareTo(object obj)
		{
			SaveInfoProvider.WorldEntryInfo worldEntryInfo = obj as SaveInfoProvider.WorldEntryInfo;
			if (worldEntryInfo != null)
			{
				return string.Compare(this.WorldKey, worldEntryInfo.WorldKey, StringComparison.OrdinalIgnoreCase);
			}
			return 1;
		}

		public string WorldKey;

		public string Name;

		public string Type;

		public PathAbstractions.AbstractedLocation Location;

		public bool Deletable;

		public bool Moveable;

		public long WorldDataSize;

		public VersionInformation Version;

		public Vector2i? WorldSize;

		public long SaveDataSizeForLimit;

		public long SaveDataSizeTotal;

		public int SaveDataCount;

		public long BarStartOffset;

		public bool HideIfEmpty;

		public readonly List<SaveInfoProvider.SaveEntryInfo> SaveEntryInfos = new List<SaveInfoProvider.SaveEntryInfo>();
	}

	public class SaveEntryInfo : IComparable
	{
		public string DisplayName
		{
			get
			{
				if (PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional)
				{
					return this.Name + " [808080][i](" + this.StorageType.LocalizedName() + ")[/i][-]";
				}
				return this.Name;
			}
		}

		public int CompareTo(object obj)
		{
			SaveInfoProvider.SaveEntryInfo saveEntryInfo = obj as SaveInfoProvider.SaveEntryInfo;
			if (saveEntryInfo == null)
			{
				return 1;
			}
			int num = saveEntryInfo.LastSaved.CompareTo(this.LastSaved);
			if (num == 0)
			{
				return saveEntryInfo.Name.CompareTo(this.Name);
			}
			return num;
		}

		public string Name;

		public UserDataStorageType StorageType;

		public string SaveDir;

		public SaveInfoProvider.SaveSizeInfo SizeInfo;

		public long BarStartOffset;

		public SaveInfoProvider.WorldEntryInfo WorldEntry;

		public DateTime LastSaved;

		public VersionInformation Version;

		public readonly List<SaveInfoProvider.PlayerEntryInfo> PlayerEntryInfos = new List<SaveInfoProvider.PlayerEntryInfo>();
	}

	public class PlayerEntryInfo : IComparable
	{
		public string PlatformName
		{
			get
			{
				PlatformUserIdentifierAbs platformUserIdentifierAbs = this.NativeUserId ?? this.PrimaryUserId;
				return ((platformUserIdentifierAbs != null) ? platformUserIdentifierAbs.PlatformIdentifierString : null) ?? "-";
			}
		}

		public IPlatformUserData PlatformUserData
		{
			get
			{
				if (this.platformUserData == null && this.PrimaryUserId != null)
				{
					IPlatformUserData orCreate = PlatformUserManager.GetOrCreate(this.PrimaryUserId);
					if (this.NativeUserId != null)
					{
						orCreate.NativeId = this.NativeUserId;
					}
					this.platformUserData = orCreate;
				}
				return this.platformUserData;
			}
		}

		public int CompareTo(object obj)
		{
			SaveInfoProvider.PlayerEntryInfo playerEntryInfo = obj as SaveInfoProvider.PlayerEntryInfo;
			if (playerEntryInfo == null)
			{
				return 1;
			}
			int num = playerEntryInfo.LastPlayed.CompareTo(this.LastPlayed);
			if (num == 0)
			{
				return playerEntryInfo.CachedName.CompareTo(this.CachedName);
			}
			return num;
		}

		public string Id;

		public string CachedName;

		public PlatformUserIdentifierAbs PrimaryUserId;

		public PlatformUserIdentifierAbs NativeUserId;

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatformUserData platformUserData;

		public long Size;

		public long BarStartOffset;

		public SaveInfoProvider.SaveEntryInfo SaveEntry;

		public DateTime LastPlayed;

		public int PlayerLevel;

		public float DistanceWalked;
	}

	public class PlayerEntryInfoPlatformDataResolver
	{
		public bool IsComplete
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public PlayerEntryInfoPlatformDataResolver(List<SaveInfoProvider.PlayerEntryInfo> playerEntries)
		{
			this.pendingPlayerEntries = playerEntries;
		}

		public static SaveInfoProvider.PlayerEntryInfoPlatformDataResolver StartNew(IEnumerable<SaveInfoProvider.PlayerEntryInfo> playerEntries)
		{
			List<SaveInfoProvider.PlayerEntryInfo> list = new List<SaveInfoProvider.PlayerEntryInfo>();
			List<IPlatformUserData> list2 = null;
			foreach (SaveInfoProvider.PlayerEntryInfo playerEntryInfo in playerEntries)
			{
				list.Add(playerEntryInfo);
				IPlatformUserData platformUserData = playerEntryInfo.PlatformUserData;
				if (platformUserData != null)
				{
					if (list2 == null)
					{
						list2 = new List<IPlatformUserData>();
					}
					list2.Add(platformUserData);
				}
			}
			SaveInfoProvider.PlayerEntryInfoPlatformDataResolver playerEntryInfoPlatformDataResolver = new SaveInfoProvider.PlayerEntryInfoPlatformDataResolver(list);
			if (list2 == null)
			{
				playerEntryInfoPlatformDataResolver.IsComplete = true;
				return playerEntryInfoPlatformDataResolver;
			}
			if (!PlatformUserManager.AreUsersPendingResolve(list2))
			{
				playerEntryInfoPlatformDataResolver.IsComplete = true;
				return playerEntryInfoPlatformDataResolver;
			}
			ThreadManager.StartCoroutine(playerEntryInfoPlatformDataResolver.ResolveUserData(list2));
			return playerEntryInfoPlatformDataResolver;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator ResolveUserData(List<IPlatformUserData> resolvingPlatformData)
		{
			yield return PlatformUserManager.ResolveUsersDetailsCoroutine(resolvingPlatformData);
			yield return PlatformUserManager.ResolveUserBlocksCoroutine(resolvingPlatformData);
			this.IsComplete = true;
			yield break;
		}

		public readonly List<SaveInfoProvider.PlayerEntryInfo> pendingPlayerEntries;
	}
}
