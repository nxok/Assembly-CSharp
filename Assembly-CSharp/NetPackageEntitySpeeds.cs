using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntitySpeeds : NetPackageEntityTargeted
{
	public override bool ReliableDelivery
	{
		get
		{
			return false;
		}
	}

	public NetPackageEntitySpeeds Setup(Entity _entity)
	{
		this.entityId = _entity.entityId;
		this.movementState = _entity.MovementState;
		this.speedForward = _entity.speedForward;
		this.speedStrafe = _entity.speedStrafe;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		base.read(_reader);
		this.movementState = (int)_reader.ReadByte();
		this.speedForward = _reader.ReadSingle();
		this.speedStrafe = _reader.ReadSingle();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((byte)this.movementState);
		_writer.Write(this.speedForward);
		_writer.Write(this.speedStrafe);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		Entity entity = _world.GetEntity(this.entityId);
		if (entity != null)
		{
			entity.MovementState = this.movementState;
			entity.speedForward = this.speedForward;
			entity.speedStrafe = this.speedStrafe;
			if (!_world.IsRemote())
			{
				_world.entityDistributer.SendPacketToTrackedPlayers(this.entityId, this.entityId, this, true);
			}
		}
	}

	public override int GetLength()
	{
		return 20;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int movementState;

	[PublicizedFrom(EAccessModifier.Private)]
	public float speedForward;

	[PublicizedFrom(EAccessModifier.Private)]
	public float speedStrafe;
}
