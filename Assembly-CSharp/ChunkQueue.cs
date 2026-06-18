using System;
using ConcurrentCollections;

public class ChunkQueue
{
	public void Add(long item)
	{
		object @lock = this._lock;
		lock (@lock)
		{
			this.KeyQueue.Add(item);
		}
	}

	public void Clear()
	{
		this.KeyQueue.Clear();
	}

	public bool Contains(long item)
	{
		return this.KeyQueue.Contains(item);
	}

	public void Remove(long item)
	{
		this.KeyQueue.TryRemove(item);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public object _lock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public ConcurrentHashSet<long> KeyQueue = new ConcurrentHashSet<long>();
}
