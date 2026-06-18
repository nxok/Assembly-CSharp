using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageBlockTrigger : NetPackage
{
	public NetPackageBlockTrigger Setup(Vector3i _blockPos, BlockValue _blockValue)
	{
		this.blockPos = _blockPos;
		this.blockValue = _blockValue;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.blockPos = StreamUtils.ReadVector3i(_br);
		this.blockValue = new BlockValue(_br.ReadUInt32());
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		StreamUtils.Write(_bw, this.blockPos);
		_bw.Write(this.blockValue.rawData);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (base.Sender.bAttachedToEntity)
		{
			EntityPlayer entityPlayer = _world.GetEntity(base.Sender.entityId) as EntityPlayer;
			this.blockValue.Block.HandleTrigger(entityPlayer, _world, this.blockPos, this.blockValue);
		}
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public override int GetLength()
	{
		return 30;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockValue;
}
