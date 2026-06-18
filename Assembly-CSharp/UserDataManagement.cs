using System;
using System.IO;
using Platform;
using WorldGenerationEngineFinal;

public static class UserDataManagement
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static UserDataManagement.Result MoveWorld(SaveInfoProvider.WorldEntryInfo worldInfo, UserDataStorageType moveToStorageType)
	{
		string worldPath = WorldBuilder.GetWorldPath(moveToStorageType, worldInfo.Name);
		if (SdDirectory.Exists(worldPath))
		{
			Log.Error(string.Concat(new string[]
			{
				"Could not move world from ",
				worldInfo.Location.FullPath,
				" to ",
				worldPath,
				". Directory already exists"
			}));
			return UserDataManagement.Result.TargetAlreadyExists;
		}
		Log.Out(string.Concat(new string[]
		{
			"Moving world ",
			worldInfo.Name,
			" from ",
			worldInfo.Location.FullPath,
			" to ",
			worldPath
		}));
		try
		{
			GameIO.SafeDirectoryMove(worldInfo.Location.FullPath, worldPath);
		}
		catch (Exception ex)
		{
			Log.Error("Could not move world: " + ex.Message);
			Log.Exception(ex);
			return UserDataManagement.Result.Exception;
		}
		return UserDataManagement.Result.Success;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static UserDataManagement.Result MoveSave(string sourceDir, string targetDir)
	{
		if (SdDirectory.Exists(targetDir))
		{
			Log.Error(string.Concat(new string[] { "Could not move save from ", sourceDir, " to ", targetDir, ". Directory already exists" }));
			return UserDataManagement.Result.TargetAlreadyExists;
		}
		Log.Out("Moving save from " + sourceDir + " to " + targetDir);
		try
		{
			GameIO.SafeDirectoryMove(sourceDir, targetDir);
		}
		catch (Exception ex)
		{
			Log.Error("Could not move save: " + ex.Message);
			Log.Exception(ex);
			return UserDataManagement.Result.Exception;
		}
		return UserDataManagement.Result.Success;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static UserDataManagement.Result CopySave(string sourceDir, string targetDir)
	{
		if (SdDirectory.Exists(targetDir))
		{
			Log.Error(string.Concat(new string[] { "Could not copy save from ", sourceDir, " to ", targetDir, ". Directory already exists" }));
			return UserDataManagement.Result.TargetAlreadyExists;
		}
		Log.Out("Copying save from " + sourceDir + " to " + targetDir);
		try
		{
			GameIO.SafeDirectoryCopy(sourceDir, targetDir);
		}
		catch (Exception ex)
		{
			Log.Error("Could not copy save: " + ex.Message);
			Log.Exception(ex);
			return UserDataManagement.Result.Exception;
		}
		return UserDataManagement.Result.Success;
	}

	public static string GetSavePathTarget(SaveInfoProvider.SaveEntryInfo saveInfo, UserDataStorageType storageTarget)
	{
		SaveInfoProvider.WorldEntryInfo worldEntry = saveInfo.WorldEntry;
		if (worldEntry.Type == SaveInfoProvider.RemoteWorldsType)
		{
			return GameIO.GetSaveGameLocalDir(storageTarget, Path.GetFileName(saveInfo.SaveDir));
		}
		return GameIO.GetSaveGameDir(worldEntry.Name, saveInfo.Name, storageTarget);
	}

	public static void SetSaveArchived(string saveDir, bool shouldArchive)
	{
		try
		{
			string text = Path.Combine(saveDir, "archived.flag");
			if (SdFile.Exists(text))
			{
				if (!shouldArchive)
				{
					Log.Out("Unarchiving save by deleting: " + text);
					SdFile.Delete(text);
				}
			}
			else if (shouldArchive)
			{
				Log.Out("Archiving save by creating: " + text);
				using (SdFile.Create(text))
				{
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error(string.Format("Failed to set archived to {0} for save at {1}", shouldArchive, saveDir));
			Log.Exception(ex);
		}
	}

	public static void SetSaveDataLimit(string saveDir, long reservedBytes)
	{
		try
		{
			string text = Path.Combine(saveDir, "main.ttw");
			WorldState worldState = new WorldState();
			if (!worldState.Load(text, false, false, false))
			{
				Log.Error(string.Format("Could not update save data limit for {0} to {1}B. WorldState failed to load", saveDir, reservedBytes));
			}
			else if (worldState.saveDataLimit != reservedBytes)
			{
				Log.Out(string.Format("Updating reserved bytes for {0} to {1}", saveDir, reservedBytes));
				worldState.saveDataLimit = reservedBytes;
				if (!worldState.Save(text))
				{
					Log.Error(string.Format("Could not update save data limit for {0} to {1}B. WorldState failed to save", saveDir, reservedBytes));
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error(string.Format("Failed to set data limit to {0} for save at {1}", reservedBytes, saveDir));
			Log.Exception(ex);
		}
	}

	public enum Result
	{
		Success,
		TargetAlreadyExists,
		Exception,
		FailedToMoveSaves
	}

	public struct SaveMoveResultInfo
	{
		public UserDataManagement.Result Result;

		public string NewSaveDir;
	}

	public struct SaveCopyResultInfo
	{
		public UserDataManagement.Result Result;

		public string NewSaveDir;
	}

	public struct WorldMove
	{
		public bool IsReady
		{
			get
			{
				return this.worldInfo != null;
			}
		}

		public WorldMove(SaveInfoProvider.WorldEntryInfo worldInfo, UserDataStorageType moveToStorageType)
		{
			if (!worldInfo.Moveable)
			{
				throw new Exception("Cannot create world move op for non-moveable world");
			}
			this.worldInfo = worldInfo;
			this.moveToStorageType = moveToStorageType;
		}

		public int CountOfAssociatedSavesInSameStorage
		{
			get
			{
				int num = 0;
				foreach (SaveInfoProvider.SaveEntryInfo saveEntryInfo in SaveInfoProvider.Instance.SaveEntryInfos)
				{
					if (saveEntryInfo.WorldEntry == this.worldInfo && saveEntryInfo.StorageType == this.worldInfo.Location.StorageType)
					{
						num++;
					}
				}
				return num;
			}
		}

		public UserDataManagement.Result PerformMove()
		{
			UserDataManagement.Result result = UserDataManagement.MoveWorld(this.worldInfo, this.moveToStorageType);
			if (result == UserDataManagement.Result.Success && !this.moveToStorageType.UsesDataLimit())
			{
				bool flag = true;
				foreach (SaveInfoProvider.SaveEntryInfo saveEntryInfo in SaveInfoProvider.Instance.SaveEntryInfos)
				{
					if (saveEntryInfo.WorldEntry == this.worldInfo && saveEntryInfo.StorageType != this.moveToStorageType)
					{
						string saveGameDir = GameIO.GetSaveGameDir(this.worldInfo.Name, saveEntryInfo.Name, this.moveToStorageType);
						UserDataManagement.Result result2 = UserDataManagement.MoveSave(saveEntryInfo.SaveDir, saveGameDir);
						if (result2 == UserDataManagement.Result.Success)
						{
							UserDataManagement.SetSaveArchived(saveGameDir, false);
						}
						flag &= result2 == UserDataManagement.Result.Success;
					}
				}
				if (!flag)
				{
					result = UserDataManagement.Result.FailedToMoveSaves;
				}
			}
			PathAbstractions.WorldsSearchPaths.InvalidateCache();
			SaveInfoProvider.Instance.SetDirty();
			return result;
		}

		public readonly SaveInfoProvider.WorldEntryInfo worldInfo;

		public readonly UserDataStorageType moveToStorageType;
	}

	public struct GameSaveMove
	{
		public bool IsReady
		{
			get
			{
				return this.saveInfo != null;
			}
		}

		public bool WorldRequiresMoving
		{
			get
			{
				return this.worldInfo.ShouldBeMovedWithSave(this.moveToStorageType);
			}
		}

		public GameSaveMove(SaveInfoProvider.SaveEntryInfo saveInfo, UserDataStorageType moveToStorageType)
		{
			this = new UserDataManagement.GameSaveMove(saveInfo, saveInfo.Name, moveToStorageType);
		}

		public GameSaveMove(SaveInfoProvider.SaveEntryInfo saveInfo, string targetName, UserDataStorageType moveToStorageType)
		{
			if (saveInfo.WorldEntry.Type == SaveInfoProvider.RemoteWorldsType && targetName != saveInfo.Name)
			{
				throw new ArgumentException("Cannot rename remote world saves");
			}
			this.saveInfo = saveInfo;
			this.worldInfo = saveInfo.WorldEntry;
			this.targetName = targetName;
			this.moveToStorageType = moveToStorageType;
		}

		public UserDataManagement.SaveMoveResultInfo PerformMove()
		{
			UserDataManagement.SaveMoveResultInfo saveMoveResultInfo;
			if (this.WorldRequiresMoving)
			{
				UserDataManagement.Result result = UserDataManagement.MoveWorld(this.worldInfo, this.moveToStorageType);
				if (result != UserDataManagement.Result.Success)
				{
					saveMoveResultInfo = new UserDataManagement.SaveMoveResultInfo
					{
						Result = result
					};
					return saveMoveResultInfo;
				}
				PathAbstractions.WorldsSearchPaths.InvalidateCache();
				SaveInfoProvider.Instance.SetDirty();
			}
			string text = ((this.targetName != this.saveInfo.Name) ? GameIO.GetSaveGameDir(this.worldInfo.Name, this.targetName, this.moveToStorageType) : UserDataManagement.GetSavePathTarget(this.saveInfo, this.moveToStorageType));
			UserDataManagement.Result result2 = UserDataManagement.MoveSave(this.saveInfo.SaveDir, text);
			SaveInfoProvider.Instance.SetDirty();
			saveMoveResultInfo = new UserDataManagement.SaveMoveResultInfo
			{
				Result = result2,
				NewSaveDir = ((result2 == UserDataManagement.Result.Success) ? text : null)
			};
			return saveMoveResultInfo;
		}

		public readonly SaveInfoProvider.SaveEntryInfo saveInfo;

		public readonly SaveInfoProvider.WorldEntryInfo worldInfo;

		public readonly string targetName;

		public readonly UserDataStorageType moveToStorageType;
	}

	public struct GameSaveCopy
	{
		public bool IsReady
		{
			get
			{
				return this.saveInfo != null;
			}
		}

		public bool WorldRequiresMoving
		{
			get
			{
				return this.worldInfo.ShouldBeMovedWithSave(this.storageTarget);
			}
		}

		public GameSaveCopy(SaveInfoProvider.SaveEntryInfo saveInfo, string targetName, UserDataStorageType moveToStorageType)
		{
			this.saveInfo = saveInfo;
			if (saveInfo.WorldEntry.Type == SaveInfoProvider.RemoteWorldsType)
			{
				throw new Exception("Copy not permitted for remote world saves");
			}
			this.worldInfo = saveInfo.WorldEntry;
			this.targetName = targetName;
			this.storageTarget = moveToStorageType;
		}

		public UserDataManagement.SaveCopyResultInfo PerformCopy()
		{
			UserDataManagement.SaveCopyResultInfo saveCopyResultInfo;
			if (this.WorldRequiresMoving)
			{
				UserDataManagement.Result result = UserDataManagement.MoveWorld(this.worldInfo, this.storageTarget);
				if (result != UserDataManagement.Result.Success)
				{
					saveCopyResultInfo = new UserDataManagement.SaveCopyResultInfo
					{
						Result = result
					};
					return saveCopyResultInfo;
				}
				PathAbstractions.WorldsSearchPaths.InvalidateCache();
				SaveInfoProvider.Instance.SetDirty();
			}
			string saveGameDir = GameIO.GetSaveGameDir(this.worldInfo.Name, this.targetName, this.storageTarget);
			UserDataManagement.Result result2 = UserDataManagement.CopySave(this.saveInfo.SaveDir, saveGameDir);
			SaveInfoProvider.Instance.SetDirty();
			saveCopyResultInfo = new UserDataManagement.SaveCopyResultInfo
			{
				Result = result2,
				NewSaveDir = ((result2 == UserDataManagement.Result.Success) ? saveGameDir : null)
			};
			return saveCopyResultInfo;
		}

		public readonly SaveInfoProvider.SaveEntryInfo saveInfo;

		public readonly SaveInfoProvider.WorldEntryInfo worldInfo;

		public readonly string targetName;

		public readonly UserDataStorageType storageTarget;
	}
}
