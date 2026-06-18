using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public struct UnsafeChunkData<[IsUnmanaged] T> : IDisposable where T : struct, ValueType, IEquatable<T>
{
	public bool IsCreated
	{
		get
		{
			return this.layers != null;
		}
	}

	public unsafe UnsafeChunkData(AllocatorManager.AllocatorHandle _allocator)
	{
		this.allocator = _allocator;
		this.layers = AllocatorManager.Allocate<UnsafeChunkData<T>.LayerData>(this.allocator, 64);
		UnsafeUtility.MemClear((void*)this.layers, (long)(UnsafeUtility.SizeOf<UnsafeChunkData<T>.LayerData>() * 64));
		this.sameValues = AllocatorManager.Allocate<T>(this.allocator, 64);
		UnsafeUtility.MemClear((void*)this.sameValues, (long)(UnsafeUtility.SizeOf<T>() * 64));
	}

	public unsafe T Get(int _x, int _y, int _z)
	{
		int num = _y / 4;
		UnsafeChunkData<T>.LayerData* ptr = this.layers + (IntPtr)num * (IntPtr)sizeof(UnsafeChunkData<T>.LayerData) / (IntPtr)sizeof(UnsafeChunkData<T>.LayerData);
		if (ptr->items == null)
		{
			return this.sameValues[(IntPtr)num * (IntPtr)sizeof(T) / (IntPtr)sizeof(T)];
		}
		int num2 = _x + 16 * _z;
		int num3 = _y % 4;
		int num4 = num2 + num3 * 256;
		return ptr->items[(IntPtr)num4 * (IntPtr)sizeof(T) / (IntPtr)sizeof(T)];
	}

	public unsafe T Get(int _chunkIndex)
	{
		int num = _chunkIndex / 256;
		int num2 = num / 4;
		UnsafeChunkData<T>.LayerData* ptr = this.layers + (IntPtr)num2 * (IntPtr)sizeof(UnsafeChunkData<T>.LayerData) / (IntPtr)sizeof(UnsafeChunkData<T>.LayerData);
		if (ptr->items == null)
		{
			return this.sameValues[(IntPtr)num2 * (IntPtr)sizeof(T) / (IntPtr)sizeof(T)];
		}
		int num3 = _chunkIndex % 256;
		int num4 = num % 4;
		int num5 = num3 + num4 * 256;
		return ptr->items[(IntPtr)num5 * (IntPtr)sizeof(T) / (IntPtr)sizeof(T)];
	}

	public void CheckSameValues()
	{
		for (int i = 0; i < 64; i++)
		{
			this.CheckSameValue(i);
		}
	}

	public unsafe void CheckSameValue(int _layerIndex)
	{
		UnsafeChunkData<T>.LayerData* ptr = this.layers + (IntPtr)_layerIndex * (IntPtr)sizeof(UnsafeChunkData<T>.LayerData) / (IntPtr)sizeof(UnsafeChunkData<T>.LayerData);
		T* items = ptr->items;
		if (items == null)
		{
			return;
		}
		if (items != null)
		{
			T t = *items;
			for (int i = 1; i < 1024; i++)
			{
				if (!t.Equals(items[(IntPtr)i * (IntPtr)sizeof(T) / (IntPtr)sizeof(T)]))
				{
					return;
				}
			}
			this.sameValues[(IntPtr)_layerIndex * (IntPtr)sizeof(T) / (IntPtr)sizeof(T)] = t;
			AllocatorManager.Free<T>(this.allocator, ptr->items, 1024);
			ptr->items = null;
		}
	}

	public unsafe void Set(int _chunkIndex, T _value)
	{
		int num = _chunkIndex / 256;
		int num2 = num / 4;
		UnsafeChunkData<T>.LayerData* ptr = this.layers + (IntPtr)num2 * (IntPtr)sizeof(UnsafeChunkData<T>.LayerData) / (IntPtr)sizeof(UnsafeChunkData<T>.LayerData);
		if (ptr->items == null)
		{
			T* ptr2 = AllocatorManager.Allocate<T>(this.allocator, 1024);
			T t = this.sameValues[(IntPtr)num2 * (IntPtr)sizeof(T) / (IntPtr)sizeof(T)];
			for (int i = 0; i < 1024; i++)
			{
				ptr2[(IntPtr)i * (IntPtr)sizeof(T) / (IntPtr)sizeof(T)] = t;
			}
			ptr->items = ptr2;
		}
		int num3 = _chunkIndex % 256;
		int num4 = num % 4;
		int num5 = num3 + num4 * 256;
		ptr->items[(IntPtr)num5 * (IntPtr)sizeof(T) / (IntPtr)sizeof(T)] = _value;
	}

	public unsafe void Clear()
	{
		if (this.layers != null)
		{
			for (int i = 0; i < 64; i++)
			{
				UnsafeChunkData<T>.LayerData* ptr = this.layers + (IntPtr)i * (IntPtr)sizeof(UnsafeChunkData<T>.LayerData) / (IntPtr)sizeof(UnsafeChunkData<T>.LayerData);
				if (ptr->items != null)
				{
					AllocatorManager.Free<T>(this.allocator, ptr->items, 1024);
					ptr->items = null;
				}
			}
		}
		if (this.sameValues != null)
		{
			UnsafeUtility.MemClear((void*)this.sameValues, (long)(UnsafeUtility.SizeOf<T>() * 64));
		}
	}

	public void Dispose()
	{
		this.Clear();
		if (this.layers != null)
		{
			AllocatorManager.Free<UnsafeChunkData<T>.LayerData>(this.allocator, this.layers, 64);
			this.layers = null;
		}
		if (this.sameValues != null)
		{
			AllocatorManager.Free<T>(this.allocator, this.sameValues, 64);
			this.sameValues = null;
		}
	}

	public unsafe int CalculateOwnedBytes()
	{
		int num = UnsafeUtility.SizeOf<UnsafeChunkData<T>>();
		if (this.layers != null)
		{
			num += CollectionHelper.Align(64 * UnsafeUtility.SizeOf<UnsafeChunkData<T>.LayerData>(), UnsafeUtility.AlignOf<UnsafeChunkData<T>.LayerData>());
			for (int i = 0; i < 64; i++)
			{
				if (this.layers[(IntPtr)i * (IntPtr)sizeof(UnsafeChunkData<T>.LayerData) / (IntPtr)sizeof(UnsafeChunkData<T>.LayerData)].items != null)
				{
					num += CollectionHelper.Align(1024 * UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>());
				}
			}
		}
		if (this.sameValues != null)
		{
			num += CollectionHelper.Align(64 * UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>());
		}
		return num;
	}

	public const int LAYER_SIZE = 1024;

	public const int NUM_LAYERS = 64;

	[NativeDisableUnsafePtrRestriction]
	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe UnsafeChunkData<T>.LayerData* layers;

	[NativeDisableUnsafePtrRestriction]
	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe T* sameValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public AllocatorManager.AllocatorHandle allocator;

	[PublicizedFrom(EAccessModifier.Private)]
	public struct LayerData
	{
		public unsafe T* items;
	}
}
