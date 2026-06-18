using System;

namespace Platform
{
	public static class AchievementUtils
	{
		public static bool IsCreativeModeActive()
		{
			return GamePrefs.GetString(EnumGamePrefs.GameMode).Equals(GameModeCreative.TypeName) || GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled);
		}
	}
}
