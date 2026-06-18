using System;

public class XUiM_Workstation : XUiModel
{
	public TileEntityWorkstation TileEntity
	{
		get
		{
			return this.tileEntity;
		}
	}

	public XUiM_Workstation(TileEntityWorkstation _te)
	{
		this.tileEntity = _te;
	}

	public bool GetIsBurning()
	{
		return this.tileEntity.IsBurning;
	}

	public bool GetIsBesideWater()
	{
		return this.tileEntity.IsBesideWater;
	}

	public void SetIsBurning(bool _isBurning)
	{
		this.tileEntity.IsBurning = _isBurning;
		this.tileEntity.ResetTickTime();
	}

	public ItemStack[] GetInputStacks()
	{
		return this.tileEntity.Input;
	}

	public void SetInputStacks(ItemStack[] _itemStacks)
	{
		this.tileEntity.Input = _itemStacks;
	}

	public void SetInputInSlot(int _idx, ItemStack _itemStack)
	{
		this.tileEntity.Input[_idx] = _itemStack.Clone();
	}

	public ItemStack[] GetOutputStacks()
	{
		return this.tileEntity.Output;
	}

	public void SetOutputStacks(ItemStack[] _itemStacks)
	{
		this.tileEntity.Output = _itemStacks;
	}

	public void SetOutputInSlot(int _idx, ItemStack _itemStack)
	{
		this.tileEntity.Output[_idx] = _itemStack.Clone();
	}

	public ItemStack[] GetToolStacks()
	{
		return this.tileEntity.Tools;
	}

	public void SetToolStacks(ItemStack[] _itemStacks)
	{
		this.tileEntity.Tools = _itemStacks;
	}

	public void SetToolInSlot(int _idx, ItemStack _itemStack)
	{
		this.tileEntity.Tools[_idx] = _itemStack.Clone();
	}

	public ItemStack[] GetFuelStacks()
	{
		return this.tileEntity.Fuel;
	}

	public void SetFuelStacks(ItemStack[] _itemStacks)
	{
		this.tileEntity.Fuel = _itemStacks;
	}

	public void SetFuelInSlot(int _idx, ItemStack _itemStack)
	{
		this.tileEntity.Fuel[_idx] = _itemStack.Clone();
	}

	public float GetBurnTimeLeft()
	{
		if (this.tileEntity.BurnTimeLeft == 0f)
		{
			return 0f;
		}
		return this.tileEntity.BurnTimeLeft + 0.5f;
	}

	public float GetTotalBurnTimeLeft()
	{
		if (this.tileEntity.BurnTotalTimeLeft == 0f)
		{
			return 0f;
		}
		return this.tileEntity.BurnTotalTimeLeft + 0.5f;
	}

	public RecipeQueueItem[] GetRecipeQueueItems()
	{
		return this.tileEntity.Queue;
	}

	public void SetRecipeQueueItems(RecipeQueueItem[] _queueStacks)
	{
		this.tileEntity.Queue = _queueStacks;
	}

	public void SetQueueInSlot(int _idx, RecipeQueueItem _queueStack)
	{
		this.tileEntity.Queue[_idx] = _queueStack;
	}

	public void SetUserAccessing(bool _isUserAccessing)
	{
		this.tileEntity.SetUserAccessing(_isUserAccessing);
	}

	public string[] GetMaterialNames()
	{
		return this.tileEntity.MaterialNames;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly TileEntityWorkstation tileEntity;
}
