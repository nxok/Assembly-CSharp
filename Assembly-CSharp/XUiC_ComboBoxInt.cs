using System;
using System.Globalization;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxInt : XUiC_ComboBoxOrdinal<long>
{
	public override long ValueGeneric
	{
		get
		{
			return this.Value;
		}
		set
		{
			this.Value = value;
		}
	}

	public override long ValueMinGeneric
	{
		get
		{
			return this.Min;
		}
		set
		{
			this.Min = value;
		}
	}

	public override long ValueMaxGeneric
	{
		get
		{
			return this.Max;
		}
		set
		{
			this.Max = value;
		}
	}

	public override double RelativeValue
	{
		get
		{
			return (double)(this.Value - this.Min) / (double)(this.Max - this.Min);
		}
		set
		{
			this.Value = (long)((double)(this.Max - this.Min) * value) + this.Min;
		}
	}

	public XUiC_ComboBoxInt()
	{
		this.Max = long.MaxValue;
		this.Min = long.MinValue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void BackPressed()
	{
		this.currentValue -= this.incrementSize;
		base.BackPressed();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ForwardPressed()
	{
		this.currentValue += this.incrementSize;
		base.ForwardPressed();
	}

	[XuiXmlAttribute("value_min", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValueMin(string _value)
	{
		if (!_value.EqualsCaseInsensitive("@def"))
		{
			this.Min = StringParsers.ParseSInt64(_value, 0, -1, NumberStyles.Integer);
		}
	}

	[XuiXmlAttribute("value_max", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValueMax(string _value)
	{
		if (!_value.EqualsCaseInsensitive("@def"))
		{
			this.Max = StringParsers.ParseSInt64(_value, 0, -1, NumberStyles.Integer);
		}
	}

	[XuiXmlAttribute("value_increment", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValueIncrement(string _value)
	{
		if (!_value.EqualsCaseInsensitive("@def"))
		{
			this.incrementSize = StringParsers.ParseSInt64(_value, 0, -1, NumberStyles.Integer);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void incrementalChangeValue(double _value)
	{
		long num = (long)((double)(this.Max - this.Min) * _value * 0.5);
		if (_value > 0.0 && num == 0L)
		{
			num = 1L;
		}
		else if (_value < 0.0 && num == 0L)
		{
			num = -1L;
		}
		if (num > this.incrementSize)
		{
			num = this.incrementSize;
		}
		else if (num < -this.incrementSize)
		{
			num = -this.incrementSize;
		}
		long value = this.Value;
		long num2 = this.Value + num;
		if (_value < 0.0 && num2 < this.Min && this.Wrap)
		{
			num2 = this.Max;
		}
		else if (_value > 0.0 && num2 > this.Max && this.Wrap)
		{
			num2 = this.Min;
		}
		this.Value = num2;
		base.TriggerValueChangedEvent(value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public long incrementSize = 1L;
}
