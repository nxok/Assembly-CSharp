using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorChunkEventComponent : AIDirectorHordeComponent
{
	public void Clear()
	{
		this.activeChunks.Clear();
		this.checkChunks.Clear();
	}

	public override void Tick(double _dt)
	{
		base.Tick(_dt);
		float num = (float)_dt;
		this.spawnDelay -= num;
		if (this.spawnDelay <= 0f)
		{
			this.spawnDelay = 5f;
			this.CheckToSpawn();
			foreach (KeyValuePair<long, AIDirectorChunkData> keyValuePair in this.activeChunks)
			{
				if (!keyValuePair.Value.Tick(5f))
				{
					this.removeChunks.Add(keyValuePair.Key);
				}
			}
			if (this.removeChunks.Count > 0)
			{
				for (int i = 0; i < this.removeChunks.Count; i++)
				{
					this.activeChunks.Remove(this.removeChunks[i]);
				}
				this.removeChunks.Clear();
			}
		}
		this.TickActiveSpawns(num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickActiveSpawns(float dt)
	{
		for (int i = this.scoutSpawnList.Count - 1; i >= 0; i--)
		{
			if (this.scoutSpawnList[i].Update(this.Director.World, dt))
			{
				AIDirector.LogAIExtra("Scout horde spawn finished (all mobs spawned)", Array.Empty<object>());
				this.scoutSpawnList[i].Cleanup();
				this.scoutSpawnList.RemoveAt(i);
			}
		}
		for (int j = this.hordeSpawnList.Count - 1; j >= 0; j--)
		{
			if (this.hordeSpawnList[j].Tick((double)dt))
			{
				AIDirector.LogAIExtra("Scout triggered horde finished (all mobs spawned)", Array.Empty<object>());
				this.hordeSpawnList.RemoveAt(j);
			}
		}
	}

	public override void Read(BinaryReader _stream, int _outerVersion)
	{
		if (_outerVersion >= 5)
		{
			this.activeChunks.Clear();
			int num = _stream.ReadInt32();
			int num2 = _stream.ReadInt32();
			for (int i = 0; i < num2; i++)
			{
				long num3 = _stream.ReadInt64();
				AIDirectorChunkData aidirectorChunkData = new AIDirectorChunkData();
				aidirectorChunkData.Read(_stream, num);
				this.activeChunks[num3] = aidirectorChunkData;
			}
		}
	}

	public override void Write(BinaryWriter _stream)
	{
		_stream.Write(1);
		_stream.Write(this.activeChunks.Count);
		foreach (KeyValuePair<long, AIDirectorChunkData> keyValuePair in this.activeChunks)
		{
			_stream.Write(keyValuePair.Key);
			keyValuePair.Value.Write(_stream);
		}
	}

	public int GetActiveCount()
	{
		return this.activeChunks.Count;
	}

	public bool HasAnySpawns
	{
		get
		{
			return this.hordeSpawnList.Count != 0;
		}
	}

	public AIDirectorChunkData GetChunkDataFromPosition(Vector3i _position, bool _createIfNeeded)
	{
		int num = World.toChunkXZ(_position.x) / 5;
		int num2 = World.toChunkXZ(_position.z) / 5;
		long num3 = WorldChunkCache.MakeChunkKey(num, num2);
		AIDirectorChunkData aidirectorChunkData;
		if (this.activeChunks.TryGetValue(num3, out aidirectorChunkData))
		{
			return aidirectorChunkData;
		}
		if (_createIfNeeded)
		{
			aidirectorChunkData = new AIDirectorChunkData();
			this.activeChunks[num3] = aidirectorChunkData;
		}
		return aidirectorChunkData;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartCooldownOnNeighbors(Vector3i _position, bool _isLong)
	{
		int num = World.toChunkXZ(_position.x) / 5;
		int num2 = World.toChunkXZ(_position.z) / 5;
		for (int i = 0; i < AIDirectorChunkEventComponent.neighbors.Length; i += 2)
		{
			long num3 = WorldChunkCache.MakeChunkKey(num + AIDirectorChunkEventComponent.neighbors[i], num2 + AIDirectorChunkEventComponent.neighbors[i + 1]);
			AIDirectorChunkData aidirectorChunkData;
			if (!this.activeChunks.TryGetValue(num3, out aidirectorChunkData))
			{
				aidirectorChunkData = new AIDirectorChunkData();
				this.activeChunks[num3] = aidirectorChunkData;
			}
			aidirectorChunkData.StartNeighborCooldown(_isLong);
		}
	}

	public void NotifyEvent(AIDirectorChunkEvent _chunkEvent)
	{
		AIDirectorChunkData chunkDataFromPosition = this.GetChunkDataFromPosition(_chunkEvent.Position, true);
		if (chunkDataFromPosition.IsReady)
		{
			chunkDataFromPosition.AddEvent(_chunkEvent);
			if (!this.checkChunks.Contains(chunkDataFromPosition))
			{
				this.checkChunks.Add(chunkDataFromPosition);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckToSpawn()
	{
		if (this.checkChunks.Count > 0)
		{
			AIDirectorChunkData aidirectorChunkData = this.checkChunks[0];
			this.checkChunks.RemoveAt(0);
			this.CheckToSpawn(aidirectorChunkData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckToSpawn(AIDirectorChunkData _chunkData)
	{
		if (GameStats.GetBool(EnumGameStats.ZombieHordeMeter) && GameStats.GetBool(EnumGameStats.IsSpawnEnemies) && _chunkData.ActivityLevel >= 25f)
		{
			AIDirectorChunkEvent aidirectorChunkEvent = _chunkData.FindBestEventAndReset();
			if (aidirectorChunkEvent != null)
			{
				bool flag = this.Director.random.RandomFloat < 0.2f && !GameUtils.IsPlaytesting();
				this.StartCooldownOnNeighbors(aidirectorChunkEvent.Position, flag);
				if (flag)
				{
					_chunkData.SetLongDelay();
					this.SpawnScouts(aidirectorChunkEvent.Position.ToVector3());
					return;
				}
			}
			else
			{
				AIDirector.LogAI("Chunk event not found!", Array.Empty<object>());
			}
		}
	}

	public void SpawnScouts(Vector3 targetPos)
	{
		Vector3 vector;
		if (base.FindScoutStartPos(targetPos, out vector))
		{
			EntityPlayer closestPlayer = this.Director.World.GetClosestPlayer(targetPos, 120f, false);
			if (closestPlayer)
			{
				int num = GameStageDefinition.CalcGameStageAround(closestPlayer);
				string text = "ScoutsRadiated";
				if (num < 45)
				{
					text = "Scouts1";
				}
				else if (num < 85)
				{
					text = "Scouts2";
				}
				else if (num < 125)
				{
					text = "ScoutsFeral";
				}
				EntitySpawner entitySpawner = new EntitySpawner(text, Vector3i.zero, Vector3i.zero, 0, null);
				this.scoutSpawnList.Add(new AIScoutHordeSpawner(entitySpawner, vector, targetPos, false));
				AIDirector.LogAI("Spawning {0} at {1}, to {2}", new object[]
				{
					text,
					vector.ToCultureInvariantString(),
					targetPos.ToCultureInvariantString()
				});
				return;
			}
		}
		else
		{
			AIDirector.LogAI("Scout spawning failed", Array.Empty<object>());
		}
	}

	public AIScoutHordeSpawner.IHorde CreateHorde(Vector3 startPos)
	{
		AIDirectorChunkEventComponent.Horde horde = new AIDirectorChunkEventComponent.Horde(this, startPos);
		this.hordeSpawnList.Add(horde);
		return horde;
	}

	public const int cVersion = 1;

	public const int cChunksPerArea = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cEventDelay = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cActivityLevelToSpawn = 25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSpawnChance = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, AIDirectorChunkData> activeChunks = new Dictionary<long, AIDirectorChunkData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> removeChunks = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<AIScoutHordeSpawner> scoutSpawnList = new List<AIScoutHordeSpawner>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<AIDirectorChunkEventComponent.Horde> hordeSpawnList = new List<AIDirectorChunkEventComponent.Horde>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<AIDirectorChunkData> checkChunks = new List<AIDirectorChunkData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float spawnDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] neighbors = new int[]
	{
		-1, 0, 1, 0, 0, -1, 0, 1, -1, -1,
		1, -1, -1, 1, 1, 1
	};

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class Horde : AIScoutHordeSpawner.IHorde
	{
		public Horde(AIDirectorChunkEventComponent outer, Vector3 pos)
		{
			this._outer = outer;
			this._pos = pos;
		}

		public void SpawnMore(int size)
		{
			int num = this._numSpawned + size;
			int num2 = num - this._numSpawned;
			this._numSpawned = num;
			if (this._horde != null)
			{
				this._horde.numToSpawn += num2;
				return;
			}
			this._horde = new AIHordeSpawner(this._outer.Director.World, "ScoutGSList", this._pos, 30f);
			this._horde.numToSpawn = num2;
		}

		public void SetSpawnPos(Vector3 pos)
		{
			if (this._horde != null)
			{
				this._horde.targetPos = pos;
			}
			this._pos = pos;
		}

		public void Destroy()
		{
			if (this._horde != null)
			{
				this._horde.Cleanup();
			}
			this._horde = null;
			this._destroy = true;
		}

		public bool Tick(double dt)
		{
			if (this._destroy)
			{
				return true;
			}
			if (this._horde != null && this._horde.Tick(dt))
			{
				this._horde.Cleanup();
				this._horde = null;
			}
			return false;
		}

		public bool canSpawnMore
		{
			get
			{
				return this._numSpawned < 25;
			}
		}

		public bool isSpawning
		{
			get
			{
				return this._horde != null && this._horde.isSpawning;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public AIDirectorChunkEventComponent _outer;

		[PublicizedFrom(EAccessModifier.Private)]
		public AIHordeSpawner _horde;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 _pos;

		[PublicizedFrom(EAccessModifier.Private)]
		public int _numSpawned;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool _destroy;
	}
}
