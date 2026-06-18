using System;
using System.Collections.Generic;
using System.Text;
using InControl;
using Platform;
using UnityEngine;

public static class XUiUtils
{
	public static string GetBindingXuiMarkupString(this PlayerAction _action, XUiUtils.EmptyBindingStyle _emptyStyle = XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle _displayStyle = XUiUtils.DisplayStyle.Plain, string _customDisplayStyle = null)
	{
		if (_action == null)
		{
			return "";
		}
		string name = ((PlayerActionsBase)_action.Owner).Name;
		string name2 = _action.Name;
		bool flag = _emptyStyle > XUiUtils.EmptyBindingStyle.EmptyString;
		bool flag2 = !string.IsNullOrEmpty(_customDisplayStyle);
		bool flag3 = _displayStyle > XUiUtils.DisplayStyle.Plain || flag2;
		string text = ((flag || flag3) ? (":" + _emptyStyle.ToStringCached<XUiUtils.EmptyBindingStyle>()) : "");
		string text2 = (flag3 ? (":" + (flag2 ? _customDisplayStyle : _displayStyle.ToStringCached<XUiUtils.DisplayStyle>())) : "");
		return string.Concat(new string[] { "[action:", name, ":", name2, text, text2, "]" });
	}

	public static bool ParseActionsMarkup(XUi _xui, string _input, out string _parsed, string _defaultCustomFormat = null, XUiUtils.ForceLabelInputStyle _forceInputStyle = XUiUtils.ForceLabelInputStyle.Off)
	{
		bool flag = false;
		int num;
		while ((num = _input.IndexOf("[action:", StringComparison.OrdinalIgnoreCase)) >= 0)
		{
			int num2 = num + "[action:".Length;
			int num3 = _input.IndexOf(':', num2);
			int num4 = _input.IndexOf(':', num3 + 1);
			int num5 = _input.IndexOf(':', num4 + 1);
			int i = 0;
			int num6 = num2;
			while (i >= 0)
			{
				int num7 = _input.IndexOf('[', num6);
				int num8 = _input.IndexOf(']', num6);
				bool flag2 = num7 >= 0;
				bool flag3 = num8 >= 0;
				if (flag2 && num7 < num8)
				{
					i++;
					num6 = num7 + 1;
				}
				else
				{
					if (!flag3)
					{
						break;
					}
					i--;
					num6 = num8 + 1;
				}
			}
			if (i >= 0)
			{
				Log.Warning("[XUi] Could not parse action descriptor in label text, no closing bracket found");
			}
			else
			{
				int num9 = num6 - 1;
				bool flag4 = num4 >= 0 && num4 < num9;
				bool flag5 = flag4 && num5 >= 0 && num5 < num9;
				if (num9 < 0)
				{
					Log.Warning("[XUi] Could not parse action descriptor in label text, no closing bracket found");
				}
				else if (num3 < 0 || num3 > num9)
				{
					Log.Warning("[XUi] Could not parse action descriptor in label text, no separator between action set name and action found");
				}
				else
				{
					int num10 = (flag4 ? (num4 - 1) : (num9 - 1));
					int num11 = (flag5 ? (num5 - 1) : (num9 - 1));
					string text = _input.Substring(num2, num3 - num2);
					string text2 = _input.Substring(num3 + 1, num10 - num3);
					string text3 = (flag4 ? _input.Substring(num4 + 1, num11 - num4) : null);
					string text4 = (flag5 ? _input.Substring(num5 + 1, num9 - num5 - 1) : null);
					PlayerActionsBase actionSetForName = PlatformManager.NativePlatform.Input.GetActionSetForName(text);
					if (actionSetForName == null)
					{
						Log.Warning("[XUi] Could not parse action descriptor in label text, action set \"" + text + "\" not found");
					}
					else
					{
						PlayerAction playerActionByName = actionSetForName.GetPlayerActionByName(text2);
						if (playerActionByName != null)
						{
							XUiUtils.EmptyBindingStyle emptyBindingStyle = XUiUtils.EmptyBindingStyle.EmptyString;
							if (flag4 && text3.Length > 0 && !EnumUtils.TryParse<XUiUtils.EmptyBindingStyle>(text3, out emptyBindingStyle, true))
							{
								Log.Warning("[XUi] Could not parse action descriptor empty style, \"" + text3 + "\" unknown");
							}
							bool flag6 = false;
							XUiUtils.DisplayStyle displayStyle = XUiUtils.DisplayStyle.Plain;
							if (flag5 && !EnumUtils.TryParse<XUiUtils.DisplayStyle>(text4, out displayStyle, true))
							{
								if (text4.Length < 1)
								{
									Log.Warning("[XUi] Could not parse action descriptor display type, \"" + text4 + "\" unknown");
								}
								else if (text4.IndexOf("###", StringComparison.Ordinal) < 0)
								{
									Log.Warning("[XUi] Could not parse action descriptor display type, \"" + text4 + "\" assumed to be a custom format, missing the '#' placeholder");
								}
								else
								{
									flag6 = true;
								}
							}
							if (!flag5 && !string.IsNullOrEmpty(_defaultCustomFormat))
							{
								flag6 = true;
								text4 = _defaultCustomFormat;
							}
							string bindingString = playerActionByName.GetBindingString(_forceInputStyle != XUiUtils.ForceLabelInputStyle.Keyboard && (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard || _forceInputStyle == XUiUtils.ForceLabelInputStyle.Controller), XUiUtils.GetInputStyleFromForcedStyle(_forceInputStyle), emptyBindingStyle, displayStyle, flag6, text4);
							_input = _input.Remove(num, num9 - num + 1);
							_input = _input.Insert(num, bindingString);
							flag = true;
							continue;
						}
						Log.Warning("[XUi] Could not parse action descriptor in label text, action \"" + text2 + "\" not found");
					}
				}
			}
			IL_0337:
			while ((num = _input.IndexOf("[button:", StringComparison.OrdinalIgnoreCase)) >= 0)
			{
				int num12 = num + "[button:".Length;
				int num13 = _input.IndexOf(']', num);
				if (num13 < 0)
				{
					Log.Warning("[XUi] Could not parse button descriptor in label text, no closing bracket found");
					break;
				}
				string text5 = InControlExtensions.TryLocalizeButtonName(_input.Substring(num12, num13 - num12));
				_input = _input.Remove(num, num13 - num + 1);
				_input = _input.Insert(num, text5);
				flag = true;
			}
			_parsed = _input;
			return flag;
		}
		goto IL_0337;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PlayerInputManager.InputStyle GetInputStyleFromForcedStyle(XUiUtils.ForceLabelInputStyle _forceStyle)
	{
		if (_forceStyle == XUiUtils.ForceLabelInputStyle.Keyboard)
		{
			return PlayerInputManager.InputStyle.Keyboard;
		}
		if (_forceStyle != XUiUtils.ForceLabelInputStyle.Controller)
		{
			return PlatformManager.NativePlatform.Input.CurrentInputStyle;
		}
		return PlatformManager.NativePlatform.Input.CurrentControllerInputStyle;
	}

	public static string GetXuiHierarchy(XUiController _current)
	{
		StringBuilder stringBuilder = new StringBuilder();
		XUiUtils.getXuiHierarchyRec(_current, stringBuilder);
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void getXuiHierarchyRec(XUiController _current, StringBuilder _sb)
	{
		if (_current.Parent != null)
		{
			XUiUtils.getXuiHierarchyRec(_current.Parent, _sb);
			_sb.Append(" -> ");
		}
		string text;
		string text2;
		if (_current.ViewComponent != null)
		{
			text = _current.ViewComponent.GetType().Name.Replace("XUiV_", "");
			text2 = _current.ViewComponent.ID;
		}
		else
		{
			text = "windowgroup";
			text2 = _current.WindowGroup.Id;
		}
		if (text.EqualsCaseInsensitive(text2))
		{
			_sb.Append(text2);
			return;
		}
		_sb.Append(text);
		_sb.Append(" (");
		_sb.Append(text2);
		_sb.Append(")");
	}

	public static string ToXuiColorString(this Color32 _color)
	{
		return string.Format("{0},{1},{2},{3}", new object[] { _color.r, _color.g, _color.b, _color.a });
	}

	public static XUiView FindHierarchyClosestView(XUiController _startController, string _name)
	{
		XUiController controller = _startController.WindowGroup.Controller;
		XUiController xuiController = _startController.Parent;
		XUiController childById;
		for (;;)
		{
			childById = xuiController.GetChildById(_name);
			if (childById != null)
			{
				break;
			}
			xuiController = xuiController.Parent;
			if (xuiController == controller)
			{
				goto Block_2;
			}
		}
		return childById.ViewComponent;
		Block_2:
		return null;
	}

	public static bool HotkeysAllowedFor(XUiView _view, bool _ignoreViewLock = false)
	{
		XUi xui = _view.xui;
		CursorControllerAbs cursorController = xui.playerUI.CursorController;
		GUIWindowManager windowManager = xui.playerUI.windowManager;
		return _view.IsVisible && !GUIWindowConsole.IsOpen() && windowManager.IsKeyShortcutsAllowed() && (_ignoreViewLock || cursorController.IsWithinNavigationLockView(_view)) && !XUiC_FullScreenCollider.IsBlocked(_view);
	}

	public static void ApplyFillPaddedSprite(UISprite _uiSprite, string _origSpriteName)
	{
		INGUIAtlas atlas = _uiSprite.atlas;
		if (atlas == null)
		{
			return;
		}
		XUiUtils.modifyAtlas(_uiSprite, atlas, _origSpriteName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void modifyAtlas(UISprite _uiSprite, INGUIAtlas _atlas, string _origSpriteName)
	{
		if (_atlas.replacement != null)
		{
			XUiUtils.modifyAtlas(_uiSprite, _atlas.replacement, _origSpriteName);
			return;
		}
		string filledSpriteName = _origSpriteName + "##filled";
		List<UISpriteData> spriteList = _atlas.spriteList;
		if (spriteList.FindIndex((UISpriteData _data) => _data.name == filledSpriteName) >= 0)
		{
			_uiSprite.spriteName = filledSpriteName;
			return;
		}
		int num = spriteList.FindIndex((UISpriteData _data) => _data.name == _origSpriteName);
		if (num < 0)
		{
			return;
		}
		UISpriteData uispriteData = new UISpriteData();
		uispriteData.CopyFrom(spriteList[num]);
		uispriteData.name = filledSpriteName;
		uispriteData.x++;
		uispriteData.y++;
		uispriteData.width -= 2;
		uispriteData.height -= 2;
		spriteList.Add(uispriteData);
		XUiUtils.setAtlasSpriteList(_atlas, spriteList);
		_uiSprite.spriteName = filledSpriteName;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setAtlasSpriteList(INGUIAtlas _atlas, List<UISpriteData> _list)
	{
		UIAtlas uiatlas = _atlas as UIAtlas;
		if (uiatlas != null)
		{
			uiatlas.spriteList = _list;
			uiatlas.MarkSpriteListAsChanged();
			return;
		}
		NGUIAtlas nguiatlas = _atlas as NGUIAtlas;
		if (nguiatlas != null)
		{
			nguiatlas.spriteList = _list;
			return;
		}
		throw new Exception();
	}

	public enum EmptyBindingStyle
	{
		EmptyString,
		NullString,
		LocalizedUnbound,
		LocalizedNone
	}

	public enum DisplayStyle
	{
		Plain,
		KeyboardWithAngleBrackets,
		KeyboardWithParentheses,
		KeyboardWithBrackets
	}

	public enum ForceLabelInputStyle
	{
		Off,
		Keyboard,
		Controller
	}
}
