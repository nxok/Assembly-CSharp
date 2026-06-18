using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class WildernessPlanner
	{
		public WildernessPlanner(WorldBuilder _worldBuilder)
		{
			this.worldBuilder = _worldBuilder;
		}

		public void AddPathInfo(Vector2i _startPos, float _pathRadius)
		{
			this.WildernessPathInfos.Add(new WildernessPlanner.WildernessPathInfo(new Vector2i(_startPos), _pathRadius));
		}

		public void Plan(DynamicProperties thisWorldProperties, int worldSeed)
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			this.WildernessPathInfos.Clear();
			int num = 0;
			List<StreetTile> list = new List<StreetTile>(200);
			int num2 = worldSeed + 409651;
			GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(num2);
			for (int i = 0; i < 5; i++)
			{
				int num3 = this.worldBuilder.GetCount(((BiomeType)i).ToString() + "_wilderness", this.worldBuilder.Wilderness, null);
				if (num3 < 0)
				{
					num3 = this.worldBuilder.GetCount("wilderness", this.worldBuilder.Wilderness, null);
				}
				int j = num3;
				if (j < 0)
				{
					j = 200;
					Log.Warning("No wilderness settings in rwgmixer for this world size, using default count of {0}", new object[] { j });
				}
				int num4 = j;
				List<StreetTile> unusedWildernessTiles = this.GetUnusedWildernessTiles((BiomeType)i);
				while (j > 0)
				{
					this.GetUnusedWildernessTiles(unusedWildernessTiles, list);
					if (list.Count == 0)
					{
						break;
					}
					this.worldBuilder.SetTaskMessage(string.Format(this.worldBuilder.messageWildernessPOIs, Mathf.FloorToInt(100f * (1f - (float)j / (float)num4))));
					for (int k = 0; k < 20; k++)
					{
						StreetTile streetTile = list[WildernessPlanner.GetLowBiasedRandom(gameRandom, list.Count)];
						if (!streetTile.Used && streetTile.SpawnPrefabs())
						{
							streetTile.Used = true;
							break;
						}
						num++;
					}
					j--;
				}
			}
			GameRandomManager.Instance.FreeGameRandom(gameRandom);
			Log.Out(string.Format("WildernessPlanner Plan {0} prefabs spawned, in {1}, retries {2}, r={3:x}", new object[]
			{
				this.worldBuilder.WildernessPrefabCount,
				(float)microStopwatch.ElapsedMilliseconds * 0.001f,
				num,
				Rand.Instance.PeekSample()
			}));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static int GetLowBiasedRandom(GameRandom rnd, int max)
		{
			float randomFloat = rnd.RandomFloat;
			return (int)(randomFloat * randomFloat * (float)max);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public List<StreetTile> GetUnusedWildernessTiles(BiomeType _biome)
		{
			return (from StreetTile st in this.worldBuilder.StreetTileMap
				where !st.OverlapsRadiation && !st.AllIsWater && (st.District == null || st.District.name == "wilderness") && !st.Used && st.BiomeType == _biome
				select st).ToList<StreetTile>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void GetUnusedWildernessTiles(List<StreetTile> _list, List<StreetTile> _resultList)
		{
			IOrderedEnumerable<StreetTile> orderedEnumerable = from StreetTile st in _list
				where !st.Used
				orderby this.distanceFromClosestTownship(st) descending
				select st;
			_resultList.Clear();
			_resultList.AddRange(orderedEnumerable);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int distanceFromClosestTownship(StreetTile st)
		{
			int num = int.MaxValue;
			foreach (Township township in this.worldBuilder.Townships)
			{
				int num2 = Vector2i.DistanceSqrInt(st.WorldPositionCenter, this.worldBuilder.GetStreetTileGrid(township.GridCenter).WorldPositionCenter);
				if (num2 < num)
				{
					num = num2;
				}
			}
			return num;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static float distanceSqr(Vector2 pointA, Vector2 pointB)
		{
			Vector2 vector = pointA - pointB;
			return vector.x * vector.x + vector.y * vector.y;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cWildernessSpawnTries = 20;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;

		public readonly List<WildernessPlanner.WildernessPathInfo> WildernessPathInfos = new List<WildernessPlanner.WildernessPathInfo>();

		public class WildernessPathInfo
		{
			public WildernessPathInfo(Vector2i _startPos, float _pathRadius)
			{
				this.Position = _startPos;
				this.PathRadius = _pathRadius;
			}

			public Vector2i Position;

			public float PathRadius;

			public Path Path;

			public float highwayDistance;

			public Vector2 highwayPoint;
		}
	}
}
