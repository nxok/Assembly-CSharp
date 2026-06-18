using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignEditorControl : XUiController
{
	public object defaultValue
	{
		get
		{
			return this._defaultValue;
		}
		set
		{
			this.label.EventOnPress = true;
			this._defaultValue = value;
		}
	}

	public override void Init()
	{
		base.Init();
		this.label = base.GetChildById("label").ViewComponent as XUiV_Label;
		this.label.Controller.OnRightPress += delegate(XUiController sender, int button)
		{
			this.SetDefault();
		};
		this.label.EventOnPress = false;
		this.control = base.GetChildById("value");
		if (this.control == null)
		{
			this.control = base.GetChildById("valueA");
			this.controlB = base.GetChildById("valueB");
		}
	}

	public virtual void SetDefault()
	{
		if (this.defaultValue == null)
		{
			return;
		}
		XUiController xuiController = this.control;
		XUiC_ComboBoxInt xuiC_ComboBoxInt = xuiController as XUiC_ComboBoxInt;
		if (xuiC_ComboBoxInt != null)
		{
			long value = xuiC_ComboBoxInt.Value;
			xuiC_ComboBoxInt.Value = (long)((int)this.defaultValue);
			xuiC_ComboBoxInt.TriggerValueChangedEvent(value);
			return;
		}
		XUiC_ComboBoxFloat xuiC_ComboBoxFloat = xuiController as XUiC_ComboBoxFloat;
		if (xuiC_ComboBoxFloat != null)
		{
			double value2 = xuiC_ComboBoxFloat.Value;
			xuiC_ComboBoxFloat.Value = (double)((float)this.defaultValue);
			xuiC_ComboBoxFloat.TriggerValueChangedEvent(value2);
			return;
		}
		XUiC_TextInput xuiC_TextInput = xuiController as XUiC_TextInput;
		if (xuiC_TextInput == null)
		{
			return;
		}
		XUiC_TextInput xuiC_TextInput2 = this.controlB as XUiC_TextInput;
		if (xuiC_TextInput2 != null)
		{
			ValueTuple<float, float> valueTuple = (ValueTuple<float, float>)this.defaultValue;
			xuiC_TextInput.Text = valueTuple.Item1.ToString();
			xuiC_TextInput.TriggerOnChangeHandler(false);
			xuiC_TextInput2.Text = valueTuple.Item2.ToString();
			xuiC_TextInput2.TriggerOnChangeHandler(false);
			return;
		}
		xuiC_TextInput.Text = this.defaultValue.ToString();
		xuiC_TextInput.TriggerOnChangeHandler(false);
	}

	public XUiV_Label label;

	public XUiController control;

	public XUiController controlB;

	[PublicizedFrom(EAccessModifier.Private)]
	public object _defaultValue;
}
