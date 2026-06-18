using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PartList : XUiC_ItemStackGrid
{
	public override void Init()
	{
		base.Init();
		foreach (XUiC_ItemStack xuiC_ItemStack in this.itemControllers)
		{
			xuiC_ItemStack.ViewComponent.IsNavigatable = false;
			xuiC_ItemStack.StackLocation = XUiC_ItemStack.StackLocationTypes.Part;
		}
	}

	public override ItemStack[] GetSlots()
	{
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
	}

	public void SetSlot(ItemValue part, int index)
	{
		XUiC_ItemStack xuiC_ItemStack = this.itemControllers[index];
		if (part != null && !part.IsEmpty())
		{
			ItemStack itemStack = new ItemStack(part.Clone(), 1);
			xuiC_ItemStack.ItemStack = itemStack;
			xuiC_ItemStack.GreyedOut = false;
		}
		else
		{
			xuiC_ItemStack.ItemStack = ItemStack.Empty;
			xuiC_ItemStack.GreyedOut = false;
		}
		this.itemControllers[index].ViewComponent.EventOnPress = false;
		this.itemControllers[index].ViewComponent.EventOnHover = false;
	}

	public void SetSlots(ItemValue[] parts, int startIndex = 0)
	{
		for (int i = 0; i < this.itemControllers.Length - startIndex; i++)
		{
			int num = i + startIndex;
			XUiC_ItemStack xuiC_ItemStack = this.itemControllers[num];
			if (parts.Length > i && parts[i] != null && !parts[i].IsEmpty())
			{
				ItemStack itemStack = new ItemStack(parts[i].Clone(), 1);
				xuiC_ItemStack.ItemStack = itemStack;
				xuiC_ItemStack.GreyedOut = false;
			}
			else
			{
				xuiC_ItemStack.ItemStack = ItemStack.Empty;
				xuiC_ItemStack.GreyedOut = false;
			}
			xuiC_ItemStack.ViewComponent.EventOnPress = false;
			xuiC_ItemStack.ViewComponent.EventOnHover = false;
		}
	}

	public void SetAmmoSlot(ItemValue ammo, int count)
	{
		int num = 5;
		int num2 = ((count < 1) ? 1 : count);
		if (ammo != null && !ammo.IsEmpty())
		{
			ItemStack itemStack = new ItemStack(ammo, num2);
			this.itemControllers[num].ItemStack = itemStack;
			this.itemControllers[num].GreyedOut = count == 0;
			return;
		}
		this.itemControllers[num].ItemStack = ItemStack.Empty;
		this.itemControllers[num].GreyedOut = false;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetMainItem(ItemStack itemStack)
	{
		this.itemClass = itemStack.itemValue.ItemClass;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass itemClass;
}
