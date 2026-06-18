using System;
using UnityEngine;

public class QualityInfo
{
	public static void Cleanup()
	{
		QualityInfo.qualityColors = new Color[7];
		QualityInfo.hexColors = new string[7];
	}

	public static void Add(int _key, string _hexColor)
	{
		QualityInfo.qualityColors[_key] = QualityInfo.HexToRGB(_hexColor);
		QualityInfo.hexColors[_key] = _hexColor;
	}

	public static Color GetQualityColor(int _quality)
	{
		return QualityInfo.GetTierColor(_quality);
	}

	public static Color GetTierColor(int _tier)
	{
		if (_tier > QualityInfo.qualityColors.Length - 1)
		{
			_tier = QualityInfo.qualityColors.Length - 1;
		}
		return QualityInfo.qualityColors[_tier];
	}

	public static string GetQualityColorHex(int _quality)
	{
		if (_quality > QualityInfo.qualityColors.Length - 1)
		{
			_quality = QualityInfo.qualityColors.Length - 1;
		}
		return QualityInfo.hexColors[_quality];
	}

	public static string GetQualityLevelName(int _quality, bool _useQualityColor = false)
	{
		if (_quality == 0)
		{
			return Localization.Get("lblQualityBroken", false, null);
		}
		string text = "";
		_quality /= 1;
		switch (_quality)
		{
		case 0:
			text = Localization.Get("lblQualityDamaged", false, null);
			break;
		case 1:
			text = Localization.Get("lblQualityPoor", false, null);
			break;
		case 2:
			text = Localization.Get("lblQualityAverage", false, null);
			break;
		case 3:
			text = Localization.Get("lblQualityGreat", false, null);
			break;
		case 4:
			text = Localization.Get("lblQualityFlawless", false, null);
			break;
		case 5:
			text = Localization.Get("lblQualityLegendary", false, null);
			break;
		case 6:
			text = Localization.Get("lblQualityLegendary", false, null);
			break;
		}
		if (_useQualityColor)
		{
			text = string.Format("[{0}]{1}[-]", QualityInfo.GetQualityColorHex(_quality), text);
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int HexToInt(char hexChar)
	{
		switch (hexChar)
		{
		case '0':
			return 0;
		case '1':
			return 1;
		case '2':
			return 2;
		case '3':
			return 3;
		case '4':
			return 4;
		case '5':
			return 5;
		case '6':
			return 6;
		case '7':
			return 7;
		case '8':
			return 8;
		case '9':
			return 9;
		case 'A':
			return 10;
		case 'B':
			return 11;
		case 'C':
			return 12;
		case 'D':
			return 13;
		case 'E':
			return 14;
		case 'F':
			return 15;
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color HexToRGB(string color)
	{
		color.Replace("#", "");
		float num = ((float)QualityInfo.HexToInt(color[1]) + (float)QualityInfo.HexToInt(color[0]) * 16f) / 255f;
		float num2 = ((float)QualityInfo.HexToInt(color[3]) + (float)QualityInfo.HexToInt(color[2]) * 16f) / 255f;
		float num3 = ((float)QualityInfo.HexToInt(color[5]) + (float)QualityInfo.HexToInt(color[4]) * 16f) / 255f;
		Color color2 = default(Color);
		color2.r = num;
		color2.g = num2;
		color2.b = num3;
		color2.a = 1f;
		return color2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color[] qualityColors = new Color[7];

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] hexColors = new string[7];
}
