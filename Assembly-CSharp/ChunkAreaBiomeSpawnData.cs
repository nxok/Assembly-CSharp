using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ChunkAreaBiomeSpawnData
{
	public ChunkAreaBiomeSpawnData(Chunk _chunk, byte _biomeId, ChunkCustomData _ccd)
	{
		this.biomeId = _biomeId;
		this.area = new Rect((float)(_chunk.X * 16), (float)(_chunk.Z * 16), 80f, 80f);
		this.chunk = _chunk;
		this.ccd = _ccd;
		this.ccd.TriggerWriteDataDelegate = new ChunkCustomData.TriggerWriteData(this.BeforeWrite);
		if (this.ccd.data != null)
		{
			using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
			{
				pooledBinaryReader.SetBaseStream(new MemoryStream(this.ccd.data));
				this.read(pooledBinaryReader);
			}
		}
	}

	public bool IsSpawnNeeded(WorldBiomes _worldBiomes, ulong _worldTime)
	{
		BiomeDefinition biome = _worldBiomes.GetBiome(this.biomeId);
		if (biome == null)
		{
			return false;
		}
		BiomeSpawnEntityGroupList biomeSpawnEntityGroupList = BiomeSpawningClass.list[biome.m_sBiomeName];
		if (biomeSpawnEntityGroupList == null)
		{
			return false;
		}
		for (int i = 0; i < biomeSpawnEntityGroupList.list.Count; i++)
		{
			BiomeSpawnEntityGroupData biomeSpawnEntityGroupData = biomeSpawnEntityGroupList.list[i];
			ChunkAreaBiomeSpawnData.CountsAndTime countsAndTime;
			if (!this.entitesSpawned.TryGetValue(biomeSpawnEntityGroupData.idHash, out countsAndTime))
			{
				return true;
			}
			if (countsAndTime.count < countsAndTime.maxCount || _worldTime > countsAndTime.delayWorldTime)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanSpawn(int _idHash)
	{
		ChunkAreaBiomeSpawnData.CountsAndTime countsAndTime;
		return this.entitesSpawned.TryGetValue(_idHash, out countsAndTime) && countsAndTime.count < countsAndTime.maxCount;
	}

	public void SetCounts(int _idHash, int _count, int _maxCount)
	{
		ChunkAreaBiomeSpawnData.CountsAndTime countsAndTime;
		this.entitesSpawned.TryGetValue(_idHash, out countsAndTime);
		countsAndTime.count = _count;
		countsAndTime.maxCount = _maxCount;
		this.entitesSpawned[_idHash] = countsAndTime;
	}

	public void IncCount(int _idHash)
	{
		ChunkAreaBiomeSpawnData.CountsAndTime countsAndTime;
		if (!this.entitesSpawned.TryGetValue(_idHash, out countsAndTime))
		{
			countsAndTime.count = 1;
		}
		else
		{
			countsAndTime.count++;
		}
		this.entitesSpawned[_idHash] = countsAndTime;
		this.chunk.isModified = true;
	}

	public void DecCount(int _idHash, bool _killed)
	{
		ChunkAreaBiomeSpawnData.CountsAndTime countsAndTime;
		if (this.entitesSpawned.TryGetValue(_idHash, out countsAndTime))
		{
			countsAndTime.count = Utils.FastMax(countsAndTime.count - 1, 0);
			if (_killed)
			{
				countsAndTime.maxCount = Utils.FastMax(0, countsAndTime.maxCount - 1);
			}
			this.entitesSpawned[_idHash] = countsAndTime;
			this.chunk.isModified = true;
		}
	}

	public void DecMaxCount(int _idHash)
	{
		ChunkAreaBiomeSpawnData.CountsAndTime countsAndTime;
		if (this.entitesSpawned.TryGetValue(_idHash, out countsAndTime))
		{
			countsAndTime.maxCount = Utils.FastMax(0, countsAndTime.maxCount - 1);
			this.entitesSpawned[_idHash] = countsAndTime;
			this.chunk.isModified = true;
		}
	}

	public ulong GetDelayWorldTime(int _idHash)
	{
		ChunkAreaBiomeSpawnData.CountsAndTime countsAndTime;
		this.entitesSpawned.TryGetValue(_idHash, out countsAndTime);
		return countsAndTime.delayWorldTime;
	}

	public void ResetRespawn(int _idHash, World _world, int _maxCount)
	{
		BiomeDefinition biome = _world.Biomes.GetBiome(this.biomeId);
		if (biome == null)
		{
			return;
		}
		BiomeSpawnEntityGroupList biomeSpawnEntityGroupList = BiomeSpawningClass.list[biome.m_sBiomeName];
		if (biomeSpawnEntityGroupList == null)
		{
			return;
		}
		BiomeSpawnEntityGroupData biomeSpawnEntityGroupData = biomeSpawnEntityGroupList.Find(_idHash);
		if (biomeSpawnEntityGroupData == null)
		{
			return;
		}
		ChunkAreaBiomeSpawnData.CountsAndTime countsAndTime;
		this.entitesSpawned.TryGetValue(_idHash, out countsAndTime);
		int num = ((biomeSpawnEntityGroupData.respawnDelayInWorldTime.Length == 1) ? 0 : (biomeSpawnEntityGroupData.isAnimal ? ChunkAreaBiomeSpawnData.RespawnDelayIndexAnimals : ChunkAreaBiomeSpawnData.RespawnDelayIndexEnemies));
		if (_maxCount > 0 && ChunkAreaBiomeSpawnData.RespawnCountOverride > 0 && EntityGroups.IsEnemyGroup(biomeSpawnEntityGroupData.entityGroupName))
		{
			_maxCount = ChunkAreaBiomeSpawnData.RespawnCountOverride;
		}
		countsAndTime.delayWorldTime = _world.worldTime + (ulong)((float)biomeSpawnEntityGroupData.respawnDelayInWorldTime[num] * _world.RandomRange(0.9f, 1.1f));
		countsAndTime.maxCount = _maxCount;
		this.entitesSpawned[_idHash] = countsAndTime;
		this.chunk.isModified = true;
	}

	public bool DelayAllEnemySpawningUntil(ulong _worldTime, WorldBiomes _worldBiomes)
	{
		bool flag = false;
		BiomeDefinition biome = _worldBiomes.GetBiome(this.biomeId);
		if (biome == null)
		{
			return false;
		}
		BiomeSpawnEntityGroupList biomeSpawnEntityGroupList = BiomeSpawningClass.list[biome.m_sBiomeName];
		if (biomeSpawnEntityGroupList == null)
		{
			return false;
		}
		Dictionary<int, ChunkAreaBiomeSpawnData.CountsAndTime> dictionary = new Dictionary<int, ChunkAreaBiomeSpawnData.CountsAndTime>();
		foreach (KeyValuePair<int, ChunkAreaBiomeSpawnData.CountsAndTime> keyValuePair in this.entitesSpawned)
		{
			BiomeSpawnEntityGroupData biomeSpawnEntityGroupData = biomeSpawnEntityGroupList.Find(keyValuePair.Key);
			if (biomeSpawnEntityGroupData != null && EntityGroups.IsEnemyGroup(biomeSpawnEntityGroupData.entityGroupName))
			{
				ChunkAreaBiomeSpawnData.CountsAndTime value = keyValuePair.Value;
				bool flag2 = false;
				if (value.delayWorldTime < _worldTime)
				{
					value.delayWorldTime = _worldTime;
					flag2 = true;
				}
				if (value.maxCount > 0)
				{
					value.maxCount = 0;
					flag2 = true;
				}
				if (flag2)
				{
					dictionary[keyValuePair.Key] = value;
					flag = true;
				}
			}
		}
		foreach (KeyValuePair<int, ChunkAreaBiomeSpawnData.CountsAndTime> keyValuePair2 in dictionary)
		{
			this.entitesSpawned[keyValuePair2.Key] = keyValuePair2.Value;
		}
		for (int i = 0; i < biomeSpawnEntityGroupList.list.Count; i++)
		{
			BiomeSpawnEntityGroupData biomeSpawnEntityGroupData2 = biomeSpawnEntityGroupList.list[i];
			if (EntityGroups.IsEnemyGroup(biomeSpawnEntityGroupData2.entityGroupName) && !this.entitesSpawned.ContainsKey(biomeSpawnEntityGroupData2.idHash))
			{
				this.entitesSpawned[biomeSpawnEntityGroupData2.idHash] = new ChunkAreaBiomeSpawnData.CountsAndTime(0, 0, _worldTime);
				flag = true;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void read(BinaryReader _br)
	{
		int num = (int)_br.ReadByte();
		this.entitesSpawned.Clear();
		int num2 = (int)_br.ReadByte();
		for (int i = 0; i < num2; i++)
		{
			if (num <= 1)
			{
				_br.ReadString();
				_br.ReadUInt16();
				_br.ReadUInt64();
			}
			else
			{
				int num3 = _br.ReadInt32();
				int num4 = (int)_br.ReadUInt16();
				ChunkAreaBiomeSpawnData.CountsAndTime countsAndTime;
				countsAndTime.count = num4 & 255;
				countsAndTime.maxCount = num4 >> 8;
				countsAndTime.delayWorldTime = _br.ReadUInt64();
				this.entitesSpawned[num3] = countsAndTime;
			}
		}
	}

	public void BeforeWrite()
	{
		using (PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true))
		{
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				this.write(pooledBinaryWriter);
			}
			this.ccd.data = pooledExpandableMemoryStream.ToArray();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void write(BinaryWriter _bw)
	{
		_bw.Write(2);
		int num = 0;
		int num2 = Utils.FastMin(this.entitesSpawned.Count, 255);
		_bw.Write((byte)num2);
		foreach (KeyValuePair<int, ChunkAreaBiomeSpawnData.CountsAndTime> keyValuePair in this.entitesSpawned)
		{
			_bw.Write(keyValuePair.Key);
			_bw.Write((ushort)((keyValuePair.Value.maxCount << 8) | keyValuePair.Value.count));
			_bw.Write(keyValuePair.Value.delayWorldTime);
			if (++num >= num2)
			{
				break;
			}
		}
	}

	public override string ToString()
	{
		World world = GameManager.Instance.World;
		ulong worldTime = world.worldTime;
		BiomeDefinition biome = world.Biomes.GetBiome(this.biomeId);
		if (biome == null)
		{
			return "biome? " + this.biomeId.ToString();
		}
		BiomeSpawnEntityGroupList biomeSpawnEntityGroupList = BiomeSpawningClass.list[biome.m_sBiomeName];
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<int, ChunkAreaBiomeSpawnData.CountsAndTime> keyValuePair in this.entitesSpawned)
		{
			string text = "?";
			if (biomeSpawnEntityGroupList != null)
			{
				BiomeSpawnEntityGroupData biomeSpawnEntityGroupData = biomeSpawnEntityGroupList.Find(keyValuePair.Key);
				if (biomeSpawnEntityGroupData != null)
				{
					text = biomeSpawnEntityGroupData.entityGroupName + " " + biomeSpawnEntityGroupData.daytime.ToString();
				}
			}
			ulong num = keyValuePair.Value.delayWorldTime - worldTime;
			if (num < 0UL)
			{
				num = 0UL;
			}
			stringBuilder.Append(string.Format("{0} #{1}/{2} {3}, ", new object[]
			{
				text,
				keyValuePair.Value.count,
				keyValuePair.Value.maxCount,
				GameUtils.WorldTimeDeltaToString(num)
			}));
		}
		return string.Format("biomeId {0}, XZ {1} {2}: {3}", new object[]
		{
			this.biomeId,
			this.area.x.ToCultureInvariantString("0"),
			this.area.y.ToCultureInvariantString("0"),
			stringBuilder.ToString()
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cCurrentSaveVersion = 2;

	public byte biomeId;

	public Rect area;

	public Chunk chunk;

	public bool checkedPOITags;

	public FastTags<TagGroup.Poi> poiTags;

	public int groupsEnabledFlags;

	public static int RespawnDelayIndexEnemies;

	public static int RespawnDelayIndexAnimals;

	public static int RespawnCountOverride;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkCustomData ccd;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, ChunkAreaBiomeSpawnData.CountsAndTime> entitesSpawned = new Dictionary<int, ChunkAreaBiomeSpawnData.CountsAndTime>();

	[PublicizedFrom(EAccessModifier.Private)]
	public struct CountsAndTime
	{
		public CountsAndTime(int _count, int _maxCount, ulong _delayWorldTime)
		{
			this.count = _count;
			this.maxCount = _maxCount;
			this.delayWorldTime = _delayWorldTime;
		}

		public override string ToString()
		{
			return string.Format("cnt {0}, maxCnt {1}, wtime {2}", this.count, this.maxCount, this.delayWorldTime);
		}

		public int count;

		public int maxCount;

		public ulong delayWorldTime;
	}
}
