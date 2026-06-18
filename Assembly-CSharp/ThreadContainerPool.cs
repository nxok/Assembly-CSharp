using System;
using System.Collections.Generic;

public class ThreadContainerPool
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

	public ThreadContainerPool(int initialCapacity, int initialCount)
	{
		this._poolLock = new object();
		this.pool = new List<ThreadContainer>(initialCapacity);
		for (int i = 0; i < initialCount; i++)
		{
			this.pool.Add(new ThreadContainer());
		}
	}

	public ThreadContainer GetObject(DistantTerrain _TerExt, DistantChunk _DChunk, DistantChunkBasicMesh _BMesh, bool _WasReset)
	{
		object poolLock = this._poolLock;
		ThreadContainer threadContainer;
		lock (poolLock)
		{
			if (this.pool.Count == 0)
			{
				threadContainer = new ThreadContainer(_TerExt, _DChunk, _BMesh, _WasReset);
			}
			else
			{
				ThreadContainer threadContainer2 = this.pool[this.pool.Count - 1];
				this.pool.RemoveAt(this.pool.Count - 1);
				threadContainer2.Init(_TerExt, _DChunk, _BMesh, _WasReset);
				threadContainer = threadContainer2;
			}
		}
		return threadContainer;
	}

	public void ReturnObject(ThreadContainer item, bool IsClearItem)
	{
		if (item == null)
		{
			throw new ArgumentNullException("ThreadContainer is null");
		}
		object poolLock = this._poolLock;
		lock (poolLock)
		{
			if (this.pool.Contains(item))
			{
				throw new InvalidOperationException("ThreadContainer already in pool");
			}
			item.Clear(IsClearItem);
			this.pool.Add(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ThreadContainer> pool;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object _poolLock;
}
