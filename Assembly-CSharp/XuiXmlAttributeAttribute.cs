using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
[MeansImplicitUse]
public class XuiXmlAttributeAttribute : Attribute
{
	public XuiXmlAttributeAttribute(string _attributeName, bool _override = false)
	{
		this.AttributeName = _attributeName;
		this.Override = _override;
	}

	public readonly string AttributeName;

	public readonly bool Override;
}
