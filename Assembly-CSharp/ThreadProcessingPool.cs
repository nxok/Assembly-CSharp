using System;
using System.Collections.Generic;

public class ThreadProcessingPool
{
	public int Count
	{
		get
		{
			object poolLock = this._poolLock;
			int count;
			lock (poolLock)
			{
				count = this.pool.Count;
			}
			return count;
		}
	}

	public ThreadProcessingPool(int initialCapacity, int initialCount)
	{
		this._poolLock = new object();
		this.pool = new List<ThreadProcessing>(initialCapacity);
		for (int i = 0; i < initialCount; i++)
		{
			this.pool.Add(new ThreadProcessing());
		}
	}

	public ThreadProcessing GetObject(List<ThreadInfoParam> _JobList)
	{
		object poolLock = this._poolLock;
		ThreadProcessing threadProcessing;
		lock (poolLock)
		{
			if (this.pool.Count == 0)
			{
				threadProcessing = new ThreadProcessing(_JobList);
			}
			else
			{
				ThreadProcessing threadProcessing2 = this.pool[this.pool.Count - 1];
				this.pool.RemoveAt(this.pool.Count - 1);
				threadProcessing2.Init(_JobList);
				threadProcessing = threadProcessing2;
			}
		}
		return threadProcessing;
	}

	public void ReturnObject(ThreadProcessing item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("ThreadProcessing is null");
		}
		object poolLock = this._poolLock;
		lock (poolLock)
		{
			if (this.pool.Contains(item))
			{
				throw new InvalidOperationException("ThreadProcessing already in pool");
			}
			this.pool.Add(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ThreadProcessing> pool;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object _poolLock;
}
