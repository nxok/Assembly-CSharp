using System;
using System.Collections.Generic;

public class CachedStringFormatter<T1, T2>
{
	public CachedStringFormatter(Func<T1, T2, string> _formatterFunc)
	{
		this.formatter = _formatterFunc;
	}

	public string Format(T1 _v1, T2 _v2)
	{
		bool flag;
		return this.Format(_v1, _v2, out flag);
	}

	public string Format(T1 _v1, T2 _v2, out bool _valueChanged)
	{
		_valueChanged = this.cachedResult == null;
		if (!this.comparer1.Equals(this.oldValue1, _v1))
		{
			this.oldValue1 = _v1;
			_valueChanged = true;
		}
		if (!this.comparer2.Equals(this.oldValue2, _v2))
		{
			this.oldValue2 = _v2;
			_valueChanged = true;
		}
		if (_valueChanged)
		{
			this.cachedResult = this.formatter(_v1, _v2);
		}
		return this.cachedResult;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Func<T1, T2, string> formatter;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedResult;

	[PublicizedFrom(EAccessModifier.Private)]
	public T1 oldValue1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T1> comparer1 = EqualityComparer<T1>.Default;

	[PublicizedFrom(EAccessModifier.Private)]
	public T2 oldValue2;

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEqualityComparer<T2> comparer2 = EqualityComparer<T2>.Default;
}
