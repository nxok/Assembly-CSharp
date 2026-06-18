using System;
using System.Globalization;
using System.IO;
using SandboxOptions;

public class GameStats
{
	public static event GameStats.OnChangedDelegate OnChangedDelegates;

	[PublicizedFrom(EAccessModifier.Private)]
	public void initPropertyDecl()
	{
		this.propertyList = new GameStats.PropertyDecl[]
		{
			new GameStats.PropertyDecl(EnumGameStats.GameState, true, GameStats.EnumType.Int, 0),
			new GameStats.PropertyDecl(EnumGameStats.GameModeId, true, GameStats.EnumType.Int, 0),
			new GameStats.PropertyDecl(EnumGameStats.TimeLimitActive, true, GameStats.EnumType.Bool, false),
			new GameStats.PropertyDecl(EnumGameStats.TimeLimitThisRound, true, GameStats.EnumType.Int, 0),
			new GameStats.PropertyDecl(EnumGameStats.FragLimitActive, true, GameStats.EnumType.Bool, false),
			new GameStats.PropertyDecl(EnumGameStats.FragLimitThisRound, true, GameStats.EnumType.Int, 0),
			new GameStats.PropertyDecl(EnumGameStats.DayLimitActive, true, GameStats.EnumType.Bool, false),
			new GameStats.PropertyDecl(EnumGameStats.DayLimitThisRound, true, GameStats.EnumType.Int, 0),
			new GameStats.PropertyDecl(EnumGameStats.ShowWindow, true, GameStats.EnumType.String, string.Empty),
			new GameStats.PropertyDecl(EnumGameStats.LoadScene, true, GameStats.EnumType.String, string.Empty),
			new GameStats.PropertyDecl(EnumGameStats.CurrentRoundIx, true, GameStats.EnumType.Int, 0),
			new GameStats.PropertyDecl(EnumGameStats.ShowAllPlayersOnMap, true, GameStats.EnumType.Bool, false),
			new GameStats.PropertyDecl(EnumGameStats.ShowFriendPlayerOnMap, true, GameStats.EnumType.Bool, true),
			new GameStats.PropertyDecl(EnumGameStats.ShowSpawnWindow, true, GameStats.EnumType.Bool, false),
			new GameStats.PropertyDecl(EnumGameStats.IsSpawnNearOtherPlayer, true, GameStats.EnumType.Bool, false),
			new GameStats.PropertyDecl(EnumGameStats.TimeOfDayIncPerSec, true, GameStats.EnumType.Int, 20),
			new GameStats.PropertyDecl(EnumGameStats.IsCreativeMenuEnabled, true, GameStats.EnumType.Bool, false),
			new GameStats.PropertyDecl(EnumGameStats.IsTeleportEnabled, true, GameStats.EnumType.Bool, false),
			new GameStats.PropertyDecl(EnumGameStats.IsFlyingEnabled, true, GameStats.EnumType.Bool, false),
			new GameStats.PropertyDecl(EnumGameStats.IsPlayerDamageEnabled, true, GameStats.EnumType.Bool, true),
			new GameStats.PropertyDecl(EnumGameStats.IsPlayerCollisionEnabled, true, GameStats.EnumType.Bool, true),
			new GameStats.PropertyDecl(EnumGameStats.IsSaveSupplyCrates, false, GameStats.EnumType.Bool, true),
			new GameStats.PropertyDecl(EnumGameStats.IsResetMapOnRestart, false, GameStats.EnumType.Bool, false),
			new GameStats.PropertyDecl(EnumGameStats.IsSpawnEnemies, true, GameStats.EnumType.Bool, true),
			new GameStats.PropertyDecl(EnumGameStats.PlayerKillingMode, true, GameStats.EnumType.Int, EnumPlayerKillingMode.KillStrangersOnly),
			new GameStats.PropertyDecl(EnumGameStats.ScorePlayerKillMultiplier, true, GameStats.EnumType.Int, 1),
			new GameStats.PropertyDecl(EnumGameStats.ScoreZombieKillMultiplier, true, GameStats.EnumType.Int, 1),
			new GameStats.PropertyDecl(EnumGameStats.ScoreDiedMultiplier, true, GameStats.EnumType.Int, -5),
			new GameStats.PropertyDecl(EnumGameStats.EnemyCount, false, GameStats.EnumType.Int, 0),
			new GameStats.PropertyDecl(EnumGameStats.AnimalCount, false, GameStats.EnumType.Int, 0),
			new GameStats.PropertyDecl(EnumGameStats.IsVersionCheckDone, false, GameStats.EnumType.Bool, false),
			new GameStats.PropertyDecl(EnumGameStats.ZombieHordeMeter, false, GameStats.EnumType.Bool, false),
			new GameStats.PropertyDecl(EnumGameStats.DropOnDeath, true, GameStats.EnumType.Int, 1),
			new GameStats.PropertyDecl(EnumGameStats.DropOnQuit, true, GameStats.EnumType.Int, 0),
			new GameStats.PropertyDecl(EnumGameStats.GameDifficulty, true, GameStats.EnumType.Int, 2),
			new GameStats.PropertyDecl(EnumGameStats.BloodMoonEnemyCount, true, GameStats.EnumType.Int, 8),
			new GameStats.PropertyDecl(EnumGameStats.EnemySpawnMode, true, GameStats.EnumType.Bool, true),
			new GameStats.PropertyDecl(EnumGameStats.EnemyDifficulty, true, GameStats.EnumType.Int, EnumEnemyDifficulty.Normal),
			new GameStats.PropertyDecl(EnumGameStats.DayLightLength, true, GameStats.EnumType.Int, 18),
			new GameStats.PropertyDecl(EnumGameStats.LandClaimCount, true, GameStats.EnumType.Int, 5),
			new GameStats.PropertyDecl(EnumGameStats.LandClaimSize, true, GameStats.EnumType.Int, 41),
			new GameStats.PropertyDecl(EnumGameStats.LandClaimDeadZone, true, GameStats.EnumType.Int, 30),
			new GameStats.PropertyDecl(EnumGameStats.LandClaimExpiryTime, true, GameStats.EnumType.Int, 3),
			new GameStats.PropertyDecl(EnumGameStats.LandClaimDecayMode, true, GameStats.EnumType.Int, 0),
			new GameStats.PropertyDecl(EnumGameStats.LandClaimOnlineDurabilityModifier, true, GameStats.EnumType.Int, 32),
			new GameStats.PropertyDecl(EnumGameStats.LandClaimOfflineDurabilityModifier, true, GameStats.EnumType.Int, 32),
			new GameStats.PropertyDecl(EnumGameStats.LandClaimOfflineDelay, true, GameStats.EnumType.Int, 0),
			new GameStats.PropertyDecl(EnumGameStats.BedrollExpiryTime, true, GameStats.EnumType.Int, 45),
			new GameStats.PropertyDecl(EnumGameStats.AirDropFrequency, true, GameStats.EnumType.Int, 0),
			new GameStats.PropertyDecl(EnumGameStats.GlobalMessageToShow, false, GameStats.EnumType.String, ""),
			new GameStats.PropertyDecl(EnumGameStats.AirDropMarker, true, GameStats.EnumType.Bool, true),
			new GameStats.PropertyDecl(EnumGameStats.PartySharedKillRange, true, GameStats.EnumType.Int, 100),
			new GameStats.PropertyDecl(EnumGameStats.ChunkStabilityEnabled, false, GameStats.EnumType.Bool, true),
			new GameStats.PropertyDecl(EnumGameStats.AutoParty, true, GameStats.EnumType.Bool, false),
			new GameStats.PropertyDecl(EnumGameStats.OptionsPOICulling, true, GameStats.EnumType.Int, 0),
			new GameStats.PropertyDecl(EnumGameStats.BloodMoonDay, true, GameStats.EnumType.Int, 0),
			new GameStats.PropertyDecl(EnumGameStats.BlockDamagePlayer, true, GameStats.EnumType.Int, 100),
			new GameStats.PropertyDecl(EnumGameStats.XPMultiplier, true, GameStats.EnumType.Int, 100),
			new GameStats.PropertyDecl(EnumGameStats.BloodMoonWarning, true, GameStats.EnumType.Int, 8),
			new GameStats.PropertyDecl(EnumGameStats.AllowedViewDistance, false, GameStats.EnumType.Int, 12),
			new GameStats.PropertyDecl(EnumGameStats.TwitchBloodMoonAllowed, true, GameStats.EnumType.Bool, true),
			new GameStats.PropertyDecl(EnumGameStats.DeathPenalty, true, GameStats.EnumType.Int, EnumDeathPenalty.XPOnly),
			new GameStats.PropertyDecl(EnumGameStats.QuestProgressionDailyLimit, true, GameStats.EnumType.Int, 4),
			new GameStats.PropertyDecl(EnumGameStats.BiomeProgression, true, GameStats.EnumType.Bool, true),
			new GameStats.PropertyDecl(EnumGameStats.StormFreq, true, GameStats.EnumType.Int, 0),
			new GameStats.PropertyDecl(EnumGameStats.CameraRestrictionMode, true, GameStats.EnumType.Int, 0),
			new GameStats.PropertyDecl(EnumGameStats.JarRefund, true, GameStats.EnumType.Int, 60),
			new GameStats.PropertyDecl(EnumGameStats.SandboxPreset, true, GameStats.EnumType.String, ""),
			new GameStats.PropertyDecl(EnumGameStats.SandboxCode, true, GameStats.EnumType.String, ""),
			new GameStats.PropertyDecl(EnumGameStats.DayNightLength, true, GameStats.EnumType.Int, 60),
			new GameStats.PropertyDecl(EnumGameStats.BlockDamageAI, true, GameStats.EnumType.Int, 100),
			new GameStats.PropertyDecl(EnumGameStats.BlockDamageAIBM, true, GameStats.EnumType.Int, 100),
			new GameStats.PropertyDecl(EnumGameStats.LootAbundance, true, GameStats.EnumType.Int, 100),
			new GameStats.PropertyDecl(EnumGameStats.LootRespawnDays, true, GameStats.EnumType.Int, 7),
			new GameStats.PropertyDecl(EnumGameStats.GlobalGSModifier, true, GameStats.EnumType.Int, 100),
			new GameStats.PropertyDecl(EnumGameStats.BiomeGSModifier, true, GameStats.EnumType.Int, 100),
			new GameStats.PropertyDecl(EnumGameStats.GlobalLSModifier, true, GameStats.EnumType.Int, 100),
			new GameStats.PropertyDecl(EnumGameStats.BiomeLSModifier, true, GameStats.EnumType.Int, 100)
		};
	}

	public static GameStats Instance
	{
		get
		{
			if (GameStats.m_Instance == null)
			{
				GameStats.m_Instance = new GameStats();
				GameStats.m_Instance.initPropertyDecl();
				GameStats.m_Instance.initDefault();
			}
			return GameStats.m_Instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initDefault()
	{
		foreach (GameStats.PropertyDecl propertyDecl in this.propertyList)
		{
			int name = (int)propertyDecl.name;
			this.propertyValues[name] = propertyDecl.defaultValue;
		}
	}

	public void Write(BinaryWriter _write)
	{
		foreach (GameStats.PropertyDecl propertyDecl in this.propertyList)
		{
			if (propertyDecl.bPersistent)
			{
				switch (propertyDecl.type)
				{
				case GameStats.EnumType.Int:
					_write.Write(GameStats.GetInt(propertyDecl.name));
					break;
				case GameStats.EnumType.Float:
					_write.Write(GameStats.GetFloat(propertyDecl.name));
					break;
				case GameStats.EnumType.String:
					_write.Write(GameStats.GetString(propertyDecl.name));
					break;
				case GameStats.EnumType.Bool:
					_write.Write(GameStats.GetBool(propertyDecl.name));
					break;
				case GameStats.EnumType.Binary:
					_write.Write(Utils.ToBase64(GameStats.GetString(propertyDecl.name)));
					break;
				}
			}
		}
	}

	public void Read(BinaryReader _reader)
	{
		foreach (GameStats.PropertyDecl propertyDecl in this.propertyList)
		{
			if (propertyDecl.bPersistent)
			{
				int name = (int)propertyDecl.name;
				switch (propertyDecl.type)
				{
				case GameStats.EnumType.Int:
					this.propertyValues[name] = _reader.ReadInt32();
					break;
				case GameStats.EnumType.Float:
					this.propertyValues[name] = _reader.ReadSingle();
					break;
				case GameStats.EnumType.String:
					this.propertyValues[name] = _reader.ReadString();
					break;
				case GameStats.EnumType.Bool:
					this.propertyValues[name] = _reader.ReadBoolean();
					break;
				case GameStats.EnumType.Binary:
					this.propertyValues[name] = Utils.FromBase64(_reader.ReadString());
					break;
				}
			}
		}
	}

	public static object Parse(EnumGameStats _enum, string _val)
	{
		int num = GameStats.find(_enum);
		if (num == -1)
		{
			return null;
		}
		switch (GameStats.Instance.propertyList[num].type)
		{
		case GameStats.EnumType.Int:
			return int.Parse(_val);
		case GameStats.EnumType.Float:
			return StringParsers.ParseFloat(_val, 0, -1, NumberStyles.Any);
		case GameStats.EnumType.String:
			return _val;
		case GameStats.EnumType.Bool:
			return StringParsers.ParseBool(_val, 0, -1, true);
		case GameStats.EnumType.Binary:
			return _val;
		default:
			return null;
		}
	}

	public static string GetString(EnumGameStats _eProperty)
	{
		string text;
		try
		{
			text = (string)GameStats.Instance.propertyValues[(int)_eProperty];
		}
		catch (InvalidCastException)
		{
			Log.Error("GetString: InvalidCastException " + _eProperty.ToStringCached<EnumGameStats>());
			text = string.Empty;
		}
		return text;
	}

	public static float GetFloat(EnumGameStats _eProperty)
	{
		float num;
		try
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				BaseSandboxOption baseSandboxOption = GameStats.Instance.sandboxReferences[(int)_eProperty];
				if (baseSandboxOption != null)
				{
					return baseSandboxOption.GetFloatValue();
				}
			}
			num = (float)GameStats.Instance.propertyValues[(int)_eProperty];
		}
		catch (InvalidCastException)
		{
			Log.Error("GetFloat: InvalidCastException " + _eProperty.ToStringCached<EnumGameStats>());
			num = 0f;
		}
		return num;
	}

	public static int GetInt(EnumGameStats _eProperty)
	{
		int num;
		try
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				BaseSandboxOption baseSandboxOption = GameStats.Instance.sandboxReferences[(int)_eProperty];
				if (baseSandboxOption != null)
				{
					return baseSandboxOption.GetIntValue();
				}
			}
			num = (int)GameStats.Instance.propertyValues[(int)_eProperty];
		}
		catch (InvalidCastException)
		{
			Log.Error("GetInt: InvalidCastException " + _eProperty.ToStringCached<EnumGameStats>());
			num = 0;
		}
		return num;
	}

	public static bool GetBool(EnumGameStats _eProperty)
	{
		bool flag;
		try
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				BaseSandboxOption baseSandboxOption = GameStats.Instance.sandboxReferences[(int)_eProperty];
				if (baseSandboxOption != null)
				{
					return baseSandboxOption.GetBoolValue();
				}
			}
			flag = (bool)GameStats.Instance.propertyValues[(int)_eProperty];
		}
		catch (InvalidCastException)
		{
			Log.Error("GetBool: InvalidCastException " + _eProperty.ToStringCached<EnumGameStats>());
			flag = false;
		}
		return flag;
	}

	public static void SetupSandboxReferences()
	{
		GameStats.Instance.sandboxReferences = new BaseSandboxOption[81];
		for (int i = 0; i < GameStats.Instance.propertyList.Length; i++)
		{
			GameStats.PropertyDecl propertyDecl = GameStats.Instance.propertyList[i];
			global::SandboxOptions.SandboxOptions sandboxOptions = global::SandboxOptions.SandboxOptions.Max;
			if (Enum.TryParse<global::SandboxOptions.SandboxOptions>(propertyDecl.name.ToString(), out sandboxOptions))
			{
				GameStats.Instance.sandboxReferences[(int)propertyDecl.name] = SandboxOptionManager.GetOption(sandboxOptions);
			}
			else
			{
				EnumGameStats name = propertyDecl.name;
				if (name != EnumGameStats.BlockDamagePlayer)
				{
					if (name == EnumGameStats.LootAbundance)
					{
						GameStats.Instance.sandboxReferences[(int)propertyDecl.name] = SandboxOptionManager.GetOption(global::SandboxOptions.SandboxOptions.GlobalLootCount);
					}
				}
				else
				{
					GameStats.Instance.sandboxReferences[(int)propertyDecl.name] = SandboxOptionManager.GetOption(global::SandboxOptions.SandboxOptions.BlockDamage);
				}
			}
		}
	}

	public static object GetObject(EnumGameStats _eProperty)
	{
		return GameStats.Instance.propertyValues[(int)_eProperty];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int find(EnumGameStats _eProperty)
	{
		for (int i = 0; i < GameStats.Instance.propertyList.Length; i++)
		{
			if (GameStats.Instance.propertyList[i].name == _eProperty)
			{
				return i;
			}
		}
		return -1;
	}

	public static void SetObject(EnumGameStats _eProperty, object _value)
	{
		GameStats.Instance.propertyValues[(int)_eProperty] = _value;
		if (GameStats.OnChangedDelegates != null)
		{
			GameStats.OnChangedDelegates(_eProperty, _value);
		}
	}

	public static void Set(EnumGameStats _eProperty, int _value)
	{
		GameStats.SetObject(_eProperty, _value);
	}

	public static void Set(EnumGameStats _eProperty, float _value)
	{
		GameStats.SetObject(_eProperty, _value);
	}

	public static void Set(EnumGameStats _eProperty, string _value)
	{
		GameStats.SetObject(_eProperty, _value);
	}

	public static void Set(EnumGameStats _eProperty, bool _value)
	{
		GameStats.SetObject(_eProperty, _value);
	}

	public static bool IsDefault(EnumGameStats _eProperty)
	{
		return GameStats.Instance.propertyValues[(int)_eProperty] != null && GameStats.Instance.propertyValues[(int)_eProperty].Equals(GameStats.Instance.propertyList[(int)_eProperty].defaultValue);
	}

	public static GameStats.EnumType? GetStatType(EnumGameStats _eProperty)
	{
		foreach (GameStats.PropertyDecl propertyDecl in GameStats.Instance.propertyList)
		{
			if (propertyDecl.name == _eProperty)
			{
				return new GameStats.EnumType?(propertyDecl.type);
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public GameStats.PropertyDecl[] propertyList;

	[PublicizedFrom(EAccessModifier.Private)]
	public object[] propertyValues = new object[81];

	[PublicizedFrom(EAccessModifier.Private)]
	public BaseSandboxOption[] sandboxReferences;

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameStats m_Instance;

	public delegate void OnChangedDelegate(EnumGameStats _gameState, object _newValue);

	public enum EnumType
	{
		Int,
		Float,
		String,
		Bool,
		Binary
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct PropertyDecl
	{
		public PropertyDecl(EnumGameStats _name, bool _bPersistent, GameStats.EnumType _type, object _defaultValue)
		{
			this.name = _name;
			this.type = _type;
			this.defaultValue = _defaultValue;
			this.bPersistent = _bPersistent;
		}

		public EnumGameStats name;

		public GameStats.EnumType type;

		public object defaultValue;

		public bool bPersistent;
	}
}
