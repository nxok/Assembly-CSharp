using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Creative2Stack : XUiC_ItemStack
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setItemStack(ItemStack _stack)
	{
		this.itemStack = _stack;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleDropOne()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleClickComplete()
	{
		this.lastClicked = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SwapItem()
	{
		this.xui.DragAndDropWindow.CurrentStack = this.itemStack.Clone();
		this.xui.DragAndDropWindow.PickUpType = base.StackLocation;
		base.HandleSlotChangeEvent();
	}
}
