using System;

namespace Platform
{
	public static class DeviceName
	{
		public static string GetDeviceName(this DeviceFlag _deviceId)
		{
			if (_deviceId <= DeviceFlag.XBoxSeriesS)
			{
				switch (_deviceId)
				{
				case DeviceFlag.StandaloneWindows:
					return "Windows";
				case DeviceFlag.StandaloneLinux:
					return "Linux";
				case DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux:
					break;
				case DeviceFlag.StandaloneOSX:
					return "OSX";
				default:
					if (_deviceId == DeviceFlag.XBoxSeriesS)
					{
						return "XBoxSeriesS";
					}
					break;
				}
			}
			else
			{
				if (_deviceId == DeviceFlag.XBoxSeriesX)
				{
					return "XBoxSeriesX";
				}
				if (_deviceId == DeviceFlag.PS5)
				{
					return "PS5";
				}
			}
			Log.Warning(string.Format("Device name for flag '{0}' is unknown", _deviceId));
			return string.Empty;
		}

		public const string StandaloneWindows = "Windows";

		public const string StandaloneLinux = "Linux";

		public const string StandaloneOSX = "OSX";

		public const string PS5 = "PS5";

		public const string XBoxSeriesS = "XBoxSeriesS";

		public const string XBoxSeriesX = "XBoxSeriesX";
	}
}
