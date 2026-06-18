using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Toolbelt : XUiC_ItemStackGrid
{
	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.ToolBelt;
		}
	}

	public bool HasSecondRow
	{
		get
		{
			return this.itemControllers != null && this.backendSlotCount > this.itemControllers.Length / 2;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_Toolbelt.ID = base.WindowGroup.Id;
	}

	public override ItemStack[] GetSlots()
	{
		return this.xui.PlayerInventory.GetToolbeltItemStacks();
	}

	public override void Update(float _dt)
	{
		if (!XUi.IsGameRunning())
		{
			return;
		}
		Inventory toolbelt = this.xui.PlayerInventory.Toolbelt;
		if (this.currentHoldingIndex != toolbelt.GetFocusedItemIdx())
		{
			if (this.currentHoldingIndex != toolbelt.DUMMY_SLOT_IDX)
			{
				this.itemControllers[this.currentHoldingIndex].IsHolding = false;
			}
			this.currentHoldingIndex = toolbelt.GetFocusedItemIdx();
			if (this.currentHoldingIndex != toolbelt.DUMMY_SLOT_IDX)
			{
				this.itemControllers[this.currentHoldingIndex].IsHolding = true;
			}
		}
		if (Time.time > this.updateTime)
		{
			this.updateTime = Time.time + 0.5f;
			bool flag = toolbelt.IsHoldingItemActionRunning();
			if (this.lastActionRunning != flag)
			{
				this.currentHoldingIndex = toolbelt.holdingItemIdx;
				if (this.currentHoldingIndex != toolbelt.DUMMY_SLOT_IDX)
				{
					if (flag)
					{
						this.lastActionSlot = toolbelt.holdingItemIdx;
						this.itemControllers[toolbelt.holdingItemIdx].HiddenLock = true;
					}
					else
					{
						this.itemControllers[this.lastActionSlot].HiddenLock = flag;
					}
				}
				this.lastActionRunning = flag;
			}
			if (!GameManager.Instance.bCursorVisible)
			{
				this.ClearHoveredItems();
			}
		}
		this.UpdateQuickSwap();
		if (this.IsDirty)
		{
			base.RefreshBindings();
			this.IsDirty = false;
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		this.xui.PlayerInventory.SetToolbeltItemStacks(stackList);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateQuickSwap()
	{
		int quickSwapSlot = this.xui.PlayerInventory.QuickSwapSlot;
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			this.itemControllers[i].isQuickSwap = i == quickSwapSlot;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.xui.PlayerInventory.OnToolbeltItemsChanged += this.PlayerInventory_OnToolbeltItemsChanged;
		this.PlayerInventory_OnToolbeltItemsChanged();
		this.currentHoldingIndex = this.xui.PlayerInventory.Toolbelt.holdingItemIdx;
		if (this.currentHoldingIndex != this.xui.PlayerInventory.Toolbelt.DUMMY_SLOT_IDX)
		{
			this.itemControllers[this.currentHoldingIndex].IsHolding = true;
		}
		this.xui.playerUI.windowManager.Open(this.xui.DragAndDropWindow.WindowGroup, false);
		if (this.backendSlotCount < 0)
		{
			this.backendSlotCount = this.xui.PlayerInventory.Toolbelt.PUBLIC_SLOTS;
			this.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnToolbeltItemsChanged()
	{
		this.SetStacks(this.GetSlots());
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.xui.PlayerInventory.OnToolbeltItemsChanged -= this.PlayerInventory_OnToolbeltItemsChanged;
		GUIWindowManager windowManager = this.xui.playerUI.windowManager;
		XUiC_DragAndDropWindow dragAndDropWindow = this.xui.DragAndDropWindow;
		windowManager.Close((dragAndDropWindow != null) ? dragAndDropWindow.WindowGroup : null, false);
		if (this.currentHoldingIndex != this.xui.PlayerInventory.Toolbelt.DUMMY_SLOT_IDX)
		{
			this.itemControllers[this.currentHoldingIndex].IsHolding = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public XUiC_ItemStack GetSlotControl(int slotIdx)
	{
		return this.itemControllers[slotIdx];
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "secondrow")
		{
			_value = this.HasSecondRow.ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentHoldingIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastActionSlot;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastActionRunning;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public int backendSlotCount = -1;
}
