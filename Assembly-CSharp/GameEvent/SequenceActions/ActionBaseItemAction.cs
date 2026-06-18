using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionBaseItemAction : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			EntityPlayer entityPlayer = target as EntityPlayer;
			if (entityPlayer != null)
			{
				this.OnClientActionStarted(entityPlayer);
				this.count = GameEventManager.GetIntValue(entityPlayer, this.countText, -1);
				bool flag = false;
				FastTags<TagGroup.Global>.Parse(this.itemTags);
				if (this.itemLocations.Contains(ActionBaseItemAction.ItemLocations.Toolbelt) && !this.isFinished)
				{
					ItemStack[] array = ((entityPlayer.AttachedToEntity != null && entityPlayer.saveInventory != null) ? entityPlayer.saveInventory.GetSlots() : entityPlayer.inventory.GetSlots());
					for (int i = 0; i < array.Length; i++)
					{
						if (this.HandleItemStackChange(ref array[i], entityPlayer))
						{
							flag = true;
						}
						if (this.isFinished)
						{
							break;
						}
					}
					if (flag)
					{
						entityPlayer.inventory.SetSlots(array, true);
						entityPlayer.bPlayerStatsChanged = true;
					}
				}
				flag = false;
				if (this.itemLocations.Contains(ActionBaseItemAction.ItemLocations.Equipment) || (this.itemLocations.Contains(ActionBaseItemAction.ItemLocations.BiomeBadge) && !this.isFinished))
				{
					int slotCount = entityPlayer.equipment.GetSlotCount();
					int num = 4;
					for (int j = 0; j < slotCount; j++)
					{
						if ((j < num || this.itemLocations.Contains(ActionBaseItemAction.ItemLocations.BiomeBadge)) && (j >= num || this.itemLocations.Contains(ActionBaseItemAction.ItemLocations.Equipment)))
						{
							if (this.CheckEquipmentReplace(entityPlayer.equipment, j))
							{
								ItemValue slotItemOrNone = entityPlayer.equipment.GetSlotItemOrNone(j);
								if (this.HandleItemValueChange(ref slotItemOrNone, entityPlayer))
								{
									entityPlayer.equipment.SetSlotItem(j, slotItemOrNone, true);
									flag = true;
								}
							}
							if (this.isFinished)
							{
								break;
							}
						}
					}
					if (flag)
					{
						entityPlayer.bPlayerStatsChanged = true;
					}
				}
				flag = false;
				if (this.itemLocations.Contains(ActionBaseItemAction.ItemLocations.Backpack) && !this.isFinished)
				{
					ItemStack[] slots = entityPlayer.bag.GetSlots();
					for (int k = 0; k < slots.Length; k++)
					{
						if (this.HandleItemStackChange(ref slots[k], entityPlayer))
						{
							flag = true;
						}
						if (this.isFinished)
						{
							break;
						}
					}
					if (flag)
					{
						entityPlayer.bag.SetSlots(slots);
						entityPlayer.bPlayerStatsChanged = true;
					}
				}
				if (this.itemLocations.Contains(ActionBaseItemAction.ItemLocations.Backpack) && !this.isFinished)
				{
					XUiC_DragAndDropWindow dragAndDropWindow = LocalPlayerUI.GetUIForPrimaryPlayer().xui.DragAndDropWindow;
					if (!dragAndDropWindow.CurrentStack.IsEmpty())
					{
						ItemStack currentStack = dragAndDropWindow.CurrentStack;
						if (this.HandleItemStackChange(ref currentStack, entityPlayer))
						{
							entityPlayer.bPlayerStatsChanged = true;
						}
					}
				}
				if (!this.itemLocations.Contains(ActionBaseItemAction.ItemLocations.Toolbelt) && this.itemLocations.Contains(ActionBaseItemAction.ItemLocations.Held) && !this.isFinished)
				{
					Inventory inventory = ((entityPlayer.saveInventory != null) ? entityPlayer.saveInventory : entityPlayer.inventory);
					if (inventory.holdingItem != entityPlayer.inventory.GetBareHandItem())
					{
						ItemStack holdingItemStack = inventory.holdingItemStack;
						if (this.HandleItemStackChange(ref holdingItemStack, entityPlayer))
						{
							inventory.SetItem(inventory.holdingItemIdx, holdingItemStack);
							entityPlayer.bPlayerStatsChanged = true;
						}
					}
				}
				this.OnClientActionEnded(entityPlayer);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual bool CheckEquipmentReplace(Equipment equipment, int slot)
		{
			return true;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void OnClientActionStarted(EntityPlayer player)
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void OnClientActionEnded(EntityPlayer player)
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual bool HandleItemStackChange(ref ItemStack stack, EntityPlayer player)
		{
			return false;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual bool HandleItemValueChange(ref ItemValue itemValue, EntityPlayer player)
		{
			return false;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void AddStack(EntityPlayerLocal player, ItemStack stack)
		{
			if (!LocalPlayerUI.GetUIForPlayer(player).xui.PlayerInventory.AddItem(stack))
			{
				GameManager.Instance.ItemDropServer(stack, player.GetPosition(), Vector3.zero, -1, 60f, false);
			}
		}

		public override BaseAction Clone()
		{
			ActionBaseItemAction actionBaseItemAction = (ActionBaseItemAction)base.Clone();
			actionBaseItemAction.countText = this.countText;
			actionBaseItemAction.countType = this.countType;
			actionBaseItemAction.itemTags = this.itemTags;
			actionBaseItemAction.fastItemTags = this.fastItemTags;
			actionBaseItemAction.itemLocations = new List<ActionBaseItemAction.ItemLocations>(this.itemLocations);
			return actionBaseItemAction;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			ActionBaseItemAction.ItemLocations itemLocations = ActionBaseItemAction.ItemLocations.Toolbelt;
			if (properties.Values.ContainsKey(ActionBaseItemAction.PropItemLocation))
			{
				string[] array = properties.Values[ActionBaseItemAction.PropItemLocation].Split(',', StringSplitOptions.None);
				this.itemLocations.Clear();
				for (int i = 0; i < array.Length; i++)
				{
					if (Enum.TryParse<ActionBaseItemAction.ItemLocations>(array[i], true, out itemLocations))
					{
						this.itemLocations.Add(itemLocations);
					}
				}
			}
			properties.ParseString(ActionBaseItemAction.PropItemTag, ref this.itemTags);
			this.fastItemTags = FastTags<TagGroup.Global>.Parse(this.itemTags);
			properties.ParseString(ActionBaseItemAction.PropFullCount, ref this.countText);
			properties.ParseEnum<ActionBaseItemAction.CountTypes>(ActionBaseItemAction.PropCountType, ref this.countType);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public List<ActionBaseItemAction.ItemLocations> itemLocations = new List<ActionBaseItemAction.ItemLocations>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public string itemTags = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public FastTags<TagGroup.Global> fastItemTags = FastTags<TagGroup.Global>.none;

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionBaseItemAction.CountTypes countType;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool isFinished;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int count = -1;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string countText = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropItemLocation = "items_location";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropItemTag = "items_tags";

		public static string PropFullCount = "count";

		public static string PropCountType = "count_type";

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum ItemLocations
		{
			Toolbelt,
			Backpack,
			Equipment,
			BiomeBadge,
			Held
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum CountTypes
		{
			Items,
			Slots
		}
	}
}
