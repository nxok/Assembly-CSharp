using System;
using UnityEngine.Scripting;

[Preserve]
public class BlockBatteryBank : BlockPowerSource
{
	public override TileEntityPowerSource CreateTileEntity(Chunk chunk)
	{
		if (this.slotItem == null)
		{
			this.slotItem = ItemClass.GetItemClass(this.SlotItemName, false);
		}
		return new TileEntityPowerSource(chunk)
		{
			PowerItemType = PowerItem.PowerItemTypes.BatteryBank,
			SlotItem = this.slotItem
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string GetPowerSourceIcon()
	{
		return "battery";
	}
}
