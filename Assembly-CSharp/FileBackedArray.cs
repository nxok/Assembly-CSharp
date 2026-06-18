using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Platform;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public sealed class FileBackedArray<[IsUnmanaged] T> : IBackedArray<T>, IDisposable where T : struct, ValueType
{
	public FileBackedArray(int length)
	{
		if (length <= 0)
		{
			throw new ArgumentOutOfRangeException("length", length, "Length should be positive.");
		}
		this.m_length = length;
		this.m_valueSize = UnsafeUtility.SizeOf(typeof(T));
		this.m_filePath = PlatformManager.NativePlatform.Utils.GetTempFileName("fba", ".fba");
		this.m_fileStream = new FileStream(this.m_filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.DeleteOnClose);
		this.m_fileStream.Seek((long)(length * this.m_valueSize - 1), SeekOrigin.Begin);
		this.m_fileStream.WriteByte(0);
		this.m_fileStream.Flush();
		this.m_fileStreams = new ThreadLocal<FileStream>(new Func<FileStream>(this.CreateFileStream), true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Dispose(bool disposing)
	{
		if (!disposing)
		{
			Log.Error("FileBackedArray<T> is being finalized, it should be disposed properly.");
			return;
		}
		object fileStreamsLock = this.m_fileStreamsLock;
		lock (fileStreamsLock)
		{
			if (this.m_fileStreams != null)
			{
				foreach (FileStream fileStream in this.m_fileStreams.Values)
				{
					fileStream.Dispose();
				}
				this.m_fileStreams.Dispose();
				this.m_fileStreams = null;
			}
			if (this.m_fileStream != null)
			{
				this.m_fileStream.Dispose();
				this.m_fileStream = null;
			}
		}
		try
		{
			File.Delete(this.m_filePath);
		}
		catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
		{
			Log.Warning("FileBackedArray<T> Failed to delete: " + this.m_filePath);
		}
	}

	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~FileBackedArray()
	{
		this.Dispose(false);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ThrowIfDisposed()
	{
		if (this.m_fileStream == null)
		{
			throw new ObjectDisposedException("FileBackedArray has already been disposed.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckBounds(int start, int length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException(string.Format("Expected length to be non-negative but was {0}.", length));
		}
		if (start < 0 || start + length > this.m_length)
		{
			throw new ArgumentOutOfRangeException(string.Format("Expected requested range [{0}, {1}) to be a subset of [0, {2}).", start, start + length, this.m_length));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FileStream CreateFileStream()
	{
		object fileStreamsLock = this.m_fileStreamsLock;
		FileStream fileStream;
		lock (fileStreamsLock)
		{
			fileStream = new FileStream(this.m_filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read | FileShare.Write | FileShare.Delete, 4096);
		}
		return fileStream;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FileStream GetFileStream()
	{
		object fileStreamsLock = this.m_fileStreamsLock;
		FileStream value;
		lock (fileStreamsLock)
		{
			value = this.m_fileStreams.Value;
		}
		return value;
	}

	public int Length
	{
		get
		{
			return this.m_length;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FileBackedArray<T>.FileBackedArrayHandle GetHandle(int start, int length, BackedArrayHandleMode mode)
	{
		this.CheckBounds(start, length);
		return new FileBackedArray<T>.FileBackedArrayHandle(this, this.m_valueSize, start, length, mode);
	}

	public IBackedArrayHandle GetMemory(int start, int length, out Memory<T> memory)
	{
		FileBackedArray<T>.FileBackedArrayHandle handle = this.GetHandle(start, length, BackedArrayHandleMode.ReadWrite);
		memory = handle.GetMemory();
		return handle;
	}

	public IBackedArrayHandle GetReadOnlyMemory(int start, int length, out ReadOnlyMemory<T> memory)
	{
		FileBackedArray<T>.FileBackedArrayHandle handle = this.GetHandle(start, length, BackedArrayHandleMode.ReadOnly);
		memory = handle.GetReadOnlyMemory();
		return handle;
	}

	public unsafe IBackedArrayHandle GetMemoryUnsafe(int start, int length, out T* arrayPtr)
	{
		FileBackedArray<T>.FileBackedArrayHandle handle = this.GetHandle(start, length, BackedArrayHandleMode.ReadWrite);
		arrayPtr = handle.GetPtr();
		return handle;
	}

	public unsafe IBackedArrayHandle GetReadOnlyMemoryUnsafe(int start, int length, out T* arrayPtr)
	{
		FileBackedArray<T>.FileBackedArrayHandle handle = this.GetHandle(start, length, BackedArrayHandleMode.ReadOnly);
		arrayPtr = handle.GetPtr();
		return handle;
	}

	public IBackedArrayHandle GetSpan(int start, int length, out Span<T> span)
	{
		FileBackedArray<T>.FileBackedArrayHandle handle = this.GetHandle(start, length, BackedArrayHandleMode.ReadWrite);
		span = handle.GetSpan();
		return handle;
	}

	public IBackedArrayHandle GetReadOnlySpan(int start, int length, out ReadOnlySpan<T> span)
	{
		FileBackedArray<T>.FileBackedArrayHandle handle = this.GetHandle(start, length, BackedArrayHandleMode.ReadOnly);
		span = handle.GetReadOnlySpan();
		return handle;
	}

	public FileBackedArray<T>.OnWrittenHandler OnWritten
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int FILE_STREAM_BUFFER_SIZE = 4096;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int m_length;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int m_valueSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string m_filePath;

	[PublicizedFrom(EAccessModifier.Private)]
	public FileStream m_fileStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_fileStreamsLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadLocal<FileStream> m_fileStreams;

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate void OnWrittenHandler(int start, ReadOnlySpan<T> span);

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class FileBackedArrayMemoryManager : MemoryManager<T>
	{
		public unsafe FileBackedArrayMemoryManager(T* ptr, int length, int valueSize)
		{
			this.m_ptr = ptr;
			this.m_length = length;
			this.m_valueSize = valueSize;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void Dispose(bool disposing)
		{
			this.m_ptr = null;
			this.m_length = 0;
			this.m_valueSize = 0;
		}

		public unsafe override Span<T> GetSpan()
		{
			return new Span<T>((void*)this.m_ptr, this.m_length);
		}

		public unsafe override MemoryHandle Pin(int elementIndex = 0)
		{
			return new MemoryHandle((void*)(this.m_ptr + (IntPtr)(elementIndex * this.m_valueSize) * (IntPtr)sizeof(T) / (IntPtr)sizeof(T)), default(GCHandle), null);
		}

		public override void Unpin()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe T* m_ptr;

		[PublicizedFrom(EAccessModifier.Private)]
		public int m_length;

		[PublicizedFrom(EAccessModifier.Private)]
		public int m_valueSize;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class FileBackedArrayHandle : IBackedArrayHandle, IDisposable
	{
		public unsafe FileBackedArrayHandle(FileBackedArray<T> array, int valueSize, int start, int length, BackedArrayHandleMode mode)
		{
			this.m_array = array;
			this.m_valueSize = valueSize;
			this.m_start = start;
			this.m_length = length;
			this.m_mode = mode;
			this.m_buffer = new NativeArray<T>(this.m_length, 4, 1);
			BackedArrayHandleMode mode2 = this.m_mode;
			void* ptr;
			if (mode2 != BackedArrayHandleMode.ReadOnly)
			{
				if (mode2 != BackedArrayHandleMode.ReadWrite)
				{
					throw new ArgumentOutOfRangeException("mode", mode, string.Format("Unknown mode: {0}", mode));
				}
				ptr = NativeArrayUnsafeUtility.GetUnsafePtr<T>(this.m_buffer);
			}
			else
			{
				ptr = NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr<T>(this.m_buffer);
			}
			this.m_ptr = (T*)ptr;
			byte* ptr2 = (byte*)this.m_ptr;
			int num = this.m_length * this.m_valueSize;
			int i = 0;
			int num2 = this.m_start * this.m_valueSize;
			FileStream fileStream = this.m_array.GetFileStream();
			fileStream.Seek((long)num2, SeekOrigin.Begin);
			while (i < num)
			{
				int num3 = fileStream.Read(new Span<byte>((void*)(ptr2 + i), num - i));
				if (num3 <= 0)
				{
					this.m_buffer.Dispose();
					this.m_buffer = default(NativeArray<T>);
					throw new IOException(string.Format("Unexpected end of file (read {0} but expected {1} after offset {2}).", i, num, num2));
				}
				i += num3;
			}
			FileBackedArray<T> array2 = this.m_array;
			array2.OnWritten = (FileBackedArray<T>.OnWrittenHandler)Delegate.Combine(array2.OnWritten, new FileBackedArray<T>.OnWrittenHandler(this.OnWritten));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Dispose(bool disposing)
		{
			if (!disposing)
			{
				Log.Error("FileBackedArrayHandle is being finalized, it should be disposed properly.");
				return;
			}
			if (this.m_array != null)
			{
				FileBackedArray<T> array = this.m_array;
				array.OnWritten = (FileBackedArray<T>.OnWrittenHandler)Delegate.Remove(array.OnWritten, new FileBackedArray<T>.OnWrittenHandler(this.OnWritten));
				if (this.m_mode.CanWrite())
				{
					try
					{
						this.FlushInternal();
					}
					catch (Exception ex)
					{
						Log.Error("Failed to write potential changes back to the FileBackedArray file stream.");
						Log.Exception(ex);
					}
				}
			}
			if (this.m_memoryOwner != null)
			{
				this.m_memoryOwner.Dispose();
				this.m_memoryOwner = null;
			}
			if (this.m_buffer != default(NativeArray<T>))
			{
				this.m_buffer.Dispose();
				this.m_buffer = default(NativeArray<T>);
			}
			this.m_start = 0;
			this.m_length = 0;
			this.m_ptr = null;
			this.m_array = null;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ~FileBackedArrayHandle()
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
				throw new ObjectDisposedException("FileBackedArrayHandle has already been disposed.");
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
				throw new NotSupportedException("This FileBackedArrayHandle is not writable.");
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnWritten(int start, ReadOnlySpan<T> span)
		{
			int num = Math.Max(start, this.m_start);
			int num2 = Math.Min(start + span.Length, this.m_start + this.m_length) - num;
			if (num2 <= 0)
			{
				return;
			}
			ReadOnlySpan<T> readOnlySpan = span.Slice(num - start, num2);
			Span<T> span2 = this.GetSpan().Slice(num - this.m_start, num2);
			readOnlySpan.CopyTo(span2);
		}

		public BackedArrayHandleMode Mode
		{
			get
			{
				return this.m_mode;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe void FlushInternal()
		{
			FileStream fileStream = this.m_array.GetFileStream();
			ReadOnlySpan<T> readOnlySpan = new ReadOnlySpan<T>((void*)this.m_ptr, this.m_length);
			fileStream.Seek((long)(this.m_start * this.m_valueSize), SeekOrigin.Begin);
			fileStream.Write(MemoryMarshal.Cast<T, byte>(readOnlySpan));
			fileStream.Flush();
			FileBackedArray<T>.OnWrittenHandler onWritten = this.m_array.OnWritten;
			if (onWritten == null)
			{
				return;
			}
			onWritten(this.m_start, readOnlySpan);
		}

		public void Flush()
		{
			this.FlushInternal();
		}

		public Memory<T> GetMemory()
		{
			if (this.m_memoryOwner == null)
			{
				this.m_memoryOwner = new FileBackedArray<T>.FileBackedArrayMemoryManager(this.m_ptr, this.m_length, this.m_valueSize);
			}
			return this.m_memoryOwner.Memory;
		}

		public ReadOnlyMemory<T> GetReadOnlyMemory()
		{
			if (this.m_memoryOwner == null)
			{
				this.m_memoryOwner = new FileBackedArray<T>.FileBackedArrayMemoryManager(this.m_ptr, this.m_length, this.m_valueSize);
			}
			return this.m_memoryOwner.Memory;
		}

		public unsafe T* GetPtr()
		{
			return this.m_ptr;
		}

		public unsafe Span<T> GetSpan()
		{
			return new Span<T>((void*)this.m_ptr, this.m_length);
		}

		public unsafe ReadOnlySpan<T> GetReadOnlySpan()
		{
			return new ReadOnlySpan<T>((void*)this.m_ptr, this.m_length);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public FileBackedArray<T> m_array;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int m_valueSize;

		[PublicizedFrom(EAccessModifier.Private)]
		public int m_start;

		[PublicizedFrom(EAccessModifier.Private)]
		public int m_length;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BackedArrayHandleMode m_mode;

		[PublicizedFrom(EAccessModifier.Private)]
		public NativeArray<T> m_buffer;

		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe T* m_ptr;

		[PublicizedFrom(EAccessModifier.Private)]
		public IMemoryOwner<T> m_memoryOwner;
	}
}
