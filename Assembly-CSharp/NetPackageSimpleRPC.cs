using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSimpleRPC : NetPackage
{
	public NetPackageSimpleRPC Setup(int _entityId, SimpleRPCType _type)
	{
		this.entityId = _entityId;
		this.type = _type;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.entityId = _reader.ReadInt32();
		this.type = (SimpleRPCType)_reader.ReadByte();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.entityId);
		_writer.Write((byte)this.type);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!base.ValidEntityIdForSender(this.entityId, false))
		{
			return;
		}
		_callbacks.SimpleRPC(this.entityId, this.type, true, _world.IsRemote());
	}

	public override int GetLength()
	{
		return 10;
	}

	public int entityId;

	public SimpleRPCType type;
}
