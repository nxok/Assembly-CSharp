using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityPhysics : NetPackage
{
	public NetPackageEntityPhysics Setup(Entity _entity)
	{
		this.EntityId = _entity.entityId;
		this.Pos = _entity.position;
		this.QRot = _entity.qrotation;
		ushort num = 0;
		if (_entity.isPhysicsMaster)
		{
			num |= 1;
		}
		if (_entity.isCollided)
		{
			num |= 2;
		}
		if (_entity.onGround)
		{
			num |= 4;
		}
		this.Velocity = _entity.GetVelocityPerSecond();
		this.AngularVelocity = _entity.GetAngularVelocityPerSecond();
		this.Flags = num;
		return this;
	}

	public NetPackageEntityPhysics Setup(NetPackageEntityPhysics _p)
	{
		this.EntityId = _p.EntityId;
		this.Pos = _p.Pos;
		this.QRot = _p.QRot;
		this.Velocity = _p.Velocity;
		this.AngularVelocity = _p.AngularVelocity;
		this.Flags = _p.Flags;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.Flags = _br.ReadUInt16();
		this.EntityId = _br.ReadInt32();
		this.Pos.x = _br.ReadSingle();
		this.Pos.y = _br.ReadSingle();
		this.Pos.z = _br.ReadSingle();
		this.QRot.x = _br.ReadSingle();
		this.QRot.y = _br.ReadSingle();
		this.QRot.z = _br.ReadSingle();
		this.QRot.w = _br.ReadSingle();
		this.Velocity.x = _br.ReadSingle();
		this.Velocity.y = _br.ReadSingle();
		this.Velocity.z = _br.ReadSingle();
		this.AngularVelocity.x = _br.ReadSingle();
		this.AngularVelocity.y = _br.ReadSingle();
		this.AngularVelocity.z = _br.ReadSingle();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.Flags);
		_bw.Write(this.EntityId);
		_bw.Write(this.Pos.x);
		_bw.Write(this.Pos.y);
		_bw.Write(this.Pos.z);
		_bw.Write(this.QRot.x);
		_bw.Write(this.QRot.y);
		_bw.Write(this.QRot.z);
		_bw.Write(this.QRot.w);
		_bw.Write(this.Velocity.x);
		_bw.Write(this.Velocity.y);
		_bw.Write(this.Velocity.z);
		_bw.Write(this.AngularVelocity.x);
		_bw.Write(this.AngularVelocity.y);
		_bw.Write(this.AngularVelocity.z);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		Entity entity = _world.GetEntity(this.EntityId);
		if (entity == null)
		{
			return;
		}
		if (entity.isPhysicsMaster)
		{
			return;
		}
		Entity attachedMainEntity = entity.AttachedMainEntity;
		if (attachedMainEntity != null && _world.GetPrimaryPlayerId() == attachedMainEntity.entityId)
		{
			return;
		}
		entity.serverPos = NetEntityDistributionEntry.EncodePos(this.Pos);
		entity.isCollided = (this.Flags & 2) > 0;
		entity.onGround = (this.Flags & 4) > 0;
		entity.PhysicsMasterSetTargetOrientation(this.Pos, this.QRot);
		entity.SetVelocityPerSecond(this.Velocity, this.AngularVelocity);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if ((this.Flags & 1) == 0)
			{
				entity.PhysicsMasterBecome();
			}
			NetPackageEntityPhysics netPackageEntityPhysics = NetPackageManager.GetPackage<NetPackageEntityPhysics>().Setup(this);
			_world.entityDistributer.SendPacketToTrackedPlayers(entity.entityId, entity.belongsPlayerId, netPackageEntityPhysics, false);
		}
	}

	public override int GetLength()
	{
		return 58;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFlagIsMaster = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFlagIsCollided = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cFlagOnGround = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public int EntityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 Pos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion QRot;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 Velocity;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 AngularVelocity;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort Flags;
}
