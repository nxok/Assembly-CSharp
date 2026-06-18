using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
[MeansImplicitUse]
public class XuiBindingNcalcFunctionAttribute : Attribute
{
	public XuiBindingNcalcFunctionAttribute()
	{
	}

	public XuiBindingNcalcFunctionAttribute(object _errorResultValue, int _expectedArgumentCount, string _functionName = null)
	{
		this.FunctionName = _functionName;
		this.ExpectedArgumentCount = _expectedArgumentCount;
		this.ErrorResult = _errorResultValue;
	}

	public readonly string FunctionName;

	public readonly int ExpectedArgumentCount;

	public readonly object ErrorResult;
}
