using System;
using Platform;
using UnityEngine;

public static class PlatformOptimizations
{
	public static int MaxWorldSizeHost
	{
		get
		{
			return LaunchPrefs.MaxWorldSizeHost.Value;
		}
	}

	public static bool EnforceMaxWorldSizeHost
	{
		get
		{
			return PlatformOptimizations.MaxWorldSizeHost >= 0;
		}
	}

	public static int MaxWorldSizeClient
	{
		get
		{
			return LaunchPrefs.MaxWorldSizeClient.Value;
		}
	}

	public static bool EnforceMaxWorldSizeClient
	{
		get
		{
			return PlatformOptimizations.MaxWorldSizeClient >= 0;
		}
	}

	public static XUiC_WorldGenerationWindow.PreviewQuality MaxRWGPreviewQuality
	{
		get
		{
			if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() && Submission.Enabled)
			{
				return XUiC_WorldGenerationWindow.PreviewQuality.High;
			}
			if (SystemInfo.systemMemorySize < 22000)
			{
				return XUiC_WorldGenerationWindow.PreviewQuality.High;
			}
			return EnumUtils.MaxValue<XUiC_WorldGenerationWindow.PreviewQuality>();
		}
	}

	public static void ConfigureGameObjectPoolForPlatform(GameObjectPool pool)
	{
		if (!(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			return;
		}
		pool.MaxPooledInstancesPerItem = 500;
		pool.MaxDestroysPerUpdate = int.MaxValue;
		pool.ShrinkThresholdHigh = new GameObjectPool.ShrinkThreshold(100, 10, 0f);
		pool.ShrinkThresholdMedium = new GameObjectPool.ShrinkThreshold(50, 2, 0.1f);
	}

	public static void Init()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const ulong kB = 1024UL;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ulong MB = 1048576UL;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ulong GB = 1073741824UL;

	[PublicizedFrom(EAccessModifier.Private)]
	public const DeviceFlag ENABLE_FILE_BACKED_ARRAYS_PLATFORMS = DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const DeviceFlag ENABLE_FILE_BACKED_BLOCK_PROPERTIES_PLATFORMS = DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const DeviceFlag ENABLE_FILE_BACKED_TERRAIN_TILES_PLATFORMS = DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const DeviceFlag ENABLE_FILE_BACKED_RADIATION_TILES_PLATFORMS = DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const DeviceFlag LOAD_HALF_RES_ASSETS = DeviceFlag.XBoxSeriesS;

	[PublicizedFrom(EAccessModifier.Private)]
	public const DeviceFlag RESTART_PROCESS_SUPPORTED = DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const DeviceFlag RESTART_AFTER_RWG = DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const DeviceFlag LIMITED_SAVE_DATA = DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const DeviceFlag GC_FORCE_UNMAP_ON_GCOLLECT = DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	public static readonly bool FileBackedArrays = (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent();

	public static readonly bool FileBackedBlockProperties = (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent();

	public static readonly bool FileBackedTerrainTiles = (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent();

	public static readonly bool FileBackedRadiationTiles = (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent();

	public static readonly bool LoadHalfResAssets = DeviceFlag.XBoxSeriesS.IsCurrent();

	public static readonly bool RestartProcessSupported = (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent();

	public static readonly bool RestartAfterRwg = (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent();

	public static readonly bool LimitedSaveData = (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent();

	public static readonly bool GCForceUnmapOnGCollect = (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent();

	public static readonly int DefaultMaxWorldSizeHost = ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() ? 8192 : (-1));

	public const int DefaultMaxWorldSizeClient = -1;

	public static readonly int DefaultGameCoreBlobOperationQueueMaxTotalSize = 268435456;
}
