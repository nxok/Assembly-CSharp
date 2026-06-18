using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class RequirementObjectiveGroupCraft : BaseRequirementObjectiveGroup
	{
		public RequirementObjectiveGroupCraft(string itemID)
		{
			this.ItemID = itemID;
			this.ItemRecipe = CraftingManager.GetRecipe(itemID);
		}

		public override void CreateRequirements()
		{
			if (this.PhaseList == null)
			{
				this.PhaseList = new List<RequirementGroupPhase>();
			}
			RequirementGroupPhase requirementGroupPhase = new RequirementGroupPhase();
			ChallengeObjectiveCraft challengeObjectiveCraft = new ChallengeObjectiveCraft();
			challengeObjectiveCraft.Owner = this.Owner;
			challengeObjectiveCraft.SetupItem(this.ItemID);
			challengeObjectiveCraft.IsRequirement = true;
			challengeObjectiveCraft.MaxCount = 1;
			challengeObjectiveCraft.Init();
			requirementGroupPhase.AddChallengeObjective(challengeObjectiveCraft);
			this.PhaseList.Add(requirementGroupPhase);
		}

		public override bool HasPrerequisiteCondition()
		{
			EntityPlayerLocal player = this.Owner.Owner.Player;
			LocalPlayerUI.GetUIForPlayer(this.Owner.Owner.Player);
			XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(player).xui.PlayerInventory;
			int num = (this.ItemRecipe.GetOutputItemClass().HasQuality ? 1 : 0);
			for (int i = 0; i < this.ItemRecipe.ingredients.Count; i++)
			{
				ItemStack itemStack = this.ItemRecipe.ingredients[i].Clone();
				if (this.ItemRecipe.UseIngredientModifier)
				{
					itemStack.count = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, (float)itemStack.count, this.Owner.Owner.Player, this.ItemRecipe, FastTags<TagGroup.Global>.Parse(itemStack.itemValue.ItemClass.GetItemName()), true, true, true, true, true, num, true, false);
					if (itemStack.count > 0)
					{
						itemStack.count = (int)((float)itemStack.count * XUiM_Recipes.GetCraftingInputModifier(this.ItemRecipe));
					}
				}
				if (itemStack.count != 0 && !playerInventory.HasItem(itemStack))
				{
					return false;
				}
			}
			return true;
		}

		public override BaseRequirementObjectiveGroup Clone()
		{
			return new RequirementObjectiveGroupCraft(this.ItemID)
			{
				ItemRecipe = this.ItemRecipe
			};
		}

		public string ItemID = "";

		public Recipe ItemRecipe;
	}
}
