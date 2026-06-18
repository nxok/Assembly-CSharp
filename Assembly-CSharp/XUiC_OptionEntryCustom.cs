using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionEntryCustom : XUiC_OptionEntryAbs
{
	public override bool IsChanged
	{
		get
		{
			Func<bool> isChangedDelegate = this.IsChangedDelegate;
			return isChangedDelegate != null && isChangedDelegate();
		}
	}

	public override bool IsDefault
	{
		get
		{
			Func<bool> isDefaultDelegate = this.IsDefaultDelegate;
			return isDefaultDelegate == null || isDefaultDelegate();
		}
	}

	[XuiBindEvent("OnValueChangedGeneric", "comboGeneric")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnComboValueChanged(XUiController _sender)
	{
		this.SelectionChanged();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void initCurrentValue()
	{
		if (this.GetSettingValue == null)
		{
			Log.Error("[XUi] " + base.GetType().Name + ": Can not init current value, delegate not assigned. Hierarchy: " + base.GetXuiHierarchy());
			return;
		}
		this.GetSettingValue();
		XUiC_OptionEntryAbs.DebugLog("GOT CURRENT CUSTOMVALUE FOR ");
	}

	public override void DiscardCurrentChange()
	{
		Action discardChanges = this.DiscardChanges;
		if (discardChanges != null)
		{
			discardChanges();
		}
		if (base.ApplyImmediately)
		{
			XUiC_OptionEntryCustom.ApplyChangesDelegate applyChanges = this.ApplyChanges;
			if (applyChanges != null)
			{
				applyChanges(true);
			}
		}
		XUiC_OptionEntryAbs.DebugLog("DISCARDED CUSTOMVALUE FOR ");
	}

	public override void ApplySelection()
	{
		if (this.ApplyChanges == null)
		{
			Log.Error("[XUi] " + base.GetType().Name + ": Can not apply changes, delegate not assigned. Hierarchy: " + base.GetXuiHierarchy());
			return;
		}
		this.ApplyChanges(false);
		XUiC_OptionEntryAbs.DebugLog("SAVED CUSTOMVALUE FOR ");
		this.IsDirty = true;
	}

	public override void ResetToDefault()
	{
		if (!base.ApplyDefaults)
		{
			return;
		}
		if (this.ResetDefaults == null)
		{
			Log.Error("[XUi] " + base.GetType().Name + ": Can not reset to defaults, delegate not assigned. Hierarchy: " + base.GetXuiHierarchy());
			return;
		}
		this.ResetDefaults();
		if (base.ApplyImmediately)
		{
			XUiC_OptionEntryCustom.ApplyChangesDelegate applyChanges = this.ApplyChanges;
			if (applyChanges != null)
			{
				applyChanges(true);
			}
		}
		XUiC_OptionEntryAbs.DebugLog("RESET CUSTOMVALUE FOR ");
		base.invokeValueChanged();
	}

	public void SelectionChanged()
	{
		if (base.ApplyImmediately)
		{
			XUiC_OptionEntryCustom.ApplyChangesDelegate applyChanges = this.ApplyChanges;
			if (applyChanges != null)
			{
				applyChanges(true);
			}
		}
		base.invokeValueChanged();
	}

	public Func<bool> IsChangedDelegate;

	public Func<bool> IsDefaultDelegate;

	public Action GetSettingValue;

	public Action DiscardChanges;

	public XUiC_OptionEntryCustom.ApplyChangesDelegate ApplyChanges;

	public Action ResetDefaults;

	public delegate void ApplyChangesDelegate(bool _isImmediateApply);
}
