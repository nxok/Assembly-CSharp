using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

public class BlockPlaceholdersFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		BlockPlaceholderMap.InitStatic();
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <placeholder> found!");
		}
		using (IEnumerator<XElement> enumerator = root.Elements().GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XElement xelement = enumerator.Current;
				if (!xelement.HasAttribute("name"))
				{
					throw new Exception("Attribute 'name' missing on placeholder");
				}
				string attribute = xelement.GetAttribute("name");
				BlockValue blockValue = ItemClass.GetItem(attribute, false).ToBlockValue(false);
				foreach (XElement xelement2 in xelement.Elements("block"))
				{
					if (!xelement2.HasAttribute("name"))
					{
						throw new Exception("Attribute 'name' missing on placeholder block of '" + attribute + "'");
					}
					string attribute2 = xelement2.GetAttribute("name");
					BlockValue blockValue2 = ItemClass.GetItem(attribute2, false).ToBlockValue(false);
					float num = 1f;
					if (xelement2.HasAttribute("prob") && !StringParsers.TryParseFloat(xelement2.GetAttribute("prob"), out num))
					{
						throw new Exception(string.Concat(new string[]
						{
							"Parsing error prob '",
							xelement2.GetAttribute("prob"),
							"' in '",
							attribute2,
							"'"
						}));
					}
					if (num <= 0f)
					{
						throw new Exception(string.Concat(new string[]
						{
							"Parsing error prob '",
							xelement2.GetAttribute("prob"),
							"' in '",
							attribute2,
							"': can not be negative!"
						}));
					}
					string text = null;
					if (xelement2.HasAttribute("biome"))
					{
						text = xelement2.GetAttribute("biome");
					}
					FastTags<TagGroup.Global> fastTags = FastTags<TagGroup.Global>.none;
					if (xelement2.HasAttribute("questtag"))
					{
						fastTags = FastTags<TagGroup.Global>.Parse(xelement2.GetAttribute("questtag"));
					}
					bool flag = false;
					if (xelement2.HasAttribute("randomrotation"))
					{
						flag = StringParsers.ParseBool(xelement2.GetAttribute("randomrotation"), 0, -1, true);
					}
					string text2 = "";
					if (xelement2.HasAttribute("sandboxoption"))
					{
						text2 = xelement2.GetAttribute("sandboxoption");
					}
					if (!fastTags.IsEmpty)
					{
						BlockPlaceholderMap.Instance.AddQuestResetPlaceholder(blockValue, blockValue2, num, text, flag, fastTags);
					}
					else
					{
						BlockPlaceholderMap.Instance.AddPlaceholder(blockValue, blockValue2, num, text, flag, text2);
					}
				}
				BlockPlaceholderMap.Instance.AdjustData(blockValue);
			}
			yield break;
		}
		yield break;
	}
}
