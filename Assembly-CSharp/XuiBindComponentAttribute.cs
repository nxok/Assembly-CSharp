using System;
using JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Field)]
[MeansImplicitUse]
public class XuiBindComponentAttribute : Attribute
{
	public XuiBindComponentAttribute(bool _required = true)
	{
		this.Required = _required;
	}

	public XuiBindComponentAttribute(string _xmlElementName, bool _required = true)
	{
		this.XmlElementName = _xmlElementName;
		this.Required = _required;
	}

	public readonly string XmlElementName;

	public readonly bool Required;
}
