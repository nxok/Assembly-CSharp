using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public readonly struct PinnedBufferRef<[IsUnmanaged] T> where T : struct, ValueType
{
	public unsafe PinnedBufferRef(T* ptr, int length)
	{
		this.Ptr = ptr;
		this.Length = length;
	}

	public unsafe Span<T> AsSpan()
	{
		return new Span<T>((void*)this.Ptr, this.Length);
	}

	public static implicit operator Span<T>(PinnedBufferRef<T> buffer)
	{
		return buffer.AsSpan();
	}

	public unsafe ReadOnlySpan<T> AsReadOnlySpan()
	{
		return new ReadOnlySpan<T>((void*)this.Ptr, this.Length);
	}

	public static implicit operator ReadOnlySpan<T>(PinnedBufferRef<T> buffer)
	{
		return buffer.AsReadOnlySpan();
	}

	public unsafe PinnedBufferRef<byte> AsBytes()
	{
		return new PinnedBufferRef<byte>((byte*)this.Ptr, this.Length * sizeof(T));
	}

	public static implicit operator PinnedBufferRef<byte>(PinnedBufferRef<T> buffer)
	{
		return buffer.AsBytes();
	}

	public AtomicSafeHandleScope CreateNativeArray(out NativeArray<T> array)
	{
		return MeshDataUtils.CreateNativeArray<T>(this.Ptr, this.Length, out array);
	}

	[NativeDisableUnsafePtrRestriction]
	public unsafe readonly T* Ptr;

	public readonly int Length;
}
