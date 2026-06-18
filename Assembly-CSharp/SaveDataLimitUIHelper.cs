using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class SaveDataLimitUIHelper
{
	public static SaveDataLimitType CurrentValue
	{
		get
		{
			return SaveDataLimitUIHelper.s_currentValue;
		}
	}

	public static XUiC_ComboBoxEnum<SaveDataLimitType> AddComboBox(XUiC_ComboBoxEnum<SaveDataLimitType> saveDataLimitComboBox)
	{
		SaveDataLimitUIHelper.AddComboBoxInternal(saveDataLimitComboBox);
		return saveDataLimitComboBox;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static SaveDataLimitType Load()
	{
		SaveDataLimitType saveDataLimitType;
		if (!EnumUtils.TryParse<SaveDataLimitType>(GamePrefs.GetString(EnumGamePrefs.SaveDataLimitType), out saveDataLimitType, true) || !saveDataLimitType.IsSupported())
		{
			return SaveDataLimitUIHelper.s_defaultValue;
		}
		return saveDataLimitType;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Save()
	{
		GamePrefs.Set(EnumGamePrefs.SaveDataLimitType, SaveDataLimitUIHelper.s_currentValue.ToStringCached<SaveDataLimitType>());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AddComboBoxInternal(XUiC_ComboBoxEnum<SaveDataLimitType> saveDataLimitComboBox)
	{
		object obj;
		if (saveDataLimitComboBox == null || SaveDataLimitUIHelper.s_saveDataLimitComboBoxes.TryGetValue(saveDataLimitComboBox, out obj))
		{
			return;
		}
		SaveDataLimitUIHelper.s_saveDataLimitComboBoxes.Add(saveDataLimitComboBox, null);
		if (PlatformOptimizations.LimitedSaveData)
		{
			saveDataLimitComboBox.Min = new SaveDataLimitType?(SaveDataLimitType.Short);
		}
		saveDataLimitComboBox.Value = SaveDataLimitUIHelper.s_currentValue;
		saveDataLimitComboBox.OnValueChanged += SaveDataLimitUIHelper.SaveDataLimitComboBox_OnValueChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SetCurrentValue(SaveDataLimitType limitType)
	{
		if (!limitType.IsSupported())
		{
			Log.Error(string.Format("Can not set unsupported limit: {0}", limitType));
			return;
		}
		if (SaveDataLimitUIHelper.s_currentValue == limitType)
		{
			return;
		}
		SaveDataLimitUIHelper.s_currentValue = limitType;
		foreach (KeyValuePair<XUiC_ComboBoxEnum<SaveDataLimitType>, object> keyValuePair in ((IEnumerable<KeyValuePair<XUiC_ComboBoxEnum<SaveDataLimitType>, object>>)SaveDataLimitUIHelper.s_saveDataLimitComboBoxes))
		{
			XUiC_ComboBoxEnum<SaveDataLimitType> xuiC_ComboBoxEnum;
			object obj;
			keyValuePair.Deconstruct(out xuiC_ComboBoxEnum, out obj);
			xuiC_ComboBoxEnum.Value = SaveDataLimitUIHelper.s_currentValue;
		}
		SaveDataLimitUIHelper.Save();
		Action onValueChanged = SaveDataLimitUIHelper.OnValueChanged;
		if (onValueChanged == null)
		{
			return;
		}
		onValueChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SaveDataLimitComboBox_OnValueChanged(XUiController _sender, SaveDataLimitType _oldvalue, SaveDataLimitType _newvalue)
	{
		SaveDataLimitUIHelper.SetCurrentValue(_newvalue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ConditionalWeakTable<XUiC_ComboBoxEnum<SaveDataLimitType>, object> s_saveDataLimitComboBoxes = new ConditionalWeakTable<XUiC_ComboBoxEnum<SaveDataLimitType>, object>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SaveDataLimitType s_defaultValue = (PlatformOptimizations.LimitedSaveData ? SaveDataLimitType.VeryLong : SaveDataLimitType.Unlimited);

	[PublicizedFrom(EAccessModifier.Private)]
	public static SaveDataLimitType s_currentValue = SaveDataLimitUIHelper.Load();

	public static Action OnValueChanged;
}
