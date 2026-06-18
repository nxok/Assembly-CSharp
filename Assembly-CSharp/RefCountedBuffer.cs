using System;

public sealed class RefCountedBuffer : IDisposable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public RefCountedBuffer(RefCountedBuffer.SharedBuffer buffer)
	{
		if (buffer == null || buffer.IsDisposed)
		{
			this.Dispose();
			return;
		}
		this.m_buffer = buffer;
		this.m_buffer.Increment();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Dispose(bool disposing)
	{
		if (!disposing)
		{
			Log.Error("RefCountedBuffer is being finalized. It should be disposed properly.");
			return;
		}
		if (this.m_buffer == null)
		{
			return;
		}
		this.m_buffer.Decrement();
		this.m_buffer = null;
	}

	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~RefCountedBuffer()
	{
		this.Dispose(false);
	}

	public bool IsDisposed
	{
		get
		{
			return this.m_buffer == null || this.m_buffer.IsDisposed;
		}
	}

	public Span<byte> Span
	{
		get
		{
			RefCountedBuffer.SharedBuffer buffer = this.m_buffer;
			if (((buffer != null) ? buffer.BufferRaw : null) != null)
			{
				return this.m_buffer.BufferRaw.AsSpan(this.m_buffer.Offset, this.m_buffer.Length);
			}
			return Span<byte>.Empty;
		}
	}

	public Memory<byte> Memory
	{
		get
		{
			RefCountedBuffer.SharedBuffer buffer = this.m_buffer;
			Memory<byte>? memory;
			if (buffer == null)
			{
				memory = null;
			}
			else
			{
				byte[] bufferRaw = buffer.BufferRaw;
				memory = ((bufferRaw != null) ? new Memory<byte>?(bufferRaw.AsMemory(this.m_buffer.Offset, this.m_buffer.Length)) : null);
			}
			Memory<byte>? memory2 = memory;
			if (memory2 == null)
			{
				return Memory<byte>.Empty;
			}
			return memory2.GetValueOrDefault();
		}
	}

	public byte[] BufferRaw
	{
		get
		{
			RefCountedBuffer.SharedBuffer buffer = this.m_buffer;
			return ((buffer != null) ? buffer.BufferRaw : null) ?? Array.Empty<byte>();
		}
	}

	public int Offset
	{
		get
		{
			RefCountedBuffer.SharedBuffer buffer = this.m_buffer;
			if (buffer == null)
			{
				return 0;
			}
			return buffer.Offset;
		}
	}

	public int Length
	{
		get
		{
			RefCountedBuffer.SharedBuffer buffer = this.m_buffer;
			if (buffer == null)
			{
				return 0;
			}
			return buffer.Length;
		}
	}

	public RefCountedBuffer CreateRef()
	{
		RefCountedBuffer.SharedBuffer buffer = this.m_buffer;
		if (buffer == null)
		{
			return null;
		}
		return buffer.CreateRef();
	}

	public static RefCountedBuffer CreateFromExisting(byte[] buffer)
	{
		return RefCountedBuffer.CreateFromExisting(buffer, 0, buffer.Length);
	}

	public static RefCountedBuffer CreateFromExisting(byte[] buffer, int offset, int length)
	{
		return new RefCountedBuffer.SharedBuffer(buffer, offset, length, false).CreateRef();
	}

	public static RefCountedBuffer CreatePooled(int length)
	{
		return new RefCountedBuffer.SharedBuffer(SaveBufferPool.Instance.Alloc(length), 0, length, true).CreateRef();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public RefCountedBuffer.SharedBuffer m_buffer;

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class SharedBuffer : IDisposable
	{
		public SharedBuffer(byte[] bufferRaw, int offset, int length, bool pooled)
		{
			this.m_bufferRaw = bufferRaw;
			this.m_offset = offset;
			this.m_length = length;
			this.m_pooled = pooled;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Dispose(bool disposing)
		{
			if (!disposing)
			{
				Log.Error("RefCountedBuffer.SharedBuffer is being finalized. It should be disposed of properly by making use of RefCountedBuffer.");
				return;
			}
			object lockObj = this.m_lockObj;
			lock (lockObj)
			{
				if (this.m_bufferRaw != null)
				{
					if (this.m_refs != 0)
					{
						Log.Error(string.Format("{0} expected {1} to be 0 when disposing but was {2}.", "RefCountedBuffer", "m_refs", this.m_refs));
					}
					if (this.m_pooled)
					{
						SaveBufferPool.Instance.Free(this.m_bufferRaw);
					}
					this.m_bufferRaw = null;
					this.m_offset = 0;
					this.m_length = 0;
				}
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ~SharedBuffer()
		{
			this.Dispose(false);
		}

		public bool IsDisposed
		{
			get
			{
				return this.m_bufferRaw == null;
			}
		}

		public void Increment()
		{
			object lockObj = this.m_lockObj;
			lock (lockObj)
			{
				if (this.m_bufferRaw == null)
				{
					throw new ObjectDisposedException("Shared Buffer has already been disposed.");
				}
				this.m_refs++;
			}
		}

		public void Decrement()
		{
			object lockObj = this.m_lockObj;
			lock (lockObj)
			{
				if (this.m_bufferRaw == null)
				{
					throw new ObjectDisposedException("Shared Buffer has already been disposed.");
				}
				this.m_refs--;
				if (this.m_refs <= 0)
				{
					this.Dispose();
				}
			}
		}

		public byte[] BufferRaw
		{
			get
			{
				return this.m_bufferRaw;
			}
		}

		public int Offset
		{
			get
			{
				return this.m_offset;
			}
		}

		public int Length
		{
			get
			{
				return this.m_length;
			}
		}

		public RefCountedBuffer CreateRef()
		{
			return new RefCountedBuffer(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly object m_lockObj = new object();

		[PublicizedFrom(EAccessModifier.Private)]
		public byte[] m_bufferRaw;

		[PublicizedFrom(EAccessModifier.Private)]
		public int m_offset;

		[PublicizedFrom(EAccessModifier.Private)]
		public int m_length;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool m_pooled;

		[PublicizedFrom(EAccessModifier.Private)]
		public int m_refs;
	}
}
