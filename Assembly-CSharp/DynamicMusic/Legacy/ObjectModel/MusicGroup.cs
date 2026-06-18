using System;
using System.Collections.Generic;
using MusicUtils.Enums;

namespace DynamicMusic.Legacy.ObjectModel
{
	public class MusicGroup : EnumDictionary<ThreatLevelLegacyType, ThreatLevel>
	{
		public MusicGroup(int _sampleRate, byte _hbLength)
		{
			this.SampleRate = _sampleRate;
			this.HBLength = _hbLength;
			this.ConfigIDs = new List<int>();
		}

		public static void InitStatic()
		{
			MusicGroup.AllGroups = new List<MusicGroup>();
		}

		public static void Cleanup()
		{
			if (MusicGroup.AllGroups != null)
			{
				MusicGroup.AllGroups.Clear();
			}
		}

		public static List<MusicGroup> AllGroups;

		public List<int> ConfigIDs;

		public readonly int SampleRate;

		public readonly byte HBLength;
	}
}
