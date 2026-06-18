using System;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryFavorite : BaseItemActionEntry
{
	public ItemActionEntryFavorite(XUiController _controller, Recipe _recipe)
		: base(_controller, "lblContextActionFavorite", "server_favorite", BaseItemActionEntry.GamepadShortCut.DPadRight, "crafting/craft_click_craft", "ui/ui_denied")
	{
		this.recipe = _recipe;
	}

	public override void OnActivated()
	{
		XUiC_RecipeEntry xuiC_RecipeEntry = (XUiC_RecipeEntry)base.ItemController;
		if (((xuiC_RecipeEntry != null) ? xuiC_RecipeEntry.Recipe : null) == null)
		{
			if (this.recipe != null)
			{
				CraftingManager.ToggleFavoriteRecipe(this.recipe);
			}
		}
		else
		{
			CraftingManager.ToggleFavoriteRecipe(xuiC_RecipeEntry.Recipe);
		}
		if (xuiC_RecipeEntry != null)
		{
			XUiC_RecipeList childByType = xuiC_RecipeEntry.WindowGroup.Controller.GetChildByType<XUiC_RecipeList>();
			if (childByType == null)
			{
				return;
			}
			childByType.RefreshCurrentRecipes();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Recipe recipe;
}
