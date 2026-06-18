using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

public static class LabelUrlUtils
{
	public static void RegisterLabelUrlHandler(string _type, LabelUrlUtils.HandleTextClickUrlDelegate _clickHandler, LabelUrlUtils.HandleTextHoverUrlDelegate _hoverHandler)
	{
		if (LabelUrlUtils.urlHandlers.ContainsKey(_type))
		{
			Log.Warning("Registering a new label URL handler for the type '" + _type + "'");
		}
		LabelUrlUtils.urlHandlers[_type] = new LabelUrlUtils.SHandlerData(_clickHandler, _hoverHandler);
	}

	public static void HandleLabelUrlClick(XUiView _view, UILabel _label, HashSet<string> _allowedTypes = null)
	{
		LabelUrlUtils.SHandlerData shandlerData;
		string text;
		Dictionary<string, string> dictionary;
		if (LabelUrlUtils.tryGetDataForLabelPos(_view, _label, _allowedTypes, out shandlerData, out text, out dictionary))
		{
			LabelUrlUtils.HandleTextClickUrlDelegate clickDelegate = shandlerData.ClickDelegate;
			if (clickDelegate == null)
			{
				return;
			}
			clickDelegate(_view, text, dictionary);
		}
	}

	public static void HandleLabelUrlHover(XUiView _view, UILabel _label, HashSet<string> _allowedTypes, out bool _isOverUrl, out string _tooltipText)
	{
		LabelUrlUtils.SHandlerData shandlerData;
		string text;
		Dictionary<string, string> dictionary;
		if (LabelUrlUtils.tryGetDataForLabelPos(_view, _label, _allowedTypes, out shandlerData, out text, out dictionary) && shandlerData.HoverDelegate != null)
		{
			_isOverUrl = true;
			shandlerData.HoverDelegate(_view, text, dictionary, out _tooltipText);
			return;
		}
		_isOverUrl = false;
		_tooltipText = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryGetDataForLabelPos(XUiView _view, UILabel _label, HashSet<string> _allowedTypes, out LabelUrlUtils.SHandlerData _handler, out string _fullUrlString, out Dictionary<string, string> _urlElements)
	{
		_handler = default(LabelUrlUtils.SHandlerData);
		_urlElements = null;
		_fullUrlString = _label.GetUrlAtPosition(UICamera.lastWorldPosition);
		if (string.IsNullOrEmpty(_fullUrlString))
		{
			return false;
		}
		if (_fullUrlString.StartsWith("http://") || _fullUrlString.StartsWith("https://"))
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["Type"] = "HTTP";
			dictionary["Target"] = _fullUrlString;
			_urlElements = dictionary;
		}
		else
		{
			MatchCollection matchCollection = LabelUrlUtils.urlMatcher.Matches(_fullUrlString);
			if (matchCollection.Count == 0)
			{
				Log.Warning("Text URL ('" + _fullUrlString + "'): Invalid URL");
				return false;
			}
			_urlElements = new Dictionary<string, string>();
			foreach (object obj in matchCollection)
			{
				Match match = (Match)obj;
				_urlElements[match.Groups[1].Value] = match.Groups[2].Value;
			}
		}
		string text;
		if (!_urlElements.TryGetValue("Type", out text))
		{
			Log.Warning(string.Concat(new string[] { "Text URL ('", _fullUrlString, "'): No explicit or implicit type defined in '", _fullUrlString, "'" }));
			return false;
		}
		if (!LabelUrlUtils.urlHandlers.TryGetValue(text, out _handler))
		{
			Log.Warning(string.Concat(new string[] { "Text URL ('", _fullUrlString, "'): No handler for type '", text, "'" }));
			return false;
		}
		if (_allowedTypes != null && !_allowedTypes.Contains(text))
		{
			Log.Warning(string.Concat(new string[] { "Text URL ('", _fullUrlString, "'): URL type '", text, "' not allowed on this label" }));
			return false;
		}
		return true;
	}

	public static string BuildUrlFunctionString(string _type)
	{
		return "[url=Type>" + _type + "]";
	}

	public static string BuildUrlFunctionString(string _type, [TupleElementNames(new string[] { "key", "value" })] ValueTuple<string, string> _param1)
	{
		return string.Concat(new string[] { "[url=Type>", _type, "|", _param1.Item1, ">", _param1.Item2, "]" });
	}

	public static string BuildUrlFunctionString(string _type, [TupleElementNames(new string[] { "key", "value" })] ValueTuple<string, string> _param1, [TupleElementNames(new string[] { "key", "value" })] ValueTuple<string, string> _param2)
	{
		return string.Concat(new string[]
		{
			"[url=Type>", _type, "|", _param1.Item1, ">", _param1.Item2, "|", _param2.Item1, ">", _param2.Item2,
			"]"
		});
	}

	public static string BuildUrlFunctionString(string _type, [TupleElementNames(new string[] { "key", "value" })] ValueTuple<string, string> _param1, [TupleElementNames(new string[] { "key", "value" })] ValueTuple<string, string> _param2, [TupleElementNames(new string[] { "key", "value" })] ValueTuple<string, string> _param3)
	{
		return string.Concat(new string[]
		{
			"[url=Type>", _type, "|", _param1.Item1, ">", _param1.Item2, "|", _param2.Item1, ">", _param2.Item2,
			"|", _param3.Item1, ">", _param3.Item2, "]"
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static LabelUrlUtils()
	{
		LabelUrlUtils.RegisterLabelUrlHandler("Chat", new LabelUrlUtils.HandleTextClickUrlDelegate(LabelUrlUtils.handleChatTargetUrlClick), new LabelUrlUtils.HandleTextHoverUrlDelegate(LabelUrlUtils.handleChatTargetUrlHover));
		LabelUrlUtils.RegisterLabelUrlHandler("HTTP", new LabelUrlUtils.HandleTextClickUrlDelegate(LabelUrlUtils.handleHttpUrl), new LabelUrlUtils.HandleTextHoverUrlDelegate(LabelUrlUtils.handleHttpUrlHover));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void handleChatTargetUrlHover(XUiView _sender, string _sourceUrl, Dictionary<string, string> _urlElements, out string _tooltipText)
	{
		_tooltipText = null;
		string text;
		if (!_urlElements.TryGetValue("ChatType", out text))
		{
			Log.Warning("Chat URL ('" + _sourceUrl + "'): No ChatType defined");
			return;
		}
		string text2;
		_urlElements.TryGetValue("Sender", out text2);
		EChatType echatType;
		if (!EnumUtils.TryParse<EChatType>(text, out echatType, false))
		{
			Log.Warning(string.Concat(new string[] { "Chat URL ('", _sourceUrl, "'): Invalid chat type value '", text, "'" }));
			return;
		}
		string text3;
		if (XUiC_Chat.GetChatTargetTooltip(_sender.xui, echatType, text2, out text3))
		{
			_tooltipText = "Chat with: " + text3;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void handleChatTargetUrlClick(XUiView _sender, string _sourceUrl, Dictionary<string, string> _urlElements)
	{
		string text;
		if (!_urlElements.TryGetValue("ChatType", out text))
		{
			Log.Warning("Chat URL ('" + _sourceUrl + "'): No ChatType defined");
			return;
		}
		string text2;
		_urlElements.TryGetValue("Sender", out text2);
		EChatType echatType;
		if (!EnumUtils.TryParse<EChatType>(text, out echatType, false))
		{
			Log.Warning(string.Concat(new string[] { "Chat URL ('", _sourceUrl, "'): Invalid chat type value '", text, "'" }));
			return;
		}
		XUiC_Chat.SetChatTarget(_sender.xui, echatType, text2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void handleHttpUrlHover(XUiView _sender, string _sourceUrl, Dictionary<string, string> _urlElements, out string _tooltipText)
	{
		_tooltipText = null;
		string text;
		if (!_urlElements.TryGetValue("Target", out text))
		{
			Log.Warning("Web URL (" + _sourceUrl + "): No Target defined");
			return;
		}
		_tooltipText = text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void handleHttpUrl(XUiView _sender, string _sourceUrl, Dictionary<string, string> _urlElements)
	{
		string text;
		if (!_urlElements.TryGetValue("Target", out text))
		{
			Log.Warning("Web URL (" + _sourceUrl + "): No Target defined");
			return;
		}
		XUiC_MessageBoxWindowGroup.ShowUrlConfirmationDialog(_sender.xui, text, false, null, null, null, null, false);
	}

	public const string LabelUrlTypeFieldName = "Type";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, LabelUrlUtils.SHandlerData> urlHandlers = new Dictionary<string, LabelUrlUtils.SHandlerData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex urlMatcher = new Regex("([^|]+)>([^|]+)", RegexOptions.Compiled | RegexOptions.Singleline);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly struct SHandlerData
	{
		public SHandlerData(LabelUrlUtils.HandleTextClickUrlDelegate _clickDelegate, LabelUrlUtils.HandleTextHoverUrlDelegate _hoverDelegate)
		{
			this.ClickDelegate = _clickDelegate;
			this.HoverDelegate = _hoverDelegate;
		}

		public readonly LabelUrlUtils.HandleTextClickUrlDelegate ClickDelegate;

		public readonly LabelUrlUtils.HandleTextHoverUrlDelegate HoverDelegate;
	}

	public delegate void HandleTextClickUrlDelegate(XUiView _sender, string _sourceUrl, Dictionary<string, string> _urlElements);

	public delegate void HandleTextHoverUrlDelegate(XUiView _sender, string _sourceUrl, Dictionary<string, string> _urlElements, out string _tooltipText);
}
