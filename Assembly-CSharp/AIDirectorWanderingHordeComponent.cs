using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorWanderingHordeComponent : AIDirectorHordeComponent
{
	public override void InitNewGame()
	{
		this.isPlaytest = GameUtils.IsPlaytesting();
		this.BanditNextTime = 0UL;
		this.HordeNextTime = 0UL;
	}

	public override void Tick(double _dt)
	{
		if (this.isPlaytest)
		{
			return;
		}
		base.Tick(_dt);
		this.TickActiveSpawns((float)_dt);
		this.TickNextTime(ref this.HordeNextTime, AIWanderingHordeSpawner.SpawnType.Horde);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickActiveSpawns(float dt)
	{
		for (int i = this.spawners.Count - 1; i >= 0; i--)
		{
			AIWanderingHordeSpawner aiwanderingHordeSpawner = this.spawners[i];
			if (aiwanderingHordeSpawner.Update(this.Director.World, dt))
			{
				AIDirector.LogAIExtra("Wandering spawner finished {0}", new object[] { aiwanderingHordeSpawner.spawnType });
				aiwanderingHordeSpawner.Cleanup();
				this.spawners.RemoveAt(i);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickNextTime(ref ulong _nextTime, AIWanderingHordeSpawner.SpawnType _spawnType)
	{
		if (!GameStats.GetBool(EnumGameStats.ZombieHordeMeter) || !GameStats.GetBool(EnumGameStats.IsSpawnEnemies))
		{
			_nextTime = 0UL;
			return;
		}
		if (_nextTime == 0UL)
		{
			if (this.Director.World.worldTime > 28000UL)
			{
				this.ChooseNextTime(_spawnType);
				return;
			}
		}
		else
		{
			int num = (int)(_nextTime - this.Director.World.worldTime);
			int num2 = num / 1000;
			if (num2 < 7)
			{
				if (this.OtherHordesAreActive)
				{
					_nextTime += (ulong)((7 - num2) * 1000);
					return;
				}
				if (num <= 0)
				{
					if (this.Director.World.Players.Count > 0)
					{
						this.StartSpawning(_spawnType);
						return;
					}
					this.ChooseNextTime(_spawnType);
				}
			}
		}
	}

	public override void Read(BinaryReader _stream, int _version)
	{
		base.Read(_stream, _version);
		this.HordeNextTime = _stream.ReadUInt64();
		if (_version > 3)
		{
			this.BanditNextTime = _stream.ReadUInt64();
		}
	}

	public override void Write(BinaryWriter _stream)
	{
		base.Write(_stream);
		_stream.Write(this.HordeNextTime);
		_stream.Write(this.BanditNextTime);
	}

	public void StartSpawning(AIWanderingHordeSpawner.SpawnType _spawnType)
	{
		AIDirector.LogAI("Wandering StartSpawning {0}", new object[] { _spawnType });
		this.CleanupType(_spawnType);
		bool flag = false;
		DictionaryList<int, AIDirectorPlayerState> trackedPlayers = this.Director.GetComponent<AIDirectorPlayerManagementComponent>().trackedPlayers;
		for (int i = 0; i < trackedPlayers.list.Count; i++)
		{
			if (!trackedPlayers.list[i].Dead)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			AIDirector.LogAI("Spawn {0}, no living players, wait 4 hours", new object[] { _spawnType });
			this.SetNextTime(_spawnType, this.Director.World.worldTime + 4000UL);
			return;
		}
		List<AIDirectorPlayerState> list = new List<AIDirectorPlayerState>();
		Vector3 vector;
		Vector3 vector2;
		Vector3 vector3;
		uint num = base.FindTargets(out vector, out vector2, out vector3, list);
		if (num > 0U)
		{
			AIDirector.LogAI("Spawn {0}, find targets, wait {1} hours", new object[] { _spawnType, num });
			this.SetNextTime(_spawnType, this.Director.World.worldTime + (ulong)(1000U * num));
			return;
		}
		this.ChooseNextTime(_spawnType);
		this.spawners.Add(new AIWanderingHordeSpawner(this.Director, _spawnType, null, list, this.Director.World.worldTime + 12000UL, vector, vector2, vector3));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CleanupType(AIWanderingHordeSpawner.SpawnType _spawnType)
	{
		for (int i = this.spawners.Count - 1; i >= 0; i--)
		{
			AIWanderingHordeSpawner aiwanderingHordeSpawner = this.spawners[i];
			if (aiwanderingHordeSpawner.spawnType == _spawnType)
			{
				aiwanderingHordeSpawner.Cleanup();
				this.spawners.RemoveAt(i);
			}
		}
	}

	public bool HasAnySpawns
	{
		get
		{
			return this.spawners.Count != 0;
		}
	}

	public bool OtherHordesAreActive
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return SkyManager.IsBloodMoonVisible() || this.Director.GetComponent<AIDirectorChunkEventComponent>().HasAnySpawns;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ChooseNextTime(AIWanderingHordeSpawner.SpawnType _spawnType)
	{
		if (_spawnType == AIWanderingHordeSpawner.SpawnType.Bandits)
		{
			this.BanditNextTime = this.Director.World.worldTime + (ulong)((long)base.Random.RandomRange(12000, 24000));
			this.BanditNextTime += 2000UL;
			return;
		}
		if (_spawnType == AIWanderingHordeSpawner.SpawnType.Horde)
		{
			this.HordeNextTime = this.Director.World.worldTime + (ulong)((long)base.Random.RandomRange(12000, 24000));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetNextTime(AIWanderingHordeSpawner.SpawnType _spawnType, ulong _time)
	{
		if (_spawnType == AIWanderingHordeSpawner.SpawnType.Bandits)
		{
			this.BanditNextTime = _time;
			return;
		}
		if (_spawnType == AIWanderingHordeSpawner.SpawnType.Horde)
		{
			this.HordeNextTime = _time;
		}
	}

	public void LogTimes()
	{
		AIDirector.LogAI("Next wandering - bandit {0}, horde {1}", new object[]
		{
			GameUtils.WorldTimeToString(this.BanditNextTime),
			GameUtils.WorldTimeToString(this.HordeNextTime)
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cNextHourMin = 7;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPlaytest;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<AIWanderingHordeSpawner> spawners = new List<AIWanderingHordeSpawner>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong BanditNextTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong HordeNextTime;
}
