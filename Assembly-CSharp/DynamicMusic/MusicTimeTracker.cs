using System;
using System.Collections.Generic;
using MusicUtils.Enums;
using UnityEngine;

namespace DynamicMusic
{
	public class MusicTimeTracker : AbstractMusicTimeTracker, IMultiNotifiableFilter, INotifiable, INotifiableFilter<MusicActionType, SectionType>, INotifiable<MusicActionType>, IFilter<SectionType>, IGamePrefsChangedListener
	{
		public MusicTimeTracker()
		{
			this.dailyAllottedPlayTime = GamePrefs.GetFloat(EnumGamePrefs.OptionsDynamicMusicDailyTime) * (float)GamePrefs.GetInt(EnumGamePrefs.DayNightLength) * 60f;
			this.MusicActions = new EnumDictionary<MusicActionType, Action>(4);
			this.MusicActions.Add(MusicActionType.Play, new Action(this.OnPlay));
			this.MusicActions.Add(MusicActionType.Pause, new Action(this.OnPause));
			this.MusicActions.Add(MusicActionType.UnPause, new Action(this.OnUnPause));
			this.MusicActions.Add(MusicActionType.Stop, new Action(this.OnStop));
			this.MusicActions.Add(MusicActionType.FadeIn, new Action(this.OnFadeIn));
			this.FrequencyLimiter = new FrequencyLimiter();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnPlay()
		{
			this.musicStartTime = (float)AudioSettings.dspTime;
			this.IsMusicPlaying = true;
			this.pauseStartTime = (this.pauseDuration = 0f);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnPause()
		{
			this.pauseStartTime = (float)AudioSettings.dspTime;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnUnPause()
		{
			this.pauseDuration += (float)AudioSettings.dspTime - this.pauseStartTime;
			this.pauseStartTime = 0f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnStop()
		{
			this.dailyPlayTimeUsed += (float)AudioSettings.dspTime - this.musicStartTime - this.pauseDuration;
			this.musicStartTime = (this.pauseDuration = 0f);
			this.IsMusicPlaying = false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnFadeIn()
		{
			if (!this.IsMusicPlaying)
			{
				this.musicStartTime = (float)AudioSettings.dspTime;
			}
		}

		public void OnGamePrefChanged(EnumGamePrefs _enum)
		{
			if (_enum == EnumGamePrefs.OptionsDynamicMusicDailyTime || _enum == EnumGamePrefs.DayNightLength)
			{
				this.dailyAllottedPlayTime = GamePrefs.GetFloat(EnumGamePrefs.OptionsDynamicMusicDailyTime) * (float)GamePrefs.GetInt(EnumGamePrefs.DayNightLength);
			}
		}

		public void Cleanup()
		{
		}

		public override List<SectionType> Filter(List<SectionType> _sectionTypes)
		{
			if (!this.IsMusicPlaying)
			{
				if (this.dailyPlayTimeUsed < this.dailyAllottedPlayTime)
				{
					this.FrequencyLimiter.Filter(_sectionTypes);
				}
				else
				{
					_sectionTypes.Remove(SectionType.HomeDay);
					_sectionTypes.Remove(SectionType.HomeNight);
					_sectionTypes.Remove(SectionType.Exploration);
					_sectionTypes.Remove(SectionType.Suspense);
				}
			}
			return _sectionTypes;
		}

		public void Notify()
		{
			this.dailyPlayTimeUsed = 0f;
			this.FrequencyLimiter.Notify();
		}

		public void Notify(MusicActionType _state)
		{
			Action action;
			if (this.MusicActions.TryGetValue(_state, out action))
			{
				action();
			}
			this.FrequencyLimiter.Notify(_state);
		}

		public override string ToString()
		{
			return string.Format("Daily Play Time Allotted: {0}\nPlay Time Used: {1}\nIs Music Playing: {2}\nMusic Start Time: {3}\nPause Start Time: {4}\nPause Duration: {5}\n", new object[] { this.dailyAllottedPlayTime, this.dailyPlayTimeUsed, this.IsMusicPlaying, this.musicStartTime, this.pauseStartTime, this.pauseDuration });
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IMultiNotifiableFilter FrequencyLimiter;

		[PublicizedFrom(EAccessModifier.Private)]
		public EnumDictionary<MusicActionType, Action> MusicActions;
	}
}
