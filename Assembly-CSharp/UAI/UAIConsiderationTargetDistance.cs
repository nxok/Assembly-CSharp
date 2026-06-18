using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

namespace UAI
{
	[Preserve]
	public class UAIConsiderationTargetDistance : UAIConsiderationBase
	{
		public override void Init(Dictionary<string, string> _parameters)
		{
			base.Init(_parameters);
			if (_parameters.ContainsKey("min"))
			{
				this.min = StringParsers.ParseFloat(_parameters["min"], 0, -1, NumberStyles.Any);
				this.min *= this.min;
			}
			if (_parameters.ContainsKey("max"))
			{
				this.max = StringParsers.ParseFloat(_parameters["max"], 0, -1, NumberStyles.Any);
				this.max *= this.max;
			}
		}

		public override float GetScore(Context _context, object target)
		{
			EntityAlive entityAlive = UAIUtils.ConvertToEntityAlive(target);
			if (entityAlive != null)
			{
				float num = UAIUtils.DistanceSqr(_context.Self.position, entityAlive.position);
				return Mathf.Clamp01(Mathf.Max(0f, num - this.min) / (this.max - this.min));
			}
			if (target.GetType() == typeof(Vector3))
			{
				Vector3 vector = (Vector3)target;
				float num2 = UAIUtils.DistanceSqr(_context.Self.position, vector);
				return Mathf.Clamp01(Mathf.Max(0f, num2 - this.min) / (this.max - this.min));
			}
			return 0f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float min;

		[PublicizedFrom(EAccessModifier.Private)]
		public float max = 9126f;
	}
}
