using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PowerSourceSlots : XUiC_ItemStackGrid
{
	public TileEntityPowerSource TileEntity
	{
		get
		{
			return this.tileEntity;
		}
		set
		{
			this.tileEntity = value;
			this.SetSlots(this.tileEntity.ItemSlots);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRequirements()
	{
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			XUiC_RequiredItemStack xuiC_RequiredItemStack = this.itemControllers[i] as XUiC_RequiredItemStack;
			if (xuiC_RequiredItemStack != null)
			{
				xuiC_RequiredItemStack.RequiredType = XUiC_RequiredItemStack.RequiredTypes.ItemClass;
				xuiC_RequiredItemStack.SetAllowedItemClassSingle(this.tileEntity.SlotItem);
			}
		}
	}

	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Workstation;
		}
	}

	public XUiC_PowerSourceWindowGroup Owner { get; set; }

	public virtual void SetSlots(ItemStack[] stacks)
	{
		this.items = stacks;
		base.SetStacks(stacks);
	}

	public virtual bool HasRequirement(Recipe recipe)
	{
		return true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		this.IsDirty = true;
		this.SetRequirements();
		this.xui.CurrentPowerSourceSlots = this;
		XUiC_PowerSourceSlots.Current = this;
	}

	public override void OnClose()
	{
		base.OnClose();
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnClose();
			base.ViewComponent.IsVisible = false;
		}
		this.IsDirty = true;
		XUiC_PowerSourceSlots.Current = (this.xui.CurrentPowerSourceSlots = null);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetOn(bool isOn)
	{
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			XUiC_RequiredItemStack xuiC_RequiredItemStack = this.itemControllers[i] as XUiC_RequiredItemStack;
			if (xuiC_RequiredItemStack != null)
			{
				xuiC_RequiredItemStack.ToolLock = isOn;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.UpdateBackend(stackList);
		this.tileEntity.ItemSlots = stackList;
		this.tileEntity.SetSendSlots();
		this.windowGroup.Controller.SetAllChildrenDirty();
	}

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null && GameManager.Instance.World == null)
		{
			return;
		}
		if (this.tileEntity == null)
		{
			return;
		}
		base.Update(_dt);
		if (this.tileEntity.IsOn)
		{
			this.SetSlots(this.tileEntity.ItemSlots);
		}
		base.RefreshBindings();
	}

	public void Refresh()
	{
		this.SetSlots(this.tileEntity.ItemSlots);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool TryAddItemToSlot(ItemClass itemClass, ItemStack itemStack)
	{
		if (itemClass != this.tileEntity.SlotItem)
		{
			return false;
		}
		bool flag = this.tileEntity.TryAddItemToSlot(itemClass, itemStack);
		if (flag)
		{
			this.SetSlots(this.tileEntity.ItemSlots);
		}
		return flag;
	}

	public static XUiC_PowerSourceSlots Current;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPowerSource tileEntity;
}
