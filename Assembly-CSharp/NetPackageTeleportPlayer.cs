using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageTeleportPlayer : NetPackage
{
	public NetPackageTeleportPlayer Setup(Vector3 _newPos, Vector3? _viewDirection = null, bool _onlyIfNotFlying = false)
	{
		this.pos = _newPos;
		this.viewDirection = _viewDirection;
		this.onlyIfNotFlying = _onlyIfNotFlying;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.pos = new Vector3(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
		if (_reader.ReadBoolean())
		{
			this.viewDirection = new Vector3?(new Vector3(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle()));
		}
		else
		{
			this.viewDirection = null;
		}
		this.onlyIfNotFlying = _reader.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.pos.x);
		_writer.Write(this.pos.y);
		_writer.Write(this.pos.z);
		_writer.Write(this.viewDirection != null);
		if (this.viewDirection != null)
		{
			_writer.Write(this.viewDirection.Value.x);
			_writer.Write(this.viewDirection.Value.y);
			_writer.Write(this.viewDirection.Value.z);
		}
		_writer.Write(this.onlyIfNotFlying);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityPlayerLocal primaryPlayer = _world.GetPrimaryPlayer();
		if (primaryPlayer != null)
		{
			primaryPlayer.TeleportToPosition(this.pos, this.onlyIfNotFlying, this.viewDirection);
			return;
		}
		Log.Out("Discarding " + base.GetType().Name + " (no local player)");
	}

	public override int GetLength()
	{
		return 13;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 pos;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3? viewDirection;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool onlyIfNotFlying;
}
