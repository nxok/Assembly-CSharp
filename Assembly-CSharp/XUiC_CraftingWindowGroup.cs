using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CraftingWindowGroup : XUiController
{
	public string Workstation
	{
		get
		{
			return this.workstation;
		}
		set
		{
			this.workstation = value;
			if (this.recipeList != null)
			{
				this.recipeList.Workstation = this.workstation;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		this.recipeList = base.GetChildByType<XUiC_RecipeList>();
		this.craftingQueue = base.GetChildByType<XUiC_CraftingQueue>();
		this.craftCountControl = base.GetChildByType<XUiC_RecipeCraftCount>();
		this.categoryList = base.GetChildByType<XUiC_CategoryList>();
		this.craftInfoWindow = base.GetChildByType<XUiC_CraftingInfoWindow>();
		this.recipeList.InfoWindow = this.craftInfoWindow;
	}

	public virtual bool AddItemToQueue(Recipe _recipe)
	{
		if (this.craftCountControl != null)
		{
			return this.craftingQueue.AddRecipeToCraft(_recipe, this.craftCountControl.Count, -1f, true, -1f);
		}
		return this.craftingQueue.AddRecipeToCraft(_recipe, 1, -1f, true, -1f);
	}

	public virtual bool AddItemToQueue(Recipe _recipe, int _count)
	{
		return this.craftingQueue.AddRecipeToCraft(_recipe, _count, -1f, true, -1f);
	}

	public virtual bool AddItemToQueue(Recipe _recipe, int _count, float _craftTime)
	{
		return this.craftingQueue.AddRecipeToCraft(_recipe, _count, _craftTime, true, -1f);
	}

	public virtual bool AddRepairItemToQueue(float _repairTime, ItemValue _itemToRepair, int _amountToRepair)
	{
		return this.craftingQueue.AddItemToRepair(_repairTime, _itemToRepair, _amountToRepair);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.recipeList != null && this.categoryList != null && this.categoryList.SetupCategoriesByWorkstation(""))
		{
			this.recipeList.Workstation = "";
			this.recipeList.SetCategory("Basics");
			this.categoryList.SetCategory("Basics");
		}
		this.xui.playerUI.windowManager.Open("windowpaging", false);
		XUiC_WindowSelector childByType = this.xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>();
		if (childByType != null)
		{
			childByType.SetSelected("crafting");
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.playerUI.windowManager.Close("windowpaging");
	}

	public virtual bool CraftingRequirementsValid(Recipe _recipe)
	{
		return true;
	}

	public virtual string CraftingRequirementsInvalidMessage(Recipe _recipe)
	{
		return "";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string workstation = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool firstRun = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_RecipeList recipeList;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_CraftingQueue craftingQueue;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_RecipeCraftCount craftCountControl;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_CraftingInfoWindow craftInfoWindow;
}
