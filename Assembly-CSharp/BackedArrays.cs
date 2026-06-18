using System;
using System.Runtime.CompilerServices;
using Platform;

public static class BackedArrays
{
	[PublicizedFrom(EAccessModifier.Private)]
	static BackedArrays()
	{
		Log.Out(string.Format("Initial {0} == {1}", "ENABLE_FILE_BACKED_ARRAYS", BackedArrays.ENABLE_FILE_BACKED_ARRAYS));
	}

	public static IBackedArray<T> Create<[IsUnmanaged] T>(int length) where T : struct, ValueType
	{
		if (BackedArrays.ENABLE_FILE_BACKED_ARRAYS && length > 0)
		{
			return new FileBackedArray<T>(length);
		}
		return new MemoryBackedArray<T>(length);
	}

	public static IBackedArrayView<T> CreateSingleView<[IsUnmanaged] T>(IBackedArray<T> array, BackedArrayHandleMode mode, int viewLength = 0) where T : struct, ValueType
	{
		MemoryBackedArray<T> memoryBackedArray = array as MemoryBackedArray<T>;
		if (memoryBackedArray != null)
		{
			return new MemoryBackedArray<T>.MemoryBackedArrayView(memoryBackedArray, mode);
		}
		return new BackedArraySingleView<T>(array, mode, viewLength);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const DeviceFlag ENABLE_FILE_BACKED_ARRAYS_PLATFORMS = DeviceFlag.XBoxSeriesS;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly bool ENABLE_FILE_BACKED_ARRAYS = PlatformOptimizations.FileBackedArrays;
}
