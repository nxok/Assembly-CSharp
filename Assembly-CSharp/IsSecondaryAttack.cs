using System;
using UnityEngine.Scripting;

[Preserve]
public class IsSecondaryAttack : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (this.target == null || this.target.inventory.holdingItemItemValue.ItemClass.Actions[1] == null)
		{
			return false;
		}
		if (this.invert)
		{
			return !this.target.inventory.holdingItemItemValue.ItemClass.Actions[1].IsActionRunning(this.target.inventory.GetItemActionDataInSlot(this.target.inventory.holdingItemIdx, 1));
		}
		return this.target.inventory.holdingItemItemValue.ItemClass.Actions[1].IsActionRunning(this.target.inventory.GetItemActionDataInSlot(this.target.inventory.holdingItemIdx, 1));
	}
}
