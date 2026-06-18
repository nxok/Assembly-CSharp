using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemStackGrid : XUiController
{
	public virtual XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Backpack;
		}
	}

	public override void Init()
	{
		base.Init();
		this.itemControllers = base.GetChildrenByType<XUiC_ItemStack>(null);
		this.bAwakeCalled = true;
		this.IsDirty = false;
		this.handleSlotChangedDelegate = new XUiEvent_SlotChangedEventHandler(this.HandleSlotChangedEvent);
	}

	public virtual XUiC_ItemStack[] GetItemStackControllers()
	{
		return this.itemControllers;
	}

	public virtual ItemStack[] GetSlots()
	{
		return this.getUISlots();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual ItemStack[] getUISlots()
	{
		ItemStack[] array = new ItemStack[this.itemControllers.Length];
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			array[i] = this.itemControllers[i].ItemStack.Clone();
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetStacks(ItemStack[] stackList)
	{
		if (stackList == null)
		{
			return;
		}
		XUiC_ItemInfoWindow childByType = this.xui.GetChildByType<XUiC_ItemInfoWindow>();
		int num = 0;
		while (num < stackList.Length && this.itemControllers.Length > num && stackList.Length > num)
		{
			XUiC_ItemStack xuiC_ItemStack = this.itemControllers[num];
			xuiC_ItemStack.SlotChangedEvent -= this.handleSlotChangedDelegate;
			xuiC_ItemStack.ItemStack = stackList[num].Clone();
			xuiC_ItemStack.SlotChangedEvent += this.handleSlotChangedDelegate;
			xuiC_ItemStack.SlotNumber = num;
			xuiC_ItemStack.InfoWindow = childByType;
			xuiC_ItemStack.StackLocation = this.StackLocation;
			num++;
		}
	}

	public void AssembleLockSingleStack(ItemStack stack)
	{
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			XUiC_ItemStack xuiC_ItemStack = this.itemControllers[i];
			if (xuiC_ItemStack.ItemStack.itemValue.Equals(stack.itemValue))
			{
				this.xui.AssembleItem.CurrentItemStackController = xuiC_ItemStack;
				return;
			}
		}
	}

	public virtual void HandleSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		if (this.items != null)
		{
			this.items[slotNumber] = stack.Clone();
		}
		this.UpdateBackend(this.getUISlots());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateBackend(ItemStack[] stackList)
	{
	}

	public virtual void ClearHoveredItems()
	{
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			this.itemControllers[i].Hovered(false);
		}
	}

	public int FindFirstEmptySlot()
	{
		int num = -1;
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			if (this.itemControllers[i].ViewComponent.IsActiveInHierarchy && this.itemControllers[i].ItemStack.IsEmpty())
			{
				num = i;
				break;
			}
		}
		return num;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.IsDirty = true;
		this.xui.playerUI.RegisterItemStackGrid(this);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.ClearHoveredItems();
		this.xui.playerUI.UnregisterItemStackGrid(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_ItemStack[] itemControllers;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemStack[] items;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiEvent_SlotChangedEventHandler handleSlotChangedDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bAwakeCalled;
}
