using System;
using InControl;

public class PlayerActionsVehicle : PlayerActionsBase
{
	public PlayerActionsVehicle()
	{
		base.Name = "vehicle";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateActions()
	{
		this.MoveForward = base.CreatePlayerAction("Forward");
		this.MoveForward.UserData = new PlayerActionData.ActionUserData("inpActVehicleMoveForwardName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.MoveBack = base.CreatePlayerAction("Back");
		this.MoveBack.UserData = new PlayerActionData.ActionUserData("inpActVehicleMoveBackName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.MoveLeft = base.CreatePlayerAction("Left");
		this.MoveLeft.UserData = new PlayerActionData.ActionUserData("inpActVehicleMoveLeftName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.MoveRight = base.CreatePlayerAction("Right");
		this.MoveRight.UserData = new PlayerActionData.ActionUserData("inpActVehicleMoveRightName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Move = base.CreateTwoAxisPlayerAction(this.MoveLeft, this.MoveRight, this.MoveBack, this.MoveForward);
		this.LookLeft = base.CreatePlayerAction("LookLeft");
		this.LookLeft.UserData = new PlayerActionData.ActionUserData("inpActVehicleLookName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.LookRight = base.CreatePlayerAction("LookRight");
		this.LookUp = base.CreatePlayerAction("LookUp");
		this.LookDown = base.CreatePlayerAction("LookDown");
		this.Look = base.CreateTwoAxisPlayerAction(this.LookLeft, this.LookRight, this.LookDown, this.LookUp);
		this.Turbo = base.CreatePlayerAction("Run");
		this.Turbo.UserData = new PlayerActionData.ActionUserData("inpActVehicleTurboName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Brake = base.CreatePlayerAction("Jump");
		this.Brake.UserData = new PlayerActionData.ActionUserData("inpActVehicleBrakeName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Hop = base.CreatePlayerAction("Crouch");
		this.Hop.UserData = new PlayerActionData.ActionUserData("inpActVehicleHopName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Activate = base.CreatePlayerAction("Activate");
		this.Activate.UserData = new PlayerActionData.ActionUserData("inpActActivateName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.Activate.FirstRepeatDelay = 0.3f;
		this.ToggleFlashlight = base.CreatePlayerAction("ToggleFlashlight");
		this.ToggleFlashlight.UserData = new PlayerActionData.ActionUserData("inpActVehicleToggleLightName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.HonkHorn = base.CreatePlayerAction("HonkHorn");
		this.HonkHorn.UserData = new PlayerActionData.ActionUserData("inpActHonkHornName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Menu = base.CreatePlayerAction("Menu");
		this.Menu.UserData = new PlayerActionData.ActionUserData("inpActMenuName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Inventory = base.CreatePlayerAction("Inventory");
		this.Inventory.UserData = new PlayerActionData.ActionUserData("inpActInventoryName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.Scoreboard = base.CreatePlayerAction("Scoreboard");
		this.Scoreboard.UserData = new PlayerActionData.ActionUserData("inpActScoreboardName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, true, false, false, true);
		this.ToggleTurnMode = base.CreatePlayerAction("ToggleTurnMode");
		this.ToggleTurnMode.UserData = new PlayerActionData.ActionUserData("inpActToggleTurnMode", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.ScrollUp = base.CreatePlayerAction("ScrollUp");
		this.ScrollUp.UserData = new PlayerActionData.ActionUserData("inpActCameraZoomInName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.ScrollDown = base.CreatePlayerAction("ScrollDown");
		this.ScrollDown.UserData = new PlayerActionData.ActionUserData("inpActCameraZoomOutName", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.Both, true, false, false, true);
		this.Scroll = base.CreateOneAxisPlayerAction(this.ScrollDown, this.ScrollUp);
		this.LeftStickLeft = base.CreatePlayerAction("LeftStickLeft");
		this.LeftStickLeft.UserData = new PlayerActionData.ActionUserData("inpActVehicleLeftStickLeft", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, false, false, true, true);
		this.LeftStickRight = base.CreatePlayerAction("LeftStickRight");
		this.LeftStickRight.UserData = new PlayerActionData.ActionUserData("inpActVehicleLeftStickRight", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, false, false, true, true);
		this.LeftStickForward = base.CreatePlayerAction("LeftStickForward");
		this.LeftStickForward.UserData = new PlayerActionData.ActionUserData("inpActVehicleLeftStickForward", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, false, false, true, true);
		this.LeftStickBack = base.CreatePlayerAction("LeftStickBack");
		this.LeftStickBack.UserData = new PlayerActionData.ActionUserData("inpActVehicleLeftStickBack", null, PlayerActionData.GroupVehicle, PlayerActionData.EAppliesToInputType.ControllerOnly, false, false, true, true);
		this.LeftStick = base.CreateTwoAxisPlayerAction(this.LeftStickLeft, this.LeftStickRight, this.LeftStickBack, this.LeftStickForward);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultJoystickBindings()
	{
		base.ListenOptions.IncludeControllers = true;
		this.MoveLeft.AddDefaultBinding(3);
		this.MoveRight.AddDefaultBinding(4);
		this.MoveForward.AddDefaultBinding(16);
		this.MoveBack.AddDefaultBinding(15);
		this.LookLeft.AddDefaultBinding(8);
		this.LookRight.AddDefaultBinding(9);
		this.LookUp.AddDefaultBinding(6);
		this.LookDown.AddDefaultBinding(7);
		this.Turbo.AddDefaultBinding(18);
		this.Brake.AddDefaultBinding(21);
		this.Hop.AddDefaultBinding(19);
		this.Activate.AddDefaultBinding(22);
		this.ToggleFlashlight.AddDefaultBinding(13);
		this.HonkHorn.AddDefaultBinding(17);
		this.Menu.AddDefaultBinding(106);
		this.Menu.AddDefaultBinding(104);
		this.Menu.AddDefaultBinding(101);
		this.Inventory.AddDefaultBinding(20);
		this.Scoreboard.AddDefaultBinding(109);
		this.Scoreboard.AddDefaultBinding(258);
		this.Scoreboard.AddDefaultBinding(100);
		this.ToggleTurnMode.AddDefaultBinding(10);
		this.ScrollUp.AddDefaultBinding(11);
		this.ScrollDown.AddDefaultBinding(12);
		this.LeftStickLeft.AddDefaultBinding(3);
		this.LeftStickRight.AddDefaultBinding(4);
		this.LeftStickForward.AddDefaultBinding(1);
		this.LeftStickBack.AddDefaultBinding(2);
		this.ControllerRebindableActions.Clear();
		this.ControllerRebindableActions.Add(this.MoveForward);
		this.ControllerRebindableActions.Add(this.MoveBack);
		this.ControllerRebindableActions.Add(this.Hop);
		this.ControllerRebindableActions.Add(this.Brake);
		this.ControllerRebindableActions.Add(this.Inventory);
		this.ControllerRebindableActions.Add(this.Activate);
		this.ControllerRebindableActions.Add(this.Turbo);
		this.ControllerRebindableActions.Add(this.HonkHorn);
		this.ControllerRebindableActions.Add(this.ToggleFlashlight);
		this.ControllerRebindableActions.Add(this.ToggleTurnMode);
		this.ControllerRebindableActions.Add(this.ScrollUp);
		this.ControllerRebindableActions.Add(this.ScrollDown);
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
		this.Turbo.AddDefaultBinding(new Key[] { 5 });
		this.Brake.AddDefaultBinding(new Key[] { 76 });
		this.Hop.AddDefaultBinding(new Key[] { 38 });
		this.HonkHorn.AddDefaultBinding(new Key[] { 59 });
		this.Menu.AddDefaultBinding(new Key[] { 13 });
		this.ToggleTurnMode.AddDefaultBinding(1);
		this.ScrollUp.AddDefaultBinding(8);
		this.ScrollDown.AddDefaultBinding(9);
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

	public PlayerAction Turbo;

	public PlayerAction Brake;

	public PlayerAction Hop;

	public PlayerAction Activate;

	public PlayerAction ToggleFlashlight;

	public PlayerAction HonkHorn;

	public PlayerAction Menu;

	public PlayerAction Inventory;

	public PlayerAction Scoreboard;

	public PlayerAction ToggleTurnMode;

	public PlayerAction ScrollUp;

	public PlayerAction ScrollDown;

	public PlayerOneAxisAction Scroll;

	public PlayerTwoAxisAction LeftStick;

	public PlayerAction LeftStickLeft;

	public PlayerAction LeftStickRight;

	public PlayerAction LeftStickForward;

	public PlayerAction LeftStickBack;
}
