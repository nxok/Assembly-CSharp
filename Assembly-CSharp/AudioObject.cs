using System;
using System.Collections.Generic;
using Audio;
using MusicUtils.Enums;
using SandboxOptions;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class AudioObject
{
	public void Init()
	{
		foreach (AudioClip audioClip in this.audioClips)
		{
			if (audioClip != null)
			{
				AudioSource audioSource = Object.Instantiate<AudioSource>(this.masterAudioSource);
				if (this.playOrder == AudioObject.PlayOrder.ByValue)
				{
					audioSource.transform.parent = EnvironmentAudioManager.Instance.transform;
					audioSource.loop = true;
					audioSource.gameObject.SetActive(false);
				}
				else
				{
					audioSource.transform.parent = EnvironmentAudioManager.sourceSounds.transform;
				}
				audioSource.clip = audioClip;
				audioSource.name = audioClip.name;
				audioSource.volume = 0f;
				audioSource.outputAudioMixerGroup = this.audioMixerGroup;
				this.runtimeAudioSrcs.Add(audioSource);
			}
		}
		this.audioClips = null;
		if (this.trigger == AudioObject.Trigger.Random)
		{
			this.repeatTime = Time.time + Manager.random.RandomRange(this.repeatFreqRange.x, this.repeatFreqRange.y);
		}
	}

	public void SetValue(float _value)
	{
		this.value = _value;
		if (this.playOrder == AudioObject.PlayOrder.ByValue)
		{
			float num = this.transitionCurve.Evaluate(this.value) * (float)this.runtimeAudioSrcs.Count;
			int num2 = 0;
			foreach (AudioSource audioSource in this.runtimeAudioSrcs)
			{
				float num3 = Mathf.Clamp01(num - (float)num2);
				if (num > (float)(num2 + 1))
				{
					num3 = 1f - Mathf.Clamp01(num - (float)(num2 + 1));
				}
				audioSource.volume = num3 * (this.music ? EnvironmentAudioManager.musicVolume : 1f) * this.biomeVolume * EnvironmentAudioManager.GlobalEnvironmentVolumeScale;
				if (GameManager.Instance != null && GameManager.Instance.World != null && GameManager.Instance.World.GetPrimaryPlayer() != null && GameManager.Instance.World.GetPrimaryPlayer().Stats != null)
				{
					if (this.outdoorOnly)
					{
						audioSource.volume *= EnvironmentAudioManager.Instance.invAmountEnclosedPow;
					}
					else if (this.indoorOnly)
					{
						audioSource.volume *= 1f - EnvironmentAudioManager.Instance.invAmountEnclosedPow;
					}
				}
				audioSource.gameObject.SetActive(audioSource.volume > 0f);
				if (audioSource.volume > 0f && !audioSource.isPlaying)
				{
					if (!audioSource.isActiveAndEnabled)
					{
						audioSource.gameObject.SetActive(true);
					}
					audioSource.Play();
				}
				num2++;
			}
		}
	}

	public void SetBiomeVolume(float _volume)
	{
		this.biomeVolume = _volume;
	}

	public void Pause()
	{
		if (this.currentAudioSrc != null)
		{
			this.currentAudioSrc.Pause();
		}
		if (this.playOrder == AudioObject.PlayOrder.ByValue)
		{
			foreach (AudioSource audioSource in this.runtimeAudioSrcs)
			{
				if (audioSource.isPlaying)
				{
					audioSource.Pause();
				}
			}
		}
	}

	public void UnPause()
	{
		if (this.currentAudioSrc != null)
		{
			this.currentAudioSrc.UnPause();
		}
		if (this.playOrder == AudioObject.PlayOrder.ByValue)
		{
			foreach (AudioSource audioSource in this.runtimeAudioSrcs)
			{
				if (audioSource.volume > 0f)
				{
					audioSource.UnPause();
				}
			}
		}
	}

	public void TurnOff(bool immediate = false)
	{
		this.fadingOut = true;
		if (immediate)
		{
			if (this.currentAudioSrc != null)
			{
				this.currentAudioSrc.Stop();
			}
			if (this.playOrder == AudioObject.PlayOrder.ByValue)
			{
				foreach (AudioSource audioSource in this.runtimeAudioSrcs)
				{
					audioSource.Stop();
				}
			}
			this.DestroySources();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool PlayConditionPasses()
	{
		World world = GameManager.Instance.World;
		EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
		if (primaryPlayer == null || primaryPlayer.Stats == null || primaryPlayer.IsDead())
		{
			return false;
		}
		if (WeatherManager.currentWeather == null)
		{
			return false;
		}
		if (WeatherManager.currentWeather.Wind() < this.minWind)
		{
			return false;
		}
		float num = world.GetWorldTime() / 24000f;
		float num2 = (num - (float)((int)num)) * 24f;
		float num3 = SkyManager.GetDawnTime() + this.dawnOffset;
		float num4 = SkyManager.GetDuskTime() + this.duskOffset;
		bool flag = SkyManager.IsBloodMoonVisible();
		bool flag2 = SkyManager.isAllTimeDay;
		bool flag3 = SkyManager.isAllTimeNight;
		if (flag2 || flag3)
		{
			num4 = 22f;
			num3 = 4f;
			if (SkyManager.WasBloodMoon)
			{
				flag2 = false;
				flag3 = false;
			}
		}
		if (this.outdoorOnly && primaryPlayer.Stats.AmountEnclosed >= 1f)
		{
			return false;
		}
		if (this.indoorOnly && primaryPlayer.Stats.AmountEnclosed <= 0f)
		{
			return false;
		}
		if (this.dayOnly && !flag2)
		{
			if (num2 < 12f && num2 < num3 - 0.02f)
			{
				return false;
			}
			if (num2 > 12f && num2 > num4 + 0.02f)
			{
				return false;
			}
		}
		if (this.nightOnly && !flag3)
		{
			if (num2 < 12f && num2 > num3 + 0.02f)
			{
				return false;
			}
			if (num2 > 12f && num2 < num4 - 0.02f)
			{
				return false;
			}
		}
		bool flag4 = false;
		ThreatLevelType category = primaryPlayer.ThreatLevel.Category;
		for (int i = 0; i < this.validThreatLevels.Length; i++)
		{
			if (this.validThreatLevels[i] == category)
			{
				flag4 = true;
				break;
			}
		}
		if (!flag4)
		{
			return false;
		}
		switch (this.trigger)
		{
		case AudioObject.Trigger.Dusk:
		{
			bool flag5 = EffectManager.GetValue(PassiveEffects.NoTimeDisplay, null, 0f, primaryPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) == 1f || !SandboxOptionManager.GetBool(global::SandboxOptions.SandboxOptions.ShowDayTime) || (flag2 || flag3);
			if (num4 > num2 + 0.01f || num4 < num2 - 0.01f || flag || flag5)
			{
				return false;
			}
			break;
		}
		case AudioObject.Trigger.Dawn:
		{
			bool flag6 = EffectManager.GetValue(PassiveEffects.NoTimeDisplay, null, 0f, primaryPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) == 1f || !SandboxOptionManager.GetBool(global::SandboxOptions.SandboxOptions.ShowDayTime) || (flag2 || flag3);
			if (num3 > num2 + 0.01f || num3 < num2 - 0.01f || flag6)
			{
				return false;
			}
			break;
		}
		case AudioObject.Trigger.Day7Dusk:
			if (!flag)
			{
				return false;
			}
			if (num4 > num2 + 0.01f || num4 < num2 - 0.01f)
			{
				return false;
			}
			break;
		case AudioObject.Trigger.Day8Dawn:
			if (SkyManager.dayCount - (float)(8 * ((int)SkyManager.dayCount / 8)) >= 1f)
			{
				return false;
			}
			if (num3 > num2 + 0.01f || num3 < num2 - 0.01f)
			{
				return false;
			}
			break;
		case AudioObject.Trigger.Random:
			if ((num2 > num4 - 0.25f && num2 < num4 + 0.25f) || (num2 > num3 - 0.25f && num2 < num3 + 0.25f))
			{
				return false;
			}
			if (world.dmsConductor != null && world.dmsConductor.IsMusicPlaying)
			{
				return false;
			}
			if (Time.time < this.repeatTime)
			{
				return false;
			}
			break;
		}
		return true;
	}

	public void DestroySources()
	{
		if (this.playOrder == AudioObject.PlayOrder.ByValue)
		{
			foreach (AudioSource audioSource in this.runtimeAudioSrcs)
			{
				if (audioSource != null)
				{
					Object.DestroyImmediate(audioSource.gameObject);
				}
			}
			this.runtimeAudioSrcs.Clear();
		}
		if (this.currentAudioSrc != null)
		{
			Object.DestroyImmediate(this.currentAudioSrc.gameObject);
			this.currentAudioSrc = null;
		}
	}

	public void SetVolume(float volume)
	{
		if (this.currentAudioSrc != null)
		{
			this.currentAudioSrc.volume = volume;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayAtPoint()
	{
		if (this.currentAudioSrc != null)
		{
			if (this.playAtPosition != Vector3.zero)
			{
				this.currentAudioSrc.gameObject.transform.position = this.playAtPosition - Origin.position;
			}
			this.currentAudioSrc.Play();
		}
	}

	public void Play()
	{
		int num = 0;
		AudioObject.PlayOrder playOrder = this.playOrder;
		if (playOrder != AudioObject.PlayOrder.Random)
		{
			if (playOrder == AudioObject.PlayOrder.FirstToLast)
			{
				this.currentPlayNum = 1;
			}
		}
		else
		{
			num = Manager.random.RandomRange(this.runtimeAudioSrcs.Count);
		}
		if (this.currentAudioSrc)
		{
			Object.DestroyImmediate(this.currentAudioSrc.gameObject);
		}
		AudioSource audioSource = this.runtimeAudioSrcs[num];
		this.currentAudioSrc = Object.Instantiate<AudioSource>(audioSource);
		GameObject gameObject = this.currentAudioSrc.gameObject;
		gameObject.name = this.name;
		gameObject.transform.SetParent(EnvironmentAudioManager.Instance.transform, false);
		gameObject.SetActive(true);
		this.currentAudioSrc.volume = (float)((this.trigger == AudioObject.Trigger.Thunder) ? 1 : 0);
		if (!this.currentAudioSrc.isPlaying)
		{
			this.PlayAtPoint();
		}
		if (this.trigger == AudioObject.Trigger.Continual)
		{
			this.currentAudioSrc.loop = true;
		}
	}

	public bool IsPlaying()
	{
		return !(this.currentAudioSrc == null) && this.currentAudioSrc.isPlaying;
	}

	public void SetPosition(Vector3 _position)
	{
		this.playAtPosition = _position;
	}

	public void Update(float deltaTime)
	{
		if (this.runtimeAudioSrcs.Count == 0)
		{
			return;
		}
		if (!(this.currentAudioSrc != null))
		{
			if (this.PlayConditionPasses())
			{
				this.DestroySources();
				this.value = 0f;
				this.fadingOut = false;
				this.playTime = 0f;
				this.loopTime = Time.time;
				this.Play();
			}
			return;
		}
		if (this.playOrder != AudioObject.PlayOrder.ByValue)
		{
			if (this.playOrder != AudioObject.PlayOrder.FirstToLast || this.currentPlayNum == 1 || this.fadingOut)
			{
				this.playTime += (this.fadingOut ? (-deltaTime * 0.02f) : deltaTime);
				float num = this.fadeInSec.Evaluate(this.playTime);
				float time = this.fadeInSec[this.fadeInSec.length - 1].time;
				if (this.playTime >= time)
				{
					this.playTime = time;
				}
				float num2 = num * this.biomeVolume * EnvironmentAudioManager.GlobalEnvironmentVolumeScale;
				if (!this.name.Contains("Stinger"))
				{
					num2 *= (this.music ? EnvironmentAudioManager.musicVolume : 1f);
				}
				if (!this.fadingOut)
				{
					if (num2 == 0f)
					{
						num2 = 0.001f;
					}
				}
				else if (num2 < 0.01f)
				{
					num2 = 0f;
				}
				else
				{
					EnvironmentAudioManager.Instance.fadingBiomes = true;
				}
				this.currentAudioSrc.volume = num2;
			}
			else
			{
				this.currentAudioSrc.volume = (this.music ? EnvironmentAudioManager.musicVolume : 1f) * this.biomeVolume * EnvironmentAudioManager.GlobalEnvironmentVolumeScale;
			}
			this.currentAudioSrc.gameObject.SetActive(this.currentAudioSrc.volume > 0f && this.currentAudioSrc.isPlaying);
			if (this.playOrder == AudioObject.PlayOrder.FirstToLast && this.currentPlayNum < this.runtimeAudioSrcs.Count && (this.currentAudioSrc.volume == 0f || !this.currentAudioSrc.isPlaying))
			{
				Object.DestroyImmediate(this.currentAudioSrc.gameObject);
				this.currentAudioSrc = null;
				List<AudioSource> list = this.runtimeAudioSrcs;
				int num3 = this.currentPlayNum;
				this.currentPlayNum = num3 + 1;
				this.currentAudioSrc = Object.Instantiate<AudioSource>(list[num3]);
				this.currentAudioSrc.transform.parent = EnvironmentAudioManager.Instance.transform;
				this.currentAudioSrc.gameObject.name = this.name;
				this.currentAudioSrc.name = this.name;
				this.currentAudioSrc.gameObject.SetActive(true);
				this.currentAudioSrc.volume = EnvironmentAudioManager.GlobalEnvironmentVolumeScale * (this.music ? EnvironmentAudioManager.musicVolume : 1f) * this.biomeVolume;
				if (!this.currentAudioSrc.isPlaying)
				{
					this.PlayAtPoint();
				}
			}
		}
		if (this.outdoorOnly)
		{
			this.currentAudioSrc.volume = this.currentAudioSrc.volume * EnvironmentAudioManager.Instance.invAmountEnclosedPow;
		}
		else if (this.indoorOnly)
		{
			this.currentAudioSrc.volume = this.currentAudioSrc.volume * (1f - EnvironmentAudioManager.Instance.invAmountEnclosedPow);
		}
		if (this.loopDuration > 0f && Time.time > this.loopTime + this.loopDuration)
		{
			this.TurnOff(false);
		}
		float num4 = GameManager.Instance.World.GetWorldTime() / 24000f;
		float num5 = (num4 - (float)((int)num4)) * 24f;
		float num6 = SkyManager.GetDawnTime() + this.dawnOffset;
		float num7 = SkyManager.GetDuskTime() + this.duskOffset;
		if (SkyManager.isAllTimeDay)
		{
			if (this.nightOnly)
			{
				this.TurnOff(false);
			}
		}
		else if (SkyManager.isAllTimeNight)
		{
			if (this.dayOnly)
			{
				this.TurnOff(false);
			}
		}
		else
		{
			if (this.dayOnly && num5 < 12f && num5 < num6 - 0.02f)
			{
				this.TurnOff(false);
			}
			if (this.dayOnly && num5 > 12f && num5 > num7 + 0.02f)
			{
				this.TurnOff(false);
			}
			if (this.nightOnly && num5 < 12f && num5 > num6 + 0.02f)
			{
				this.TurnOff(false);
			}
			if (this.nightOnly && num5 > 12f && num5 < num7 - 0.02f)
			{
				this.TurnOff(false);
			}
		}
		if (this.playOrder == AudioObject.PlayOrder.ByValue)
		{
			return;
		}
		if (this.trigger == AudioObject.Trigger.Continual && this.currentAudioSrc != null && !this.currentAudioSrc.isPlaying && this.currentAudioSrc.volume > 0f)
		{
			if (!this.currentAudioSrc.isActiveAndEnabled)
			{
				this.currentAudioSrc.gameObject.SetActive(true);
			}
			if (this.currentAudioSrc.isActiveAndEnabled)
			{
				this.PlayAtPoint();
			}
		}
		if (!(this.currentAudioSrc != null) || !this.currentAudioSrc.isPlaying || (this.fadingOut && this.currentAudioSrc.volume <= 0f))
		{
			Object.DestroyImmediate(this.currentAudioSrc.gameObject);
			this.currentAudioSrc = null;
			if (this.trigger == AudioObject.Trigger.Random)
			{
				this.repeatTime = Time.time + Manager.random.RandomRange(this.repeatFreqRange.x, this.repeatFreqRange.y);
			}
		}
	}

	public static Dictionary<byte, BiomeDefinition.BiomeType> biomeIdMap;

	public string name;

	public AudioMixerGroup audioMixerGroup;

	public AudioSource masterAudioSource;

	public AudioClip[] audioClips;

	public List<AudioSource> runtimeAudioSrcs = new List<AudioSource>();

	public AudioObject.Trigger trigger;

	public bool indoorOnly;

	public bool outdoorOnly;

	public bool dayOnly;

	public bool nightOnly;

	public float duskOffset;

	public float dawnOffset;

	public float minWind;

	public bool affectedByEnv;

	public AudioObject.PlayOrder playOrder;

	public BiomeDefinition.BiomeType[] validBiomes;

	public AnimationCurve fadeInSec = new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0f, 0f),
		new Keyframe(1f, 1f)
	});

	public AnimationCurve transitionCurve;

	public Vector2 repeatFreqRange;

	public float loopDuration;

	public bool music;

	public ThreatLevelType[] validThreatLevels;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AudioSource currentAudioSrc;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float loopTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float repeatTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool fadingOut;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float value;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float playTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float biomeVolume = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int currentPlayNum;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 playAtPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float timeEpsilon = 0.01f;

	public enum Trigger
	{
		Rain,
		Snow,
		Thunder,
		TimeOfDay,
		Dusk,
		Dawn,
		Day7Times,
		Day7Dusk,
		Day8Dawn,
		Random,
		Continual,
		Wind
	}

	public enum PlayOrder
	{
		Random,
		FirstToLast,
		ByValue
	}
}
