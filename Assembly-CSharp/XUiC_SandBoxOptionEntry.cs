using System;
using SandboxOptions;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SandBoxOptionEntry : XUiController
{
	public BaseSandboxOption Option
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public XUiC_SandBoxOptionEntry.SandboxOptionValue OriginalValue
	{
		get
		{
			return this.originalValue;
		}
	}

	[XuiXmlBinding("option_enabled")]
	public bool Enabled
	{
		get
		{
			return this.HasEntry && SandboxOptionManager.Current.IsEnabled(this.sandboxOption) && !SandboxOptionManager.Current.IsOverriden(this.sandboxOption);
		}
	}

	[XuiXmlBinding("option_disabled_by_mod")]
	public bool DisabledByMod
	{
		get
		{
			return this.HasEntry && SandboxOptionManager.Current.IsOverriden(this.sandboxOption);
		}
	}

	[XuiXmlBinding("option_disabled_by_option")]
	public bool DisabledByOption
	{
		get
		{
			return this.HasEntry && !SandboxOptionManager.Current.IsEnabled(this.sandboxOption);
		}
	}

	[XuiXmlBinding("option_changed")]
	public bool IsChanged
	{
		get
		{
			return this.HasEntry && !this.originalValue.Equals(this.controlCombo.Value);
		}
	}

	[XuiXmlBinding("title")]
	public string Title
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			BaseSandboxOption option = this.Option;
			return ((option != null) ? option.OptionNameText : null) ?? "";
		}
	}

	[XuiXmlBinding("tooltip_key")]
	public string TooltipKey
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			BaseSandboxOption option = this.Option;
			return ((option != null) ? option.DescriptionText : null) ?? "";
		}
	}

	[XuiXmlBinding("tooltip_key_disabledoption")]
	public string TooltipKeyDisabledOption
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			BaseSandboxOption option = this.Option;
			return ((option != null) ? option.DisabledByText : null) ?? "";
		}
	}

	[XuiXmlBinding("is_separator")]
	public bool IsSeparator
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.isSeparator;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.isSeparator = value;
		}
	}

	[XuiXmlBinding("has_entry")]
	public bool HasEntry
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.Option != null;
		}
	}

	public override void Init()
	{
		base.Init();
		this.sandboxOption = EnumUtils.Parse<global::SandboxOptions.SandboxOptions>(this.viewComponent.ID, false);
		this.setupOptions();
	}

	public void setupSeparator()
	{
		this.isSeparator = true;
		this.Option = null;
		this.sandboxOption = global::SandboxOptions.SandboxOptions.Max;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void setupOption(BaseSandboxOption _baseSandboxOption)
	{
		this.isSeparator = false;
		this.Option = _baseSandboxOption;
		if (_baseSandboxOption == null)
		{
			this.sandboxOption = global::SandboxOptions.SandboxOptions.Max;
		}
		else
		{
			this.sandboxOption = this.Option.Option;
			this.valueType = this.Option.OptionType;
			this.CategoryName = this.Option.CategoryName;
			this.setupOptions();
			this.setComboBoxValue(false, false);
		}
		this.IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.handleDirtyUpdateDefault() && this.HasEntry)
		{
			this.controlCombo.ValueTextOverride = (SandboxOptionManager.Current.IsEnabled(this.sandboxOption) ? null : "----");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupOptions()
	{
		this.controlCombo.Elements.Clear();
		if (this.Option == null)
		{
			return;
		}
		SandboxOptionValueSet valueSet = this.Option.GetValueSet();
		for (int i = 0; i < valueSet.DisplayValues.Length; i++)
		{
			switch (this.Option.OptionType)
			{
			case BaseSandboxOption.OptionTypes.Int:
			{
				int num;
				if (valueSet.GetIntValue(i, out num))
				{
					this.controlCombo.Elements.Add(new XUiC_SandBoxOptionEntry.SandboxOptionValue(num, valueSet.GetDisplayAtIndex(i)));
				}
				break;
			}
			case BaseSandboxOption.OptionTypes.Float:
			{
				float num2;
				if (valueSet.GetFloatValue(i, out num2))
				{
					this.controlCombo.Elements.Add(new XUiC_SandBoxOptionEntry.SandboxOptionValue(num2, valueSet.GetDisplayAtIndex(i)));
				}
				break;
			}
			case BaseSandboxOption.OptionTypes.Bool:
				this.controlCombo.Elements.Add(new XUiC_SandBoxOptionEntry.SandboxOptionValue(valueSet.GetBoolValue(i) ? 1 : 0, valueSet.DisplayValues[i]));
				break;
			}
		}
	}

	[XuiBindEvent("OnValueChanged", "controlCombo")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlCombo_OnValueChanged(XUiController _sender, XUiC_SandBoxOptionEntry.SandboxOptionValue _oldValue, XUiC_SandBoxOptionEntry.SandboxOptionValue _newValue)
	{
		Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions> onValueChanged = this.OnValueChanged;
		if (onValueChanged != null)
		{
			onValueChanged(this, this.sandboxOption);
		}
		this.IsDirty = true;
	}

	public void ForceOriginalValue(XUiC_SandBoxOptionEntry.SandboxOptionValue _value)
	{
		this.originalValue = _value;
	}

	public void ResetToDefault(bool _storeAsOriginal)
	{
		SandboxOptionManager.Current.SetOptionToDefault(this.sandboxOption);
		this.setComboBoxValue(false, _storeAsOriginal);
	}

	public void ApplyChange(bool _forceChange)
	{
		if (this.originalValue.Equals(this.controlCombo.Value) && !_forceChange)
		{
			return;
		}
		switch (this.valueType)
		{
		case BaseSandboxOption.OptionTypes.Int:
			SandboxOptionManager.Current.SetOption(this.sandboxOption, this.controlCombo.Value.IntValue);
			break;
		case BaseSandboxOption.OptionTypes.Float:
			SandboxOptionManager.Current.SetOption(this.sandboxOption, this.controlCombo.Value.FloatValue);
			break;
		case BaseSandboxOption.OptionTypes.String:
			SandboxOptionManager.Current.SetOption(this.sandboxOption, this.controlCombo.Value.StringValue);
			break;
		case BaseSandboxOption.OptionTypes.Bool:
			SandboxOptionManager.Current.SetOption(this.sandboxOption, this.controlCombo.Value.IntValue == 1);
			break;
		default:
			throw new Exception("Illegal option setup for " + this.sandboxOption.ToStringCached<global::SandboxOptions.SandboxOptions>());
		}
		this.IsDirty = true;
	}

	public void SetComboBoxIndex(int _index)
	{
		if (this.controlCombo.Elements.Count > _index)
		{
			this.controlCombo.SelectedIndex = _index;
		}
		this.originalValue = this.controlCombo.Value;
		Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions> onValueChanged = this.OnValueChanged;
		if (onValueChanged != null)
		{
			onValueChanged(this, this.sandboxOption);
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setComboBoxValue(bool _useCurrent, bool _storeAsOriginal)
	{
		try
		{
			switch (this.valueType)
			{
			case BaseSandboxOption.OptionTypes.Int:
			{
				int num = (_useCurrent ? this.controlCombo.Value.IntValue : SandboxOptionManager.GetInt(this.sandboxOption));
				bool flag = false;
				for (int i = 1; i < this.controlCombo.Elements.Count; i++)
				{
					if (this.controlCombo.Elements[i].IntValue == num)
					{
						this.controlCombo.SelectedIndex = i;
						flag = true;
						break;
					}
				}
				if (flag)
				{
					goto IL_0256;
				}
				int num2 = -1;
				int num3 = int.MaxValue;
				for (int j = 0; j < this.controlCombo.Elements.Count; j++)
				{
					int num4 = Math.Abs(this.controlCombo.Elements[j].IntValue - num);
					if (num2 < 0 || num4 < num3)
					{
						num2 = j;
						num3 = num4;
						if (num3 <= 0)
						{
							break;
						}
					}
				}
				if (num2 >= 0)
				{
					this.controlCombo.SelectedIndex = num2;
					goto IL_0256;
				}
				goto IL_0256;
			}
			case BaseSandboxOption.OptionTypes.Float:
			{
				float num5 = (_useCurrent ? this.controlCombo.Value.FloatValue : SandboxOptionManager.GetFloat(this.sandboxOption));
				bool flag2 = false;
				for (int k = 1; k < this.controlCombo.Elements.Count; k++)
				{
					if (Mathf.Approximately(this.controlCombo.Elements[k].FloatValue, num5))
					{
						this.controlCombo.SelectedIndex = k;
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					goto IL_0256;
				}
				int num6 = -1;
				float num7 = float.MaxValue;
				for (int l = 0; l < this.controlCombo.Elements.Count; l++)
				{
					float num8 = Math.Abs(this.controlCombo.Elements[l].FloatValue - num5);
					if (num6 < 0 || num8 < num7)
					{
						num6 = l;
						num7 = num8;
						if (num7 <= 0f)
						{
							break;
						}
					}
				}
				if (num6 >= 0)
				{
					this.controlCombo.SelectedIndex = num6;
					goto IL_0256;
				}
				goto IL_0256;
			}
			case BaseSandboxOption.OptionTypes.Bool:
				this.controlCombo.SelectedIndex = (_useCurrent ? this.controlCombo.Value.IntValue : (SandboxOptionManager.GetBool(this.sandboxOption) ? 1 : 0));
				goto IL_0256;
			}
			throw new Exception("Illegal option setup for " + this.sandboxOption.ToStringCached<global::SandboxOptions.SandboxOptions>());
			IL_0256:;
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
		if (!_useCurrent && _storeAsOriginal)
		{
			this.originalValue = this.controlCombo.Value;
		}
		Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions> onValueChanged = this.OnValueChanged;
		if (onValueChanged != null)
		{
			onValueChanged(this, this.sandboxOption);
		}
		this.IsDirty = true;
	}

	[XuiXmlBinding("option_default")]
	public bool IsDefaultValue()
	{
		if (this.Option == null)
		{
			this.Option = SandboxOptionManager.GetOption(this.sandboxOption);
		}
		if (!this.HasEntry)
		{
			return true;
		}
		bool flag;
		switch (this.valueType)
		{
		case BaseSandboxOption.OptionTypes.Int:
			flag = this.controlCombo.Value.IntValue == (int)this.Option.GetDefaultValue();
			break;
		case BaseSandboxOption.OptionTypes.Float:
			flag = Mathf.Approximately(this.controlCombo.Value.FloatValue, (float)this.Option.GetDefaultValue());
			break;
		case BaseSandboxOption.OptionTypes.String:
			flag = this.controlCombo.Value.StringValue == (string)this.Option.GetDefaultValue();
			break;
		case BaseSandboxOption.OptionTypes.Bool:
			flag = this.controlCombo.Value.IntValue == 1 == (bool)this.Option.GetDefaultValue();
			break;
		default:
			flag = false;
			break;
		}
		return flag;
	}

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<XUiC_SandBoxOptionEntry.SandboxOptionValue> controlCombo;

	public string CategoryName;

	[PublicizedFrom(EAccessModifier.Private)]
	public global::SandboxOptions.SandboxOptions sandboxOption = global::SandboxOptions.SandboxOptions.Max;

	[PublicizedFrom(EAccessModifier.Private)]
	public BaseSandboxOption.OptionTypes valueType;

	public Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions> OnValueChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SandBoxOptionEntry.SandboxOptionValue originalValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSeparator;

	public enum EOptionValueType
	{
		Null,
		Custom,
		Any,
		Int,
		Float,
		String
	}

	public readonly struct SandboxOptionValue : IEquatable<XUiC_SandBoxOptionEntry.SandboxOptionValue>
	{
		public SandboxOptionValue(XUiC_SandBoxOptionEntry.EOptionValueType _type, string _displayName)
		{
			this.Type = _type;
			this.IntValue = -1;
			this.FloatValue = -1f;
			this.StringValue = null;
			this.DisplayName = _displayName;
		}

		public SandboxOptionValue(int _intValue, string _displayName)
		{
			this.Type = XUiC_SandBoxOptionEntry.EOptionValueType.Int;
			this.IntValue = _intValue;
			this.FloatValue = -1f;
			this.StringValue = null;
			this.DisplayName = _displayName;
		}

		public SandboxOptionValue(float _floatValue, string _displayName)
		{
			this.Type = XUiC_SandBoxOptionEntry.EOptionValueType.Float;
			this.IntValue = -1;
			this.FloatValue = _floatValue;
			this.StringValue = null;
			this.DisplayName = _displayName;
		}

		public SandboxOptionValue(string _stringValue, string _displayName)
		{
			this.Type = XUiC_SandBoxOptionEntry.EOptionValueType.String;
			this.IntValue = -1;
			this.FloatValue = -1f;
			this.StringValue = _stringValue;
			this.DisplayName = _displayName;
		}

		public override string ToString()
		{
			return this.DisplayName;
		}

		public bool Equals(XUiC_SandBoxOptionEntry.SandboxOptionValue _other)
		{
			if (this.Type != _other.Type)
			{
				return false;
			}
			bool flag;
			switch (this.Type)
			{
			case XUiC_SandBoxOptionEntry.EOptionValueType.Custom:
				flag = true;
				break;
			case XUiC_SandBoxOptionEntry.EOptionValueType.Any:
				flag = true;
				break;
			case XUiC_SandBoxOptionEntry.EOptionValueType.Int:
				flag = this.IntValue == _other.IntValue;
				break;
			case XUiC_SandBoxOptionEntry.EOptionValueType.Float:
				flag = Mathf.Approximately(this.FloatValue, _other.FloatValue);
				break;
			case XUiC_SandBoxOptionEntry.EOptionValueType.String:
				flag = this.StringValue == _other.StringValue;
				break;
			default:
				flag = false;
				break;
			}
			return flag;
		}

		public override bool Equals(object _obj)
		{
			if (_obj is XUiC_SandBoxOptionEntry.SandboxOptionValue)
			{
				XUiC_SandBoxOptionEntry.SandboxOptionValue sandboxOptionValue = (XUiC_SandBoxOptionEntry.SandboxOptionValue)_obj;
				return this.Equals(sandboxOptionValue);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (int)((((((this.Type * (XUiC_SandBoxOptionEntry.EOptionValueType)397) ^ (XUiC_SandBoxOptionEntry.EOptionValueType)this.IntValue) * (XUiC_SandBoxOptionEntry.EOptionValueType)397) ^ (XUiC_SandBoxOptionEntry.EOptionValueType)((this.StringValue != null) ? this.StringValue.GetHashCode() : 0)) * (XUiC_SandBoxOptionEntry.EOptionValueType)397) ^ (XUiC_SandBoxOptionEntry.EOptionValueType)((this.DisplayName != null) ? this.DisplayName.GetHashCode() : 0));
		}

		public readonly XUiC_SandBoxOptionEntry.EOptionValueType Type;

		public readonly int IntValue;

		public readonly float FloatValue;

		public readonly string StringValue;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string DisplayName;
	}
}
