using System;
using XMLData.Exceptions;

namespace XMLData.Parsers
{
	public static class doubleParser
	{
		public static double Parse(string _value)
		{
			double num;
			if (StringParsers.TryParseDouble(_value, out num))
			{
				return num;
			}
			throw new InvalidValueException("Expected double value, found \"" + _value + "\"", -1);
		}

		public static string Unparse(double _value)
		{
			return _value.ToCultureInvariantString();
		}
	}
}
