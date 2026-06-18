using System;
using UnityEngine.Scripting;

[Preserve]
public class IsPrimaryAttack : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (this.target == null || this.target.inventory.holdingItemItemValue.ItemClass.Actions[0] == null)
		{
			return false;
		}
		if (this.invert)
		{
			return !this.target.inventory.holdingItemItemValue.ItemClass.Actions[0].IsActionRunning(this.target.inventory.GetItemActionDataInSlot(this.target.inventory.holdingItemIdx, 0));
		}
		return this.target.inventory.holdingItemItemValue.ItemClass.Actions[0].IsActionRunning(this.target.inventory.GetItemActionDataInSlot(this.target.inventory.holdingItemIdx, 0));
	}
}
