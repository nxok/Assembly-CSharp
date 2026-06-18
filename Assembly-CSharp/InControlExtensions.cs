using System;
using System.Collections.Generic;
using System.Text;
using InControl;
using Platform;

public static class InControlExtensions
{
	public static string GetBindingString(this PlayerAction _action, bool _forController, PlayerInputManager.InputStyle _inputStyle = PlayerInputManager.InputStyle.Undefined, XUiUtils.EmptyBindingStyle _emptyStyle = XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle _displayStyle = XUiUtils.DisplayStyle.Plain, bool _isCustomDisplayStyle = false, string _customDisplayStyleString = null)
	{
		if (_action != null)
		{
			string name = _action.Name;
			if (!(name == "GUI Action Up"))
			{
				if (!(name == "GUI Action Down"))
				{
					if (!(name == "GUI Action Left"))
					{
						if (name == "GUI Action Right")
						{
							if (_forController)
							{
								return LocalPlayerUI.primaryUI.playerInput.GUIActions.Cancel.GetBindingOfType(true).GetBindingSourceString(_inputStyle, _emptyStyle, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString);
							}
							return LocalPlayerUI.primaryUI.playerInput.GUIActions.DPad_Right.GetBindingOfType(false).GetBindingSourceString(_inputStyle, _emptyStyle, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString);
						}
					}
					else
					{
						if (_forController)
						{
							return LocalPlayerUI.primaryUI.playerInput.GUIActions.HalfStack.GetBindingOfType(true).GetBindingSourceString(_inputStyle, _emptyStyle, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString);
						}
						return LocalPlayerUI.primaryUI.playerInput.GUIActions.DPad_Left.GetBindingOfType(false).GetBindingSourceString(_inputStyle, _emptyStyle, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString);
					}
				}
				else
				{
					if (_forController)
					{
						return LocalPlayerUI.primaryUI.playerInput.GUIActions.Submit.GetBindingOfType(true).GetBindingSourceString(_inputStyle, _emptyStyle, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString);
					}
					return LocalPlayerUI.primaryUI.playerInput.GUIActions.DPad_Down.GetBindingOfType(false).GetBindingSourceString(_inputStyle, _emptyStyle, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString);
				}
			}
			else
			{
				if (_forController)
				{
					return LocalPlayerUI.primaryUI.playerInput.GUIActions.Inspect.GetBindingOfType(true).GetBindingSourceString(_inputStyle, _emptyStyle, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString);
				}
				return LocalPlayerUI.primaryUI.playerInput.GUIActions.DPad_Up.GetBindingOfType(false).GetBindingSourceString(_inputStyle, _emptyStyle, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString);
			}
		}
		return _action.GetBindingOfType(_forController).GetBindingSourceString(_inputStyle, _emptyStyle, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetBindingSourceString(this BindingSource _bs, PlayerInputManager.InputStyle _inputStyle = PlayerInputManager.InputStyle.Undefined, XUiUtils.EmptyBindingStyle _emptyStyle = XUiUtils.EmptyBindingStyle.LocalizedNone, XUiUtils.DisplayStyle _displayStyle = XUiUtils.DisplayStyle.Plain, bool _isCustomDisplayStyle = false, string _customDisplayStyleString = null)
	{
		string text;
		if (_bs == null)
		{
			switch (_emptyStyle)
			{
			case XUiUtils.EmptyBindingStyle.EmptyString:
				text = string.Empty;
				break;
			case XUiUtils.EmptyBindingStyle.NullString:
				text = null;
				break;
			case XUiUtils.EmptyBindingStyle.LocalizedUnbound:
				text = InControlExtensions.TryLocalizeButtonName("Unbound");
				break;
			case XUiUtils.EmptyBindingStyle.LocalizedNone:
				text = InControlExtensions.TryLocalizeButtonName("None");
				break;
			default:
				throw new ArgumentOutOfRangeException("_emptyStyle", _emptyStyle, null);
			}
			return text;
		}
		switch (_bs.BindingSourceType)
		{
		case 1:
			text = InControlExtensions.GetGamepadSourceString(((DeviceBindingSource)_bs).Control);
			break;
		case 2:
			text = InControlExtensions.GetKeyboardSourceString((KeyBindingSource)_bs, _displayStyle, _isCustomDisplayStyle, _customDisplayStyleString);
			break;
		case 3:
			text = InControlExtensions.GetMouseSourceString(((MouseBindingSource)_bs).Control);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetKeyboardSourceString(KeyBindingSource _kbs, XUiUtils.DisplayStyle _displayStyle, bool _isCustomDisplayStyle, string _customDisplayStyleString)
	{
		KeyCombo control = _kbs.Control;
		InControlExtensions.bindingToString.Clear();
		for (int i = 0; i < control.IncludeCount; i++)
		{
			if (i > 0)
			{
				InControlExtensions.bindingToString.Append(" + ");
			}
			string localizedName = control.GetInclude(i).GetLocalizedName();
			if (_isCustomDisplayStyle)
			{
				InControlExtensions.bindingToString.Append(_customDisplayStyleString.Replace("###", localizedName));
			}
			else
			{
				switch (_displayStyle)
				{
				case XUiUtils.DisplayStyle.Plain:
					InControlExtensions.bindingToString.Append(localizedName);
					break;
				case XUiUtils.DisplayStyle.KeyboardWithAngleBrackets:
					InControlExtensions.bindingToString.Append("<");
					InControlExtensions.bindingToString.Append(localizedName);
					InControlExtensions.bindingToString.Append(">");
					break;
				case XUiUtils.DisplayStyle.KeyboardWithParentheses:
					InControlExtensions.bindingToString.Append("( ");
					InControlExtensions.bindingToString.Append(localizedName);
					InControlExtensions.bindingToString.Append(" )");
					break;
				case XUiUtils.DisplayStyle.KeyboardWithBrackets:
					InControlExtensions.bindingToString.Append("[");
					InControlExtensions.bindingToString.Append(localizedName);
					InControlExtensions.bindingToString.Append("]");
					break;
				default:
					throw new ArgumentOutOfRangeException("_displayStyle");
				}
			}
		}
		return InControlExtensions.bindingToString.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetMouseSourceString(Mouse _control)
	{
		string text;
		switch (_control)
		{
		case 1:
			text = "[sp=Mouse_LeftButton_Large]";
			break;
		case 2:
			text = "[sp=Mouse_RightButton_Large]";
			break;
		case 3:
			text = "[sp=Mouse_MiddleButton_Large]";
			break;
		default:
			text = _control.GetLocalizedName();
			break;
		}
		return text;
	}

	public static string GetGamepadSourceString(InputControlType _control)
	{
		PlayerInputManager.InputStyle inputStyle = PlayerInputManager.InputStyleFromSelectedIconStyle();
		InputControlType inputControlType;
		switch (_control)
		{
		case 100:
		case 102:
		case 109:
		case 114:
			break;
		case 101:
		case 104:
		case 106:
		case 113:
			if (inputStyle != PlayerInputManager.InputStyle.PS4)
			{
				if (inputStyle != PlayerInputManager.InputStyle.XB1)
				{
					inputControlType = _control;
				}
				else
				{
					inputControlType = 106;
				}
			}
			else
			{
				inputControlType = 104;
			}
			_control = inputControlType;
			goto IL_008C;
		case 103:
		case 105:
		case 107:
		case 108:
		case 110:
		case 111:
		case 112:
			goto IL_008C;
		default:
			if (_control != 258)
			{
				goto IL_008C;
			}
			break;
		}
		if (inputStyle != PlayerInputManager.InputStyle.PS4)
		{
			if (inputStyle != PlayerInputManager.InputStyle.XB1)
			{
				inputControlType = _control;
			}
			else
			{
				inputControlType = 100;
			}
		}
		else
		{
			inputControlType = 258;
		}
		_control = inputControlType;
		IL_008C:
		string text;
		if (inputStyle != PlayerInputManager.InputStyle.PS4)
		{
			if (inputStyle != PlayerInputManager.InputStyle.XB1)
			{
				text = "[sp={_control.ToStringCached ()}]";
			}
			else
			{
				text = "[sp=XB_Button_" + _control.ToStringCached<InputControlType>() + "]";
			}
		}
		else
		{
			text = "[sp=PS5_Button_" + _control.ToStringCached<InputControlType>() + "]";
		}
		return text;
	}

	public static string GetBlankDPadSourceString()
	{
		if (PlayerInputManager.InputStyleFromSelectedIconStyle() == PlayerInputManager.InputStyle.PS4)
		{
			return "[sp=PS5_Button_DPadBlank]";
		}
		return "[sp=XB_Button_DPadBlank]";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetLocalizedName(this Key _key)
	{
		string name = UnityKeyboardProvider.KeyMappings[_key].Name;
		string text = "inpButton" + name.Replace(" ", "");
		string text2 = Localization.Get(text, false, null);
		if (text2 != text)
		{
			return text2;
		}
		return name;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetLocalizedName(this Mouse _key)
	{
		string text = "inpButton" + _key.ToStringCached<Mouse>();
		string text2 = Localization.Get(text, false, null);
		if (text2 != text)
		{
			return text2;
		}
		return _key.ToStringCached<Mouse>();
	}

	public static string TryLocalizeButtonName(string _buttonName)
	{
		string text = "inpButton" + _buttonName.Replace(" ", "");
		string text2 = Localization.Get(text, false, null);
		if (text2 != text)
		{
			return text2;
		}
		return _buttonName;
	}

	public static void UnbindBindingsOfType(this PlayerAction _action, bool _controller)
	{
		foreach (BindingSource bindingSource in _action.Bindings)
		{
			if (_controller == (bindingSource.BindingSourceType == 1))
			{
				_action.RemoveBinding(bindingSource);
			}
		}
	}

	public static void UnbindBindingsOfType(this PlayerAction _action, BindingSourceType _bindingType)
	{
		foreach (BindingSource bindingSource in _action.Bindings)
		{
			if (bindingSource.BindingSourceType == _bindingType)
			{
				_action.RemoveBinding(bindingSource);
			}
		}
	}

	public static PlayerAction BindingUsed(this PlayerActionSet _actionSet, BindingSource _binding)
	{
		if (_binding == null)
		{
			return null;
		}
		int count = _actionSet.Actions.Count;
		for (int i = 0; i < count; i++)
		{
			if (_actionSet.Actions[i].HasBinding(_binding))
			{
				return _actionSet.Actions[i];
			}
		}
		return null;
	}

	public static BindingSource GetBindingOfType(this PlayerAction _action, bool _forController = false)
	{
		if (_action == null)
		{
			return null;
		}
		foreach (BindingSource bindingSource in _action.Bindings)
		{
			bool flag = 2 == bindingSource.BindingSourceType || 3 == bindingSource.BindingSourceType;
			if (_forController != flag)
			{
				return bindingSource;
			}
		}
		return null;
	}

	public static string GetNameForControlType(this InputDeviceProfile _profile, InputControlType _controlType)
	{
		foreach (InputControlMapping inputControlMapping in _profile.AnalogMappings)
		{
			if (inputControlMapping.Target == _controlType)
			{
				return inputControlMapping.Target.ToStringCached<InputControlType>();
			}
		}
		foreach (InputControlMapping inputControlMapping2 in _profile.ButtonMappings)
		{
			if (inputControlMapping2.Target == _controlType)
			{
				return inputControlMapping2.Target.ToStringCached<InputControlType>();
			}
		}
		return null;
	}

	public static void GetBoundAction(this InputControlType _controlType, PlayerActionSet[] _actionSets, IList<PlayerAction> _result)
	{
		_result.Clear();
		if (_actionSets == null || _actionSets.Length == 0)
		{
			return;
		}
		for (int i = 0; i < _actionSets.Length; i++)
		{
			foreach (PlayerAction playerAction in _actionSets[i].Actions)
			{
				foreach (BindingSource bindingSource in playerAction.UnfilteredBindings)
				{
					if (bindingSource.BindingSourceType == 1 && ((DeviceBindingSource)bindingSource).Control == _controlType)
					{
						_result.Add(playerAction);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly StringBuilder bindingToString = new StringBuilder();
}
