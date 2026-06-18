using System;
using System.Collections.Generic;
using System.Xml.Linq;
using NCalc;

public static class XmlPatchConditionEvaluator
{
	public static XElement FindActiveConditionalBranchElement(XmlFile _xmlFile, XElement _conditionalElement)
	{
		foreach (XElement xelement in _conditionalElement.Elements())
		{
			string localName = xelement.Name.LocalName;
			if (localName.EqualsCaseInsensitive("if"))
			{
				XAttribute xattribute = xelement.Attribute("cond");
				if (xattribute == null)
				{
					throw new XmlPatchException(xelement, "Conditional", "Patch child 'if'-element does not have an 'cond' attribute");
				}
				string value = xattribute.Value;
				if (XmlPatchConditionEvaluator.Evaluate(_xmlFile, xelement, value))
				{
					return xelement;
				}
			}
			else
			{
				if (localName.EqualsCaseInsensitive("else"))
				{
					return xelement;
				}
				throw new XmlPatchException(xelement, "Conditional", "Unexpected child element '" + localName + "' in conditional patch block");
			}
		}
		return null;
	}

	public static bool Evaluate(XmlFile _xmlFile, XElement _xmlElement, string _expression)
	{
		Expression expression = new Expression(_expression, 1542);
		expression.Parameters["xml"] = _xmlFile;
		expression.EvaluateFunction += new EvaluateFunctionHandler(XmlPatchConditionEvaluator.NCalcEvaluateFunction);
		object obj;
		try
		{
			obj = expression.Evaluate();
		}
		catch (Exception ex)
		{
			throw new XmlPatchException(_xmlElement, "Evaluate", "Error evaluating conditional expression: " + _expression, ex);
		}
		if (obj is bool)
		{
			return (bool)obj;
		}
		throw new XmlPatchException(_xmlElement, "Evaluate", string.Format("Conditional expression did not evaluate to a boolean value: {0} == {1}", _expression, obj));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void NCalcEvaluateFunction(string _name, FunctionArgs _args, bool _ignoreCase)
	{
		if (_name.EqualsCaseInsensitive("mod_loaded"))
		{
			XmlPatchConditionEvaluator.mod_loaded(_args);
			return;
		}
		if (_name.EqualsCaseInsensitive("mod_version"))
		{
			XmlPatchConditionEvaluator.mod_version(_args);
			return;
		}
		if (_name.EqualsCaseInsensitive("game_version"))
		{
			XmlPatchConditionEvaluator.game_version(_args);
			return;
		}
		if (_name.EqualsCaseInsensitive("version"))
		{
			XmlPatchConditionEvaluator.version(_args);
			return;
		}
		if (_name.EqualsCaseInsensitive("serverinfo"))
		{
			XmlPatchConditionEvaluator.serverinfo(_args);
			return;
		}
		if (_name.EqualsCaseInsensitive("gamepref"))
		{
			XmlPatchConditionEvaluator.gamepref(_args);
			return;
		}
		if (_name.EqualsCaseInsensitive("event"))
		{
			XmlPatchConditionEvaluator.eventActive(_args);
			return;
		}
		if (_name.EqualsCaseInsensitive("time_minutes"))
		{
			XmlPatchConditionEvaluator.time_minutes(_args);
			return;
		}
		if (_name.EqualsCaseInsensitive("game_loaded"))
		{
			XmlPatchConditionEvaluator.game_loaded(_args);
			return;
		}
		if (_name.EqualsCaseInsensitive("xpath"))
		{
			XmlPatchConditionEvaluator.xpath(_args);
			return;
		}
		if (_name.EqualsCaseInsensitive("has_entitlement"))
		{
			XmlPatchConditionEvaluator.has_entitlement(_args);
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void mod_loaded(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "mod_loaded", _args.Parameters.Length, 1));
		}
		string text = _args.Parameters[0].Evaluate() as string;
		if (text == null)
		{
			throw new ArgumentException("Calling function mod_loaded: Expected string argument");
		}
		_args.Result = ModManager.ModLoaded(text.Trim());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void mod_version(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "mod_version", _args.Parameters.Length, 1));
		}
		string text = _args.Parameters[0].Evaluate() as string;
		if (text == null)
		{
			throw new ArgumentException("Calling function mod_version: Expected string argument");
		}
		Mod mod = ModManager.GetMod(text.Trim(), true);
		_args.Result = ((mod != null) ? mod.Version : null) ?? new Version(0, 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void game_version(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 0)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "game_version", _args.Parameters.Length, 0));
		}
		_args.Result = Constants.cVersionInformation.Version;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void version(FunctionArgs _args)
	{
		if (_args.Parameters.Length < 2)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected at least {2})", "version", _args.Parameters.Length, 2));
		}
		object obj = _args.Parameters[0].Evaluate();
		if (!(obj is int))
		{
			throw new ArgumentException("Calling function version: Expected int arguments");
		}
		int num = (int)obj;
		obj = _args.Parameters[1].Evaluate();
		if (obj is int)
		{
			int num2 = (int)obj;
			int num3 = 0;
			if (_args.Parameters.Length >= 3)
			{
				obj = _args.Parameters[2].Evaluate();
				if (!(obj is int))
				{
					throw new ArgumentException("Calling function version: Expected int arguments");
				}
				int num4 = (int)obj;
				num3 = num4;
			}
			int num5 = 0;
			if (_args.Parameters.Length >= 4)
			{
				obj = _args.Parameters[3].Evaluate();
				if (!(obj is int))
				{
					throw new ArgumentException("Calling function version: Expected int arguments");
				}
				int num6 = (int)obj;
				num5 = num6;
			}
			int num7 = _args.Parameters.Length;
			Version version;
			if (num7 != 2)
			{
				if (num7 != 3)
				{
					version = new Version(num, num2, num3, num5);
				}
				else
				{
					version = new Version(num, num2, num3);
				}
			}
			else
			{
				version = new Version(num, num2);
			}
			_args.Result = version;
			return;
		}
		throw new ArgumentException("Calling function version: Expected int arguments");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void gamepref(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "gamepref", _args.Parameters.Length, 1));
		}
		string text = _args.Parameters[0].Evaluate() as string;
		if (text == null)
		{
			throw new ArgumentException("Calling function gamepref: Expected string argument");
		}
		EnumGamePrefs enumGamePrefs;
		if (!EnumUtils.TryParse<EnumGamePrefs>(text, out enumGamePrefs, true))
		{
			throw new ArgumentException("Calling function gamepref: Unknown GamePref name '" + text + "'");
		}
		_args.Result = GamePrefs.GetObject(enumGamePrefs);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void eventActive(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "eventActive", _args.Parameters.Length, 1));
		}
		string text = _args.Parameters[0].Evaluate() as string;
		if (text == null)
		{
			throw new ArgumentException("Calling function eventActive: Expected string argument");
		}
		EventsFromXml.EventDefinition eventDefinition;
		if (!EventsFromXml.Events.TryGetValue(text, out eventDefinition))
		{
			throw new ArgumentException("Calling function eventActive: Unknown event name '" + text + "'");
		}
		_args.Result = eventDefinition.Active && !GamePrefs.GetBool(EnumGamePrefs.OptionsDisableXmlEvents);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void serverinfo(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "serverinfo", _args.Parameters.Length, 1));
		}
		string text = _args.Parameters[0].Evaluate() as string;
		if (text == null)
		{
			throw new ArgumentException("Calling function serverinfo: Expected string argument");
		}
		GameServerInfo gameServerInfo = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
		if (gameServerInfo == null)
		{
			throw new Exception("'serverinfo' conditional function can only be executed with a game loaded!");
		}
		GameInfoBool gameInfoBool;
		if (EnumUtils.TryParse<GameInfoBool>(text, out gameInfoBool, true))
		{
			_args.Result = gameServerInfo.GetValue(gameInfoBool);
			return;
		}
		GameInfoInt gameInfoInt;
		if (EnumUtils.TryParse<GameInfoInt>(text, out gameInfoInt, true))
		{
			_args.Result = gameServerInfo.GetValue(gameInfoInt);
			return;
		}
		GameInfoString gameInfoString;
		if (EnumUtils.TryParse<GameInfoString>(text, out gameInfoString, true))
		{
			_args.Result = gameServerInfo.GetValue(gameInfoString);
			return;
		}
		throw new ArgumentException("Calling function serverinfo: Unknown ServerInfo name '" + text + "'");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void time_minutes(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 0)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "time_minutes", _args.Parameters.Length, 0));
		}
		_args.Result = DateTime.Now.Minute;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void game_loaded(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 0)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "game_loaded", _args.Parameters.Length, 0));
		}
		_args.Result = SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentMode > ProtocolManager.NetworkType.None;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void xpath(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "xpath", _args.Parameters.Length, 1));
		}
		string text = _args.Parameters[0].Evaluate() as string;
		if (text == null)
		{
			throw new ArgumentException("Calling function xpath: Expected string argument");
		}
		object obj;
		if (_args.Parameters[0].Parameters.TryGetValue("xml", out obj))
		{
			XmlFile xmlFile = obj as XmlFile;
			if (xmlFile != null)
			{
				Log.Out("Xpath conditional on file: " + xmlFile.Filename);
				List<XObject> list = new List<XObject>();
				if (!xmlFile.GetXpathResultsInList(text, list))
				{
					_args.Result = null;
					return;
				}
				Log.Out(string.Format("Xpath matches: {0}", list.Count));
				if (list.Count > 1)
				{
					_args.Result = "More than one match";
					return;
				}
				_args.Result = list[0].ToString();
				Log.Out(string.Format("Match: {0}", _args.Result));
				return;
			}
		}
		throw new ArgumentException("Calling function xpath: XML file reference not found");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void has_entitlement(FunctionArgs _args)
	{
		if (_args.Parameters.Length != 1)
		{
			throw new ArgumentException(string.Format("Calling function {0} with invalid number of arguments ({1}, expected {2})", "has_entitlement", _args.Parameters.Length, 1));
		}
		string text = _args.Parameters[0].Evaluate() as string;
		if (text == null)
		{
			throw new ArgumentException("Calling function has_entitlement: Expected string argument");
		}
		EntitlementSetEnum entitlementSetEnum;
		if (!EnumUtils.TryParse<EntitlementSetEnum>(text, out entitlementSetEnum, true))
		{
			throw new ArgumentException("Calling function has_entitlement: Unknown EntitlementSetEnum name '" + text + "'");
		}
		_args.Result = EntitlementManager.Instance.HasEntitlement(entitlementSetEnum);
	}
}
