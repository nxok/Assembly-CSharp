using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Field)]
[MeansImplicitUse]
public class XuiBindParentAttribute : Attribute
{
	public XuiBindParentAttribute(bool _required = true)
	{
		this.Required = _required;
	}

	public readonly bool Required;
}
