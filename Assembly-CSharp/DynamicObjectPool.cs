using System;
using System.Collections.Generic;

public class DynamicObjectPool<T> where T : new()
{
	public DynamicObjectPool(int objectsPerBlock)
	{
		this.Init(objectsPerBlock, int.MaxValue);
	}

	public DynamicObjectPool(int objectsPerBlock, int maxObjects)
	{
		this.Init(objectsPerBlock, maxObjects);
	}

	public void AllocateBlock(int numObjects)
	{
		for (int i = 0; i < numObjects; i++)
		{
			this.push(new T());
		}
		this.m_numAllocatedObjects += numObjects;
	}

	public T Allocate()
	{
		if (this.m_numFreeObjects < 1)
		{
			this.AllocateBlock(this.m_numObjectsPerBlock);
		}
		this.m_numUsedObjects++;
		return this.pop();
	}

	public T[] Allocate(int numToAllocate)
	{
		if (numToAllocate < 1)
		{
			return null;
		}
		T[] array = new T[numToAllocate];
		while (this.m_numFreeObjects < numToAllocate)
		{
			this.AllocateBlock(this.m_numObjectsPerBlock);
		}
		for (int i = 0; i < numToAllocate; i++)
		{
			array[i] = this.Allocate();
		}
		return array;
	}

	public void Free(T obj)
	{
		this.push(obj);
		this.m_numUsedObjects--;
	}

	public void Free(T[] array)
	{
		foreach (T t in array)
		{
			this.Free(t);
		}
	}

	public void Compact()
	{
		this.m_numFreeObjects = 0;
		this.m_numAllocatedObjects -= this.m_freeObjects.Count;
		this.m_freeObjects = new List<T>(this.m_numObjectsPerBlock);
	}

	public int NumAllocatedObjects
	{
		get
		{
			return this.m_numAllocatedObjects;
		}
	}

	public int NumUsedObjects
	{
		get
		{
			return this.m_numUsedObjects;
		}
	}

	public int NumFreeObjects
	{
		get
		{
			return this.m_numFreeObjects;
		}
	}

	public int MaxObjects
	{
		get
		{
			return this.m_maxObjects;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init(int objectsPerBlock, int maxObjects)
	{
		this.m_numObjectsPerBlock = objectsPerBlock;
		this.m_maxObjects = maxObjects;
		this.m_freeObjects = new List<T>(objectsPerBlock);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void push(T t)
	{
		if (this.m_numFreeObjects >= this.m_freeObjects.Count)
		{
			this.m_freeObjects.Add(t);
		}
		else
		{
			this.m_freeObjects[this.m_numFreeObjects] = t;
		}
		this.m_numFreeObjects++;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public T pop()
	{
		if (this.m_numFreeObjects < 1)
		{
			return default(T);
		}
		List<T> freeObjects = this.m_freeObjects;
		int num = this.m_numFreeObjects - 1;
		this.m_numFreeObjects = num;
		return freeObjects[num];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_numAllocatedObjects;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_numUsedObjects;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_numFreeObjects;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_numObjectsPerBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_maxObjects;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<T> m_freeObjects;
}
