using System;
using System.Xml;

namespace ICSharpCode.WpfDesign.XamlDom
{
	public class PositionXmlCDataSection : XmlCDataSection, IXmlLineInfo
	{
		[PublicizedFrom(EAccessModifier.Internal)]
		public PositionXmlCDataSection(string data, XmlDocument doc, IXmlLineInfo lineInfo)
			: base(data, doc)
		{
			if (lineInfo != null)
			{
				this.lineNumber = lineInfo.LineNumber;
				this.linePosition = lineInfo.LinePosition;
				this.hasLineInfo = true;
			}
		}

		public bool HasLineInfo()
		{
			return this.hasLineInfo;
		}

		public int LineNumber
		{
			get
			{
				return this.lineNumber;
			}
		}

		public int LinePosition
		{
			get
			{
				return this.linePosition;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int lineNumber;

		[PublicizedFrom(EAccessModifier.Private)]
		public int linePosition;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasLineInfo;
	}
}
