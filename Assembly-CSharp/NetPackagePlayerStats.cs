using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerStats : NetPackageEntityTargeted
{
	public NetPackagePlayerStats Setup(EntityAlive _entity)
	{
		base.Setup(_entity.entityId);
		this.entityNetworkStats.FillFromEntity(_entity);
		return this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~NetPackagePlayerStats()
	{
	}

	public override void read(PooledBinaryReader _reader)
	{
		base.read(_reader);
		this.entityNetworkStats.read(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		this.entityNetworkStats.write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = _world.GetEntity(this.entityId) as EntityAlive;
		if (!entityAlive)
		{
			Log.Out("Discarding " + base.GetType().Name + " for entity Id=" + this.entityId.ToString());
			return;
		}
		if (!base.ValidEntityIdForSender(this.entityId, true))
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && entityAlive is EntityPlayer)
		{
			this.entityNetworkStats.SetName(base.Sender.playerName);
		}
		this.entityNetworkStats.ToEntity(entityAlive);
		entityAlive.EnqueueNetworkStats(this.entityNetworkStats);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerStats>().Setup(entityAlive), false, -1, base.Sender.entityId, -1, null, 192, false);
		}
	}

	public override int GetLength()
	{
		return 60;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive.EntityNetworkStats entityNetworkStats = new EntityAlive.EntityNetworkStats();
}
