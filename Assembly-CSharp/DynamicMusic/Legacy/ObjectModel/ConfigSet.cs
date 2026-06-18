using System;
using System.Collections.Generic;
using MusicUtils.Enums;

namespace DynamicMusic.Legacy.ObjectModel
{
	public class ConfigSet : EnumDictionary<ThreatLevelLegacyType, ThreatLevelConfig>
	{
		public static void Cleanup()
		{
			if (ConfigSet.AllConfigSets != null)
			{
				ConfigSet.AllConfigSets.Clear();
			}
		}

		public static Dictionary<int, ConfigSet> AllConfigSets = new Dictionary<int, ConfigSet>();
	}
}
