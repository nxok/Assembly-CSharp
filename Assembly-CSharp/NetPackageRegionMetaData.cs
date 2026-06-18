using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageRegionMetaData : DynamicMeshServerData
{
	public NetPackageRegionMetaData()
	{
	}

	public NetPackageRegionMetaData(DynamicMeshRegion region)
	{
		this.X = region.WorldPosition.x;
		this.Z = region.WorldPosition.z;
	}

	public override bool Prechecks()
	{
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg(string.Concat(new string[]
			{
				"Sending region data: ",
				this.X.ToString(),
				",",
				this.Z.ToString(),
				"  Items: ",
				this.ChunksWithData.Count.ToString(),
				"   length: ",
				this.GetLength().ToString()
			}));
		}
		return true;
	}

	public override int GetLength()
	{
		return 12 + this.ChunksWithData.Count * 8;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!DynamicMeshManager.CONTENT_ENABLED)
		{
			return;
		}
		if (DynamicMeshManager.Instance == null)
		{
			return;
		}
		DynamicMeshRegion region = DynamicMeshManager.Instance.GetRegion(this.X, this.Z);
		if (DynamicMeshManager.DoLog)
		{
			string text = "Recieved Region meta data ";
			Vector3i worldPosition = region.WorldPosition;
			DynamicMeshManager.LogMsg(text + worldPosition.ToString() + " items: " + this.ChunksWithData.Count.ToString());
		}
		foreach (Vector2i vector2i in this.ChunksWithData)
		{
			DynamicMeshManager.Instance.AddChunk(DynamicMeshUnity.GetItemKey(vector2i.x, vector2i.y), false, false, null);
		}
	}

	public override void read(PooledBinaryReader reader)
	{
		this.X = reader.ReadInt32();
		this.Z = reader.ReadInt32();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			this.ChunksWithData.Add(new Vector2i(reader.ReadInt32(), reader.ReadInt32()));
		}
	}

	public override bool FlushQueue
	{
		get
		{
			return true;
		}
	}

	public override void write(PooledBinaryWriter writer)
	{
		base.write(writer);
		writer.Write(this.X);
		writer.Write(this.Z);
		writer.Write(this.ChunksWithData.Count);
		for (int i = 0; i < this.ChunksWithData.Count; i++)
		{
			writer.Write(this.ChunksWithData[i].x);
			writer.Write(this.ChunksWithData[i].y);
		}
	}

	public List<Vector2i> ChunksWithData = new List<Vector2i>();
}
