using System;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageChunk : NetPackage
{
	public override int Channel
	{
		get
		{
			return 1;
		}
	}

	public override bool Compress
	{
		get
		{
			return true;
		}
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~NetPackageChunk()
	{
		if (this.chunk != null)
		{
			this.chunk.InProgressNetworking = false;
			this.chunk = null;
		}
		if (this.serializedData != null)
		{
			MemoryPools.poolMS.FreeSync(this.serializedData);
			this.serializedData = null;
		}
	}

	public NetPackageChunk Setup(Chunk _chunk, bool _bOverwriteExisting = false)
	{
		this.chunk = _chunk;
		this.bOverwriteExisting = _bOverwriteExisting;
		this.serializedData = MemoryPools.poolMS.AllocSync(true);
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
		{
			pooledBinaryWriter.SetBaseStream(this.serializedData);
			this.chunk.write(pooledBinaryWriter);
		}
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.bOverwriteExisting = _reader.ReadBoolean();
		if (this.bOverwriteExisting)
		{
			this.chunkX = (int)_reader.ReadInt16();
			this.chunkY = (int)_reader.ReadInt16();
			this.chunkZ = (int)_reader.ReadInt16();
		}
		this.dataLen = _reader.ReadInt32();
		this.data = _reader.ReadBytes(this.dataLen);
		if (!this.bOverwriteExisting)
		{
			if (this.chunk == null)
			{
				this.chunk = MemoryPools.PoolChunks.AllocSync(true);
			}
			using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
			{
				pooledBinaryReader.SetBaseStream(new MemoryStream(this.data));
				this.chunk.read(pooledBinaryReader, uint.MaxValue);
			}
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.bOverwriteExisting);
		if (this.bOverwriteExisting)
		{
			_writer.Write((short)this.chunk.X);
			_writer.Write((short)this.chunk.Y);
			_writer.Write((short)this.chunk.Z);
		}
		_writer.Write((int)this.serializedData.Length);
		this.serializedData.Position = 0L;
		StreamUtils.StreamCopy(this.serializedData, _writer.BaseStream, null, true);
		MemoryPools.poolMS.FreeSync(this.serializedData);
		this.serializedData = null;
		this.chunk = null;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			Log.Warning("Received chunk while world is not set up");
			if (this.chunk != null)
			{
				MemoryPools.PoolChunks.FreeSync(this.chunk);
				this.chunk = null;
			}
			return;
		}
		long num = ((!this.bOverwriteExisting) ? this.chunk.Key : WorldChunkCache.MakeChunkKey(this.chunkX, this.chunkZ));
		Chunk chunkSync;
		if ((chunkSync = _world.ChunkCache.GetChunkSync(num)) != null && !this.bOverwriteExisting)
		{
			string name = base.GetType().Name;
			string text = ": chunk already loaded ";
			Chunk chunk = this.chunk;
			Log.Error(name + text + ((chunk != null) ? chunk.ToString() : null));
			return;
		}
		if (this.bOverwriteExisting)
		{
			Bounds bounds = Chunk.CalculateAABB(this.chunkX, this.chunkY, this.chunkZ);
			MultiBlockManager.Instance.DeregisterTrackedBlockDatas(bounds);
		}
		if (!this.bOverwriteExisting)
		{
			_world.ChunkCache.AddChunkSync(this.chunk, false);
			this.chunk.NeedsRegeneration = true;
			this.chunk = null;
			return;
		}
		if (chunkSync != null)
		{
			using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
			{
				pooledBinaryReader.SetBaseStream(new MemoryStream(this.data));
				chunkSync.OnUnload(_world);
				_world.ChunkCache.RemoveChunkSync(chunkSync.Key);
				chunkSync.Reset();
				chunkSync.read(pooledBinaryReader, uint.MaxValue);
				_world.ChunkCache.AddChunkSync(chunkSync, false);
				this.data = null;
			}
		}
	}

	public override int GetLength()
	{
		return 14 + (int)this.serializedData.Length;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk chunk;

	[PublicizedFrom(EAccessModifier.Private)]
	public PooledMemoryStream serializedData;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bOverwriteExisting;

	[PublicizedFrom(EAccessModifier.Private)]
	public int dataLen;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] data;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkY;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkZ;
}
