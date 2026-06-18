using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
[MeansImplicitUse]
public class XuiXmlBindingAttribute : Attribute
{
	public XuiXmlBindingAttribute(string _bindingName)
	{
		this.BindingName = _bindingName;
	}

	public readonly string BindingName;
}
