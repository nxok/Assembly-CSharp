using System;

public class TileAreaSimple<T> : ITileArea<T> where T : class
{
	public TileAreaSimple(T[,] _data = null)
	{
		this.data = _data;
	}

	public T this[uint _key]
	{
		get
		{
			return this.data[0, 0];
		}
	}

	public T this[int _tileX, int _tileZ]
	{
		get
		{
			return this.data[0, 0];
		}
	}

	public TileAreaConfig Config
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public T[,] data;
}
