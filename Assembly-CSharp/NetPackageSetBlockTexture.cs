using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSetBlockTexture : NetPackage, IMemoryPoolableObject
{
	public NetPackageSetBlockTexture Setup(Vector3i _blockPos, BlockFace _blockFace, int _idx, int _playerIdThatChanged, byte _channel)
	{
		this.blockPos = _blockPos;
		this.blockFace = _blockFace;
		this.idx = (byte)_idx;
		this.playerIdThatChanged = _playerIdThatChanged;
		this.channel = _channel;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.blockPos = StreamUtils.ReadVector3i(_br);
		this.blockFace = (BlockFace)_br.ReadByte();
		this.idx = _br.ReadByte();
		this.playerIdThatChanged = _br.ReadInt32();
		this.channel = _br.ReadByte();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		StreamUtils.Write(_bw, this.blockPos);
		_bw.Write((byte)this.blockFace);
		_bw.Write(this.idx);
		_bw.Write(this.playerIdThatChanged);
		_bw.Write(this.channel);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && _world.ChunkCache != null)
		{
			GameManager.Instance.SetBlockTextureClient(this.blockPos, this.blockFace, (int)this.idx, this.channel);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			NetPackageSetBlockTexture netPackageSetBlockTexture = NetPackageManager.GetPackage<NetPackageSetBlockTexture>().Setup(this.blockPos, this.blockFace, (int)this.idx, this.playerIdThatChanged, this.channel);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(netPackageSetBlockTexture, false, -1, this.playerIdThatChanged, -1, null, 192, false);
		}
	}

	public override int GetLength()
	{
		return 19;
	}

	public void Reset()
	{
	}

	public void Cleanup()
	{
	}

	public static int GetPoolSize()
	{
		return 500;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockFace blockFace;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte idx;

	[PublicizedFrom(EAccessModifier.Private)]
	public int playerIdThatChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte channel;
}
