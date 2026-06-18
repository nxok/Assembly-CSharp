using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DecoOccupiedMap
{
	public DecoOccupiedMap(int _worldWidth, int _worldHeight)
	{
		this.width = _worldWidth;
		this.height = _worldHeight;
		this.widthHalf = this.width / 2;
		this.heightHalf = this.height / 2;
		this.occupiedMap = new EnumDecoOccupied[this.width * this.height];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public EnumDecoOccupied Get(int _offs)
	{
		return this.occupiedMap[_offs];
	}

	public EnumDecoOccupied Get(int _x, int _z)
	{
		int num = DecoManager.CheckPosition(this.width, this.height, _x, _z);
		if (num >= 0)
		{
			return this.occupiedMap[num];
		}
		return EnumDecoOccupied.NoneAllowed;
	}

	public void Set(int _offs, EnumDecoOccupied _v)
	{
		this.occupiedMap[_offs] = _v;
	}

	public void Set(int _x, int _z, EnumDecoOccupied _v)
	{
		int num = DecoManager.CheckPosition(this.width, this.height, _x, _z);
		if (num >= 0)
		{
			this.occupiedMap[num] = _v;
		}
	}

	public bool CheckArea(int _x, int _z, EnumDecoOccupied _v, int _rectSizeX, int _rectSizeZ)
	{
		int num = DecoManager.CheckPosition(this.width, this.height, _x, _z);
		if (num < 0)
		{
			return true;
		}
		for (int i = 0; i < _rectSizeZ; i++)
		{
			for (int j = 0; j < _rectSizeX; j++)
			{
				if (num >= this.occupiedMap.Length)
				{
					return true;
				}
				if (this.occupiedMap[num] >= _v)
				{
					return true;
				}
				num++;
			}
			num += this.width - _rectSizeX;
		}
		return false;
	}

	public void SetArea(int _x, int _z, EnumDecoOccupied _v, int _rectSizeX, int _rectSizeZ)
	{
		int num = _x + this.widthHalf + (_z + this.heightHalf) * this.width;
		for (int i = 0; i < _rectSizeZ; i++)
		{
			for (int j = 0; j < _rectSizeX; j++)
			{
				if (num < 0 || num >= this.occupiedMap.Length)
				{
					num++;
				}
				else
				{
					if (this.occupiedMap[num] < _v)
					{
						this.occupiedMap[num] = _v;
					}
					num++;
				}
			}
			num += this.width - _rectSizeX;
		}
	}

	public EnumDecoOccupied[] GetData()
	{
		return this.occupiedMap;
	}

	public void SaveAsTexture(string path, bool includeFlatAreas = false, List<FlatArea> flatAreas = null)
	{
		Color32[] array = new Color32[this.occupiedMap.Length];
		for (int i = 0; i < this.occupiedMap.Length; i++)
		{
			Color color = Color.black;
			switch (this.occupiedMap[i])
			{
			case EnumDecoOccupied.SmallSlope:
				color = Color.blue;
				break;
			case EnumDecoOccupied.Stop_BigDeco:
				color = Color.gray;
				break;
			case EnumDecoOccupied.Perimeter:
				if (!includeFlatAreas)
				{
					color = Color.red;
				}
				break;
			case EnumDecoOccupied.Stop_AnyDeco:
				color = Color.cyan;
				break;
			case EnumDecoOccupied.Deco:
				if (!includeFlatAreas)
				{
					color = Color.green;
				}
				break;
			case EnumDecoOccupied.POI:
				color = Color.magenta;
				break;
			case EnumDecoOccupied.BigSlope:
				color = Color.yellow;
				break;
			case EnumDecoOccupied.NoneAllowed:
				color = Color.white;
				break;
			}
			array[i] = color;
		}
		if (includeFlatAreas)
		{
			if (flatAreas == null || flatAreas.Count == 0)
			{
				FlatAreaManager flatAreaManager = GameManager.Instance.World.FlatAreaManager;
				flatAreas = ((flatAreaManager != null) ? flatAreaManager.GetAllFlatAreas() : null);
			}
			if (flatAreas != null)
			{
				foreach (FlatArea flatArea in flatAreas)
				{
					foreach (Vector2i vector2i in flatArea.GetPositions())
					{
						Color red;
						if (vector2i.x == flatArea.position.x && vector2i.y == flatArea.position.z)
						{
							red = Color.red;
						}
						else if (flatArea.size == 16)
						{
							red..ctor(0.75f, 0.75f, 0.75f, 1f);
						}
						else
						{
							red..ctor(0.5f, 0.5f, 0.5f, 1f);
						}
						int num = DecoManager.CheckPosition(this.width, this.height, vector2i.x, vector2i.y);
						array[num] = red;
					}
				}
			}
		}
		Texture2D texture2D = new Texture2D(this.width, this.height);
		texture2D.SetPixels32(array);
		texture2D.Apply();
		TextureUtils.SaveTexture(texture2D, path);
		Log.Out("Saved deco texture to {0}", new object[] { path });
		Object.Destroy(texture2D);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumDecoOccupied[] occupiedMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public int width;

	[PublicizedFrom(EAccessModifier.Private)]
	public int height;

	[PublicizedFrom(EAccessModifier.Private)]
	public int widthHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public int heightHalf;
}
