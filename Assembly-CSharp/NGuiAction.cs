using System;
using InControl;

public class NGuiAction
{
	public NGuiAction()
	{
	}

	public NGuiAction(string _text, PlayerAction _hotkey)
		: this(_text, null, false)
	{
		this.hotkey = _hotkey;
	}

	public NGuiAction(string _text, string _icon, bool _isToggle)
		: this(_text, _icon, null, _isToggle, null)
	{
	}

	public NGuiAction(string _text, string _icon, string _description, bool _isToggle, PlayerAction _hotkey)
	{
		this.text = _text;
		this.icon = _icon;
		this.description = _description;
		this.hotkey = _hotkey;
		this.bToggle = _isToggle;
		this.bEnabled = true;
	}

	public virtual void OnClick()
	{
		if (this.IsEnabled() && this.clickActionDelegate != null)
		{
			this.clickActionDelegate();
		}
		this.UpdateUI();
	}

	public virtual void OnRelease()
	{
		if (this.IsEnabled() && this.releaseActionDelegate != null)
		{
			this.releaseActionDelegate();
		}
		this.UpdateUI();
	}

	public virtual void OnDoubleClick()
	{
		if (this.IsEnabled() && this.doubleClickActionDelegate != null)
		{
			this.doubleClickActionDelegate();
		}
		this.UpdateUI();
	}

	public virtual void OnSelect(bool _bSelected)
	{
		if (this.IsEnabled())
		{
			if (this.IsToggle())
			{
				this.SetChecked(_bSelected);
			}
			if (this.selectActionDelegate != null)
			{
				this.selectActionDelegate(_bSelected);
			}
		}
		this.UpdateUI();
	}

	public virtual bool IsActive()
	{
		return this.isVisibleDelegate == null || this.isVisibleDelegate();
	}

	public virtual string GetIcon()
	{
		return this.icon;
	}

	public virtual string GetText()
	{
		return this.text;
	}

	public void SetText(string _text)
	{
		this.text = _text;
		this.UpdateUI();
	}

	public virtual string GetTooltip()
	{
		return this.tooltip;
	}

	public NGuiAction SetTooltip(string _tooltip)
	{
		this.tooltip = (string.IsNullOrEmpty(_tooltip) ? null : Localization.Get(_tooltip, false, null));
		return this;
	}

	public virtual int GetColumnCount()
	{
		return 0;
	}

	public virtual string GetColumnIcon(int _col)
	{
		return null;
	}

	public virtual string GetColumnText(int _col)
	{
		return null;
	}

	public virtual string GetDescription()
	{
		return this.description;
	}

	public virtual NGuiAction SetDescription(string _desc)
	{
		this.description = _desc;
		return this;
	}

	public virtual PlayerAction GetHotkey()
	{
		return this.hotkey;
	}

	public virtual NGuiAction SetEnabled(bool _bEnabled)
	{
		this.bEnabled = _bEnabled;
		this.UpdateUI();
		return this;
	}

	public virtual bool IsEnabled()
	{
		if (this.isEnabledDelegate != null)
		{
			return this.isEnabledDelegate();
		}
		return this.bEnabled;
	}

	public virtual bool IsToggle()
	{
		return this.bToggle;
	}

	public virtual void SetChecked(bool _bChecked)
	{
		this.bChecked = _bChecked;
		this.UpdateUI();
	}

	public virtual bool IsChecked()
	{
		if (this.isCheckedDelegate != null)
		{
			return this.isCheckedDelegate();
		}
		return this.bChecked;
	}

	public virtual NGuiAction SetIsCheckedDelegate(NGuiAction.IsCheckedDelegate _checkedDelegate)
	{
		this.isCheckedDelegate = _checkedDelegate;
		return this;
	}

	public virtual NGuiAction SetIsVisibleDelegate(NGuiAction.IsVisibleDelegate _isVisibleDelegate)
	{
		this.isVisibleDelegate = _isVisibleDelegate;
		return this;
	}

	public virtual NGuiAction SetIsEnabledDelegate(NGuiAction.IsEnabledDelegate _isEnabledDelegate)
	{
		this.isEnabledDelegate = _isEnabledDelegate;
		return this;
	}

	public virtual NGuiAction SetClickActionDelegate(NGuiAction.OnClickActionDelegate _actionDelegate)
	{
		this.clickActionDelegate = _actionDelegate;
		return this;
	}

	public virtual NGuiAction SetReleaseActionDelegate(NGuiAction.OnReleaseActionDelegate _actionDelegate)
	{
		this.releaseActionDelegate = _actionDelegate;
		return this;
	}

	public virtual NGuiAction SetDoubleClickActionDelegate(NGuiAction.OnDoubleClickActionDelegate _actionDelegate)
	{
		this.doubleClickActionDelegate = _actionDelegate;
		return this;
	}

	public virtual NGuiAction SetSelectActionDelegate(NGuiAction.OnSelectActionDelegate _selectActionDelegate)
	{
		this.selectActionDelegate = _selectActionDelegate;
		return this;
	}

	public void UpdateUI()
	{
	}

	public override string ToString()
	{
		if (this.text == null)
		{
			return string.Empty;
		}
		return this.text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction hotkey;

	public static NGuiAction Separator = new NGuiAction("Sep", null);

	[PublicizedFrom(EAccessModifier.Private)]
	public string text;

	[PublicizedFrom(EAccessModifier.Private)]
	public string icon;

	[PublicizedFrom(EAccessModifier.Private)]
	public string description;

	[PublicizedFrom(EAccessModifier.Private)]
	public string tooltip;

	[PublicizedFrom(EAccessModifier.Private)]
	public NGuiAction.OnClickActionDelegate clickActionDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public NGuiAction.OnReleaseActionDelegate releaseActionDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public NGuiAction.OnDoubleClickActionDelegate doubleClickActionDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public NGuiAction.OnSelectActionDelegate selectActionDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public NGuiAction.IsVisibleDelegate isVisibleDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public NGuiAction.IsCheckedDelegate isCheckedDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public NGuiAction.IsEnabledDelegate isEnabledDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bToggle;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bChecked;

	public NGuiAction.EnumKeyMode KeyMode = NGuiAction.EnumKeyMode.FireOnPress;

	[Flags]
	public enum EnumKeyMode
	{
		None = 0,
		FireOnPress = 1,
		FireOnRelease = 2,
		FireOnRepeat = 4
	}

	public delegate void OnClickActionDelegate();

	public delegate void OnReleaseActionDelegate();

	public delegate void OnDoubleClickActionDelegate();

	public delegate void OnSelectActionDelegate(bool _bSelected);

	public delegate bool IsEnabledDelegate();

	public delegate bool IsVisibleDelegate();

	public delegate bool IsCheckedDelegate();
}
