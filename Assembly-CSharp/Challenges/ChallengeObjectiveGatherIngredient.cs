using System;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveGatherIngredient : ChallengeBaseTrackedItemObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.GatherIngredient;
			}
		}

		public override string DescriptionText
		{
			get
			{
				return Localization.Get("challengeObjectiveGather", false, null) + " " + this.expectedItemClass.GetLocalizedItemName();
			}
		}

		public override string StatusText
		{
			get
			{
				int num = Math.Max(0, this.MaxCount - this.currentNeededCount);
				if (base.Complete)
				{
					return string.Format("{0}/{1}", num, num);
				}
				return string.Format("{0}/{1}", this.current, num);
			}
		}

		public override void Init()
		{
			this.expectedItem = this.itemRecipe.ingredients[this.IngredientIndex].itemValue;
			this.expectedItemClass = this.expectedItem.ItemClass;
		}

		public override void HandleAddHooks()
		{
			EntityPlayerLocal player = this.Owner.Owner.Player;
			LocalPlayerUI.GetUIForPlayer(this.Owner.Owner.Player);
			XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(player).xui.PlayerInventory;
			playerInventory.Backpack.OnBackpackItemsChangedInternal -= this.ItemsChangedInternal;
			playerInventory.Toolbelt.OnToolbeltItemsChangedInternal -= this.ItemsChangedInternal;
			playerInventory.Backpack.OnBackpackItemsChangedInternal += this.ItemsChangedInternal;
			playerInventory.Toolbelt.OnToolbeltItemsChangedInternal += this.ItemsChangedInternal;
			player.DragAndDropItemChanged += this.ItemsChangedInternal;
			base.HandleAddHooks();
			if (this.trackingEntry != null)
			{
				this.Owner.AddTrackingEntry(this.trackingEntry);
				this.trackingEntry.TrackingHelper = this.Owner.TrackingHandler;
				this.trackingEntry.AddHooks();
			}
		}

		public override bool CheckObjectiveComplete(bool handleComplete = true)
		{
			if (this.CheckForNeededItem())
			{
				base.Current = this.MaxCount;
				base.Complete = true;
				if (handleComplete)
				{
					this.Owner.HandleComplete(true);
				}
				return true;
			}
			base.Complete = false;
			return base.CheckObjectiveComplete(handleComplete);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ItemsChangedInternal()
		{
			if (this.CheckObjectiveComplete(true))
			{
				if (this.trackingEntry != null)
				{
					this.trackingEntry.RemoveHooks();
				}
				this.Parent.CheckPrerequisites();
				return;
			}
			if (this.trackingEntry != null)
			{
				this.trackingEntry.AddHooks();
			}
		}

		public override void UpdateStatus()
		{
			base.UpdateStatus();
			if (base.Complete)
			{
				if (this.trackingEntry != null)
				{
					this.trackingEntry.RemoveHooks();
					return;
				}
			}
			else if (this.trackingEntry != null)
			{
				this.trackingEntry.AddHooks();
			}
		}

		public override void HandleRemoveHooks()
		{
			EntityPlayerLocal player = this.Owner.Owner.Player;
			if (player == null)
			{
				return;
			}
			LocalPlayerUI.GetUIForPlayer(player);
			XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(player).xui.PlayerInventory;
			playerInventory.Backpack.OnBackpackItemsChangedInternal -= this.ItemsChangedInternal;
			playerInventory.Toolbelt.OnToolbeltItemsChangedInternal -= this.ItemsChangedInternal;
			player.DragAndDropItemChanged -= this.ItemsChangedInternal;
			if (this.trackingEntry != null)
			{
				this.trackingEntry.RemoveHooks();
				this.Owner.RemoveTrackingEntry(this.trackingEntry);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleUpdatingCurrent()
		{
			base.HandleUpdatingCurrent();
			int num = this.itemRecipe.ingredients[this.IngredientIndex].count;
			ItemValue itemValue = new ItemValue(this.itemRecipe.itemValueType, false);
			if (this.itemRecipe.UseIngredientModifier)
			{
				num = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, (float)num, this.Owner.Owner.Player, this.itemRecipe, FastTags<TagGroup.Global>.Parse(this.expectedItemClass.GetItemName()), true, true, true, true, true, itemValue.HasQuality ? 1 : 0, true, false);
				if (num > 0)
				{
					num = (int)((float)num * XUiM_Recipes.GetCraftingInputModifier(this.itemRecipe));
				}
			}
			LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(this.Owner.Owner.Player);
			CraftingData craftingData = uiforPlayer.xui.GetCraftingData();
			XUiM_PlayerInventory playerInventory = uiforPlayer.xui.PlayerInventory;
			RecipeQueueItem[] recipeQueueItems = craftingData.RecipeQueueItems;
			int num2 = 0;
			if (recipeQueueItems != null)
			{
				foreach (RecipeQueueItem recipeQueueItem in recipeQueueItems)
				{
					if (recipeQueueItem.Recipe != null && recipeQueueItem.Recipe.itemValueType == this.itemRecipe.itemValueType)
					{
						num2 += recipeQueueItem.Recipe.count * (int)recipeQueueItem.Multiplier;
					}
				}
			}
			num2 += playerInventory.Backpack.GetItemCount(itemValue, -1, -1, true);
			num2 += playerInventory.Toolbelt.GetItemCount(itemValue, false, -1, -1, true);
			int num3 = this.IngredientCount * Math.Max(0, this.NeededCount - num2);
			int num4 = playerInventory.Backpack.GetItemCount(this.expectedItem, -1, -1, true);
			num4 += playerInventory.Toolbelt.GetItemCount(this.expectedItem, false, -1, -1, true);
			if (num4 > num3)
			{
				num4 = num3;
			}
			if (this.current != num4)
			{
				base.Current = num4;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool CheckForNeededItem()
		{
			LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(this.Owner.Owner.Player);
			XUiM_PlayerInventory playerInventory = uiforPlayer.xui.PlayerInventory;
			ItemValue itemValue = new ItemValue(this.itemRecipe.itemValueType, false);
			RecipeQueueItem[] recipeQueueItems = uiforPlayer.xui.GetCraftingData().RecipeQueueItems;
			int num = playerInventory.Backpack.GetItemCount(this.expectedItem, -1, -1, true);
			num += playerInventory.Toolbelt.GetItemCount(this.expectedItem, false, -1, -1, true);
			this.currentNeededCount = 0;
			this.currentNeededCount = playerInventory.Backpack.GetItemCount(itemValue, -1, -1, true);
			this.currentNeededCount += playerInventory.Toolbelt.GetItemCount(itemValue, false, -1, -1, true);
			int num2 = 0;
			if (recipeQueueItems != null)
			{
				foreach (RecipeQueueItem recipeQueueItem in recipeQueueItems)
				{
					if (recipeQueueItem.Recipe != null && recipeQueueItem.Recipe.itemValueType == this.itemRecipe.itemValueType)
					{
						num2 += recipeQueueItem.Recipe.count * (int)recipeQueueItem.Multiplier;
					}
				}
			}
			return num >= this.IngredientCount * Math.Max(0, this.NeededCount - (this.currentNeededCount + num2));
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveGatherIngredient
			{
				itemRecipe = this.itemRecipe,
				IngredientIndex = this.IngredientIndex,
				expectedItem = this.expectedItem,
				expectedItemClass = this.expectedItemClass,
				NeededCount = this.NeededCount
			};
		}

		public Recipe itemRecipe;

		public int IngredientIndex = -1;

		public int IngredientCount = -1;

		public int NeededCount;

		public int currentNeededCount;

		public BaseRequirementObjectiveGroup Parent;
	}
}
