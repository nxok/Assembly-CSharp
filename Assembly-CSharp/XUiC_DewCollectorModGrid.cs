using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DewCollectorModGrid : XUiC_ItemStackGrid
{
	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Workstation;
		}
	}

	public override void Init()
	{
		base.Init();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.UpdateBackend(stackList);
		this.tileEntity.SetModSlots(stackList, false);
		this.tileEntity.SetModified();
		this.windowGroup.Controller.SetAllChildrenDirty();
	}

	public void SetTileEntity(TileEntityCollector te)
	{
		this.tileEntity = te;
		this.blockCollector = te.blockValue.Block as BlockCollector;
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			XUiC_RequiredItemStack xuiC_RequiredItemStack = this.itemControllers[i] as XUiC_RequiredItemStack;
			if (xuiC_RequiredItemStack != null)
			{
				if (i < this.blockCollector.RequiredMods.Length)
				{
					xuiC_RequiredItemStack.SetAllowedItemClassSingle(this.blockCollector.RequiredMods[i]);
					xuiC_RequiredItemStack.RequiredItemOnly = this.blockCollector.RequiredModsOnly;
				}
				else
				{
					xuiC_RequiredItemStack.ClearAllowedItemClasses();
					xuiC_RequiredItemStack.RequiredItemOnly = false;
				}
				xuiC_RequiredItemStack.StackLocation = this.StackLocation;
			}
		}
		this.SetStacks(te.ModSlots);
	}

	public bool TryAddMod(ItemClass newItemClass, ItemStack newItemStack)
	{
		if (!this.blockCollector.RequiredModsOnly)
		{
			return false;
		}
		XUiC_ItemStack[] itemControllers = this.itemControllers;
		for (int i = 0; i < itemControllers.Length; i++)
		{
			((XUiC_RequiredItemStack)itemControllers[i]).TryStack(newItemStack, -1);
			if (newItemStack.count == 0)
			{
				return true;
			}
		}
		return newItemStack.count == 0;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.xui.CurrentCollectorModGrid = this;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.CurrentCollectorModGrid = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityCollector tileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockCollector blockCollector;
}
