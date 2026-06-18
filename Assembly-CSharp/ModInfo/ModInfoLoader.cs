using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using ICSharpCode.WpfDesign.XamlDom;
using XMLData;

namespace ModInfo
{
	[XmlParser]
	public static class ModInfoLoader
	{
		public static List<ModInfo> ParseXml(string _filename, XmlFile _xml)
		{
			List<ModInfo> list = new List<ModInfo>();
			Dictionary<PositionXmlElement, DataItem<ModInfo>> dictionary = new Dictionary<PositionXmlElement, DataItem<ModInfo>>();
			XElement root = _xml.XmlDoc.Root;
			if (root == null || !root.HasElements)
			{
				Log.Error("No document root or no children found!");
				return list;
			}
			foreach (XElement xelement in root.Elements())
			{
				if (xelement.Name.LocalName == "ModInfo")
				{
					ModInfo modInfo = ModInfo.Parser.Parse(xelement, dictionary);
					if (modInfo != null)
					{
						list.Add(modInfo);
					}
				}
				else
				{
					Log.Warning(string.Format("Unknown element found: {0} (file {1}, line {2})", xelement.Name, _filename, ((IXmlLineInfo)xelement).LineNumber));
				}
			}
			return list;
		}
	}
}
