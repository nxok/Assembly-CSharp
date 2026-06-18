using System;
using System.Threading;

public class ThreadSafeSemantics : IThreadingSemantics
{
	public ThreadSafeSemantics()
	{
		this.monitor = this;
	}

	public ThreadSafeSemantics(object _monitor)
	{
		this.monitor = _monitor;
	}

	public void Synchronize(AtomicActionDelegate _delegate)
	{
		object obj = this.monitor;
		lock (obj)
		{
			_delegate();
		}
	}

	public int InterlockedAdd(ref int _number, int _add)
	{
		return Interlocked.Add(ref _number, _add);
	}

	public T Synchronize<T>(Func<T> _delegate)
	{
		object obj = this.monitor;
		T t;
		lock (obj)
		{
			t = _delegate();
		}
		return t;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public object monitor;
}
