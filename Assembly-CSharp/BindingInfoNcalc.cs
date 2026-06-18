using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using NCalc;
using Unity.Profiling;

public class BindingInfoNcalc : IBindingInstance, IBindingNcalc
{
	public static bool IsFullNcalcBinding(string _input)
	{
		if (!BindingInfoNcalc.fullBinding.IsMatch(_input))
		{
			return false;
		}
		int num = 0;
		for (int i = 2; i < _input.Length - 1; i++)
		{
			if (_input[i] == '{')
			{
				num++;
			}
			else if (_input[i] == '}')
			{
				num--;
			}
			if (num < 0)
			{
				return false;
			}
		}
		return num <= 0;
	}

	public BindingInfoNcalc(IXUiElement _targetViewOrTween, string _attribute, string _sourceString)
	{
		this.targetViewOrTween = _targetViewOrTween;
		this.attributeName = _attribute;
		this.sourceString = _sourceString;
		this.pmRefreshValueCompleteSourceText = new ProfilerMarker(_sourceString);
		int num = 1;
		if (_sourceString[1] == '%' || _sourceString[1] == '#')
		{
			num++;
		}
		string text = _sourceString.Substring(num, _sourceString.Length - num - 1);
		Match match = BindingInfoNcalc.singleIdentifierExpression.Match(text);
		if (match.Success)
		{
			string value = match.Groups[1].Value;
			this.singleIdentifierBinding = true;
			if (!this.FindParameter(value, out this.currentValue))
			{
				if (XUiFromXml.DebugXuiLoading != XUiFromXml.DebugLevel.Off)
				{
					Log.Warning("[XUi] Binding name '" + value + "' not found! Hierarchy: " + this.xuiHierarchy());
				}
				return;
			}
		}
		else
		{
			this.expression = new Expression(text, 2564)
			{
				Parameters = this.expressionParamDict
			};
			this.expression.EvaluateFunction += new EvaluateFunctionHandler(this.nCalcEvaluateFunction);
			this.expression.EvaluateParameter += new EvaluateParameterHandler(this.nCalcEvaluateParameter);
			this.IsInitializing = true;
			this.currentValue = this.evaluateExpression();
			this.IsInitializing = false;
		}
		if (this.currentValue == null)
		{
			this.expression = null;
			Log.Error(string.Concat(new string[]
			{
				"[XUi] Failed evaluating binding expression, result is null. Binding ",
				this.attributeName,
				"=\"",
				this.sourceString,
				"\", view hierarchy: ",
				this.xuiHierarchy()
			}));
			return;
		}
		this.<.ctor>g__BindToController|16_0();
		Type type = this.currentValue.GetType();
		ParsingMethodCache.ParsingMethodData parsingMethodData;
		if (ParsingMethodCache.Instance.TryGetParsingDelegate(this.targetViewOrTween, this.attributeName, out parsingMethodData))
		{
			if (parsingMethodData.TryGetDelegateForSourceType(type, out this.targetParsingDelegate))
			{
				this.targetParsingElement = this.targetViewOrTween;
				return;
			}
			Log.Error(string.Concat(new string[]
			{
				"[XUi] Failed mapping binding to parser on view, no conversion possible. Binding ",
				this.attributeName,
				"=\"",
				this.sourceString,
				"\", binding result type ",
				type.FullName,
				", parser native type ",
				parsingMethodData.NativeParseType.FullName,
				", view hierarchy: ",
				this.xuiHierarchy()
			}));
			this.expression = null;
			return;
		}
		else
		{
			if (this.targetViewOrTween is XUiTweenAbs)
			{
				Log.Error("[XUi] Failed mapping binding to parser on XUiTween, no parse found for attribute name '" + this.attributeName + "', view hierarchy: " + this.xuiHierarchy());
				this.expression = null;
				return;
			}
			ParsingMethodCache.ParsingMethodData parsingMethodData2;
			if (!ParsingMethodCache.Instance.TryGetParsingDelegate(this.targetViewOrTween.Controller, this.attributeName, out parsingMethodData2))
			{
				this.targetParsingElement = this.targetViewOrTween.Controller;
				return;
			}
			if (parsingMethodData2.TryGetDelegateForSourceType(type, out this.targetParsingDelegate))
			{
				this.targetParsingElement = this.targetViewOrTween.Controller;
				return;
			}
			Log.Error(string.Concat(new string[]
			{
				"[XUi] Failed mapping binding to parser on controller, no conversion possible. Binding ",
				this.attributeName,
				"=\"",
				this.sourceString,
				"\", binding result type ",
				type.FullName,
				", parser native type ",
				parsingMethodData2.NativeParseType.FullName,
				", view hierarchy: ",
				this.xuiHierarchy()
			}));
			this.expression = null;
			return;
		}
	}

	public void RefreshValue()
	{
		if ((this.singleIdentifierBinding && this.variables.Count == 0) || (this.expression == null && !this.singleIdentifierBinding))
		{
			return;
		}
		using (this.pmRefreshValueComplete.Auto())
		{
			using (this.pmRefreshValueCompleteSourceText.Auto())
			{
				bool flag = this.usesIndeterministicFunctions;
				using (this.pmRefreshBindings.Auto())
				{
					for (int i = 0; i < this.variables.Count; i++)
					{
						flag |= this.variables[i].RefreshValue();
					}
				}
				if (this.singleIdentifierBinding)
				{
					this.currentValue = this.variables[0].CurrentValue;
				}
				else
				{
					using (this.pmNcalcEval.Auto())
					{
						if (flag)
						{
							this.currentValue = this.evaluateExpression();
						}
					}
				}
				try
				{
					object obj = this.currentValue;
					string text = obj as string;
					if (text != null && text.Contains("{cvar("))
					{
						obj = BindingsManager.ReplaceCVars(text);
					}
					using (this.pmApplyAttribute.Auto())
					{
						if (this.targetParsingDelegate == null)
						{
							((XUiController)this.targetParsingElement).CustomAttributes[this.attributeName] = obj;
						}
						else
						{
							this.targetParsingDelegate(this.targetParsingElement, obj);
						}
					}
				}
				catch (Exception ex)
				{
					Log.Error(string.Format("[XUi] Exception parsing result of binding. Binding {0}=\"{1}\", binding result: '{2}' (type: {3}), view hierarchy: {4}:", new object[]
					{
						this.attributeName,
						this.sourceString,
						this.currentValue,
						this.currentValue.GetType().FullName,
						this.xuiHierarchy()
					}));
					Log.Exception(ex);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public object evaluateExpression()
	{
		object obj;
		try
		{
			obj = this.expression.Evaluate();
		}
		catch (Exception ex)
		{
			Log.Error(string.Concat(new string[]
			{
				"[XUi] Binding expression can not be evaluated. Binding ",
				this.attributeName,
				"=\"",
				this.sourceString,
				"\" --- hierarchy: ",
				this.xuiHierarchy()
			}));
			Log.Exception(ex);
			obj = null;
		}
		return obj;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void nCalcEvaluateParameter(string _name, ParameterArgs _args)
	{
		object obj;
		if (this.FindParameter(_name, out obj))
		{
			_args.Result = obj;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void nCalcEvaluateFunction(string _name, FunctionArgs _args, bool _ignoreCase)
	{
		BindingNcalcFunctions.EvaluateFunc(this, _name, _args);
	}

	public bool FindParameter(string _name, out object _value)
	{
		if (this.expressionParamDict.TryGetValue(_name, out _value))
		{
			return true;
		}
		for (XUiController xuiController = this.targetViewOrTween.Controller; xuiController != null; xuiController = xuiController.Parent)
		{
			if (xuiController.GetType() != typeof(XUiController))
			{
				object obj = null;
				BindingInfoNcalc.VariableStateAbs variableStateAbs = null;
				XuiBindingDelegate xuiBindingDelegate;
				if (BindingMethodCache.Instance.TryGetBindingDelegate(xuiController, _name, out xuiBindingDelegate))
				{
					obj = xuiBindingDelegate(xuiController);
					variableStateAbs = new BindingInfoNcalc.VariableStateParamBinding(_name, xuiController, obj, xuiBindingDelegate, this.expressionParamDict);
				}
				else
				{
					string text = "";
					if (xuiController.GetBindingValue(ref text, _name))
					{
						variableStateAbs = new BindingInfoNcalc.VariableStateLegacyBinding(_name, xuiController, text, this.expressionParamDict);
						obj = text;
					}
				}
				if (variableStateAbs != null)
				{
					xuiController.Bindings.AddBinding(this);
					this.boundToControllers++;
					this.variables.Add(variableStateAbs);
					_value = obj;
					return true;
				}
			}
		}
		if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
		{
			Log.Warning("[XUi] Binding name '" + _name + "' not found! Hierarchy: " + this.xuiHierarchy());
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string xuiHierarchy()
	{
		return this.targetViewOrTween.GetXuiHierarchy();
	}

	public string SourceText
	{
		get
		{
			return this.sourceString;
		}
	}

	public IXUiElement TargetElement
	{
		get
		{
			return this.targetViewOrTween;
		}
	}

	public bool IsInitializing { get; }

	public void SetIndeterministic()
	{
		this.usesIndeterministicFunctions = true;
	}

	public bool RegisterVariable(BindingInfoNcalc.VariableStateAbs _var)
	{
		this.variables.Add(_var);
		return true;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <.ctor>g__BindToController|16_0()
	{
		if (!this.usesIndeterministicFunctions && this.boundToControllers != 0)
		{
			return;
		}
		for (XUiController xuiController = this.targetViewOrTween.Controller; xuiController != null; xuiController = xuiController.Parent)
		{
			if (xuiController.GetType() != typeof(XUiController))
			{
				xuiController.Bindings.AddBinding(this);
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IXUiElement targetViewOrTween;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string attributeName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string sourceString;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IXUiElement targetParsingElement;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XuiParsingDelegate targetParsingDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<BindingInfoNcalc.VariableStateAbs> variables = new List<BindingInfoNcalc.VariableStateAbs>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, object> expressionParamDict = new Dictionary<string, object>(StringComparer.Ordinal);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Expression expression;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool singleIdentifierBinding;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool usesIndeterministicFunctions;

	[PublicizedFrom(EAccessModifier.Private)]
	public int boundToControllers;

	[PublicizedFrom(EAccessModifier.Private)]
	public object currentValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex fullBinding = new Regex("^\\{%.*\\}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex singleIdentifierExpression = new Regex("^\\s*([a-zA-Z]\\w*)\\s*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmCtor = new ProfilerMarker("BindingInfoNcalc.ctor");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmRefreshValueComplete = new ProfilerMarker("BindingInfoNcalc.RefreshValue");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmRefreshValueCompleteSourceText;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmRefreshBindings = new ProfilerMarker("Refresh Bindings");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmNcalcEval = new ProfilerMarker("NCalc.Evaluate");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmApplyAttribute = new ProfilerMarker("Apply Attribute");

	public abstract class VariableStateAbs
	{
		public abstract object CurrentValue { get; }

		public abstract bool RefreshValue();

		[PublicizedFrom(EAccessModifier.Protected)]
		public VariableStateAbs()
		{
		}
	}

	public abstract class VariableStateBinding : BindingInfoNcalc.VariableStateAbs
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public VariableStateBinding(string _name, XUiController _controller, Dictionary<string, object> _expressionParamDict)
		{
			this.BindingName = _name;
			this.Controller = _controller;
			this.ExpressionParamDict = _expressionParamDict;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly XUiController Controller;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly string BindingName;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly Dictionary<string, object> ExpressionParamDict;
	}

	public class VariableStateParamBinding : BindingInfoNcalc.VariableStateBinding
	{
		public VariableStateParamBinding(string _name, XUiController _controller, object _initialValue, XuiBindingDelegate _bindingDelegate, Dictionary<string, object> _expressionParamDict)
			: base(_name, _controller, _expressionParamDict)
		{
			this.currentValue = _initialValue;
			this.bindingDelegate = _bindingDelegate;
			this.ExpressionParamDict[this.BindingName] = _initialValue;
		}

		public override bool RefreshValue()
		{
			object obj = this.bindingDelegate(this.Controller);
			if (obj == null)
			{
				Log.Warning(string.Concat(new string[]
				{
					"[XUi] Binding '",
					this.BindingName,
					"' on controller '",
					this.Controller.GetType().FullName,
					"' returned null, should always return appropriate non-null value of same data type. Hierarchy: ",
					this.Controller.GetXuiHierarchy()
				}));
				return false;
			}
			if (obj.Equals(this.CurrentValue))
			{
				return false;
			}
			this.currentValue = obj;
			this.ExpressionParamDict[this.BindingName] = obj;
			return true;
		}

		public override string ToString()
		{
			return string.Format("ParamBinding: {0}='{1}'", this.BindingName, this.currentValue);
		}

		public override object CurrentValue
		{
			get
			{
				return this.currentValue;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XuiBindingDelegate bindingDelegate;

		[PublicizedFrom(EAccessModifier.Private)]
		public object currentValue;
	}

	public class VariableStateLegacyBinding : BindingInfoNcalc.VariableStateBinding
	{
		public VariableStateLegacyBinding(string _name, XUiController _controller, string _initialValue, Dictionary<string, object> _expressionParamDict)
			: base(_name, _controller, _expressionParamDict)
		{
			this.currentValue = _initialValue;
			this.ExpressionParamDict[this.BindingName] = _initialValue;
		}

		public override bool RefreshValue()
		{
			string text = "";
			if (!this.Controller.GetBindingValue(ref text, this.BindingName))
			{
				Log.Error(string.Concat(new string[]
				{
					"[XUi] Refreshing binding failed: Controller's GetBindingValue no longer returns true! (Binding: ",
					this.BindingName,
					", hierarchy: ",
					this.Controller.GetXuiHierarchy(),
					")"
				}));
				return false;
			}
			if (string.Equals(this.currentValue, text, StringComparison.Ordinal))
			{
				return false;
			}
			this.currentValue = text;
			this.ExpressionParamDict[this.BindingName] = text;
			return true;
		}

		public override string ToString()
		{
			return string.Concat(new string[] { "LegacyBinding: ", this.BindingName, "='", this.currentValue, "'" });
		}

		public override object CurrentValue
		{
			get
			{
				return this.currentValue;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string currentValue;
	}

	public class VariableStateCVar : BindingInfoNcalc.VariableStateAbs
	{
		public VariableStateCVar(string _name)
		{
			this.cVarName = _name;
			this.currentValue = 0f;
		}

		public override bool RefreshValue()
		{
			GameManager instance = GameManager.Instance;
			if (instance == null)
			{
				return false;
			}
			World world = instance.World;
			EntityPlayer entityPlayer = ((world != null) ? world.GetPrimaryPlayer() : null);
			if (entityPlayer == null)
			{
				return false;
			}
			float cvar = entityPlayer.GetCVar(this.cVarName);
			if (cvar.Equals(this.currentValue))
			{
				return false;
			}
			this.currentValue = cvar;
			return true;
		}

		public override string ToString()
		{
			return string.Format("cVar: {0}={1}", this.cVarName, this.currentValue);
		}

		public override object CurrentValue
		{
			get
			{
				return this.currentValue;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string cVarName;

		[PublicizedFrom(EAccessModifier.Private)]
		public float currentValue;
	}

	public abstract class VariableStateSimpleLookupAbs : BindingInfoNcalc.VariableStateAbs
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract object getCurrentValue();

		public abstract string VarType
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get;
		}

		public abstract string VarName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get;
		}

		public override bool RefreshValue()
		{
			object obj = this.getCurrentValue();
			if (obj == null)
			{
				Log.Warning(string.Concat(new string[] { "[XUi] ", this.VarType, " '", this.VarName, "' returned null." }));
				return false;
			}
			if (obj.Equals(this.CurrentValue))
			{
				return false;
			}
			this.currentValue = obj;
			return true;
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}={2}", this.VarType, this.VarName, this.currentValue);
		}

		public override object CurrentValue
		{
			get
			{
				return this.currentValue;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public VariableStateSimpleLookupAbs()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public object currentValue;
	}

	public class VariableStateGamePref : BindingInfoNcalc.VariableStateSimpleLookupAbs
	{
		public VariableStateGamePref(EnumGamePrefs _pref)
		{
			this.pref = _pref;
			this.currentValue = this.getCurrentValue();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override object getCurrentValue()
		{
			return GamePrefs.GetObject(this.pref);
		}

		public override string VarType
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return "GamePref";
			}
		}

		public override string VarName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return this.pref.ToStringCached<EnumGamePrefs>();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly EnumGamePrefs pref;
	}

	public class VariableStateGameStat : BindingInfoNcalc.VariableStateSimpleLookupAbs
	{
		public VariableStateGameStat(EnumGameStats _stat)
		{
			this.stat = _stat;
			this.currentValue = this.getCurrentValue();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override object getCurrentValue()
		{
			return GameStats.GetObject(this.stat);
		}

		public override string VarType
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return "GameStat";
			}
		}

		public override string VarName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return this.stat.ToStringCached<EnumGameStats>();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly EnumGameStats stat;
	}

	public class VariableStateGameInfoInt : BindingInfoNcalc.VariableStateSimpleLookupAbs
	{
		public VariableStateGameInfoInt(GameInfoInt _name)
		{
			this.name = _name;
			this.currentValue = this.getCurrentValue();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override object getCurrentValue()
		{
			return (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo).GetValue(this.name);
		}

		public override string VarType
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return "GameInfoInt";
			}
		}

		public override string VarName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return this.name.ToStringCached<GameInfoInt>();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly GameInfoInt name;
	}

	public class VariableStateGameInfoBool : BindingInfoNcalc.VariableStateSimpleLookupAbs
	{
		public VariableStateGameInfoBool(GameInfoBool _name)
		{
			this.name = _name;
			this.currentValue = this.getCurrentValue();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override object getCurrentValue()
		{
			return (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo).GetValue(this.name);
		}

		public override string VarType
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return "GameInfoBool";
			}
		}

		public override string VarName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return this.name.ToStringCached<GameInfoBool>();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly GameInfoBool name;
	}

	public class VariableStateGameInfoString : BindingInfoNcalc.VariableStateSimpleLookupAbs
	{
		public VariableStateGameInfoString(GameInfoString _name)
		{
			this.name = _name;
			this.currentValue = this.getCurrentValue();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override object getCurrentValue()
		{
			return (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo).GetValue(this.name);
		}

		public override string VarType
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return "GameInfoString";
			}
		}

		public override string VarName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return this.name.ToStringCached<GameInfoString>();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly GameInfoString name;
	}
}
