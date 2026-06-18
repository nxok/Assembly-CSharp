using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityRemove : NetPackageEntityTargeted
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageEntityRemove Setup(int _entityId, EnumRemoveEntityReason _reason)
	{
		base.Setup(_entityId);
		this.reason = (byte)_reason;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		base.read(_reader);
		this.reason = _reader.ReadByte();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.reason);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (_world.GetEntity(this.entityId) == null)
		{
			Log.Error(string.Format("NetPackageEntityRemove entity {0} missing", this.entityId));
		}
		_world.RemoveEntity(this.entityId, (EnumRemoveEntityReason)this.reason);
	}

	public override int GetLength()
	{
		return 4;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte reason;
}
