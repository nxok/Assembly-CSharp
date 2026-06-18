using System;

namespace XMLData.Exceptions
{
	public class XmlParserException : Exception
	{
		public int Line
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public XmlParserException(string _msg, int _line)
			: base(_msg)
		{
			this.Line = _line;
		}

		public XmlParserException(string _msg, int _line, Exception _innerException)
			: base(_msg, _innerException)
		{
			this.Line = _line;
		}

		public override string Message
		{
			get
			{
				return string.Format("{0} (line {1})", base.Message, this.Line);
			}
		}

		public override string ToString()
		{
			return this.Message;
		}
	}
}
