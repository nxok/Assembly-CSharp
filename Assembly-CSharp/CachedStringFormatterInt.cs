using System;

public class CachedStringFormatterInt : CachedStringFormatter<int>
{
	public CachedStringFormatterInt()
		: base(new Func<int, string>(CachedStringFormatterInt.formatterFunc))
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string formatterFunc(int _i)
	{
		return _i.ToString();
	}
}
