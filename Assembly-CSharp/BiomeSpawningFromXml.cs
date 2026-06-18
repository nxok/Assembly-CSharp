using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

public class BiomeSpawningFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <spawning> found!");
		}
		using (IEnumerator<XElement> enumerator = root.Elements("biome").GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XElement xelement = enumerator.Current;
				string attribute = xelement.GetAttribute("name");
				if (attribute.Length == 0)
				{
					throw new Exception("Attribute 'name' missing on biome tag");
				}
				BiomeSpawnEntityGroupList biomeSpawnEntityGroupList = new BiomeSpawnEntityGroupList();
				BiomeSpawningClass.list[attribute] = biomeSpawnEntityGroupList;
				foreach (XElement xelement2 in xelement.Elements("spawn"))
				{
					string attribute2 = xelement2.GetAttribute("id");
					int hashCode = attribute2.GetHashCode();
					if (biomeSpawnEntityGroupList.Find(hashCode) != null)
					{
						throw new Exception(string.Concat(new string[] { "Duplicate id hash '", attribute2, "' in biome '", attribute, "'" }));
					}
					int num = 1;
					if (xelement2.HasAttribute("maxcount"))
					{
						num = int.Parse(xelement2.GetAttribute("maxcount"));
					}
					int[] array = null;
					if (xelement2.HasAttribute("respawndelay"))
					{
						string[] array2 = xelement2.GetAttribute("respawndelay").Split(',', StringSplitOptions.None);
						array = new int[array2.Length];
						for (int i = 0; i < array2.Length; i++)
						{
							array[i] = (int)(StringParsers.ParseFloat(array2[i], 0, -1, NumberStyles.Any) * 24000f);
						}
					}
					EDaytime edaytime = EDaytime.Any;
					if (xelement2.HasAttribute("time"))
					{
						edaytime = EnumUtils.Parse<EDaytime>(xelement2.GetAttribute("time"), false);
					}
					bool flag = false;
					if (xelement2.HasAttribute("isanimal"))
					{
						flag = StringParsers.ParseBool(xelement2.GetAttribute("isanimal"), 0, -1, true);
					}
					BiomeSpawnEntityGroupData biomeSpawnEntityGroupData = new BiomeSpawnEntityGroupData(hashCode, num, array, edaytime, flag);
					string text = xelement2.GetAttribute("tags");
					if (text.Length > 0)
					{
						biomeSpawnEntityGroupData.POITags = FastTags<TagGroup.Poi>.Parse(text);
					}
					text = xelement2.GetAttribute("notags");
					if (text.Length > 0)
					{
						biomeSpawnEntityGroupData.noPOITags = FastTags<TagGroup.Poi>.Parse(text);
					}
					string attribute3 = xelement2.GetAttribute("entitygroup");
					if (attribute3.Length == 0)
					{
						throw new Exception("Missing attribute 'entitygroup' in entitygroup of biome '" + attribute + "'");
					}
					if (!EntityGroups.list.ContainsKey(attribute3))
					{
						throw new Exception("Entity group '" + attribute3 + "' not existing!");
					}
					biomeSpawnEntityGroupData.entityGroupName = attribute3;
					biomeSpawnEntityGroupList.list.Add(biomeSpawnEntityGroupData);
				}
			}
			yield break;
		}
		yield break;
	}
}
