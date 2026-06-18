using System;

public static class SaveDataLimitExtensions
{
	public static bool IsSupported(this SaveDataLimitType saveDataLimitType)
	{
		return !PlatformOptimizations.LimitedSaveData || saveDataLimitType > SaveDataLimitType.Unlimited;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static long GetRegionSizeLimit(this SaveDataLimitType saveDataLimitType)
	{
		if (!saveDataLimitType.IsSupported())
		{
			throw new ArgumentException(string.Format("Unexpected usage of {0}.{1}.{2}() when not supported by the current device.", "SaveDataLimitType", saveDataLimitType, "GetRegionSizeLimit"), "saveDataLimitType");
		}
		long num;
		switch (saveDataLimitType)
		{
		case SaveDataLimitType.Unlimited:
			num = -1L;
			break;
		case SaveDataLimitType.Short:
			num = 33554432L;
			break;
		case SaveDataLimitType.Medium:
			num = 67108864L;
			break;
		case SaveDataLimitType.Long:
			num = 134217728L;
			break;
		case SaveDataLimitType.VeryLong:
			num = 268435456L;
			break;
		default:
			throw new ArgumentOutOfRangeException("saveDataLimitType", saveDataLimitType, null);
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool TryGetSaveDataLimitType(long regionLimit, out SaveDataLimitType saveDataLimitType)
	{
		if (regionLimit == -1L)
		{
			saveDataLimitType = SaveDataLimitType.Unlimited;
			return true;
		}
		foreach (SaveDataLimitType saveDataLimitType2 in EnumUtils.Values<SaveDataLimitType>())
		{
			if (saveDataLimitType2 != SaveDataLimitType.Unlimited && regionLimit / 1048576L == saveDataLimitType2.GetRegionSizeLimit() / 1048576L)
			{
				saveDataLimitType = saveDataLimitType2;
				return true;
			}
		}
		saveDataLimitType = SaveDataLimitType.Unlimited;
		return false;
	}

	public static long CalculateTotalSize(this SaveDataLimitType saveDataLimitType, Vector2i worldSize)
	{
		if (!saveDataLimitType.IsSupported())
		{
			throw new ArgumentException(string.Format("Unexpected usage of {0}.{1}.{2}() when not supported by the current device.", "SaveDataLimitType", saveDataLimitType, "CalculateTotalSize"), "saveDataLimitType");
		}
		long regionSizeLimit = saveDataLimitType.GetRegionSizeLimit();
		if (regionSizeLimit <= 0L)
		{
			return -1L;
		}
		long num = SaveDataLimitUtils.CalculatePlayerMapSize(worldSize);
		return 104857600L + num + regionSizeLimit;
	}

	public static bool TryCalculateSaveDataLimitType(long reservedBytes, Vector2i worldSize, out SaveDataLimitType saveDataLimitType)
	{
		long num = SaveDataLimitUtils.CalculatePlayerMapSize(worldSize);
		return SaveDataLimitExtensions.TryGetSaveDataLimitType(reservedBytes - 104857600L - num, out saveDataLimitType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const long MB = 1048576L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long FLAT_OVERHEAD = 104857600L;
}
