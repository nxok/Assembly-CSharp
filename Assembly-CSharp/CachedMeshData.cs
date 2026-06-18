using System;
using UnityEngine;

[Serializable]
public class CachedMeshData
{
	public bool ApproximatelyEquals(CachedMeshData other)
	{
		return this.name.Equals(other.name) && Mathf.Abs(this.vertexCount - other.vertexCount) < 10 && Mathf.Abs(this.triCount - other.triCount) < 10;
	}

	public string name;

	public int vertexCount;

	public int triCount;
}
