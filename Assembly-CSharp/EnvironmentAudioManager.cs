using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentAudioManager : MonoBehaviour, IGamePrefsChangedListener
{
	public static void DestroyInstance()
	{
		if (EnvironmentAudioManager.Instance != null && EnvironmentAudioManager.Instance.gameObject != null)
		{
			Object.DestroyImmediate(EnvironmentAudioManager.Instance.gameObject);
		}
	}

	public static IEnumerator CreateNewInstance()
	{
		if (!GameManager.IsDedicatedServer)
		{
			EnvironmentAudioManager.DestroyInstance();
			if (EnvironmentAudioManager.loadedPrefab == null)
			{
				EnvironmentAudioManager.<>c__DisplayClass42_0 CS$<>8__locals1 = new EnvironmentAudioManager.<>c__DisplayClass42_0();
				CS$<>8__locals1.requestTask = LoadManager.LoadAsset<GameObject>("@:Sounds/Prefabs/EnvironmentAudioMaster.prefab", null, null, false, false, false);
				yield return new WaitUntil(() => CS$<>8__locals1.requestTask.IsDone);
				EnvironmentAudioManager.loadedPrefab = CS$<>8__locals1.requestTask.Asset;
				CS$<>8__locals1 = null;
			}
			EnvironmentAudioManager.Instance = (Object.Instantiate(EnvironmentAudioManager.loadedPrefab) as GameObject).GetComponent<EnvironmentAudioManager>();
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		if (EnvironmentAudioManager.sourceSounds == null)
		{
			EnvironmentAudioManager.sourceSounds = new GameObject();
			EnvironmentAudioManager.sourceSounds.name = "SourceSounds";
			EnvironmentAudioManager.sourceSounds.transform.parent = base.transform;
		}
		this.InitRain();
		GamePrefs.AddChangeListener(this);
		AmbientAudioController.Instance.SetAmbientVolume(GamePrefs.GetFloat(EnumGamePrefs.OptionsAmbientVolumeLevel));
		EnvironmentAudioManager.musicVolume = GamePrefs.GetFloat(EnumGamePrefs.OptionsMusicVolumeLevel);
		this.numBiomes = Enum.GetNames(typeof(BiomeDefinition.BiomeType)).Length;
		this.numTriggers = Enum.GetNames(typeof(AudioObject.Trigger)).Length;
		this.prevTriggerValue = new float[this.numTriggers];
		for (int i = 0; i < this.numTriggers; i++)
		{
			this.prevTriggerValue[i] = 0f;
		}
		this.audioBiomes = new AudioBiome[this.numBiomes];
		for (int j = 0; j < this.numBiomes; j++)
		{
			this.audioBiomes[j] = new AudioBiome();
		}
		this.InitSounds();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitRain()
	{
		for (int i = 0; i < this.rainClipsLowToHigh.Length; i++)
		{
			AudioSource audioSource = Object.Instantiate<AudioSource>(this.rainMasterAudioSource);
			audioSource.clip = this.rainClipsLowToHigh[i];
			audioSource.transform.parent = base.transform;
			audioSource.loop = true;
			this.rainAudioSources.Add(audioSource);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitSounds()
	{
		AudioObject[] array = new AudioObject[0 + this.mixedBiomeSounds.Length + this.forestOnlyBiomeSounds.Length + this.snowOnlyBiomeSounds.Length + this.desertOnlyBiomeSounds.Length + this.wastelandOnlyBiomeSounds.Length + this.waterOnlyBiomeSounds.Length + this.burnt_forestOnlyBiomeSounds.Length];
		int num = 0;
		foreach (AudioObject audioObject in this.mixedBiomeSounds)
		{
			array[num++] = audioObject;
		}
		foreach (AudioObject audioObject2 in this.forestOnlyBiomeSounds)
		{
			array[num++] = audioObject2;
		}
		foreach (AudioObject audioObject3 in this.snowOnlyBiomeSounds)
		{
			array[num++] = audioObject3;
		}
		foreach (AudioObject audioObject4 in this.desertOnlyBiomeSounds)
		{
			array[num++] = audioObject4;
		}
		foreach (AudioObject audioObject5 in this.wastelandOnlyBiomeSounds)
		{
			array[num++] = audioObject5;
		}
		foreach (AudioObject audioObject6 in this.waterOnlyBiomeSounds)
		{
			array[num++] = audioObject6;
		}
		foreach (AudioObject audioObject7 in this.burnt_forestOnlyBiomeSounds)
		{
			array[num++] = audioObject7;
		}
		int num2 = 0;
		foreach (AudioObject audioObject8 in array)
		{
			foreach (BiomeDefinition.BiomeType biomeType in audioObject8.validBiomes)
			{
				this.audioBiomes[(int)biomeType].Add(audioObject8);
				if (audioObject8.trigger == AudioObject.Trigger.Day7Times || audioObject8.trigger == AudioObject.Trigger.TimeOfDay)
				{
					num2++;
				}
			}
			audioObject8.Init();
		}
		this.fromBiomeLoops = this.audioBiomes[(int)this.fromBiome];
		this.toBiomeLoops = this.audioBiomes[(int)this.toBiome];
		this.soundsInitDone = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FixedUpdate()
	{
		if (GameManager.Instance.IsPaused() || !this.soundsInitDone)
		{
			return;
		}
		World world = GameManager.Instance.World;
		if (world == null)
		{
			this.TurnOffSounds();
			this.UpdateRainAudio();
			return;
		}
		float deltaTime = Time.deltaTime;
		this.prevBiomeTransition = this.biomeTransition;
		this.biomeTransition += 0.1f * deltaTime;
		this.biomeTransition = Mathf.Clamp01(this.biomeTransition);
		this.fadingBiomes = false;
		if (this.prevBiomeTransition != this.biomeTransition)
		{
			this.fromBiomeLoops.TransitionFrom(this.biomeTransition);
		}
		this.toBiomeLoops.TransitionTo(this.biomeTransition);
		this.invAmountEnclosedPow = Mathf.Lerp(this.invAmountEnclosedPow, this.invAmountEnclosedTarget, deltaTime * 1.5f);
		this.UpdateRainAudio();
		this.UpdateValueTrigger(WeatherManager.Instance.GetCurrentSnowfallPercent(), AudioObject.Trigger.Snow);
		this.UpdateValueTrigger(Mathf.Clamp01(WeatherManager.currentWeather.Wind() * 0.01f + 0.12f), AudioObject.Trigger.Wind);
		if (this.thunderPlaying)
		{
			List<AudioObject> sound = this.toBiomeLoops.triggers[2].sound;
			if (sound.Count > 0)
			{
				AudioObject audioObject = sound[0];
				audioObject.SetVolume(this.invAmountEnclosedPow);
				if (!(this.thunderPlaying = audioObject.IsPlaying()))
				{
					audioObject.DestroySources();
				}
			}
		}
		if (this.thunderTriggerWorldTime > 0)
		{
			List<AudioObject> sound2 = this.toBiomeLoops.triggers[2].sound;
			if (sound2.Count > 0 && (int)world.GetWorldTime() > this.thunderTriggerWorldTime)
			{
				this.thunderTriggerWorldTime = 0;
				if (world.GetPrimaryPlayer() != null)
				{
					sound2[0].SetPosition(this.lightningPos);
					SkyManager.TriggerLightning(this.lightningPos);
					this.thunderTimer = Time.time + world.RandomRange(200f, 1000f) / 343f;
				}
			}
		}
		if (Time.time > this.thunderTimer)
		{
			this.thunderTimer = float.PositiveInfinity;
			List<AudioObject> sound3 = this.toBiomeLoops.triggers[2].sound;
			for (int i = 0; i < sound3.Count; i++)
			{
				AudioObject audioObject2 = sound3[i];
				this.thunderPlaying = true;
				audioObject2.Play();
			}
		}
		if (!this.fadingBiomes)
		{
			if (this.enteredBiome)
			{
				BiomeDefinition.BiomeType biomeType;
				if (!AudioObject.biomeIdMap.TryGetValue(this.biomeEntered, out biomeType))
				{
					return;
				}
				if (this.biomeTransition != 1f)
				{
					this.queuedBiome = biomeType;
				}
				else
				{
					this.SetNewBiome(biomeType);
				}
				this.enteredBiome = false;
			}
			if (this.queuedBiome != BiomeDefinition.BiomeType.Any && this.biomeTransition >= 1f)
			{
				this.SetNewBiome(this.queuedBiome);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateRainAudio()
	{
		float currentRainfallPercent = WeatherManager.Instance.GetCurrentRainfallPercent();
		float num = Time.deltaTime * 0.025f;
		if (currentRainfallPercent <= 0f)
		{
			this.IncrementRainVolumes(-num, -num, -num);
		}
		else if (currentRainfallPercent < 0.28f)
		{
			this.IncrementRainVolumes(num, -num, -num);
		}
		else if (currentRainfallPercent < 0.56f)
		{
			this.IncrementRainVolumes(-num, num, -num);
		}
		else
		{
			this.IncrementRainVolumes(-num, -num, num);
		}
		foreach (AudioSource audioSource in this.rainAudioSources)
		{
			if (audioSource.volume == 0f)
			{
				audioSource.Stop();
			}
			else if (!audioSource.isPlaying)
			{
				audioSource.Play();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void IncrementRainVolumes(float inc0, float inc1, float inc2)
	{
		float num = 0.25f * this.invAmountEnclosedPow * EnvironmentAudioManager.GlobalEnvironmentVolumeScale;
		this.currentRainVolume[0] = Utils.FastClamp01(this.currentRainVolume[0] + inc0);
		this.rainAudioSources[0].volume = Utils.FastClamp01(this.currentRainVolume[0] * num);
		this.currentRainVolume[1] = Utils.FastClamp01(this.currentRainVolume[1] + inc1);
		this.rainAudioSources[1].volume = Utils.FastClamp01(this.currentRainVolume[1] * num);
		this.currentRainVolume[2] = Utils.FastClamp01(this.currentRainVolume[2] + inc2);
		this.rainAudioSources[2].volume = Utils.FastClamp01(this.currentRainVolume[2] * num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateValueTrigger(float value, AudioObject.Trigger trigger)
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			return;
		}
		if (primaryPlayer.Stats == null)
		{
			return;
		}
		if (this.biomeTransition != this.prevBiomeTransition || this.prevTriggerValue[(int)trigger] != value || this.prevAmountEnclosed != primaryPlayer.Stats.AmountEnclosed)
		{
			this.prevTriggerValue[(int)trigger] = value;
			if (this.biomeTransition < 1f || this.prevBiomeTransition < 1f)
			{
				foreach (AudioObject audioObject in this.fromBiomeLoops.triggers[(int)trigger].sound)
				{
					audioObject.SetBiomeVolume(1f - this.biomeTransition);
					audioObject.SetValue(value);
				}
			}
			foreach (AudioObject audioObject2 in this.toBiomeLoops.triggers[(int)trigger].sound)
			{
				audioObject2.SetBiomeVolume(this.biomeTransition);
				audioObject2.SetValue(value);
			}
			this.prevAmountEnclosed = primaryPlayer.Stats.AmountEnclosed;
			this.invAmountEnclosedTarget = Mathf.Pow(1f - this.prevAmountEnclosed, 2f);
			if (this.invAmountEnclosedPow < 0f)
			{
				this.invAmountEnclosedPow = this.invAmountEnclosedTarget;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetNewBiome(BiomeDefinition.BiomeType newBiome)
	{
		if (this.audioBiomes == null)
		{
			return;
		}
		this.fromBiome = this.toBiome;
		this.toBiome = newBiome;
		this.queuedBiome = BiomeDefinition.BiomeType.Any;
		this.biomeTransition = 0f;
		this.fromBiomeLoops = this.audioBiomes[(int)this.fromBiome];
		this.toBiomeLoops = this.audioBiomes[(int)this.toBiome];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		for (int i = 0; i < this.rainAudioSources.Count; i++)
		{
			if (!(this.rainAudioSources[i] == null))
			{
				this.rainAudioSources[i].Stop();
				GameObject gameObject = this.rainAudioSources[i].transform.gameObject;
				Object.DestroyImmediate(this.rainAudioSources[i]);
				if (gameObject != null)
				{
					Object.DestroyImmediate(gameObject);
				}
			}
		}
		this.rainAudioSources.Clear();
		this.TurnOffSounds();
		this.fromBiomeLoops = null;
		this.toBiomeLoops = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TurnOffSounds()
	{
		this.fromBiomeLoops.TurnOff();
		this.toBiomeLoops.TurnOff();
	}

	public void Pause()
	{
		this.toBiomeLoops.Pause();
		this.fromBiomeLoops.Pause();
		for (int i = 0; i < this.rainAudioSources.Count; i++)
		{
			this.rainAudioSources[i].Pause();
		}
	}

	public void UnPause()
	{
		this.toBiomeLoops.UnPause();
		this.fromBiomeLoops.UnPause();
		for (int i = 0; i < this.rainAudioSources.Count; i++)
		{
			this.rainAudioSources[i].UnPause();
		}
	}

	public void TriggerThunder(Vector3 _pos)
	{
		this.lightningPos = _pos;
		this.thunderTriggerWorldTime = 1;
		this.thunderTimer = float.PositiveInfinity;
	}

	public void EnterBiome(BiomeDefinition _biome)
	{
		if (AudioObject.biomeIdMap == null)
		{
			AudioObject.biomeIdMap = new Dictionary<byte, BiomeDefinition.BiomeType>();
			foreach (KeyValuePair<string, byte> keyValuePair in BiomeDefinition.nameToId)
			{
				for (int i = 0; i < BiomeDefinition.BiomeNames.Length; i++)
				{
					if (keyValuePair.Key.EqualsCaseInsensitive(BiomeDefinition.BiomeNames[i]))
					{
						AudioObject.biomeIdMap[keyValuePair.Value] = (BiomeDefinition.BiomeType)i;
						break;
					}
				}
			}
		}
		if (_biome != null)
		{
			this.enteredBiome = true;
			this.biomeEntered = _biome.m_Id;
		}
	}

	public void OnGamePrefChanged(EnumGamePrefs _enum)
	{
		if (_enum == EnumGamePrefs.OptionsMusicVolumeLevel)
		{
			EnvironmentAudioManager.musicVolume = GamePrefs.GetFloat(EnumGamePrefs.OptionsMusicVolumeLevel);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cBiomeTransitionSpeed = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cRainVolumeTransitionSpeed = 0.025f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cMaxRainVolume = 0.25f;

	public static float GlobalEnvironmentVolumeScale = 0.2f;

	public static float musicVolume = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 lightningPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool thunderPlaying;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int thunderTriggerWorldTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float thunderTimer;

	public bool fadingBiomes;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool enteredBiome;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public byte biomeEntered;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float biomeTransition = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float prevBiomeTransition = 1f;

	public float invAmountEnclosedPow = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float invAmountEnclosedTarget = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float prevAmountEnclosed = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float[] prevTriggerValue;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int numBiomes;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int numTriggers;

	public static EnvironmentAudioManager Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AudioBiome[] audioBiomes;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BiomeDefinition.BiomeType fromBiome;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BiomeDefinition.BiomeType toBiome;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BiomeDefinition.BiomeType queuedBiome;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AudioBiome fromBiomeLoops;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AudioBiome toBiomeLoops;

	public static GameObject sourceSounds = null;

	public AudioObject[] mixedBiomeSounds;

	public AudioObject[] forestOnlyBiomeSounds;

	public AudioObject[] snowOnlyBiomeSounds;

	public AudioObject[] desertOnlyBiomeSounds;

	public AudioObject[] wastelandOnlyBiomeSounds;

	public AudioObject[] waterOnlyBiomeSounds;

	public AudioObject[] burnt_forestOnlyBiomeSounds;

	public AudioSource rainMasterAudioSource;

	public AudioClip[] rainClipsLowToHigh;

	public List<AudioSource> rainAudioSources;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float[] currentRainVolume = new float[3];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Object loadedPrefab = null;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool soundsInitDone;
}
