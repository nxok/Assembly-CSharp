using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerEquipment : NetPackageEntityTargeted
{
	public NetPackagePlayerEquipment Setup(EntityAlive _entity)
	{
		base.Setup(_entity.entityId);
		this.equipment = _entity.equipment;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		base.read(_reader);
		this.equipment = Equipment.Read(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		this.equipment.Write(_writer);
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
		entityAlive.equipment.Apply(this.equipment, false);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerEquipment>().Setup(entityAlive), false, -1, base.Sender.entityId, -1, null, 192, false);
		}
	}

	public override int GetLength()
	{
		return 60;
	}

	public Equipment equipment;
}
