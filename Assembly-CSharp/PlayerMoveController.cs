using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Audio;
using InControl;
using Platform;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
	public bool RunToggleActive
	{
		get
		{
			return this.runToggleActive;
		}
	}

	public PlayerActionsLocal playerInput
	{
		get
		{
			return PlatformManager.NativePlatform.Input.PrimaryPlayer;
		}
	}

	public void Init()
	{
		PlayerMoveController.Instance = this;
		this.entityPlayerLocal = base.GetComponent<EntityPlayerLocal>();
		this.playerUI = LocalPlayerUI.GetUIForPlayer(this.entityPlayerLocal);
		this.windowManager = this.playerUI.windowManager;
		this.nguiWindowManager = this.playerUI.nguiWindowManager;
		this.gameManager = Object.FindAnyObjectByType<GameManager>();
		this.guiInGame = this.nguiWindowManager.InGameHUD;
		this.nguiWindowManager.Show(EnumNGUIWindow.InGameHUD, true);
		PlayerMoveController.UpdateControlsOptions();
		GamePrefs.Set(EnumGamePrefs.DebugMenuShowTasks, false);
		this.focusBoxScript = new RenderDisplacedCube((this.guiInGame.FocusCube != null) ? Object.Instantiate<Transform>(this.guiInGame.FocusCube) : null);
		this.playerAutoPilotControllor = new PlayerAutoPilotControllor(this.gameManager);
		this.toggleGodMode = delegate
		{
			this.entityPlayerLocal.bEntityAliveFlagsChanged = true;
			this.entityPlayerLocal.IsGodMode.Value = !this.entityPlayerLocal.IsGodMode.Value;
			this.entityPlayerLocal.IsNoCollisionMode.Value = this.entityPlayerLocal.IsGodMode.Value;
			this.entityPlayerLocal.IsFlyMode.Value = this.entityPlayerLocal.IsGodMode.Value;
			if (this.entityPlayerLocal.IsGodMode.Value)
			{
				this.entityPlayerLocal.Buffs.AddBuff("god", -1, true, false, -1f);
				return;
			}
			if (!GameManager.Instance.World.IsEditor() && !GameModeCreative.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode)))
			{
				this.entityPlayerLocal.Buffs.RemoveBuff("god", -1, true);
			}
		};
		this.teleportPlayer = delegate
		{
			Ray lookRay = this.entityPlayerLocal.GetLookRay();
			if (InputUtils.ControlKeyPressed)
			{
				lookRay.direction *= -1f;
			}
			lookRay.origin -= Origin.position;
			RaycastHit raycastHit;
			Vector3 vector;
			if (Physics.SphereCast(lookRay, 0.3f, ref raycastHit, 500f, 1342242816))
			{
				vector = raycastHit.point - lookRay.direction.normalized * 0.5f;
			}
			else
			{
				vector = lookRay.origin + lookRay.direction.normalized * 100f;
			}
			this.entityPlayerLocal.SetPosition(vector + Origin.position, true);
			GameEventManager.Current.HandleForceBossDespawn(this.entityPlayerLocal);
		};
		NGuiAction nguiAction = new NGuiAction("SelectionMode", null, null, true, this.playerInput.Drop);
		nguiAction.SetClickActionDelegate(delegate
		{
			if (InputUtils.AltKeyPressed)
			{
				GamePrefs.Set(EnumGamePrefs.SelectionOperationMode, 4);
				return;
			}
			if (InputUtils.ShiftKeyPressed)
			{
				GamePrefs.Set(EnumGamePrefs.SelectionOperationMode, 2);
				GameManager.Instance.SetCursorEnabledOverride(true, true);
				return;
			}
			if (InputUtils.ControlKeyPressed && GameManager.Instance.IsEditMode())
			{
				GamePrefs.Set(EnumGamePrefs.SelectionOperationMode, 3);
				GameManager.Instance.SetCursorEnabledOverride(true, true);
				return;
			}
			GamePrefs.Set(EnumGamePrefs.SelectionOperationMode, 1);
			GameManager.Instance.SetCursorEnabledOverride(true, true);
		});
		nguiAction.SetReleaseActionDelegate(delegate
		{
			GamePrefs.Set(EnumGamePrefs.SelectionOperationMode, 0);
			GameManager.Instance.SetCursorEnabledOverride(false, false);
		});
		nguiAction.SetIsEnabledDelegate(() => (this.gameManager.gameStateManager.IsGameStarted() && GameStats.GetInt(EnumGameStats.GameState) == 1 && GameManager.Instance.World.IsEditor()) || BlockToolSelection.Instance.SelectionActive);
		this.globalActions.Add(nguiAction);
		NGuiAction.IsEnabledDelegate menuIsEnabled = () => !XUiC_SpawnSelectionWindow.IsOpenInUI(LocalPlayerUI.primaryUI) && this.gameManager.gameStateManager.IsGameStarted() && GameStats.GetInt(EnumGameStats.GameState) == 1 && !LocalPlayerUI.primaryUI.windowManager.IsModalWindowOpen() && !this.windowManager.IsFullHUDDisabled();
		NGuiAction.OnClickActionDelegate onClickActionDelegate = delegate
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && GameManager.Instance.isAnyCursorWindowOpen(null))
			{
				return;
			}
			this.entityPlayerLocal.AimingGun = false;
			if (this.windowManager.IsWindowOpen("windowpaging") || this.windowManager.IsModalWindowOpen())
			{
				this.windowManager.CloseAllOpenModalWindows(null, false);
				this.windowManager.Close("windowpaging");
				return;
			}
			this.windowManager.CloseAllOpenModalWindows(null, false);
			this.playerUI.xui.RadialWindow.Open();
			this.playerUI.xui.RadialWindow.SetupMenuData();
		};
		NGuiAction.IsCheckedDelegate isCheckedDelegate = () => this.windowManager.IsWindowOpen("windowpaging");
		NGuiAction.IsEnabledDelegate isEnabledDelegate = () => menuIsEnabled() && !this.windowManager.IsWindowOpen(this.playerUI.xui.RadialWindow.WindowGroup);
		NGuiAction nguiAction2 = new NGuiAction("Inventory", null, null, true, this.playerInput.Inventory);
		nguiAction2.SetClickActionDelegate(onClickActionDelegate);
		nguiAction2.SetIsEnabledDelegate(isEnabledDelegate);
		nguiAction2.SetIsCheckedDelegate(isCheckedDelegate);
		this.globalActions.Add(nguiAction2);
		NGuiAction nguiAction3 = new NGuiAction("Inventory", null, null, true, this.playerInput.PermanentActions.Inventory);
		nguiAction3.SetClickActionDelegate(onClickActionDelegate);
		nguiAction3.SetIsEnabledDelegate(isEnabledDelegate);
		nguiAction3.SetIsCheckedDelegate(isCheckedDelegate);
		this.globalActions.Add(nguiAction3);
		NGuiAction nguiAction4 = new NGuiAction("Inventory", null, null, true, this.playerInput.VehicleActions.Inventory);
		nguiAction4.SetClickActionDelegate(onClickActionDelegate);
		nguiAction4.SetIsEnabledDelegate(isEnabledDelegate);
		nguiAction4.SetIsCheckedDelegate(isCheckedDelegate);
		this.globalActions.Add(nguiAction4);
		NGuiAction nguiAction5 = new NGuiAction("Creative", null, null, true, this.playerInput.PermanentActions.Creative);
		nguiAction5.SetClickActionDelegate(delegate
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && GameManager.Instance.isAnyCursorWindowOpen(null))
			{
				return;
			}
			this.entityPlayerLocal.AimingGun = false;
			XUiC_WindowSelector.OpenSelectorAndWindow(this.entityPlayerLocal, "creative");
		});
		nguiAction5.SetIsEnabledDelegate(() => menuIsEnabled() && (GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled)));
		nguiAction5.SetIsCheckedDelegate(() => this.windowManager.IsWindowOpen("creative"));
		this.globalActions.Add(nguiAction5);
		NGuiAction nguiAction6 = new NGuiAction("Map", null, null, true, this.playerInput.PermanentActions.Map);
		nguiAction6.SetClickActionDelegate(delegate
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && GameManager.Instance.isAnyCursorWindowOpen(null))
			{
				return;
			}
			if (!World.MapEnabled)
			{
				return;
			}
			this.entityPlayerLocal.AimingGun = false;
			XUiC_WindowSelector.OpenSelectorAndWindow(this.entityPlayerLocal, "map");
		});
		nguiAction6.SetIsEnabledDelegate(menuIsEnabled);
		nguiAction6.SetIsCheckedDelegate(() => this.windowManager.IsWindowOpen("map"));
		this.globalActions.Add(nguiAction6);
		NGuiAction nguiAction7 = new NGuiAction("Character", null, null, true, this.playerInput.PermanentActions.Character);
		nguiAction7.SetClickActionDelegate(delegate
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && GameManager.Instance.isAnyCursorWindowOpen(null))
			{
				return;
			}
			this.entityPlayerLocal.AimingGun = false;
			XUiC_WindowSelector.OpenSelectorAndWindow(this.entityPlayerLocal, "character");
		});
		nguiAction7.SetIsEnabledDelegate(menuIsEnabled);
		this.globalActions.Add(nguiAction7);
		NGuiAction nguiAction8 = new NGuiAction("Skills", null, null, true, this.playerInput.PermanentActions.Skills);
		nguiAction8.SetClickActionDelegate(delegate
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && GameManager.Instance.isAnyCursorWindowOpen(null))
			{
				return;
			}
			this.entityPlayerLocal.AimingGun = false;
			XUiC_WindowSelector.OpenSelectorAndWindow(this.entityPlayerLocal, "skills");
		});
		nguiAction8.SetIsEnabledDelegate(menuIsEnabled);
		this.globalActions.Add(nguiAction8);
		NGuiAction nguiAction9 = new NGuiAction("Quests", null, null, true, this.playerInput.PermanentActions.Quests);
		nguiAction9.SetClickActionDelegate(delegate
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && GameManager.Instance.isAnyCursorWindowOpen(null))
			{
				return;
			}
			this.entityPlayerLocal.AimingGun = false;
			XUiC_WindowSelector.OpenSelectorAndWindow(this.entityPlayerLocal, "quests");
		});
		nguiAction9.SetIsEnabledDelegate(menuIsEnabled);
		this.globalActions.Add(nguiAction9);
		NGuiAction.OnClickActionDelegate onClickActionDelegate2 = delegate
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && GameManager.Instance.isAnyCursorWindowOpen(null))
			{
				return;
			}
			this.entityPlayerLocal.AimingGun = false;
			XUiC_WindowSelector.OpenSelectorAndWindow(this.entityPlayerLocal, "players");
		};
		NGuiAction nguiAction10 = new NGuiAction("Players", null, null, true, this.playerInput.Scoreboard);
		nguiAction10.SetClickActionDelegate(onClickActionDelegate2);
		nguiAction10.SetIsEnabledDelegate(menuIsEnabled);
		this.globalActions.Add(nguiAction10);
		NGuiAction nguiAction11 = new NGuiAction("Players", null, null, true, this.playerInput.VehicleActions.Scoreboard);
		nguiAction11.SetClickActionDelegate(onClickActionDelegate2);
		nguiAction11.SetIsEnabledDelegate(menuIsEnabled);
		this.globalActions.Add(nguiAction11);
		NGuiAction nguiAction12 = new NGuiAction("Players", null, null, true, this.playerInput.PermanentActions.Scoreboard);
		nguiAction12.SetClickActionDelegate(onClickActionDelegate2);
		nguiAction12.SetIsEnabledDelegate(menuIsEnabled);
		this.globalActions.Add(nguiAction12);
		NGuiAction nguiAction13 = new NGuiAction("Challenges", null, null, true, this.playerInput.PermanentActions.Challenges);
		nguiAction13.SetClickActionDelegate(delegate
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && GameManager.Instance.isAnyCursorWindowOpen(null))
			{
				return;
			}
			if (!ChallengeJournal.AllowChallenges && !World.BiomeProgressionEnabled)
			{
				return;
			}
			this.entityPlayerLocal.AimingGun = false;
			XUiC_WindowSelector.OpenSelectorAndWindow(this.entityPlayerLocal, "challenges");
		});
		nguiAction13.SetIsEnabledDelegate(menuIsEnabled);
		this.globalActions.Add(nguiAction13);
		NGuiAction nguiAction14 = new NGuiAction("Chat", null, null, true, this.playerInput.PermanentActions.Chat);
		nguiAction14.SetClickActionDelegate(delegate
		{
			this.entityPlayerLocal.AimingGun = false;
			this.windowManager.Open(XUiC_Chat.ID, true);
		});
		nguiAction14.SetIsEnabledDelegate(menuIsEnabled);
		nguiAction14.SetIsCheckedDelegate(() => this.windowManager.IsWindowOpen(XUiC_Chat.ID));
		this.globalActions.Add(nguiAction14);
		NGuiAction nguiAction15 = new NGuiAction("Prefab", null, null, true, this.playerInput.Prefab);
		nguiAction15.SetClickActionDelegate(delegate
		{
			this.entityPlayerLocal.AimingGun = false;
			SelectionBoxManager.Instance.OpenPropertiesWindow(this.windowManager);
		});
		nguiAction15.SetIsEnabledDelegate(() => menuIsEnabled() && this.gameManager.IsEditMode());
		this.globalActions.Add(nguiAction15);
		NGuiAction nguiAction16 = new NGuiAction("DetachCamera", null, null, false, this.playerInput.DetachCamera);
		nguiAction16.SetClickActionDelegate(delegate
		{
			Manager.PlayButtonClick();
			if (this.entityPlayerLocal.bFirstPersonView)
			{
				this.entityPlayerLocal.SetCameraAttachedToPlayer(!this.entityPlayerLocal.IsCameraAttachedToPlayerOrScope(), true);
				this.entityPlayerLocal.Buffs.SetBuff("buffShowDetachCameraEnabled", !this.entityPlayerLocal.IsCameraAttachedToPlayerOrScope());
				return;
			}
			this.entityPlayerLocal.SetCameraAttachedToPlayer(false, !this.entityPlayerLocal.vp_FPCamera.Locked3rdPerson);
			this.entityPlayerLocal.Buffs.SetBuff("buffShowDetachCameraEnabled", this.entityPlayerLocal.vp_FPCamera.Locked3rdPerson);
		});
		nguiAction16.SetIsEnabledDelegate(() => this.gameManager.gameStateManager.IsGameStarted() && GameStats.GetInt(EnumGameStats.GameState) == 1 && !this.entityPlayerLocal.AimingGun && (this.gameManager.IsEditMode() || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled)) && !InputUtils.ControlKeyPressed);
		this.globalActions.Add(nguiAction16);
		NGuiAction nguiAction17 = new NGuiAction("ToggleDCMove", null, null, false, this.playerInput.ToggleDCMove);
		nguiAction17.SetClickActionDelegate(delegate
		{
			Manager.PlayButtonClick();
			this.entityPlayerLocal.movementInput.bDetachedCameraMove = !this.entityPlayerLocal.movementInput.bDetachedCameraMove && !this.entityPlayerLocal.IsCameraAttachedToPlayerOrScope();
			this.entityPlayerLocal.Buffs.SetBuff("buffShowToggleDCEnabled", this.entityPlayerLocal.movementInput.bDetachedCameraMove);
		});
		nguiAction17.SetIsEnabledDelegate(() => this.gameManager.gameStateManager.IsGameStarted() && GameStats.GetInt(EnumGameStats.GameState) == 1 && !this.entityPlayerLocal.AimingGun && (this.gameManager.IsEditMode() || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled)));
		this.globalActions.Add(nguiAction17);
		NGuiAction nguiAction18 = new NGuiAction("LockCamera", null, null, false, this.playerInput.LockFreeCamera);
		nguiAction18.SetClickActionDelegate(delegate
		{
			Manager.PlayButtonClick();
			this.entityPlayerLocal.movementInput.bCameraPositionLocked = !this.entityPlayerLocal.movementInput.bCameraPositionLocked;
		});
		nguiAction18.SetIsEnabledDelegate(() => this.gameManager.gameStateManager.IsGameStarted() && GameStats.GetInt(EnumGameStats.GameState) == 1 && !this.entityPlayerLocal.AimingGun && (this.gameManager.IsEditMode() || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled)));
		this.globalActions.Add(nguiAction18);
		NGuiAction.OnClickActionDelegate onClickActionDelegate3 = delegate
		{
			if (!XUiC_SpawnSelectionWindow.IsOpenInUI(LocalPlayerUI.primaryUI))
			{
				Manager.PlayButtonClick();
				if (!this.windowManager.CloseAllOpenModalWindows(null, false))
				{
					this.entityPlayerLocal.PlayerUI.CursorController.HoverTarget = null;
					this.windowManager.SwitchVisible(XUiC_InGameMenuWindow.ID, true);
				}
			}
		};
		NGuiAction nguiAction19 = new NGuiAction("Menu", null, null, false, this.playerInput.Menu);
		nguiAction19.SetClickActionDelegate(onClickActionDelegate3);
		nguiAction19.SetIsEnabledDelegate(() => !this.windowManager.IsFullHUDDisabled());
		this.globalActions.Add(nguiAction19);
		NGuiAction nguiAction20 = new NGuiAction("Menu", null, null, false, this.playerInput.VehicleActions.Menu);
		nguiAction20.SetClickActionDelegate(onClickActionDelegate3);
		nguiAction20.SetIsEnabledDelegate(() => !this.windowManager.IsFullHUDDisabled());
		this.globalActions.Add(nguiAction20);
		NGuiAction nguiAction21 = new NGuiAction("Fly Mode", null, null, true, this.playerInput.Fly);
		nguiAction21.SetClickActionDelegate(delegate
		{
			Manager.PlayButtonClick();
			this.entityPlayerLocal.IsFlyMode.Value = !this.entityPlayerLocal.IsFlyMode.Value;
		});
		nguiAction21.SetIsCheckedDelegate(() => this.entityPlayerLocal != null && this.entityPlayerLocal.IsFlyMode.Value);
		nguiAction21.SetIsEnabledDelegate(() => GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled) || GameStats.GetBool(EnumGameStats.IsFlyingEnabled));
		this.globalActions.Add(nguiAction21);
		NGuiAction nguiAction22 = new NGuiAction("God Mode", null, null, true, this.playerInput.God);
		nguiAction22.SetClickActionDelegate(delegate
		{
			Manager.PlayButtonClick();
			if (InputUtils.ShiftKeyPressed)
			{
				this.teleportPlayer();
				this.isAutorunInvalid = true;
				return;
			}
			this.toggleGodMode();
		});
		nguiAction22.SetIsCheckedDelegate(() => this.entityPlayerLocal != null && this.entityPlayerLocal.IsGodMode.Value);
		nguiAction22.SetIsEnabledDelegate(() => GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) || !GameStats.GetBool(EnumGameStats.IsPlayerDamageEnabled));
		this.globalActions.Add(nguiAction22);
		NGuiAction nguiAction23 = new NGuiAction("No Collision", null, null, true, null);
		nguiAction23.SetClickActionDelegate(delegate
		{
			Manager.PlayButtonClick();
			this.entityPlayerLocal.IsNoCollisionMode.Value = !this.entityPlayerLocal.IsNoCollisionMode.Value;
		});
		nguiAction23.SetIsCheckedDelegate(() => this.entityPlayerLocal != null && this.entityPlayerLocal.IsNoCollisionMode.Value);
		nguiAction23.SetIsEnabledDelegate(() => GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) || !GameStats.GetBool(EnumGameStats.IsPlayerCollisionEnabled));
		this.globalActions.Add(nguiAction23);
		NGuiAction nguiAction24 = new NGuiAction("Invisible", null, null, true, this.playerInput.Invisible);
		nguiAction24.SetClickActionDelegate(delegate
		{
			Manager.PlayButtonClick();
			this.entityPlayerLocal.IsSpectator = !this.entityPlayerLocal.IsSpectator;
		});
		nguiAction24.SetIsCheckedDelegate(() => this.entityPlayerLocal != null && this.entityPlayerLocal.IsSpectator);
		nguiAction24.SetIsEnabledDelegate(() => GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) || !GameStats.GetBool(EnumGameStats.IsPlayerDamageEnabled));
		this.globalActions.Add(nguiAction24);
		for (int i = 0; i < this.globalActions.Count; i++)
		{
			this.windowManager.AddGlobalAction(this.globalActions[i]);
		}
		EAIManager.isAnimFreeze = false;
	}

	public static void UpdateControlsOptions()
	{
		if (PlayerMoveController.Instance != null)
		{
			PlayerMoveController.Instance.invertMouse = (GamePrefs.GetBool(EnumGamePrefs.OptionsInvertMouse) ? (-1) : 1);
			PlayerMoveController.Instance.invertController = (GamePrefs.GetBool(EnumGamePrefs.OptionsControllerLookInvert) ? (-1) : 1);
			PlayerMoveController.Instance.bControllerVibration = GamePrefs.GetBool(EnumGamePrefs.OptionsControllerVibration);
			PlayerMoveController.Instance.UpdateLookSensitivity(GamePrefs.GetFloat(EnumGamePrefs.OptionsLookSensitivity), GamePrefs.GetFloat(EnumGamePrefs.OptionsZoomSensitivity), GamePrefs.GetFloat(EnumGamePrefs.OptionsZoomAccel), GamePrefs.GetFloat(EnumGamePrefs.OptionsVehicleLookSensitivity), new Vector2(GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerSensitivityX), GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerSensitivityY)));
			PlayerMoveController.Instance.lookAccelerationRate = GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerLookAcceleration) * 0.5f;
			PlayerMoveController.Instance.controllerZoomSensitivity = GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerZoomSensitivity);
			PlayerMoveController.Instance.controllerVehicleSensitivity = GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerVehicleSensitivity);
			PlayerMoveController.Instance.controllerAimAssistsEnabled = GamePrefs.GetBool(EnumGamePrefs.OptionsControllerAimAssists);
			PlayerMoveController.Instance.playerInput.SetJoyStickLayout((eControllerJoystickLayout)GamePrefs.GetInt(EnumGamePrefs.OptionsControllerJoystickLayout));
			PlayerMoveController.Instance.sprintMode = GamePrefs.GetInt(EnumGamePrefs.OptionsControlsSprintLock);
			PlayerMoveController.SetDeadzones();
		}
	}

	public static void SetDeadzones()
	{
		PlayerMoveController.Instance.playerInput.SetDeadzones(GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerLookAxisDeadzone), GamePrefs.GetFloat(EnumGamePrefs.OptionsControllerMoveAxisDeadzone));
	}

	public void UpdateInvertMouse(bool _invertMouse)
	{
		this.invertMouse = (_invertMouse ? (-1) : 1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLookSensitivity(float _sensitivity, float _zoomSensitivity, float _zoomAccel, float _vehicleSensitivity, Vector2 _controllerLookSensitivity)
	{
		this.mouseLookSensitivity = Vector3.one * _sensitivity * 5f;
		this.mouseZoomSensitivity = _zoomSensitivity;
		this.zoomAccel = _zoomAccel * 0.5f;
		this.vehicleLookSensitivity = _vehicleSensitivity * 5f;
		this.controllerLookSensitivity = _controllerLookSensitivity * 10f;
	}

	public Vector2 GetCameraInputSensitivity()
	{
		if (this.playerInput.LastInputType == 1)
		{
			return this.controllerLookSensitivity;
		}
		return this.mouseLookSensitivity;
	}

	public bool GetControllerVibration()
	{
		return this.bControllerVibration;
	}

	public void UpdateControllerVibration(bool _controllerVibration)
	{
		this.bControllerVibration = _controllerVibration;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnDestroy()
	{
		for (int i = 0; i < this.globalActions.Count; i++)
		{
			this.windowManager.RemoveGlobalAction(this.globalActions[i]);
		}
		this.focusBoxScript.Cleanup();
		PlayerMoveController.Instance = null;
		this.playerUI = null;
		this.windowManager = null;
		this.nguiWindowManager = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnGUI()
	{
		if (!this.gameManager.gameStateManager.IsGameStarted() || GameStats.GetInt(EnumGameStats.GameState) != 1)
		{
			return;
		}
		if (this.windowManager.IsFullHUDDisabled())
		{
			return;
		}
		if (this.entityPlayerLocal.inventory != null && this.gameManager.World.worldTime % 2UL == 0UL)
		{
			ItemValue holdingItemItemValue = this.entityPlayerLocal.inventory.holdingItemItemValue;
			ItemClass forId = ItemClass.GetForId(holdingItemItemValue.type);
			int maxUseTimes = holdingItemItemValue.MaxUseTimes;
			if (maxUseTimes > 0 && forId.MaxUseTimesBreaksAfter.Value && holdingItemItemValue.UseTimes >= (float)maxUseTimes)
			{
				this.entityPlayerLocal.inventory.DecHoldingItem(1);
				if (forId.Properties.Values.ContainsKey(ItemClass.PropSoundDestroy))
				{
					Manager.BroadcastPlay(this.entityPlayerLocal, forId.Properties.Values[ItemClass.PropSoundDestroy], false, 1f);
				}
			}
			this.entityPlayerLocal.equipment.CheckBreakUseItems();
		}
		if (!this.windowManager.IsInputActive() && !this.windowManager.IsModalWindowOpen() && Event.current.rawType == 4 && this.gameManager.IsEditMode() && this.entityPlayerLocal.inventory != null)
		{
			this.gameManager.GetActiveBlockTool().CheckSpecialKeys(Event.current, this.playerInput);
			if (XUiC_WoPropsPOIMarker.Instance != null)
			{
				XUiC_WoPropsPOIMarker.Instance.CheckSpecialKeys(Event.current, this.playerInput);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		if (this.entityPlayerLocal.inventory.GetFocusedItemIdx() < 0 || this.entityPlayerLocal.inventory.GetFocusedItemIdx() >= this.entityPlayerLocal.inventory.PUBLIC_SLOTS)
		{
			this.entityPlayerLocal.inventory.SetFocusedItemIdx(0);
			this.entityPlayerLocal.inventory.SetHoldingItemIdx(0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateRespawn()
	{
		if (this.entityPlayerLocal.Spawned)
		{
			this.respawnStateHistory.Add(RespawnProgress.Done);
			return;
		}
		if (GameManager.IsVideoPlaying())
		{
			this.respawnStateHistory.Add(RespawnProgress.WaitingForVideoToPlay);
			return;
		}
		if (!this.bLastRespawnActive)
		{
			this.spawnWindowOpened = false;
			this.spawnPosition = SpawnPosition.Undef;
			this.entityPlayerLocal.BeforePlayerRespawn(this.respawnReason);
			this.bLastRespawnActive = true;
			this.waitingForSpawnPointSelection = false;
		}
		this.entityPlayerLocal.ResetLastTickPos(this.entityPlayerLocal.GetPosition());
		this.respawnTime -= Time.deltaTime;
		if (this.respawnTime > 0f)
		{
			this.respawnStateHistory.Add(RespawnProgress.WaitingForRespawnTime);
			return;
		}
		this.respawnTime = 0f;
		if (this.spawnWindowOpened && XUiC_SpawnSelectionWindow.IsOpenInUI(LocalPlayerUI.primaryUI))
		{
			if (Mathf.Abs(this.entityPlayerLocal.GetPosition().y - Constants.cStartPositionPlayerInLevel.y) < 0.01f)
			{
				Vector3 position = this.entityPlayerLocal.GetPosition();
				Vector3i blockPosition = this.entityPlayerLocal.GetBlockPosition();
				if (this.gameManager.World.GetChunkFromWorldPos(blockPosition) != null)
				{
					float num = (float)(this.gameManager.World.GetHeight(blockPosition.x, blockPosition.z) + 1);
					if (position.y < 0f || num < position.y || (num > position.y && num - 2.5f < position.y))
					{
						this.entityPlayerLocal.SetPosition(new Vector3(this.entityPlayerLocal.GetPosition().x, num, this.entityPlayerLocal.GetPosition().z), true);
					}
				}
			}
			if (this.playerAutoPilotControllor != null && this.playerAutoPilotControllor.IsEnabled())
			{
				XUiC_SpawnSelectionWindow.Close(LocalPlayerUI.primaryUI);
			}
			this.respawnStateHistory.Add(RespawnProgress.WaitingForSpawnWindowToClose);
			return;
		}
		bool flag = this.respawnReason == RespawnType.NewGame || this.respawnReason == RespawnType.EnterMultiplayer || this.respawnReason == RespawnType.JoinMultiplayer || this.respawnReason == RespawnType.LoadedGame;
		Entity entity = (this.entityPlayerLocal.AttachedToEntity ? this.entityPlayerLocal.AttachedToEntity : this.entityPlayerLocal);
		switch (this.respawnReason)
		{
		case RespawnType.NewGame:
			if (!this.spawnWindowOpened)
			{
				this.openSpawnWindow(this.respawnReason);
				this.respawnStateHistory.Add(RespawnProgress.WaitingForSpawnWindowToOpen);
				return;
			}
			this.spawnPosition = new SpawnPosition(this.entityPlayerLocal.GetPosition(), this.entityPlayerLocal.rotation.y);
			goto IL_0615;
		case RespawnType.LoadedGame:
			if (!this.spawnWindowOpened)
			{
				this.spawnPosition = new SpawnPosition(this.entityPlayerLocal.GetPosition(), this.entityPlayerLocal.rotation.y);
				this.entityPlayerLocal.SetPosition(this.spawnPosition.position, true);
				this.openSpawnWindow(this.respawnReason);
				this.respawnStateHistory.Add(RespawnProgress.WaitingForSpawnWindowToOpen);
				return;
			}
			this.spawnPosition = new SpawnPosition(this.entityPlayerLocal.GetPosition(), this.entityPlayerLocal.rotation.y);
			goto IL_0615;
		case RespawnType.Teleport:
			this.entityPlayerLocal.UpdateRespawn();
			this.spawnPosition = new SpawnPosition(entity.GetPosition(), entity.rotation.y);
			this.spawnPosition.position.y = -1f;
			goto IL_0615;
		case RespawnType.EnterMultiplayer:
		case RespawnType.JoinMultiplayer:
			if (!this.spawnWindowOpened)
			{
				this.spawnPosition = new SpawnPosition(this.entityPlayerLocal.GetPosition(), this.entityPlayerLocal.rotation.y);
				if ((this.spawnPosition.IsUndef() || this.spawnPosition.position.Equals(Constants.cStartPositionPlayerInLevel)) && !this.entityPlayerLocal.lastSpawnPosition.IsUndef())
				{
					this.spawnPosition = this.entityPlayerLocal.lastSpawnPosition;
				}
				if (this.spawnPosition.IsUndef() || this.spawnPosition.position.Equals(Constants.cStartPositionPlayerInLevel))
				{
					this.spawnPosition = this.gameManager.GetSpawnPointList().GetRandomSpawnPosition(this.entityPlayerLocal.world, null, 0, 0);
				}
				this.entityPlayerLocal.SetPosition(new Vector3(this.spawnPosition.position.x, (this.spawnPosition.position.y == 0f) ? Constants.cStartPositionPlayerInLevel.y : this.spawnPosition.position.y, this.spawnPosition.position.z), true);
				this.openSpawnWindow(this.respawnReason);
				this.respawnStateHistory.Add(RespawnProgress.WaitingForSpawnWindowToOpen);
				return;
			}
			this.spawnPosition = new SpawnPosition(this.entityPlayerLocal.GetPosition(), this.entityPlayerLocal.rotation.y);
			goto IL_0615;
		}
		if (!this.gameManager.IsEditMode() && !this.spawnWindowOpened)
		{
			this.openSpawnWindow(this.respawnReason);
			this.respawnStateHistory.Add(RespawnProgress.WaitingForSpawnWindowToOpen);
			return;
		}
		XUiC_SpawnSelectionWindow window = XUiC_SpawnSelectionWindow.GetWindow(LocalPlayerUI.primaryUI);
		if (!this.waitingForSpawnPointSelection && !this.gameManager.IsEditMode() && this.spawnWindowOpened && window.spawnMethod != SpawnMethod.Invalid)
		{
			base.StartCoroutine(this.FindRespawnSpawnPointRoutine(window.spawnMethod, window.spawnTarget));
			window.spawnMethod = SpawnMethod.Invalid;
			window.spawnTarget = SpawnPosition.Undef;
		}
		if (this.waitingForSpawnPointSelection)
		{
			this.respawnStateHistory.Add(RespawnProgress.WaitingForSpawnPointSelection);
			return;
		}
		if (this.entityPlayerLocal.position != this.spawnPosition.position)
		{
			Vector3 vector = this.spawnPosition.position;
			if (this.spawnPosition.IsUndef())
			{
				vector = this.entityPlayerLocal.GetPosition();
			}
			this.spawnPosition = new SpawnPosition(vector + new Vector3(0f, 5f, 0f), this.entityPlayerLocal.rotation.y);
			this.entityPlayerLocal.SetPosition(this.spawnPosition.position, true);
		}
		IL_0615:
		if (GameUtils.IsPlaytesting() || (GameManager.Instance.IsEditMode() && GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty"))
		{
			SpawnPointList spawnPointList = GameManager.Instance.GetSpawnPointList();
			if (this.respawnReason != RespawnType.Teleport && spawnPointList.Count > 0)
			{
				this.spawnPosition.position = spawnPointList[0].spawnPosition.position;
				this.spawnPosition.heading = spawnPointList[0].spawnPosition.heading;
				this.entityPlayerLocal.SetPosition(this.spawnPosition.position, true);
			}
		}
		if (!this.spawnPosition.IsUndef())
		{
			if (!PrefabEditModeManager.Instance.IsActive() && !this.gameManager.World.IsPositionAvailable(this.spawnPosition.position))
			{
				this.spawnPosition.position = this.gameManager.World.ClampToValidWorldPos(this.spawnPosition.position);
				if (this.entityPlayerLocal.position != this.spawnPosition.position)
				{
					this.entityPlayerLocal.SetPosition(this.spawnPosition.position, true);
				}
				this.respawnStateHistory.Add(RespawnProgress.ClampingToValidWorldPos);
				return;
			}
			if (!this.entityPlayerLocal.CheckSpawnPointStillThere())
			{
				this.entityPlayerLocal.RemoveSpawnPoints(true);
				if (flag)
				{
					this.entityPlayerLocal.QuestJournal.RemoveAllSharedQuests();
					this.entityPlayerLocal.QuestJournal.StartQuests();
				}
			}
			Vector3i vector3i = World.worldToBlockPos(this.spawnPosition.position);
			float num2 = (float)(this.gameManager.World.GetHeight(vector3i.x, vector3i.z) + 1);
			if (this.spawnPosition.position.y < 0f || this.spawnPosition.position.y > num2)
			{
				this.spawnPosition.position.y = num2;
			}
			else if (this.spawnPosition.position.y < num2 && !this.gameManager.World.CanPlayersSpawnAtPos(this.spawnPosition.position, true))
			{
				this.spawnPosition.position.y = this.spawnPosition.position.y + 1f;
				if (!this.gameManager.World.CanPlayersSpawnAtPos(this.spawnPosition.position, true))
				{
					this.spawnPosition.position.y = num2;
				}
			}
		}
		if (flag)
		{
			GameOptionsManager.ApplyCameraOptions(this.entityPlayerLocal);
		}
		Log.Out("Respawn almost done");
		if (this.spawnPosition.IsUndef())
		{
			this.entityPlayerLocal.Respawn(this.respawnReason);
			this.respawnStateHistory.Add(RespawnProgress.RetryingRespawn);
			return;
		}
		RaycastHit raycastHit;
		float num3;
		if (Physics.Raycast(new Ray(this.spawnPosition.position + Vector3.up - Origin.position, Vector3.down), ref raycastHit, 3f, 1342242816))
		{
			num3 = raycastHit.point.y - this.spawnPosition.position.y + Origin.position.y;
		}
		else
		{
			num3 = this.gameManager.World.GetTerrainOffset(World.worldToBlockPos(this.spawnPosition.position)) + 0.05f;
		}
		this.gameManager.ClearTooltips(this.nguiWindowManager);
		this.spawnPosition.position.y = this.spawnPosition.position.y + (num3 + this.entityPlayerLocal.m_characterController.GetSkinWidth());
		this.entityPlayerLocal.onGround = true;
		this.entityPlayerLocal.lastSpawnPosition = this.spawnPosition;
		this.entityPlayerLocal.Spawned = true;
		GameManager.Instance.PlayerSpawnedInWorld(null, this.respawnReason, new Vector3i(this.spawnPosition.position), this.entityPlayerLocal.entityId);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToClientsOrServer(NetPackageManager.GetPackage<NetPackagePlayerSpawnedInWorld>().Setup(this.respawnReason, new Vector3i(this.spawnPosition.position), this.entityPlayerLocal.entityId));
		if (this.respawnReason == RespawnType.Died || this.respawnReason == RespawnType.EnterMultiplayer || this.respawnReason == RespawnType.NewGame)
		{
			this.entityPlayerLocal.SetAlive();
		}
		else
		{
			this.entityPlayerLocal.bDead = false;
		}
		if (this.respawnReason == RespawnType.NewGame || this.respawnReason == RespawnType.LoadedGame || this.respawnReason == RespawnType.EnterMultiplayer || this.respawnReason == RespawnType.JoinMultiplayer)
		{
			this.entityPlayerLocal.TryAddRecoveryPosition(Vector3i.FromVector3Rounded(this.spawnPosition.position));
		}
		this.entityPlayerLocal.ResetLastTickPos(this.spawnPosition.position);
		if (!this.entityPlayerLocal.AttachedToEntity)
		{
			this.entityPlayerLocal.transform.position = this.spawnPosition.position - Origin.position;
		}
		else
		{
			this.spawnPosition.position.y = this.spawnPosition.position.y + 2f;
		}
		entity.SetPosition(this.spawnPosition.position, true);
		entity.SetRotation(new Vector3(0f, this.spawnPosition.heading, 0f));
		this.entityPlayerLocal.JetpackWearing = false;
		this.entityPlayerLocal.ParachuteWearing = false;
		this.entityPlayerLocal.AfterPlayerRespawn(this.respawnReason);
		if (flag)
		{
			this.entityPlayerLocal.QuestJournal.RemoveAllSharedQuests();
			this.entityPlayerLocal.QuestJournal.StartQuests();
		}
		if ((this.respawnReason == RespawnType.NewGame || this.respawnReason == RespawnType.EnterMultiplayer) && !GameManager.Instance.World.IsEditor() && !(GameMode.GetGameModeForId(GameStats.GetInt(EnumGameStats.GameModeId)) is GameModeCreative) && !GameUtils.IsPlaytesting() && !GameManager.bRecordNextSession && !GameManager.bPlayRecordedSession)
		{
			GameEventManager.Current.HandleAction("game_first_spawn", this.entityPlayerLocal, this.entityPlayerLocal, false, "", "", false, true, "", null);
		}
		if (this.respawnReason != RespawnType.Died && this.respawnReason != RespawnType.Teleport && GameStats.GetBool(EnumGameStats.AutoParty) && this.entityPlayerLocal.Party == null)
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyActions>().Setup(NetPackagePartyActions.PartyActions.JoinAutoParty, this.entityPlayerLocal.entityId, this.entityPlayerLocal.entityId, null, null), false);
			}
			else
			{
				Party.ServerHandleAutoJoinParty(this.entityPlayerLocal);
			}
		}
		if (this.respawnReason == RespawnType.JoinMultiplayer || this.respawnReason == RespawnType.LoadedGame)
		{
			this.entityPlayerLocal.ReassignEquipmentTransforms();
			GameEventManager.Current.HandleAction("game_on_spawn", this.entityPlayerLocal, this.entityPlayerLocal, false, "", "", false, true, "", null);
		}
		this.entityPlayerLocal.EnableCamera(true);
		GameManager.Instance.World.RefreshEntitiesOnMap();
		LocalPlayerUI.primaryUI.windowManager.Close(XUiC_LoadingScreen.ID);
		LocalPlayerUI.primaryUI.windowManager.Close("eacWarning");
		LocalPlayerUI.primaryUI.windowManager.Close("crossplayWarning");
		if (flag && PlatformManager.NativePlatform.GameplayNotifier != null)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				bool flag2 = SingletonMonoBehaviour<ConnectionManager>.Instance.CurrentMode == ProtocolManager.NetworkType.Server;
				bool allowsCrossplay = SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.AllowsCrossplay;
				PlatformManager.NativePlatform.GameplayNotifier.GameplayStart(flag2, allowsCrossplay);
			}
			else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				bool allowsCrossplay2 = SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo.AllowsCrossplay;
				PlatformManager.NativePlatform.GameplayNotifier.GameplayStart(true, allowsCrossplay2);
			}
		}
		if (this.respawnReason == RespawnType.Died)
		{
			this.entityPlayerLocal.QuestJournal.FailAllActivatedQuests();
			this.entityPlayerLocal.Progression.OnRespawnFromDeath();
			switch (GameStats.GetInt(EnumGameStats.DeathPenalty))
			{
			case 0:
				GameEventManager.Current.HandleAction("game_on_respawn_none", this.entityPlayerLocal, this.entityPlayerLocal, false, "", "", false, true, "", null);
				break;
			case 1:
				GameEventManager.Current.HandleAction("game_on_respawn_default", this.entityPlayerLocal, this.entityPlayerLocal, false, "", "", false, true, "", null);
				break;
			case 2:
				GameEventManager.Current.HandleAction("game_on_respawn_injured", this.entityPlayerLocal, this.entityPlayerLocal, false, "", "", false, true, "", null);
				break;
			case 3:
				GameEventManager.Current.HandleAction("game_on_respawn_permanent", this.entityPlayerLocal, this.entityPlayerLocal, false, "", "", false, true, "", null);
				break;
			}
			this.entityPlayerLocal.ResetBiomeWeatherOnDeath();
		}
		if (!this.gameManager.IsEditMode() && (this.respawnReason == RespawnType.NewGame || this.respawnReason == RespawnType.EnterMultiplayer))
		{
			this.windowManager.TempHUDDisable();
			this.entityPlayerLocal.SetControllable(false);
			this.entityPlayerLocal.bIntroAnimActive = true;
			GameManager.Instance.StartCoroutine(this.showUILater());
			if (!GameUtils.IsPlaytesting())
			{
				GameManager.Instance.StartCoroutine(this.initializeHoldingItemLater(4f));
			}
		}
		else
		{
			this.entityPlayerLocal.SetControllable(true);
			GameManager.Instance.StartCoroutine(this.initializeHoldingItemLater(0.1f));
		}
		if (this.entityPlayerLocal.AttachedToEntity != null)
		{
			EntityVehicle entityVehicle = this.entityPlayerLocal.AttachedToEntity as EntityVehicle;
			if (entityVehicle != null)
			{
				entityVehicle.CameraInit();
			}
		}
		this.respawnStateHistory.Add(RespawnProgress.Done);
		this.bLastRespawnActive = false;
	}

	public void LogCurrentRespawnState()
	{
		string text = "[FELLTHROUGHWORLD] Respawn State for player {0} ('{1}'): {2}";
		object obj = this.entityPlayerLocal.entityId;
		EntityPlayerLocal entityPlayerLocal = this.entityPlayerLocal;
		Log.Out(string.Format(text, obj, (entityPlayerLocal != null) ? entityPlayerLocal.EntityName : null, this.respawnStateHistory));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator initializeHoldingItemLater(float _time)
	{
		yield return new WaitForSeconds(_time);
		if (this.entityPlayerLocal != null && this.entityPlayerLocal.inventory != null)
		{
			this.entityPlayerLocal.inventory.ForceHoldingItemUpdate();
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator showUILater()
	{
		yield return new WaitForSeconds(4f);
		if (this.entityPlayerLocal != null && this.entityPlayerLocal.transform != null)
		{
			this.entityPlayerLocal.bIntroAnimActive = false;
			this.entityPlayerLocal.SetControllable(true);
		}
		if (this.windowManager != null)
		{
			this.windowManager.ReEnableHUD();
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openSpawnWindow(RespawnType _respawnReason)
	{
		Log.Out("OpenSpawnWindow");
		XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
		if (!this.gameManager.IsEditMode())
		{
			LocalPlayerUI.primaryUI.windowManager.Open(XUiC_LoadingScreen.ID, false);
		}
		XUiC_SpawnSelectionWindow.Open(LocalPlayerUI.primaryUI, _respawnReason != RespawnType.EnterMultiplayer && _respawnReason != RespawnType.JoinMultiplayer && _respawnReason != RespawnType.NewGame && _respawnReason != RespawnType.Teleport && _respawnReason != RespawnType.LoadedGame, false, false);
		this.spawnWindowOpened = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnPosition findSpawnPosition(SpawnMethod _spawnMethod, SpawnPosition _spawnTarget)
	{
		SpawnPosition spawnPosition = SpawnPosition.Undef;
		if (_spawnMethod == SpawnMethod.OnBedRoll && spawnPosition.IsUndef())
		{
			spawnPosition = _spawnTarget;
			if (!spawnPosition.IsUndef())
			{
				string text = string.Format("Spawn pos: {0} ", SpawnMethod.OnBedRoll);
				SpawnPosition spawnPosition2 = spawnPosition;
				Log.Out(text + spawnPosition2.ToString());
			}
		}
		if ((_spawnMethod == SpawnMethod.NearBedroll || _spawnMethod == SpawnMethod.NearBackpack || _spawnMethod == SpawnMethod.NearDeath) && spawnPosition.IsUndef() && !_spawnTarget.IsUndef())
		{
			Vector3 vector;
			if (this.gameManager.World.GetRandomSpawnPositionMinMaxToPosition(_spawnTarget.position, 48, 96, 48, false, out vector, this.entityPlayerLocal.entityId, true, 50, false, EnumLandClaimOwner.None, false))
			{
				spawnPosition.position = vector;
				string text2 = string.Format("Spawn pos: random {0} ", _spawnMethod);
				SpawnPosition spawnPosition2 = spawnPosition;
				Log.Out(text2 + spawnPosition2.ToString());
			}
			if (spawnPosition.IsUndef() && this.entityPlayerLocal.recoveryPositions.Count > 0)
			{
				for (int i = this.entityPlayerLocal.recoveryPositions.Count - 1; i >= 0; i--)
				{
					if (Vector3.Distance(this.entityPlayerLocal.recoveryPositions[i], _spawnTarget.position) > 48f)
					{
						spawnPosition.position = this.entityPlayerLocal.recoveryPositions[i];
						string text3 = "Spawn pos: Recovery Point ";
						SpawnPosition spawnPosition2 = spawnPosition;
						Log.Out(text3 + spawnPosition2.ToString());
						break;
					}
				}
			}
		}
		if (_spawnMethod == SpawnMethod.NewRandomSpawn && spawnPosition.IsUndef() && !_spawnTarget.IsUndef())
		{
			spawnPosition = _spawnTarget;
			string text4 = string.Format("Spawn pos: random {0} ", _spawnMethod);
			SpawnPosition spawnPosition2 = spawnPosition;
			Log.Out(text4 + spawnPosition2.ToString());
		}
		if (spawnPosition.IsUndef())
		{
			if (!_spawnTarget.IsUndef())
			{
				spawnPosition = this.gameManager.GetSpawnPointList().GetRandomSpawnPosition(this.entityPlayerLocal.world, new Vector3?(_spawnTarget.position), 300, 600);
				string text5 = "Spawn pos: start point ";
				SpawnPosition spawnPosition2 = spawnPosition;
				Log.Out(text5 + spawnPosition2.ToString() + " distance to backpack: " + (spawnPosition.position - _spawnTarget.position).magnitude.ToCultureInvariantString());
			}
			else
			{
				spawnPosition = this.gameManager.GetSpawnPointList().GetRandomSpawnPosition(this.entityPlayerLocal.world, new Vector3?(this.entityPlayerLocal.position), 300, 600);
				string text6 = "Spawn pos: start point ";
				SpawnPosition spawnPosition2 = spawnPosition;
				Log.Out(text6 + spawnPosition2.ToString());
			}
		}
		if (spawnPosition.IsUndef())
		{
			int num = Utils.Fastfloor(this.entityPlayerLocal.position.x);
			int num2 = Utils.Fastfloor(this.entityPlayerLocal.position.y);
			int num3 = Utils.Fastfloor(this.entityPlayerLocal.position.z);
			IChunk chunkFromWorldPos = this.gameManager.World.GetChunkFromWorldPos(num, num2, num3);
			if (chunkFromWorldPos != null)
			{
				if (this.entityPlayerLocal.position.y == Constants.cStartPositionPlayerInLevel.y)
				{
					this.entityPlayerLocal.position.y = (float)(chunkFromWorldPos.GetHeight(ChunkBlockLayerLegacy.CalcOffset(num, num3)) + 1);
				}
				spawnPosition = new SpawnPosition(this.entityPlayerLocal.GetPosition(), this.entityPlayerLocal.rotation.y);
				string text7 = "Spawn pos: current player pos ";
				SpawnPosition spawnPosition2 = spawnPosition;
				Log.Out(text7 + spawnPosition2.ToString());
			}
		}
		return spawnPosition;
	}

	public IEnumerator FindRespawnSpawnPointRoutine(SpawnMethod _method, SpawnPosition _spawnTarget)
	{
		this.waitingForSpawnPointSelection = true;
		Vector3 targetPosition = this.entityPlayerLocal.position;
		if (!_spawnTarget.IsUndef())
		{
			targetPosition = _spawnTarget.position;
		}
		this.entityPlayerLocal.SetPosition(targetPosition, true);
		yield return new WaitForSeconds(2f);
		float waitTime = 0f;
		while (!GameManager.Instance.World.IsChunkAreaLoaded(targetPosition) && waitTime < 5f)
		{
			yield return new WaitForSeconds(0.25f);
			waitTime += 0.25f;
		}
		this.spawnPosition = this.findSpawnPosition(_method, _spawnTarget);
		this.waitingForSpawnPointSelection = false;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stopMoving()
	{
		this.isAutorun = false;
		this.entityPlayerLocal.movementInput.moveForward = 0f;
		this.entityPlayerLocal.movementInput.moveStrafe = 0f;
		this.entityPlayerLocal.MoveByInput();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDebugKeys()
	{
		if (this.windowManager.IsModalWindowOpen())
		{
			return;
		}
		if (!GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
		{
			return;
		}
		bool flag = this.playerUI.windowManager.IsInputActive();
		bool flag2 = flag || this.wasUIInputActive;
		this.wasUIInputActive = flag;
		if (flag2)
		{
			return;
		}
		if (Input.GetKeyDown(256))
		{
			Manager.PlayButtonClick();
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageDebug>().Setup(NetPackageDebug.Type.AINameInfoServerToggle, -1, null), false);
			}
			else
			{
				bool flag3 = !GamePrefs.GetBool(EnumGamePrefs.DebugMenuShowTasks);
				GamePrefs.Set(EnumGamePrefs.DebugMenuShowTasks, flag3);
				EntityAlive.SetupAllDebugNameHUDs(flag3);
			}
		}
		if (this.gameManager.World.IsRemote())
		{
			return;
		}
		bool shiftKeyPressed = InputUtils.ShiftKeyPressed;
		if (!shiftKeyPressed)
		{
			float num = Time.timeScale;
			if (Input.GetKeyDown(271))
			{
				if (num > 0f)
				{
					num = 0f;
				}
				else
				{
					num = 1f;
				}
			}
			if (Input.GetKeyDown(269))
			{
				num = Mathf.Max(num - 0.05f, 0f);
			}
			if (Input.GetKeyDown(270))
			{
				num = Mathf.Min(num + 0.05f, 2f);
			}
			if (num != Time.timeScale)
			{
				Time.timeScale = num;
				Log.Out("Time scale {0}", new object[] { num.ToCultureInvariantString() });
				Manager.PlayButtonClick();
			}
			if (Input.GetKeyDown(266))
			{
				if (InputUtils.ControlKeyPressed)
				{
					if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync("killall all", null);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup("killall all"), false);
					}
				}
				else
				{
					float num2 = (float)(InputUtils.AltKeyPressed ? 100 : 0);
					this.entityPlayerLocal.emodel.DoRagdoll(EModelBase.RagdollMode.Default, 2f, EnumBodyPartHit.Torso, this.entityPlayerLocal.rand.RandomInsideUnitSphere * num2, this.entityPlayerLocal.transform.position + this.entityPlayerLocal.rand.RandomInsideUnitSphere * 0.1f + Origin.position, false);
				}
			}
		}
		else if (Input.GetKeyDown(270))
		{
			this.drawChunkMode = (this.drawChunkMode + 1) % 3;
		}
		if (this.playerInput.AiFreeze.WasPressed && !GameManager.Instance.IsEditMode())
		{
			Manager.PlayButtonClick();
			if (InputUtils.ControlKeyPressed)
			{
				EAIManager.ToggleAnimFreeze();
				if (EAIManager.isAnimFreeze)
				{
					this.entityPlayerLocal.Buffs.AddBuff("buffShowAnimationDisabled", -1, true, false, -1f);
					return;
				}
				this.entityPlayerLocal.Buffs.RemoveBuff("buffShowAnimationDisabled", -1, true);
				return;
			}
			else
			{
				if (shiftKeyPressed)
				{
					this.entityPlayerLocal.SetIgnoredByAI(!this.entityPlayerLocal.IsIgnoredByAI());
					return;
				}
				bool flag4 = !GamePrefs.GetBool(EnumGamePrefs.DebugStopEnemiesMoving);
				GamePrefs.Set(EnumGamePrefs.DebugStopEnemiesMoving, flag4);
				if (flag4)
				{
					this.entityPlayerLocal.Buffs.AddBuff("buffShowAIDisabled", -1, true, false, -1f);
					return;
				}
				this.entityPlayerLocal.Buffs.RemoveBuff("buffShowAIDisabled", -1, true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canSwapHeldItem()
	{
		ItemActionEat itemActionEat = this.entityPlayerLocal.inventory.holdingItem.Actions[0] as ItemActionEat;
		bool flag = false;
		if (itemActionEat != null)
		{
			flag = itemActionEat.PercentDone(this.entityPlayerLocal.inventory.holdingItemData.actionData[0]) > 0.75f;
		}
		return this.entityPlayerLocal.inventory.GetIsFinishedSwitchingHeldItem() && !flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (this.nextHeldItem.Count > 0 && this.canSwapHeldItem())
		{
			this.swapItem(this.nextHeldItem[this.nextHeldItem.Count - 1]);
			this.nextHeldItem.Clear();
		}
		PlayerActionsLocal playerInput = this.playerInput;
		bool flag = !this.playerUI.windowManager.IsCursorWindowOpen() && !this.playerUI.windowManager.IsModalWindowOpen() && (playerInput.Enabled || playerInput.VehicleActions.Enabled);
		if (DroneManager.Debug_LocalControl)
		{
			flag = false;
		}
		if (this.playerAutoPilotControllor != null && this.playerAutoPilotControllor.IsEnabled())
		{
			this.playerAutoPilotControllor.Update();
		}
		if ((!this.bCanControlOverride || !flag) && GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 0)
		{
			XUiC_InteractionPrompt.SetText(this.playerUI, null);
			this.strTextLabelPointingTo = string.Empty;
		}
		if (!this.gameManager.gameStateManager.IsGameStarted() || GameStats.GetInt(EnumGameStats.GameState) != 1)
		{
			this.stopMoving();
			return;
		}
		this.updateRespawn();
		this.updateDebugKeys();
		if (this.drawChunkMode > 0)
		{
			this.DrawChunkBoundary();
			if (this.drawChunkMode == 2)
			{
				this.DrawChunkDensities();
			}
		}
		if (this.entityPlayerLocal.emodel.IsRagdollActive)
		{
			this.stopMoving();
			return;
		}
		if (this.entityPlayerLocal.IsDead())
		{
			XUiC_InteractionPrompt.SetText(this.playerUI, null);
			this.strTextLabelPointingTo = string.Empty;
			return;
		}
		this.guiOpenThisUpdate = this.entityPlayerLocal.PlayerUI.windowManager.IsModalWindowOpen();
		this.wasGuiClosedThisUpdate = !this.guiOpenThisUpdate && this.guiOpenLastUpdate;
		this.guiOpenLastUpdate = this.guiOpenThisUpdate;
		if (this.wasGuiClosedThisUpdate)
		{
			this.guiClosedUpdateCount = 0;
		}
		if (this.guiClosedUpdateCount < 3)
		{
			this.guiClosedUpdateCount++;
			return;
		}
		bool flag2 = false;
		float num = playerInput.Scroll.Value;
		if (playerInput.LastInputType == 1)
		{
			if (!this.entityPlayerLocal.AimingGun)
			{
				num = 0f;
			}
			else
			{
				num *= 0.25f;
			}
		}
		num *= 0.25f;
		if (Mathf.Abs(num) < 0.001f)
		{
			num = 0f;
		}
		this.gameManager.GetActiveBlockTool().CheckKeys(this.entityPlayerLocal.inventory.holdingItemData, this.entityPlayerLocal.HitInfo, playerInput);
		if (this.gameManager.IsEditMode() || BlockToolSelection.Instance.SelectionActive)
		{
			SelectionBoxManager.Instance.CheckKeys(this.gameManager, playerInput, this.entityPlayerLocal.HitInfo);
			if (!flag2)
			{
				flag2 = SelectionBoxManager.Instance.ConsumeScrollWheel(num, playerInput);
			}
			flag2 = this.gameManager.GetActiveBlockTool().ConsumeScrollWheel(this.entityPlayerLocal.inventory.holdingItemData, num, playerInput);
		}
		bool flag3 = GameManager.Instance.isAnyCursorWindowOpen(null);
		bool flag4 = (playerInput.QuickMenu.IsPressed || playerInput.PermanentActions.QuickMenu.IsPressed) && this.entityPlayerLocal.PlayerUI.windowManager.IsWindowOpen(this.playerUI.xui.RadialWindow.WindowGroup);
		bool flag5 = playerInput.CameraChange.IsPressed && !flag3;
		if (this.cameraChangePressTime < 0f)
		{
			if (flag4 || flag5)
			{
				this.cameraChangePressTime = Time.time;
			}
			else
			{
				this.cameraChangePressTime = -1f;
			}
		}
		if (playerInput.CameraChange.WasReleased && !flag3)
		{
			if (!this.wasCameraChangeUsedWithWheel)
			{
				this.cameraChangeRequested = true;
			}
			this.wasCameraChangeUsedWithWheel = false;
		}
		if (!this.entityPlayerLocal.AimingGun && (playerInput.CameraChange.IsPressed || playerInput.QuickMenu.IsPressed || playerInput.PermanentActions.QuickMenu.IsPressed))
		{
			if (this.cameraChangePressTime > 0f && Time.time - this.cameraChangePressTime > 0.5f)
			{
				this.entityPlayerLocal.ClearMovementInputs();
			}
			if (flag4 || flag5)
			{
				float num2 = playerInput.PermanentActions.CameraZoom.Value * 0.25f;
				if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
				{
					num2 *= 0.1f;
				}
				if (this.entityPlayerLocal.TryUpdateCameraDistanceMultiplier(-num2))
				{
					this.wasCameraChangeUsedWithWheel = true;
					this.cameraChangeRequested = false;
				}
				this.entityPlayerLocal.PlayerUI.xui.CalloutWindow.SetCalloutsEnabled(XUiC_GamepadCalloutWindow.CalloutType.CameraZoom, true);
				return;
			}
			this.wasCameraChangeUsedWithWheel = false;
		}
		this.entityPlayerLocal.PlayerUI.xui.CalloutWindow.SetCalloutsEnabled(XUiC_GamepadCalloutWindow.CalloutType.CameraZoom, false);
		if (!flag2 && !flag3 && !this.entityPlayerLocal.AimingGun && (this.playerInput.QuickMenu.WasPressed || this.playerInput.PermanentActions.QuickMenu.WasPressed))
		{
			if (!GameManager.Instance.IsEditMode())
			{
				this.playerUI.xui.RadialWindow.Open();
				this.playerUI.xui.RadialWindow.SetupQuickActionsMenu(this.entityPlayerLocal);
				return;
			}
			this.entityPlayerLocal.SwitchFirstPersonViewFromInput();
		}
		if ((!this.bCanControlOverride || !flag) && GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 0)
		{
			this.stopMoving();
			return;
		}
		bool flag6 = playerInput.LastInputType == 1;
		this.entityPlayerLocal.movementInput.lastInputController = flag6;
		if (!this.gameManager.IsEditMode() || GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 0)
		{
			bool controlKeyPressed = InputUtils.ControlKeyPressed;
			bool enabled = playerInput.VehicleActions.Enabled;
			bool flag7 = this.wasVehicle != enabled;
			PlayerAction playerAction = (enabled ? playerInput.VehicleActions.Turbo : playerInput.Run);
			PlayerAction playerAction2 = (enabled ? playerInput.VehicleActions.MoveForward : playerInput.MoveForward);
			if (this.sprintMode == 2)
			{
				if (!enabled)
				{
					PlayerAction moveBack = playerInput.MoveBack;
				}
				else
				{
					PlayerAction moveBack2 = playerInput.VehicleActions.MoveBack;
				}
				bool isPressed = playerAction.IsPressed;
				bool flag8 = Utils.FastAbs(enabled ? playerInput.VehicleActions.Move.Y : playerInput.Move.Y) >= 0.35f || (enabled && playerInput.VehicleActions.Brake);
				flag8 |= this.gameManager.IsEditMode();
				if (!this.isAutorun)
				{
					bool running = this.entityPlayerLocal.movementInput.running;
					this.entityPlayerLocal.movementInput.running = isPressed;
					if (isPressed)
					{
						if (playerAction.WasPressed)
						{
							this.runPressedWhileActive = true;
						}
						this.runInputTime += Time.deltaTime;
						if (!running)
						{
							this.runInputTime = 0f;
						}
						if (flag8)
						{
							this.isAutorunInvalid = true;
						}
					}
					else if (playerAction.WasReleased)
					{
						if (!this.isAutorunInvalid)
						{
							float num3 = this.runInputTime;
							if (num3 > 0f && num3 < 0.2f)
							{
								this.isAutorun = true;
								this.entityPlayerLocal.movementInput.running = true;
								this.runInputTime = 0f;
							}
						}
						this.isAutorunInvalid = false;
					}
				}
				else
				{
					if (isPressed)
					{
						this.isAutorun = false;
						this.isAutorunInvalid = true;
					}
					if (flag8)
					{
						this.isAutorun = false;
					}
				}
			}
			else
			{
				if (playerAction.WasPressed)
				{
					this.runInputTime = 0f;
					this.entityPlayerLocal.movementInput.running = true;
					this.runPressedWhileActive = true;
				}
				else if (playerAction.WasReleased && this.runPressedWhileActive)
				{
					if (this.runInputTime > 0.2f)
					{
						this.entityPlayerLocal.movementInput.running = false;
						this.runToggleActive = false;
					}
					else if (this.runToggleActive)
					{
						this.runToggleActive = false;
						this.entityPlayerLocal.movementInput.running = false;
					}
					else if (playerAction2.IsPressed || this.sprintMode == 1)
					{
						this.runToggleActive = true;
					}
					else
					{
						this.runToggleActive = false;
						this.entityPlayerLocal.movementInput.running = false;
					}
					this.runPressedWhileActive = false;
				}
				if (playerAction.IsPressed)
				{
					this.runInputTime += Time.deltaTime;
				}
				if (this.runToggleActive)
				{
					if (flag7 || (this.sprintMode == 0 && (this.entityPlayerLocal.Stamina <= 0f || !playerAction2.IsPressed)))
					{
						this.runToggleActive = false;
						this.runPressedWhileActive = false;
						this.entityPlayerLocal.movementInput.running = false;
					}
					else
					{
						this.entityPlayerLocal.movementInput.running = true;
					}
				}
			}
			this.entityPlayerLocal.movementInput.down = playerInput.Crouch.IsPressed && (!this.gameManager.IsEditMode() || !controlKeyPressed);
			this.entityPlayerLocal.movementInput.jump = playerInput.Jump.IsPressed;
			if (this.entityPlayerLocal.movementInput.running && this.entityPlayerLocal.AimingGun)
			{
				this.entityPlayerLocal.AimingGun = false;
			}
			this.wasVehicle = enabled;
		}
		else
		{
			this.entityPlayerLocal.movementInput.running = false;
			this.runPressedWhileActive = false;
			this.isAutorun = false;
		}
		this.entityPlayerLocal.movementInput.downToggle = !this.gameManager.IsEditMode() && !this.entityPlayerLocal.IsFlyMode.Value && playerInput.ToggleCrouch.WasPressed;
		if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) && playerInput.PermanentActions.DebugControllerLeft.IsPressed && playerInput.PermanentActions.DebugControllerRight.IsPressed)
		{
			if (playerInput.GodAlternate.WasPressed)
			{
				this.toggleGodMode();
			}
			if (playerInput.TeleportAlternate.WasPressed)
			{
				this.teleportPlayer();
			}
		}
		if (playerInput.DecSpeed.WasPressed)
		{
			this.entityPlayerLocal.GodModeSpeedModifier = Utils.FastMax(0.1f, this.entityPlayerLocal.GodModeSpeedModifier - 0.1f);
		}
		if (playerInput.IncSpeed.WasPressed)
		{
			this.entityPlayerLocal.GodModeSpeedModifier = Utils.FastMin(3f, this.entityPlayerLocal.GodModeSpeedModifier + 0.1f);
		}
		Vector2 vector;
		Vector2 vector3;
		if (playerInput.Look.LastInputType != 3)
		{
			this.entityPlayerLocal.movementInput.down = this.entityPlayerLocal.IsFlyMode.Value && playerInput.ToggleCrouch.IsPressed;
			float num4;
			if (playerInput.VehicleActions.Enabled)
			{
				vector.x = playerInput.VehicleActions.Look.X;
				vector.y = playerInput.VehicleActions.Look.Y * (float)this.invertController;
				num4 = playerInput.VehicleActions.Look.Vector.magnitude;
			}
			else
			{
				vector.x = playerInput.Look.X;
				vector.y = playerInput.Look.Y * (float)this.invertController;
				num4 = playerInput.Look.Vector.magnitude;
			}
			if (this.lookAccelerationRate <= 0f)
			{
				this.currentLookAcceleration = 1f;
			}
			else if (num4 > 0f)
			{
				this.currentLookAcceleration = Mathf.Clamp(this.currentLookAcceleration + this.lookAccelerationRate * num4 * Time.unscaledDeltaTime, 0f, num4);
			}
			else
			{
				this.currentLookAcceleration = 0f;
			}
			Vector2 vector2 = this.controllerLookSensitivity;
			if (this.entityPlayerLocal.AimingGun)
			{
				vector2 *= this.controllerZoomSensitivity;
			}
			else if (playerInput.VehicleActions.Enabled)
			{
				vector2 *= this.controllerVehicleSensitivity;
			}
			vector3 = vector2 * this.lookAccelerationCurve.Evaluate(this.currentLookAcceleration);
			if (this.entityPlayerLocal.AimingGun)
			{
				float num5 = Mathf.Lerp(0.2f, 1f, (this.entityPlayerLocal.playerCamera.fieldOfView - 10f) / ((float)Constants.cDefaultCameraFieldOfView - 10f));
				vector3 *= num5;
			}
			if (this.entityPlayerLocal.AttachedToEntity != null)
			{
				this.aimAssistSlowAmount = 1f;
			}
			else
			{
				bool flag9 = false;
				WorldRayHitInfo hitInfo = this.entityPlayerLocal.HitInfo;
				if (hitInfo.bHitValid)
				{
					if (hitInfo.transform)
					{
						Transform hitRootTransform = GameUtils.GetHitRootTransform(hitInfo.tag, hitInfo.transform);
						if (hitRootTransform != null)
						{
							EntityAlive entityAlive;
							EntityItem entityItem;
							if (hitRootTransform.TryGetComponent<EntityAlive>(ref entityAlive) && entityAlive.IsAlive() && entityAlive.IsValidAimAssistSlowdownTarget && hitInfo.hit.distanceSq <= 50f && (this.entityPlayerLocal.inventory.holdingItem.Actions[0] is ItemActionAttack || this.entityPlayerLocal.inventory.holdingItem.Actions[0] is ItemActionDynamicMelee))
							{
								this.bAimAssistTargetingItem = false;
								flag9 = true;
							}
							else if ((hitInfo.tag.StartsWith("Item", StringComparison.Ordinal) || hitRootTransform.TryGetComponent<EntityItem>(ref entityItem)) && hitInfo.hit.distanceSq <= 10f)
							{
								this.bAimAssistTargetingItem = true;
								flag9 = true;
							}
						}
					}
					else if (this.entityPlayerLocal.ThreatLevel.Numeric < 0.75f && GameUtils.IsBlockOrTerrain(hitInfo.tag) && this.entityPlayerLocal.PlayerUI.windowManager.IsWindowOpen("interactionPrompt"))
					{
						BlockValue blockValue = hitInfo.hit.blockValue;
						if (!blockValue.Block.isMultiBlock && !blockValue.Block.isOversized && blockValue.Block.shape is BlockShapeModelEntity)
						{
							this.bAimAssistTargetingItem = true;
							flag9 = true;
						}
					}
				}
				if (flag9)
				{
					this.aimAssistSlowAmount = (this.bAimAssistTargetingItem ? 0.6f : 0.5f);
				}
				else
				{
					this.aimAssistSlowAmount = Mathf.MoveTowards(this.aimAssistSlowAmount, 1f, Time.unscaledDeltaTime * 5f);
				}
				vector3 *= this.aimAssistSlowAmount;
				if (this.controllerAimAssistsEnabled && this.cameraSnapTargetEntity != null && this.cameraSnapTargetEntity.IsAlive() && Time.time - this.cameraSnapTime < 0.3f)
				{
					Vector2 vector4 = Vector2.one * 0.5f;
					Vector2 vector5 = (this.snapTargetingHead ? this.entityPlayerLocal.playerCamera.WorldToViewportPoint(this.cameraSnapTargetEntity.emodel.GetHeadTransform().position) : this.entityPlayerLocal.playerCamera.WorldToViewportPoint(this.cameraSnapTargetEntity.GetChestTransformPosition())) - vector4;
					float num6 = ((this.cameraSnapMode == eCameraSnapMode.MeleeAttack) ? 1.5f : 1f);
					vector += vector5.normalized * num6 * vector5.magnitude / 0.15f;
				}
			}
		}
		else
		{
			vector3 = this.mouseLookSensitivity;
			Vector2 vector6;
			vector6..ctor(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * (float)this.invertMouse);
			vector = (vector6 + this.previousMouseInput) / 2f;
			this.previousMouseInput = vector6;
			if (playerInput.VehicleActions.Enabled)
			{
				vector3 *= this.vehicleLookSensitivity;
			}
			else
			{
				float magnitude = vector.magnitude;
				float num7 = 1f;
				if (this.entityPlayerLocal.AimingGun && magnitude > 0f)
				{
					vector3 *= this.mouseZoomSensitivity;
					float num8 = Mathf.Pow(magnitude * 0.4f, 2.5f) / magnitude;
					num7 += num8 * this.zoomAccel;
					num7 *= Mathf.Lerp(0.2f, 1f, (this.entityPlayerLocal.playerCamera.fieldOfView - 10f) / ((float)Constants.cDefaultCameraFieldOfView - 10f));
					vector3 *= num7;
					if (vector3.magnitude > this.mouseLookSensitivity.magnitude)
					{
						vector3 = this.mouseLookSensitivity;
					}
				}
			}
			if (this.skipMouseLookNextFrame > 0 && (vector.x <= -1f || vector.x >= 1f || vector.y <= -1f || vector.y >= 1f))
			{
				this.skipMouseLookNextFrame--;
				vector = Vector2.zero;
			}
		}
		MovementInput movementInput = this.entityPlayerLocal.movementInput;
		if (!movementInput.bDetachedCameraMove)
		{
			PlayerActionsLocal playerActionsLocal = playerInput;
			if (this.playerAutoPilotControllor != null && this.playerAutoPilotControllor.IsEnabled())
			{
				movementInput.moveForward = this.playerAutoPilotControllor.GetForwardMovement();
			}
			else
			{
				movementInput.moveForward = playerActionsLocal.Move.Y;
				if (this.isAutorun)
				{
					movementInput.moveForward = 1f;
				}
			}
			movementInput.moveStrafe = playerActionsLocal.Move.X;
			if (movementInput.bCameraPositionLocked)
			{
				vector = Vector2.zero;
			}
			float num9 = Utils.FastMin(0.033333335f, Time.unscaledDeltaTime);
			if (PlayerMoveController.useScaledMouseLook && !this.entityPlayerLocal.movementInput.lastInputController)
			{
				float num10 = num9 * PlayerMoveController.mouseDeltaTimeScale;
				MovementInput movementInput2 = movementInput;
				movementInput2.rotation.x = movementInput2.rotation.x + vector.y * vector3.y * num10;
				MovementInput movementInput3 = movementInput;
				movementInput3.rotation.y = movementInput3.rotation.y + vector.x * vector3.x * num10;
			}
			else if (this.entityPlayerLocal.movementInput.lastInputController)
			{
				float num11 = num9 * PlayerMoveController.lookDeltaTimeScale;
				MovementInput movementInput4 = movementInput;
				movementInput4.rotation.x = movementInput4.rotation.x + vector.y * vector3.y * num11;
				MovementInput movementInput5 = movementInput;
				movementInput5.rotation.y = movementInput5.rotation.y + vector.x * vector3.x * num11;
			}
			else
			{
				MovementInput movementInput6 = movementInput;
				movementInput6.rotation.x = movementInput6.rotation.x + vector.y * vector3.y;
				MovementInput movementInput7 = movementInput;
				movementInput7.rotation.y = movementInput7.rotation.y + vector.x * vector3.x;
			}
			if (this.cameraChangeRequested)
			{
				if (!this.wasCameraChangeUsedWithWheel && !playerActionsLocal.Primary.IsPressed && !playerActionsLocal.Secondary.IsPressed)
				{
					this.entityPlayerLocal.SwitchFirstPersonViewFromInput();
					this.cameraChangeRequested = false;
				}
				this.wasCameraChangeUsedWithWheel = false;
			}
			if (playerActionsLocal.CameraFunction.WasPressed && !this.entityPlayerLocal.inventory.holdingItem.ConsumeCameraFunction(this.entityPlayerLocal.inventory.holdingItemData) && !this.entityPlayerLocal.bFirstPersonView && !this.entityPlayerLocal.isAimingScoped)
			{
				this.entityPlayerLocal.flipCameraSide = !this.entityPlayerLocal.flipCameraSide;
			}
			if ((this.gameManager.IsEditMode() || BlockToolSelection.Instance.SelectionActive) && (Input.GetKey(306) || GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) != 0))
			{
				movementInput.Clear();
			}
			this.entityPlayerLocal.MoveByInput();
		}
		else
		{
			float num12 = 0.15f;
			if (this.entityPlayerLocal.movementInput.running)
			{
				num12 *= 3f;
			}
			else
			{
				num12 *= this.entityPlayerLocal.GodModeSpeedModifier;
			}
			if (playerInput.MoveForward.IsPressed)
			{
				this.entityPlayerLocal.cameraTransform.position += this.entityPlayerLocal.cameraTransform.forward * num12;
			}
			if (playerInput.MoveBack.IsPressed)
			{
				this.entityPlayerLocal.cameraTransform.position -= this.entityPlayerLocal.cameraTransform.forward * num12;
			}
			if (playerInput.MoveLeft.IsPressed)
			{
				this.entityPlayerLocal.cameraTransform.position -= this.entityPlayerLocal.cameraTransform.right * num12;
			}
			if (playerInput.MoveRight.IsPressed)
			{
				this.entityPlayerLocal.cameraTransform.position += this.entityPlayerLocal.cameraTransform.right * num12;
			}
			if (playerInput.Jump.IsPressed)
			{
				this.entityPlayerLocal.cameraTransform.position += Vector3.up * num12;
			}
			if (playerInput.Crouch.IsPressed)
			{
				this.entityPlayerLocal.cameraTransform.position -= Vector3.up * num12;
			}
			if (!movementInput.bCameraPositionLocked)
			{
				Vector3 localEulerAngles = this.entityPlayerLocal.cameraTransform.localEulerAngles;
				this.entityPlayerLocal.cameraTransform.localEulerAngles = new Vector3(localEulerAngles.x - vector.y, localEulerAngles.y + vector.x, localEulerAngles.z);
			}
		}
		bool flag10 = this.gameManager.IsEditMode() && playerInput.Run.IsPressed;
		Ray ray = this.entityPlayerLocal.GetLookRay();
		if (this.gameManager.IsEditMode() && GamePrefs.GetInt(EnumGamePrefs.SelectionOperationMode) == 4)
		{
			ray = this.entityPlayerLocal.cameraTransform.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
			ray.origin += Origin.position;
		}
		ray.origin += ray.direction.normalized * 0.1f;
		float num13 = Utils.FastMax(Utils.FastMax(Constants.cDigAndBuildDistance, Constants.cCollectItemDistance), 30f);
		RaycastHit raycastHit;
		bool flag11 = Physics.Raycast(new Ray(ray.origin - Origin.position, ray.direction), ref raycastHit, num13, 73728);
		bool flag12 = false;
		if (flag11 && raycastHit.transform.CompareTag("E_BP_Body"))
		{
			flag12 = true;
		}
		if (flag11)
		{
			flag11 &= raycastHit.transform.CompareTag("Item");
		}
		int num14 = 69;
		bool flag13;
		if (!this.gameManager.IsEditMode())
		{
			flag13 = Voxel.Raycast(this.gameManager.World, ray, num13, -555528221, num14, 0f);
			if (flag13)
			{
				Transform hitRootTransform2 = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, Voxel.voxelRayHitInfo.transform);
				Entity entity;
				EntityAlive entityAlive2 = ((hitRootTransform2 != null && hitRootTransform2.TryGetComponent<Entity>(ref entity)) ? (entity as EntityAlive) : null);
				if (entityAlive2 == null || !entityAlive2.IsDead())
				{
					flag13 = Voxel.Raycast(this.gameManager.World, ray, num13, -555266077, num14, 0f);
				}
			}
		}
		else
		{
			num14 |= 256;
			int num15 = -555266077;
			num15 |= 268435456;
			if (!GameManager.bVolumeBlocksEditing)
			{
				num15 = int.MinValue;
			}
			flag13 = Voxel.RaycastOnVoxels(this.gameManager.World, ray, num13, num15, num14, 0f);
			if (flag13 && !GameManager.bVolumeBlocksEditing)
			{
				Voxel.voxelRayHitInfo.lastBlockPos = Vector3i.zero;
				Voxel.voxelRayHitInfo.hit.voxelData.Clear();
				Voxel.voxelRayHitInfo.hit.blockPos = Vector3i.zero;
			}
		}
		WorldRayHitInfo hitInfo2 = this.entityPlayerLocal.HitInfo;
		Vector3i zero = Vector3i.zero;
		Vector3i vector3i = Vector3i.zero;
		if (flag13)
		{
			hitInfo2.CopyFrom(Voxel.voxelRayHitInfo);
			vector3i = hitInfo2.hit.blockPos;
			Vector3i lastBlockPos = hitInfo2.lastBlockPos;
			hitInfo2.bHitValid = true;
		}
		else
		{
			hitInfo2.bHitValid = false;
		}
		if (!hitInfo2.hit.blockValue.isair)
		{
			Block block = hitInfo2.hit.blockValue.Block;
			if (!block.IsCollideMovement || block.CanBlocksReplace)
			{
				hitInfo2.lastBlockPos = vector3i;
			}
		}
		bool flag14 = true;
		bool flag15 = true;
		bool flag16 = playerInput.Primary.IsPressed && this.bAllowPlayerInput;
		bool flag17 = playerInput.Secondary.IsPressed && this.bAllowPlayerInput;
		if (flag16 && GameManager.Instance.World.IsEditor())
		{
			if (this.bIgnoreLeftMouseUntilReleased)
			{
				flag16 = false;
			}
		}
		else
		{
			this.bIgnoreLeftMouseUntilReleased = false;
		}
		bool flag18 = !this.windowManager.IsInputActive() && !this.windowManager.IsFullHUDDisabled() && (playerInput.Activate.IsPressed || playerInput.VehicleActions.Activate.IsPressed || playerInput.PermanentActions.Activate.IsPressed) && (playerInput.Activate.WasPressed || playerInput.VehicleActions.Activate.WasPressed || playerInput.PermanentActions.Activate.WasPressed);
		this.HandleInteraction(hitInfo2, vector3i, flag10, flag13, flag11, flag12, raycastHit, flag18);
		if (this.gameManager.IsEditMode() && flag16 && flag15 && !playerInput.Drop.IsPressed)
		{
			WorldRayHitInfo worldRayHitInfo = Voxel.voxelRayHitInfo.Clone();
			num14 = 325;
			int minValue = int.MinValue;
			if (Voxel.RaycastOnVoxels(this.gameManager.World, ray, 250f, minValue, num14, 0f) && SelectionBoxManager.Instance.Select(Voxel.voxelRayHitInfo))
			{
				flag15 = false;
				this.bIgnoreLeftMouseUntilReleased = true;
			}
			Voxel.voxelRayHitInfo.CopyFrom(worldRayHitInfo);
		}
		if (flag16 && (GameManager.Instance.World.IsEditor() || BlockToolSelection.Instance.SelectionActive))
		{
			flag16 &= !playerInput.Drop.IsPressed;
		}
		int num16 = playerInput.InventorySlotWasPressed;
		if (num16 >= 0)
		{
			if (playerInput.LastInputType == 1)
			{
				if (this.entityPlayerLocal.AimingGun)
				{
					num16 = -1;
				}
			}
			else if (InputUtils.ShiftKeyPressed && this.entityPlayerLocal.inventory.PUBLIC_SLOTS > this.entityPlayerLocal.inventory.SHIFT_KEY_SLOT_OFFSET)
			{
				num16 += this.entityPlayerLocal.inventory.SHIFT_KEY_SLOT_OFFSET;
			}
		}
		if (num16 == -1 && this.inventoryScrollPressed && this.inventoryScrollIdxToSelect != -1)
		{
			num16 = this.inventoryScrollIdxToSelect;
		}
		if (!flag2)
		{
			flag2 = this.entityPlayerLocal.inventory.holdingItem.ConsumeScrollWheel(this.entityPlayerLocal.inventory.holdingItemData, num, playerInput);
		}
		this.entityPlayerLocal.inventory.holdingItem.CheckKeys(this.entityPlayerLocal.inventory.holdingItemData, hitInfo2);
		ItemClass holdingItem = this.entityPlayerLocal.inventory.holdingItem;
		bool flag19 = (holdingItem.Actions[0] != null && holdingItem.Actions[0].AllowConcurrentActions()) || (holdingItem.Actions[1] != null && holdingItem.Actions[1].AllowConcurrentActions());
		bool flag20 = holdingItem.Actions[1] != null && holdingItem.Actions[1].IsActionRunning(this.entityPlayerLocal.inventory.holdingItemData.actionData[1]);
		bool flag21 = holdingItem.Actions[0] != null && holdingItem.Actions[0].IsActionRunning(this.entityPlayerLocal.inventory.holdingItemData.actionData[0]);
		if (flag15 && flag16 && !flag21 && (flag19 || !flag20) && this.entityPlayerLocal.inventory.GetIsFinishedSwitchingHeldItem())
		{
			if (this.gameManager.IsEditMode())
			{
				flag15 = !this.gameManager.GetActiveBlockTool().ExecuteAttackAction(this.entityPlayerLocal.inventory.holdingItemData, false, playerInput);
			}
			if (flag15)
			{
				this.entityPlayerLocal.inventory.Execute(0, false, playerInput);
			}
		}
		if (flag15 && playerInput.Primary.WasReleased && this.entityPlayerLocal.inventory.GetIsFinishedSwitchingHeldItem())
		{
			if (this.gameManager.IsEditMode() && !this.entityPlayerLocal.inventory.holdingItem.IsGun())
			{
				flag15 = !this.gameManager.GetActiveBlockTool().ExecuteAttackAction(this.entityPlayerLocal.inventory.holdingItemData, true, playerInput);
			}
			if (flag15)
			{
				this.entityPlayerLocal.inventory.Execute(0, true, playerInput);
			}
		}
		ItemAction itemAction = this.entityPlayerLocal.inventory.holdingItem.Actions[0];
		if (flag14 && flag17 && (flag19 || !flag21))
		{
			if (this.gameManager.IsEditMode())
			{
				flag14 = !this.gameManager.GetActiveBlockTool().ExecuteUseAction(this.entityPlayerLocal.inventory.holdingItemData, false, playerInput);
			}
			if (flag14)
			{
				this.entityPlayerLocal.inventory.Execute(1, false, playerInput);
			}
		}
		if (flag14 && playerInput.Secondary.WasReleased && this.entityPlayerLocal.inventory != null)
		{
			this.entityPlayerLocal.inventory.Execute(1, true, playerInput);
			if (this.gameManager.IsEditMode() && !this.entityPlayerLocal.inventory.holdingItem.IsGun())
			{
				this.gameManager.GetActiveBlockTool().ExecuteUseAction(this.entityPlayerLocal.inventory.holdingItemData, true, playerInput);
			}
		}
		if (playerInput.Drop.WasPressed)
		{
			this.DropHeldItem();
		}
		if (!playerInput.InventorySlotLeft.WasPressed && !playerInput.InventorySlotRight.WasPressed)
		{
			bool flag22 = this.inventoryScrollPressed;
		}
		this.inventoryScrollPressed = false;
		if (this.entityPlayerLocal.AttachedToEntity == null)
		{
			if (num16 != -1 && num16 != this.entityPlayerLocal.inventory.GetFocusedItemIdx() && num16 < this.entityPlayerLocal.inventory.PUBLIC_SLOTS && this.entityPlayerLocal.inventory != null && !this.entityPlayerLocal.CancellingInventoryActions)
			{
				if (this.canSwapHeldItem())
				{
					this.swapItem(this.entityPlayerLocal.inventory.SetFocusedItemIdx(num16));
				}
				else
				{
					this.entityPlayerLocal.inventory.SetFocusedItemIdx(num16);
					this.nextHeldItem.Add(num16);
				}
			}
			if ((playerInput.Reload.WasPressed || playerInput.PermanentActions.Reload.WasPressed) && this.entityPlayerLocal.inventory != null && this.entityPlayerLocal.inventory.GetIsFinishedSwitchingHeldItem())
			{
				bool flag23 = this.entityPlayerLocal.inventory.IsHoldingGun() || this.entityPlayerLocal.inventory.IsHoldingDynamicMelee();
				ItemAction holdingPrimary = this.entityPlayerLocal.inventory.GetHoldingPrimary();
				ItemAction holdingSecondary = this.entityPlayerLocal.inventory.GetHoldingSecondary();
				if (flag23 && holdingPrimary != null)
				{
					if (holdingPrimary.HasRadial())
					{
						this.timeActivatePressed = Time.time;
						this.playerUI.xui.RadialWindow.Open();
						holdingPrimary.SetupRadial(this.playerUI.xui.RadialWindow, this.entityPlayerLocal);
					}
					else
					{
						holdingPrimary.CancelAction(this.entityPlayerLocal.inventory.holdingItemData.actionData[0]);
						if (holdingSecondary != null && !(holdingSecondary is ItemActionSpawnTurret))
						{
							holdingSecondary.CancelAction(this.entityPlayerLocal.inventory.holdingItemData.actionData[1]);
						}
					}
				}
				else if (this.entityPlayerLocal.inventory.GetHoldingBlock() != null)
				{
					this.timeActivatePressed = Time.time;
					this.playerUI.xui.RadialWindow.Open();
					this.playerUI.xui.RadialWindow.SetupBlockShapeData();
				}
			}
			if (!flag2 && (playerInput.ToggleFlashlight.WasPressed || playerInput.PermanentActions.ToggleFlashlight.WasPressed))
			{
				this.timeActivatePressed = Time.time;
				this.playerUI.xui.RadialWindow.Open();
				this.playerUI.xui.RadialWindow.SetActivatableItemData(this.entityPlayerLocal);
			}
			if (!flag2 && (playerInput.Swap.WasPressed || playerInput.PermanentActions.Swap.WasPressed))
			{
				this.timeActivatePressed = Time.time;
				this.playerUI.xui.RadialWindow.Open();
				this.playerUI.xui.RadialWindow.SetupToolbeltMenu(0);
			}
			if (!flag2 && playerInput.InventorySlotRight.WasPressed)
			{
				this.timeActivatePressed = Time.time;
				this.playerUI.xui.RadialWindow.Open();
				this.playerUI.xui.RadialWindow.SetupToolbeltMenu(1);
			}
			if (!flag2 && playerInput.InventorySlotLeft.WasPressed)
			{
				this.timeActivatePressed = Time.time;
				this.playerUI.xui.RadialWindow.Open();
				this.playerUI.xui.RadialWindow.SetupToolbeltMenu(-1);
				return;
			}
		}
		else
		{
			EntityVehicle entityVehicle = this.entityPlayerLocal.AttachedToEntity as EntityVehicle;
			if (entityVehicle != null)
			{
				if (playerInput.PermanentActions.ToggleFlashlight.WasPressed || playerInput.VehicleActions.ToggleFlashlight.WasPressed)
				{
					if (entityVehicle.HasHeadlight())
					{
						entityVehicle.ToggleHeadlight();
					}
					else
					{
						this.timeActivatePressed = Time.time;
						this.playerUI.xui.RadialWindow.Open();
						this.playerUI.xui.RadialWindow.SetActivatableItemData(this.entityPlayerLocal);
					}
				}
				if (playerInput.VehicleActions.HonkHorn.WasPressed)
				{
					entityVehicle.UseHorn(this.entityPlayerLocal);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleInteraction(WorldRayHitInfo _hitInfo, Vector3i _blockPosInside, bool _bAlternativeBlockPos, bool _bHitWorldAndEntities, bool _bItemHit, bool _bBackpackHit, RaycastHit _raycastHitItems, bool _activate)
	{
		PlayerMoveController.<>c__DisplayClass106_0 CS$<>8__locals1;
		CS$<>8__locals1._hitInfo = _hitInfo;
		CS$<>8__locals1._blockPosInside = _blockPosInside;
		CS$<>8__locals1.<>4__this = this;
		CS$<>8__locals1._activate = _activate;
		float num = (_bItemHit ? _raycastHitItems.distance : 1000f);
		if (_bHitWorldAndEntities && GameUtils.IsBlockOrTerrain(CS$<>8__locals1._hitInfo.tag))
		{
			num -= 1.2f;
			if (num < 0f)
			{
				num = 0.1f;
			}
		}
		if (_bItemHit && (!_bHitWorldAndEntities || num * num <= CS$<>8__locals1._hitInfo.hit.distanceSq))
		{
			CS$<>8__locals1._hitInfo.bHitValid = true;
			CS$<>8__locals1._hitInfo.tag = "Item";
			CS$<>8__locals1._hitInfo.transform = _raycastHitItems.collider.transform;
			CS$<>8__locals1._hitInfo.hit.pos = _raycastHitItems.point;
			CS$<>8__locals1._hitInfo.hit.blockPos = World.worldToBlockPos(CS$<>8__locals1._hitInfo.hit.pos);
			CS$<>8__locals1._hitInfo.hit.distanceSq = _raycastHitItems.distance * _raycastHitItems.distance;
		}
		if (_bBackpackHit && _raycastHitItems.distance * _raycastHitItems.distance <= CS$<>8__locals1._hitInfo.hit.distanceSq)
		{
			CS$<>8__locals1._hitInfo.bHitValid = true;
			CS$<>8__locals1._hitInfo.tag = "E_BP_Body";
			CS$<>8__locals1._hitInfo.transform = _raycastHitItems.collider.transform;
			CS$<>8__locals1._hitInfo.hit.pos = _raycastHitItems.point;
			CS$<>8__locals1._hitInfo.hit.blockPos = World.worldToBlockPos(CS$<>8__locals1._hitInfo.hit.pos);
			CS$<>8__locals1._hitInfo.hit.distanceSq = _raycastHitItems.distance * _raycastHitItems.distance;
		}
		bool flag = true;
		EntityCollisionRules entityCollisionRules;
		if (CS$<>8__locals1._hitInfo.hitCollider && CS$<>8__locals1._hitInfo.hitCollider.TryGetComponent<EntityCollisionRules>(ref entityCollisionRules) && !entityCollisionRules.IsInteractable)
		{
			flag = false;
		}
		bool flag2 = this.entityPlayerLocal.TPCameraCheckResult == eTPCameraCheckResult.LineOfSightCheckFailed;
		CS$<>8__locals1.bMeshSelected = GameManager.Instance.IsEditMode() && this.entityPlayerLocal.HitInfo.transform != null && this.entityPlayerLocal.HitInfo.transform.gameObject.layer == 28;
		string text = null;
		bool flag3 = false;
		if (this.entityPlayerLocal.AttachedToEntity == null && flag && !flag2)
		{
			if (!this.<HandleInteraction>g__TryHandleBlock|106_0(out text, ref CS$<>8__locals1) && !this.<HandleInteraction>g__TryHandleItem|106_1(out text, ref CS$<>8__locals1) && !this.<HandleInteraction>g__TryHandleEntity|106_2(out text, ref CS$<>8__locals1))
			{
				this.InteractName = null;
				if (this.entityPlayerLocal.IsMoveStateStill() && (!this.entityPlayerLocal.IsSwimming() || this.entityPlayerLocal.cameraTransform.up.y < 0.7f))
				{
					this.InteractName = this.entityPlayerLocal.inventory.CanInteract();
					if (this.InteractName != null && this.InteractWaitTime == 0f)
					{
						this.InteractWaitTime = Time.time + 0.3f;
					}
				}
				if (this.InteractName != null && Time.time >= this.InteractWaitTime)
				{
					flag3 = true;
					PlayerActionsLocal playerInput = this.entityPlayerLocal.playerInput;
					string text2 = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
					text = string.Format(Localization.Get("ttPressTo", false, null), text2, Localization.Get(this.InteractName, false, null));
				}
				else if (this.InteractName == null)
				{
					this.InteractWaitTime = 0f;
				}
			}
			else
			{
				this.InteractWaitTime = 0f;
			}
		}
		if (CS$<>8__locals1._activate)
		{
			this.timeActivatePressed = Time.time;
			if (this.entityPlayerLocal.AttachedToEntity != null)
			{
				this.entityPlayerLocal.SendDetach();
			}
			else if (flag3)
			{
				this.entityPlayerLocal.inventory.Interact();
			}
		}
		if (this.entityPlayerLocal.IsAlive())
		{
			if (!string.Equals(text, this.strTextLabelPointingTo) && (Time.time - this.timeActivatePressed > 0.5f || string.IsNullOrEmpty(text)))
			{
				XUiC_InteractionPrompt.SetText(this.playerUI, text);
				this.strTextLabelPointingTo = text;
			}
		}
		else
		{
			this.strTextLabelPointingTo = "";
			XUiC_InteractionPrompt.SetText(this.playerUI, null);
		}
		this.FocusBoxPosition = CS$<>8__locals1._hitInfo.lastBlockPos;
		if (_bAlternativeBlockPos || (this.entityPlayerLocal.inventory != null && this.entityPlayerLocal.inventory.holdingItem.IsFocusBlockInside()))
		{
			this.FocusBoxPosition = CS$<>8__locals1._blockPosInside;
		}
		this.focusBoxScript.Update(CS$<>8__locals1.bMeshSelected, this.gameManager.World, CS$<>8__locals1._hitInfo, this.FocusBoxPosition, this.entityPlayerLocal, this.gameManager.persistentLocalPlayer, _bAlternativeBlockPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void swapItem(int inventoryItemToSet)
	{
		this.entityPlayerLocal.AimingGun = false;
		this.entityPlayerLocal.inventory.BeginSwapHoldingItem();
		base.StartCoroutine(this.entityPlayerLocal.CancelInventoryActions(delegate
		{
			Inventory inventory = this.entityPlayerLocal.inventory;
			if (inventory != null)
			{
				this.playerInput.Primary.ClearInputState();
				inventory.SetHoldingItemIdx(inventoryItemToSet);
			}
		}, true));
	}

	public void DropHeldItem()
	{
		if (!this.gameManager.IsEditMode() && !BlockToolSelection.Instance.SelectionActive && this.entityPlayerLocal.inventory != null && !this.entityPlayerLocal.inventory.IsHoldingItemActionRunning() && !this.entityPlayerLocal.inventory.IsHolsterDelayActive() && !this.entityPlayerLocal.inventory.IsUnholsterDelayActive() && this.entityPlayerLocal.inventory.holdingItemIdx != this.entityPlayerLocal.inventory.DUMMY_SLOT_IDX && !this.entityPlayerLocal.AimingGun)
		{
			Vector3 dropPosition = this.entityPlayerLocal.GetDropPosition();
			ItemValue holdingItemItemValue = this.entityPlayerLocal.inventory.holdingItemItemValue;
			if (ItemClass.GetForId(holdingItemItemValue.type).CanDrop(holdingItemItemValue) && this.entityPlayerLocal.inventory.holdingCount > 0 && this.entityPlayerLocal.DropTimeDelay <= 0f)
			{
				this.entityPlayerLocal.DropTimeDelay = 0.5f;
				int count = this.entityPlayerLocal.inventory.holdingItemStack.count;
				this.gameManager.ItemDropServer(this.entityPlayerLocal.inventory.holdingItemStack.Clone(), dropPosition, Vector3.zero, this.entityPlayerLocal.entityId, ItemClass.GetForId(holdingItemItemValue.type).GetLifetimeOnDrop(), false);
				this.entityPlayerLocal.AddUIHarvestingItem(new ItemStack(holdingItemItemValue, -count), false);
				Manager.BroadcastPlay(this.entityPlayerLocal, "itemdropped", false, 1f);
				this.entityPlayerLocal.inventory.DecHoldingItem(count);
			}
		}
	}

	public void SkipMouseLookNextFrame()
	{
		this.skipMouseLookNextFrame = 3;
	}

	public void SetControllableOverride(bool _b)
	{
		this.bCanControlOverride = _b;
	}

	public void Respawn(RespawnType _type)
	{
		this.gameManager.World.GetPrimaryPlayer().Spawned = false;
		this.respawnReason = _type;
		switch (_type)
		{
		case RespawnType.NewGame:
			this.respawnTime = Constants.cRespawnEnterGameTime;
			return;
		case RespawnType.LoadedGame:
			this.respawnTime = 0f;
			return;
		case RespawnType.Died:
			this.respawnTime = Constants.cRespawnAfterDeathTime;
			return;
		default:
			return;
		}
	}

	public void AllowPlayerInput(bool allow)
	{
		this.bAllowPlayerInput = allow;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawChunkBoundary()
	{
		Vector3i vector3i = World.toChunkXYZCube(this.entityPlayerLocal.position);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				for (int k = -1; k <= 1; k++)
				{
					string text = string.Format("PlayerChunk{0},{1},{2}", k, i, j);
					Vector3 vector;
					vector.x = (float)((vector3i.x + k) * 16);
					vector.y = (float)((vector3i.y + i) * 16);
					vector.z = (float)((vector3i.z + j) * 16);
					Vector3 vector2 = vector;
					vector2.x += 16f;
					vector2.y += 16f;
					vector2.z += 16f;
					DebugLines debugLines;
					if (k == 0 && i == 0 && j == 0)
					{
						debugLines = DebugLines.Create(text, this.entityPlayerLocal.RootTransform, new Color(1f, 1f, 1f), new Color(1f, 1f, 1f), 0.1f, 0.1f, 0.1f);
					}
					else
					{
						debugLines = DebugLines.Create(text, this.entityPlayerLocal.RootTransform, new Color(0.3f, 0.3f, 0.3f), new Color(0.3f, 0.3f, 0.3f), 0.033f, 0.033f, 0.1f);
					}
					debugLines.AddCube(vector, vector2);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawChunkDensities()
	{
		Vector3i vector3i = World.toChunkXYZCube(this.entityPlayerLocal.position);
		vector3i *= 16;
		IChunk chunkFromWorldPos = this.entityPlayerLocal.world.GetChunkFromWorldPos(World.worldToBlockPos(this.entityPlayerLocal.position));
		if (chunkFromWorldPos == null)
		{
			return;
		}
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					int num = i + vector3i.y;
					int density = (int)chunkFromWorldPos.GetDensity(k, num, j);
					if (density != (int)MarchingCubes.DensityAir && density != (int)MarchingCubes.DensityTerrain)
					{
						float num2 = 0f;
						if (num > 0)
						{
							sbyte density2 = chunkFromWorldPos.GetDensity(k, num - 1, j);
							num2 = MarchingCubes.GetDecorationOffsetY((sbyte)density, density2);
						}
						string text = string.Format("PlayerDensity{0},{1},{2}", k, i, j);
						Vector3 vector;
						vector.x = (float)(vector3i.x + k) + 0.5f - 0.5f;
						vector.y = (float)num;
						vector.z = (float)(vector3i.z + j) + 0.5f - 0.5f;
						Vector3 vector2 = vector;
						vector2.x += 1f;
						vector2.y += 0.5f + num2;
						vector2.z += 1f;
						DebugLines debugLines;
						if (density > 0)
						{
							float num3 = 1f - (float)density / 127f;
							Color color;
							color..ctor(0.2f, 0.2f, num3);
							debugLines = DebugLines.Create(text, this.entityPlayerLocal.RootTransform, color, color, 0.005f, 0.005f, 0.1f);
						}
						else
						{
							float num4 = (float)(-(float)density) / 128f;
							Color color2;
							color2..ctor(num4, num4, 0.2f);
							debugLines = DebugLines.Create(text, this.entityPlayerLocal.RootTransform, color2, color2, 0.01f, 0.01f, 0.1f);
						}
						debugLines.AddCube(vector, vector2);
					}
				}
			}
		}
	}

	public void FindCameraSnapTarget(eCameraSnapMode snapMode, float maxDistance)
	{
		PlayerMoveController.cameraSnapTargets.Clear();
		GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityAlive), new Bounds(this.entityPlayerLocal.position, Vector3.one * maxDistance), PlayerMoveController.cameraSnapTargets);
		float num = float.MaxValue;
		EntityAlive entityAlive = null;
		if (PlayerMoveController.cameraSnapTargets.Count > 0)
		{
			foreach (Entity entity in PlayerMoveController.cameraSnapTargets)
			{
				EntityAlive entityAlive2 = (EntityAlive)entity;
				if (!(entityAlive2 == this.entityPlayerLocal) && entityAlive2.IsValidAimAssistSnapTarget && entityAlive2.IsAlive() && !(entityAlive2.ModelTransform == null))
				{
					Vector3 vector = entityAlive2.GetChestTransformPosition() - this.entityPlayerLocal.cameraTransform.position;
					float sqrMagnitude = vector.sqrMagnitude;
					float num2 = Vector3.Angle(this.entityPlayerLocal.cameraTransform.forward, vector.normalized);
					if (snapMode == eCameraSnapMode.Zoom)
					{
						float num3 = 15f * (1f - this.targetSnapFalloffCurve.Evaluate(sqrMagnitude / 50f));
						if (num2 > num3)
						{
							continue;
						}
					}
					else if (num2 > 20f)
					{
						continue;
					}
					if (this.entityPlayerLocal.HitInfo.transform && this.entityPlayerLocal.HitInfo.transform.IsChildOf(entityAlive2.ModelTransform) && sqrMagnitude < num)
					{
						entityAlive = entityAlive2;
						break;
					}
					RaycastHit raycastHit;
					if (sqrMagnitude < num && Physics.Raycast(new Ray(this.entityPlayerLocal.cameraTransform.position, vector), ref raycastHit, maxDistance, -538751005) && ((entityAlive2.PhysicsTransform != null && raycastHit.collider.transform.IsChildOf(entityAlive2.PhysicsTransform)) || raycastHit.collider.transform.IsChildOf(entityAlive2.ModelTransform)))
					{
						num = sqrMagnitude;
						entityAlive = entityAlive2;
					}
				}
			}
			this.SetCameraSnapEntity(entityAlive, snapMode);
		}
	}

	public void SetCameraSnapEntity(EntityAlive _target, eCameraSnapMode _snapMode)
	{
		this.cameraSnapTargetEntity = _target;
		this.cameraSnapMode = _snapMode;
		if (this.cameraSnapTargetEntity != null)
		{
			Vector2 vector = Vector2.one * 0.5f;
			Vector2 vector2 = this.entityPlayerLocal.playerCamera.WorldToViewportPoint(this.cameraSnapTargetEntity.GetChestTransformPosition());
			Vector2 vector3 = this.entityPlayerLocal.playerCamera.WorldToViewportPoint(this.cameraSnapTargetEntity.emodel.GetHeadTransform().position);
			Vector2 vector4 = vector2 - vector;
			this.snapTargetingHead = (vector3 - vector).sqrMagnitude < vector4.sqrMagnitude;
			this.cameraSnapTime = Time.time;
		}
	}

	public void ForceStopRunning()
	{
		this.entityPlayerLocal.movementInput.running = false;
		this.runToggleActive = false;
	}

	public void SetInventoryIdxFromScroll(int _idx)
	{
		this.inventoryScrollPressed = true;
		this.inventoryScrollIdxToSelect = _idx;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <HandleInteraction>g__TryHandleBlock|106_0(out string _tooltip, ref PlayerMoveController.<>c__DisplayClass106_0 A_2)
	{
		_tooltip = null;
		if (!A_2._hitInfo.bHitValid || !(A_2.bMeshSelected |= GameUtils.IsBlockOrTerrain(A_2._hitInfo.tag)))
		{
			return false;
		}
		int activationDistanceSq = A_2._hitInfo.hit.blockValue.Block.GetActivationDistanceSq();
		if (A_2._hitInfo.hit.distanceSq >= (float)activationDistanceSq)
		{
			return false;
		}
		BlockValue blockValue = A_2._hitInfo.hit.blockValue;
		Block block = blockValue.Block;
		BlockValue blockValue2 = blockValue;
		Vector3i vector3i = A_2._blockPosInside;
		if (blockValue2.ischild && block != null && block.multiBlockPos != null)
		{
			vector3i = block.multiBlockPos.GetParentPos(vector3i, blockValue2);
			blockValue2 = this.gameManager.World.GetBlock(vector3i);
		}
		if (block.HasBlockActivationCommands(this.gameManager.World, blockValue2, vector3i, this.entityPlayerLocal))
		{
			if (A_2._activate)
			{
				this.entityPlayerLocal.vp_FPCamera.AlignCharacterToCamera();
				this.playerUI.xui.RadialWindow.Open();
				this.playerUI.xui.RadialWindow.SetCurrentBlockData(this.gameManager.World, vector3i, blockValue2, this.entityPlayerLocal);
			}
			PlayerActionsLocal playerInput = this.entityPlayerLocal.playerInput;
			string text = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
			_tooltip = block.GetActivationText(this.gameManager.World, blockValue2, vector3i, this.entityPlayerLocal);
			if (_tooltip != null)
			{
				_tooltip = string.Format(_tooltip, text);
			}
			else if (block.CustomCmds.Length != 0)
			{
				string localizedBlockName = blockValue2.Block.GetLocalizedBlockName();
				_tooltip = string.Format(Localization.Get("questBlockActivate", false, null), text, localizedBlockName);
				_tooltip = string.Format(_tooltip, text);
			}
			return true;
		}
		if (block.DisplayInfo == Block.EnumDisplayInfo.Name)
		{
			_tooltip = block.GetLocalizedBlockName();
			return true;
		}
		if (block.DisplayInfo == Block.EnumDisplayInfo.Description)
		{
			_tooltip = Localization.Get(block.DescriptionKey, false, null);
			return true;
		}
		if (block.DisplayInfo == Block.EnumDisplayInfo.Custom)
		{
			_tooltip = block.GetCustomDescription(vector3i, blockValue);
			return true;
		}
		return false;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <HandleInteraction>g__TryHandleItem|106_1(out string _tooltip, ref PlayerMoveController.<>c__DisplayClass106_0 A_2)
	{
		_tooltip = null;
		if (!A_2._hitInfo.bHitValid || !A_2._hitInfo.tag.Equals("Item") || A_2._hitInfo.hit.distanceSq >= Constants.cCollectItemDistance * Constants.cCollectItemDistance)
		{
			return false;
		}
		Entity entity = A_2._hitInfo.transform.GetComponent<EntityItem>();
		RootTransformRefEntity component;
		if (entity == null && (component = A_2._hitInfo.transform.GetComponent<RootTransformRefEntity>()) != null && component.RootTransform != null)
		{
			entity = component.RootTransform.GetComponent<EntityItem>();
		}
		if (!(entity != null))
		{
			return this.<HandleInteraction>g__TryHandleProjectile|106_3(A_2._hitInfo.transform, out _tooltip, ref A_2);
		}
		if (!entity.HasEnabledActivationCommands(this.entityPlayerLocal))
		{
			return false;
		}
		if (A_2._activate)
		{
			this.entityPlayerLocal.vp_FPCamera.AlignCharacterToCamera();
			this.entityPlayerLocal.AimingGun = false;
			this.playerUI.xui.RadialWindow.Open();
			this.playerUI.xui.RadialWindow.SetCurrentEntityData(entity, this.entityPlayerLocal);
		}
		_tooltip = entity.GetActivationText();
		return true;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <HandleInteraction>g__TryHandleEntity|106_2(out string _tooltip, ref PlayerMoveController.<>c__DisplayClass106_0 A_2)
	{
		_tooltip = null;
		if (!A_2._hitInfo.bHitValid || !A_2._hitInfo.tag.StartsWith("E_") || A_2._hitInfo.hit.distanceSq >= Constants.cCollectItemDistance * Constants.cCollectItemDistance)
		{
			return false;
		}
		Transform hitRootTransform = GameUtils.GetHitRootTransform(A_2._hitInfo.tag, A_2._hitInfo.transform);
		if (hitRootTransform == null)
		{
			return false;
		}
		Entity component = hitRootTransform.GetComponent<Entity>();
		if (component == null)
		{
			return false;
		}
		bool flag;
		if (!component.IsDead())
		{
			EntityPlayer entityPlayer = component as EntityPlayer;
			if (entityPlayer != null)
			{
				Inventory inventory = entityPlayer.inventory;
				if (((inventory != null) ? inventory.holdingItem : null) != null)
				{
					flag = !entityPlayer.inventory.holdingItem.HasAnyTags(PlayerMoveController.BowTag);
					goto IL_00C0;
				}
			}
		}
		flag = true;
		IL_00C0:
		if (flag && this.<HandleInteraction>g__TryHandleProjectile|106_3(hitRootTransform, out _tooltip, ref A_2))
		{
			return true;
		}
		if (!component.HasEnabledActivationCommands(this.entityPlayerLocal))
		{
			return false;
		}
		if (A_2._activate)
		{
			this.entityPlayerLocal.vp_FPCamera.AlignCharacterToCamera();
			this.entityPlayerLocal.AimingGun = false;
			this.playerUI.xui.RadialWindow.Open();
			this.playerUI.xui.RadialWindow.SetCurrentEntityData(component, this.entityPlayerLocal);
		}
		_tooltip = component.GetActivationText();
		return true;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <HandleInteraction>g__TryHandleProjectile|106_3(Transform _transform, out string _tooltip, ref PlayerMoveController.<>c__DisplayClass106_0 A_3)
	{
		_tooltip = null;
		int num = -1;
		ProjectileMoveScript componentInChildren = _transform.GetComponentInChildren<ProjectileMoveScript>();
		if (componentInChildren != null)
		{
			if (A_3._activate)
			{
				componentInChildren.TryCollect(this.entityPlayerLocal);
			}
			num = componentInChildren.itemValueProjectile.type;
		}
		else
		{
			ThrownWeaponMoveScript componentInChildren2 = _transform.GetComponentInChildren<ThrownWeaponMoveScript>();
			if (componentInChildren2 != null)
			{
				if (A_3._activate)
				{
					componentInChildren2.TryCollect(this.entityPlayerLocal);
				}
				num = componentInChildren2.itemValueWeapon.type;
			}
		}
		if (num < 0)
		{
			return false;
		}
		if (A_3._activate)
		{
			this.entityPlayerLocal.vp_FPCamera.AlignCharacterToCamera();
		}
		PlayerActionsLocal playerInput = this.entityPlayerLocal.playerInput;
		string localizedItemName = ItemClass.GetForId(num).GetLocalizedItemName();
		string text = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		_tooltip = string.Format(Localization.Get("itemTooltipFocusedOne", false, null), text, localizedItemName);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bAllowPlayerInput = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static FastTags<TagGroup.Global> BowTag = FastTags<TagGroup.Global>.Parse("bow");

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static PlayerMoveController Instance = null;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<NGuiAction> globalActions = new List<NGuiAction>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 mouseLookSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float mouseZoomSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float zoomAccel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float vehicleLookSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 controllerLookSensitivity;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public GameManager gameManager;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public NGuiWdwInGameHUD guiInGame;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bCanControlOverride;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bLastRespawnActive;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float respawnTime;

	public RespawnType respawnReason;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly StateHistory<RespawnProgress> respawnStateHistory = new StateHistory<RespawnProgress>(16, true, false);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool spawnWindowOpened;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public EntityPlayerLocal entityPlayerLocal;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public LocalPlayerUI playerUI;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public GUIWindowManager windowManager;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public NGUIWindowManager nguiWindowManager;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int invertMouse;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int invertController;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bControllerVibration;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public RenderDisplacedCube focusBoxScript;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string strTextLabelPointingTo;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float inventoryItemSwitchTimeout;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int inventoryItemToSetAfterTimeout = int.MinValue;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public PlayerAutoPilotControllor playerAutoPilotControllor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string InteractName;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float InteractWaitTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timeActivatePressed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bIgnoreLeftMouseUntilReleased;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int skipMouseLookNextFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool wasCameraChangeUsedWithWheel;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float cameraChangePressTime = -1f;

	public int drawChunkMode;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public AnimationCurve lookAccelerationCurve;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lookAccelerationRate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float currentLookAcceleration;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float controllerZoomSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float controllerVehicleSensitivity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool controllerAimAssistsEnabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cSprintModeHold = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cSprintModeToggle = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cSprintModeAutorun = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int sprintMode;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float aimAssistSlowAmount = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bAimAssistTargetingItem;

	public Vector3i FocusBoxPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityAlive cameraSnapTargetEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool snapTargetingHead;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public eCameraSnapMode cameraSnapMode;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float cameraSnapTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<Entity> cameraSnapTargets = new List<Entity>();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public AnimationCurve targetSnapFalloffCurve;

	public Action toggleGodMode;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Action teleportPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cGuiClosedInputWaitUpdates = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool wasGuiClosedThisUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool guiOpenLastUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool guiOpenThisUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int guiClosedUpdateCount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool wasVehicle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool runToggleActive;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float runInputTime;

	public bool isAutorun;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isAutorunInvalid;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool inventoryScrollPressed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int inventoryScrollIdxToSelect = -1;

	public bool cameraChangeRequested;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public SpawnPosition spawnPosition = SpawnPosition.Undef;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool waitingForSpawnPointSelection;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool wasUIInputActive;

	public static bool useScaledMouseLook = false;

	public static float lookDeltaTimeScale = 100f;

	public static float mouseDeltaTimeScale = 75f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 previousMouseInput = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool runPressedWhileActive;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<int> nextHeldItem = new List<int>();
}
