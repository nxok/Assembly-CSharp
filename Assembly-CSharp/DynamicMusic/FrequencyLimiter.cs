using System;
using System.Collections.Generic;
using MusicUtils.Enums;
using UnityEngine;

namespace DynamicMusic
{
	public class FrequencyLimiter : AbstractFilter, IMultiNotifiableFilter, INotifiable, INotifiableFilter<MusicActionType, SectionType>, INotifiable<MusicActionType>, IFilter<SectionType>, IGamePrefsChangedListener
	{
		public FrequencyLimiter()
		{
			this.rng = GameRandomManager.Instance.CreateGameRandom();
			GamePrefs.AddChangeListener(this);
			this.UpdateParameters();
			this.UpdateRollTime();
		}

		public override List<SectionType> Filter(List<SectionType> _sectionTypes)
		{
			if (AudioSettings.dspTime <= this.rollTime || this.rng.RandomRange(1f) > this.chanceOfPositiveRoll)
			{
				for (int i = _sectionTypes.Count - 1; i >= 0; i--)
				{
					if (_sectionTypes[i] != SectionType.None && _sectionTypes[i] != SectionType.Combat)
					{
						_sectionTypes.RemoveAt(i);
					}
				}
			}
			return _sectionTypes;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void UpdateRollTime()
		{
			this.rollTime = AudioSettings.dspTime + (double)this.cooldown;
		}

		public void Notify(MusicActionType _state)
		{
			if (_state.Equals(MusicActionType.Stop))
			{
				this.UpdateRollTime();
			}
		}

		public void Notify()
		{
			this.UpdateRollTime();
		}

		public void OnGamePrefChanged(EnumGamePrefs _enum)
		{
			if (_enum.Equals(EnumGamePrefs.OptionsDynamicMusicDailyTime) || _enum.Equals(EnumGamePrefs.DayNightLength))
			{
				this.UpdateParameters();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void UpdateParameters()
		{
			this.dayLengthInSeconds = (float)GamePrefs.GetInt(EnumGamePrefs.DayNightLength) * 60f;
			this.dailyAllottedPlaySeconds = GamePrefs.GetFloat(EnumGamePrefs.OptionsDynamicMusicDailyTime) * this.dayLengthInSeconds;
			this.rollsPerDay = Mathf.Ceil(this.dailyAllottedPlaySeconds / 168f);
			float num = this.dayLengthInSeconds / this.rollsPerDay;
			this.cooldown = Mathf.Max(num - 168f, 30f);
			this.chanceOfPositiveRoll = (float)Math.Pow(0.8999999761581421, 1.0 / (double)this.rollsPerDay);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cMinCooldown = 30f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cProbOfFailingToReachDailyAllotted = 0.1f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cPlayTime = 168f;

		[PublicizedFrom(EAccessModifier.Private)]
		public float dayLengthInSeconds;

		[PublicizedFrom(EAccessModifier.Private)]
		public float dailyAllottedPlaySeconds;

		[PublicizedFrom(EAccessModifier.Private)]
		public float rollsPerDay;

		[PublicizedFrom(EAccessModifier.Private)]
		public float cooldown;

		[PublicizedFrom(EAccessModifier.Private)]
		public float chanceOfPositiveRoll;

		[PublicizedFrom(EAccessModifier.Private)]
		public double rollTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public GameRandom rng;
	}
}
