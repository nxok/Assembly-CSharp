using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ComboBoxBool : XUiC_ComboBox<bool>
{
	[XuiXmlAttribute("localization_prefix", false)]
	public string LocalizationPrefix { get; set; }

	public override bool Value
	{
		get
		{
			return this.currentValue;
		}
		set
		{
			if (this.currentValue != value)
			{
				this.currentValue = value;
				this.IsDirty = true;
				this.UpdateLabel();
			}
		}
	}

	public override double RelativeValue
	{
		get
		{
			return (double)(this.Value ? 1 : 0);
		}
		set
		{
			if (!this.UsesIndexMarkers)
			{
				return;
			}
			this.Value = value >= 0.5;
		}
	}

	public override long ValueGeneric
	{
		get
		{
			return this.Value ? 1L : 0L;
		}
		set
		{
			this.Value = value != 0L;
		}
	}

	public override long ValueMinGeneric
	{
		get
		{
			return 0L;
		}
		set
		{
		}
	}

	public override long ValueMaxGeneric
	{
		get
		{
			return 1L;
		}
		set
		{
		}
	}

	public override int IndexElementCount
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return 2;
		}
	}

	public override int IndexMarkerIndex
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (!this.currentValue)
			{
				return 0;
			}
			return 1;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateLabel()
	{
		base.ValueText = ((!string.IsNullOrEmpty(this.LocalizationPrefix)) ? Localization.Get(this.LocalizationPrefix + (this.currentValue ? "On" : "Off"), false, null) : (this.currentValue ? "Yes" : "No"));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDifferentValue(bool _oldVal, bool _currentValue)
	{
		return _oldVal != _currentValue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void BackPressed()
	{
		this.currentValue = !this.currentValue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ForwardPressed()
	{
		this.currentValue = !this.currentValue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMax()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isMin()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isEmpty()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void incrementalChangeValue(double _value)
	{
		this.TryPageUp();
	}
}
