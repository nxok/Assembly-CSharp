using System;
using UnityEngine;

public class AudioGamepadRumbleSource
{
	public AudioGamepadRumbleSource()
	{
		this.samples = new float[64];
	}

	public void SetAudioSource(AudioSource _audioSource, float _strengthMultiplier, bool _locationBased)
	{
		this.audioSrc = _audioSource;
		this.strengthMultiplier = _strengthMultiplier;
		this.locationBased = _locationBased;
		this.timeAdded = Time.time;
	}

	public float GetSample(int channel)
	{
		this.audioSrc.GetOutputData(this.samples, channel);
		float num = 0f;
		for (int i = 0; i < 64; i++)
		{
			num += this.samples[i];
		}
		return num / 64f;
	}

	public void Clear()
	{
		this.audioSrc = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSampleCount = 64;

	public AudioSource audioSrc;

	public float[] samples;

	public float strengthMultiplier;

	public bool locationBased;

	public float timeAdded;
}
