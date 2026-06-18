using System;
using UnityEngine;

public abstract class DynamicMeshContainer
{
	public string ToDebugLocation()
	{
		return this.WorldPosition.x.ToString() + " " + this.WorldPosition.z.ToString();
	}

	public abstract GameObject GetGameObject();

	[PublicizedFrom(EAccessModifier.Protected)]
	public DynamicMeshContainer()
	{
	}

	public Vector3i WorldPosition;

	public long Key;
}
