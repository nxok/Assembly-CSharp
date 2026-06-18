using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TerrainFromRaw : TerrainGeneratorWithBiomeResource
{
	public void Init(HeightMap _heightMap, IBiomeProvider _biomeProvider, int _seed)
	{
		base.Init(null, _biomeProvider, _seed);
		this.heightMap = _heightMap;
		this.terrainWidth = this.heightMap.GetWidth() << this.heightMap.GetScaleShift();
		this.terrainHeight = this.heightMap.GetHeight() << this.heightMap.GetScaleShift();
		this.terrainWidthHalf = this.terrainWidth / 2;
		this.terrainHeightHalf = this.terrainHeight / 2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool checkCoordinates(ref int _x, ref int _z)
	{
		_x += this.terrainWidthHalf;
		if ((ulong)_x >= (ulong)((long)this.terrainWidth))
		{
			return false;
		}
		_z += this.terrainHeightHalf;
		return (ulong)_z < (ulong)((long)this.terrainHeight);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void fillDensityInBlock(Chunk _chunk, int _x, int _y, int _z, BlockValue _bv)
	{
		int num = (_chunk.X << 4) + _x;
		int num2 = (_chunk.Z << 4) + _z;
		if (!this.checkCoordinates(ref num, ref num2))
		{
			return;
		}
		float num3 = this.heightMap.GetAt(num, num2) + 0.5f;
		int num4 = (int)num3;
		sbyte b;
		if (_y < num4)
		{
			b = MarchingCubes.DensityTerrain;
		}
		else if (_y > num4 + 1)
		{
			b = MarchingCubes.DensityAir;
		}
		else
		{
			float num5 = num3 - (float)num4;
			if (num4 == _y)
			{
				b = (sbyte)((float)MarchingCubes.DensityTerrain * num5);
			}
			else
			{
				b = (sbyte)((float)MarchingCubes.DensityAir * (1f - num5));
			}
			if (b == 0)
			{
				if (_bv.Block.shape.IsTerrain())
				{
					b = -1;
				}
				else
				{
					b = 1;
				}
			}
		}
		_chunk.SetDensity(_x, _y, _z, b);
	}

	public override sbyte GetDensityAt(int _xWorld, int _yWorld, int _zWorld, BiomeDefinition _bd, float _biomeIntensity)
	{
		if (!this.checkCoordinates(ref _xWorld, ref _zWorld))
		{
			return MarchingCubes.DensityAir;
		}
		if (this.heightMap.GetAt(_xWorld, _zWorld) > (float)_yWorld)
		{
			return MarchingCubes.DensityAir;
		}
		return MarchingCubes.DensityTerrain;
	}

	public override byte GetTerrainHeightByteAt(int _x, int _z)
	{
		if (!this.checkCoordinates(ref _x, ref _z))
		{
			return 0;
		}
		return (byte)(this.heightMap.GetAt(_x, _z) + 0.5f);
	}

	public override float GetTerrainHeightAt(int _x, int _z)
	{
		if (!this.checkCoordinates(ref _x, ref _z))
		{
			return 0f;
		}
		return this.heightMap.GetAt(_x, _z);
	}

	public List<float[,]> ConvertToUnityHeightmap(int _sliceAtWidth)
	{
		int width = this.heightMap.GetWidth();
		int height = this.heightMap.GetHeight();
		int scaleSteps = this.heightMap.GetScaleSteps();
		List<float[,]> list = new List<float[,]>();
		int num = width / _sliceAtWidth;
		int num2 = height / _sliceAtWidth;
		new Terrain[num, num2];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				float[,] array = new float[_sliceAtWidth, _sliceAtWidth];
				for (int k = _sliceAtWidth - 1; k >= 0; k--)
				{
					for (int l = _sliceAtWidth - 1; l >= 0; l--)
					{
						float at = this.heightMap.GetAt(j * _sliceAtWidth + k * scaleSteps, i * _sliceAtWidth + l * scaleSteps);
						array[l, k] = at / 256f;
					}
				}
				list.Add(array);
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public HeightMap heightMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public int terrainWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int terrainHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public int terrainWidthHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public int terrainHeightHalf;
}
