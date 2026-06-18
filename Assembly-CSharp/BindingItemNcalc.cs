using System;
using System.Collections.Generic;
using NCalc;
using Unity.Profiling;

public class BindingItemNcalc : BindingItem, IBindingNcalc
{
	public BindingItemNcalc(BindingInfo _parent, string _sourceText)
		: base(_sourceText)
	{
		if (this.fieldName[0] == '#')
		{
			this.fieldName = this.fieldName.Substring(1);
		}
		this.Parent = _parent;
		this.IsInitializing = true;
		this.expression = new Expression(this.fieldName, 2566)
		{
			Parameters = this.expressionParamDict
		};
		this.expression.EvaluateFunction += new EvaluateFunctionHandler(this.nCalcEvaluateFunction);
		this.expression.EvaluateParameter += new EvaluateParameterHandler(this.nCalcEvaluateParameter);
		this.currentValue = this.evaluateExpression();
		this.IsInitializing = false;
		if (this.usesIndeterministicFunctions || this.bindings.Count == 0)
		{
			for (XUiController xuiController = this.Parent.View.Controller; xuiController != null; xuiController = xuiController.Parent)
			{
				if (xuiController.GetType() != typeof(XUiController))
				{
					xuiController.Bindings.AddBinding(_parent);
					return;
				}
			}
		}
	}

	public override string GetValue()
	{
		bool flag = this.usesIndeterministicFunctions;
		foreach (KeyValuePair<string, BindingItemNcalc.BindingState> keyValuePair in this.bindings)
		{
			if (keyValuePair.Value.RefreshValue())
			{
				this.expressionParamDict[keyValuePair.Key] = keyValuePair.Value.CurrentValue;
				flag = true;
			}
		}
		if (!flag)
		{
			return this.currentValue;
		}
		using (this.pmNcalcEval.Auto())
		{
			this.currentValue = this.evaluateExpression();
		}
		return this.currentValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string evaluateExpression()
	{
		string text = null;
		try
		{
			object obj = this.expression.Evaluate();
			string text2;
			if (obj != null)
			{
				if (obj is decimal)
				{
					decimal num = (decimal)obj;
					text2 = num.ToCultureInvariantString("0.########");
				}
				else if (obj is float)
				{
					float num2 = (float)obj;
					text2 = num2.ToCultureInvariantString();
				}
				else if (obj is double)
				{
					double num3 = (double)obj;
					text2 = num3.ToCultureInvariantString();
				}
				else
				{
					text2 = obj.ToString();
				}
			}
			else
			{
				text2 = "";
			}
			text = text2;
		}
		catch (ArgumentException ex)
		{
			Log.Error("[XUi] Binding expression can not be evaluated: " + this.SourceText + " --- hierarchy: " + this.Parent.View.GetXuiHierarchy());
			Log.Exception(ex);
		}
		catch (Exception ex2)
		{
			Log.Error("[XUi] Binding expression can not be evaluated: " + this.SourceText + " --- hierarchy: " + this.Parent.View.GetXuiHierarchy());
			Log.Exception(ex2);
			text = "";
		}
		return text;
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

	public bool FindParameter(string _name, out object _value)
	{
		BindingItemNcalc.BindingState bindingState;
		if (this.bindings.TryGetValue(_name, out bindingState))
		{
			_value = bindingState.CurrentValue;
			return true;
		}
		for (XUiController xuiController = this.Parent.View.Controller; xuiController != null; xuiController = xuiController.Parent)
		{
			if (xuiController.GetType() != typeof(XUiController))
			{
				string text = "";
				if (xuiController.GetBindingValue(ref text, _name))
				{
					xuiController.Bindings.AddBinding(this.Parent);
					bindingState = new BindingItemNcalc.BindingState(_name, xuiController, text, null);
					this.bindings[_name] = bindingState;
					this.expressionParamDict[_name] = text;
					_value = text;
					return true;
				}
				XuiBindingDelegate xuiBindingDelegate;
				if (BindingMethodCache.Instance.TryGetBindingDelegate(xuiController, _name, out xuiBindingDelegate))
				{
					xuiController.Bindings.AddBinding(this.Parent);
					text = xuiBindingDelegate(xuiController).ToString();
					bindingState = new BindingItemNcalc.BindingState(_name, xuiController, text, xuiBindingDelegate);
					this.bindings[_name] = bindingState;
					this.expressionParamDict[_name] = text;
					_value = xuiBindingDelegate(xuiController);
					return true;
				}
			}
		}
		_value = null;
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void nCalcEvaluateFunction(string _name, FunctionArgs _args, bool _ignoreCase)
	{
		BindingNcalcFunctions.EvaluateFunc(this, _name, _args);
	}

	public new string SourceText
	{
		get
		{
			return this.SourceText;
		}
	}

	public IXUiElement TargetElement
	{
		get
		{
			return this.Parent.View;
		}
	}

	public bool IsInitializing { get; }

	public void SetIndeterministic()
	{
		this.usesIndeterministicFunctions = true;
	}

	public bool RegisterVariable(BindingInfoNcalc.VariableStateAbs _var)
	{
		return false;
	}

	public readonly BindingInfo Parent;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, BindingItemNcalc.BindingState> bindings = new Dictionary<string, BindingItemNcalc.BindingState>(StringComparer.Ordinal);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, object> expressionParamDict = new Dictionary<string, object>(StringComparer.Ordinal);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Expression expression;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool usesIndeterministicFunctions;

	[PublicizedFrom(EAccessModifier.Private)]
	public string currentValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmNcalcEval = new ProfilerMarker("NCalc.Evaluate");

	[PublicizedFrom(EAccessModifier.Private)]
	public class BindingState
	{
		public BindingState(string _bindingName, XUiController _controller, string _initialValue, XuiBindingDelegate _bindingDelegate)
		{
			this.bindingName = _bindingName;
			this.controller = _controller;
			this.CurrentValue = _initialValue;
			this.bindingDelegate = _bindingDelegate;
		}

		public bool RefreshValue()
		{
			string text = "";
			if (this.bindingDelegate == null)
			{
				if (!this.controller.GetBindingValue(ref text, this.bindingName))
				{
					Log.Error(string.Concat(new string[]
					{
						"[XUi] Refreshing binding failed: Controller's GetBindingValue no longer returns true! (Binding: ",
						this.bindingName,
						", hierarchy: ",
						this.controller.GetXuiHierarchy(),
						")"
					}));
					return false;
				}
			}
			else
			{
				object obj = this.bindingDelegate(this.controller);
				if (obj == null)
				{
					Log.Warning("[XUi] Binding '" + this.bindingName + "' returned null, should always return appropriate non-null value of same data type. Hierarchy: " + this.controller.GetXuiHierarchy());
					obj = "";
				}
				text = obj.ToString();
			}
			if (string.Equals(this.CurrentValue, text, StringComparison.Ordinal))
			{
				return false;
			}
			this.CurrentValue = text;
			return true;
		}

		public override string ToString()
		{
			return "Binding:" + this.bindingName + "=" + this.CurrentValue;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string bindingName;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiController controller;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XuiBindingDelegate bindingDelegate;

		public string CurrentValue;
	}
}
