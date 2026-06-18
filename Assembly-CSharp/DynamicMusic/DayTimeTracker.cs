using System;
using System.Collections.Generic;
using DynamicMusic.Factories;
using MusicUtils.Enums;

namespace DynamicMusic
{
	public class DayTimeTracker : AbstractDayTimeTracker, INotifiableFilter<MusicActionType, SectionType>, INotifiable<MusicActionType>, IFilter<SectionType>
	{
		public DayTimeTracker()
		{
			this.world = GameManager.Instance.World;
			this.conductor = this.world.dmsConductor;
			ValueTuple<int, int> valueTuple = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
			int item = valueTuple.Item1;
			int item2 = valueTuple.Item2;
			int @int = GamePrefs.GetInt(EnumGamePrefs.DayNightLength);
			this.duskTime = (float)item / 24f * (float)@int;
			this.dawnTime = (float)item2 / 24f * (float)@int;
			this.currentDay = this.GetCurrentDay();
			this.MusicTimeTracker = Factory.CreateMusicTimeTracker();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Update()
		{
			if (this.currentDay != this.GetCurrentDay())
			{
				this.UpdateDay();
			}
			this.currentTime = this.GetCurrentTime();
			this.UpdateDayPeriod();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void UpdateDay()
		{
			this.currentDay = this.GetCurrentDay();
			this.MusicTimeTracker.Notify();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override int GetCurrentDay()
		{
			return GameUtils.WorldTimeToDays(this.world.worldTime);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override float GetCurrentTime()
		{
			return SkyManager.GetTimeOfDayAsMinutes();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void UpdateDayPeriod()
		{
			if (this.currentTime < this.dawnTime - 0.33333334f)
			{
				this.dayPeriod = AbstractDayTimeTracker.DayPeriodType.Morning;
				return;
			}
			if (this.currentTime <= this.dawnTime + 0.33333334f)
			{
				this.dayPeriod = AbstractDayTimeTracker.DayPeriodType.Dusk;
				return;
			}
			if (this.currentTime < this.duskTime - 0.33333334f)
			{
				this.dayPeriod = AbstractDayTimeTracker.DayPeriodType.Day;
				return;
			}
			if (this.currentTime <= this.duskTime + 0.33333334f)
			{
				this.dayPeriod = AbstractDayTimeTracker.DayPeriodType.Dusk;
				return;
			}
			this.dayPeriod = AbstractDayTimeTracker.DayPeriodType.Night;
		}

		public override string ToString()
		{
			return string.Format("Current Day: {0}\nCurrent part of the day: {1}\nCurrent Time: {2}\nDawn time: {3}\nDusk Time: {4}\n", new object[]
			{
				this.currentDay,
				this.dayPeriod.ToStringCached<AbstractDayTimeTracker.DayPeriodType>(),
				this.currentTime,
				this.dawnTime,
				this.duskTime
			});
		}

		public override List<SectionType> Filter(List<SectionType> _sectionTypes)
		{
			this.Update();
			GameStats.GetInt(EnumGameStats.BloodMoonDay);
			GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
			if (GameUtils.IsBloodMoonTime(this.world.worldTime, GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength)), GameStats.GetInt(EnumGameStats.BloodMoonDay)))
			{
				_sectionTypes.Clear();
				_sectionTypes.Add(this.conductor.IsBloodmoonMusicEligible ? SectionType.Bloodmoon : SectionType.None);
				return _sectionTypes;
			}
			if (this.dayPeriod.Equals(AbstractDayTimeTracker.DayPeriodType.Dawn) || this.dayPeriod.Equals(AbstractDayTimeTracker.DayPeriodType.Dusk))
			{
				_sectionTypes.Remove(SectionType.Exploration);
				_sectionTypes.Remove(SectionType.HomeDay);
				_sectionTypes.Remove(SectionType.HomeNight);
				_sectionTypes.Remove(SectionType.Suspense);
			}
			else if (!this.dayPeriod.Equals(AbstractDayTimeTracker.DayPeriodType.Day))
			{
				_sectionTypes.Remove(SectionType.Exploration);
				_sectionTypes.Remove(SectionType.HomeDay);
			}
			else
			{
				_sectionTypes.Remove(SectionType.HomeNight);
			}
			return this.MusicTimeTracker.Filter(_sectionTypes);
		}

		public void Notify(MusicActionType _state)
		{
			this.MusicTimeTracker.Notify(_state);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const float duskDawnWindowRadius = 0.33333334f;

		[PublicizedFrom(EAccessModifier.Private)]
		public World world;

		[PublicizedFrom(EAccessModifier.Private)]
		public Conductor conductor;

		[PublicizedFrom(EAccessModifier.Private)]
		public IMultiNotifiableFilter MusicTimeTracker;
	}
}
