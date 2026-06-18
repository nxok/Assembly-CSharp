using System;
using UnityEngine;

public sealed class HeightMap : IDisposable
{
	public HeightMap(int _w, int _h, float _maxHeight, IBackedArray<ushort> _data, int _targetSize = 0)
	{
		this.w = _w;
		this.h = _h;
		this.scaleShift = ((_targetSize != 0) ? ((int)Mathf.Log((float)(_targetSize / _w), 2f)) : 0);
		this.scalePixs = _targetSize / _w;
		this.maxHeight = _maxHeight;
		this.data = BackedArrays.CreateSingleView<ushort>(_data, BackedArrayHandleMode.ReadOnly, 16 * _w);
	}

	public void Dispose()
	{
		IBackedArrayView<ushort> backedArrayView = this.data;
		if (backedArrayView != null)
		{
			backedArrayView.Dispose();
		}
		this.data = null;
	}

	public float GetAt(int _x, int _z)
	{
		ushort num;
		if (this.scaleShift == 0 && _x + _z * this.w < this.data.Length)
		{
			num = this.data[_x + _z * this.w];
		}
		else
		{
			num = this.getInterpolatedHeight(_x, _z);
		}
		return (float)num * (this.maxHeight / 65535f);
	}

	public ushort getInterpolatedHeight(int xf, int zf)
	{
		object obj = ((xf >= 0) ? (xf >> this.scaleShift) : (xf - this.scalePixs + 1 >> this.scaleShift));
		int num = ((zf >= 0) ? (zf >> this.scaleShift) : (zf - this.scalePixs + 1 >> this.scaleShift));
		object obj2 = obj;
		int num2 = obj2 + num * this.w;
		ushort num3 = this.data[(num2 + this.w) & (this.data.Length - 1)];
		ushort num4 = this.data[num2 & (this.data.Length - 1)];
		ushort num5 = this.data[(num2 + 1) & (this.data.Length - 1)];
		ushort num6 = this.data[(num2 + 1 + this.w) & (this.data.Length - 1)];
		int num7 = obj2 << (this.scaleShift & 31);
		int num8 = num << this.scaleShift;
		float num9 = 1f - (float)(zf - num8) / (float)this.scalePixs;
		float num10 = (float)(xf - num7) / (float)this.scalePixs;
		return (ushort)((1f - num9) * ((1f - num10) * (float)num3 + num10 * (float)num6) + num9 * ((1f - num10) * (float)num4 + num10 * (float)num5));
	}

	public float GetAt(int _offs)
	{
		ushort num;
		if (this.scaleShift == 0)
		{
			num = this.data[_offs];
		}
		else
		{
			int num2 = _offs / this.w;
			int num3 = _offs % this.w;
			num = this.getInterpolatedHeight(num3, num2);
		}
		return (float)num * this.maxHeight / 65535f;
	}

	public int CalcOffset(int _x, int _z)
	{
		_x >>= this.scaleShift;
		_z >>= this.scaleShift;
		return _x + _z * this.w;
	}

	public int GetWidth()
	{
		return this.w;
	}

	public int GetHeight()
	{
		return this.h;
	}

	public int GetScaleSteps()
	{
		return this.scalePixs;
	}

	public int GetScaleShift()
	{
		return this.scaleShift;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int w;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int h;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int scaleShift;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int scalePixs;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly float maxHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public IBackedArrayView<ushort> data;
}
