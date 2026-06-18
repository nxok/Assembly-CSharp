using System;
using System.Collections.Generic;
using InControl;

public class PlayerActionsLocal : PlayerActionsBase
{
	public int InventorySlotWasPressed
	{
		get
		{
			for (int i = 0; i < this.InventoryActions.Count; i++)
			{
				if (this.InventoryActions[i].WasPressed)
				{
					return i;
				}
			}
			return -1;
		}
	}

	public int InventorySlotWasReleased
	{
		get
		{
			for (int i = 0; i < this.InventoryActions.Count; i++)
			{
				if (this.InventoryActions[i].WasReleased)
				{
					return i;
				}
			}
			return -1;
		}
	}

	public int InventorySlotIsPressed
	{
		get
		{
			for (int i = 0; i < this.InventoryActions.Count; i++)
			{
				if (this.InventoryActions[i].IsPressed)
				{
					return i;
				}
			}
			return -1;
		}
	}

	public PlayerActionsGUI GUIActions { get; }

	public PlayerActionsVehicle VehicleActions { get; }

	public PlayerActionsPermanent PermanentActions { get; }

	public PlayerActionsLocal()
	{
		base.Name = "local";
		this.GUIActions = new PlayerActionsGUI
		{
			Enabled = false
		};
		this.PermanentActions = new PlayerActionsPermanent
		{
			Enabled = true
		};
		this.VehicleActions = new PlayerActionsVehicle
		{
			Enabled = false
		};
		base.UserData = new PlayerActionData.ActionSetUserData(new PlayerActionsBase[] { this.PermanentActions });
		this.VehicleActions.UserData = new PlayerActionData.ActionSetUserData(new PlayerActionsBase[] { this.PermanentActions });
		this.GUIActions.UserData = new PlayerActionData.ActionSetUserData(new PlayerActionsBase[] { this.PermanentActions });
		this.PermanentActions.UserData = new PlayerActionData.ActionSetUserData(new PlayerActionsBase[] { this, this.VehicleActions, this.GUIActions });
		this.InventoryActions.Add(this.InventorySlot1);
		this.InventoryActions.Add(this.InventorySlot2);
		this.InventoryActions.Add(this.InventorySlot3);
		this.InventoryActions.Add(this.InventorySlot4);
		this.InventoryActions.Add(this.InventorySlot5);
		this.InventoryActions.Add(this.InventorySlot6);
		this.InventoryActions.Add(this.InventorySlot7);
		this.InventoryActions.Add(this.InventorySlot8);
		this.InventoryActions.Add(this.InventorySlot9);
		this.InventoryActions.Add(this.InventorySlot10);
		InputManager.OnActiveDeviceChanged += delegate(InputDevice inputDevice)
		{
			this.UpdateDeadzones();
		};
		this.UpdateDeadzones();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateActions()
	{
		this.MoveForward = base.CreatePlayerAction("Forward");
		this.MoveForward.UserData = new PlayerActionData.ActionUserData("inpActPlayerMoveForwardName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.MoveBack = base.CreatePlayerAction("Back");
		this.MoveBack.UserData = new PlayerActionData.ActionUserData("inpActPlayerMoveBackName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.MoveLeft = base.CreatePlayerAction("Left");
		this.MoveLeft.UserData = new PlayerActionData.ActionUserData("inpActPlayerMoveLeftName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.MoveRight = base.CreatePlayerAction("Right");
		this.MoveRight.UserData = new PlayerActionData.ActionUserData("inpActPlayerMoveRightName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Move = base.CreateTwoAxisPlayerAction(this.MoveLeft, this.MoveRight, this.MoveBack, this.MoveForward);
		this.LookLeft = base.CreatePlayerAction("LookLeft");
		this.LookLeft.UserData = new PlayerActionData.ActionUserData("inpActPlayerLookLeft", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.LookRight = base.CreatePlayerAction("LookRight");
		this.LookRight.UserData = new PlayerActionData.ActionUserData("inpActPlayerLookRight", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.LookUp = base.CreatePlayerAction("LookUp");
		this.LookUp.UserData = new PlayerActionData.ActionUserData("inpActPlayerLookUp", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.LookDown = base.CreatePlayerAction("LookDown");
		this.LookDown.UserData = new PlayerActionData.ActionUserData("inpActPlayerLookDown", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.Look = base.CreateTwoAxisPlayerAction(this.LookLeft, this.LookRight, this.LookDown, this.LookUp);
		this.Primary = base.CreatePlayerAction("Primary");
		this.Primary.UserData = new PlayerActionData.ActionUserData("inpActPlayerPrimaryName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Primary.StateThreshold = 0.25f;
		this.Secondary = base.CreatePlayerAction("Secondary");
		this.Secondary.UserData = new PlayerActionData.ActionUserData("inpActPlayerSecondaryName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Secondary.StateThreshold = 0.25f;
		this.Run = base.CreatePlayerAction("Run");
		this.Run.UserData = new PlayerActionData.ActionUserData("inpActPlayerRunName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Jump = base.CreatePlayerAction("Jump");
		this.Jump.UserData = new PlayerActionData.ActionUserData("inpActPlayerJumpName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Crouch = base.CreatePlayerAction("Crouch");
		this.Crouch.UserData = new PlayerActionData.ActionUserData("inpActPlayerCrouchName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.JumpCrouch = base.CreateOneAxisPlayerAction(this.Crouch, this.Jump);
		this.ToggleCrouch = base.CreatePlayerAction("ToggleCrouch");
		this.ToggleCrouch.UserData = new PlayerActionData.ActionUserData("inpActPlayerToggleCrouchName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.ScrollUp = base.CreatePlayerAction("ScrollUp");
		this.ScrollUp.UserData = new PlayerActionData.ActionUserData("inpActScopeZoomInName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, true, false, true);
		this.ScrollDown = base.CreatePlayerAction("ScrollDown");
		this.ScrollDown.UserData = new PlayerActionData.ActionUserData("inpActScopeZoomOutName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, true, false, true);
		this.Scroll = base.CreateOneAxisPlayerAction(this.ScrollDown, this.ScrollUp);
		this.Activate = base.CreatePlayerAction("Activate");
		this.Activate.UserData = new PlayerActionData.ActionUserData("inpActActivateName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.Activate.FirstRepeatDelay = 0.3f;
		this.Drop = base.CreatePlayerAction("Drop");
		this.Drop.UserData = new PlayerActionData.ActionUserData("inpActPlayerDropName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Swap = base.CreatePlayerAction("Swap");
		this.Swap.UserData = new PlayerActionData.ActionUserData("inpActSwapName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.Reload = base.CreatePlayerAction("Reload");
		this.Reload.UserData = new PlayerActionData.ActionUserData("inpActPlayerReloadName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.Reload.FirstRepeatDelay = 0.3f;
		this.ToggleFlashlight = base.CreatePlayerAction("ToggleFlashlight");
		this.ToggleFlashlight.UserData = new PlayerActionData.ActionUserData("inpActPlayerToggleFlashlightName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.ToggleFlashlight.FirstRepeatDelay = 0.3f;
		this.InventorySlot1 = base.CreatePlayerAction("Inventory1");
		this.InventorySlot1.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot1Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.InventorySlot2 = base.CreatePlayerAction("Inventory2");
		this.InventorySlot2.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot2Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.InventorySlot3 = base.CreatePlayerAction("Inventory3");
		this.InventorySlot3.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot3Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.InventorySlot4 = base.CreatePlayerAction("Inventory4");
		this.InventorySlot4.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot4Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.InventorySlot5 = base.CreatePlayerAction("Inventory5");
		this.InventorySlot5.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot5Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.InventorySlot6 = base.CreatePlayerAction("Inventory6");
		this.InventorySlot6.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot6Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.InventorySlot7 = base.CreatePlayerAction("Inventory7");
		this.InventorySlot7.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot7Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.InventorySlot8 = base.CreatePlayerAction("Inventory8");
		this.InventorySlot8.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot8Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.InventorySlot9 = base.CreatePlayerAction("Inventory9");
		this.InventorySlot9.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot9Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.InventorySlot10 = base.CreatePlayerAction("Inventory10");
		this.InventorySlot10.UserData = new PlayerActionData.ActionUserData("inpActInventorySlot10Name", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.InventorySlotLeft = base.CreatePlayerAction("InventorySelectLeft");
		this.InventorySlotLeft.UserData = new PlayerActionData.ActionUserData("inpActInventorySlotLeftName", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.InventorySlotRight = base.CreatePlayerAction("InventorySelectRight");
		this.InventorySlotRight.UserData = new PlayerActionData.ActionUserData("inpActInventorySlotRightName", null, PlayerActionData.GroupToolbelt, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Menu = base.CreatePlayerAction("Menu");
		this.Menu.UserData = new PlayerActionData.ActionUserData("inpActMenuName", null, PlayerActionData.GroupMenu, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.God = base.CreatePlayerAction("God");
		this.God.UserData = new PlayerActionData.ActionUserData("inpActGodModeName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.Fly = base.CreatePlayerAction("Fly");
		this.Fly.UserData = new PlayerActionData.ActionUserData("inpActFlyModeName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.Invisible = base.CreatePlayerAction("Invisible");
		this.Invisible.UserData = new PlayerActionData.ActionUserData("inpActInvisibleModeName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.IncSpeed = base.CreatePlayerAction("IncSpeed");
		this.IncSpeed.UserData = new PlayerActionData.ActionUserData("inpActIncGodSpeedName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.DecSpeed = base.CreatePlayerAction("DecSpeed");
		this.DecSpeed.UserData = new PlayerActionData.ActionUserData("inpActDecGodSpeedName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.GodAlternate = base.CreatePlayerAction("GodAlternate");
		this.GodAlternate.UserData = new PlayerActionData.ActionUserData("inpActGodAlternateModeName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.ControllerOnly, false, true, true, true);
		this.TeleportAlternate = base.CreatePlayerAction("TeleportAlternate");
		this.TeleportAlternate.UserData = new PlayerActionData.ActionUserData("inpActTeleportAlternateModeName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.ControllerOnly, false, true, true, true);
		this.CameraChange = base.CreatePlayerAction("CameraChange");
		this.CameraChange.UserData = new PlayerActionData.ActionUserData("inpActCameraChangeName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.CameraFunction = base.CreatePlayerAction("FlipCamera");
		this.CameraFunction.UserData = new PlayerActionData.ActionUserData("inpActFlipCameraName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.DetachCamera = base.CreatePlayerAction("DetachCamera");
		this.DetachCamera.UserData = new PlayerActionData.ActionUserData("inpActDetachCameraName", null, PlayerActionData.GroupEditCamera, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.ToggleDCMove = base.CreatePlayerAction("ToggleDCMove");
		this.ToggleDCMove.UserData = new PlayerActionData.ActionUserData("inpActToggleDCMoveName", null, PlayerActionData.GroupEditCamera, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.LockFreeCamera = base.CreatePlayerAction("LockFreeCamera");
		this.LockFreeCamera.UserData = new PlayerActionData.ActionUserData("inpActLockFreeCameraName", null, PlayerActionData.GroupEditCamera, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.SelectionFill = base.CreatePlayerAction("SelectionFill");
		this.SelectionFill.UserData = new PlayerActionData.ActionUserData("inpActSelectionFillName", null, PlayerActionData.GroupEditSelection, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.SelectionClear = base.CreatePlayerAction("SelectionClear");
		this.SelectionClear.UserData = new PlayerActionData.ActionUserData("inpActSelectionClearName", null, PlayerActionData.GroupEditSelection, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.SelectionSet = base.CreatePlayerAction("SelectionSet");
		this.SelectionSet.UserData = new PlayerActionData.ActionUserData("inpActSelectionSetName", null, PlayerActionData.GroupEditSelection, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.SelectionRotate = base.CreatePlayerAction("SelectionRotate");
		this.SelectionRotate.UserData = new PlayerActionData.ActionUserData("inpActSelectionRotateName", null, PlayerActionData.GroupEditSelection, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.SelectionDelete = base.CreatePlayerAction("SelectionDelete");
		this.SelectionDelete.UserData = new PlayerActionData.ActionUserData("inpActSelectionDeleteName", null, PlayerActionData.GroupEditSelection, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.SelectionMoveMode = base.CreatePlayerAction("SelectionMoveMode");
		this.SelectionMoveMode.UserData = new PlayerActionData.ActionUserData("inpActSelectionMoveModeName", null, PlayerActionData.GroupEditSelection, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.FocusCopyBlock = base.CreatePlayerAction("FocusCopyBlock");
		this.FocusCopyBlock.UserData = new PlayerActionData.ActionUserData("inpActFocusCopyBlockName", null, PlayerActionData.GroupEditOther, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.Prefab = base.CreatePlayerAction("Prefab");
		this.Prefab.UserData = new PlayerActionData.ActionUserData("inpActPrefabName", null, PlayerActionData.GroupEditOther, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.DensityM1 = base.CreatePlayerAction("DensityM1");
		this.DensityM1.UserData = new PlayerActionData.ActionUserData("inpActDensityM1Name", null, PlayerActionData.GroupEditOther, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.DensityP1 = base.CreatePlayerAction("DensityP1");
		this.DensityP1.UserData = new PlayerActionData.ActionUserData("inpActDensityP1Name", null, PlayerActionData.GroupEditOther, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.DensityM10 = base.CreatePlayerAction("DensityM10");
		this.DensityM10.UserData = new PlayerActionData.ActionUserData("inpActDensityM10Name", null, PlayerActionData.GroupEditOther, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.DensityP10 = base.CreatePlayerAction("DensityP10");
		this.DensityP10.UserData = new PlayerActionData.ActionUserData("inpActDensityP10Name", null, PlayerActionData.GroupEditOther, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.Inventory = base.CreatePlayerAction("Inventory");
		this.Inventory.UserData = new PlayerActionData.ActionUserData("inpActInventoryName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.Scoreboard = base.CreatePlayerAction("Scoreboard");
		this.Scoreboard.UserData = new PlayerActionData.ActionUserData("inpActScoreboardName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.QuickMenu = base.CreatePlayerAction("QuickMenu");
		this.QuickMenu.UserData = new PlayerActionData.ActionUserData("inpQuickMenuName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.AiFreeze = base.CreatePlayerAction("AiFreeze");
		this.AiFreeze.UserData = new PlayerActionData.ActionUserData("inpActAiFreezeName", null, PlayerActionData.GroupDebugFunctions, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultJoystickBindings()
	{
		base.ListenOptions.IncludeControllers = true;
		this.ConfigureJoystickLayout();
		this.Run.AddDefaultBinding(5);
		this.Jump.AddDefaultBinding(19);
		this.ToggleCrouch.AddDefaultBinding(20);
		this.Activate.AddDefaultBinding(22);
		this.Reload.AddDefaultBinding(21);
		this.Primary.AddDefaultBinding(16);
		this.Secondary.AddDefaultBinding(15);
		this.InventorySlotLeft.AddDefaultBinding(17);
		this.InventorySlotRight.AddDefaultBinding(18);
		this.Swap.AddDefaultBinding(13);
		this.Menu.AddDefaultBinding(106);
		this.Menu.AddDefaultBinding(104);
		this.Menu.AddDefaultBinding(101);
		this.Menu.AddDefaultBinding(113);
		this.Inventory.AddDefaultBinding(11);
		this.Scoreboard.AddDefaultBinding(109);
		this.Scoreboard.AddDefaultBinding(258);
		this.Scoreboard.AddDefaultBinding(100);
		this.GodAlternate.AddDefaultBinding(22);
		this.TeleportAlternate.AddDefaultBinding(19);
		this.QuickMenu.AddDefaultBinding(12);
		this.CameraFunction.AddDefaultBinding(10);
		this.ControllerRebindableActions.Clear();
		this.ControllerRebindableActions.Add(this.Jump);
		this.ControllerRebindableActions.Add(this.ToggleCrouch);
		this.ControllerRebindableActions.Add(this.Reload);
		this.ControllerRebindableActions.Add(this.Activate);
		this.ControllerRebindableActions.Add(this.Run);
		this.ControllerRebindableActions.Add(this.Primary);
		this.ControllerRebindableActions.Add(this.Secondary);
		this.ControllerRebindableActions.Add(this.Inventory);
		this.ControllerRebindableActions.Add(this.Swap);
		this.ControllerRebindableActions.Add(this.QuickMenu);
		this.ControllerRebindableActions.Add(this.CameraFunction);
		this.ControllerRebindableActions.Add(this.CameraChange);
		this.ControllerRebindableActions.Add(this.ToggleFlashlight);
		this.ControllerRebindableActions.Add(this.Drop);
		this.ControllerRebindableActions.Add(this.InventorySlotRight);
		this.ControllerRebindableActions.Add(this.InventorySlotLeft);
		this.ControllerRebindableActions.Add(this.InventorySlot1);
		this.ControllerRebindableActions.Add(this.InventorySlot2);
		this.ControllerRebindableActions.Add(this.InventorySlot3);
		this.ControllerRebindableActions.Add(this.InventorySlot4);
		this.ControllerRebindableActions.Add(this.InventorySlot5);
		this.ControllerRebindableActions.Add(this.InventorySlot6);
		this.ControllerRebindableActions.Add(this.InventorySlot7);
		this.ControllerRebindableActions.Add(this.InventorySlot8);
		this.ControllerRebindableActions.Add(this.InventorySlot9);
		this.ControllerRebindableActions.Add(this.InventorySlot10);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultKeyboardBindings()
	{
		base.ListenOptions.IncludeKeys = true;
		base.ListenOptions.IncludeMouseButtons = true;
		base.ListenOptions.IncludeMouseScrollWheel = true;
		this.MoveLeft.AddDefaultBinding(new Key[] { 36 });
		this.MoveRight.AddDefaultBinding(new Key[] { 39 });
		this.MoveForward.AddDefaultBinding(new Key[] { 58 });
		this.MoveBack.AddDefaultBinding(new Key[] { 54 });
		this.LookLeft.AddDefaultBinding(4);
		this.LookRight.AddDefaultBinding(5);
		this.LookUp.AddDefaultBinding(7);
		this.LookDown.AddDefaultBinding(6);
		this.Run.AddDefaultBinding(new Key[] { 5 });
		this.Jump.AddDefaultBinding(new Key[] { 76 });
		this.Crouch.AddDefaultBinding(new Key[] { 38 });
		this.ToggleCrouch.AddDefaultBinding(new Key[] { 8 });
		this.Drop.AddDefaultBinding(new Key[] { 42 });
		this.God.AddDefaultBinding(new Key[] { 52 });
		this.Fly.AddDefaultBinding(new Key[] { 43 });
		this.Invisible.AddDefaultBinding(new Key[] { 98 });
		this.IncSpeed.AddDefaultBinding(new Key[] { 1, 64 });
		this.DecSpeed.AddDefaultBinding(new Key[] { 1, 63 });
		this.CameraFunction.AddDefaultBinding(3);
		this.Primary.AddDefaultBinding(1);
		this.Secondary.AddDefaultBinding(2);
		this.SelectionFill.AddDefaultBinding(new Key[] { 47 });
		this.SelectionClear.AddDefaultBinding(new Key[] { 45 });
		this.SelectionSet.AddDefaultBinding(new Key[] { 61 });
		this.SelectionRotate.AddDefaultBinding(new Key[] { 59 });
		this.SelectionDelete.AddDefaultBinding(new Key[] { 65 });
		this.SelectionMoveMode.AddDefaultBinding(new Key[] { 77 });
		this.FocusCopyBlock.AddDefaultBinding(3);
		this.Prefab.AddDefaultBinding(new Key[] { 46 });
		this.DetachCamera.AddDefaultBinding(new Key[] { 51 });
		this.ToggleDCMove.AddDefaultBinding(new Key[] { 67 });
		this.LockFreeCamera.AddDefaultBinding(new Key[] { 88 });
		this.DensityM1.AddDefaultBinding(new Key[] { 84 });
		this.DensityP1.AddDefaultBinding(new Key[] { 83 });
		this.DensityM10.AddDefaultBinding(new Key[] { 85 });
		this.DensityP10.AddDefaultBinding(new Key[] { 86 });
		this.ScrollUp.AddDefaultBinding(8);
		this.ScrollDown.AddDefaultBinding(9);
		this.Menu.AddDefaultBinding(new Key[] { 13 });
		this.InventorySlot1.AddDefaultBinding(new Key[] { 27 });
		this.InventorySlot2.AddDefaultBinding(new Key[] { 28 });
		this.InventorySlot3.AddDefaultBinding(new Key[] { 29 });
		this.InventorySlot4.AddDefaultBinding(new Key[] { 30 });
		this.InventorySlot5.AddDefaultBinding(new Key[] { 31 });
		this.InventorySlot6.AddDefaultBinding(new Key[] { 32 });
		this.InventorySlot7.AddDefaultBinding(new Key[] { 33 });
		this.InventorySlot8.AddDefaultBinding(new Key[] { 34 });
		this.InventorySlot9.AddDefaultBinding(new Key[] { 35 });
		this.InventorySlot10.AddDefaultBinding(new Key[] { 26 });
		this.InventorySlotRight.AddDefaultBinding(9);
		this.InventorySlotLeft.AddDefaultBinding(8);
		this.AiFreeze.AddDefaultBinding(new Key[] { 99 });
	}

	public void SetDeadzones(float _left, float _right)
	{
		this.leftStickDeadzone = _left;
		this.rightStickDeadzone = _right;
		this.UpdateDeadzones();
	}

	public void UpdateDeadzones()
	{
		InputManager.ActiveDevice.LeftStick.LowerDeadZone = this.leftStickDeadzone;
		InputManager.ActiveDevice.RightStick.LowerDeadZone = this.rightStickDeadzone;
	}

	public void SetJoyStickLayout(eControllerJoystickLayout layout)
	{
		this.joystickLayout = layout;
		this.ConfigureJoystickLayout();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConfigureJoystickLayout()
	{
		switch (this.joystickLayout)
		{
		case eControllerJoystickLayout.Standard:
			this.MoveForward.AddDefaultBinding(1);
			this.MoveForward.RemoveBinding(new DeviceBindingSource(6));
			this.MoveBack.AddDefaultBinding(2);
			this.MoveBack.RemoveBinding(new DeviceBindingSource(7));
			this.MoveLeft.AddDefaultBinding(3);
			this.MoveLeft.RemoveBinding(new DeviceBindingSource(8));
			this.MoveRight.AddDefaultBinding(4);
			this.MoveRight.RemoveBinding(new DeviceBindingSource(9));
			this.LookUp.AddDefaultBinding(6);
			this.LookUp.RemoveBinding(new DeviceBindingSource(1));
			this.LookDown.AddDefaultBinding(7);
			this.LookDown.RemoveBinding(new DeviceBindingSource(2));
			this.LookLeft.AddDefaultBinding(8);
			this.LookLeft.RemoveBinding(new DeviceBindingSource(3));
			this.LookRight.AddDefaultBinding(9);
			this.LookRight.RemoveBinding(new DeviceBindingSource(4));
			return;
		case eControllerJoystickLayout.Southpaw:
			this.MoveForward.AddDefaultBinding(6);
			this.MoveForward.RemoveBinding(new DeviceBindingSource(1));
			this.MoveBack.AddDefaultBinding(7);
			this.MoveBack.RemoveBinding(new DeviceBindingSource(2));
			this.MoveLeft.AddDefaultBinding(8);
			this.MoveLeft.RemoveBinding(new DeviceBindingSource(3));
			this.MoveRight.AddDefaultBinding(9);
			this.MoveRight.RemoveBinding(new DeviceBindingSource(4));
			this.LookUp.AddDefaultBinding(1);
			this.LookUp.RemoveBinding(new DeviceBindingSource(6));
			this.LookDown.AddDefaultBinding(2);
			this.LookDown.RemoveBinding(new DeviceBindingSource(7));
			this.LookLeft.AddDefaultBinding(3);
			this.LookLeft.RemoveBinding(new DeviceBindingSource(8));
			this.LookRight.AddDefaultBinding(4);
			this.LookRight.RemoveBinding(new DeviceBindingSource(9));
			return;
		case eControllerJoystickLayout.Legacy:
			this.MoveForward.AddDefaultBinding(1);
			this.MoveForward.RemoveBinding(new DeviceBindingSource(6));
			this.MoveBack.AddDefaultBinding(2);
			this.MoveBack.RemoveBinding(new DeviceBindingSource(7));
			this.MoveLeft.AddDefaultBinding(8);
			this.MoveLeft.RemoveBinding(new DeviceBindingSource(3));
			this.MoveRight.AddDefaultBinding(9);
			this.MoveRight.RemoveBinding(new DeviceBindingSource(4));
			this.LookUp.AddDefaultBinding(6);
			this.LookUp.RemoveBinding(new DeviceBindingSource(1));
			this.LookDown.AddDefaultBinding(7);
			this.LookDown.RemoveBinding(new DeviceBindingSource(2));
			this.LookLeft.AddDefaultBinding(3);
			this.LookLeft.RemoveBinding(new DeviceBindingSource(8));
			this.LookRight.AddDefaultBinding(4);
			this.LookRight.RemoveBinding(new DeviceBindingSource(9));
			return;
		case eControllerJoystickLayout.LegacySouthpaw:
			this.MoveForward.AddDefaultBinding(6);
			this.MoveForward.RemoveBinding(new DeviceBindingSource(1));
			this.MoveBack.AddDefaultBinding(7);
			this.MoveBack.RemoveBinding(new DeviceBindingSource(2));
			this.MoveLeft.AddDefaultBinding(3);
			this.MoveLeft.RemoveBinding(new DeviceBindingSource(8));
			this.MoveRight.AddDefaultBinding(4);
			this.MoveRight.RemoveBinding(new DeviceBindingSource(9));
			this.LookUp.AddDefaultBinding(1);
			this.LookUp.RemoveBinding(new DeviceBindingSource(6));
			this.LookDown.AddDefaultBinding(2);
			this.LookDown.RemoveBinding(new DeviceBindingSource(7));
			this.LookLeft.AddDefaultBinding(8);
			this.LookLeft.RemoveBinding(new DeviceBindingSource(3));
			this.LookRight.AddDefaultBinding(9);
			this.LookRight.RemoveBinding(new DeviceBindingSource(4));
			return;
		default:
			return;
		}
	}

	public bool AnyGUIActionPressed()
	{
		using (IEnumerator<PlayerAction> enumerator = this.GUIActions.Actions.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.WasPressed)
				{
					return true;
				}
			}
		}
		return false;
	}

	public PlayerTwoAxisAction Move;

	public PlayerAction MoveLeft;

	public PlayerAction MoveRight;

	public PlayerAction MoveForward;

	public PlayerAction MoveBack;

	public PlayerTwoAxisAction Look;

	public PlayerAction LookLeft;

	public PlayerAction LookRight;

	public PlayerAction LookUp;

	public PlayerAction LookDown;

	public PlayerAction Run;

	public PlayerAction Jump;

	public PlayerAction Crouch;

	public PlayerOneAxisAction JumpCrouch;

	public PlayerAction ToggleCrouch;

	public PlayerAction Activate;

	public PlayerAction Drop;

	public PlayerAction Swap;

	public PlayerAction Reload;

	public PlayerAction Primary;

	public PlayerAction Secondary;

	public PlayerAction ToggleFlashlight;

	public PlayerAction God;

	public PlayerAction Fly;

	public PlayerAction Invisible;

	public PlayerAction IncSpeed;

	public PlayerAction DecSpeed;

	public PlayerAction GodAlternate;

	public PlayerAction TeleportAlternate;

	public PlayerAction CameraChange;

	public PlayerAction CameraFunction;

	public PlayerAction SelectionFill;

	public PlayerAction SelectionClear;

	public PlayerAction SelectionSet;

	public PlayerAction SelectionRotate;

	public PlayerAction SelectionDelete;

	public PlayerAction SelectionMoveMode;

	public PlayerAction FocusCopyBlock;

	public PlayerAction Prefab;

	public PlayerAction DetachCamera;

	public PlayerAction ToggleDCMove;

	public PlayerAction LockFreeCamera;

	public PlayerAction DensityM1;

	public PlayerAction DensityP1;

	public PlayerAction DensityM10;

	public PlayerAction DensityP10;

	public PlayerAction ScrollUp;

	public PlayerAction ScrollDown;

	public PlayerOneAxisAction Scroll;

	public PlayerAction Menu;

	public PlayerAction Inventory;

	public PlayerAction Scoreboard;

	public PlayerAction InventorySlot1;

	public PlayerAction InventorySlot2;

	public PlayerAction InventorySlot3;

	public PlayerAction InventorySlot4;

	public PlayerAction InventorySlot5;

	public PlayerAction InventorySlot6;

	public PlayerAction InventorySlot7;

	public PlayerAction InventorySlot8;

	public PlayerAction InventorySlot9;

	public PlayerAction InventorySlot10;

	public PlayerAction InventorySlotLeft;

	public PlayerAction InventorySlotRight;

	public PlayerAction AiFreeze;

	public PlayerAction QuickMenu;

	public float leftStickDeadzone = 0.1f;

	public float rightStickDeadzone = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PlayerAction> InventoryActions = new List<PlayerAction>();

	public eControllerJoystickLayout joystickLayout;
}
