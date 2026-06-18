using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageTurretSpawn : NetPackage
{
	public NetPackageTurretSpawn Setup(int _entityType, Vector3 _pos, Vector3 _rot, ItemValue _itemValue, int _entityThatPlaced = -1)
	{
		this.entityType = _entityType;
		this.pos = _pos;
		this.rot = _rot;
		this.itemValue = _itemValue;
		this.entityThatPlaced = _entityThatPlaced;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.entityType = _reader.ReadInt32();
		this.pos = StreamUtils.ReadVector3(_reader);
		this.rot = StreamUtils.ReadVector3(_reader);
		this.itemValue = new ItemValue();
		this.itemValue.Read(_reader);
		this.entityThatPlaced = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.entityType);
		StreamUtils.Write(_writer, this.pos);
		StreamUtils.Write(_writer, this.rot);
		this.itemValue.Write(_writer);
		_writer.Write(this.entityThatPlaced);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!base.ValidEntityIdForSender(this.entityThatPlaced, false))
		{
			return;
		}
		bool flag = false;
		if (this.itemValue.ItemClass.HasAnyTags(FastTags<TagGroup.Global>.Parse("drone")) && DroneManager.CanAddMoreDrones())
		{
			flag = true;
		}
		else if ((this.itemValue.ItemClass.HasAnyTags(FastTags<TagGroup.Global>.Parse("turretRanged")) || this.itemValue.ItemClass.HasAnyTags(FastTags<TagGroup.Global>.Parse("turretMelee"))) && TurretTracker.CanAddMoreTurrets())
		{
			flag = true;
		}
		EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(this.entityThatPlaced) as EntityPlayer;
		if (flag && entityPlayer != null)
		{
			Entity entity = EntityFactory.CreateEntity(this.entityType, this.pos, this.rot);
			entity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
			if (entity as EntityTurret != null)
			{
				EntityTurret entityTurret = entity as EntityTurret;
				entityTurret.factionId = entityPlayer.factionId;
				entityTurret.belongsPlayerId = entityPlayer.entityId;
				entityTurret.factionRank = entityPlayer.factionRank - 1;
				entityTurret.OriginalItemValue = this.itemValue.Clone();
				entityTurret.groundPosition = this.pos;
				entityTurret.ForceOn = true;
				entityTurret.Spawned = true;
				ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(this.entityThatPlaced);
				entityTurret.OwnerID = clientInfo.InternalId;
				entityPlayer.AddOwnedEntity(entityTurret);
				_world.SpawnEntityInWorld(entityTurret);
				entityTurret.bPlayerStatsChanged = true;
			}
			else if (entity as EntityDrone != null)
			{
				EntityDrone entityDrone = entity as EntityDrone;
				entityDrone.factionId = entityPlayer.factionId;
				entityDrone.belongsPlayerId = entityPlayer.entityId;
				entityDrone.factionRank = entityPlayer.factionRank - 1;
				entityDrone.OriginalItemValue = this.itemValue.Clone();
				entityDrone.SetItemValueToLoad(entityDrone.OriginalItemValue);
				entityDrone.Spawned = true;
				entityDrone.PrepareToSpawn();
				ClientInfo clientInfo2 = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(this.entityThatPlaced);
				entityDrone.OwnerID = clientInfo2.InternalId;
				entityPlayer.AddOwnedEntity(entityDrone);
				_world.SpawnEntityInWorld(entityDrone);
				entityDrone.bPlayerStatsChanged = true;
			}
		}
		else
		{
			GameManager.Instance.ItemDropServer(new ItemStack(this.itemValue, 1), this.pos, Vector3.zero, this.entityThatPlaced, 60f, false);
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageVehicleCount>().Setup(), false, -1, -1, -1, null, 192, false);
	}

	public override int GetLength()
	{
		return 20;
	}

	public int entityType;

	public Vector3 pos;

	public Vector3 rot;

	public ItemValue itemValue;

	public int entityThatPlaced;
}
