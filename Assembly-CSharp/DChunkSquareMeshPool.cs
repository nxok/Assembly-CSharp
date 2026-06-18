using System;
using System.Collections.Generic;

public class DChunkSquareMeshPool
{
	public int Count
	{
		get
		{
			object poolLock = this._poolLock;
			int num2;
			lock (poolLock)
			{
				int num = 0;
				for (int i = 0; i < this.pool.Length; i++)
				{
					num += this.pool[i].Count;
				}
				num2 = num;
			}
			return num2;
		}
	}

	public DChunkSquareMeshPool(int initialCapacity, int NbLODLevel)
	{
		this._poolLock = new object();
		this.pool = new List<DChunkSquareMesh>[NbLODLevel];
		for (int i = 0; i < NbLODLevel; i++)
		{
			this.pool[i] = new List<DChunkSquareMesh>(initialCapacity);
		}
	}

	public DChunkSquareMesh GetObject(DistantChunkMap DCMap, int LODLevel)
	{
		object poolLock = this._poolLock;
		DChunkSquareMesh dchunkSquareMesh;
		lock (poolLock)
		{
			List<DChunkSquareMesh> list = this.pool[LODLevel];
			if (list.Count == 0)
			{
				dchunkSquareMesh = new DChunkSquareMesh(DCMap, LODLevel);
			}
			else
			{
				DChunkSquareMesh dchunkSquareMesh2 = list[list.Count - 1];
				list.RemoveAt(list.Count - 1);
				dchunkSquareMesh = dchunkSquareMesh2;
			}
		}
		return dchunkSquareMesh;
	}

	public void ReturnObject(DChunkSquareMesh item, int LODLevel)
	{
		if (item == null)
		{
			throw new ArgumentNullException("DChunkSquareMesh is null");
		}
		object poolLock = this._poolLock;
		lock (poolLock)
		{
			if (this.pool[LODLevel].Contains(item))
			{
				throw new InvalidOperationException("ThreadProcessing already in pool");
			}
			this.pool[LODLevel].Add(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<DChunkSquareMesh>[] pool;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object _poolLock;
}
