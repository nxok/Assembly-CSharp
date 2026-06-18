using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_RecipeTrackerIngredientsList : XUiController
{
	public Recipe Recipe
	{
		get
		{
			return this.recipe;
		}
		set
		{
			this.recipe = value;
			Recipe recipe = this.recipe;
			this.selectedCraftingTier = ((recipe != null) ? recipe.craftingTier : (-1));
			this.IsDirty = true;
			this.firstSetup = true;
		}
	}

	public int Count
	{
		get
		{
			return this.count;
		}
		set
		{
			this.count = value;
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_RecipeTrackerIngredientEntry[] childrenByType = base.GetChildrenByType<XUiC_RecipeTrackerIngredientEntry>(null);
		for (int i = 0; i < childrenByType.Length; i++)
		{
			if (childrenByType[i] != null)
			{
				childrenByType[i].Owner = this;
				this.ingredientEntries.Add(childrenByType[i]);
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.xui.PlayerInventory.OnBackpackItemsChanged += this.PlayerInventory_OnBackpackItemsChanged;
		this.xui.PlayerInventory.OnToolbeltItemsChanged += this.PlayerInventory_OnToolbeltItemsChanged;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.PlayerInventory.OnBackpackItemsChanged -= this.PlayerInventory_OnBackpackItemsChanged;
		this.xui.PlayerInventory.OnToolbeltItemsChanged -= this.PlayerInventory_OnToolbeltItemsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnToolbeltItemsChanged()
	{
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnBackpackItemsChanged()
	{
		this.IsDirty = true;
	}

	public override void Update(float _dt)
	{
		if (this.IsDirty)
		{
			if (this.recipe != null)
			{
				bool flag = true;
				int num = this.ingredientEntries.Count;
				int num2 = this.recipe.ingredients.Count;
				int num3 = ((this.selectedCraftingTier == -1) ? this.recipe.GetCraftingTier(this.xui.playerUI.entityPlayer) : this.selectedCraftingTier);
				for (int i = 0; i < num; i++)
				{
					XUiC_RecipeTrackerIngredientEntry xuiC_RecipeTrackerIngredientEntry = this.ingredientEntries[i];
					ItemStack itemStack = ((i < num2) ? this.recipe.ingredients[i].Clone() : null);
					if (itemStack != null && this.recipe.UseIngredientModifier)
					{
						itemStack.count = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, (float)itemStack.count, this.xui.playerUI.entityPlayer, this.recipe, FastTags<TagGroup.Global>.Parse(itemStack.itemValue.ItemClass.GetItemName()), true, true, true, true, true, num3, true, false);
						if (itemStack.count > 0)
						{
							itemStack.count = (int)((float)itemStack.count * XUiM_Recipes.GetCraftingInputModifier(this.recipe));
						}
					}
					if (itemStack == null || (itemStack != null && itemStack.count > 0))
					{
						xuiC_RecipeTrackerIngredientEntry.Ingredient = itemStack;
					}
					else
					{
						xuiC_RecipeTrackerIngredientEntry.Ingredient = null;
					}
					if (xuiC_RecipeTrackerIngredientEntry.Ingredient != null && !xuiC_RecipeTrackerIngredientEntry.IsComplete)
					{
						flag = false;
					}
				}
				if (this.firstSetup)
				{
					this.lastComplete = flag;
					this.firstSetup = false;
				}
				if (flag && !this.lastComplete)
				{
					GameManager.ShowTooltip(this.xui.playerUI.entityPlayer, "ttAllIngredientsFound", Localization.Get(this.recipe.GetName(), false, null), null, null, false, false, 0f);
					this.lastComplete = flag;
					this.IsDirty = false;
					base.Update(_dt);
					return;
				}
				this.lastComplete = flag;
			}
			else
			{
				int num4 = this.ingredientEntries.Count;
				for (int j = 0; j < num4; j++)
				{
					this.ingredientEntries[j].Ingredient = null;
				}
			}
			this.IsDirty = false;
		}
		base.Update(_dt);
	}

	public int GetActiveIngredientCount()
	{
		if (this.recipe == null)
		{
			return 0;
		}
		int num = ((this.selectedCraftingTier == -1) ? this.recipe.GetCraftingTier(this.xui.playerUI.entityPlayer) : this.selectedCraftingTier);
		int num2 = 0;
		for (int i = 0; i < this.recipe.ingredients.Count; i++)
		{
			ItemStack itemStack = this.recipe.ingredients[i];
			if (this.recipe.UseIngredientModifier)
			{
				int num3 = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, (float)itemStack.count, this.xui.playerUI.entityPlayer, this.recipe, FastTags<TagGroup.Global>.Parse(itemStack.itemValue.ItemClass.GetItemName()), true, true, true, true, true, num, true, false);
				if (num3 > 0)
				{
					num3 = (int)((float)num3 * XUiM_Recipes.GetCraftingInputModifier(this.recipe));
				}
				if (num3 > 0)
				{
					num2++;
				}
			}
		}
		return num2;
	}

	public override bool ParseAttribute(string name, string value)
	{
		if (name == "complete_icon")
		{
			this.completeIconName = value;
			return true;
		}
		if (name == "incomplete_icon")
		{
			this.incompleteIconName = value;
			return true;
		}
		if (name == "complete_color")
		{
			Color32 color = StringParsers.ParseColor(value);
			this.completeColor = string.Format("{0},{1},{2},{3}", new object[] { color.r, color.g, color.b, color.a });
			this.completeHexColor = Utils.ColorToHex(color);
			return true;
		}
		if (!(name == "incomplete_color"))
		{
			return base.ParseAttribute(name, value);
		}
		Color32 color2 = StringParsers.ParseColor(value);
		this.incompleteColor = string.Format("{0},{1},{2},{3}", new object[] { color2.r, color2.g, color2.b, color2.a });
		this.incompleteHexColor = Utils.ColorToHex(color2);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe recipe;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selectedCraftingTier;

	[PublicizedFrom(EAccessModifier.Private)]
	public int count = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_RecipeTrackerIngredientEntry> ingredientEntries = new List<XUiC_RecipeTrackerIngredientEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstSetup;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastComplete;

	public string completeIconName = "";

	public string incompleteIconName = "";

	public string completeHexColor = "FF00FF00";

	public string incompleteHexColor = "FFB400";

	public string warningHexColor = "FFFF00FF";

	public string inactiveHexColor = "888888FF";

	public string activeHexColor = "FFFFFFFF";

	public string completeColor = "0,255,0,255";

	public string incompleteColor = "255, 180, 0, 255";

	public string warningColor = "255,255,0,255";
}
