using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_RecipeList : XUiController
{
	public string Workstation
	{
		get
		{
			return this.workStation;
		}
		set
		{
			this.workStation = value;
			Block blockByName = Block.GetBlockByName(this.workStation, false);
			if (blockByName != null && blockByName.Properties.Contains("Workstation", "CraftingAreaRecipes"))
			{
				string @string = blockByName.Properties.GetString("Workstation", "CraftingAreaRecipes");
				this.craftingArea = new string[] { @string };
				if (@string.Contains(","))
				{
					this.craftingArea = @string.Replace("player", "").Replace(", ", ",").Replace(" ,", ",")
						.Replace(" , ", ",")
						.Split(',', StringSplitOptions.None);
				}
			}
			else
			{
				this.craftingArea = new string[] { this.workStation };
			}
			this.GetRecipeData();
			this.IsDirty = true;
		}
	}

	public Recipe CurrentRecipe
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public XUiC_RecipeCraftCount CraftCount { get; set; }

	public int Page
	{
		get
		{
			return this.page;
		}
		set
		{
			if (this.page != value)
			{
				this.page = value;
				XUiC_Paging xuiC_Paging = this.pager;
				if (xuiC_Paging != null)
				{
					xuiC_Paging.SetPage(this.page);
				}
				if (this.PageNumberChanged != null)
				{
					this.PageNumberChanged(this.page);
				}
				this.IsDirty = true;
				this.pageChanged = true;
				this.CurrentRecipe = null;
			}
		}
	}

	public XUiC_CraftingInfoWindow InfoWindow { get; set; }

	public XUiC_RecipeEntry SelectedEntry
	{
		get
		{
			return this.selectedEntry;
		}
		set
		{
			if (this.selectedEntry != null)
			{
				this.selectedEntry.IsSelected = false;
			}
			this.selectedEntry = value;
			if (this.selectedEntry != null)
			{
				this.selectedEntry.IsSelected = true;
				this.InfoWindow.ViewComponent.IsVisible = true;
				this.RecipeChanged(this.selectedEntry.Recipe, this.selectedEntry);
				this.InfoWindow.SetRecipe(this.selectedEntry);
				this.CurrentRecipe = this.selectedEntry.Recipe;
			}
			else
			{
				this.InfoWindow.SetRecipe(null);
			}
			this.IsDirty = true;
			this.pageChanged = true;
		}
	}

	public event XUiEvent_RecipeChangedEventHandler RecipeChanged;

	public event XUiEvent_PageNumberChangedEventHandler PageNumberChanged;

	public override void Init()
	{
		base.Init();
		this.windowGroup.Controller.GetChildByType<XUiC_CategoryList>().CategoryChanged += this.HandleCategoryChanged;
		this.pager = base.Parent.GetChildByType<XUiC_Paging>();
		if (this.pager != null)
		{
			this.pager.OnPageChanged += delegate
			{
				this.Page = this.pager.CurrentPageNumber;
			};
		}
		this.recipeControls = base.GetChildrenByType<XUiC_RecipeEntry>(null);
		for (int i = 0; i < this.recipeControls.Length; i++)
		{
			XUiC_RecipeEntry xuiC_RecipeEntry = this.recipeControls[i];
			xuiC_RecipeEntry.OnScroll += this.HandleOnScroll;
			xuiC_RecipeEntry.OnPress += this.OnPressRecipe;
			xuiC_RecipeEntry.RecipeList = this;
		}
		this.parent.OnScroll += this.HandleOnScroll;
		XUiController childById = base.Parent.GetChildById("favorites");
		childById.OnPress += this.HandleFavoritesChanged;
		this.favorites = (XUiV_Button)childById.ViewComponent;
		XUiV_Grid xuiV_Grid = (XUiV_Grid)base.ViewComponent;
		if (xuiV_Grid != null)
		{
			this.length = xuiV_Grid.Columns * xuiV_Grid.Rows;
		}
		this.txtInput = (XUiC_TextInput)this.windowGroup.Controller.GetChildById("searchInput");
		if (this.txtInput != null)
		{
			this.txtInput.OnChangeHandler += this.HandleOnChangeHandler;
			this.txtInput.OnSubmitHandler += this.HandleOnSubmitHandler;
		}
		this.IsDirty = true;
		this.resortRecipes = true;
		this.pageChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleFavoritesChanged(XUiController _sender, int _mouseButton)
	{
		this.showFavorites = !this.showFavorites;
		this.favorites.Selected = this.showFavorites;
		this.GetRecipeData();
		this.IsDirty = true;
		this.resortRecipes = true;
		this.pageChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			XUiC_Paging xuiC_Paging = this.pager;
			if (xuiC_Paging == null)
			{
				return;
			}
			xuiC_Paging.PageDown();
			return;
		}
		else
		{
			XUiC_Paging xuiC_Paging2 = this.pager;
			if (xuiC_Paging2 == null)
			{
				return;
			}
			xuiC_Paging2.PageUp();
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnSubmitHandler(XUiController _sender, string _text)
	{
		this.GetRecipeData();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.GetRecipeData();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressRecipe(XUiController _sender, int _mouseButton)
	{
		XUiC_RecipeEntry xuiC_RecipeEntry = _sender as XUiC_RecipeEntry;
		if (xuiC_RecipeEntry != null && this.RecipeChanged != null)
		{
			this.SelectedEntry = xuiC_RecipeEntry;
			if (InputUtils.ShiftKeyPressed)
			{
				this.CraftCount.SetToMaxCount();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleCategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		this.SetCategory(_categoryEntry.CategoryName);
		this.IsDirty = true;
	}

	public void SetCategory(string _category)
	{
		if (this.txtInput != null)
		{
			this.txtInput.Text = "";
		}
		this.category = _category;
		this.GetRecipeData();
		this.IsDirty = true;
		this.resortRecipes = true;
		this.pageChanged = true;
	}

	public string GetCategory()
	{
		return this.category;
	}

	public void RefreshRecipes()
	{
		this.GetRecipeData();
	}

	public void RefreshCurrentRecipes()
	{
		this.IsDirty = true;
		this.pageChanged = true;
		if (this.showFavorites)
		{
			CraftingManager.GetFavoriteRecipesFromList(ref this.recipes);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetRecipeData()
	{
		ReadOnlyCollection<Recipe> readOnlyCollection = XUiM_Recipes.GetRecipes();
		List<string> questRecipes = this.xui.playerUI.entityPlayer.QuestJournal.GetQuestRecipes();
		List<Recipe> list = ((this.xui.QuestTracker.TrackedChallenge != null) ? this.xui.QuestTracker.TrackedChallenge.CraftedRecipes() : null);
		if (questRecipes.Count > 0 || (list != null && list.Count > 0))
		{
			for (int i = 0; i < readOnlyCollection.Count; i++)
			{
				if (list != null && list.Contains(readOnlyCollection[i]))
				{
					readOnlyCollection[i].isChallenge = true;
					readOnlyCollection[i].isQuest = false;
				}
				else if (questRecipes.Contains(readOnlyCollection[i].GetName()))
				{
					readOnlyCollection[i].isQuest = true;
					readOnlyCollection[i].isChallenge = false;
				}
				else
				{
					readOnlyCollection[i].isQuest = false;
					readOnlyCollection[i].isChallenge = false;
				}
			}
		}
		else
		{
			for (int j = 0; j < readOnlyCollection.Count; j++)
			{
				readOnlyCollection[j].isQuest = false;
				readOnlyCollection[j].isChallenge = false;
			}
		}
		if (this.txtInput != null && this.txtInput.Text.Length > 0)
		{
			this.recipes = XUiM_Recipes.FilterRecipesByName(this.txtInput.Text, XUiM_Recipes.GetRecipes());
		}
		else
		{
			this.recipes = XUiM_Recipes.FilterRecipesByWorkstation(this.workStation, readOnlyCollection);
			if (this.showFavorites)
			{
				CraftingManager.GetFavoriteRecipesFromList(ref this.recipes);
			}
			else if (this.category != "")
			{
				XUiM_Recipes.FilterRecipesByCategory(this.category, ref this.recipes);
			}
		}
		this.Page = 0;
		this.IsDirty = true;
		this.resortRecipes = true;
		this.pageChanged = true;
	}

	public void SetRecipeDataByIngredientStack(ItemStack stack)
	{
		if (this.txtInput != null)
		{
			this.txtInput.Text = "";
		}
		this.CurrentRecipe = null;
		this.recipes = XUiM_Recipes.FilterRecipesByIngredient(stack, XUiM_Recipes.GetRecipes());
		this.Page = 0;
		this.IsDirty = true;
		this.resortRecipes = true;
		this.pageChanged = true;
	}

	public void SetRecipeDataByItems(List<int> items)
	{
		if (items.Count == 0)
		{
			return;
		}
		if (this.txtInput != null)
		{
			this.txtInput.Text = "";
		}
		this.CurrentRecipe = null;
		this.recipes = XUiM_Recipes.FilterRecipesByItem(items, XUiM_Recipes.GetRecipes());
		this.Page = 0;
		this.IsDirty = true;
		this.resortRecipes = true;
		this.pageChanged = true;
	}

	public void SetRecipeDataByItem(int itemID)
	{
		if (this.txtInput != null)
		{
			this.txtInput.Text = "";
		}
		this.CurrentRecipe = null;
		this.recipes = XUiM_Recipes.FilterRecipesByID(itemID, XUiM_Recipes.GetRecipes());
		if (this.recipes != null && this.recipes.Count > 0)
		{
			this.CurrentRecipe = this.recipes[0];
		}
		this.Page = 0;
		this.IsDirty = true;
		this.resortRecipes = true;
		this.pageChanged = true;
	}

	public override void Update(float _dt)
	{
		if (this.IsDirty && this.xui.PlayerInventory != null)
		{
			this.FindShowingWindow();
			if (this.resortRecipes)
			{
				List<ItemStack> list = this.updateStackList;
				list.Clear();
				list.AddRange(this.xui.PlayerInventory.GetBackpackItemStacks());
				list.AddRange(this.xui.PlayerInventory.GetToolbeltItemStacks());
				XUiC_WorkstationInputGrid childByType = this.windowGroup.Controller.GetChildByType<XUiC_WorkstationInputGrid>();
				if (childByType != null && (!XUiM_Recipes.DisableSmelter || !(childByType is XUiC_WorkstationMaterialInputGrid)))
				{
					list.Clear();
					list.AddRange(childByType.GetSlots());
				}
				this.BuildRecipeInfosList(list);
				this.recipeInfos.Sort(new Comparison<XUiC_RecipeList.RecipeInfo>(this.CompareRecipeInfos));
				this.UpdateRecipes();
				this.resortRecipes = false;
			}
			if (this.pageChanged)
			{
				for (int i = 0; i < this.length; i++)
				{
					int num = i + this.length * this.page;
					XUiC_RecipeEntry xuiC_RecipeEntry = ((i < this.recipeControls.Length) ? this.recipeControls[i] : null);
					if (xuiC_RecipeEntry != null)
					{
						if (num < this.recipeInfos.Count)
						{
							XUiC_RecipeList.RecipeInfo recipeInfo = this.recipeInfos[num];
							xuiC_RecipeEntry.SetRecipeAndHasIngredients(recipeInfo.recipe, recipeInfo.hasIngredients);
							xuiC_RecipeEntry.ViewComponent.Enabled = true;
						}
						else
						{
							xuiC_RecipeEntry.SetRecipeAndHasIngredients(null, false);
							xuiC_RecipeEntry.ViewComponent.Enabled = false;
							if (xuiC_RecipeEntry.IsSelected)
							{
								xuiC_RecipeEntry.IsSelected = false;
							}
						}
						if (this.CurrentRecipe != null && this.CurrentRecipe == xuiC_RecipeEntry.Recipe && this.SelectedEntry != xuiC_RecipeEntry)
						{
							this.SelectedEntry = xuiC_RecipeEntry;
							this.CraftCount.IsDirty = true;
						}
						if (this.SelectedEntry != null && this.SelectedEntry.Recipe != this.CurrentRecipe)
						{
							this.ClearSelection();
						}
					}
				}
				this.pageChanged = false;
			}
			if (this.pager != null)
			{
				this.pager.SetLastPageByElementsAndPageLength(this.recipeInfos.Count, this.recipeControls.Length);
				this.pager.CurrentPageNumber = this.page;
			}
			this.IsDirty = false;
		}
		base.Update(_dt);
		if (this.xui.playerUI.playerInput.GUIActions.Inspect.WasPressed && this.xui.playerUI.CursorController.navigationTarget != null)
		{
			this.OnPressRecipe(this.xui.playerUI.CursorController.navigationTarget.Controller, 0);
		}
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = true;
		}
		if (this.xui.PlayerInventory != null)
		{
			this.xui.PlayerInventory.OnBackpackItemsChanged += this.PlayerInventory_OnBackpackItemsChanged;
			this.xui.PlayerInventory.OnToolbeltItemsChanged += this.PlayerInventory_OnToolbeltItemsChanged;
		}
		this.xui.playerUI.entityPlayer.QuestChanged += this.QuestJournal_QuestChanged;
		this.xui.playerUI.entityPlayer.QuestRemoved += this.QuestJournal_QuestChanged;
		this.xui.QuestTracker.OnTrackedChallengeChanged += this.QuestTracker_OnTrackedChallengeChanged;
		this.xui.Recipes.OnTrackedRecipeChanged += this.Recipes_OnTrackedRecipeChanged;
		XUiC_WorkstationMaterialInputWindow childByType = this.windowGroup.Controller.GetChildByType<XUiC_WorkstationMaterialInputWindow>();
		if (childByType != null)
		{
			childByType.OnWorkstationMaterialWeightsChanged += this.WorkstationMaterial_OnWeightsChanged;
		}
		XUiC_WorkstationFuelGrid childByType2 = this.windowGroup.Controller.GetChildByType<XUiC_WorkstationFuelGrid>();
		if (childByType2 != null)
		{
			childByType2.OnWorkstationFuelChanged += this.WorkStation_OnToolsOrFuelChanged;
		}
		XUiC_WorkstationToolGrid childByType3 = this.windowGroup.Controller.GetChildByType<XUiC_WorkstationToolGrid>();
		if (childByType3 != null)
		{
			childByType3.OnWorkstationToolsChanged += this.WorkStation_OnToolsOrFuelChanged;
		}
		this.ClearSelection();
		if (this.xui.playerUI.entityPlayer.QuestJournal.HasCraftingQuest() && (this.txtInput == null || this.txtInput.Text == ""))
		{
			this.GetRecipeData();
			this.pageChanged = true;
		}
		if (this.xui.QuestTracker.TrackedChallenge != null)
		{
			this.GetRecipeData();
			this.pageChanged = true;
		}
		this.IsDirty = true;
		this.resortRecipes = true;
	}

	public override void OnClose()
	{
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = false;
		}
		this.xui.PlayerInventory.OnBackpackItemsChanged -= this.PlayerInventory_OnBackpackItemsChanged;
		this.xui.PlayerInventory.OnToolbeltItemsChanged -= this.PlayerInventory_OnToolbeltItemsChanged;
		this.xui.playerUI.entityPlayer.QuestChanged -= this.QuestJournal_QuestChanged;
		this.xui.playerUI.entityPlayer.QuestRemoved -= this.QuestJournal_QuestChanged;
		this.xui.QuestTracker.OnTrackedChallengeChanged -= this.QuestTracker_OnTrackedChallengeChanged;
		this.xui.Recipes.OnTrackedRecipeChanged -= this.Recipes_OnTrackedRecipeChanged;
		XUiC_WorkstationMaterialInputWindow childByType = this.windowGroup.Controller.GetChildByType<XUiC_WorkstationMaterialInputWindow>();
		if (childByType != null)
		{
			childByType.OnWorkstationMaterialWeightsChanged -= this.WorkstationMaterial_OnWeightsChanged;
		}
		XUiC_WorkstationFuelGrid childByType2 = this.windowGroup.Controller.GetChildByType<XUiC_WorkstationFuelGrid>();
		if (childByType2 != null)
		{
			childByType2.OnWorkstationFuelChanged -= this.WorkStation_OnToolsOrFuelChanged;
		}
		XUiC_WorkstationToolGrid childByType3 = this.windowGroup.Controller.GetChildByType<XUiC_WorkstationToolGrid>();
		if (childByType3 != null)
		{
			childByType3.OnWorkstationToolsChanged -= this.WorkStation_OnToolsOrFuelChanged;
		}
		this.SelectedEntry = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WorkStation_OnToolsOrFuelChanged()
	{
		this.IsDirty = true;
		this.resortRecipes = true;
		this.pageChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnToolbeltItemsChanged()
	{
		this.IsDirty = true;
		this.resortRecipes = true;
		this.pageChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnBackpackItemsChanged()
	{
		this.IsDirty = true;
		this.resortRecipes = true;
		this.pageChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestJournal_QuestChanged(Quest q)
	{
		this.GetRecipeData();
		this.IsDirty = true;
		this.resortRecipes = true;
		this.pageChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestTracker_OnTrackedChallengeChanged()
	{
		this.GetRecipeData();
		this.IsDirty = true;
		this.resortRecipes = true;
		this.pageChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WorkstationMaterial_OnWeightsChanged()
	{
		this.IsDirty = true;
		this.resortRecipes = true;
		this.pageChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Recipes_OnTrackedRecipeChanged()
	{
		this.GetRecipeData();
		this.IsDirty = true;
		this.resortRecipes = true;
		this.pageChanged = true;
	}

	public void ClearSelection()
	{
		this.SelectedEntry = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildRecipeInfosList(List<ItemStack> _items)
	{
		this.recipeInfos.Clear();
		for (int i = 0; i < this.recipes.Count; i++)
		{
			XUiC_RecipeList.RecipeInfo recipeInfo;
			recipeInfo.recipe = this.recipes[i];
			recipeInfo.unlocked = XUiM_Recipes.GetRecipeIsUnlocked(this.xui, recipeInfo.recipe);
			EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
			bool flag = XUiM_Recipes.HasIngredientsForRecipe(_items, recipeInfo.recipe, entityPlayer);
			recipeInfo.hasIngredients = flag && (this.craftingWindow == null || this.craftingWindow.CraftingRequirementsValid(recipeInfo.recipe));
			recipeInfo.name = Localization.Get(recipeInfo.recipe.GetName(), false, null);
			this.recipeInfos.Add(recipeInfo);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateRecipes()
	{
		this.recipes.Clear();
		for (int i = 0; i < this.recipeInfos.Count; i++)
		{
			this.recipes.Add(this.recipeInfos[i].recipe);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CompareRecipeInfos(XUiC_RecipeList.RecipeInfo lhs, XUiC_RecipeList.RecipeInfo rhs)
	{
		if (lhs.recipe.IsTracked != rhs.recipe.IsTracked)
		{
			if (!lhs.recipe.IsTracked)
			{
				return 1;
			}
			return -1;
		}
		else if (lhs.recipe.isChallenge != rhs.recipe.isChallenge)
		{
			if (!lhs.recipe.isChallenge)
			{
				return 1;
			}
			return -1;
		}
		else if (lhs.recipe.isQuest != rhs.recipe.isQuest)
		{
			if (!lhs.recipe.isQuest)
			{
				return 1;
			}
			return -1;
		}
		else if (lhs.unlocked != rhs.unlocked)
		{
			if (!lhs.unlocked)
			{
				return 1;
			}
			return -1;
		}
		else if (lhs.hasIngredients != rhs.hasIngredients)
		{
			if (!lhs.hasIngredients)
			{
				return 1;
			}
			return -1;
		}
		else
		{
			if (!(lhs.name == rhs.name))
			{
				return string.Compare(lhs.name, rhs.name, StringComparison.Ordinal);
			}
			if (lhs.recipe.count > rhs.recipe.count)
			{
				return 1;
			}
			if (lhs.recipe.count < rhs.recipe.count)
			{
				return -1;
			}
			if (lhs.recipe.itemValueType > rhs.recipe.itemValueType)
			{
				return 1;
			}
			if (lhs.recipe.itemValueType < rhs.recipe.itemValueType)
			{
				return -1;
			}
			return this.CompareRecipeIngredients(lhs.recipe.ingredients, rhs.recipe.ingredients);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CompareRecipeIngredients(List<ItemStack> lhs, List<ItemStack> rhs)
	{
		if (lhs.Count > rhs.Count)
		{
			return 1;
		}
		if (lhs.Count < rhs.Count)
		{
			return -1;
		}
		for (int i = 0; i < lhs.Count; i++)
		{
			int itemId = lhs[i].itemValue.GetItemId();
			int itemId2 = rhs[i].itemValue.GetItemId();
			if (itemId > itemId2)
			{
				return 1;
			}
			if (itemId < itemId2)
			{
				return -1;
			}
			if (lhs[i].count > rhs[i].count)
			{
				return 1;
			}
			if (lhs[i].count < rhs[i].count)
			{
				return -1;
			}
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FindShowingWindow()
	{
		List<XUiC_CraftingWindowGroup> childrenByType = this.xui.GetChildrenByType<XUiC_CraftingWindowGroup>();
		for (int i = 0; i < childrenByType.Count; i++)
		{
			if (childrenByType[i].WindowGroup != null && childrenByType[i].WindowGroup.isShowing)
			{
				this.craftingWindow = childrenByType[i];
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_RecipeList.RecipeInfo> recipeInfos = new List<XUiC_RecipeList.RecipeInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RecipeEntry[] recipeControls;

	[PublicizedFrom(EAccessModifier.Private)]
	public string workStation = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RecipeEntry selectedEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button favorites;

	[PublicizedFrom(EAccessModifier.Private)]
	public string category = "Basics";

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Recipe> recipes = new List<Recipe>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showFavorites;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectedColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool resortRecipes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pageChanged;

	public string[] craftingArea = new string[] { "" };

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ItemStack> updateStackList = new List<ItemStack>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CraftingWindowGroup craftingWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public struct RecipeInfo
	{
		public Recipe recipe;

		public bool unlocked;

		public bool hasIngredients;

		public string name;
	}
}
