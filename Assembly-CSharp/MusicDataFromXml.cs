using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using DynamicMusic.Legacy.ObjectModel;
using MusicUtils.Enums;

public class MusicDataFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <root> found!");
		}
		string text = "@:Sounds/Music/";
		using (IEnumerator<XElement> enumerator = root.Elements().GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XElement xelement = enumerator.Current;
				if (xelement.Name == "property")
				{
					text = xelement.GetAttribute("value");
				}
				if (xelement.Name == "music_groups")
				{
					foreach (XElement xelement2 in xelement.Elements("group"))
					{
						int.Parse(xelement2.GetAttribute("id"));
						int num = int.Parse(xelement2.GetAttribute("sample-rate"));
						byte b = byte.Parse(xelement2.GetAttribute("hyperbar-length"));
						MusicGroup musicGroup = new MusicGroup(num, b);
						foreach (XElement xelement3 in xelement2.Elements("configurations"))
						{
							if (xelement3.Name == "configurations")
							{
								foreach (XElement xelement4 in xelement3.Elements("configuration"))
								{
									musicGroup.ConfigIDs.Add(int.Parse(xelement4.Attribute("id").Value));
								}
							}
							if (xelement3.Name == "threat_level")
							{
								ThreatLevelLegacyType threatLevelLegacyType = EnumUtils.Parse<ThreatLevelLegacyType>(xelement3.GetAttribute("name"), false);
								double num2 = StringParsers.ParseDouble(xelement3.GetAttribute("tempo"), 0, -1, NumberStyles.Any);
								double num3 = StringParsers.ParseDouble(xelement3.GetAttribute("sig-hi"), 0, -1, NumberStyles.Any);
								double num4 = StringParsers.ParseDouble(xelement3.GetAttribute("sig-lo"), 0, -1, NumberStyles.Any);
								ThreatLevel threatLevel;
								musicGroup.Add(threatLevelLegacyType, threatLevel = new ThreatLevel(num2, num3, num4));
								foreach (XElement xelement5 in xelement3.Elements("layer"))
								{
									LayerType layerType = EnumUtils.Parse<LayerType>(xelement5.GetAttribute("name"), false);
									Layer layer;
									threatLevel.Add(layerType, layer = new Layer());
									foreach (XElement xelement6 in xelement5.Elements("instrument_ID"))
									{
										int num5 = int.Parse(xelement6.GetAttribute("id"));
										InstrumentID instrumentID;
										layer.Add(num5, instrumentID = new InstrumentID());
										instrumentID.Name = xelement6.GetAttribute("name");
										instrumentID.SourceName = xelement6.GetAttribute("AudioSource");
										if (!StringParsers.TryParseFloat(xelement6.GetAttribute("Volume"), out instrumentID.Volume))
										{
											instrumentID.Volume = 1f;
										}
										foreach (XElement xelement7 in xelement6.Elements("placement"))
										{
											PlacementType placementType = EnumUtils.Parse<PlacementType>(xelement7.GetAttribute("value"), false);
											string attribute = xelement7.GetAttribute("location");
											instrumentID.Add(placementType, text + attribute);
										}
									}
									layer.PopulateQueue();
								}
							}
						}
						MusicGroup.AllGroups.Add(musicGroup);
					}
				}
				if (xelement.Name == "configurations")
				{
					foreach (XElement xelement8 in xelement.Elements("configuration"))
					{
						int num6 = int.Parse(xelement8.GetAttribute("id"));
						ConfigSet configSet = new ConfigSet();
						foreach (XElement xelement9 in xelement8.Elements("threat_level"))
						{
							ThreatLevelLegacyType threatLevelLegacyType2 = EnumUtils.Parse<ThreatLevelLegacyType>(xelement9.GetAttribute("name"), false);
							ThreatLevelConfig threatLevelConfig;
							configSet.Add(threatLevelLegacyType2, threatLevelConfig = new ThreatLevelConfig());
							foreach (XElement xelement10 in xelement9.Elements("layer"))
							{
								LayerType layerType2 = EnumUtils.Parse<LayerType>(xelement10.GetAttribute("name"), false);
								LayerConfig layerConfig;
								threatLevelConfig.Add(layerType2, layerConfig = new LayerConfig());
								string[] array = xelement10.GetAttribute("values").Split(',', StringSplitOptions.None);
								for (int i = 0; i < array.Length; i++)
								{
									layerConfig.Add(byte.Parse(array[i]), (i != 0 && i != array.Length - 1) ? PlacementType.Loop : ((i == 0) ? PlacementType.Begin : PlacementType.End));
								}
							}
						}
						ConfigSet.AllConfigSets.Add(num6, configSet);
					}
				}
			}
			yield break;
		}
		yield break;
	}
}
