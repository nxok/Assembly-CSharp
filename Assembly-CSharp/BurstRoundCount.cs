using System;
using System.Collections.Generic;

public class BurstRoundCount : TargetedCompareRequirementBase
{
	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (_params.ItemValue.IsEmpty())
		{
			return false;
		}
		ItemActionRanged itemActionRanged = _params.ItemValue.ItemClass.Actions[0] as ItemActionRanged;
		if (itemActionRanged == null)
		{
			return false;
		}
		if (this.invert)
		{
			return !RequirementBase.compareValues((float)itemActionRanged.GetBurstCount(this.target.inventory.holdingItemData.actionData[0]), this.operation, this.value);
		}
		return RequirementBase.compareValues((float)itemActionRanged.GetBurstCount(this.target.inventory.holdingItemData.actionData[0]), this.operation, this.value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add(string.Format("Rounds in Magazine: {0}{1} {2}", this.invert ? "NOT " : "", this.operation.ToStringCached<RequirementBase.OperationTypes>(), this.value.ToCultureInvariantString()));
	}
}
