using System;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicMono : SingletonMonoBehaviour<BackgroundMusicMono>
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		AudioListener[] array = Object.FindObjectsByType<AudioListener>(0);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].enabled)
			{
				base.transform.position = array[i].transform.position;
				break;
			}
		}
		AudioListener.volume = Mathf.Min(GamePrefs.GetFloat(EnumGamePrefs.OptionsOverallAudioVolumeLevel), 1f);
		this.AddMusicTrack(BackgroundMusicMono.MusicTrack.None, null);
		this.AddMusicTrack(BackgroundMusicMono.MusicTrack.BackgroundMusic, GameManager.Instance.BackgroundMusicClip);
		this.AddMusicTrack(BackgroundMusicMono.MusicTrack.CreditsSong, GameManager.Instance.CreditsSongClip);
		this.Play(BackgroundMusicMono.MusicTrack.BackgroundMusic);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (GameStats.GetInt(EnumGameStats.GameState) == 1 && !GameManager.Instance.IsPaused())
		{
			this.Play(BackgroundMusicMono.MusicTrack.None);
		}
		else if (LocalPlayerUI.primaryUI.windowManager.IsWindowOpen(XUiC_Credits.ID))
		{
			this.Play(BackgroundMusicMono.MusicTrack.CreditsSong);
		}
		else
		{
			this.Play(BackgroundMusicMono.MusicTrack.BackgroundMusic);
		}
		this.activeTracks.RemoveWhere((BackgroundMusicMono.MusicTrack activeTrack) => !this.UpdateTrack(activeTrack));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddMusicTrack(BackgroundMusicMono.MusicTrack musicTrack, AudioClip audioClip)
	{
		if (!audioClip)
		{
			this.musicTrackStates.Add(musicTrack, new BackgroundMusicMono.MusicTrackState(null));
			return;
		}
		AudioSource audioSource = base.gameObject.AddComponent<AudioSource>();
		audioSource.volume = 0f;
		audioSource.clip = audioClip;
		audioSource.loop = true;
		this.musicTrackStates.Add(musicTrack, new BackgroundMusicMono.MusicTrackState(audioSource));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Play(BackgroundMusicMono.MusicTrack musicTrack)
	{
		if (this.currentlyPlaying == musicTrack)
		{
			return;
		}
		this.currentlyPlaying = musicTrack;
		this.activeTracks.Add(musicTrack);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool UpdateTrack(BackgroundMusicMono.MusicTrack activeTrack)
	{
		BackgroundMusicMono.MusicTrackState musicTrackState = this.musicTrackStates[activeTrack];
		AudioSource audioSource = musicTrackState.AudioSource;
		if (!audioSource)
		{
			return false;
		}
		float num = musicTrackState.CurrentVolume;
		if (activeTrack == this.currentlyPlaying)
		{
			num += Time.deltaTime / 3f;
		}
		else
		{
			num -= Time.deltaTime / 3f;
		}
		num = Mathf.Clamp01(num);
		musicTrackState.CurrentVolume = num;
		bool flag = activeTrack == this.currentlyPlaying || num > 0f;
		audioSource.volume = Mathf.Clamp01(GamePrefs.GetFloat(EnumGamePrefs.OptionsMenuMusicVolumeLevel) * num);
		if (audioSource.isPlaying == flag)
		{
			return flag;
		}
		if (flag)
		{
			audioSource.Play();
		}
		else
		{
			audioSource.Stop();
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float secondsToFadeOut = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float secondsToFadeIn = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly EnumDictionary<BackgroundMusicMono.MusicTrack, BackgroundMusicMono.MusicTrackState> musicTrackStates = new EnumDictionary<BackgroundMusicMono.MusicTrack, BackgroundMusicMono.MusicTrackState>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly HashSet<BackgroundMusicMono.MusicTrack> activeTracks = new HashSet<BackgroundMusicMono.MusicTrack>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BackgroundMusicMono.MusicTrack currentlyPlaying;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum MusicTrack
	{
		None,
		BackgroundMusic,
		CreditsSong
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class MusicTrackState
	{
		public MusicTrackState(AudioSource audioSource)
		{
			this.AudioSource = audioSource;
		}

		public readonly AudioSource AudioSource;

		public float CurrentVolume;
	}
}
