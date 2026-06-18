using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionDropHeldItem : ActionBaseItemAction
	{
		public override bool CanPerform(Entity target)
		{
			EntityPlayer entityPlayer = target as EntityPlayer;
			return entityPlayer != null && !((entityPlayer.saveInventory != null) ? entityPlayer.saveInventory : entityPlayer.inventory).holdingItemStack.IsEmpty();
		}

		public override void OnClientPerform(Entity target)
		{
			EntityAlive entityAlive = target as EntityAlive;
			if (entityAlive != null)
			{
				Inventory inventory = ((entityAlive.saveInventory != null) ? entityAlive.saveInventory : entityAlive.inventory);
				if (inventory.holdingItem != entityAlive.inventory.GetBareHandItem())
				{
					Vector3 dropPosition = entityAlive.GetDropPosition();
					ItemValue holdingItemItemValue = inventory.holdingItemItemValue;
					int count = inventory.holdingItemStack.count;
					GameManager.Instance.DropContentInLootContainerServer(entityAlive.entityId, "DroppedLootContainerTwitch", dropPosition, new ItemStack[] { inventory.holdingItemStack.Clone() }, false, null);
					entityAlive.AddUIHarvestingItem(new ItemStack(holdingItemItemValue, -count), false);
					Manager.BroadcastPlay(entityAlive, this.DropSound, false, 1f);
					inventory.DecHoldingItem(count);
				}
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionDropHeldItem.PropDropSound, ref this.DropSound);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionDropHeldItem
			{
				targetGroup = this.targetGroup,
				DropSound = this.DropSound
			};
		}

		public string DropSound = "";

		public static string PropDropSound = "drop_sound";
	}
}
