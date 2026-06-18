using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntitySpawn : NetPackageEntityTargeted
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageEntitySpawn Setup(EntityCreationData _es)
	{
		base.Setup(_es.id);
		this.es = _es;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		base.read(_reader);
		this.es = new EntityCreationData();
		this.es.read(_reader, true);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		this.es.write(_writer, true);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.es.clientEntityId != 0)
		{
			List<EntityPlayerLocal> localPlayers = _world.GetLocalPlayers();
			for (int i = 0; i < localPlayers.Count; i++)
			{
				if (localPlayers[i].entityId == this.es.belongsPlayerId)
				{
					_world.ChangeClientEntityIdToServer(this.es.clientEntityId, this.es.id);
					return;
				}
			}
		}
		_world.entityAsyncManager.StartCreateEntity(this.es, delegate(EntityAsyncManager.EntityCreateHandle op)
		{
			_world.SpawnEntityInWorld(op.Entity);
		});
	}

	public override int GetLength()
	{
		return 20;
	}

	public EntityCreationData es;
}
