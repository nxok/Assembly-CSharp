using System;

public class DynamicMeshUpdateData
{
	public string ToDebugLocation()
	{
		return this.ChunkPosition.x.ToString() + "," + this.ChunkPosition.z.ToString();
	}

	public Vector3i ChunkPosition;

	public long Key;

	public float MaxTime;

	public float UpdateTime;

	public bool IsUrgent;

	public bool AddToThread;
}
