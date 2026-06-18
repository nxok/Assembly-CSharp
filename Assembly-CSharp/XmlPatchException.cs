using System;
using System.Xml;
using System.Xml.Linq;

public class XmlPatchException : Exception
{
	public XmlPatchException(XElement _patchElement, string _patchMethodName, string _message)
		: this(XmlPatchException.buildMessage(_patchElement, _patchMethodName, _message))
	{
	}

	public XmlPatchException(XElement _patchElement, string _patchMethodName, string _message, Exception _innerException)
		: this(XmlPatchException.buildMessage(_patchElement, _patchMethodName, _message), _innerException)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string buildMessage(XElement _patchElement, string _patchMethodName, string _message)
	{
		return string.Format("XML.{0} ({1}, line {2} at pos {3}): {4}", new object[]
		{
			_patchMethodName,
			_patchElement.GetXPath(),
			((IXmlLineInfo)_patchElement).LineNumber,
			((IXmlLineInfo)_patchElement).LinePosition,
			_message
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XmlPatchException(string _message)
		: base(_message)
	{
	}

	public XmlPatchException(string _message, Exception _innerException)
		: base(_message, _innerException)
	{
	}
}
