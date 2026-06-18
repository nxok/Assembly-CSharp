using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIScoutHordeSpawner
{
	public AIScoutHordeSpawner(EntitySpawner _spawner, Vector3 _startPos, Vector3 _endPos, bool _isBloodMoon)
	{
		this.spawner = _spawner;
		this.startPos = _startPos;
		this.endPos = _endPos;
		this.isBloodMoon = _isBloodMoon;
	}

	public bool Update(World world, float dt)
	{
		if (world.GetPlayers().Count == 0)
		{
			return true;
		}
		if (this.SpawnUpdate(world) && this.hordeList.Count == 0)
		{
			return true;
		}
		this.UpdateHorde(world, dt);
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool SpawnUpdate(World world)
	{
		if (!AIDirector.CanSpawn(1f) || this.spawner.CurrentWave > 0)
		{
			return true;
		}
		this.spawner.SpawnManually(world, GameUtils.WorldTimeToDays(world.worldTime), true, delegate(EntitySpawner _es, out EntityPlayer _outPlayerToAttack)
		{
			_outPlayerToAttack = null;
			return true;
		}, delegate(EntitySpawner _es, EntityPlayer _inPlayerToAttack, out EntityPlayer _outPlayerToAttack, out Vector3 _pos)
		{
			_outPlayerToAttack = null;
			return world.GetMobRandomSpawnPosWithWater(this.startPos, 0, 8, 10, true, out _pos);
		}, null, this.spawnedList);
		for (int i = 0; i < this.spawnedList.Count; i++)
		{
			EntityEnemy entityEnemy = this.spawnedList[i] as EntityEnemy;
			if (entityEnemy != null)
			{
				entityEnemy.IsHordeZombie = true;
				entityEnemy.IsScoutZombie = true;
				entityEnemy.IsBloodMoon = this.isBloodMoon;
				entityEnemy.bIsChunkObserver = true;
				AIScoutHordeSpawner.ZombieCommand zombieCommand = new AIScoutHordeSpawner.ZombieCommand();
				zombieCommand.Zombie = entityEnemy;
				zombieCommand.TargetPos = AIScoutHordeSpawner.CalcRandomPos(world.aiDirector, this.endPos, 6f);
				zombieCommand.Wandering = false;
				zombieCommand.AttackDelay = 2f;
				entityEnemy.SetInvestigatePosition(zombieCommand.TargetPos, 6000, true);
				this.hordeList.Add(zombieCommand);
				string text = "scout horde spawned '";
				EntityEnemy entityEnemy2 = entityEnemy;
				AIDirector.LogAI(text + ((entityEnemy2 != null) ? entityEnemy2.ToString() : null) + "'. Moving to point of interest", Array.Empty<object>());
			}
		}
		this.spawnedList.Clear();
		return this.spawner.CurrentWave > 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateHorde(World world, float deltaTime)
	{
		int i = 0;
		while (i < this.hordeList.Count)
		{
			bool flag = false;
			AIScoutHordeSpawner.ZombieCommand zombieCommand = this.hordeList[i];
			EntityEnemy zombie = zombieCommand.Zombie;
			if (zombie.IsDead())
			{
				flag = true;
			}
			else
			{
				EntityAlive attackTarget = zombie.GetAttackTarget();
				bool flag2 = attackTarget is EntityPlayer;
				if (zombieCommand.Horde != null)
				{
					if (attackTarget && !attackTarget.IsDead() && flag2)
					{
						zombieCommand.Horde.SetSpawnPos(attackTarget.GetPosition());
					}
					else
					{
						zombieCommand.Horde.SetSpawnPos(zombie.GetPosition());
					}
				}
				if (zombieCommand.Attacking)
				{
					if (!zombieCommand.Zombie.HasInvestigatePosition && (attackTarget == null || attackTarget.IsDead() || !flag2))
					{
						zombieCommand.Wandering = true;
						zombieCommand.WorldExpiryTime = world.worldTime + 2000UL;
					}
					else
					{
						zombieCommand.AttackDelay -= deltaTime;
						if (zombieCommand.AttackDelay <= 0f && zombie.bodyDamage.CurrentStun == EnumEntityStunType.None)
						{
							if (zombie.HasInvestigatePosition || (flag2 && !attackTarget.IsDead()))
							{
								Vector3 vector = (attackTarget ? attackTarget.GetPosition() : zombieCommand.Zombie.InvestigatePosition);
								if (this.spawnHordeNear(world, zombieCommand, vector))
								{
									zombieCommand.AttackDelay = 18f;
								}
								else
								{
									flag = true;
								}
							}
							else
							{
								zombieCommand.Wandering = true;
								zombieCommand.WorldExpiryTime = world.worldTime + 2000UL;
							}
						}
					}
				}
				else if (attackTarget)
				{
					if (flag2)
					{
						zombieCommand.Attacking = true;
					}
				}
				else if (zombieCommand.Wandering)
				{
					if (world.worldTime >= zombieCommand.WorldExpiryTime)
					{
						flag = true;
					}
				}
				else if (zombieCommand.Zombie.HasInvestigatePosition)
				{
					if (zombieCommand.Zombie.InvestigatePosition == zombieCommand.TargetPos)
					{
						zombieCommand.Zombie.SetInvestigatePosition(zombieCommand.TargetPos, 6000, true);
					}
				}
				else
				{
					zombieCommand.Wandering = true;
					zombieCommand.WorldExpiryTime = world.worldTime + 2000UL;
				}
			}
			if (flag)
			{
				if (zombieCommand.Horde != null)
				{
					zombieCommand.Horde.Destroy();
				}
				string text = "scout horde '";
				EntityEnemy zombie2 = zombieCommand.Zombie;
				AIDirector.LogAIExtra(text + ((zombie2 != null) ? zombie2.ToString() : null) + "' removed from control", Array.Empty<object>());
				zombieCommand.Zombie.IsHordeZombie = false;
				zombieCommand.Zombie.bIsChunkObserver = false;
				this.hordeList.RemoveAt(i);
			}
			else
			{
				i++;
			}
		}
	}

	public void Cleanup()
	{
		for (int i = 0; i < this.hordeList.Count; i++)
		{
			AIScoutHordeSpawner.ZombieCommand zombieCommand = this.hordeList[i];
			zombieCommand.Zombie.IsHordeZombie = false;
			zombieCommand.Zombie.bIsChunkObserver = false;
		}
		this.hordeList.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool spawnHordeNear(World world, AIScoutHordeSpawner.ZombieCommand command, Vector3 target)
	{
		AIDirector.LogAI("Scout spawned a zombie horde", Array.Empty<object>());
		if (command.Horde == null)
		{
			AIDirectorChunkEventComponent component = world.GetAIDirector().GetComponent<AIDirectorChunkEventComponent>();
			command.Horde = component.CreateHorde(target);
		}
		if (command.Horde.canSpawnMore)
		{
			int num = 5;
			if (world.aiDirector.random.RandomFloat < 0.12f)
			{
				num--;
				if (this.spawner.CurrentWave > 0)
				{
					Vector3 vector = this.endPos;
					this.spawner.ResetSpawner();
					this.spawner.numberToSpawnThisWave = 1;
					this.endPos = target;
					this.SpawnUpdate(world);
					this.endPos = vector;
				}
				else
				{
					this.spawner.numberToSpawnThisWave++;
				}
			}
			command.Horde.SpawnMore(num);
			command.Zombie.PlayOneShot(command.Zombie.GetSoundAlert(), false, false, false, null, 1f);
		}
		command.Horde.SetSpawnPos(target);
		return command.Horde.canSpawnMore || command.Horde.isSpawning;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 CalcRandomPos(AIDirector director, Vector3 target, float radius)
	{
		Vector2 vector = director.random.RandomOnUnitCircle * radius;
		return target + new Vector3(vector.x, 0f, vector.y);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntitySpawner spawner;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 endPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBloodMoon;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> spawnedList = new List<Entity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<AIScoutHordeSpawner.ZombieCommand> hordeList = new List<AIScoutHordeSpawner.ZombieCommand>();

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class ZombieCommand
	{
		public EntityEnemy Zombie;

		public ulong WorldExpiryTime;

		public Vector3 TargetPos;

		public bool Wandering;

		public bool Attacking;

		public float AttackDelay;

		public AIScoutHordeSpawner.IHorde Horde;
	}

	public interface IHorde
	{
		void SpawnMore(int size);

		void SetSpawnPos(Vector3 pos);

		void Destroy();

		bool canSpawnMore { get; }

		bool isSpawning { get; }
	}
}
