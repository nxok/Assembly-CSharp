using System;
using System.Collections.Generic;
using UnityEngine;

[PublicizedFrom(EAccessModifier.Internal)]
public class AudioTrigger
{
	public AudioTrigger(AudioObject.Trigger _trigger)
	{
		this.trigger = _trigger;
	}

	public void Add(AudioObject _audioObject)
	{
		this.sound.Add(_audioObject);
	}

	public void Update()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		for (int i = this.sound.Count - 1; i >= 0; i--)
		{
			this.sound[i].Update(fixedDeltaTime);
		}
	}

	public void SetVolume(float _vol)
	{
		for (int i = this.sound.Count - 1; i >= 0; i--)
		{
			this.sound[i].SetBiomeVolume(_vol);
		}
	}

	public void Pause()
	{
		for (int i = this.sound.Count - 1; i >= 0; i--)
		{
			this.sound[i].Pause();
		}
	}

	public void UnPause()
	{
		for (int i = this.sound.Count - 1; i >= 0; i--)
		{
			this.sound[i].UnPause();
		}
	}

	public void TurnOff()
	{
		for (int i = this.sound.Count - 1; i >= 0; i--)
		{
			this.sound[i].TurnOff(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly AudioObject.Trigger trigger;

	public List<AudioObject> sound = new List<AudioObject>();
}
