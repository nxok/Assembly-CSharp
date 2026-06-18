using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIHordeSpawner
{
	public AIHordeSpawner(World _world, string _spawnerDefinition, Vector3 _targetPos, float _playerSearchBounds)
	{
		this.world = _world;
		this.spawner = new AIDirectorGameStagePartySpawner(_world, _spawnerDefinition);
		this.playerSearchBounds = _playerSearchBounds;
		this.targetPos = _targetPos;
	}

	public bool Tick(double _dt)
	{
		if (this.world.GetPlayers().Count == 0 || !AIDirector.CanSpawn(1f))
		{
			return true;
		}
		if (!this.isInited)
		{
			List<Entity> entitiesInBounds = this.world.GetEntitiesInBounds(typeof(EntityPlayer), BoundsUtils.BoundsForMinMax(this.targetPos.x - this.playerSearchBounds, this.targetPos.y - this.playerSearchBounds, this.targetPos.z - this.playerSearchBounds, this.targetPos.x + this.playerSearchBounds, this.targetPos.y + this.playerSearchBounds, this.targetPos.z + this.playerSearchBounds), new List<Entity>());
			for (int i = 0; i < entitiesInBounds.Count; i++)
			{
				EntityPlayer entityPlayer = (EntityPlayer)entitiesInBounds[i];
				if (!entityPlayer.IsIgnoredByAI())
				{
					this.spawner.AddMember(entityPlayer);
				}
			}
			if (this.spawner.partyMembers.Count == 0)
			{
				return false;
			}
			this.isInited = true;
			this.spawner.ResetPartyLevel(0);
			this.spawner.ClearMembers();
		}
		if (!this.spawner.Tick(_dt))
		{
			return true;
		}
		if (!this.spawner.canSpawn || this.numSpawned >= this.numToSpawn)
		{
			return false;
		}
		Vector3 vector;
		if (this.world.IsDaytime())
		{
			if (!this.world.GetMobRandomSpawnPosWithWater(this.targetPos, 45, 55, 45, true, out vector))
			{
				return false;
			}
		}
		else if (!this.world.GetMobRandomSpawnPosWithWater(this.targetPos, 55, 70, 55, true, out vector))
		{
			return false;
		}
		EntityEnemy entityEnemy = (EntityEnemy)EntityFactory.CreateEntity(EntityGroups.GetRandomFromGroup(this.spawner.spawnGroupName, ref this.lastClassId, null), vector);
		Log.Out("Screamer spawned {0} from {1}", new object[]
		{
			entityEnemy.EntityName,
			this.spawner.spawnGroupName
		});
		entityEnemy.SetSpawnerSource(EnumSpawnerSource.Dynamic);
		this.world.SpawnEntityInWorld(entityEnemy);
		entityEnemy.IsHordeZombie = true;
		entityEnemy.bIsChunkObserver = true;
		entityEnemy.SetInvestigatePosition(AIWanderingHordeSpawner.RandomPos(this.world.aiDirector, this.targetPos, 3f), 2400, true);
		this.hordeList.Add(entityEnemy);
		this.spawner.IncSpawnCount();
		this.numSpawned++;
		return false;
	}

	public bool isSpawning
	{
		get
		{
			return this.spawner.canSpawn;
		}
	}

	public void Cleanup()
	{
		for (int i = 0; i < this.hordeList.Count; i++)
		{
			EntityEnemy entityEnemy = this.hordeList[i];
			entityEnemy.IsHordeZombie = false;
			entityEnemy.bIsChunkObserver = false;
		}
		this.hordeList.Clear();
	}

	public Vector3 targetPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorGameStagePartySpawner spawner;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastClassId;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityEnemy> hordeList = new List<EntityEnemy>();

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public float playerSearchBounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInited;

	public int numToSpawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public int numSpawned;
}
