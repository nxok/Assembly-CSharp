using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BiomeDefinition
{
	public static string LocalizedBiomeName(BiomeDefinition.BiomeType _biomeType)
	{
		return Localization.Get("biome_" + _biomeType.ToStringCached<BiomeDefinition.BiomeType>(), false, null);
	}

	public BiomeDefinition(byte _id, byte _subId, string _name, uint _color, int _radiationLevel, string _buff)
	{
		this.m_Id = _id;
		this.m_BiomeType = (BiomeDefinition.BiomeType)(Enum.IsDefined(typeof(BiomeDefinition.BiomeType), (int)this.m_Id) ? this.m_Id : 0);
		this.subId = _subId;
		this.m_sBiomeName = _name;
		this.LocalizedName = Localization.Get("biome_" + _name, false, null);
		this.m_SpectrumName = this.m_sBiomeName;
		this.m_uiColor = _color;
		this.m_RadiationLevel = _radiationLevel;
		this.m_Layers = new List<BiomeLayer>();
		this.m_DecoBlocks = new List<BiomeBlockDecoration>();
		this.m_DistantDecoBlocks = new List<BiomeBlockDecoration>();
		this.m_DecoPrefabs = new List<BiomePrefabDecoration>();
		this.m_DecoBluffs = new List<BiomeBluffDecoration>();
		this.Buff = _buff;
		this.InitWeather();
	}

	public void AddLayer(BiomeLayer _layer)
	{
		this.m_Layers.Add(_layer);
		this.TotalLayerDepth += _layer.m_Depth;
	}

	public void AddDecoBlock(BiomeBlockDecoration _deco)
	{
		this.m_DecoBlocks.Add(_deco);
		if (Block.BlocksLoaded)
		{
			Block block = _deco.blockValues[0].Block;
			if (block != null && block.IsDistantDecoration)
			{
				this.m_DistantDecoBlocks.Add(_deco);
			}
		}
	}

	public void AddDecoPrefab(BiomePrefabDecoration _deco)
	{
		this.m_DecoPrefabs.Add(_deco);
	}

	public void AddBluff(BiomeBluffDecoration _deco)
	{
		this.m_DecoBluffs.Add(_deco);
	}

	public void AddReplacement(int _sourceId, int _targetId)
	{
		this.Replacements[_sourceId] = _targetId;
	}

	public void addSubBiome(BiomeDefinition _subbiome)
	{
		this.subbiomes.Add(_subbiome);
	}

	public override bool Equals(object obj)
	{
		return obj is BiomeDefinition && ((BiomeDefinition)obj).m_Id == this.m_Id;
	}

	public override int GetHashCode()
	{
		return (int)this.m_Id;
	}

	public override string ToString()
	{
		return this.m_sBiomeName;
	}

	public static uint GetBiomeColor(BiomeDefinition.BiomeType _type)
	{
		return BiomeDefinition.BiomeColors[(int)_type];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitWeather()
	{
	}

	public BiomeDefinition.WeatherGroup AddWeatherGroup(string _name, float _prob, float _duration, Vector2 _delay, string _buffName)
	{
		BiomeDefinition.WeatherGroup weatherGroup = new BiomeDefinition.WeatherGroup();
		weatherGroup.name = _name;
		weatherGroup.stormLevel = ((!_name.StartsWith("storm")) ? 0 : (_name.Contains("build") ? 1 : 2));
		weatherGroup.prob = _prob;
		weatherGroup.duration = (int)(_duration * 1000f);
		weatherGroup.delay.x = (int)(_delay.x * 1000f);
		weatherGroup.delay.y = (int)(_delay.y * 1000f);
		if (!string.IsNullOrEmpty(_buffName))
		{
			weatherGroup.buffName = _buffName;
		}
		this.weatherGroups.Add(weatherGroup);
		return weatherGroup;
	}

	public void SetupWeather()
	{
		float num = 0f;
		for (int i = 0; i < this.weatherGroups.Count; i++)
		{
			BiomeDefinition.WeatherGroup weatherGroup = this.weatherGroups[i];
			num += weatherGroup.prob;
			weatherGroup.probabilities.Normalize();
		}
		num += 1E-06f;
		for (int j = 0; j < this.weatherGroups.Count; j++)
		{
			this.weatherGroups[j].prob /= num;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float WeatherGetValue(BiomeDefinition.Probabilities.ProbType _type)
	{
		return this.weatherValues[(int)_type];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WeatherSetValue(BiomeDefinition.Probabilities.ProbType _type, float _value)
	{
		this.weatherValues[(int)_type] = _value;
	}

	public void WeatherRandomize(float _rand)
	{
		float num = 0f;
		for (int i = 0; i < this.weatherGroups.Count; i++)
		{
			BiomeDefinition.WeatherGroup weatherGroup = this.weatherGroups[i];
			num += weatherGroup.prob;
			if (_rand < num)
			{
				this.SelectWeatherGroup(i);
				return;
			}
		}
	}

	public bool WeatherRandomize(string _weatherGroup)
	{
		int num = this.FindWeatherGroupIndex(_weatherGroup);
		if (num >= 0)
		{
			this.SelectWeatherGroup(num);
			return true;
		}
		return false;
	}

	public int WeatherGetDuration(string _weatherGroup)
	{
		BiomeDefinition.WeatherGroup weatherGroup = this.FindWeatherGroup(_weatherGroup);
		if (weatherGroup != null)
		{
			return weatherGroup.duration;
		}
		return 0;
	}

	public int WeatherGetDuration(string _weatherGroup, out Vector2i _delay)
	{
		BiomeDefinition.WeatherGroup weatherGroup = this.FindWeatherGroup(_weatherGroup);
		if (weatherGroup != null)
		{
			_delay = weatherGroup.delay;
			return weatherGroup.duration;
		}
		_delay = Vector2i.zero;
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition.WeatherGroup FindWeatherGroup(string _name)
	{
		for (int i = 0; i < this.weatherGroups.Count; i++)
		{
			BiomeDefinition.WeatherGroup weatherGroup = this.weatherGroups[i];
			if (weatherGroup.name == _name)
			{
				return weatherGroup;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int FindWeatherGroupIndex(string _name)
	{
		for (int i = 0; i < this.weatherGroups.Count; i++)
		{
			if (this.weatherGroups[i].name == _name)
			{
				return i;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SelectWeatherGroup(int _index)
	{
		BiomeDefinition.WeatherGroup weatherGroup = this.weatherGroups[_index];
		this.weatherName = weatherGroup.name;
		this.weatherSpectrum = weatherGroup.spectrum;
		this.currentWeatherGroupIndex = _index;
		this.currentWeatherGroup = weatherGroup;
		for (int i = 0; i < 5; i++)
		{
			float randomValue = weatherGroup.probabilities.GetRandomValue((BiomeDefinition.Probabilities.ProbType)i);
			this.weatherValues[i] = randomValue;
		}
	}

	public void SetWeatherGroup(int _index)
	{
		this.currentWeatherGroupIndex = _index;
		BiomeDefinition.WeatherGroup weatherGroup = this.weatherGroups[_index];
		this.currentWeatherGroup = weatherGroup;
		this.weatherSpectrum = weatherGroup.spectrum;
	}

	public float currentPlayerIntensity;

	public const string BiomeNameLocalizationPrefix = "biome_";

	public static string[] BiomeNames = new string[]
	{
		"any", "snow", "forest", "pine_forest", "plains", "desert", "water", "radiated", "wasteland", "burnt_forest",
		"city", "city_wasteland", "wasteland_hub", "caveFloor", "caveCeiling"
	};

	public static uint[] BiomeColors = new uint[]
	{
		0U, 16777215U, 0U, 16384U, 0U, 16770167U, 25599U, 0U, 16754688U, 12189951U,
		8421504U, 12632256U, 10526880U, 0U, 0U
	};

	public readonly byte m_Id;

	public readonly BiomeDefinition.BiomeType m_BiomeType;

	public byte subId;

	public readonly string m_sBiomeName;

	public readonly string LocalizedName;

	public uint m_uiColor;

	public string m_SpectrumName;

	public static Dictionary<string, byte> nameToId;

	public int m_RadiationLevel;

	public int Difficulty = 1;

	public List<BiomeLayer> m_Layers;

	public List<BiomeBlockDecoration> m_DecoBlocks;

	public List<BiomeBlockDecoration> m_DistantDecoBlocks;

	public List<BiomePrefabDecoration> m_DecoPrefabs;

	public List<BiomeBluffDecoration> m_DecoBluffs;

	public List<BiomeDefinition.WeatherGroup> weatherGroups = new List<BiomeDefinition.WeatherGroup>();

	public string weatherName;

	public int currentWeatherGroupIndex;

	public BiomeDefinition.WeatherGroup currentWeatherGroup;

	public SpectrumWeatherType weatherSpectrum;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] weatherValues = new float[5];

	public int TotalLayerDepth;

	public List<BiomeDefinition> subbiomes = new List<BiomeDefinition>();

	public float noiseFreq = 0.03f;

	public float noiseMin = 0.2f;

	public float noiseMax = 1f;

	public Vector2 noiseOffset;

	public Dictionary<int, int> Replacements = new Dictionary<int, int>();

	public float GameStageMod;

	public float GameStageBonus;

	public float LootStageMod;

	public float LootStageBonus;

	public int LootStageMin = -1;

	public int LootStageMax = -1;

	public string Buff;

	public enum BiomeType
	{
		Any,
		Snow,
		Forest,
		PineForest,
		Plains,
		Desert,
		Water,
		Radiated,
		Wasteland,
		burnt_forest,
		city,
		city_wasteland,
		wasteland_hub,
		caveFloor,
		caveCeiling
	}

	public class Probabilities
	{
		public Probabilities()
		{
			this.probabilities = new List<Vector3>[5];
			for (int i = 0; i < 5; i++)
			{
				this.probabilities[i] = new List<Vector3>();
			}
		}

		public void AddProbability(BiomeDefinition.Probabilities.ProbType _type, Vector2 _range, float _probability)
		{
			this.probabilities[(int)_type].Add(new Vector3(_range.x, _range.y, _probability));
		}

		public Vector2 CalcMinMaxPossibleValue(BiomeDefinition.Probabilities.ProbType type)
		{
			Vector2 vector;
			vector..ctor(float.MaxValue, float.MinValue);
			List<Vector3> list = this.probabilities[(int)type];
			for (int i = 0; i < list.Count; i++)
			{
				Vector3 vector2 = list[i];
				if (vector2.x < vector.x)
				{
					vector.x = vector2.x;
				}
				if (vector2.y > vector.y)
				{
					vector.y = vector2.y;
				}
			}
			return vector;
		}

		public float GetRandomValue(BiomeDefinition.Probabilities.ProbType _type)
		{
			GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
			float randomFloat = gameRandom.RandomFloat;
			float num = 0f;
			List<Vector3> list = this.probabilities[(int)_type];
			for (int i = 0; i < list.Count; i++)
			{
				Vector3 vector = list[i];
				num += vector.z;
				if (randomFloat < num)
				{
					float randomFloat2 = gameRandom.RandomFloat;
					return vector.x * randomFloat2 + vector.y * (1f - randomFloat2);
				}
			}
			return 0f;
		}

		public void Normalize()
		{
			for (int i = 0; i < 5; i++)
			{
				List<Vector3> list = this.probabilities[i];
				float num = 0f;
				for (int j = 0; j < list.Count; j++)
				{
					num += list[j].z;
				}
				for (int k = 0; k < list.Count; k++)
				{
					Vector3 vector = list[k];
					vector.z /= num;
					list[k] = vector;
				}
			}
		}

		public const int ProbTypeCount = 5;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<Vector3>[] probabilities;

		public enum ProbType
		{
			Temperature,
			Precipitation,
			CloudThickness,
			Wind,
			Fog,
			Count
		}
	}

	public class WeatherGroup
	{
		public void AddProbability(BiomeDefinition.Probabilities.ProbType _type, Vector2 _range, float _probability)
		{
			this.probabilities.AddProbability(_type, _range, _probability);
		}

		public string name;

		public int stormLevel;

		public float prob;

		public int duration;

		public Vector2i delay;

		public string buffName;

		public SpectrumWeatherType spectrum;

		public BiomeDefinition.Probabilities probabilities = new BiomeDefinition.Probabilities();
	}
}
