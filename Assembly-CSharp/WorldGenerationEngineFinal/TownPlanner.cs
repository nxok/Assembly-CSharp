using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class TownPlanner
	{
		public TownPlanner(WorldBuilder _worldBuilder)
		{
			this.worldBuilder = _worldBuilder;
		}

		public void Plan(DynamicProperties _properties, int worldSeed)
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			Dictionary<BiomeType, List<Vector2i>> streetTilesByBiome = this.getStreetTilesByBiome();
			Dictionary<int, TownPlanner.TownshipSpawnInfo> townshipSpawnInfos = new Dictionary<int, TownPlanner.TownshipSpawnInfo>();
			this.getTownshipCounts(_properties, townshipSpawnInfos);
			int num = 0;
			List<int> list = new List<int>();
			townshipSpawnInfos.CopyKeysTo(list);
			list.Sort((int key1, int key2) => townshipSpawnInfos[key2].max.CompareTo(townshipSpawnInfos[key1].max));
			int num2 = 1982;
			GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom();
			List<int> list2 = new List<int> { 0, 1, 2, 3 };
			foreach (int num3 in list)
			{
				TownshipData townshipData;
				if (WorldBuilderStatic.idToTownshipData.TryGetValue(num3, out townshipData))
				{
					string name = townshipData.Name;
					this.worldBuilder.SetTaskMessage(string.Format(this.worldBuilder.messageTownPlanning, Application.isEditor ? name : string.Empty));
					gameRandom.SetSeed(worldSeed + num2++);
					bool flag = townshipData.Category == TownshipData.eCategory.Roadside;
					int num4 = this.worldBuilder.StreetTileMapWidth / 10;
					if (flag)
					{
						num4 = this.worldBuilder.StreetTileMapWidth / 8;
					}
					int num5 = this.worldBuilder.StreetTileMapWidth - num4;
					int num6 = this.worldBuilder.StreetTileMapWidth - num4;
					int num7 = 80;
					int num8 = 30;
					bool flag2 = false;
					TownPlanner.TownshipSpawnInfo townshipSpawnInfo = townshipSpawnInfos[num3];
					BiomeType biomeType = BiomeType.none;
					int i = 0;
					while (i < townshipSpawnInfo.count)
					{
						int num9 = (flag2 ? 1 : (flag ? 3 : townshipSpawnInfo.distance));
						if (biomeType == BiomeType.none)
						{
							biomeType = this.getBiomeWithMostAvailableSpace(townshipData);
						}
						List<Vector2i> list3 = null;
						BiomeType biomeType2 = biomeType;
						for (int j = 0; j < 5; j++)
						{
							List<Vector2i> list4;
							if (streetTilesByBiome.TryGetValue(biomeType, out list4))
							{
								list3 = new List<Vector2i>(list4);
								for (int k = list3.Count - 1; k >= 0; k--)
								{
									Vector2i vector2i = list3[k];
									if (vector2i.x <= num4 || vector2i.y <= num4 || vector2i.x >= num5 || vector2i.y >= num6 || this.IsTooClose(vector2i, flag, num9))
									{
										list3.RemoveAt(k);
									}
								}
								if (list3.Count > 0)
								{
									break;
								}
							}
							biomeType = this.nextBiomeType(biomeType, townshipData);
							if (biomeType == biomeType2)
							{
								break;
							}
						}
						Township township = null;
						List<StreetTile> list5 = null;
						List<StreetTile> list6 = null;
						int l;
						if (list3 != null && list3.Count > 0)
						{
							list5 = new List<StreetTile>();
							list6 = new List<StreetTile>();
							township = new Township(this.worldBuilder, townshipData)
							{
								BiomeType = biomeType
							};
							int num10 = townshipSpawnInfo.min;
							if (townshipSpawnInfo.max >= 10)
							{
								num10 = (townshipSpawnInfo.min + townshipSpawnInfo.max) / 2;
							}
							l = gameRandom.RandomRange(num10, townshipSpawnInfo.max + 1);
							num10 = townshipSpawnInfo.min;
							if (flag2 && num10 >= 4)
							{
								num10 = 1;
								l = 4;
							}
							while (l >= num10)
							{
								int num11 = gameRandom.RandomRange(0, list3.Count);
								int num12 = ((gameRandom.RandomFloat < 0.5f) ? (list3.Count - 1) : 1);
								for (int m = 0; m < list3.Count; m++)
								{
									Vector2i vector2i2 = list3[num11];
									township.GridCenter = vector2i2;
									num11 = (num11 + num12) % list3.Count;
									for (int n = 0; n < 12; n++)
									{
										this.GetStreetLayout(vector2i2, l, gameRandom, flag, list5);
										if (list5.Count >= l && (townshipData.OutskirtDistrictPercent <= 0f || this.Grow(townshipData, list5, list6, !flag2)))
										{
											m = 999999;
											l = -1;
											break;
										}
									}
								}
								l--;
							}
						}
						else
						{
							l = 0;
							num7 = 0;
						}
						if (l < 0)
						{
							goto IL_0422;
						}
						if (num7 > 0)
						{
							num7--;
							i--;
							biomeType = this.nextBiomeType(biomeType, townshipData);
						}
						else
						{
							if (townshipSpawnInfo.min < 4 || num8 <= 0)
							{
								goto IL_0422;
							}
							num8--;
							i--;
							flag2 = true;
							biomeType = this.nextBiomeType(biomeType, townshipData);
						}
						IL_0768:
						i++;
						continue;
						IL_0422:
						num7 = 80;
						num8 = 30;
						flag2 = false;
						if (l >= 0)
						{
							biomeType = BiomeType.none;
							goto IL_0768;
						}
						if (townshipData.OutskirtDistrictPercent > 0f)
						{
							string outskirtDistrict = townshipData.OutskirtDistrict;
							foreach (StreetTile streetTile in list6)
							{
								streetTile.SetTownship(township);
								streetTile.District = DistrictPlannerStatic.Districts[outskirtDistrict];
								township.Streets[streetTile.GridPosition] = streetTile;
							}
						}
						foreach (StreetTile streetTile2 in list5)
						{
							streetTile2.SetTownship(township);
							township.Streets[streetTile2.GridPosition] = streetTile2;
							streetTile2.District = null;
						}
						this.worldBuilder.DistrictPlanner.PlanTownship(township);
						if (!township.IsRoadside())
						{
							foreach (StreetTile streetTile3 in township.Streets.Values)
							{
								streetTile3.SetExitsToMyTownship();
							}
							foreach (StreetTile streetTile4 in township.Streets.Values)
							{
								if (streetTile4.District.type != District.Type.Gateway)
								{
									list2.Shuffle(gameRandom);
									foreach (int num13 in list2)
									{
										StreetTile neighbor = streetTile4.GetNeighbor(num13);
										if (neighbor != null && neighbor.District != null && neighbor.District.type != District.Type.Gateway && neighbor.District != streetTile4.District)
										{
											int num14 = streetTile4.CountDistrictExitsBetween(neighbor);
											int num15 = ((streetTile4.District.type == District.Type.Downtown || neighbor.District.type == District.Type.Downtown) ? 2 : 1);
											if (num14 > num15)
											{
												streetTile4.SetExitUnUsedAndFromNeighbor(num13);
											}
										}
									}
								}
							}
							int num16 = 0;
							foreach (StreetTile streetTile5 in township.Streets.Values)
							{
								if (streetTile5.District.type != District.Type.Gateway)
								{
									int num17 = 1 << (int)streetTile5.District.type;
									if ((num16 & num17) == 0 && streetTile5.ChangeLConnectionToCap(gameRandom))
									{
										num16 |= num17;
									}
								}
							}
						}
						township.CleanupStreets();
						township.ID = num++;
						this.worldBuilder.Townships.Add(township);
						TownPlanner.BiomeStats biomeStats;
						if (!this.biomeStats.TryGetValue(biomeType, out biomeStats))
						{
							biomeStats = new TownPlanner.BiomeStats();
							this.biomeStats.Add(biomeType, biomeStats);
						}
						biomeStats.townshipCount++;
						int num18;
						if (!biomeStats.counts.TryGetValue(name, out num18))
						{
							biomeStats.counts.Add(name, 1);
						}
						else
						{
							biomeStats.counts[name] = num18 + 1;
						}
						biomeType = this.nextBiomeType(biomeType, townshipData);
						goto IL_0768;
					}
				}
			}
			Log.Out("TownPlanner Plan {0} in {1}", new object[]
			{
				this.worldBuilder.Townships.Count,
				(float)microStopwatch.ElapsedMilliseconds * 0.001f
			});
			for (int num19 = 0; num19 < 5; num19++)
			{
				BiomeType biomeType3 = (BiomeType)num19;
				TownPlanner.BiomeStats biomeStats2;
				if (this.biomeStats.TryGetValue(biomeType3, out biomeStats2))
				{
					string text = "";
					foreach (KeyValuePair<string, int> keyValuePair in biomeStats2.counts)
					{
						text += string.Format(", {0} {1}", keyValuePair.Key, keyValuePair.Value);
					}
					Log.Out("TownPlanner {0} has {1} townships{2}", new object[] { biomeType3, biomeStats2.townshipCount, text });
				}
			}
			this.biomeStats.Clear();
			this.worldBuilder.SetTaskMessage(this.worldBuilder.messageTownPlanningFinished);
		}

		public void SpawnPrefabs()
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			foreach (Township township in this.worldBuilder.Townships)
			{
				township.SpawnPrefabs();
			}
			Log.Out(string.Format("TownPlanner SpawnPrefabs in {0}, r={1:x}", (float)microStopwatch.ElapsedMilliseconds * 0.001f, Rand.Instance.PeekSample()));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool IsTooClose(Vector2i position, bool isRoadsideTownship, int distance)
		{
			foreach (Township township in this.worldBuilder.Townships)
			{
				bool flag = township.IsRoadside();
				foreach (Vector2i vector2i in township.Streets.Keys)
				{
					if (isRoadsideTownship && flag && Utils.FastAbs((float)(position.x - vector2i.x)) < 5f && Utils.FastAbs((float)(position.y - vector2i.y)) < 5f)
					{
						return true;
					}
					if (Utils.FastAbs((float)(position.x - vector2i.x)) < (float)distance && Utils.FastAbs((float)(position.y - vector2i.y)) < (float)distance)
					{
						return true;
					}
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool Grow(TownshipData _data, List<StreetTile> baseTiles, List<StreetTile> finalTiles, bool _cancelIfInvalidNeighbor)
		{
			finalTiles.Clear();
			Vector2i[] array = ((_data.OutskirtDistrictPercent < 1f) ? this.dir4way : this.dir8way);
			foreach (StreetTile streetTile in baseTiles)
			{
				foreach (Vector2i vector2i in array)
				{
					StreetTile streetTileGrid = this.worldBuilder.GetStreetTileGrid(streetTile.GridPosition + vector2i);
					if (streetTileGrid != null && !baseTiles.Contains(streetTileGrid) && !finalTiles.Contains(streetTileGrid))
					{
						if (!streetTileGrid.IsValidForStreetTile)
						{
							if (_cancelIfInvalidNeighbor)
							{
								return false;
							}
						}
						else
						{
							StreetTile[] neighbors = streetTileGrid.GetNeighbors();
							for (int j = 0; j < neighbors.Length; j++)
							{
								if (neighbors[j].Township != null)
								{
									return false;
								}
							}
							finalTiles.Add(streetTileGrid);
						}
					}
				}
			}
			if (_data.OutskirtDistrictPercent < 1f)
			{
				float num = 1f - _data.OutskirtDistrictPercent;
				float num2 = 0f;
				for (int k = finalTiles.Count - 1; k >= 0; k--)
				{
					num2 += num;
					if (num2 >= 1f)
					{
						num2 -= 1f;
						finalTiles.RemoveAt(k);
					}
				}
			}
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int getTownshipCounts(DynamicProperties _properties, Dictionary<int, TownPlanner.TownshipSpawnInfo> townshipSpawnInfo)
		{
			int num = 0;
			GameRandom gameRandom = Rand.Instance.gameRandom;
			foreach (KeyValuePair<int, TownshipData> keyValuePair in WorldBuilderStatic.idToTownshipData)
			{
				TownshipData value = keyValuePair.Value;
				if (value.Category != TownshipData.eCategory.Wilderness)
				{
					string text = value.Name.ToLower();
					float num2 = 1f;
					float num3 = 1f;
					_properties.ParseVec(text, "tiles", ref num2, ref num3);
					int count = this.worldBuilder.GetCount(text, this.worldBuilder.Towns, gameRandom);
					int num4 = 5;
					_properties.ParseInt(text, "distance", ref num4);
					if (count >= 1 && num2 >= 1f && num3 >= 1f)
					{
						townshipSpawnInfo.Add(value.Id, new TownPlanner.TownshipSpawnInfo((int)num2, (int)num3, count, num4));
					}
					num += count;
				}
			}
			return num;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public BiomeType nextBiomeType(BiomeType _current, TownshipData _townshipData)
		{
			int num = (int)_current;
			for (int i = 0; i < 4; i++)
			{
				num = (num + 1) % 5;
				if (_townshipData.Biomes.IsEmpty || _townshipData.Biomes.Test_Bit(this.worldBuilder.biomeTagBits[num]))
				{
					return (BiomeType)num;
				}
			}
			return BiomeType.none;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public BiomeType getBiomeWithMostAvailableSpace(TownshipData _townshipData)
		{
			int num = 0;
			int num2 = 255;
			List<Vector2i> list = new List<Vector2i>();
			for (int i = 0; i < 5; i++)
			{
				if (_townshipData.Biomes.IsEmpty || _townshipData.Biomes.Test_Bit(this.worldBuilder.biomeTagBits[i]))
				{
					this.getStreetTilesForBiome((BiomeType)i, list);
					if (list.Count > num)
					{
						num = list.Count;
						num2 = i;
					}
				}
			}
			return (BiomeType)num2;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<BiomeType, List<Vector2i>> getStreetTilesByBiome()
		{
			Dictionary<BiomeType, List<Vector2i>> dictionary = new Dictionary<BiomeType, List<Vector2i>>();
			for (int i = 0; i < 5; i++)
			{
				List<Vector2i> list = new List<Vector2i>();
				this.getStreetTilesForBiome((BiomeType)i, list);
				if (list.Count > 0)
				{
					dictionary.Add((BiomeType)i, list);
				}
			}
			return dictionary;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void getStreetTilesForBiome(BiomeType _biomeType, List<Vector2i> _biomeStreetTiles)
		{
			_biomeStreetTiles.Clear();
			foreach (StreetTile streetTile in this.worldBuilder.StreetTileMap)
			{
				if (streetTile.IsValidForStreetTile && streetTile.Township == null && !streetTile.Used && streetTile.BiomeType == _biomeType)
				{
					_biomeStreetTiles.Add(streetTile.GridPosition);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void GetStreetLayout(Vector2i startPosition, int townSize, GameRandom rnd, bool isRoadside, List<StreetTile> townTiles)
		{
			townTiles.Clear();
			StreetTile streetTileGrid = this.worldBuilder.GetStreetTileGrid(startPosition);
			if (!isRoadside)
			{
				using (List<Township>.Enumerator enumerator = this.worldBuilder.Townships.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Township township = enumerator.Current;
						if (streetTileGrid.Area.Overlaps(township.BufferArea))
						{
							return;
						}
					}
					goto IL_0103;
				}
			}
			if (streetTileGrid.Township != null || streetTileGrid.District != null)
			{
				return;
			}
			foreach (Township township2 in this.worldBuilder.Townships)
			{
				if (streetTileGrid.Area.Overlaps(township2.BufferArea))
				{
					return;
				}
			}
			foreach (StreetTile streetTile in streetTileGrid.GetNeighbors8Way())
			{
				if (streetTile.Township != null || streetTile.District != null)
				{
					return;
				}
			}
			IL_0103:
			if (townSize == 1)
			{
				townTiles.Add(streetTileGrid);
				return;
			}
			List<StreetTile> list = new List<StreetTile>();
			if (townSize <= 16)
			{
				townTiles.Add(streetTileGrid);
				list.Add(streetTileGrid);
			}
			else
			{
				int num = (int)Mathf.Sqrt((float)townSize) - 1;
				int num2 = num / -2;
				int num3 = num / 2 - 1;
				for (int j = num2; j <= num3; j++)
				{
					Vector2i vector2i;
					vector2i.y = startPosition.y + j;
					bool flag = j == num2 || j == num3;
					for (int k = num2; k <= num3; k++)
					{
						vector2i.x = startPosition.x + k;
						StreetTile streetTile2 = this.StreetLayoutCheckPos(vector2i);
						if (streetTile2 == null)
						{
							townTiles.Clear();
							return;
						}
						townTiles.Add(streetTile2);
						if (flag || k == num2 || k == num3)
						{
							list.Add(streetTile2);
						}
					}
				}
				if (townTiles.Count >= townSize)
				{
					return;
				}
			}
			while (list.Count > 0)
			{
				int num4 = rnd.RandomRange(0, list.Count);
				StreetTile streetTile3 = list[num4];
				list.RemoveAt(num4);
				int num5 = rnd.RandomRange(4);
				for (int l = 0; l < 4; l++)
				{
					Vector2i vector2i2 = streetTile3.GridPosition + this.dir4way[(l + num5) & 3];
					StreetTile streetTile4 = this.StreetLayoutCheckPos(vector2i2);
					if (streetTile4 != null && !townTiles.Contains(streetTile4))
					{
						townTiles.Add(streetTile4);
						if (townTiles.Count >= townSize)
						{
							return;
						}
						list.Add(streetTile4);
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public StreetTile StreetLayoutCheckPos(Vector2i _pos)
		{
			int num = 2;
			if (_pos.x < num || _pos.x >= this.worldBuilder.StreetTileMapWidth - num || _pos.y < num || _pos.y >= this.worldBuilder.StreetTileMapWidth - num)
			{
				return null;
			}
			StreetTile streetTileGrid = this.worldBuilder.GetStreetTileGrid(_pos);
			if (streetTileGrid != null && streetTileGrid.IsValidForStreetTile)
			{
				foreach (Township township in this.worldBuilder.Townships)
				{
					if (streetTileGrid.Area.Overlaps(township.BufferArea))
					{
						return null;
					}
				}
				return streetTileGrid;
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cTriesPerTownshipSpawnInfo = 80;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cTriesSmaller = 30;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<BiomeType, TownPlanner.BiomeStats> biomeStats = new Dictionary<BiomeType, TownPlanner.BiomeStats>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Vector2i[] dir4way = new Vector2i[]
		{
			Vector2i.up,
			Vector2i.right,
			Vector2i.down,
			Vector2i.left
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Vector2i[] dir8way = new Vector2i[]
		{
			Vector2i.up,
			Vector2i.up + Vector2i.right,
			Vector2i.right,
			Vector2i.right + Vector2i.down,
			Vector2i.down,
			Vector2i.down + Vector2i.left,
			Vector2i.left,
			Vector2i.left + Vector2i.up
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public class BiomeStats
		{
			public int townshipCount;

			public Dictionary<string, int> counts = new Dictionary<string, int>();
		}

		public class TownshipSpawnInfo
		{
			public TownshipSpawnInfo(int _min, int _max, int _count, int _distance)
			{
				this.min = _min;
				this.max = _max;
				this.count = _count;
				this.distance = _distance;
			}

			public int min;

			public int max;

			public int count;

			public int distance;
		}
	}
}
