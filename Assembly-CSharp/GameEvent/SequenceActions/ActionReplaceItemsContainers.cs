using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionReplaceItemsContainers : ActionBaseContainersAction
	{
		public override bool CheckValidTileEntity(TileEntity te, out bool isEmpty)
		{
			isEmpty = true;
			TileEntityType tileEntityType = te.GetTileEntityType();
			if (tileEntityType != TileEntityType.Workstation)
			{
				ITileEntityLootable tileEntityLootable;
				if (tileEntityType == TileEntityType.Composite && te.TryGetSelfOrFeature(out tileEntityLootable))
				{
					for (int i = 0; i < tileEntityLootable.items.Length; i++)
					{
						ItemStack itemStack = tileEntityLootable.items[i];
						if (!itemStack.IsEmpty() && itemStack.itemValue.ItemClass.HasAnyTags(this.fastItemTags) && itemStack.itemValue.ItemClass.GetItemName() != this.ReplacedByItem)
						{
							isEmpty = false;
						}
					}
					return true;
				}
			}
			else if (this.includeOutputs)
			{
				TileEntityWorkstation tileEntityWorkstation = te as TileEntityWorkstation;
				if (tileEntityWorkstation != null)
				{
					foreach (ItemStack itemStack2 in tileEntityWorkstation.Output)
					{
						if (!itemStack2.IsEmpty() && itemStack2.itemValue.ItemClass.HasAnyTags(this.fastItemTags) && itemStack2.itemValue.ItemClass.GetItemName() != this.ReplacedByItem)
						{
							isEmpty = false;
						}
					}
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override bool HandleContainerAction(List<TileEntity> tileEntityList)
		{
			new List<ItemStack>();
			List<TileEntity> list = new List<TileEntity>();
			bool flag = false;
			for (int i = 0; i < tileEntityList.Count; i++)
			{
				bool flag2 = false;
				TileEntityType tileEntityType = tileEntityList[i].GetTileEntityType();
				if (tileEntityType != TileEntityType.Workstation)
				{
					ITileEntityLootable tileEntityLootable;
					if (tileEntityType == TileEntityType.Composite && tileEntityList[i].TryGetSelfOrFeature(out tileEntityLootable))
					{
						for (int j = 0; j < tileEntityLootable.items.Length; j++)
						{
							ItemStack itemStack = tileEntityLootable.items[j];
							if (!itemStack.IsEmpty() && itemStack.itemValue.ItemClass.HasAnyTags(this.fastItemTags) && itemStack.itemValue.ItemClass.GetItemName() != this.ReplacedByItem)
							{
								tileEntityLootable.items[j] = new ItemStack(ItemClass.GetItem(this.ReplacedByItem, false), itemStack.count);
								flag = true;
								flag2 = true;
							}
						}
					}
				}
				else if (this.includeOutputs)
				{
					TileEntityWorkstation tileEntityWorkstation = tileEntityList[i] as TileEntityWorkstation;
					if (tileEntityWorkstation != null)
					{
						ItemStack[] output = tileEntityWorkstation.Output;
						for (int k = 0; k < output.Length; k++)
						{
							ItemStack itemStack2 = output[k];
							if (!itemStack2.IsEmpty() && itemStack2.itemValue.ItemClass.HasAnyTags(this.fastItemTags) && itemStack2.itemValue.ItemClass.GetItemName() != this.ReplacedByItem)
							{
								output[k] = new ItemStack(ItemClass.GetItem(this.ReplacedByItem, false), itemStack2.count);
								flag = true;
								flag2 = true;
							}
						}
						if (flag2)
						{
							tileEntityWorkstation.Output = output;
							list.Add(tileEntityWorkstation);
						}
					}
				}
				if (flag2)
				{
					tileEntityList[i].SetModified();
				}
			}
			if (flag && this.changeName && base.Owner.Target != null)
			{
				PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(base.Owner.Target.entityId);
				for (int l = 0; l < tileEntityList.Count; l++)
				{
					ITileEntitySignable tileEntitySignable;
					if (tileEntityList[l].TryGetSelfOrFeature(out tileEntitySignable))
					{
						tileEntitySignable.SetText(base.ModifiedName, true, (playerDataFromEntityID != null) ? playerDataFromEntityID.PrimaryId : null);
					}
				}
			}
			return flag;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseBool(ActionReplaceItemsContainers.PropIncludeOutputs, ref this.includeOutputs);
			properties.ParseString(ActionReplaceItemsContainers.PropReplacedByItem, ref this.ReplacedByItem);
			properties.ParseString(ActionReplaceItemsContainers.PropItemTag, ref this.itemTags);
			this.fastItemTags = FastTags<TagGroup.Global>.Parse(this.itemTags);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionReplaceItemsContainers
			{
				TargetingType = this.TargetingType,
				maxDistance = this.maxDistance,
				newName = this.newName,
				changeName = this.changeName,
				includeOutputs = this.includeOutputs,
				tileEntityList = this.tileEntityList,
				fastItemTags = this.fastItemTags,
				ReplacedByItem = this.ReplacedByItem
			};
		}

		public string ReplacedByItem = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool includeOutputs;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string itemTags = "";

		public static string PropReplacedByItem = "replaced_by_item";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropIncludeOutputs = "include_outputs";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropItemTag = "items_tags";

		[PublicizedFrom(EAccessModifier.Protected)]
		public FastTags<TagGroup.Global> fastItemTags = FastTags<TagGroup.Global>.none;
	}
}
