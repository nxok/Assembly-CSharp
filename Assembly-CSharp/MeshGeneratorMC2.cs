using System;
using UnityEngine;

public class MeshGeneratorMC2 : MeshGenerator
{
	public MeshGeneratorMC2(INeighborBlockCache _nBlocks, ChunkCacheNeighborChunks _nChunks)
		: base(_nBlocks)
	{
		this.nChunks = _nChunks;
	}

	public override bool IsLayerEmpty(int _startLayerIdx, int _endLayerIdx)
	{
		return base.IsLayerEmpty(_startLayerIdx, _endLayerIdx) && this.mc2LayerIsEmpty(_startLayerIdx, _endLayerIdx);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool mc2LayerIsEmpty(int _startLayerIdx, int _endLayerIdx)
	{
		ref Vector3i ptr = new Vector3i(0, Utils.FastMax(1, _startLayerIdx * 16), 0);
		Vector3i vector3i = new Vector3i(15, (_endLayerIdx + 1) * 16 - 1, 15);
		int y = ptr.y;
		IChunk chunk = this.nChunks[0, 0];
		if (chunk.HasSameDensityValue(y))
		{
			int sameDensityValue = (int)chunk.GetSameDensityValue(y);
			int num = Utils.FastMin(vector3i.y + 1, 255);
			for (int i = y - 1; i <= num; i++)
			{
				if (((chunk = this.nChunks[0, 0]) != null && (!chunk.HasSameDensityValue(i) || (int)chunk.GetSameDensityValue(i) != sameDensityValue)) || ((chunk = this.nChunks[1, 0]) != null && (!chunk.HasSameDensityValue(i) || (int)chunk.GetSameDensityValue(i) != sameDensityValue)) || ((chunk = this.nChunks[-1, 0]) != null && (!chunk.HasSameDensityValue(i) || (int)chunk.GetSameDensityValue(i) != sameDensityValue)) || ((chunk = this.nChunks[0, 1]) != null && (!chunk.HasSameDensityValue(i) || (int)chunk.GetSameDensityValue(i) != sameDensityValue)) || ((chunk = this.nChunks[0, -1]) != null && (!chunk.HasSameDensityValue(i) || (int)chunk.GetSameDensityValue(i) != sameDensityValue)) || ((chunk = this.nChunks[1, 1]) != null && (!chunk.HasSameDensityValue(i) || (int)chunk.GetSameDensityValue(i) != sameDensityValue)) || ((chunk = this.nChunks[-1, -1]) != null && (!chunk.HasSameDensityValue(i) || (int)chunk.GetSameDensityValue(i) != sameDensityValue)) || ((chunk = this.nChunks[1, -1]) != null && (!chunk.HasSameDensityValue(i) || (int)chunk.GetSameDensityValue(i) != sameDensityValue)) || ((chunk = this.nChunks[-1, 1]) != null && (!chunk.HasSameDensityValue(i) || (int)chunk.GetSameDensityValue(i) != sameDensityValue)))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateMesh(Vector3i _worldPos, Vector3 _drawPosOffset, Vector3i _start, Vector3i _end, VoxelMesh[] _meshes, bool _bCalcAmbientLight, bool _bOnlyDistortVertices)
	{
		base.CreateMesh(_worldPos, _drawPosOffset, _start, _end, _meshes, _bCalcAmbientLight, _bOnlyDistortVertices);
		if (this.mc2LayerIsEmpty(_start.y / 16, _end.y / 16))
		{
			return;
		}
		int y = _start.y;
		IChunk chunk = this.nChunks[0, 0];
		for (int i = _start.x - 1; i <= _end.x + 2; i++)
		{
			for (int j = _start.z - 1; j <= _end.z + 2; j++)
			{
				if (i >= 0 && i < 16)
				{
					if (j >= 0 && j < 16)
					{
						this.chunksCache[i + 1 + (j + 1) * 19] = chunk;
					}
					else if (j >= 0)
					{
						this.chunksCache[i + 1 + (j + 1) * 19] = this.nChunks[0, 1];
					}
					else
					{
						this.chunksCache[i + 1 + (j + 1) * 19] = this.nChunks[0, -1];
					}
				}
				else if (i >= 0)
				{
					if (j >= 0 && j < 16)
					{
						this.chunksCache[i + 1 + (j + 1) * 19] = this.nChunks[1, 0];
					}
					else if (j >= 0)
					{
						this.chunksCache[i + 1 + (j + 1) * 19] = this.nChunks[1, 1];
					}
					else
					{
						this.chunksCache[i + 1 + (j + 1) * 19] = this.nChunks[1, -1];
					}
				}
				else if (j >= 0 && j < 16)
				{
					this.chunksCache[i + 1 + (j + 1) * 19] = this.nChunks[-1, 0];
				}
				else if (j >= 0)
				{
					this.chunksCache[i + 1 + (j + 1) * 19] = this.nChunks[-1, 1];
				}
				else
				{
					this.chunksCache[i + 1 + (j + 1) * 19] = this.nChunks[-1, -1];
				}
			}
		}
		int num = 0;
		for (int k = _start.x; k <= _end.x; k++)
		{
			for (int l = _start.z; l <= _end.z; l++)
			{
				this.heights[0] = (int)this.chunksCache[k + 1 + (l + 1) * 19].GetHeight(ChunkBlockLayerLegacy.CalcOffset(k, l));
				this.heights[1] = (int)this.chunksCache[k + 1 + 1 + (l + 1) * 19].GetHeight(ChunkBlockLayerLegacy.CalcOffset(k + 1, l));
				this.heights[2] = (int)this.chunksCache[k + 1 + (l + 1 - 1) * 19].GetHeight(ChunkBlockLayerLegacy.CalcOffset(k, l - 1));
				this.heights[3] = (int)this.chunksCache[k + 1 - 1 + (l + 1) * 19].GetHeight(ChunkBlockLayerLegacy.CalcOffset(k - 1, l));
				this.heights[4] = (int)this.chunksCache[k + 1 + (l + 1 + 1) * 19].GetHeight(ChunkBlockLayerLegacy.CalcOffset(k, l + 1));
				this.heights[5] = (int)this.chunksCache[k + 1 + 1 + (l + 1 + 1) * 19].GetHeight(ChunkBlockLayerLegacy.CalcOffset(k + 1, l + 1));
				this.heights[6] = (int)this.chunksCache[k + 1 + 1 + (l + 1 - 1) * 19].GetHeight(ChunkBlockLayerLegacy.CalcOffset(k + 1, l - 1));
				this.heights[7] = (int)this.chunksCache[k + 1 - 1 + (l + 1 + 1) * 19].GetHeight(ChunkBlockLayerLegacy.CalcOffset(k - 1, l + 1));
				this.heights[8] = (int)this.chunksCache[k + 1 - 1 + (l + 1 - 1) * 19].GetHeight(ChunkBlockLayerLegacy.CalcOffset(k - 1, l - 1));
				int num2 = this.heights[0];
				for (int m = 1; m < this.heights.Length; m++)
				{
					if (num2 < this.heights[m])
					{
						num2 = this.heights[m];
					}
				}
				num2 = Utils.FastMin(_end.y, num2);
				num2++;
				num2 = Utils.FastMin(255, num2);
				this.allHeights[k + l * 16] = num2;
				num = Utils.FastMax(num, num2);
			}
		}
		if (y < num)
		{
			int num3 = y + 15;
			if (num3 >= 255)
			{
				num3 = 254;
			}
			this.build(MeshGeneratorMC2.cVectorAdd + _drawPosOffset, new Vector3i(_start.x, y, _start.z), new Vector3i(_end.x, num3, _end.z), _meshes[5], _worldPos);
		}
		Array.Clear(this.chunksCache, 0, this.chunksCache.Length);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sbyte getDensity(int _x, int _y, int _z, int _idxInBlockValues = -1)
	{
		if (_y < 0)
		{
			return MarchingCubes.DensityTerrain;
		}
		if (_y >= 256)
		{
			return MarchingCubes.DensityAir;
		}
		sbyte b = MarchingCubes.DensityAir;
		Chunk chunk = (Chunk)this.chunksCache[_x + 1 + (_z + 1) * 19];
		if (chunk != null)
		{
			b = chunk.GetDensity(_x & 15, _y, _z & 15);
			if (b == 0)
			{
				b = 1;
			}
		}
		return b;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue getBlockValue(int _x, int _y, int _z)
	{
		return ((Chunk)this.chunksCache[_x + 1 + (_z + 1) * 19]).GetBlockNoDamage(_x & 15, _y, _z & 15);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTopSoil(int _x, int _z)
	{
		IChunk chunk = this.chunksCache[_x + 1 + (_z + 1) * 19];
		return chunk != null && chunk.IsTopSoil(_x & 15, _z & 15);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetTerrainHeight(int _x, int _z)
	{
		IChunk chunk = this.chunksCache[_x + 1 + (_z + 1) * 19];
		if (chunk == null)
		{
			return 0;
		}
		return (int)chunk.GetTerrainHeight(_x & 15, _z & 15);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool calcTopSoil(int _x, int _y, int _z)
	{
		_x++;
		_z++;
		int num = this.terrainHeightsCache[_x + _z * this.sizeX];
		if (_y >= num && this.topSoilCache[_x + _z * this.sizeX])
		{
			return true;
		}
		num = this.terrainHeightsCache[_x + 1 + _z * this.sizeX];
		if (_y > num && this.topSoilCache[_x + 1 + _z * this.sizeX])
		{
			return true;
		}
		num = this.terrainHeightsCache[_x + (_z + 1) * this.sizeX];
		if (_y > num && this.topSoilCache[_x + (_z + 1) * this.sizeX])
		{
			return true;
		}
		num = this.terrainHeightsCache[_x + (_z - 1) * this.sizeX];
		if (_y > num && this.topSoilCache[_x + (_z - 1) * this.sizeX])
		{
			return true;
		}
		num = this.terrainHeightsCache[_x - 1 + _z * this.sizeX];
		return _y > num && this.topSoilCache[_x - 1 + _z * this.sizeX];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetTextureFor(int _x, int _y, int _z, int idx)
	{
		BlockValue blockValue = this.blockValues[idx];
		if (!blockValue.isTerrain)
		{
			_x++;
			_z++;
			if (_y > 0)
			{
				blockValue = this.chunksCache[_x + 1 + (_z + 1) * 19].GetBlockNoDamage(_x & 15, _y - 1, _z & 15);
			}
			if (!blockValue.isTerrain)
			{
				for (int i = 0; i < 16; i += 2)
				{
					int num = _x + MeshGeneratorMC2.neighbors[i];
					int num2 = _z + MeshGeneratorMC2.neighbors[i + 1];
					IChunk chunk = this.chunksCache[num + 1 + (num2 + 1) * 19];
					if (chunk != null)
					{
						blockValue = chunk.GetBlockNoDamage(num & 15, _y, num2 & 15);
						if (blockValue.isTerrain)
						{
							break;
						}
					}
				}
			}
			if (!blockValue.isTerrain)
			{
				return VoxelMeshTerrain.EncodeTexIds(1, 1);
			}
		}
		Block block = blockValue.Block;
		int sideTextureId = block.GetSideTextureId(blockValue, BlockFace.Top, 0);
		int sideTextureId2 = block.GetSideTextureId(blockValue, BlockFace.South, 0);
		return VoxelMeshTerrain.EncodeTexIds(sideTextureId, sideTextureId2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void calcLights(int _x, int _y, int _z, out byte _sun, out byte _block)
	{
		int num = _x + MeshGeneratorMC2.adjLightPos[0].x;
		int num2 = _z + MeshGeneratorMC2.adjLightPos[0].y;
		int num3 = 1 + num + (1 + num2) * 19;
		int num4 = _x + MeshGeneratorMC2.adjLightPos[1].x;
		int num5 = _z + MeshGeneratorMC2.adjLightPos[1].y;
		int num6 = 1 + num4 + (1 + num5) * 19;
		int num7 = _x + MeshGeneratorMC2.adjLightPos[2].x;
		int num8 = _z + MeshGeneratorMC2.adjLightPos[2].y;
		int num9 = 1 + num7 + (1 + num8) * 19;
		int num10 = _x + MeshGeneratorMC2.adjLightPos[3].x;
		int num11 = _z + MeshGeneratorMC2.adjLightPos[3].y;
		int num12 = 1 + num10 + (1 + num11) * 19;
		byte b = (byte)Utils.FastMax((int)this.chunksCache[num3].GetLight(num, _y, num2, Chunk.LIGHT_TYPE.SUN), (int)this.chunksCache[num6].GetLight(num4, _y, num5, Chunk.LIGHT_TYPE.SUN), (int)this.chunksCache[num9].GetLight(num7, _y, num8, Chunk.LIGHT_TYPE.SUN), (int)this.chunksCache[num12].GetLight(num10, _y, num11, Chunk.LIGHT_TYPE.SUN));
		byte b2 = ((_y < 255) ? ((byte)Utils.FastMax((int)this.chunksCache[num3].GetLight(num, _y + 1, num2, Chunk.LIGHT_TYPE.SUN), (int)this.chunksCache[num6].GetLight(num4, _y + 1, num5, Chunk.LIGHT_TYPE.SUN), (int)this.chunksCache[num9].GetLight(num7, _y + 1, num8, Chunk.LIGHT_TYPE.SUN), (int)this.chunksCache[num12].GetLight(num10, _y + 1, num11, Chunk.LIGHT_TYPE.SUN))) : 15);
		byte b3 = (byte)Utils.FastMax((int)this.chunksCache[num3].GetLight(num, _y, num2, Chunk.LIGHT_TYPE.BLOCK), (int)this.chunksCache[num6].GetLight(num4, _y, num5, Chunk.LIGHT_TYPE.BLOCK), (int)this.chunksCache[num9].GetLight(num7, _y, num8, Chunk.LIGHT_TYPE.BLOCK), (int)this.chunksCache[num12].GetLight(num10, _y, num11, Chunk.LIGHT_TYPE.BLOCK));
		byte b4 = ((_y < 255) ? ((byte)Utils.FastMax((int)this.chunksCache[num3].GetLight(num, _y + 1, num2, Chunk.LIGHT_TYPE.BLOCK), (int)this.chunksCache[num6].GetLight(num4, _y + 1, num5, Chunk.LIGHT_TYPE.BLOCK), (int)this.chunksCache[num9].GetLight(num7, _y + 1, num8, Chunk.LIGHT_TYPE.BLOCK), (int)this.chunksCache[num12].GetLight(num10, _y + 1, num11, Chunk.LIGHT_TYPE.BLOCK))) : 15);
		_sun = Utils.FastMax(b, b2);
		_block = Utils.FastMax(b3, b4);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetDeckIndex(int x, int y, int z)
	{
		return x * 16 * 4 + y * 4 + z;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void build(Vector3 _drawPosOffset, Vector3i _start, Vector3i _end, VoxelMesh _mesh, Vector3i _worldPos)
	{
		int x = _start.x;
		int z = _start.z;
		this.sizeX = _end.x - _start.x + 1 + 3;
		this.sizeZ = _end.z - _start.z + 1 + 3;
		this.startY = _start.y;
		for (int i = 0; i < this.sizeX; i++)
		{
			for (int j = 0; j < this.sizeZ; j++)
			{
				this.terrainHeightsCache[i + j * this.sizeX] = this.GetTerrainHeight(i - 1 + x, j - 1 + z);
			}
		}
		for (int k = 0; k < this.sizeX; k++)
		{
			for (int l = 0; l < this.sizeZ; l++)
			{
				this.topSoilCache[k + l * this.sizeX] = this.isTopSoil(k - 1 + x, l - 1 + z);
			}
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		ushort[] array = this.deck1;
		ushort[] array2 = this.deck2;
		for (int m = _start.z; m <= _end.z; m++)
		{
			for (int n = _start.y; n <= _end.y; n++)
			{
				int num4 = n - _start.y;
				for (int num5 = _start.x; num5 <= _end.x; num5++)
				{
					byte b = 0;
					this.blockValues[0] = this.getBlockValue(num5, n, m);
					this.blockValues[1] = this.getBlockValue(num5 + 1, n, m);
					this.blockValues[2] = this.getBlockValue(num5, n + 1, m);
					this.blockValues[3] = this.getBlockValue(num5 + 1, n + 1, m);
					this.blockValues[4] = this.getBlockValue(num5, n, m + 1);
					this.blockValues[5] = this.getBlockValue(num5 + 1, n, m + 1);
					this.blockValues[6] = this.getBlockValue(num5, n + 1, m + 1);
					this.blockValues[7] = this.getBlockValue(num5 + 1, n + 1, m + 1);
					this.density[0] = this.getDensity(num5, n, m, 0);
					this.density[1] = this.getDensity(num5 + 1, n, m, 1);
					this.density[2] = this.getDensity(num5, n + 1, m, 2);
					this.density[3] = this.getDensity(num5 + 1, n + 1, m, 3);
					this.density[4] = this.getDensity(num5, n, m + 1, 4);
					this.density[5] = this.getDensity(num5 + 1, n, m + 1, 5);
					this.density[6] = this.getDensity(num5, n + 1, m + 1, 6);
					this.density[7] = this.getDensity(num5 + 1, n + 1, m + 1, 7);
					this.bTopSoil[0] = this.calcTopSoil(num5, n, m);
					this.bTopSoil[1] = this.calcTopSoil(num5 + 1, n, m);
					this.bTopSoil[2] = this.calcTopSoil(num5, n + 1, m);
					this.bTopSoil[3] = this.calcTopSoil(num5 + 1, n + 1, m);
					this.bTopSoil[4] = this.calcTopSoil(num5, n, m + 1);
					this.bTopSoil[5] = this.calcTopSoil(num5 + 1, n, m + 1);
					this.bTopSoil[6] = this.calcTopSoil(num5, n + 1, m + 1);
					this.bTopSoil[7] = this.calcTopSoil(num5 + 1, n + 1, m + 1);
					sbyte b2 = this.density[7];
					int num6 = ((this.density[0] >> 7) & 1) | ((this.density[1] >> 6) & 2) | ((this.density[2] >> 5) & 4) | ((this.density[3] >> 4) & 8) | ((this.density[4] >> 3) & 16) | ((this.density[5] >> 2) & 32) | ((this.density[6] >> 1) & 64) | ((int)b2 & 128);
					if (b2 == 0)
					{
						int num7 = num++;
						array[this.GetDeckIndex(num4, num5, 0)] = (ushort)num7;
						this.vertexStorage[num7].position0 = new Vector3i(num5 + 1 << 8, num4 + 1 << 8, m + 1 << 8);
						this.vertexStorage[num7].normal = this.CalculateNormal(new Vector3i(num5 + 1, num4 + 1, m + 1));
						this.vertexStorage[num7].material = b;
						this.vertexStorage[num7].texture = this.GetTextureFor(num5, n, m, 7);
						this.vertexStorage[num7].bTopSoil = this.bTopSoil[7];
					}
					if ((num6 ^ ((b2 >> 7) & 255)) != 0)
					{
						byte b3 = Transvoxel.regularCellClass[num6];
						Transvoxel.RegularCellData regularCellData = Transvoxel.regularCellData[0, (int)b3];
						int triangleCount = regularCellData.GetTriangleCount();
						if (num2 + triangleCount > 8192)
						{
							goto IL_0A2B;
						}
						int vertexCount = regularCellData.GetVertexCount();
						Transvoxel.RegularVertexData.Row row = Transvoxel.regularVertexData[num6];
						int num8 = 0;
						while (num8 < vertexCount)
						{
							ushort num9 = row.data[num8];
							int num10 = (num9 >> 4) & 15;
							int num11 = (int)(num9 & 15);
							int num12 = (int)this.density[num10];
							int num13 = (int)this.density[num11];
							int num14 = ((num13 << 8) + 128) / (num13 - num12);
							Vector3i vector3i = new Vector3i(num5 + (num10 & 1), num4 + ((num10 >> 1) & 1), m + ((num10 >> 2) & 1));
							Vector3i vector3i2 = new Vector3i(num5 + (num11 & 1), num4 + ((num11 >> 1) & 1), m + ((num11 >> 2) & 1));
							int num17;
							if ((num14 & 255) != 0)
							{
								int num15 = (num9 >> 8) & 15;
								int num16 = (num9 >> 12) & 15;
								if ((num16 & num3) == num16)
								{
									if ((num16 & 4) != 0)
									{
										int deckIndex = this.GetDeckIndex(num4 - ((num16 >> 1) & 1), num5 - (num16 & 1), num15);
										num17 = (int)array2[deckIndex];
										goto IL_0927;
									}
									int deckIndex2 = this.GetDeckIndex(num4 - (num16 >> 1), num5 - (num16 & 1), num15);
									num17 = (int)array[deckIndex2];
									goto IL_0927;
								}
								else
								{
									num17 = num++;
									int num18 = 256 - num14;
									this.vertexStorage[num17].position0 = vector3i * num14 + vector3i2 * num18;
									this.vertexStorage[num17].normal = this.CalculateNormal(vector3i, vector3i2, num14, num18, num12 < num13);
									this.vertexStorage[num17].material = b;
									int num19 = ((num12 < num13) ? num10 : num11);
									this.vertexStorage[num17].texture = this.GetTextureFor(num5, n, m, num19);
									this.vertexStorage[num17].bTopSoil = this.bTopSoil[num19];
									if (num11 == 7)
									{
										int deckIndex3 = this.GetDeckIndex(num4, num5, num15);
										array[deckIndex3] = (ushort)num17;
									}
									this.globalVertexIndex[num8] = num17;
								}
							}
							else if (num14 == 0)
							{
								if (num11 == 7)
								{
									num17 = (int)array[this.GetDeckIndex(num4, num5, 0)];
									goto IL_0927;
								}
								int num20 = num11 ^ 7;
								if ((num20 & num3) == num20)
								{
									if ((num20 & 4) != 0)
									{
										num17 = (int)array2[this.GetDeckIndex(num4 - ((num20 >> 1) & 1), num5 - (num20 & 1), 0)];
										goto IL_0927;
									}
									num17 = (int)array[this.GetDeckIndex(num4 - (num20 >> 1), num5 - (num20 & 1), 0)];
									goto IL_0927;
								}
								else
								{
									num17 = num++;
									this.vertexStorage[num17].position0 = new Vector3i(vector3i2.x << 8, vector3i2.y << 8, vector3i2.z << 8);
									this.vertexStorage[num17].normal = this.CalculateNormal(vector3i2);
									this.vertexStorage[num17].material = b;
									this.vertexStorage[num17].texture = this.GetTextureFor(num5, n, m, num11);
									this.vertexStorage[num17].bTopSoil = this.bTopSoil[num11];
									this.globalVertexIndex[num8] = num17;
								}
							}
							else
							{
								int num21 = num10 ^ 7;
								if ((num21 & num3) == num21)
								{
									if ((num21 & 4) != 0)
									{
										num17 = (int)array2[this.GetDeckIndex(num4 - ((num21 >> 1) & 1), num5 - (num21 & 1), 0)];
										goto IL_0927;
									}
									num17 = (int)array[this.GetDeckIndex(num4 - (num21 >> 1), num5 - (num21 & 1), 0)];
									goto IL_0927;
								}
								else
								{
									num17 = num++;
									this.vertexStorage[num17].position0 = new Vector3i(vector3i.x << 8, vector3i.y << 8, vector3i.z << 8);
									this.vertexStorage[num17].normal = this.CalculateNormal(vector3i);
									this.vertexStorage[num17].material = b;
									this.vertexStorage[num17].texture = this.GetTextureFor(num5, n, m, num10);
									this.vertexStorage[num17].bTopSoil = this.bTopSoil[num10];
									this.globalVertexIndex[num8] = num17;
								}
							}
							IL_0932:
							num8++;
							continue;
							IL_0927:
							this.globalVertexIndex[num8] = num17;
							goto IL_0932;
						}
						if (b != 255)
						{
							byte[] vertexIndex = regularCellData.vertexIndex;
							int num22 = 0;
							for (int num23 = 0; num23 < triangleCount; num23++)
							{
								int num24 = num2 + num23;
								this.triangleStorage[num24].index0 = this.globalVertexIndex[(int)vertexIndex[num22]];
								this.triangleStorage[num24].index1 = this.globalVertexIndex[(int)vertexIndex[1 + num22]];
								this.triangleStorage[num24].index2 = this.globalVertexIndex[(int)vertexIndex[2 + num22]];
								num22 += 3;
							}
							num2 += triangleCount;
						}
					}
					num3 |= 1;
				}
				num3 = (num3 | 2) & 6;
			}
			num3 = 4;
			ushort[] array3 = array;
			array = array2;
			array2 = array3;
		}
		IL_0A2B:
		int num25 = 0;
		int num26 = 0;
		while (num26 < num)
		{
			if (this.vertexStorage[num26].material == 255)
			{
				goto IL_0B10;
			}
			Vector3i position = this.vertexStorage[num26].position0;
			if ((position.x | position.y | position.z) < 0 || Utils.FastMax(Utils.FastMax(position.x, position.y), position.z) > 4096)
			{
				goto IL_0B10;
			}
			int num27 = 1;
			num27 |= (position.x >> 11) & 2;
			num27 |= (position.y >> 10) & 4;
			num27 |= (position.z >> 9) & 8;
			this.vertexStorage[num26].statusFlags = (byte)num27;
			this.vertexStorage[num26].remapIndex = num25;
			num25++;
			IL_0B23:
			num26++;
			continue;
			IL_0B10:
			this.vertexStorage[num26].statusFlags = 0;
			goto IL_0B23;
		}
		int num28 = 0;
		for (int num29 = 0; num29 < num2; num29++)
		{
			int index = this.triangleStorage[num29].index0;
			int index2 = this.triangleStorage[num29].index1;
			int index3 = this.triangleStorage[num29].index2;
			Vector3i vector3i3 = Vector3i.Cross(this.vertexStorage[index2].position0 - this.vertexStorage[index].position0, this.vertexStorage[index3].position0 - this.vertexStorage[index].position0);
			bool flag = false;
			if ((vector3i3.x | vector3i3.y | vector3i3.z) != 0)
			{
				int num30 = ((vector3i3.x >> 31) & 2) | ((vector3i3.y >> 31) & 4) | ((vector3i3.z >> 31) & 8) | 1;
				flag = ((int)(this.vertexStorage[index].statusFlags & this.vertexStorage[index2].statusFlags & this.vertexStorage[index3].statusFlags) & num30) == 1;
			}
			this.triangleStorage[num29].inclusionFlag = flag;
			if (flag)
			{
				num28++;
				int num31 = _mesh.FindOrCreateSubMesh(this.vertexStorage[index].texture, this.vertexStorage[index2].texture, this.vertexStorage[index3].texture);
				this.triangleStorage[num29].submeshIdx = num31;
				_mesh.GetColorForTextureId(num31, ref this.vertexStorage[index]);
				_mesh.GetColorForTextureId(num31, ref this.vertexStorage[index2]);
				_mesh.GetColorForTextureId(num31, ref this.vertexStorage[index3]);
			}
		}
		if (num28 != num2)
		{
			Log.Warning("MG build tris {0} != {1}", new object[] { num28, num2 });
		}
		int count = _mesh.m_Vertices.Count;
		Vector3 vector = new Vector3(0f, (float)_start.y, 0f) + _drawPosOffset;
		for (int num32 = 0; num32 < num; num32++)
		{
			if ((this.vertexStorage[num32].statusFlags & 1) != 0)
			{
				_mesh.m_Vertices.Add(this.vertexStorage[num32].position0.ToVector3() / 256f + vector);
				_mesh.m_Normals.Add(this.vertexStorage[num32].normal);
				_mesh.m_ColorVertices.Add(this.vertexStorage[num32].color);
				_mesh.m_Uvs.Add(this.vertexStorage[num32].uv);
				_mesh.UvsCrack.Add(this.vertexStorage[num32].uv2);
				_mesh.m_Uvs3.Add(this.vertexStorage[num32].uv3);
				_mesh.m_Uvs4.Add(this.vertexStorage[num32].uv4);
			}
		}
		for (int num33 = 0; num33 < num2; num33++)
		{
			if (this.triangleStorage[num33].inclusionFlag)
			{
				int index4 = this.triangleStorage[num33].index0;
				int index5 = this.triangleStorage[num33].index1;
				int index6 = this.triangleStorage[num33].index2;
				_mesh.AddIndices(count + this.vertexStorage[index4].remapIndex, count + this.vertexStorage[index5].remapIndex, count + this.vertexStorage[index6].remapIndex, this.triangleStorage[num33].submeshIdx);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalculateSecondaryPosition(int level, int buildVertex)
	{
		int num = 256 << level;
		int num2 = 15 << 8 + level;
		int num3 = num >> 2;
		Vector3 normal = this.vertexStorage[buildVertex].normal;
		int x = this.vertexStorage[buildVertex].position0.x;
		Vector3i zero;
		if (x < num)
		{
			int num4 = num3 * (num - x) >> 8 + level;
			Vector3 vector;
			vector..ctor(1f - normal.x * normal.x, -normal.x * normal.y, -normal.x * normal.z);
			zero.x = (int)(vector.x * 256f) * num4;
			zero.y = (int)(vector.y * 256f) * num4;
			zero.z = (int)(vector.z * 256f) * num4;
		}
		else if (x > num2)
		{
			int num5 = num3 * (num2 + num - 256 - x) >> 8;
			Vector3 vector2;
			vector2..ctor(1f - normal.x * normal.x, -normal.x * normal.y, -normal.x * normal.z);
			zero.x = (int)(vector2.x * 256f) * num5;
			zero.y = (int)(vector2.y * 256f) * num5;
			zero.z = (int)(vector2.z * 256f) * num5;
		}
		else
		{
			zero = Vector3i.zero;
		}
		int y = this.vertexStorage[buildVertex].position0.y;
		if (y < num)
		{
			int num6 = num3 * (num - y) >> 8 + level;
			Vector3 vector3;
			vector3..ctor(-normal.x * normal.y, 1f - normal.y * normal.y, -normal.y * normal.z);
			zero.x += (int)(vector3.x * 256f) * num6;
			zero.y += (int)(vector3.y * 256f) * num6;
			zero.z += (int)(vector3.z * 256f) * num6;
		}
		else if (y > num2)
		{
			int num7 = num3 * (num2 + num - 256 - y) >> 8;
			Vector3 vector4;
			vector4..ctor(-normal.x * normal.y, 1f - normal.y * normal.y, -normal.y * normal.z);
			zero.x += (int)(vector4.x * 256f) * num7;
			zero.y += (int)(vector4.y * 256f) * num7;
			zero.z += (int)(vector4.z * 256f) * num7;
		}
		int z = this.vertexStorage[buildVertex].position0.z;
		if (z < num)
		{
			int num8 = num3 * (num - z) >> 8 + level;
			Vector3 vector5;
			vector5..ctor(-normal.x * normal.z, -normal.y * normal.z, 1f - normal.z * normal.z);
			zero.x += (int)(vector5.x * 256f) * num8;
			zero.y += (int)(vector5.y * 256f) * num8;
			zero.z += (int)(vector5.z * 256f) * num8;
			return;
		}
		if (z > num2)
		{
			int num9 = num3 * (num2 + num - 256 - z) >> 8;
			Vector3 vector6;
			vector6..ctor(-normal.x * normal.z, -normal.y * normal.z, 1f - normal.z * normal.z);
			zero.x += (int)(vector6.x * 256f) * num9;
			zero.y += (int)(vector6.y * 256f) * num9;
			zero.z += (int)(vector6.z * 256f) * num9;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 CalculateNormal(Vector3i coord)
	{
		int x = coord.x;
		int num = coord.y + this.startY;
		int z = coord.z;
		int num2 = x - 1;
		int num3 = x + 1;
		int num4 = num - 1;
		int num5 = num + 1;
		int num6 = z - 1;
		int num7 = z + 1;
		int num8 = (int)this.getDensity(num2, num, z, -1);
		int num9 = (int)this.getDensity(num3, num, z, -1);
		int num10 = (int)this.getDensity(x, num4, z, -1);
		int num11 = (int)this.getDensity(x, num5, z, -1);
		int num12 = (int)this.getDensity(x, num, num6, -1);
		int num13 = (int)this.getDensity(x, num, num7, -1);
		Vector3 vector;
		vector.x = (float)(num9 - num8);
		vector.y = (float)(num11 - num10);
		vector.z = (float)(num13 - num12);
		float magnitude = vector.magnitude;
		if (magnitude > 1E-05f)
		{
			vector *= 1f / magnitude;
		}
		return vector;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 CalculateNormalUnscaled(Vector3i coord)
	{
		int x = coord.x;
		int num = coord.y + this.startY;
		int z = coord.z;
		int num2 = x - 1;
		int num3 = x + 1;
		int num4 = num - 1;
		int num5 = num + 1;
		int num6 = z - 1;
		int num7 = z + 1;
		int num8 = (int)this.getDensity(num2, num, z, -1);
		int num9 = (int)this.getDensity(num3, num, z, -1);
		int num10 = (int)this.getDensity(x, num4, z, -1);
		int num11 = (int)this.getDensity(x, num5, z, -1);
		int num12 = (int)this.getDensity(x, num, num6, -1);
		int num13 = (int)this.getDensity(x, num, num7, -1);
		Vector3 vector;
		vector.x = (float)(num9 - num8);
		vector.y = (float)(num11 - num10);
		vector.z = (float)(num13 - num12);
		return vector;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 CalculateNormal(Vector3i coord0, Vector3i coord1, int t, int u, bool is0Main)
	{
		Vector3 vector = this.CalculateNormalUnscaled(coord0);
		Vector3 vector2 = this.CalculateNormalUnscaled(coord1);
		Vector3 vector3 = vector * (float)t + vector2 * (float)u;
		Vector3 vector4 = (is0Main ? (coord1 - coord0) : (coord0 - coord1)).ToVector3();
		float num = vector3.magnitude;
		if (num > 1E-05f)
		{
			vector3 *= 1f / num;
			float num2 = Vector3.Dot(vector3, vector4);
			if (num2 < 0.2f)
			{
				if (num2 < -0.5f)
				{
					vector3 = vector4;
				}
				else
				{
					float num3 = 0.8f + Utils.FastAbs((float)(t - u)) * 0.00078124995f;
					vector3 = Vector3.LerpUnclamped(vector4, vector3, num3);
					num = vector3.magnitude;
					vector3 *= 1f / num;
				}
			}
		}
		else
		{
			vector3 = vector4;
		}
		return vector3;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FindSurfaceCrossingEdge(int level, Vector3i coord0, Vector3i coord1, ref int d0, ref int d1)
	{
		Vector3i vector3i = coord0 + coord1;
		vector3i.x >>= 1;
		vector3i.y >>= 1;
		vector3i.z >>= 1;
		int num = (int)this.getDensity(vector3i.x, vector3i.y, vector3i.z, -1);
		if (((d0 ^ num) & 128) != 0)
		{
			coord1 = vector3i;
			d1 = num;
		}
		else
		{
			coord0 = vector3i;
			d0 = num;
		}
		if (level > 1)
		{
			this.FindSurfaceCrossingEdge(level - 1, coord0, coord1, ref d0, ref d1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ChooseTriangulation(int cellClass, int x, int y, int z, int dv, ushort[] _globalVertexIndex)
	{
		dv >>= 1;
		x += dv;
		y += dv;
		z += dv;
		float num = (float)this.getDensity(x, y, z, -1) * 0.007874016f;
		Vector3i vector3i = new Vector3i(x << 8, y << 8, z << 8);
		Transvoxel.InternalEdgeData internalEdgeData = Transvoxel.regularInternalEdgeData[0, cellClass];
		int num2 = (int)internalEdgeData.edgeCount;
		float num3 = 2f;
		for (int i = 0; i < num2; i++)
		{
			Vector3i vector3i2 = this.vertexStorage[(int)_globalVertexIndex[(int)internalEdgeData.vertexIndex[i, 0]]].position0 - vector3i;
			Vector3i vector3i3 = this.vertexStorage[(int)_globalVertexIndex[(int)internalEdgeData.vertexIndex[i, 1]]].position0 - vector3i;
			Vector3i vector3i4 = this.vertexStorage[(int)_globalVertexIndex[(int)internalEdgeData.vertexIndex[i, 2]]].position0 - vector3i;
			Vector3i vector3i5 = this.vertexStorage[(int)_globalVertexIndex[(int)internalEdgeData.vertexIndex[i, 3]]].position0 - vector3i;
			Vector3 vector;
			vector..ctor((float)vector3i2.x * 0.00390625f, (float)vector3i2.y * 0.00390625f, (float)vector3i2.z * 0.00390625f);
			Vector3 vector2 = new Vector3((float)vector3i3.x * 0.00390625f, (float)vector3i3.y * 0.00390625f, (float)vector3i3.z * 0.00390625f);
			Vector3 vector3;
			vector3..ctor((float)vector3i4.x * 0.00390625f, (float)vector3i4.y * 0.00390625f, (float)vector3i4.z * 0.00390625f);
			Vector3 vector4;
			vector4..ctor((float)vector3i5.x * 0.00390625f, (float)vector3i5.y * 0.00390625f, (float)vector3i5.z * 0.00390625f);
			Vector3 vector5 = vector2 - vector;
			float num4 = Vector3.Dot(vector, vector5);
			float num5 = Mathf.Sqrt(Vector3.Dot(vector, vector) - num4 * num4 / Vector3.Dot(vector5, vector5));
			if (Vector3.Dot(Vector3.Cross(vector5, vector4 - vector3), vector) > 0f)
			{
				num5 = -num5;
			}
			num5 = Mathf.Abs(num5 - num);
			if (num5 < num3)
			{
				num3 = num5;
			}
		}
		internalEdgeData = Transvoxel.regularInternalEdgeData[1, cellClass];
		num2 = (int)internalEdgeData.edgeCount;
		float num6 = 2f;
		for (int j = 0; j < num2; j++)
		{
			Vector3i vector3i6 = this.vertexStorage[(int)_globalVertexIndex[(int)internalEdgeData.vertexIndex[j, 0]]].position0 - vector3i;
			Vector3i vector3i7 = this.vertexStorage[(int)_globalVertexIndex[(int)internalEdgeData.vertexIndex[j, 1]]].position0 - vector3i;
			Vector3i vector3i8 = this.vertexStorage[(int)_globalVertexIndex[(int)internalEdgeData.vertexIndex[j, 2]]].position0 - vector3i;
			Vector3i vector3i9 = this.vertexStorage[(int)_globalVertexIndex[(int)internalEdgeData.vertexIndex[j, 3]]].position0 - vector3i;
			Vector3 vector6;
			vector6..ctor((float)vector3i6.x * 0.00390625f, (float)vector3i6.y * 0.00390625f, (float)vector3i6.z * 0.00390625f);
			Vector3 vector7 = new Vector3((float)vector3i7.x * 0.00390625f, (float)vector3i7.y * 0.00390625f, (float)vector3i7.z * 0.00390625f);
			Vector3 vector8;
			vector8..ctor((float)vector3i8.x * 0.00390625f, (float)vector3i8.y * 0.00390625f, (float)vector3i8.z * 0.00390625f);
			Vector3 vector9;
			vector9..ctor((float)vector3i9.x * 0.00390625f, (float)vector3i9.y * 0.00390625f, (float)vector3i9.z * 0.00390625f);
			Vector3 vector10 = vector7 - vector6;
			float num7 = Vector3.Dot(vector6, vector10);
			float num8 = Mathf.Sqrt(Vector3.Dot(vector6, vector6) - num7 * num7 / Vector3.Dot(vector10, vector10));
			if (Vector3.Dot(Vector3.Cross(vector10, vector9 - vector8), vector6) > 0f)
			{
				num8 = -num8;
			}
			num8 = Mathf.Abs(num8 - num);
			if (num8 < num6)
			{
				num6 = num8;
			}
		}
		Transvoxel.RegularCellData regularCellData = Transvoxel.regularCellData[0, cellClass];
		num2 = regularCellData.GetTriangleCount();
		int num9 = 0;
		for (int k = 0; k < num2; k++)
		{
			Vector3i vector3i10 = this.vertexStorage[(int)_globalVertexIndex[(int)regularCellData.vertexIndex[num9]]].position0 - vector3i;
			Vector3i vector3i11 = this.vertexStorage[(int)_globalVertexIndex[(int)regularCellData.vertexIndex[1 + num9]]].position0 - vector3i;
			Vector3i vector3i12 = this.vertexStorage[(int)_globalVertexIndex[(int)regularCellData.vertexIndex[2 + num9]]].position0 - vector3i;
			Vector3 vector11;
			vector11..ctor((float)vector3i10.x * 0.00390625f, (float)vector3i10.y * 0.00390625f, (float)vector3i10.z * 0.00390625f);
			Vector3 vector12;
			vector12..ctor((float)vector3i11.x * 0.00390625f, (float)vector3i11.y * 0.00390625f, (float)vector3i11.z * 0.00390625f);
			Vector3 vector13;
			vector13..ctor((float)vector3i12.x * 0.00390625f, (float)vector3i12.y * 0.00390625f, (float)vector3i12.z * 0.00390625f);
			Vector3 vector14 = vector12 - vector11;
			Vector3 vector15 = vector13 - vector11;
			Vector3 vector16 = Vector3.Cross(vector14, vector15);
			if (Vector3.Dot(Vector3.Cross(vector16, vector14), vector11) < 0f && Vector3.Dot(Vector3.Cross(vector16, vector13 - vector12), vector12) < 0f && Vector3.Dot(Vector3.Cross(vector15, vector16), vector13) < 0f)
			{
				float num10 = -Vector3.Dot(vector16, vector11);
				num10 = Mathf.Abs(num10 - num);
				if (num10 < num3)
				{
					num3 = num10;
				}
			}
			num9 += 3;
		}
		regularCellData = Transvoxel.regularCellData[1, cellClass];
		num9 = 0;
		num2 = regularCellData.GetTriangleCount();
		for (int l = 0; l < num2; l++)
		{
			Vector3i vector3i13 = this.vertexStorage[(int)_globalVertexIndex[(int)regularCellData.vertexIndex[num9]]].position0 - vector3i;
			Vector3i vector3i14 = this.vertexStorage[(int)_globalVertexIndex[(int)regularCellData.vertexIndex[1 + num9]]].position0 - vector3i;
			Vector3i vector3i15 = this.vertexStorage[(int)_globalVertexIndex[(int)regularCellData.vertexIndex[2 + num9]]].position0 - vector3i;
			Vector3 vector17;
			vector17..ctor((float)vector3i13.x * 0.00390625f, (float)vector3i13.y * 0.00390625f, (float)vector3i13.z * 0.00390625f);
			Vector3 vector18;
			vector18..ctor((float)vector3i14.x * 0.00390625f, (float)vector3i14.y * 0.00390625f, (float)vector3i14.z * 0.00390625f);
			Vector3 vector19;
			vector19..ctor((float)vector3i15.x * 0.00390625f, (float)vector3i15.y * 0.00390625f, (float)vector3i15.z * 0.00390625f);
			Vector3 vector20 = vector18 - vector17;
			Vector3 vector21 = vector19 - vector17;
			Vector3 vector22 = Vector3.Cross(vector20, vector21);
			if (Vector3.Dot(Vector3.Cross(vector22, vector20), vector17) < 0f && Vector3.Dot(Vector3.Cross(vector22, vector19 - vector18), vector18) < 0f && Vector3.Dot(Vector3.Cross(vector21, vector22), vector19) < 0f)
			{
				float num11 = -Vector3.Dot(vector22, vector17);
				num11 = Mathf.Abs(num11 - num);
				if (num11 < num6)
				{
					num6 = num11;
				}
			}
			num9 += 3;
		}
		return num6 < num3;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildMipBorder(int detailLevel, int face, ref int globalVertexCount, ref int globalTriangleCount, VoxelMesh _mesh)
	{
		byte b = 0;
		ushort num = 0;
		int num2 = 1 << detailLevel - 1;
		int num3 = (int)MeshGeneratorMC2.voxelDelta[face, 0] * num2;
		int num4 = (int)MeshGeneratorMC2.voxelDelta[face, 1] * num2;
		int num5 = (int)MeshGeneratorMC2.voxelDelta[face, 2] * num2;
		int num6 = (int)MeshGeneratorMC2.voxelDelta[face, 3] * num2;
		for (int i = 0; i < 16; i++)
		{
			int num7 = (int)MeshGeneratorMC2.voxelStart[face, 0] * num2;
			int num8 = (int)MeshGeneratorMC2.voxelStart[face, 1] * num2 + i * num5 * 2;
			int num9 = (int)MeshGeneratorMC2.voxelStart[face, 2] * num2 + i * num6 * 2;
			ushort[,] array = this.rowStorage[(int)b];
			ushort[,] array2 = this.rowStorage[(int)(b ^ 1)];
			for (int j = 0; j < 16; j++)
			{
				this.mipmapCoord[0] = new Vector3i(num7, num8, num9);
				this.mipmapCoord[1] = new Vector3i(num7 + num3, num8 + num4, num9);
				this.mipmapCoord[2] = new Vector3i(num7 + num3 * 2, num8 + num4 * 2, num9);
				this.mipmapCoord[3] = new Vector3i(num7, num8 + num5, num9 + num6);
				this.mipmapCoord[4] = new Vector3i(num7 + num3, num8 + num4 + num5, num9 + num6);
				this.mipmapCoord[5] = new Vector3i(num7 + num3 * 2, num8 + num4 * 2 + num5, num9 + num6);
				this.mipmapCoord[6] = new Vector3i(num7, num8 + num5 * 2, num9 + num6 * 2);
				this.mipmapCoord[7] = new Vector3i(num7 + num3, num8 + num4 + num5 * 2, num9 + num6 * 2);
				this.mipmapCoord[8] = new Vector3i(num7 + num3 * 2, num8 + num4 * 2 + num5 * 2, num9 + num6 * 2);
				for (int k = 0; k < 9; k++)
				{
					this.mipmapDensity[k] = this.getDensity(this.mipmapCoord[k].x, this.mipmapCoord[k].y, this.mipmapCoord[k].z, -1);
				}
				byte b2 = 0;
				this.mipmapCoord[9] = this.mipmapCoord[0];
				this.mipmapCoord[10] = this.mipmapCoord[2];
				this.mipmapCoord[11] = this.mipmapCoord[6];
				this.mipmapCoord[12] = this.mipmapCoord[8];
				this.mipmapDensity[9] = this.mipmapDensity[0];
				this.mipmapDensity[10] = this.mipmapDensity[2];
				this.mipmapDensity[11] = this.mipmapDensity[6];
				this.mipmapDensity[12] = this.mipmapDensity[8];
				if (this.mipmapDensity[8] == 0)
				{
					int num10 = globalVertexCount;
					globalVertexCount = num10 + 1;
					ushort num11 = (ushort)num10;
					array[j, 0] = num11;
					this.vertexStorage[(int)num11].position0 = new Vector3i(this.mipmapCoord[8].x << 8, this.mipmapCoord[8].y << 8, this.mipmapCoord[8].z << 8);
					this.vertexStorage[(int)num11].normal = this.CalculateNormal(this.mipmapCoord[8]);
				}
				if (this.mipmapDensity[7] == 0)
				{
					int num10 = globalVertexCount;
					globalVertexCount = num10 + 1;
					ushort num12 = (ushort)num10;
					array[j, 1] = num12;
					this.vertexStorage[(int)num12].position0 = new Vector3i(this.mipmapCoord[7].x << 8, this.mipmapCoord[7].y << 8, this.mipmapCoord[7].z << 8);
					this.vertexStorage[(int)num12].normal = this.CalculateNormal(this.mipmapCoord[7]);
				}
				if (this.mipmapDensity[5] == 0)
				{
					int num10 = globalVertexCount;
					globalVertexCount = num10 + 1;
					ushort num13 = (ushort)num10;
					array[j, 2] = num13;
					this.vertexStorage[(int)num13].position0 = new Vector3i(this.mipmapCoord[5].x << 8, this.mipmapCoord[5].y << 8, this.mipmapCoord[5].z << 8);
					this.vertexStorage[(int)num13].normal = this.CalculateNormal(this.mipmapCoord[5]);
				}
				if (this.mipmapDensity[12] == 0)
				{
					int num10 = globalVertexCount;
					globalVertexCount = num10 + 1;
					ushort num14 = (ushort)num10;
					array[j, 7] = num14;
					this.vertexStorage[(int)num14].position0 = new Vector3i(this.mipmapCoord[12].x << 8, this.mipmapCoord[12].y << 8, this.mipmapCoord[12].z << 8);
					this.vertexStorage[(int)num14].normal = this.CalculateNormal(this.mipmapCoord[12]);
					this.CalculateSecondaryPosition(detailLevel, (int)num14);
				}
				int num15 = ((this.mipmapDensity[0] >> 7) & 1) | ((this.mipmapDensity[1] >> 6) & 2) | ((this.mipmapDensity[2] >> 5) & 4) | ((this.mipmapDensity[5] >> 4) & 8) | ((this.mipmapDensity[8] >> 3) & 16) | ((this.mipmapDensity[7] >> 2) & 32) | ((this.mipmapDensity[6] >> 1) & 64) | ((int)this.mipmapDensity[3] & 128) | (((int)this.mipmapDensity[4] << 1) & 256);
				if ((num15 ^ ((this.mipmapDensity[8] >> 7) & 511)) != 0)
				{
					byte b3 = Transvoxel.transitionCellClass[num15];
					Transvoxel.TransitionCellData transitionCellData = Transvoxel.transitionCellData[(int)(b3 & 127)];
					int triangleCount = transitionCellData.GetTriangleCount();
					if (globalTriangleCount + triangleCount > 8192)
					{
						return;
					}
					int vertexCount = transitionCellData.GetVertexCount();
					Transvoxel.TransitionVertexData.Row row = Transvoxel.transitionVertexData[num15];
					int l = 0;
					while (l < vertexCount)
					{
						ushort num16 = row[l];
						ushort num17 = (ushort)((num16 >> 4) & 15);
						ushort num18 = num16 & 15;
						int num19 = (int)this.mipmapDensity[(int)num17];
						int num20 = (int)this.mipmapDensity[(int)num18];
						int num21 = (num20 << 8) / (num20 - num19);
						ushort num24;
						if ((num21 & 255) != 0)
						{
							ushort num22 = (ushort)((num16 >> 8) & 15);
							ushort num23 = (ushort)((num16 >> 12) & 15);
							if ((num23 & num) == num23)
							{
								if ((num23 & 2) != 0)
								{
									num24 = array2[j - (int)(num23 & 1), (int)num22];
									goto IL_0A5D;
								}
								num24 = array[j - (int)(num23 & 1), (int)num22];
								goto IL_0A5D;
							}
							else
							{
								Vector3i vector3i = this.mipmapCoord[(int)num17];
								Vector3i vector3i2 = this.mipmapCoord[(int)num18];
								if (num22 > 7)
								{
									this.FindSurfaceCrossingEdge(detailLevel, vector3i, vector3i2, ref num19, ref num20);
									num21 = (num20 << 8) / (num20 - num19);
								}
								else if (detailLevel > 1)
								{
									this.FindSurfaceCrossingEdge(detailLevel - 1, vector3i, vector3i2, ref num19, ref num20);
									num21 = (num20 << 8) / (num20 - num19);
								}
								int num10 = globalVertexCount;
								globalVertexCount = num10 + 1;
								num24 = (ushort)num10;
								int num25 = 256 - num21;
								this.vertexStorage[(int)num24].position0 = vector3i * num21 + vector3i2 * num25;
								this.vertexStorage[(int)num24].normal = this.CalculateNormal(vector3i, vector3i2, num21, num25, num19 < num20);
								if (num22 > 7)
								{
									this.CalculateSecondaryPosition(detailLevel, (int)num24);
								}
								if (num23 == 8)
								{
									array[j, (int)num22] = num24;
								}
								this.mipmapGlobalVertexIndex[l] = num24;
							}
						}
						else if (num21 == 0)
						{
							byte b4 = Transvoxel.transitionCornerData[(int)num18];
							ushort num26 = (ushort)(b4 >> 4);
							if (num26 == 8)
							{
								num24 = array[j, (int)(b4 & 15)];
								goto IL_0A5D;
							}
							if ((num26 & num) == num26)
							{
								if ((num26 & 2) != 0)
								{
									num24 = array2[j - (int)(num26 & 1), (int)(b4 & 15)];
									goto IL_0A5D;
								}
								num24 = array[j - (int)(num26 & 1), (int)(b4 & 15)];
								goto IL_0A5D;
							}
							else
							{
								int num10 = globalVertexCount;
								globalVertexCount = num10 + 1;
								num24 = (ushort)num10;
								this.vertexStorage[(int)num24].position0 = new Vector3i(this.mipmapCoord[(int)num18].x << 8, this.mipmapCoord[(int)num18].y << 8, this.mipmapCoord[(int)num18].z << 8);
								if ((ushort)((num16 >> 8) & 15) > 7)
								{
									this.vertexStorage[(int)num24].normal = this.CalculateNormal(this.mipmapCoord[(int)num18]);
									this.CalculateSecondaryPosition(detailLevel, (int)num24);
								}
								else
								{
									this.vertexStorage[(int)num24].normal = this.CalculateNormal(this.mipmapCoord[(int)num18]);
								}
								this.mipmapGlobalVertexIndex[l] = num24;
							}
						}
						else
						{
							byte b5 = Transvoxel.transitionCornerData[(int)num17];
							ushort num27 = (ushort)(b5 >> 4);
							if (num27 == 8)
							{
								num24 = array[j, (int)(b5 & 15)];
								goto IL_0A5D;
							}
							if ((num27 & num) == num27)
							{
								if ((num27 & 2) != 0)
								{
									num24 = array2[j - (int)(num27 & 1), (int)(b5 & 15)];
									goto IL_0A5D;
								}
								num24 = array[j - (int)(num27 & 1), (int)(b5 & 15)];
								goto IL_0A5D;
							}
							else
							{
								int num10 = globalVertexCount;
								globalVertexCount = num10 + 1;
								num24 = (ushort)num10;
								this.vertexStorage[(int)num24].position0 = new Vector3i(this.mipmapCoord[(int)num17].x << 8, this.mipmapCoord[(int)num17].y << 8, this.mipmapCoord[(int)num17].z << 8);
								if ((ushort)((num16 >> 8) & 15) > 7)
								{
									this.vertexStorage[(int)num24].normal = this.CalculateNormal(this.mipmapCoord[(int)num17]);
									this.CalculateSecondaryPosition(detailLevel, (int)num24);
								}
								else
								{
									this.vertexStorage[(int)num24].normal = this.CalculateNormal(this.mipmapCoord[(int)num17]);
								}
								this.mipmapGlobalVertexIndex[l] = num24;
							}
						}
						IL_0A68:
						l++;
						continue;
						IL_0A5D:
						this.mipmapGlobalVertexIndex[l] = num24;
						goto IL_0A68;
					}
					if (b2 != 255)
					{
						int num28 = 0;
						int num29 = 0;
						if (((b3 & 128) ^ MeshGeneratorMC2.faceFlip[face]) != 0)
						{
							for (int m = 0; m < triangleCount; m++)
							{
								this.triangleStorage[globalTriangleCount + num29].index0 = (int)this.mipmapGlobalVertexIndex[(int)transitionCellData.vertexIndex[num28]];
								this.triangleStorage[globalTriangleCount + num29].index1 = (int)this.mipmapGlobalVertexIndex[(int)transitionCellData.vertexIndex[1 + num28]];
								this.triangleStorage[globalTriangleCount + num29].index2 = (int)this.mipmapGlobalVertexIndex[(int)transitionCellData.vertexIndex[2 + num28]];
								num28 += 3;
								num29++;
							}
						}
						else
						{
							for (int n = 0; n < triangleCount; n++)
							{
								this.triangleStorage[globalTriangleCount + num29].index0 = (int)this.mipmapGlobalVertexIndex[(int)transitionCellData.vertexIndex[num28]];
								this.triangleStorage[globalTriangleCount + num29].index1 = (int)this.mipmapGlobalVertexIndex[(int)transitionCellData.vertexIndex[2 + num28]];
								this.triangleStorage[globalTriangleCount + num29].index2 = (int)this.mipmapGlobalVertexIndex[(int)transitionCellData.vertexIndex[1 + num28]];
								num28 += 3;
								num29++;
							}
						}
						globalTriangleCount += triangleCount;
					}
				}
				num |= 1;
				num7 += num3 * 2;
				num8 += num4 * 2;
			}
			num = 2;
			b ^= 1;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkCacheNeighborChunks nChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort[] deck1 = new ushort[1024];

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort[] deck2 = new ushort[1024];

	[PublicizedFrom(EAccessModifier.Private)]
	public Transvoxel.BuildVertex[] vertexStorage = new Transvoxel.BuildVertex[8192];

	[PublicizedFrom(EAccessModifier.Private)]
	public Transvoxel.BuildTriangle[] triangleStorage = new Transvoxel.BuildTriangle[8192];

	[PublicizedFrom(EAccessModifier.Private)]
	public sbyte[] density = new sbyte[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] globalVertexIndex = new int[12];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] allHeights = new int[256];

	[PublicizedFrom(EAccessModifier.Private)]
	public const int chunksCacheDim = 19;

	[PublicizedFrom(EAccessModifier.Private)]
	public IChunk[] chunksCache = new IChunk[361];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3 cVectorAdd = new Vector3(0.5f, 0.5f, 0.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue[] blockValues = new BlockValue[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public bool[] bTopSoil = new bool[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] terrainHeightsCache = new int[1600];

	[PublicizedFrom(EAccessModifier.Private)]
	public bool[] topSoilCache = new bool[1600];

	[PublicizedFrom(EAccessModifier.Private)]
	public int sizeX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sizeZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector2i[] adjLightPos = new Vector2i[]
	{
		Vector2i.zero,
		new Vector2i(1, 0),
		new Vector2i(0, 1),
		new Vector2i(1, 1)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] neighbors = new int[]
	{
		-1, 0, 1, 0, 0, -1, 0, 1, -1, -1,
		1, -1, -1, 1, 1, 1
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public const int voxelBoundsMin = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int voxelBoundsMax = 17;

	[PublicizedFrom(EAccessModifier.Private)]
	public int startY;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[,] voxelDelta = new byte[,]
	{
		{ 0, 1, 0, 1 },
		{ 0, 1, 0, 1 },
		{ 1, 0, 0, 1 },
		{ 1, 0, 0, 1 },
		{ 1, 0, 1, 0 },
		{ 1, 0, 1, 0 }
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[,] voxelStart = new byte[,]
	{
		{ 32, 0, 0 },
		{ 0, 0, 0 },
		{ 0, 32, 0 },
		{ 0, 0, 0 },
		{ 0, 0, 32 },
		{ 0, 0, 0 }
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] faceFlip = new byte[] { 0, 128, 128, 0, 0, 128 };

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i[] mipmapCoord = new Vector3i[13];

	[PublicizedFrom(EAccessModifier.Private)]
	public sbyte[] mipmapDensity = new sbyte[13];

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort[] mipmapGlobalVertexIndex = new ushort[12];

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort[][,] rowStorage = new ushort[][,]
	{
		new ushort[10, 16],
		new ushort[10, 16]
	};
}
