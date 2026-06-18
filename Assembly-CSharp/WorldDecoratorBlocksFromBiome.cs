using System;
using System.Collections.Generic;
using System.Threading;

public class WorldDecoratorBlocksFromBiome : IWorldDecorator
{
	public WorldDecoratorBlocksFromBiome(IBiomeProvider _biomeProvider, DynamicPrefabDecorator _prefabDecorator)
	{
		this.biomeProvider = _biomeProvider;
		this.prefabDecorator = _prefabDecorator;
	}

	public void DecorateChunkOverlapping(World _world, Chunk _chunk, Chunk cX1Z0, Chunk cX0Z1, Chunk cX1Z1, int seed)
	{
		GameRandom gameRandom = null;
		try
		{
			this.rwlock.EnterWriteLock();
			gameRandom = Utils.RandomFromSeedOnPos(_chunk.X, _chunk.Z, seed);
			if (this.resourceNoise == null)
			{
				this.resourceNoise = new PerlinNoise(seed);
			}
			for (int i = this.biomePositionsHighId; i >= 0; i--)
			{
				List<Vector2i> list = this.biomePositions[i];
				if (list != null)
				{
					list.Clear();
				}
			}
			IChunkProvider chunkProvider = _world.ChunkCache.ChunkProvider;
			BiomeDefinition biome = _world.GetBiome("underwater");
			int num = -1;
			int blockWorldPosX = _chunk.GetBlockWorldPosX(0);
			int blockWorldPosZ = _chunk.GetBlockWorldPosZ(0);
			bool flag = this.prefabDecorator.IsWithinTraderArea(_chunk.worldPosIMin, _chunk.worldPosIMax);
			for (int j = 0; j < 16; j++)
			{
				int num2 = blockWorldPosZ + j;
				Vector2i vector2i;
				vector2i.y = j;
				for (int k = 0; k < 16; k++)
				{
					int num3 = blockWorldPosX + k;
					num++;
					if (flag && this.prefabDecorator.GetTraderAtPosition(new Vector3i(num3, 0, num2), 0) != null)
					{
						this.chunkBiomes[num] = null;
					}
					else
					{
						int poiblockIdOverride = chunkProvider.GetPOIBlockIdOverride(num3, num2);
						BiomeDefinition biomeDefinition;
						if (poiblockIdOverride > 0 && Block.list[poiblockIdOverride].blockMaterial.IsLiquid)
						{
							biomeDefinition = biome;
						}
						else
						{
							biomeDefinition = this.biomeProvider.GetBiomeAt(num3, num2);
						}
						if (biomeDefinition == null)
						{
							return;
						}
						List<Vector2i> list2 = this.biomePositions[(int)biomeDefinition.m_Id];
						if (list2 == null)
						{
							list2 = new List<Vector2i>(256);
							this.biomePositions[(int)biomeDefinition.m_Id] = list2;
							this.biomePositionsHighId = Utils.FastMax(this.biomePositionsHighId, (int)biomeDefinition.m_Id);
						}
						vector2i.x = k;
						list2.Add(vector2i);
						int terrainHeight = (int)_chunk.GetTerrainHeight(k, j);
						int subBiomeIdxAt = this.biomeProvider.GetSubBiomeIdxAt(biomeDefinition, num3, terrainHeight, num2);
						if (subBiomeIdxAt >= 0)
						{
							biomeDefinition = biomeDefinition.subbiomes[subBiomeIdxAt];
						}
						this.chunkBiomes[num] = biomeDefinition;
					}
				}
			}
			this.decoratePrefabs(_world, _chunk, cX1Z0, cX0Z1, cX1Z1, flag, gameRandom);
			this.decorateSingleBlocks(_world, _chunk, cX1Z0, cX0Z1, cX1Z1, flag, gameRandom);
		}
		catch (Exception ex)
		{
			Log.Error("{0} DecorateChunkOverlapping {1}, {2}", new object[]
			{
				GameManager.frameCount,
				_chunk,
				ex
			});
		}
		finally
		{
			GameRandomManager.Instance.FreeGameRandom(gameRandom);
			this.rwlock.ExitWriteLock();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void decoratePrefabs(World _world, Chunk _chunk, Chunk cX1Z0, Chunk cX0Z1, Chunk cX1Z1, bool _chunkOverlapsTrader, GameRandom _random)
	{
		if (this.prefabDecorator == null)
		{
			return;
		}
		for (int i = 0; i <= this.biomePositionsHighId; i++)
		{
			List<Vector2i> list = this.biomePositions[i];
			if (list != null)
			{
				int count = list.Count;
				if (count != 0)
				{
					for (int j = 0; j < count; j++)
					{
						int x = list[j].x;
						int y = list[j].y;
						BiomeDefinition biomeDefinition = this.chunkBiomes[x + y * 16];
						if (biomeDefinition != null && biomeDefinition.m_DecoPrefabs.Count != 0)
						{
							BiomePrefabDecoration biomePrefabDecoration = biomeDefinition.m_DecoPrefabs[_random.RandomRange(biomeDefinition.m_DecoPrefabs.Count)];
							if (biomePrefabDecoration != null && biomePrefabDecoration.prob >= _random.RandomFloat)
							{
								EnumDecoAllowed decoAllowedAt = _chunk.GetDecoAllowedAt(x, y);
								if (!decoAllowedAt.IsNothing() && !decoAllowedAt.GetStreetOnly())
								{
									EnumDecoAllowedSlope slope = decoAllowedAt.GetSlope();
									if (!biomePrefabDecoration.isDecorateOnSlopes)
									{
										if (slope >= EnumDecoAllowedSlope.Sloped)
										{
											goto IL_03E1;
										}
									}
									else if (slope >= EnumDecoAllowedSlope.Steep)
									{
										goto IL_03E1;
									}
									Prefab prefab = _world.m_PrefabCache.GetPrefab(biomePrefabDecoration.prefabName, true, true, false, false);
									if (prefab == null)
									{
										Log.Error("Prefab with name '" + biomePrefabDecoration.prefabName + "' not found!");
									}
									else
									{
										Vector3i size = prefab.size;
										if (x + size.x / 2 < 16 && y + size.z / 2 < 16)
										{
											int blockWorldPosX = _chunk.GetBlockWorldPosX(x);
											int blockWorldPosZ = _chunk.GetBlockWorldPosZ(y);
											int num = (int)(_chunk.GetTerrainHeight(x + size.x / 2, y + size.z / 2) + 1);
											if ((biomePrefabDecoration.checkResourceOffsetY >= 2147483647 || (num + prefab.yOffset >= 0 && GameUtils.CheckOreNoiseAt(this.resourceNoise, blockWorldPosX, num + biomePrefabDecoration.checkResourceOffsetY, blockWorldPosZ))) && num > 0 && num < 255 && _chunk.GetBlock(x, num + 1, y).isair)
											{
												BlockValue block = _chunk.GetBlock(x, num - 1, y);
												if (!block.isair && !block.Equals(Block.GetBlockValue("water", false)))
												{
													bool flag = true;
													bool flag2 = size.x > 1 || size.z > 1;
													int num2 = 0;
													while (flag && num2 < size.x)
													{
														int num3 = x + num2;
														int num4 = World.toBlockXZ(blockWorldPosX + num2);
														int num5 = 0;
														while (flag && num5 < size.z)
														{
															Chunk chunk = _chunk;
															int num6 = y + num5;
															if (num3 >= 16)
															{
																chunk = cX1Z0;
																if (num6 >= 16)
																{
																	chunk = cX1Z1;
																}
															}
															else if (num6 >= 16)
															{
																chunk = cX0Z1;
															}
															int num7 = World.toBlockXZ(blockWorldPosZ + num5);
															EnumDecoAllowed decoAllowedAt2 = chunk.GetDecoAllowedAt(num4, num7);
															if (decoAllowedAt2.IsNothing())
															{
																flag = false;
															}
															if (flag && decoAllowedAt2.GetStreetOnly())
															{
																flag = false;
															}
															if (flag && flag2 && !decoAllowedAt2.AllowBigDeco())
															{
																flag = false;
															}
															EnumDecoAllowedSlope slope2 = decoAllowedAt2.GetSlope();
															if (!biomePrefabDecoration.isDecorateOnSlopes)
															{
																if (flag && slope2 >= EnumDecoAllowedSlope.Sloped)
																{
																	flag = false;
																}
																if (flag && (int)chunk.GetHeight(num4, num7) != num - 1)
																{
																	flag = false;
																}
															}
															else if (flag && slope2 >= EnumDecoAllowedSlope.Steep)
															{
																flag = false;
															}
															if ((int)chunk.GetHeight(num4, num7) + prefab.size.y >= 255)
															{
																flag = false;
															}
															num5++;
														}
														num2++;
													}
													if (flag)
													{
														int num8 = _random.RandomRange(4);
														if (num8 != 0)
														{
															prefab = prefab.Clone(false);
															prefab.RotateY(true, num8);
														}
														Vector3i vector3i = new Vector3i(blockWorldPosX, num + prefab.yOffset, blockWorldPosZ);
														prefab.CopyIntoLocal(_world.ChunkCache, vector3i, false, false, FastTags<TagGroup.Global>.none);
														prefab.SnapTerrainToArea(_world.ChunkCache, vector3i);
													}
												}
											}
										}
									}
								}
							}
						}
						IL_03E1:;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void decorateSingleBlocks(World _world, Chunk _chunk, Chunk cX1Z0, Chunk cX0Z1, Chunk cX1Z1, bool _chunkOverlapsTrader, GameRandom _random)
	{
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				if (!_chunkOverlapsTrader || this.chunkBiomes[j + i * 16] != null)
				{
					int num = (int)(_chunk.GetTerrainHeight(j, i) + 1);
					if (num < 255)
					{
						Vector3i vector3i = new Vector3i(j, num, i);
						this.decorateSingleBlock(_world, _chunk, cX1Z0, cX0Z1, cX1Z1, _random, vector3i);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void decorateSingleBlock(World _world, Chunk _chunk, Chunk cX1Z0, Chunk cX0Z1, Chunk cX1Z1, GameRandom _random, Vector3i blockPos)
	{
		if (!_chunk.GetBlockNoDamage(blockPos.x, blockPos.y, blockPos.z).isair)
		{
			return;
		}
		bool flag = _chunk.IsWater(blockPos.x, blockPos.y, blockPos.z);
		EnumDecoAllowed decoAllowedAt = _chunk.GetDecoAllowedAt(blockPos.x, blockPos.z);
		if (!flag && decoAllowedAt.IsNothing())
		{
			return;
		}
		if (decoAllowedAt.GetStreetOnly())
		{
			return;
		}
		BlockValue blockNoDamage = _chunk.GetBlockNoDamage(blockPos.x, blockPos.y + 1, blockPos.z);
		BlockValue blockNoDamage2 = _chunk.GetBlockNoDamage(blockPos.x, blockPos.y - 1, blockPos.z);
		if (!blockNoDamage.isair || blockNoDamage2.isair)
		{
			return;
		}
		if (flag && !_chunk.IsWater(blockPos.x, blockPos.y + 1, blockPos.z))
		{
			return;
		}
		int num = blockPos.x + blockPos.z * 16;
		BiomeDefinition biomeDefinition = this.chunkBiomes[num];
		Vector3i worldPos = _chunk.GetWorldPos();
		float terrainNormalY = _chunk.GetTerrainNormalY(blockPos.x, blockPos.z);
		for (int i = 0; i < biomeDefinition.m_DecoBlocks.Count; i++)
		{
			BiomeBlockDecoration biomeBlockDecoration = biomeDefinition.m_DecoBlocks[i];
			if (this.decorateSingleBlockTryPlaceDeco(_world, _chunk, cX1Z0, cX0Z1, cX1Z1, _random, blockPos, decoAllowedAt, biomeDefinition, biomeBlockDecoration, worldPos, terrainNormalY))
			{
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool decorateSingleBlockTryPlaceDeco(World _world, Chunk _chunk, Chunk cX1Z0, Chunk cX0Z1, Chunk cX1Z1, GameRandom _random, Vector3i blockPos, EnumDecoAllowed decoAllowed, BiomeDefinition bd, BiomeBlockDecoration deco, Vector3i chunkWorldPos, float normalY)
	{
		BlockValue blockValue = deco.blockValues[0];
		Block block = blockValue.Block;
		if (block.IsDistantDecoration && DecoManager.Instance.IsEnabled)
		{
			return false;
		}
		if (!block.CanDecorateOnSlopes)
		{
			if (decoAllowed.GetSlope() >= EnumDecoAllowedSlope.Sloped)
			{
				return false;
			}
		}
		else if (block.SlopeMaxCos > normalY)
		{
			return false;
		}
		if (block.IsPlant() && blockPos.y > 0 && _chunk.GetBlock(blockPos.x, blockPos.y - 1, blockPos.z).Block.blockMaterial.FertileLevel == 0)
		{
			return false;
		}
		if (block.isMultiBlock && block.multiBlockPos.dim.y + blockPos.y > 255)
		{
			return false;
		}
		if (_random.RandomFloat >= deco.prob)
		{
			return false;
		}
		if (deco.checkResourceOffsetY < 2147483647)
		{
			Vector3i vector3i = _chunk.ToWorldPos(blockPos);
			vector3i.y += deco.checkResourceOffsetY;
			if (!GameUtils.CheckOreNoiseAt(this.resourceNoise, vector3i.x, vector3i.y, vector3i.z))
			{
				return false;
			}
		}
		BlockValue blockValue2 = BlockPlaceholderMap.Instance.Replace(blockValue, _random, _chunk, chunkWorldPos.x + blockPos.x, 0, chunkWorldPos.z + blockPos.z, FastTags<TagGroup.Global>.none, false, true);
		if (blockValue2.isair)
		{
			return true;
		}
		if (deco.randomRotateMax > 0)
		{
			blockValue2.rotation = BiomeBlockDecoration.GetRandomRotation(_random.RandomFloat, deco.randomRotateMax);
		}
		Block block2 = blockValue2.Block;
		int decoRadius = DecoUtils.GetDecoRadius(blockValue2, block2);
		if (decoRadius > 0)
		{
			blockPos.x += decoRadius;
			blockPos.z += decoRadius;
			if (blockPos.x >= 16 || blockPos.z >= 16)
			{
				return false;
			}
			blockPos.y = (int)(_chunk.GetTerrainHeight(blockPos.x, blockPos.z) + 1);
			if (block.IsPlant() && _chunk.GetBlock(blockPos.x, blockPos.y - 1, blockPos.z).Block.blockMaterial.FertileLevel == 0)
			{
				return false;
			}
			if (!_chunk.GetBlock(blockPos).isair)
			{
				return false;
			}
		}
		if (!DecoUtils.CanPlaceDeco(_chunk, cX1Z0, cX0Z1, cX1Z1, chunkWorldPos + blockPos, blockValue2, (EnumDecoAllowed da) => !da.GetStreetOnly()))
		{
			return false;
		}
		DecoUtils.ApplyDecoAllowed(_chunk, cX1Z0, cX0Z1, cX1Z1, chunkWorldPos + blockPos, blockValue2);
		blockValue2 = block2.OnBlockPlaced(_world, chunkWorldPos + blockPos, blockValue2, _random);
		_chunk.SetBlock(_world, blockPos.x, blockPos.y, blockPos.z, blockValue2, true, true, false, false, -1);
		if (!block2.shape.IsOmitTerrainSnappingUp && !block2.IsTerrainDecoration)
		{
			_world.ChunkCache.SnapTerrainToPositionAroundLocal(_chunk.ToWorldPos(blockPos) - Vector3i.up);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IBiomeProvider biomeProvider;

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicPrefabDecorator prefabDecorator;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim();

	[PublicizedFrom(EAccessModifier.Private)]
	public PerlinNoise resourceNoise;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BiomeDefinition[] chunkBiomes = new BiomeDefinition[256];

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cBiomeIdMax = 256;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Vector2i>[] biomePositions = new List<Vector2i>[256];

	[PublicizedFrom(EAccessModifier.Private)]
	public int biomePositionsHighId = -1;
}
