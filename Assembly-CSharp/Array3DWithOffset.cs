using System;
using System.Runtime.CompilerServices;

public class Array3DWithOffset<T>
{
	public Array3DWithOffset()
	{
	}

	public Array3DWithOffset(int _dimX, int _dimY, int _dimZ)
	{
		this.DimX = _dimX;
		this.DimY = _dimY;
		this.DimZ = _dimZ;
		this.data = new T[_dimX * _dimY * _dimZ];
		this.addX = _dimX / 2;
		this.addY = _dimY / 2;
		this.addZ = _dimZ / 2;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetIndex(int _x, int _y, int _z)
	{
		return _x + this.addX + (_z + this.addZ) * this.DimX + (_y + this.addY) * this.DimZ * this.DimX;
	}

	public virtual T this[int _x, int _y, int _z]
	{
		get
		{
			return this.data[this.GetIndex(_x, _y, _z)];
		}
		set
		{
			this.data[this.GetIndex(_x, _y, _z)] = value;
		}
	}

	public bool Contains(int _x, int _y, int _z)
	{
		return _x >= -this.addX && _y >= -this.addY && _x < this.addX - 1 && _y < this.addY - 1 && _z < this.addZ - 1 && _z < this.addZ - 1;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public T[] data;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int addX;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int addY;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int addZ;

	public int DimX;

	public int DimY;

	public int DimZ;
}
