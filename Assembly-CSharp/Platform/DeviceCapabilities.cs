using System;

namespace Platform
{
	public static class DeviceCapabilities
	{
		public static bool CanUserAccessFilesystem()
		{
			return !Submission.Enabled || (DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent();
		}
	}
}
