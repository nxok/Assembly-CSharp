using System;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryTrackRecipe : BaseItemActionEntry
{
	public ItemActionEntryTrackRecipe(XUiController _controller, int _craftingTier)
		: base(_controller, "lblContextActionTrack", "ui_game_symbol_compass", BaseItemActionEntry.GamepadShortCut.DPadLeft, "crafting/craft_click_craft", "ui/ui_denied")
	{
		Recipe recipe = ((XUiC_RecipeEntry)base.ItemController).Recipe;
		recipe.craftingTier = _craftingTier;
		this.selectedCraftingTier = _craftingTier;
	}

	public override void OnActivated()
	{
		Recipe recipe = ((XUiC_RecipeEntry)base.ItemController).Recipe;
		recipe.craftingTier = this.selectedCraftingTier;
		XUi xui = base.ItemController.xui;
		EntityPlayerLocal entityPlayer = xui.playerUI.entityPlayer;
		if (xui.Recipes.TrackedRecipe != recipe || xui.Recipes.TrackedRecipeQuality != this.selectedCraftingTier)
		{
			if (xui.Recipes.TrackedRecipe == null)
			{
				xui.Recipes.SetPreviousTracked(entityPlayer);
			}
			xui.Recipes.TrackedRecipeQuality = this.selectedCraftingTier;
			xui.Recipes.TrackedRecipeCount = 1;
			xui.Recipes.TrackedRecipe = recipe;
			return;
		}
		xui.Recipes.TrackedRecipe = null;
		xui.Recipes.ResetToPreviousTracked(entityPlayer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int selectedCraftingTier;
}
