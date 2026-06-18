using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PowerCameraWindowGroup : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiController xuiController = base.GetChildByType<XUiC_WindowNonPagingHeader>();
		if (xuiController != null)
		{
			this.nonPagingHeader = (XUiC_WindowNonPagingHeader)xuiController;
		}
		xuiController = base.GetChildByType<XUiC_CameraWindow>();
		if (xuiController != null)
		{
			this.cameraWindow = (XUiC_CameraWindow)xuiController;
		}
	}

	public TileEntityPowered TileEntity
	{
		get
		{
			return this.tileEntity;
		}
		set
		{
			this.tileEntity = value;
			this.cameraWindow.TileEntity = this.tileEntity;
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
			World world = GameManager.Instance.World;
			string localizedBlockName = this.tileEntity.GetChunk().GetBlock(this.tileEntity.localChunkPos).Block.GetLocalizedBlockName();
			this.nonPagingHeader.SetHeader(localizedBlockName);
		}
		this.IsDirty = true;
		this.xui.playerUI.CursorController.Locked = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		bool flag = true;
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		if (entityPlayer.IsDead() || Vector3.Distance(entityPlayer.position, this.TileEntity.ToWorldPos().ToVector3()) > Constants.cDigAndBuildDistance * Constants.cDigAndBuildDistance)
		{
			flag = false;
		}
		this.wasReleased = false;
		this.activeKeyDown = false;
		GUIWindow guiwindow;
		if (flag && this.xui.playerUI.windowManager.TryGetWindow(XUiC_CameraWindow.lastWindowGroup, out guiwindow))
		{
			XUiController controller = ((XUiWindowGroup)guiwindow).Controller;
			if (XUiC_CameraWindow.lastWindowGroup == "powerrangedtrap")
			{
				((XUiC_PowerRangedTrapWindowGroup)controller).TileEntity = (TileEntityPoweredRangedTrap)this.TileEntity;
			}
			else if (XUiC_CameraWindow.lastWindowGroup == "powertrigger")
			{
				((XUiC_PowerTriggerWindowGroup)controller).TileEntity = (TileEntityPoweredTrigger)this.TileEntity;
			}
			else
			{
				((XUiC_PoweredGenericWindowGroup)controller).TileEntity = this.TileEntity;
			}
			this.xui.playerUI.windowManager.Open(XUiC_CameraWindow.lastWindowGroup, true);
		}
		this.xui.playerUI.CursorController.Locked = false;
	}

	public bool UseEdgeDetection = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeader;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CameraWindow cameraWindow;

	public static string ID = "powercamera";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPowered tileEntity;
}
