using System;
using System.Xml;
using System.Xml.Linq;

public class XmlLoadException : Exception
{
	public XmlLoadException(string _xmlName, XElement _element, string _message)
		: this(XmlLoadException.buildMessage(_element, _xmlName, _message))
	{
	}

	public XmlLoadException(string _xmlName, XElement _element, string _message, Exception _innerException)
		: this(XmlLoadException.buildMessage(_element, _xmlName, _message), _innerException)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string buildMessage(XElement _element, string _xmlName, string _message)
	{
		return string.Format("Error loading {0}: {1} (line {2} at pos {3})", new object[]
		{
			_xmlName,
			_message,
			((IXmlLineInfo)_element).LineNumber,
			((IXmlLineInfo)_element).LinePosition
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XmlLoadException(string _message)
		: base(_message)
	{
	}

	public XmlLoadException(string _message, Exception _innerException)
		: base(_message, _innerException)
	{
	}
}
