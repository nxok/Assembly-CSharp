using System;

public class ChunkSnapshotUtil : IRegionFileChunkSnapshotUtil
{
	public ChunkSnapshotUtil(RegionFileAccessAbstract regionFileAccess)
	{
		this.regionFileAccess = regionFileAccess;
		this.chunkReader = new RegionFileChunkReader(regionFileAccess);
		this.chunkWriter = new RegionFileChunkWriter(regionFileAccess);
	}

	public IRegionFileChunkSnapshot TakeSnapshot(Chunk chunk, bool saveIfUnchanged)
	{
		RegionFileChunkSnapshot regionFileChunkSnapshot = this.poolSnapshots.AllocSync(true);
		regionFileChunkSnapshot.Update(chunk, saveIfUnchanged);
		return regionFileChunkSnapshot;
	}

	public void WriteSnapshot(IRegionFileChunkSnapshot snapshot, string dir, int chunkX, int chunkZ)
	{
		snapshot.Write(this.chunkWriter, dir, chunkX, chunkZ);
	}

	public Chunk LoadChunk(string dir, long key)
	{
		int num = WorldChunkCache.extractX(key);
		int num2 = WorldChunkCache.extractZ(key);
		try
		{
			uint num3;
			PooledBinaryReader pooledBinaryReader = this.chunkReader.readIntoLoadStream(dir, num, num2, "7rg", out num3);
			if (pooledBinaryReader == null)
			{
				return null;
			}
			Chunk chunk = MemoryPools.PoolChunks.AllocSync(true);
			chunk.load(pooledBinaryReader, num3);
			chunk.NeedsRegeneration = true;
			return chunk;
		}
		catch (Exception ex)
		{
			Log.Error(string.Concat(new string[]
			{
				"EXCEPTION: In load chunk (chunkX=",
				num.ToString(),
				" chunkZ=",
				num2.ToString(),
				")"
			}));
			Log.Exception(ex);
			try
			{
				this.chunkReader.WriteBackup(dir, num, num2);
			}
			catch (Exception ex2)
			{
				Log.Error("Error backing up data:");
				Log.Exception(ex2);
			}
		}
		try
		{
			this.regionFileAccess.Remove(dir, num, num2);
		}
		catch (Exception ex3)
		{
			Log.Error(string.Concat(new string[]
			{
				"In remove chunk (chunkX=",
				num.ToString(),
				" chunkZ=",
				num2.ToString(),
				"):",
				ex3.Message
			}));
		}
		return null;
	}

	public void Free(IRegionFileChunkSnapshot iSnapshot)
	{
		if (iSnapshot == null)
		{
			return;
		}
		RegionFileChunkSnapshot regionFileChunkSnapshot = iSnapshot as RegionFileChunkSnapshot;
		if (regionFileChunkSnapshot != null)
		{
			this.poolSnapshots.FreeSync(regionFileChunkSnapshot);
			return;
		}
		Log.Error("Attempting to free snapshot of wrong type. Expected: RegionFileChunkSnapshot, Actual: " + iSnapshot.GetType().Name);
	}

	public void Cleanup()
	{
		this.poolSnapshots.Cleanup();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileAccessAbstract regionFileAccess;

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileChunkReader chunkReader;

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileChunkWriter chunkWriter;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryPooledObject<RegionFileChunkSnapshot> poolSnapshots = new MemoryPooledObject<RegionFileChunkSnapshot>(255);
}
