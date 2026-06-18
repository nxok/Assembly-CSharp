using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	[BurstCompile(CompileSynchronously = true)]
	public class PathingUtils
	{
		public PathingUtils(WorldBuilder _worldBuilder)
		{
			PathingUtils.worldBuilder = _worldBuilder;
			this.data.nodePool = new PathNodePool(100000);
		}

		public int GetPathCost(Vector2i start, Vector2i end, bool isCountryRoad = false)
		{
			this.InitPathData();
			int num = 0;
			WorldBuilder worldBuilder = PathingUtils.worldBuilder;
			Vector2i vector2i = start / 10;
			Vector2i vector2i2 = end / 10;
			PathNode pathNode;
			for (int i = PathingUtils.FindDetailedPath(ref worldBuilder.data, ref this.data, in vector2i, in vector2i2, isCountryRoad, false); i >= 0; i = pathNode.pathNext)
			{
				this.data.nodePool.Node(i, out pathNode);
				num++;
			}
			this.data.nodePool.ReturnAll();
			return num;
		}

		public NativeList<Vector2i> GetPath(Vector2i _start, Vector2i _end, bool _isCountryRoad, bool _isRiver = false)
		{
			this.InitPathData();
			WorldBuilder worldBuilder = PathingUtils.worldBuilder;
			Vector2i vector2i = _start / 10;
			Vector2i vector2i2 = _end / 10;
			int i = PathingUtils.FindDetailedPath(ref worldBuilder.data, ref this.data, in vector2i, in vector2i2, _isCountryRoad, _isRiver);
			this.pathTemp.Clear();
			while (i >= 0)
			{
				PathNode pathNode;
				this.data.nodePool.Node(i, out pathNode);
				Vector2i vector2i3 = pathNode.position * 10 + PathingUtils.stepHalf;
				this.pathTemp.Add(ref vector2i3);
				i = pathNode.pathNext;
			}
			this.data.nodePool.ReturnAll();
			return this.pathTemp;
		}

		public Vector2i GetPathPoint(Vector2i _start, ref NativeList<Vector2> _endPath, bool _isCountryRoad, bool _isRiver, out int _cost)
		{
			this.InitPathData();
			this.ConvertPathToTemp(ref _endPath);
			Vector2i vector2i = Vector2i.min;
			int num = 0;
			WorldBuilder worldBuilder = PathingUtils.worldBuilder;
			Vector2i vector2i2 = _start / 10;
			int i = PathingUtils.FindDetailedPath(ref worldBuilder.data, ref this.data, in vector2i2, in this.pathTemp, _isCountryRoad, _isRiver);
			if (i >= 0)
			{
				PathNode pathNode;
				this.data.nodePool.Node(i, out pathNode);
				vector2i = pathNode.position * 10 + PathingUtils.stepHalf;
				Vector2 vector = vector2i.AsVector2();
				Vector2 vector2;
				if (PathingUtils.FindClosestPathPoint(in _endPath, in vector, out vector2, 1) < 400f)
				{
					vector2i = new Vector2i(vector2);
				}
				while (i >= 0)
				{
					this.data.nodePool.Node(i, out pathNode);
					num++;
					i = pathNode.pathNext;
				}
			}
			this.data.nodePool.ReturnAll();
			_cost = num;
			return vector2i;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void InitPathData()
		{
			this.data.WorldSize = PathingUtils.worldBuilder.WorldSize;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ConvertPathToTemp(ref NativeList<Vector2> _path)
		{
			this.pathTemp.Clear();
			for (int i = 0; i < _path.Length; i++)
			{
				Vector2i vector2i;
				vector2i.x = ((int)_path[i].x + PathingUtils.stepHalf.x) / 10;
				vector2i.y = ((int)_path[i].y + PathingUtils.stepHalf.y) / 10;
				this.pathTemp.Add(ref vector2i);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe static void ClosedListInit(ref PathingUtils.Data data)
		{
			int num = data.WorldSize / 10 + 1;
			if (data.closedList.IsCreated)
			{
				int num2 = data.closedListMinY * data.closedListWidth;
				int num3 = (data.closedListMaxY + 1) * data.closedListWidth;
				void* ptr = (void*)((byte*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks<byte>(data.closedList) + num2);
				UnsafeUtility.MemSet(ptr, 0, (long)(num3 - num2));
			}
			if (!data.closedList.IsCreated || data.closedListWidth != num)
			{
				data.closedListWidth = num;
				data.closedList = new NativeArray<byte>(num * num, 4, 1);
			}
		}

		[BurstCompile(CompileSynchronously = true)]
		[PublicizedFrom(EAccessModifier.Private)]
		public static int FindDetailedPath(ref WorldBuilder.Data wd, ref PathingUtils.Data data, in Vector2i startPos, in Vector2i endPos, bool _isCountryRoad, bool _isRiver)
		{
			return PathingUtils.FindDetailedPath_0000B5ED$BurstDirectCall.Invoke(ref wd, ref data, in startPos, in endPos, _isCountryRoad, _isRiver);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float DiagonalDist(Vector2i v1, Vector2i v2)
		{
			float num = Utils.FastAbs((float)(v1.x - v2.x));
			float num2 = Utils.FastAbs((float)(v1.y - v2.y));
			return num + num2 + -0.585786f * Utils.FastMin(num, num2);
		}

		[BurstCompile(CompileSynchronously = true)]
		[PublicizedFrom(EAccessModifier.Private)]
		public static int FindDetailedPath(ref WorldBuilder.Data wd, ref PathingUtils.Data data, in Vector2i startPos, in NativeList<Vector2i> _endPath, bool _isCountryRoad, bool _isRiver)
		{
			return PathingUtils.FindDetailedPath_0000B5EF$BurstDirectCall.Invoke(ref wd, ref data, in startPos, in _endPath, _isCountryRoad, _isRiver);
		}

		[BurstCompile(CompileSynchronously = true)]
		[PublicizedFrom(EAccessModifier.Private)]
		public static void CalcPathBounds(in NativeList<Vector2i> _path, out Vector2i _min, out Vector2i _max)
		{
			PathingUtils.CalcPathBounds_0000B5F0$BurstDirectCall.Invoke(in _path, out _min, out _max);
		}

		[BurstCompile(CompileSynchronously = true)]
		public static float FindClosestPathPoint(in NativeList<Vector2> _path, in Vector2 _startPos, out Vector2 _destPoint, int _step = 1)
		{
			return PathingUtils.FindClosestPathPoint_0000B5F1$BurstDirectCall.Invoke(in _path, in _startPos, out _destPoint, _step);
		}

		[BurstCompile(CompileSynchronously = true)]
		public static float FindClosestPathPoint(in NativeList<Vector2i> _path, in Vector2i _startPos, out Vector2i _destPoint)
		{
			return PathingUtils.FindClosestPathPoint_0000B5F2$BurstDirectCall.Invoke(in _path, in _startPos, out _destPoint);
		}

		[BurstCompile(CompileSynchronously = true)]
		public static bool IsPointOnPath(in NativeList<Vector2i> _path, in Vector2i _point)
		{
			return PathingUtils.IsPointOnPath_0000B5F3$BurstDirectCall.Invoke(in _path, in _point);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool IsBlocked(ref WorldBuilder.Data wd, int pathX, int pathY, bool isRiver = false)
		{
			Vector2i vector2i = PathingUtils.pathPositionToWorldCenter(pathX, pathY);
			return !wd.InWorldBounds(vector2i.x, vector2i.y) || wd.GetStreetTileDataWorld(vector2i.x, vector2i.y).IsCity || (!isRiver && PathingUtils.IsWater(ref wd, pathX, pathY));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool InBounds(ref WorldBuilder.Data wd, Vector2i pos)
		{
			return PathingUtils.InBounds(ref wd, pos.x, pos.y);
		}

		public static bool InBounds(ref WorldBuilder.Data wd, int pathX, int pathY)
		{
			Vector2i vector2i = PathingUtils.pathPositionToWorldCenter(pathX, pathY);
			return (ulong)vector2i.x < (ulong)((long)wd.WorldSize) && (ulong)vector2i.y < (ulong)((long)wd.WorldSize);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool IsWater(ref WorldBuilder.Data wd, int pathX, int pathY)
		{
			Vector2i vector2i = PathingUtils.pathPositionToWorldMin(pathX, pathY);
			if (wd.GetStreetTileDataWorld(vector2i.x, vector2i.y).OverlapsWater)
			{
				for (int i = vector2i.y; i < vector2i.y + 10; i++)
				{
					for (int j = vector2i.x; j < vector2i.x + 10; j++)
					{
						if (wd.GetWater(j, i) > 0)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetHeight(ref WorldBuilder.Data wd, Vector2i pos)
		{
			return PathingUtils.GetHeight(ref wd, pos.x, pos.y);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float GetHeight(ref WorldBuilder.Data wd, int pathX, int pathY)
		{
			return wd.GetHeight(PathingUtils.pathPositionToWorldCenter(pathX, pathY));
		}

		public BiomeType GetBiome(Vector2i pos)
		{
			return this.GetBiome(pos.x, pos.y);
		}

		public BiomeType GetBiome(int pathX, int pathY)
		{
			return PathingUtils.worldBuilder.GetBiome(PathingUtils.pathPositionToWorldCenter(pathX, pathY));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2i pathPositionToWorldCenter(int pathX, int pathY)
		{
			Vector2i vector2i;
			vector2i.x = pathX * 10 + 5;
			vector2i.y = pathY * 10 + 5;
			return vector2i;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2i pathPositionToWorldMin(int pathX, int pathY)
		{
			Vector2i vector2i;
			vector2i.x = pathX * 10;
			vector2i.y = pathY * 10;
			return vector2i;
		}

		public void AddMoveLimitArea(Rect r)
		{
			int num = (int)r.xMin;
			int num2 = (int)r.yMin;
			num /= 10;
			num2 /= 10;
			for (int i = 0; i < 15; i++)
			{
				for (int j = 0; j < 15; j++)
				{
					if (j != 7 && i != 7)
					{
						this.SetPathBlocked(num + j, num2 + i, true);
					}
				}
			}
		}

		public void RemoveFullyBlockedArea(Rect r)
		{
			int num = (int)r.xMin;
			int num2 = (int)r.yMin;
			num /= 10;
			num2 /= 10;
			for (int i = 0; i < 15; i++)
			{
				for (int j = 0; j < 15; j++)
				{
					this.SetPathBlocked(num + j, num2 + i, false);
				}
			}
		}

		public void AddFullyBlockedArea(Rect r)
		{
			int num = (int)(r.xMin + 0.5f);
			int num2 = (int)(r.yMin + 0.5f);
			num /= 10;
			num2 /= 10;
			for (int i = 0; i < 15; i++)
			{
				for (int j = 0; j < 15; j++)
				{
					this.SetPathBlocked(num + j, num2 + i, true);
				}
			}
		}

		public void SetPathBlocked(Vector2i pos, bool isBlocked)
		{
			this.SetPathBlocked(pos.x, pos.y, isBlocked);
		}

		public void SetPathBlocked(int x, int y, bool isBlocked)
		{
			this.SetPathBlocked(x, y, isBlocked ? sbyte.MinValue : 0);
		}

		public void SetPathBlocked(int x, int y, sbyte costMult)
		{
			if (!this.data.pathingGrid.IsCreated)
			{
				this.SetupPathingGrid();
			}
			if ((ulong)x >= (ulong)((long)this.data.pathingGridSize) || (ulong)y >= (ulong)((long)this.data.pathingGridSize))
			{
				return;
			}
			this.data.pathingGrid[x + y * this.data.pathingGridSize] = costMult;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsPathBlocked(ref PathingUtils.Data data, int x, int y)
		{
			return (ulong)x >= (ulong)((long)data.pathingGridSize) || (ulong)y >= (ulong)((long)data.pathingGridSize) || data.pathingGrid[x + y * data.pathingGridSize] == sbyte.MinValue;
		}

		public bool IsPointOnHighwayWorld(int x, int y)
		{
			int num = x / 10 + y / 10 * PathingUtils.worldBuilder.data.PathTileGridWidth;
			return PathingUtils.worldBuilder.data.PathTileGrid[num].TileState == PathTile.PathTileStates.Highway;
		}

		public void SetupPathingGrid()
		{
			this.data.pathingGridSize = PathingUtils.worldBuilder.WorldSize / 10;
			this.data.pathingGrid = new NativeArray<sbyte>(this.data.pathingGridSize * this.data.pathingGridSize, 4, 1);
		}

		public void Cleanup()
		{
			this.data.pathingGrid.Dispose();
			this.data.pathingGridSize = 0;
			this.data.nodePool.Cleanup();
			this.data.minHeapBinned.Cleanup();
			this.data.closedList.Dispose();
			this.pathTemp.Dispose();
		}

		[BurstCompile(CompileSynchronously = true)]
		[PublicizedFrom(EAccessModifier.Internal)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FindDetailedPath$BurstManaged(ref WorldBuilder.Data wd, ref PathingUtils.Data data, in Vector2i startPos, in Vector2i endPos, bool _isCountryRoad, bool _isRiver)
		{
			if (!PathingUtils.InBounds(ref wd, startPos) || !PathingUtils.InBounds(ref wd, endPos))
			{
				return -1;
			}
			PathingUtils.ClosedListInit(ref data);
			data.closedList[startPos.x + startPos.y * data.closedListWidth] = 1;
			data.closedListMinY = startPos.y;
			data.closedListMaxY = startPos.y;
			data.minHeapBinned.Init();
			int num = data.nodePool.Alloc();
			data.nodePool.Node(num).Set(startPos, 0f, 0f, -1);
			data.minHeapBinned.Add(ref data, num);
			Vector2i vector2i = new Vector2i(Utils.FastMin(startPos.x, endPos.x), Utils.FastMin(startPos.y, endPos.y));
			Vector2i vector2i2 = new Vector2i(Utils.FastMax(startPos.x, endPos.x), Utils.FastMax(startPos.y, endPos.y));
			int num2 = Utils.FastMax(0, vector2i.x - 200);
			int num3 = Utils.FastMax(0, vector2i.y - 200);
			int num4 = Utils.FastMin(vector2i2.x + 200, data.closedListWidth - 1);
			int num5 = Utils.FastMin(vector2i2.y + 200, data.closedListWidth - 1);
			float num6 = (_isCountryRoad ? 12f : 11f);
			int num7 = 20000;
			int num8;
			while ((num8 = data.minHeapBinned.ExtractFirst(ref data)) >= 0 && --num7 >= 0)
			{
				ref PathNode ptr = ref data.nodePool.Node(num8);
				Vector2i position = ptr.position;
				if (position == endPos)
				{
					return num8;
				}
				for (int i = 0; i < 8; i++)
				{
					Vector2i vector2i3 = PathingUtils.neighborOffset8Ways[i];
					Vector2i vector2i4 = ptr.position + vector2i3;
					if (vector2i4.x >= num2 && vector2i4.y >= num3 && vector2i4.x < num4 && vector2i4.y < num5)
					{
						int num9 = vector2i4.x + vector2i4.y * data.closedListWidth;
						if (data.closedList[num9] == 0)
						{
							bool flag = vector2i4 == endPos;
							if (!flag)
							{
								bool flag2 = PathingUtils.IsPathBlocked(ref data, vector2i4.x, vector2i4.y);
								if (!flag2)
								{
									flag2 = PathingUtils.IsBlocked(ref wd, vector2i4.x, vector2i4.y, _isRiver);
								}
								if (flag2)
								{
									data.closedList[num9] = 1;
									data.closedListMinY = Utils.FastMin(data.closedListMinY, vector2i4.y);
									data.closedListMaxY = Utils.FastMax(data.closedListMaxY, vector2i4.y);
									goto IL_054C;
								}
							}
							float num10 = Utils.FastAbs(PathingUtils.GetHeight(ref wd, position) - PathingUtils.GetHeight(ref wd, vector2i4));
							if (num10 <= num6)
							{
								float num11 = PathingUtils.DiagonalDist(vector2i4, endPos);
								num11 *= 1.4f;
								bool flag3 = vector2i3.x != 0 && vector2i3.y != 0;
								bool flag4 = true;
								if (!_isCountryRoad)
								{
									float num12 = Vector2i.DistanceSqr(vector2i4, startPos);
									float num13 = Vector2i.DistanceSqr(vector2i4, endPos);
									if (num12 <= 8.410001f || num13 <= 8.410001f)
									{
										flag4 = false;
									}
									Vector2i vector2i5 = vector2i4 * 10;
									if (wd.GetStreetTileDataWorld(vector2i5.x, vector2i5.y).ConnectedHighwayCount >= 3)
									{
										goto IL_054C;
									}
									if (!flag)
									{
										bool flag5 = wd.PathTileGrid[vector2i4.x + vector2i4.y * wd.PathTileGridWidth].TileState == PathTile.PathTileStates.Highway;
										if (flag5)
										{
											goto IL_054C;
										}
										if (flag3)
										{
											for (int j = 0; j < 2; j++)
											{
												int num14 = ptr.position.x;
												int num15 = ptr.position.y;
												if (j == 0)
												{
													num14 += vector2i3.x;
												}
												else
												{
													num15 += vector2i3.y;
												}
												flag5 = PathingUtils.IsPathBlocked(ref data, num14, num15);
												if (flag5)
												{
													break;
												}
												flag5 = PathingUtils.IsBlocked(ref wd, num14, num15, false);
												if (flag5)
												{
													break;
												}
												if (wd.PathTileGrid[num14 + num15 * wd.PathTileGridWidth].TileState == PathTile.PathTileStates.Highway)
												{
													flag5 = true;
													break;
												}
											}
											if (flag5)
											{
												goto IL_054C;
											}
										}
									}
									num10 = Utils.FastMax(0f, num10 - 0.5f);
									num10 *= 3f;
								}
								num10 *= 0.2f;
								float num16 = 1f;
								if (flag3)
								{
									num16 += 0.414214f;
								}
								if (flag4 && data.pathingGrid.IsCreated)
								{
									int num17 = (int)data.pathingGrid[vector2i4.x + vector2i4.y * data.pathingGridSize];
									if (num17 > 0)
									{
										num16 *= (float)num17;
									}
								}
								data.closedList[num9] = 1;
								data.closedListMinY = Utils.FastMin(data.closedListMinY, vector2i4.y);
								data.closedListMaxY = Utils.FastMax(data.closedListMaxY, vector2i4.y);
								int num18 = data.nodePool.Alloc();
								ref PathNode ptr2 = ref data.nodePool.Node(num18);
								float num19 = ptr.travelledCost + num16 + num10;
								float num20 = num19 + num11;
								ptr2.Set(vector2i4, num19, num20, num8);
								data.minHeapBinned.Add(ref data, num18);
							}
						}
					}
					IL_054C:;
				}
			}
			return -1;
		}

		[BurstCompile(CompileSynchronously = true)]
		[PublicizedFrom(EAccessModifier.Internal)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FindDetailedPath$BurstManaged(ref WorldBuilder.Data wd, ref PathingUtils.Data data, in Vector2i startPos, in NativeList<Vector2i> _endPath, bool _isCountryRoad, bool _isRiver)
		{
			PathingUtils.ClosedListInit(ref data);
			data.closedList[startPos.x + startPos.y * data.closedListWidth] = 1;
			data.closedListMinY = startPos.y;
			data.closedListMaxY = startPos.y;
			data.minHeapBinned.Init();
			int num = data.nodePool.Alloc();
			data.nodePool.Node(num).Set(startPos, 0f, 0f, -1);
			data.minHeapBinned.Add(ref data, num);
			Vector2i vector2i;
			Vector2i vector2i2;
			PathingUtils.CalcPathBounds(in _endPath, out vector2i, out vector2i2);
			int num2 = Utils.FastMax(0, vector2i.x - 200);
			int num3 = Utils.FastMax(0, vector2i.y - 200);
			int num4 = Utils.FastMin(vector2i2.x + 200, data.closedListWidth - 1);
			int num5 = Utils.FastMin(vector2i2.y + 200, data.closedListWidth - 1);
			float num6 = (_isCountryRoad ? 12f : 11f);
			int num7 = 20000;
			int num8;
			while ((num8 = data.minHeapBinned.ExtractFirst(ref data)) >= 0 && --num7 >= 0)
			{
				ref PathNode ptr = ref data.nodePool.Node(num8);
				Vector2i position = ptr.position;
				if (PathingUtils.IsPointOnPath(in _endPath, in position))
				{
					return num8;
				}
				for (int i = 0; i < 8; i++)
				{
					Vector2i vector2i3 = PathingUtils.neighborOffset8Ways[i];
					Vector2i vector2i4 = ptr.position + vector2i3;
					if (vector2i4.x >= num2 && vector2i4.y >= num3 && vector2i4.x < num4 && vector2i4.y < num5)
					{
						int num9 = vector2i4.x + vector2i4.y * data.closedListWidth;
						if (data.closedList[num9] == 0)
						{
							Vector2i vector2i5;
							PathingUtils.FindClosestPathPoint(in _endPath, in vector2i4, out vector2i5);
							if (vector2i4 != vector2i5)
							{
								bool flag = PathingUtils.IsPathBlocked(ref data, vector2i4.x, vector2i4.y);
								if (!flag)
								{
									flag = PathingUtils.IsBlocked(ref wd, vector2i4.x, vector2i4.y, _isRiver);
								}
								if (flag)
								{
									data.closedList[num9] = 1;
									data.closedListMinY = Utils.FastMin(data.closedListMinY, vector2i4.y);
									data.closedListMaxY = Utils.FastMax(data.closedListMaxY, vector2i4.y);
									goto IL_037A;
								}
							}
							float num10 = Utils.FastAbs(PathingUtils.GetHeight(ref wd, position) - PathingUtils.GetHeight(ref wd, vector2i4));
							if (num10 <= num6)
							{
								float num11 = Vector2i.Distance(vector2i4, vector2i5);
								num11 *= 1.4f;
								num10 *= 0.2f;
								num10 += 1f;
								if (vector2i3.x != 0 && vector2i3.y != 0)
								{
									num10 += 0.414214f;
								}
								if (data.pathingGrid.IsCreated)
								{
									int num12 = (int)data.pathingGrid[vector2i4.x + vector2i4.y * data.pathingGridSize];
									if (num12 > 0)
									{
										num10 *= (float)num12;
									}
								}
								data.closedList[num9] = 1;
								data.closedListMinY = Utils.FastMin(data.closedListMinY, vector2i4.y);
								data.closedListMaxY = Utils.FastMax(data.closedListMaxY, vector2i4.y);
								int num13 = data.nodePool.Alloc();
								ref PathNode ptr2 = ref data.nodePool.Node(num13);
								float num14 = ptr.travelledCost + num10;
								float num15 = num14 + num11;
								ptr2.Set(vector2i4, num14, num15, num8);
								data.minHeapBinned.Add(ref data, num13);
							}
						}
					}
					IL_037A:;
				}
			}
			return -1;
		}

		[BurstCompile(CompileSynchronously = true)]
		[PublicizedFrom(EAccessModifier.Internal)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CalcPathBounds$BurstManaged(in NativeList<Vector2i> _path, out Vector2i _min, out Vector2i _max)
		{
			_min = Vector2i.max;
			_max = Vector2i.min;
			NativeList<Vector2i> nativeList = _path;
			foreach (Vector2i vector2i in nativeList)
			{
				_min.x = Utils.FastMin(_min.x, vector2i.x);
				_min.y = Utils.FastMin(_min.y, vector2i.y);
				_max.x = Utils.FastMax(_max.x, vector2i.x);
				_max.y = Utils.FastMax(_max.y, vector2i.y);
			}
		}

		[BurstCompile(CompileSynchronously = true)]
		[PublicizedFrom(EAccessModifier.Internal)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float FindClosestPathPoint$BurstManaged(in NativeList<Vector2> _path, in Vector2 _startPos, out Vector2 _destPoint, int _step = 1)
		{
			_destPoint = Vector2.zero;
			float num = float.MaxValue;
			int length = _path.Length;
			for (int i = 0; i < length; i += _step)
			{
				NativeList<Vector2> nativeList = _path;
				Vector2 vector = nativeList[i];
				float sqrMagnitude = (_startPos - vector).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					_destPoint = vector;
				}
			}
			return num;
		}

		[BurstCompile(CompileSynchronously = true)]
		[PublicizedFrom(EAccessModifier.Internal)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float FindClosestPathPoint$BurstManaged(in NativeList<Vector2i> _path, in Vector2i _startPos, out Vector2i _destPoint)
		{
			_destPoint = Vector2i.zero;
			float num = float.MaxValue;
			NativeList<Vector2i> nativeList = _path;
			foreach (Vector2i vector2i in nativeList)
			{
				float num2 = Vector2i.DistanceSqr(_startPos, vector2i);
				if (num2 < num)
				{
					num = num2;
					_destPoint = vector2i;
				}
			}
			return num;
		}

		[BurstCompile(CompileSynchronously = true)]
		[PublicizedFrom(EAccessModifier.Internal)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsPointOnPath$BurstManaged(in NativeList<Vector2i> _path, in Vector2i _point)
		{
			NativeList<Vector2i> nativeList = _path;
			foreach (Vector2i vector2i in nativeList)
			{
				if (vector2i.x == _point.x && vector2i.y == _point.y)
				{
					return true;
				}
			}
			return false;
		}

		public const int PATHING_GRID_TILE_SIZE = 10;

		public const int stepSize = 10;

		public static readonly Vector2i stepHalf = new Vector2i(5, 5);

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cRoadCountryMaxStepH = 12f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cRoadHighwayMaxStepH = 11f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cHeightCostScale = 0.2f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cNeighborsCount = 8;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly Vector2i[] neighborOffset8Ways = new Vector2i[]
		{
			new Vector2i(0, 1),
			new Vector2i(1, 1),
			new Vector2i(1, 0),
			new Vector2i(1, -1),
			new Vector2i(0, -1),
			new Vector2i(-1, -1),
			new Vector2i(-1, 0),
			new Vector2i(-1, 1)
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public static WorldBuilder worldBuilder;

		[PublicizedFrom(EAccessModifier.Private)]
		public NativeList<Vector2i> pathTemp = new NativeList<Vector2i>(200, 4);

		[PublicizedFrom(EAccessModifier.Private)]
		public PathingUtils.Data data;

		public struct Data
		{
			public int WorldSize;

			public NativeArray<sbyte> pathingGrid;

			public int pathingGridSize;

			public PathNodePool nodePool;

			public PathingUtils.MinHeapBinned minHeapBinned;

			public NativeArray<byte> closedList;

			public int closedListWidth;

			public int closedListMinY;

			public int closedListMaxY;
		}

		public struct MinHeapBinned
		{
			public void Init()
			{
				if (!this.nodeBins.IsCreated)
				{
					this.nodeBins = new NativeArray<int>(32768, 4, 0);
					this.lowBin = 0;
					this.highBin = 32767;
				}
				this.Reset();
			}

			public unsafe void Reset()
			{
				if (this.lowBin <= this.highBin)
				{
					int num = UnsafeUtility.SizeOf<int>();
					void* ptr = (void*)((byte*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks<int>(this.nodeBins) + this.lowBin * num);
					UnsafeUtility.MemSet(ptr, byte.MaxValue, (long)((this.highBin - this.lowBin + 1) * num));
				}
				this.lowBin = 32768;
				this.highBin = 0;
			}

			public int ExtractFirst(ref PathingUtils.Data data)
			{
				if (this.lowBin <= this.highBin)
				{
					int num = this.nodeBins[this.lowBin];
					PathNode pathNode;
					data.nodePool.Node(num, out pathNode);
					this.nodeBins[this.lowBin] = pathNode.listNext;
					if (pathNode.listNext < 0)
					{
						int num2;
						do
						{
							num2 = this.lowBin + 1;
							this.lowBin = num2;
						}
						while (num2 <= this.highBin && this.nodeBins[this.lowBin] < 0);
						if (this.lowBin > this.highBin)
						{
							this.lowBin = 32768;
							this.highBin = 0;
						}
					}
					return num;
				}
				return -1;
			}

			public void Add(ref PathingUtils.Data data, int _nodeIndex)
			{
				ref PathNode ptr = ref data.nodePool.Node(_nodeIndex);
				int num = (int)(ptr.totalCost * 0.07f);
				if (num >= 32768)
				{
					num = 32767;
				}
				if (num < this.lowBin)
				{
					this.lowBin = num;
				}
				if (num > this.highBin)
				{
					this.highBin = num;
				}
				int num2 = this.nodeBins[num];
				if (num2 < 0)
				{
					ptr.listNext = -1;
					this.nodeBins[num] = _nodeIndex;
					return;
				}
				ref PathNode ptr2 = ref data.nodePool.Node(num2);
				if (ptr.totalCost <= ptr2.totalCost)
				{
					ptr.listNext = num2;
					this.nodeBins[num] = _nodeIndex;
					return;
				}
				ref PathNode ptr3 = ref ptr2;
				int i;
				for (i = ptr3.listNext; i >= 0; i = ptr3.listNext)
				{
					ref PathNode ptr4 = ref data.nodePool.Node(i);
					if (ptr.totalCost <= ptr4.totalCost)
					{
						break;
					}
					ptr3 = ref ptr4;
				}
				ptr.listNext = i;
				ptr3.listNext = _nodeIndex;
			}

			public void Cleanup()
			{
				this.nodeBins.Dispose();
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public const int cBins = 32768;

			[PublicizedFrom(EAccessModifier.Private)]
			public const float cScale = 0.07f;

			[PublicizedFrom(EAccessModifier.Private)]
			public NativeArray<int> nodeBins;

			[PublicizedFrom(EAccessModifier.Private)]
			public int lowBin;

			[PublicizedFrom(EAccessModifier.Private)]
			public int highBin;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public enum PathNodeType
		{
			Free,
			Road,
			Prefab,
			CityLimits = 4,
			Blocked = 8
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public delegate int FindDetailedPath_0000B5ED$PostfixBurstDelegate(ref WorldBuilder.Data wd, ref PathingUtils.Data data, in Vector2i startPos, in Vector2i endPos, bool _isCountryRoad, bool _isRiver);

		[PublicizedFrom(EAccessModifier.Internal)]
		public static class FindDetailedPath_0000B5ED$BurstDirectCall
		{
			[BurstDiscard]
			[PublicizedFrom(EAccessModifier.Private)]
			public static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (PathingUtils.FindDetailedPath_0000B5ED$BurstDirectCall.Pointer == 0)
				{
					PathingUtils.FindDetailedPath_0000B5ED$BurstDirectCall.Pointer = BurstCompiler.GetILPPMethodFunctionPointer2(PathingUtils.FindDetailedPath_0000B5ED$BurstDirectCall.DeferredCompilation, methodof(PathingUtils.FindDetailedPath$BurstManaged(ref WorldBuilder.Data, ref PathingUtils.Data, ref Vector2i, ref Vector2i, bool, bool)).MethodHandle, typeof(PathingUtils.FindDetailedPath_0000B5ED$PostfixBurstDelegate).TypeHandle);
				}
				A_0 = PathingUtils.FindDetailedPath_0000B5ED$BurstDirectCall.Pointer;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				PathingUtils.FindDetailedPath_0000B5ED$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public static void Constructor()
			{
				PathingUtils.FindDetailedPath_0000B5ED$BurstDirectCall.DeferredCompilation = BurstCompiler.CompileILPPMethod2(methodof(PathingUtils.FindDetailedPath(ref WorldBuilder.Data, ref PathingUtils.Data, ref Vector2i, ref Vector2i, bool, bool)).MethodHandle);
			}

			public static void Initialize()
			{
			}

			// Note: this type is marked as 'beforefieldinit'.
			[PublicizedFrom(EAccessModifier.Private)]
			static FindDetailedPath_0000B5ED$BurstDirectCall()
			{
				PathingUtils.FindDetailedPath_0000B5ED$BurstDirectCall.Constructor();
			}

			public static int Invoke(ref WorldBuilder.Data wd, ref PathingUtils.Data data, in Vector2i startPos, in Vector2i endPos, bool _isCountryRoad, bool _isRiver)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = PathingUtils.FindDetailedPath_0000B5ED$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(WorldGenerationEngineFinal.WorldBuilder/Data&,WorldGenerationEngineFinal.PathingUtils/Data&,Vector2i&,Vector2i&,System.Boolean,System.Boolean), ref wd, ref data, ref startPos, ref endPos, _isCountryRoad, _isRiver, functionPointer);
					}
				}
				return PathingUtils.FindDetailedPath$BurstManaged(ref wd, ref data, in startPos, in endPos, _isCountryRoad, _isRiver);
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr Pointer;

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr DeferredCompilation;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public delegate int FindDetailedPath_0000B5EF$PostfixBurstDelegate(ref WorldBuilder.Data wd, ref PathingUtils.Data data, in Vector2i startPos, in NativeList<Vector2i> _endPath, bool _isCountryRoad, bool _isRiver);

		[PublicizedFrom(EAccessModifier.Internal)]
		public static class FindDetailedPath_0000B5EF$BurstDirectCall
		{
			[BurstDiscard]
			[PublicizedFrom(EAccessModifier.Private)]
			public static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (PathingUtils.FindDetailedPath_0000B5EF$BurstDirectCall.Pointer == 0)
				{
					PathingUtils.FindDetailedPath_0000B5EF$BurstDirectCall.Pointer = BurstCompiler.GetILPPMethodFunctionPointer2(PathingUtils.FindDetailedPath_0000B5EF$BurstDirectCall.DeferredCompilation, methodof(PathingUtils.FindDetailedPath$BurstManaged(ref WorldBuilder.Data, ref PathingUtils.Data, ref Vector2i, ref NativeList<Vector2i>, bool, bool)).MethodHandle, typeof(PathingUtils.FindDetailedPath_0000B5EF$PostfixBurstDelegate).TypeHandle);
				}
				A_0 = PathingUtils.FindDetailedPath_0000B5EF$BurstDirectCall.Pointer;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				PathingUtils.FindDetailedPath_0000B5EF$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public static void Constructor()
			{
				PathingUtils.FindDetailedPath_0000B5EF$BurstDirectCall.DeferredCompilation = BurstCompiler.CompileILPPMethod2(methodof(PathingUtils.FindDetailedPath(ref WorldBuilder.Data, ref PathingUtils.Data, ref Vector2i, ref NativeList<Vector2i>, bool, bool)).MethodHandle);
			}

			public static void Initialize()
			{
			}

			// Note: this type is marked as 'beforefieldinit'.
			[PublicizedFrom(EAccessModifier.Private)]
			static FindDetailedPath_0000B5EF$BurstDirectCall()
			{
				PathingUtils.FindDetailedPath_0000B5EF$BurstDirectCall.Constructor();
			}

			public static int Invoke(ref WorldBuilder.Data wd, ref PathingUtils.Data data, in Vector2i startPos, in NativeList<Vector2i> _endPath, bool _isCountryRoad, bool _isRiver)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = PathingUtils.FindDetailedPath_0000B5EF$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Int32(WorldGenerationEngineFinal.WorldBuilder/Data&,WorldGenerationEngineFinal.PathingUtils/Data&,Vector2i&,Unity.Collections.NativeList`1<Vector2i>&,System.Boolean,System.Boolean), ref wd, ref data, ref startPos, ref _endPath, _isCountryRoad, _isRiver, functionPointer);
					}
				}
				return PathingUtils.FindDetailedPath$BurstManaged(ref wd, ref data, in startPos, in _endPath, _isCountryRoad, _isRiver);
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr Pointer;

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr DeferredCompilation;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public delegate void CalcPathBounds_0000B5F0$PostfixBurstDelegate(in NativeList<Vector2i> _path, out Vector2i _min, out Vector2i _max);

		[PublicizedFrom(EAccessModifier.Internal)]
		public static class CalcPathBounds_0000B5F0$BurstDirectCall
		{
			[BurstDiscard]
			[PublicizedFrom(EAccessModifier.Private)]
			public static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (PathingUtils.CalcPathBounds_0000B5F0$BurstDirectCall.Pointer == 0)
				{
					PathingUtils.CalcPathBounds_0000B5F0$BurstDirectCall.Pointer = BurstCompiler.GetILPPMethodFunctionPointer2(PathingUtils.CalcPathBounds_0000B5F0$BurstDirectCall.DeferredCompilation, methodof(PathingUtils.CalcPathBounds$BurstManaged(ref NativeList<Vector2i>, ref Vector2i, ref Vector2i)).MethodHandle, typeof(PathingUtils.CalcPathBounds_0000B5F0$PostfixBurstDelegate).TypeHandle);
				}
				A_0 = PathingUtils.CalcPathBounds_0000B5F0$BurstDirectCall.Pointer;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				PathingUtils.CalcPathBounds_0000B5F0$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public static void Constructor()
			{
				PathingUtils.CalcPathBounds_0000B5F0$BurstDirectCall.DeferredCompilation = BurstCompiler.CompileILPPMethod2(methodof(PathingUtils.CalcPathBounds(ref NativeList<Vector2i>, ref Vector2i, ref Vector2i)).MethodHandle);
			}

			public static void Initialize()
			{
			}

			// Note: this type is marked as 'beforefieldinit'.
			[PublicizedFrom(EAccessModifier.Private)]
			static CalcPathBounds_0000B5F0$BurstDirectCall()
			{
				PathingUtils.CalcPathBounds_0000B5F0$BurstDirectCall.Constructor();
			}

			public static void Invoke(in NativeList<Vector2i> _path, out Vector2i _min, out Vector2i _max)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = PathingUtils.CalcPathBounds_0000B5F0$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						calli(System.Void(Unity.Collections.NativeList`1<Vector2i>&,Vector2i&,Vector2i&), ref _path, ref _min, ref _max, functionPointer);
						return;
					}
				}
				PathingUtils.CalcPathBounds$BurstManaged(in _path, out _min, out _max);
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr Pointer;

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr DeferredCompilation;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public delegate float FindClosestPathPoint_0000B5F1$PostfixBurstDelegate(in NativeList<Vector2> _path, in Vector2 _startPos, out Vector2 _destPoint, int _step = 1);

		[PublicizedFrom(EAccessModifier.Internal)]
		public static class FindClosestPathPoint_0000B5F1$BurstDirectCall
		{
			[BurstDiscard]
			[PublicizedFrom(EAccessModifier.Private)]
			public static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (PathingUtils.FindClosestPathPoint_0000B5F1$BurstDirectCall.Pointer == 0)
				{
					PathingUtils.FindClosestPathPoint_0000B5F1$BurstDirectCall.Pointer = BurstCompiler.GetILPPMethodFunctionPointer2(PathingUtils.FindClosestPathPoint_0000B5F1$BurstDirectCall.DeferredCompilation, methodof(PathingUtils.FindClosestPathPoint$BurstManaged(ref NativeList<Vector2>, ref Vector2, ref Vector2, int)).MethodHandle, typeof(PathingUtils.FindClosestPathPoint_0000B5F1$PostfixBurstDelegate).TypeHandle);
				}
				A_0 = PathingUtils.FindClosestPathPoint_0000B5F1$BurstDirectCall.Pointer;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				PathingUtils.FindClosestPathPoint_0000B5F1$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public static void Constructor()
			{
				PathingUtils.FindClosestPathPoint_0000B5F1$BurstDirectCall.DeferredCompilation = BurstCompiler.CompileILPPMethod2(methodof(PathingUtils.FindClosestPathPoint(ref NativeList<Vector2>, ref Vector2, ref Vector2, int)).MethodHandle);
			}

			public static void Initialize()
			{
			}

			// Note: this type is marked as 'beforefieldinit'.
			[PublicizedFrom(EAccessModifier.Private)]
			static FindClosestPathPoint_0000B5F1$BurstDirectCall()
			{
				PathingUtils.FindClosestPathPoint_0000B5F1$BurstDirectCall.Constructor();
			}

			public static float Invoke(in NativeList<Vector2> _path, in Vector2 _startPos, out Vector2 _destPoint, int _step = 1)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = PathingUtils.FindClosestPathPoint_0000B5F1$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Single(Unity.Collections.NativeList`1<UnityEngine.Vector2>&,UnityEngine.Vector2&,UnityEngine.Vector2&,System.Int32), ref _path, ref _startPos, ref _destPoint, _step, functionPointer);
					}
				}
				return PathingUtils.FindClosestPathPoint$BurstManaged(in _path, in _startPos, out _destPoint, _step);
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr Pointer;

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr DeferredCompilation;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public delegate float FindClosestPathPoint_0000B5F2$PostfixBurstDelegate(in NativeList<Vector2i> _path, in Vector2i _startPos, out Vector2i _destPoint);

		[PublicizedFrom(EAccessModifier.Internal)]
		public static class FindClosestPathPoint_0000B5F2$BurstDirectCall
		{
			[BurstDiscard]
			[PublicizedFrom(EAccessModifier.Private)]
			public static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (PathingUtils.FindClosestPathPoint_0000B5F2$BurstDirectCall.Pointer == 0)
				{
					PathingUtils.FindClosestPathPoint_0000B5F2$BurstDirectCall.Pointer = BurstCompiler.GetILPPMethodFunctionPointer2(PathingUtils.FindClosestPathPoint_0000B5F2$BurstDirectCall.DeferredCompilation, methodof(PathingUtils.FindClosestPathPoint$BurstManaged(ref NativeList<Vector2i>, ref Vector2i, ref Vector2i)).MethodHandle, typeof(PathingUtils.FindClosestPathPoint_0000B5F2$PostfixBurstDelegate).TypeHandle);
				}
				A_0 = PathingUtils.FindClosestPathPoint_0000B5F2$BurstDirectCall.Pointer;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				PathingUtils.FindClosestPathPoint_0000B5F2$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public static void Constructor()
			{
				PathingUtils.FindClosestPathPoint_0000B5F2$BurstDirectCall.DeferredCompilation = BurstCompiler.CompileILPPMethod2(methodof(PathingUtils.FindClosestPathPoint(ref NativeList<Vector2i>, ref Vector2i, ref Vector2i)).MethodHandle);
			}

			public static void Initialize()
			{
			}

			// Note: this type is marked as 'beforefieldinit'.
			[PublicizedFrom(EAccessModifier.Private)]
			static FindClosestPathPoint_0000B5F2$BurstDirectCall()
			{
				PathingUtils.FindClosestPathPoint_0000B5F2$BurstDirectCall.Constructor();
			}

			public static float Invoke(in NativeList<Vector2i> _path, in Vector2i _startPos, out Vector2i _destPoint)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = PathingUtils.FindClosestPathPoint_0000B5F2$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Single(Unity.Collections.NativeList`1<Vector2i>&,Vector2i&,Vector2i&), ref _path, ref _startPos, ref _destPoint, functionPointer);
					}
				}
				return PathingUtils.FindClosestPathPoint$BurstManaged(in _path, in _startPos, out _destPoint);
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr Pointer;

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr DeferredCompilation;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public delegate bool IsPointOnPath_0000B5F3$PostfixBurstDelegate(in NativeList<Vector2i> _path, in Vector2i _point);

		[PublicizedFrom(EAccessModifier.Internal)]
		public static class IsPointOnPath_0000B5F3$BurstDirectCall
		{
			[BurstDiscard]
			[PublicizedFrom(EAccessModifier.Private)]
			public static void GetFunctionPointerDiscard(ref IntPtr A_0)
			{
				if (PathingUtils.IsPointOnPath_0000B5F3$BurstDirectCall.Pointer == 0)
				{
					PathingUtils.IsPointOnPath_0000B5F3$BurstDirectCall.Pointer = BurstCompiler.GetILPPMethodFunctionPointer2(PathingUtils.IsPointOnPath_0000B5F3$BurstDirectCall.DeferredCompilation, methodof(PathingUtils.IsPointOnPath$BurstManaged(ref NativeList<Vector2i>, ref Vector2i)).MethodHandle, typeof(PathingUtils.IsPointOnPath_0000B5F3$PostfixBurstDelegate).TypeHandle);
				}
				A_0 = PathingUtils.IsPointOnPath_0000B5F3$BurstDirectCall.Pointer;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr GetFunctionPointer()
			{
				IntPtr intPtr = (IntPtr)0;
				PathingUtils.IsPointOnPath_0000B5F3$BurstDirectCall.GetFunctionPointerDiscard(ref intPtr);
				return intPtr;
			}

			public static void Constructor()
			{
				PathingUtils.IsPointOnPath_0000B5F3$BurstDirectCall.DeferredCompilation = BurstCompiler.CompileILPPMethod2(methodof(PathingUtils.IsPointOnPath(ref NativeList<Vector2i>, ref Vector2i)).MethodHandle);
			}

			public static void Initialize()
			{
			}

			// Note: this type is marked as 'beforefieldinit'.
			[PublicizedFrom(EAccessModifier.Private)]
			static IsPointOnPath_0000B5F3$BurstDirectCall()
			{
				PathingUtils.IsPointOnPath_0000B5F3$BurstDirectCall.Constructor();
			}

			public static bool Invoke(in NativeList<Vector2i> _path, in Vector2i _point)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = PathingUtils.IsPointOnPath_0000B5F3$BurstDirectCall.GetFunctionPointer();
					if (functionPointer != 0)
					{
						return calli(System.Boolean(Unity.Collections.NativeList`1<Vector2i>&,Vector2i&), ref _path, ref _point, functionPointer);
					}
				}
				return PathingUtils.IsPointOnPath$BurstManaged(in _path, in _point);
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr Pointer;

			[PublicizedFrom(EAccessModifier.Private)]
			public static IntPtr DeferredCompilation;
		}
	}
}
