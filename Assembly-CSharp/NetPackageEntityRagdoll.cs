using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityRagdoll : NetPackage
{
	public NetPackageEntityRagdoll Setup(Entity _entity, byte _state)
	{
		this.entityId = _entity.entityId;
		this.flags = 4;
		this.state = _state;
		return this;
	}

	public NetPackageEntityRagdoll Setup(Entity _entity, byte _mode, float _duration, EnumBodyPartHit _bodyPart, Vector3 _forceVec, Vector3 _forceWorldPos)
	{
		this.entityId = _entity.entityId;
		this.flags = 1;
		if (_mode != 0)
		{
			this.flags |= 2;
			this.mode = _mode;
		}
		this.duration = _duration;
		this.bodyPart = _bodyPart;
		this.forceVec = _forceVec;
		this.forceWorldPos = _forceWorldPos;
		this.hipPos = _entity.emodel.GetHipPosition();
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.entityId = _br.ReadInt32();
		this.flags = _br.ReadByte();
		if ((this.flags & 1) > 0)
		{
			this.duration = _br.ReadSingle();
			this.bodyPart = (EnumBodyPartHit)_br.ReadInt16();
			this.forceVec = StreamUtils.ReadVector3(_br);
			this.forceWorldPos = StreamUtils.ReadVector3(_br);
			this.hipPos = StreamUtils.ReadVector3(_br);
		}
		this.mode = 0;
		if ((this.flags & 2) > 0)
		{
			this.mode = _br.ReadByte();
		}
		this.state = 0;
		if ((this.flags & 4) > 0)
		{
			this.state = _br.ReadByte();
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.entityId);
		_bw.Write(this.flags);
		if ((this.flags & 1) > 0)
		{
			_bw.Write(this.duration);
			_bw.Write((short)this.bodyPart);
			StreamUtils.Write(_bw, this.forceVec);
			StreamUtils.Write(_bw, this.forceWorldPos);
			StreamUtils.Write(_bw, this.hipPos);
		}
		if ((this.flags & 2) > 0)
		{
			_bw.Write(this.mode);
		}
		if ((this.flags & 4) > 0)
		{
			_bw.Write(this.state);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = _world.GetEntity(this.entityId) as EntityAlive;
		if (entityAlive == null)
		{
			Log.Out("Discarding " + base.GetType().Name + " for entity Id=" + this.entityId.ToString());
			return;
		}
		if ((this.flags & 1) > 0)
		{
			entityAlive.emodel.DoRagdoll((EModelBase.RagdollMode)this.mode, this.duration, this.bodyPart, this.forceVec, this.forceWorldPos, true);
		}
		if ((this.flags & 4) > 0)
		{
			entityAlive.emodel.SetRagdollState((int)this.state);
		}
	}

	public override int GetLength()
	{
		return 48;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cFlagsForce = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cFlagsMode = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cFlagsState = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte flags;

	[PublicizedFrom(EAccessModifier.Private)]
	public float duration;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumBodyPartHit bodyPart;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 forceVec;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 forceWorldPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 hipPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte mode;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte state;
}
