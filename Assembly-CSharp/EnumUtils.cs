using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

public static class EnumUtils
{
	public static string ToStringCached<TEnum>(this TEnum _enumValue) where TEnum : struct, IConvertible
	{
		return EnumUtils.EnumInfoCache<TEnum>.Instance.GetName(_enumValue);
	}

	public static int Ordinal<TEnum>(this TEnum _enumValue) where TEnum : struct, IConvertible
	{
		return EnumInt32ToInt.Convert<TEnum>(_enumValue);
	}

	public static bool TryFromOrdinal<TEnum>(int _ordinal, out TEnum _result) where TEnum : struct, IConvertible
	{
		IList<TEnum> list = EnumUtils.Values<TEnum>();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Ordinal<TEnum>() == _ordinal)
			{
				_result = list[i];
				return true;
			}
		}
		_result = default(TEnum);
		return false;
	}

	public static TEnum Parse<TEnum>(string _name, TEnum _default, bool _ignoreCase = false) where TEnum : struct, IConvertible
	{
		TEnum tenum;
		if (!EnumUtils.TryParse<TEnum>(_name, out tenum, _ignoreCase))
		{
			tenum = _default;
		}
		return tenum;
	}

	public static TEnum Parse<TEnum>(string _name, bool _ignoreCase = false) where TEnum : struct, IConvertible
	{
		return EnumUtils.EnumInfoCache<TEnum>.Instance.Parse(_name, _ignoreCase);
	}

	public static bool TryParse<TEnum>(string _name, out TEnum _result, bool _ignoreCase = false) where TEnum : struct, IConvertible
	{
		return EnumUtils.EnumInfoCache<TEnum>.Instance.TryParse(_name, out _result, _ignoreCase);
	}

	public static bool TryParseIgnoreCase<TEnum>(string _name, out TEnum _result) where TEnum : struct, IConvertible
	{
		return EnumUtils.EnumInfoCache<TEnum>.Instance.TryParse(_name, out _result, true);
	}

	public static bool HasName<TEnum>(string _name, bool _ignoreCase = false) where TEnum : struct, IConvertible
	{
		return EnumUtils.EnumInfoCache<TEnum>.Instance.HasName(_name, _ignoreCase);
	}

	public static IList<TEnum> Values<TEnum>() where TEnum : struct, IConvertible
	{
		return EnumUtils.EnumInfoCache<TEnum>.Instance.EnumValues;
	}

	public static IList<string> Names<TEnum>() where TEnum : struct, IConvertible
	{
		return EnumUtils.EnumInfoCache<TEnum>.Instance.EnumNames;
	}

	public static TEnum CycleEnum<TEnum>(this TEnum _enumVal, bool _reverse = false, bool _wrap = true) where TEnum : struct, IConvertible
	{
		if (!typeof(TEnum).IsEnum)
		{
			throw new ArgumentException("Argument " + typeof(TEnum).FullName + " is not an Enum");
		}
		IList<TEnum> enumValues = EnumUtils.EnumInfoCache<TEnum>.Instance.EnumValues;
		int num = enumValues.IndexOf(_enumVal) + (_reverse ? (-1) : 1);
		if (num >= enumValues.Count)
		{
			num = (_wrap ? 0 : (enumValues.Count - 1));
		}
		else if (num < 0)
		{
			num = (_wrap ? (enumValues.Count - 1) : 0);
		}
		return enumValues[num];
	}

	public static TEnum CycleEnum<TEnum>(this TEnum _enumVal, TEnum _minVal, TEnum _maxVal, bool _reverse = false, bool _wrap = true) where TEnum : struct, IConvertible
	{
		if (!typeof(TEnum).IsEnum)
		{
			throw new ArgumentException("Argument " + typeof(TEnum).FullName + " is not an Enum");
		}
		IList<TEnum> enumValues = EnumUtils.EnumInfoCache<TEnum>.Instance.EnumValues;
		int num = enumValues.IndexOf(_minVal);
		if (num < 0)
		{
			throw new ArgumentException(string.Format("Could not find index of {0}", _minVal), "_minVal");
		}
		int num2 = enumValues.IndexOf(_maxVal);
		if (num2 < 0)
		{
			throw new ArgumentException(string.Format("Could not find index of {0}", _maxVal), "_maxVal");
		}
		if (num2 < num)
		{
			throw new ArgumentException(string.Format("Max of {0} with index {1} is less than min of {2} with index {3}", new object[] { _maxVal, num2, _minVal, num }));
		}
		int num3 = enumValues.IndexOf(_enumVal);
		if (num3 < 0)
		{
			Log.Warning(string.Format("Could not find index of {0}: {1} (using min)", "_enumVal", _enumVal));
			return enumValues[num];
		}
		int num4 = num2 - num + 1;
		if (num4 <= 1)
		{
			return enumValues[num];
		}
		int num5 = num3 - num + (_reverse ? (-1) : 1);
		if (_wrap)
		{
			num5 %= num4;
			if (num5 < 0)
			{
				num5 += num4;
			}
		}
		else if (num5 < 0)
		{
			num5 = 0;
		}
		else if (num5 >= num4)
		{
			num5 = num4 - 1;
		}
		int num6 = num + num5;
		return enumValues[num6];
	}

	public static TEnum MaxValue<TEnum>() where TEnum : struct, IConvertible
	{
		return EnumUtils.EnumInfoCache<TEnum>.Instance.EnumValues[EnumUtils.EnumInfoCache<TEnum>.Instance.EnumValues.Count - 1];
	}

	public static TEnum MinValue<TEnum>() where TEnum : struct, IConvertible
	{
		return EnumUtils.EnumInfoCache<TEnum>.Instance.EnumValues[0];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class EnumInfoCache<TEnum> where TEnum : struct, IConvertible
	{
		public static EnumUtils.EnumInfoCache<TEnum> Instance
		{
			get
			{
				if (EnumUtils.EnumInfoCache<TEnum>.instance == null)
				{
					EnumUtils.EnumInfoCache<TEnum>.instance = new EnumUtils.EnumInfoCache<TEnum>();
				}
				return EnumUtils.EnumInfoCache<TEnum>.instance;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public EnumInfoCache()
		{
			if (!typeof(TEnum).IsEnum)
			{
				throw new NotSupportedException(typeof(TEnum).FullName + " is not an enum type.");
			}
			object[] customAttributes = typeof(TEnum).GetCustomAttributes(false);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				if (((Attribute)customAttributes[i]).GetType().Name == "FlagsAttribute")
				{
					this.isFlags = true;
					break;
				}
			}
			Array values = Enum.GetValues(typeof(TEnum));
			this.enumValues = new List<TEnum>(values.Length);
			this.enumToName = new EnumDictionary<TEnum, string>(values.Length);
			foreach (object obj in values)
			{
				TEnum tenum = (TEnum)((object)obj);
				string text = tenum.ToString(CultureInfo.InvariantCulture);
				if (!this.enumValues.Contains(tenum))
				{
					this.enumValues.Add(tenum);
				}
				if (!this.enumToName.ContainsKey(tenum))
				{
					this.enumToName.Add(tenum, text);
				}
			}
			this.enumValues.Sort((TEnum _enumA, TEnum _enumB) => _enumA.Ordinal<TEnum>().CompareTo(_enumB.Ordinal<TEnum>()));
			string[] names = Enum.GetNames(typeof(TEnum));
			this.enumNames = new List<string>(names.Length);
			this.nameToEnumCaseSensitive = new Dictionary<string, TEnum>(names.Length, StringComparer.Ordinal);
			this.nameToEnumCaseInsensitive = new CaseInsensitiveStringDictionary<TEnum>(names.Length);
			foreach (string text2 in names)
			{
				TEnum tenum2 = (TEnum)((object)Enum.Parse(typeof(TEnum), text2));
				if (!this.enumNames.Contains(text2))
				{
					this.enumNames.Add(text2);
				}
				if (!this.nameToEnumCaseSensitive.ContainsKey(text2))
				{
					this.nameToEnumCaseSensitive.Add(text2, tenum2);
				}
				if (!this.nameToEnumCaseInsensitive.ContainsKey(text2))
				{
					this.nameToEnumCaseInsensitive.Add(text2, tenum2);
				}
			}
			this.EnumValues = new ReadOnlyCollection<TEnum>(this.enumValues);
			this.EnumNames = new ReadOnlyCollection<string>(this.enumNames);
		}

		public string GetName(TEnum _enumValue)
		{
			if (this.isFlags)
			{
				if (!this.enumToName.ContainsKey(_enumValue))
				{
					this.enumToName.Add(_enumValue, _enumValue.ToString(CultureInfo.InvariantCulture));
				}
				return this.enumToName[_enumValue];
			}
			if (this.enumToName.ContainsKey(_enumValue))
			{
				return this.enumToName[_enumValue];
			}
			return _enumValue.ToString(CultureInfo.InvariantCulture);
		}

		public TEnum Parse(string _name, bool _ignoreCase)
		{
			if (string.IsNullOrEmpty(_name))
			{
				throw new ArgumentException("Value null or empty", "_name");
			}
			TEnum tenum;
			if ((_ignoreCase ? this.nameToEnumCaseInsensitive : this.nameToEnumCaseSensitive).TryGetValue(_name, out tenum))
			{
				return tenum;
			}
			TEnum tenum2 = (TEnum)((object)Enum.Parse(typeof(TEnum), _name, _ignoreCase));
			this.nameToEnumCaseSensitive.Add(_name, tenum2);
			this.nameToEnumCaseInsensitive.Add(_name, tenum2);
			return tenum2;
		}

		public bool TryParse(string _name, out TEnum _result, bool _ignoreCase)
		{
			_result = default(TEnum);
			if (string.IsNullOrEmpty(_name))
			{
				return false;
			}
			if ((_ignoreCase ? this.nameToEnumCaseInsensitive : this.nameToEnumCaseSensitive).TryGetValue(_name, out _result))
			{
				return true;
			}
			bool flag;
			try
			{
				_result = (TEnum)((object)Enum.Parse(typeof(TEnum), _name, _ignoreCase));
				this.nameToEnumCaseSensitive.Add(_name, _result);
				this.nameToEnumCaseInsensitive.Add(_name, _result);
				flag = true;
			}
			catch (Exception)
			{
				flag = false;
			}
			return flag;
		}

		public bool HasName(string _name, bool _ignoreCase)
		{
			if (string.IsNullOrEmpty(_name))
			{
				throw new ArgumentException("Value null or empty", "_name");
			}
			return (_ignoreCase ? this.nameToEnumCaseInsensitive : this.nameToEnumCaseSensitive).ContainsKey(_name);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static EnumUtils.EnumInfoCache<TEnum> instance;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<TEnum> enumValues;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<string> enumNames;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<TEnum, string> enumToName;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, TEnum> nameToEnumCaseSensitive;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, TEnum> nameToEnumCaseInsensitive;

		public readonly ReadOnlyCollection<TEnum> EnumValues;

		public readonly ReadOnlyCollection<string> EnumNames;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool isFlags;
	}
}
