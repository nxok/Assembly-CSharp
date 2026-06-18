using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class PlayerLevel : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (this.target.Progression == null)
		{
			return false;
		}
		int level = this.target.Progression.GetLevel();
		if (this.invert)
		{
			return !RequirementBase.compareValues((float)level, this.operation, this.value);
		}
		return RequirementBase.compareValues((float)level, this.operation, this.value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Player level {0} {1} {2}", this.invert ? "NOT" : "", this.operation.ToStringCached<RequirementBase.OperationTypes>(), this.value.ToCultureInvariantString()));
	}
}
