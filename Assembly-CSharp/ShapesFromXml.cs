using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

public static class ShapesFromXml
{
	public static ShapesFromXml.EDebugLevel DebugLevel
	{
		get
		{
			return ShapesFromXml.debug;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static ShapesFromXml()
	{
		string launchArgument = GameUtils.GetLaunchArgument("debugshapes");
		if (launchArgument != null)
		{
			if (launchArgument == "verbose")
			{
				ShapesFromXml.debug = ShapesFromXml.EDebugLevel.Verbose;
			}
			else
			{
				ShapesFromXml.debug = ShapesFromXml.EDebugLevel.Normal;
			}
		}
		ShapesFromXml.TextureLabelsByChannel[0] = new ShapesFromXml.TextureLabels(string.Empty);
		for (int i = 1; i < 1; i++)
		{
			ShapesFromXml.TextureLabelsByChannel[i] = new ShapesFromXml.TextureLabels((i + 1).ToString());
		}
	}

	public static IEnumerator LoadShapes(XmlFile _xmlFile)
	{
		ShapesFromXml.shapes = new CaseInsensitiveStringDictionary<XElement>();
		ShapesFromXml.shapeCategories.Clear();
		BlockShapeNew.Cleanup();
		XElement root = _xmlFile.XmlDoc.Root;
		if (root == null || !root.HasElements)
		{
			throw new Exception("No element <shapes> found!");
		}
		int num = 1;
		foreach (XElement xelement in root.Elements("shape"))
		{
			string attribute = xelement.GetAttribute(XNames.name);
			ShapesFromXml.SetProperty(xelement, Block.PropCreativeSort2, XNames.value, num++.ToString("0000"));
			ShapesFromXml.shapes.Add(attribute, xelement);
		}
		using (IEnumerator<XElement> enumerator = root.Elements("categories").GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				XElement xelement2 = enumerator.Current;
				ShapesFromXml.ParseCategories(xelement2);
			}
			yield break;
		}
		yield break;
	}

	public static void Cleanup()
	{
		ShapesFromXml.shapes.Clear();
		ShapesFromXml.shapes = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseCategories(XElement _parentElement)
	{
		foreach (XElement xelement in _parentElement.Elements(XNames.category))
		{
			string attribute = xelement.GetAttribute(XNames.name);
			string attribute2 = xelement.GetAttribute(XNames.icon);
			int num = int.Parse(xelement.GetAttribute(XNames.order));
			ShapesFromXml.shapeCategories.Add(attribute, new ShapesFromXml.ShapeCategory(attribute, attribute2, num));
		}
	}

	public static IEnumerator CreateShapeVariants(bool _bEditMode, XElement _elementBlock)
	{
		string blockBaseName = _elementBlock.GetAttribute(XNames.name);
		string allowedShapes = _elementBlock.GetAttribute(XNames.shapes);
		XAttribute xattribute = _elementBlock.Attribute(XNames.hideHelperInCreative);
		bool hideHelperInCreative;
		StringParsers.TryParseBool(((xattribute != null) ? xattribute.Value : null) ?? "false", out hideHelperInCreative);
		if (ShapesFromXml.debug != ShapesFromXml.EDebugLevel.Off)
		{
			Log.Out("Creating block+shape combinations for base block " + blockBaseName);
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
		}
		List<string> shapeNames = new List<string>();
		bool isAll = allowedShapes.EqualsCaseInsensitive("All");
		MicroStopwatch msw = new MicroStopwatch(true);
		foreach (KeyValuePair<string, XElement> keyValuePair in ShapesFromXml.shapes)
		{
			if (isAll || keyValuePair.Value.GetAttribute(XNames.tag).EqualsCaseInsensitive(allowedShapes))
			{
				ShapesFromXml.CreateShapeMaterialCombination(_bEditMode, _elementBlock, blockBaseName, keyValuePair);
				shapeNames.Add(keyValuePair.Key);
				if (msw.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					msw.ResetAndRestart();
				}
			}
		}
		Dictionary<string, XElement>.Enumerator enumerator = default(Dictionary<string, XElement>.Enumerator);
		if (shapeNames.Count > 0)
		{
			ShapesFromXml.CreateMaterialHelper(_bEditMode, blockBaseName, shapeNames, hideHelperInCreative);
		}
		if (ShapesFromXml.debug != ShapesFromXml.EDebugLevel.Off)
		{
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
		}
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CreateMaterialHelper(bool _bEditMode, string _blockBaseName, List<string> _shapeNames, bool _hideHelperInCreative)
	{
		string text = _blockBaseName + ":" + ShapesFromXml.VariantHelperName;
		string text2 = _blockBaseName + ":" + _shapeNames[0];
		DynamicProperties dynamicProperties = BlocksFromXml.CreateProperties(text2, null);
		dynamicProperties.SetValue("Extends", text2);
		dynamicProperties.SetValue(Block.PropCreativeMode, (_hideHelperInCreative ? EnumCreativeMode.None : EnumCreativeMode.All).ToStringCached<EnumCreativeMode>());
		dynamicProperties.SetValue(Block.PropCreativeSort2, "0000");
		dynamicProperties.SetValue(Block.PropDescriptionKey, "blockVariantHelperGroupDesc");
		dynamicProperties.SetValue(Block.PropItemTypeIcon, "all_blocks");
		dynamicProperties.SetValue("SelectAlternates", "true");
		string text3 = _blockBaseName + ":" + string.Join("," + _blockBaseName + ":", _shapeNames);
		dynamicProperties.SetValue(Block.PropPlaceAltBlockValue, text3);
		dynamicProperties.SetValue(Block.PropAutoShape, EAutoShapeType.Helper.ToStringCached<EAutoShapeType>());
		if (ShapesFromXml.debug != ShapesFromXml.EDebugLevel.Off)
		{
			Console.WriteLine("{0}: {1}", text, dynamicProperties.PrettyPrint());
		}
		Block block = BlocksFromXml.CreateBlock(_bEditMode, text, dynamicProperties);
		BlocksFromXml.LoadExtendedItemDrops(block);
		BlocksFromXml.InitBlock(block);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CreateShapeMaterialCombination(bool _bEditMode, XElement _elementBlock, string _blockBaseName, KeyValuePair<string, XElement> _shapeKvp)
	{
		string key = _shapeKvp.Key;
		string text = _blockBaseName + ":" + key;
		string text2;
		string text3;
		BlocksFromXml.ParseExtendedBlock(_elementBlock, out text2, out text3);
		if (text2 == null)
		{
			BlocksFromXml.ParseExtendedBlock(_shapeKvp.Value, out text2, out text3);
		}
		DynamicProperties dynamicProperties = BlocksFromXml.CreateProperties(text2, text3);
		BlocksFromXml.LoadProperties(dynamicProperties, _elementBlock);
		BlocksFromXml.LoadProperties(dynamicProperties, _shapeKvp.Value);
		string text4;
		if (dynamicProperties.TryGetValue(Block.PropDowngradeBlock, out text4))
		{
			text4 = ShapesFromXml.AppendShapeName(text4, _blockBaseName, key);
			dynamicProperties.SetValue(Block.PropDowngradeBlock, text4);
		}
		string text5;
		if (dynamicProperties.TryGetValue(Block.PropUpgradeBlockClass, Block.PropUpgradeBlockToBlock, out text5))
		{
			text5 = ShapesFromXml.AppendShapeName(text5, _blockBaseName, key);
			dynamicProperties.SetValue("UpgradeBlock", "ToBlock", text5);
		}
		string text6;
		if (dynamicProperties.TryGetValue(Block.PropSiblingBlock, out text6))
		{
			text6 = ShapesFromXml.PrependBlockBaseName(text6, _blockBaseName);
			dynamicProperties.SetValue(Block.PropSiblingBlock, text6);
		}
		string text7;
		if (dynamicProperties.TryGetValue("MirrorSibling", out text7))
		{
			text7 = ShapesFromXml.PrependBlockBaseName(text7, _blockBaseName);
			dynamicProperties.SetValue("MirrorSibling", text7);
		}
		dynamicProperties.SetValue(Block.PropCreativeMode, EnumCreativeMode.Dev.ToStringCached<EnumCreativeMode>());
		dynamicProperties.SetParam1(Block.PropCanPickup, _blockBaseName + ":" + ShapesFromXml.VariantHelperName);
		ShapesFromXml.FixCustomIcon(dynamicProperties, _blockBaseName, _shapeKvp.Key);
		ShapesFromXml.FixImposterExchangeId(dynamicProperties, _blockBaseName, _shapeKvp.Key, 0);
		for (int i = 0; i < 1; i++)
		{
			ShapesFromXml.FixTextureId(dynamicProperties, _blockBaseName, _shapeKvp.Key, i);
		}
		ShapesFromXml.SetMaxDamage(dynamicProperties, _blockBaseName, _shapeKvp.Key);
		dynamicProperties.SetValue("AutoShape", EAutoShapeType.Shape.ToStringCached<EAutoShapeType>());
		if (ShapesFromXml.debug != ShapesFromXml.EDebugLevel.Off)
		{
			Console.WriteLine("{0}: {1}", text, dynamicProperties.PrettyPrint());
		}
		Block block = BlocksFromXml.CreateBlock(_bEditMode, text, dynamicProperties);
		bool flag;
		BlocksFromXml.ParseItemDrops(block, _shapeKvp.Value, out flag);
		if (block.itemsToDrop.Count == 0)
		{
			bool flag2;
			BlocksFromXml.ParseItemDrops(block, _elementBlock, out flag2);
			if (!flag2 && !flag)
			{
				BlocksFromXml.LoadExtendedItemDrops(block);
			}
		}
		BlocksFromXml.InitBlock(block);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetMaxDamage(DynamicProperties _properties, string _blockBaseName, string _shapeName)
	{
		if (!_properties.Contains("MaterialHitpointMultiplier"))
		{
			return;
		}
		if (!_properties.Contains("Material"))
		{
			Log.Warning(string.Concat(new string[] { "Blocks: Shape ", _shapeName, " defines a 'MaterialHitpointMultiplier' but block template ", _blockBaseName, " does not define a 'Material'!" }));
			return;
		}
		float num = StringParsers.ParseFloat(_properties.GetString("MaterialHitpointMultiplier"), 0, -1, NumberStyles.Any);
		if (num == 1f)
		{
			return;
		}
		string @string = _properties.GetString("Material");
		MaterialBlock materialBlock = MaterialBlock.fromString(@string);
		if (materialBlock == null)
		{
			Log.Error(string.Concat(new string[] { "Blocks: Block template ", _blockBaseName, " refers to an unknown Material '", @string, "'!" }));
			return;
		}
		int num2 = Mathf.RoundToInt(num * (float)materialBlock.MaxDamage);
		num2 = Utils.FastClamp(num2, 1, 65535);
		_properties.SetValue(Block.PropMaxDamage, num2.ToString());
		ShapesFromXml.ScaleProperty(_properties, Block.PropUpgradeBlockClass, Block.PropUpgradeBlockItemCount, num);
		_properties.SetValue(Block.PropResourceScale, num.ToCultureInvariantString());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void FixCustomIcon(DynamicProperties _properties, string _blockBaseName, string _shapeName)
	{
		if (_properties.Contains(Block.PropCustomIcon))
		{
			return;
		}
		if (!_properties.Contains("Model"))
		{
			Log.Warning(string.Concat(new string[] { "Blocks: Neither shape ", _shapeName, " nor the block template ", _blockBaseName, " define a 'CustomIcon' or 'Model'!" }));
			return;
		}
		string @string = _properties.GetString("Model");
		_properties.SetValue(Block.PropCustomIcon, "shape" + @string);
	}

	public static void SetProperty(XElement _element, string _propertyName, XName _attribName, string _value)
	{
		XElement xelement = (from e in _element.Elements(XNames.property)
			where e.GetAttribute(XNames.name) == _propertyName
			select e).FirstOrDefault<XElement>();
		if (xelement == null)
		{
			xelement = new XElement(XNames.property, new XAttribute(XNames.name, _propertyName));
			_element.Add(xelement);
		}
		xelement.SetAttributeValue(_attribName, _value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ScaleProperty(DynamicProperties _properties, string _className, string _propertyName, float _scale)
	{
		if (_properties.Contains(_className, _propertyName))
		{
			int num = int.Parse(_properties.GetString(_className, _propertyName));
			if (num > 0)
			{
				num = (int)((float)num * _scale);
				if (num < 1)
				{
					num = 1;
				}
				_properties.SetValue(_className, _propertyName, num.ToString());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void FixImposterExchangeId(DynamicProperties _properties, string _blockBaseName, string _shapeName, int channel)
	{
		string texture = ShapesFromXml.TextureLabelsByChannel[channel].Texture;
		string imposterExchange = ShapesFromXml.TextureLabelsByChannel[channel].ImposterExchange;
		if (!_properties.Contains(imposterExchange))
		{
			return;
		}
		if (!_properties.Contains(texture))
		{
			Log.Warning(string.Concat(new string[] { "Blocks: Shape ", _shapeName, " defines ", imposterExchange, " but block template ", _blockBaseName, " does not have a '", texture, "' property!" }));
			return;
		}
		int id = BlockTextureData.GetDataByTextureID(int.Parse(_properties.GetString(texture))).ID;
		_properties.SetParam1(imposterExchange, id.ToString());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void FixTextureId(DynamicProperties _properties, string _blockBaseName, string _shapeName, int channel)
	{
		string texture = ShapesFromXml.TextureLabelsByChannel[channel].Texture;
		string shapeAltTexture = ShapesFromXml.TextureLabelsByChannel[channel].ShapeAltTexture;
		if (!_properties.Contains(shapeAltTexture))
		{
			return;
		}
		if (!_properties.Contains(texture))
		{
			Log.Warning(string.Concat(new string[] { "Blocks: Shape ", _shapeName, " defines ", shapeAltTexture, " but block template ", _blockBaseName, " does not have a '", texture, "' property!" }));
			return;
		}
		string[] array = _properties.GetString(shapeAltTexture).Split(',', StringSplitOptions.None);
		string @string = _properties.GetString(texture);
		for (int i = 0; i < array.Length; i++)
		{
			int num;
			if (!int.TryParse(array[i], out num))
			{
				array[i] = @string;
			}
		}
		string text = string.Join(",", array);
		_properties.SetValue(texture, text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string AppendShapeName(string _innerText, string _blockBaseName, string _shapeName)
	{
		if (_innerText[0] == ':')
		{
			return _blockBaseName + _innerText;
		}
		if (!_innerText.Contains(":"))
		{
			return _innerText + ":" + _shapeName;
		}
		return _innerText;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PrependBlockBaseName(string _innerText, string _blockBaseName)
	{
		return _blockBaseName + ":" + _innerText;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static ShapesFromXml.EDebugLevel debug = ShapesFromXml.EDebugLevel.Off;

	public static readonly ShapesFromXml.TextureLabels[] TextureLabelsByChannel = new ShapesFromXml.TextureLabels[1];

	public static readonly string VariantHelperName = "VariantHelper";

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, XElement> shapes;

	public static readonly Dictionary<string, ShapesFromXml.ShapeCategory> shapeCategories = new CaseInsensitiveStringDictionary<ShapesFromXml.ShapeCategory>();

	public class ShapeCategory : IComparable<ShapesFromXml.ShapeCategory>, IEquatable<ShapesFromXml.ShapeCategory>, IComparable
	{
		public string LocalizedName
		{
			get
			{
				return Localization.Get(this.localizationName, false, null);
			}
		}

		public ShapeCategory(string _name, string _icon, int _order)
		{
			this.Name = _name;
			this.Icon = _icon;
			this.Order = _order;
			this.localizationName = "shapeCategory" + this.Name;
		}

		public bool Equals(ShapesFromXml.ShapeCategory _other)
		{
			return _other != null && (this == _other || (this.Name == _other.Name && this.Icon == _other.Icon && this.Order == _other.Order));
		}

		public override bool Equals(object _obj)
		{
			return _obj != null && (this == _obj || (!(_obj.GetType() != base.GetType()) && this.Equals((ShapesFromXml.ShapeCategory)_obj)));
		}

		public override int GetHashCode()
		{
			return (((((this.Name != null) ? this.Name.GetHashCode() : 0) * 397) ^ ((this.Icon != null) ? this.Icon.GetHashCode() : 0)) * 397) ^ this.Order;
		}

		public int CompareTo(ShapesFromXml.ShapeCategory _other)
		{
			if (this == _other)
			{
				return 0;
			}
			if (_other == null)
			{
				return 1;
			}
			return this.Order.CompareTo(_other.Order);
		}

		public int CompareTo(object _obj)
		{
			if (_obj == null)
			{
				return 1;
			}
			if (this == _obj)
			{
				return 0;
			}
			ShapesFromXml.ShapeCategory shapeCategory = _obj as ShapesFromXml.ShapeCategory;
			if (shapeCategory == null)
			{
				throw new ArgumentException("Object must be of type ShapeCategory");
			}
			return this.CompareTo(shapeCategory);
		}

		public readonly string Name;

		public readonly string Icon;

		public readonly int Order;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string localizationName;
	}

	public enum EDebugLevel
	{
		Off,
		Normal,
		Verbose
	}

	public struct TextureLabels
	{
		public TextureLabels(string suffix)
		{
			this.Texture = "Texture" + suffix;
			this.ImposterExchange = "ImposterExchange" + suffix;
			this.ShapeAltTexture = "ShapeAltTexture" + suffix;
			this.UseGlobalUV = "UseGlobalUV" + suffix;
		}

		public readonly string Texture;

		public readonly string ImposterExchange;

		public readonly string ShapeAltTexture;

		public readonly string UseGlobalUV;
	}
}
