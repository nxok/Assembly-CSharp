using System;

public class RegionFileChunkSnapshot : IRegionFileChunkSnapshot, IMemoryPoolableObject
{
	public long Size
	{
		get
		{
			PooledMemoryStream pooledMemoryStream = this.stream;
			if (pooledMemoryStream == null)
			{
				return 0L;
			}
			return pooledMemoryStream.Length;
		}
	}

	public void Update(Chunk chunk, bool saveIfUnchanged)
	{
		if (saveIfUnchanged || chunk.NeedsSaving)
		{
			if (this.stream != null)
			{
				this.stream.SetLength(0L);
			}
			else
			{
				this.stream = MemoryPools.poolMS.AllocSync(true);
			}
			try
			{
				using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
				{
					pooledBinaryWriter.SetBaseStream(this.stream);
					pooledBinaryWriter.Write(116);
					pooledBinaryWriter.Write(116);
					pooledBinaryWriter.Write(99);
					pooledBinaryWriter.Write(0);
					pooledBinaryWriter.Write(47U);
					chunk.save(pooledBinaryWriter);
					this.stream.Position = 0L;
				}
			}
			catch (Exception ex)
			{
				Log.Error(string.Concat(new string[]
				{
					"Error writing blocks to stream (chunkX=",
					chunk.X.ToString(),
					" chunkZ=",
					chunk.Z.ToString(),
					"): ",
					ex.Message,
					"\nStackTrace: ",
					ex.StackTrace
				}));
				MemoryPools.poolMS.FreeSync(this.stream);
			}
		}
	}

	public void Write(RegionFileChunkWriter writer, string dir, int chunkX, int chunkZ)
	{
		if (this.stream != null)
		{
			writer.WriteStreamCompressed(dir, chunkX, chunkZ, "7rg", this.stream);
		}
	}

	public void Cleanup()
	{
		this.Reset();
	}

	public void Reset()
	{
		if (this.stream != null)
		{
			MemoryPools.poolMS.FreeSync(this.stream);
			this.stream = null;
		}
	}

	public const string EXT = "7rg";

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledMemoryStream stream;
}
