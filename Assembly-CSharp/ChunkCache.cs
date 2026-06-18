using System;

public class ChunkCache : IBlockAccess
{
	public ChunkCache(int _dim)
	{
		this.chunkArray = new Chunk[_dim, _dim];
	}

	public void Init(World world, int x, int y, int z, int sx, int sy, int sz)
	{
		this.worldObj = world;
		this.chunkX = x >> 4;
		this.chunkZ = z >> 4;
		int num = sx >> 4;
		int num2 = sz >> 4;
		for (int i = this.chunkX; i <= num; i++)
		{
			for (int j = this.chunkZ; j <= num2; j++)
			{
				Chunk chunkSync = world.ChunkCache.GetChunkSync(i, j);
				if (chunkSync != null)
				{
					this.chunkArray[i - this.chunkX, j - this.chunkZ] = chunkSync;
				}
			}
		}
	}

	public void Clear()
	{
		Array.Clear(this.chunkArray, 0, this.chunkArray.GetLength(0) * this.chunkArray.GetLength(1));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsChunkValid(Chunk _chunk)
	{
		return _chunk != null && _chunk.IsInitialized;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetChunk(int _cX, int _cY, out Chunk _chunk)
	{
		int num = _cX - this.chunkX;
		int num2 = _cY - this.chunkZ;
		Chunk chunk;
		if (num < 0 || num >= this.chunkArray.GetLength(0) || num2 < 0 || num2 >= this.chunkArray.GetLength(1))
		{
			chunk = this.worldObj.ChunkCache.GetChunkSync(_cX, _cY);
		}
		else
		{
			chunk = this.chunkArray[num, num2];
		}
		if (!this.IsChunkValid(chunk))
		{
			_chunk = null;
			return false;
		}
		_chunk = chunk;
		return true;
	}

	public BlockValue GetBlock(int _x, int _y, int _z)
	{
		if (_y < 0)
		{
			return BlockValue.Air;
		}
		if (_y >= 256)
		{
			return BlockValue.Air;
		}
		Chunk chunk;
		if (!this.TryGetChunk(World.toChunkXZ(_x), World.toChunkXZ(_z), out chunk))
		{
			return BlockValue.Air;
		}
		return chunk.GetBlock(_x & 15, _y, _z & 15);
	}

	public BlockValue GetBlock(Vector3i pos)
	{
		return IBlockAccess.DefaultGetBlock(this, pos);
	}

	public BlockValue GetBlock(BlockValueRef bvRef)
	{
		return IBlockAccess.DefaultGetBlock(this, bvRef);
	}

	public PropValue GetProp(int chunkX, int chunkZ, int propId)
	{
		Chunk chunk;
		if (!this.TryGetChunk(chunkX, chunkZ, out chunk))
		{
			return PropValue.AIR;
		}
		return chunk.GetProp(chunkX, chunkZ, propId);
	}

	public PropValue GetProp(long chunkKey, int propId)
	{
		return this.GetProp(WorldChunkCache.extractX(chunkKey), WorldChunkCache.extractZ(chunkKey), propId);
	}

	public PropValue GetProp(Vector2i chunkPos, int propId)
	{
		return IBlockAccess.DefaultGetProp(this, chunkPos, propId);
	}

	public PropValue GetProp(PropRef propRef)
	{
		return IBlockAccess.DefaultGetProp(this, propRef);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk[,] chunkArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public World worldObj;
}
