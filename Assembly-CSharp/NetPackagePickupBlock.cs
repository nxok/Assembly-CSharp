using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePickupBlock : NetPackage
{
	public NetPackagePickupBlock Setup(Vector3i _blockPos, BlockValue _blockValue, int _playerId, PersistentPlayerData _persistentPlayerData)
	{
		this.blockPos = _blockPos;
		this.blockValue = _blockValue;
		this.playerId = _playerId;
		this.persistentPlayerId = ((_persistentPlayerData != null) ? _persistentPlayerData.PrimaryId : null);
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.blockPos = StreamUtils.ReadVector3i(_br);
		this.blockValue = new BlockValue(_br.ReadUInt32());
		this.playerId = _br.ReadInt32();
		this.persistentPlayerId = PlatformUserIdentifierAbs.FromStream(_br, false, false);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		StreamUtils.Write(_bw, this.blockPos);
		_bw.Write(this.blockValue.rawData);
		_bw.Write(this.playerId);
		this.persistentPlayerId.ToStream(_bw, false);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!base.ValidEntityIdForSender(this.playerId, false))
		{
			return;
		}
		if (!base.ValidUserIdForSender(this.persistentPlayerId))
		{
			return;
		}
		if (!_world.IsRemote())
		{
			_world.GetGameManager().PickupBlockServer(this.blockPos, this.blockValue, this.playerId, this.persistentPlayerId);
			return;
		}
		_world.GetGameManager().PickupBlockClient(this.blockPos, this.blockValue, this.playerId);
	}

	public override int GetLength()
	{
		return 36;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int playerId;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs persistentPlayerId;
}
