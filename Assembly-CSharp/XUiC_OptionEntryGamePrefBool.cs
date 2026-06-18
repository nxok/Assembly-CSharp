using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionEntryGamePrefBool : XUiC_OptionEntryGamePrefAbs
{
	[XuiXmlAttribute("invert", false)]
	public bool Invert
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool SettingValue
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool CurrentValue
	{
		get
		{
			XUiC_ComboBoxBool xuiC_ComboBoxBool = this.combo;
			return xuiC_ComboBoxBool != null && xuiC_ComboBoxBool.Value;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.combo.Value = value;
		}
	}

	public override object SelectedValue
	{
		get
		{
			return this.CurrentValue ^ this.Invert;
		}
		set
		{
			if (value is bool)
			{
				bool flag = (bool)value;
				this.CurrentValue = flag ^ this.Invert;
				base.invokeValueChanged();
				return;
			}
			Log.Error(string.Concat(new string[]
			{
				"[XUi] ",
				base.GetType().Name,
				".SelectedValue: Given value is not a ",
				this.expectedPrefType.ToStringCached<GamePrefs.EnumType>(),
				". Hierarchy: ",
				base.GetXuiHierarchy()
			}));
		}
	}

	public override bool IsChanged
	{
		get
		{
			return this.CurrentValue != this.SettingValue && (this.DoSaveOverride == null || this.DoSaveOverride());
		}
	}

	public override bool IsDefault
	{
		get
		{
			return this.gamePref == null || this.CurrentValue == this.getPrefDefault();
		}
	}

	public override GamePrefs.EnumType expectedPrefType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return GamePrefs.EnumType.Bool;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool getPrefDefault()
	{
		return (bool)GamePrefs.GetDefault(this.gamePref.Value) ^ this.Invert;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool getPrefValue()
	{
		return GamePrefs.GetBool(this.gamePref.Value) ^ this.Invert;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setPrefValue(bool _value)
	{
		GamePrefs.Set(this.gamePref.Value, _value ^ this.Invert);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void initCurrentValue()
	{
		if (this.gamePref == null)
		{
			return;
		}
		this.CurrentValue = (this.SettingValue = this.getPrefValue());
		XUiC_OptionEntryAbs.DebugLog(string.Format("GOT CURRENT BOOL FOR {0}: {1}", this.gamePref, this.SettingValue));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool applyValueOverride(bool _value)
	{
		bool flag;
		if (this.OverrideSettingValue != null && this.OverrideSettingValue(_value, out flag))
		{
			_value = flag;
		}
		return _value;
	}

	public override void DiscardCurrentChange()
	{
		if (this.gamePref == null)
		{
			return;
		}
		this.CurrentValue = this.applyValueOverride(this.SettingValue);
		if (base.ApplyImmediately)
		{
			this.setPrefValue(this.CurrentValue);
		}
		XUiC_OptionEntryAbs.DebugLog(string.Format("DISCARDED BOOL FOR {0} TO {1}", this.gamePref, this.SettingValue));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void applySelectionInternal()
	{
		this.SettingValue = this.CurrentValue;
		this.setPrefValue(this.SettingValue);
		XUiC_OptionEntryAbs.DebugLog(string.Format("SAVED BOOL FOR {0} TO {1}", this.gamePref, this.SettingValue));
	}

	public override void ResetToDefault()
	{
		if (!base.ApplyDefaults || this.gamePref == null)
		{
			return;
		}
		this.CurrentValue = this.getPrefDefault() ^ this.Invert;
		if (base.ApplyImmediately)
		{
			this.setPrefValue(this.CurrentValue);
		}
		XUiC_OptionEntryAbs.DebugLog(string.Format("RESET BOOL FOR {0} TO {1}", this.gamePref, this.CurrentValue));
		base.invokeValueChanged();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void immediatelyApplyCurrentSelection()
	{
		this.setPrefValue(this.CurrentValue);
	}

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxBool combo;

	public XUiC_OptionEntryGamePrefBool.OverrideSettingValueDelegate OverrideSettingValue;

	public delegate bool OverrideSettingValueDelegate(bool _settingValue, out bool _overrideValue);
}
