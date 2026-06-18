using System;
using System.Collections;
using System.Globalization;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BagContainer : XUiC_ItemStackGrid
{
	public Bag Bag
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public Vector2i GridCellSize
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Vehicle;
		}
	}

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
			base.RefreshBindings();
		}
	}

	public override ItemStack[] GetSlots()
	{
		return this.items;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] _stackList)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetStacks(ItemStack[] _stackList)
	{
	}

	public override void Init()
	{
		base.Init();
		this.window = (XUiV_Window)this.viewComponent;
		this.grid = (XUiV_Grid)base.GetChildById("queue").ViewComponent;
		this.GridCellSize = new Vector2i(this.grid.CellWidth, this.grid.CellHeight);
		this.standardControls = base.GetChildByType<XUiC_ContainerStandardControls>();
		if (this.standardControls != null)
		{
			this.standardControls.GetLockedSlotsFromStorage = delegate
			{
				Bag bag = this.Bag;
				PackedBoolArray packedBoolArray;
				if ((packedBoolArray = bag.LockedSlots) == null)
				{
					packedBoolArray = (bag.LockedSlots = new PackedBoolArray(this.Bag.SlotCount));
				}
				return packedBoolArray;
			};
			this.standardControls.SetLockedSlotsToStorage = delegate(PackedBoolArray _slots)
			{
				this.Bag.LockedSlots = _slots;
			};
			this.standardControls.ApplyLockedSlotStates = new Action<PackedBoolArray>(this.ApplyLockedSlotStates);
			this.standardControls.UpdateLockedSlotStates = new Action<XUiC_ContainerStandardControls>(this.UpdateLockedSlots);
			this.standardControls.LockModeToggled = delegate
			{
				this.UserLockMode = !this.UserLockMode;
			};
			this.standardControls.SortPressed = new Action<PackedBoolArray>(this.btnSort_OnPress);
			this.standardControls.MoveAllowed = delegate(out XUiController _parentWindow, out XUiC_ItemStackGrid _grid, out IInventory _inventory)
			{
				_parentWindow = this;
				_grid = this;
				_inventory = this.xui.PlayerInventory;
				return true;
			};
			this.standardControls.MoveAllDone = delegate(bool _allMoved, bool _anyMoved)
			{
				if (_anyMoved)
				{
					Manager.BroadcastPlayByLocalPlayer(XUiM_Player.GetPlayer().position, "UseActions/takeall1");
				}
				if (_allMoved)
				{
					ThreadManager.StartCoroutine(this.closeInventoryLater());
				}
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnSort_OnPress(PackedBoolArray _ignoredSlots)
	{
		if (this.Bag == null)
		{
			return;
		}
		ItemStack[] array = StackSortUtil.CombineAndSortStacks(this.Bag.GetSlots(), 0, _ignoredSlots);
		this.Bag.SetSlots(array);
	}

	public void SetBag(Bag _bag, LootContainer _lootContainer, string _containerName, Action _onModified = null)
	{
		if (_bag == null || _lootContainer == null)
		{
			return;
		}
		this.Bag = _bag;
		this.containerName = _containerName;
		this.onBagModified = _onModified;
		this.grid.Columns = _lootContainer.size.x;
		this.grid.Rows = _lootContainer.size.y;
		ItemStack[] slots = _bag.GetSlots();
		int num = Mathf.CeilToInt((float)slots.Length / (float)this.grid.Columns);
		Vector2i vector2i = new Vector2i(this.grid.Columns * this.GridCellSize.x, num * this.GridCellSize.y);
		this.windowWidth = vector2i.x + this.windowGridWidthDifference;
		_bag.OnBackpackItemsChangedInternal += this.OnBagItemChangedInternal;
		this.items = slots;
		XUiC_ItemInfoWindow childByType = this.xui.GetChildByType<XUiC_ItemInfoWindow>();
		int num2 = slots.Length;
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			XUiC_ItemStack xuiC_ItemStack = this.itemControllers[i];
			xuiC_ItemStack.SlotNumber = i;
			xuiC_ItemStack.SlotChangedEvent -= this.HandleBagSlotChangedEvent;
			xuiC_ItemStack.InfoWindow = childByType;
			xuiC_ItemStack.StackLocation = XUiC_ItemStack.StackLocationTypes.LootContainer;
			xuiC_ItemStack.UnlockStack();
			if (i < num2)
			{
				xuiC_ItemStack.ForceSetItemStack(this.items[i]);
				this.itemControllers[i].ViewComponent.IsVisible = true;
				xuiC_ItemStack.SlotChangedEvent += this.HandleBagSlotChangedEvent;
			}
			else
			{
				xuiC_ItemStack.ItemStack = ItemStack.Empty;
				this.itemControllers[i].ViewComponent.IsVisible = false;
			}
		}
		base.RefreshBindings();
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

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null || GameManager.Instance.World == null)
		{
			return;
		}
		if (this.IsDirty)
		{
			base.ViewComponent.IsVisible = this.Bag != null;
			base.RefreshBindings();
			this.IsDirty = false;
		}
		base.Update(_dt);
		if (this.windowGroup.isShowing)
		{
			if (!this.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
			{
				this.wasReleased = true;
			}
			if (this.wasReleased)
			{
				if (this.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
				{
					this.activeKeyDown = true;
				}
				if (this.xui.playerUI.playerInput.PermanentActions.Activate.WasReleased && this.activeKeyDown)
				{
					this.activeKeyDown = false;
					this.OnClose();
					this.xui.playerUI.windowManager.CloseAllOpenModalWindows(null, false);
				}
			}
			if (!this.isClosing && base.ViewComponent != null && base.ViewComponent.IsVisible && this.items != null && (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard || !this.xui.playerUI.windowManager.IsInputActive()) && (this.xui.playerUI.playerInput.GUIActions.LeftStick.WasPressed || this.xui.playerUI.playerInput.PermanentActions.Reload.WasPressed))
			{
				this.standardControls.MoveAll();
			}
		}
		this.UpdateInput();
	}

	public void HandleBagSlotChangedEvent(int _slotNumber, ItemStack _stack)
	{
		if (this.Bag == null)
		{
			return;
		}
		this.Bag.SetSlot(_slotNumber, _stack, true);
	}

	public void OnBagItemChangedInternal()
	{
		if (this.Bag == null)
		{
			return;
		}
		ItemStack[] slots = this.Bag.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			this.SetItemInSlot(i, slots[i]);
		}
		Action action = this.onBagModified;
		if (action == null)
		{
			return;
		}
		action();
	}

	public void SetItemInSlot(int _i, ItemStack _stack)
	{
		if (_i >= this.itemControllers.Length)
		{
			return;
		}
		this.itemControllers[_i].ItemStack = _stack;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.wasReleased = false;
		this.activeKeyDown = false;
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.UserLockMode = false;
		if (this.Bag != null)
		{
			this.Bag.OnBackpackItemsChangedInternal -= this.OnBagItemChangedInternal;
		}
		this.Bag = null;
		this.onBagModified = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator closeInventoryLater()
	{
		this.isClosing = true;
		yield return null;
		this.xui.playerUI.windowManager.Close("bagStorage");
		this.isClosing = false;
		yield break;
	}

	public bool AddItem(ItemStack _itemStack)
	{
		if (!_itemStack.CanMoveTo(XUiC_ItemStack.StackLocationTypes.Backpack))
		{
			return false;
		}
		if (this.Bag == null)
		{
			return false;
		}
		this.Bag.TryStackItem(0, _itemStack);
		return _itemStack.count > 0 && this.Bag.AddItem(_itemStack);
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		if (_name == "window_grid_width_difference")
		{
			this.windowGridWidthDifference = StringParsers.ParseSInt32(_value, 0, -1, NumberStyles.Integer);
			return true;
		}
		return base.ParseAttribute(_name, _value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 775821220U)
		{
			if (num != 58930246U)
			{
				if (num != 610671175U)
				{
					if (num == 775821220U)
					{
						if (_bindingName == "lootcontainer_name")
						{
							_value = ((!string.IsNullOrEmpty(this.containerName)) ? this.containerName : Localization.Get("xuiStorage", false, null));
							return true;
						}
					}
				}
				else if (_bindingName == "windowWidth")
				{
					_value = this.windowWidth.ToString();
					return true;
				}
			}
			else if (_bindingName == "container_slots")
			{
				Bag bag = this.Bag;
				_value = ((bag != null) ? bag.SlotCount : 0).ToString();
				return true;
			}
		}
		else if (num <= 1899514017U)
		{
			if (num != 1249315359U)
			{
				if (num == 1899514017U)
				{
					if (_bindingName == "hasslotlocks")
					{
						XUiC_ContainerStandardControls xuiC_ContainerStandardControls = this.standardControls;
						_value = (((xuiC_ContainerStandardControls != null) ? xuiC_ContainerStandardControls.LockedSlots : null) != null).ToString();
						return true;
					}
				}
			}
			else if (_bindingName == "buttons_visible")
			{
				_value = (this.windowWidth >= 450).ToString();
				return true;
			}
		}
		else if (num != 2305669482U)
		{
			if (num == 4041168338U)
			{
				if (_bindingName == "take_all_tooltip")
				{
					_value = string.Format(Localization.Get("xuiLootTakeAllTooltip", false, null), "[action:permanent:Reload:emptystring:KeyboardWithAngleBrackets]");
					return true;
				}
			}
		}
		else if (_bindingName == "userlockmode")
		{
			_value = this.UserLockMode.ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyLockedSlotStates(PackedBoolArray _lockedSlots)
	{
		XUiC_ItemStack[] itemStackControllers = this.GetItemStackControllers();
		for (int i = 0; i < itemStackControllers.Length; i++)
		{
			itemStackControllers[i].UserLockedSlot = _lockedSlots != null && i < _lockedSlots.Length && _lockedSlots[i];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLockedSlots(XUiC_ContainerStandardControls _csc)
	{
		if (_csc == null || this.Bag == null)
		{
			return;
		}
		int slotCount = this.Bag.SlotCount;
		PackedBoolArray packedBoolArray = _csc.LockedSlots ?? new PackedBoolArray(slotCount);
		if (packedBoolArray.Length != slotCount)
		{
			packedBoolArray.Length = slotCount;
		}
		XUiC_ItemStack[] itemStackControllers = this.GetItemStackControllers();
		int num = 0;
		while (num < itemStackControllers.Length && num < packedBoolArray.Length)
		{
			packedBoolArray[num] = itemStackControllers[num].UserLockedSlot;
			num++;
		}
		_csc.LockedSlots = packedBoolArray;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isClosing;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Window window;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Grid grid;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool userLockMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ContainerStandardControls standardControls;

	[PublicizedFrom(EAccessModifier.Private)]
	public string containerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onBagModified;

	[PublicizedFrom(EAccessModifier.Private)]
	public int windowWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int windowGridWidthDifference;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;
}
