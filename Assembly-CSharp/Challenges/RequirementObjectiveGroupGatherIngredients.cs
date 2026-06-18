using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class RequirementObjectiveGroupGatherIngredients : BaseRequirementObjectiveGroup
	{
		public RequirementObjectiveGroupGatherIngredients(string itemID)
		{
			this.ItemID = itemID;
			this.itemRecipe = CraftingManager.GetRecipe(itemID);
		}

		public override void CreateRequirements()
		{
			if (this.PhaseList == null)
			{
				this.PhaseList = new List<RequirementGroupPhase>();
			}
			RequirementGroupPhase requirementGroupPhase = new RequirementGroupPhase();
			int num = (this.itemRecipe.GetOutputItemClass().HasQuality ? 1 : 0);
			for (int i = 0; i < this.itemRecipe.ingredients.Count; i++)
			{
				int num2 = this.itemRecipe.ingredients[i].count;
				if (this.itemRecipe.UseIngredientModifier)
				{
					num2 = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, (float)num2, this.Owner.Owner.Player, this.itemRecipe, FastTags<TagGroup.Global>.Parse(this.itemRecipe.ingredients[i].itemValue.ItemClass.GetItemName()), true, true, true, true, true, num, true, false);
					if (num2 > 0)
					{
						num2 = (int)((float)num2 * XUiM_Recipes.GetCraftingInputModifier(this.itemRecipe));
					}
				}
				if (num2 != 0)
				{
					ChallengeObjectiveGatherIngredient challengeObjectiveGatherIngredient = new ChallengeObjectiveGatherIngredient();
					challengeObjectiveGatherIngredient.Parent = this;
					challengeObjectiveGatherIngredient.Owner = this.Owner;
					challengeObjectiveGatherIngredient.IsRequirement = true;
					challengeObjectiveGatherIngredient.itemRecipe = this.itemRecipe;
					challengeObjectiveGatherIngredient.IngredientIndex = i;
					challengeObjectiveGatherIngredient.IngredientCount = num2;
					challengeObjectiveGatherIngredient.NeededCount = ((this.CraftObj == null) ? 1 : this.CraftObj.MaxCount);
					challengeObjectiveGatherIngredient.MaxCount = num2 * challengeObjectiveGatherIngredient.NeededCount;
					challengeObjectiveGatherIngredient.Init();
					requirementGroupPhase.AddChallengeObjective(challengeObjectiveGatherIngredient);
				}
			}
			this.PhaseList.Add(requirementGroupPhase);
		}

		public override bool HasPrerequisiteCondition()
		{
			return true;
		}

		public override BaseRequirementObjectiveGroup Clone()
		{
			return new RequirementObjectiveGroupGatherIngredients(this.ItemID);
		}

		public string ItemID = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public Recipe itemRecipe;

		public ChallengeObjectiveCraft CraftObj;
	}
}
