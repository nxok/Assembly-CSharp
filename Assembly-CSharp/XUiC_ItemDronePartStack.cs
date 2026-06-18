using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemDronePartStack : XUiC_ItemPartStack
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CanSwap(ItemStack stack)
	{
		bool flag = base.CanSwap(stack);
		if (base.ItemClass != null && this.itemClass.HasAnyTags(EntityDrone.StorageModifierTags))
		{
			EntityDrone currentVehicleEntity = ((XUiC_DroneWindowGroup)this.windowGroup.Controller).CurrentVehicleEntity;
			if (!currentVehicleEntity.CanRemoveExtraStorage())
			{
				currentVehicleEntity.NotifyToManyStoredItems();
				return false;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CanRemove()
	{
		bool flag = base.CanRemove();
		if (this.itemClass != null && this.itemClass.HasAnyTags(EntityDrone.StorageModifierTags))
		{
			EntityDrone currentVehicleEntity = ((XUiC_DroneWindowGroup)this.windowGroup.Controller).CurrentVehicleEntity;
			if (!currentVehicleEntity.CanRemoveExtraStorage())
			{
				if (this.xui.playerUI.CursorController.GetMouseButtonUp(0))
				{
					currentVehicleEntity.NotifyToManyStoredItems();
				}
				return false;
			}
		}
		return flag;
	}
}
