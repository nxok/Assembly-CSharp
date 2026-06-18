using System;
using System.Globalization;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxFloat : XUiC_ComboBoxOrdinal<double>
{
	public override long ValueGeneric { get; set; }

	public override long ValueMinGeneric { get; set; }

	public override long ValueMaxGeneric { get; set; }

	public override double RelativeValue
	{
		get
		{
			return (this.Value - this.Min) / (this.Max - this.Min);
		}
		set
		{
			this.Value = (this.Max - this.Min) * value + this.Min;
		}
	}

	public XUiC_ComboBoxFloat()
	{
		this.Max = 1.0;
		this.Min = 0.0;
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
			this.Min = StringParsers.ParseDouble(_value, 0, -1, NumberStyles.Any);
		}
	}

	[XuiXmlAttribute("value_max", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValueMax(string _value)
	{
		if (!_value.EqualsCaseInsensitive("@def"))
		{
			this.Max = StringParsers.ParseDouble(_value, 0, -1, NumberStyles.Any);
		}
	}

	[XuiXmlAttribute("value_increment", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attributeValueIncrement(string _value)
	{
		if (!_value.EqualsCaseInsensitive("@def"))
		{
			this.incrementSize = StringParsers.ParseDouble(_value, 0, -1, NumberStyles.Any);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void incrementalChangeValue(double _value)
	{
		double num = (this.Max - this.Min) * _value * 0.5;
		if (num > this.incrementSize)
		{
			num = this.incrementSize;
		}
		else if (num < -this.incrementSize)
		{
			num = -this.incrementSize;
		}
		double value = this.Value;
		double num2 = this.Value + num;
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
	public double incrementSize = 1.0;
}
