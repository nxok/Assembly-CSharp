using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
	public static void Init(World _world, GameObject _obj)
	{
		WeatherManager.Cleanup();
		WeatherManager.Instance = _obj.GetComponent<WeatherManager>();
		WeatherManager.Instance.Init(_world);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init(World _world)
	{
		this.world = _world;
		WeatherManager.gameRand = _world.GetGameRandom();
		string @string = GamePrefs.GetString(EnumGamePrefs.GameMode);
		this.isGameModeNormal = !GameModeEditWorld.TypeName.Equals(@string) && !GameModeCreative.TypeName.Equals(@string);
		this.InitBiomeWeather();
		WeatherManager.currentWeather = new WeatherManager.BiomeWeather(this.world.Biomes.GetBiome(3));
		WeatherManager.currentWeather.parameters[2].name = "CloudThickness";
		WeatherManager.currentWeather.parameters[4].name = "Fog";
		WeatherManager.currentWeather.parameters[1].name = "Precipitation";
		WeatherManager.currentWeather.parameters[0].name = "Temperature";
		WeatherManager.currentWeather.parameters[3].name = "Wind";
		WeatherManager.currentWeather.rainParam.name = "Rain";
		WeatherManager.currentWeather.snowFallParam.name = "SnowFall";
		this.ApplyLoad();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitBiomeWeather()
	{
		this.biomeWeather = new List<WeatherManager.BiomeWeather>();
		foreach (KeyValuePair<uint, BiomeDefinition> keyValuePair in this.world.Biomes.GetBiomeMap())
		{
			BiomeDefinition value = keyValuePair.Value;
			if (value.weatherGroups.Count > 0)
			{
				WeatherManager.BiomeWeather biomeWeather = new WeatherManager.BiomeWeather(value);
				this.biomeWeather.Add(biomeWeather);
				biomeWeather.Reset();
			}
		}
		int count = this.biomeWeather.Count;
		this.weatherPackages = new WeatherPackage[count];
		for (int i = 0; i < count; i++)
		{
			WeatherManager.BiomeWeather biomeWeather2 = this.biomeWeather[i];
			WeatherPackage weatherPackage = new WeatherPackage();
			weatherPackage.biomeId = biomeWeather2.biomeDefinition.m_Id;
			this.weatherPackages[i] = weatherPackage;
		}
	}

	public static void Cleanup()
	{
		if (WeatherManager.Instance)
		{
			Object.Destroy(WeatherManager.Instance.stormDistantRootT.gameObject);
			Object.DestroyImmediate(WeatherManager.Instance.gameObject);
			WeatherManager.forceClouds = -1f;
			WeatherManager.forceRain = -1f;
			WeatherManager.forceSnowfall = -1f;
			WeatherManager.forceTemperature = -100f;
			WeatherManager.forceWind = -1f;
		}
	}

	public static void SetWorldTime(ulong _time)
	{
		if (WeatherManager.Instance)
		{
			int num = (int)_time;
			if (num < WeatherManager.worldTime)
			{
				WeatherManager.Instance.AdjustTimeRewind();
			}
			WeatherManager.worldTime = num;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AdjustTimeRewind()
	{
		this.weatherLastUpdateWorldTime = 0f;
		for (int i = 0; i < this.biomeWeather.Count; i++)
		{
			WeatherManager.BiomeWeather biomeWeather = this.biomeWeather[i];
			biomeWeather.stormWorldTime = 0;
			biomeWeather.stormDuration = 0;
			biomeWeather.nextRandWorldTime = 0;
		}
	}

	public static void Load(IBinaryReaderOrWriter _RW, int _loadSize)
	{
		if (WeatherManager.loadData == null)
		{
			WeatherManager.loadData = new MemoryStream();
		}
		WeatherManager.loadData.SetLength((long)_loadSize);
		_RW.ReadWrite(WeatherManager.loadData.GetBuffer(), 0, _loadSize);
		WeatherManager.loadData.Position = 0L;
	}

	public void ApplyLoad()
	{
		if (WeatherManager.loadData != null)
		{
			using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
			{
				pooledBinaryReader.SetBaseStream(WeatherManager.loadData);
				this.ReadWriteData(pooledBinaryReader, true);
			}
			WeatherManager.loadData = null;
		}
	}

	public static void Save(IBinaryReaderOrWriter _RW)
	{
		if (WeatherManager.loadData != null)
		{
			_RW.ReadWrite(WeatherManager.loadData.GetBuffer(), 0, (int)WeatherManager.loadData.Length);
			return;
		}
		if (WeatherManager.Instance)
		{
			WeatherManager.Instance.ReadWriteData(_RW, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReadWriteData(IBinaryReaderOrWriter _RW, bool _load)
	{
		ushort num = _RW.ReadWrite(4);
		if (!_load)
		{
			_RW.ReadWrite((byte)GamePrefs.GetInt(EnumGamePrefs.DayNightLength));
			byte b = (byte)this.biomeWeather.Count;
			_RW.ReadWrite(b);
			for (int i = 0; i < (int)b; i++)
			{
				WeatherManager.BiomeWeather biomeWeather = this.biomeWeather[i];
				_RW.ReadWrite(biomeWeather.biomeDefinition.m_Id);
				_RW.ReadWrite((byte)biomeWeather.biomeDefinition.currentWeatherGroupIndex);
				_RW.ReadWrite(biomeWeather.stormWorldTime);
				_RW.ReadWrite((short)biomeWeather.stormDuration);
				_RW.ReadWrite(biomeWeather.nextRandWorldTime);
				for (int j = 0; j < 5; j++)
				{
					_RW.ReadWrite(biomeWeather.parameters[j].value);
				}
				_RW.ReadWrite(biomeWeather.rainParam.value);
				_RW.ReadWrite(biomeWeather.snowFallParam.value);
			}
			return;
		}
		if (num < 4)
		{
			return;
		}
		if ((int)_RW.ReadWrite(0) != GamePrefs.GetInt(EnumGamePrefs.DayNightLength))
		{
			return;
		}
		int num2 = (int)_RW.ReadWrite(0);
		for (int k = 0; k < num2; k++)
		{
			int num3 = (int)_RW.ReadWrite(0);
			WeatherManager.BiomeWeather biomeWeather2 = this.FindBiomeWeather(num3);
			if (biomeWeather2 == null)
			{
				return;
			}
			int num4 = (int)_RW.ReadWrite(0);
			biomeWeather2.biomeDefinition.SetWeatherGroup(num4);
			biomeWeather2.stormWorldTime = _RW.ReadWrite(0);
			biomeWeather2.stormDuration = (int)_RW.ReadWrite(0);
			biomeWeather2.nextRandWorldTime = _RW.ReadWrite(0);
			for (int l = 0; l < 5; l++)
			{
				float num5 = _RW.ReadWrite(0f);
				biomeWeather2.parameters[l].Set(num5);
				biomeWeather2.biomeDefinition.WeatherSetValue((BiomeDefinition.Probabilities.ProbType)l, num5);
			}
			biomeWeather2.rainParam.Set(_RW.ReadWrite(0f));
			biomeWeather2.snowFallParam.Set(_RW.ReadWrite(0f));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WeatherManager.BiomeWeather FindBiomeWeather(int _id)
	{
		for (int i = 0; i < this.biomeWeather.Count; i++)
		{
			WeatherManager.BiomeWeather biomeWeather = this.biomeWeather[i];
			if ((int)biomeWeather.biomeDefinition.m_Id == _id)
			{
				return biomeWeather;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WeatherManager.BiomeWeather FindBiomeWeather(BiomeDefinition.BiomeType _type)
	{
		for (int i = 0; i < this.biomeWeather.Count; i++)
		{
			WeatherManager.BiomeWeather biomeWeather = this.biomeWeather[i];
			if (biomeWeather.biomeDefinition.m_BiomeType == _type)
			{
				return biomeWeather;
			}
		}
		return null;
	}

	public bool IsStorming(BiomeDefinition.BiomeType _type)
	{
		WeatherManager.BiomeWeather biomeWeather = WeatherManager.Instance.FindBiomeWeather(_type);
		return biomeWeather != null && biomeWeather.stormState >= 2;
	}

	public static void ClearTemperatureOffSetHeights()
	{
	}

	public static void AddTemperatureOffSetHeight(float height, float degreesOffset)
	{
	}

	public static float SeaLevel()
	{
		return 0f;
	}

	public static float GetCloudThickness()
	{
		if (WeatherManager.forceClouds >= 0f)
		{
			return WeatherManager.forceClouds * 100f;
		}
		if (WeatherManager.currentWeather == null)
		{
			return 0f;
		}
		return WeatherManager.currentWeather.parameters[2].value;
	}

	public static float GetTemperature()
	{
		if (WeatherManager.forceTemperature > -100f)
		{
			return WeatherManager.forceTemperature;
		}
		if (WeatherManager.currentWeather == null)
		{
			return 0f;
		}
		return WeatherManager.currentWeather.parameters[0].value;
	}

	public static float GetWindSpeed()
	{
		if (WeatherManager.forceWind >= 0f)
		{
			return WeatherManager.forceWind;
		}
		if (WeatherManager.currentWeather == null)
		{
			return 0f;
		}
		return WeatherManager.currentWeather.parameters[3].value;
	}

	public static void EntityAddedToWorld(Entity entity)
	{
	}

	public static void EntityRemovedFromWorld(Entity entity)
	{
	}

	public float GetCurrentSnowfallPercent()
	{
		if (WeatherManager.forceSnowfall < 0f && WeatherManager.currentWeather != null)
		{
			return WeatherManager.currentWeather.snowFallParam.value;
		}
		return WeatherManager.forceSnowfall;
	}

	public float GetCurrentRainfallPercent()
	{
		if (WeatherManager.forceRain < 0f && WeatherManager.currentWeather != null)
		{
			return WeatherManager.currentWeather.rainParam.value;
		}
		return WeatherManager.forceRain;
	}

	public float GetCurrentWetPercent(EntityAlive _ea)
	{
		float num = 0f;
		BiomeDefinition biomeStandingOn = _ea.biomeStandingOn;
		if (biomeStandingOn != null && biomeStandingOn.m_BiomeType == BiomeDefinition.BiomeType.Snow && biomeStandingOn.currentWeatherGroup.stormLevel >= 2)
		{
			num = 1f;
		}
		float num2 = Utils.FastMax(this.GetCurrentSnowfallPercent(), this.GetCurrentRainfallPercent());
		return Utils.FastMax(num, num2);
	}

	public float GetCurrentCloudThicknessPercent()
	{
		return WeatherManager.GetCloudThickness() * 0.01f;
	}

	public float GetCurrentTemperatureValue()
	{
		return WeatherManager.GetTemperature();
	}

	public static void SetSimRandom(float _random)
	{
		WeatherManager.forceSimRandom = _random;
		if (WeatherManager.Instance)
		{
			WeatherManager.Instance.weatherLastUpdateWorldTime = 0f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LoadSpectrums()
	{
		if (WeatherManager.atmosphereSpectrum == null)
		{
			WeatherManager.atmosphereSpectrum = new AtmosphereEffect[Enum.GetNames(typeof(SpectrumWeatherType)).Length - 1];
			WeatherManager.ReloadSpectrums();
		}
	}

	public static void ReloadSpectrums()
	{
		WeatherManager.atmosphereSpectrum[1] = AtmosphereEffect.Load("Snowy", null);
		WeatherManager.atmosphereSpectrum[2] = AtmosphereEffect.Load("Stormy", null);
		WeatherManager.atmosphereSpectrum[3] = AtmosphereEffect.Load("Rainy", null);
		WeatherManager.atmosphereSpectrum[4] = AtmosphereEffect.Load("Foggy", null);
		WeatherManager.atmosphereSpectrum[5] = AtmosphereEffect.Load("BloodMoon", null);
	}

	public void Start()
	{
		this.windZoneObj = GameObject.Find("WindZone");
		if (this.windZoneObj)
		{
			this.windZone = this.windZoneObj.GetComponent<WindZone>();
		}
		WeatherManager.LoadSpectrums();
		this.raycastMask = LayerMask.GetMask(new string[] { "Water", "NoShadow", "Items", "CC Physics", "TerrainCollision", "CC Physics Dead", "CC Local Physics" }) | 1;
		string text = "@:Textures/Environment/Spectrums/default/";
		for (int i = 0; i < WeatherManager.strCloudTypes.Length; i++)
		{
			Texture texture = DataLoader.LoadAsset<Texture>(text + WeatherManager.strCloudTypes[i] + "Clouds.tga", false);
			WeatherManager.clouds[i] = texture;
		}
	}

	public void PushTransitions()
	{
		this.spectrumBlend = 1f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CurrentWeatherFromNearBiomesFrameUpdate()
	{
		if (this.world.BiomeAtmosphereEffects != null)
		{
			BiomeDefinition[] nearBiomes = this.world.BiomeAtmosphereEffects.nearBiomes;
			BiomeDefinition biomeDefinition = nearBiomes[0];
			WeatherManager.currentWeather.biomeDefinition = biomeDefinition;
			if (biomeDefinition == null)
			{
				return;
			}
			for (int i = 0; i < WeatherManager.currentWeather.parameters.Length; i++)
			{
				WeatherManager.currentWeather.parameters[i].target = 0f;
			}
			WeatherManager.inWeatherGracePeriod = (WeatherManager.worldTime < 22000 || !this.isGameModeNormal) && this.CustomWeatherTime == -1f;
			if (WeatherManager.inWeatherGracePeriod)
			{
				WeatherManager.currentWeather.rainParam.Set(0f);
				WeatherManager.currentWeather.snowFallParam.Set(0f);
				int num = 70;
				BiomeDefinition.BiomeType biomeType = biomeDefinition.m_BiomeType;
				if (biomeType != BiomeDefinition.BiomeType.Snow)
				{
					if (biomeType - BiomeDefinition.BiomeType.Forest <= 1)
					{
						num = 60;
					}
				}
				else
				{
					num = 45;
				}
				WeatherManager.currentWeather.parameters[0].Set((float)num);
				WeatherManager.currentWeather.parameters[3].Set(8f);
				return;
			}
			WeatherManager.BiomeWeather biomeWeather = this.FindBiomeWeather(biomeDefinition.m_BiomeType);
			WeatherManager.currentWeather.remainingSeconds = biomeWeather.remainingSeconds;
			WeatherManager.currentWeather.rainParam.target = biomeWeather.rainParam.value;
			WeatherManager.currentWeather.snowFallParam.target = biomeWeather.snowFallParam.value;
			if (!this.isCurrentWeatherUpdatedFirstTime)
			{
				WeatherManager.currentWeather.rainParam.value = biomeWeather.rainParam.value;
				WeatherManager.currentWeather.snowFallParam.value = biomeWeather.snowFallParam.value;
			}
			float num2 = 0f;
			foreach (BiomeDefinition biomeDefinition2 in nearBiomes)
			{
				if (biomeDefinition2 != null)
				{
					num2 += biomeDefinition2.currentPlayerIntensity;
				}
			}
			foreach (BiomeDefinition biomeDefinition3 in nearBiomes)
			{
				if (biomeDefinition3 != null)
				{
					biomeWeather = this.FindBiomeWeather(biomeDefinition3.m_BiomeType);
					float num3 = Utils.FastClamp01(biomeDefinition3.currentPlayerIntensity / num2);
					for (int l = 0; l < WeatherManager.currentWeather.parameters.Length; l++)
					{
						WeatherManager.Param param = WeatherManager.currentWeather.parameters[l];
						param.target += biomeWeather.parameterFinals[l] * num3;
						if (!this.isCurrentWeatherUpdatedFirstTime)
						{
							param.value = param.target;
						}
					}
				}
			}
			this.isCurrentWeatherUpdatedFirstTime = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GenerateWeatherServerFrameUpdate()
	{
		if (WeatherManager.inWeatherGracePeriod)
		{
			this.GeneralReset();
			return;
		}
		if (this.CustomWeatherTime > 0f)
		{
			this.CustomWeatherTime -= Time.deltaTime;
			if (this.CustomWeatherTime <= 0f)
			{
				this.CustomWeatherName = "";
				this.CustomWeatherTime = -1f;
				this.GeneralReset();
				this.SetAllWeather("default");
				this.weatherLastUpdateWorldTime = (float)WeatherManager.worldTime;
				return;
			}
		}
		else
		{
			string text = this.CalcGlobalWeatherType();
			if (text != null)
			{
				this.SetAllWeather(text);
				return;
			}
			this.weatherAllName = null;
			if (Utils.FastAbs((float)WeatherManager.worldTime - this.weatherLastUpdateWorldTime) >= 5f)
			{
				this.weatherLastUpdateWorldTime = (float)WeatherManager.worldTime;
				float stormFrequency = World.StormFrequency;
				for (int i = 0; i < this.biomeWeather.Count; i++)
				{
					this.biomeWeather[i].ServerTimeUpdate(WeatherManager.worldTime, stormFrequency);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string CalcGlobalWeatherType()
	{
		if (SkyManager.IsBloodMoonVisible())
		{
			int num = 5000;
			for (int i = 0; i < this.biomeWeather.Count; i++)
			{
				WeatherManager.BiomeWeather biomeWeather = this.biomeWeather[i];
				if (biomeWeather.stormWorldTime - WeatherManager.worldTime < num)
				{
					biomeWeather.stormWorldTime = WeatherManager.worldTime + num;
				}
			}
			return "bloodMoon";
		}
		return null;
	}

	public void ForceWeather(string _weatherName, float _duration)
	{
		this.CustomWeatherName = _weatherName;
		this.CustomWeatherTime = _duration;
		this.GeneralReset();
		this.SetAllWeather(_weatherName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetAllWeather(string _weatherName)
	{
		if (this.weatherAllName != _weatherName)
		{
			this.weatherAllName = _weatherName;
			for (int i = 0; i < this.biomeWeather.Count; i++)
			{
				this.biomeWeather[i].SetWeather(_weatherName);
			}
		}
	}

	public void SetStorm(string _biomeName, int _duration)
	{
		for (int i = 0; i < this.biomeWeather.Count; i++)
		{
			WeatherManager.BiomeWeather biomeWeather = this.biomeWeather[i];
			if (_biomeName == null || biomeWeather.biomeDefinition.m_sBiomeName == _biomeName)
			{
				biomeWeather.stormWorldTime = WeatherManager.worldTime;
				biomeWeather.stormDuration = _duration;
			}
		}
	}

	public void TriggerUpdate()
	{
		this.weatherAllName = null;
		this.weatherLastUpdateWorldTime = 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GeneralReset()
	{
	}

	public void FrameUpdate()
	{
		if (GameManager.Instance == null || SkyManager.random == null)
		{
			return;
		}
		if (this.world == null)
		{
			return;
		}
		float time = Time.time;
		EntityPlayerLocal primaryPlayer = this.world.GetPrimaryPlayer();
		if (time > this.checkPlayerMoveTime + 1f)
		{
			this.checkPlayerMoveTime = time;
			if (primaryPlayer)
			{
				Vector3 vector = primaryPlayer.position - this.playerPosition;
				if (vector.x * vector.x + vector.y * vector.y + vector.z * vector.z > 400f)
				{
					this.spectrumBlend = 1f;
				}
				this.playerPosition = primaryPlayer.position;
			}
		}
		int num = Time.frameCount;
		if (this.frameCount == num)
		{
			return;
		}
		this.frameCount = num;
		if (primaryPlayer)
		{
			this.ParticlesFrameUpdate(primaryPlayer);
			primaryPlayer.WeatherStatusFrameUpdate();
		}
		bool flag = GameManager.IsDedicatedServer || SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		if (flag)
		{
			this.GenerateWeatherServerFrameUpdate();
		}
		for (int i = 0; i < this.biomeWeather.Count; i++)
		{
			this.biomeWeather[i].FrameUpdate(flag);
		}
		this.CurrentWeatherFromNearBiomesFrameUpdate();
		WeatherManager.currentWeather.ParamsFrameUpdate();
		if (primaryPlayer)
		{
			this.thunderDelay -= Time.deltaTime;
			if (this.thunderDelay <= 0f)
			{
				this.thunderDelay = Utils.FastLerpUnclamped(30f, 60f, SkyManager.random.RandomFloat);
				if (((((WeatherManager.forceRain >= 0f) ? WeatherManager.forceRain : WeatherManager.currentWeather.rainParam.value) > 0.5f && ((WeatherManager.forceClouds >= 0f) ? (WeatherManager.forceClouds * 100f) : WeatherManager.currentWeather.CloudThickness()) >= 70f) || SkyManager.IsBloodMoonVisible()) && EnvironmentAudioManager.Instance != null)
				{
					EnvironmentAudioManager.Instance.TriggerThunder(primaryPlayer.position);
				}
			}
		}
		if (WeatherManager.needToReUpdateWeatherSpectrums)
		{
			WeatherManager.needToReUpdateWeatherSpectrums = false;
			this.spectrumBlend = 1f;
		}
		this.SpectrumsFrameUpdate();
		this.CloudsFrameUpdate();
		this.WindFrameUpdate();
		TriggerEffectManager.UpdateDualSenseLightFromWeather(WeatherManager.currentWeather);
	}

	public void CloudsFrameUpdateNow()
	{
		this.CloudsFrameUpdate();
		this.cloudThickness = this.cloudThicknessTarget;
	}

	public void CloudsFrameUpdate()
	{
		if (WeatherManager.currentWeather == null)
		{
			return;
		}
		float num = WeatherManager.currentWeather.CloudThickness();
		if (WeatherManager.forceClouds >= 0f)
		{
			num = WeatherManager.forceClouds * 100f;
		}
		this.cloudThicknessTarget = num;
		if (num < 20f)
		{
			this.cloudThicknessTarget = 0f;
		}
		this.cloudThickness = Mathf.MoveTowards(this.cloudThickness, this.cloudThicknessTarget, 0.05f);
		Texture texture;
		Texture texture2;
		float num2;
		if (this.cloudThickness <= 40f)
		{
			texture = WeatherManager.clouds[0];
			texture2 = WeatherManager.clouds[1];
			num2 = this.cloudThickness / 40f;
		}
		else
		{
			texture = WeatherManager.clouds[2];
			texture2 = WeatherManager.clouds[1];
			num2 = (this.cloudThickness - 40f) / 50f;
			if (num2 >= 1f)
			{
				num2 = 1f;
			}
			num2 = 1f - num2;
		}
		SkyManager.SetCloudTextures(texture, texture2);
		SkyManager.SetCloudTransition(num2);
	}

	public void WindFrameUpdate()
	{
		float deltaTime = Time.deltaTime;
		float num = WeatherManager.GetWindSpeed();
		float num2 = num * 0.01f;
		this.windGust += this.windGustStep * deltaTime;
		if (this.windGust <= 0f)
		{
			this.windGust = 0f;
			this.windGustTime -= deltaTime;
			if (this.windGustTime <= 0f)
			{
				GameRandom gameRandom = this.world.GetGameRandom();
				this.windGustTarget = (3f + num * 0.33f) * gameRandom.RandomFloat + 5f;
				this.windGustStep = 0.35f * this.windGustTarget;
				this.windGustTime = (1f + 5f * gameRandom.RandomFloat) * (1f - num2) + 0.5f;
			}
		}
		if (this.windGust > this.windGustTarget)
		{
			this.windGust = this.windGustTarget;
			this.windGustStep = -this.windGustStep;
		}
		num += this.windGust;
		num *= 0.01f;
		this.windZone.windMain = num * 1.5f;
		this.windSpeedPrevious = this.windSpeed;
		this.windSpeed = num;
		this.windTimePrevious = this.windTime;
		this.windTime += num * deltaTime;
		Shader.SetGlobalVector("_Wind", new Vector4(this.windSpeed, this.windTime, this.windSpeedPrevious, this.windTimePrevious));
	}

	public void InitParticles()
	{
		this.GetParticleParts("Rain", out this.rainParticleObj, out this.rainParticleMat, out this.rainParticleSys);
		this.rainParticleT = this.rainParticleObj.transform;
		this.rainEmissionMaxRate = this.rainParticleSys.emission.rateOverTime.constant;
		this.InitParticleData("Snow", this.snowData, false);
		for (int i = 0; i < 4; i++)
		{
			WeatherManager.ParticleData particleData = new WeatherManager.ParticleData();
			this.stormData[i] = particleData;
			particleData.weather = this.FindBiomeWeather(this.stormBiomes[i]);
			particleData.weather.stormIndex = i;
			string text = "Storm" + this.stormNames[i];
			this.InitParticleData(text, particleData, true);
		}
		this.ParticleBiomeDistantStormInit();
		this.stormLightScale = 1f;
		WeatherManager.stormDistantLightScaleTarget = 1f;
	}

	public void InitParticleData(string _name, WeatherManager.ParticleData _data, bool _isStorm)
	{
		this.GetParticleParts(_name, out _data.rootObj, out _data.rootParticleMat, out _data.rootParticleSys);
		_data.rootT = _data.rootObj.transform;
		_data.rootEmissionMaxRate = _data.rootParticleSys.emission.rateOverTime.constant;
		Transform transform = _data.rootT.Find("Near");
		if (transform)
		{
			_data.nearParticleSys = transform.GetComponent<ParticleSystem>();
			_data.nearEmissionMaxRate = _data.nearParticleSys.emission.rateOverTime.constant;
		}
		Transform transform2 = _data.rootT.Find("Top");
		if (transform2)
		{
			_data.topParticleSys = transform2.GetComponent<ParticleSystem>();
			_data.topEmissionMaxRate = _data.topParticleSys.emission.rateOverTime.constant;
		}
		Transform transform3 = _data.rootT.Find("Far");
		if (transform3)
		{
			if (_isStorm)
			{
				transform3.SetParent(_data.rootT.parent);
				transform3.gameObject.SetActive(false);
			}
			_data.farT = transform3;
			_data.farParticleSys = transform3.GetComponent<ParticleSystem>();
			_data.farBaseColor = _data.farParticleSys.main.startColor.color;
			Transform transform4 = transform3.Find("Ring");
			if (transform4)
			{
				_data.ringParticleSys = transform4.GetComponent<ParticleSystem>();
				_data.ringRadiusMax = _data.ringParticleSys.shape.radius;
			}
		}
		_data.rootObj.GetComponentsInChildren<ParticleSystem>(true, _data.particleSystems);
		for (int i = _data.particleSystems.Count - 1; i >= 0; i--)
		{
			if (_data.particleSystems[i] == _data.farParticleSys)
			{
				_data.particleSystems.RemoveAt(i);
			}
		}
		_data.psEmissionMaxRates = new float[_data.particleSystems.Count];
		for (int j = 0; j < _data.particleSystems.Count; j++)
		{
			ParticleSystem.EmissionModule emission = _data.particleSystems[j].emission;
			_data.psEmissionMaxRates[j] = emission.rateOverTime.constant;
		}
		_data.forceT = _data.rootT.Find("Force");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform GetParticle(string name)
	{
		return SkyManager.skyManager.transform.Find(name);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetParticleParts(string name, out GameObject obj, out Material mat, out ParticleSystem ps)
	{
		Transform particle = this.GetParticle(name);
		obj = particle.gameObject;
		ps = obj.GetComponent<ParticleSystem>();
		Renderer component = ps.GetComponent<Renderer>();
		mat = component.material;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ParticlesFrameUpdate(EntityPlayerLocal localPlayer)
	{
		if (!this.mainCamera)
		{
			this.mainCamera = Camera.main;
			if (!this.mainCamera)
			{
				return;
			}
		}
		float deltaTime = Time.deltaTime;
		Vector3 position = this.mainCamera.transform.position;
		position.y += 250f;
		RaycastHit raycastHit;
		if (Physics.SphereCast(new Ray(position, Vector3.down), 9f, ref raycastHit, float.PositiveInfinity, this.raycastMask))
		{
			this.particleFallLastPos = raycastHit.point;
			Vector3 velocityPerSecond = localPlayer.GetVelocityPerSecond();
			this.particleFallLastPos.x = this.particleFallLastPos.x + velocityPerSecond.x * 2f;
			this.particleFallLastPos.z = this.particleFallLastPos.z + velocityPerSecond.z * 2f;
			if (velocityPerSecond.y < -5f)
			{
				velocityPerSecond.y = -5f;
			}
			this.particleFallLastPos.y = this.particleFallLastPos.y + velocityPerSecond.y;
		}
		this.particleFallPos = this.particleFallLastPos;
		this.particleFallPos.y = this.particleFallPos.y + 12f;
		this.rainParticleT.position = this.particleFallPos;
		this.snowData.rootT.position = this.particleFallPos;
		for (int i = 0; i < 4; i++)
		{
			WeatherManager.ParticleData particleData = this.stormData[i];
			particleData.rootT.position = this.particleFallPos;
			if (particleData.farT)
			{
				particleData.farT.position = this.particleFallPos;
			}
		}
		WeatherManager.BiomeWeather biomeWeather = ((localPlayer.biomeStandingOn != null) ? this.FindBiomeWeather(localPlayer.biomeStandingOn.m_BiomeType) : WeatherManager.currentWeather);
		float num = ((WeatherManager.forceRain >= 0f) ? WeatherManager.forceRain : biomeWeather.rainParam.value);
		if (num > 0f && this.rainParticleSys)
		{
			this.rainParticleSys.emission.rateOverTime = this.rainEmissionMaxRate * (num * 0.99f + 0.01f);
		}
		this.rainParticleObj.SetActive(num > 0f);
		float num2 = ((WeatherManager.forceSnowfall >= 0f) ? WeatherManager.forceSnowfall : Utils.FastClamp01(biomeWeather.snowFallParam.value));
		if (num2 <= 0f)
		{
			this.snowData.rootObj.SetActive(false);
		}
		else
		{
			this.snowData.rootObj.SetActive(true);
			bool flag = SkyManager.GetFogDensity() < 0.3f;
			float num3 = num2 * 0.99f + 0.01f;
			ParticleSystem.EmissionModule emissionModule = this.snowData.rootParticleSys.emission;
			emissionModule.rateOverTime = this.snowData.rootEmissionMaxRate * num3;
			emissionModule = this.snowData.nearParticleSys.emission;
			emissionModule.rateOverTime = this.snowData.nearEmissionMaxRate * num3;
			emissionModule = this.snowData.topParticleSys.emission;
			emissionModule.rateOverTime = (flag ? (this.snowData.topEmissionMaxRate * num3) : 0f);
			ParticleSystem.MainModule main = this.snowData.farParticleSys.main;
			ParticleSystem.MinMaxGradient startColor = main.startColor;
			Color farBaseColor = this.snowData.farBaseColor;
			farBaseColor.a *= num2 * 0.95f + 0.05f;
			startColor.color = farBaseColor;
			main.startColor = startColor;
			Vector3 vector = localPlayer.position - Origin.position;
			vector.y += 1f;
			this.snowData.forceT.position = vector;
		}
		this.stormFogTarget = 0f;
		this.stormLightScaleTarget = 1f;
		for (int j = 0; j < 4; j++)
		{
			WeatherManager.ParticleData particleData2 = this.stormData[j];
			WeatherManager.BiomeWeather weather = particleData2.weather;
			int num4 = ((weather.biomeDefinition != biomeWeather.biomeDefinition) ? 0 : weather.biomeDefinition.currentWeatherGroup.stormLevel);
			if (num4 == 0)
			{
				if (particleData2.intensity > -1f)
				{
					particleData2.intensity -= 0.2f * deltaTime;
					if (particleData2.intensity <= -1f)
					{
						particleData2.stormLevel = 0;
						particleData2.thunderDelay = 0f;
						if (particleData2.farT)
						{
							particleData2.farT.gameObject.SetActive(false);
						}
						particleData2.rootObj.SetActive(false);
					}
				}
			}
			else
			{
				if (particleData2.stormLevel != num4)
				{
					particleData2.stormLevel = num4;
					particleData2.intensity = 0f;
				}
				particleData2.intensity += 0.2f * deltaTime;
				particleData2.intensity = Utils.FastClamp01(particleData2.intensity);
				particleData2.thunderDelay -= deltaTime;
				if (particleData2.thunderDelay <= 0f)
				{
					particleData2.thunderDelay = (float)WeatherManager.gameRand.RandomRange(7, 12);
					if (EnvironmentAudioManager.Instance != null)
					{
						EnvironmentAudioManager.Instance.TriggerThunder(localPlayer.position);
					}
				}
			}
			if (particleData2.intensity > -1f)
			{
				float num5 = Utils.FastMax(0f, particleData2.intensity);
				float num6 = (60f - (float)weather.remainingSeconds) / 60f;
				if (num6 > 0f)
				{
					float num7 = Utils.FastLerp(0f, 0.2f, num6);
					this.stormFogTarget = Utils.FastMax(num7, this.stormFogTarget);
				}
				this.stormLightScaleTarget = Utils.FastMin(this.stormLightScaleTarget, Utils.FastLerp(1f, 0.6f, num5));
				if (num4 == 1)
				{
					if (particleData2.farT)
					{
						particleData2.farT.gameObject.SetActive(true);
						ParticleSystem.MainModule main2 = this.snowData.farParticleSys.main;
						ParticleSystem.MinMaxGradient startColor2 = main2.startColor;
						Color farBaseColor2 = particleData2.farBaseColor;
						farBaseColor2.a *= num5 * 0.95f + 0.05f;
						startColor2.color = farBaseColor2;
						main2.startColor = startColor2;
						if (particleData2.ringParticleSys)
						{
							particleData2.ringParticleSys.shape.radius = Utils.FastLerp(0.3f, 1f, (float)weather.remainingSeconds / 60f) * particleData2.ringRadiusMax;
						}
					}
					particleData2.rootObj.SetActive(false);
				}
				else
				{
					if (num5 < 0.2f && particleData2.ringParticleSys)
					{
						particleData2.ringParticleSys.Stop();
					}
					if (num5 > 0.99f && particleData2.farT)
					{
						particleData2.farT.gameObject.SetActive(false);
					}
					particleData2.rootObj.SetActive(true);
					if (localPlayer.isIndoorsCurrent)
					{
						num5 *= 0.5f;
					}
					for (int k = 0; k < particleData2.particleSystems.Count; k++)
					{
						particleData2.particleSystems[k].emission.rateOverTime = particleData2.psEmissionMaxRates[k] * num5;
					}
					Vector3 vector2 = localPlayer.position - Origin.position;
					vector2.y += 2f;
					particleData2.forceT.position = vector2;
				}
			}
		}
		this.ParticleBiomeDistantStormFrameUpdate(localPlayer);
		if (localPlayer.isIndoorsCurrent)
		{
			this.stormFogTarget *= 0.3f;
		}
		float num8 = Utils.FastMax(this.stormFogTarget, WeatherManager.stormDistantFogTarget);
		WeatherManager.stormFog = Utils.FastMoveTowards(WeatherManager.stormFog, num8, deltaTime / 2.1f);
		num8 = Utils.FastMin(this.stormLightScaleTarget, WeatherManager.stormDistantLightScaleTarget);
		this.stormLightScale = Utils.FastMoveTowards(this.stormLightScale, num8, deltaTime / 3f);
		SkyManager.SetWeatherLightScale(this.stormLightScale);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ParticleBiomeDistantStormInit()
	{
		this.stormDistantRootT = new GameObject("StormDistant").transform;
		Origin.particleSystemTs.Add(this.stormDistantRootT);
		WeatherManager.DistantStormGrid distantStormGrid = new WeatherManager.DistantStormGrid();
		this.distantStormGrids[0] = distantStormGrid;
		Transform[] array = new Transform[4];
		for (int i = 0; i < 4; i++)
		{
			string text = "StormDistant" + this.stormNames[i];
			array[i] = this.GetParticle(text);
		}
		distantStormGrid.Init(this.world, 32, 0, 130, array);
		distantStormGrid = new WeatherManager.DistantStormGrid();
		this.distantStormGrids[1] = distantStormGrid;
		Transform[] array2 = new Transform[4];
		for (int j = 0; j < 4; j++)
		{
			string text2 = "StormDistant" + this.stormNames[j] + "Far";
			array2[j] = this.GetParticle(text2);
		}
		distantStormGrid.Init(this.world, 128, 90, 470, array2);
		distantStormGrid = new WeatherManager.DistantStormGrid();
		this.distantStormGrids[2] = distantStormGrid;
		Transform[] array3 = new Transform[4];
		for (int k = 0; k < 4; k++)
		{
			string text3 = "StormDistant" + this.stormNames[k] + "Away";
			array3[k] = this.GetParticle(text3);
		}
		distantStormGrid.Init(this.world, 512, 450, 1800, array3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ParticleBiomeDistantStormFrameUpdate(EntityPlayerLocal localPlayer)
	{
		ChunkCluster chunkCache = this.world.ChunkCache;
		IChunkProvider chunkProvider = ((chunkCache != null) ? chunkCache.ChunkProvider : null);
		if (chunkProvider == null)
		{
			return;
		}
		IBiomeProvider biomeProvider = chunkProvider.GetBiomeProvider();
		if (biomeProvider == null)
		{
			return;
		}
		ITerrainGenerator terrainGenerator = chunkProvider.GetTerrainGenerator();
		if (terrainGenerator == null)
		{
			return;
		}
		this.stormDistantRootT.position = -Origin.position;
		float deltaTime = Time.deltaTime;
		for (int i = 0; i < 3; i++)
		{
			this.distantStormGrids[i].updateElapsedTime += deltaTime;
		}
		int num = this.distantStormFrame;
		this.distantStormFrame = num + 1;
		int num2 = num % 3;
		this.distantStormGrids[num2].Update(localPlayer, biomeProvider, terrainGenerator);
	}

	public string GetSpectrumInfo()
	{
		float num = 1f - this.spectrumBlend;
		float num2 = this.spectrumBlend;
		string text = this.spectrumSourceType.ToString();
		string text2 = this.spectrumTargetType.ToString();
		return string.Format("source {0} {1}, target {2} {3}", new object[]
		{
			text,
			num.ToCultureInvariantString(),
			text2,
			num2.ToCultureInvariantString()
		});
	}

	public static void SetForceSpectrum(SpectrumWeatherType type)
	{
		WeatherManager.forcedSpectrum = type;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpectrumsFrameUpdate()
	{
		WeatherManager.LoadSpectrums();
		float num = SkyManager.BloodMoonVisiblePercent();
		if (num > 0f && this.spectrumTargetType == SpectrumWeatherType.BloodMoon)
		{
			this.spectrumBlend = num;
		}
		else if (this.spectrumBlend < 1f)
		{
			this.spectrumBlend += Time.deltaTime / 10f;
			if (this.spectrumBlend > 1f)
			{
				this.spectrumBlend = 1f;
			}
		}
		if (this.spectrumSourceType == this.spectrumTargetType)
		{
			this.spectrumBlend = 1f;
		}
		if (this.spectrumBlend >= 1f)
		{
			this.spectrumSourceType = this.spectrumTargetType;
			this.spectrumTargetType = SpectrumWeatherType.Biome;
			if (WeatherManager.currentWeather.biomeDefinition != null)
			{
				this.spectrumTargetType = WeatherManager.currentWeather.biomeDefinition.weatherSpectrum;
			}
			if (num > 0f)
			{
				this.spectrumTargetType = SpectrumWeatherType.BloodMoon;
			}
			if (this.spectrumSourceType != this.spectrumTargetType)
			{
				this.spectrumBlend = 0f;
			}
		}
	}

	public Color GetWeatherSpectrum(Color regularSpectrum, AtmosphereEffect.ESpecIdx type, float dayTimeScalar)
	{
		if (WeatherManager.forcedSpectrum == SpectrumWeatherType.None)
		{
			Color color = regularSpectrum;
			Color color2 = regularSpectrum;
			if (this.isGameModeNormal)
			{
				if (this.spectrumSourceType != SpectrumWeatherType.Biome)
				{
					ColorSpectrum colorSpectrum = WeatherManager.atmosphereSpectrum[(int)this.spectrumSourceType].spectrums[(int)type];
					if (colorSpectrum != null)
					{
						color = colorSpectrum.GetValue(dayTimeScalar);
					}
				}
				if (this.spectrumTargetType != SpectrumWeatherType.Biome)
				{
					ColorSpectrum colorSpectrum2 = WeatherManager.atmosphereSpectrum[(int)this.spectrumTargetType].spectrums[(int)type];
					if (colorSpectrum2 != null)
					{
						color2 = colorSpectrum2.GetValue(dayTimeScalar);
					}
				}
			}
			return color * (1f - this.spectrumBlend) + color2 * this.spectrumBlend;
		}
		int num = (int)WeatherManager.forcedSpectrum;
		AtmosphereEffect atmosphereEffect = WeatherManager.atmosphereSpectrum[num];
		if (atmosphereEffect == null)
		{
			return regularSpectrum;
		}
		ColorSpectrum colorSpectrum3 = atmosphereEffect.spectrums[(int)type];
		if (colorSpectrum3 == null)
		{
			return regularSpectrum;
		}
		return colorSpectrum3.GetValue(dayTimeScalar);
	}

	public void TriggerThunder(int _playWorldTime, Vector3 _pos)
	{
		EnvironmentAudioManager.Instance.TriggerThunder(_pos);
	}

	public void ClientProcessPackages(WeatherPackage[] _packages)
	{
		int num = Time.frameCount;
		if (this.processingPackageFrame == num)
		{
			return;
		}
		this.processingPackageFrame = num;
		foreach (WeatherPackage weatherPackage in _packages)
		{
			BiomeDefinition biomeDefinition;
			if (WorldBiomes.Instance.TryGetBiome(weatherPackage.biomeId, out biomeDefinition))
			{
				biomeDefinition.SetWeatherGroup((int)weatherPackage.groupIndex);
				WeatherManager.BiomeWeather biomeWeather = this.FindBiomeWeather((int)weatherPackage.biomeId);
				if (biomeWeather != null)
				{
					weatherPackage.CopyTo(biomeWeather);
				}
			}
		}
	}

	public void SendPackages()
	{
		this.CalcPackages();
		NetPackageWeather package = NetPackageManager.GetPackage<NetPackageWeather>();
		package.Setup(this.weatherPackages);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, true, -1, -1, -1, null, 192, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcPackages()
	{
		int count = this.biomeWeather.Count;
		for (int i = 0; i < count; i++)
		{
			WeatherManager.BiomeWeather biomeWeather = this.biomeWeather[i];
			WeatherPackage weatherPackage = this.weatherPackages[i];
			weatherPackage.biomeId = biomeWeather.biomeDefinition.m_Id;
			weatherPackage.groupIndex = (byte)biomeWeather.biomeDefinition.currentWeatherGroupIndex;
			weatherPackage.remainingSeconds = biomeWeather.remainingSeconds;
			int num = 0;
			while (num < biomeWeather.parameters.Length && num < weatherPackage.param.Length)
			{
				weatherPackage.param[num] = biomeWeather.parameterFinals[num];
				num++;
			}
		}
	}

	public override string ToString()
	{
		string text = string.Format("#{0}", this.biomeWeather.Count);
		for (int i = 0; i < this.biomeWeather.Count; i++)
		{
			WeatherManager.BiomeWeather biomeWeather = this.biomeWeather[i];
			text = text + "\n" + biomeWeather.ToString();
		}
		return text;
	}

	[Conditional("DEBUG_WEATHERNET")]
	public void LogNet(string _format = "", params object[] _args)
	{
		_format = string.Format("{0} WeatherManager net {1}", GameManager.frameCount, _format);
		Log.Warning(_format, _args);
	}

	public const ushort cVersion = 4;

	public static WeatherManager Instance;

	public const float cStormWarningDuration = 60f;

	public const int BaseTemperature = 70;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cWeatherTransitionSeconds = 10f;

	public List<WeatherManager.BiomeWeather> biomeWeather;

	public static float forceClouds = -1f;

	public static float forceRain = -1f;

	public static float forceSnowfall = -1f;

	public const float cForceTempDefault = -100f;

	public static float forceTemperature = -100f;

	public static float forceWind = -1f;

	public static bool needToReUpdateWeatherSpectrums;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float forceSimRandom = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cGracePeriodWorldTime = 22000;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int worldTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static GameRandom gameRand;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isGameModeNormal;

	public static bool inWeatherGracePeriod = true;

	public static WeatherManager.BiomeWeather currentWeather;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public WeatherPackage[] weatherPackages;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cLightningDelayMin = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cLightningDelayMax = 60f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float thunderDelay = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float stormFog;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int frameCount = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static string[] strCloudTypes = new string[] { "Whispy", "Fluffy", "ThickOvercast" };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Texture[] clouds = new Texture[WeatherManager.strCloudTypes.Length];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isCurrentWeatherUpdatedFirstTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cWindMax = 100f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cWindScale = 0.01f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject windZoneObj;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public WindZone windZone;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float windSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float windSpeedPrevious;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float windTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float windTimePrevious;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float windGust;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float windGustStep;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float windGustTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float windGustTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Camera mainCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int raycastMask;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject rainParticleObj;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform rainParticleT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ParticleSystem rainParticleSys;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material rainParticleMat;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float rainEmissionMaxRate = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cIntensityOff = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public WeatherManager.ParticleData snowData = new WeatherManager.ParticleData();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cStormTypeCount = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string[] stormNames = new string[] { "Burnt", "Desert", "Snow", "Wasteland" };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BiomeDefinition.BiomeType[] stormBiomes = new BiomeDefinition.BiomeType[]
	{
		BiomeDefinition.BiomeType.burnt_forest,
		BiomeDefinition.BiomeType.Desert,
		BiomeDefinition.BiomeType.Snow,
		BiomeDefinition.BiomeType.Wasteland
	};

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public WeatherManager.ParticleData[] stormData = new WeatherManager.ParticleData[4];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform stormDistantRootT;

	public float spectrumBlend = 1f;

	public static SpectrumWeatherType forcedSpectrum = SpectrumWeatherType.None;

	public SpectrumWeatherType spectrumSourceType;

	public SpectrumWeatherType spectrumTargetType;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static AtmosphereEffect[] atmosphereSpectrum;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 playerPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float checkPlayerMoveTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static MemoryStream loadData;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string weatherAllName;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float weatherLastUpdateWorldTime;

	public string CustomWeatherName = "";

	public float CustomWeatherTime = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float cloudThickness;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float cloudThicknessTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 particleFallLastPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 particleFallPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float stormFogTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float stormDistantFogTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float stormLightScale;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float stormLightScaleTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float stormDistantLightScaleTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cGridCount = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public WeatherManager.DistantStormGrid[] distantStormGrids = new WeatherManager.DistantStormGrid[3];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int distantStormFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int processingPackageFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum CloudTypes
	{
		Whispy,
		Fluffy,
		ThickOvercast
	}

	public class ParticleData
	{
		public WeatherManager.BiomeWeather weather;

		public int stormLevel;

		public float intensity = -1f;

		public float thunderDelay;

		public GameObject rootObj;

		public Transform rootT;

		public Material rootParticleMat;

		public ParticleSystem rootParticleSys;

		public float rootEmissionMaxRate = 1f;

		public ParticleSystem nearParticleSys;

		public float nearEmissionMaxRate = 1f;

		public ParticleSystem topParticleSys;

		public float topEmissionMaxRate = 1f;

		public Transform farT;

		public ParticleSystem farParticleSys;

		public Color farBaseColor;

		public ParticleSystem ringParticleSys;

		public float ringRadiusMax;

		public List<ParticleSystem> particleSystems = new List<ParticleSystem>();

		public float[] psEmissionMaxRates;

		public Transform forceT;
	}

	[Serializable]
	public class Param
	{
		public Param(float _value, float _step1Time = 0.25f)
		{
			this.name = "Param";
			this.value = _value;
			this.target = _value;
			this.step1Time = _step1Time;
		}

		public void FrameUpdate()
		{
			float time = Time.time;
			if (this.value == this.target)
			{
				this.lastTime = time;
				return;
			}
			float num = time - this.lastTime;
			if (num < 0f)
			{
				this.lastTime = time;
			}
			if (num >= 0.01f)
			{
				if (num > 1f)
				{
					num = 1f;
				}
				float num2 = num / this.step1Time;
				if (this.value > this.target)
				{
					this.value -= num2;
					if (this.value < this.target)
					{
						this.value = this.target;
					}
				}
				else
				{
					this.value += num2;
					if (this.value > this.target)
					{
						this.value = this.target;
					}
				}
				this.lastTime = time;
			}
		}

		public void Clamp()
		{
			this.value = Utils.FastClamp01(this.value);
			this.target = Utils.FastClamp01(this.target);
		}

		public void Set(float _value)
		{
			this.value = _value;
			this.target = _value;
		}

		public void SetTarget(float _target)
		{
			if (this.target == _target)
			{
				return;
			}
			this.target = _target;
			this.lastTime = Time.time;
		}

		public string name;

		public float value;

		public float target;

		public float step1Time;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public float lastTime;
	}

	[Serializable]
	public class BiomeWeather
	{
		public BiomeWeather(BiomeDefinition _definition)
		{
			this.biomeDefinition = _definition;
			for (int i = 0; i < 5; i++)
			{
				this.parameters[i] = new WeatherManager.Param(0f, 0.3f);
			}
			this.parameters[0].step1Time = 0.15f;
		}

		public float CloudThickness()
		{
			return this.parameters[2].value;
		}

		public float FogPercent()
		{
			return this.parameters[4].value * 0.01f + WeatherManager.stormFog;
		}

		public float Wind()
		{
			return this.parameters[3].value;
		}

		public void FrameUpdate(bool _isServer)
		{
			WeatherManager.Param[] array = this.parameters;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].FrameUpdate();
			}
			float value = this.parameters[1].value;
			float num = this.parameters[2].value;
			float num2 = this.parameters[0].value;
			if (_isServer)
			{
				this.CalcGlobalTemperature(value, num, ref num2);
				if (WeatherManager.forceClouds >= 0f)
				{
					num = WeatherManager.forceClouds * 100f;
				}
				if (WeatherManager.forceTemperature > -100f)
				{
					num2 = WeatherManager.forceTemperature;
				}
			}
			this.parameterFinals[1] = value;
			this.parameterFinals[2] = num;
			this.parameterFinals[0] = num2;
			this.parameterFinals[3] = this.parameters[3].value;
			this.parameterFinals[4] = this.parameters[4].value;
			float num3 = (value * 0.01f - 0.3f) / 0.7f;
			num3 = Utils.FastMax(0f, num3);
			this.rainParam.target = ((num2 > 32f) ? num3 : 0f);
			this.snowFallParam.target = ((num2 <= 32f) ? num3 : 0f);
			this.rainParam.FrameUpdate();
			this.snowFallParam.FrameUpdate();
			if (!_isServer)
			{
				this.stormState = this.biomeDefinition.currentWeatherGroup.stormLevel;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CalcGlobalTemperature(float _precipitation, float _cloudThickness, ref float _temperature)
		{
			float num = Utils.FastClamp01(_precipitation * 0.01f);
			_temperature += num * -5f;
			float num2 = SkyManager.GetSunPercent();
			if (num2 > 0f)
			{
				num2 *= 1f - _cloudThickness * 0.01f;
			}
			_temperature += (-num2 + 1f) * -7.5f;
		}

		public void ParamsFrameUpdate()
		{
			for (int i = 0; i < this.parameters.Length; i++)
			{
				WeatherManager.Param param = this.parameters[i];
				param.FrameUpdate();
				this.parameterFinals[i] = param.value;
			}
			this.rainParam.FrameUpdate();
			this.snowFallParam.FrameUpdate();
		}

		public void Reset()
		{
			this.stormWorldTime = 0;
			this.nextRandWorldTime = 0;
			this.biomeDefinition.WeatherRandomize(0f);
			for (int i = 0; i < 5; i++)
			{
				this.parameters[i].Set(this.biomeDefinition.WeatherGetValue((BiomeDefinition.Probabilities.ProbType)i));
			}
			this.rainParam.Set(0f);
			this.snowFallParam.Set(0f);
		}

		public void ServerTimeUpdate(int _worldTime, float _freq)
		{
			if (_freq == 0f)
			{
				if (this.stormWorldTime != 2147483647)
				{
					this.stormWorldTime = int.MaxValue;
					this.nextRandWorldTime = 0;
				}
			}
			else if (this.stormWorldTime == 2147483647)
			{
				this.stormWorldTime = 0;
			}
			int num = _worldTime - this.stormWorldTime;
			if (num >= 0)
			{
				float num2 = 60f / (float)GamePrefs.GetInt(EnumGamePrefs.DayNightLength);
				int num3 = this.biomeDefinition.WeatherGetDuration("stormbuild");
				num3 = (int)((float)num3 * num2);
				int num4 = num3 - num;
				if (num4 > 0)
				{
					if (this.stormState != 1)
					{
						this.stormState = 1;
						this.SetWeather("stormbuild");
					}
					int @int = GameStats.GetInt(EnumGameStats.TimeOfDayIncPerSec);
					if (@int > 0)
					{
						this.remainingSeconds = (byte)Utils.FastMin(num4 / @int, 255);
					}
					return;
				}
				if (WeatherManager.worldTime < this.stormWorldTime + this.stormDuration)
				{
					if (this.stormState != 2)
					{
						this.stormState = 2;
						this.SetWeather("storm");
					}
					return;
				}
				this.stormState = 0;
				Vector2i vector2i;
				int num5 = this.biomeDefinition.WeatherGetDuration("storm", out vector2i);
				this.stormWorldTime = WeatherManager.worldTime + (int)((float)WeatherManager.gameRand.RandomRange(vector2i.x, vector2i.y) / _freq);
				int num6 = num5 / 8;
				num5 += WeatherManager.gameRand.RandomRange(num5 - num6, num5 + num6);
				num5 = (int)((float)num5 * num2);
				this.stormDuration = num3 + num5;
			}
			if (_worldTime >= this.nextRandWorldTime)
			{
				float num7 = WeatherManager.gameRand.RandomFloat;
				if (WeatherManager.forceSimRandom >= 0f)
				{
					num7 = WeatherManager.forceSimRandom;
				}
				this.SetWeather(_worldTime, num7);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetWeather(int _worldTime, float _rand)
		{
			if (this.biomeDefinition != null)
			{
				this.biomeDefinition.WeatherRandomize(_rand);
				for (int i = 0; i < 5; i++)
				{
					BiomeDefinition.Probabilities.ProbType probType = (BiomeDefinition.Probabilities.ProbType)i;
					this.parameters[i].target = this.biomeDefinition.WeatherGetValue(probType);
				}
				this.nextRandWorldTime = _worldTime + this.biomeDefinition.currentWeatherGroup.duration;
			}
		}

		public void SetWeather(string name)
		{
			if (this.biomeDefinition != null)
			{
				this.biomeDefinition.WeatherRandomize(name);
				for (int i = 0; i < 5; i++)
				{
					BiomeDefinition.Probabilities.ProbType probType = (BiomeDefinition.Probabilities.ProbType)i;
					this.parameters[i].target = this.biomeDefinition.WeatherGetValue(probType);
				}
				this.nextRandWorldTime = 0;
			}
		}

		public override string ToString()
		{
			string text = string.Format("{0}: {1}, nxtT{2}, ", this.biomeDefinition.m_sBiomeName, this.biomeDefinition.weatherName, this.nextRandWorldTime);
			for (int i = 0; i < 5; i++)
			{
				BiomeDefinition.Probabilities.ProbType probType = (BiomeDefinition.Probabilities.ProbType)i;
				text += string.Format("{0} {1}, ", probType, this.biomeDefinition.WeatherGetValue(probType));
			}
			text += string.Format("rain {0}, ", this.rainParam.value);
			text += string.Format("snow {0}, ", this.snowFallParam.value);
			return text + string.Format("storm WT {0}, dur {1}, state {2}", this.stormWorldTime, this.stormDuration, this.stormState);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public const float cStep1Time = 0.15f;

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public const float cPrecipitationPercentVisibleMin = 0.3f;

		public BiomeDefinition biomeDefinition;

		public int stormWorldTime;

		public int stormDuration;

		public int stormState;

		public int stormIndex;

		public int nextRandWorldTime;

		public byte remainingSeconds;

		public WeatherManager.Param[] parameters = new WeatherManager.Param[5];

		public float[] parameterFinals = new float[5];

		public WeatherManager.Param rainParam = new WeatherManager.Param(0f, 0.25f);

		public WeatherManager.Param snowFallParam = new WeatherManager.Param(0f, 0.25f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class DistantStormGrid
	{
		public void Init(World _world, int _cellSize, int _distMin, int _distMax, Transform[] _prefabTs)
		{
			ITerrainGenerator terrainGenerator = _world.ChunkCache.ChunkProvider.GetTerrainGenerator();
			if (terrainGenerator == null)
			{
				return;
			}
			this.cellSize = _cellSize;
			Vector2i worldSize = _world.ChunkCache.ChunkProvider.GetWorldSize();
			this.worldOffset = worldSize / 2;
			this.width = worldSize.x / _cellSize;
			this.items = new WeatherManager.DistantStormGrid.Item[this.width * this.width];
			this.distMin = _distMin;
			this.distMax = _distMax;
			this.prefabTs = _prefabTs;
			int num = this.cellSize / 2;
			int num2 = this.cellSize / 3;
			for (int i = 0; i < this.width; i++)
			{
				Vector2i vector2i;
				vector2i.y = i * this.cellSize + num - this.worldOffset.y;
				for (int j = 0; j < this.width; j++)
				{
					vector2i.x = j * this.cellSize + num - this.worldOffset.x;
					int num3 = j + i * this.width;
					this.items[num3].worldPos = vector2i;
					int num4 = 0;
					float num5 = 0f;
					int num6 = num2 * 3;
					for (int k = -num6; k <= num6; k += num2)
					{
						for (int l = -num6; l <= num6; l += num2)
						{
							float terrainHeightAt = terrainGenerator.GetTerrainHeightAt(l + vector2i.x, k + vector2i.y);
							if (terrainHeightAt > 0f)
							{
								num4++;
								num5 += terrainHeightAt;
							}
						}
					}
					this.items[num3].height = (byte)(num5 / (float)num4);
				}
			}
			this.stormHeight = 8f;
		}

		public void Update(EntityPlayerLocal _localPlayer, IBiomeProvider _biomeProvider, ITerrainGenerator _terrainGen)
		{
			Vector2i vector2i = new Vector2i((int)_localPlayer.position.x, (int)_localPlayer.position.z);
			bool flag = false;
			BiomeDefinition biomeAt = _biomeProvider.GetBiomeAt(vector2i.x, vector2i.y);
			if (biomeAt != null)
			{
				flag = WeatherManager.Instance.FindBiomeWeather(biomeAt.m_BiomeType).stormState >= 2;
			}
			BiomeDefinition biomeDefinition = null;
			int num = 0;
			int num2 = 0;
			int num3 = this.distMin * this.distMin;
			int num4 = this.distMax * this.distMax;
			Vector2i vector2i2 = vector2i + this.worldOffset;
			int num5 = this.width - 1;
			int num6 = Utils.FastClamp((vector2i2.x - this.distMax) / this.cellSize, 0, num5);
			int num7 = Utils.FastClamp((vector2i2.x + this.distMax) / this.cellSize, 0, num5);
			int num8 = Utils.FastClamp((vector2i2.y - this.distMax) / this.cellSize, 0, num5);
			int num9 = Utils.FastClamp((vector2i2.y + this.distMax) / this.cellSize, 0, num5);
			for (int i = num8; i <= num9; i++)
			{
				int num10 = i * this.width;
				for (int j = num6; j <= num7; j++)
				{
					int num11 = j + num10;
					ref WeatherManager.DistantStormGrid.Item ptr = ref this.items[num11];
					if (!ptr.active)
					{
						float num12 = Vector2i.DistanceSqr(ptr.worldPos, vector2i);
						if (num12 >= (float)num3 && num12 <= (float)num4)
						{
							BiomeDefinition biomeAt2 = _biomeProvider.GetBiomeAt(ptr.worldPos.x, ptr.worldPos.y);
							if (biomeAt2 != null)
							{
								if (biomeAt2 != biomeDefinition)
								{
									biomeDefinition = biomeAt2;
									WeatherManager.BiomeWeather biomeWeather = WeatherManager.Instance.FindBiomeWeather(biomeAt2.m_BiomeType);
									num2 = biomeWeather.stormState;
									num = biomeWeather.stormIndex;
									if (num2 == 1 && biomeWeather.remainingSeconds > 20)
									{
										num2 = 0;
									}
								}
							}
							else
							{
								num2 = 0;
							}
							if (num2 > 0 && biomeDefinition != biomeAt && !flag)
							{
								ptr.active = true;
								this.activeItems.Add(num11);
								if (!ptr.t)
								{
									ptr.t = Object.Instantiate<Transform>(this.prefabTs[num], WeatherManager.Instance.stormDistantRootT, false);
									ptr.t.gameObject.SetActive(true);
								}
								float num13 = (float)ptr.height + this.stormHeight;
								ptr.t.localPosition = new Vector3((float)ptr.worldPos.x, num13, (float)ptr.worldPos.y);
							}
						}
					}
				}
			}
			float num14 = float.MaxValue;
			Vector3 vector = _localPlayer.position - Origin.position;
			Quaternion quaternion = Quaternion.Euler(0f, 180f, 0f);
			int k = 0;
			while (k < this.activeItems.Count)
			{
				bool flag2 = flag;
				int num15 = this.activeItems[k];
				ref WeatherManager.DistantStormGrid.Item ptr2 = ref this.items[num15];
				float num16 = Vector2i.DistanceSqr(ptr2.worldPos, vector2i);
				if (this.distMin == 0 && num16 < num14)
				{
					num14 = num16;
				}
				if (num16 < (float)num3 || num16 > (float)num4)
				{
					flag2 = true;
				}
				if (!flag2)
				{
					BiomeDefinition biomeAt3 = _biomeProvider.GetBiomeAt(ptr2.worldPos.x, ptr2.worldPos.y);
					if (biomeAt3 != null)
					{
						if (biomeAt3 != biomeDefinition)
						{
							biomeDefinition = biomeAt3;
							WeatherManager.BiomeWeather biomeWeather2 = WeatherManager.Instance.FindBiomeWeather(biomeAt3.m_BiomeType);
							num2 = biomeWeather2.stormState;
							if (num2 == 1 && biomeWeather2.remainingSeconds > 20)
							{
								num2 = 0;
							}
						}
					}
					else
					{
						num2 = 0;
					}
					if (num2 == 0 || biomeDefinition == biomeAt)
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					ptr2.removeTime = 4f;
					goto IL_048A;
				}
				ptr2.removeTime -= this.updateElapsedTime;
				if (ptr2.removeTime > 0f)
				{
					WeatherManager.DistantStormGrid.gradient.color = new Color(ptr2.removeTime / 4f, 1f, 1f, 1f);
					ptr2.t.GetComponentsInChildren<ParticleSystem>(WeatherManager.DistantStormGrid.tempPSList);
					for (int l = 0; l < WeatherManager.DistantStormGrid.tempPSList.Count; l++)
					{
						WeatherManager.DistantStormGrid.tempPSList[l].customData.SetColor(0, WeatherManager.DistantStormGrid.gradient);
					}
					WeatherManager.DistantStormGrid.tempPSList.Clear();
					goto IL_048A;
				}
				if (ptr2.t)
				{
					Object.Destroy(ptr2.t.gameObject);
					ptr2.t = null;
				}
				ptr2.active = false;
				this.activeItems.RemoveAt(k);
				k--;
				IL_04B1:
				k++;
				continue;
				IL_048A:
				ptr2.t.LookAt(vector);
				ptr2.t.localRotation *= quaternion;
				goto IL_04B1;
			}
			if (this.distMin == 0)
			{
				float num17 = 40f;
				float num18 = 100f;
				float num19 = 0.05f;
				float num20 = 1f;
				if (num14 < num18 * num18)
				{
					num20 = (Mathf.Sqrt(num14) - num17) / (num18 - num17);
				}
				num20 = Utils.FastClamp01(num20);
				WeatherManager.stormDistantFogTarget = Utils.FastLerpUnclamped(num19, 0f, num20);
				WeatherManager.stormDistantLightScaleTarget = Utils.FastLerpUnclamped(0.6f, 1f, num20);
			}
			this.updateElapsedTime = 0f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cRemoveTime = 4f;

		public int cellSize;

		public int width;

		public Vector2i worldOffset;

		public int distMin;

		public int distMax;

		public Transform[] prefabTs;

		public Transform originalT;

		public float stormHeight;

		public float updateElapsedTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public WeatherManager.DistantStormGrid.Item[] items;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<int> activeItems = new List<int>();

		[PublicizedFrom(EAccessModifier.Private)]
		public static List<ParticleSystem> tempPSList = new List<ParticleSystem>();

		[PublicizedFrom(EAccessModifier.Private)]
		public static ParticleSystem.MinMaxGradient gradient = default(ParticleSystem.MinMaxGradient);

		public struct Item
		{
			public Vector2i worldPos;

			public Transform t;

			public byte height;

			public bool active;

			public float removeTime;
		}
	}
}
