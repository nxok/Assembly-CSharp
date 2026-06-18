using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ContainerStandardControls : XUiController
{
	public PackedBoolArray LockedSlots
	{
		get
		{
			Func<PackedBoolArray> getLockedSlotsFromStorage = this.GetLockedSlotsFromStorage;
			if (getLockedSlotsFromStorage == null)
			{
				return null;
			}
			return getLockedSlotsFromStorage();
		}
		set
		{
			Action<PackedBoolArray> setLockedSlotsToStorage = this.SetLockedSlotsToStorage;
			if (setLockedSlotsToStorage == null)
			{
				return;
			}
			setLockedSlotsToStorage(value);
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController xuiController = base.GetChildById("btnSort");
		if (xuiController != null)
		{
			xuiController.OnPress += delegate(XUiController _, int _)
			{
				this.Sort();
			};
		}
		xuiController = base.GetChildById("btnMoveAll");
		if (xuiController != null)
		{
			xuiController.OnPress += delegate(XUiController _, int _)
			{
				this.MoveAll();
			};
		}
		xuiController = base.GetChildById("btnMoveFillAndSmart");
		if (xuiController != null)
		{
			xuiController.OnPress += delegate(XUiController _, int _)
			{
				this.MoveFillAndSmart();
			};
		}
		xuiController = base.GetChildById("btnMoveFillStacks");
		if (xuiController != null)
		{
			xuiController.OnPress += delegate(XUiController _, int _)
			{
				this.MoveFillStacks();
			};
		}
		xuiController = base.GetChildById("btnMoveSmart");
		if (xuiController != null)
		{
			xuiController.OnPress += delegate(XUiController _, int _)
			{
				this.MoveSmart();
			};
		}
		xuiController = base.GetChildById("btnToggleLockMode");
		if (xuiController != null)
		{
			xuiController.OnPress += delegate(XUiController _, int _)
			{
				this.ToggleLockMode();
			};
			XUiV_Button xuiV_Button = xuiController.ViewComponent as XUiV_Button;
			if (xuiV_Button != null)
			{
				this.lockModeButtonColorTweener = xuiV_Button.UiTransform.gameObject.GetOrAddComponent<TweenColor>();
				this.lockModeButtonColorTweener.from = xuiV_Button.DefaultSpriteColor;
				this.lockModeButtonColorTweener.to = xuiV_Button.SelectedSpriteColor;
				this.lockModeButtonColorTweener.style = 2;
				this.lockModeButtonColorTweener.enabled = false;
				this.lockModeButtonColorTweener.duration = 0.4f;
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		Action<PackedBoolArray> applyLockedSlotStates = this.ApplyLockedSlotStates;
		if (applyLockedSlotStates == null)
		{
			return;
		}
		applyLockedSlotStates(this.LockedSlots);
	}

	public void Sort()
	{
		Action<XUiC_ContainerStandardControls> updateLockedSlotStates = this.UpdateLockedSlotStates;
		if (updateLockedSlotStates != null)
		{
			updateLockedSlotStates(this);
		}
		Action<PackedBoolArray> sortPressed = this.SortPressed;
		if (sortPressed == null)
		{
			return;
		}
		sortPressed(this.LockedSlots);
	}

	public void MoveAll()
	{
		Action<XUiC_ContainerStandardControls> updateLockedSlotStates = this.UpdateLockedSlotStates;
		if (updateLockedSlotStates != null)
		{
			updateLockedSlotStates(this);
		}
		XUiController xuiController;
		XUiC_ItemStackGrid xuiC_ItemStackGrid;
		IInventory inventory;
		if (this.MoveAllowed(out xuiController, out xuiC_ItemStackGrid, out inventory))
		{
			ValueTuple<bool, bool> valueTuple = XUiM_LootContainer.StashItems(xuiController, xuiC_ItemStackGrid, inventory, 0, this.LockedSlots, XUiM_LootContainer.EItemMoveKind.All, this.MoveStartBottomRight);
			bool item = valueTuple.Item1;
			bool item2 = valueTuple.Item2;
			Action<bool, bool> moveAllDone = this.MoveAllDone;
			if (moveAllDone == null)
			{
				return;
			}
			moveAllDone(item, item2);
		}
	}

	public void MoveFillAndSmart()
	{
		Action<XUiC_ContainerStandardControls> updateLockedSlotStates = this.UpdateLockedSlotStates;
		if (updateLockedSlotStates != null)
		{
			updateLockedSlotStates(this);
		}
		XUiController xuiController;
		XUiC_ItemStackGrid xuiC_ItemStackGrid;
		IInventory inventory;
		if (this.MoveAllowed(out xuiController, out xuiC_ItemStackGrid, out inventory))
		{
			XUiM_LootContainer.StashItems(xuiController, xuiC_ItemStackGrid, inventory, 0, this.LockedSlots, XUiM_LootContainer.EItemMoveKind.FillOnlyFirstCreateSecond, this.MoveStartBottomRight);
		}
	}

	public void MoveFillStacks()
	{
		Action<XUiC_ContainerStandardControls> updateLockedSlotStates = this.UpdateLockedSlotStates;
		if (updateLockedSlotStates != null)
		{
			updateLockedSlotStates(this);
		}
		XUiController xuiController;
		XUiC_ItemStackGrid xuiC_ItemStackGrid;
		IInventory inventory;
		if (this.MoveAllowed(out xuiController, out xuiC_ItemStackGrid, out inventory))
		{
			XUiM_LootContainer.StashItems(xuiController, xuiC_ItemStackGrid, inventory, 0, this.LockedSlots, XUiM_LootContainer.EItemMoveKind.FillOnly, this.MoveStartBottomRight);
		}
	}

	public void MoveSmart()
	{
		Action<XUiC_ContainerStandardControls> updateLockedSlotStates = this.UpdateLockedSlotStates;
		if (updateLockedSlotStates != null)
		{
			updateLockedSlotStates(this);
		}
		XUiController xuiController;
		XUiC_ItemStackGrid xuiC_ItemStackGrid;
		IInventory inventory;
		if (this.MoveAllowed(out xuiController, out xuiC_ItemStackGrid, out inventory))
		{
			XUiM_LootContainer.StashItems(xuiController, xuiC_ItemStackGrid, inventory, 0, this.LockedSlots, XUiM_LootContainer.EItemMoveKind.FillAndCreate, this.MoveStartBottomRight);
		}
	}

	public void ToggleLockMode()
	{
		Action<XUiC_ContainerStandardControls> updateLockedSlotStates = this.UpdateLockedSlotStates;
		if (updateLockedSlotStates != null)
		{
			updateLockedSlotStates(this);
		}
		Action lockModeToggled = this.LockModeToggled;
		if (lockModeToggled == null)
		{
			return;
		}
		lockModeToggled();
	}

	public void LockModeChanged(bool _state)
	{
		if (this.lockModeButtonColorTweener != null)
		{
			this.lockModeButtonColorTweener.enabled = _state;
		}
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		if (_name == "move_start_bottom_left")
		{
			this.MoveStartBottomRight = StringParsers.ParseBool(_value, 0, -1, true);
			return true;
		}
		return base.ParseAttribute(_name, _value);
	}

	public Func<PackedBoolArray> GetLockedSlotsFromStorage;

	public Action<PackedBoolArray> SetLockedSlotsToStorage;

	public Action<PackedBoolArray> ApplyLockedSlotStates;

	public Action<XUiC_ContainerStandardControls> UpdateLockedSlotStates;

	public Action<PackedBoolArray> SortPressed;

	public Action LockModeToggled;

	public XUiC_ContainerStandardControls.MoveAllowedDelegate MoveAllowed;

	public Action<bool, bool> MoveAllDone;

	public bool MoveStartBottomRight;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool LockModeEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public TweenColor lockModeButtonColorTweener;

	public delegate bool MoveAllowedDelegate(out XUiController _parentWindow, out XUiC_ItemStackGrid _sourceGrid, out IInventory _destinationInventory);
}
