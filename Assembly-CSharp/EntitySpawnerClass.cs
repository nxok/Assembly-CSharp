using System;
using System.Globalization;
using UnityEngine;

public class EntitySpawnerClass
{
	public void Init()
	{
		if (!this.Properties.Values.ContainsKey(EntitySpawnerClass.PropEntityGroupName))
		{
			throw new Exception(string.Concat(new string[]
			{
				"Mandatory property '",
				EntitySpawnerClass.PropEntityGroupName,
				"' missing in entityspawnerclass '",
				this.name,
				"'"
			}));
		}
		this.entityGroupName = this.Properties.Values[EntitySpawnerClass.PropEntityGroupName];
		if (!EntityGroups.list.ContainsKey(this.entityGroupName))
		{
			throw new Exception("Entity spawner '" + this.name + "' contains invalid group " + this.entityGroupName);
		}
		if (this.Properties.Values.ContainsKey(EntitySpawnerClass.PropStartSound))
		{
			this.startSound = this.Properties.Values[EntitySpawnerClass.PropStartSound];
		}
		if (this.Properties.Values.ContainsKey(EntitySpawnerClass.PropStartText))
		{
			this.startText = this.Properties.Values[EntitySpawnerClass.PropStartText];
		}
		this.spawnAtTimeOfDay = EDaytime.Any;
		if (this.Properties.Values.ContainsKey(EntitySpawnerClass.PropTime))
		{
			this.spawnAtTimeOfDay = EnumUtils.Parse<EDaytime>(this.Properties.Values[EntitySpawnerClass.PropTime], false);
		}
		this.delayBetweenSpawns = 0f;
		if (this.Properties.Values.ContainsKey(EntitySpawnerClass.PropDelayBetweenSpawns))
		{
			this.delayBetweenSpawns = StringParsers.ParseFloat(this.Properties.Values[EntitySpawnerClass.PropDelayBetweenSpawns], 0, -1, NumberStyles.Any);
		}
		this.totalAlive = 1;
		if (this.Properties.Values.ContainsKey(EntitySpawnerClass.PropTotalAlive))
		{
			this.totalAlive = int.Parse(this.Properties.Values[EntitySpawnerClass.PropTotalAlive]);
		}
		this.totalPerWaveMin = 1;
		this.totalPerWaveMax = 1;
		if (this.Properties.Values.ContainsKey(EntitySpawnerClass.PropTotalPerWave))
		{
			StringParsers.ParseMinMaxCount(this.Properties.Values[EntitySpawnerClass.PropTotalPerWave], out this.totalPerWaveMin, out this.totalPerWaveMax);
		}
		this.delayToNextWave = 1f;
		if (this.Properties.Values.ContainsKey(EntitySpawnerClass.PropDelayToNextWave))
		{
			this.delayToNextWave = StringParsers.ParseFloat(this.Properties.Values[EntitySpawnerClass.PropDelayToNextWave], 0, -1, NumberStyles.Any);
		}
		this.bAttackPlayerImmediately = false;
		if (this.Properties.Values.ContainsKey(EntitySpawnerClass.PropAttackPlayerAtOnce))
		{
			this.bAttackPlayerImmediately = StringParsers.ParseBool(this.Properties.Values[EntitySpawnerClass.PropAttackPlayerAtOnce], 0, -1, true);
		}
		if (this.Properties.Values.ContainsKey(EntitySpawnerClass.PropNumberOfWaves))
		{
			this.numberOfWaves = int.Parse(this.Properties.Values[EntitySpawnerClass.PropNumberOfWaves]);
		}
		this.bTerritorial = false;
		if (this.Properties.Values.ContainsKey(EntitySpawnerClass.PropTerritorial))
		{
			this.bTerritorial = StringParsers.ParseBool(this.Properties.Values[EntitySpawnerClass.PropTerritorial], 0, -1, true);
		}
		this.territorialRange = 10;
		if (this.Properties.Values.ContainsKey(EntitySpawnerClass.PropTerritorialRange))
		{
			this.territorialRange = int.Parse(this.Properties.Values[EntitySpawnerClass.PropTerritorialRange]);
		}
		this.bSpawnOnGround = true;
		if (this.Properties.Values.ContainsKey(EntitySpawnerClass.PropSpawnOnGround))
		{
			this.bSpawnOnGround = StringParsers.ParseBool(this.Properties.Values[EntitySpawnerClass.PropSpawnOnGround], 0, -1, true);
		}
		this.bIgnoreTrigger = false;
		if (this.Properties.Values.ContainsKey(EntitySpawnerClass.PropIgnoreTrigger))
		{
			this.bIgnoreTrigger = StringParsers.ParseBool(this.Properties.Values[EntitySpawnerClass.PropIgnoreTrigger], 0, -1, true);
		}
		this.bPropResetToday = true;
		if (this.Properties.Values.ContainsKey(EntitySpawnerClass.PropResetToday))
		{
			this.bPropResetToday = StringParsers.ParseBool(this.Properties.Values[EntitySpawnerClass.PropResetToday], 0, -1, true);
		}
		this.daysToRespawnIfPlayerLeft = 0;
		if (this.Properties.Values.ContainsKey(EntitySpawnerClass.PropDaysToRespawnIfPlayerLeft))
		{
			this.daysToRespawnIfPlayerLeft = Mathf.RoundToInt(StringParsers.ParseFloat(this.Properties.Values[EntitySpawnerClass.PropDaysToRespawnIfPlayerLeft], 0, -1, NumberStyles.Any));
		}
		if (EntitySpawnerClass.DefaultClassName == null)
		{
			EntitySpawnerClass.DefaultClassName = this;
		}
	}

	public static void Cleanup()
	{
		EntitySpawnerClass.list.Clear();
	}

	public static string PropStartSound = "StartSound";

	public static string PropStartText = "StartText";

	public static string PropEntityGroupName = "EntityGroupName";

	public static string PropTime = "Time";

	public static string PropDelayBetweenSpawns = "DelayBetweenSpawns";

	public static string PropTotalAlive = "TotalAlive";

	public static string PropTotalPerWave = "TotalPerWave";

	public static string PropDelayToNextWave = "DelayToNextWave";

	public static string PropAttackPlayerAtOnce = "AttackPlayerAtOnce";

	public static string PropNumberOfWaves = "NumberOfWaves";

	public static string PropTerritorial = "Territorial";

	public static string PropTerritorialRange = "TerritorialRange";

	public static string PropSpawnOnGround = "SpawnOnGround";

	public static string PropIgnoreTrigger = "IgnoreTrigger";

	public static string PropResetToday = "ResetToday";

	public static string PropDaysToRespawnIfPlayerLeft = "DaysToRespawnIfPlayerLeft";

	public static DictionarySave<string, EntitySpawnerClassForDay> list = new DictionarySave<string, EntitySpawnerClassForDay>();

	public static EntitySpawnerClass DefaultClassName;

	public DynamicProperties Properties = new DynamicProperties();

	public string name;

	public string entityGroupName;

	public EDaytime spawnAtTimeOfDay;

	public float delayBetweenSpawns;

	public int totalAlive;

	public float delayToNextWave;

	public int totalPerWaveMin;

	public int totalPerWaveMax;

	public int numberOfWaves;

	public bool bAttackPlayerImmediately;

	public bool bSpawnOnGround;

	public bool bIgnoreTrigger;

	public bool bTerritorial;

	public int territorialRange;

	public bool bPropResetToday;

	public int daysToRespawnIfPlayerLeft;

	public string startSound;

	public string startText;
}
