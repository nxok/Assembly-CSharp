using System;
using InControl;

public class PlayerActionsPermanent : PlayerActionsBase
{
	public PlayerActionsPermanent()
	{
		base.Name = "permanent";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateActions()
	{
		this.Reload = base.CreatePlayerAction("Reload");
		this.Reload.UserData = new PlayerActionData.ActionUserData("inpActReloadTakeAllName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.Reload.FirstRepeatDelay = 0.3f;
		this.Activate = base.CreatePlayerAction("Activate");
		this.Activate.UserData = new PlayerActionData.ActionUserData("inpActActivateName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.ToggleFlashlight = base.CreatePlayerAction("ToggleFlashlight");
		this.ToggleFlashlight.UserData = new PlayerActionData.ActionUserData("inpActPlayerToggleFlashlightName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.ToggleFlashlight.FirstRepeatDelay = 0.3f;
		this.Inventory = base.CreatePlayerAction("Inventory");
		this.Inventory.UserData = new PlayerActionData.ActionUserData("inpActInventoryName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.Both, true, false, false, false);
		this.Character = base.CreatePlayerAction("Character");
		this.Character.UserData = new PlayerActionData.ActionUserData("inpActCharacterName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.Both, true, false, false, false);
		this.Map = base.CreatePlayerAction("Map");
		this.Map.UserData = new PlayerActionData.ActionUserData("inpActMapName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.Both, true, false, false, false);
		this.Skills = base.CreatePlayerAction("Skills");
		this.Skills.UserData = new PlayerActionData.ActionUserData("inpActSkillsName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.Both, true, false, false, false);
		this.Quests = base.CreatePlayerAction("Quests");
		this.Quests.UserData = new PlayerActionData.ActionUserData("inpActQuestsName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.Both, true, false, false, false);
		this.Challenges = base.CreatePlayerAction("Challenges");
		this.Challenges.UserData = new PlayerActionData.ActionUserData("inpActChallengesName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.Both, true, false, false, false);
		this.Scoreboard = base.CreatePlayerAction("Scoreboard");
		this.Scoreboard.UserData = new PlayerActionData.ActionUserData("inpActScoreboardName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.Both, true, false, false, false);
		this.Creative = base.CreatePlayerAction("Creative");
		this.Creative.UserData = new PlayerActionData.ActionUserData("inpActCreativeName", null, PlayerActionData.GroupDialogs, PlayerActionData.EAppliesToInputType.Both, true, false, false, false);
		this.DebugControllerLeft = base.CreatePlayerAction("DebugControllerLeft");
		this.DebugControllerLeft.UserData = new PlayerActionData.ActionUserData("inpActDebugControllerLeftName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.ControllerOnly, false, true, true, true);
		this.DebugControllerRight = base.CreatePlayerAction("DebugControllerRight");
		this.DebugControllerRight.UserData = new PlayerActionData.ActionUserData("inpActDebugControllerRightName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.ControllerOnly, false, true, true, true);
		this.Chat = base.CreatePlayerAction("Chat");
		this.Chat.UserData = new PlayerActionData.ActionUserData("inpActChatName", null, PlayerActionData.GroupMp, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.PushToTalk = base.CreatePlayerAction("PushToTalk");
		this.PushToTalk.UserData = new PlayerActionData.ActionUserData("inpActPushToTalkName", null, PlayerActionData.GroupMp, PlayerActionData.EAppliesToInputType.Both, true, false, false, false);
		this.Cancel = base.CreatePlayerAction("Cancel");
		this.Cancel.UserData = new PlayerActionData.ActionUserData("inpActCancelName", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.Both, false, true, true, true);
		this.Swap = base.CreatePlayerAction("Swap");
		this.Swap.UserData = new PlayerActionData.ActionUserData("inpActSwapName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.PageTipsForward = base.CreatePlayerAction("PageTipsForward");
		this.PageTipsForward.UserData = new PlayerActionData.ActionUserData("inpActPageTipsForward", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.Both, false, true, true, true);
		this.PageTipsBack = base.CreatePlayerAction("PageTipsBack");
		this.PageTipsBack.UserData = new PlayerActionData.ActionUserData("inpActPageTipsBack", null, PlayerActionData.GroupAdmin, PlayerActionData.EAppliesToInputType.Both, false, true, true, true);
		this.QuickMenu = base.CreatePlayerAction("QuickMenu");
		this.QuickMenu.UserData = new PlayerActionData.ActionUserData("inpQuickMenuName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.KbdMouseOnly, true, false, false, true);
		this.CameraZoomIn = base.CreatePlayerAction("ScrollUp");
		this.CameraZoomIn.UserData = new PlayerActionData.ActionUserData("inpActScopeZoomInName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, false, true, true, true);
		this.CameraZoomOut = base.CreatePlayerAction("ScrollDown");
		this.CameraZoomOut.UserData = new PlayerActionData.ActionUserData("inpActScopeZoomOutName", null, PlayerActionData.GroupPlayerControl, PlayerActionData.EAppliesToInputType.Both, false, true, true, true);
		this.CameraZoom = base.CreateOneAxisPlayerAction(this.CameraZoomOut, this.CameraZoomIn);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultJoystickBindings()
	{
		base.ListenOptions.IncludeControllers = true;
		this.DebugControllerLeft.AddDefaultBinding(17);
		this.DebugControllerRight.AddDefaultBinding(18);
		this.Cancel.AddDefaultBinding(20);
		this.PageTipsForward.AddDefaultBinding(16);
		this.PageTipsBack.AddDefaultBinding(15);
		this.PushToTalk.AddDefaultBinding(14);
		this.CameraZoomIn.AddDefaultBinding(16);
		this.CameraZoomOut.AddDefaultBinding(15);
		this.ControllerRebindableActions.Clear();
		this.ControllerRebindableActions.Add(this.PushToTalk);
		this.ControllerRebindableActions.Add(this.Character);
		this.ControllerRebindableActions.Add(this.Map);
		this.ControllerRebindableActions.Add(this.Skills);
		this.ControllerRebindableActions.Add(this.Quests);
		this.ControllerRebindableActions.Add(this.Challenges);
		this.ControllerRebindableActions.Add(this.Scoreboard);
		this.ControllerRebindableActions.Add(this.Creative);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateDefaultKeyboardBindings()
	{
		base.ListenOptions.IncludeKeys = true;
		base.ListenOptions.IncludeMouseButtons = true;
		base.ListenOptions.IncludeMouseScrollWheel = true;
		this.Reload.AddDefaultBinding(new Key[] { 53 });
		this.Activate.AddDefaultBinding(new Key[] { 40 });
		this.ToggleFlashlight.AddDefaultBinding(new Key[] { 41 });
		this.Inventory.AddDefaultBinding(new Key[] { 66 });
		this.Skills.AddDefaultBinding(new Key[] { 49 });
		this.Quests.AddDefaultBinding(new Key[] { 50 });
		this.Challenges.AddDefaultBinding(new Key[] { 60 });
		this.Character.AddDefaultBinding(new Key[] { 37 });
		this.Map.AddDefaultBinding(new Key[] { 48 });
		this.Creative.AddDefaultBinding(new Key[] { 56 });
		this.Scoreboard.AddDefaultBinding(new Key[] { 44 });
		this.Chat.AddDefaultBinding(new Key[] { 55 });
		this.PushToTalk.AddDefaultBinding(new Key[] { 57 });
		this.Cancel.AddDefaultBinding(new Key[] { 13 });
		this.Swap.AddDefaultBinding(10);
		this.PageTipsForward.AddDefaultBinding(1);
		this.PageTipsBack.AddDefaultBinding(2);
		this.QuickMenu.AddDefaultBinding(new Key[] { 62 });
		this.CameraZoomIn.AddDefaultBinding(8);
		this.CameraZoomOut.AddDefaultBinding(9);
	}

	public PlayerAction Reload;

	public PlayerAction Activate;

	public PlayerAction ToggleFlashlight;

	public PlayerAction Inventory;

	public PlayerAction Skills;

	public PlayerAction Quests;

	public PlayerAction Challenges;

	public PlayerAction Character;

	public PlayerAction Map;

	public PlayerAction Creative;

	public PlayerAction Scoreboard;

	public PlayerAction DebugControllerLeft;

	public PlayerAction DebugControllerRight;

	public PlayerAction Chat;

	public PlayerAction PushToTalk;

	public PlayerAction Cancel;

	public PlayerAction Swap;

	public PlayerAction PageTipsForward;

	public PlayerAction PageTipsBack;

	public PlayerAction QuickMenu;

	public PlayerAction CameraZoomIn;

	public PlayerAction CameraZoomOut;

	public PlayerOneAxisAction CameraZoom;
}
