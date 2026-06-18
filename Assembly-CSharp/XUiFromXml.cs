using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NCalc;
using NCalc.Domain;
using Platform;
using UnityEngine;

public static class XUiFromXml
{
	[PublicizedFrom(EAccessModifier.Private)]
	static XUiFromXml()
	{
		string launchArgument = GameUtils.GetLaunchArgument("debugxui");
		if (launchArgument != null)
		{
			XUiFromXml.DebugXuiLoading = ((launchArgument == "verbose") ? XUiFromXml.DebugLevel.Verbose : XUiFromXml.DebugLevel.Warning);
			return;
		}
		XUiFromXml.DebugXuiLoading = XUiFromXml.DebugLevel.Off;
	}

	public static void ClearLoadingData()
	{
		XUiFromXml.mainXuiXmlRoot = null;
		Dictionary<string, XElement> dictionary = XUiFromXml.windowData;
		if (dictionary != null)
		{
			dictionary.Clear();
		}
		XUiFromXml.windowData = null;
		Dictionary<string, XElement> dictionary2 = XUiFromXml.templateData;
		if (dictionary2 != null)
		{
			dictionary2.Clear();
		}
		XUiFromXml.templateData = null;
		IDictionary<string, int> dictionary3 = XUiFromXml.usedWindows;
		if (dictionary3 != null)
		{
			dictionary3.Clear();
		}
		XUiFromXml.usedWindows = null;
		Dictionary<string, Dictionary<string, object>> dictionary4 = XUiFromXml.templateDefaults;
		if (dictionary4 != null)
		{
			dictionary4.Clear();
		}
		XUiFromXml.templateDefaults = null;
		IDictionary<string, int> dictionary5 = XUiFromXml.usedTemplates;
		if (dictionary5 != null)
		{
			dictionary5.Clear();
		}
		XUiFromXml.usedTemplates = null;
		if (XUiFromXml.expressionCache != null)
		{
			foreach (KeyValuePair<string, Expression> keyValuePair in XUiFromXml.expressionCache)
			{
				string text;
				Expression expression;
				keyValuePair.Deconstruct(out text, out expression);
				Expression expression2 = expression;
				expression2.EvaluateFunction -= new EvaluateFunctionHandler(XUiFromXml.nCalcFunctions);
				expression2.EvaluateParameter -= new EvaluateParameterHandler(XUiFromXml.nCalcEvaluateParameter);
			}
		}
		Dictionary<string, Expression> dictionary6 = XUiFromXml.expressionCache;
		if (dictionary6 != null)
		{
			dictionary6.Clear();
		}
		XUiFromXml.expressionCache = null;
	}

	public static void ClearData()
	{
		XUiFromXml.ClearLoadingData();
		Dictionary<string, XUiFromXml.StyleData> dictionary = XUiFromXml.styles;
		if (dictionary != null)
		{
			dictionary.Clear();
		}
		XUiFromXml.styles = null;
	}

	public static bool HasData()
	{
		return XUiFromXml.mainXuiXmlRoot != null && XUiFromXml.windowData.Count > 0 && XUiFromXml.templateData.Count > 0 && XUiFromXml.styles.Count > 0;
	}

	public static IEnumerator Load(XmlFile _xmlFile)
	{
		if (GameManager.IsDedicatedServer)
		{
			yield break;
		}
		if (XUiFromXml.windowData == null)
		{
			XUiFromXml.windowData = new Dictionary<string, XElement>(StringComparer.Ordinal);
		}
		if (XUiFromXml.usedWindows == null)
		{
			XUiFromXml.usedWindows = new SortedDictionary<string, int>(StringComparer.Ordinal);
		}
		if (XUiFromXml.templateData == null)
		{
			XUiFromXml.templateData = new Dictionary<string, XElement>(StringComparer.Ordinal);
		}
		if (XUiFromXml.templateDefaults == null)
		{
			XUiFromXml.templateDefaults = new Dictionary<string, Dictionary<string, object>>(StringComparer.Ordinal);
		}
		if (XUiFromXml.usedTemplates == null)
		{
			XUiFromXml.usedTemplates = new SortedDictionary<string, int>(StringComparer.Ordinal);
		}
		if (XUiFromXml.styles == null)
		{
			XUiFromXml.styles = new CaseInsensitiveStringDictionary<XUiFromXml.StyleData>();
		}
		if (XUiFromXml.expressionCache == null)
		{
			XUiFromXml.expressionCache = new Dictionary<string, Expression>();
		}
		XElement root = _xmlFile.XmlDoc.Root;
		if (root == null || !root.HasElements)
		{
			throw new Exception("No root element found in " + _xmlFile.Filename + "!");
		}
		string localName = root.Name.LocalName;
		if (!(localName == "xui"))
		{
			if (!(localName == "windows"))
			{
				if (!(localName == "styles"))
				{
					if (localName == "templates")
					{
						XUiFromXml.loadTemplates(root);
					}
				}
				else
				{
					XUiFromXml.loadStyles(root);
				}
			}
			else
			{
				XUiFromXml.loadWindows(root);
			}
		}
		else
		{
			XUiFromXml.mainXuiXmlRoot = root;
		}
		yield break;
	}

	public static void LoadDone(bool _logUnused)
	{
		if (!_logUnused)
		{
			return;
		}
		foreach (KeyValuePair<string, int> keyValuePair in XUiFromXml.usedTemplates)
		{
			string text;
			int num;
			keyValuePair.Deconstruct(out text, out num);
			string text2 = text;
			int num2 = num;
			if (XUiFromXml.DebugXuiLoading != XUiFromXml.DebugLevel.Off && (XUiFromXml.DebugXuiLoading != XUiFromXml.DebugLevel.Warning || num2 <= 0))
			{
				if (num2 > 0)
				{
					Log.Out(string.Format("[XUi] Template '{0}' used {1} times!", text2, num2));
				}
				else
				{
					Log.Out("[XUi] Template '" + text2 + "' not used!");
				}
			}
		}
		foreach (KeyValuePair<string, int> keyValuePair in XUiFromXml.usedWindows)
		{
			string text;
			int num;
			keyValuePair.Deconstruct(out text, out num);
			string text3 = text;
			int num3 = num;
			if (XUiFromXml.DebugXuiLoading != XUiFromXml.DebugLevel.Off && (XUiFromXml.DebugXuiLoading != XUiFromXml.DebugLevel.Warning || num3 <= 0))
			{
				if (num3 > 0)
				{
					Log.Out(string.Format("[XUi] Window '{0}' used {1} times!", text3, num3));
				}
				else
				{
					Log.Out("[XUi] Window '" + text3 + "' not used!");
				}
			}
		}
	}

	public static void GetWindowGroupNames(out List<string> _windowGroupNames)
	{
		_windowGroupNames = new List<string>();
		using (IEnumerator<XElement> enumerator = XUiFromXml.mainXuiXmlRoot.Elements("window_group").GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				string text;
				if (enumerator.Current.TryGetAttribute("name", out text) && !_windowGroupNames.Contains(text))
				{
					_windowGroupNames.Add(text);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void loadWindows(XElement _root)
	{
		foreach (XElement xelement in _root.Elements())
		{
			string text;
			if (!xelement.TryGetAttribute("platform", out text) || XUiFromXml.IsMatchingPlatform(text))
			{
				string attribute = xelement.GetAttribute("name");
				if (string.IsNullOrEmpty(attribute))
				{
					Log.Warning("[XUi] windows.xml top level element with empty/missing 'name' attribute");
				}
				else if (XUiFromXml.windowData.TryAdd(attribute, xelement))
				{
					XUiFromXml.usedWindows[attribute] = 0;
				}
				else if (XUiFromXml.DebugXuiLoading != XUiFromXml.DebugLevel.Off)
				{
					Log.Warning("[XUi] window data already contains '" + attribute + "'");
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void loadTemplates(XElement _root)
	{
		foreach (XElement xelement in _root.Elements())
		{
			string localName = xelement.Name.LocalName;
			Dictionary<string, object> dictionary = new CaseInsensitiveStringDictionary<object>();
			foreach (XAttribute xattribute in xelement.Attributes())
			{
				string text = xattribute.Value;
				if (!XUiFromXml.tryResolveStyleRef(text, out text, true))
				{
					XUiFromXml.logForNode(0, xelement, "Style key '" + text + "' not found!");
				}
				else
				{
					if (text.IndexOf("\\n", StringComparison.Ordinal) >= 0)
					{
						text = text.Replace("\\n", "\n", StringComparison.Ordinal);
					}
					dictionary[xattribute.Name.LocalName] = text;
				}
			}
			int num = xelement.Elements().Count<XElement>();
			if (num > 1)
			{
				if (XUiFromXml.DebugXuiLoading != XUiFromXml.DebugLevel.Off)
				{
					Log.Out("[XUi] Template '{0}' cannot have more than a single child node!", new object[] { localName });
				}
			}
			else if (num < 1)
			{
				if (XUiFromXml.DebugXuiLoading != XUiFromXml.DebugLevel.Off)
				{
					Log.Warning("[XUi] Template '{0}' must have a single child node!", new object[] { localName });
					continue;
				}
				continue;
			}
			if (XUiFromXml.templateData.ContainsKey(localName) && XUiFromXml.DebugXuiLoading != XUiFromXml.DebugLevel.Off)
			{
				Log.Warning("[XUi] Template '" + localName + "' already defined, overwriting!");
			}
			XUiFromXml.templateData[localName] = xelement.Elements().First<XElement>();
			XUiFromXml.templateDefaults[localName] = dictionary;
			XUiFromXml.usedTemplates[localName] = 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void loadStyles(XElement _root)
	{
		foreach (XElement xelement in _root.Elements())
		{
			XUiFromXml.StyleData styleData;
			if (xelement.Name == "global")
			{
				if (!XUiFromXml.styles.TryGetValue("global", out styleData))
				{
					styleData = new XUiFromXml.StyleData("global", string.Empty);
					XUiFromXml.styles.Add("global", styleData);
				}
			}
			else
			{
				string text;
				xelement.TryGetAttribute("name", out text);
				string text2;
				xelement.TryGetAttribute("type", out text2);
				if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(text2))
				{
					Log.Warning("[XUi] Style entry with neither 'Type' or 'Name' attribute");
					continue;
				}
				if (text == "*")
				{
					text = "";
				}
				if (text2 == "*")
				{
					text2 = "";
				}
				XUiFromXml.StyleData styleData2 = new XUiFromXml.StyleData(text, text2);
				if (XUiFromXml.styles.TryGetValue(styleData2.KeyName, out styleData))
				{
					if (XUiFromXml.DebugXuiLoading != XUiFromXml.DebugLevel.Off)
					{
						Log.Warning("[XUi] Style '" + styleData2.KeyName + "' already defined, merging contents");
					}
				}
				else
				{
					XUiFromXml.styles.Add(styleData2.KeyName, styleData2);
					styleData = styleData2;
				}
			}
			foreach (XElement xelement2 in xelement.Elements())
			{
				string text3;
				string text4;
				if (!xelement2.TryGetAttribute("name", out text3))
				{
					Log.Error("[XUi] Style '" + styleData.KeyName + "' contains a entry that has no 'name' attribute!");
				}
				else if (!xelement2.TryGetAttribute("value", out text4))
				{
					Log.Error("[XUi] Style '" + styleData.KeyName + "' contains a entry that has no 'value' attribute!");
				}
				else
				{
					XUiFromXml.StyleEntryData styleEntryData = new XUiFromXml.StyleEntryData(text3, text4);
					styleData.StyleEntries[text3] = styleEntryData;
				}
			}
		}
	}

	public static void LoadXui(XUi _xui, string _windowGroupToLoad)
	{
		string text;
		if (XUiFromXml.mainXuiXmlRoot.TryGetAttribute("scale", out text))
		{
			_xui.SetScale(StringParsers.ParseFloat(text, 0, -1, NumberStyles.Any));
		}
		string text2;
		if (XUiFromXml.mainXuiXmlRoot.TryGetAttribute("stackpanel_scale", out text2))
		{
			_xui.SetStackPanelScale(StringParsers.ParseFloat(text2, 0, -1, NumberStyles.Any));
		}
		foreach (XElement xelement in XUiFromXml.mainXuiXmlRoot.Elements("window_group"))
		{
			XUiWindowGroup xuiWindowGroup;
			if (XUiFromXml.parseWindowGroup(_xui, _windowGroupToLoad, xelement, out xuiWindowGroup))
			{
				_xui.WindowGroups.Add(xuiWindowGroup);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool parseWindowGroup(XUi _xui, string _windowGroupToLoad, XElement _groupElement, out XUiWindowGroup _windowGroup)
	{
		string text;
		if (!_groupElement.TryGetAttribute("name", out text))
		{
			text = "";
		}
		if (_xui.FindWindowGroupByName(text) != null || !_windowGroupToLoad.EqualsCaseInsensitive(text))
		{
			_windowGroup = null;
			return false;
		}
		XUiWindowGroup.EHasActionSetFor ehasActionSetFor = XUiWindowGroup.EHasActionSetFor.Both;
		string text2;
		if (_groupElement.TryGetAttribute("actionset", out text2))
		{
			text2 = text2.ToLower().Trim();
			XUiWindowGroup.EHasActionSetFor ehasActionSetFor2;
			if (!(text2 == "true"))
			{
				if (!(text2 == "false"))
				{
					if (!(text2 == "controller"))
					{
						if (!(text2 == "keyboard"))
						{
							ehasActionSetFor2 = ehasActionSetFor;
						}
						else
						{
							ehasActionSetFor2 = XUiWindowGroup.EHasActionSetFor.OnlyKeyboard;
						}
					}
					else
					{
						ehasActionSetFor2 = XUiWindowGroup.EHasActionSetFor.OnlyController;
					}
				}
				else
				{
					ehasActionSetFor2 = XUiWindowGroup.EHasActionSetFor.None;
				}
			}
			else
			{
				ehasActionSetFor2 = XUiWindowGroup.EHasActionSetFor.Both;
			}
			ehasActionSetFor = ehasActionSetFor2;
		}
		string text3;
		if (!_groupElement.TryGetAttribute("defaultselected", out text3))
		{
			text3 = "";
		}
		string text4;
		int minValue;
		if (!_groupElement.TryGetAttribute("stack_panel_y_offset", out text4) || !int.TryParse(text4, out minValue))
		{
			minValue = int.MinValue;
		}
		string text5;
		int minValue2;
		if (!_groupElement.TryGetAttribute("stack_panel_padding", out text5) || !int.TryParse(text5, out minValue2))
		{
			minValue2 = int.MinValue;
		}
		string text6;
		bool flag;
		if (!_groupElement.TryGetAttribute("open_backpack_on_open", out text6) || !StringParsers.TryParseBool(text6, out flag))
		{
			flag = false;
		}
		string text7;
		bool flag2;
		if (!_groupElement.TryGetAttribute("close_compass_on_open", out text7) || !StringParsers.TryParseBool(text7, out flag2))
		{
			flag2 = false;
		}
		_windowGroup = new XUiWindowGroup(_xui, text, ehasActionSetFor, text3, minValue, minValue2, flag, flag2);
		_windowGroup.Controller = XUiFromXml.parseController(_groupElement, _xui, _windowGroup, null);
		string text8;
		if (_groupElement.TryGetAttribute("always_update", out text8))
		{
			StringParsers.TryParseBool(text8, out _windowGroup.Controller.AlwaysUpdate);
		}
		foreach (XElement xelement in _groupElement.Elements("window"))
		{
			XUiV_Window xuiV_Window;
			XUiFromXml.parseWindow(xelement, _windowGroup, out xuiV_Window);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool parseWindow(XElement _windowElement, XUiWindowGroup _windowGroup, out XUiV_Window _window)
	{
		_window = null;
		string text = "";
		if (_windowElement.HasAttribute("name"))
		{
			text = _windowElement.GetAttribute("name");
		}
		XElement xelement;
		if (_windowElement.HasElements)
		{
			xelement = _windowElement;
		}
		else
		{
			XElement xelement2;
			if (!XUiFromXml.windowData.TryGetValue(text, out xelement2))
			{
				if (XUiFromXml.DebugXuiLoading != XUiFromXml.DebugLevel.Off)
				{
					Log.Warning(string.Concat(new string[] { "[XUi] window name '", text, "' not found for window group '", _windowGroup.Id, "'!" }));
				}
				return false;
			}
			IDictionary<string, int> dictionary = XUiFromXml.usedWindows;
			string text2 = text;
			int num = dictionary[text2];
			dictionary[text2] = num + 1;
			xelement = xelement2;
		}
		XUiView xuiView = XUiFromXml.parseViewComponents(xelement, _windowGroup, _windowGroup.Controller, "", null);
		if (xuiView == null)
		{
			return false;
		}
		XUiV_Window xuiV_Window = xuiView as XUiV_Window;
		if (xuiV_Window == null)
		{
			Log.Error(string.Concat(new string[]
			{
				"[XUi] Failed parsing window name '",
				text,
				"' in window group '",
				_windowGroup.Id,
				"': Named element is not a 'Window' view but a '",
				xuiView.GetType().Name,
				"'!"
			}));
			return false;
		}
		_window = xuiV_Window;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiView parseViewComponents(XElement _node, XUiWindowGroup _windowGroup, XUiController _parent, string _nodeNameOverride = "", Dictionary<string, object> _templateParams = null)
	{
		string text;
		if (_node.TryGetAttribute("platform", out text) && !XUiFromXml.IsMatchingPlatform(text))
		{
			return null;
		}
		XUi xui = _windowGroup.xui;
		string localName = _node.Name.LocalName;
		string text2;
		if (!string.IsNullOrEmpty(_nodeNameOverride))
		{
			text2 = _nodeNameOverride;
		}
		else if (!_node.TryGetAttribute("name", out text2))
		{
			text2 = localName;
		}
		bool flag = true;
		bool flag2 = true;
		bool flag3 = false;
		XUiFromXml.parseParams(_node, _parent, _templateParams);
		XUiView xuiView = XUiFromXml.createView(xui, text2, localName, _node, _parent, _windowGroup, _templateParams, ref flag, ref flag2, ref flag3);
		if (flag2)
		{
			xuiView.Controller = XUiFromXml.parseController(_node, xui, _windowGroup, _parent);
			xuiView.SetDefaults(_parent);
			XUiFromXml.parseAttributes(_node, xuiView, _templateParams);
			xuiView.SetPostParsingDefaults(_parent);
		}
		XUiFromXml.parseTweeners(_node, xuiView, _parent, _templateParams);
		if (!flag3 && xuiView.RepeatContent)
		{
			if (_node.Elements().Count<XElement>() != 1)
			{
				if (XUiFromXml.DebugXuiLoading != XUiFromXml.DebugLevel.Off)
				{
					XUiFromXml.logForNode(2, _node, "XUiFromXml::parseByElementName: Invalid repeater child count. Must have one child element.");
				}
			}
			else
			{
				int repeatCount = xuiView.RepeatCount;
				if (_templateParams == null)
				{
					_templateParams = new CaseInsensitiveStringDictionary<object>();
				}
				_templateParams["repeat_count"] = repeatCount;
				XElement xelement = _node.Elements().First<XElement>();
				for (int i = 0; i < repeatCount; i++)
				{
					_templateParams["repeat_i"] = i;
					xuiView.SetRepeatContentTemplateParams(_templateParams, i);
					XElement xelement2 = new XElement(xelement);
					_node.Add(xelement2);
					XUiFromXml.parseViewComponents(xelement2, _windowGroup, xuiView.Controller, i.ToString(), _templateParams);
					xelement2.Remove();
				}
			}
			flag = false;
		}
		if (flag)
		{
			foreach (XElement xelement3 in _node.Elements())
			{
				XUiFromXml.parseViewComponents(xelement3, _windowGroup, xuiView.Controller, "", _templateParams);
			}
		}
		return xuiView;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseTweeners(XElement _node, XUiView _viewComponent, XUiController _parent, Dictionary<string, object> _templateParams)
	{
		foreach (XElement xelement in _node.Elements("tween"))
		{
			XUiTweenAbs xuiTweenAbs;
			if (XUiFromXml.parseTween(_viewComponent, xelement, _parent, _templateParams, out xuiTweenAbs))
			{
				_viewComponent.Tweeners.Add(xuiTweenAbs);
			}
		}
		_node.Elements("tween").Remove<XElement>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool parseTween(XUiView _view, XElement _tweenElement, XUiController _parent, Dictionary<string, object> _templateParams, out XUiTweenAbs _result)
	{
		_result = null;
		XUiFromXml.parseParams(_tweenElement, _parent, _templateParams);
		string text;
		if (!_tweenElement.TryGetAttribute("type", out text))
		{
			XUiFromXml.logForNode(0, _tweenElement, "Tween element without 'type' attribute");
			return false;
		}
		XUiTweenAbs.ETweenType etweenType;
		if (!EnumUtils.TryParse<XUiTweenAbs.ETweenType>(text, out etweenType, true))
		{
			XUiFromXml.logForNode(0, _tweenElement, "Tween element with invalid 'type' value ('" + text + "')");
			return false;
		}
		try
		{
			XUiTweenAbs xuiTweenAbs;
			switch (etweenType)
			{
			case XUiTweenAbs.ETweenType.Alpha:
				xuiTweenAbs = new XUiTweenAlpha(_view);
				break;
			case XUiTweenAbs.ETweenType.Color:
				xuiTweenAbs = new XUiTweenColor(_view);
				break;
			case XUiTweenAbs.ETweenType.Fill:
				xuiTweenAbs = new XUiTweenFill(_view);
				break;
			case XUiTweenAbs.ETweenType.Height:
				xuiTweenAbs = new XUiTweenHeight(_view);
				break;
			case XUiTweenAbs.ETweenType.Position:
				xuiTweenAbs = new XUiTweenPosition(_view);
				break;
			case XUiTweenAbs.ETweenType.Rotation:
				xuiTweenAbs = new XUiTweenRotation(_view);
				break;
			case XUiTweenAbs.ETweenType.Scale:
				xuiTweenAbs = new XUiTweenScale(_view);
				break;
			case XUiTweenAbs.ETweenType.Width:
				xuiTweenAbs = new XUiTweenWidth(_view);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			_result = xuiTweenAbs;
		}
		catch (Exception ex)
		{
			XUiFromXml.logForNode(0, _tweenElement, ex.Message);
			Log.Exception(ex);
			return false;
		}
		foreach (XAttribute xattribute in _tweenElement.Attributes())
		{
			string text2 = xattribute.Name.LocalName.ToLower();
			if (!(text2 == "type"))
			{
				string text3 = xattribute.Value;
				if (!XUiFromXml.tryResolveStyleRef(text3, out text3, true))
				{
					XUiFromXml.logForNode(0, _tweenElement, "Style key '" + text3 + "' not found!");
				}
				else
				{
					if (text3.IndexOf("\\n", StringComparison.Ordinal) >= 0)
					{
						text3 = text3.Replace("\\n", "\n", StringComparison.Ordinal);
					}
					_result.ParseInitialAttributeValue(text2, text3);
				}
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiView createView(XUi _xui, string _name, string _type, XElement _node, XUiController _parent, XUiWindowGroup _windowGroup, Dictionary<string, object> _templateParams, ref bool _parseChildren, ref bool _parseControllerAndAttributes, ref bool _replacedByTemplate)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_type);
		if (num <= 2354395792U)
		{
			if (num <= 1179827136U)
			{
				if (num != 1013213428U)
				{
					if (num != 1135768689U)
					{
						if (num == 1179827136U)
						{
							if (_type == "gamepad_icon")
							{
								return new XUiV_GamepadIcon(_xui, _name);
							}
						}
					}
					else if (_type == "button")
					{
						return new XUiV_Button(_xui, _name);
					}
				}
				else if (_type == "texture")
				{
					return new XUiV_Texture(_xui, _name);
				}
			}
			else if (num <= 2179094556U)
			{
				if (num != 1251777503U)
				{
					if (num == 2179094556U)
					{
						if (_type == "sprite")
						{
							return new XUiV_Sprite(_xui, _name);
						}
					}
				}
				else if (_type == "table")
				{
					return new XUiV_Table(_xui, _name);
				}
			}
			else if (num != 2240103498U)
			{
				if (num == 2354395792U)
				{
					if (_type == "filledsprite")
					{
						return new XUiV_FilledSprite(_xui, _name);
					}
				}
			}
			else if (_type == "textlist")
			{
				return new XUiV_TextList(_xui, _name);
			}
		}
		else if (num <= 3472427884U)
		{
			if (num <= 2944866961U)
			{
				if (num != 2708649949U)
				{
					if (num == 2944866961U)
					{
						if (_type == "grid")
						{
							return new XUiV_Grid(_xui, _name);
						}
					}
				}
				else if (_type == "window")
				{
					return new XUiV_Window(_xui, _name);
				}
			}
			else if (num != 3439217733U)
			{
				if (num == 3472427884U)
				{
					if (_type == "video")
					{
						return new XUiV_Video(_xui, _name);
					}
				}
			}
			else if (_type == "panel")
			{
				return new XUiV_Panel(_xui, _name);
			}
		}
		else if (num <= 3940830471U)
		{
			if (num != 3585523833U)
			{
				if (num == 3940830471U)
				{
					if (_type == "rect")
					{
						return new XUiV_Rect(_xui, _name);
					}
				}
			}
			else if (_type == "scrollbar")
			{
				return new XUiV_ScrollBar(_xui, _name);
			}
		}
		else if (num != 4137097213U)
		{
			if (num == 4172442677U)
			{
				if (_type == "scrollview")
				{
					return new XUiV_ScrollView(_xui, _name);
				}
			}
		}
		else if (_type == "label")
		{
			return new XUiV_Label(_xui, _name);
		}
		return XUiFromXml.createFromTemplate(_type, _name, _node, _parent, _windowGroup, _templateParams, ref _parseChildren, ref _parseControllerAndAttributes, ref _replacedByTemplate);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiView createFromTemplate(string _templateName, string _viewName, XElement _node, XUiController _parent, XUiWindowGroup _windowGroup, Dictionary<string, object> _outerParams, ref bool _parseChildren, ref bool _parseControllerAndAttributes, ref bool _replacedByTemplate)
	{
		XElement xelement;
		if (!XUiFromXml.templateData.TryGetValue(_templateName, out xelement))
		{
			if (XUiFromXml.DebugXuiLoading != XUiFromXml.DebugLevel.Off)
			{
				XUiFromXml.logForNode(2, _node, "Template \"" + _templateName + "\" not found!");
			}
			return XUiFromXml.createEmptyView(_viewName, _parent, _windowGroup, out _parseControllerAndAttributes);
		}
		if (_node.HasElements)
		{
			if (XUiFromXml.DebugXuiLoading != XUiFromXml.DebugLevel.Off)
			{
				XUiFromXml.logForNode(2, _node, "Instantiation of templates may not have any child nodes!");
			}
			_parseChildren = false;
			return XUiFromXml.createEmptyView(_viewName, _parent, _windowGroup, out _parseControllerAndAttributes);
		}
		Dictionary<string, object> dictionary = new CaseInsensitiveStringDictionary<object>();
		if (_outerParams != null)
		{
			_outerParams.CopyTo(dictionary, false);
		}
		if (((_parent != null) ? _parent.ViewComponent : null) != null)
		{
			dictionary["width"] = _parent.ViewComponent.InnerSize.x;
			dictionary["height"] = _parent.ViewComponent.InnerSize.y;
			dictionary["outerwidth"] = _parent.ViewComponent.Size.x;
			dictionary["outerheight"] = _parent.ViewComponent.Size.y;
		}
		Dictionary<string, object> dictionary2;
		if (XUiFromXml.templateDefaults.TryGetValue(_templateName, out dictionary2))
		{
			dictionary2.CopyTo(dictionary, true);
		}
		XUiFromXml.parseAttributes(_node, null, dictionary);
		XElement xelement2 = new XElement(xelement);
		IDictionary<string, int> dictionary3 = XUiFromXml.usedTemplates;
		int num = dictionary3[_templateName];
		dictionary3[_templateName] = num + 1;
		_node.Add(xelement2);
		XUiView xuiView = XUiFromXml.parseViewComponents(xelement2, _windowGroup, _parent, _viewName, dictionary);
		if (xuiView == null)
		{
			return null;
		}
		xelement2.Remove();
		_parseChildren = false;
		_parseControllerAndAttributes = false;
		_replacedByTemplate = true;
		return xuiView;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiView createEmptyView(string _viewName, XUiController _parent, XUiWindowGroup _windowGroup, out bool _parseControllerAndAttributes)
	{
		XUiV_Empty xuiV_Empty = new XUiV_Empty(_windowGroup.xui, _viewName);
		xuiV_Empty.Controller = new XUiController
		{
			xui = _windowGroup.xui,
			WindowGroup = _windowGroup,
			Parent = _parent
		};
		xuiV_Empty.SetDefaults(_parent);
		_parseControllerAndAttributes = false;
		return xuiV_Empty;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseParams(XElement _node, XUiController _parent, Dictionary<string, object> _templateParams)
	{
		foreach (XAttribute xattribute in _node.Attributes())
		{
			string text = xattribute.Value;
			bool flag = false;
			int num;
			while ((num = text.LastIndexOf("${", StringComparison.Ordinal)) >= 0)
			{
				int num2 = text.IndexOf('}', num);
				int num3 = num2 - num + 1;
				if (num2 < 0)
				{
					XUiFromXml.logForNode(0, _node, string.Format("Expression has unclosed parameter references: {0}={1}", xattribute.Name, text));
					break;
				}
				string text2 = text.Substring(num + 2, num2 - (num + 2));
				Expression expression;
				if (!XUiFromXml.expressionCache.TryGetValue(text2, out expression))
				{
					expression = new Expression(text2, 514);
					expression.EvaluateFunction += new EvaluateFunctionHandler(XUiFromXml.nCalcFunctions);
					expression.EvaluateParameter += new EvaluateParameterHandler(XUiFromXml.nCalcEvaluateParameter);
					XUiFromXml.expressionCache.Add(text2, expression);
				}
				XUiFromXml.ncalcCurrentViewParent = _parent;
				expression.Parameters = _templateParams;
				string text4;
				try
				{
					object obj = expression.Evaluate();
					string text3;
					if (obj is decimal)
					{
						decimal num4 = (decimal)obj;
						text3 = num4.ToCultureInvariantString("0.########");
					}
					else if (obj is float)
					{
						float num5 = (float)obj;
						text3 = num5.ToCultureInvariantString();
					}
					else if (obj is double)
					{
						double num6 = (double)obj;
						text3 = num6.ToCultureInvariantString();
					}
					else
					{
						text3 = obj.ToString();
					}
					text4 = text3;
				}
				catch (ArgumentException ex)
				{
					XUiFromXml.logForNode(0, _node, string.Format("Template parameter '{0}' undefined (in: {1}=\"{2}\")", ex.ParamName, xattribute.Name, text));
					text4 = "";
				}
				catch (Exception ex2)
				{
					XUiFromXml.logForNode(4, _node, "Template expression can not be evaluated: " + text2);
					Log.Exception(ex2);
					text4 = "";
				}
				XUiFromXml.ncalcCurrentViewParent = null;
				text = text.Remove(num, num3).Insert(num, text4);
				flag = true;
			}
			if (flag)
			{
				xattribute.Value = text;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void nCalcEvaluateParameter(string _name, ParameterArgs _args)
	{
		XUiController xuiController = XUiFromXml.ncalcCurrentViewParent;
		XUiView xuiView = ((xuiController != null) ? xuiController.ViewComponent : null);
		if (xuiView == null)
		{
			return;
		}
		if (_name == "parentinnerwidth")
		{
			_args.Result = xuiView.InnerSize.x;
			return;
		}
		if (_name == "parentinnerheight")
		{
			_args.Result = xuiView.InnerSize.y;
			return;
		}
		if (_name == "parentouterwidth")
		{
			_args.Result = xuiView.Size.x;
			return;
		}
		if (!(_name == "parentouterheight"))
		{
			return;
		}
		_args.Result = xuiView.Size.y;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void nCalcFunctions(string _name, FunctionArgs _args, bool _ignoreCase)
	{
		if (_name.EqualsCaseInsensitive("defined"))
		{
			XUiFromXml.nCalcFuncIdentifierDefined(_args, _ignoreCase);
			return;
		}
		if (_name.EqualsCaseInsensitive("style"))
		{
			XUiFromXml.nCalcFuncStyle(_args, _ignoreCase);
			return;
		}
		if (_name.EqualsCaseInsensitive("length"))
		{
			XUiFromXml.nCalcFuncLength(_args, _ignoreCase);
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void nCalcFuncLength(FunctionArgs _args, bool _ignoreCase)
	{
		Expression[] parameters = _args.Parameters;
		if (parameters.Length != 1)
		{
			return;
		}
		string text = parameters[0].Evaluate() as string;
		if (text == null)
		{
			return;
		}
		_args.Result = text.Length;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void nCalcFuncStyle(FunctionArgs _args, bool _ignoreCase)
	{
		Expression[] parameters = _args.Parameters;
		if (parameters.Length != 1)
		{
			return;
		}
		string text = parameters[0].Evaluate() as string;
		if (text == null)
		{
			return;
		}
		string text2;
		if (!XUiFromXml.tryResolveStyleRef(text, out text2, false))
		{
			throw new ArgumentException("", text);
		}
		_args.Result = text2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void nCalcFuncIdentifierDefined(FunctionArgs _args, bool _ignoreCase)
	{
		Expression[] parameters = _args.Parameters;
		if (parameters.Length != 1)
		{
			return;
		}
		Identifier identifier = parameters[0].ParsedExpression as Identifier;
		if (identifier == null)
		{
			return;
		}
		string name = identifier.Name;
		_args.Result = parameters[0].Parameters.ContainsKey(name);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseAttributes(XElement _node, XUiView _viewComponent, Dictionary<string, object> _templateParams = null)
	{
		string localName = _node.Name.LocalName;
		XUiFromXml.tryApplyAttributesFromStyle("", "", _viewComponent, _templateParams);
		XUiFromXml.tryApplyAttributesFromStyle(localName, "", _viewComponent, _templateParams);
		string text;
		if (_node.TryGetAttribute("style", out text))
		{
			string[] array;
			if (!XUiFromXml.styleNameSplitCache.TryGetValue(text, out array))
			{
				array = text.Replace(" ", "").Split(',', StringSplitOptions.None);
				XUiFromXml.styleNameSplitCache[text] = array;
			}
			foreach (string text2 in array)
			{
				if (!(XUiFromXml.tryApplyAttributesFromStyle("", text2, _viewComponent, _templateParams) | XUiFromXml.tryApplyAttributesFromStyle(localName, text2, _viewComponent, _templateParams)))
				{
					XUiFromXml.logForNode(0, _node, string.Concat(new string[] { "No style with name '", text2, "' (and optional type '", localName, "') found!" }));
				}
			}
		}
		foreach (XAttribute xattribute in _node.Attributes())
		{
			string localName2 = xattribute.Name.LocalName;
			if (!(localName2 == "style"))
			{
				string text3 = xattribute.Value;
				if (!XUiFromXml.tryResolveStyleRef(text3, out text3, true))
				{
					XUiFromXml.logForNode(0, _node, "Style key '" + text3 + "' not found!");
				}
				else
				{
					if (text3.IndexOf("\\n", StringComparison.Ordinal) >= 0)
					{
						text3 = text3.Replace("\\n", "\n", StringComparison.Ordinal);
					}
					string text4 = localName2.ToLower();
					XUiFromXml.parseAttribute(_viewComponent, text4, text3, _templateParams);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryApplyAttributesFromStyle(string _typeName, string _styleName, XUiView _viewComponent, Dictionary<string, object> _templateParams = null)
	{
		if (_typeName == null)
		{
			_typeName = "";
		}
		if (_styleName == null)
		{
			_styleName = "";
		}
		Dictionary<string, string> dictionary;
		if (!XUiFromXml.styleNameTypeToStyleKeyCache.TryGetValue(_styleName, out dictionary))
		{
			dictionary = new Dictionary<string, string>();
			XUiFromXml.styleNameTypeToStyleKeyCache[_styleName] = dictionary;
		}
		string text;
		if (!dictionary.TryGetValue(_typeName, out text))
		{
			text = _typeName + "." + _styleName;
			dictionary[_typeName] = text;
		}
		XUiFromXml.StyleData styleData;
		if (!XUiFromXml.styles.TryGetValue(text, out styleData))
		{
			return false;
		}
		foreach (KeyValuePair<string, XUiFromXml.StyleEntryData> keyValuePair in styleData.StyleEntries)
		{
			string text2;
			XUiFromXml.StyleEntryData styleEntryData;
			keyValuePair.Deconstruct(out text2, out styleEntryData);
			XUiFromXml.StyleEntryData styleEntryData2 = styleEntryData;
			XUiFromXml.parseAttribute(_viewComponent, styleEntryData2.Name, styleEntryData2.Value, _templateParams);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseAttribute(XUiView _viewComponent, string _attributeNameLower, string _value, Dictionary<string, object> _templateParams)
	{
		if (_viewComponent == null)
		{
			_templateParams[_attributeNameLower] = _value;
			return;
		}
		_viewComponent.ParseInitialAttributeValue(_attributeNameLower, _value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiController parseController(XElement _node, XUi _xui, XUiWindowGroup _windowGroup, XUiController _parent)
	{
		XUiController xuiController = null;
		string text;
		if (_node.TryGetAttribute("controller", out text))
		{
			Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("XUiC_", text);
			if (typeWithPrefix == null)
			{
				XUiFromXml.logForNode(0, _node, "Controller '" + text + "' not found, using base XUiController");
			}
			else if (typeWithPrefix.IsAbstract)
			{
				XUiFromXml.logForNode(0, _node, "Controller '" + text + "' not instantiable, class is abstract");
			}
			else
			{
				xuiController = (XUiController)Activator.CreateInstance(typeWithPrefix);
			}
		}
		if (xuiController == null)
		{
			xuiController = new XUiController();
		}
		xuiController.xui = _xui;
		xuiController.WindowGroup = _windowGroup;
		xuiController.Parent = _parent;
		return xuiController;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void logForNode(LogType _level, XElement _node, string _message)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[XUi] ");
		stringBuilder.Append(_message);
		stringBuilder.Append(" --- hierarchy: ");
		XUiFromXml.logTree(stringBuilder, _node);
		string text = stringBuilder.ToString();
		switch (_level)
		{
		case 0:
		case 4:
			Log.Error(text);
			return;
		case 2:
			Log.Warning(text);
			return;
		case 3:
			Log.Out(text);
			return;
		}
		throw new ArgumentOutOfRangeException();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void logTree(StringBuilder _sb, XElement _node)
	{
		if (_node.Parent != null)
		{
			XUiFromXml.logTree(_sb, _node.Parent);
			_sb.Append(" -> ");
		}
		if (_node.HasAttribute("name"))
		{
			_sb.Append(_node.Name);
			_sb.Append(" (");
			_sb.Append(_node.GetAttribute("name"));
			_sb.Append(")");
			return;
		}
		_sb.Append(_node.Name);
	}

	public static bool IsMatchingPlatform(string _platformStr)
	{
		bool flag = true;
		string[] array = _platformStr.Split(",", StringSplitOptions.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim().ToUpper();
			if (!array[i].StartsWith("!"))
			{
				flag = false;
			}
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (Submission.Enabled)
			{
				if (array[j] == "SUBMISSION")
				{
					return true;
				}
				if (array[j] == "!SUBMISSION")
				{
					return false;
				}
			}
			if (DeviceFlag.StandaloneWindows.IsCurrent())
			{
				if (array[j] == "WINDOWS")
				{
					return true;
				}
				if (array[j] == "!WINDOWS")
				{
					return false;
				}
			}
			if (DeviceFlag.StandaloneLinux.IsCurrent())
			{
				if (array[j] == "LINUX")
				{
					return true;
				}
				if (array[j] == "!LINUX")
				{
					return false;
				}
			}
			if (DeviceFlag.StandaloneOSX.IsCurrent())
			{
				if (array[j] == "OSX")
				{
					return true;
				}
				if (array[j] == "!OSX")
				{
					return false;
				}
			}
			if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
			{
				if (array[j] == "STANDALONE")
				{
					return true;
				}
				if (array[j] == "!STANDALONE")
				{
					return false;
				}
			}
			if (DeviceFlag.PS5.IsCurrent())
			{
				if (array[j] == "PS5")
				{
					return true;
				}
				if (array[j] == "!PS5")
				{
					return false;
				}
			}
			if (DeviceFlag.XBoxSeriesS.IsCurrent())
			{
				if (array[j] == "XBOX_S")
				{
					return true;
				}
				if (array[j] == "!XBOX_S")
				{
					return false;
				}
			}
			if (DeviceFlag.XBoxSeriesX.IsCurrent())
			{
				if (array[j] == "XBOX_X")
				{
					return true;
				}
				if (array[j] == "!XBOX_X")
				{
					return false;
				}
			}
			if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent())
			{
				if (array[j] == "XBOX")
				{
					return true;
				}
				if (array[j] == "!XBOX")
				{
					return false;
				}
			}
			if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
			{
				if (array[j] == "CONSOLE")
				{
					return true;
				}
				if (array[j] == "!CONSOLE")
				{
					return false;
				}
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryResolveStyleRef(string _value, out string _resolved, bool _expectBrackets = true)
	{
		_resolved = _value;
		if (_expectBrackets && (!_value.StartsWith("[", StringComparison.Ordinal) || _value.IndexOf("]", StringComparison.Ordinal) != _value.Length - 1 || _value.IndexOf("[", 1, StringComparison.Ordinal) >= 0))
		{
			return true;
		}
		string text = _value;
		if (text.StartsWith("[", StringComparison.Ordinal))
		{
			text = text.Substring(1, _value.Length - 2);
		}
		int num = text.IndexOf(':');
		if (num > 0)
		{
			string text2 = text.Substring(0, num);
			string text3 = text.Substring(num + 1);
			XUiFromXml.StyleData styleData;
			if (!XUiFromXml.styles.TryGetValue(text2, out styleData))
			{
				XUiFromXml.styles.TryGetValue("." + text2, out styleData);
			}
			XUiFromXml.StyleEntryData styleEntryData;
			if (styleData != null && styleData.StyleEntries.TryGetValue(text3, out styleEntryData))
			{
				_resolved = styleEntryData.Value;
				return true;
			}
		}
		XUiFromXml.StyleEntryData styleEntryData2;
		if (XUiFromXml.styles["global"].StyleEntries.TryGetValue(text, out styleEntryData2))
		{
			_resolved = styleEntryData2.Value;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string globalStyleName = "global";

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, XElement> windowData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static IDictionary<string, int> usedWindows;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, XElement> templateData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, Dictionary<string, object>> templateDefaults;

	[PublicizedFrom(EAccessModifier.Private)]
	public static IDictionary<string, int> usedTemplates;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, XUiFromXml.StyleData> styles;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, Expression> expressionCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public static XElement mainXuiXmlRoot;

	public static readonly XUiFromXml.DebugLevel DebugXuiLoading;

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiController ncalcCurrentViewParent;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, string[]> styleNameSplitCache = new Dictionary<string, string[]>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, Dictionary<string, string>> styleNameTypeToStyleKeyCache = new Dictionary<string, Dictionary<string, string>>();

	public enum DebugLevel
	{
		Off,
		Warning,
		Verbose
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class StyleData
	{
		public StyleData(string _name, string _type)
		{
			this.KeyName = _type + "." + _name;
		}

		public readonly string KeyName;

		public readonly Dictionary<string, XUiFromXml.StyleEntryData> StyleEntries = new Dictionary<string, XUiFromXml.StyleEntryData>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class StyleEntryData
	{
		public string Value
		{
			get
			{
				string text = this.value;
				string text2;
				if (XUiFromXml.tryResolveStyleRef(text, out text2, true))
				{
					text = text2;
					this.value = text;
				}
				return text;
			}
		}

		public StyleEntryData(string _name, string _value)
		{
			this.Name = _name;
			this.value = _value;
		}

		public readonly string Name;

		[PublicizedFrom(EAccessModifier.Private)]
		public string value;
	}
}
