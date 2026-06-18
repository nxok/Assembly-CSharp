using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorBloodMoonParty
{
	public AIDirectorBloodMoonParty(EntityPlayer _initialPlayer, AIDirectorBloodMoonComponent _controller, int _bloodMoonCountUNUSED)
	{
		this.spawnWorld = _initialPlayer.world;
		this.spawnBasePos = _initialPlayer.position;
		this.controller = _controller;
		this.partySpawner = new AIDirectorGameStagePartySpawner(_controller.Director.World, "BloodMoonHorde");
		this.partySpawner.AddMember(_initialPlayer);
		_initialPlayer.bloodMoonParty = this;
		this.spawnBaseDir = _controller.Random.RandomRange(0, 360);
		this.groupIndex = -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcBestDir(Vector3 basePos)
	{
		int[] array = new int[16];
		int num = 0;
		for (int i = 0; i < 16; i++)
		{
			float num2 = (float)i * 22.5f;
			this.spawnDirectionV = Quaternion.AngleAxis(num2, Vector3.up) * Vector3.forward * 40f;
			int num3 = 0;
			for (int j = 0; j < 9; j++)
			{
				Vector3 vector;
				if (this.spawnWorld.GetRandomSpawnPositionMinMaxToPosition(basePos + this.spawnDirectionV, 0, 10, 30, false, out vector, -1, true, 30, false, EnumLandClaimOwner.None, false))
				{
					num3++;
				}
			}
			if (num3 > 0)
			{
				num3 = (num3 + 2) / 3;
				if (Utils.FastAbs(Mathf.DeltaAngle(num2, (float)this.spawnBaseDir)) <= 60f)
				{
					num3 *= 3;
				}
			}
			array[i] = num3;
			num = Utils.FastMax(num, num3);
		}
		int num4 = 0;
		for (int k = 0; k < 16; k++)
		{
			if (array[k] == num)
			{
				num4++;
			}
		}
		int num5 = 0;
		int num6 = this.controller.Random.RandomRange(0, num4);
		for (int l = 0; l < 16; l++)
		{
			if (array[l] >= num && --num6 < 0)
			{
				num5 = l;
				break;
			}
		}
		this.spawnDirectionV = Quaternion.AngleAxis((float)num5 * 22.5f, Vector3.up) * Vector3.forward * 40f;
	}

	public bool Tick(World _world, double _dt, bool _canSpawn)
	{
		if (this.partySpawner.partyLevel < 0)
		{
			this.InitParty();
		}
		for (int i = this.zombies.Count - 1; i >= 0; i--)
		{
			AIDirectorBloodMoonParty.ManagedZombie managedZombie = this.zombies[i];
			managedZombie.updateDelay -= (float)_dt;
			if (managedZombie.updateDelay <= 0f)
			{
				managedZombie.updateDelay = 1.8f;
				if (!this.SeekTarget(managedZombie))
				{
					this.zombies.RemoveAt(i);
				}
			}
		}
		this.partySpawner.Tick(_dt);
		bool flag = false;
		if (_canSpawn)
		{
			if (!this.partySpawner.canSpawn || this.partySpawner.partyMembers.Count == 0)
			{
				return true;
			}
			if (AIDirector.CanSpawn(1.9f))
			{
				int num = this.partySpawner.groupIndex;
				if (num != this.groupIndex)
				{
					this.groupIndex = num;
					this.spawnBaseDir += 120;
					this.CalcBestDir(this.spawnBasePos);
				}
				flag = true;
				int count = this.partySpawner.partyMembers.Count;
				int num2 = Utils.FastMin(this.partySpawner.maxAlive, this.enemyActiveMax);
				if (this.zombies.Count < num2)
				{
					for (int j = Utils.FastMin(count, 3); j > 0; j--)
					{
						if (this.nextPlayer >= count)
						{
							this.nextPlayer = 0;
						}
						EntityPlayer entityPlayer = this.partySpawner.partyMembers[this.nextPlayer];
						bool flag2 = false;
						if (this.IsPlayerATarget(entityPlayer))
						{
							flag2 = this.SpawnZombie(_world, entityPlayer, entityPlayer.position, this.spawnDirectionV);
						}
						this.nextPlayer++;
						if (flag2)
						{
							break;
						}
					}
				}
			}
		}
		return flag;
	}

	public void PlayerLoggedOut(EntityPlayer _player)
	{
		this.partySpawner.RemoveMember(_player, false);
		if (this.nextPlayer >= this.partySpawner.partyMembers.Count)
		{
			this.nextPlayer = 0;
		}
	}

	public void KillPartyZombies()
	{
		int count = this.zombies.Count;
		if (count > 0)
		{
			this.partySpawner.DecSpawnCount(count);
			for (int i = 0; i < count; i++)
			{
				EntityEnemy zombie = this.zombies[i].zombie;
				if (zombie && !zombie.IsDead() && !zombie.IsDespawned && zombie.gameObject)
				{
					zombie.Kill(DamageResponse.New(true));
				}
			}
			this.zombies.Clear();
		}
	}

	public bool IsEmpty
	{
		get
		{
			return this.partySpawner.partyMembers.Count <= 0;
		}
	}

	public bool IsMemberOfParty(int _entityID)
	{
		return this.partySpawner.IsMemberOfParty(_entityID);
	}

	public bool TryAddPlayer(EntityPlayer _player)
	{
		for (int i = 0; i < this.partySpawner.partyMembers.Count; i++)
		{
			if ((this.partySpawner.partyMembers[i].GetPosition() - _player.GetPosition()).sqrMagnitude <= 6400f)
			{
				this.AddPlayer(_player);
				return true;
			}
		}
		return false;
	}

	public void AddPlayer(EntityPlayer _player)
	{
		this.partySpawner.AddMember(_player);
		_player.bloodMoonParty = this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitParty()
	{
		int num = this.partySpawner.CalcPartyLevel();
		int num2 = AIDirectorBloodMoonComponent.BloodMoonEnemyCount * this.partySpawner.partyMembers.Count;
		this.enemyActiveMax = Utils.FastMin(30, num2);
		float num3 = Utils.FastMax(1f, (float)num2 / (float)this.enemyActiveMax);
		num3 = Utils.FastLerp(1f, num3, (float)num / 60f);
		this.partySpawner.SetScaling(num3);
		this.partySpawner.SetPartyLevel(num);
		this.bonusLootSpawnCount = this.partySpawner.bonusLootEvery / 2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool SpawnZombie(World _world, EntityPlayer _target, Vector3 _focusPos, Vector3 _radiusV)
	{
		Vector3 vector;
		if (!this.CalcSpawnPos(_world, _focusPos, _radiusV, out vector))
		{
			return false;
		}
		bool flag = true;
		int num = EntityGroups.GetRandomFromGroup(this.partySpawner.spawnGroupName, ref this.lastClassId, null);
		if (_target.AttachedToEntity && this.controller.Random.RandomFloat < 0.5f)
		{
			flag = false;
			num = EntityClass.FromString("animalZombieVultureRadiated");
		}
		EntityEnemy entityEnemy = (EntityEnemy)EntityFactory.CreateEntity(num, vector);
		entityEnemy.SetSpawnerSource(EnumSpawnerSource.Dynamic);
		_world.SpawnEntityInWorld(entityEnemy);
		entityEnemy.IsHordeZombie = true;
		entityEnemy.IsBloodMoon = true;
		entityEnemy.bIsChunkObserver = true;
		entityEnemy.timeStayAfterDeath /= 3;
		if (flag)
		{
			int num2 = this.bonusLootSpawnCount + 1;
			this.bonusLootSpawnCount = num2;
			if (num2 >= this.partySpawner.bonusLootEvery)
			{
				this.bonusLootSpawnCount = 0;
				entityEnemy.lootDropProb *= GameStageDefinition.LootBonusScale;
			}
		}
		AIDirectorBloodMoonParty.ManagedZombie managedZombie = new AIDirectorBloodMoonParty.ManagedZombie(entityEnemy, _target);
		this.zombies.Add(managedZombie);
		this.SeekTarget(managedZombie);
		this.partySpawner.IncSpawnCount();
		AstarManager.Instance.AddLocation(vector, 40);
		ValueTuple<int, int, int> valueTuple = GameUtils.WorldTimeToElements(_world.worldTime);
		int item = valueTuple.Item1;
		int item2 = valueTuple.Item2;
		int item3 = valueTuple.Item3;
		Log.Out("BloodMoonParty: SpawnZombie grp {0}, cnt {1}, {2}, loot {3}, at player {4}, day/time {5} {6:D2}:{7:D2}", new object[]
		{
			this.partySpawner.ToString(),
			this.zombies.Count,
			entityEnemy.EntityName,
			entityEnemy.lootDropProb,
			_target.entityId,
			item,
			item2,
			item3
		});
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CalcSpawnPos(World _world, Vector3 _focusPos, Vector3 _radiusV, out Vector3 spawnPos)
	{
		_radiusV = Quaternion.AngleAxis((this.controller.Random.RandomFloat - 0.5f) * 90f, Vector3.up) * _radiusV;
		return _world.GetMobRandomSpawnPosWithWater(_focusPos + _radiusV, 0, 10, 30, false, out spawnPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer FindPartyTarget(Vector3 fromPos)
	{
		float num = float.MaxValue;
		EntityPlayer entityPlayer = null;
		for (int i = this.partySpawner.partyMembers.Count - 1; i >= 0; i--)
		{
			EntityPlayer entityPlayer2 = this.partySpawner.partyMembers[i];
			if (this.IsPlayerATarget(entityPlayer2))
			{
				float sqrMagnitude = (fromPos - entityPlayer2.position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					entityPlayer = entityPlayer2;
				}
			}
		}
		return entityPlayer;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool SeekTarget(AIDirectorBloodMoonParty.ManagedZombie mz)
	{
		EntityAlive zombie = mz.zombie;
		if (!zombie || zombie.IsDead() || zombie.IsDespawned || !zombie.gameObject)
		{
			return false;
		}
		EntityPlayer entityPlayer = zombie.GetAttackTarget() as EntityPlayer;
		if (entityPlayer)
		{
			mz.player = entityPlayer;
		}
		if (!mz.player || !this.IsPlayerATarget(mz.player))
		{
			mz.player = this.FindPartyTarget(zombie.position);
		}
		if (mz.player)
		{
			Vector3 vector = zombie.position - mz.player.position;
			float sqrMagnitude = vector.sqrMagnitude;
			vector.y = 0f;
			Vector3 vector2;
			if (vector.sqrMagnitude >= 22500f && this.CalcSpawnPos(zombie.world, mz.player.position, this.spawnDirectionV, out vector2) && !zombie.world.IsPlayerAliveAndNear(zombie.position, 70f))
			{
				if (this.controller.Random.RandomFloat < 0.5f)
				{
					this.partySpawner.DecSpawnCount(1);
					zombie.lootDropProb = 0f;
					zombie.Kill(DamageResponse.New(true));
					return false;
				}
				zombie.SetPosition(vector2, true);
				zombie.moveHelper.Stop();
				Log.Warning("SeekTarget {0}, far, move {1}", new object[]
				{
					zombie.GetDebugName(),
					vector2
				});
			}
			if (sqrMagnitude <= 10000f || entityPlayer != mz.player)
			{
				zombie.SetAttackTarget(mz.player, 1200);
			}
			else
			{
				if (entityPlayer)
				{
					zombie.SetAttackTarget(null, 0);
				}
				zombie.SetInvestigatePosition(mz.player.position, 1200, true);
			}
			return true;
		}
		if (!zombie.world.IsPlayerAliveAndNear(zombie.position, 60f))
		{
			zombie.Kill(DamageResponse.New(true));
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsPlayerATarget(EntityPlayer player)
	{
		return !player.IsDead() && player.IsSpawned() && player.entityId != -1 && !player.IsIgnoredByAI() && player.Progression.Level > 1 && !player.IsBloodMoonDead;
	}

	public bool BloodmoonZombiesRemain
	{
		get
		{
			return this.zombies.Count > 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPartyJoinDistance = 80f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cPartyJoinDistanceSq = 6400f;

	public const float cSightDist = 100f;

	public const float cSightDistSq = 10000f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTeleportDist = 150f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTeleportDistSq = 22500f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSpawnPreferredArc = 120;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSpawnAngle = 90f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSpawnDistance = 40f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSpawnMinRandDistance = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSpawnMaxRandDistance = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSpawnMinPlayerDistance = 30;

	public AIDirectorGameStagePartySpawner partySpawner;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastClassId;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<AIDirectorBloodMoonParty.ManagedZombie> zombies = new List<AIDirectorBloodMoonParty.ManagedZombie>();

	[PublicizedFrom(EAccessModifier.Private)]
	public World spawnWorld;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 spawnBasePos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int spawnBaseDir;

	[PublicizedFrom(EAccessModifier.Private)]
	public int enemyActiveMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorBloodMoonComponent controller;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 spawnDirectionV;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nextPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public int groupIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int bonusLootSpawnCount;

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class ManagedZombie
	{
		public ManagedZombie(EntityEnemy _zombie, EntityPlayer _player)
		{
			this.zombie = _zombie;
			this.player = _player;
		}

		public EntityPlayer player;

		public EntityEnemy zombie;

		public float updateDelay;
	}
}
