using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public sealed class PinnedBuffer<[IsUnmanaged] T> : PinnedBuffer where T : struct, ValueType
{
	public PinnedBuffer(MemoryPooledArray<T> pool, int length)
	{
		if (length <= 0)
		{
			this.InitBuffer(Array.Empty<T>(), 0);
			return;
		}
		this.m_pool = pool;
		this.m_poolArray = this.m_pool.Alloc(length);
		this.InitBuffer(this.m_poolArray, length);
	}

	public PinnedBuffer()
	{
		this.InitBuffer(Array.Empty<T>(), 0);
	}

	public PinnedBuffer(int length)
	{
		if (length <= 0)
		{
			this.InitBuffer(Array.Empty<T>(), 0);
			return;
		}
		this.InitBuffer(new T[length], length);
	}

	public PinnedBuffer(T[] buffer, int length)
	{
		if (length <= 0)
		{
			this.InitBuffer(Array.Empty<T>(), 0);
			return;
		}
		this.InitBuffer(buffer, length);
	}

	public PinnedBuffer(MemoryPooledArray<T> pool, T[] poolArray, int length)
	{
		this.m_pool = pool;
		this.m_poolArray = poolArray;
		this.InitBuffer(poolArray, length);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe void InitBuffer(T[] buffer, int length)
	{
		this.m_ptr = (T*)UnsafeUtility.PinGCArrayAndGetDataAddress(buffer, ref this.m_gcHandle);
		this.m_length = length;
	}

	public override void Dispose()
	{
		this.m_length = 0;
		this.m_ptr = null;
		if (this.m_gcHandle > 0UL)
		{
			UnsafeUtility.ReleaseGCObject(this.m_gcHandle);
			this.m_gcHandle = 0UL;
		}
		if (this.m_pool != null)
		{
			this.m_pool.Free(this.m_poolArray);
			this.m_pool = null;
		}
		this.m_poolArray = null;
	}

	public unsafe T* Ptr
	{
		get
		{
			return this.m_ptr;
		}
	}

	public int Length
	{
		get
		{
			return this.m_length;
		}
	}

	public PinnedBufferRef<T> AsRef()
	{
		return new PinnedBufferRef<T>(this.m_ptr, this.m_length);
	}

	public static implicit operator PinnedBufferRef<T>(PinnedBuffer<T> buffer)
	{
		return buffer.AsRef();
	}

	public PinnedBufferRef<byte> AsBytes()
	{
		return this.AsRef().AsBytes();
	}

	public static implicit operator PinnedBufferRef<byte>(PinnedBuffer<T> buffer)
	{
		return buffer.AsBytes();
	}

	public unsafe Span<T> AsSpan()
	{
		return new Span<T>((void*)this.m_ptr, this.m_length);
	}

	public static implicit operator Span<T>(PinnedBuffer<T> buffer)
	{
		return buffer.AsSpan();
	}

	public unsafe ReadOnlySpan<T> AsReadOnlySpan()
	{
		return new ReadOnlySpan<T>((void*)this.m_ptr, this.m_length);
	}

	public static implicit operator ReadOnlySpan<T>(PinnedBuffer<T> buffer)
	{
		return buffer.AsReadOnlySpan();
	}

	public AtomicSafeHandleScope CreateNativeArray(out NativeArray<T> array)
	{
		return MeshDataUtils.CreateNativeArray<T>(this.m_ptr, this.m_length, out array);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryPooledArray<T> m_pool;

	[PublicizedFrom(EAccessModifier.Private)]
	public T[] m_poolArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe T* m_ptr;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_length;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong m_gcHandle;
}
