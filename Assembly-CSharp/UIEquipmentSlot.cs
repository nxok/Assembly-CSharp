using System;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/UI Equipment Slot")]
public class UIEquipmentSlot : UIItemSlot
{
	public override InvGameItem observedItem
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (!(this.equipment != null))
			{
				return null;
			}
			return this.equipment.GetItem(this.slot);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override InvGameItem Replace(InvGameItem item)
	{
		if (!(this.equipment != null))
		{
			return item;
		}
		return this.equipment.Replace(this.slot, item);
	}

	public InvEquipment equipment;

	public InvBaseItem.Slot slot;
}
