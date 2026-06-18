using System;
using System.Collections;
using System.Collections.Generic;

public abstract class DebugWrapper
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public DebugWrapper(DebugWrapper parent)
	{
		this.m_readCounter = ((parent != null) ? parent.m_readCounter : null) ?? new AtomicCounter();
		this.m_writeCounter = ((parent != null) ? parent.m_writeCounter : null) ?? new AtomicCounter();
		this.DebugNonMainThreadAccess = parent != null && parent.DebugNonMainThreadAccess;
		this.DebugConcurrentModifications = parent != null && parent.DebugConcurrentModifications;
	}

	public bool DebugNonMainThreadAccess { get; set; }

	public bool DebugConcurrentModifications { get; set; }

	public bool NeedsScope
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.DebugConcurrentModifications;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IDisposable DebugReadScope()
	{
		if (this.DebugNonMainThreadAccess && !ThreadManager.IsMainThread())
		{
			Log.Exception(new DebugWrapperException("A read is being issued outside of the main thread."));
		}
		if (!this.NeedsScope)
		{
			return DebugWrapper.DummyScope.Instance;
		}
		return new DebugWrapper.ReadScope(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IDisposable DebugReadWriteScope()
	{
		if (this.DebugNonMainThreadAccess && !ThreadManager.IsMainThread())
		{
			Log.Exception(new DebugWrapperException("A read/write is being issued outside of the main thread."));
		}
		if (!this.NeedsScope)
		{
			return DebugWrapper.DummyScope.Instance;
		}
		return new DebugWrapper.ReadWriteScope(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator<T> DebugEnumerator<T>(IEnumerator<T> enumerator)
	{
		return new DebugWrapper.Enumerator<T>(this.DebugReadScope(), enumerator);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator DebugEnumerator(IEnumerator enumerator)
	{
		return new DebugWrapper.Enumerator(this.DebugReadScope(), enumerator);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly AtomicCounter m_readCounter;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly AtomicCounter m_writeCounter;

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class DummyScope : IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public DummyScope()
		{
		}

		public void Dispose()
		{
		}

		public static readonly DebugWrapper.DummyScope Instance = new DebugWrapper.DummyScope();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class ReadScope : IDisposable
	{
		public ReadScope(DebugWrapper wrapper)
		{
			this.m_wrapper = wrapper;
			if (this.m_wrapper.DebugConcurrentModifications && this.m_wrapper.m_writeCounter.Value > 0)
			{
				Log.Exception(new DebugWrapperException("A read is being issued while there are active writers."));
				this.m_concurrentModificationNotified = true;
			}
			this.m_wrapper.m_readCounter.Increment();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ~ReadScope()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Dispose(bool disposing)
		{
			if (this.m_disposed)
			{
				return;
			}
			this.m_disposed = true;
			if (!disposing)
			{
				Log.Error("DebugWrapper.ReadScope was not disposed of correctly.");
			}
			this.m_wrapper.m_readCounter.Decrement();
			if (!this.m_concurrentModificationNotified && this.m_wrapper.DebugConcurrentModifications && this.m_wrapper.m_writeCounter.Value > 0)
			{
				Log.Exception(new DebugWrapperException("A read was issued while there were active writers."));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DebugWrapper m_wrapper;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool m_concurrentModificationNotified;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_disposed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class ReadWriteScope : IDisposable
	{
		public ReadWriteScope(DebugWrapper wrapper)
		{
			this.m_wrapper = wrapper;
			if (this.m_wrapper.DebugConcurrentModifications && this.m_wrapper.m_writeCounter.Value > 0)
			{
				if (this.m_wrapper.m_writeCounter.Value > 0)
				{
					Log.Exception(new DebugWrapperException("A read/write is being issued while there are active writers."));
				}
				else if (this.m_wrapper.m_readCounter.Value > 0)
				{
					Log.Exception(new DebugWrapperException("A read/write is being issued while there are active readers."));
				}
				this.m_concurrentModificationNotified = true;
			}
			this.m_wrapper.m_readCounter.Increment();
			this.m_wrapper.m_writeCounter.Increment();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ~ReadWriteScope()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Dispose(bool disposing)
		{
			if (this.m_disposed)
			{
				return;
			}
			this.m_disposed = true;
			if (!disposing)
			{
				Log.Error("DebugWrapper.ReadWriteScope was not disposed of correctly.");
			}
			this.m_wrapper.m_writeCounter.Decrement();
			this.m_wrapper.m_readCounter.Decrement();
			if (!this.m_concurrentModificationNotified && this.m_wrapper.DebugConcurrentModifications)
			{
				if (this.m_wrapper.m_writeCounter.Value > 0)
				{
					Log.Exception(new DebugWrapperException("A read/write was issued while there were active writers."));
					return;
				}
				if (this.m_wrapper.m_readCounter.Value > 0)
				{
					Log.Exception(new DebugWrapperException("A read/write was issued while there are active readers."));
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DebugWrapper m_wrapper;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool m_concurrentModificationNotified;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_disposed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class Enumerator : IEnumerator, IDisposable
	{
		public Enumerator(IDisposable scope, IEnumerator enumerator)
		{
			this.m_scope = scope;
			this.m_enumerator = enumerator;
		}

		public void Dispose()
		{
			this.m_scope.Dispose();
			IDisposable disposable = this.m_enumerator as IDisposable;
			if (disposable == null)
			{
				return;
			}
			disposable.Dispose();
		}

		public bool MoveNext()
		{
			return this.m_enumerator.MoveNext();
		}

		public void Reset()
		{
			this.m_enumerator.Reset();
		}

		public object Current
		{
			get
			{
				return this.m_enumerator.Current;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly IDisposable m_scope;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly IEnumerator m_enumerator;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class Enumerator<T> : IEnumerator<T>, IEnumerator, IDisposable
	{
		public Enumerator(IDisposable scope, IEnumerator<T> enumerator)
		{
			this.m_scope = scope;
			this.m_enumerator = enumerator;
		}

		public void Dispose()
		{
			this.m_scope.Dispose();
			this.m_enumerator.Dispose();
		}

		public bool MoveNext()
		{
			return this.m_enumerator.MoveNext();
		}

		public void Reset()
		{
			this.m_enumerator.Reset();
		}

		public T Current
		{
			get
			{
				return this.m_enumerator.Current;
			}
		}

		public object System.Collections.IEnumerator.Current
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return this.m_enumerator.Current;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly IDisposable m_scope;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly IEnumerator<T> m_enumerator;
	}
}
