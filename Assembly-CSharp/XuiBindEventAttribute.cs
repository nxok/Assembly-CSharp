using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
[MeansImplicitUse]
public class XuiBindEventAttribute : Attribute
{
	public XuiBindEventAttribute(string _targetEvent, string _componentFieldName = null)
	{
		this.ComponentFieldName = _componentFieldName;
		this.TargetEvent = _targetEvent;
	}

	public readonly string ComponentFieldName;

	public readonly string TargetEvent;
}
