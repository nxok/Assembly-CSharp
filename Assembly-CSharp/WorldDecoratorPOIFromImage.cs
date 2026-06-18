using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.Collections;
using UnityEngine;

public class WorldDecoratorPOIFromImage : IWorldDecorator
{
	public WorldDecoratorPOIFromImage(string _levelName, string _worldPath, DynamicPrefabDecorator _prefabDecorator, int _worldX, int _worldZ, Texture2D _splat3Tex = null, bool _bChangeWaterDensity = true, int _worldScale = 1, HeightMap _heightMap = null, Texture2D _splat4Tex = null)
	{
		this.m_PrefabDecorator = _prefabDecorator;
		this.bChangeWaterDensity = _bChangeWaterDensity;
		this.worldScale = _worldScale;
		this.worldSizeX = _worldX;
		this.worldSizeZ = _worldZ;
		this.heightMap = _heightMap;
		this.worldPath = _worldPath;
		this.splat3Tex = _splat3Tex;
		this.splat4Tex = _splat4Tex;
	}

	public IEnumerator InitData()
	{
		MicroStopwatch msw = new MicroStopwatch(true);
		int num = this.worldSizeX * this.worldScale;
		int num2 = this.worldSizeZ * this.worldScale;
		GridCompressedData<byte> poiCols = new GridCompressedData<byte>(num, num2, 16, 16);
		this.water16x16ChunksW = num / 16;
		this.water16x16Chunks = new byte[num / 16 * num2 / 16];
		yield return null;
		List<WaterInfo> waterSources = WorldDecoratorPOIFromImage.LoadWaterInfo(this.worldPath + "/water_info.xml");
		BlockValue dirtBV = new BlockValue((uint)Block.GetBlockByName("terrDirt", false).blockID);
		byte b = 5;
		WorldBiomes.Instance.AddPoiMapElement(new PoiMapElement((uint)b, null, dirtBV, dirtBV, 0, -1, 0, 0));
		byte idSand = b;
		b += 1;
		yield return null;
		if (this.splat4Tex != null)
		{
			if (this.splat4Tex.format != 5)
			{
				throw new Exception(string.Format("splat4Tex was not in the correct format. Expected: {0}, Actual: {1}", 5, this.splat4Tex.format));
			}
			NativeArray<TextureUtils.ColorARGB32> waterColors = this.splat4Tex.GetPixelData<TextureUtils.ColorARGB32>(0);
			int w = this.splat4Tex.width;
			int h = this.splat4Tex.height;
			int num4;
			for (int z = 0; z < h; z = num4 + 1)
			{
				int num3 = z * w;
				for (int i = 0; i < w; i++)
				{
					TextureUtils.ColorARGB32 colorARGB = waterColors[i + num3];
					if (colorARGB.b > 0)
					{
						b = (byte)Mathf.Clamp((int)(5 + colorARGB.b), 0, 255);
						if (WorldBiomes.Instance.getPoiForColor((uint)b) == null)
						{
							WorldBiomes.Instance.AddPoiMapElement(new PoiMapElement((uint)b, null, new BlockValue((uint)Block.GetBlockByName("water", false).blockID), new BlockValue((uint)Block.GetBlockByName("terrDirt", false).blockID), 0, -1, (int)(colorARGB.b + 1), 0));
						}
						poiCols.SetValue(i, z, b);
						this.water16x16Chunks[i / 16 + z / 16 * w / 16] = b - 5;
					}
				}
				if (msw.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					msw.ResetAndRestart();
				}
				num4 = z;
			}
			waterColors.Dispose();
			waterColors = default(NativeArray<TextureUtils.ColorARGB32>);
		}
		else if (waterSources != null && waterSources.Count > 0)
		{
			BlockValue waterBV = new BlockValue(240U);
			List<Vector2i> floodList = new List<Vector2i>(100000);
			int num4;
			for (int h = 0; h < waterSources.Count; h = num4 + 1)
			{
				WaterInfo waterInfo = waterSources[h];
				b = (byte)Mathf.Clamp(5 + waterInfo.pos.y, 0, 255);
				WorldBiomes.Instance.AddPoiMapElement(new PoiMapElement((uint)b, null, waterBV, dirtBV, 0, -1, waterInfo.pos.y + 1, 0));
				GameUtils.WaterFloodFill(poiCols, this.water16x16Chunks, this.worldSizeX * this.worldScale, this.heightMap, waterInfo.pos.x, waterInfo.pos.y, waterInfo.pos.z, b, idSand, floodList, waterInfo.minX, waterInfo.maxX, waterInfo.minZ, waterInfo.maxZ, this.worldScale);
				floodList.Clear();
				if (msw.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					msw.ResetAndRestart();
				}
				num4 = h;
			}
			floodList = null;
		}
		yield return null;
		this.heightMap = null;
		int zE = this.worldSizeZ * this.worldScale;
		int xE = this.worldSizeX * this.worldScale;
		Texture2D localSplat3 = null;
		if (this.splat3Tex == null)
		{
			localSplat3 = (SdFile.Exists(this.worldPath + "/splat3.png") ? TextureUtils.LoadTexture(this.worldPath + "/splat3.png", 0, false, false, null) : TextureUtils.LoadTexture(this.worldPath + " / splat3.tga", 0, false, false, null));
			if (localSplat3.width != xE || localSplat3.height != zE)
			{
				localSplat3.BilinearScale(xE, zE);
			}
			this.splat3Tex = localSplat3;
		}
		bool flag = this.splat3Tex.format == 5;
		if (!flag && this.splat3Tex.format != 4)
		{
			Log.Error("World's splat3 file is not in the correct format (needs to be either RGBA32 or ARGB32)!");
			yield break;
		}
		int splatMapWidth = this.splat3Tex.width;
		this.splat3Width = this.splat3Tex.width;
		this.splat3Height = this.splat3Tex.height;
		if (this.splat3Tex == null)
		{
			yield break;
		}
		if (flag)
		{
			NativeArray<TextureUtils.ColorARGB32> waterColors = this.splat3Tex.GetRawTextureData<TextureUtils.ColorARGB32>();
			int num4;
			for (int h = 0; h < zE; h = num4 + 1)
			{
				for (int j = 0; j < xE; j++)
				{
					TextureUtils.ColorARGB32 colorARGB2 = waterColors[j / this.worldScale + h / this.worldScale * splatMapWidth];
					if (colorARGB2.r > 127)
					{
						poiCols.SetValue(j, h, 2);
					}
					else if (colorARGB2.g > 127)
					{
						poiCols.SetValue(j, h, 3);
					}
					else if (colorARGB2.b > 127)
					{
						poiCols.SetValue(j, h, 4);
					}
				}
				if (msw.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					msw.ResetAndRestart();
				}
				num4 = h;
			}
			waterColors = default(NativeArray<TextureUtils.ColorARGB32>);
		}
		else
		{
			NativeArray<Color32> splat3Cols = this.splat3Tex.GetRawTextureData<Color32>();
			int num4;
			for (int h = 0; h < zE; h = num4 + 1)
			{
				for (int k = 0; k < xE; k++)
				{
					Color32 color = splat3Cols[k / this.worldScale + h / this.worldScale * splatMapWidth];
					if (color.r > 127)
					{
						poiCols.SetValue(k, h, 2);
					}
					else if (color.g > 127)
					{
						poiCols.SetValue(k, h, 3);
					}
					else if (color.b > 127)
					{
						poiCols.SetValue(k, h, 4);
					}
				}
				if (msw.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					msw.ResetAndRestart();
				}
				num4 = h;
			}
			splat3Cols = default(NativeArray<Color32>);
		}
		poiCols.CheckSameValues();
		this.m_Poi = new WorldGridCompressedData<byte>(poiCols);
		this.splat3Tex = null;
		if (localSplat3 != null)
		{
			Object.Destroy(localSplat3);
		}
		yield break;
	}

	public void GetWaterChunks16x16(out int _water16x16ChunksW, out byte[] _water16x16Chunks)
	{
		_water16x16ChunksW = this.water16x16ChunksW;
		_water16x16Chunks = this.water16x16Chunks;
	}

	public static List<WaterInfo> LoadWaterInfo(string _filename)
	{
		if (!SdFile.Exists(_filename))
		{
			return null;
		}
		List<WaterInfo> list = new List<WaterInfo>();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.SdLoad(_filename);
		foreach (object obj in xmlDocument.DocumentElement.ChildNodes)
		{
			XmlNode xmlNode = (XmlNode)obj;
			if (xmlNode.NodeType == XmlNodeType.Element && xmlNode.Name.EqualsCaseInsensitive("Water"))
			{
				XmlElement xmlElement = (XmlElement)xmlNode;
				string attribute = xmlElement.GetAttribute("pos");
				if (attribute.Length != 0)
				{
					WaterInfo waterInfo = default(WaterInfo);
					waterInfo.pos = StringParsers.ParseVector3i(attribute, 0, -1, false);
					waterInfo.minX = int.MinValue;
					string text = xmlElement.GetAttribute("minx");
					if (text.Length > 0)
					{
						waterInfo.minX = int.Parse(text);
					}
					waterInfo.maxX = int.MaxValue;
					text = xmlElement.GetAttribute("maxx");
					if (text.Length > 0)
					{
						waterInfo.maxX = int.Parse(text);
					}
					waterInfo.minZ = int.MinValue;
					text = xmlElement.GetAttribute("minz");
					if (text.Length > 0)
					{
						waterInfo.minZ = int.Parse(text);
					}
					waterInfo.maxZ = int.MaxValue;
					text = xmlElement.GetAttribute("maxz");
					if (text.Length > 0)
					{
						waterInfo.maxZ = int.Parse(text);
					}
					list.Add(waterInfo);
				}
			}
		}
		return list;
	}

	public void DecorateChunkOverlapping(World _world, Chunk _chunk, Chunk _c10, Chunk _c01, Chunk _c11, int seed)
	{
		if (this.m_Poi == null)
		{
			return;
		}
		if (_c10 == null || _c01 == null || _c11 == null)
		{
			Log.Warning("Adjacent chunk missing on decoration " + ((_chunk != null) ? _chunk.ToString() : null));
			return;
		}
		GameRandom gameRandom = Utils.RandomFromSeedOnPos(_chunk.X, _chunk.Z, seed);
		GameManager.Instance.GetSpawnPointList();
		Vector3i worldPos = _chunk.GetWorldPos();
		bool flag = this.m_PrefabDecorator.IsWithinTraderArea(_chunk.worldPosIMin, _chunk.worldPosIMax);
		for (int i = 0; i < 16; i++)
		{
			int blockWorldPosZ = _chunk.GetBlockWorldPosZ(i);
			for (int j = 0; j < 16; j++)
			{
				int blockWorldPosX = _chunk.GetBlockWorldPosX(j);
				if ((!flag || this.m_PrefabDecorator.GetTraderAtPosition(new Vector3i(blockWorldPosX, 0, blockWorldPosZ), 0) == null) && this.m_Poi.Contains(blockWorldPosX, blockWorldPosZ))
				{
					byte data = this.m_Poi.GetData(blockWorldPosX, blockWorldPosZ);
					if (data != 0 && data != 255)
					{
						PoiMapElement poiForColor = _world.Biomes.getPoiForColor((uint)data);
						if (poiForColor != null)
						{
							EnumDecoAllowed decoAllowedAt = _chunk.GetDecoAllowedAt(j, i);
							if (!poiForColor.m_BlockValue.isWater)
							{
								_chunk.SetDecoAllowedStreetOnlyAt(j, i, true);
							}
							int num = poiForColor.m_YPos;
							if (num < 0)
							{
								num = (int)_chunk.GetTerrainHeight(j, i);
							}
							if (!poiForColor.m_BlockValue.isair)
							{
								BlockValue blockValue = poiForColor.m_BlockValue;
								PoiMapDecal randomDecal = poiForColor.GetRandomDecal(gameRandom);
								if (randomDecal != null && _chunk.GetTerrainNormalY(j, i) > 0.98f && _world.GetBlock(new Vector3i(blockWorldPosX, num, blockWorldPosZ) + new Vector3i(Utils.BlockFaceToVector(randomDecal.face))).isair)
								{
									blockValue.hasdecal = true;
									blockValue.decalface = randomDecal.face;
									blockValue.decaltex = (byte)randomDecal.textureIndex;
								}
								BlockValue block = _chunk.GetBlock(j, num, i);
								if (block.isair || (block.Block.shape.IsTerrain() && blockValue.Block.shape.IsTerrain()))
								{
									_chunk.SetBlockRaw(j, num, i, blockValue);
								}
								if (poiForColor.m_BlockValue.isWater)
								{
									for (int k = num; k <= poiForColor.m_YPosFill; k++)
									{
										if (WaterUtils.CanWaterFlowThrough(_chunk.GetBlock(j, k, i)))
										{
											_chunk.SetWater(j, k, i, WaterValue.Full);
										}
									}
								}
								else
								{
									for (int l = num; l <= poiForColor.m_YPosFill; l++)
									{
										if (_chunk.GetBlock(j, l, i).isair)
										{
											_chunk.SetBlockRaw(j, l, i, blockValue);
										}
									}
								}
								if (block.Block.shape.IsTerrain() && !poiForColor.m_BlockBelow.isair)
								{
									_chunk.SetBlockRaw(j, num, i, poiForColor.m_BlockBelow);
								}
								if (poiForColor.m_YPosFill > 0 && this.bChangeWaterDensity)
								{
									if (!blockValue.Block.shape.IsTerrain())
									{
										_chunk.SetDensity(j, num, i, MarchingCubes.DensityAir);
									}
									for (int m = num; m <= poiForColor.m_YPosFill; m++)
									{
										_chunk.SetBlockRaw(j, m, i, blockValue);
										if (!blockValue.Block.shape.IsTerrain())
										{
											_chunk.SetDensity(j, m, i, MarchingCubes.DensityAir);
										}
									}
								}
								if (!decoAllowedAt.IsNothing())
								{
									PoiMapBlock randomBlockOnTop = poiForColor.GetRandomBlockOnTop(gameRandom);
									if (randomBlockOnTop != null && (randomBlockOnTop.offset != 0 || _world.GetBlock(new Vector3i(blockWorldPosX, num + 1, blockWorldPosZ)).isair))
									{
										if (!_world.IsEditor())
										{
											blockValue = BlockPlaceholderMap.Instance.Replace(randomBlockOnTop.blockValue, gameRandom, _chunk, blockWorldPosX, 0, blockWorldPosZ, FastTags<TagGroup.Global>.none, false, true);
										}
										else
										{
											blockValue = randomBlockOnTop.blockValue;
										}
										int num2 = num + 1 + randomBlockOnTop.offset;
										Vector3i vector3i = new Vector3i(worldPos.x + j, worldPos.y + num2, worldPos.z + i);
										if (DecoUtils.CanPlaceDeco(_chunk, _c10, _c01, _c11, vector3i, blockValue, null))
										{
											DecoUtils.ApplyDecoAllowed(_chunk, _c10, _c01, _c11, vector3i, blockValue);
											Block block2 = blockValue.Block;
											blockValue = block2.OnBlockPlaced(_world, vector3i, blockValue, gameRandom);
											if (!block2.HasTileEntity)
											{
												_chunk.SetBlockRaw(j, num2, i, blockValue);
											}
											else
											{
												_chunk.SetBlock(_world, j, num2, i, blockValue, true, true, false, false, -1);
											}
										}
									}
								}
							}
							else if (poiForColor.m_sModelName != null && poiForColor.m_sModelName.Length > 0 && !decoAllowedAt.IsNothing())
							{
								_world.m_PrefabCache.GetPrefab(poiForColor.m_sModelName, true, true, false, false).CopyIntoLocal(_world.ChunkCache, new Vector3i(blockWorldPosX, num, blockWorldPosZ), false, false, FastTags<TagGroup.Global>.none);
							}
						}
					}
				}
			}
		}
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
	}

	public WorldGridCompressedData<byte> m_Poi;

	public int worldScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public int worldSizeX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int worldSizeZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public HeightMap heightMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicPrefabDecorator m_PrefabDecorator;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bChangeWaterDensity;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string worldPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D splat3Tex;

	public int splat3Width;

	public int splat3Height;

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D splat4Tex;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] water16x16Chunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int water16x16ChunksW;
}
