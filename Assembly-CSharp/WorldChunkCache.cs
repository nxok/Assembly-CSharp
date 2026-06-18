using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

public class WorldChunkCache : IChunkAccess
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long MakeChunkKey(int x, int y)
	{
		return (((long)y & 16777215L) << 24) | ((long)x & 16777215L);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int extractX(long key)
	{
		return (int)((int)key << 8) >> 8;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int extractZ(long key)
	{
		return (int)(key >> 16) >> 8;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2i extractXZ(long key)
	{
		Vector2i vector2i;
		vector2i.x = WorldChunkCache.extractX(key);
		vector2i.y = WorldChunkCache.extractZ(key);
		return vector2i;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Chunk GetChunkSync(int _x, int _y)
	{
		return this.GetChunkSync(WorldChunkCache.MakeChunkKey(_x, _y));
	}

	public virtual Chunk GetChunkSync(long _key)
	{
		this.sync.EnterReadLock();
		Chunk chunk;
		this.chunks.dict.TryGetValue(_key & 72057594037927935L, out chunk);
		this.sync.ExitReadLock();
		return chunk;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IChunk IChunkAccess.GetChunkSync(int chunkX, int chunkZ)
	{
		return this.GetChunkSync(chunkX, chunkZ);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IChunk IChunkAccess.GetChunkSync(long chunkKey)
	{
		return this.GetChunkSync(chunkKey);
	}

	public IChunk GetChunkSync(int chunkX, int chunkY, int chunkZ)
	{
		return IChunkAccess.DefaultGetChunkSync(this, chunkX, chunkY, chunkZ);
	}

	public IChunk GetChunkSync(Vector2i chunkPos)
	{
		return IChunkAccess.DefaultGetChunkSync(this, chunkPos);
	}

	public IChunk GetChunkSync(PropRef propRef)
	{
		return IChunkAccess.DefaultGetChunkSync(this, propRef);
	}

	public IChunk GetChunkSync(BlockValueRef bvRef)
	{
		return IChunkAccess.DefaultGetChunkSync(this, bvRef);
	}

	public IChunk GetChunkFromWorldPos(int x, int z)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, x, z);
	}

	public IChunk GetChunkFromWorldPos(int x, int y, int z)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, x, y, z);
	}

	public IChunk GetChunkFromWorldPos(Vector3i blockPos)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, blockPos);
	}

	public bool GetChunkFromWorldPos(int x, int z, ref IChunk chunk)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, x, z, ref chunk);
	}

	public bool GetChunkFromWorldPos(Vector3i blockPos, ref IChunk chunk)
	{
		return IChunkAccess.DefaultGetChunkFromWorldPos(this, blockPos, ref chunk);
	}

	public virtual void RemoveChunkSync(long _key)
	{
		this.sync.EnterWriteLock();
		Chunk chunk;
		if (this.chunks.dict.TryGetValue(_key & 72057594037927935L, out chunk))
		{
			this.chunks.Remove(_key & 72057594037927935L);
			this.isChunkArrayDirty = true;
		}
		this.chunkKeys.Remove(_key);
		this.isChunkKeysDirty = true;
		this.sync.ExitWriteLock();
		if (chunk != null)
		{
			for (int i = 0; i < this.chunkCallbacks.Count; i++)
			{
				this.chunkCallbacks[i].OnChunkBeforeRemove(chunk);
			}
		}
	}

	public void NotifyOnChunkBeforeSave(Chunk _c)
	{
		for (int i = 0; i < this.chunkCallbacks.Count; i++)
		{
			this.chunkCallbacks[i].OnChunkBeforeSave(_c);
		}
	}

	public virtual bool AddChunkSync(Chunk _chunk, bool _bOmitCallbacks = false)
	{
		this.ChunkMinPos.x = Utils.FastMin(this.ChunkMinPos.x, _chunk.X);
		this.ChunkMinPos.y = Utils.FastMin(this.ChunkMinPos.y, _chunk.Z);
		this.ChunkMaxPos.x = Utils.FastMax(this.ChunkMaxPos.x, _chunk.X);
		this.ChunkMaxPos.y = Utils.FastMax(this.ChunkMaxPos.y, _chunk.Z);
		this.sync.EnterWriteLock();
		long key = _chunk.Key;
		if (this.chunkKeys.Contains(key))
		{
			this.sync.ExitWriteLock();
			return false;
		}
		this.chunks.Add(key & 72057594037927935L, _chunk);
		this.chunkKeys.Add(key);
		this.isChunkArrayDirty = true;
		this.isChunkKeysDirty = true;
		this.sync.ExitWriteLock();
		int num = 0;
		while (!_bOmitCallbacks && num < this.chunkCallbacks.Count)
		{
			this.chunkCallbacks[num].OnChunkAdded(_chunk);
			num++;
		}
		return true;
	}

	public List<Chunk> GetChunkArrayCopySync()
	{
		this.sync.EnterReadLock();
		if (this.isChunkArrayDirty)
		{
			this.chunkArrayCopy = new List<Chunk>(this.chunks.list.Count);
			this.chunkArrayCopy.AddRange(this.chunks.list);
			this.isChunkArrayDirty = false;
		}
		this.sync.ExitReadLock();
		return this.chunkArrayCopy;
	}

	public LinkedList<Chunk> GetChunkArray()
	{
		return this.chunks.list;
	}

	public HashSetLong GetChunkKeysCopySync()
	{
		if (this.isChunkKeysDirty)
		{
			this.sync.EnterReadLock();
			this.chunkKeysCopy.Clear();
			foreach (long num in this.chunkKeys)
			{
				this.chunkKeysCopy.Add(num);
			}
			this.sync.ExitReadLock();
			this.isChunkKeysDirty = false;
		}
		return this.chunkKeysCopy;
	}

	public int Count()
	{
		return this.chunks.list.Count;
	}

	public ReaderWriterLockSlim GetSyncRoot()
	{
		return this.sync;
	}

	public virtual bool ContainsChunkSync(long key)
	{
		this.sync.EnterReadLock();
		bool flag = this.chunks.dict.ContainsKey(key);
		this.sync.ExitReadLock();
		return flag;
	}

	public void AddChunkCallback(IChunkCallback _callback)
	{
		this.chunkCallbacks.Add(_callback);
	}

	public void RemoveChunkCallback(IChunkCallback _callback)
	{
		this.chunkCallbacks.Remove(_callback);
	}

	public virtual void Update()
	{
	}

	public virtual void Clear()
	{
		this.sync.EnterWriteLock();
		List<Chunk> list = new List<Chunk>();
		list.AddRange(this.GetChunkArray());
		this.chunks.Clear();
		this.chunkKeys.Clear();
		this.isChunkArrayDirty = true;
		this.isChunkKeysDirty = true;
		this.sync.ExitWriteLock();
		MemoryPools.PoolChunks.FreeSync(list);
	}

	public void GetNeighborChunkKeys(long _chunkKey, bool _includeDiagonals, ref long[] _neighbours)
	{
		if (_includeDiagonals && _neighbours.Length != 8)
		{
			_neighbours = new long[8];
		}
		else if (_neighbours.Length != 8)
		{
			_neighbours = new long[4];
		}
		int num = WorldChunkCache.extractX(_chunkKey);
		int num2 = WorldChunkCache.extractZ(_chunkKey);
		_neighbours[0] = WorldChunkCache.MakeChunkKey(num, num2 - 1);
		_neighbours[1] = WorldChunkCache.MakeChunkKey(num, num2 + 1);
		_neighbours[2] = WorldChunkCache.MakeChunkKey(num - 1, num2);
		_neighbours[3] = WorldChunkCache.MakeChunkKey(num + 1, num2);
		if (_includeDiagonals)
		{
			_neighbours[4] = WorldChunkCache.MakeChunkKey(num - 1, num2 - 1);
			_neighbours[5] = WorldChunkCache.MakeChunkKey(num - 1, num2 + 1);
			_neighbours[6] = WorldChunkCache.MakeChunkKey(num + 1, num2 - 1);
			_neighbours[7] = WorldChunkCache.MakeChunkKey(num + 1, num2 + 1);
		}
	}

	public bool GetNeighborChunks(Chunk _chunk, Chunk[] neighbours)
	{
		this.sync.EnterReadLock();
		int x = _chunk.X;
		int z = _chunk.Z;
		Chunk chunk = this.GetChunk(x + 1, z);
		if (chunk != null)
		{
			neighbours[0] = chunk;
			chunk = this.GetChunk(x - 1, z);
			if (chunk != null)
			{
				neighbours[1] = chunk;
				chunk = this.GetChunk(x, z + 1);
				if (chunk != null)
				{
					neighbours[2] = chunk;
					chunk = this.GetChunk(x, z - 1);
					if (chunk != null)
					{
						neighbours[3] = chunk;
						chunk = this.GetChunk(x + 1, z + 1);
						if (chunk != null)
						{
							neighbours[4] = chunk;
							chunk = this.GetChunk(x - 1, z - 1);
							if (chunk != null)
							{
								neighbours[5] = chunk;
								chunk = this.GetChunk(x - 1, z + 1);
								if (chunk != null)
								{
									neighbours[6] = chunk;
									chunk = this.GetChunk(x + 1, z - 1);
									if (chunk != null)
									{
										neighbours[7] = chunk;
										this.sync.ExitReadLock();
										return true;
									}
								}
							}
						}
					}
				}
			}
		}
		this.sync.ExitReadLock();
		return false;
	}

	public bool HasNeighborChunks(Chunk _chunk)
	{
		this.sync.EnterReadLock();
		int x = _chunk.X;
		int z = _chunk.Z;
		bool flag = this.GetChunk(x + 1, z) != null && this.GetChunk(x - 1, z) != null && this.GetChunk(x, z + 1) != null && this.GetChunk(x, z - 1) != null && this.GetChunk(x + 1, z + 1) != null && this.GetChunk(x - 1, z - 1) != null && this.GetChunk(x - 1, z + 1) != null && this.GetChunk(x + 1, z - 1) != null;
		this.sync.ExitReadLock();
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk GetChunk(int _x, int _y)
	{
		long num = WorldChunkCache.MakeChunkKey(_x, _y);
		Chunk chunk;
		this.chunks.dict.TryGetValue(num, out chunk);
		return chunk;
	}

	public const long InvalidChunk = 9223372036854775807L;

	public Vector2i ChunkMinPos = Vector2i.zero;

	public Vector2i ChunkMaxPos = Vector2i.zero;

	public DictionaryLinkedList<long, Chunk> chunks = new DictionaryLinkedList<long, Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<IChunkCallback> chunkCallbacks = new List<IChunkCallback>();

	public HashSetLong chunkKeys = new HashSetLong();

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool isChunkKeysDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong chunkKeysCopy = new HashSetLong();

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool isChunkArrayDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Chunk> chunkArrayCopy = new List<Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ReaderWriterLockSlim sync = new ReaderWriterLockSlim();
}
