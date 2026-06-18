using System;
using System.Collections.Generic;
using MemoryPack;

public class DataItem<T> : IDataItem
{
	public event DataItem<T>.OnChangeDelegate OnChangeDelegates;

	public string Name
	{
		get
		{
			return this.name;
		}
	}

	[MemoryPackConstructor]
	public DataItem()
		: this(default(T))
	{
	}

	public DataItem(T _startValue)
		: this(null, _startValue)
	{
	}

	public DataItem(string _name, T _startValue)
	{
		this.name = _name;
		this.internalValue = _startValue;
	}

	public T Value
	{
		get
		{
			return this.internalValue;
		}
		set
		{
			T t = this.internalValue;
			this.internalValue = value;
			if (this.OnChangeDelegates != null)
			{
				this.OnChangeDelegates(t, value);
			}
		}
	}

	public override string ToString()
	{
		if (this.Formatter != null)
		{
			return this.Formatter.ToString(this.internalValue);
		}
		if (this.internalValue == null)
		{
			return "null";
		}
		return this.internalValue.ToString();
	}

	public static bool operator ==(DataItem<T> v1, T v2)
	{
		if (v1 != null)
		{
			return EqualityComparer<T>.Default.Equals(v1.internalValue, v2);
		}
		return v2 == null;
	}

	public static bool operator !=(DataItem<T> v1, T v2)
	{
		if (v1 != null)
		{
			return !EqualityComparer<T>.Default.Equals(v1.internalValue, v2);
		}
		return v2 != null;
	}

	public override bool Equals(object obj)
	{
		return obj != null && this.internalValue.Equals(obj);
	}

	public override int GetHashCode()
	{
		return this.internalValue.GetHashCode();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public T internalValue;

	public IDataItemFormatter Formatter;

	public delegate void OnChangeDelegate(T _oldValue, T _newValue);
}
