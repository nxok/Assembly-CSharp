using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using NCalc;
using UnityEngine;
using UnityEngine.Scripting;

[XuiBindingNcalcFunction]
[Preserve]
public static class BindingNcalcFunctions
{
	[XuiBindingNcalcFunction("<ERROR>", 1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void localization(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		_args.Result = Localization.Get(_evaluatedArguments[0].ToString(), false, null);
	}

	[XuiBindingNcalcFunction(1f, 1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void cvar(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		string text = _evaluatedArguments[0].ToString();
		if (!_bindingInstance.RegisterVariable(new BindingInfoNcalc.VariableStateCVar(text)))
		{
			_bindingInstance.SetIndeterministic();
		}
		if (GameManager.Instance == null || GameManager.Instance.World == null)
		{
			return;
		}
		EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			return;
		}
		_args.Result = primaryPlayer.GetCVar(text);
	}

	[XuiBindingNcalcFunction(0, 1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void gamepref(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		string text = _evaluatedArguments[0].ToString();
		if (string.IsNullOrEmpty(text))
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument 0 '{0}' is not a non-empty string.", _evaluatedArguments[0]), null, "gamepref");
			return;
		}
		EnumGamePrefs enumGamePrefs;
		if (!EnumUtils.TryParseIgnoreCase<EnumGamePrefs>(text, out enumGamePrefs))
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument 0 '{0}' is not a valid GamePref.", _evaluatedArguments[0]), null, "gamepref");
			return;
		}
		if (!_bindingInstance.RegisterVariable(new BindingInfoNcalc.VariableStateGamePref(enumGamePrefs)))
		{
			_bindingInstance.SetIndeterministic();
		}
		_args.Result = GamePrefs.GetObject(enumGamePrefs);
	}

	[XuiBindingNcalcFunction(0, 1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void gamestat(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		string text = _evaluatedArguments[0].ToString();
		if (string.IsNullOrEmpty(text))
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument 0 '{0}' is not a non-empty string.", _evaluatedArguments[0]), null, "gamestat");
			return;
		}
		EnumGameStats enumGameStats;
		if (!EnumUtils.TryParseIgnoreCase<EnumGameStats>(text, out enumGameStats))
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument 0 '{0}' is not a valid GameStat.", _evaluatedArguments[0]), null, "gamestat");
			return;
		}
		if (!_bindingInstance.RegisterVariable(new BindingInfoNcalc.VariableStateGameStat(enumGameStats)))
		{
			_bindingInstance.SetIndeterministic();
		}
		_args.Result = GameStats.GetObject(enumGameStats);
	}

	[XuiBindingNcalcFunction(0, 1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void serverinfoint(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		string text = _evaluatedArguments[0].ToString();
		if (string.IsNullOrEmpty(text))
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument 0 '{0}' is not a non-empty string.", _evaluatedArguments[0]), null, "serverinfoint");
			return;
		}
		GameInfoInt gameInfoInt;
		if (!EnumUtils.TryParseIgnoreCase<GameInfoInt>(text, out gameInfoInt))
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument 0 '{0}' is not a valid GameInfoInt.", _evaluatedArguments[0]), null, "serverinfoint");
			return;
		}
		if (!_bindingInstance.RegisterVariable(new BindingInfoNcalc.VariableStateGameInfoInt(gameInfoInt)))
		{
			_bindingInstance.SetIndeterministic();
		}
		GameServerInfo gameServerInfo = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
		if (gameServerInfo == null)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, "No game loaded.", null, "serverinfoint");
			return;
		}
		_args.Result = gameServerInfo.GetValue(gameInfoInt);
	}

	[XuiBindingNcalcFunction(0, 1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void serverinfobool(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		string text = _evaluatedArguments[0].ToString();
		if (string.IsNullOrEmpty(text))
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument 0 '{0}' is not a non-empty string.", _evaluatedArguments[0]), null, "serverinfobool");
			return;
		}
		GameInfoBool gameInfoBool;
		if (!EnumUtils.TryParseIgnoreCase<GameInfoBool>(text, out gameInfoBool))
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument 0 '{0}' is not a valid GameInfoBool.", _evaluatedArguments[0]), null, "serverinfobool");
			return;
		}
		if (!_bindingInstance.RegisterVariable(new BindingInfoNcalc.VariableStateGameInfoBool(gameInfoBool)))
		{
			_bindingInstance.SetIndeterministic();
		}
		GameServerInfo gameServerInfo = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
		if (gameServerInfo == null)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, "No game loaded.", null, "serverinfobool");
			return;
		}
		_args.Result = gameServerInfo.GetValue(gameInfoBool);
	}

	[XuiBindingNcalcFunction(0, 1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void serverinfostring(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		string text = _evaluatedArguments[0].ToString();
		if (string.IsNullOrEmpty(text))
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument 0 '{0}' is not a non-empty string.", _evaluatedArguments[0]), null, "serverinfostring");
			return;
		}
		GameInfoString gameInfoString;
		if (!EnumUtils.TryParseIgnoreCase<GameInfoString>(text, out gameInfoString))
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument 0 '{0}' is not a valid GameInfoString.", _evaluatedArguments[0]), null, "serverinfostring");
			return;
		}
		if (!_bindingInstance.RegisterVariable(new BindingInfoNcalc.VariableStateGameInfoString(gameInfoString)))
		{
			_bindingInstance.SetIndeterministic();
		}
		GameServerInfo gameServerInfo = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
		if (gameServerInfo == null)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, "No game loaded.", null, "serverinfostring");
			return;
		}
		_args.Result = gameServerInfo.GetValue(gameInfoString);
	}

	[XuiBindingNcalcFunction(null, 2, "dictvalue")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void dictValue(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		_bindingInstance.SetIndeterministic();
		IDictionary dictionary = _evaluatedArguments[0] as IDictionary;
		if (dictionary == null)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument 0 '{0}' does not evaluate to a dictionary.", _evaluatedArguments[0]), null, "dictValue");
			return;
		}
		object obj = _evaluatedArguments[1];
		if (obj == null)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, "Argument 1 is null.", null, "dictValue");
			return;
		}
		if (!dictionary.Contains(obj))
		{
			_args.Result = "";
			return;
		}
		_args.Result = dictionary[obj];
	}

	[XuiBindingNcalcFunction("", -1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void format(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		if (_args.Parameters.Length < 1)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Invalid number of arguments ({0}, expected at least 1).", _args.Parameters.Length), null, "format");
			return;
		}
		object obj = _args.Parameters[0].Evaluate();
		if (obj == null)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, "Can not evaluate argument.", null, "format");
			return;
		}
		object[] objectArray = BindingNcalcFunctions.GetObjectArray(_args.Parameters.Length - 1);
		for (int i = 1; i < _args.Parameters.Length; i++)
		{
			objectArray[i - 1] = _args.Parameters[i].Evaluate();
		}
		string text = string.Format(obj.ToString(), objectArray);
		BindingNcalcFunctions.ReturnObjectArray(objectArray);
		_args.Result = text;
	}

	[XuiBindingNcalcFunction(0, 1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void length(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		_args.Result = _evaluatedArguments[0].ToString().Length;
	}

	[XuiBindingNcalcFunction(0, 1, "int")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void toInt(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = Convert.ToInt32(_evaluatedArguments[0]);
		}
		catch (Exception ex)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument '{0}' does not evaluate to an integer.", _evaluatedArguments[0]), ex, "toInt");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "roundtoint")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void roundToInt(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		float num;
		if (!BindingNcalcFunctions.tryArgAsFloat(_bindingInstance, _evaluatedArguments, 0, "v", out num, "roundToInt"))
		{
			return;
		}
		_args.Result = Mathf.RoundToInt(num);
	}

	[XuiBindingNcalcFunction(0.0, 1, "float")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void toFloat(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = Convert.ToDouble(_evaluatedArguments[0]);
		}
		catch (Exception ex)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument '{0}' does not evaluate to a number.", _evaluatedArguments[0]), ex, "toFloat");
		}
	}

	[XuiBindingNcalcFunction("", 1, "str")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void toString(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		object obj = _evaluatedArguments[0];
		_args.Result = ((obj != null) ? obj.ToString() : null) ?? "";
	}

	[XuiBindingNcalcFunction("", 2, "itos")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void intToString(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		int num;
		if (!BindingNcalcFunctions.tryArgAsInt(_bindingInstance, _evaluatedArguments, 0, "i", out num, "intToString"))
		{
			return;
		}
		_args.Result = num.ToString(_evaluatedArguments[1].ToString());
	}

	[XuiBindingNcalcFunction("", 2, "ftos")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void floatToString(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		float num;
		if (!BindingNcalcFunctions.tryArgAsFloat(_bindingInstance, _evaluatedArguments, 0, "f", out num, "floatToString"))
		{
			return;
		}
		_args.Result = num.ToString(_evaluatedArguments[1].ToString());
	}

	[XuiBindingNcalcFunction(false, 1, "bound")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void isBound(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		throw new NotImplementedException();
	}

	[XuiBindingNcalcFunction("", 1, null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void always(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		_bindingInstance.SetIndeterministic();
		_args.Result = _evaluatedArguments[0];
	}

	[XuiBindingNcalcFunction(false, 1, "defined")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void isDefined(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		object obj;
		_args.Result = (_bindingInstance.FindParameter(_evaluatedArguments[0].ToString(), out obj) ? BindingNcalcFunctions.boxedTrue : BindingNcalcFunctions.boxedFalse);
	}

	[XuiBindingNcalcFunction(0, 2, "c_i")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void color32_i(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		int num;
		if (!BindingNcalcFunctions.tryArgAsInt(_bindingInstance, _evaluatedArguments, 1, "i", out num, "color32_i"))
		{
			return;
		}
		if (num < 0 || num > 3)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument '{0}' outside allowed range (0-3).", _evaluatedArguments[1]), null, "color32_i");
		}
		try
		{
			_args.Result = ((Color32)_evaluatedArguments[0])[num];
		}
		catch (Exception ex)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument '{0}' does not evaluate to a Color.", _evaluatedArguments[0]), ex, "color32_i");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "c_r")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void color32_r(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = ((Color32)_evaluatedArguments[0]).r;
		}
		catch (Exception ex)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument '{0}' does not evaluate to a Color.", _evaluatedArguments[0]), ex, "color32_r");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "c_g")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void color32_g(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = ((Color32)_evaluatedArguments[0]).g;
		}
		catch (Exception ex)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument '{0}' does not evaluate to a Color.", _evaluatedArguments[0]), ex, "color32_g");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "c_b")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void color32_b(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = ((Color32)_evaluatedArguments[0]).b;
		}
		catch (Exception ex)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument '{0}' does not evaluate to a Color.", _evaluatedArguments[0]), ex, "color32_b");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "c_a")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void color32_a(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = ((Color32)_evaluatedArguments[0]).a;
		}
		catch (Exception ex)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument '{0}' does not evaluate to a Color.", _evaluatedArguments[0]), ex, "color32_a");
		}
	}

	[XuiBindingNcalcFunction(null, 4, "color")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void color32(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		byte b;
		if (!BindingNcalcFunctions.tryArgAsByteClamped(_bindingInstance, _evaluatedArguments, 0, "r", out b, "color32"))
		{
			return;
		}
		byte b2;
		if (!BindingNcalcFunctions.tryArgAsByteClamped(_bindingInstance, _evaluatedArguments, 1, "g", out b2, "color32"))
		{
			return;
		}
		byte b3;
		if (!BindingNcalcFunctions.tryArgAsByteClamped(_bindingInstance, _evaluatedArguments, 2, "b", out b3, "color32"))
		{
			return;
		}
		byte b4;
		if (!BindingNcalcFunctions.tryArgAsByteClamped(_bindingInstance, _evaluatedArguments, 3, "a", out b4, "color32"))
		{
			return;
		}
		_args.Result = new Color32(b, b2, b3, b4);
	}

	[XuiBindingNcalcFunction(0, 2, "v2i_i")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void v2i_i(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		int num;
		if (!BindingNcalcFunctions.tryArgAsInt(_bindingInstance, _evaluatedArguments, 1, "i", out num, "v2i_i"))
		{
			return;
		}
		if (num < 0 || num > 1)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument '{0}' outside allowed range (0-1).", _evaluatedArguments[1]), null, "v2i_i");
		}
		try
		{
			_args.Result = ((Vector2i)_evaluatedArguments[0])[num];
		}
		catch (Exception ex)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument '{0}' does not evaluate to a Vector2i.", _evaluatedArguments[0]), ex, "v2i_i");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "v2i_x")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void v2i_x(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = ((Vector2i)_evaluatedArguments[0]).x;
		}
		catch (Exception ex)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument '{0}' does not evaluate to a Vector2i.", _evaluatedArguments[0]), ex, "v2i_x");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "v2i_y")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void v2i_y(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = ((Vector2i)_evaluatedArguments[0]).y;
		}
		catch (Exception ex)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument '{0}' does not evaluate to a Vector2i.", _evaluatedArguments[0]), ex, "v2i_y");
		}
	}

	[XuiBindingNcalcFunction(null, 2, "v2i")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void vector2i(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		int num;
		if (!BindingNcalcFunctions.tryArgAsInt(_bindingInstance, _evaluatedArguments, 0, "x", out num, "vector2i"))
		{
			return;
		}
		int num2;
		if (!BindingNcalcFunctions.tryArgAsInt(_bindingInstance, _evaluatedArguments, 1, "y", out num2, "vector2i"))
		{
			return;
		}
		_args.Result = new Vector2i(num, num2);
	}

	[XuiBindingNcalcFunction(0, 2, "v3i_i")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void v3i_i(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		int num;
		if (!BindingNcalcFunctions.tryArgAsInt(_bindingInstance, _evaluatedArguments, 1, "i", out num, "v3i_i"))
		{
			return;
		}
		if (num < 0 || num > 2)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument '{0}' outside allowed range (0-2).", _evaluatedArguments[1]), null, "v3i_i");
		}
		try
		{
			_args.Result = ((Vector2i)_evaluatedArguments[0])[num];
		}
		catch (Exception ex)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument '{0}' does not evaluate to a Vector3i.", _evaluatedArguments[0]), ex, "v3i_i");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "v3i_x")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void v3i_x(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = ((Vector3i)_evaluatedArguments[0]).x;
		}
		catch (Exception ex)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument '{0}' does not evaluate to a Vector3i.", _evaluatedArguments[0]), ex, "v3i_x");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "v3i_y")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void v3i_y(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = ((Vector3i)_evaluatedArguments[0]).y;
		}
		catch (Exception ex)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument '{0}' does not evaluate to a Vector3i.", _evaluatedArguments[0]), ex, "v3i_y");
		}
	}

	[XuiBindingNcalcFunction(0, 1, "v3i_z")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void v3i_z(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		try
		{
			_args.Result = ((Vector3i)_evaluatedArguments[0]).z;
		}
		catch (Exception ex)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument '{0}' does not evaluate to a Vector3i.", _evaluatedArguments[0]), ex, "v3i_z");
		}
	}

	[XuiBindingNcalcFunction(null, 3, "v3i")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void vector3i(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		int num;
		if (!BindingNcalcFunctions.tryArgAsInt(_bindingInstance, _evaluatedArguments, 0, "x", out num, "vector3i"))
		{
			return;
		}
		int num2;
		if (!BindingNcalcFunctions.tryArgAsInt(_bindingInstance, _evaluatedArguments, 1, "y", out num2, "vector3i"))
		{
			return;
		}
		int num3;
		if (!BindingNcalcFunctions.tryArgAsInt(_bindingInstance, _evaluatedArguments, 2, "z", out num3, "vector3i"))
		{
			return;
		}
		_args.Result = new Vector3i(num, num2, num3);
	}

	[XuiBindingNcalcFunction(null, 2, "v2")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void vector2(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		float num;
		if (!BindingNcalcFunctions.tryArgAsFloat(_bindingInstance, _evaluatedArguments, 0, "x", out num, "vector2"))
		{
			return;
		}
		float num2;
		if (!BindingNcalcFunctions.tryArgAsFloat(_bindingInstance, _evaluatedArguments, 1, "y", out num2, "vector2"))
		{
			return;
		}
		_args.Result = new Vector2(num, num2);
	}

	[XuiBindingNcalcFunction(null, 3, "v3")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void vector3(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments)
	{
		float num;
		if (!BindingNcalcFunctions.tryArgAsFloat(_bindingInstance, _evaluatedArguments, 0, "x", out num, "vector3"))
		{
			return;
		}
		float num2;
		if (!BindingNcalcFunctions.tryArgAsFloat(_bindingInstance, _evaluatedArguments, 1, "y", out num2, "vector3"))
		{
			return;
		}
		float num3;
		if (!BindingNcalcFunctions.tryArgAsFloat(_bindingInstance, _evaluatedArguments, 2, "z", out num3, "vector3"))
		{
			return;
		}
		_args.Result = new Vector3(num, num2, num3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryArgAsByteClamped(IBindingNcalc _bindingInstance, object[] _evaluatedArguments, int _argIndex, string _argName, out byte _value, [CallerMemberName] string _funcName = null)
	{
		object obj = _evaluatedArguments[_argIndex];
		bool flag;
		try
		{
			_value = (byte)Mathf.Clamp((int)obj, 0, 255);
			flag = true;
		}
		catch (Exception ex)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument {0}='{1}' (type {2}) does not evaluate to an integer.", _argName, obj, obj.GetType().FullName), ex, _funcName);
			_value = 0;
			flag = false;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryArgAsInt(IBindingNcalc _bindingInstance, object[] _evaluatedArguments, int _argIndex, string _argName, out int _value, [CallerMemberName] string _funcName = null)
	{
		object obj = _evaluatedArguments[_argIndex];
		bool flag;
		try
		{
			_value = Convert.ToInt32(obj);
			flag = true;
		}
		catch (Exception ex)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument {0}='{1}' (type {2}) does not evaluate to an integer.", _argName, obj, obj.GetType().FullName), ex, _funcName);
			_value = 0;
			flag = false;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool tryArgAsFloat(IBindingNcalc _bindingInstance, object[] _evaluatedArguments, int _argIndex, string _argName, out float _value, [CallerMemberName] string _funcName = null)
	{
		object obj = _evaluatedArguments[_argIndex];
		bool flag;
		try
		{
			_value = Convert.ToSingle(obj);
			flag = true;
		}
		catch (Exception ex)
		{
			BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Argument {0}='{1}' (type {2}) does not evaluate to a number.", _argName, obj, obj.GetType().FullName), ex, _funcName);
			_value = 0f;
			flag = false;
		}
		return flag;
	}

	public static void LogFunctionError(IBindingNcalc _bindingInstance, string _message, Exception _e = null, [CallerMemberName] string _funcName = null)
	{
		Log.Error(string.Concat(new string[]
		{
			"[XUi] Binding expression calling function '",
			_funcName,
			"': ",
			_message,
			" Binding expression: '",
			_bindingInstance.SourceText,
			"' --- hierarchy: ",
			_bindingInstance.TargetElement.GetXuiHierarchy()
		}));
		if (_e != null)
		{
			Log.Exception(_e);
		}
	}

	public static void EvaluateFunc(IBindingNcalc _bindingInstance, string _name, FunctionArgs _args)
	{
		BindingNcalcFunctions.FunctionDefinition functionDefinition;
		if (!BindingNcalcFunctions.ncalcFunctions.TryGetValue(_name, out functionDefinition))
		{
			return;
		}
		object[] array = null;
		_args.Result = null;
		if (functionDefinition.ExpectedArgumentCount >= 0)
		{
			if (_args.Parameters.Length != functionDefinition.ExpectedArgumentCount)
			{
				BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Invalid number of arguments ({0}, expected {1}).", _args.Parameters.Length, functionDefinition.ExpectedArgumentCount), null, _name);
				if (functionDefinition.ErrorResult != null)
				{
					_args.Result = functionDefinition.ErrorResult;
				}
				return;
			}
			if (_args.Parameters.Length != 0)
			{
				array = BindingNcalcFunctions.GetObjectArray(_args.Parameters.Length);
				for (int i = 0; i < _args.Parameters.Length; i++)
				{
					object obj = _args.Parameters[i].Evaluate();
					if (obj == null)
					{
						BindingNcalcFunctions.LogFunctionError(_bindingInstance, string.Format("Can not evaluate argument at index {0}.", i), null, _name);
						if (functionDefinition.ErrorResult != null)
						{
							_args.Result = functionDefinition.ErrorResult;
						}
						return;
					}
					array[i] = obj;
				}
			}
		}
		functionDefinition.Delegate(_bindingInstance, _args, array);
		if (_args.Result == null && functionDefinition.ErrorResult != null)
		{
			_args.Result = functionDefinition.ErrorResult;
		}
		BindingNcalcFunctions.ReturnObjectArray(array);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static object[] GetObjectArray(int _length)
	{
		if (_length > 20)
		{
			return new object[_length];
		}
		if (BindingNcalcFunctions.objectArrays.Count == 0)
		{
			return new object[_length];
		}
		return BindingNcalcFunctions.objectArrays.Pop();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ReturnObjectArray(object[] _arr)
	{
		if (_arr != null && _arr.Length == 20)
		{
			BindingNcalcFunctions.objectArrays.Push(_arr);
		}
	}

	public static void RegisterNcalcFunctions()
	{
		if (BindingNcalcFunctions.ncalcFunctions.Count > 0)
		{
			return;
		}
		for (int i = 0; i < 10; i++)
		{
			BindingNcalcFunctions.objectArrays.Push(new object[20]);
		}
		ReflectionHelpers.FindTypesWithAttribute<XuiBindingNcalcFunctionAttribute>(new Action<Type, bool, XuiBindingNcalcFunctionAttribute>(BindingNcalcFunctions.<RegisterNcalcFunctions>g__TypeFoundCallback|50_0), true);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <RegisterNcalcFunctions>g__TypeFoundCallback|50_0(Type _type, bool _hasMultiple, XuiBindingNcalcFunctionAttribute _classAttribute)
	{
		ReflectionHelpers.GetMethodsWithAttribute<XuiBindingNcalcFunctionAttribute>(_type, new Action<MethodInfo, bool, XuiBindingNcalcFunctionAttribute>(BindingNcalcFunctions.<RegisterNcalcFunctions>g__MethodFoundCallback|50_1), true, true, true);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <RegisterNcalcFunctions>g__MethodFoundCallback|50_1(MethodInfo _method, bool _hasMultiple, XuiBindingNcalcFunctionAttribute _methodAttribute)
	{
		if (!ReflectionHelpers.MethodCompatibleWithDelegate<BindingNcalcFunctions.CustomNcalcFunctionDelegate>(_method, true))
		{
			Log.Error(string.Concat(new string[]
			{
				"[XUi] Binding NCalc function method ",
				_method.DeclaringType.FullName,
				".",
				_method.Name,
				" not compatible with delegate signature"
			}));
			return;
		}
		string text = _methodAttribute.FunctionName ?? _method.Name.ToLowerInvariant();
		BindingNcalcFunctions.CustomNcalcFunctionDelegate customNcalcFunctionDelegate = (BindingNcalcFunctions.CustomNcalcFunctionDelegate)_method.CreateDelegate(typeof(BindingNcalcFunctions.CustomNcalcFunctionDelegate));
		if (BindingNcalcFunctions.ncalcFunctions.ContainsKey(text))
		{
			Log.Warning(string.Concat(new string[]
			{
				"[XUi] Binding NCalc function method ",
				_method.DeclaringType.FullName,
				".",
				_method.Name,
				" overriding previously defined method for NCalc function '",
				text,
				"'"
			}));
		}
		BindingNcalcFunctions.ncalcFunctions[text] = new BindingNcalcFunctions.FunctionDefinition(customNcalcFunctionDelegate, _methodAttribute.ExpectedArgumentCount, _methodAttribute.ErrorResult);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object boxedFalse = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object boxedTrue = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ObjectArrayLengthCached = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Stack<object[]> objectArrays = new Stack<object[]>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, BindingNcalcFunctions.FunctionDefinition> ncalcFunctions = new Dictionary<string, BindingNcalcFunctions.FunctionDefinition>();

	public delegate void CustomNcalcFunctionDelegate(IBindingNcalc _bindingInstance, FunctionArgs _args, object[] _evaluatedArguments);

	[PublicizedFrom(EAccessModifier.Private)]
	public class FunctionDefinition
	{
		public FunctionDefinition(BindingNcalcFunctions.CustomNcalcFunctionDelegate _delegate, int _expectedArgumentCount, object _errorResult)
		{
			this.Delegate = _delegate;
			this.ExpectedArgumentCount = _expectedArgumentCount;
			this.ErrorResult = _errorResult;
		}

		public readonly BindingNcalcFunctions.CustomNcalcFunctionDelegate Delegate;

		public readonly int ExpectedArgumentCount;

		public readonly object ErrorResult;
	}
}
