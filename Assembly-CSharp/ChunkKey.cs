using System;

public struct ChunkKey : IEquatable<ChunkKey>
{
	public ChunkKey(IChunk _chunk)
	{
		this.x = _chunk.X;
		this.z = _chunk.Z;
	}

	public ChunkKey(int _x, int _z)
	{
		this.x = _x;
		this.z = _z;
	}

	public override int GetHashCode()
	{
		return WaterUtils.GetVoxelKey2D(this.x, this.z);
	}

	public override bool Equals(object obj)
	{
		return base.Equals((ChunkKey)obj);
	}

	public bool Equals(ChunkKey other)
	{
		return this.x == other.x && this.z == other.z;
	}

	public int x;

	public int z;
}
