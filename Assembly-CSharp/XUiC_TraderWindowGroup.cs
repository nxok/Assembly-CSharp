using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TraderWindowGroup : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiController xuiController = base.GetChildByType<XUiC_WindowNonPagingHeader>();
		if (xuiController != null)
		{
			this.nonPagingHeader = (XUiC_WindowNonPagingHeader)xuiController;
		}
		xuiController = base.GetChildByType<XUiC_TraderWindow>();
		if (xuiController != null)
		{
			this.TraderWindow = (XUiC_TraderWindow)xuiController;
		}
		xuiController = base.GetChildByType<XUiC_ServiceInfoWindow>();
		if (xuiController != null)
		{
			this.ServiceInfoWindow = (XUiC_ServiceInfoWindow)xuiController;
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
		this.xui.Trader.TraderWindowGroup = this;
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		if (this.nonPagingHeader != null)
		{
			this.nonPagingHeader.SetHeader((this.xui.Trader.Trader is TileEntityVendingMachine) ? Localization.Get("xuiVending", false, null) : Localization.Get("xuiTrader", false, null));
		}
		this.xui.RecenterWindowGroup(this.windowGroup, false);
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].OnOpen();
		}
		if (this.ServiceInfoWindow != null)
		{
			this.ServiceInfoWindow.ViewComponent.IsVisible = false;
		}
		EntityTrader entityTrader = this.xui.Trader.Trader as EntityTrader;
		if (entityTrader != null && this.xui.playerUI.entityPlayer.OverrideFOV != 30f)
		{
			this.xui.playerUI.entityPlayer.OverrideFOV = 30f;
			this.xui.playerUI.entityPlayer.OverrideLookAt = entityTrader.getHeadPosition();
		}
		this.xui.Dialog.KeepZoomOnClose = false;
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.wasReleased = false;
		this.activeKeyDown = false;
		this.xui.Trader.TraderWindowGroup = null;
		this.xui.playerUI.entityPlayer.OverrideFOV = -1f;
	}

	public void RefreshTraderItems()
	{
		this.TraderWindow.CompletedTransaction = true;
		this.TraderWindow.RefreshTraderItems();
	}

	public void RefreshTraderWindow()
	{
		this.TraderWindow.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeader;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TraderWindow TraderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServiceInfoWindow ServiceInfoWindow;

	public static string ID = "trader";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;
}
