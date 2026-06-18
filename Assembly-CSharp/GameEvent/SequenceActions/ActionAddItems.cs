using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionAddItems : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			EntityPlayerLocal entityPlayerLocal = target as EntityPlayerLocal;
			if (entityPlayerLocal != null)
			{
				for (int i = 0; i < this.AddItems.Length; i++)
				{
					string text = ((this.AddItemCounts != null && this.AddItemCounts.Length > i) ? this.AddItemCounts[i] : "1");
					int num = 1;
					ItemClass itemClass = ItemClass.GetItemClass(this.AddItems[i], false);
					num = GameEventManager.GetIntValue(entityPlayerLocal, text, num);
					ItemValue itemValue;
					if (itemClass.HasQuality)
					{
						itemValue = new ItemValue(itemClass.Id, num, num, false, null, 1f);
						num = 1;
					}
					else
					{
						itemValue = new ItemValue(itemClass.Id, false);
					}
					ItemStack itemStack = new ItemStack(itemValue, num);
					if (!LocalPlayerUI.GetUIForPlayer(entityPlayerLocal).xui.PlayerInventory.AddItem(itemStack))
					{
						GameManager.Instance.ItemDropServer(itemStack, entityPlayerLocal.GetPosition(), Vector3.zero, -1, 60f, false);
					}
				}
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			if (!properties.Values.ContainsKey(ActionAddItems.PropAddItems))
			{
				this.AddItems = null;
				this.AddItemCounts = null;
				return;
			}
			this.AddItems = properties.Values[ActionAddItems.PropAddItems].Replace(" ", "").Split(',', StringSplitOptions.None);
			if (properties.Values.ContainsKey(ActionAddItems.PropAddItemCounts))
			{
				this.AddItemCounts = properties.Values[ActionAddItems.PropAddItemCounts].Replace(" ", "").Split(',', StringSplitOptions.None);
				return;
			}
			this.AddItemCounts = null;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionAddItems
			{
				AddItems = this.AddItems,
				AddItemCounts = this.AddItemCounts,
				targetGroup = this.targetGroup
			};
		}

		public string[] AddItems;

		public string[] AddItemCounts;

		public static string PropAddItems = "added_items";

		public static string PropAddItemCounts = "added_item_counts";
	}
}
