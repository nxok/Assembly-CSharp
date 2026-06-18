using System;
using System.Collections.Generic;

public class BucketHashSetList
{
	public BucketHashSetList()
	{
		this.buckets = new OptimizedList<HashSetLong>(4);
	}

	public BucketHashSetList(int _noBuckets)
	{
		this.buckets = new OptimizedList<HashSetLong>(_noBuckets);
		for (int i = 0; i < _noBuckets; i++)
		{
			this.buckets.Add(new HashSetLong());
		}
	}

	public void Add(int _bucketIdx, long _value)
	{
		this.buckets.array[_bucketIdx].Add(_value);
	}

	public void Add(int _bucketIdx, HashSetLong _otherBucket)
	{
		this.buckets.array[_bucketIdx].UnionWithHashSetLong(_otherBucket);
	}

	public bool Contains(long _value)
	{
		if (!this.IsRecalc)
		{
			return false;
		}
		for (int i = 0; i < this.buckets.Count; i++)
		{
			if (this.buckets.array[i].Contains(_value))
			{
				return true;
			}
		}
		return false;
	}

	public void Remove(long _value)
	{
		this.list.Remove(_value);
		for (int i = 0; i < this.buckets.Count; i++)
		{
			if (this.buckets.array[i].Contains(_value))
			{
				this.buckets.array[i].Remove(_value);
				return;
			}
		}
	}

	public void Clear()
	{
		for (int i = 0; i < this.buckets.Count; i++)
		{
			this.buckets.array[i].Clear();
		}
		this.list.Clear();
		this.IsRecalc = false;
	}

	public void ExceptTarget(HashSetLong hash)
	{
		if (!this.IsRecalc)
		{
			return;
		}
		for (int i = 0; i < this.buckets.Count; i++)
		{
			hash.ExceptWithHashSetLong(this.buckets.array[i]);
		}
	}

	public void RecalcHashSetList()
	{
		this.list.Clear();
		this.elementsInList.Clear();
		this.IsRecalc = true;
		for (int i = 0; i < this.buckets.Count; i++)
		{
			foreach (long num in this.buckets.array[i])
			{
				if (this.elementsInList.Add(num))
				{
					this.list.Add(num);
				}
			}
		}
	}

	public BucketHashSetList Clone()
	{
		BucketHashSetList bucketHashSetList = new BucketHashSetList(this.buckets.Count);
		for (int i = 0; i < this.buckets.Count; i++)
		{
			bucketHashSetList.buckets.array[i].UnionWithHashSetLong(this.buckets.array[i]);
		}
		return bucketHashSetList;
	}

	public List<long> list = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<long> elementsInList = new HashSet<long>();

	public OptimizedList<HashSetLong> buckets;

	public bool IsRecalc;
}
