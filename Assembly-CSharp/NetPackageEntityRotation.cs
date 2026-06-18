using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityRotation : NetPackageEntityTargeted
{
	public override bool ReliableDelivery
	{
		get
		{
			return false;
		}
	}

	public NetPackageEntityRotation Setup(int _entityId, Vector3i _rot, Quaternion _qrot, bool _bUseQRot)
	{
		base.Setup(_entityId);
		this.rot = _rot;
		this.qrot = _qrot;
		this.bUseQRotation = _bUseQRot;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		base.read(_br);
		this.bUseQRotation = _br.ReadBoolean();
		if (!this.bUseQRotation)
		{
			this.rot = new Vector3i((int)_br.ReadInt16(), (int)_br.ReadInt16(), (int)_br.ReadInt16());
			return;
		}
		this.qrot = new Quaternion(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.bUseQRotation);
		if (!this.bUseQRotation)
		{
			_bw.Write((short)this.rot.x);
			_bw.Write((short)this.rot.y);
			_bw.Write((short)this.rot.z);
			return;
		}
		_bw.Write(this.qrot.x);
		_bw.Write(this.qrot.y);
		_bw.Write(this.qrot.z);
		_bw.Write(this.qrot.w);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!base.ValidEntityIdForSender(this.entityId, false))
		{
			return;
		}
		Entity entity = _world.GetEntity(this.entityId);
		if (entity == null)
		{
			return;
		}
		Entity attachedMainEntity = entity.AttachedMainEntity;
		if (attachedMainEntity != null && _world.GetPrimaryPlayerId() == attachedMainEntity.entityId)
		{
			return;
		}
		if (this.bUseQRotation)
		{
			entity.SetQRotFromNetwork(this.qrot, 3);
			return;
		}
		Vector3 vector;
		vector..ctor((float)(this.rot.x * 360) / 256f, (float)(this.rot.y * 360) / 256f, (float)(this.rot.z * 360) / 256f);
		entity.SetRotFromNetwork(vector, 3);
	}

	public override int GetLength()
	{
		return 12;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3i rot;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Quaternion qrot;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bUseQRotation;
}
