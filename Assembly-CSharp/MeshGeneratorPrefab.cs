using System;
using UnityEngine;

public class MeshGeneratorPrefab : MeshGenerator
{
	public MeshGeneratorPrefab(Prefab _prefab)
		: base(_prefab)
	{
		this.prefab = _prefab;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateMesh(Vector3i _worldPos, Vector3 _drawPosOffset, Vector3i _start, Vector3i _end, VoxelMesh[] _meshes, bool _bCalcAmbientLight, bool _bOnlyDistortVertices)
	{
		base.CreateMesh(_worldPos, _drawPosOffset, _start, _end, _meshes, _bCalcAmbientLight, _bOnlyDistortVertices);
		this.build(MeshGeneratorPrefab.cVectorAdd + _drawPosOffset, new Vector3i(_start.x, _start.y, _start.z), new Vector3i(_end.x, _end.y, _end.z), _meshes[5]);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sbyte getDensity(int _x, int _y, int _z, int _idxInBlockValues = -1)
	{
		_x--;
		_z--;
		if (_y < 0)
		{
			return MarchingCubes.DensityTerrain;
		}
		if (_y >= 256)
		{
			return MarchingCubes.DensityAir;
		}
		if (!this.checkCoordinates(_x, _y, _z))
		{
			return MarchingCubes.DensityAir;
		}
		return this.prefab.GetDensity(_x, _y, _z);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue getBlockValue(int _x, int _y, int _z)
	{
		_x--;
		_z--;
		if (!this.checkCoordinates(_x, _y, _z))
		{
			return BlockValue.Air;
		}
		return this.prefab.GetBlockNoDamage(this.prefab.GetLocalRotation(), _x, _y, _z);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkCoordinates(int _x, int _y, int _z)
	{
		return _x >= 0 && _x < this.prefab.size.x && _y >= 0 && _y < this.prefab.size.y && _z >= 0 && _z < this.prefab.size.z;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTopSoil(int _x, int _z)
	{
		IChunk chunkFromWorldPos = this.prefab.GetChunkFromWorldPos(new Vector3i(_x, 0, _z));
		return chunkFromWorldPos != null && chunkFromWorldPos.IsTopSoil(_x & 15, _z & 15);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetTerrainHeight(int _x, int _z)
	{
		IChunk chunkFromWorldPos = this.prefab.GetChunkFromWorldPos(new Vector3i(_x, 0, _z));
		if (chunkFromWorldPos == null)
		{
			return 0;
		}
		return (int)chunkFromWorldPos.GetTerrainHeight(_x & 15, _z & 15);
	}

	public void GenerateMeshOffset(Vector3i _worldStartPos, Vector3i _worldEndPos, VoxelMesh[] _meshes)
	{
		base.CreateMesh(Vector3i.zero, Vector3.zero, _worldStartPos - new Vector3i(1, 0, 1), _worldEndPos, _meshes, true, false);
		this.build(Vector3.zero, new Vector3i(_worldStartPos.x, _worldStartPos.y, _worldStartPos.z), new Vector3i(_worldEndPos.x, _worldEndPos.y, _worldEndPos.z), _meshes[5]);
		for (int i = 0; i < _meshes.Length; i++)
		{
			_meshes[i].Finished();
		}
	}

	public void GenerateMeshNoTerrain(Vector3i _worldStartPos, Vector3i _worldEndPos, VoxelMesh[] _meshes)
	{
		base.CreateMesh(Vector3i.zero, Vector3.zero, _worldStartPos, _worldEndPos, _meshes, true, false);
		for (int i = 0; i < _meshes.Length; i++)
		{
			_meshes[i].Finished();
		}
	}

	public void GenerateMeshTerrainOnly(Vector3i _worldStartPos, Vector3i _worldEndPos, VoxelMesh[] _meshes)
	{
		this.build(Vector3.zero, new Vector3i(_worldStartPos.x, _worldStartPos.y, _worldStartPos.z), new Vector3i(_worldEndPos.x, _worldEndPos.y, _worldEndPos.z), _meshes[5]);
		for (int i = 0; i < _meshes.Length; i++)
		{
			_meshes[i].Finished();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool calcTopSoil(int _x, int _y, int _z)
	{
		_x++;
		_z++;
		int num = this.GetTerrainHeight(_x, _z);
		if (_y >= num && this.isTopSoil(_x, _z))
		{
			return true;
		}
		num = this.GetTerrainHeight(_x + 1, _z);
		if (_y > num && this.isTopSoil(_x + 1, _z))
		{
			return true;
		}
		num = this.GetTerrainHeight(_x, _z + 1);
		if (_y > num && this.isTopSoil(_x, _z + 1))
		{
			return true;
		}
		num = this.GetTerrainHeight(_x, _z - 1);
		if (_y > num && this.isTopSoil(_x, _z - 1))
		{
			return true;
		}
		num = this.GetTerrainHeight(_x - 1, _z);
		return _y > num && this.isTopSoil(_x - 1, _z);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetTextureFor(int _x, int _y, int _z, int idx)
	{
		BlockValue blockValue = this.blockValues[idx];
		Block block = blockValue.Block;
		if (!block.shape.IsTerrain())
		{
			if (_y > 0)
			{
				_y--;
				blockValue = this.prefab.GetBlock(_x, _y, _z);
			}
			if (!blockValue.Block.shape.IsTerrain())
			{
				blockValue = new BlockValue(1U);
			}
			block = blockValue.Block;
		}
		int sideTextureId = block.GetSideTextureId(blockValue, BlockFace.Top, 0);
		int sideTextureId2 = block.GetSideTextureId(blockValue, BlockFace.South, 0);
		return VoxelMeshTerrain.EncodeTexIds(sideTextureId, sideTextureId2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void calcLights(int _x, int _y, int _z, out byte _sun, out byte _block)
	{
		int num = _x + MeshGeneratorPrefab.adjLightPos[0].x;
		int num2 = _z + MeshGeneratorPrefab.adjLightPos[0].y;
		IChunk chunkFromWorldPos = this.prefab.GetChunkFromWorldPos(new Vector3i(num, _y, num2));
		int num3 = _x + MeshGeneratorPrefab.adjLightPos[1].x;
		int num4 = _z + MeshGeneratorPrefab.adjLightPos[1].y;
		IChunk chunkFromWorldPos2 = this.prefab.GetChunkFromWorldPos(new Vector3i(num3, _y, num4));
		int num5 = _x + MeshGeneratorPrefab.adjLightPos[2].x;
		int num6 = _z + MeshGeneratorPrefab.adjLightPos[2].y;
		IChunk chunkFromWorldPos3 = this.prefab.GetChunkFromWorldPos(new Vector3i(num5, _y, num6));
		int num7 = _x + MeshGeneratorPrefab.adjLightPos[3].x;
		int num8 = _z + MeshGeneratorPrefab.adjLightPos[3].y;
		IChunk chunkFromWorldPos4 = this.prefab.GetChunkFromWorldPos(new Vector3i(num7, _y, num8));
		byte b = (byte)Utils.FastMax((int)chunkFromWorldPos.GetLight(num, _y, num2, Chunk.LIGHT_TYPE.SUN), (int)chunkFromWorldPos2.GetLight(num3, _y, num4, Chunk.LIGHT_TYPE.SUN), (int)chunkFromWorldPos3.GetLight(num5, _y, num6, Chunk.LIGHT_TYPE.SUN), (int)chunkFromWorldPos4.GetLight(num7, _y, num8, Chunk.LIGHT_TYPE.SUN));
		byte b2 = ((_y < 255) ? ((byte)Utils.FastMax((int)chunkFromWorldPos.GetLight(num, _y + 1, num2, Chunk.LIGHT_TYPE.SUN), (int)chunkFromWorldPos2.GetLight(num3, _y + 1, num4, Chunk.LIGHT_TYPE.SUN), (int)chunkFromWorldPos3.GetLight(num5, _y + 1, num6, Chunk.LIGHT_TYPE.SUN), (int)chunkFromWorldPos4.GetLight(num7, _y + 1, num8, Chunk.LIGHT_TYPE.SUN))) : 15);
		byte b3 = (byte)Utils.FastMax((int)chunkFromWorldPos.GetLight(num, _y, num2, Chunk.LIGHT_TYPE.BLOCK), (int)chunkFromWorldPos2.GetLight(num3, _y, num4, Chunk.LIGHT_TYPE.BLOCK), (int)chunkFromWorldPos3.GetLight(num5, _y, num6, Chunk.LIGHT_TYPE.BLOCK), (int)chunkFromWorldPos4.GetLight(num7, _y, num8, Chunk.LIGHT_TYPE.BLOCK));
		byte b4 = ((_y < 255) ? ((byte)Utils.FastMax((int)chunkFromWorldPos.GetLight(num, _y + 1, num2, Chunk.LIGHT_TYPE.BLOCK), (int)chunkFromWorldPos2.GetLight(num3, _y + 1, num4, Chunk.LIGHT_TYPE.BLOCK), (int)chunkFromWorldPos3.GetLight(num5, _y + 1, num6, Chunk.LIGHT_TYPE.BLOCK), (int)chunkFromWorldPos4.GetLight(num7, _y + 1, num8, Chunk.LIGHT_TYPE.BLOCK))) : 15);
		_sun = Utils.FastMax(b, b2);
		_block = Utils.FastMax(b3, b4);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetDeckIndex(int x, int y, int z)
	{
		return x * 16 * 4 + y * 4 + z;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void build(Vector3 _drawPosOffset, Vector3i _start, Vector3i _end, VoxelMesh _mesh)
	{
		if (this.IsLayerEmpty(_start.y / 16, _end.y / 16))
		{
			return;
		}
		bool flag = false;
		for (int i = _start.z; i <= _end.z; i++)
		{
			for (int j = _start.y; j <= _end.y; j++)
			{
				for (int k = _start.x; k <= _end.x; k++)
				{
					if (this.getBlockValue(k, j, i).Block.shape.IsTerrain())
					{
						flag = true;
						goto IL_0090;
					}
				}
			}
		}
		IL_0090:
		if (!flag)
		{
			return;
		}
		this.decks[0] = this.deck1;
		this.decks[1] = this.deck2;
		int num = 0;
		int num2 = 0;
		byte b = 0;
		int num3 = 0;
		for (int l = _start.z; l <= _end.z; l++)
		{
			ushort[,,] array = this.decks[(int)b];
			ushort[,,] array2 = this.decks[(int)(b ^ 1)];
			for (int m = _start.y; m <= _end.y; m++)
			{
				int num4 = m - _start.y;
				for (int n = _start.x; n <= _end.x; n++)
				{
					int num5 = n - _start.x;
					byte b2 = 0;
					this.blockValues[0] = this.getBlockValue(n, m, l);
					this.blockValues[1] = this.getBlockValue(n + 1, m, l);
					this.blockValues[2] = this.getBlockValue(n, m + 1, l);
					this.blockValues[3] = this.getBlockValue(n + 1, m + 1, l);
					this.blockValues[4] = this.getBlockValue(n, m, l + 1);
					this.blockValues[5] = this.getBlockValue(n + 1, m, l + 1);
					this.blockValues[6] = this.getBlockValue(n, m + 1, l + 1);
					this.blockValues[7] = this.getBlockValue(n + 1, m + 1, l + 1);
					this.density[0] = this.getDensity(n, m, l, 0);
					this.density[1] = this.getDensity(n + 1, m, l, 1);
					this.density[2] = this.getDensity(n, m + 1, l, 2);
					this.density[3] = this.getDensity(n + 1, m + 1, l, 3);
					this.density[4] = this.getDensity(n, m, l + 1, 4);
					this.density[5] = this.getDensity(n + 1, m, l + 1, 5);
					this.density[6] = this.getDensity(n, m + 1, l + 1, 6);
					this.density[7] = this.getDensity(n + 1, m + 1, l + 1, 7);
					this.bTopSoil[0] = true;
					this.bTopSoil[1] = true;
					this.bTopSoil[2] = true;
					this.bTopSoil[3] = true;
					this.bTopSoil[4] = true;
					this.bTopSoil[5] = true;
					this.bTopSoil[6] = true;
					this.bTopSoil[7] = true;
					sbyte b3 = this.density[7];
					int num6 = ((this.density[0] >> 7) & 1) | ((this.density[1] >> 6) & 2) | ((this.density[2] >> 5) & 4) | ((this.density[3] >> 4) & 8) | ((this.density[4] >> 3) & 16) | ((this.density[5] >> 2) & 32) | ((this.density[6] >> 1) & 64) | ((int)b3 & 128);
					if (b3 == 0)
					{
						int num7 = num++;
						array[num4, num5, 0] = (ushort)num7;
						this.vertexStorage[num7].position0 = new Vector3i(n + 1 << 8, num4 + 1 << 8, l + 1 << 8);
						this.vertexStorage[num7].normal = this.CalculateNormal(new Vector3i(n + 1, num4 + 1, l + 1));
						this.vertexStorage[num7].material = b2;
						this.vertexStorage[num7].texture = this.GetTextureFor(n, m, l, 7);
						this.vertexStorage[num7].bTopSoil = this.bTopSoil[7];
					}
					if ((num6 ^ ((b3 >> 7) & 255)) != 0)
					{
						byte b4 = Transvoxel.regularCellClass[num6];
						Transvoxel.RegularCellData regularCellData = Transvoxel.regularCellData[0, (int)b4];
						int triangleCount = regularCellData.GetTriangleCount();
						if (num2 + triangleCount > 8192)
						{
							goto IL_09E6;
						}
						int vertexCount = regularCellData.GetVertexCount();
						Transvoxel.RegularVertexData.Row row = Transvoxel.regularVertexData[num6];
						int num8 = 0;
						while (num8 < vertexCount)
						{
							int num9 = 0;
							ushort num10 = row.data[num8];
							int num11 = (num10 >> 4) & 15;
							int num12 = (int)(num10 & 15);
							int num13 = (int)this.density[num11];
							int num14 = (int)this.density[num12];
							int num15 = (num14 << 8) / (num14 - num13);
							Vector3i vector3i = new Vector3i(n + (num11 & 1), num4 + ((num11 >> 1) & 1), l + ((num11 >> 2) & 1));
							Vector3i vector3i2 = new Vector3i(n + (num12 & 1), num4 + ((num12 >> 1) & 1), l + ((num12 >> 2) & 1));
							if ((num15 & 255) != 0)
							{
								int num16 = (num10 >> 8) & 15;
								int num17 = (num10 >> 12) & 15;
								if ((num17 & num3) == num17)
								{
									if ((num17 & 4) != 0)
									{
										num9 = (int)array2[num4 - ((num17 >> 1) & 1), num5 - (num17 & 1), num16];
										goto IL_08E5;
									}
									try
									{
										num9 = (int)array[num4 - (num17 >> 1), num5 - (num17 & 1), num16];
										goto IL_08E5;
									}
									catch
									{
										Log.Error("Out of bounds! dY='{0}' x='{1}' edgeIndex='{2}'", new object[]
										{
											num4 - (num17 >> 1),
											num5 - (num17 & 1),
											num16
										});
										goto IL_08E5;
									}
								}
								num9 = num++;
								int num18 = 256 - num15;
								this.vertexStorage[num9].position0 = vector3i * num15 + vector3i2 * num18;
								this.vertexStorage[num9].normal = this.CalculateNormal(vector3i, vector3i2, num15, num18);
								this.vertexStorage[num9].material = b2;
								this.vertexStorage[num9].texture = this.GetTextureFor(n, m, l, (num13 < num14) ? num11 : num12);
								this.vertexStorage[num9].bTopSoil = this.bTopSoil[(num13 < num14) ? num11 : num12];
								if (num12 == 7)
								{
									try
									{
										array[num4, num5, num16] = (ushort)num9;
									}
									catch
									{
										Log.Error("Out of bounds! dY='{0}' x='{1}' edgeIndex='{2}'", new object[] { num4, num5, num16 });
									}
								}
								this.globalVertexIndex[num8] = num9;
							}
							else if (num15 == 0)
							{
								if (num12 == 7)
								{
									num9 = (int)array[num4, num5, 0];
									goto IL_08E5;
								}
								int num19 = num12 ^ 7;
								if ((num19 & num3) == num19)
								{
									if ((num19 & 4) != 0)
									{
										num9 = (int)array2[num4 - ((num19 >> 1) & 1), num5 - (num19 & 1), 0];
										goto IL_08E5;
									}
									num9 = (int)array[num4 - (num19 >> 1), num5 - (num19 & 1), 0];
									goto IL_08E5;
								}
								else
								{
									num9 = num++;
									this.vertexStorage[num9].position0 = new Vector3i(vector3i2.x << 8, vector3i2.y << 8, vector3i2.z << 8);
									this.vertexStorage[num9].normal = this.CalculateNormal(vector3i2);
									this.vertexStorage[num9].material = b2;
									this.vertexStorage[num9].texture = this.GetTextureFor(n, m, l, num12);
									this.vertexStorage[num9].bTopSoil = this.bTopSoil[num12];
									this.globalVertexIndex[num8] = num9;
								}
							}
							else
							{
								int num20 = num11 ^ 7;
								if ((num20 & num3) == num20)
								{
									if ((num20 & 4) != 0)
									{
										num9 = (int)array2[num4 - ((num20 >> 1) & 1), num5 - (num20 & 1), 0];
										goto IL_08E5;
									}
									num9 = (int)array[num4 - (num20 >> 1), num5 - (num20 & 1), 0];
									goto IL_08E5;
								}
								else
								{
									num9 = num++;
									this.vertexStorage[num9].position0 = new Vector3i(vector3i.x << 8, vector3i.y << 8, vector3i.z << 8);
									this.vertexStorage[num9].normal = this.CalculateNormal(vector3i);
									this.vertexStorage[num9].material = b2;
									this.vertexStorage[num9].texture = this.GetTextureFor(n, m, l, num11);
									this.vertexStorage[num9].bTopSoil = this.bTopSoil[num11];
									this.globalVertexIndex[num8] = num9;
								}
							}
							IL_08F0:
							num8++;
							continue;
							IL_08E5:
							this.globalVertexIndex[num8] = num9;
							goto IL_08F0;
						}
						if (b2 != 255)
						{
							byte[] vertexIndex = regularCellData.vertexIndex;
							int num21 = 0;
							for (int num22 = 0; num22 < triangleCount; num22++)
							{
								int num23 = num2 + num22;
								this.triangleStorage[num23].index0 = this.globalVertexIndex[(int)vertexIndex[num21]];
								this.triangleStorage[num23].index1 = this.globalVertexIndex[(int)vertexIndex[1 + num21]];
								this.triangleStorage[num23].index2 = this.globalVertexIndex[(int)vertexIndex[2 + num21]];
								num21 += 3;
							}
							num2 += triangleCount;
						}
					}
					num3 |= 1;
				}
				num3 = (num3 | 2) & 6;
			}
			num3 = 4;
			b ^= 1;
		}
		IL_09E6:
		int num24 = 0;
		for (int num25 = 0; num25 < num; num25++)
		{
			if (this.vertexStorage[num25].material != 255)
			{
				Vector3i position = this.vertexStorage[num25].position0;
				if ((position.x | position.y | position.z) >= 0)
				{
					int num26 = 1;
					num26 |= (position.x >> 11) & 2;
					num26 |= (position.y >> 10) & 4;
					num26 |= (position.z >> 9) & 8;
					this.vertexStorage[num25].statusFlags = (byte)num26;
					this.vertexStorage[num25].remapIndex = (int)((ushort)num24);
					num24++;
				}
			}
			else
			{
				this.vertexStorage[num25].statusFlags = 0;
			}
		}
		int num27 = 0;
		for (int num28 = 0; num28 < num2; num28++)
		{
			int index = this.triangleStorage[num28].index0;
			int index2 = this.triangleStorage[num28].index1;
			int index3 = this.triangleStorage[num28].index2;
			Vector3i vector3i3 = Vector3i.Cross(this.vertexStorage[index2].position0 - this.vertexStorage[index].position0, this.vertexStorage[index3].position0 - this.vertexStorage[index].position0);
			bool flag2;
			if ((vector3i3.x | vector3i3.y | vector3i3.z) != 0)
			{
				int num29 = ((vector3i3.x >> 31) & 2) | ((vector3i3.y >> 31) & 4) | ((vector3i3.z >> 31) & 8) | 1;
				flag2 = ((int)(this.vertexStorage[index].statusFlags & this.vertexStorage[index2].statusFlags & this.vertexStorage[index3].statusFlags) & num29) == 1;
			}
			flag2 = true;
			this.triangleStorage[num28].inclusionFlag = flag2;
			if (flag2)
			{
				num27++;
				int num30 = _mesh.FindOrCreateSubMesh(this.vertexStorage[index].texture, this.vertexStorage[index2].texture, this.vertexStorage[index3].texture);
				this.triangleStorage[num28].submeshIdx = num30;
				_mesh.GetColorForTextureId(num30, ref this.vertexStorage[index]);
				_mesh.GetColorForTextureId(num30, ref this.vertexStorage[index2]);
				_mesh.GetColorForTextureId(num30, ref this.vertexStorage[index3]);
			}
			else
			{
				Debug.LogError("excluding triangle!");
			}
		}
		if (num27 != num2)
		{
			Log.Warning("MG build {0}, {1}", new object[] { num27, num2 });
		}
		int count = _mesh.m_Vertices.Count;
		Vector3 vector = new Vector3(-0.5f, (float)_start.y + 0.5f, -0.5f) + _drawPosOffset;
		for (int num31 = 0; num31 < num; num31++)
		{
			if ((this.vertexStorage[num31].statusFlags & 1) != 0)
			{
				_mesh.m_Vertices.Add(this.vertexStorage[num31].position0.ToVector3() / 256f + vector);
				_mesh.m_ColorVertices.Add(this.vertexStorage[num31].color);
				_mesh.m_Uvs.Add(this.vertexStorage[num31].uv);
				_mesh.UvsCrack.Add(this.vertexStorage[num31].uv2);
				if (_mesh.m_Uvs3 == null)
				{
					_mesh.m_Uvs3 = new ArrayListMP<Vector2>(MemoryPools.poolVector2, 100000);
				}
				if (_mesh.m_Uvs4 == null)
				{
					_mesh.m_Uvs4 = new ArrayListMP<Vector2>(MemoryPools.poolVector2, 100000);
				}
				_mesh.m_Uvs3.Add(this.vertexStorage[num31].uv3);
				_mesh.m_Uvs4.Add(this.vertexStorage[num31].uv4);
			}
		}
		for (int num32 = 0; num32 < num2; num32++)
		{
			if (this.triangleStorage[num32].inclusionFlag)
			{
				int index4 = this.triangleStorage[num32].index0;
				int index5 = this.triangleStorage[num32].index1;
				int index6 = this.triangleStorage[num32].index2;
				_mesh.AddIndices(count + this.vertexStorage[index4].remapIndex, count + this.vertexStorage[index5].remapIndex, count + this.vertexStorage[index6].remapIndex, this.triangleStorage[num32].submeshIdx);
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
		return Vector3.zero;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 CalculateNormal(Vector3i coord0, Vector3i coord1, int t, int u)
	{
		Vector3 vector = this.CalculateNormal(coord0);
		Vector3 vector2 = this.CalculateNormal(coord1);
		Vector3 vector3 = vector * (float)t + vector2 * (float)u;
		vector3.Normalize();
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
	public Prefab prefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort[,,] deck1 = new ushort[17, 17, 4];

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort[,,] deck2 = new ushort[17, 17, 4];

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort[][,,] decks = new ushort[2][,,];

	[PublicizedFrom(EAccessModifier.Private)]
	public Transvoxel.BuildVertex[] vertexStorage = new Transvoxel.BuildVertex[8192];

	[PublicizedFrom(EAccessModifier.Private)]
	public Transvoxel.BuildTriangle[] triangleStorage = new Transvoxel.BuildTriangle[8192];

	[PublicizedFrom(EAccessModifier.Private)]
	public sbyte[] density = new sbyte[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] globalVertexIndex = new int[12];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3 cVectorAdd = new Vector3(0.5f, 0.5f, 0.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue[] blockValues = new BlockValue[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public bool[] bTopSoil = new bool[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public int startX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int startZ;

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
}
