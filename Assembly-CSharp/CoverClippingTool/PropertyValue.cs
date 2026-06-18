using System;
using System.Xml.Linq;

namespace CoverClippingTool
{
	public struct PropertyValue
	{
		public PropertyValue(XElement element, string value)
		{
			this.Element = element;
			this.Value = value;
		}

		public PropertyValue Clone()
		{
			return new PropertyValue(this.Element, this.Value);
		}

		public readonly string Value;

		public readonly XElement Element;
	}
}
