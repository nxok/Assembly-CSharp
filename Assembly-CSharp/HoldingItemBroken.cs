using System;
using UnityEngine.Scripting;

[Preserve]
public class HoldingItemBroken : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (this.target == null)
		{
			return false;
		}
		bool flag = this.target.inventory.holdingItemItemValue.PercentUsesLeft <= 0f;
		if (!this.invert)
		{
			return flag;
		}
		return !flag;
	}
}
