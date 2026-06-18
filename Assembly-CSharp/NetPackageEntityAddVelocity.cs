using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityAddVelocity : NetPackage
{
	public NetPackageEntityAddVelocity Setup(int _entityId, Vector3 _addVelocity)
	{
		this.entityId = _entityId;
		this.addVelocity = _addVelocity;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.entityId = _br.ReadInt32();
		this.addVelocity = StreamUtils.ReadVector3(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.entityId);
		StreamUtils.Write(_bw, this.addVelocity);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		_world.GetGameManager().AddVelocityToEntityServer(this.entityId, this.addVelocity);
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
		return 16;
	}

	public int entityId;

	public Vector3 addVelocity;
}
