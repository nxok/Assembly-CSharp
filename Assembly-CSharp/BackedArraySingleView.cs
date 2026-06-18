using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public sealed class BackedArraySingleView<[IsUnmanaged] T> : IBackedArrayView<T>, IDisposable where T : struct, ValueType
{
	[PublicizedFrom(EAccessModifier.Private)]
	public BackedArraySingleView<T>.View CreateView()
	{
		BackedArraySingleView<T>.View view = new BackedArraySingleView<T>.View(this);
		int num = Interlocked.Increment(ref this.m_viewCount);
		if (num > 16)
		{
			Log.Warning(string.Format("{0}<T> has opened a large amount of array views, this could indicate a memory issue. Count: {1}, View Length: {2}", "BackedArraySingleView", num, this.m_viewLength));
		}
		return view;
	}

	public BackedArraySingleView(IBackedArray<T> array, BackedArrayHandleMode mode, int viewLength = 0)
	{
		this.m_array = array;
		this.m_length = array.Length;
		this.m_mode = mode;
		if (!mode.CanRead())
		{
			throw new ArgumentException("Expected a readable mode.", "mode");
		}
		if (viewLength <= 0)
		{
			viewLength = BackedArraySingleView<T>.GetDefaultViewLength(array.Length);
		}
		this.m_viewLength = Math.Min(this.m_length, viewLength);
		this.m_views = new ThreadLocal<BackedArraySingleView<T>.View>(new Func<BackedArraySingleView<T>.View>(this.CreateView), true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Dispose(bool disposing)
	{
		if (!disposing)
		{
			Log.Error("BackedArraySingleView<T> is being finalized, it should be disposed properly.");
			return;
		}
		if (this.IsDisposed())
		{
			return;
		}
		foreach (BackedArraySingleView<T>.View view in this.m_views.Values)
		{
			view.Dispose();
		}
		this.m_views.Dispose();
		this.m_views = null;
		this.m_array = null;
	}

	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~BackedArraySingleView()
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
			throw new ObjectDisposedException("BackedArraySingleView has already been disposed.");
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
			throw new NotSupportedException("This BackedArraySingleView is not writable.");
		}
	}

	public int Length
	{
		get
		{
			return this.m_length;
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
			return this.m_views.Value[i];
		}
		set
		{
			this.m_views.Value[i] = value;
		}
	}

	public void Flush()
	{
		foreach (BackedArraySingleView<T>.View view in this.m_views.Values)
		{
			view.m_handle.Flush();
		}
	}

	public static int GetDefaultViewLength(int length)
	{
		return Mathf.NextPowerOfTwo(4 * (int)Math.Sqrt((double)length));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IBackedArray<T> m_array;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int m_length;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BackedArrayHandleMode m_mode;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int m_viewLength;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadLocal<BackedArraySingleView<T>.View> m_views;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_viewCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int VIEW_COUNT_WARNING_THRESHOLD = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	public class View : IDisposable
	{
		public View(BackedArraySingleView<T> backedArraySingleView)
		{
			this.m_backedArraySingleView = backedArraySingleView;
		}

		public void Cache(int offset)
		{
			if (this.m_start <= offset && offset < this.m_end)
			{
				return;
			}
			IBackedArray<T> array = this.m_backedArraySingleView.m_array;
			int length = this.m_backedArraySingleView.m_length;
			int viewLength = this.m_backedArraySingleView.m_viewLength;
			BackedArrayHandleMode mode = this.m_backedArraySingleView.m_mode;
			if (offset < 0 || offset >= length)
			{
				throw new IndexOutOfRangeException(string.Format("Expected index {0} to be in the range [0, {1}).", offset, length));
			}
			if (offset + viewLength > length)
			{
				offset = length - viewLength;
			}
			IBackedArrayHandle handle = this.m_handle;
			if (handle != null)
			{
				handle.Dispose();
			}
			IBackedArrayHandle backedArrayHandle;
			if (mode != BackedArrayHandleMode.ReadOnly)
			{
				if (mode != BackedArrayHandleMode.ReadWrite)
				{
					throw new ArgumentOutOfRangeException("mode", mode, string.Format("Unknown mode: {0}", mode));
				}
				backedArrayHandle = array.GetMemoryUnsafe(offset, viewLength, out this.m_ptr);
			}
			else
			{
				backedArrayHandle = array.GetReadOnlyMemoryUnsafe(offset, viewLength, out this.m_ptr);
			}
			this.m_handle = backedArrayHandle;
			this.m_start = offset;
			this.m_end = offset + viewLength;
		}

		public unsafe T this[int i]
		{
			get
			{
				this.Cache(i);
				return this.m_ptr[(IntPtr)(i - this.m_start) * (IntPtr)sizeof(T) / (IntPtr)sizeof(T)];
			}
			set
			{
				this.Cache(i);
				this.m_ptr[(IntPtr)(i - this.m_start) * (IntPtr)sizeof(T) / (IntPtr)sizeof(T)] = value;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ~View()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Dispose(bool isDisposing)
		{
			object disposeLock = this.m_disposeLock;
			lock (disposeLock)
			{
				if (!isDisposing)
				{
					Log.Warning("View<T> is being finalized, it should be disposed properly.");
				}
				if (this.m_handle != null)
				{
					this.m_handle.Dispose();
					this.m_handle = null;
					this.m_ptr = null;
					this.m_start = 0;
					this.m_end = 0;
					Interlocked.Decrement(ref this.m_backedArraySingleView.m_viewCount);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BackedArraySingleView<T> m_backedArraySingleView;

		public IBackedArrayHandle m_handle;

		public unsafe T* m_ptr;

		public int m_start;

		public int m_end;

		[PublicizedFrom(EAccessModifier.Private)]
		public object m_disposeLock = new object();
	}
}
