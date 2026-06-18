using System;

public static class WaterUtils
{
	public static int GetVoxelKey2D(int _x, int _z)
	{
		return _x * 8976890 + _z * 981131;
	}

	public static int GetVoxelKey(int _x, int _y, int _z = 0)
	{
		return _x * 8976890 + _y * 981131 + _z;
	}

	public static bool IsChunkSafeToUpdate(Chunk chunk)
	{
		return chunk != null && !chunk.NeedsDecoration && !chunk.NeedsCopying && !chunk.IsLocked;
	}

	public static bool TryOpenChunkForUpdate(ChunkCluster _chunks, long _key, out Chunk _chunk)
	{
		bool flag;
		using (ScopedChunkWriteAccess chunkWriteAccess = ScopedChunkAccess.GetChunkWriteAccess(_chunks, _key))
		{
			Chunk chunk = chunkWriteAccess.Chunk;
			if (!WaterUtils.IsChunkSafeToUpdate(chunk))
			{
				_chunk = null;
				flag = false;
			}
			else
			{
				_chunk = chunk;
				_chunk.InProgressWaterSim = true;
				flag = true;
			}
		}
		return flag;
	}

	public static bool CanWaterFlowThrough(BlockValue _bv)
	{
		Block block = _bv.Block;
		return block != null && block.WaterFlowMask != BlockFaceFlag.All;
	}

	public static bool CanWaterFlowThrough(int _blockId)
	{
		Block block = Block.list[_blockId];
		return block != null && block.WaterFlowMask != BlockFaceFlag.All;
	}

	public static int GetWaterLevel(WaterValue waterValue)
	{
		if (waterValue.GetMass() > 195)
		{
			return 1;
		}
		return 0;
	}

	public static bool IsVoxelOutsideChunk(int _neighborX, int _neighborZ)
	{
		return _neighborX < 0 || _neighborX > 15 || _neighborZ < 0 || _neighborZ > 15;
	}
}
