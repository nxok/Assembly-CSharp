using System;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DewCollectorContainer : XUiC_VariableHeightGrid, ITileEntityChangedListener
{
	public Vector2i GridCellSize
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override ItemStack[] GetSlots()
	{
		return this.localTileEntity.Items;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetStacks(ItemStack[] stackList)
	{
	}

	public void SetTileEntity(TileEntityCollector _te)
	{
		if (_te != null)
		{
			base.SetHeight(_te.OutputWindowHeight);
		}
	}

	public void SetSlots(TileEntityCollector lootContainer, ItemStack[] stackList)
	{
		if (stackList == null)
		{
			return;
		}
		this.localTileEntity = lootContainer;
		this.items = this.localTileEntity.Items;
		XUiC_ItemInfoWindow childByType = this.xui.GetChildByType<XUiC_ItemInfoWindow>();
		XUiV_Grid xuiV_Grid = (XUiV_Grid)this.viewComponent;
		xuiV_Grid.Columns = lootContainer.GetContainerSize().x;
		xuiV_Grid.Rows = lootContainer.GetContainerSize().y;
		int num = stackList.Length;
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			XUiC_ItemStack xuiC_ItemStack = this.itemControllers[i];
			xuiC_ItemStack.InfoWindow = childByType;
			xuiC_ItemStack.SlotNumber = i;
			xuiC_ItemStack.SlotChangedEvent -= this.HandleLootSlotChangedEvent;
			xuiC_ItemStack.InfoWindow = childByType;
			xuiC_ItemStack.OverrideStackCount = 1;
			xuiC_ItemStack.StackLocation = XUiC_ItemStack.StackLocationTypes.LootContainer;
			if (i < num)
			{
				this.SetItemInSlot(i, this.localTileEntity.Items[i], false);
				this.itemControllers[i].ViewComponent.IsVisible = true;
				xuiC_ItemStack.SlotChangedEvent += this.HandleLootSlotChangedEvent;
			}
			else
			{
				xuiC_ItemStack.ItemStack = ItemStack.Empty;
				this.itemControllers[i].ViewComponent.IsVisible = false;
			}
		}
		this.localTileEntity.SetModified();
		this.windowGroup.Controller.SetAllChildrenDirty();
	}

	public override void Init()
	{
		base.Init();
		XUiV_Grid xuiV_Grid = (XUiV_Grid)this.viewComponent;
		this.GridCellSize = new Vector2i(xuiV_Grid.CellWidth, xuiV_Grid.CellHeight);
		base.InitVariableHeightGrids(new XUiV_Grid[] { xuiV_Grid });
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			XUiC_DewCollectorStack xuiC_DewCollectorStack = (XUiC_DewCollectorStack)this.itemControllers[i];
			xuiC_DewCollectorStack.SetAllowedItemClassSingle(this.requiredItem);
			xuiC_DewCollectorStack.RequiredItemOnly = true;
			xuiC_DewCollectorStack.TakeOnly = true;
		}
	}

	public void HandleLootSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		this.localTileEntity.UpdateSlot(slotNumber, stack);
		this.localTileEntity.SetModified();
	}

	public void OnTileEntityChanged(ITileEntity _te)
	{
		TileEntityCollector tileEntityCollector = _te as TileEntityCollector;
		base.SetHeight(tileEntityCollector.OutputWindowHeight);
		ItemStack[] slots = this.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			XUiC_ItemStack xuiC_ItemStack = this.itemControllers[i];
			xuiC_ItemStack.SlotChangedEvent -= this.HandleLootSlotChangedEvent;
			this.SetItemInSlot(i, slots[i], true);
			xuiC_ItemStack.SlotChangedEvent += this.HandleLootSlotChangedEvent;
		}
	}

	public void SetItemInSlot(int i, ItemStack stack, bool onTEChanged)
	{
		ItemStack itemStack = ItemStack.Empty;
		if (stack != null)
		{
			itemStack = stack.Clone();
		}
		XUiC_ItemStack xuiC_ItemStack = this.itemControllers[i];
		ItemStack itemStack2 = ItemStack.Empty;
		if (xuiC_ItemStack != null)
		{
			if (xuiC_ItemStack.ItemStack != null)
			{
				itemStack2 = xuiC_ItemStack.ItemStack;
			}
			xuiC_ItemStack.ItemStack = itemStack.Clone();
		}
		if (onTEChanged && itemStack2.IsEmpty() && !itemStack.IsEmpty())
		{
			Manager.PlayInsidePlayerHead(this.localTileEntity.GetSlotOutputType(i).ConvertSound, -1, 0f, false, false);
		}
		XUiC_DewCollectorStack xuiC_DewCollectorStack = (XUiC_DewCollectorStack)xuiC_ItemStack;
		if (xuiC_DewCollectorStack != null)
		{
			TileEntityCollector.FillData slotFillData = this.localTileEntity.GetSlotFillData(i);
			if (slotFillData != null)
			{
				xuiC_DewCollectorStack.FillAmount = (float)(slotFillData.fillTime - slotFillData.fillTimeLeft);
				xuiC_DewCollectorStack.MaxFill = (float)slotFillData.fillTime;
			}
			xuiC_DewCollectorStack.IsCurrentStack = this.localTileEntity.IsCurrentStack(i);
			xuiC_DewCollectorStack.IsBlocked = this.localTileEntity.IsSlotDisabled(i);
			xuiC_DewCollectorStack.IsModded = this.localTileEntity.HasModConvert;
			xuiC_DewCollectorStack.RefreshBindings();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!this.localTileEntity.listeners.Contains(this))
		{
			this.localTileEntity.listeners.Add(this);
		}
		this.localTileEntity.Destroyed += this.LocalTileEntity_Destroyed;
		this.blockValue = GameManager.Instance.World.GetBlock(this.localTileEntity.ToWorldPos());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LocalTileEntity_Destroyed(ITileEntity te)
	{
		if (GameManager.Instance != null)
		{
			if (te == this.localTileEntity)
			{
				XUiC_DewCollectorWindowGroup.CloseIfOpenAtPos(te.ToWorldPos(), null);
				return;
			}
			te.Destroyed -= this.LocalTileEntity_Destroyed;
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
			this.localTileEntity = null;
		}
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		if (_name == "required_item")
		{
			this.requiredItem = _value;
			return true;
		}
		return base.ParseAttribute(_name, _value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityCollector localTileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public string requiredItem = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockValue;
}
