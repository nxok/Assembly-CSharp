using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemPartStackGrid : XUiController
{
	public virtual XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Backpack;
		}
	}

	public ItemStack CurrentItem { get; set; }

	public XUiC_AssembleWindow AssembleWindow { get; set; }

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = base.GetChildrenByType<XUiC_ItemPartStack>(null);
		this.itemControllers = childrenByType;
		this.IsDirty = false;
	}

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null && GameManager.Instance.World == null)
		{
			return;
		}
		base.Update(_dt);
	}

	public void SetParts(ItemValue[] stackList)
	{
		if (stackList == null)
		{
			return;
		}
		this.currentItemClass = this.CurrentItem.itemValue.ItemClass;
		XUiC_ItemInfoWindow childByType = this.xui.GetChildByType<XUiC_ItemInfoWindow>();
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			XUiC_ItemPartStack xuiC_ItemPartStack = (XUiC_ItemPartStack)this.itemControllers[i];
			if (i < this.CurrentItem.itemValue.Modifications.Length)
			{
				ItemValue itemValue = this.CurrentItem.itemValue.Modifications[i];
				if (itemValue != null && itemValue.ItemClass is ItemClassModifier)
				{
					xuiC_ItemPartStack.SlotType = (itemValue.ItemClass as ItemClassModifier).Type.ToStringCached<ItemClassModifier.ModifierTypes>().ToLower();
				}
				xuiC_ItemPartStack.SlotChangedEvent -= this.HandleSlotChangedEvent;
				xuiC_ItemPartStack.ItemValue = ((itemValue != null) ? itemValue : ItemValue.None);
				xuiC_ItemPartStack.SlotChangedEvent += this.HandleSlotChangedEvent;
				xuiC_ItemPartStack.SlotNumber = i;
				xuiC_ItemPartStack.InfoWindow = childByType;
				xuiC_ItemPartStack.StackLocation = this.StackLocation;
				xuiC_ItemPartStack.ViewComponent.IsVisible = true;
			}
			else
			{
				xuiC_ItemPartStack.ViewComponent.IsVisible = false;
			}
		}
	}

	public void HandleSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		XUiC_ItemPartStack xuiC_ItemPartStack = (XUiC_ItemPartStack)this.itemControllers[slotNumber];
		ItemValue itemValue = (xuiC_ItemPartStack.ItemStack.IsEmpty() ? ItemValue.None : xuiC_ItemPartStack.ItemStack.itemValue);
		if (itemValue.ItemClass != null)
		{
			ItemClassModifier itemClassModifier = itemValue.ItemClass as ItemClassModifier;
			if (itemClassModifier != null)
			{
				if (itemClassModifier.ModifierTags.Test_AnySet(ItemClassModifier.CosmeticModTypes) && this.CurrentItem.itemValue.CosmeticMods.Length != 0)
				{
					this.CurrentItem.itemValue.CosmeticMods[0] = itemValue;
					goto IL_00A7;
				}
				this.CurrentItem.itemValue.Modifications[slotNumber] = itemValue;
				goto IL_00A7;
			}
		}
		this.CurrentItem.itemValue.Modifications[slotNumber] = itemValue;
		IL_00A7:
		this.AssembleWindow.ItemStack = this.CurrentItem;
		this.AssembleWindow.OnChanged();
		this.xui.AssembleItem.RefreshAssembleItem();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateBackend(ItemStack[] stackList)
	{
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = true;
		}
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int curPageIdx;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int numPages;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController[] itemControllers;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemStack[] items;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass currentItemClass;
}
