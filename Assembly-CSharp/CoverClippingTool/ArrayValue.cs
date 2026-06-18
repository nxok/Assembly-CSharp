using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CoverClippingTool
{
	public class ArrayValue
	{
		public ArrayValue(XElement element, List<Dictionary<string, PropertyValue>> items)
		{
			this.Element = element;
			this.Items = items;
		}

		public ArrayValue Clone()
		{
			ArrayValue arrayValue = new ArrayValue(this.Element, new List<Dictionary<string, PropertyValue>>());
			foreach (Dictionary<string, PropertyValue> dictionary in this.Items)
			{
				Dictionary<string, PropertyValue> dictionary2 = new Dictionary<string, PropertyValue>();
				foreach (KeyValuePair<string, PropertyValue> keyValuePair in dictionary)
				{
					dictionary2[keyValuePair.Key] = keyValuePair.Value.Clone();
				}
				arrayValue.Items.Add(dictionary2);
			}
			return arrayValue;
		}

		public readonly List<Dictionary<string, PropertyValue>> Items;

		public readonly XElement Element;
	}
}
