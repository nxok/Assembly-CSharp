using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityAwardKillServer : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageEntityAwardKillServer Setup(int _killerEntityId, int _killedEntityId)
	{
		this.EntityId = _killerEntityId;
		this.KilledEntityId = _killedEntityId;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.EntityId = _reader.ReadInt32();
		this.KilledEntityId = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.EntityId);
		_writer.Write(this.KilledEntityId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityPlayerLocal entityPlayerLocal = _world.GetEntity(this.EntityId) as EntityPlayerLocal;
		if (entityPlayerLocal != null)
		{
			EntityAlive entityAlive = _world.GetEntity(this.KilledEntityId) as EntityAlive;
			if (entityAlive != null)
			{
				QuestEventManager.Current.EntityKilled(entityPlayerLocal, entityAlive);
			}
		}
	}

	public override int GetLength()
	{
		return 8;
	}

	public int EntityId;

	public int KilledEntityId;
}
