using System;
using System.IO;

public class SmartArray
{
	public SmartArray(int xPow, int yPow, int zPos)
	{
		this._lXPow = xPow;
		this._lYPow = yPow;
		this._lZPow = zPos;
		this._size = (1 << this._lXPow) * (1 << this._lYPow) * (1 << this._lZPow);
		this._halfSize = this._size / 2;
		this._array = new byte[this._halfSize];
	}

	public void clear()
	{
		for (int i = 0; i < this._array.Length; i++)
		{
			this._array[i] = 0;
		}
	}

	public void write(BinaryWriter stream)
	{
		stream.Write(this._array);
	}

	public void read(BinaryReader stream)
	{
		this._array = stream.ReadBytes(this._halfSize);
	}

	public byte get(int x, int y, int z)
	{
		int num = (x << this._lXPow << this._lYPow) + (y << this._lXPow) + z;
		if (num < this._halfSize)
		{
			return this._array[num] & 15;
		}
		return (byte)((this._array[num % this._halfSize] >> 4) & 15);
	}

	public void set(int x, int y, int z, byte b)
	{
		int num = (x << this._lXPow << this._lYPow) + (y << this._lXPow) + z;
		int num2;
		if (num < this._halfSize)
		{
			num2 = (int)((this._array[num] & 240) | (b & 15));
			this._array[num] = (byte)num2;
			return;
		}
		num2 = (((int)b << 4) & 240) | (int)(this._array[num % this._halfSize] & 15);
		this._array[num % this._halfSize] = (byte)num2;
	}

	public int size()
	{
		return this._size;
	}

	public int sizePacked()
	{
		return this._halfSize;
	}

	public void copyFrom(SmartArray _other)
	{
		_other._array.CopyTo(this._array, 0);
	}

	public int GetUsedMem()
	{
		return this._array.Length;
	}

	public byte[] _array;

	[PublicizedFrom(EAccessModifier.Private)]
	public int _lXPow;

	[PublicizedFrom(EAccessModifier.Private)]
	public int _lYPow;

	[PublicizedFrom(EAccessModifier.Private)]
	public int _lZPow;

	public int _size;

	public int _halfSize;
}
