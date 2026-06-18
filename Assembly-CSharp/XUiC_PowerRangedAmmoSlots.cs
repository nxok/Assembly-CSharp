using System;
using Audio;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PowerRangedAmmoSlots : XUiC_ItemStackGrid
{
	public TileEntityPoweredRangedTrap TileEntity
	{
		get
		{
			return this.tileEntity;
		}
		set
		{
			this.tileEntity = value;
			this.SetSlots(this.tileEntity.ItemSlots);
		}
	}

	public override void HandleSlotChangedEvent(int _slotNumber, ItemStack _stack)
	{
		base.HandleSlotChangedEvent(_slotNumber, _stack);
		this.tileEntity.SetSendSlots();
		this.tileEntity.SetModified();
	}

	public override void Init()
	{
		base.Init();
		this.btnOn = this.windowGroup.Controller.GetChildById("windowPowerTrapSlots").GetChildById("btnOn");
		this.btnOn_Background = (XUiV_Button)this.btnOn.GetChildById("clickable").ViewComponent;
		this.btnOn_Background.Controller.OnPress += this.btnOn_OnPress;
		XUiController xuiController = this.btnOn.GetChildById("lblOnOff");
		if (xuiController != null)
		{
			this.lblOnOff = (XUiV_Label)xuiController.ViewComponent;
		}
		xuiController = this.btnOn.GetChildById("sprOnOff");
		if (xuiController != null)
		{
			this.sprOnOff = (XUiV_Sprite)xuiController.ViewComponent;
		}
		this.IsDirty = true;
		this.turnOff = Localization.Get("xuiUnlockAmmo", false, null);
		this.turnOn = Localization.Get("xuiLockAmmo", false, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnOn_OnPress(XUiController _sender, int _mouseButton)
	{
		bool flag = false;
		for (int i = 0; i < this.TileEntity.ItemSlots.Length; i++)
		{
			if (!this.TileEntity.ItemSlots[i].IsEmpty())
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
			GameManager.ShowTooltip(this.xui.playerUI.localPlayer.entityPlayerLocal, Localization.Get("ttRequiresAmmo", false, null), false, false, 0f);
			return;
		}
		this.tileEntity.IsLocked = !this.tileEntity.IsLocked;
		this.tileEntity.SetModified();
		this.RefreshIsLocked(this.tileEntity.IsLocked);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setRequirements()
	{
		XUiC_ItemStack[] itemControllers = this.itemControllers;
		for (int i = 0; i < itemControllers.Length; i++)
		{
			XUiC_RequiredItemStack xuiC_RequiredItemStack = itemControllers[i] as XUiC_RequiredItemStack;
			if (xuiC_RequiredItemStack != null)
			{
				xuiC_RequiredItemStack.RequiredType = XUiC_RequiredItemStack.RequiredTypes.ItemClass;
				xuiC_RequiredItemStack.SetAllowedItemClasses(this.tileEntity.AmmoItems);
			}
		}
	}

	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Workstation;
		}
	}

	public XUiC_PowerRangedTrapWindowGroup Owner { get; set; }

	public virtual void SetSlots(ItemStack[] _stacks)
	{
		this.items = _stacks;
		base.SetStacks(_stacks);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.tileEntity.SetUserAccessing(true);
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		this.IsDirty = true;
		this.setRequirements();
		this.RefreshIsLocked(this.tileEntity.IsLocked);
		this.xui.CurrentPowerAmmoSlots = this;
		this.xui.CalloutWindow.AddCallout(UIUtils.ButtonIcon.RightBumper, "igcoPoweredTrapLockAmmo", XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
		this.xui.CalloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts, 0f);
	}

	public override void OnClose()
	{
		base.OnClose();
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnClose();
			base.ViewComponent.IsVisible = false;
		}
		if (!XUiC_CameraWindow.hackyIsOpeningMaximizedWindow)
		{
			this.tileEntity.SetUserAccessing(false);
			LockManager.Instance.UnlockRequestLocal();
			this.tileEntity.SetModified();
		}
		this.IsDirty = true;
		this.tileEntity = null;
		this.xui.CurrentPowerAmmoSlots = null;
		this.xui.CalloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshIsLocked(bool _isOn)
	{
		if (_isOn)
		{
			this.lblOnOff.Text = this.turnOff;
			if (this.sprOnOff != null)
			{
				this.sprOnOff.Color = this.onColor;
				this.sprOnOff.SpriteName = "ui_game_symbol_lock";
			}
		}
		else
		{
			this.lblOnOff.Text = this.turnOn;
			if (this.sprOnOff != null)
			{
				this.sprOnOff.Color = this.offColor;
				this.sprOnOff.SpriteName = "ui_game_symbol_unlock";
			}
		}
		XUiC_ItemStack[] itemControllers = this.itemControllers;
		for (int i = 0; i < itemControllers.Length; i++)
		{
			XUiC_RequiredItemStack xuiC_RequiredItemStack = itemControllers[i] as XUiC_RequiredItemStack;
			if (xuiC_RequiredItemStack != null)
			{
				xuiC_RequiredItemStack.ToolLock = _isOn;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetLocked(bool _isOn)
	{
		XUiC_ItemStack[] itemControllers = this.itemControllers;
		for (int i = 0; i < itemControllers.Length; i++)
		{
			XUiC_RequiredItemStack xuiC_RequiredItemStack = itemControllers[i] as XUiC_RequiredItemStack;
			if (xuiC_RequiredItemStack != null)
			{
				xuiC_RequiredItemStack.ToolLock = _isOn;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] _stackList)
	{
		base.UpdateBackend(_stackList);
		this.tileEntity.ItemSlots = _stackList;
		this.tileEntity.SetSendSlots();
		this.windowGroup.Controller.SetAllChildrenDirty();
	}

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null && GameManager.Instance.World == null)
		{
			return;
		}
		if (this.tileEntity == null)
		{
			return;
		}
		base.Update(_dt);
		if (this.lastLocked != this.tileEntity.IsLocked)
		{
			this.lastLocked = this.tileEntity.IsLocked;
			this.RefreshIsLocked(this.tileEntity.IsLocked);
		}
		if (this.tileEntity.IsLocked)
		{
			this.SetSlots(this.tileEntity.ItemSlots);
		}
		if (this.xui.playerUI.playerInput.GUIActions.WindowPagingRight.WasPressed && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			this.btnOn_OnPress(null, 0);
		}
		base.RefreshBindings();
	}

	public bool TryAddItemToSlot(ItemClass _itemClass, ItemStack _itemStack)
	{
		XUiC_ItemStack[] itemControllers = this.itemControllers;
		for (int i = 0; i < itemControllers.Length; i++)
		{
			((XUiC_RequiredItemStack)itemControllers[i]).TryStack(_itemStack, -1);
			if (_itemStack.count == 0)
			{
				return true;
			}
		}
		return _itemStack.count == 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnOn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnOn_Background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblOnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite sprOnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Color32 onColor = new Color32(250, byte.MaxValue, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Color32 offColor = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public string turnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public string turnOn;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPoweredRangedTrap tileEntity;
}
