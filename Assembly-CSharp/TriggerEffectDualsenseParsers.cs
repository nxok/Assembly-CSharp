using System;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine;

[PublicizedFrom(EAccessModifier.Internal)]
public static class TriggerEffectDualsenseParsers
{
	public static bool ParseWeaponEffects(string effectType, XElement elementTriggerEffect, string name, out byte[] strengths, out TriggerEffectManager.EffectDualsense effectTypeDualsense, out byte frequency, out byte strength, out byte position, out byte endPosition, out byte amplitude)
	{
		effectTypeDualsense = TriggerEffectManager.EffectDualsense.Off;
		strength = 0;
		position = 0;
		endPosition = 0;
		strengths = null;
		frequency = 0;
		amplitude = 0;
		if (effectType.ContainsCaseInsensitive("MultipointWeapon") || effectType.ContainsCaseInsensitive("WeaponMultipoint"))
		{
			Debug.LogError("Trigger Effect " + effectType + " is recognized, but not supported.");
			return false;
		}
		effectTypeDualsense = TriggerEffectManager.EffectDualsense.WeaponSingle;
		return TriggerEffectDualsenseParsers.ParseStartEndPosition(effectType, elementTriggerEffect, name, out position, out endPosition) && TriggerEffectDualsenseParsers.ParseStrength(effectType, elementTriggerEffect, name, out strength);
	}

	public static bool ParseStrength(string effectType, XElement elementTriggerEffect, string name, out byte strength)
	{
		string text;
		if (!elementTriggerEffect.TryGetAttribute("strength", out text))
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, "): strength is missing" }));
			strength = 0;
			return false;
		}
		if (!StringParsers.TryParseUInt8(text, out strength))
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, ") strength failed to parse as float" }));
			return false;
		}
		if (strength >= 10)
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, ") strength is invalid, correct values are 0 to 9 inclusive" }));
			return false;
		}
		return true;
	}

	public static bool ParseStartEndPosition(string effectType, XElement elementTriggerEffect, string name, out byte startPosition, out byte endPosition)
	{
		string text;
		if (!elementTriggerEffect.TryGetAttribute("startPosition", out text))
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, "): startPosition is missing" }));
			startPosition = 0;
			endPosition = 0;
			return false;
		}
		if (!StringParsers.TryParseUInt8(text, out startPosition))
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, ") endPosition failed to parse" }));
			endPosition = 0;
			return false;
		}
		if (startPosition >= 10)
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, ") startPosition is invalid, correct values are 0 to 9 inclusive, and must be less than EndPosition" }));
			endPosition = 0;
			return false;
		}
		string text2;
		if (!elementTriggerEffect.TryGetAttribute("endPosition", out text2))
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, "): endPosition is missing" }));
			startPosition = 0;
			endPosition = 0;
			return false;
		}
		if (!StringParsers.TryParseUInt8(text2, out endPosition))
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, ") endPosition failed to parse" }));
			return false;
		}
		if (endPosition >= 11 || startPosition >= endPosition)
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, ") endPosition is invalid, correct values are 1 to 10 inclusive, and must be greater than StartPosition" }));
			return false;
		}
		return true;
	}

	public static bool ParseStartEndStrengths(string effectType, XElement elementTriggerEffect, string name, out byte startStrength, out byte endStrength)
	{
		string text;
		if (!elementTriggerEffect.TryGetAttribute("startStrength", out text))
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, "): startPosition is missing" }));
			startStrength = 0;
			endStrength = 0;
			return false;
		}
		if (!StringParsers.TryParseUInt8(text, out startStrength))
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, ") endStrength failed to parse" }));
			endStrength = 0;
			return false;
		}
		if (startStrength >= 10)
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, ") startStrength is invalid, correct values are 0 to 9 inclusive" }));
			endStrength = 0;
			return false;
		}
		string text2;
		if (!elementTriggerEffect.TryGetAttribute("endPosition", out text2))
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, "): endPosition is missing" }));
			startStrength = 0;
			endStrength = 0;
			return false;
		}
		if (!StringParsers.TryParseUInt8(text2, out endStrength))
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, ") endPosition failed to parse" }));
			return false;
		}
		if (endStrength >= 11 || startStrength >= endStrength)
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, ") endPosition is invalid, correct values are 1 to 10 inclusive, and must be greater than StartPosition" }));
			return false;
		}
		return true;
	}

	public static bool ParsePosition(string effectType, XElement elementTriggerEffect, string name, out byte position)
	{
		string text;
		if (!elementTriggerEffect.TryGetAttribute("position", out text))
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, "): position is missing" }));
			position = 0;
			return false;
		}
		if (!StringParsers.TryParseUInt8(text, out position))
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, ") position failed to parse" }));
			return false;
		}
		if (position > 9)
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, ") position is invalid, correct values are 0 to 9 inclusive" }));
			return false;
		}
		return true;
	}

	public static bool ParseAmplitude(string effectType, XElement elementTriggerEffect, string name, out byte amplitude)
	{
		string text;
		if (!elementTriggerEffect.TryGetAttribute("amplitude", out text))
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, "): amplitude is missing" }));
			amplitude = 0;
			return false;
		}
		if (!StringParsers.TryParseUInt8(text, out amplitude))
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, ") amplitude failed to parse" }));
			return false;
		}
		if (amplitude > 8)
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, ") amplitude is invalid, correct values are 0 to 8 inclusive" }));
			return false;
		}
		return true;
	}

	public static bool ParseFrequency(string effectType, XElement elementTriggerEffect, string name, out byte frequency)
	{
		string text;
		if (!elementTriggerEffect.TryGetAttribute("frequency", out text))
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, "): frequency is missing" }));
			frequency = 0;
			return false;
		}
		if (!StringParsers.TryParseUInt8(text, out frequency))
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, ") frequency failed to parse, valid values are 0-255 inclusive" }));
			return false;
		}
		return true;
	}

	public static bool ParseEffectStrengths(string effectType, XElement elementTriggerEffect, string name, out byte[] strengths)
	{
		string text;
		if (!elementTriggerEffect.TryGetAttribute("strengths", out text))
		{
			Debug.LogError(string.Concat(new string[] { "Trigger effect ", name, "(", effectType, "): Missing attribute; length 10 array \"strengths\" " }));
			strengths = null;
			return false;
		}
		strengths = new byte[10];
		int num = 0;
		int num2 = 0;
		while (num < text.Length && num2 < 10)
		{
			byte b;
			if (!StringParsers.TryParseUInt8(text, out b, num, num, NumberStyles.Integer))
			{
				Debug.LogError(string.Format("Trigger effect {0}({1}) strengths[{2}] failed to parse at character {3}", new object[] { name, effectType, num2, num }));
				strengths = null;
				break;
			}
			if (b >= 9)
			{
				Debug.LogError(string.Format("Trigger effect {0}({1}) strengths[{2}] is invalid, correct array values are 0 to 8 inclusive, separated by some character", name, effectType, num2));
				break;
			}
			strengths[num2] = b;
			num += 2;
			num2++;
		}
		if (num2 == 11 && strengths != null)
		{
			Debug.LogError(string.Format("Trigger effect {0}({1}) has invalid strength array definition, it's always a 10 length array of 0 to 9 inclusive. actual length:{2}", name, effectType, num2 + 1));
			strengths = null;
			return false;
		}
		return true;
	}
}
