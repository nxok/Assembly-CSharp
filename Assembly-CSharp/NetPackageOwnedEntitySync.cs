using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageOwnedEntitySync : NetPackage
{
	public NetPackageOwnedEntitySync Setup(int _ownerId, int _entityId, NetPackageOwnedEntitySync.SyncType _syncType)
	{
		this.ownerId = _ownerId;
		this.entityId = _entityId;
		this.syncType = _syncType;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.ownerId = _reader.ReadInt32();
		this.entityId = _reader.ReadInt32();
		this.syncType = (NetPackageOwnedEntitySync.SyncType)_reader.ReadByte();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.ownerId);
		_writer.Write(this.entityId);
		_writer.Write((byte)this.syncType);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = GameManager.Instance.World.GetEntity(this.ownerId) as EntityAlive;
		if (entityAlive)
		{
			entityAlive.RemoveOwnedEntity(this.entityId);
		}
	}

	public override int GetLength()
	{
		return 9;
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public int ownerId;

	public int entityId;

	public NetPackageOwnedEntitySync.SyncType syncType;

	public enum SyncType : byte
	{
		Remove,
		Add
	}
}
