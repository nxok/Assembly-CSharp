using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PowerSourceWindowGroup : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiController xuiController = base.GetChildByType<XUiC_WindowNonPagingHeader>();
		if (xuiController != null)
		{
			this.nonPagingHeader = (XUiC_WindowNonPagingHeader)xuiController;
		}
		xuiController = base.GetChildByType<XUiC_PowerSourceStats>();
		if (xuiController != null)
		{
			this.GeneratorStats = (XUiC_PowerSourceStats)xuiController;
			this.GeneratorStats.Owner = this;
		}
		xuiController = base.GetChildByType<XUiC_PowerSourceSlots>();
		if (xuiController != null)
		{
			this.PowerSourceSlots = (XUiC_PowerSourceSlots)xuiController;
			this.PowerSourceSlots.Owner = this;
		}
	}

	public TileEntityPowerSource TileEntity
	{
		get
		{
			return this.tileEntity;
		}
		set
		{
			this.tileEntity = value;
			this.GeneratorStats.TileEntity = this.tileEntity;
			this.PowerSourceSlots.TileEntity = this.tileEntity;
		}
	}

	public void SetOn(bool isOn)
	{
		if (this.PowerSourceSlots != null && this.PowerSourceSlots.ViewComponent.IsVisible)
		{
			this.PowerSourceSlots.SetOn(isOn);
		}
	}

	public override void Update(float _dt)
	{
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
					if (!this.xui.playerUI.windowManager.IsInputActive())
					{
						this.xui.playerUI.windowManager.CloseAllOpenModalWindows(null, false);
					}
				}
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.nonPagingHeader != null)
		{
			string text = "";
			EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
			switch (this.TileEntity.PowerItemType)
			{
			case PowerItem.PowerItemTypes.Generator:
				text = Localization.Get("generatorbank", false, null);
				break;
			case PowerItem.PowerItemTypes.SolarPanel:
				text = Localization.Get("solarbank", false, null);
				break;
			case PowerItem.PowerItemTypes.BatteryBank:
				text = Localization.Get("batterybank", false, null);
				break;
			}
			this.nonPagingHeader.SetHeader(text);
		}
		if (this.PowerSourceSlots != null && this.TileEntity != null)
		{
			this.PowerSourceSlots.OnOpen();
		}
		Manager.BroadcastPlayByLocalPlayer(this.TileEntity.ToWorldPos().ToVector3() + Vector3.one * 0.5f, "open_vending");
		this.IsDirty = true;
		this.TileEntity.Destroyed += this.TileEntity_Destroyed;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.wasReleased = false;
		this.activeKeyDown = false;
		Manager.BroadcastPlayByLocalPlayer(this.TileEntity.ToWorldPos().ToVector3() + Vector3.one * 0.5f, "close_vending");
		this.TileEntity.Destroyed -= this.TileEntity_Destroyed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TileEntity_Destroyed(ITileEntity te)
	{
		if (this.TileEntity == te)
		{
			if (GameManager.Instance != null)
			{
				this.xui.playerUI.windowManager.Close("powersource");
				return;
			}
		}
		else
		{
			te.Destroyed -= this.TileEntity_Destroyed;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeader;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PowerSourceStats GeneratorStats;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PowerSourceSlots PowerSourceSlots;

	public static string ID = "powersource";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPowerSource tileEntity;
}
