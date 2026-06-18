using System;
using System.Collections.Generic;

public class CachedStringFormatter<T1>
{
	public CachedStringFormatter(Func<T1, string> _formatterFunc)
	{
		this.formatter = _formatterFunc;
	}

	public string Format(T1 _v1)
	{
		bool flag;
		return this.Format(_v1, out flag);
	}

	public string Format(T1 _v1, out bool _valueChanged)
	{
		_valueChanged = this.cachedResult == null;
		if (!this.comparer1.Equals(this.oldValue1, _v1))
		{
			this.oldValue1 = _v1;
			_valueChanged = true;
		}
		if (_valueChanged)
		{
			this.cachedResult = this.formatter(_v1);
		}
		return this.cachedResult;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Func<T1, string> formatter;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedResult;

	[PublicizedFrom(EAccessModifier.Private)]
	public T1 oldValue1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T1> comparer1 = EqualityComparer<T1>.Default;
}
