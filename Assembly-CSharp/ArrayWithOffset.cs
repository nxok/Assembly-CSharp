using System;

public class ArrayWithOffset<T>
{
	public ArrayWithOffset()
	{
	}

	public ArrayWithOffset(int _dimX, int _dimY)
		: this(_dimX, _dimY, 0, 0)
	{
	}

	public ArrayWithOffset(int _dimX, int _dimY, int _addXOffs, int _addYOffs)
	{
		this.DimX = _dimX;
		this.DimY = _dimY;
		this.data = new T[_dimX, _dimY];
		this.sizeXHalf = _dimX / 2;
		this.sizeYHalf = _dimY / 2;
		this.MinPos = new Vector2i(-this.sizeXHalf - _addXOffs, -this.sizeYHalf - _addYOffs);
		this.MaxPos = new Vector2i(this.sizeXHalf - _addXOffs - 1, this.sizeYHalf - _addYOffs - 1);
		this.addXOffs = _addXOffs + this.sizeXHalf;
		this.addYOffs = _addYOffs + this.sizeXHalf;
	}

	public virtual T this[int _x, int _y]
	{
		get
		{
			return this.data[_x + this.addXOffs, _y + this.addYOffs];
		}
		set
		{
			this.data[_x + this.addXOffs, _y + this.addYOffs] = value;
		}
	}

	public bool Contains(int _x, int _y)
	{
		return _x >= this.MinPos.x && _y >= this.MinPos.y && _x < this.MaxPos.x && _y < this.MaxPos.y;
	}

	public void CopyInto(ArrayWithOffset<T> _other)
	{
		for (int i = 0; i < this.data.GetLength(0); i++)
		{
			for (int j = 0; j < this.data.GetLength(1); j++)
			{
				_other.data[i, j] = this.data[i, j];
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public T[,] data;

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
