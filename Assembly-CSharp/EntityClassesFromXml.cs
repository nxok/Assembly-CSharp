using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

public class EntityClassesFromXml
{
	public static IEnumerator LoadMain(XmlFile _xmlFile)
	{
		return EntityClassesFromXml.Load(_xmlFile, false);
	}

	public static IEnumerator LoadAppend(XmlFile _xmlFile)
	{
		return EntityClassesFromXml.Load(_xmlFile, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator Load(XmlFile _xmlFile, bool _append)
	{
		MicroStopwatch msw = new MicroStopwatch(true);
		if (!_append)
		{
			EntityClass.list.Clear();
			EntityClass.sColors.Clear();
		}
		Dictionary<int, XElement> sEntityClassElements = new Dictionary<int, XElement>();
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <entity_classes> found!");
		}
		foreach (XElement xelement in root.Elements())
		{
			string localName = xelement.Name.LocalName;
			if (localName == "color")
			{
				XElement xelement2 = xelement;
				string attribute = xelement2.GetAttribute("name");
				string attribute2 = xelement2.GetAttribute("value");
				EntityClass.sColors.Add(attribute, StringParsers.ParseColor(attribute2));
			}
			else
			{
				if (localName == "replace_properties")
				{
					EntityClassesFromXml.sReplaceProperties.Clear();
					using (IEnumerator<XElement> enumerator2 = xelement.Elements().GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							XElement xelement3 = enumerator2.Current;
							if (xelement3.Name == "property")
							{
								EntityClassesFromXml.sReplaceProperties.Add("^" + xelement3.GetAttribute("name"), xelement3.GetAttribute("value"));
							}
						}
						goto IL_022A;
					}
				}
				if (localName == "replace_passive_effect")
				{
					EntityClassesFromXml.sReplacePassiveEffects.Clear();
					foreach (XElement xelement4 in xelement.Elements())
					{
						if (xelement4.Name == "property")
						{
							EntityClassesFromXml.sReplacePassiveEffects.Add("^" + xelement4.GetAttribute("name"), xelement4.GetAttribute("value"));
						}
					}
				}
			}
			IL_022A:
			if (localName == "entity_class")
			{
				XElement xelement5 = xelement;
				string attribute3 = xelement5.GetAttribute("name");
				if (attribute3.Length == 0)
				{
					throw new Exception("Attribute 'name' missing on property in entity_class");
				}
				EntityClass entityClass = new EntityClass();
				entityClass.entityClassName = attribute3;
				int num = EntityClass.FromString(entityClass.entityClassName);
				if (!sEntityClassElements.TryAdd(num, xelement5))
				{
					string attribute4 = sEntityClassElements[num].GetAttribute("name");
					throw new ArgumentException(string.Concat(new string[] { "Can not add entity '", attribute3, "' with conflicting hash to existing entity '", attribute4, "'" }));
				}
				string attribute5 = xelement5.GetAttribute("extends");
				if (attribute5.Length > 0)
				{
					int num2 = EntityClass.FromString(attribute5);
					if (!EntityClass.list.ContainsKey(num2))
					{
						throw new Exception("Did not find 'extends' entity '" + attribute5 + "'");
					}
					HashSet<string> hashSet = new HashSet<string>();
					if (xelement5.HasAttribute("ignore"))
					{
						foreach (string text in xelement5.GetAttribute("ignore").Split(',', StringSplitOptions.None))
						{
							hashSet.Add(text.Trim());
						}
					}
					hashSet.Add("HideInSpawnMenu");
					entityClass.CopyFrom(EntityClass.list[num2], hashSet);
					entityClass.Effects = MinEffectController.ParseXml(xelement5, sEntityClassElements[num2], MinEffectController.SourceParentType.EntityClass, num);
				}
				else
				{
					entityClass.Effects = MinEffectController.ParseXml(xelement5, null, MinEffectController.SourceParentType.EntityClass, num);
				}
				foreach (XElement xelement6 in xelement5.Elements())
				{
					string localName2 = xelement6.Name.LocalName;
					if (localName2 == "property")
					{
						entityClass.Properties.Add(xelement6, true);
					}
					else if (localName2 == XNames.array)
					{
						entityClass.Properties.AddArray(xelement6);
					}
					else if (localName2 == "drop")
					{
						XElement xelement7 = xelement6;
						int num3 = 1;
						int num4 = 1;
						if (xelement7.HasAttribute("count"))
						{
							StringParsers.ParseMinMaxCount(xelement7.GetAttribute("count"), out num3, out num4);
						}
						float num5 = 1f;
						if (xelement7.HasAttribute("prob"))
						{
							num5 = StringParsers.ParseFloat(xelement7.GetAttribute("prob"), 0, -1, NumberStyles.Any);
						}
						string attribute6 = xelement7.GetAttribute("name");
						EnumDropEvent enumDropEvent = EnumDropEvent.Destroy;
						if (xelement7.HasAttribute("event"))
						{
							enumDropEvent = EnumUtils.Parse<EnumDropEvent>(xelement7.GetAttribute("event"), false);
						}
						float num6 = 0f;
						if (xelement7.HasAttribute("stick_chance"))
						{
							num6 = StringParsers.ParseFloat(xelement7.GetAttribute("stick_chance"), 0, -1, NumberStyles.Any);
						}
						string text2 = null;
						if (xelement7.HasAttribute("tool_category"))
						{
							text2 = xelement7.GetAttribute("tool_category");
						}
						string text3 = "";
						if (xelement7.HasAttribute("tag"))
						{
							text3 = xelement7.GetAttribute("tag");
						}
						entityClass.AddDroppedId(enumDropEvent, attribute6, num3, num4, num5, num6, text2, text3);
					}
				}
				EntityClass.list[num] = entityClass;
				entityClass.Init();
			}
			if (msw.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
		IEnumerator<XElement> enumerator = null;
		yield break;
		yield break;
	}

	public static string ReplaceProperty(string _value)
	{
		if (_value.Length > 0 && _value[0] == '^')
		{
			_value = EntityClassesFromXml.sReplaceProperties[_value];
		}
		return _value;
	}

	public const char cReplaceChar = '^';

	public static Dictionary<string, string> sReplaceProperties = new Dictionary<string, string>();

	public static Dictionary<string, string> sReplacePassiveEffects = new Dictionary<string, string>();
}
