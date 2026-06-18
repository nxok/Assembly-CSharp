using System;
using System.Collections.Generic;

public class PrefabCache
{
	public Prefab GetPrefabRotated(string _name, int _rotation, bool _applyMapping = true, bool _fixChildblocks = true, bool _allowMissingBlocks = false, bool _skipBlockData = false)
	{
		_rotation &= 3;
		Dictionary<string, Prefab> dictionary = this.prefabCache;
		Prefab prefab2;
		lock (dictionary)
		{
			Prefab[] array;
			if (this.prefabCacheRotations.TryGetValue(_name, out array))
			{
				if (array[_rotation] != null)
				{
					return array[_rotation];
				}
			}
			else
			{
				array = new Prefab[4];
				this.prefabCacheRotations[_name] = array;
			}
			Prefab prefab = this.GetPrefab(_name, _applyMapping, _fixChildblocks && _rotation == 0, _allowMissingBlocks, _skipBlockData);
			if (prefab == null)
			{
				prefab2 = null;
			}
			else
			{
				if (_rotation > 0)
				{
					prefab = prefab.Clone(true);
					prefab.RotateY(true, _rotation);
				}
				array[_rotation] = prefab;
				prefab2 = prefab;
			}
		}
		return prefab2;
	}

	public Prefab GetPrefab(string _name, bool _applyMapping = true, bool _fixChildblocks = true, bool _allowMissingBlocks = false, bool _skipBlockData = false)
	{
		Dictionary<string, Prefab> dictionary = this.prefabCache;
		Prefab prefab;
		lock (dictionary)
		{
			if (this.prefabCache.ContainsKey(_name))
			{
				prefab = this.prefabCache[_name];
			}
			else
			{
				Prefab prefab2 = new Prefab();
				if (prefab2.Load(_name, _applyMapping, _fixChildblocks, _allowMissingBlocks, _skipBlockData))
				{
					this.prefabCache[_name] = prefab2;
					prefab = prefab2;
				}
				else
				{
					prefab = null;
				}
			}
		}
		return prefab;
	}

	public void Clear()
	{
		Dictionary<string, Prefab> dictionary = this.prefabCache;
		lock (dictionary)
		{
			this.prefabCache.Clear();
			this.prefabCacheRotations.Clear();
		}
	}

	public void CalculateStats(out int basePrefabCount, out int rotatedPrefabsCount, out int basePrefabBytes, out int rotatedPrefabBytes)
	{
		Dictionary<string, Prefab> dictionary = this.prefabCache;
		lock (dictionary)
		{
			basePrefabCount = this.prefabCache.Count;
			basePrefabBytes = 0;
			foreach (Prefab prefab in this.prefabCache.Values)
			{
				basePrefabBytes += prefab.EstimateOwnedBytes();
			}
			rotatedPrefabsCount = 0;
			rotatedPrefabBytes = 0;
			foreach (Prefab[] array in this.prefabCacheRotations.Values)
			{
				for (int i = 1; i < array.Length; i++)
				{
					Prefab prefab2 = array[i];
					if (prefab2 != null)
					{
						rotatedPrefabsCount++;
						rotatedPrefabBytes += prefab2.EstimateOwnedBytes();
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, Prefab> prefabCache = new Dictionary<string, Prefab>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, Prefab[]> prefabCacheRotations = new Dictionary<string, Prefab[]>();
}
