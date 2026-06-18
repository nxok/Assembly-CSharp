using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BackpackWindow : XUiController
{
	public bool UserLockMode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.userLockMode;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value == this.userLockMode)
			{
				return;
			}
			if (this.userLockMode)
			{
				this.UpdateLockedSlots(this.standardControls);
			}
			XUiC_ContainerStandardControls xuiC_ContainerStandardControls = this.standardControls;
			if (xuiC_ContainerStandardControls != null)
			{
				xuiC_ContainerStandardControls.LockModeChanged(value);
			}
			this.userLockMode = value;
			base.WindowGroup.isEscClosable = !this.userLockMode;
			this.xui.playerUI.windowManager.GetModalWindow().isEscClosable = !this.userLockMode;
			base.RefreshBindings();
		}
	}

	public override void Init()
	{
		base.Init();
		this.backpackGrid = base.GetChildByType<XUiC_Backpack>();
		this.standardControls = base.GetChildByType<XUiC_ContainerStandardControls>();
		if (this.standardControls != null)
		{
			this.standardControls.GetLockedSlotsFromStorage = () => this.xui.playerUI.entityPlayer.bag.LockedSlots;
			this.standardControls.SetLockedSlotsToStorage = delegate(PackedBoolArray _slots)
			{
				this.xui.playerUI.entityPlayer.bag.LockedSlots = _slots;
			};
			this.standardControls.ApplyLockedSlotStates = new Action<PackedBoolArray>(this.ApplyLockedSlotStates);
			this.standardControls.UpdateLockedSlotStates = new Action<XUiC_ContainerStandardControls>(this.UpdateLockedSlots);
			this.standardControls.LockModeToggled = delegate
			{
				this.UserLockMode = !this.UserLockMode;
			};
			this.standardControls.SortPressed = new Action<PackedBoolArray>(this.BtnSort_OnPress);
			this.standardControls.MoveAllowed = delegate(out XUiController _parentWindow, out XUiC_ItemStackGrid _grid, out IInventory _inventory)
			{
				_parentWindow = this;
				_grid = this.backpackGrid;
				return this.TryGetMoveDestinationInventory(out _inventory);
			};
		}
		XUiController childById = base.GetChildById("btnClearInventory");
		if (childById != null)
		{
			childById.OnPress += this.BtnClearInventory_OnPress;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetMoveDestinationInventory(out IInventory _dstInventory)
	{
		_dstInventory = null;
		XUiM_AssembleItem assembleItem = this.xui.AssembleItem;
		if (((assembleItem != null) ? assembleItem.CurrentItem : null) != null)
		{
			return false;
		}
		if (this.xui.LootContainer != null)
		{
			_dstInventory = this.xui.LootContainer;
			return true;
		}
		XUiC_BagStorageWindowGroup xuiC_BagStorageWindowGroup = this.xui.FindWindowGroupByName(XUiC_BagStorageWindowGroup.ID) as XUiC_BagStorageWindowGroup;
		if (xuiC_BagStorageWindowGroup != null && xuiC_BagStorageWindowGroup.Bag != null)
		{
			_dstInventory = xuiC_BagStorageWindowGroup.Bag;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnClearInventory_OnPress(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.entityPlayer.EmptyBackpack();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSort_OnPress(PackedBoolArray _ignoredSlots)
	{
		ItemStack itemStack = null;
		if (this.xui.AssembleItem.CurrentItemStackController != null)
		{
			itemStack = this.xui.AssembleItem.CurrentItemStackController.ItemStack;
		}
		this.xui.PlayerInventory.SortStacks(0, _ignoredSlots);
		if (itemStack != null)
		{
			base.GetChildByType<XUiC_ItemStackGrid>().AssembleLockSingleStack(itemStack);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyLockedSlotStates(PackedBoolArray _lockedSlots)
	{
		XUiC_ItemStack[] itemStackControllers = this.backpackGrid.GetItemStackControllers();
		for (int i = 0; i < itemStackControllers.Length; i++)
		{
			itemStackControllers[i].UserLockedSlot = _lockedSlots != null && i < _lockedSlots.Length && _lockedSlots[i];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLockedSlots(XUiC_ContainerStandardControls _csc)
	{
		if (_csc == null)
		{
			return;
		}
		int slotCount = this.xui.PlayerInventory.Backpack.SlotCount;
		PackedBoolArray packedBoolArray = _csc.LockedSlots ?? new PackedBoolArray(slotCount);
		if (packedBoolArray.Length != slotCount)
		{
			packedBoolArray.Length = slotCount;
		}
		XUiC_ItemStack[] itemStackControllers = this.backpackGrid.GetItemStackControllers();
		int num = 0;
		while (num < itemStackControllers.Length && num < packedBoolArray.Length)
		{
			packedBoolArray[num] = itemStackControllers[num].UserLockedSlot;
			num++;
		}
		_csc.LockedSlots = packedBoolArray;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (bindingName == "currencyamount")
		{
			value = "0";
			if (XUi.IsGameRunning() && this.xui != null && this.xui.PlayerInventory != null)
			{
				value = this.currencyFormatter.Format(this.xui.PlayerInventory.CurrencyAmount);
			}
			return true;
		}
		if (bindingName == "currencyicon")
		{
			value = TraderInfo.CurrencyItem;
			return true;
		}
		if (bindingName == "lootingorvehiclestorage")
		{
			IInventory inventory;
			value = this.TryGetMoveDestinationInventory(out inventory).ToString();
			return true;
		}
		if (bindingName == "creativewindowopen")
		{
			value = this.xui.playerUI.windowManager.IsWindowOpen("creative").ToString();
			return true;
		}
		if (!(bindingName == "userlockmode"))
		{
			return base.GetBindingValueInternal(ref value, bindingName);
		}
		value = this.UserLockMode.ToString();
		return true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.xui.PlayerInventory.RefreshCurrency();
		this.xui.PlayerInventory.OnCurrencyChanged += this.PlayerInventory_OnCurrencyChanged;
		if (!string.IsNullOrEmpty(XUiC_BackpackWindow.defaultSelectedElement))
		{
			base.GetChildById(XUiC_BackpackWindow.defaultSelectedElement).SelectCursorElement(true, false);
			XUiC_BackpackWindow.defaultSelectedElement = "";
		}
		GameManager.Instance.StartCoroutine(this.xui.playerUI.entityPlayer.CancelInventoryActions(delegate
		{
		}, false));
	}

	public override void OnClose()
	{
		base.OnClose();
		this.UserLockMode = false;
		if (this.xui != null && this.xui.PlayerInventory != null)
		{
			this.xui.PlayerInventory.OnCurrencyChanged -= this.PlayerInventory_OnCurrencyChanged;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.UpdateInput();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateInput()
	{
		PlayerActionsLocal playerInput = this.xui.playerUI.playerInput;
		if (this.UserLockMode && (playerInput.GUIActions.Cancel.WasPressed || playerInput.PermanentActions.Cancel.WasPressed))
		{
			this.UserLockMode = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnCurrencyChanged()
	{
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Backpack backpackGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isHidden;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool userLockMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ContainerStandardControls standardControls;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt currencyFormatter = new CachedStringFormatterInt();

	public static string defaultSelectedElement;
}
