using System;
using UnityEngine.Scripting;

[Preserve]
public class IsSDCS : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		EntityAlive target = this.target;
		bool flag = ((target != null) ? target.emodel : null) as EModelSDCS != null;
		if (!this.invert)
		{
			return flag;
		}
		return !flag;
	}
}
