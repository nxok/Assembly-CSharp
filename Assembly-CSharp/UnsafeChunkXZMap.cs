using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public struct UnsafeChunkXZMap<[IsUnmanaged] T> : IDisposable where T : struct, ValueType
{
	public bool IsCreated
	{
		get
		{
			return this.map != null;
		}
	}

	public UnsafeChunkXZMap(AllocatorManager.AllocatorHandle _allocator)
	{
		this.allocator = _allocator;
		this.map = AllocatorManager.Allocate<T>(_allocator, 256);
	}

	public unsafe T Get(int _x, int _z)
	{
		return this.map[(IntPtr)UnsafeChunkXZMap<T>.GetMapIndex(_x, _z) * (IntPtr)sizeof(T) / (IntPtr)sizeof(T)];
	}

	public unsafe void Set(int _x, int _z, T value)
	{
		this.map[(IntPtr)UnsafeChunkXZMap<T>.GetMapIndex(_x, _z) * (IntPtr)sizeof(T) / (IntPtr)sizeof(T)] = value;
	}

	public unsafe void Clear()
	{
		if (this.map != null)
		{
			UnsafeUtility.MemClear((void*)this.map, (long)(256 * UnsafeUtility.SizeOf<T>()));
		}
	}

	public void Dispose()
	{
		if (this.map != null)
		{
			AllocatorManager.Free<T>(this.allocator, this.map, 256);
		}
	}

	public int CalculateOwnedBytes()
	{
		int num = UnsafeUtility.SizeOf<UnsafeChunkXZMap<T>>();
		if (this.map != null)
		{
			num += CollectionHelper.Align(256 * UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>());
		}
		return num;
	}

	public static int GetMapIndex(int _x, int _z)
	{
		return _x + 16 * _z;
	}

	public static void GetMapCoords(int _index, out int _x, out int _z)
	{
		_z = _index / 16;
		_x = _index % 16;
	}

	public const int MAP_SIZE = 256;

	[NativeDisableUnsafePtrRestriction]
	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe T* map;

	[PublicizedFrom(EAccessModifier.Private)]
	public AllocatorManager.AllocatorHandle allocator;
}
