using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SlotPreview : XUiController
{
	public override void Init()
	{
		base.Init();
		this.slots = base.GetChildrenById("slot", null);
		XUiController[] childrenByType = this.parent.GetChildrenByType<XUiC_ItemStack>(null);
		this.itemStacks = childrenByType;
	}

	public override void OnOpen()
	{
		for (int i = 0; i < this.itemStacks.Length; i++)
		{
			this.XUiC_SlotPreview_SlotChangedEvent(i, ((XUiC_ItemStack)this.itemStacks[i]).ItemStack);
			((XUiC_ItemStack)this.itemStacks[i]).SlotChangedEvent += this.XUiC_SlotPreview_SlotChangedEvent;
			((XUiC_ItemStack)this.itemStacks[i]).ToolLockChangedEvent += this.XUiC_SlotPreview_ToolLockChangedEvent;
			XUiC_ItemStack xuiC_ItemStack = (XUiC_ItemStack)this.itemStacks[i];
			this.slots[i].ViewComponent.IsVisible = ((XUiC_ItemStack)this.itemStacks[i]).ItemStack.IsEmpty() && !xuiC_ItemStack.ToolLock;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiC_SlotPreview_ToolLockChangedEvent(int slotNumber, ItemStack stack, bool locked)
	{
		this.slots[slotNumber].ViewComponent.IsVisible = stack.IsEmpty() && !locked;
	}

	public override void OnClose()
	{
		for (int i = 0; i < this.itemStacks.Length; i++)
		{
			((XUiC_ItemStack)this.itemStacks[i]).SlotChangedEvent -= this.XUiC_SlotPreview_SlotChangedEvent;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiC_SlotPreview_SlotChangedEvent(int slotNumber, ItemStack stack)
	{
		this.slots[slotNumber].ViewComponent.IsVisible = stack.IsEmpty();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController[] slots;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController[] itemStacks;
}
