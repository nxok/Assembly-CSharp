using System;
using System.Collections.Generic;

namespace CoverClippingTool
{
	public class ShapeData
	{
		public ShapeData(string name)
		{
			this.Name = name;
		}

		public ShapeData Clone(string name)
		{
			ShapeData shapeData = new ShapeData(name);
			foreach (KeyValuePair<string, PropertyValue> keyValuePair in this.Properties)
			{
				shapeData.Properties[keyValuePair.Key] = keyValuePair.Value.Clone();
			}
			foreach (KeyValuePair<string, ArrayValue> keyValuePair2 in this.Arrays)
			{
				shapeData.Arrays[keyValuePair2.Key] = keyValuePair2.Value.Clone();
			}
			return shapeData;
		}

		public readonly string Name;

		public ShapeData Extends;

		public Dictionary<string, PropertyValue> Properties = new Dictionary<string, PropertyValue>(StringComparer.OrdinalIgnoreCase);

		public Dictionary<string, ArrayValue> Arrays = new Dictionary<string, ArrayValue>(StringComparer.OrdinalIgnoreCase);
	}
}
