using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionDropItems : ActionBaseItemAction
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnClientActionStarted(EntityPlayer player)
		{
			this.droppedItems = new List<ItemStack>();
			this.replaceItemTag = ((this.itemTags == "") ? FastTags<TagGroup.Global>.none : FastTags<TagGroup.Global>.Parse(this.itemTags));
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnClientActionEnded(EntityPlayer player)
		{
			if (this.droppedItems.Count > 0)
			{
				Vector3 dropPosition = player.GetDropPosition();
				GameManager.Instance.DropContentInLootContainerServer(player.entityId, "DroppedLootContainerTwitch", dropPosition, this.droppedItems.ToArray(), false, null);
				if (this.DropSound != "")
				{
					Manager.BroadcastPlay(player, this.DropSound, false, 1f);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override bool HandleItemStackChange(ref ItemStack stack, EntityPlayer player)
		{
			if (!stack.IsEmpty() && (this.replaceItemTag.IsEmpty || stack.itemValue.ItemClass.HasAnyTags(this.replaceItemTag)) && stack.itemValue.ItemClass.GetItemName() != this.ReplacedByItem)
			{
				if (this.count != -1)
				{
					if (this.countType == ActionBaseItemAction.CountTypes.Slots)
					{
						this.droppedItems.Add(stack.Clone());
						if (this.ReplacedByItem == "")
						{
							stack = ItemStack.Empty;
						}
						else
						{
							stack = new ItemStack(ItemClass.GetItem(this.ReplacedByItem, false), stack.count);
						}
						this.count--;
						if (this.count == 0)
						{
							this.isFinished = true;
						}
						return true;
					}
					if (stack.count > this.count)
					{
						ItemStack itemStack = stack.Clone();
						itemStack.count = this.count;
						this.droppedItems.Add(itemStack);
						stack.count -= this.count;
						if (this.ReplacedByItem != "")
						{
							ItemStack itemStack2 = new ItemStack(ItemClass.GetItem(this.ReplacedByItem, false), this.count);
							base.AddStack(player as EntityPlayerLocal, itemStack2);
						}
						this.count = 0;
						this.isFinished = true;
					}
					else
					{
						this.count -= stack.count;
						this.droppedItems.Add(stack.Clone());
						if (this.ReplacedByItem == "")
						{
							stack = ItemStack.Empty;
						}
						else
						{
							stack = new ItemStack(ItemClass.GetItem(this.ReplacedByItem, false), stack.count);
						}
					}
				}
				else
				{
					this.droppedItems.Add(stack.Clone());
					if (this.ReplacedByItem == "")
					{
						stack = ItemStack.Empty;
					}
					else
					{
						stack = new ItemStack(ItemClass.GetItem(this.ReplacedByItem, false), stack.count);
					}
				}
				return true;
			}
			return false;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionDropItems.PropReplacedByItem, ref this.ReplacedByItem);
			properties.ParseString(ActionDropItems.PropDropSound, ref this.DropSound);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionDropItems
			{
				ReplacedByItem = this.ReplacedByItem,
				DropSound = this.DropSound
			};
		}

		public string ReplacedByItem = "";

		public string DropSound = "";

		public static string PropReplacedByItem = "replaced_by_item";

		public static string PropDropSound = "drop_sound";

		[PublicizedFrom(EAccessModifier.Private)]
		public List<ItemStack> droppedItems;

		[PublicizedFrom(EAccessModifier.Private)]
		public FastTags<TagGroup.Global> replaceItemTag = FastTags<TagGroup.Global>.none;
	}
}
