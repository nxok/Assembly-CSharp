using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using UAI;
using UnityEngine.Scripting;

[Preserve]
public class UAIFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <ai_packages> found!");
		}
		using (IEnumerator<XElement> enumerator = root.Elements("ai_packages").GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XElement xelement = enumerator.Current;
				UAIFromXml.parseAIPackagesNode(xelement);
			}
			yield break;
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseAIPackagesNode(XElement _element)
	{
		foreach (XElement xelement in _element.Elements("ai_package"))
		{
			UAIFromXml.parseAIPackageNode(xelement);
		}
	}

	public static void Cleanup()
	{
		UAIBase.Cleanup();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseAIPackageNode(XElement _element)
	{
		string text = "";
		float num = 1f;
		if (_element.HasAttribute("name"))
		{
			text = _element.GetAttribute("name");
		}
		if (_element.HasAttribute("weight"))
		{
			num = StringParsers.ParseFloat(_element.GetAttribute("weight"), 0, -1, NumberStyles.Any);
		}
		UAIPackage uaipackage = new UAIPackage(text, num);
		foreach (XElement xelement in _element.Elements("action"))
		{
			UAIFromXml.parseActionNode(uaipackage, xelement);
		}
		if (!UAIBase.AIPackages.ContainsKey(text))
		{
			UAIBase.AIPackages.Add(text, uaipackage);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseActionNode(UAIPackage _package, XElement _element)
	{
		string text = "";
		float num = 1f;
		if (_element.HasAttribute("name"))
		{
			text = _element.GetAttribute("name");
		}
		if (_element.HasAttribute("weight"))
		{
			num = StringParsers.ParseFloat(_element.GetAttribute("weight"), 0, -1, NumberStyles.Any);
		}
		UAIAction uaiaction = new UAIAction(text, num);
		foreach (XElement xelement in _element.Elements())
		{
			if (xelement.Name == "task")
			{
				UAIFromXml.parseTaskNode(uaiaction, xelement);
			}
			if (xelement.Name == "consideration")
			{
				UAIFromXml.parseConsiderationNode(uaiaction, xelement);
			}
		}
		_package.AddAction(uaiaction);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseTaskNode(UAIAction _action, XElement _element)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (XAttribute xattribute in _element.Attributes())
		{
			dictionary.Add(xattribute.Name.LocalName, xattribute.Value);
		}
		if (_element.HasAttribute("class"))
		{
			string attribute = _element.GetAttribute("class");
			Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("UAI.UAITask", attribute);
			if (typeWithPrefix != null)
			{
				UAITaskBase uaitaskBase = (UAITaskBase)Activator.CreateInstance(typeWithPrefix);
				uaitaskBase.Name = attribute;
				uaitaskBase.Parameters = dictionary;
				_action.AddTask(uaitaskBase);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseConsiderationNode(UAIAction _action, XElement _element)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (XAttribute xattribute in _element.Attributes())
		{
			dictionary.Add(xattribute.Name.LocalName, xattribute.Value);
		}
		if (_element.HasAttribute("class"))
		{
			string attribute = _element.GetAttribute("class");
			Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("UAI.UAIConsideration", attribute);
			if (typeWithPrefix != null)
			{
				UAIConsiderationBase uaiconsiderationBase = (UAIConsiderationBase)Activator.CreateInstance(typeWithPrefix);
				uaiconsiderationBase.Name = attribute;
				uaiconsiderationBase.Init(dictionary);
				_action.AddConsideration(uaiconsiderationBase);
			}
		}
	}
}
