using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

public class NavObjectClassesFromXml
{
	public static IEnumerator Load(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <nav_object_classes> found!");
		}
		NavObjectClass.NavObjectClassList.Clear();
		NavObjectClassesFromXml.ParseNode(root);
		NavObjectManager.Instance.RefreshNavObjects();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseNode(XElement root)
	{
		foreach (XElement xelement in root.Elements("nav_object_class"))
		{
			NavObjectClassesFromXml.ParseNavObjectClass(xelement);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseNavObjectClass(XElement e)
	{
		if (!e.HasAttribute("name"))
		{
			throw new Exception("nav_object_class must have an name attribute");
		}
		string attribute = e.GetAttribute("name");
		NavObjectClass navObjectClass = new NavObjectClass(attribute);
		NavObjectClass.NavObjectClassList.Add(navObjectClass);
		if (e.HasAttribute("extends"))
		{
			string attribute2 = e.GetAttribute("extends");
			NavObjectClass navObjectClass2 = NavObjectClass.GetNavObjectClass(attribute2);
			if (navObjectClass2 == null)
			{
				throw new Exception(string.Format("Extends nav object {0} is not specified for nav object {1}'", attribute2, attribute));
			}
			NavObjectClassesFromXml.HandleExtends(navObjectClass, navObjectClass2);
		}
		foreach (XElement xelement in e.Elements())
		{
			if (xelement.Name == "property")
			{
				navObjectClass.Properties.Add(xelement, false);
			}
			if (xelement.Name == "inactive_map_settings")
			{
				NavObjectMapSettings navObjectMapSettings = ((navObjectClass.InactiveMapSettings == null) ? new NavObjectMapSettings() : navObjectClass.InactiveMapSettings);
				NavObjectClassesFromXml.ParseSettings(navObjectMapSettings, xelement);
				if (navObjectMapSettings != null)
				{
					navObjectClass.InactiveMapSettings = navObjectMapSettings;
				}
			}
			if (xelement.Name == "map_settings")
			{
				NavObjectMapSettings navObjectMapSettings2 = ((navObjectClass.MapSettings == null) ? new NavObjectMapSettings() : navObjectClass.MapSettings);
				NavObjectClassesFromXml.ParseSettings(navObjectMapSettings2, xelement);
				if (navObjectMapSettings2 != null)
				{
					navObjectClass.MapSettings = navObjectMapSettings2;
				}
			}
			if (xelement.Name == "inactive_onscreen_settings")
			{
				NavObjectScreenSettings navObjectScreenSettings = ((navObjectClass.InactiveOnScreenSettings == null) ? new NavObjectScreenSettings() : navObjectClass.InactiveOnScreenSettings);
				NavObjectClassesFromXml.ParseSettings(navObjectScreenSettings, xelement);
				if (navObjectScreenSettings != null)
				{
					navObjectClass.InactiveOnScreenSettings = navObjectScreenSettings;
				}
			}
			if (xelement.Name == "onscreen_settings")
			{
				NavObjectScreenSettings navObjectScreenSettings2 = ((navObjectClass.OnScreenSettings == null) ? new NavObjectScreenSettings() : navObjectClass.OnScreenSettings);
				NavObjectClassesFromXml.ParseSettings(navObjectScreenSettings2, xelement);
				if (navObjectScreenSettings2 != null)
				{
					navObjectClass.OnScreenSettings = navObjectScreenSettings2;
				}
			}
			if (xelement.Name == "inactive_compass_settings")
			{
				NavObjectCompassSettings navObjectCompassSettings = ((navObjectClass.InactiveCompassSettings == null) ? new NavObjectCompassSettings() : navObjectClass.InactiveCompassSettings);
				NavObjectClassesFromXml.ParseSettings(navObjectCompassSettings, xelement);
				if (navObjectCompassSettings != null)
				{
					navObjectClass.InactiveCompassSettings = navObjectCompassSettings;
				}
			}
			if (xelement.Name == "compass_settings")
			{
				NavObjectCompassSettings navObjectCompassSettings2 = ((navObjectClass.CompassSettings == null) ? new NavObjectCompassSettings() : navObjectClass.CompassSettings);
				NavObjectClassesFromXml.ParseSettings(navObjectCompassSettings2, xelement);
				if (navObjectCompassSettings2 != null)
				{
					navObjectClass.CompassSettings = navObjectCompassSettings2;
				}
			}
		}
		navObjectClass.Init();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseSettings(NavObjectSettings entry, XElement e)
	{
		DynamicProperties dynamicProperties = ((entry.Properties == null) ? new DynamicProperties() : entry.Properties);
		foreach (XElement xelement in e.Elements("property"))
		{
			dynamicProperties.Add(xelement, false);
		}
		entry.Properties = dynamicProperties;
		entry.Init();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void HandleExtends(NavObjectClass navClass, NavObjectClass extendedClass)
	{
		HashSet<string> hashSet = null;
		if (extendedClass.InactiveMapSettings != null)
		{
			navClass.InactiveMapSettings = new NavObjectMapSettings();
			DynamicProperties dynamicProperties = new DynamicProperties();
			dynamicProperties.CopyFrom(extendedClass.InactiveMapSettings.Properties, hashSet);
			navClass.InactiveMapSettings.Properties = dynamicProperties;
			navClass.InactiveMapSettings.Init();
		}
		if (extendedClass.MapSettings != null)
		{
			navClass.MapSettings = new NavObjectMapSettings();
			DynamicProperties dynamicProperties2 = new DynamicProperties();
			dynamicProperties2.CopyFrom(extendedClass.MapSettings.Properties, hashSet);
			navClass.MapSettings.Properties = dynamicProperties2;
			navClass.MapSettings.Init();
		}
		if (extendedClass.InactiveCompassSettings != null)
		{
			navClass.InactiveCompassSettings = new NavObjectCompassSettings();
			DynamicProperties dynamicProperties3 = new DynamicProperties();
			dynamicProperties3.CopyFrom(extendedClass.InactiveCompassSettings.Properties, hashSet);
			navClass.InactiveCompassSettings.Properties = dynamicProperties3;
			navClass.InactiveCompassSettings.Init();
		}
		if (extendedClass.CompassSettings != null)
		{
			navClass.CompassSettings = new NavObjectCompassSettings();
			DynamicProperties dynamicProperties4 = new DynamicProperties();
			dynamicProperties4.CopyFrom(extendedClass.CompassSettings.Properties, hashSet);
			navClass.CompassSettings.Properties = dynamicProperties4;
			navClass.CompassSettings.Init();
		}
		if (extendedClass.InactiveOnScreenSettings != null)
		{
			navClass.InactiveOnScreenSettings = new NavObjectScreenSettings();
			DynamicProperties dynamicProperties5 = new DynamicProperties();
			dynamicProperties5.CopyFrom(extendedClass.InactiveOnScreenSettings.Properties, hashSet);
			navClass.InactiveOnScreenSettings.Properties = dynamicProperties5;
			navClass.InactiveOnScreenSettings.Init();
		}
		if (extendedClass.OnScreenSettings != null)
		{
			navClass.OnScreenSettings = new NavObjectScreenSettings();
			DynamicProperties dynamicProperties6 = new DynamicProperties();
			dynamicProperties6.CopyFrom(extendedClass.OnScreenSettings.Properties, hashSet);
			navClass.OnScreenSettings.Properties = dynamicProperties6;
			navClass.OnScreenSettings.Init();
		}
	}
}
