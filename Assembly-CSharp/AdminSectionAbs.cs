using System;
using System.Xml;

public abstract class AdminSectionAbs
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public AdminSectionAbs(AdminTools _parent, string _sectionTypeName)
	{
		this.Parent = _parent;
		this.SectionTypeName = _sectionTypeName;
	}

	public abstract void Clear();

	public virtual void Parse(XmlNode _parentNode)
	{
		foreach (object obj in _parentNode.ChildNodes)
		{
			XmlNode xmlNode = (XmlNode)obj;
			if (xmlNode.NodeType != XmlNodeType.Comment)
			{
				if (xmlNode.NodeType != XmlNodeType.Element)
				{
					Log.Warning("Unexpected XML node found in '" + this.SectionTypeName + "' section: " + xmlNode.OuterXml);
				}
				else
				{
					XmlElement xmlElement = (XmlElement)xmlNode;
					this.ParseElement(xmlElement);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void ParseElement(XmlElement _childElement);

	public abstract void Save(XmlElement _root);

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly AdminTools Parent;

	public readonly string SectionTypeName;
}
