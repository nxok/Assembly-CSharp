using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class RequirementObjectiveGroupPlace : BaseRequirementObjectiveGroup
	{
		public RequirementObjectiveGroupPlace(string itemID)
		{
			this.ItemID = itemID;
		}

		public override void CreateRequirements()
		{
			if (this.PhaseList == null)
			{
				this.PhaseList = new List<RequirementGroupPhase>();
			}
			this.PhaseList.Add(this.AddIngredientGatheringReqs());
			RequirementGroupPhase requirementGroupPhase = new RequirementGroupPhase();
			ChallengeObjectiveCraft challengeObjectiveCraft = new ChallengeObjectiveCraft();
			challengeObjectiveCraft.Owner = this.Owner;
			challengeObjectiveCraft.SetupItem(this.ItemID);
			challengeObjectiveCraft.IsRequirement = true;
			challengeObjectiveCraft.MaxCount = 1;
			challengeObjectiveCraft.Init();
			requirementGroupPhase.AddChallengeObjective(challengeObjectiveCraft);
			this.PhaseList.Add(requirementGroupPhase);
			requirementGroupPhase = new RequirementGroupPhase();
			ChallengeObjectiveHold challengeObjectiveHold = new ChallengeObjectiveHold();
			challengeObjectiveHold.Owner = this.Owner;
			challengeObjectiveHold.itemClassID = this.ItemID;
			challengeObjectiveHold.IsRequirement = true;
			challengeObjectiveHold.MaxCount = 1;
			challengeObjectiveHold.Init();
			requirementGroupPhase.AddChallengeObjective(challengeObjectiveHold);
			this.PhaseList.Add(requirementGroupPhase);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public RequirementGroupPhase AddIngredientGatheringReqs()
		{
			Recipe recipe = CraftingManager.GetRecipe(this.ItemID);
			RequirementGroupPhase requirementGroupPhase = new RequirementGroupPhase();
			int num = (recipe.GetOutputItemClass().HasQuality ? 1 : 0);
			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				int num2 = recipe.ingredients[i].count;
				if (recipe.UseIngredientModifier)
				{
					num2 = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, (float)num2, this.Owner.Owner.Player, recipe, FastTags<TagGroup.Global>.Parse(recipe.ingredients[i].itemValue.ItemClass.GetItemName()), true, true, true, true, true, num, true, false);
					if (num2 > 0)
					{
						num2 = (int)((float)num2 * XUiM_Recipes.GetCraftingInputModifier(recipe));
					}
				}
				if (num2 != 0)
				{
					ChallengeObjectiveGatherIngredient challengeObjectiveGatherIngredient = new ChallengeObjectiveGatherIngredient();
					challengeObjectiveGatherIngredient.Owner = this.Owner;
					challengeObjectiveGatherIngredient.Parent = this;
					challengeObjectiveGatherIngredient.IsRequirement = true;
					challengeObjectiveGatherIngredient.itemRecipe = recipe;
					challengeObjectiveGatherIngredient.IngredientIndex = i;
					challengeObjectiveGatherIngredient.IngredientCount = num2;
					challengeObjectiveGatherIngredient.NeededCount = 1;
					challengeObjectiveGatherIngredient.MaxCount = num2;
					challengeObjectiveGatherIngredient.Init();
					requirementGroupPhase.AddChallengeObjective(challengeObjectiveGatherIngredient);
				}
			}
			return requirementGroupPhase;
		}

		public override bool HasPrerequisiteCondition()
		{
			EntityPlayerLocal player = this.Owner.Owner.Player;
			LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(this.Owner.Owner.Player);
			XUiM_PlayerInventory playerInventory = uiforPlayer.xui.PlayerInventory;
			ItemClass holdingItem = this.Owner.Owner.Player.inventory.holdingItem;
			return (!playerInventory.HasItem(ItemClass.GetItem(this.ItemID, false)) && !this.CheckDragDropItem(uiforPlayer.xui.DragAndDropWindow.CurrentStack, this.ItemID)) || holdingItem.Name != this.ItemID;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool CheckDragDropItem(ItemStack stack, string itemID)
		{
			return !stack.IsEmpty() && stack.itemValue.ItemClass.GetItemName() == itemID;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override bool CheckPhaseStatus(int index)
		{
			EntityPlayerLocal player = this.Owner.Owner.Player;
			LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(this.Owner.Owner.Player);
			XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(player).xui.PlayerInventory;
			ItemClass holdingItem = this.Owner.Owner.Player.inventory.holdingItem;
			if (playerInventory.HasItem(ItemClass.GetItem(this.ItemID, false)) && holdingItem.Name == this.ItemID)
			{
				return false;
			}
			if (index > 1)
			{
				return index != 2 || ((playerInventory.HasItem(ItemClass.GetItem(this.ItemID, false)) || this.CheckDragDropItem(uiforPlayer.xui.DragAndDropWindow.CurrentStack, this.ItemID)) && holdingItem.Name != this.ItemID);
			}
			return !playerInventory.HasItem(ItemClass.GetItem(this.ItemID, false)) && !this.CheckDragDropItem(uiforPlayer.xui.DragAndDropWindow.CurrentStack, this.ItemID);
		}

		public override BaseRequirementObjectiveGroup Clone()
		{
			return new RequirementObjectiveGroupPlace(this.ItemID);
		}

		public string ItemID = "";
	}
}
