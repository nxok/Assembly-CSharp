using System;
using System.Collections.Generic;

namespace Platform
{
	public static class EPlayGroupExtensions
	{
		public static bool IsCurrent(this EPlayGroup group)
		{
			return group == EPlayGroupExtensions.Current;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static EPlayGroup GetCurrentPlayGroup()
		{
			if (DeviceFlag.PS5.IsCurrent())
			{
				return EPlayGroup.PS5;
			}
			if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent())
			{
				return EPlayGroup.XBS;
			}
			return EPlayGroup.Standalone;
		}

		public static EPlayGroup ToPlayGroup(this DeviceFlag device)
		{
			if (device <= DeviceFlag.XBoxSeriesS)
			{
				switch (device)
				{
				case DeviceFlag.StandaloneWindows:
					return EPlayGroup.Standalone;
				case DeviceFlag.StandaloneLinux:
					return EPlayGroup.Standalone;
				case DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux:
					break;
				case DeviceFlag.StandaloneOSX:
					return EPlayGroup.Standalone;
				default:
					if (device == DeviceFlag.XBoxSeriesS)
					{
						return EPlayGroup.XBS;
					}
					break;
				}
			}
			else
			{
				if (device == DeviceFlag.XBoxSeriesX)
				{
					return EPlayGroup.XBS;
				}
				if (device == DeviceFlag.PS5)
				{
					return EPlayGroup.PS5;
				}
			}
			throw new ArgumentOutOfRangeException("device", device, string.Format("Missing play group mapping for {0}.", device));
		}

		public static EPlayGroup ToPlayGroup(this ClientInfo.EDeviceType deviceType)
		{
			EPlayGroup eplayGroup;
			switch (deviceType)
			{
			case ClientInfo.EDeviceType.Linux:
				eplayGroup = EPlayGroup.Standalone;
				break;
			case ClientInfo.EDeviceType.Mac:
				eplayGroup = EPlayGroup.Standalone;
				break;
			case ClientInfo.EDeviceType.Windows:
				eplayGroup = EPlayGroup.Standalone;
				break;
			case ClientInfo.EDeviceType.PlayStation:
				eplayGroup = EPlayGroup.PS5;
				break;
			case ClientInfo.EDeviceType.Xbox:
				eplayGroup = EPlayGroup.XBS;
				break;
			case ClientInfo.EDeviceType.Unknown:
				eplayGroup = EPlayGroup.Standalone;
				break;
			default:
				throw new ArgumentOutOfRangeException("deviceType", deviceType, string.Format("Missing play group mapping for {0}.", deviceType));
			}
			return eplayGroup;
		}

		public static uint[] GetCurrentlyAllowedPlatformIds()
		{
			if (PermissionsManager.IsCrossplayAllowed())
			{
				return null;
			}
			return EPlayGroupExtensions.s_playGroupToAllowedPlatformIds[EPlayGroupExtensions.Current];
		}

		public static readonly EPlayGroup Current = EPlayGroupExtensions.GetCurrentPlayGroup();

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly Dictionary<EPlayGroup, uint[]> s_playGroupToAllowedPlatformIds = new Dictionary<EPlayGroup, uint[]>
		{
			{
				EPlayGroup.Standalone,
				null
			},
			{
				EPlayGroup.XBS,
				null
			},
			{
				EPlayGroup.PS5,
				null
			}
		};
	}
}
