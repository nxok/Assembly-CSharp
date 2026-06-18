using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageChunkRemove : NetPackage
{
	public override int Channel
	{
		get
		{
			return 1;
		}
	}

	public NetPackageChunkRemove Setup(long _chunkKey)
	{
		this.chunkKey = _chunkKey;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.chunkKey = _reader.ReadInt64();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.chunkKey);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		_callbacks.RemoveChunk(this.chunkKey);
	}

	public override int GetLength()
	{
		return 4;
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public long chunkKey;
}
