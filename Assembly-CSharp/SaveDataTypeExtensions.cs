using System;

public static class SaveDataTypeExtensions
{
	public static bool IsRoot(this SaveDataType saveDataType)
	{
		return saveDataType == SaveDataType.User;
	}

	public static int GetSlotPathDepth(this SaveDataType saveDataType)
	{
		switch (saveDataType)
		{
		case SaveDataType.User:
			return 0;
		case SaveDataType.Saves:
			return 2;
		case SaveDataType.SavesLocal:
			return 1;
		case SaveDataType.GeneratedWorlds:
			return 1;
		default:
			Log.Error(string.Format("{0}.{1} does not have a slot path length, defaulting to '0'.", "SaveDataType", saveDataType));
			return 0;
		}
	}

	public static string GetPathRaw(this SaveDataType saveDataType)
	{
		string text;
		switch (saveDataType)
		{
		case SaveDataType.User:
			text = string.Empty;
			break;
		case SaveDataType.Saves:
			text = "Saves";
			break;
		case SaveDataType.SavesLocal:
			text = "SavesLocal";
			break;
		case SaveDataType.GeneratedWorlds:
			text = "GeneratedWorlds";
			break;
		default:
			throw new ArgumentOutOfRangeException("saveDataType", saveDataType, string.Format("No path specified for {0}.", saveDataType));
		}
		return text;
	}

	public static SaveDataManagedPath GetPath(this SaveDataType saveDataType)
	{
		SaveDataManagedPath saveDataManagedPath;
		switch (saveDataType)
		{
		case SaveDataType.User:
			saveDataManagedPath = SaveDataTypeExtensions.s_rootPathUser;
			break;
		case SaveDataType.Saves:
			saveDataManagedPath = SaveDataTypeExtensions.s_rootPathSaves;
			break;
		case SaveDataType.SavesLocal:
			saveDataManagedPath = SaveDataTypeExtensions.s_rootPathSavesLocal;
			break;
		case SaveDataType.GeneratedWorlds:
			saveDataManagedPath = SaveDataTypeExtensions.s_rootPathGeneratedWorlds;
			break;
		default:
			throw new ArgumentOutOfRangeException("saveDataType", saveDataType, string.Format("No relative path specified for {0}.", saveDataType));
		}
		return saveDataManagedPath;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SaveDataManagedPath s_rootPathUser = new SaveDataManagedPath(SaveDataType.User.GetPathRaw());

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SaveDataManagedPath s_rootPathSaves = new SaveDataManagedPath(SaveDataType.Saves.GetPathRaw());

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SaveDataManagedPath s_rootPathSavesLocal = new SaveDataManagedPath(SaveDataType.SavesLocal.GetPathRaw());

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SaveDataManagedPath s_rootPathGeneratedWorlds = new SaveDataManagedPath(SaveDataType.GeneratedWorlds.GetPathRaw());
}
