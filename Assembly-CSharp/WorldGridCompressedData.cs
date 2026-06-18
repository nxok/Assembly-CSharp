using System;
using System.Runtime.CompilerServices;

public class WorldGridCompressedData<[IsUnmanaged] T> where T : struct, ValueType, IEquatable<T>
{
	public WorldGridCompressedData(T[] _colors, int _dimX, int _dimY, int _gridSizeX, int _gridSizeY)
		: this(_colors, _dimX, _dimY, _gridSizeX, _gridSizeY, 0, 0)
	{
	}

	public WorldGridCompressedData(T[] _colors, int _dimX, int _dimY, int _gridSizeX, int _gridSizeY, int _addXOffs, int _addYOffs)
	{
		this.colors = new GridCompressedData<T>(_dimX, _dimY, _gridSizeX, _gridSizeY);
		this.colors.FromArray(_colors);
		this.DimX = _dimX;
		this.DimY = _dimY;
		this.sizeXHalf = _dimX / 2;
		this.sizeYHalf = _dimY / 2;
		this.addXOffs = _addXOffs;
		this.addYOffs = _addYOffs;
		this.MinPos = new Vector2i(-this.sizeXHalf - this.addXOffs, -this.sizeYHalf - this.addYOffs);
		this.MaxPos = new Vector2i(this.sizeXHalf - this.addXOffs - 1, this.sizeYHalf - this.addYOffs - 1);
	}

	public WorldGridCompressedData(GridCompressedData<T> _data)
		: this(_data, 0, 0)
	{
	}

	public WorldGridCompressedData(GridCompressedData<T> _data, int _addXOffs, int _addYOffs)
	{
		this.colors = _data;
		this.DimX = _data.width;
		this.DimY = _data.height;
		this.sizeXHalf = this.DimX / 2;
		this.sizeYHalf = this.DimY / 2;
		this.addXOffs = _addXOffs;
		this.addYOffs = _addYOffs;
		this.MinPos = new Vector2i(-this.sizeXHalf - this.addXOffs, -this.sizeYHalf - this.addYOffs);
		this.MaxPos = new Vector2i(this.sizeXHalf - this.addXOffs - 1, this.sizeYHalf - this.addYOffs - 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T GetData(int _x, int _y)
	{
		return this.colors.GetValue(_x + this.addXOffs + this.sizeXHalf, _y + this.addYOffs + this.sizeYHalf);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T GetData(int _offs)
	{
		return this.colors.GetValue(_offs);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(int _x, int _y)
	{
		return _x >= this.MinPos.x && _y >= this.MinPos.y && _x <= this.MaxPos.x && _y <= this.MaxPos.y;
	}

	public GridCompressedData<T> colors;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sizeXHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sizeYHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public int addXOffs;

	[PublicizedFrom(EAccessModifier.Private)]
	public int addYOffs;

	public int DimX;

	public int DimY;

	public Vector2i MinPos;

	public Vector2i MaxPos;
}
