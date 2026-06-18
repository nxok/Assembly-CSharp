using System;
using System.Collections.Generic;
using MusicUtils;
using UnityEngine.Audio;

namespace DynamicMusic.Legacy
{
	public class TransitionManager : IGamePrefsChangedListener
	{
		public float MasterParam
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				if (!this.dynamicMusicManager.IsMusicPlayingThisTick)
				{
					return 0f;
				}
				return this.dynamicMusicManager.ThreatLevelTracker.NumericalThreatLevel;
			}
		}

		public static void Init(DynamicMusicManager _dynamicMusicManager)
		{
			_dynamicMusicManager.TransitionManager = new TransitionManager();
			_dynamicMusicManager.TransitionManager.dynamicMusicManager = _dynamicMusicManager;
			TransitionManager.Master = DataLoader.LoadAsset<AudioMixer>("@:Sound_Mixers/MasterAudioMixer.mixer", false);
			TransitionManager.DmsAbsoluteVolumeCurve = new LogarithmicCurve(2.0, 6.0, -80f, 0f, 0f, 1f);
			TransitionManager.SetDynamicMusicVolume(GamePrefs.GetFloat(EnumGamePrefs.OptionsMusicVolumeLevel));
			GamePrefs.AddChangeListener(_dynamicMusicManager.TransitionManager);
		}

		public void Tick()
		{
			if (TransitionManager.Master == null)
			{
				return;
			}
			foreach (KeyValuePair<string, Curve> keyValuePair in SignalProcessing.DspCurves)
			{
				TransitionManager.Master.SetFloat(keyValuePair.Key, keyValuePair.Value.GetMixerValue(this.MasterParam));
			}
			if (this.dynamicMusicManager.IsInDeadWindow)
			{
				TransitionManager.Master.SetFloat("dmsVol", TransitionManager.currentEventDMSLogVolume = -80f);
				return;
			}
			if (this.dynamicMusicManager.DistanceFromDeadWindow > TransitionManager.dawnDuskFadeTime)
			{
				TransitionManager.Master.SetFloat("dmsVol", TransitionManager.currentEventDMSLogVolume = TransitionManager.currentAbsoluteDMSLogVolume);
				return;
			}
			TransitionManager.Master.SetFloat("dmsVol", TransitionManager.currentEventDMSLogVolume = TransitionManager.DmsEventRangeVolumeCurve.GetMixerValue(this.dynamicMusicManager.DistanceFromDeadWindow));
		}

		public static void SetDynamicMusicVolume(float _value)
		{
			if (TransitionManager.Master != null)
			{
				TransitionManager.currentAbsoluteDMSLogVolume = TransitionManager.DmsAbsoluteVolumeCurve.GetMixerValue(_value);
				TransitionManager.Master.SetFloat("dmsVol", TransitionManager.currentAbsoluteDMSLogVolume);
				TransitionManager.DmsEventRangeVolumeCurve = new LogarithmicCurve(2.0, 6.0, TransitionManager.currentAbsoluteDMSLogVolume, -80f, TransitionManager.dawnDuskFadeTime, 0f);
			}
		}

		public static void ApplyPauseFilter()
		{
			if (TransitionManager.Master != null)
			{
				TransitionManager.Master.SetFloat("dmsCutOff", 500f);
			}
		}

		public static void RemovePauseFilter()
		{
			if (TransitionManager.Master != null)
			{
				TransitionManager.Master.SetFloat("dmsCutOff", 22000f);
			}
		}

		public void OnGamePrefChanged(EnumGamePrefs _enum)
		{
			if (_enum == EnumGamePrefs.OptionsMusicVolumeLevel)
			{
				TransitionManager.SetDynamicMusicVolume(GamePrefs.GetFloat(EnumGamePrefs.OptionsMusicVolumeLevel));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public DynamicMusicManager dynamicMusicManager;

		[PublicizedFrom(EAccessModifier.Private)]
		public static AudioMixer Master;

		[PublicizedFrom(EAccessModifier.Private)]
		public static LogarithmicCurve DmsAbsoluteVolumeCurve;

		[PublicizedFrom(EAccessModifier.Private)]
		public static LogarithmicCurve DmsEventRangeVolumeCurve;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly float dawnDuskFadeTime = 0.16666667f;

		[PublicizedFrom(EAccessModifier.Private)]
		public static float currentAbsoluteDMSLogVolume;

		[PublicizedFrom(EAccessModifier.Private)]
		public static float currentEventDMSLogVolume;
	}
}
