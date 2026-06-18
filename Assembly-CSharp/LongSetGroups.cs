using System;
using System.Collections.Generic;

public class LongSetGroups
{
	public IReadOnlyCollection<LongSetGroups.Group> Groups
	{
		get
		{
			return this.groups;
		}
	}

	public int Count
	{
		get
		{
			return this.groups.Count;
		}
	}

	public int GroupedLongsCount
	{
		get
		{
			return this.groupsByLongKey.Count;
		}
	}

	public LongSetGroups.Group MergeOrCreateGroup(HashSetLong longsToGroup)
	{
		LongSetGroups.Group group = null;
		foreach (long num in longsToGroup)
		{
			LongSetGroups.Group group2;
			if (this.groupsByLongKey.TryGetValue(num, out group2))
			{
				if (group == null)
				{
					group = group2;
				}
				else if (group != group2)
				{
					group.MergeFrom(group2);
					this.groups.Remove(group2);
					foreach (long num2 in group2.Keys)
					{
						this.groupsByLongKey[num2] = group;
					}
				}
			}
		}
		if (group == null)
		{
			group = new LongSetGroups.Group();
			group.AddSet(new HashSetLong(longsToGroup));
			this.groups.Add(group);
		}
		else
		{
			group.AddSet(new HashSetLong(longsToGroup));
		}
		foreach (long num3 in longsToGroup)
		{
			this.groupsByLongKey[num3] = group;
		}
		return group;
	}

	public void RemoveGroupedKeys(HashSetLong keys)
	{
		List<HashSetLong> list = new List<HashSetLong>();
		for (int i = this.groups.Count - 1; i >= 0; i--)
		{
			LongSetGroups.Group group = this.groups[i];
			if (group.RemoveSet(keys, list))
			{
				foreach (long num in keys)
				{
					if (!group.Contains(num))
					{
						this.groupsByLongKey.Remove(num);
					}
				}
				foreach (HashSetLong hashSetLong in list)
				{
					foreach (long num2 in hashSetLong)
					{
						this.groupsByLongKey.Remove(num2);
					}
				}
				if (group.Count == 0)
				{
					this.groups.RemoveAt(i);
				}
			}
		}
		foreach (HashSetLong hashSetLong2 in list)
		{
			this.MergeOrCreateGroup(hashSetLong2);
		}
	}

	public bool TryGetGroup(long key, out LongSetGroups.Group group)
	{
		return this.groupsByLongKey.TryGetValue(key, out group);
	}

	public void Clear()
	{
		this.groupsByLongKey.Clear();
		this.groups.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<LongSetGroups.Group> groups = new List<LongSetGroups.Group>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, LongSetGroups.Group> groupsByLongKey = new Dictionary<long, LongSetGroups.Group>();

	public class Group
	{
		public IEnumerable<long> Keys
		{
			get
			{
				return this.keys;
			}
		}

		public IEnumerable<HashSetLong> Sets
		{
			get
			{
				return this.sets;
			}
		}

		public int Count
		{
			get
			{
				return this.keys.Count;
			}
		}

		public void AddSet(HashSetLong addSet)
		{
			this.sets.Add(addSet);
			this.keys.UnionWith(addSet);
		}

		public bool RemoveSet(HashSetLong removeSet, List<HashSetLong> orphanedSets)
		{
			bool flag = false;
			for (int i = this.sets.Count - 1; i >= 0; i--)
			{
				if (this.sets[i].SetEquals(removeSet))
				{
					this.sets.RemoveAt(i);
					flag = true;
					break;
				}
			}
			if (flag)
			{
				if (this.sets.Count > 1)
				{
					HashSetLong hashSetLong = this.sets[this.sets.Count - 1];
					for (int j = this.sets.Count - 2; j >= 0; j--)
					{
						HashSetLong hashSetLong2 = this.sets[j];
						if (!hashSetLong.Overlaps(hashSetLong2))
						{
							orphanedSets.Add(hashSetLong2);
							this.sets.RemoveAt(j);
						}
					}
				}
				this.keys.Clear();
				foreach (HashSetLong hashSetLong3 in this.sets)
				{
					this.keys.UnionWith(hashSetLong3);
				}
			}
			return flag;
		}

		public bool Contains(long key)
		{
			return this.keys.Contains(key);
		}

		public void MergeFrom(LongSetGroups.Group other)
		{
			foreach (HashSetLong hashSetLong in other.sets)
			{
				bool flag = false;
				using (List<HashSetLong>.Enumerator enumerator2 = this.sets.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						if (enumerator2.Current.SetEquals(hashSetLong))
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					this.sets.Add(hashSetLong);
					this.keys.UnionWith(hashSetLong);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSetLong keys = new HashSetLong();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<HashSetLong> sets = new List<HashSetLong>();
	}
}
