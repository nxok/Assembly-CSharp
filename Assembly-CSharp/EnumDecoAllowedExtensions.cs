using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class EnumDecoAllowedExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EnumDecoAllowedSlope GetSlope(this EnumDecoAllowed decoAllowed)
	{
		return (EnumDecoAllowedSlope)((decoAllowed & (EnumDecoAllowed.SlopeLo | EnumDecoAllowed.SlopeHi)) / EnumDecoAllowed.SlopeLo);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EnumDecoAllowed WithSlope(this EnumDecoAllowed decoAllowed, EnumDecoAllowedSlope slope)
	{
		return (EnumDecoAllowed)(((int)decoAllowed & -4) | (int)slope);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EnumDecoAllowedSize GetSize(this EnumDecoAllowed decoAllowed)
	{
		return (EnumDecoAllowedSize)((decoAllowed & (EnumDecoAllowed.SizeLo | EnumDecoAllowed.SizeHi)) / EnumDecoAllowed.SizeLo);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EnumDecoAllowed WithSize(this EnumDecoAllowed decoAllowed, EnumDecoAllowedSize size)
	{
		return (EnumDecoAllowed)(((int)decoAllowed & -13) | (int)(size * (EnumDecoAllowedSize)4));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool AllowBigDeco(this EnumDecoAllowed decoAllowed)
	{
		return decoAllowed.GetSize() == EnumDecoAllowedSize.Any;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool AllowSmallDeco(this EnumDecoAllowed decoAllowed)
	{
		return decoAllowed.GetSize() < EnumDecoAllowedSize.None;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool GetStreetOnly(this EnumDecoAllowed decoAllowed)
	{
		return (decoAllowed & EnumDecoAllowed.StreetOnly) == EnumDecoAllowed.StreetOnly;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EnumDecoAllowed WithStreetOnly(this EnumDecoAllowed decoAllowed, bool streetOnly)
	{
		if (streetOnly)
		{
			return decoAllowed | EnumDecoAllowed.StreetOnly;
		}
		return (EnumDecoAllowed)((int)decoAllowed & -17);
	}

	public static bool IsNothing(this EnumDecoAllowed decoAllowed)
	{
		return decoAllowed.GetSlope().IsNothing() || decoAllowed.GetSize().IsNothing();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNothing(this EnumDecoAllowedSlope decoSlope)
	{
		return decoSlope >= EnumDecoAllowedSlope.Steep;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNothing(this EnumDecoAllowedSize decoSize)
	{
		return decoSize >= EnumDecoAllowedSize.None;
	}

	public static string ToStringFriendlyCached(this EnumDecoAllowed decoAllowed)
	{
		string text;
		if (EnumDecoAllowedExtensions.s_toStringCache.TryGetValue(decoAllowed, out text))
		{
			return text;
		}
		string text2 = EnumDecoAllowedExtensions.ToStringInternal(decoAllowed);
		EnumDecoAllowedExtensions.s_toStringCache[decoAllowed] = text2;
		return text2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string ToStringInternal(EnumDecoAllowed decoAllowed)
	{
		if (decoAllowed == EnumDecoAllowed.Everything)
		{
			return "Everything";
		}
		if (decoAllowed == EnumDecoAllowed.Nothing)
		{
			return "Nothing";
		}
		List<string> list = new List<string>();
		EnumDecoAllowedSlope slope = decoAllowed.GetSlope();
		if (slope > EnumDecoAllowedSlope.Flat)
		{
			list.Add(slope.ToStringCached<EnumDecoAllowedSlope>());
		}
		EnumDecoAllowedSize size = decoAllowed.GetSize();
		if (size > EnumDecoAllowedSize.Any)
		{
			list.Add(size.ToStringCached<EnumDecoAllowedSize>());
		}
		if (decoAllowed.GetStreetOnly())
		{
			list.Add("StreetOnly");
		}
		if (list.Count > 0)
		{
			return string.Join(",", list);
		}
		return "Unknown";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly EnumDictionary<EnumDecoAllowed, string> s_toStringCache = new EnumDictionary<EnumDecoAllowed, string>();
}
