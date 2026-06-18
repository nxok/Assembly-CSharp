using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class MemoryBackedArray<[IsUnmanaged] T> : IBackedArray<T>, IDisposable where T : struct, ValueType
{
	public MemoryBackedArray(int length)
	{
		this.m_array = new T[length];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Dispose(bool disposing)
	{
		if (!disposing)
		{
			Log.Error("MemoryBackedArray<T> is being finalized, it should be disposed properly.");
			return;
		}
		this.m_array = null;
	}

	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~MemoryBackedArray()
	{
		this.Dispose(false);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ThrowIfDisposed()
	{
		if (this.m_array == null)
		{
			throw new ObjectDisposedException("MemoryBackedArray has already been disposed.");
		}
	}

	public int Length
	{
		get
		{
			return this.m_array.Length;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static MemoryBackedArray<T>.MemoryBackedArrayHandle GetStaticHandle(BackedArrayHandleMode mode)
	{
		MemoryBackedArray<T>.MemoryBackedArrayHandle memoryBackedArrayHandle;
		if (mode != BackedArrayHandleMode.ReadOnly)
		{
			if (mode != BackedArrayHandleMode.ReadWrite)
			{
				throw new ArgumentOutOfRangeException("mode", mode, string.Format("Unknown mode: {0}", mode));
			}
			memoryBackedArrayHandle = MemoryBackedArray<T>.s_handleReadWrite;
		}
		else
		{
			memoryBackedArrayHandle = MemoryBackedArray<T>.s_handleReadOnly;
		}
		return memoryBackedArrayHandle;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IBackedArrayHandle GetMemoryInternal(int start, int length, out Memory<T> memory, BackedArrayHandleMode mode)
	{
		memory = this.m_array.AsMemory(start, length);
		return MemoryBackedArray<T>.GetStaticHandle(mode);
	}

	public IBackedArrayHandle GetMemory(int start, int length, out Memory<T> memory)
	{
		return this.GetMemoryInternal(start, length, out memory, BackedArrayHandleMode.ReadWrite);
	}

	public IBackedArrayHandle GetReadOnlyMemory(int start, int length, out ReadOnlyMemory<T> memory)
	{
		Memory<T> memory2;
		IBackedArrayHandle memoryInternal = this.GetMemoryInternal(start, length, out memory2, BackedArrayHandleMode.ReadOnly);
		memory = memory2;
		return memoryInternal;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe IBackedArrayHandle GetMemoryUnsafeInternal(int start, int length, out T* arrayPtr, BackedArrayHandleMode mode)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException(string.Format("Expected length to be non-negative but was {0}.", length));
		}
		if (start < 0 || start + length > this.m_array.Length)
		{
			throw new ArgumentOutOfRangeException(string.Format("Expected requested memory range [{0}, {1}) to be a subset of [0, {2}).", start, start + length, this.m_array.Length));
		}
		GCHandle gchandle = GCHandle.Alloc(this.m_array, GCHandleType.Pinned);
		arrayPtr = (void*)gchandle.AddrOfPinnedObject();
		arrayPtr += (IntPtr)start * (IntPtr)sizeof(T);
		return new MemoryBackedArray<T>.MemoryBackedArrayUnsafeHandle(gchandle, mode);
	}

	public unsafe IBackedArrayHandle GetMemoryUnsafe(int start, int length, out T* arrayPtr)
	{
		return this.GetMemoryUnsafeInternal(start, length, out arrayPtr, BackedArrayHandleMode.ReadWrite);
	}

	public unsafe IBackedArrayHandle GetReadOnlyMemoryUnsafe(int start, int length, out T* arrayPtr)
	{
		return this.GetMemoryUnsafeInternal(start, length, out arrayPtr, BackedArrayHandleMode.ReadOnly);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IBackedArrayHandle GetSpanInternal(int start, int length, out Span<T> span, BackedArrayHandleMode mode)
	{
		span = this.m_array.AsSpan(start, length);
		return MemoryBackedArray<T>.GetStaticHandle(mode);
	}

	public IBackedArrayHandle GetSpan(int start, int length, out Span<T> span)
	{
		return this.GetSpanInternal(start, length, out span, BackedArrayHandleMode.ReadWrite);
	}

	public IBackedArrayHandle GetReadOnlySpan(int start, int length, out ReadOnlySpan<T> span)
	{
		Span<T> span2;
		IBackedArrayHandle spanInternal = this.GetSpanInternal(start, length, out span2, BackedArrayHandleMode.ReadOnly);
		span = span2;
		return spanInternal;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly MemoryBackedArray<T>.MemoryBackedArrayHandle s_handleReadWrite = new MemoryBackedArray<T>.MemoryBackedArrayHandle(BackedArrayHandleMode.ReadWrite);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly MemoryBackedArray<T>.MemoryBackedArrayHandle s_handleReadOnly = new MemoryBackedArray<T>.MemoryBackedArrayHandle(BackedArrayHandleMode.ReadOnly);

	[PublicizedFrom(EAccessModifier.Private)]
	public T[] m_array;

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class MemoryBackedArrayHandle : IBackedArrayHandle, IDisposable
	{
		public MemoryBackedArrayHandle(BackedArrayHandleMode mode)
		{
			this.m_mode = mode;
		}

		public void Dispose()
		{
		}

		public BackedArrayHandleMode Mode
		{
			get
			{
				return this.m_mode;
			}
		}

		public void Flush()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BackedArrayHandleMode m_mode;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class MemoryBackedArrayUnsafeHandle : IBackedArrayHandle, IDisposable
	{
		public MemoryBackedArrayUnsafeHandle(GCHandle gcHandle, BackedArrayHandleMode mode)
		{
			this.m_gcHandle = gcHandle;
			this.m_mode = mode;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Dispose(bool disposing)
		{
			if (!disposing)
			{
				Log.Error("MemoryBackedArrayHandle is being finalized, it should be disposed properly.");
				return;
			}
			if (this.m_gcHandle != default(GCHandle))
			{
				this.m_gcHandle.Free();
				this.m_gcHandle = default(GCHandle);
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ~MemoryBackedArrayUnsafeHandle()
		{
			this.Dispose(false);
		}

		[Conditional("DEVELOPMENT_BUILD")]
		[Conditional("UNITY_EDITOR")]
		[PublicizedFrom(EAccessModifier.Private)]
		public void ThrowIfDisposed()
		{
			if (this.IsDisposed())
			{
				throw new ObjectDisposedException("MemoryBackedArrayUnsafeHandle has already been disposed.");
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool IsDisposed()
		{
			return this.m_gcHandle == default(GCHandle);
		}

		[Conditional("DEVELOPMENT_BUILD")]
		[Conditional("UNITY_EDITOR")]
		[PublicizedFrom(EAccessModifier.Private)]
		public void ThrowIfCannotWrite()
		{
			if (!this.m_mode.CanWrite())
			{
				throw new NotSupportedException("This MemoryBackedArrayUnsafeHandle is not writable.");
			}
		}

		public BackedArrayHandleMode Mode
		{
			get
			{
				return this.m_mode;
			}
		}

		public void Flush()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public GCHandle m_gcHandle;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BackedArrayHandleMode m_mode;
	}

	public sealed class MemoryBackedArrayView : IBackedArrayView<T>, IDisposable
	{
		public MemoryBackedArrayView(MemoryBackedArray<T> array, BackedArrayHandleMode mode)
		{
			this.m_array = array.m_array;
			this.m_mode = mode;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Dispose(bool disposing)
		{
			if (!disposing)
			{
				Log.Error("MemoryBackedArrayView is being finalized, it should be disposed properly.");
				return;
			}
			this.m_array = null;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ~MemoryBackedArrayView()
		{
			this.Dispose(false);
		}

		[Conditional("DEVELOPMENT_BUILD")]
		[Conditional("UNITY_EDITOR")]
		[PublicizedFrom(EAccessModifier.Private)]
		public void ThrowIfDisposed()
		{
			if (this.IsDisposed())
			{
				throw new ObjectDisposedException("MemoryBackedArrayView has already been disposed.");
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool IsDisposed()
		{
			return this.m_array == null;
		}

		[Conditional("DEVELOPMENT_BUILD")]
		[Conditional("UNITY_EDITOR")]
		[PublicizedFrom(EAccessModifier.Private)]
		public void ThrowIfCannotWrite()
		{
			if (!this.m_mode.CanWrite())
			{
				throw new NotSupportedException("This MemoryBackedArrayView is not writable.");
			}
		}

		public int Length
		{
			get
			{
				return this.m_array.Length;
			}
		}

		public BackedArrayHandleMode Mode
		{
			get
			{
				return this.m_mode;
			}
		}

		public T this[int i]
		{
			get
			{
				return this.m_array[i];
			}
			set
			{
				this.m_array[i] = value;
			}
		}

		public void Flush()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public T[] m_array;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BackedArrayHandleMode m_mode;
	}
}
