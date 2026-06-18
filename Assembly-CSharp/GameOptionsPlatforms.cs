using System;
using Platform;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public static class GameOptionsPlatforms
{
	public static float GetStreamingMipmapBudget()
	{
		return (float)SystemInfo.graphicsMemorySize * 0.9f;
	}

	public static string GetItemIconFilterString()
	{
		string text = "mip0";
		if ((float)SystemInfo.graphicsMemorySize <= 3200f || SystemInfo.systemMemorySize < 6800)
		{
			text = "mip1";
		}
		return text;
	}

	public static int CalcTextureQualityMin()
	{
		int systemMemorySize = SystemInfo.systemMemorySize;
		if (SystemInfo.graphicsMemorySize < 2400 || systemMemorySize < 4900)
		{
			return 1;
		}
		return 0;
	}

	public static GameOptionsPlatforms.GfxPreset CalcDefaultGfxPreset()
	{
		if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
		{
			GameOptionsPlatforms.GfxPreset gfxPreset = GameOptionsPlatforms.GfxPreset.Medium;
			float num = (float)SystemInfo.systemMemorySize;
			float num2 = (float)SystemInfo.graphicsMemorySize;
			if (!SystemInfo.operatingSystem.Contains(" Steam ") && (num2 < 2400f || num < 4800f))
			{
				gfxPreset = GameOptionsPlatforms.GfxPreset.Low;
			}
			if (num2 > 7500f && num > 5200f)
			{
				string text = SystemInfo.graphicsDeviceVendor.ToLower();
				if (text.Contains("nvidia"))
				{
					if (!GameOptionsPlatforms.FindGfxName(" 1070, 305"))
					{
						gfxPreset = GameOptionsPlatforms.GfxPreset.High;
						if (GameOptionsPlatforms.FindGfxName(" 208, 307, 308, 309, 407, 408, 409, 507, 508, 509"))
						{
							gfxPreset = GameOptionsPlatforms.GfxPreset.Ultra;
						}
					}
				}
				else if ((text == "amd" || text == "ati") && !GameOptionsPlatforms.FindGfxName("RX 570,RX 580,RX 590,RX 5500,RX 65,RX 66"))
				{
					gfxPreset = GameOptionsPlatforms.GfxPreset.High;
					if (GameOptionsPlatforms.FindGfxName(" 680, 690, 695, 770, 780, 790, 907, 908, 909"))
					{
						gfxPreset = GameOptionsPlatforms.GfxPreset.Ultra;
					}
				}
			}
			return gfxPreset;
		}
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			return GameOptionsPlatforms.GfxPreset.ConsolePerformance;
		}
		return GameOptionsPlatforms.GfxPreset.Medium;
	}

	public static int DefaultUpscalerMode
	{
		get
		{
			if (!FSR3.FSR3Supported())
			{
				return 4;
			}
			return 2;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool FindGfxName(string names)
	{
		string text = SystemInfo.graphicsDeviceName.ToLower();
		if (text.Contains("laptop"))
		{
			return false;
		}
		string[] array = names.Split(',', StringSplitOptions.None);
		for (int i = 0; i < array.Length; i++)
		{
			if (text.Contains(array[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static int ApplyTextureFilterLimit(int filter)
	{
		if ((float)SystemInfo.graphicsMemorySize < 3200f)
		{
			filter = 0;
		}
		return filter;
	}

	public enum GfxPreset
	{
		Lowest,
		Low,
		Medium,
		High,
		Ultra,
		Custom,
		ConsolePerformance,
		LEGACY_ConsolePerformanceFSR,
		ConsoleQuality,
		LEGACY_ConsoleQualityFSR,
		Simplified = 100
	}

	public static class UpscalerMode
	{
		public static string ToString(int upscalerSettingValue)
		{
			switch (upscalerSettingValue)
			{
			case 0:
				return "Off";
			case 1:
				return "FSR2";
			case 2:
				return "FSR3";
			case 3:
				return "Dynamic Resolution";
			case 4:
				return "Scale";
			case 5:
				return "DLSS";
			default:
				return "Unknown";
			}
		}

		public const int Off = 0;

		public const int FSR2 = 1;

		public const int FSR3 = 2;

		public const int DynamicResolution = 3;

		public const int Scale = 4;

		public const int DLSS = 5;
	}
}
