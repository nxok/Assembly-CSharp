using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIWanderingHordeSpawner
{
	public AIWanderingHordeSpawner(AIDirector _director, AIWanderingHordeSpawner.SpawnType _spawnType, AIWanderingHordeSpawner.HordeArrivedDelegate _arrivedEvent, List<AIDirectorPlayerState> _targets, ulong _endTime, Vector3 _startPos, Vector3 _pitStopPos, Vector3 _endPos)
	{
		this.director = _director;
		this.startPos = _startPos;
		this.pitStopPos = _pitStopPos;
		this.endPos = _endPos;
		this.endTime = _endTime;
		this.arrivedCallback = _arrivedEvent;
		this.spawnType = _spawnType;
		AIWanderingHordeSpawner.SpawnType spawnType = this.spawnType;
		string text;
		int num;
		if (spawnType != AIWanderingHordeSpawner.SpawnType.Bandits)
		{
			if (spawnType != AIWanderingHordeSpawner.SpawnType.Horde)
			{
			}
			text = "WanderingHorde";
			num = 50;
		}
		else
		{
			text = "WanderingBandits";
			num = 0;
		}
		this.spawner = new AIDirectorGameStagePartySpawner(_director.World, text);
		for (int i = 0; i < _targets.Count; i++)
		{
			this.spawner.AddMember(_targets[i].Player);
		}
		this.spawner.ResetPartyLevel(num);
		this.spawner.ClearMembers();
	}

	public bool Update(World world, float _deltaTime)
	{
		if (world.GetPlayers().Count == 0)
		{
			return true;
		}
		if (world.worldTime >= this.endTime)
		{
			if (this.arrivedCallback != null)
			{
				this.arrivedCallback();
			}
			return true;
		}
		bool flag = this.UpdateSpawn(world, _deltaTime);
		if (flag && this.commandList.Count == 0)
		{
			if (this.arrivedCallback != null)
			{
				this.arrivedCallback();
			}
			return true;
		}
		if (!flag)
		{
			AstarManager.Instance.AddLocationLine(this.startPos, this.endPos, 64);
		}
		else
		{
			Vector3 vector = Vector3.zero;
			int num = 0;
			for (int i = 0; i < this.commandList.Count; i++)
			{
				Entity enemy = this.commandList[i].Enemy;
				if (!enemy.IsDead())
				{
					vector += enemy.position;
					num++;
				}
			}
			if (num > 0)
			{
				vector *= 1f / (float)num;
				AstarManager.Instance.AddLocation(vector, 64);
			}
		}
		this.UpdateHorde(_deltaTime);
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool UpdateSpawn(World _world, float _deltaTime)
	{
		if (!AIDirector.CanSpawn(1f))
		{
			return true;
		}
		if (!this.spawner.Tick((double)_deltaTime))
		{
			return true;
		}
		this.spawnDelay -= _deltaTime;
		if (this.spawnDelay >= 0f)
		{
			return false;
		}
		this.spawnDelay = 1f;
		if (!this.spawner.canSpawn)
		{
			return false;
		}
		Vector3 vector;
		if (!_world.GetMobRandomSpawnPosWithWater(this.startPos, 1, 6, 15, true, out vector))
		{
			return false;
		}
		EntityEnemy entityEnemy = (EntityEnemy)EntityFactory.CreateEntity(EntityGroups.GetRandomFromGroup(this.spawner.spawnGroupName, ref this.lastClassId, null), vector);
		entityEnemy.SetSpawnerSource(EnumSpawnerSource.Dynamic);
		_world.SpawnEntityInWorld(entityEnemy);
		entityEnemy.IsHordeZombie = true;
		entityEnemy.bIsChunkObserver = true;
		entityEnemy.IsHordeZombie = true;
		entityEnemy.bIsChunkObserver = true;
		int num = this.bonusLootSpawnCount + 1;
		this.bonusLootSpawnCount = num;
		if (num >= GameStageDefinition.LootWanderingBonusEvery)
		{
			this.bonusLootSpawnCount = 0;
			entityEnemy.lootDropProb *= GameStageDefinition.LootWanderingBonusScale;
		}
		AIWanderingHordeSpawner.ZombieCommand zombieCommand = new AIWanderingHordeSpawner.ZombieCommand();
		zombieCommand.Enemy = entityEnemy;
		zombieCommand.TargetPos = AIWanderingHordeSpawner.RandomPos(this.director, this.endPos, 6f);
		zombieCommand.Command = AIWanderingHordeSpawner.ECommand.EndPos;
		this.commandList.Add(zombieCommand);
		entityEnemy.SetInvestigatePosition(zombieCommand.TargetPos, 6000, false);
		AIDirector.LogAI("Spawned wandering horde (group {0}, zombie {1})", new object[]
		{
			this.spawner.spawnGroupName,
			entityEnemy
		});
		this.spawner.IncSpawnCount();
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateHorde(float dt)
	{
		int i = 0;
		while (i < this.commandList.Count)
		{
			AIWanderingHordeSpawner.ZombieCommand zombieCommand = this.commandList[i];
			bool flag = zombieCommand.Enemy.IsDead() || zombieCommand.Enemy.GetAttackTarget() != null;
			if (!flag)
			{
				if (zombieCommand.Command == AIWanderingHordeSpawner.ECommand.PitStop || zombieCommand.Command == AIWanderingHordeSpawner.ECommand.EndPos)
				{
					if (zombieCommand.Enemy.HasInvestigatePosition)
					{
						if (zombieCommand.Enemy.InvestigatePosition != zombieCommand.TargetPos)
						{
							flag = true;
							string text = "Wandering horde zombie '";
							EntityEnemy enemy = zombieCommand.Enemy;
							AIDirector.LogAIExtra(text + ((enemy != null) ? enemy.ToString() : null) + "' removed from horde control. Was killed or investigating", Array.Empty<object>());
						}
						else
						{
							zombieCommand.Enemy.SetInvestigatePosition(zombieCommand.TargetPos, 6000, false);
						}
					}
					else if (zombieCommand.Command == AIWanderingHordeSpawner.ECommand.PitStop)
					{
						string text2 = "Wandering horde zombie '";
						EntityEnemy enemy2 = zombieCommand.Enemy;
						AIDirector.LogAIExtra(text2 + ((enemy2 != null) ? enemy2.ToString() : null) + "' reached pitstop. Wander around for awhile", Array.Empty<object>());
						zombieCommand.WanderTime = 90f + this.director.random.RandomFloat * 4f;
						zombieCommand.Command = AIWanderingHordeSpawner.ECommand.Wander;
					}
					else
					{
						flag = true;
					}
				}
				else
				{
					zombieCommand.WanderTime -= dt;
					zombieCommand.Enemy.ResetDespawnTime();
					if (zombieCommand.WanderTime <= 0f && zombieCommand.Enemy.GetAttackTarget() == null)
					{
						string text3 = "Wandering horde zombie '";
						EntityEnemy enemy3 = zombieCommand.Enemy;
						AIDirector.LogAIExtra(text3 + ((enemy3 != null) ? enemy3.ToString() : null) + "' wandered long enough. Going to endstop", Array.Empty<object>());
						zombieCommand.Command = AIWanderingHordeSpawner.ECommand.EndPos;
						zombieCommand.TargetPos = AIWanderingHordeSpawner.RandomPos(this.director, this.endPos, 6f);
						zombieCommand.Enemy.SetInvestigatePosition(zombieCommand.TargetPos, 6000, false);
						zombieCommand.Enemy.IsHordeZombie = false;
					}
				}
			}
			if (flag)
			{
				string text4 = "Wandering horde zombie '";
				EntityEnemy enemy4 = zombieCommand.Enemy;
				AIDirector.LogAIExtra(text4 + ((enemy4 != null) ? enemy4.ToString() : null) + "' removed from control", Array.Empty<object>());
				zombieCommand.Enemy.IsHordeZombie = false;
				zombieCommand.Enemy.bIsChunkObserver = false;
				this.commandList.RemoveAt(i);
			}
			else
			{
				i++;
			}
		}
	}

	public void Cleanup()
	{
		for (int i = 0; i < this.commandList.Count; i++)
		{
			AIWanderingHordeSpawner.ZombieCommand zombieCommand = this.commandList[i];
			zombieCommand.Enemy.IsHordeZombie = false;
			zombieCommand.Enemy.bIsChunkObserver = false;
		}
	}

	public static Vector3 RandomPos(AIDirector director, Vector3 target, float radius)
	{
		Vector2 vector = director.random.RandomOnUnitCircle * radius;
		return target + new Vector3(vector.x, 0f, vector.y);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cInvestigateTime = 6000;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirector director;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIWanderingHordeSpawner.HordeArrivedDelegate arrivedCallback;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 pitStopPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 endPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong endTime;

	public AIWanderingHordeSpawner.SpawnType spawnType;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorGameStagePartySpawner spawner;

	[PublicizedFrom(EAccessModifier.Private)]
	public float spawnDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastClassId;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<AIWanderingHordeSpawner.ZombieCommand> commandList = new List<AIWanderingHordeSpawner.ZombieCommand>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int bonusLootSpawnCount;

	public delegate void HordeArrivedDelegate();

	public enum SpawnType
	{
		Bandits,
		Horde
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum ECommand
	{
		PitStop,
		Wander,
		EndPos
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class ZombieCommand
	{
		public AIWanderingHordeSpawner.ECommand Command;

		public EntityEnemy Enemy;

		public float WanderTime;

		public Vector3 TargetPos;
	}
}
