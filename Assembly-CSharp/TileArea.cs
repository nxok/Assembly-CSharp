using System;
using System.Collections.Generic;

public class TileArea<T> : ITileArea<T> where T : class
{
	public TileArea(TileAreaConfig _config, T[,] _data = null)
	{
		this.config = _config;
		if (_data != null)
		{
			for (int i = 0; i < _data.GetLength(0); i++)
			{
				for (int j = 0; j < _data.GetLength(1); j++)
				{
					int num = i + this.config.tileStart.x;
					int num2 = j + this.config.tileStart.y;
					uint num3 = TileAreaUtils.MakeKey(num, num2);
					this.Data[num3] = _data[i, j];
				}
			}
		}
	}

	public TileAreaConfig Config
	{
		get
		{
			return this.config;
		}
	}

	public void Remove(uint _key)
	{
		this.Data.Remove(_key);
	}

	public T this[int _tileX, int _tileZ]
	{
		get
		{
			this.config.checkCoordinates(ref _tileX, ref _tileZ);
			uint num = TileAreaUtils.MakeKey(_tileX, _tileZ);
			T t;
			if (!this.Data.TryGetValue(num, out t))
			{
				return default(T);
			}
			return t;
		}
		set
		{
			this.config.checkCoordinates(ref _tileX, ref _tileZ);
			uint num = TileAreaUtils.MakeKey(_tileX, _tileZ);
			this.Data[num] = value;
		}
	}

	public T this[uint _key]
	{
		get
		{
			T t;
			if (!this.Data.TryGetValue(_key, out t))
			{
				return default(T);
			}
			return t;
		}
		set
		{
			this.Data[_key] = value;
		}
	}

	public TileAreaConfig config;

	public Dictionary<uint, T> Data = new Dictionary<uint, T>();
}
