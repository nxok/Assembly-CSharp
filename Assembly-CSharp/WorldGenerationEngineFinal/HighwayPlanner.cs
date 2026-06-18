using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class HighwayPlanner
	{
		public HighwayPlanner(WorldBuilder _worldBuilder)
		{
			this.worldBuilder = _worldBuilder;
		}

		public void Plan(DynamicProperties thisWorldProperties, int worldSeed)
		{
			this.worldBuilder.SetTaskMessage(this.worldBuilder.messageHighways);
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			this.ExitConnections.Clear();
			List<Township> list = this.worldBuilder.Townships.FindAll((Township _township) => _township.Data.SpawnGateway);
			if (list.Count > 0)
			{
				list.Sort((Township _t1, Township _t2) => _t2.Streets.Count.CompareTo(_t1.Streets.Count));
				for (int i = 0; i < list.Count; i++)
				{
					Township township = list[i];
					this.ConnectClosest(township, list);
					if (township.IsBig())
					{
						this.ConnectSelf(township);
					}
				}
				Log.Out(string.Format("HighwayPlanner Plan townships in {0}", (float)microStopwatch.ElapsedMilliseconds * 0.001f));
				this.CleanupHighwayConnections(list);
			}
			this.RunTownshipDirtRoads();
			Log.Out(string.Format("HighwayPlanner Plan in {0}, r={1:x}", (float)microStopwatch.ElapsedMilliseconds * 0.001f, Rand.Instance.PeekSample()));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CleanupHighwayConnections(List<Township> _townships)
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			List<Vector2i> list = new List<Vector2i>();
			List<Path> list2 = new List<Path>();
			foreach (Township township in _townships)
			{
				this.worldBuilder.SetTaskMessage(this.worldBuilder.messageHighwaysConnections);
				if (township.Data.SpawnGateway && township.Gateways.Count != 0)
				{
					foreach (StreetTile streetTile in township.Gateways)
					{
						if (streetTile.UsedExitList.Count < 2)
						{
							Log.Warning("HighwayPlanner gateway has only {0} connections , {1}", new object[]
							{
								streetTile.UsedExitList.Count,
								streetTile.PrefabName
							});
							for (int i = 0; i < 4; i++)
							{
								streetTile.SetExitUnUsed(i);
								StreetTile neighbor = streetTile.GetNeighbor(i);
								if (neighbor.Township == streetTile.Township)
								{
									neighbor.SetExitUnUsed((i + 2) & 3);
								}
							}
							foreach (Path path in streetTile.ConnectedHighways)
							{
								StreetTile streetTile2;
								if (this.worldBuilder.GetStreetTileWorld(path.StartPosition) == streetTile)
								{
									streetTile2 = this.worldBuilder.GetStreetTileWorld(path.EndPosition);
									streetTile2.SetExitUnUsed(path.EndPosition);
								}
								else
								{
									streetTile2 = this.worldBuilder.GetStreetTileWorld(path.StartPosition);
									streetTile2.SetExitUnUsed(path.StartPosition);
								}
								if (streetTile2.UsedExitList.Count < 2)
								{
									list.Add(streetTile2.GridPosition);
								}
								list2.Add(path);
							}
							list.Add(streetTile.GridPosition);
						}
					}
					foreach (Vector2i vector2i in list)
					{
						StreetTile streetTileGrid = this.worldBuilder.GetStreetTileGrid(vector2i);
						if (streetTileGrid.Township != null)
						{
							streetTileGrid.Township.Gateways.Remove(streetTileGrid);
							streetTileGrid.Township.Streets.Remove(vector2i);
						}
						streetTileGrid.StreetTilePrefabDatas.Clear();
						streetTileGrid.District = null;
						streetTileGrid.SetTownship(null);
					}
					list.Clear();
					foreach (Path path2 in list2)
					{
						path2.RemoveFromStreetTiles();
						this.worldBuilder.highwayPaths.Remove(path2);
						path2.Cleanup();
					}
					list2.Clear();
				}
			}
			Log.Out(string.Format("HighwayPlanner cleanupHighwayConnections in {0}, r={1:x}", (float)microStopwatch.ElapsedMilliseconds * 0.001f, Rand.Instance.PeekSample()));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void RunTownshipDirtRoads()
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			List<Township> list = this.worldBuilder.Townships.FindAll((Township _township) => !_township.Data.SpawnGateway);
			for (int i = 0; i < list.Count; i++)
			{
				Township township = list[i];
				string text = string.Format(this.worldBuilder.messageHighwaysTownship, i + 1, list.Count);
				this.worldBuilder.SetTaskMessage(text);
				MicroStopwatch microStopwatch2 = new MicroStopwatch(true);
				int num = 0;
				foreach (Vector2i vector2i in township.GetUnusedTownExits(4))
				{
					Vector2i vector2i2 = Vector2i.zero;
					float num2 = float.MaxValue;
					foreach (Path path in this.worldBuilder.highwayPaths)
					{
						if (!path.isCountryRoad)
						{
							foreach (Vector2 vector in path.FinalPathPoints)
							{
								Vector2i vector2i3;
								vector2i3.x = Utils.Fastfloor(vector.x);
								vector2i3.y = Utils.Fastfloor(vector.y);
								float num3 = Vector2i.DistanceSqr(vector2i, vector2i3);
								if (num3 < num2)
								{
									if (this.worldBuilder.PathingUtils.GetPathCost(vector2i, vector2i3, true) > 0)
									{
										num2 = num3;
										vector2i2 = vector2i3;
									}
									num++;
									if (num % 10 == 0)
									{
										this.worldBuilder.SetTaskMessage(text + " " + string.Format(this.worldBuilder.messageHighwaysTownExits, num));
									}
								}
							}
						}
					}
					this.worldBuilder.SetTaskMessage(text);
					Path path2 = new Path(this.worldBuilder, vector2i, vector2i2, 2, true);
					if (path2.IsValid)
					{
						foreach (StreetTile streetTile in township.Streets.Values)
						{
							for (int j = 0; j < 4; j++)
							{
								if (Vector2i.Distance(streetTile.GetHighwayExitPos(j), vector2i) < 10f)
								{
									streetTile.SetExitUsed(j);
								}
							}
						}
						this.worldBuilder.highwayPaths.Add(path2);
					}
					else
					{
						path2.Cleanup();
					}
				}
				Log.Out(string.Format("HighwayPlanner runTownshipDirtRoads #{0} unused exits c{1} in {2}, r={3:x}", new object[]
				{
					i,
					num,
					(float)microStopwatch2.ElapsedMilliseconds * 0.001f,
					Rand.Instance.PeekSample()
				}));
			}
			Log.Out(string.Format("HighwayPlanner runTownshipDirtRoads, countryTownships {0}, in {1}, r={2:x}", list.Count, (float)microStopwatch.ElapsedMilliseconds * 0.001f, Rand.Instance.PeekSample()));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ConnectClosest(Township _township, List<Township> highwayTownships)
		{
			Predicate<Township> <>9__0;
			for (int i = 0; i < _township.Gateways.Count; i++)
			{
				StreetTile gateway = _township.Gateways[i];
				if (gateway.UsedExitList.Count < 2)
				{
					Predicate<Township> predicate;
					if ((predicate = <>9__0) == null)
					{
						predicate = (<>9__0 = delegate(Township t)
						{
							int num4;
							return (!_township.TownshipConnectionCounts.TryGetValue(t, out num4) || num4 <= 1) && t.ID != _township.ID && t.Data.SpawnGateway;
						});
					}
					List<Township> list = highwayTownships.FindAll(predicate);
					list.Sort((Township _t1, Township _t2) => Vector2i.DistanceSqr(gateway.GridPosition, _t1.GridCenter).CompareTo(Vector2i.DistanceSqr(gateway.GridPosition, _t2.GridCenter)));
					Path path = null;
					Township township = null;
					int num = 0;
					foreach (Township township2 in list)
					{
						this.GetPathToTownship(gateway, township2);
						Path path2 = this.getPathToTownshipResult;
						if (path2 != null)
						{
							this.getPathToTownshipResult = null;
							int num2;
							_township.TownshipConnectionCounts.TryGetValue(township2, out num2);
							if (_township.Streets.Count <= 1 || township2.Streets.Count <= 1)
							{
								if (num2 > 0)
								{
									path2.Cost *= 4;
								}
							}
							else if (num2 > 0)
							{
								path2.Cost = (int)((float)path2.Cost * 1.6f);
							}
							if (path == null || path2.Cost < path.Cost)
							{
								path = path2;
								township = township2;
							}
							if (path != path2)
							{
								path2.Cleanup();
							}
							if (++num >= 3)
							{
								break;
							}
						}
					}
					if (path != null)
					{
						int num3;
						_township.TownshipConnectionCounts.TryGetValue(township, out num3);
						_township.TownshipConnectionCounts[township] = num3 + 1;
						township.TownshipConnectionCounts.TryGetValue(_township, out num3);
						township.TownshipConnectionCounts[_township] = num3 + 1;
						this.worldBuilder.highwayPaths.Add(path);
						this.SetTileExits(path);
						path.CommitPathingMapData();
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ConnectSelf(Township _township)
		{
			_township.SortGatewaysClockwise();
			int count = _township.Gateways.Count;
			for (int i = 0; i < count; i++)
			{
				StreetTile streetTile = _township.Gateways[i];
				if (streetTile.UsedExitList.Count < 4)
				{
					StreetTile streetTile2 = _township.Gateways[(i + 1) % count];
					if (streetTile2.UsedExitList.Count < 4)
					{
						int num = int.MaxValue;
						Path path = null;
						foreach (Vector2i vector2i in streetTile.GetHighwayExits(true))
						{
							foreach (Vector2i vector2i2 in streetTile2.GetHighwayExits(true))
							{
								Path path2 = new Path(this.worldBuilder, vector2i, vector2i2, 4, false);
								if (path2.IsValid)
								{
									int cost = path2.Cost;
									if (cost < num)
									{
										num = cost;
										path = path2;
									}
								}
								path2.RemoveFromStreetTiles();
								if (path == null || path != path2)
								{
									path2.Cleanup();
								}
							}
							if (this.worldBuilder.IsMessageElapsed())
							{
								this.worldBuilder.SetTaskMessage(string.Format(this.worldBuilder.messageHighwaysTownExitsSelf, Application.isEditor ? streetTile.Township.GetTypeName() : string.Empty));
							}
						}
						if (path != null)
						{
							this.worldBuilder.highwayPaths.Add(path);
							this.SetTileExits(path);
							path.CommitPathingMapData();
						}
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetTileExits(Path path)
		{
			this.SetTileExit(path, path.StartPosition);
			this.SetTileExit(path, path.EndPosition);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetTileExit(Path currentPath, Vector2i exit)
		{
			StreetTile streetTile = this.worldBuilder.GetStreetTileWorld(exit);
			if (streetTile != null)
			{
				if (streetTile.District != null && streetTile.District.name == "gateway")
				{
					this.ExitConnections.Add(new HighwayPlanner.ExitConnection(streetTile, exit, currentPath));
					return;
				}
				foreach (StreetTile streetTile2 in streetTile.GetNeighbors())
				{
					if (streetTile2 != null && streetTile2.District != null && streetTile2.District.name == "gateway")
					{
						this.ExitConnections.Add(new HighwayPlanner.ExitConnection(streetTile2, exit, currentPath));
						return;
					}
				}
				streetTile = null;
			}
			if (streetTile == null)
			{
				Township township = null;
				foreach (Township township2 in this.worldBuilder.Townships)
				{
					if (township2.Area.Contains(exit.AsVector2()))
					{
						township = township2;
						break;
					}
				}
				if (township != null)
				{
					foreach (StreetTile streetTile3 in township.Gateways)
					{
						for (int j = 0; j < 4; j++)
						{
							if (streetTile3.GetHighwayExitPos(j) == exit || Vector2i.DistanceSqr(streetTile3.GetHighwayExitPos(j), exit) < 100f)
							{
								this.ExitConnections.Add(new HighwayPlanner.ExitConnection(streetTile3, exit, currentPath));
								return;
							}
						}
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void GetPathToTownship(StreetTile gateway, Township otherTownship)
		{
			int num = int.MaxValue;
			Path path = null;
			foreach (Vector2i vector2i in gateway.GetHighwayExits(true))
			{
				foreach (Vector2i vector2i2 in otherTownship.GetUnusedTownExits(3))
				{
					Path path2 = new Path(this.worldBuilder, vector2i, vector2i2, 4, false);
					if (path2.IsValid)
					{
						int cost = path2.Cost;
						if (cost < num)
						{
							num = cost;
							path = path2;
						}
					}
					path2.RemoveFromStreetTiles();
					if (path == null || path != path2)
					{
						path2.Cleanup();
					}
				}
				this.worldBuilder.SetTaskMessage(string.Format(this.worldBuilder.messageHighwaysTownExitsOther, Application.isEditor ? gateway.Township.GetTypeName() : string.Empty));
			}
			this.getPathToTownshipResult = path;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<HighwayPlanner.ExitConnection> ExitConnections = new List<HighwayPlanner.ExitConnection>();

		[PublicizedFrom(EAccessModifier.Private)]
		public Path getPathToTownshipResult;

		public class ExitConnection
		{
			public ExitConnection(StreetTile parent, Vector2i worldPos, Path connectedPath = null)
			{
				this.ParentTile = parent;
				this.ExitDir = parent.GetHighwayExitDir(worldPos);
				parent.SetExitUsed(this.ExitDir);
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public StreetTile ParentTile;

			[PublicizedFrom(EAccessModifier.Private)]
			public int ExitDir;
		}
	}
}
