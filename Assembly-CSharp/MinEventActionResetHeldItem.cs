using System;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionResetHeldItem : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		ItemActionData itemActionData = _params.ItemActionData;
		if (itemActionData == null)
		{
			return;
		}
		itemActionData.invData.item.OnHoldingReset(_params.ItemActionData.invData);
	}
}
