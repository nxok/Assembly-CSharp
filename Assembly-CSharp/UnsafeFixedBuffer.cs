using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public struct UnsafeFixedBuffer<[IsUnmanaged] T> : IDisposable where T : struct, ValueType
{
	public unsafe int Count
	{
		get
		{
			return this.data->count;
		}
	}

	public bool IsCreated
	{
		get
		{
			return this.data != null;
		}
	}

	public unsafe UnsafeFixedBuffer(int _capacity, AllocatorManager.AllocatorHandle _allocator)
	{
		this.data = AllocatorManager.Allocate<UnsafeFixedBuffer<T>.Data>(_allocator, 1);
		this.data->buffer = AllocatorManager.Allocate<T>(_allocator, _capacity);
		this.data->count = 0;
		this.capacity = _capacity;
		this.allocator = _allocator;
	}

	public unsafe void AddThreadSafe(T item)
	{
		int count;
		for (;;)
		{
			count = this.data->count;
			int num = this.data->count + 1;
			if (num > this.capacity)
			{
				break;
			}
			if (Interlocked.CompareExchange(ref this.data->count, num, count) == count)
			{
				goto Block_1;
			}
		}
		throw new IndexOutOfRangeException(string.Format("Index {0} is outside the UnsafeFixedBuffer capacity {1}", count, this.capacity));
		Block_1:
		UnsafeUtility.WriteArrayElement<T>((void*)this.data->buffer, count, item);
	}

	public unsafe void Clear()
	{
		this.data->count = 0;
	}

	public unsafe NativeArray<T> AsNativeArray()
	{
		return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>((void*)this.data->buffer, this.data->count, 0);
	}

	public unsafe void Dispose()
	{
		if (this.data != null)
		{
			AllocatorManager.Free<T>(this.allocator, this.data->buffer, this.capacity);
			AllocatorManager.Free<UnsafeFixedBuffer<T>.Data>(this.allocator, this.data, 1);
			this.data = null;
		}
	}

	public int CalculateOwnedBytes()
	{
		return UnsafeUtility.SizeOf<UnsafeFixedBuffer<T>>() + CollectionHelper.Align(this.capacity * UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>());
	}

	[NativeDisableUnsafePtrRestriction]
	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe UnsafeFixedBuffer<T>.Data* data;

	[PublicizedFrom(EAccessModifier.Private)]
	public int capacity;

	[PublicizedFrom(EAccessModifier.Private)]
	public AllocatorManager.AllocatorHandle allocator;

	[PublicizedFrom(EAccessModifier.Private)]
	public struct Data
	{
		public unsafe T* buffer;

		public int count;
	}
}
