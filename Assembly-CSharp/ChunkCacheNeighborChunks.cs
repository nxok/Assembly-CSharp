using System;

public class ChunkCacheNeighborChunks
{
	public ChunkCacheNeighborChunks(IChunkAccess _chunkAccess)
	{
		IChunk[,] array = new Chunk[3, 3];
		this.chunks = array;
		base..ctor();
		this.chunkAccess = _chunkAccess;
	}

	public void Init(IChunk _chunk, IChunk[] _chunkArr)
	{
		this[0, 0] = _chunk;
		this[-1, 0] = _chunkArr[1];
		this[1, 0] = _chunkArr[0];
		this[0, -1] = _chunkArr[3];
		this[0, 1] = _chunkArr[2];
		this[-1, -1] = _chunkArr[5];
		this[1, -1] = _chunkArr[7];
		this[-1, 1] = _chunkArr[6];
		this[1, 1] = _chunkArr[4];
	}

	public void Clear()
	{
		Array.Clear(this.chunks, 0, this.chunks.GetLength(0) * this.chunks.GetLength(1));
	}

	public IChunk this[int x, int y]
	{
		get
		{
			return this.chunks[x + 1, y + 1];
		}
		set
		{
			this.chunks[x + 1, y + 1] = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IChunk[,] chunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public IChunkAccess chunkAccess;
}
