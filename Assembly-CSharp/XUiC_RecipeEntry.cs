using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_RecipeEntry : XUiC_SelectableEntry
{
	public bool HasIngredients
	{
		get
		{
			return this.hasIngredients;
		}
		set
		{
			this.hasIngredients = value;
			base.RefreshBindings();
		}
	}

	public bool IsCurrentWorkstation
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public Recipe Recipe
	{
		get
		{
			return this.recipe;
		}
		set
		{
			this.recipe = value;
			this.IsCurrentWorkstation = false;
			if (this.recipe != null)
			{
				for (int i = 0; i < this.RecipeList.craftingArea.Length; i++)
				{
					if (this.RecipeList.craftingArea[i] == this.recipe.craftingArea)
					{
						this.IsCurrentWorkstation = true;
						break;
					}
				}
			}
			if (!base.IsSelected)
			{
				this.background.Color = new Color32(64, 64, 64, byte.MaxValue);
			}
			base.ViewComponent.IsNavigatable = (base.ViewComponent.IsSnappable = value != null);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SelectedChanged(bool isSelected)
	{
		if (this.background != null)
		{
			this.background.Color = (isSelected ? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue) : new Color32(64, 64, 64, byte.MaxValue));
			this.background.SpriteName = (isSelected ? "ui_game_select_row" : "menu_empty");
		}
	}

	public void SetRecipeAndHasIngredients(Recipe recipe, bool hasIngredients)
	{
		this.Recipe = recipe;
		this.hasIngredients = hasIngredients;
		this.IsDirty = true;
		base.RefreshBindings();
		if (recipe == null)
		{
			this.background.Color = new Color32(64, 64, 64, byte.MaxValue);
		}
	}

	public override void Init()
	{
		base.Init();
		for (int i = 0; i < this.children.Count; i++)
		{
			XUiView viewComponent = this.children[i].ViewComponent;
			if (viewComponent.ID.EqualsCaseInsensitive("name"))
			{
				this.lblName = viewComponent as XUiV_Label;
			}
			else if (viewComponent.ID.EqualsCaseInsensitive("icon"))
			{
				this.icoRecipe = viewComponent as XUiV_Sprite;
			}
			else if (viewComponent.ID.EqualsCaseInsensitive("favorite"))
			{
				this.icoFavorite = viewComponent as XUiV_Sprite;
			}
			else if (viewComponent.ID.EqualsCaseInsensitive("unlocked"))
			{
				this.icoBook = viewComponent as XUiV_Sprite;
			}
			else if (viewComponent.ID.EqualsCaseInsensitive("background"))
			{
				this.background = viewComponent as XUiV_Sprite;
			}
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		this.isHovered = _isOver;
		if (this.background != null && this.recipe != null && !base.IsSelected)
		{
			if (_isOver)
			{
				this.background.Color = new Color32(96, 96, 96, byte.MaxValue);
			}
			else
			{
				this.background.Color = new Color32(64, 64, 64, byte.MaxValue);
			}
		}
		base.OnHovered(_isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 1820482109U)
		{
			if (num <= 847165955U)
			{
				if (num != 98823489U)
				{
					if (num != 112674252U)
					{
						if (num == 847165955U)
						{
							if (bindingName == "itemtypeicon")
							{
								value = "";
								if (this.recipe != null)
								{
									ItemClass forId = ItemClass.GetForId(this.recipe.itemValueType);
									if (forId != null)
									{
										if (forId.IsBlock())
										{
											value = forId.GetBlock().ItemTypeIcon;
										}
										else
										{
											if (forId.AltItemTypeIcon != null && forId.Unlocks != "" && XUiM_ItemStack.CheckKnown(this.xui.playerUI.entityPlayer, forId, null))
											{
												value = forId.AltItemTypeIcon;
												return true;
											}
											value = forId.ItemTypeIcon;
										}
									}
								}
								return true;
							}
						}
					}
					else if (bindingName == "recipename")
					{
						value = ((this.recipe != null) ? Localization.Get(this.recipe.GetName(), false, null) : "");
						return true;
					}
				}
				else if (bindingName == "hasingredientsstatecolor")
				{
					if (this.recipe != null)
					{
						Color32 color;
						color..ctor(148, 148, 148, byte.MaxValue);
						if (this.HasIngredients)
						{
							if (this.CustomAttributes.ContainsKey("enabled_font_color"))
							{
								color = StringParsers.ParseColor32(this.CustomAttributes["enabled_font_color"].ToString());
							}
							else
							{
								color = Color.white;
							}
						}
						else if (this.CustomAttributes.ContainsKey("disabled_font_color"))
						{
							color = StringParsers.ParseColor32(this.CustomAttributes["disabled_font_color"].ToString());
						}
						value = this.hasingredientsstatecolorFormatter.Format(color);
					}
					else
					{
						value = "255,255,255,255";
					}
					return true;
				}
			}
			else if (num != 1236476975U)
			{
				if (num != 1388578781U)
				{
					if (num == 1820482109U)
					{
						if (bindingName == "isfavorite")
						{
							value = ((this.recipe != null) ? XUiM_Recipes.GetRecipeIsFavorite(this.xui, this.recipe).ToString() : "false");
							return true;
						}
					}
				}
				else if (bindingName == "hasitemtypeicon")
				{
					value = "false";
					if (this.recipe != null)
					{
						ItemClass forId2 = ItemClass.GetForId(this.recipe.itemValueType);
						if (forId2 != null)
						{
							if (forId2.IsBlock())
							{
								value = (forId2.GetBlock().ItemTypeIcon != "").ToString();
							}
							else
							{
								value = (forId2.ItemTypeIcon != "").ToString();
							}
						}
					}
					return true;
				}
			}
			else if (bindingName == "recipeicontint")
			{
				Color32 color2 = Color.white;
				if (this.recipe != null)
				{
					ItemClass forId3 = ItemClass.GetForId(this.recipe.itemValueType);
					if (forId3 != null)
					{
						color2 = forId3.GetIconTint(null);
					}
				}
				value = this.recipeicontintcolorFormatter.Format(color2);
				return true;
			}
		}
		else if (num <= 2172950741U)
		{
			if (num != 2017922685U)
			{
				if (num != 2154375741U)
				{
					if (num == 2172950741U)
					{
						if (bindingName == "isunlockable")
						{
							value = ((this.recipe != null) ? (!this.IsCurrentWorkstation || XUiM_Recipes.GetRecipeIsUnlockable(this.xui, this.recipe) || this.recipe.isQuest || this.recipe.isChallenge || this.recipe.IsTracked).ToString() : "false");
							return true;
						}
					}
				}
				else if (bindingName == "hasrecipe")
				{
					value = (this.recipe != null).ToString();
					return true;
				}
			}
			else if (bindingName == "hasingredients")
			{
				value = ((this.recipe != null) ? this.HasIngredients.ToString() : "false");
				return true;
			}
		}
		else if (num <= 3435046057U)
		{
			if (num != 3008660032U)
			{
				if (num == 3435046057U)
				{
					if (bindingName == "unlockstatecolor")
					{
						if (this.recipe != null)
						{
							if (this.recipe.isQuest || this.recipe.isChallenge)
							{
								value = this.unlockstatecolorFormatter.Format(Color.yellow);
							}
							else
							{
								Color32 color3 = (XUiM_Recipes.GetRecipeIsUnlocked(this.xui, this.recipe) ? Color.white : Color.gray);
								value = this.unlockstatecolorFormatter.Format(color3);
							}
						}
						else
						{
							value = "255,255,255,255";
						}
						return true;
					}
				}
			}
			else if (bindingName == "unlockicon")
			{
				if (this.recipe != null)
				{
					if (this.recipe.isChallenge)
					{
						value = "ui_game_symbol_challenge";
						return true;
					}
					if (this.recipe.isQuest)
					{
						value = "ui_game_symbol_quest";
						return true;
					}
					if (this.recipe.IsTracked)
					{
						value = "ui_game_symbol_compass";
						return true;
					}
					if (XUiM_Recipes.GetRecipeIsUnlockable(this.xui, this.recipe) && !XUiM_Recipes.GetRecipeIsUnlocked(this.xui, this.recipe))
					{
						value = "ui_game_symbol_lock";
					}
					else if (!this.IsCurrentWorkstation)
					{
						WorkstationData workstationData = CraftingManager.GetWorkstationData(this.recipe.craftingArea);
						if (workstationData != null)
						{
							value = workstationData.WorkstationIcon;
						}
						else
						{
							value = "ui_game_symbol_hammer";
						}
					}
					else
					{
						value = "";
					}
				}
				else
				{
					value = "ui_game_symbol_book";
				}
				return true;
			}
		}
		else if (num != 3974800894U)
		{
			if (num == 4049247086U)
			{
				if (bindingName == "itemtypeicontint")
				{
					value = "255,255,255,255";
					if (this.recipe != null)
					{
						ItemClass forId4 = ItemClass.GetForId(this.recipe.itemValueType);
						if (forId4 != null && forId4.Unlocks != "" && XUiM_ItemStack.CheckKnown(this.xui.playerUI.entityPlayer, forId4, null))
						{
							value = this.altitemtypeiconcolorFormatter.Format(forId4.AltItemTypeIconColor);
						}
					}
					return true;
				}
			}
		}
		else if (bindingName == "recipeicon")
		{
			value = ((this.recipe != null) ? this.recipe.GetIcon() : "");
			return true;
		}
		return base.GetBindingValueInternal(ref value, bindingName);
	}

	public void Refresh()
	{
		this.IsDirty = true;
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe recipe;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasIngredients;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite icoRecipe;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite icoFavorite;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite icoBook;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	public XUiC_RecipeList RecipeList;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor recipeicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor hasingredientsstatecolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor unlockstatecolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor altitemtypeiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();
}
