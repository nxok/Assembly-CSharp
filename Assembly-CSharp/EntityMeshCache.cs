using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityMeshCache : MonoBehaviour
{
	public void InitData(List<CachedMeshData> collection)
	{
		this.collection = new List<CachedMeshData>(collection);
	}

	public bool TryGetMeshData(string name, out CachedMeshData data)
	{
		name = name.Replace(" Instance", "");
		foreach (CachedMeshData cachedMeshData in this.collection)
		{
			if (cachedMeshData.name == name)
			{
				data = cachedMeshData;
				return true;
			}
		}
		Log.Warning("Could not find {0} in entity mesh cache for prefab: {1}", new object[]
		{
			name,
			base.gameObject.name
		});
		data = new CachedMeshData();
		return false;
	}

	public bool EqualsCollection(List<CachedMeshData> otherCollection)
	{
		if (otherCollection.Count != this.collection.Count)
		{
			return false;
		}
		for (int i = 0; i < this.collection.Count; i++)
		{
			if (!this.collection[i].ApproximatelyEquals(otherCollection[i]))
			{
				return false;
			}
		}
		return true;
	}

	[Header("This collection is filled on import. Do not edit manually")]
	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<CachedMeshData> collection;
}
