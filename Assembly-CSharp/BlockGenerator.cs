using System;
using UnityEngine.Scripting;

[Preserve]
public class BlockGenerator : BlockPowerSource
{
	public override TileEntityPowerSource CreateTileEntity(Chunk chunk)
	{
		if (this.slotItem == null)
		{
			this.slotItem = ItemClass.GetItemClass(this.SlotItemName, false);
		}
		return new TileEntityPowerSource(chunk)
		{
			PowerItemType = PowerItem.PowerItemTypes.Generator,
			SlotItem = this.slotItem
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string GetPowerSourceIcon()
	{
		return "electric_generator";
	}

	public static FastTags<TagGroup.Global> tag = FastTags<TagGroup.Global>.Parse("gasoline");
}
