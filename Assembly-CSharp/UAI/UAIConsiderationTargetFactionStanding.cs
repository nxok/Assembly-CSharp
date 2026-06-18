using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Scripting;

namespace UAI
{
	[Preserve]
	public class UAIConsiderationTargetFactionStanding : UAIConsiderationBase
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
			this.max = 255f;
		}

		public override float GetScore(Context _context, object target)
		{
			if (target is EntityAlive)
			{
				EntityAlive entityAlive = UAIUtils.ConvertToEntityAlive(target);
				return (FactionManager.Instance.GetRelationshipValue(_context.Self, entityAlive) - this.min) / (this.max - this.min);
			}
			return 0f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float min;

		[PublicizedFrom(EAccessModifier.Private)]
		public float max;
	}
}
