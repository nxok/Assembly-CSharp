using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_VehicleWindowGroup : XUiController
{
	public EntityVehicle CurrentVehicleEntity
	{
		get
		{
			return this.currentVehicleEntity;
		}
		set
		{
			this.xui.Vehicle.CurrentVehicle = value;
			this.currentVehicleEntity = value;
			this.frameWindow.Vehicle = value;
			this.frameWindow.group = this;
			Vehicle vehicle = value.GetVehicle();
			ItemValue updatedItemValue = vehicle.GetUpdatedItemValue();
			ItemStack itemStack = new ItemStack(updatedItemValue, 1);
			this.partGrid.AssembleWindow = this.frameWindow;
			this.partGrid.CurrentVehicle = vehicle;
			this.partGrid.CurrentItem = itemStack;
			this.partGrid.SetMods(updatedItemValue.Modifications);
			this.cosmeticGrid.AssembleWindow = this.frameWindow;
			this.cosmeticGrid.CurrentItem = itemStack;
			this.cosmeticGrid.SetParts(updatedItemValue.CosmeticMods);
			XUiM_AssembleItem assembleItem = this.xui.AssembleItem;
			assembleItem.AssembleWindow = this.frameWindow;
			assembleItem.CurrentItem = itemStack;
			assembleItem.CurrentItemStackController = null;
		}
	}

	public override void Init()
	{
		base.Init();
		this.nonPagingHeaderWindow = base.GetChildByType<XUiC_WindowNonPagingHeader>();
		this.headerName = Localization.Get("xuiVehicle", false, null);
		this.frameWindow = base.GetChildByType<XUiC_VehicleFrameWindow>();
		this.partGrid = base.GetChildByType<XUiC_VehiclePartStackGrid>();
		this.cosmeticGrid = base.GetChildByType<XUiC_ItemCosmeticStackGrid>();
	}

	public override void Update(float _dt)
	{
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
					if (!this.xui.playerUI.windowManager.IsInputActive())
					{
						this.xui.playerUI.windowManager.CloseAllOpenModalWindows(null, false);
					}
				}
			}
		}
		if (this.currentVehicleEntity != null && !this.currentVehicleEntity.CheckUIInteraction())
		{
			this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.nonPagingHeaderWindow != null)
		{
			this.nonPagingHeaderWindow.SetHeader(this.headerName);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiM_AssembleItem assembleItem = this.xui.AssembleItem;
		assembleItem.AssembleWindow = null;
		if (assembleItem.CurrentItem.itemValue == this.xui.Vehicle.CurrentVehicle.GetVehicle().GetUpdatedItemValue())
		{
			assembleItem.CurrentItem = null;
			assembleItem.CurrentItemStackController = null;
		}
		this.wasReleased = false;
		this.activeKeyDown = false;
		this.CurrentVehicleEntity.StopUIInteraction();
		this.xui.Vehicle.CurrentVehicle = null;
	}

	public void OnItemChanged(ItemStack itemStack)
	{
		this.partGrid.CurrentItem = itemStack;
		this.partGrid.SetMods(itemStack.itemValue.Modifications);
		this.cosmeticGrid.CurrentItem = itemStack;
		this.cosmeticGrid.SetParts(itemStack.itemValue.CosmeticMods);
	}

	public static string ID = "vehicle";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_VehicleFrameWindow frameWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_VehiclePartStackGrid partGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemCosmeticStackGrid cosmeticGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public string headerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityVehicle currentVehicleEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;
}
