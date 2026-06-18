using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Audio;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemStack : XUiC_SelectableEntry
{
	public bool isQuickSwap
	{
		get
		{
			return this._isQuickSwap;
		}
		set
		{
			if (value == this._isQuickSwap)
			{
				return;
			}
			this._isQuickSwap = value;
			this.IsDirty = true;
		}
	}

	public Color32 SelectionBorderColor
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.selectionBorderColor;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (!this.selectionBorderColor.ColorEquals(value))
			{
				this.selectionBorderColor = value;
				this.IsDirty = true;
			}
		}
	}

	public int SlotNumber { get; set; }

	public XUiC_ItemStack.StackLocationTypes StackLocation { get; set; }

	public event XUiEvent_SlotChangedEventHandler SlotChangedEvent;

	public event XUiEvent_ToolLockChangeEventHandler ToolLockChangedEvent;

	public event XUiEvent_LockChangeEventHandler LockChangedEvent;

	public event XUiEvent_TimeIntervalElapsedEventHandler TimeIntervalElapsedEvent;

	public ItemClass itemClass
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			ItemStack itemStack = this.itemStack;
			if (itemStack == null)
			{
				return null;
			}
			ItemValue itemValue = itemStack.itemValue;
			if (itemValue == null)
			{
				return null;
			}
			return itemValue.ItemClass;
		}
	}

	public ItemClass itemClassOrMissing
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			ItemStack itemStack = this.itemStack;
			if (itemStack == null)
			{
				return null;
			}
			ItemValue itemValue = itemStack.itemValue;
			if (itemValue == null)
			{
				return null;
			}
			return itemValue.ItemClassOrMissing;
		}
	}

	public float HoverIconGrow
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool AssembleLock
	{
		get
		{
			return this.stackLockType == XUiC_ItemStack.StackLockTypes.Assemble;
		}
		set
		{
			this.stackLockType = (value ? XUiC_ItemStack.StackLockTypes.Assemble : XUiC_ItemStack.StackLockTypes.None);
			this.IsDirty = true;
		}
	}

	public bool QuestLock
	{
		get
		{
			return this.stackLockType == XUiC_ItemStack.StackLockTypes.Quest;
		}
		set
		{
			this.stackLockType = (value ? XUiC_ItemStack.StackLockTypes.Quest : ((this.stackLockType == XUiC_ItemStack.StackLockTypes.Quest) ? XUiC_ItemStack.StackLockTypes.None : this.stackLockType));
			this.IsDirty = true;
		}
	}

	public bool ToolLock
	{
		get
		{
			return this.stackLockType == XUiC_ItemStack.StackLockTypes.Tool;
		}
		set
		{
			this.stackLockType = (value ? XUiC_ItemStack.StackLockTypes.Tool : XUiC_ItemStack.StackLockTypes.None);
			XUiEvent_ToolLockChangeEventHandler toolLockChangedEvent = this.ToolLockChangedEvent;
			if (toolLockChangedEvent != null)
			{
				toolLockChangedEvent(this.SlotNumber, this.itemStack, value);
			}
			this.IsDirty = true;
		}
	}

	public bool HiddenLock
	{
		get
		{
			return this.stackLockType == XUiC_ItemStack.StackLockTypes.Hidden;
		}
		set
		{
			this.stackLockType = (value ? XUiC_ItemStack.StackLockTypes.Hidden : XUiC_ItemStack.StackLockTypes.None);
			this.IsDirty = true;
		}
	}

	public bool AttributeLock
	{
		get
		{
			return this.attributeLock;
		}
		set
		{
			this.attributeLock = value;
			this.IsDirty = true;
		}
	}

	public bool StackLock
	{
		get
		{
			return this.stackLockType > XUiC_ItemStack.StackLockTypes.None;
		}
	}

	public bool IsDragAndDrop
	{
		get
		{
			return this.isDragAndDrop;
		}
		set
		{
			this.isDragAndDrop = value;
			if (!value)
			{
				return;
			}
			base.ViewComponent.EventOnPress = false;
			base.ViewComponent.EventOnHover = false;
		}
	}

	public bool IsHolding { get; set; }

	[XuiXmlBinding("islocked")]
	public bool IsLocked
	{
		get
		{
			return this.isLocked;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value == this.isLocked)
			{
				return;
			}
			this.isLocked = value;
			this.IsDirty = true;
		}
	}

	public int RepairAmount
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool AllowIconGrow
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.itemClass != null;
		}
	}

	public XUiC_ItemInfoWindow InfoWindow { get; set; }

	public bool SimpleClick
	{
		get; [PublicizedFrom(EAccessModifier.Internal)]
		set;
	}

	public bool AllowDropping
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	} = true;

	public bool PrefixId
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public bool ShowFavorites
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public float LockTime
	{
		get
		{
			return this.lockTime;
		}
		set
		{
			this.lockTime = value;
			if (value == 0f)
			{
				this.timer.Text = "";
				this.timer.IsVisible = false;
				return;
			}
			this.timer.Text = string.Format("{0:00}:{1:00}", Mathf.Floor(this.lockTime / 60f), Mathf.Floor(this.lockTime % 60f));
			this.timer.IsVisible = true;
		}
	}

	[XuiXmlBinding("userlockedslot")]
	public bool UserLockedSlot
	{
		get
		{
			return this.userLockedSlot;
		}
		set
		{
			if (value == this.userLockedSlot)
			{
				return;
			}
			this.userLockedSlot = value;
			base.RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void setItemStack(ItemStack _stack)
	{
		this.itemStack = _stack.Clone();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateItemInfoWindow(XUiC_ItemStack _itemStack)
	{
		this.InfoWindow.SetItemStack(_itemStack, true);
	}

	public ItemStack ItemStack
	{
		get
		{
			return this.itemStack;
		}
		set
		{
			if (!this.itemStack.Equals(value))
			{
				this.setItemStack(value);
				if (this.itemStack.IsEmpty())
				{
					this.itemStack.Clear();
				}
				if (base.IsSelected)
				{
					this.updateItemInfoWindow(this);
				}
				this.HandleSlotChangeEvent();
				if (value.IsEmpty())
				{
					this.stackLockType = XUiC_ItemStack.StackLockTypes.None;
				}
				this.updateBackgroundTexture();
				this.IsDirty = true;
			}
			else
			{
				if (this.ItemStack.IsEmpty() && this.backgroundTexture != null)
				{
					this.backgroundTexture.Texture = null;
				}
				if (base.IsSelected)
				{
					this.updateItemInfoWindow(this);
				}
				this.xui.playerUI.CursorController.HoverTarget = null;
			}
			if (this.ItemStack.IsEmpty() && base.IsSelected)
			{
				base.IsSelected = false;
			}
			this.viewComponent.IsSnappable = !this.itemStack.IsEmpty();
			this.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBackgroundTexture()
	{
		if (this.backgroundTexture == null)
		{
			return;
		}
		ItemClassBlock itemClassBlock = this.itemClass as ItemClassBlock;
		if (itemClassBlock != null)
		{
			Block block = itemClassBlock.GetBlock();
			if (block.GetAutoShapeType() == EAutoShapeType.None)
			{
				this.backgroundTexture.Texture = null;
				return;
			}
			int uiBackgroundTextureId = block.GetUiBackgroundTextureId(this.itemStack.itemValue.ToBlockValue(false), BlockFace.Top, 0);
			this.<updateBackgroundTexture>g__setTextureForOpaqueMeshTextureId|132_0(uiBackgroundTextureId);
			return;
		}
		else
		{
			ItemClass itemClass = this.itemClass;
			ItemActionTextureBlock itemActionTextureBlock = ((itemClass != null) ? itemClass.Actions[0] : null) as ItemActionTextureBlock;
			if (itemActionTextureBlock == null)
			{
				this.backgroundTexture.Texture = null;
				return;
			}
			int num = this.itemStack.itemValue.Meta;
			if (num == 0)
			{
				num = itemActionTextureBlock.DefaultTextureID;
			}
			int num2;
			int num3;
			if (XUiC_ItemStack.TryResolvePaintTextureId(num, itemActionTextureBlock.DefaultTextureID, out num2, out num3))
			{
				if (num2 != this.itemStack.itemValue.Meta)
				{
					this.itemStack.itemValue.Meta = (int)((byte)num2);
				}
				this.<updateBackgroundTexture>g__setTextureForOpaqueMeshTextureId|132_0(num3);
				return;
			}
			this.backgroundTexture.Texture = null;
			this.backgroundTexture.SetTextureDirty();
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool TryResolvePaintTextureId(int requestedPaintId, int fallbackPaintId, out int resolvedPaintId, out int textureId)
	{
		resolvedPaintId = 0;
		textureId = 0;
		BlockTextureData blockTextureData;
		if (XUiC_ItemStack.TryGetBlockTextureData(requestedPaintId, out blockTextureData))
		{
			resolvedPaintId = requestedPaintId;
			textureId = (int)blockTextureData.TextureID;
			return true;
		}
		BlockTextureData blockTextureData2;
		if (fallbackPaintId != requestedPaintId && XUiC_ItemStack.TryGetBlockTextureData(fallbackPaintId, out blockTextureData2))
		{
			resolvedPaintId = fallbackPaintId;
			textureId = (int)blockTextureData2.TextureID;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool TryGetBlockTextureData(int paintId, out BlockTextureData data)
	{
		data = null;
		BlockTextureData[] list = BlockTextureData.list;
		if (list == null || paintId < 0 || paintId >= list.Length)
		{
			return false;
		}
		data = list[paintId];
		return data != null;
	}

	public void ResetTweenScale()
	{
		if (this.tweenScale != null && this.tweenScale.value != Vector3.one)
		{
			this.tweenScale.from = Vector3.one * 1.5f;
			this.tweenScale.to = Vector3.one;
			this.tweenScale.enabled = true;
			this.tweenScale.duration = 0.1f;
		}
	}

	public void ForceSetItemStack(ItemStack _stack)
	{
		bool isSelected = base.IsSelected;
		this.itemStack = ItemStack.Empty;
		XUiEvent_SlotChangedEventHandler slotChangedEvent = this.SlotChangedEvent;
		if (slotChangedEvent != null)
		{
			slotChangedEvent(this.SlotNumber, this.itemStack);
		}
		if (!_stack.IsEmpty())
		{
			base.IsSelected = isSelected;
		}
		this.ItemStack = _stack;
		XUiEvent_SlotChangedEventHandler slotChangedEvent2 = this.SlotChangedEvent;
		if (slotChangedEvent2 == null)
		{
			return;
		}
		slotChangedEvent2(this.SlotNumber, this.itemStack);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleSlotChangeEvent()
	{
		if (this.itemStack.IsEmpty() && base.IsSelected)
		{
			base.IsSelected = false;
		}
		XUiEvent_SlotChangedEventHandler slotChangedEvent = this.SlotChangedEvent;
		if (slotChangedEvent == null)
		{
			return;
		}
		slotChangedEvent(this.SlotNumber, this.itemStack);
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("timer");
		if (childById != null)
		{
			this.timer = childById.ViewComponent as XUiV_Label;
		}
		XUiController childById2 = base.GetChildById("itemIcon");
		if (childById2 != null)
		{
			this.itemIconSprite = childById2.ViewComponent as XUiV_Sprite;
		}
		XUiController childById3 = base.GetChildById("lockTypeIcon");
		if (childById3 != null)
		{
			this.lockTypeIcon = childById3.ViewComponent as XUiV_Sprite;
		}
		XUiController childById4 = base.GetChildById("backgroundTexture");
		if (childById4 != null)
		{
			this.backgroundTexture = childById4.ViewComponent as XUiV_Texture;
			if (this.backgroundTexture != null)
			{
				this.backgroundTexture.CreateMaterial("Unlit/Transparent Colored Emissive TextureArray");
			}
		}
		XUiController childById5 = base.GetChildById("rectSlotLock");
		if (childById5 != null)
		{
			childById5.OnHover += delegate(XUiController _, bool _over)
			{
			};
			childById5.OnPress += delegate(XUiController _, int _)
			{
				this.UserLockedSlot = !this.UserLockedSlot;
				base.RefreshBindings();
			};
		}
		this.tweenScale = this.itemIconSprite.UiTransform.gameObject.AddComponent<TweenScale>();
		base.ViewComponent.UseSelectionBox = false;
	}

	public void UpdateTimer(float _dt)
	{
		if (!this.IsLocked)
		{
			return;
		}
		if (this.lockType == XUiC_ItemStack.LockTypes.Shell || this.lockType == XUiC_ItemStack.LockTypes.Burning)
		{
			return;
		}
		float num = this.lockTime;
		if (this.lockTime > 0f)
		{
			this.lockTime -= _dt;
			if (this.currentInterval == -1)
			{
				this.currentInterval = (int)this.lockTime / this.TimeInterval;
			}
			if (this.TimeIntervalElapsedEvent != null && this.TimeInterval != 0)
			{
				int num2 = (int)this.lockTime / this.TimeInterval;
				if (num2 != this.currentInterval)
				{
					this.TimeIntervalElapsedEvent(this.lockTime, this);
					this.currentInterval = num2;
				}
			}
		}
		if (this.lockTime <= 0f && num != 0f)
		{
			XUiEvent_TimeIntervalElapsedEventHandler timeIntervalElapsedEvent = this.TimeIntervalElapsedEvent;
			if (timeIntervalElapsedEvent != null)
			{
				timeIntervalElapsedEvent(this.lockTime, this);
			}
			if (this.LockChangedEvent != null)
			{
				this.LockChangedEvent(this.lockType, this);
			}
			else
			{
				this.IsLocked = false;
			}
		}
		if (this.lockTime <= 0f)
		{
			this.timer.IsVisible = false;
			this.timer.Text = "";
			return;
		}
		this.timer.IsVisible = true;
		this.timer.Text = string.Format("{0:00}:{1:00}", Mathf.Floor(this.lockTime / 60f), Mathf.Floor(this.lockTime % 60f));
	}

	public override void Update(float _dt)
	{
		using (this.pmIsUpdate.Auto())
		{
			using (this.pmIsUpdateBase.Auto())
			{
				base.Update(_dt);
			}
			if (base.WindowGroup.isShowing)
			{
				PlayerActionsGUI guiactions = this.xui.playerUI.playerInput.GUIActions;
				CursorControllerAbs cursorController = this.xui.playerUI.CursorController;
				Vector3 vector = cursorController.GetScreenPosition();
				bool mouseButtonUp = cursorController.GetMouseButtonUp(0);
				bool mouseButtonDown = cursorController.GetMouseButtonDown(0);
				bool mouseButton = cursorController.GetMouseButton(0);
				bool mouseButtonUp2 = cursorController.GetMouseButtonUp(1);
				bool mouseButtonDown2 = cursorController.GetMouseButtonDown(1);
				bool mouseButton2 = cursorController.GetMouseButton(1);
				if (!this.IsLocked && !this.isDragAndDrop)
				{
					if (this.isOver && base.ViewComponent.UiTransformIsHovered && base.ViewComponent.EventOnPress)
					{
						if (guiactions.LastInputType == 1)
						{
							bool wasReleased = guiactions.Submit.WasReleased;
							bool wasReleased2 = guiactions.HalfStack.WasReleased;
							bool wasPressed = guiactions.Inspect.WasPressed;
							bool wasReleased3 = guiactions.RightStick.WasReleased;
							if (this.SimpleClick && !this.StackLock)
							{
								if (wasReleased)
								{
									this.HandleMoveToPreferredLocation();
								}
								else if (wasPressed)
								{
									this.HandleItemInspect();
								}
							}
							else if (this.xui.DragAndDropWindow.CurrentStack.IsEmpty() && !this.ItemStack.IsEmpty())
							{
								if (!this.StackLock)
								{
									if (wasReleased)
									{
										this.SwapItem();
									}
									else if (wasReleased2)
									{
										this.HandlePartialStackPickup();
									}
									else if (wasReleased3)
									{
										this.HandleMoveToPreferredLocation();
									}
									else if (wasPressed)
									{
										this.HandleItemInspect();
									}
								}
							}
							else if (!this.StackLock)
							{
								if (wasReleased)
								{
									this.HandleStackSwap();
								}
								else if (wasReleased2 && this.AllowDropping)
								{
									this.HandleDropOne();
								}
							}
						}
						else if (this.SimpleClick && !this.StackLock)
						{
							if (mouseButtonUp)
							{
								this.HandleMoveToPreferredLocation();
							}
						}
						else if (InputUtils.ShiftKeyPressed)
						{
							if (!this.StackLock && mouseButtonUp)
							{
								this.HandleMoveToPreferredLocation();
							}
						}
						else if (mouseButton || mouseButton2)
						{
							if (this.xui.DragAndDropWindow.CurrentStack.IsEmpty() && !this.ItemStack.IsEmpty())
							{
								if (!this.lastClicked)
								{
									this.startMousePos = vector;
								}
								else if (Mathf.Abs((vector - this.startMousePos).magnitude) > (float)this.PickupSnapDistance && !this.StackLock)
								{
									if (mouseButton)
									{
										this.SwapItem();
									}
									else
									{
										this.HandlePartialStackPickup();
									}
								}
							}
							if (mouseButtonDown || mouseButtonDown2)
							{
								this.lastClicked = true;
							}
						}
						else if (mouseButtonUp)
						{
							if (this.xui.DragAndDropWindow.CurrentStack.IsEmpty())
							{
								this.HandleItemInspect();
							}
							else if (this.lastClicked && !this.StackLock)
							{
								this.HandleStackSwap();
							}
						}
						else if (mouseButtonUp2)
						{
							if (this.lastClicked && !this.StackLock && this.AllowDropping)
							{
								this.HandleDropOne();
							}
						}
						else
						{
							this.lastClicked = false;
						}
					}
					else
					{
						this.lastClicked = false;
						if ((this.isOver || this.itemIconSprite.UiTransform.localScale != Vector3.one) && this.tweenScale.value != Vector3.one && !this.itemStack.IsEmpty())
						{
							this.tweenScale.from = Vector3.one * 1.5f;
							this.tweenScale.to = Vector3.one;
							this.tweenScale.enabled = true;
							this.tweenScale.duration = 0.5f;
						}
					}
				}
				else if (this.IsLocked && ((guiactions.LastInputType == 1 && guiactions.Submit.WasReleased) || (guiactions.LastInputType != 1 && guiactions.LeftClick.WasPressed)) && this.isOver)
				{
					XUiEvent_LockChangeEventHandler lockChangedEvent = this.LockChangedEvent;
					if (lockChangedEvent != null)
					{
						lockChangedEvent(XUiC_ItemStack.LockTypes.None, this);
					}
				}
			}
			this.updateBorderColor();
			if (this.flashLockTypeIcon != XUiC_ItemStack.flashLockTypes.None)
			{
				Color color = ((this.flashLockTypeIcon == XUiC_ItemStack.flashLockTypes.Allowed) ? this.modAllowedColor : this.modAlreadyEquippedColor);
				float num = Mathf.PingPong(Time.time, 0.5f);
				this.setLockTypeIconColor(Color.Lerp(Color.grey, color, num * 4f));
			}
			if (this.IsDirty)
			{
				this.IsDirty = false;
				this.updateLockTypeIcon();
				base.RefreshBindings();
				this.ResetTweenScale();
			}
			if (this.IsLocked && this.lockType != XUiC_ItemStack.LockTypes.None)
			{
				this.UpdateTimer(_dt);
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBorderColor()
	{
		if (this.IsDragAndDrop)
		{
			this.SelectionBorderColor = Color.clear;
			return;
		}
		if (base.IsSelected)
		{
			this.SelectionBorderColor = this.selectColor;
			return;
		}
		if (this.isOver)
		{
			this.SelectionBorderColor = this.highlightColor;
			return;
		}
		if (this.IsHolding)
		{
			this.SelectionBorderColor = this.holdingColor;
			return;
		}
		this.SelectionBorderColor = this.backgroundColor;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanSwap(ItemStack _stack)
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleItemInspect()
	{
		if (!this.ItemStack.IsEmpty() && this.InfoWindow != null)
		{
			base.IsSelected = true;
			this.InfoWindow.SetMaxCountOnDirty = true;
			this.updateItemInfoWindow(this);
		}
		this.HandleClickComplete();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleStackSwap()
	{
		this.xui.PopupMenuWindow.Close();
		if (!this.AllowDropping)
		{
			this.xui.DragAndDropWindow.CurrentStack = ItemStack.Empty;
			this.xui.DragAndDropWindow.PickUpType = this.StackLocation;
		}
		bool flag = false;
		ItemStack currentStack = this.xui.DragAndDropWindow.CurrentStack;
		ItemClass itemClassOrMissing = currentStack.itemValue.ItemClassOrMissing;
		int num = 0;
		if (itemClassOrMissing != null)
		{
			num = ((this.OverrideStackCount == -1) ? itemClassOrMissing.Stacknumber.Value : Mathf.Min(itemClassOrMissing.Stacknumber.Value, this.OverrideStackCount));
			if (!currentStack.IsEmpty() && this.itemStack.IsEmpty() && num < currentStack.count)
			{
				flag = true;
			}
		}
		if (!flag && (this.itemStack.IsEmpty() || currentStack.IsEmpty()))
		{
			this.SwapItem();
			base.IsSelected = false;
		}
		else if (!flag && (!this.itemStack.itemValue.ItemClassOrMissing.CanStack() || !itemClassOrMissing.CanStack()))
		{
			this.SwapItem();
			base.IsSelected = false;
		}
		else if (currentStack.itemValue.type == this.itemStack.itemValue.type && !currentStack.itemValue.HasQuality && !this.itemStack.itemValue.HasQuality)
		{
			if (currentStack.count + this.itemStack.count > num)
			{
				int num2 = currentStack.count + this.itemStack.count - num;
				ItemStack itemStack = this.itemStack.Clone();
				itemStack.count = num;
				currentStack.count = num2;
				this.xui.DragAndDropWindow.CurrentStack = currentStack;
				this.xui.DragAndDropWindow.PickUpType = this.StackLocation;
				this.ItemStack = itemStack;
			}
			else
			{
				ItemStack itemStack2 = this.itemStack.Clone();
				itemStack2.count += currentStack.count;
				this.ItemStack = itemStack2;
				this.xui.DragAndDropWindow.CurrentStack = ItemStack.Empty;
				this.PlayPlaceSound(null);
			}
			if (base.IsSelected)
			{
				this.updateItemInfoWindow(this);
			}
		}
		else if (flag)
		{
			int num3 = currentStack.count - num;
			ItemStack itemStack3 = currentStack.Clone();
			itemStack3.count = num;
			currentStack.count = num3;
			this.xui.DragAndDropWindow.CurrentStack = currentStack;
			this.xui.DragAndDropWindow.PickUpType = this.StackLocation;
			this.ItemStack = itemStack3;
		}
		else
		{
			this.SwapItem();
			base.IsSelected = false;
		}
		this.HandleClickComplete();
		this.xui.CalloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, this.ItemStack, this.isOver, true, true, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandlePartialStackPickup()
	{
		ItemStack itemStack = this.xui.DragAndDropWindow.CurrentStack;
		if (itemStack.IsEmpty() && !this.itemStack.IsEmpty())
		{
			int num = this.itemStack.count / 2;
			if (num > 0)
			{
				itemStack = this.itemStack.Clone();
				itemStack.count = num;
				if (this.AllowDropping)
				{
					ItemStack itemStack2 = this.itemStack.Clone();
					itemStack2.count -= num;
					this.ItemStack = itemStack2;
				}
				this.xui.DragAndDropWindow.CurrentStack = itemStack;
				this.xui.DragAndDropWindow.PickUpType = this.StackLocation;
			}
		}
		if (base.IsSelected)
		{
			base.IsSelected = false;
		}
		this.HandleClickComplete();
		this.xui.CalloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, this.ItemStack, this.isOver, true, true, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleDropOne()
	{
		ItemStack currentStack = this.xui.DragAndDropWindow.CurrentStack;
		if (!currentStack.CanMoveTo(this.StackLocation))
		{
			return;
		}
		if (!currentStack.IsEmpty())
		{
			int num = 1;
			if (this.itemStack.IsEmpty())
			{
				ItemStack itemStack = currentStack.Clone();
				itemStack.count = num;
				currentStack.count -= num;
				this.xui.DragAndDropWindow.CurrentStack = currentStack;
				this.ItemStack = itemStack;
				this.PlayPlaceSound(null);
			}
			else if (currentStack.itemValue.type == this.itemStack.itemValue.type)
			{
				ItemClass itemClassOrMissing = currentStack.itemValue.ItemClassOrMissing;
				int num2 = ((this.OverrideStackCount == -1) ? itemClassOrMissing.Stacknumber.Value : Mathf.Min(itemClassOrMissing.Stacknumber.Value, this.OverrideStackCount));
				if (this.itemStack.count + 1 <= num2)
				{
					ItemStack itemStack2 = this.itemStack.Clone();
					itemStack2.count++;
					currentStack.count--;
					this.ItemStack = itemStack2.Clone();
					this.xui.DragAndDropWindow.CurrentStack = currentStack;
					XUiEvent_SlotChangedEventHandler slotChangedEvent = this.SlotChangedEvent;
					if (slotChangedEvent != null)
					{
						slotChangedEvent(this.SlotNumber, this.itemStack);
					}
					this.IsDirty = true;
				}
				this.PlayPlaceSound(null);
			}
			if (currentStack.count == 0)
			{
				this.xui.DragAndDropWindow.CurrentStack = ItemStack.Empty;
			}
		}
		base.IsSelected = false;
		this.HandleClickComplete();
		this.xui.CalloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, this.ItemStack, this.isOver, true, true, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleClickComplete()
	{
		this.lastClicked = false;
		if (this.itemIconSprite.UiTransform.localScale.x <= 1f)
		{
			return;
		}
		if (this.itemStack.IsEmpty())
		{
			return;
		}
		this.tweenScale.from = Vector3.one * 1.5f;
		this.tweenScale.to = Vector3.one;
		this.tweenScale.enabled = true;
		this.tweenScale.duration = 0.5f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		this.isOver = _isOver;
		if (!this.IsLocked)
		{
			if (_isOver)
			{
				if (this.InfoWindow != null && this.InfoWindow.ViewComponent.IsVisible)
				{
					this.InfoWindow.HoverEntry = this;
				}
				if (this.AllowIconGrow)
				{
					this.tweenScale.from = Vector3.one;
					this.tweenScale.to = Vector3.one * 1.5f;
					this.tweenScale.enabled = true;
					this.tweenScale.duration = 0.5f;
				}
			}
			else
			{
				if (this.InfoWindow != null && this.InfoWindow.ViewComponent.IsVisible)
				{
					this.InfoWindow.HoverEntry = null;
				}
				if (this.AllowIconGrow)
				{
					this.tweenScale.from = Vector3.one * 1.5f;
					this.tweenScale.to = Vector3.one;
					this.tweenScale.enabled = true;
					this.tweenScale.duration = 0.5f;
				}
			}
		}
		this.xui.CalloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, this.ItemStack, this.isOver, true, !this.StackLock, true);
		base.OnHovered(_isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SwapItem()
	{
		this.xui.PopupMenuWindow.Close();
		ItemStack currentStack = this.xui.DragAndDropWindow.CurrentStack;
		if (!currentStack.IsEmpty() && !this.CanSwap(currentStack))
		{
			return;
		}
		if (!currentStack.CanMoveTo(this.StackLocation))
		{
			return;
		}
		if (this.StackLocation == XUiC_ItemStack.StackLocationTypes.LootContainer && this.xui.DragAndDropWindow.PickUpType != XUiC_ItemStack.StackLocationTypes.LootContainer && !currentStack.IsEmpty() && !currentStack.itemValue.ItemClassOrMissing.CanPlaceInContainer())
		{
			Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
			GameManager.ShowTooltip(this.xui.playerUI.entityPlayer, "Quest Items cannot be placed in containers.", false, false, 0f);
			return;
		}
		if (this.itemStack.IsEmpty())
		{
			this.PlayPlaceSound(currentStack);
		}
		this.xui.DragAndDropWindow.CurrentStack = this.itemStack.Clone();
		this.xui.DragAndDropWindow.PickUpType = this.StackLocation;
		if (this.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt)
		{
			this.xui.DragAndDropWindow.CurrentStack.Deactivate();
		}
		this.ForceSetItemStack(currentStack.Clone());
		this.xui.CalloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, this.ItemStack, this.isOver, true, true, true);
	}

	public void HandleMoveToPreferredLocation()
	{
		this.xui.PopupMenuWindow.Close();
		if (this.ItemStack.IsEmpty())
		{
			return;
		}
		if (this.StackLock)
		{
			return;
		}
		if (this.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt)
		{
			this.ItemStack.Deactivate();
		}
		int count = this.ItemStack.count;
		switch (this.StackLocation)
		{
		case XUiC_ItemStack.StackLocationTypes.Backpack:
		case XUiC_ItemStack.StackLocationTypes.ToolBelt:
		{
			XUiM_AssembleItem assembleItem = this.xui.AssembleItem;
			bool flag = ((assembleItem != null) ? assembleItem.CurrentItem : null) != null;
			if (!flag)
			{
				XUiC_BagStorageWindowGroup xuiC_BagStorageWindowGroup = this.xui.FindWindowGroupByName(XUiC_BagStorageWindowGroup.ID) as XUiC_BagStorageWindowGroup;
				if (xuiC_BagStorageWindowGroup != null && xuiC_BagStorageWindowGroup.Bag != null)
				{
					string vehicleSlotType = this.ItemStack.itemValue.ItemClass.VehicleSlotType;
					ItemStack itemStack;
					if (vehicleSlotType != "" && this.xui.Vehicle.SetPart(this.xui, vehicleSlotType, this.ItemStack, out itemStack))
					{
						this.PlayPlaceSound(null);
						this.ItemStack = itemStack;
						this.HandleSlotChangeEvent();
						return;
					}
					XUiC_BagContainer childByType = this.xui.FindWindowGroupByName(XUiC_BagStorageWindowGroup.ID).GetChildByType<XUiC_BagContainer>();
					if (childByType != null && childByType.Bag != null && childByType.Bag.CanStack(this.itemStack) && childByType.AddItem(this.ItemStack))
					{
						this.PlayPlaceSound(null);
						this.ItemStack = ItemStack.Empty;
						this.HandleSlotChangeEvent();
						return;
					}
					if (count != this.ItemStack.count)
					{
						this.PlayPlaceSound(null);
						if (this.ItemStack.count == 0)
						{
							this.ItemStack = ItemStack.Empty;
						}
						this.HandleSlotChangeEvent();
						return;
					}
				}
			}
			if (flag && this.ItemStack.itemValue.ItemClass is ItemClassModifier)
			{
				ItemStack itemStack2;
				if (this.xui.AssembleItem.AddPartToItem(this.ItemStack, out itemStack2))
				{
					this.PlayPlaceSound(null);
					this.ItemStack = itemStack2;
					this.HandleSlotChangeEvent();
					return;
				}
				Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
				return;
			}
			else
			{
				if (this.xui.PlayerEquipment != null && this.xui.PlayerEquipment.IsOpen && this.itemStack.itemValue.ItemClass.CanEquip())
				{
					this.PlayPlaceSound(null);
					this.ItemStack = this.xui.PlayerEquipment.EquipItem(this.ItemStack);
					this.HandleSlotChangeEvent();
					return;
				}
				if (this.xui.LootContainer != null)
				{
					XUiC_LootContainer childByType2 = this.xui.FindWindowGroupByName(XUiC_LootWindowGroup.ID).GetChildByType<XUiC_LootContainer>();
					if (XUiM_LootContainer.AddItem(this.ItemStack, this.xui))
					{
						this.PlayPlaceSound(null);
						this.ItemStack = ItemStack.Empty;
						this.HandleSlotChangeEvent();
						if (childByType2 != null)
						{
							childByType2.SetSlots(this.xui.LootContainer, this.xui.LootContainer.items);
						}
						return;
					}
					if (count != this.ItemStack.count)
					{
						this.PlayPlaceSound(null);
						if (this.ItemStack.count == 0)
						{
							this.ItemStack = ItemStack.Empty;
						}
						this.HandleSlotChangeEvent();
						if (childByType2 != null)
						{
							childByType2.SetSlots(this.xui.LootContainer, this.xui.LootContainer.items);
						}
						return;
					}
				}
				if (this.xui.CurrentWorkstationToolGrid != null && this.xui.CurrentWorkstationToolGrid.TryAddTool(this.itemClass, this.ItemStack))
				{
					this.PlayPlaceSound(null);
					this.ItemStack = ItemStack.Empty;
					this.HandleSlotChangeEvent();
					return;
				}
				if (this.xui.CurrentWorkstationFuelGrid != null && this.itemClass.FuelValue != null && this.itemClass.FuelValue.Value > 0)
				{
					if (this.xui.CurrentWorkstationFuelGrid.AddItem(this.itemClass, this.ItemStack))
					{
						this.PlayPlaceSound(null);
						this.ItemStack = ItemStack.Empty;
						this.HandleSlotChangeEvent();
						return;
					}
					if (count != this.ItemStack.count)
					{
						this.PlayPlaceSound(null);
						if (this.ItemStack.count == 0)
						{
							this.ItemStack = ItemStack.Empty;
						}
						this.HandleSlotChangeEvent();
						return;
					}
				}
				if (this.xui.CurrentWorkstationInputGrid != null && this.xui.CurrentWorkstationInputGrid.AcceptsMaterial(this.itemClass.MadeOfMaterial))
				{
					if (this.xui.CurrentWorkstationInputGrid.AddItem(this.itemClass, this.ItemStack))
					{
						this.PlayPlaceSound(null);
						this.ItemStack = ItemStack.Empty;
						this.HandleSlotChangeEvent();
						return;
					}
					if (count != this.ItemStack.count)
					{
						this.PlayPlaceSound(null);
						if (this.ItemStack.count == 0)
						{
							this.ItemStack = ItemStack.Empty;
						}
						this.HandleSlotChangeEvent();
						return;
					}
				}
				if (this.xui.CurrentWorkstationOutputGrid != null)
				{
					if (this.xui.CurrentWorkstationOutputGrid.AddItem(this.itemClass, this.ItemStack))
					{
						this.PlayPlaceSound(null);
						this.ItemStack = ItemStack.Empty;
						this.HandleSlotChangeEvent();
						return;
					}
					if (count != this.ItemStack.count)
					{
						this.PlayPlaceSound(null);
						if (this.ItemStack.count == 0)
						{
							this.ItemStack = ItemStack.Empty;
						}
						this.HandleSlotChangeEvent();
						return;
					}
				}
				if (this.xui.CurrentCollectorModGrid != null)
				{
					if (this.xui.CurrentCollectorModGrid.TryAddMod(this.itemClass, this.ItemStack))
					{
						this.PlayPlaceSound(null);
						this.ItemStack = ItemStack.Empty;
						this.HandleSlotChangeEvent();
						return;
					}
					if (this.xui.CurrentCollectorFuelGrid != null && this.xui.CurrentCollectorFuelGrid.TryAddFuel(this.itemClass, this.ItemStack))
					{
						this.PlayPlaceSound(null);
						this.ItemStack = ItemStack.Empty;
						this.HandleSlotChangeEvent();
						return;
					}
					if (this.xui.currentCollectorCatalystGrid != null && this.xui.currentCollectorCatalystGrid.TryAddFuel(this.itemClass, this.ItemStack))
					{
						this.itemClass.OnPlacedAsCatalyst(this.xui.PlayerInventory.Toolbelt.holdingItemData);
						this.PlayPlaceSound(null);
						this.ItemStack = ItemStack.Empty;
						this.HandleSlotChangeEvent();
						return;
					}
				}
				if (this.xui.CurrentCombineGrid != null && this.xui.CurrentCombineGrid.TryAddItemToSlot(this.itemClass, this.ItemStack))
				{
					this.PlayPlaceSound(null);
					this.ItemStack = ItemStack.Empty;
					this.HandleSlotChangeEvent();
					return;
				}
				if (this.xui.CurrentPowerSourceSlots != null && this.xui.CurrentPowerSourceSlots.TryAddItemToSlot(this.itemClass, this.ItemStack))
				{
					this.PlayPlaceSound(null);
					this.ItemStack = ItemStack.Empty;
					this.HandleSlotChangeEvent();
					return;
				}
				if (this.xui.CurrentPowerAmmoSlots != null)
				{
					if (this.xui.CurrentPowerAmmoSlots.TryAddItemToSlot(this.itemClass, this.ItemStack))
					{
						this.PlayPlaceSound(null);
						this.ItemStack = ItemStack.Empty;
						this.HandleSlotChangeEvent();
						return;
					}
					if (count != this.ItemStack.count)
					{
						this.PlayPlaceSound(null);
						if (this.ItemStack.count == 0)
						{
							this.ItemStack = ItemStack.Empty;
						}
						this.HandleSlotChangeEvent();
						return;
					}
				}
				if (this.xui.Trader.TraderData != null && (this.StackLocation == XUiC_ItemStack.StackLocationTypes.Backpack || this.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt))
				{
					this.HandleItemInspect();
					this.InfoWindow.SetMaxCountOnDirty = true;
					return;
				}
				if (this.StackLocation == XUiC_ItemStack.StackLocationTypes.Backpack)
				{
					if (this.xui.PlayerInventory.AddItemToToolbelt(this.ItemStack))
					{
						this.PlayPlaceSound(null);
						this.ItemStack = ItemStack.Empty;
						this.HandleSlotChangeEvent();
						return;
					}
					if (count != this.ItemStack.count)
					{
						this.PlayPlaceSound(null);
						if (this.ItemStack.count == 0)
						{
							this.ItemStack = ItemStack.Empty;
						}
						this.HandleSlotChangeEvent();
						return;
					}
				}
				else if (this.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt)
				{
					if (this.xui.PlayerInventory.AddItemToBackpack(this.ItemStack))
					{
						this.PlayPlaceSound(null);
						this.ItemStack = ItemStack.Empty;
						this.HandleSlotChangeEvent();
						return;
					}
					if (count != this.ItemStack.count)
					{
						this.PlayPlaceSound(null);
						if (this.ItemStack.count == 0)
						{
							this.ItemStack = ItemStack.Empty;
						}
						this.HandleSlotChangeEvent();
						return;
					}
				}
			}
			break;
		}
		case XUiC_ItemStack.StackLocationTypes.LootContainer:
		case XUiC_ItemStack.StackLocationTypes.Workstation:
		case XUiC_ItemStack.StackLocationTypes.Merge:
		case XUiC_ItemStack.StackLocationTypes.CollectorCatalysts:
		case XUiC_ItemStack.StackLocationTypes.CollectorFuel:
		case XUiC_ItemStack.StackLocationTypes.Collector:
			if (this.xui.PlayerInventory.AddItem(this.ItemStack))
			{
				this.PlayPlaceSound(null);
				this.ItemStack = ItemStack.Empty;
				this.HandleSlotChangeEvent();
			}
			else if (count != this.ItemStack.count)
			{
				this.PlayPlaceSound(null);
				if (this.ItemStack.count == 0)
				{
					this.ItemStack = ItemStack.Empty;
				}
				this.HandleSlotChangeEvent();
				return;
			}
			break;
		case XUiC_ItemStack.StackLocationTypes.Creative:
		{
			ItemStack itemStack3 = this.itemStack.Clone();
			if (!this.xui.PlayerInventory.AddItem(itemStack3))
			{
				return;
			}
			this.PlayPlaceSound(null);
			break;
		}
		}
		this.xui.CalloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, this.ItemStack, this.isOver, true, true, true);
	}

	public void PlayPlaceSound(ItemStack newStack = null)
	{
		string text;
		if (newStack != null)
		{
			text = ((newStack.itemValue.ItemClass == null) ? "" : newStack.itemValue.ItemClass.SoundPlace);
		}
		else
		{
			text = ((this.itemStack.itemValue.ItemClass == null) ? "" : this.itemStack.itemValue.ItemClass.SoundPlace);
		}
		if (text != "")
		{
			if (text != null)
			{
				Manager.PlayInsidePlayerHead(text, -1, 0f, false, false);
				return;
			}
		}
		else if (this.placeSound != null)
		{
			Manager.PlayXUiSound(this.placeSound, 0.75f);
		}
	}

	public void PlayPickupSound(ItemStack newStack = null)
	{
		ItemStack itemStack = ((newStack != null) ? newStack : this.itemStack);
		string text = ((itemStack.IsEmpty() || itemStack.itemValue.ItemClass == null) ? "" : itemStack.itemValue.ItemClass.SoundPickup);
		if (text != "")
		{
			if (text != null)
			{
				Manager.PlayInsidePlayerHead(text, -1, 0f, false, false);
				return;
			}
		}
		else if (this.pickupSound != null)
		{
			Manager.PlayXUiSound(this.pickupSound, 0.75f);
		}
	}

	public void UnlockStack()
	{
		this.lockType = XUiC_ItemStack.LockTypes.None;
		this.IsLocked = false;
		this.lockTime = 0f;
		this.lockSprite = "";
		this.setLockTypeIconColor(Color.white);
		this.RepairAmount = 0;
		this.timer.IsVisible = false;
		this.IsDirty = true;
	}

	public void LockStack(XUiC_ItemStack.LockTypes _lockType, float _time, int _count, BaseItemActionEntry _itemActionEntry)
	{
		if (_lockType == XUiC_ItemStack.LockTypes.Crafting)
		{
			this.lockSprite = _itemActionEntry.IconName;
		}
		else if (_lockType == XUiC_ItemStack.LockTypes.Scrapping)
		{
			this.lockSprite = "ui_game_symbol_scrap";
		}
		else if (_lockType == XUiC_ItemStack.LockTypes.Burning)
		{
			this.lockSprite = "ui_game_symbol_campfire";
		}
		else if (_lockType == XUiC_ItemStack.LockTypes.Repairing)
		{
			this.lockSprite = "ui_game_symbol_hammer";
		}
		this.IsLocked = true;
		this.lockType = _lockType;
		if (_lockType == XUiC_ItemStack.LockTypes.Repairing)
		{
			this.RepairAmount = _count;
		}
		this.LockTime = _time;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateLockTypeIcon()
	{
		if (this.IsLocked && this.lockType != XUiC_ItemStack.LockTypes.None)
		{
			return;
		}
		this.lockSprite = "";
		if (this.itemClass != null)
		{
			Color color = Color.clear;
			ItemClassBlock itemClassBlock = this.itemClass as ItemClassBlock;
			if (itemClassBlock != null)
			{
				itemClassBlock.GetBlock();
				if (!this.itemStack.itemValue.TextureFullArray.IsDefault)
				{
					this.lockSprite = "ui_game_symbol_paint_brush";
				}
			}
			this.QuestLock = this.itemClass.IsQuestItem && (this.StackLocation == XUiC_ItemStack.StackLocationTypes.Backpack || this.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt);
			ItemClassModifier itemClassModifier = this.itemClass as ItemClassModifier;
			if (itemClassModifier != null)
			{
				this.lockSprite = "ui_game_symbol_assemble";
				if (itemClassModifier.HasAnyTags(ItemClassModifier.CosmeticModTypes))
				{
					this.lockSprite = "ui_game_symbol_paint_bucket";
				}
				if (this.xui.AssembleItem.CurrentItem != null)
				{
					if ((itemClassModifier.InstallableTags.IsEmpty || this.xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.InstallableTags)) && !this.xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.DisallowedTags))
					{
						if (this.StackLocation != XUiC_ItemStack.StackLocationTypes.Part)
						{
							int num = 0;
							for (int i = 0; i < this.xui.AssembleItem.CurrentItem.itemValue.Modifications.Length; i++)
							{
								ItemValue itemValue = this.xui.AssembleItem.CurrentItem.itemValue.Modifications[i];
								if (!itemValue.IsEmpty())
								{
									ItemClassModifier itemClassModifier2 = itemValue.ItemClass as ItemClassModifier;
									if (itemClassModifier2 != null && itemClassModifier2.ModifierTags.Test_AnySet(itemClassModifier.ModifierTags))
									{
										num++;
									}
								}
							}
							if (num >= itemClassModifier.MaxModsAllowed)
							{
								this.flashLockTypeIcon = XUiC_ItemStack.flashLockTypes.AlreadyEquipped;
								return;
							}
						}
						this.flashLockTypeIcon = XUiC_ItemStack.flashLockTypes.Allowed;
					}
					else
					{
						color = Color.grey;
					}
				}
				else
				{
					color = Color.white;
				}
			}
			bool flag = this.itemStack.itemValue.HasAnyBoostedStats();
			if (flag)
			{
				this.lockSprite = "server_favorite";
				color = this.statBoostedColor;
			}
			if (this.itemStack.itemValue.HasMods())
			{
				this.lockSprite = "ui_game_symbol_modded";
				if (!flag)
				{
					color = Color.white;
				}
			}
			if (color.a > 0f)
			{
				this.setLockTypeIconColor(color);
				this.flashLockTypeIcon = XUiC_ItemStack.flashLockTypes.None;
			}
		}
		if (this.StackLocation == XUiC_ItemStack.StackLocationTypes.Part)
		{
			this.lockSprite = "";
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setLockTypeIconColor(Color _color)
	{
		if (this.lockTypeIcon == null)
		{
			return;
		}
		this.lockTypeIcon.Color = _color;
	}

	public void ForceRefreshItemStack()
	{
		XUiEvent_SlotChangedEventHandler slotChangedEvent = this.SlotChangedEvent;
		if (slotChangedEvent == null)
		{
			return;
		}
		slotChangedEvent(this.SlotNumber, this.itemStack);
	}

	public bool IsFavorite
	{
		get
		{
			return !this.itemStack.IsEmpty() && this.xui.playerUI.entityPlayer.favoriteCreativeStacks.Contains((ushort)this.itemStack.itemValue.type);
		}
	}

	public virtual string ItemIcon
	{
		get
		{
			if (this.itemStack.IsEmpty())
			{
				return "";
			}
			ItemClass itemClassOrMissing = this.itemClassOrMissing;
			ItemClassBlock itemClassBlock = itemClassOrMissing as ItemClassBlock;
			Block block = ((itemClassBlock != null) ? itemClassBlock.GetBlock() : null);
			if (block == null)
			{
				return this.itemStack.itemValue.GetPropertyOverride(ItemClass.PropCustomIcon, itemClassOrMissing.GetIconName());
			}
			if (!block.SelectAlternates)
			{
				return this.itemStack.itemValue.GetPropertyOverride(ItemClass.PropCustomIcon, itemClassOrMissing.GetIconName());
			}
			return block.GetAltBlockValue(this.itemStack.itemValue.Meta).Block.GetIconName();
		}
	}

	public virtual string ItemIconColor
	{
		get
		{
			ItemClass itemClassOrMissing = this.itemClassOrMissing;
			if (itemClassOrMissing == null)
			{
				return "255,255,255,0";
			}
			Color32 color = itemClassOrMissing.GetIconTint(this.itemStack.itemValue);
			return this.itemiconcolorFormatter.Format(color);
		}
	}

	public bool GreyedOut
	{
		get
		{
			return this.itemIconSprite.UIAtlas == "ItemIconAtlasGreyscale";
		}
		set
		{
			if (this.itemIconSprite != null)
			{
				this.itemIconSprite.UIAtlas = (value ? "ItemIconAtlasGreyscale" : "ItemIconAtlas");
			}
		}
	}

	public string ItemNameText
	{
		get
		{
			if (this.itemStack.IsEmpty())
			{
				return "";
			}
			ItemClass itemClassOrMissing = this.itemClassOrMissing;
			string text = itemClassOrMissing.GetLocalizedItemName();
			if (itemClassOrMissing.IsBlock())
			{
				text = Block.list[this.itemStack.itemValue.type].GetLocalizedBlockName(this.itemStack.itemValue);
			}
			if (!this.PrefixId)
			{
				return text;
			}
			int itemOrBlockId = this.itemStack.itemValue.GetItemOrBlockId();
			return string.Format("{0}\n({1}) {2}", text, itemOrBlockId, itemClassOrMissing.Name);
		}
	}

	[XuiXmlBinding("hasdurability")]
	public bool ShowDurability
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (!this.IsLocked || this.lockType == XUiC_ItemStack.LockTypes.None)
			{
				ItemClass itemClass = this.itemClass;
				return itemClass != null && itemClass.ShowQualityBar;
			}
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 1768227542U)
		{
			if (num <= 727013168U)
			{
				if (num <= 468119243U)
				{
					if (num != 143608997U)
					{
						if (num != 402247192U)
						{
							if (num == 468119243U)
							{
								if (_bindingName == "haspermadurability")
								{
									if (this.ShowDurability)
									{
										ItemStack itemStack = this.itemStack;
										if (((itemStack != null) ? itemStack.itemValue : null) != null && EntityPlayerLocal.PermaDegrationOn)
										{
											_value = (this.itemStack.itemValue.MaxDurabilityModifier < 1f).ToString();
											return true;
										}
									}
									_value = "false";
									return true;
								}
							}
						}
						else if (_bindingName == "userlockedslot")
						{
							_value = this.UserLockedSlot.ToString();
							return true;
						}
					}
					else if (_bindingName == "removeddurabilityfill")
					{
						ItemStack itemStack2 = this.itemStack;
						if (((itemStack2 != null) ? itemStack2.itemValue : null) == null)
						{
							_value = "1";
							return true;
						}
						_value = this.removeddurabilityFillFormatter.Format(this.itemStack.itemValue.MaxDurabilityModifier);
						return true;
					}
				}
				else if (num != 684314633U)
				{
					if (num != 696352375U)
					{
						if (num == 727013168U)
						{
							if (_bindingName == "backgroundcolor")
							{
								_value = this.backgroundcolorFormatter.Format(this.AttributeLock ? this.attributeLockColor : this.backgroundColor);
								return true;
							}
						}
					}
					else if (_bindingName == "isQuickSwap")
					{
						_value = this.isQuickSwap.ToString();
						return true;
					}
				}
				else if (_bindingName == "stacklockicon")
				{
					if (this.stackLockType == XUiC_ItemStack.StackLockTypes.Quest)
					{
						_value = "ui_game_symbol_quest";
					}
					else if (this.attributeLock && this.itemStack.IsEmpty())
					{
						_value = "ui_game_symbol_pack_mule";
					}
					else
					{
						_value = "ui_game_symbol_lock";
					}
					return true;
				}
			}
			else if (num <= 1062608009U)
			{
				if (num != 770575247U)
				{
					if (num != 847165955U)
					{
						if (num == 1062608009U)
						{
							if (_bindingName == "durabilitycolor")
							{
								CachedStringFormatter<Color32> cachedStringFormatter = this.durabilitycolorFormatter;
								ItemStack itemStack3 = this.itemStack;
								_value = cachedStringFormatter.Format(QualityInfo.GetQualityColor((int)((itemStack3 != null) ? itemStack3.itemValue.Quality : 0)));
								return true;
							}
						}
					}
					else if (_bindingName == "itemtypeicon")
					{
						_value = "";
						if (!this.itemStack.IsEmpty())
						{
							ItemClass itemClassOrMissing = this.itemClassOrMissing;
							if (itemClassOrMissing != null)
							{
								if (itemClassOrMissing.IsBlock() && this.itemStack.itemValue.TextureFullArray.IsDefault)
								{
									_value = Block.list[this.itemStack.itemValue.type].ItemTypeIcon;
								}
								else
								{
									if (itemClassOrMissing.AltItemTypeIcon != null && itemClassOrMissing.Unlocks != "" && XUiM_ItemStack.CheckKnown(this.xui.playerUI.entityPlayer, itemClassOrMissing, this.itemStack.itemValue))
									{
										_value = itemClassOrMissing.AltItemTypeIcon;
										return true;
									}
									_value = itemClassOrMissing.ItemTypeIcon;
								}
							}
						}
						return true;
					}
				}
				else if (_bindingName == "stacklockcolor")
				{
					if (this.attributeLock && this.itemStack.IsEmpty())
					{
						_value = "200,200,200,64";
					}
					else
					{
						_value = "255,255,255,255";
					}
					return true;
				}
			}
			else if (num != 1129104269U)
			{
				if (num != 1388578781U)
				{
					if (num == 1768227542U)
					{
						if (_bindingName == "selectionbordercolor")
						{
							_value = this.selectionbordercolorFormatter.Format(this.SelectionBorderColor);
							return true;
						}
					}
				}
				else if (_bindingName == "hasitemtypeicon")
				{
					if (this.itemStack.IsEmpty() || !string.IsNullOrEmpty(this.lockSprite))
					{
						_value = "false";
					}
					else
					{
						ItemClass itemClassOrMissing2 = this.itemClassOrMissing;
						if (itemClassOrMissing2 == null)
						{
							_value = "false";
						}
						else
						{
							_value = (itemClassOrMissing2.IsBlock() ? (Block.list[this.itemStack.itemValue.type].ItemTypeIcon != "").ToString() : (itemClassOrMissing2.ItemTypeIcon != "").ToString());
						}
					}
					return true;
				}
			}
			else if (_bindingName == "showicon")
			{
				_value = (this.ItemIcon != "").ToString();
				return true;
			}
		}
		else if (num <= 2944858628U)
		{
			if (num <= 2648037987U)
			{
				if (num != 1820482109U)
				{
					if (num != 2412344255U)
					{
						if (num == 2648037987U)
						{
							if (_bindingName == "islocked")
							{
								_value = this.isLocked.ToString();
								return true;
							}
						}
					}
					else if (_bindingName == "isassemblelocked")
					{
						_value = ((this.stackLockType != XUiC_ItemStack.StackLockTypes.None && this.stackLockType != XUiC_ItemStack.StackLockTypes.Hidden) || (this.attributeLock && this.itemStack.IsEmpty())).ToString();
						return true;
					}
				}
				else if (_bindingName == "isfavorite")
				{
					_value = (this.ShowFavorites && this.IsFavorite).ToString();
					return true;
				}
			}
			else if (num != 2705680661U)
			{
				if (num != 2733860383U)
				{
					if (num == 2944858628U)
					{
						if (_bindingName == "hasdurability")
						{
							_value = this.ShowDurability.ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "locktypeicon")
				{
					_value = this.lockSprite ?? "";
					return true;
				}
			}
			else if (_bindingName == "itemcount")
			{
				_value = "";
				if (!this.itemStack.IsEmpty())
				{
					if (this.ShowDurability)
					{
						_value = ((this.itemStack.itemValue.Quality > 0) ? this.itemcountFormatter.Format((int)this.itemStack.itemValue.Quality) : (this.itemStack.itemValue.IsMod ? "*" : ""));
					}
					else
					{
						_value = ((this.itemClassOrMissing.Stacknumber == 1) ? "" : this.itemcountFormatter.Format(this.itemStack.count));
					}
				}
				return true;
			}
		}
		else if (num <= 3741212336U)
		{
			if (num != 3106195591U)
			{
				if (num != 3708628627U)
				{
					if (num == 3741212336U)
					{
						if (_bindingName == "tooltip")
						{
							_value = this.ItemNameText;
							return true;
						}
					}
				}
				else if (_bindingName == "itemicon")
				{
					_value = this.ItemIcon;
					return true;
				}
			}
			else if (_bindingName == "iconcolor")
			{
				_value = this.ItemIconColor;
				return true;
			}
		}
		else if (num != 4046184996U)
		{
			if (num != 4049247086U)
			{
				if (num == 4172540779U)
				{
					if (_bindingName == "durabilityfill")
					{
						ItemStack itemStack4 = this.itemStack;
						_value = ((((itemStack4 != null) ? itemStack4.itemValue : null) == null) ? "0.0" : ((this.itemStack.itemValue.MaxUseTimes == 0) ? "1" : this.durabilityFillFormatter.Format(((float)this.itemStack.itemValue.MaxUseTimes - this.itemStack.itemValue.UseTimes) / (float)this.itemStack.itemValue.MaxUseTimesUI)));
						return true;
					}
				}
			}
			else if (_bindingName == "itemtypeicontint")
			{
				_value = "255,255,255,255";
				if (!this.itemStack.IsEmpty())
				{
					ItemClass itemClassOrMissing3 = this.itemClassOrMissing;
					if (itemClassOrMissing3 != null && itemClassOrMissing3.Unlocks != "" && XUiM_ItemStack.CheckKnown(this.xui.playerUI.entityPlayer, itemClassOrMissing3, this.itemStack.itemValue))
					{
						_value = this.altitemtypeiconcolorFormatter.Format(itemClassOrMissing3.AltItemTypeIconColor);
					}
				}
				return true;
			}
		}
		else if (_bindingName == "ishovered")
		{
			_value = this.isOver.ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	[XuiXmlBinding("ishovered")]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bindingIshovered()
	{
		return this.isOver;
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_name);
		if (num <= 783618599U)
		{
			if (num <= 551907657U)
			{
				if (num <= 448973864U)
				{
					if (num != 310027284U)
					{
						if (num == 448973864U)
						{
							if (_name == "mod_already_equipped_color")
							{
								this.modAlreadyEquippedColor = StringParsers.ParseColor32(_value);
								return true;
							}
						}
					}
					else if (_name == "press_color")
					{
						this.pressColor = StringParsers.ParseColor32(_value);
						return true;
					}
				}
				else if (num != 505492686U)
				{
					if (num == 551907657U)
					{
						if (_name == "background_color")
						{
							this.backgroundColor = StringParsers.ParseColor32(_value);
							return true;
						}
					}
				}
				else if (_name == "show_favorites")
				{
					this.ShowFavorites = StringParsers.ParseBool(_value, 0, -1, true);
					return true;
				}
			}
			else if (num <= 589168786U)
			{
				if (num != 587922239U)
				{
					if (num == 589168786U)
					{
						if (_name == "final_pressed_color")
						{
							this.finalPressedColor = StringParsers.ParseColor32(_value);
							return true;
						}
					}
				}
				else if (_name == "highlight_color")
				{
					this.highlightColor = StringParsers.ParseColor32(_value);
					return true;
				}
			}
			else if (num != 605893325U)
			{
				if (num == 783618599U)
				{
					if (_name == "hover_icon_grow")
					{
						this.HoverIconGrow = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
						return true;
					}
				}
			}
			else if (_name == "pickup_sound")
			{
				this.xui.LoadData<AudioClip>(_value, delegate(AudioClip _o)
				{
					this.pickupSound = _o;
				});
				return true;
			}
		}
		else if (num <= 1808090607U)
		{
			if (num <= 933221862U)
			{
				if (num != 785383727U)
				{
					if (num == 933221862U)
					{
						if (_name == "allow_dropping")
						{
							this.AllowDropping = StringParsers.ParseBool(_value, 0, -1, true);
							return true;
						}
					}
				}
				else if (_name == "prefix_id")
				{
					this.PrefixId = StringParsers.ParseBool(_value, 0, -1, true);
					return true;
				}
			}
			else if (num != 1079211868U)
			{
				if (num == 1808090607U)
				{
					if (_name == "attribute_lock_color")
					{
						this.attributeLockColor = StringParsers.ParseColor32(_value);
						return true;
					}
				}
			}
			else if (_name == "mod_allowed_color")
			{
				this.modAllowedColor = StringParsers.ParseColor32(_value);
				return true;
			}
		}
		else if (num <= 3096841498U)
		{
			if (num != 2540527193U)
			{
				if (num == 3096841498U)
				{
					if (_name == "override_stack_count")
					{
						this.OverrideStackCount = StringParsers.ParseSInt32(_value, 0, -1, NumberStyles.Integer);
						return true;
					}
				}
			}
			else if (_name == "select_color")
			{
				this.selectColor = StringParsers.ParseColor32(_value);
				return true;
			}
		}
		else if (num != 3396886192U)
		{
			if (num != 3919060864U)
			{
				if (num == 3936377800U)
				{
					if (_name == "pickup_snap_distance")
					{
						this.PickupSnapDistance = int.Parse(_value);
						return true;
					}
				}
			}
			else if (_name == "place_sound")
			{
				this.xui.LoadData<AudioClip>(_value, delegate(AudioClip _o)
				{
					this.placeSound = _o;
				});
				return true;
			}
		}
		else if (_name == "holding_color")
		{
			this.holdingColor = StringParsers.ParseColor32(_value);
			return true;
		}
		return base.ParseAttribute(_name, _value);
	}

	public static bool IsStackLocationFromPlayer(XUiC_ItemStack.StackLocationTypes? location)
	{
		if (location != null)
		{
			XUiC_ItemStack.StackLocationTypes? stackLocationTypes = location;
			XUiC_ItemStack.StackLocationTypes stackLocationTypes2 = XUiC_ItemStack.StackLocationTypes.Backpack;
			if (!((stackLocationTypes.GetValueOrDefault() == stackLocationTypes2) & (stackLocationTypes != null)))
			{
				stackLocationTypes = location;
				stackLocationTypes2 = XUiC_ItemStack.StackLocationTypes.ToolBelt;
				if (!((stackLocationTypes.GetValueOrDefault() == stackLocationTypes2) & (stackLocationTypes != null)))
				{
					stackLocationTypes = location;
					stackLocationTypes2 = XUiC_ItemStack.StackLocationTypes.Equipment;
					return (stackLocationTypes.GetValueOrDefault() == stackLocationTypes2) & (stackLocationTypes != null);
				}
			}
			return true;
		}
		return false;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <updateBackgroundTexture>g__setTextureForOpaqueMeshTextureId|132_0(int _textureId)
	{
		if (_textureId == 0)
		{
			this.backgroundTexture.Texture = null;
			return;
		}
		MeshDescription meshDescription = MeshDescription.meshes[0];
		UVRectTiling uvrectTiling = meshDescription.textureAtlas.uvMapping[_textureId];
		this.backgroundTexture.Texture = meshDescription.textureAtlas.diffuseTexture;
		if (meshDescription.bTextureArray)
		{
			this.backgroundTexture.Material.SetTexture("_BumpMap", meshDescription.textureAtlas.normalTexture);
			this.backgroundTexture.Material.SetFloat("_Index", (float)uvrectTiling.index);
			this.backgroundTexture.Material.SetFloat("_Size", (float)uvrectTiling.blockW);
		}
		else
		{
			this.backgroundTexture.UVRect = uvrectTiling.uv;
		}
		this.backgroundTexture.SetTextureDirty();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemStack itemStack = ItemStack.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioClip pickupSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioClip placeSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 pressColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool lastClicked;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemStack.LockTypes lockType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lockSprite;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float lockTime;

	public int TimeInterval = 5;

	public int OverrideStackCount = -1;

	public bool _isQuickSwap;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 selectionBorderColor;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int PickupSnapDistance = 4;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 finalPressedColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 highlightColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 holdingColor = new Color32(byte.MaxValue, 128, 0, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 attributeLockColor = new Color32(48, 48, 48, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 modAllowedColor = Color.green;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 modAlreadyEquippedColor = Color.yellow;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 statBoostedColor = new Color32(byte.MaxValue, 175, 11, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite lockTypeIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite itemIconSprite;

	public XUiV_Label timer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Texture backgroundTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemStack.flashLockTypes flashLockTypeIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemStack.StackLockTypes stackLockType;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool attributeLock;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDragAndDrop;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool userLockedSlot;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentInterval = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public TweenScale tweenScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startMousePos = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerMarker pmIsUpdate = new ProfilerMarker("XCItemStack");

	[PublicizedFrom(EAccessModifier.Private)]
	public ProfilerMarker pmIsUpdateBase = new ProfilerMarker("XCItemStack.Base");

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly CachedStringFormatterXuiRgbaColor itemiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt itemcountFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat durabilityFillFormatter = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat removeddurabilityFillFormatter = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor altitemtypeiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor durabilitycolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor backgroundcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor selectionbordercolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public enum flashLockTypes
	{
		None,
		Allowed,
		AlreadyEquipped
	}

	public enum LockTypes
	{
		None,
		Shell,
		Crafting,
		Repairing,
		Scrapping,
		Burning
	}

	public enum StackLockTypes
	{
		None,
		Assemble,
		Quest,
		Tool,
		Hidden
	}

	public enum StackLocationTypes
	{
		Backpack,
		ToolBelt,
		LootContainer,
		Equipment,
		Creative,
		Vehicle,
		Workstation,
		Merge,
		Cosmetics,
		Part,
		CollectorCatalysts,
		CollectorFuel,
		Collector
	}
}
