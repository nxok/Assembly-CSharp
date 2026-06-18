using System;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/UI Storage Slot")]
public class UIStorageSlot : UIItemSlot
{
	public override InvGameItem observedItem
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (!(this.storage != null))
			{
				return null;
			}
			return this.storage.GetItem(this.slot);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override InvGameItem Replace(InvGameItem item)
	{
		if (!(this.storage != null))
		{
			return item;
		}
		return this.storage.Replace(this.slot, item);
	}

	public UIItemStorage storage;

	public int slot;
}
