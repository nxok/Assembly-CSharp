using System;
using System.Collections;
using System.Xml.Linq;

public class WorldGlobalFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <world> found!");
		}
		foreach (XContainer xcontainer in root.Elements("environment"))
		{
			DynamicProperties dynamicProperties = new DynamicProperties();
			foreach (XElement xelement in xcontainer.Elements("property"))
			{
				dynamicProperties.Add(xelement, false);
			}
			WorldEnvironment.Properties = dynamicProperties;
		}
		WorldEnvironment.OnXMLChanged();
		yield break;
	}

	public static void Reload(XmlFile xmlFile)
	{
		ThreadManager.RunCoroutineSync(WorldGlobalFromXml.Load(xmlFile));
	}
}
