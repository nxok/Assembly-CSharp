using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityVelocity : NetPackageEntityTargeted
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageEntityVelocity Setup(int _entityId, Vector3 _motion, bool _bAdd)
	{
		base.Setup(_entityId);
		this.bAdd = _bAdd;
		if (_motion.x < -8f)
		{
			_motion.x = -8f;
		}
		if (_motion.y < -8f)
		{
			_motion.y = -8f;
		}
		if (_motion.z < -8f)
		{
			_motion.z = -8f;
		}
		if (_motion.x > 8f)
		{
			_motion.x = 8f;
		}
		if (_motion.y > 8f)
		{
			_motion.y = 8f;
		}
		if (_motion.z > 8f)
		{
			_motion.z = 8f;
		}
		this.motion = _motion;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		base.read(_reader);
		this.bAdd = _reader.ReadBoolean();
		this.motion = new Vector3(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.bAdd);
		_writer.Write(this.motion.x);
		_writer.Write(this.motion.y);
		_writer.Write(this.motion.z);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		Entity entity = _world.GetEntity(this.entityId);
		if (!(entity != null))
		{
			Log.Out("Discarding " + base.GetType().Name + " for entity Id=" + this.entityId.ToString());
			return;
		}
		if (!this.bAdd)
		{
			entity.SetVelocity(this.motion);
			return;
		}
		entity.AddVelocity(this.motion);
	}

	public override int GetLength()
	{
		return 16;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float max = 8f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 motion;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bAdd;
}
