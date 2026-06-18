using System;
using System.Collections.Generic;

public class ChunkBlockClearData : ChunkCustomData
{
	public ChunkBlockClearData()
	{
	}

	public ChunkBlockClearData(string _key, ulong _expiresInWorldTime, bool _isSavedToNetwork, World _world)
		: base(_key, _expiresInWorldTime, _isSavedToNetwork)
	{
		this.World = _world;
	}

	public override void OnRemove(Chunk chunk)
	{
		for (int i = this.BlockList.Count - 1; i >= 0; i--)
		{
			Vector3i vector3i = this.BlockList[i];
			chunk.SetBlock(this.World, vector3i.x, vector3i.y, vector3i.z, BlockValue.Air, true, true, false, false, -1);
		}
	}

	public List<Vector3i> BlockList = new List<Vector3i>();

	public World World;
}
