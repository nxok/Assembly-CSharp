using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using UnityEngine;

public class WorldBiomes
{
	public WorldBiomes(XDocument _genxml, bool _instantiateReferences)
	{
		WorldBiomes.Instance = this;
		this.m_Color2BiomeMap = new Dictionary<uint, BiomeDefinition>();
		this.m_Id2BiomeArr = new BiomeDefinition[256];
		this.m_Name2BiomeMap = new CaseInsensitiveStringDictionary<BiomeDefinition>();
		this.m_PoiMap = new Dictionary<uint, PoiMapElement>();
		this.readXML(_genxml, _instantiateReferences);
	}

	public void Cleanup()
	{
	}

	public static void CleanupStatic()
	{
		if (WorldBiomes.Instance != null)
		{
			WorldBiomes.Instance.Cleanup();
		}
	}

	public int GetBiomeCount()
	{
		if (this.m_Color2BiomeMap == null)
		{
			return 0;
		}
		return this.m_Color2BiomeMap.Count;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Dictionary<uint, BiomeDefinition> GetBiomeMap()
	{
		return this.m_Color2BiomeMap;
	}

	public BiomeDefinition GetBiome(Color32 _color)
	{
		if (this.m_Color2BiomeMap.ContainsKey((uint)(((int)_color.r << 16) | ((int)_color.g << 8) | (int)_color.b)))
		{
			return this.m_Color2BiomeMap[(uint)(((int)_color.r << 16) | ((int)_color.g << 8) | (int)_color.b)];
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BiomeDefinition GetBiome(byte _id)
	{
		return this.m_Id2BiomeArr[(int)_id];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetBiome(byte _id, out BiomeDefinition _bd)
	{
		_bd = this.m_Id2BiomeArr[(int)_id];
		return _bd != null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BiomeDefinition GetBiome(string _name)
	{
		if (!this.m_Name2BiomeMap.ContainsKey(_name))
		{
			return null;
		}
		return this.m_Name2BiomeMap[_name];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void readXML(XDocument _xml, bool _instantiateReferences)
	{
		Array.Clear(this.m_Id2BiomeArr, 0, this.m_Id2BiomeArr.Length);
		this.m_Color2BiomeMap.Clear();
		this.m_Name2BiomeMap.Clear();
		if (BiomeDefinition.nameToId == null)
		{
			BiomeDefinition.nameToId = new Dictionary<string, byte>();
		}
		foreach (XElement xelement in _xml.Descendants("biomemap"))
		{
			string attribute = xelement.GetAttribute("name");
			if (!BiomeDefinition.nameToId.ContainsKey(attribute))
			{
				BiomeDefinition.nameToId.Add(attribute, byte.Parse(xelement.GetAttribute("id")));
			}
		}
		foreach (XElement xelement2 in _xml.Descendants("biome"))
		{
			string attribute2 = xelement2.GetAttribute("name");
			if (!BiomeDefinition.nameToId.ContainsKey(attribute2))
			{
				throw new Exception("Parsing biomes. Biome with name '" + attribute2 + "' also needs an entry in the biomemap");
			}
			byte b = BiomeDefinition.nameToId[attribute2];
			BiomeDefinition biomeDefinition = this.parseBiome(b, 0, attribute2, xelement2, _instantiateReferences);
			this.m_Id2BiomeArr[(int)biomeDefinition.m_Id] = biomeDefinition;
			this.m_Color2BiomeMap.Add(biomeDefinition.m_uiColor, biomeDefinition);
			this.m_Name2BiomeMap.Add(biomeDefinition.m_sBiomeName, biomeDefinition);
		}
		BiomeParticleManager.RegistrationCompleted = true;
		foreach (XElement xelement3 in _xml.Descendants("pois"))
		{
			foreach (XElement xelement4 in xelement3.Descendants("poi"))
			{
				uint num = Convert.ToUInt32(xelement4.GetAttribute("poimapcolor").Substring(1), 16);
				int num2 = 0;
				int num3 = 0;
				if (xelement4.HasAttribute("surfaceoffset"))
				{
					num2 = Convert.ToInt32(xelement4.GetAttribute("surfaceoffset"));
				}
				if (xelement4.HasAttribute("smoothness"))
				{
					int num4 = Convert.ToInt32(xelement4.GetAttribute("smoothness"));
					int num5 = ((num4 < 0) ? 0 : num4);
				}
				if (xelement4.HasAttribute("starttunnel"))
				{
					num3 = Convert.ToInt32(xelement4.GetAttribute("starttunnel"));
					num3 = ((num3 < 0) ? 0 : num3);
				}
				BlockValue blockValue = BlockValue.Air;
				if (xelement4.HasAttribute("blockname"))
				{
					blockValue = (_instantiateReferences ? WorldBiomes.GetBlockValueForName(xelement4.GetAttribute("blockname")) : BlockValue.Air);
				}
				BlockValue blockValue2 = BlockValue.Air;
				if (xelement4.HasAttribute("blockbelow"))
				{
					blockValue2 = (_instantiateReferences ? WorldBiomes.GetBlockValueForName(xelement4.GetAttribute("blockbelow")) : BlockValue.Air);
				}
				int num6 = -1;
				if (xelement4.HasAttribute("ypos"))
				{
					num6 = int.Parse(xelement4.GetAttribute("ypos"));
				}
				int num7 = -1;
				if (xelement4.HasAttribute("yposfill"))
				{
					num7 = int.Parse(xelement4.GetAttribute("yposfill"));
				}
				PoiMapElement poiMapElement = new PoiMapElement(num, xelement4.GetAttribute("prefab"), blockValue, blockValue2, num2, num6, num7, num3);
				this.m_PoiMap.Add(num, poiMapElement);
				foreach (XElement xelement5 in xelement4.Elements())
				{
					if (xelement5.Name == "decal")
					{
						int num8 = Convert.ToInt32(xelement5.GetAttribute("texture"));
						BlockFace blockFace = (BlockFace)Convert.ToInt32(xelement5.GetAttribute("face"));
						float num9 = StringParsers.ParseFloat(xelement5.GetAttribute("prob"), 0, -1, NumberStyles.Any);
						poiMapElement.decals.Add(new PoiMapDecal(num8, blockFace, num9));
					}
					if (xelement5.Name == "blockontop")
					{
						blockValue = (_instantiateReferences ? WorldBiomes.GetBlockValueForName(xelement5.GetAttribute("blockname")) : BlockValue.Air);
						float num10 = StringParsers.ParseFloat(xelement5.GetAttribute("prob"), 0, -1, NumberStyles.Any);
						int num11 = (xelement5.HasAttribute("offset") ? int.Parse(xelement5.GetAttribute("offset")) : 0);
						poiMapElement.blocksOnTop.Add(new PoiMapBlock(blockValue, num10, num11));
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition parseBiome(byte id, byte subId, string name, XElement biomeElement, bool _instantiateReferences)
	{
		uint num = 0U;
		if (biomeElement.HasAttribute("biomemapcolor"))
		{
			num = Convert.ToUInt32(biomeElement.GetAttribute("biomemapcolor").Substring(1), 16);
		}
		int num2 = 0;
		if (biomeElement.HasAttribute("radiationlevel"))
		{
			num2 = int.Parse(biomeElement.GetAttribute("radiationlevel"));
		}
		string text = null;
		if (biomeElement.HasAttribute("buff"))
		{
			text = biomeElement.GetAttribute("buff");
		}
		BiomeDefinition biomeDefinition = new BiomeDefinition(id, subId, name, num, num2, text);
		string text2 = biomeElement.GetAttribute("noise");
		if (text2.Length > 0)
		{
			Vector3 vector = StringParsers.ParseVector3(text2, 0, -1);
			biomeDefinition.noiseFreq = vector.x;
			biomeDefinition.noiseMin = vector.y;
			biomeDefinition.noiseMax = vector.z;
		}
		text2 = biomeElement.GetAttribute("noiseoffset");
		if (text2.Length > 0)
		{
			biomeDefinition.noiseOffset = StringParsers.ParseVector2(text2);
		}
		if (biomeElement.HasAttribute("gamestage_modifier"))
		{
			biomeDefinition.GameStageMod = StringParsers.ParseFloat(biomeElement.GetAttribute("gamestage_modifier"), 0, -1, NumberStyles.Any);
		}
		if (biomeElement.HasAttribute("gamestage_bonus"))
		{
			biomeDefinition.GameStageBonus = StringParsers.ParseFloat(biomeElement.GetAttribute("gamestage_bonus"), 0, -1, NumberStyles.Any);
		}
		if (biomeElement.HasAttribute("lootstage_modifier"))
		{
			biomeDefinition.LootStageMod = StringParsers.ParseFloat(biomeElement.GetAttribute("lootstage_modifier"), 0, -1, NumberStyles.Any);
		}
		if (biomeElement.HasAttribute("lootstage_bonus"))
		{
			biomeDefinition.LootStageBonus = StringParsers.ParseFloat(biomeElement.GetAttribute("lootstage_bonus"), 0, -1, NumberStyles.Any);
		}
		if (biomeElement.HasAttribute("lootstage_min"))
		{
			biomeDefinition.LootStageMin = StringParsers.ParseSInt32(biomeElement.GetAttribute("lootstage_min"), 0, -1, NumberStyles.Integer);
		}
		if (biomeElement.HasAttribute("lootstage_max"))
		{
			biomeDefinition.LootStageMax = StringParsers.ParseSInt32(biomeElement.GetAttribute("lootstage_max"), 0, -1, NumberStyles.Integer);
		}
		if (biomeElement.HasAttribute("difficulty"))
		{
			biomeDefinition.Difficulty = StringParsers.ParseSInt32(biomeElement.GetAttribute("difficulty"), 0, -1, NumberStyles.Integer);
		}
		foreach (XElement xelement in biomeElement.Elements())
		{
			if (xelement.Name == "subbiome")
			{
				subId += 1;
				BiomeDefinition biomeDefinition2 = this.parseBiome(id, subId, name, xelement, _instantiateReferences);
				biomeDefinition.addSubBiome(biomeDefinition2);
				if (biomeDefinition2.m_DecoBlocks.Count == 0 && biomeDefinition2.m_DecoPrefabs.Count == 0)
				{
					biomeDefinition2.m_DecoBlocks = biomeDefinition.m_DecoBlocks;
					biomeDefinition2.m_DistantDecoBlocks = biomeDefinition.m_DistantDecoBlocks;
					biomeDefinition2.m_DecoPrefabs = biomeDefinition.m_DecoPrefabs;
				}
			}
			else if (xelement.Name == "spectrum")
			{
				if (xelement.HasAttribute("name"))
				{
					string attribute = xelement.GetAttribute("name");
					biomeDefinition.m_SpectrumName = attribute;
				}
			}
			else if (xelement.Name == "weather")
			{
				this.ParseWeather(biomeDefinition, xelement);
			}
			else
			{
				if (xelement.Name == "layers")
				{
					using (IEnumerator<XElement> enumerator2 = xelement.Elements("layer").GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							XElement xelement2 = enumerator2.Current;
							int num3 = -1;
							string attribute2 = xelement2.GetAttribute("depth");
							if (attribute2.Length > 0 && !attribute2.Equals("*"))
							{
								num3 = int.Parse(attribute2);
							}
							string attribute3 = xelement2.GetAttribute("blockname");
							BiomeLayer biomeLayer = new BiomeLayer(num3, new BiomeBlockDecoration(attribute3, 1f, 1f, _instantiateReferences, 0, int.MaxValue));
							biomeDefinition.AddLayer(biomeLayer);
							foreach (XElement xelement3 in xelement2.Descendants("resource"))
							{
								float num4 = StringParsers.ParseFloat(xelement3.GetAttribute("prob"), 0, -1, NumberStyles.Any);
								BiomeBlockDecoration biomeBlockDecoration = new BiomeBlockDecoration(Convert.ToString(xelement3.GetAttribute("blockname")), num4, 0f, _instantiateReferences, 0, int.MaxValue);
								biomeLayer.AddResource(biomeBlockDecoration);
							}
						}
						continue;
					}
				}
				if (xelement.Name == "decorations")
				{
					using (IEnumerator<XElement> enumerator2 = xelement.Elements("decoration").GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							XElement xelement4 = enumerator2.Current;
							string attribute4 = xelement4.GetAttribute("type");
							if (attribute4.Equals("block"))
							{
								float num5 = StringParsers.ParseFloat(xelement4.GetAttribute("prob"), 0, -1, NumberStyles.Any);
								float num6 = 0f;
								string attribute5 = xelement4.GetAttribute("blockname");
								int num7 = (xelement4.HasAttribute("rotatemax") ? int.Parse(xelement4.GetAttribute("rotatemax")) : 0);
								int num8 = (xelement4.HasAttribute("checkresource") ? int.Parse(xelement4.GetAttribute("checkresource")) : int.MaxValue);
								biomeDefinition.AddDecoBlock(new BiomeBlockDecoration(attribute5, num5, num6, _instantiateReferences, num7, num8));
							}
							else if (attribute4.Equals("prefab"))
							{
								float num9 = StringParsers.ParseFloat(xelement4.GetAttribute("prob"), 0, -1, NumberStyles.Any);
								string attribute6 = xelement4.GetAttribute("name");
								if (string.IsNullOrEmpty(attribute6))
								{
									throw new Exception("Parsing biomes. No model name specified on prefab in biome '" + name + "'");
								}
								int num10 = (xelement4.HasAttribute("checkresource") ? int.Parse(xelement4.GetAttribute("checkresource")) : 10000);
								bool flag = xelement4.HasAttribute("onslopes") && StringParsers.ParseBool(xelement4.GetAttribute("onslopes"), 0, -1, true);
								biomeDefinition.AddDecoPrefab(new BiomePrefabDecoration(attribute6, num9, flag, num10));
							}
							else
							{
								if (!attribute4.Equals("terrain"))
								{
									throw new Exception("Unknown decoration type " + attribute4);
								}
								float num11 = StringParsers.ParseFloat(xelement4.GetAttribute("prob"), 0, -1, NumberStyles.Any);
								string attribute7 = xelement4.GetAttribute("name");
								if (string.IsNullOrEmpty(attribute7))
								{
									throw new Exception("Parsing biomes. No name specified on terrain in biome '" + name + "'");
								}
								if (_instantiateReferences && !SdFile.Exists(GameIO.GetGameDir("Data/Bluffs") + "/" + attribute7 + ".tga"))
								{
									throw new Exception("Parsing biomes. Prefab with name '" + attribute7 + ".tga' not found!");
								}
								float num12 = 1f;
								float num13 = 1f;
								string text3 = (xelement4.HasAttribute("scale") ? xelement4.GetAttribute("scale") : null);
								if (text3 != null && text3.IndexOf(',') > 0)
								{
									string[] array = text3.Split(',', StringSplitOptions.None);
									num12 = StringParsers.ParseFloat(array[0], 0, -1, NumberStyles.Any);
									num13 = StringParsers.ParseFloat(array[1], 0, -1, NumberStyles.Any);
								}
								else if (text3 != null)
								{
									num13 = (num12 = StringParsers.ParseFloat(text3, 0, -1, NumberStyles.Any));
								}
								biomeDefinition.AddBluff(new BiomeBluffDecoration(attribute7, num11, num12, num13));
							}
						}
						continue;
					}
				}
				if (xelement.Name == "replacements")
				{
					foreach (XElement xelement5 in xelement.Elements("replace"))
					{
						string attribute8 = xelement5.GetAttribute("source");
						string attribute9 = xelement5.GetAttribute("target");
						biomeDefinition.AddReplacement(Block.GetBlockValue(attribute8, false).type, Block.GetBlockValue(attribute9, false).type);
					}
				}
			}
		}
		biomeDefinition.SetupWeather();
		return biomeDefinition;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ParseWeather(BiomeDefinition _bd, XElement _xe)
	{
		string text = "?";
		_xe.ParseAttribute("name", ref text);
		float num = 1f;
		_xe.ParseAttribute("prob", ref num);
		float num2 = 3f;
		_xe.ParseAttribute("duration", ref num2);
		Vector2 zero = Vector2.zero;
		_xe.ParseAttribute("delay", ref zero);
		string attribute = _xe.GetAttribute("buff");
		BiomeDefinition.WeatherGroup weatherGroup = _bd.AddWeatherGroup(text, num, num2, zero, attribute);
		foreach (XElement xelement in _xe.Elements())
		{
			string localName = xelement.Name.LocalName;
			BiomeDefinition.Probabilities.ProbType probType = BiomeDefinition.Probabilities.ProbType.Count;
			Vector2 vector;
			vector.x = 0f;
			vector.y = 100f;
			if (localName.EqualsCaseInsensitive("temperature"))
			{
				probType = BiomeDefinition.Probabilities.ProbType.Temperature;
				vector.x = -50f;
				vector.y = 150f;
			}
			else if (localName.EqualsCaseInsensitive("cloudthickness"))
			{
				probType = BiomeDefinition.Probabilities.ProbType.CloudThickness;
			}
			else if (localName.EqualsCaseInsensitive("precipitation"))
			{
				probType = BiomeDefinition.Probabilities.ProbType.Precipitation;
			}
			else if (localName.EqualsCaseInsensitive("fog"))
			{
				probType = BiomeDefinition.Probabilities.ProbType.Fog;
			}
			else if (localName.EqualsCaseInsensitive("wind"))
			{
				probType = BiomeDefinition.Probabilities.ProbType.Wind;
			}
			else if (localName.EqualsCaseInsensitive("particleeffect"))
			{
				string text2 = ((xelement.GetAttribute("prefab").Length > 0) ? xelement.GetAttribute("prefab") : "error");
				int num3 = (int)((xelement.GetAttribute("ChunkMargin").Length > 0) ? StringParsers.ParseFloat(xelement.GetAttribute("ChunkMargin"), 0, -1, NumberStyles.Any) : 8f);
				BiomeParticleManager.RegisterEffect(_bd.m_sBiomeName, text2, (float)num3);
			}
			else if (localName.EqualsCaseInsensitive("spectrum"))
			{
				string attribute2 = xelement.GetAttribute("name");
				weatherGroup.spectrum = EnumUtils.Parse<SpectrumWeatherType>(attribute2, SpectrumWeatherType.Biome, true);
			}
			if (probType != BiomeDefinition.Probabilities.ProbType.Count)
			{
				xelement.ParseAttribute("min", ref vector.x);
				xelement.ParseAttribute("max", ref vector.y);
				xelement.ParseAttribute("range", ref vector);
				float num4 = 1f;
				xelement.ParseAttribute("prob", ref num4);
				weatherGroup.AddProbability(probType, vector, num4);
			}
		}
	}

	public static BlockValue GetBlockValueForName(string blockname)
	{
		ItemValue item = ItemClass.GetItem(blockname, false);
		if (item.IsEmpty())
		{
			throw new Exception("Block with name '" + blockname + "' not found!");
		}
		return item.ToBlockValue(false);
	}

	public PoiMapElement getPoiForColor(uint uiColor)
	{
		PoiMapElement poiMapElement;
		if (this.m_PoiMap.TryGetValue(uiColor, out poiMapElement))
		{
			return poiMapElement;
		}
		return null;
	}

	public void AddPoiMapElement(PoiMapElement _newElement)
	{
		if (!this.m_PoiMap.ContainsKey(_newElement.m_uColorId))
		{
			this.m_PoiMap.Add(_newElement.m_uColorId, _newElement);
		}
	}

	public int GetTotalBluffsCount()
	{
		int num = 0;
		for (int i = 0; i < this.m_Id2BiomeArr.Length; i++)
		{
			if (this.m_Id2BiomeArr[i] != null)
			{
				num += this.m_Id2BiomeArr[i].m_DecoBluffs.Count;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<uint, BiomeDefinition> m_Color2BiomeMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition[] m_Id2BiomeArr;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, BiomeDefinition> m_Name2BiomeMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<uint, PoiMapElement> m_PoiMap;

	public static WorldBiomes Instance;
}
