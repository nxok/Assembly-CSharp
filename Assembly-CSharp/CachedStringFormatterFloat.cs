using System;

public class CachedStringFormatterFloat : CachedStringFormatter<float>
{
	public CachedStringFormatterFloat(string _format = null)
		: base(null)
	{
		this.formatter = new Func<float, string>(this.formatterFunc);
		this.format = _format;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string formatterFunc(float _f)
	{
		return _f.ToCultureInvariantString(this.format);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string format;
}
