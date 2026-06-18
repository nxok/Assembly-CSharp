using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class WildernessPathPlanner
	{
		public WildernessPathPlanner(WorldBuilder _worldBuilder)
		{
			this.worldBuilder = _worldBuilder;
		}

		public void Plan(int worldSeed)
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			bool flag = this.worldBuilder.highwayPaths.Count > 0;
			List<WildernessPlanner.WildernessPathInfo> wildernessPathInfos = this.worldBuilder.WildernessPlanner.WildernessPathInfos;
			for (int i = 0; i < wildernessPathInfos.Count; i++)
			{
				WildernessPlanner.WildernessPathInfo wildernessPathInfo = wildernessPathInfos[i];
				Vector2 vector = wildernessPathInfo.Position.AsVector2();
				float num = float.MaxValue;
				Vector2 vector2 = Vector2.zero;
				foreach (Path path in this.worldBuilder.highwayPaths)
				{
					Vector2 vector3;
					if (PathingUtils.FindClosestPathPoint(in path.FinalPathPoints, in vector, out vector3, 5) < 1000000f)
					{
						int num2 = Utils.FastMin(5, path.FinalPathPoints.Length - 1);
						for (int j = 0; j < path.FinalPathPoints.Length; j += num2)
						{
							Vector2 vector4 = path.FinalPathPoints[j];
							if ((vector3 - vector4).sqrMagnitude <= 62500f)
							{
								Vector2i vector2i = new Vector2i(vector4);
								int length = this.worldBuilder.PathingUtils.GetPath(wildernessPathInfo.Position, vector2i, true, false).Length;
								if (length >= 2 && (float)length < num)
								{
									num = (float)length;
									vector2 = vector4;
								}
							}
						}
					}
				}
				wildernessPathInfo.highwayDistance = num;
				wildernessPathInfo.highwayPoint = vector2;
				this.worldBuilder.SetMessage(string.Format(this.worldBuilder.messageWildernessPaths, i * 50 / wildernessPathInfos.Count), false, false);
			}
			wildernessPathInfos.Sort(delegate(WildernessPlanner.WildernessPathInfo wp1, WildernessPlanner.WildernessPathInfo wp2)
			{
				float num7 = ((wp1.PathRadius < 2.4f) ? wp1.PathRadius : 3f);
				float num8 = ((wp2.PathRadius < 2.4f) ? wp2.PathRadius : 3f);
				if (num7 != num8)
				{
					return num8.CompareTo(num7);
				}
				return wp1.highwayDistance.CompareTo(wp2.highwayDistance);
			});
			for (int k = 0; k < wildernessPathInfos.Count; k++)
			{
				WildernessPlanner.WildernessPathInfo wildernessPathInfo2 = wildernessPathInfos[k];
				if (wildernessPathInfo2.Path == null)
				{
					Vector2i vector2i2 = Vector2i.zero;
					float num3 = float.MaxValue;
					bool flag2 = false;
					if (wildernessPathInfo2.highwayDistance >= 2f && wildernessPathInfo2.highwayDistance < 999999f)
					{
						num3 = wildernessPathInfo2.highwayDistance;
						vector2i2.x = (int)wildernessPathInfo2.highwayPoint.x;
						vector2i2.y = (int)wildernessPathInfo2.highwayPoint.y;
						flag2 = true;
					}
					foreach (WildernessPlanner.WildernessPathInfo wildernessPathInfo3 in wildernessPathInfos)
					{
						if (wildernessPathInfo3 != wildernessPathInfo2)
						{
							Vector2 vector5;
							int num5;
							if (wildernessPathInfo3.Path == null)
							{
								if (!flag)
								{
									float num4 = Vector2i.Distance(wildernessPathInfo2.Position, wildernessPathInfo3.Position);
									if (num4 < num3)
									{
										num3 = num4;
										vector2i2 = wildernessPathInfo3.Position;
									}
								}
							}
							else if (wildernessPathInfo3.Path.connectsToHighway && wildernessPathInfo3.PathRadius >= wildernessPathInfo2.PathRadius && this.FindShortestPathPointToPathTo(wildernessPathInfo3.Path.FinalPathPoints, wildernessPathInfo2.Position.AsVector2(), out vector5, out num5))
							{
								float num6 = (float)num5;
								if (num6 < num3)
								{
									num3 = num6;
									vector2i2.x = (int)vector5.x;
									vector2i2.y = (int)vector5.y;
								}
							}
						}
					}
					this.worldBuilder.SetMessage(string.Format(this.worldBuilder.messageWildernessPaths, k * 50 / wildernessPathInfos.Count + 50), false, false);
					if (num3 <= 999999f)
					{
						Path path2 = new Path(this.worldBuilder, wildernessPathInfo2.Position, vector2i2, wildernessPathInfo2.PathRadius, true);
						if (path2.IsValid)
						{
							path2.connectsToHighway = flag2;
							wildernessPathInfo2.Path = path2;
							this.worldBuilder.wildernessPaths.Add(path2);
							this.createTraderSpawnIfAble(path2.FinalPathPoints);
						}
						else
						{
							path2.Cleanup();
							this.worldBuilder.AddPreviewLinePlus(wildernessPathInfo2.Position, new Color(1f, 0.3f, 1f), 90);
							this.worldBuilder.AddPreviewLinePlus(vector2i2, new Color(1f, 0.3f, 0.5f), 70);
							Log.Warning(string.Format("WildernessPathPlanner Plan index {0} no path ({1} to {2})", k, wildernessPathInfo2.Position, vector2i2));
						}
					}
				}
			}
			Log.Out(string.Format("WildernessPathPlanner Plan #{0} in {1}, r={2:x}", wildernessPathInfos.Count, (float)microStopwatch.ElapsedMilliseconds * 0.001f, Rand.Instance.PeekSample()));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool FindShortestPathPointToPathTo(NativeList<Vector2> _path, Vector2 _startPos, out Vector2 _destPoint, out int _cost)
		{
			_destPoint = Vector2.zero;
			_cost = 0;
			Vector2 zero = Vector2.zero;
			if (PathingUtils.FindClosestPathPoint(in _path, in _startPos, out zero, 1) < 490000f)
			{
				Vector2i pathPoint = this.worldBuilder.PathingUtils.GetPathPoint(new Vector2i(_startPos), ref _path, true, false, out _cost);
				if (_cost > 0)
				{
					_destPoint.x = (float)pathPoint.x;
					_destPoint.y = (float)pathPoint.y;
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void createTraderSpawnIfAble(NativeList<Vector2> pathPoints)
		{
			if (pathPoints.Length < 5)
			{
				return;
			}
			if (this.worldBuilder.ForestBiomeWeight > 0)
			{
				BiomeType biomeType = BiomeType.none;
				for (int i = 2; i < pathPoints.Length - 2; i++)
				{
					biomeType = this.worldBuilder.GetBiome((int)pathPoints[i].x, (int)pathPoints[i].y);
					if (biomeType == BiomeType.forest)
					{
						break;
					}
				}
				if (biomeType != BiomeType.forest)
				{
					return;
				}
			}
			for (int j = 2; j < pathPoints.Length - 2; j++)
			{
				if (this.worldBuilder.ForestBiomeWeight <= 0 || this.worldBuilder.GetBiome((int)pathPoints[j].x, (int)pathPoints[j].y) == BiomeType.forest)
				{
					Vector2i vector2i;
					vector2i.x = (int)pathPoints[j].x;
					vector2i.y = (int)pathPoints[j].y;
					StreetTile streetTileWorld = this.worldBuilder.GetStreetTileWorld(vector2i);
					if (streetTileWorld != null && streetTileWorld.HasPrefabs)
					{
						bool flag = true;
						using (List<PrefabDataInstance>.Enumerator enumerator = streetTileWorld.StreetTilePrefabDatas.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								if (enumerator.Current.prefab.DifficultyTier > 1)
								{
									flag = false;
									break;
								}
							}
						}
						if (flag)
						{
							this.worldBuilder.CreatePlayerSpawn(vector2i, false);
						}
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int getMaxTraderDistance()
		{
			return (int)(0.1f * (float)this.worldBuilder.WorldSize);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;
	}
}
