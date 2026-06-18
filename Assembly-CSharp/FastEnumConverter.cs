using System;
using Unity.Collections.LowLevel.Unsafe;

public static class FastEnumConverter<TEnum> where TEnum : struct, IConvertible
{
	public static int ToInt(TEnum _enum)
	{
		return (int)((long)UnsafeUtility.EnumToInt<TEnum>(_enum) & FastEnumConverter<TEnum>.underlyingMask);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly int underlyingSize = UnsafeUtility.SizeOf(Enum.GetUnderlyingType(typeof(TEnum)));

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly long underlyingMask = ((FastEnumConverter<TEnum>.underlyingSize >= 8) ? (-1L) : ((1L << FastEnumConverter<TEnum>.underlyingSize * 8) - 1L));
}
