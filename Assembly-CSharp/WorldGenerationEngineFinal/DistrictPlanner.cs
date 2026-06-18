using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class DistrictPlanner
	{
		public DistrictPlanner(WorldBuilder _worldBuilder)
		{
			this.worldBuilder = _worldBuilder;
		}

		public void PlanTownship(Township _township)
		{
			if (_township.IsRoadside())
			{
				if (_township.Data.SpawnGateway)
				{
					this.GenerateGateway(_township);
				}
				return;
			}
			this.GenerateDistricts(_township);
			if (_township.Data.SpawnGateway)
			{
				int num = (_township.IsBig() ? 2 : 1);
				for (int i = 0; i < num; i++)
				{
					for (int j = 0; j < 4; j++)
					{
						this.GenerateGatewayDir(_township, j);
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void GenerateDistricts(Township _township)
		{
			DistrictPlanner.<>c__DisplayClass3_0 CS$<>8__locals1 = new DistrictPlanner.<>c__DisplayClass3_0();
			if (_township.Streets.Count == 0)
			{
				return;
			}
			GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(this.worldBuilder.Seed + _township.GridCenter.x + _township.GridCenter.y);
			Dictionary<string, District> dictionary = new Dictionary<string, District>();
			float num = 0f;
			string text = _township.GetTypeName().ToLower();
			foreach (KeyValuePair<string, District> keyValuePair in DistrictPlannerStatic.Districts)
			{
				string text2;
				District district;
				keyValuePair.Deconstruct(out text2, out district);
				string text3 = text2;
				District district2 = district;
				if (district2.townships.Test_AnySet(FastTags<TagGroup.Poi>.Parse(text)) && district2.weight > 0f)
				{
					dictionary.Add(text3, new District(district2));
					num += district2.weight;
				}
			}
			foreach (KeyValuePair<string, District> keyValuePair2 in dictionary)
			{
				District value = keyValuePair2.Value;
				value.weight /= num;
				foreach (string text4 in value.avoidedNeighborDistricts)
				{
					District district3;
					if (dictionary.TryGetValue(text4, out district3) && !district3.avoidedNeighborDistricts.Contains(keyValuePair2.Key))
					{
						district3.avoidedNeighborDistricts.Add(keyValuePair2.Key);
					}
				}
			}
			List<string> list = dictionary.OrderByDescending(delegate(KeyValuePair<string, District> entry)
			{
				KeyValuePair<string, District> keyValuePair3 = entry;
				return keyValuePair3.Value.spawnOrder;
			}).Select(delegate(KeyValuePair<string, District> entry)
			{
				KeyValuePair<string, District> keyValuePair4 = entry;
				return keyValuePair4.Key;
			}).ToList<string>();
			List<StreetTile> list2 = new List<StreetTile>();
			foreach (StreetTile streetTile in _township.Streets.Values)
			{
				if (streetTile.District == null)
				{
					list2.Add(streetTile);
				}
			}
			StreetTile streetTile2 = _township.CalcCenterStreetTile();
			CS$<>8__locals1.centerPos = streetTile2.WorldPositionCenter;
			DistrictPlanner.<>c__DisplayClass3_0 CS$<>8__locals2 = CS$<>8__locals1;
			CS$<>8__locals2.centerPos.x = CS$<>8__locals2.centerPos.x + 5;
			list2.Sort(delegate(StreetTile a, StreetTile b)
			{
				float num5 = Vector2i.Distance(CS$<>8__locals1.centerPos, a.WorldPositionCenter);
				float num6 = Vector2i.Distance(CS$<>8__locals1.centerPos, b.WorldPositionCenter);
				return num5.CompareTo(num6);
			});
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				District district4 = dictionary[list[i]];
				int num2 = Mathf.RoundToInt((float)list2.Count * district4.weight);
				if (i == count - 1)
				{
					num2 = int.MaxValue;
				}
				int num3 = 0;
				int num4 = 0;
				while (num4 < list2.Count && num3 < num2)
				{
					StreetTile streetTile3 = list2[num4];
					if (streetTile3.District == null)
					{
						StreetTile streetTile4 = this.FindFreeWithNeighbor(list2, district4);
						if (streetTile4 != null)
						{
							streetTile3 = streetTile4;
							num4--;
						}
						else if (this.HasAvoidNeighbor(streetTile3, district4))
						{
							goto IL_030D;
						}
						num3++;
						streetTile3.District = district4;
						streetTile3.Used = true;
						streetTile3.SetPathingConstraintsForTile(true);
					}
					IL_030D:
					num4++;
				}
			}
			foreach (StreetTile streetTile5 in list2)
			{
				if (streetTile5.District == null)
				{
					streetTile5.SetTownship(null);
					_township.Streets.Remove(streetTile5.GridPosition);
				}
			}
			GameRandomManager.Instance.FreeGameRandom(gameRandom);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public StreetTile FindFreeWithNeighbor(List<StreetTile> _streetTiles, District _district)
		{
			foreach (StreetTile streetTile in _streetTiles)
			{
				if (streetTile.District == _district)
				{
					foreach (StreetTile streetTile2 in streetTile.GetNeighbors())
					{
						if (streetTile2.District == null && _streetTiles.IndexOf(streetTile2) >= 0 && !this.HasAvoidNeighbor(streetTile2, _district))
						{
							return streetTile2;
						}
					}
				}
			}
			foreach (StreetTile streetTile3 in _streetTiles)
			{
				if (streetTile3.District == _district)
				{
					foreach (StreetTile streetTile4 in streetTile3.GetNeighborsDiagonal())
					{
						if (streetTile4.District == null && _streetTiles.IndexOf(streetTile4) >= 0 && !this.HasAvoidNeighbor(streetTile4, _district))
						{
							return streetTile4;
						}
					}
				}
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool HasAvoidNeighbor(StreetTile _st, District _district)
		{
			if (_district.avoidedNeighborDistricts.Count > 0)
			{
				foreach (StreetTile streetTile in _st.GetNeighbors())
				{
					if (streetTile.District != null && _district.avoidedNeighborDistricts.Contains(streetTile.District.name))
					{
						return true;
					}
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void GenerateGateway(Township _township)
		{
			foreach (StreetTile streetTile in _township.Streets.Values)
			{
				streetTile.District = DistrictPlannerStatic.Districts["gateway"];
				streetTile.SetTownship(_township);
				_township.Gateways.Add(streetTile);
				streetTile.SetPathingConstraintsForTile(true);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void GenerateGatewayDir(Township _township, int _baseDir)
		{
			foreach (StreetTile streetTile in _township.Streets.Values)
			{
				if (streetTile.District == null || streetTile.District.type != District.Type.Gateway)
				{
					StreetTile neighbor = streetTile.GetNeighbor(_baseDir);
					if (!_township.Streets.ContainsKey(neighbor.GridPosition) && neighbor.IsValidForGateway)
					{
						int num = 0;
						int num2 = 0;
						int num3 = 0;
						for (int i = 0; i < 4; i++)
						{
							StreetTile neighbor2 = neighbor.GetNeighbor(i);
							if (neighbor2 != null && neighbor2.IsValidForGateway)
							{
								num3 |= 1 << i;
								num2++;
								if (neighbor2.Township != null)
								{
									num++;
									if (num > 1)
									{
										break;
									}
								}
							}
						}
						if (num == 1 && num2 >= 2)
						{
							neighbor.SetTownship(_township);
							neighbor.District = DistrictPlannerStatic.Districts["gateway"];
							neighbor.Used = true;
							neighbor.SetPathingConstraintsForTile(true);
							StreetTile[] neighbors = neighbor.GetNeighbors();
							for (int j = 0; j < neighbors.Length; j++)
							{
								neighbors[j].SetPathingConstraintsForTile(false);
							}
							_township.Streets.Add(neighbor.GridPosition, neighbor);
							_township.Gateways.Add(neighbor);
							break;
						}
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;
	}
}
