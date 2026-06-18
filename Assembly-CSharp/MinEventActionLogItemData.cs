using System;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionLogItemData : MinEventActionBase
{
	public override void Execute(MinEventParams _params)
	{
		if (_params.Self.inventory.holdingItem == null)
		{
			return;
		}
		Log.Out("Debug Item: '{0}' Tags: {1}", new object[]
		{
			_params.Self.inventory.holdingItem.GetItemName(),
			_params.Self.inventory.holdingItem.ItemTags.ToString()
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string message;
}
