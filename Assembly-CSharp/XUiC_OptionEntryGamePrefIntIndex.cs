using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionEntryGamePrefIntIndex : XUiC_OptionEntryGamePrefAbs
{
	[XuiXmlAttribute("allow_custom", false)]
	public bool AllowCustomValue
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

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
			XUiC_ComboBoxBase comboGeneric = this.comboGeneric;
			return (int)((comboGeneric != null) ? comboGeneric.ValueGeneric : 0L);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.comboGeneric.ValueGeneric = (long)Mathf.Clamp(value, (int)this.comboGeneric.ValueMinGeneric, (int)this.comboGeneric.ValueMaxGeneric);
		}
	}

	public override object SelectedValue
	{
		get
		{
			int num = this.CurrentValue;
			if (this.MapListIndexToGamePref != null)
			{
				num = this.MapListIndexToGamePref(num);
			}
			return num;
		}
		set
		{
			if (value is int)
			{
				int num = (int)value;
				if (this.MapGamePrefToListIndex != null)
				{
					num = this.MapGamePrefToListIndex(num);
				}
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

	public override void Init()
	{
		base.Init();
		if (this.AllowCustomValue)
		{
			XUiC_ComboBoxList<string> xuiC_ComboBoxList = this.comboGeneric as XUiC_ComboBoxList<string>;
			if (xuiC_ComboBoxList != null)
			{
				this.origEntriesLength = (int)(this.comboGeneric.ValueMaxGeneric + 1L);
				this.comboGeneric.ValueMaxGeneric = (long)(this.origEntriesLength - 1);
				xuiC_ComboBoxList.Elements.Add("Custom");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getPrefDefault()
	{
		int num = (int)GamePrefs.GetDefault(this.gamePref.Value);
		if (this.MapGamePrefToListIndex != null)
		{
			num = this.MapGamePrefToListIndex(num);
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int getPrefValue()
	{
		int num = GamePrefs.GetInt(this.gamePref.Value);
		if (this.MapGamePrefToListIndex != null)
		{
			num = this.MapGamePrefToListIndex(num);
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setPrefValue(int _value)
	{
		if (this.MapListIndexToGamePref != null)
		{
			_value = this.MapListIndexToGamePref(_value);
		}
		GamePrefs.Set(this.gamePref.Value, _value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void initCurrentValue()
	{
		if (this.gamePref == null)
		{
			return;
		}
		int prefValue = this.getPrefValue();
		if (this.AllowCustomValue && prefValue >= this.origEntriesLength)
		{
			this.comboGeneric.ValueMaxGeneric = (long)this.origEntriesLength;
			prefValue = this.origEntriesLength;
		}
		this.CurrentValue = (this.SettingValue = prefValue);
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
		if (!this.AllowCustomValue || this.comboGeneric.ValueGeneric < (long)this.origEntriesLength)
		{
			this.SettingValue = this.CurrentValue;
			this.setPrefValue(this.SettingValue);
		}
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

	public Func<int, int> MapGamePrefToListIndex;

	public Func<int, int> MapListIndexToGamePref;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origEntriesLength;
}
