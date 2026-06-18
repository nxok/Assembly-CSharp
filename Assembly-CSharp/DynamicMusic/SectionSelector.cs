using System;
using System.Collections.Generic;
using DynamicMusic.Factories;
using MusicUtils.Enums;
using UniLinq;

namespace DynamicMusic
{
	public class SectionSelector : ISectionSelector, INotifiable<MusicActionType>, ISelector<SectionType>
	{
		public SectionSelector()
		{
			this.PlayerTracker = Factory.CreatePlayerTracker();
			this.DayTimeTracker = Factory.CreateDayTimeTracker();
		}

		public SectionType Select()
		{
			this.sectionTypesBuffer.Clear();
			this.sectionTypesBuffer.AddRange(SectionSelector.sectionTypeEnumValues);
			if (SectionSelector.IsDMSTempDisabled || GamePrefs.GetFloat(EnumGamePrefs.OptionsMusicVolumeLevel) == 0f || !GamePrefs.GetBool(EnumGamePrefs.OptionsDynamicMusicEnabled))
			{
				this.sectionTypesBuffer.Clear();
				this.sectionTypesBuffer.Add(SectionType.None);
			}
			else
			{
				this.sectionTypesBuffer = this.PlayerTracker.Filter(this.sectionTypesBuffer);
			}
			if (this.sectionTypesBuffer.Count > 1)
			{
				this.sectionTypesBuffer = this.DayTimeTracker.Filter(this.sectionTypesBuffer);
			}
			if (this.sectionTypesBuffer.Count == 2)
			{
				this.sectionTypesBuffer.Remove(SectionType.None);
			}
			return this.sectionTypesBuffer[0];
		}

		public void Notify(MusicActionType _state)
		{
			this.DayTimeTracker.Notify(_state);
		}

		public static bool IsDMSTempDisabled;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly List<SectionType> sectionTypeEnumValues = Enum.GetValues(typeof(SectionType)).Cast<SectionType>().ToList<SectionType>();

		[PublicizedFrom(EAccessModifier.Private)]
		public IFilter<SectionType> PlayerTracker;

		[PublicizedFrom(EAccessModifier.Private)]
		public INotifiableFilter<MusicActionType, SectionType> DayTimeTracker;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<SectionType> sectionTypesBuffer = new List<SectionType>(SectionSelector.sectionTypeEnumValues.Count);
	}
}
