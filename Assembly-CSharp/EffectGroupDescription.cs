using System;
using System.Globalization;
using System.Xml.Linq;

public class EffectGroupDescription
{
	public string Description
	{
		get
		{
			if (Localization.Exists(this.DescriptionKey, false))
			{
				return Localization.Get(this.DescriptionKey, false, null);
			}
			return this.CustomDescription;
		}
	}

	public string LongDescription
	{
		get
		{
			return Localization.Get(this.LongDescriptionKey, false, null);
		}
	}

	public EffectGroupDescription(int _minLevel, int _maxLevel, string _desc_key, string _description, string _long_desc_key)
	{
		this.MinLevel = _minLevel;
		this.MaxLevel = _maxLevel;
		this.DescriptionKey = _desc_key;
		this.CustomDescription = _description;
		this.LongDescriptionKey = _long_desc_key;
	}

	public static EffectGroupDescription ParseDescription(XElement _element)
	{
		if (!_element.HasAttribute("level") || (!_element.HasAttribute("desc_key") && !_element.HasAttribute("desc_base")))
		{
			return null;
		}
		int num2;
		int num;
		if (_element.GetAttribute("level").Contains(","))
		{
			string[] array = _element.GetAttribute("level").Split(',', StringSplitOptions.None);
			if (array.Length < 1)
			{
				return null;
			}
			if (array.Length == 1)
			{
				num = (num2 = StringParsers.ParseSInt32(array[0], 0, -1, NumberStyles.Integer));
			}
			else
			{
				num2 = StringParsers.ParseSInt32(array[0], 0, -1, NumberStyles.Integer);
				num = StringParsers.ParseSInt32(array[1], 0, -1, NumberStyles.Integer);
			}
		}
		else
		{
			num = (num2 = StringParsers.ParseSInt32(_element.GetAttribute("level"), 0, -1, NumberStyles.Integer));
		}
		return new EffectGroupDescription(num2, num, _element.GetAttribute("desc_key"), _element.GetAttribute("desc_base"), _element.HasAttribute("long_desc_key") ? _element.GetAttribute("long_desc_key") : "");
	}

	public readonly int MinLevel;

	public readonly int MaxLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string DescriptionKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string CustomDescription;

	public readonly string LongDescriptionKey;
}
