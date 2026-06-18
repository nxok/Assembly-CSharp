using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxEnum<TEnum> : XUiC_ComboBox<TEnum> where TEnum : struct, IConvertible
{
	[XuiXmlAttribute("localization_prefix", false)]
	public string LocalizationPrefix { get; set; }

	public TEnum? Min
	{
		get
		{
			return new TEnum?(this.min ?? EnumUtils.MinValue<TEnum>());
		}
		set
		{
			if (value != null == (this.min != null) && (value == null || value.Value.Ordinal<TEnum>() == this.min.Value.Ordinal<TEnum>()))
			{
				return;
			}
			this.min = value;
			if (this.min != null && this.Value.Ordinal<TEnum>() < this.min.Value.Ordinal<TEnum>())
			{
				this.Value = this.min.Value;
			}
			base.UpdateIndexMarkerPositions();
			this.IsDirty = true;
		}
	}

	public TEnum? Max
	{
		get
		{
			return new TEnum?(this.max ?? EnumUtils.MaxValue<TEnum>());
		}
		set
		{
			if (value != null == (this.max != null) && (value == null || value.Value.Ordinal<TEnum>() == this.max.Value.Ordinal<TEnum>()))
			{
				return;
			}
			this.max = value;
			if (this.max != null && this.Value.Ordinal<TEnum>() > this.max.Value.Ordinal<TEnum>())
			{
				this.Value = this.max.Value;
			}
			base.UpdateIndexMarkerPositions();
			this.IsDirty = true;
		}
	}

	public override TEnum Value
	{
		get
		{
			return this.currentValue;
		}
		set
		{
			if (this.currentValue.Ordinal<TEnum>() == value.Ordinal<TEnum>())
			{
				return;
			}
			if (this.max != null && value.Ordinal<TEnum>() > this.max.Value.Ordinal<TEnum>())
			{
				value = this.max.Value;
			}
			if (this.min != null && value.Ordinal<TEnum>() < this.min.Value.Ordinal<TEnum>())
			{
				value = this.min.Value;
			}
			this.currentValue = value;
			this.IsDirty = true;
			this.UpdateLabel();
		}
	}

	public int MinIndex
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.min == null)
			{
				return 0;
			}
			return this.Elements.IndexOf(this.min.Value);
		}
	}

	public int MaxIndex
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.max == null)
			{
				return this.Elements.Count - 1;
			}
			return this.Elements.IndexOf(this.max.Value);
		}
	}

	public override double RelativeValue
	{
		get
		{
			return ((double)((float)this.Elements.IndexOf(this.Value)) + 0.5 - (double)this.MinIndex) / (double)(this.MaxIndex - this.MinIndex + 1);
		}
		set
		{
			if (!this.UsesIndexMarkers)
			{
				return;
			}
			int num;
			if (value >= 1.0)
			{
				num = this.MaxIndex;
			}
			else if (value <= 0.0)
			{
				num = this.MinIndex;
			}
			else
			{
				num = Mathf.RoundToInt((float)value * (float)(this.MaxIndex - this.MinIndex + 1) + (float)this.MinIndex - 0.5f);
			}
			this.Value = this.Elements[num];
		}
	}

	public override long ValueGeneric
	{
		get
		{
			return (long)this.Value.Ordinal<TEnum>();
		}
		set
		{
			TEnum tenum;
			if (EnumUtils.TryFromOrdinal<TEnum>((int)value, out tenum))
			{
				this.Value = tenum;
			}
		}
	}

	public override long ValueMinGeneric
	{
		get
		{
			return (long)this.Min.Value.Ordinal<TEnum>();
		}
		set
		{
			TEnum tenum;
			if (EnumUtils.TryFromOrdinal<TEnum>((int)value, out tenum))
			{
				this.Min = new TEnum?(tenum);
			}
		}
	}

	public override long ValueMaxGeneric
	{
		get
		{
			return (long)this.Max.Value.Ordinal<TEnum>();
		}
		set
		{
			TEnum tenum;
			if (EnumUtils.TryFromOrdinal<TEnum>((int)value, out tenum))
			{
				this.Max = new TEnum?(tenum);
			}
		}
	}

	public override int IndexElementCount
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.Elements.Count;
		}
	}

	public override int IndexMarkerIndex
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.Elements.IndexOf(this.Value);
		}
	}

	public override void Init()
	{
		base.Init();
		if (this.Elements.Count == 0)
		{
			this.Elements.AddRange(EnumUtils.Values<TEnum>());
			if (this.Elements.Count > 0)
			{
				this.Value = this.Elements[0];
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateLabel()
	{
		base.ValueText = ((!string.IsNullOrEmpty(this.LocalizationPrefix)) ? Localization.Get(this.LocalizationPrefix + this.currentValue.ToStringCached<TEnum>(), false, null) : this.currentValue.ToStringCached<TEnum>());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDifferentValue(TEnum _oldVal, TEnum _currentValue)
	{
		return _oldVal.Ordinal<TEnum>() != _currentValue.Ordinal<TEnum>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void BackPressed()
	{
		this.changeIndex(-1);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ForwardPressed()
	{
		this.changeIndex(1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void changeIndex(int _direction)
	{
		int num = this.Elements.IndexOf(this.Value);
		int num2 = num + _direction;
		if (num2 < 0 || num2 < this.MinIndex)
		{
			if (this.Wrap)
			{
				num2 = this.Elements.IndexOf(this.Max.Value);
			}
			else
			{
				num2 = num;
			}
		}
		if (num2 >= this.Elements.Count || num2 > this.MaxIndex)
		{
			if (this.Wrap)
			{
				num2 = this.Elements.IndexOf(this.Min.Value);
			}
			else
			{
				num2 = num;
			}
		}
		this.Value = this.Elements[num2];
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMax()
	{
		return this.currentValue.Ordinal<TEnum>() == this.Max.Value.Ordinal<TEnum>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMin()
	{
		return this.currentValue.Ordinal<TEnum>() == this.Min.Value.Ordinal<TEnum>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isEmpty()
	{
		return false;
	}

	[XuiXmlAttribute("values", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValues(string _value)
	{
		if (_value.EqualsCaseInsensitive("@def"))
		{
			return;
		}
		this.Elements.Clear();
		foreach (string text in _value.Split(',', StringSplitOptions.None))
		{
			TEnum tenum;
			if (!EnumUtils.TryParse<TEnum>(text, out tenum, true))
			{
				Log.Error(string.Concat(new string[]
				{
					"[XUi] Value \"",
					text,
					"\" is not a member of the ",
					typeof(TEnum).FullName,
					" enum."
				}));
			}
			else
			{
				this.Elements.Add(tenum);
			}
		}
		if (this.Elements.Count > 0)
		{
			this.Value = this.Elements[0];
		}
	}

	[XuiXmlAttribute("value_min", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValueMin(string _value)
	{
		if (!_value.EqualsCaseInsensitive("@def"))
		{
			this.Min = new TEnum?(EnumUtils.Parse<TEnum>(_value, false));
		}
	}

	[XuiXmlAttribute("value_max", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValueMax(string _value)
	{
		if (!_value.EqualsCaseInsensitive("@def"))
		{
			this.Max = new TEnum?(EnumUtils.Parse<TEnum>(_value, false));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void incrementalChangeValue(double _value)
	{
		if (_value > 0.0)
		{
			this.TryPageUp();
			return;
		}
		if (_value < 0.0)
		{
			this.TryPageDown();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TEnum? min;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEnum? max;

	public readonly List<TEnum> Elements = new List<TEnum>();
}
