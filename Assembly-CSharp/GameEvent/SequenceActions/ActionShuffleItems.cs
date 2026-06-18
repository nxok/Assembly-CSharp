using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionShuffleItems : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			EntityPlayer entityPlayer = target as EntityPlayer;
			if (entityPlayer != null)
			{
				GameManager.Instance.StartCoroutine(this.handleShuffle(entityPlayer));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator handleShuffle(EntityPlayer player)
		{
			while (player.inventory.IsHoldingItemActionRunning() || !player.inventory.GetItem(player.inventory.DUMMY_SLOT_IDX).IsEmpty())
			{
				yield return new WaitForSeconds(0.25f);
			}
			List<ItemStack> list = new List<ItemStack>();
			if (this.itemLocations.Contains(ActionShuffleItems.ItemLocations.Toolbelt))
			{
				ItemStack[] array = ((player.AttachedToEntity != null && player.saveInventory != null) ? player.saveInventory.GetSlots() : player.inventory.GetSlots());
				list.AddRange(array);
				list.RemoveAt(list.Count - 1);
			}
			if (this.itemLocations.Contains(ActionShuffleItems.ItemLocations.Backpack))
			{
				ItemStack[] slots = player.bag.GetSlots();
				list.AddRange(slots);
			}
			GameRandom random = GameEventManager.Current.Random;
			for (int i = 0; i < list.Count * 2; i++)
			{
				int num = random.RandomRange(list.Count);
				int num2 = random.RandomRange(list.Count);
				ItemStack itemStack = list[num];
				list[num] = list[num2];
				list[num2] = itemStack;
			}
			int num3 = 0;
			if (this.itemLocations.Contains(ActionShuffleItems.ItemLocations.Toolbelt))
			{
				Inventory inventory = ((player.saveInventory != null) ? player.saveInventory : player.inventory);
				ItemStack[] slots2 = inventory.GetSlots();
				for (int j = 0; j < slots2.Length - 1; j++)
				{
					slots2[j] = list[num3++];
				}
				inventory.SetSlots(slots2, true);
				player.bPlayerStatsChanged = true;
			}
			if (this.itemLocations.Contains(ActionShuffleItems.ItemLocations.Backpack))
			{
				ItemStack[] slots3 = player.bag.GetSlots();
				for (int k = 0; k < slots3.Length; k++)
				{
					slots3[k] = list[num3++];
				}
				player.bag.SetSlots(slots3);
				player.bPlayerStatsChanged = true;
			}
			yield break;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			ActionShuffleItems.ItemLocations itemLocations = ActionShuffleItems.ItemLocations.Toolbelt;
			if (properties.Values.ContainsKey(ActionShuffleItems.PropItemLocation))
			{
				string[] array = properties.Values[ActionShuffleItems.PropItemLocation].Split(',', StringSplitOptions.None);
				for (int i = 0; i < array.Length; i++)
				{
					if (Enum.TryParse<ActionShuffleItems.ItemLocations>(array[i], true, out itemLocations))
					{
						this.itemLocations.Add(itemLocations);
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionShuffleItems
			{
				itemLocations = this.itemLocations,
				targetGroup = this.targetGroup
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public List<ActionShuffleItems.ItemLocations> itemLocations = new List<ActionShuffleItems.ItemLocations>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropItemLocation = "items_location";

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum ItemLocations
		{
			Toolbelt,
			Backpack
		}
	}
}
