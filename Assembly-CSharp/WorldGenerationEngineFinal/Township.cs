using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class Township
	{
		public Township(WorldBuilder _worldBuilder, TownshipData _townshipData)
		{
			this.worldBuilder = _worldBuilder;
			this.townshipData = _townshipData;
		}

		public void Cleanup()
		{
			GameRandomManager.Instance.FreeGameRandom(this.rand);
		}

		public TownshipData Data
		{
			get
			{
				return this.townshipData;
			}
		}

		public string GetTypeName()
		{
			return this.townshipData.Name;
		}

		public bool IsBig()
		{
			return this.townshipData.Name == "citybig";
		}

		public bool IsRoadside()
		{
			return this.townshipData.Category == TownshipData.eCategory.Roadside;
		}

		public bool IsRural()
		{
			return this.townshipData.Category == TownshipData.eCategory.Rural;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsWilderness()
		{
			return this.townshipData.Category == TownshipData.eCategory.Wilderness;
		}

		public void SortGatewaysClockwise()
		{
			this.Gateways.Sort(delegate(StreetTile _t1, StreetTile _t2)
			{
				float num = Mathf.Atan2((float)(_t1.GridPosition.y - this.GridCenter.y), (float)(_t1.GridPosition.x - this.GridCenter.x));
				float num2 = Mathf.Atan2((float)(_t2.GridPosition.y - this.GridCenter.y), (float)(_t2.GridPosition.x - this.GridCenter.x));
				return num.CompareTo(num2);
			});
		}

		public void CleanupStreets()
		{
			if (this.Streets == null || this.Streets.Count == 0)
			{
				Log.Error("CleanupStreets none!");
				return;
			}
			this.rand = GameRandomManager.Instance.CreateGameRandom(this.worldBuilder.Seed + this.ID + this.Streets.Count);
			int num = int.MaxValue;
			int num2 = int.MaxValue;
			int num3 = int.MinValue;
			int num4 = int.MinValue;
			foreach (StreetTile streetTile in this.Streets.Values)
			{
				num = Utils.FastMin(num, streetTile.WorldPosition.x);
				num2 = Utils.FastMin(num2, streetTile.WorldPosition.y);
				num3 = Utils.FastMax(num3, streetTile.WorldPositionMax.x);
				num4 = Utils.FastMax(num4, streetTile.WorldPositionMax.y);
			}
			this.Area = new Rect((float)num, (float)num2, (float)(num3 - num), (float)(num4 - num2));
			this.BufferArea = new Rect(this.Area.xMin - 150f, this.Area.yMin - 150f, this.Area.width + 300f, this.Area.height + 300f);
		}

		public void SpawnPrefabs()
		{
			foreach (StreetTile streetTile in this.Streets.Values)
			{
				if (streetTile == null)
				{
					Log.Error("WorldTileData is null, this shouldn't happen!");
				}
				else
				{
					streetTile.SpawnPrefabs();
				}
			}
			this.Prefabs.Clear();
		}

		public StreetTile CalcCenterStreetTile()
		{
			if (this.centerMostTile != null)
			{
				return this.centerMostTile;
			}
			Vector2i vector2i = Vector2i.zero;
			foreach (StreetTile streetTile in this.Streets.Values)
			{
				vector2i += streetTile.GridPosition;
			}
			vector2i /= this.Streets.Count;
			int num = int.MaxValue;
			StreetTile streetTile2 = null;
			foreach (StreetTile streetTile3 in this.Streets.Values)
			{
				int num2 = Vector2i.DistanceSqrInt(vector2i, streetTile3.GridPosition);
				if (num2 < num)
				{
					num = num2;
					streetTile2 = streetTile3;
				}
			}
			this.centerMostTile = streetTile2;
			float num3 = 0f;
			Vector2i worldPositionCenter = this.centerMostTile.WorldPositionCenter;
			for (int i = -50; i <= 50; i += 50)
			{
				Vector2i vector2i2;
				vector2i2.y = worldPositionCenter.y + i;
				for (int j = -50; j <= 50; j += 50)
				{
					vector2i2.x = worldPositionCenter.x + j;
					num3 += this.worldBuilder.GetHeight(vector2i2);
				}
			}
			this.Height = Mathf.CeilToInt(num3 / 9f);
			this.Height += 3;
			if (this.Streets.Count > 2 && this.Height < 130)
			{
				if (this.BiomeType == BiomeType.snow)
				{
					this.Height += 25;
				}
				else if (this.BiomeType == BiomeType.wasteland)
				{
					this.Height += 12;
				}
			}
			return streetTile2;
		}

		public void AddToUsedPOIList(string name)
		{
			this.worldBuilder.PrefabManager.AddUsedPrefab(name);
		}

		public List<Vector2i> GetUnusedTownExits(int _gatewayUnusedMax = 4)
		{
			this.list.Clear();
			if (this.townshipData.SpawnGateway)
			{
				using (List<StreetTile>.Enumerator enumerator = this.Gateways.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						StreetTile streetTile = enumerator.Current;
						if (streetTile.UsedExitList.Count <= _gatewayUnusedMax)
						{
							foreach (Vector2i vector2i in streetTile.GetHighwayExits(true))
							{
								this.list.Add(vector2i);
							}
						}
					}
					goto IL_0102;
				}
			}
			foreach (StreetTile streetTile2 in this.Streets.Values)
			{
				foreach (Vector2i vector2i2 in streetTile2.GetHighwayExits(false))
				{
					this.list.Add(vector2i2);
				}
			}
			IL_0102:
			return this.list;
		}

		public void AddPrefab(PrefabDataInstance pdi)
		{
			this.Prefabs.Add(pdi);
			this.worldBuilder.PrefabManager.AddUsedPrefabWorld(this.ID, pdi);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool[] GetNeighborExits(StreetTile current)
		{
			bool[] array = new bool[4];
			int num = -1;
			foreach (StreetTile streetTile in current.GetNeighbors())
			{
				num++;
				if (streetTile != null && streetTile.District != null && streetTile.Township != null)
				{
					bool flag = current.District.name == "highway";
					bool flag2 = current.District.name == "gateway";
					bool flag3 = streetTile.District.name == "highway";
					bool flag4 = streetTile.District.name == "gateway";
					if ((streetTile.Township == current.Township || ((flag || flag4 || flag2 || flag3) && (!flag2 || flag3) && (!flag || flag4))) && (streetTile.HasExitTo(current) || (flag && flag3)))
					{
						array[num] = true;
					}
				}
			}
			return array;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int GetNeighborCount(Vector2i current)
		{
			int num = 0;
			for (int i = 0; i < this.worldBuilder.TownshipShared.dir4way.Length; i++)
			{
				Vector2i vector2i = current + this.worldBuilder.TownshipShared.dir4way[i];
				if (this.Streets.ContainsKey(vector2i))
				{
					num++;
				}
			}
			return num;
		}

		public override string ToString()
		{
			return string.Format("Township {0}, {1}, {2}", this.townshipData.Name, this.townshipData.Category, this.ID);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int BUFFER_DISTANCE = 300;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly TownshipData townshipData;

		public int ID;

		public BiomeType BiomeType;

		public Rect Area;

		public Rect BufferArea;

		[PublicizedFrom(EAccessModifier.Private)]
		public StreetTile commercialCap;

		[PublicizedFrom(EAccessModifier.Private)]
		public StreetTile ruralCap;

		public Vector2i GridCenter;

		public int Height;

		[PublicizedFrom(EAccessModifier.Private)]
		public StreetTile centerMostTile;

		public Dictionary<Vector2i, StreetTile> Streets = new Dictionary<Vector2i, StreetTile>();

		public List<PrefabDataInstance> Prefabs = new List<PrefabDataInstance>();

		public List<StreetTile> Gateways = new List<StreetTile>();

		public Dictionary<Township, int> TownshipConnectionCounts = new Dictionary<Township, int>();

		public GameRandom rand;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<Vector2i> list = new List<Vector2i>();
	}
}
