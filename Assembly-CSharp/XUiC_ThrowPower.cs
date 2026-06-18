using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ThrowPower : XUiController
{
	public float CurrentPower
	{
		get
		{
			return this.currentPower;
		}
		set
		{
			if (value != this.currentPower)
			{
				this.currentPower = value;
				base.RefreshBindings();
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		((XUiV_Window)this.viewComponent).ForceVisible(1f);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.CurrentPower = 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "fill")
		{
			_value = this.currentPower.ToCultureInvariantString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public static void Status(LocalPlayerUI _playerUi, float _currentPower = -1f)
	{
		XUiC_ThrowPower windowByType = _playerUi.xui.GetWindowByType<XUiC_ThrowPower>();
		if (windowByType != null)
		{
			windowByType.CurrentPower = _currentPower;
			if (_currentPower >= 0f)
			{
				_playerUi.windowManager.Open(windowByType.windowGroup, false, true);
				return;
			}
			_playerUi.windowManager.Close(windowByType.windowGroup, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentPower;
}
