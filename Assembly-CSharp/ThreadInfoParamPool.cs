using System;
using System.Collections.Generic;

public class ThreadInfoParamPool
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

	public int CountBig
	{
		get
		{
			object poolLock = this._poolLock;
			int count;
			lock (poolLock)
			{
				count = this.poolBig.Count;
			}
			return count;
		}
	}

	public ThreadInfoParamPool(int initialCapacity, int initialCapacityBig, int initialCount)
	{
		this._poolLock = new object();
		this.pool = new List<ThreadInfoParam>(initialCapacity);
		this.poolBig = new List<ThreadInfoParam>(initialCapacityBig);
		for (int i = 0; i < initialCount; i++)
		{
			this.pool.Add(new ThreadInfoParam());
			this.poolBig.Add(new ThreadInfoParam());
		}
	}

	public ThreadInfoParam GetObject(DistantChunkMap _CMap, int _ResLevel, int _OutId)
	{
		object poolLock = this._poolLock;
		ThreadInfoParam threadInfoParam;
		lock (poolLock)
		{
			if (this.pool.Count == 0)
			{
				threadInfoParam = new ThreadInfoParam(_CMap, _ResLevel, _OutId, false);
			}
			else
			{
				ThreadInfoParam threadInfoParam2 = this.pool[this.pool.Count - 1];
				this.pool.RemoveAt(this.pool.Count - 1);
				threadInfoParam2.Init(_CMap, _ResLevel, _OutId, false);
				threadInfoParam = threadInfoParam2;
			}
		}
		return threadInfoParam;
	}

	public ThreadInfoParam GetObjectBig(DistantChunkMap _CMap, int _ResLevel, int _OutId)
	{
		object poolLock = this._poolLock;
		ThreadInfoParam threadInfoParam;
		lock (poolLock)
		{
			if (this.poolBig.Count == 0)
			{
				threadInfoParam = new ThreadInfoParam(_CMap, _ResLevel, _OutId, true);
			}
			else
			{
				ThreadInfoParam threadInfoParam2 = this.poolBig[this.poolBig.Count - 1];
				this.poolBig.RemoveAt(this.poolBig.Count - 1);
				threadInfoParam2.Init(_CMap, _ResLevel, _OutId, true);
				threadInfoParam = threadInfoParam2;
			}
		}
		return threadInfoParam;
	}

	public void ReturnObject(ThreadInfoParam item, ThreadContainerPool TmpThContPool = null)
	{
		if (item == null)
		{
			throw new ArgumentNullException("ThreadInfoParam is null");
		}
		item.ClearAll(TmpThContPool);
		object poolLock = this._poolLock;
		lock (poolLock)
		{
			if (item.IsBigCapacity)
			{
				if (this.poolBig.Contains(item))
				{
					throw new InvalidOperationException("ThreadInfoParam Big already in pool");
				}
				this.poolBig.Add(item);
			}
			else
			{
				if (this.pool.Contains(item))
				{
					throw new InvalidOperationException("ThreadInfoParam already in pool");
				}
				this.pool.Add(item);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ThreadInfoParam> pool;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ThreadInfoParam> poolBig;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object _poolLock;
}
