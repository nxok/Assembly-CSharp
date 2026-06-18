using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Scripting;

namespace UAI
{
	[Preserve]
	public class UAIConsiderationSelfHealth : UAIConsiderationBase
	{
		public override void Init(Dictionary<string, string> parameters)
		{
			base.Init(parameters);
			if (parameters.ContainsKey("min"))
			{
				this.min = StringParsers.ParseFloat(parameters["min"], 0, -1, NumberStyles.Any);
			}
			else
			{
				this.min = 0f;
			}
			if (parameters.ContainsKey("max"))
			{
				this.max = StringParsers.ParseFloat(parameters["max"], 0, -1, NumberStyles.Any);
				return;
			}
			this.max = float.NaN;
		}

		public override float GetScore(Context _context, object _target)
		{
			if (float.IsNaN(this.max))
			{
				this.max = (float)_context.Self.GetMaxHealth();
			}
			return ((float)_context.Self.Health - this.min) / (this.max - this.min);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float min;

		[PublicizedFrom(EAccessModifier.Private)]
		public float max;
	}
}
