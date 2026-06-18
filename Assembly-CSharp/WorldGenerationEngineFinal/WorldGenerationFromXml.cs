using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public static class WorldGenerationFromXml
	{
		public static void Cleanup()
		{
			WorldBuilderStatic.Properties.Clear();
			WorldBuilderStatic.WorldSizeMapper.Clear();
			WorldBuilderStatic.idToTownshipData.Clear();
			PrefabManagerStatic.prefabWeightData.Clear();
			PrefabManagerStatic.TileMinMaxCounts.Clear();
			PrefabManagerStatic.TileMaxDensityScore.Clear();
			DistrictPlannerStatic.Districts.Clear();
		}

		public static void Reload(XmlFile _xmlFile)
		{
			Debug.LogError("Reloading world generation data!");
			WorldGenerationFromXml.Cleanup();
			ThreadManager.RunCoroutineSync(WorldGenerationFromXml.Load(_xmlFile));
		}

		public static IEnumerator Load(XmlFile file)
		{
			WorldGenerationFromXml.Cleanup();
			int num = 0;
			XElement root = file.XmlDoc.Root;
			if (!root.HasElements)
			{
				throw new Exception("No element <rwgmixer> found!");
			}
			using (IEnumerator<XElement> enumerator = root.Elements().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					XElement xelement = enumerator.Current;
					if (xelement.Name == "world")
					{
						XElement xelement2 = xelement;
						if (xelement2.HasAttribute("name"))
						{
							string attribute = xelement2.GetAttribute("name");
							DynamicProperties dynamicProperties = new DynamicProperties();
							Vector2i one = Vector2i.one;
							foreach (XElement xelement3 in xelement2.Elements("property"))
							{
								dynamicProperties.Add(xelement3, false);
								if (xelement3.HasAttribute("name") && xelement3.GetAttribute("name") == "world_size" && xelement3.HasAttribute("value"))
								{
									WorldBuilderStatic.WorldSizeMapper[attribute] = StringParsers.ParseVector2i(xelement3.GetAttribute("value"), ',');
								}
							}
							WorldBuilderStatic.Properties[attribute] = dynamicProperties;
						}
					}
					else if (xelement.Name == "streettile")
					{
						XElement xelement4 = xelement;
						string attribute2 = xelement4.GetAttribute("name");
						if (attribute2.Length != 0)
						{
							int num2 = 0;
							int num3 = int.MaxValue;
							int num4 = -1;
							foreach (XElement xelement5 in xelement4.Elements("property"))
							{
								string attribute3 = xelement5.GetAttribute("name");
								string attribute4 = xelement5.GetAttribute("value");
								if (attribute3.EqualsCaseInsensitive("maxtiles"))
								{
									num3 = StringParsers.ParseSInt32(attribute4, 0, -1, NumberStyles.Integer);
								}
								else if (attribute3.EqualsCaseInsensitive("mintiles"))
								{
									num2 = StringParsers.ParseSInt32(attribute4, 0, -1, NumberStyles.Integer);
								}
								else if (attribute3.EqualsCaseInsensitive("maxdensity"))
								{
									num4 = StringParsers.ParseSInt32(attribute4, 0, -1, NumberStyles.Integer);
								}
							}
							if (num2 > 0 || num3 != 2147483647)
							{
								PrefabManagerStatic.TileMinMaxCounts[attribute2] = new Vector2i(num2, num3);
							}
							if (num4 >= 0)
							{
								PrefabManagerStatic.TileMaxDensityScore[attribute2] = (float)num4;
							}
						}
					}
					else if (xelement.Name == "district")
					{
						XElement xelement6 = xelement;
						District district = new District();
						district.name = xelement6.GetAttribute("name").ToLower();
						district.prefabName = district.name;
						district.tag = FastTags<TagGroup.Poi>.Parse(district.name);
						foreach (XElement xelement7 in xelement6.Elements("property"))
						{
							string attribute5 = xelement7.GetAttribute("name");
							string text = xelement7.GetAttribute("value").ToLower();
							if (attribute5.Length > 0 && text.Length > 0)
							{
								if (attribute5.EqualsCaseInsensitive("prefab_name"))
								{
									district.prefabName = text;
								}
								else if (attribute5.EqualsCaseInsensitive("tag"))
								{
									district.tag = FastTags<TagGroup.Poi>.Parse(text);
								}
								else if (attribute5.EqualsCaseInsensitive("spawn_weight"))
								{
									district.weight = StringParsers.ParseFloat(text, 0, -1, NumberStyles.Any);
								}
								else if (attribute5.EqualsCaseInsensitive("required_township"))
								{
									district.townships = FastTags<TagGroup.Poi>.Parse(text);
								}
								else if (attribute5.EqualsCaseInsensitive("preview_color"))
								{
									district.preview_color = StringParsers.ParseColor(text);
								}
								else if (attribute5.EqualsCaseInsensitive("spawn_custom_size_prefabs"))
								{
									district.spawnCustomSizePrefabs = StringParsers.ParseBool(text, 0, -1, true);
								}
								else if (attribute5.EqualsCaseInsensitive("avoided_neighbor_districts"))
								{
									district.avoidedNeighborDistricts = new List<string>(text.Split(',', StringSplitOptions.None));
								}
							}
						}
						District district2 = district;
						district2.prefabName += "_";
						district.Init();
						DistrictPlannerStatic.Districts[district.name] = district;
					}
					else
					{
						if (xelement.Name == "township")
						{
							TownshipData townshipData = new TownshipData(xelement.GetAttribute("name"), num);
							num++;
							using (IEnumerator<XElement> enumerator2 = xelement.Elements("property").GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									XElement xelement8 = enumerator2.Current;
									string attribute6 = xelement8.GetAttribute("name");
									string attribute7 = xelement8.GetAttribute("value");
									if (attribute6.Length > 0 && attribute7.Length > 0)
									{
										if (attribute6.EqualsCaseInsensitive("spawnable_terrain"))
										{
											townshipData.SpawnableTerrain.AddRange(attribute7.Replace(" ", "").Split(',', StringSplitOptions.None));
										}
										else if (attribute6.EqualsCaseInsensitive("outskirt_district"))
										{
											string[] array = attribute7.Split(",", StringSplitOptions.None);
											townshipData.OutskirtDistrict = array[0];
											townshipData.OutskirtDistrictPercent = ((array.Length >= 2) ? float.Parse(array[1]) : 1f);
										}
										else if (attribute6.EqualsCaseInsensitive("spawn_custom_size_prefabs"))
										{
											townshipData.SpawnCustomSizes = StringParsers.ParseBool(attribute7, 0, -1, true);
										}
										else if (attribute6.EqualsCaseInsensitive("spawn_trader"))
										{
											townshipData.SpawnTrader = StringParsers.ParseBool(attribute7, 0, -1, true);
										}
										else if (attribute6.EqualsCaseInsensitive("spawn_gateway"))
										{
											townshipData.SpawnGateway = StringParsers.ParseBool(attribute7, 0, -1, true);
										}
										else if (attribute6.EqualsCaseInsensitive("biomes"))
										{
											townshipData.Biomes = FastTags<TagGroup.Poi>.Parse(attribute7.ToLower());
										}
									}
								}
								continue;
							}
						}
						if (xelement.Name == "prefab_spawn_adjust")
						{
							FastTags<TagGroup.Poi> fastTags = FastTags<TagGroup.Poi>.none;
							FastTags<TagGroup.Poi> fastTags2 = FastTags<TagGroup.Poi>.none;
							float num5 = 1f;
							float num6 = 0f;
							int num7 = int.MaxValue;
							int num8 = 1;
							string attribute8 = xelement.GetAttribute("partial_name");
							if (xelement.HasAttribute("tags"))
							{
								fastTags = FastTags<TagGroup.Poi>.Parse(xelement.GetAttribute("tags"));
							}
							string text2;
							if (xelement.TryGetAttribute("biomeTags", out text2))
							{
								fastTags2 = FastTags<TagGroup.Poi>.Parse(text2);
							}
							if (xelement.HasAttribute("weight"))
							{
								num5 = StringParsers.ParseFloat(xelement.GetAttribute("weight"), 0, -1, NumberStyles.Any);
							}
							if (xelement.HasAttribute("bias"))
							{
								num6 = StringParsers.ParseFloat(xelement.GetAttribute("bias"), 0, -1, NumberStyles.Any);
							}
							if (xelement.HasAttribute("min_count"))
							{
								num8 = StringParsers.ParseSInt32(xelement.GetAttribute("min_count"), 0, -1, NumberStyles.Integer);
							}
							if (xelement.HasAttribute("max_count"))
							{
								num7 = StringParsers.ParseSInt32(xelement.GetAttribute("max_count"), 0, -1, NumberStyles.Integer);
							}
							if (!string.IsNullOrEmpty(attribute8) || !fastTags.IsEmpty)
							{
								PrefabManagerStatic.prefabWeightData.Add(new PrefabManager.POIWeightData(attribute8, fastTags, fastTags2, num5, num6, num8, num7));
							}
						}
					}
				}
				yield break;
			}
			yield break;
		}
	}
}
