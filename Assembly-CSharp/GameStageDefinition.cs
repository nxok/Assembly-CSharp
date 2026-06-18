using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameStageDefinition
{
	public GameStageDefinition(string _name)
	{
		this.name = _name;
	}

	public static void AddGameStage(GameStageDefinition gameStage)
	{
		gameStage.SortStages();
		GameStageDefinition.gameStages.Add(gameStage.name, gameStage);
	}

	public static GameStageDefinition GetGameStage(string name)
	{
		return GameStageDefinition.gameStages[name];
	}

	public static bool TryGetGameStage(string name, out GameStageDefinition definition)
	{
		return GameStageDefinition.gameStages.TryGetValue(name, out definition);
	}

	public static void Clear()
	{
		GameStageDefinition.gameStages.Clear();
	}

	public void AddStage(GameStageDefinition.Stage stage)
	{
		this.stages.Add(stage);
	}

	public void SortStages()
	{
		this.stages.Sort((GameStageDefinition.Stage x, GameStageDefinition.Stage y) => x.stageNum.CompareTo(y.stageNum));
	}

	public GameStageDefinition.Stage GetStage(int stage)
	{
		if (this.stages.Count < 1)
		{
			return null;
		}
		if (stage < this.stages[0].stageNum)
		{
			return null;
		}
		int num = GameStageDefinition.GetBoundIndex<GameStageDefinition.Stage>(this.stages, (GameStageDefinition.Stage s) => s.stageNum <= stage);
		num = Mathf.Clamp(num, 0, this.stages.Count - 1);
		return this.stages[num];
	}

	public static int GetBoundIndex<T>(IList<T> sortedList, Func<T, bool> f)
	{
		int num = 0;
		int num2 = sortedList.Count;
		while (num + 1 < num2)
		{
			int num3 = (num + num2) / 2;
			if (f(sortedList[num3]))
			{
				num = num3;
			}
			else
			{
				num2 = num3;
			}
		}
		if (num2 < sortedList.Count && f(sortedList[num2]))
		{
			num = num2;
		}
		return num;
	}

	public static int CalcPartyLevel(List<int> playerGameStages)
	{
		float num = 0f;
		playerGameStages.Sort();
		float num2 = GameStageDefinition.StartingWeight;
		for (int i = playerGameStages.Count - 1; i >= 0; i--)
		{
			num += (float)playerGameStages[i] * num2;
			num2 *= GameStageDefinition.DiminishingReturns;
		}
		return Mathf.FloorToInt(num);
	}

	public static int CalcGameStageAround(EntityPlayer player)
	{
		List<EntityPlayer> list = new List<EntityPlayer>();
		player.world.GetPlayersAround(player.position, 100f, list);
		List<int> list2 = new List<int>();
		for (int i = 0; i < list.Count; i++)
		{
			EntityPlayer entityPlayer = list[i];
			if (entityPlayer.prefab == player.prefab)
			{
				list2.Add(entityPlayer.gameStage);
			}
		}
		return GameStageDefinition.CalcPartyLevel(list2);
	}

	public static float DifficultyBonus = 1f;

	public static float StartingWeight = 1f;

	public static float DiminishingReturns = 0.5f;

	public static long DaysAliveChangeWhenKilled = 2L;

	public static int LootBonusEvery;

	public static int LootBonusMaxCount;

	public static float LootBonusScale;

	public static int LootWanderingBonusEvery;

	public static float LootWanderingBonusScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, GameStageDefinition> gameStages = new Dictionary<string, GameStageDefinition>();

	public string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameStageDefinition.Stage> stages = new List<GameStageDefinition.Stage>();

	public class Stage
	{
		public Stage(int _stageNum)
		{
			this.stageNum = _stageNum;
		}

		public void AddSpawnGroup(GameStageDefinition.SpawnGroup spawn)
		{
			this.spawnGroups.Add(spawn);
		}

		public GameStageDefinition.SpawnGroup GetSpawnGroup(int index)
		{
			if (index >= 0 && index < this.spawnGroups.Count)
			{
				return this.spawnGroups[index];
			}
			return null;
		}

		public int Count
		{
			get
			{
				return this.spawnGroups.Count;
			}
		}

		public readonly int stageNum;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<GameStageDefinition.SpawnGroup> spawnGroups = new List<GameStageDefinition.SpawnGroup>();
	}

	public class SpawnGroup
	{
		public SpawnGroup(string _groupName, int _spawnCount, int _maxAlive, int _interval, int _duration)
		{
			this.groupName = _groupName;
			this.spawnCount = (ushort)_spawnCount;
			this.maxAlive = (ushort)_maxAlive;
			this.interval = (ushort)_interval;
			this.duration = (ushort)_duration;
		}

		public readonly string groupName;

		public readonly ushort spawnCount;

		public readonly ushort maxAlive;

		public readonly ushort interval;

		public readonly ushort duration;
	}
}
