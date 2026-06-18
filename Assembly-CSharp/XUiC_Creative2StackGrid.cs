using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Creative2StackGrid : XUiC_ItemStackGrid
{
	public int Length
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int Page
	{
		get
		{
			return this.page;
		}
		set
		{
			this.page = value;
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		this.Length = this.itemControllers.Length;
		this.IsDirty = false;
	}

	public override ItemStack[] GetSlots()
	{
		return this.items;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetStacks(ItemStack[] stackList)
	{
	}

	public void SetSlots(ItemStack[] stackList)
	{
		if (stackList == null)
		{
			return;
		}
		this.items = stackList;
		this.IsDirty = true;
		XUiC_ItemInfoWindow childByType = this.xui.GetChildByType<XUiC_ItemInfoWindow>();
		for (int i = 0; i < this.Length; i++)
		{
			XUiC_ItemStack xuiC_ItemStack = this.itemControllers[i];
			xuiC_ItemStack.InfoWindow = childByType;
			xuiC_ItemStack.StackLocation = XUiC_ItemStack.StackLocationTypes.Creative;
			this.itemControllers[i].ViewComponent.IsVisible = true;
		}
	}

	public override void Update(float _dt)
	{
		if (!base.ViewComponent.IsVisible)
		{
			return;
		}
		if (GameManager.Instance == null && GameManager.Instance.World == null)
		{
			return;
		}
		if (this.IsDirty && this.xui.PlayerInventory != null)
		{
			for (int i = 0; i < this.Length; i++)
			{
				int num = i + this.Length * this.page;
				XUiC_ItemStack xuiC_ItemStack = this.itemControllers[i];
				if (xuiC_ItemStack != null)
				{
					if (num < this.items.Length)
					{
						xuiC_ItemStack.ItemStack = this.items[num];
					}
					else
					{
						xuiC_ItemStack.ItemStack = ItemStack.Empty;
						if (xuiC_ItemStack.IsSelected)
						{
							xuiC_ItemStack.IsSelected = false;
						}
					}
				}
			}
			this.IsDirty = false;
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;
}
