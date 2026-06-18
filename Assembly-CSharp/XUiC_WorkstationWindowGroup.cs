using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorkstationWindowGroup : XUiC_CraftingWindowGroup
{
	public void SetTileEntity(TileEntityWorkstation _te)
	{
		this.WorkstationData = new XUiM_Workstation(_te);
		this.workstationBlock = _te.GetChunk().GetBlock(_te.localChunkPos);
		base.Workstation = this.workstationBlock.Block.GetBlockName();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TileEntity_Destroyed(ITileEntity te)
	{
		if (this.WorkstationData == null || this.WorkstationData.TileEntity == te)
		{
			this.xui.playerUI.windowManager.Close(this.windowGroup, false);
			return;
		}
		te.Destroyed -= this.TileEntity_Destroyed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TileEntity_ToolChanged()
	{
		if (this.toolWindow != null)
		{
			this.toolWindow.SetSlots(this.WorkstationData.GetToolStacks());
			base.SetAllChildrenDirty();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TileEntity_OutputChanged()
	{
		if (this.outputWindow != null)
		{
			this.outputWindow.SetSlots(this.WorkstationData.GetOutputStacks());
			base.SetAllChildrenDirty();
		}
	}

	public void TileEntity_InputChanged()
	{
		if (this.inputWindow != null)
		{
			this.inputWindow.SetSlots(this.WorkstationData.GetInputStacks());
			base.SetAllChildrenDirty();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TileEntity_FuelChanged()
	{
		if (this.fuelWindow != null)
		{
			this.fuelWindow.SetSlots(this.WorkstationData.GetFuelStacks());
			base.SetAllChildrenDirty();
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController xuiController = base.GetChildByType<XUiC_WorkstationToolGrid>();
		if (xuiController != null)
		{
			this.toolWindow = (XUiC_WorkstationToolGrid)xuiController;
		}
		xuiController = base.GetChildByType<XUiC_WorkstationInputGrid>();
		if (xuiController != null)
		{
			this.inputWindow = (XUiC_WorkstationInputGrid)xuiController;
		}
		xuiController = base.GetChildByType<XUiC_WorkstationOutputGrid>();
		if (xuiController != null)
		{
			this.outputWindow = (XUiC_WorkstationOutputGrid)xuiController;
		}
		xuiController = base.GetChildByType<XUiC_WorkstationFuelGrid>();
		if (xuiController != null)
		{
			this.fuelWindow = (XUiC_WorkstationFuelGrid)xuiController;
		}
		xuiController = base.GetChildByType<XUiC_CraftingQueue>();
		if (xuiController != null)
		{
			this.craftingQueue = (XUiC_CraftingQueue)xuiController;
		}
		xuiController = base.GetChildByType<XUiC_WindowNonPagingHeader>();
		if (xuiController != null)
		{
			this.nonPagingHeaderWindow = base.GetChildByType<XUiC_WindowNonPagingHeader>();
		}
		xuiController = base.GetChildById("burnTimeLeft");
		if (xuiController != null)
		{
			this.burnTimeLeft = (XUiV_Label)base.GetChildById("burnTimeLeft").ViewComponent;
		}
	}

	public override bool CraftingRequirementsValid(Recipe _recipe)
	{
		bool flag = true;
		if (this.toolWindow != null)
		{
			flag &= this.toolWindow.HasRequirement(_recipe);
		}
		if (this.inputWindow != null)
		{
			flag &= this.inputWindow.HasRequirement(_recipe);
		}
		if (this.outputWindow != null)
		{
			flag &= this.outputWindow.HasRequirement(_recipe);
		}
		if (this.fuelWindow != null)
		{
			flag &= this.fuelWindow.HasRequirement(_recipe);
		}
		return flag;
	}

	public override string CraftingRequirementsInvalidMessage(Recipe _recipe)
	{
		if (this.toolWindow != null && !this.toolWindow.HasRequirement(_recipe))
		{
			return Localization.Get("ttMissingCraftingTools", false, null);
		}
		if (this.inputWindow != null && !this.inputWindow.HasRequirement(_recipe))
		{
			return Localization.Get("ttMissingCraftingResources", false, null);
		}
		if (this.outputWindow != null)
		{
			this.outputWindow.HasRequirement(_recipe);
		}
		if (this.fuelWindow != null && !this.fuelWindow.HasRequirement(_recipe))
		{
			return Localization.Get("ttMissingCraftingFuel", false, null);
		}
		return "";
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		bool flag = this.craftingQueue.IsCrafting();
		if (flag != this.wasCrafting)
		{
			this.wasCrafting = flag;
			this.syncTEfromUI();
		}
		if (this.windowGroup.isShowing)
		{
			if (this.WorkstationData != null && GameManager.Instance != null && GameManager.Instance.World != null && this.WorkstationData.TileEntity.IsBurning)
			{
				if (this.openTEUpdateTime <= 0f)
				{
					this.WorkstationData.TileEntity.UpdateTick(GameManager.Instance.World);
					this.openTEUpdateTime = 0.5f;
				}
				else
				{
					this.openTEUpdateTime -= _dt;
				}
			}
			if (!this.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
			{
				this.wasReleased = true;
			}
			if (this.wasReleased)
			{
				if (this.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
				{
					this.activeKeyDown = true;
				}
				if (this.xui.playerUI.playerInput.PermanentActions.Activate.WasReleased && this.activeKeyDown)
				{
					this.activeKeyDown = false;
					if (!this.xui.playerUI.windowManager.IsInputActive())
					{
						this.xui.playerUI.windowManager.CloseAllOpenModalWindows(null, false);
					}
				}
			}
		}
		if (this.fuelWindow != null && this.craftingQueue != null && this.burnTimeLeft != null && this.WorkstationData != null)
		{
			float totalBurnTimeLeft = this.WorkstationData.GetTotalBurnTimeLeft();
			if ((!this.WorkstationData.GetIsBurning() || totalBurnTimeLeft == 0f) && this.craftingQueue.IsCrafting())
			{
				this.craftingQueue.HaltCrafting();
			}
			else if (this.WorkstationData.GetIsBurning() && totalBurnTimeLeft == 0f && !this.craftingQueue.IsCrafting())
			{
				this.craftingQueue.ResumeCrafting();
			}
			this.burnTimeLeft.Text = string.Format("{0}:{1}", ((int)(totalBurnTimeLeft / 60f)).ToString("00"), ((int)(totalBurnTimeLeft % 60f)).ToString("00"));
		}
	}

	public override bool AddItemToQueue(Recipe _recipe)
	{
		if (this.fuelWindow != null)
		{
			this.fuelWindow.TurnOn();
		}
		return base.AddItemToQueue(_recipe);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void syncTEfromUI()
	{
		TileEntityWorkstation tileEntity = this.WorkstationData.TileEntity;
		tileEntity.SetDisableModifiedCheck(true);
		if (this.toolWindow != null)
		{
			this.WorkstationData.SetToolStacks(this.toolWindow.GetSlots());
		}
		if (this.inputWindow != null)
		{
			this.WorkstationData.SetInputStacks(this.inputWindow.GetSlots());
		}
		if (this.outputWindow != null)
		{
			this.WorkstationData.SetOutputStacks(this.outputWindow.GetSlots());
		}
		if (this.fuelWindow != null)
		{
			this.WorkstationData.SetFuelStacks(this.fuelWindow.GetSlots());
		}
		if (this.craftingQueue != null)
		{
			XUiC_RecipeStack[] recipesToCraft = this.craftingQueue.GetRecipesToCraft();
			RecipeQueueItem[] array = new RecipeQueueItem[recipesToCraft.Length];
			for (int i = 0; i < recipesToCraft.Length; i++)
			{
				array[i] = new RecipeQueueItem
				{
					Recipe = recipesToCraft[i].GetRecipe(),
					Multiplier = (short)recipesToCraft[i].GetRecipeCount(),
					CraftingTimeLeft = recipesToCraft[i].GetRecipeCraftingTimeLeft(),
					IsCrafting = recipesToCraft[i].IsCrafting,
					Quality = (byte)recipesToCraft[i].OutputQuality,
					StartingEntityId = recipesToCraft[i].StartingEntityId,
					OneItemCraftTime = recipesToCraft[i].GetOneItemCraftTime()
				};
			}
			this.WorkstationData.SetRecipeQueueItems(array);
		}
		tileEntity.SetDisableModifiedCheck(false);
		tileEntity.SetModified();
		tileEntity.ResetTickTime();
	}

	public override void OnOpen()
	{
		this.WorkstationData.SetUserAccessing(true);
		this.WorkstationData.TileEntity.FuelChanged += this.TileEntity_FuelChanged;
		this.WorkstationData.TileEntity.InputChanged += this.TileEntity_InputChanged;
		this.WorkstationData.TileEntity.Destroyed += this.TileEntity_Destroyed;
		this.openTEUpdateTime = 0.5f;
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		if (this.recipeList != null && this.categoryList != null && this.categoryList.SetupCategoriesByWorkstation(base.Workstation))
		{
			this.recipeList.Workstation = base.Workstation;
			this.categoryList.SetCategoryToFirst();
		}
		if (this.nonPagingHeaderWindow != null)
		{
			this.nonPagingHeaderWindow.SetHeader(Localization.Get(base.Workstation, false, null));
		}
		GUIWindowManager windowManager = this.xui.playerUI.windowManager;
		this.syncUIfromTE();
		if (this.craftingQueue != null)
		{
			this.craftingQueue.ClearQueue();
			RecipeQueueItem[] recipeQueueItems = this.WorkstationData.GetRecipeQueueItems();
			for (int i = 0; i < recipeQueueItems.Length; i++)
			{
				if (recipeQueueItems[i] != null)
				{
					this.craftingQueue.AddRecipeToCraftAtIndex(i, recipeQueueItems[i].Recipe, (int)recipeQueueItems[i].Multiplier, recipeQueueItems[i].CraftingTimeLeft, recipeQueueItems[i].IsCrafting, false, (int)recipeQueueItems[i].Quality, recipeQueueItems[i].StartingEntityId, recipeQueueItems[i].OneItemCraftTime);
				}
				else
				{
					this.craftingQueue.AddRecipeToCraftAtIndex(i, null, 0, -1f, false, true, -1, -1, -1f);
				}
			}
			this.craftingQueue.IsDirty = true;
		}
		this.xui.RecenterWindowGroup(this.windowGroup, false);
		for (int j = 0; j < this.children.Count; j++)
		{
			this.children[j].OnOpen();
		}
		WorkstationData workstationData = CraftingManager.GetWorkstationData(this.workstation);
		if (workstationData != null)
		{
			Manager.BroadcastPlayByLocalPlayer(this.WorkstationData.TileEntity.ToWorldPos().ToVector3() + Vector3.one * 0.5f, workstationData.OpenSound);
			this.WorkstationData.TileEntity.CheckForCraftComplete(this.xui.playerUI.entityPlayer);
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void syncUIfromTE()
	{
		if (this.WorkstationData != null && GameManager.Instance != null && GameManager.Instance.World != null)
		{
			this.WorkstationData.TileEntity.UpdateTick(GameManager.Instance.World);
		}
		if (this.toolWindow != null)
		{
			this.toolWindow.SetSlots(this.WorkstationData.GetToolStacks());
			this.toolWindow.IsDirty = true;
		}
		if (this.inputWindow != null)
		{
			this.inputWindow.SetSlots(this.WorkstationData.GetInputStacks());
			this.inputWindow.IsDirty = true;
		}
		if (this.outputWindow != null)
		{
			this.outputWindow.SetSlots(this.WorkstationData.GetOutputStacks());
			this.outputWindow.IsDirty = true;
		}
		if (this.fuelWindow != null)
		{
			this.fuelWindow.SetSlots(this.WorkstationData.GetFuelStacks());
			this.fuelWindow.IsDirty = true;
		}
		base.SetAllChildrenDirty();
	}

	public override void OnClose()
	{
		this.wasReleased = false;
		this.activeKeyDown = false;
		this.syncTEfromUI();
		this.WorkstationData.SetUserAccessing(false);
		this.WorkstationData.TileEntity.FuelChanged -= this.TileEntity_FuelChanged;
		this.WorkstationData.TileEntity.InputChanged -= this.TileEntity_InputChanged;
		this.WorkstationData.TileEntity.Destroyed -= this.TileEntity_Destroyed;
		base.OnClose();
		LockManager.Instance.UnlockRequestLocal();
		WorkstationData workstationData = CraftingManager.GetWorkstationData(this.workstation);
		if (workstationData != null)
		{
			Manager.BroadcastPlayByLocalPlayer(this.WorkstationData.TileEntity.ToWorldPos().ToVector3() + Vector3.one * 0.5f, workstationData.CloseSound);
		}
		this.xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
	}

	public XUiM_Workstation WorkstationData;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue workstationBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationToolGrid toolWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationInputGrid inputWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationOutputGrid outputWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationFuelGrid fuelWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public new XUiC_CraftingQueue craftingQueue;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label burnTimeLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasCrafting;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;

	[PublicizedFrom(EAccessModifier.Private)]
	public float openTEUpdateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float openTEUpdateTimeMax = 0.5f;
}
