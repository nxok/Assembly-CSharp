using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageChunkClusterInfo : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageChunkClusterInfo Setup(ChunkCluster _chunkCluster)
	{
		this.name = _chunkCluster.Name;
		this.cMinPos = _chunkCluster.ChunkMinPos;
		this.cMaxPos = _chunkCluster.ChunkMaxPos;
		this.bInfinite = !_chunkCluster.IsFixedSize;
		this.pos = _chunkCluster.Position;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.name = _br.ReadString();
		this.cMinPos = new Vector2i(_br.ReadInt32(), _br.ReadInt32());
		this.cMaxPos = new Vector2i(_br.ReadInt32(), _br.ReadInt32());
		this.bInfinite = _br.ReadBoolean();
		this.pos = StreamUtils.ReadVector3(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.name);
		_bw.Write(this.cMinPos.x);
		_bw.Write(this.cMinPos.y);
		_bw.Write(this.cMaxPos.x);
		_bw.Write(this.cMaxPos.y);
		_bw.Write(this.bInfinite);
		StreamUtils.Write(_bw, this.pos);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_callbacks.ChunkClusterInfo(this.name, this.bInfinite, this.cMinPos, this.cMaxPos, this.pos);
	}

	public override int GetLength()
	{
		return 40;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i cMinPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i cMaxPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bInfinite;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 pos;
}
