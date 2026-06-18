using System;
using MusicUtils;
using UnityEngine.Audio;

public class AmbientAudioController : IGamePrefsChangedListener
{
	public static AmbientAudioController Instance
	{
		get
		{
			if (AmbientAudioController.instance == null)
			{
				AmbientAudioController.instance = new AmbientAudioController();
			}
			return AmbientAudioController.instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public AmbientAudioController()
	{
		GamePrefs.AddChangeListener(this);
	}

	public void SetAmbientVolume(float _val)
	{
		if (AmbientAudioController.master)
		{
			AmbientAudioController.master.SetFloat("ambVol", AmbientAudioController.volumeCurve.GetMixerValue(_val));
		}
	}

	public void OnGamePrefChanged(EnumGamePrefs _enum)
	{
		if (_enum == EnumGamePrefs.OptionsAmbientVolumeLevel)
		{
			this.SetAmbientVolume(GamePrefs.GetFloat(EnumGamePrefs.OptionsAmbientVolumeLevel));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static AmbientAudioController instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public static AudioMixer master = DataLoader.LoadAsset<AudioMixer>("@:Sound_Mixers/MasterAudioMixer.mixer", false);

	[PublicizedFrom(EAccessModifier.Private)]
	public static LogarithmicCurve volumeCurve = new LogarithmicCurve(2.0, 6.0, -80f, 0f, 0f, 1f);
}
