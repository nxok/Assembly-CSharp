using System;
using UnityEngine;

public class DynamicObserver
{
	public void Start(Vector3 pos)
	{
		this.Position = pos;
		if (GameManager.Instance.World == null)
		{
			return;
		}
		if (this.Observer == null)
		{
			this.Observer = GameManager.Instance.AddChunkObserver(pos, false, DynamicObserver.ViewSize, GameManager.Instance.World.GetPrimaryPlayerId());
		}
		this.Observer.SetPosition(this.Position);
		this.StopTime = float.MaxValue;
	}

	public bool ContainsPoint(Vector3i pos)
	{
		int num = DynamicObserver.ViewSize * 16;
		return (float)pos.x >= this.Position.x - (float)num && (float)pos.x <= this.Position.x + (float)num && (float)pos.z >= this.Position.z - (float)num && (float)pos.z <= this.Position.z + (float)num;
	}

	public bool HasFallingBlocks()
	{
		foreach (long num in this.Observer.chunksLoaded)
		{
			if (GameManager.Instance == null)
			{
				return false;
			}
			if (GameManager.Instance.World == null)
			{
				return false;
			}
			Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkSync(num);
			if (chunk == null)
			{
				DynamicMeshManager.LogMsg("Observer couldn't load chunk so assuming falling");
				return true;
			}
			if (chunk.HasFallingBlocks())
			{
				DynamicMeshManager.Instance.AddUpdateData(chunk.Key, false, true, true, 3);
				return true;
			}
		}
		return false;
	}

	public void Stop()
	{
		if (this.Observer == null)
		{
			return;
		}
		try
		{
			GameManager.Instance.RemoveChunkObserver(this.Observer);
		}
		catch (Exception ex)
		{
			if (DynamicMeshManager.DoLog)
			{
				DynamicMeshManager.LogMsg("Observer already destroyed: " + ex.Message);
			}
		}
		this.Observer = null;
	}

	public static int ViewSize = 3;

	public Vector3 Position;

	public ChunkManager.ChunkObserver Observer;

	public float StopTime = float.MaxValue;
}
