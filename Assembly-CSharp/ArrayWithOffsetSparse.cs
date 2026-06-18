using System;
using System.Collections.Generic;

public class ArrayWithOffsetSparse<T>
{
	public ArrayWithOffsetSparse(int _dimX, int _dimY, T _emptyValue)
	{
		this.DimX = _dimX;
		this.DimY = _dimY;
		this.EmptyValue = _emptyValue;
		this.myData = new Dictionary<long, T>();
		this.addX = _dimX / 2;
		this.addY = _dimY / 2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public long makeKey(int _x, int _y)
	{
		return (((long)(_y + this.addY) & (long)((ulong)(-1))) << 32) | ((long)(_x + this.addX) & (long)((ulong)(-1)));
	}

	public bool Contains(int _x, int _y)
	{
		return _x >= -this.addX && _y >= -this.addY && _x < this.addX - 1 && _y < this.addY - 1;
	}

	public virtual T this[int _x, int _y]
	{
		get
		{
			if (!this.myData.ContainsKey(this.makeKey(_x, _y)))
			{
				return this.EmptyValue;
			}
			return this.myData[this.makeKey(_x, _y)];
		}
		set
		{
			this.myData[this.makeKey(_x, _y)] = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, T> myData;

	public T EmptyValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public int addX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int addY;

	public int DimX;

	public int DimY;
}
