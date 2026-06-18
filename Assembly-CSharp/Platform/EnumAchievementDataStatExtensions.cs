using System;

namespace Platform
{
	public static class EnumAchievementDataStatExtensions
	{
		public static bool IsSupported(this EnumAchievementDataStat _stat)
		{
			IAchievementManager achievementManager = PlatformManager.NativePlatform.AchievementManager;
			return achievementManager == null || achievementManager.IsAchievementStatSupported(_stat);
		}
	}
}
