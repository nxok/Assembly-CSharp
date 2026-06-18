using System;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_ComboBox<TValue> : XUiC_ComboBoxBase
{
	public event XUiC_ComboBox<TValue>.XUiEvent_ValueChanged OnValueChanged;

	public abstract TValue Value { get; set; }

	public void TriggerValueChangedEvent(TValue _oldVal)
	{
		base.UpdateIndexMarkerStates();
		base.invokeValueChangedGeneric();
		XUiC_ComboBox<TValue>.XUiEvent_ValueChanged onValueChanged = this.OnValueChanged;
		if (onValueChanged != null)
		{
			onValueChanged(this, _oldVal, this.currentValue);
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool TryPageUp()
	{
		bool flag = false;
		if (base.Enabled)
		{
			TValue tvalue = this.currentValue;
			this.ForwardPressed();
			this.UpdateLabel();
			flag = this.isDifferentValue(tvalue, this.currentValue);
			if (flag)
			{
				this.TriggerValueChangedEvent(tvalue);
			}
			this.IsDirty = true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool TryPageDown()
	{
		bool flag = false;
		if (base.Enabled)
		{
			TValue tvalue = this.currentValue;
			this.BackPressed();
			this.UpdateLabel();
			flag = this.isDifferentValue(tvalue, this.currentValue);
			if (flag)
			{
				this.TriggerValueChangedEvent(tvalue);
			}
			this.IsDirty = true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setRelativeValue(float _value)
	{
		if (!base.Enabled)
		{
			return;
		}
		TValue tvalue = this.currentValue;
		this.RelativeValue = (double)_value;
		this.UpdateLabel();
		if (this.isDifferentValue(tvalue, this.currentValue))
		{
			this.TriggerValueChangedEvent(tvalue);
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract bool isDifferentValue(TValue _oldVal, TValue _currentValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_ComboBox()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public TValue currentValue;

	public delegate void XUiEvent_ValueChanged(XUiController _sender, TValue _oldValue, TValue _newValue);
}
