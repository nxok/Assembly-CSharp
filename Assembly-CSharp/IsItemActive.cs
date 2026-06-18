using System;
using UnityEngine.Scripting;

[Preserve]
public class IsItemActive : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (_params.ItemValue == null)
		{
			return false;
		}
		if (_params.ItemValue.Activated > 0)
		{
			return !this.invert;
		}
		return this.invert;
	}
}
