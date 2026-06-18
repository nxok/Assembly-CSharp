using System;
using UnityEngine;

namespace XMLData.Parsers
{
	public static class Vector3Parser
	{
		public static Vector3 Parse(string _value)
		{
			return StringParsers.ParseVector3(_value, 0, -1);
		}

		public static string Unparse(Vector3 _value)
		{
			return string.Format("{0},{1},{2}", _value.x.ToCultureInvariantString(), _value.y.ToCultureInvariantString(), _value.z.ToCultureInvariantString());
		}
	}
}
