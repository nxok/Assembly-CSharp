using System;
using UnityEngine.Scripting;

[Preserve]
public class IsInstigator : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		return base.IsValid(_params) && this.invert != (this.target == _params.Instigator);
	}
}
