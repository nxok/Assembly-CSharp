using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LootContainer : XUiC_ItemStackGrid, ITileEntityChangedListener
{
	public Vector2i GridCellSize
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override ItemStack[] GetSlots()
	{
		return this.localTileEntity.items;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] _stackList)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetStacks(ItemStack[] _stackList)
	{
	}

	public void SetSlots(ITileEntityLootable _lootContainer, ItemStack[] _stackList)
	{
		if (_stackList == null)
		{
			return;
		}
		this.localTileEntity = _lootContainer;
		this.items = this.localTileEntity.items;
		XUiC_ItemInfoWindow childByType = this.xui.GetChildByType<XUiC_ItemInfoWindow>();
		XUiV_Grid xuiV_Grid = (XUiV_Grid)this.viewComponent;
		xuiV_Grid.Columns = _lootContainer.GetContainerSize().x;
		xuiV_Grid.Rows = _lootContainer.GetContainerSize().y;
		int num = _stackList.Length;
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			XUiC_ItemStack xuiC_ItemStack = this.itemControllers[i];
			xuiC_ItemStack.InfoWindow = childByType;
			xuiC_ItemStack.SlotNumber = i;
			xuiC_ItemStack.SlotChangedEvent -= this.HandleLootSlotChangedEvent;
			xuiC_ItemStack.InfoWindow = childByType;
			xuiC_ItemStack.StackLocation = XUiC_ItemStack.StackLocationTypes.LootContainer;
			if (i < num)
			{
				xuiC_ItemStack.ForceSetItemStack(this.localTileEntity.items[i].Clone());
				this.itemControllers[i].ViewComponent.IsVisible = true;
				xuiC_ItemStack.SlotChangedEvent += this.HandleLootSlotChangedEvent;
			}
			else
			{
				xuiC_ItemStack.ItemStack = ItemStack.Empty;
				this.itemControllers[i].ViewComponent.IsVisible = false;
			}
		}
		this.windowGroup.Controller.SetAllChildrenDirty();
	}

	public override void Init()
	{
		base.Init();
		XUiV_Grid xuiV_Grid = (XUiV_Grid)this.viewComponent;
		this.GridCellSize = new Vector2i(xuiV_Grid.CellWidth, xuiV_Grid.CellHeight);
	}

	public void HandleLootSlotChangedEvent(int _slotNumber, ItemStack _stack)
	{
		this.localTileEntity.UpdateSlot(_slotNumber, _stack);
		this.localTileEntity.SetModified();
	}

	public void OnTileEntityChanged(ITileEntity _te)
	{
		ItemStack[] slots = this.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			XUiC_ItemStack xuiC_ItemStack = this.itemControllers[i];
			xuiC_ItemStack.SlotChangedEvent -= this.HandleLootSlotChangedEvent;
			xuiC_ItemStack.ItemStack = slots[i];
			xuiC_ItemStack.SlotChangedEvent += this.HandleLootSlotChangedEvent;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!this.localTileEntity.listeners.Contains(this))
		{
			this.localTileEntity.listeners.Add(this);
		}
		this.xui.LootContainer = this.localTileEntity;
		this.localTileEntity.Destroyed += this.LocalTileEntity_Destroyed;
		QuestEventManager.Current.OpenedContainer(this.localTileEntity.ToWorldPos(), this.localTileEntity);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LocalTileEntity_Destroyed(ITileEntity _te)
	{
		if (GameManager.Instance != null)
		{
			if (_te == this.localTileEntity)
			{
				this.xui.playerUI.windowManager.Close(XUiC_LootWindowGroup.ID);
				return;
			}
			_te.Destroyed -= this.LocalTileEntity_Destroyed;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.LootContainer = null;
		if (this.localTileEntity != null)
		{
			this.localTileEntity.Destroyed -= this.LocalTileEntity_Destroyed;
			if (this.localTileEntity.listeners.Contains(this))
			{
				this.localTileEntity.listeners.Remove(this);
			}
			QuestEventManager.Current.ClosedContainer(this.localTileEntity.ToWorldPos(), this.localTileEntity);
			this.localTileEntity = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ITileEntityLootable localTileEntity;
}
