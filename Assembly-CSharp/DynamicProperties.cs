using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using UnityEngine;

[MemoryPackable(0)]
public class DynamicProperties : IMemoryPackable<DynamicProperties>, IMemoryPackFormatterRegister
{
	public Dictionary<string, string> ParseKeyData(string key)
	{
		try
		{
			string text;
			if (this.Data.TryGetValue(key, out text))
			{
				return DynamicProperties.ParseData(text);
			}
		}
		catch (Exception ex)
		{
			Log.Error("ParseKeyData error parsing key {0}, {1}", new object[] { key, ex });
		}
		return null;
	}

	public static Dictionary<string, string> ParseData(string data)
	{
		Dictionary<string, string> dictionary = null;
		try
		{
			dictionary = new Dictionary<string, string>();
			if (data.IndexOf(';') < 0)
			{
				string[] array = data.Split(DynamicProperties.equalSeparator, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length >= 2)
				{
					dictionary[array[0]] = array[1];
				}
			}
			else
			{
				string[] array2 = data.Split(DynamicProperties.semicolonSeparator, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < array2.Length; i++)
				{
					string[] array3 = array2[i].Split(DynamicProperties.equalSeparator, StringSplitOptions.RemoveEmptyEntries);
					if (array3.Length >= 2)
					{
						dictionary[array3[0]] = array3[1];
					}
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("ParseData error parsing {0}, {1}", new object[] { data, ex });
		}
		return dictionary;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ValidateKey(string _key)
	{
		if (string.IsNullOrEmpty(_key) || _key.IndexOf('.') >= 0)
		{
			throw new Exception(string.Format("Property name '{0}' contains illegal character '{1}'", _key, '.'));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicProperties GetOrCreateClass(string _className)
	{
		DynamicProperties.ValidateKey(_className);
		DynamicProperties dynamicProperties;
		if (!this.Classes.TryGetValue(_className, out dynamicProperties))
		{
			dynamicProperties = new DynamicProperties();
			this.Classes[_className] = dynamicProperties;
		}
		return dynamicProperties;
	}

	public DynamicProperties GetClass(string _className)
	{
		DynamicProperties dynamicProperties;
		if (this.Classes.TryGetValue(_className, out dynamicProperties))
		{
			return dynamicProperties;
		}
		return null;
	}

	public void SetValue(string _propName, string _value)
	{
		DynamicProperties.ValidateKey(_propName);
		this.Values[_propName] = _value;
	}

	public void SetValue(string _className, string _propName, string _value)
	{
		this.GetOrCreateClass(_className).SetValue(_propName, _value);
	}

	public string GetValue(string _propName)
	{
		string text;
		if (this.Values.TryGetValue(_propName, out text))
		{
			return text;
		}
		return null;
	}

	public bool TryGetValue(string _propName, out string _value)
	{
		DynamicProperties.ValidateKey(_propName);
		return this.Values.TryGetValue(_propName, out _value);
	}

	public bool TryGetValue(string _className, string _propName, out string _value)
	{
		DynamicProperties.ValidateKey(_className);
		DynamicProperties dynamicProperties;
		if (!this.Classes.TryGetValue(_className, out dynamicProperties))
		{
			_value = null;
			return false;
		}
		return dynamicProperties.TryGetValue(_propName, out _value);
	}

	public bool Contains(string _propName)
	{
		DynamicProperties.ValidateKey(_propName);
		return this.Values.ContainsKey(_propName);
	}

	public bool Contains(string _className, string _propName)
	{
		string text;
		return this.TryGetValue(_className, _propName, out text);
	}

	public void SetParam1(string _propName, string _param1)
	{
		DynamicProperties.ValidateKey(_propName);
		if (!this.Values.ContainsKey(_propName))
		{
			this.Values.Add(_propName, null);
		}
		if (this.Params1.ContainsKey(_propName))
		{
			this.Params1[_propName] = _param1;
			return;
		}
		this.Params1.Add(_propName, _param1);
	}

	public void SetParam1(string _className, string _propName, string _param1)
	{
		this.GetOrCreateClass(_className).SetParam1(_propName, _param1);
	}

	public bool TryGetParam1(string _propName, out string _param1)
	{
		DynamicProperties.ValidateKey(_propName);
		return this.Params1.TryGetValue(_propName, out _param1);
	}

	public bool TryGetParam1(string _className, string _propName, out string _param1)
	{
		DynamicProperties.ValidateKey(_className);
		DynamicProperties dynamicProperties;
		if (!this.Classes.TryGetValue(_className, out dynamicProperties))
		{
			_param1 = null;
			return false;
		}
		return dynamicProperties.TryGetParam1(_propName, out _param1);
	}

	public string GetParam1(string _propName)
	{
		string text;
		if (this.Params1.TryGetValue(_propName, out text))
		{
			return text;
		}
		return null;
	}

	public bool GetBool(string _propName)
	{
		string text;
		if (!this.TryGetValue(_propName, out text))
		{
			return false;
		}
		bool flag;
		StringParsers.TryParseBool(text, out flag);
		return flag;
	}

	public bool GetBool(string _className, string _propName)
	{
		string text;
		if (!this.TryGetValue(_className, _propName, out text))
		{
			return false;
		}
		bool flag;
		StringParsers.TryParseBool(text, out flag);
		return flag;
	}

	public float GetFloat(string _propName)
	{
		string text;
		if (!this.TryGetValue(_propName, out text))
		{
			return 0f;
		}
		float num;
		StringParsers.TryParseFloat(text, out num);
		return num;
	}

	public float GetFloat(string _className, string _propName)
	{
		string text;
		if (!this.TryGetValue(_className, _propName, out text))
		{
			return 0f;
		}
		float num;
		StringParsers.TryParseFloat(text, out num);
		return num;
	}

	public int GetInt(string _propName)
	{
		string text;
		if (!this.TryGetValue(_propName, out text))
		{
			return 0;
		}
		int num;
		int.TryParse(text, out num);
		return num;
	}

	public int GetInt(string _className, string _propName)
	{
		string text;
		if (!this.TryGetValue(_className, _propName, out text))
		{
			return 0;
		}
		int num;
		int.TryParse(text, out num);
		return num;
	}

	public string GetString(string _propName)
	{
		string text;
		if (!this.TryGetValue(_propName, out text))
		{
			return string.Empty;
		}
		return text;
	}

	public string GetString(string _className, string _propName)
	{
		string text;
		if (!this.TryGetValue(_className, _propName, out text))
		{
			return string.Empty;
		}
		return text;
	}

	public bool Load(string _directory, string _name)
	{
		try
		{
			foreach (XElement xelement in new XmlFile(_directory, _name, false, false).XmlDoc.Root.Elements(XNames.property))
			{
				this.Add(xelement, false);
			}
			return true;
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
		return false;
	}

	public bool Save(string _rootNodeName, Stream stream)
	{
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
			xmlDocument.InsertBefore(xmlDeclaration, xmlDocument.DocumentElement);
			XmlNode xmlNode = xmlDocument.AppendChild(xmlDocument.CreateElement(_rootNodeName));
			this.toXml(xmlDocument, xmlNode);
			xmlDocument.Save(stream);
			return true;
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
		return false;
	}

	public bool Save(string _rootNodeName, string _path, string _name)
	{
		try
		{
			using (Stream stream = SdFile.Create(Path.Join(_path, _name + ".xml")))
			{
				return this.Save(_rootNodeName, stream);
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toXml(XmlDocument _doc, XmlNode _parent)
	{
		foreach (KeyValuePair<string, string> keyValuePair in this.Values)
		{
			XmlElement xmlElement = _doc.CreateElement("property");
			XmlAttribute xmlAttribute = _doc.CreateAttribute("name");
			xmlAttribute.Value = keyValuePair.Key;
			xmlElement.Attributes.Append(xmlAttribute);
			XmlAttribute xmlAttribute2 = _doc.CreateAttribute("value");
			xmlAttribute2.Value = this.Values[keyValuePair.Key];
			xmlElement.Attributes.Append(xmlAttribute2);
			if (this.Params1.ContainsKey(keyValuePair.Key))
			{
				XmlAttribute xmlAttribute3 = _doc.CreateAttribute("param1");
				xmlAttribute3.Value = this.Params1[keyValuePair.Key];
				xmlElement.Attributes.Append(xmlAttribute3);
			}
			if (this.Data.ContainsKey(keyValuePair.Key))
			{
				XmlAttribute xmlAttribute4 = _doc.CreateAttribute("fields");
				xmlAttribute4.Value = this.Data[keyValuePair.Key];
				xmlElement.Attributes.Append(xmlAttribute4);
			}
			_parent.AppendChild(xmlElement);
		}
		if (this.Classes.Count > 0)
		{
			foreach (KeyValuePair<string, DynamicProperties> keyValuePair2 in this.Classes)
			{
				XmlElement xmlElement2 = _doc.CreateElement("property");
				XmlAttribute xmlAttribute5 = _doc.CreateAttribute("class");
				xmlAttribute5.Value = keyValuePair2.Key;
				xmlElement2.Attributes.Append(xmlAttribute5);
				keyValuePair2.Value.toXml(_doc, xmlElement2);
				_parent.AppendChild(xmlElement2);
			}
		}
	}

	public void Add(XElement _propertyNode, bool _doValueReplace = false)
	{
		this.Parse(_propertyNode, _doValueReplace);
	}

	public void AddArray(XElement _arrayNode)
	{
		string attribute = _arrayNode.GetAttribute(XNames.name);
		if (!string.IsNullOrEmpty(attribute))
		{
			List<Dictionary<string, string>> list;
			if (!this.Array.TryGetValue(attribute, out list) || list == null)
			{
				list = new List<Dictionary<string, string>>();
				this.Array[attribute] = list;
			}
			list.Clear();
			foreach (XElement xelement in _arrayNode.Elements(XNames.item))
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				list.Add(dictionary);
				foreach (XAttribute xattribute in xelement.Attributes())
				{
					dictionary[xattribute.Name.ToString()] = xattribute.Value;
				}
			}
		}
	}

	public void Parse(XElement elementProperty, bool _doValueReplace = false)
	{
		if (elementProperty.HasAttribute(XNames.class_))
		{
			string attribute = elementProperty.GetAttribute(XNames.class_);
			DynamicProperties orCreateClass = this.GetOrCreateClass(attribute);
			using (IEnumerator<XElement> enumerator = elementProperty.Elements(XNames.property).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					XElement xelement = enumerator.Current;
					orCreateClass.Parse(xelement, _doValueReplace);
				}
				return;
			}
		}
		string attribute2 = elementProperty.GetAttribute(XNames.name);
		if (attribute2.Length == 0)
		{
			throw new Exception("Attribute 'name' missing on property");
		}
		DynamicProperties.ValidateKey(attribute2);
		string text = attribute2;
		string text2 = elementProperty.GetAttribute(XNames.value);
		if (_doValueReplace)
		{
			text2 = EntityClassesFromXml.ReplaceProperty(text2);
		}
		string text3 = null;
		if (elementProperty.HasAttribute(XNames.param1))
		{
			text3 = elementProperty.GetAttribute(XNames.param1);
		}
		string text4 = null;
		if (elementProperty.HasAttribute(XNames.param2))
		{
			text4 = elementProperty.GetAttribute(XNames.param2);
		}
		if (text3 != null)
		{
			this.Params1[text] = text3;
		}
		if (text4 != null)
		{
			this.Params2[text] = text4;
		}
		if (text2 != null)
		{
			string attribute3 = elementProperty.GetAttribute(XNames.data);
			if (attribute3.Length > 0)
			{
				this.Data[text] = attribute3;
			}
		}
		if (this.Classes.ContainsKey(text))
		{
			throw new Exception("Cannot create property '" + text + "': a class with the same name already exists. Property and class names must be unique.");
		}
		this.Values[text] = text2;
	}

	public void Clear()
	{
		this.Values.Clear();
		this.Params1.Clear();
		this.Params2.Clear();
		this.Data.Clear();
		this.Classes.Clear();
	}

	public void ParseString(string _propName, ref string optionalValue)
	{
		string text;
		if (this.TryGetValue(_propName, out text))
		{
			optionalValue = text;
		}
	}

	public void ParseString(string _className, string _propName, ref string optionalValue)
	{
		string text;
		if (this.TryGetValue(_className, _propName, out text))
		{
			optionalValue = text;
		}
	}

	public void ParseStringFloatDictWithSubStringKey(string _propName, char _propSeparator, Dictionary<string, string> _dict, ref Dictionary<string, float> optionalValue)
	{
		foreach (KeyValuePair<string, string> keyValuePair in _dict)
		{
			if (keyValuePair.Key.StartsWith(_propName))
			{
				string text = keyValuePair.Key.Substring(keyValuePair.Key.IndexOf(_propSeparator) + 1);
				if (text.Length != keyValuePair.Key.Length)
				{
					optionalValue.Add(text, StringParsers.ParseFloat(keyValuePair.Value, 0, -1, NumberStyles.Any));
				}
			}
		}
	}

	public IntRange TryParseRange(string _propName, IntRange defaultValue = default(IntRange))
	{
		IntRange intRange = defaultValue;
		string text;
		if (this.TryGetValue(_propName, out text))
		{
			try
			{
				if (!StringParsers.TryParseRange(text, out intRange, null, '-'))
				{
					intRange = defaultValue;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		return intRange;
	}

	public FloatRange TryParseRange(string _propName, FloatRange defaultValue = default(FloatRange))
	{
		FloatRange floatRange = defaultValue;
		string text;
		if (this.TryGetValue(_propName, out text))
		{
			try
			{
				if (!StringParsers.TryParseRange(text, out floatRange, null, '-'))
				{
					floatRange = defaultValue;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		return floatRange;
	}

	public string GetLocalizedString(string _propName)
	{
		string text;
		if (this.TryGetValue(_propName, out text))
		{
			return text;
		}
		return string.Empty;
	}

	public void ParseLocalizedString(string _propName, ref string optionalValue)
	{
		string text;
		if (this.TryGetValue(_propName, out text))
		{
			optionalValue = Localization.Get(text, false, null);
		}
	}

	public void ParseBool(string _propName, ref bool optionalValue)
	{
		string text;
		if (this.TryGetValue(_propName, out text))
		{
			bool flag;
			if (StringParsers.TryParseBool(text, out flag))
			{
				optionalValue = flag;
				return;
			}
			Log.Warning("Can't parse bool {0} '{1}'", new object[] { _propName, text });
		}
	}

	public void ParseByte(string _propName, ref byte optionalValue)
	{
		string text;
		if (this.TryGetValue(_propName, out text))
		{
			byte b;
			if (StringParsers.TryParseUInt8(text, out b))
			{
				optionalValue = b;
				return;
			}
			Log.Warning("Can't parse byte {0} '{1}'", new object[] { _propName, text });
		}
	}

	public void ParseColor(string _propName, ref Color optionalValue)
	{
		string text;
		if (this.TryGetValue(_propName, out text))
		{
			optionalValue = StringParsers.ParseColor32(text);
		}
	}

	public void ParseColorHex(string _propName, ref Color optionalValue)
	{
		string text;
		if (this.TryGetValue(_propName, out text))
		{
			optionalValue = StringParsers.ParseHexColor(text);
		}
	}

	public void ParseEnum<T>(string _propName, ref T optionalValue) where T : struct, IConvertible
	{
		string text;
		T t;
		if (this.TryGetValue(_propName, out text) && EnumUtils.TryParse<T>(text, out t, true))
		{
			optionalValue = t;
		}
	}

	public void ParseFloat(string _propName, ref float optionalValue)
	{
		string text;
		if (this.TryGetValue(_propName, out text))
		{
			float num;
			if (StringParsers.TryParseFloat(text, out num))
			{
				optionalValue = num;
				return;
			}
			Log.Warning("Can't parse float {0} '{1}'", new object[] { _propName, text });
		}
	}

	public void ParseFloat(string _className, string _propName, ref float optionalValue)
	{
		string text;
		if (this.TryGetValue(_className, _propName, out text))
		{
			float num;
			if (StringParsers.TryParseFloat(text, out num))
			{
				optionalValue = num;
				return;
			}
			Log.Warning("Can't parse float '{0}'", new object[] { text });
		}
	}

	public static void ParseFloat(XElement _e, string _propName, ref float optionalValue)
	{
		XAttribute xattribute = _e.Attribute(_propName);
		if (xattribute != null)
		{
			string value = xattribute.Value;
			float num;
			if (StringParsers.TryParseFloat(value, out num))
			{
				optionalValue = num;
				return;
			}
			Log.Warning("Can't parse float {0} '{1}'", new object[] { _propName, value });
		}
	}

	public void ParseInt(string _propName, ref int optionalValue)
	{
		string text;
		if (this.TryGetValue(_propName, out text))
		{
			int num;
			if (StringParsers.TryParseSInt32(text, out num))
			{
				optionalValue = num;
				return;
			}
			Log.Warning("Can't parse int {0} '{1}'", new object[] { _propName, text });
		}
	}

	public void ParseInt(string _className, string _propName, ref int optionalValue)
	{
		string text;
		if (this.TryGetValue(_className, _propName, out text))
		{
			int num;
			if (StringParsers.TryParseSInt32(text, out num))
			{
				optionalValue = num;
				return;
			}
			Log.Warning("Can't parse int {0}.{1} '{2}'", new object[] { _className, _propName, text });
		}
	}

	public void ParseVec(string _propName, ref Vector2 optionalValue)
	{
		string text;
		if (this.TryGetValue(_propName, out text))
		{
			optionalValue = StringParsers.ParseVector2(text);
		}
	}

	public void ParseVec(string _className, string _propName, ref Vector2 optionalValue)
	{
		string text;
		if (this.TryGetValue(_className, _propName, out text))
		{
			optionalValue = StringParsers.ParseVector2(text);
		}
	}

	public void ParseVec(string _propName, ref Vector2 optionalValue, float _defaultValue)
	{
		string text;
		if (this.TryGetValue(_propName, out text))
		{
			optionalValue = StringParsers.ParseVector2(text, _defaultValue);
		}
	}

	public void ParseVec(string _className, string _propName, ref Vector2 optionalValue, float _defaultValue)
	{
		string text;
		if (this.TryGetValue(_className, _propName, out text))
		{
			optionalValue = StringParsers.ParseVector2(text, _defaultValue);
		}
	}

	public void ParseVec(string _propName, ref Vector3 optionalValue)
	{
		string text;
		if (this.TryGetValue(_propName, out text))
		{
			optionalValue = StringParsers.ParseVector3(text, 0, -1);
		}
	}

	public void ParseVec(string _className, string _propName, ref Vector3 optionalValue)
	{
		string text;
		if (this.TryGetValue(_className, _propName, out text))
		{
			optionalValue = StringParsers.ParseVector3(text, 0, -1);
		}
	}

	public void ParseVec(string _propName, ref Vector3 optionalValue, float _defaultValue)
	{
		string text;
		if (this.TryGetValue(_propName, out text))
		{
			optionalValue = StringParsers.ParseVector3(text, _defaultValue);
		}
	}

	public void ParseVec(string _className, string _propName, ref Vector3 optionalValue, float _defaultValue)
	{
		string text;
		if (this.TryGetValue(_className, _propName, out text))
		{
			optionalValue = StringParsers.ParseVector3(text, _defaultValue);
		}
	}

	public void ParseVec(string _propName, ref Vector3i optionalValue)
	{
		string text;
		if (this.TryGetValue(_propName, out text))
		{
			optionalValue = StringParsers.ParseVector3i(text, 0, -1, false);
		}
	}

	public void ParseVec(string _className, string _propName, ref Vector3i optionalValue)
	{
		string text;
		if (this.TryGetValue(_className, _propName, out text))
		{
			optionalValue = StringParsers.ParseVector3i(text, 0, -1, false);
		}
	}

	public void ParseVec(string _propName, ref float optionalValue1, ref float optionalValue2)
	{
		string text;
		if (this.TryGetValue(_propName, out text))
		{
			Vector2 vector = StringParsers.ParseVector2(text);
			optionalValue1 = vector.x;
			optionalValue2 = vector.y;
		}
	}

	public void ParseVec(string _className, string _propName, ref float optionalValue1, ref float optionalValue2)
	{
		string text;
		if (this.TryGetValue(_className, _propName, out text))
		{
			Vector2 vector = StringParsers.ParseVector2(text);
			optionalValue1 = vector.x;
			optionalValue2 = vector.y;
		}
	}

	public void ParseVec(string _propName, ref float optionalValue1, ref float optionalValue2, ref float optionalValue3)
	{
		string text;
		if (this.TryGetValue(_propName, out text))
		{
			Vector3 vector = StringParsers.ParseVector3(text, 0, -1);
			optionalValue1 = vector.x;
			optionalValue2 = vector.y;
			optionalValue3 = vector.z;
		}
	}

	public void ParseVec(string _className, string _propName, ref float optionalValue1, ref float optionalValue2, ref float optionalValue3)
	{
		string text;
		if (this.TryGetValue(_className, _propName, out text))
		{
			Vector3 vector = StringParsers.ParseVector3(text, 0, -1);
			optionalValue1 = vector.x;
			optionalValue2 = vector.y;
			optionalValue3 = vector.z;
		}
	}

	public void ParseVec(string _propName, ref float optionalValue1, ref float optionalValue2, ref float optionalValue3, ref float optionalValue4)
	{
		string text;
		if (this.TryGetValue(_propName, out text))
		{
			Vector4 vector = StringParsers.ParseVector4(text);
			optionalValue1 = vector.x;
			optionalValue2 = vector.y;
			optionalValue3 = vector.z;
			optionalValue4 = vector.w;
		}
	}

	public void ParseVec(string _className, string _propName, ref float optionalValue1, ref float optionalValue2, ref float optionalValue3, ref float optionalValue4)
	{
		string text;
		if (this.TryGetValue(_className, _propName, out text))
		{
			Vector4 vector = StringParsers.ParseVector4(text);
			optionalValue1 = vector.x;
			optionalValue2 = vector.y;
			optionalValue3 = vector.z;
			optionalValue4 = vector.w;
		}
	}

	public void CopyFrom(DynamicProperties _other, HashSet<string> _exclude = null)
	{
		DynamicProperties.copyDict(_other.Values, this.Values, _exclude);
		DynamicProperties.copyDict(_other.Params1, this.Params1, _exclude);
		DynamicProperties.copyDict(_other.Params2, this.Params2, _exclude);
		DynamicProperties.copyDict(_other.Data, this.Data, _exclude);
		foreach (KeyValuePair<string, DynamicProperties> keyValuePair in _other.Classes)
		{
			if (_exclude == null || !_exclude.Contains(keyValuePair.Key))
			{
				DynamicProperties dynamicProperties = (this.Classes.ContainsKey(keyValuePair.Key) ? this.Classes[keyValuePair.Key] : new DynamicProperties());
				this.Classes[keyValuePair.Key] = dynamicProperties;
				HashSet<string> nestedExclusions = DynamicProperties.GetNestedExclusions(keyValuePair.Key, _exclude);
				dynamicProperties.CopyFrom(keyValuePair.Value, nestedExclusions);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<string> GetNestedExclusions(string _className, HashSet<string> _exclude)
	{
		if (_exclude == null)
		{
			return null;
		}
		HashSet<string> hashSet = null;
		string text = _className + ".";
		foreach (string text2 in _exclude)
		{
			if (text2.StartsWith(text))
			{
				if (hashSet == null)
				{
					hashSet = new HashSet<string>();
				}
				hashSet.Add(text2.Substring(text.Length));
			}
		}
		return hashSet;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void copyDict(Dictionary<string, string> _source, Dictionary<string, string> _dest, HashSet<string> _exclude)
	{
		foreach (KeyValuePair<string, string> keyValuePair in _source)
		{
			if (DynamicProperties.copyKey(keyValuePair.Key, _exclude))
			{
				_dest[keyValuePair.Key] = keyValuePair.Value;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool copyKey(string _key, HashSet<string> _exclude)
	{
		if (_exclude == null)
		{
			return true;
		}
		if (_exclude.Contains(_key))
		{
			return false;
		}
		if (_key.IndexOf('.') > 0)
		{
			foreach (string text in _exclude)
			{
				if (_key.StartsWith(text + "."))
				{
					return false;
				}
			}
			return true;
		}
		return true;
	}

	public string PrettyPrint()
	{
		StringBuilder stringBuilder = new StringBuilder();
		this.PrettyPrint(stringBuilder, "");
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrettyPrint(StringBuilder sb, string indent)
	{
		sb.AppendFormat("{0}Properties:\n", indent);
		foreach (KeyValuePair<string, string> keyValuePair in this.Values.OrderBy((KeyValuePair<string, string> kvp) => kvp.Key))
		{
			sb.AppendFormat("{2}    name={0}, value={1}", keyValuePair.Key, this.Values[keyValuePair.Key], indent);
			if (this.Params1.ContainsKey(keyValuePair.Key))
			{
				sb.AppendFormat(", param1={0}", this.Params1[keyValuePair.Key]);
			}
			if (this.Params2.ContainsKey(keyValuePair.Key))
			{
				sb.AppendFormat(", param2={0}", this.Params2[keyValuePair.Key]);
			}
			if (this.Data.ContainsKey(keyValuePair.Key))
			{
				sb.AppendFormat(", fields={0}", this.Data[keyValuePair.Key]);
			}
			sb.AppendLine();
		}
		if (this.Classes.Count > 0)
		{
			sb.AppendFormat("{0}Classes:\n", indent);
			foreach (KeyValuePair<string, DynamicProperties> keyValuePair2 in this.Classes)
			{
				sb.AppendFormat("{1}    class={0}\n", keyValuePair2.Key, indent);
				keyValuePair2.Value.PrettyPrint(sb, string.Format("{0}    ", indent));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static DynamicProperties()
	{
		DynamicProperties.RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DynamicProperties>())
		{
			MemoryPackFormatterProvider.Register<DynamicProperties>(new DynamicProperties.DynamicPropertiesFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DynamicProperties[]>())
		{
			MemoryPackFormatterProvider.Register<DynamicProperties[]>(new ArrayFormatter<DynamicProperties>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<string, string>>())
		{
			MemoryPackFormatterProvider.Register<Dictionary<string, string>>(new DictionaryFormatter<string, string>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<string, DynamicProperties>>())
		{
			MemoryPackFormatterProvider.Register<Dictionary<string, DynamicProperties>>(new DictionaryFormatter<string, DynamicProperties>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<string, List<Dictionary<string, string>>>>())
		{
			MemoryPackFormatterProvider.Register<Dictionary<string, List<Dictionary<string, string>>>>(new DictionaryFormatter<string, List<Dictionary<string, string>>>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<Dictionary<string, string>>>())
		{
			MemoryPackFormatterProvider.Register<List<Dictionary<string, string>>>(new ListFormatter<Dictionary<string, string>>());
		}
	}

	[NullableContext(2)]
	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref DynamicProperties value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(6);
		writer.WriteValue<Dictionary<string, string>>(ref value.Values);
		writer.WriteValue<Dictionary<string, string>>(ref value.Params1);
		writer.WriteValue<Dictionary<string, string>>(ref value.Params2);
		writer.WriteValue<Dictionary<string, string>>(ref value.Data);
		writer.WriteValue<Dictionary<string, DynamicProperties>>(ref value.Classes);
		writer.WriteValue<Dictionary<string, List<Dictionary<string, string>>>>(ref value.Array);
	}

	[NullableContext(2)]
	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref DynamicProperties value)
	{
		byte b;
		if (!reader.TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		Dictionary<string, string> dictionary;
		Dictionary<string, string> dictionary2;
		Dictionary<string, string> dictionary3;
		Dictionary<string, string> dictionary4;
		Dictionary<string, DynamicProperties> dictionary5;
		Dictionary<string, List<Dictionary<string, string>>> dictionary6;
		if (b == 6)
		{
			if (value == null)
			{
				dictionary = reader.ReadValue<Dictionary<string, string>>();
				dictionary2 = reader.ReadValue<Dictionary<string, string>>();
				dictionary3 = reader.ReadValue<Dictionary<string, string>>();
				dictionary4 = reader.ReadValue<Dictionary<string, string>>();
				dictionary5 = reader.ReadValue<Dictionary<string, DynamicProperties>>();
				dictionary6 = reader.ReadValue<Dictionary<string, List<Dictionary<string, string>>>>();
				goto IL_0194;
			}
			dictionary = value.Values;
			dictionary2 = value.Params1;
			dictionary3 = value.Params2;
			dictionary4 = value.Data;
			dictionary5 = value.Classes;
			dictionary6 = value.Array;
			reader.ReadValue<Dictionary<string, string>>(ref dictionary);
			reader.ReadValue<Dictionary<string, string>>(ref dictionary2);
			reader.ReadValue<Dictionary<string, string>>(ref dictionary3);
			reader.ReadValue<Dictionary<string, string>>(ref dictionary4);
			reader.ReadValue<Dictionary<string, DynamicProperties>>(ref dictionary5);
			reader.ReadValue<Dictionary<string, List<Dictionary<string, string>>>>(ref dictionary6);
		}
		else
		{
			if (b > 6)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DynamicProperties), 6, b);
				return;
			}
			if (value == null)
			{
				dictionary = null;
				dictionary2 = null;
				dictionary3 = null;
				dictionary4 = null;
				dictionary5 = null;
				dictionary6 = null;
			}
			else
			{
				dictionary = value.Values;
				dictionary2 = value.Params1;
				dictionary3 = value.Params2;
				dictionary4 = value.Data;
				dictionary5 = value.Classes;
				dictionary6 = value.Array;
			}
			if (b != 0)
			{
				reader.ReadValue<Dictionary<string, string>>(ref dictionary);
				if (b != 1)
				{
					reader.ReadValue<Dictionary<string, string>>(ref dictionary2);
					if (b != 2)
					{
						reader.ReadValue<Dictionary<string, string>>(ref dictionary3);
						if (b != 3)
						{
							reader.ReadValue<Dictionary<string, string>>(ref dictionary4);
							if (b != 4)
							{
								reader.ReadValue<Dictionary<string, DynamicProperties>>(ref dictionary5);
								if (b != 5)
								{
									reader.ReadValue<Dictionary<string, List<Dictionary<string, string>>>>(ref dictionary6);
								}
							}
						}
					}
				}
			}
			if (value == null)
			{
				goto IL_0194;
			}
		}
		value.Values = dictionary;
		value.Params1 = dictionary2;
		value.Params2 = dictionary3;
		value.Data = dictionary4;
		value.Classes = dictionary5;
		value.Array = dictionary6;
		return;
		IL_0194:
		value = new DynamicProperties
		{
			Values = dictionary,
			Params1 = dictionary2,
			Params2 = dictionary3,
			Data = dictionary4,
			Classes = dictionary5,
			Array = dictionary6
		};
	}

	[MemoryPackInclude]
	public Dictionary<string, string> Values = new Dictionary<string, string>();

	[MemoryPackInclude]
	public Dictionary<string, string> Params1 = new Dictionary<string, string>();

	[MemoryPackInclude]
	public Dictionary<string, string> Params2 = new Dictionary<string, string>();

	[MemoryPackInclude]
	public Dictionary<string, string> Data = new Dictionary<string, string>();

	[MemoryPackInclude]
	public Dictionary<string, DynamicProperties> Classes = new Dictionary<string, DynamicProperties>();

	[MemoryPackInclude]
	public Dictionary<string, List<Dictionary<string, string>>> Array = new Dictionary<string, List<Dictionary<string, string>>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const char PathAccessSeparator = '.';

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] semicolonSeparator = new char[] { ';' };

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] equalSeparator = new char[] { '=' };

	[NullableContext(1)]
	[Nullable(new byte[] { 0, 1 })]
	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class DynamicPropertiesFormatter : MemoryPackFormatter<DynamicProperties>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref DynamicProperties value)
		{
			DynamicProperties.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DynamicProperties value)
		{
			DynamicProperties.Deserialize(ref reader, ref value);
		}
	}
}
