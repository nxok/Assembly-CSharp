using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionEntryGamePrefFloat : XUiC_OptionEntryGamePrefAbs
{
	public float SettingValue
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public float CurrentValue
	{
		get
		{
			XUiC_ComboBoxFloat xuiC_ComboBoxFloat = this.combo;
			return (float)((xuiC_ComboBoxFloat != null) ? xuiC_ComboBoxFloat.Value : 0.0);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.combo.Value = (double)value;
		}
	}

	public override object SelectedValue
	{
		get
		{
			return this.CurrentValue;
		}
		set
		{
			if (value is float)
			{
				float num = (float)value;
				this.CurrentValue = num;
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
			return !Mathf.Approximately(this.CurrentValue, this.SettingValue);
		}
	}

	public override bool IsDefault
	{
		get
		{
			return this.gamePref == null || Mathf.Approximately(this.CurrentValue, this.getPrefDefault());
		}
	}

	public override GamePrefs.EnumType expectedPrefType
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return GamePrefs.EnumType.Float;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float getPrefDefault()
	{
		return (float)GamePrefs.GetDefault(this.gamePref.Value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float getPrefValue()
	{
		return GamePrefs.GetFloat(this.gamePref.Value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setPrefValue(float _value)
	{
		GamePrefs.Set(this.gamePref.Value, _value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void initCurrentValue()
	{
		if (this.gamePref == null)
		{
			return;
		}
		this.CurrentValue = (this.SettingValue = this.getPrefValue());
		XUiC_OptionEntryAbs.DebugLog(string.Format("GOT CURRENT FLOAT FOR {0}: {1}", this.gamePref, this.SettingValue));
	}

	public override void DiscardCurrentChange()
	{
		if (this.gamePref == null)
		{
			return;
		}
		this.CurrentValue = this.SettingValue;
		if (base.ApplyImmediately)
		{
			this.setPrefValue(this.CurrentValue);
		}
		XUiC_OptionEntryAbs.DebugLog(string.Format("DISCARDED FLOAT FOR {0} TO {1}", this.gamePref, this.SettingValue));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void applySelectionInternal()
	{
		this.SettingValue = this.CurrentValue;
		this.setPrefValue(this.SettingValue);
		XUiC_OptionEntryAbs.DebugLog(string.Format("SAVED FLOAT FOR {0} TO {1}", this.gamePref, this.SettingValue));
	}

	public override void ResetToDefault()
	{
		if (!base.ApplyDefaults || this.gamePref == null)
		{
			return;
		}
		this.CurrentValue = this.getPrefDefault();
		if (base.ApplyImmediately)
		{
			this.setPrefValue(this.CurrentValue);
		}
		XUiC_OptionEntryAbs.DebugLog(string.Format("RESET FLOAT FOR {0} TO {1}", this.gamePref, this.CurrentValue));
		base.invokeValueChanged();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void immediatelyApplyCurrentSelection()
	{
		this.setPrefValue(this.CurrentValue);
	}

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxFloat combo;
}
