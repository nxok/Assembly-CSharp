using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionEntryGamePrefInt : XUiC_OptionEntryGamePrefAbs
{
	public int SettingValue
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int CurrentValue
	{
		get
		{
			XUiC_ComboBoxInt xuiC_ComboBoxInt = this.combo;
			return (int)((xuiC_ComboBoxInt != null) ? xuiC_ComboBoxInt.Value : 0L);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.combo.Value = (long)value;
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
			if (value is int)
			{
				int num = (int)value;
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
			return this.CurrentValue != this.SettingValue;
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
			return GamePrefs.EnumType.Int;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getPrefDefault()
	{
		return (int)GamePrefs.GetDefault(this.gamePref.Value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getPrefValue()
	{
		return GamePrefs.GetInt(this.gamePref.Value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setPrefValue(int _value)
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
		XUiC_OptionEntryAbs.DebugLog(string.Format("GOT CURRENT INT FOR {0}: {1}", this.gamePref, this.SettingValue));
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
		XUiC_OptionEntryAbs.DebugLog(string.Format("DISCARDED INT FOR {0} TO {1}", this.gamePref, this.SettingValue));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void applySelectionInternal()
	{
		this.SettingValue = this.CurrentValue;
		this.setPrefValue(this.SettingValue);
		XUiC_OptionEntryAbs.DebugLog(string.Format("SAVED INT FOR {0} TO {1}", this.gamePref, this.SettingValue));
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
		XUiC_OptionEntryAbs.DebugLog(string.Format("RESET INT FOR {0} TO {1}", this.gamePref, this.CurrentValue));
		base.invokeValueChanged();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void immediatelyApplyCurrentSelection()
	{
		this.setPrefValue(this.CurrentValue);
	}

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxInt combo;
}
