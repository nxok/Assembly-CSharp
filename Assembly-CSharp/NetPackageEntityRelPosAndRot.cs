using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityRelPosAndRot : NetPackageEntityRotation
{
	public override bool ReliableDelivery
	{
		get
		{
			return false;
		}
	}

	public NetPackageEntityRelPosAndRot Setup(int _entityId, Vector3i _deltaPos, Vector3i _absRot, Quaternion _qrot, bool _onGround, bool _bUseQRot, int _updateSteps)
	{
		base.Setup(_entityId, _absRot, _qrot, _bUseQRot);
		this.dPos = _deltaPos;
		this.onGround = _onGround;
		this.updateSteps = (short)_updateSteps;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		base.read(_reader);
		this.dPos = new Vector3i((int)_reader.ReadInt16(), (int)_reader.ReadInt16(), (int)_reader.ReadInt16());
		this.onGround = _reader.ReadBoolean();
		this.updateSteps = _reader.ReadInt16();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((short)this.dPos.x);
		_writer.Write((short)this.dPos.y);
		_writer.Write((short)this.dPos.z);
		_writer.Write(this.onGround);
		_writer.Write(this.updateSteps);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!base.ValidEntityIdForSender(this.entityId, true))
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
		entity.serverPos += this.dPos;
		Vector3 vector = entity.serverPos.ToVector3() / 32f;
		Vector3 vector2;
		vector2..ctor((float)(this.rot.x * 360) / 256f, (float)(this.rot.y * 360) / 256f, (float)(this.rot.z * 360) / 256f);
		if (this.bUseQRotation)
		{
			entity.SetPosAndQRotFromNetwork(vector, this.qrot, (int)this.updateSteps);
		}
		else
		{
			entity.SetPosAndRotFromNetwork(vector, vector2, (int)this.updateSteps);
		}
		entity.onGround = this.onGround;
	}

	public override int GetLength()
	{
		return 20;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i dPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool onGround;

	[PublicizedFrom(EAccessModifier.Private)]
	public short updateSteps;
}
