using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CombineGrid : XUiC_ItemStackGrid
{
	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = base.GetChildrenByType<XUiC_ItemStack>(null);
		XUiController[] array = childrenByType;
		if (array.Length == 3)
		{
			this.merge1 = (XUiC_RequiredItemStack)array[0];
			this.merge2 = (XUiC_RequiredItemStack)array[1];
			this.result1 = (XUiC_RequiredItemStack)array[2];
			this.merge1.StackLocation = XUiC_ItemStack.StackLocationTypes.Merge;
			this.merge2.StackLocation = XUiC_ItemStack.StackLocationTypes.Merge;
			this.result1.StackLocation = XUiC_ItemStack.StackLocationTypes.Merge;
			this.merge1.RequiredType = (this.merge2.RequiredType = (this.result1.RequiredType = XUiC_RequiredItemStack.RequiredTypes.HasQualityNoParts));
			this.merge1.SlotChangedEvent += this.Merge_SlotChangedEvent;
			this.merge2.SlotChangedEvent += this.Merge_SlotChangedEvent;
			this.result1.HiddenLock = true;
			this.result1.SlotChangedEvent += this.Result1_SlotChangedEvent;
			this.result1.TakeOnly = true;
			this.merge1.FailedSwap += this.Merge_FailedSwap;
			this.merge2.FailedSwap += this.Merge_FailedSwap;
		}
	}

	public void SetTileEntity(TEFeatureCombine _te)
	{
		this.te = _te;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Merge_FailedSwap(ItemStack stack)
	{
		ItemClass itemClass = stack.itemValue.ItemClass;
		if (!stack.itemValue.HasQuality || itemClass.HasSubItems)
		{
			GameManager.ShowTooltip(this.xui.playerUI.entityPlayer, Localization.Get("ttCombineInvalidItem", false, null), false, false, 0f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Result1_SlotChangedEvent(int slotNumber, ItemStack stack)
	{
		if (stack.IsEmpty())
		{
			ItemActionEntryScrap.HandleRemoveAmmo(this.merge1.ItemStack, this.xui);
			ItemActionEntryScrap.HandleRemoveAmmo(this.merge2.ItemStack, this.xui);
			this.merge1.ItemStack = ItemStack.Empty;
			this.merge2.ItemStack = ItemStack.Empty;
			if (this.lastResult != null)
			{
				this.xui.playerUI.entityPlayer.Progression.AddLevelExp((int)this.experienceFromLastResult, "_xpOther", Progression.XPTypes.Other, true, true, -1, null);
				this.lastResult = null;
			}
			if (this.te != null)
			{
				this.te.HandlePlayComplete();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Merge_SlotChangedEvent(int slotNumber, ItemStack stack)
	{
		if (this.merge1.ItemStack.IsEmpty() || this.merge2.ItemStack.IsEmpty() || this.merge1.ItemStack.itemValue.type != this.merge2.ItemStack.itemValue.type)
		{
			this.result1.SlotChangedEvent -= this.Result1_SlotChangedEvent;
			this.result1.ItemStack = ItemStack.Empty;
			this.result1.HiddenLock = true;
			this.result1.SlotChangedEvent += this.Result1_SlotChangedEvent;
			return;
		}
		ItemValue itemValue = this.merge1.ItemStack.itemValue;
		ItemValue itemValue2 = this.merge2.ItemStack.itemValue;
		if ((itemValue.HasMods() && itemValue2.HasMods()) || (itemValue.HasCosmetics() && itemValue2.HasCosmetics()))
		{
			GameManager.ShowTooltip(this.xui.playerUI.entityPlayer, string.Format(Localization.Get("ttCombineRemoveMods", false, null), 0), string.Empty, "ui_denied", null, false, false, 0f);
			return;
		}
		bool flag = true;
		if (!itemValue.EqualsForMerging(itemValue2))
		{
			ItemStack itemStack = this.merge1.ItemStack.Clone();
			itemStack.itemValue.MergeBest(this.merge2.ItemStack.itemValue);
			if ((itemStack.itemValue.Quality > itemValue.Quality || !itemStack.itemValue.EqualsForMerging(itemValue)) && (itemStack.itemValue.Quality > itemValue2.Quality || !itemStack.itemValue.EqualsForMerging(itemValue2)))
			{
				itemStack.itemValue.Meta = 0;
				this.lastResult = itemStack;
				this.result1.ItemStack = itemStack;
				this.result1.HiddenLock = false;
				flag = false;
			}
		}
		if (flag)
		{
			GameManager.ShowTooltip(this.xui.playerUI.entityPlayer, string.Format(Localization.Get("ttCombineSameItem", false, null), 0), string.Empty, "ui_denied", null, false, false, 0f);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		XUiC_ItemInfoWindow childByType = this.xui.GetChildByType<XUiC_ItemInfoWindow>();
		this.merge1.InfoWindow = childByType;
		this.merge2.InfoWindow = childByType;
		this.result1.InfoWindow = childByType;
		this.IsDirty = true;
		this.merge1.ItemStack = ItemStack.Empty;
		this.merge2.ItemStack = ItemStack.Empty;
		this.result1.ItemStack = ItemStack.Empty;
		this.xui.CurrentCombineGrid = this;
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
		XUiM_PlayerInventory playerInventory = this.xui.PlayerInventory;
		if (!this.merge1.ItemStack.IsEmpty() && !playerInventory.AddItem(this.merge1.ItemStack))
		{
			EntityPlayer entityPlayer = this.xui.playerUI.entityPlayer;
			GameManager.Instance.ItemDropServer(this.merge1.ItemStack, entityPlayer.GetPosition(), Vector3.zero, -1, 60f, false);
			entityPlayer.PlayOneShot("itemdropped", false, false, false, null, 1f);
		}
		if (!this.merge2.ItemStack.IsEmpty() && !playerInventory.AddItem(this.merge2.ItemStack))
		{
			EntityPlayer entityPlayer2 = this.xui.playerUI.entityPlayer;
			GameManager.Instance.ItemDropServer(this.merge2.ItemStack, entityPlayer2.GetPosition(), Vector3.zero, -1, 60f, false);
			entityPlayer2.PlayOneShot("itemdropped", false, false, false, null, 1f);
		}
		this.xui.CurrentCombineGrid = null;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool TryAddItemToSlot(ItemClass itemClass, ItemStack itemStack)
	{
		if (this.merge1.ItemStack.IsEmpty())
		{
			if (itemClass.HasQuality && !itemClass.HasSubItems)
			{
				this.merge1.ItemStack = itemStack;
				return true;
			}
		}
		else if (this.merge2.ItemStack.IsEmpty() && itemClass.HasQuality && !itemClass.HasSubItems)
		{
			this.merge2.ItemStack = itemStack;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RequiredItemStack merge1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RequiredItemStack merge2;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RequiredItemStack result1;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack lastResult;

	[PublicizedFrom(EAccessModifier.Private)]
	public float experienceFromLastResult;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureCombine te;
}
