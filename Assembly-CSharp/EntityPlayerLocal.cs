using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using DynamicMusic;
using DynamicMusic.Factories;
using InControl;
using Platform;
using SandboxOptions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting;

[Preserve]
public class EntityPlayerLocal : EntityPlayer, IInventoryChangedListener, IGamePrefsChangedListener
{
	public event Action InventoryChangedEvent;

	public PlayerActionsLocal playerInput
	{
		get
		{
			return PlatformManager.NativePlatform.Input.PrimaryPlayer;
		}
	}

	public static bool PermaDegrationOn
	{
		get
		{
			return EntityPlayerLocal.DegradeOnDeathType == EntityPlayerLocal.DegradeOnDeathTypes.MaxDurability || EntityPlayerLocal.DegradeOnDeathType == EntityPlayerLocal.DegradeOnDeathTypes.Both || ItemAction.ItemMaxDegrationAmount > 0f;
		}
	}

	public Vector3 LookPoint
	{
		get
		{
			return this.vp_FPCamera.LookPoint;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkedGetFPController()
	{
		if (!this.PhysicsTransform)
		{
			return;
		}
		if (!this.m_checked_vp_FPController || this.m_vp_FPController == null)
		{
			this.m_checked_vp_FPController = true;
			this.m_vp_FPController = this.PhysicsTransform.GetComponent<vp_FPController>();
			Transform transform = this.PhysicsTransform.Find("Camera");
			if (transform != null)
			{
				this.m_vp_FPCamera = transform.GetComponent<vp_FPCamera>();
			}
			this.m_vp_FPPlayerEventHandler = this.PhysicsTransform.GetComponent<vp_FPPlayerEventHandler>();
		}
		if (this.m_vp_FPWeapon == null)
		{
			this.m_vp_FPWeapon = this.PhysicsTransform.GetComponentInChildren<vp_FPWeapon>();
			if (this.m_vp_FPWeapon != null && this.emodel is EModelSDCS)
			{
				this.m_vp_FPWeapon.DefaultState.Preset.SetFieldValue("PositionOffset", new Vector3(0f, -1.7f, 0.02f));
				this.m_vp_FPWeapon.DefaultState.Preset.SetFieldValue("RenderingFieldOfView", 45);
			}
		}
	}

	public vp_FPController vp_FPController
	{
		get
		{
			this.checkedGetFPController();
			return this.m_vp_FPController;
		}
	}

	public vp_FPCamera vp_FPCamera
	{
		get
		{
			this.checkedGetFPController();
			return this.m_vp_FPCamera;
		}
	}

	public vp_FPWeapon vp_FPWeapon
	{
		get
		{
			this.checkedGetFPController();
			return this.m_vp_FPWeapon;
		}
	}

	public vp_PlayerEventHandler vp_PlayerEventHandler
	{
		get
		{
			this.checkedGetFPController();
			return this.m_vp_FPPlayerEventHandler;
		}
	}

	public void ShowHoldingItemLayer(bool show)
	{
		if (this.playerCamera)
		{
			if (show)
			{
				this.playerCamera.cullingMask |= 1024;
				return;
			}
			this.playerCamera.cullingMask &= -1025;
		}
	}

	public event Action DragAndDropItemChanged;

	public ItemStack DragAndDropItem
	{
		get
		{
			return this.dragAndDropItem;
		}
		set
		{
			this.dragAndDropItem = value;
			this.DragAndDropItemChanged();
		}
	}

	public PlayerMoveController MoveController
	{
		get
		{
			return this.moveController;
		}
	}

	public LocalPlayerUI PlayerUI
	{
		get
		{
			return this.playerUI;
		}
	}

	public void MakeAttached(bool bAttached)
	{
		this.isLadderAttached = bAttached;
	}

	public bool InAir
	{
		get
		{
			return this.inAir;
		}
		set
		{
			if (this.inAir != value)
			{
				this.inAir = value;
				this.emodel.avatarController.SetInAir(this.inAir);
			}
		}
	}

	public bool CameraRelativeMovement
	{
		get
		{
			return this._cameraRelativeMovement;
		}
		set
		{
			if (this._cameraRelativeMovement != value)
			{
				this._cameraRelativeMovement = value;
				this.vp_PlayerEventHandler.CameraRelativeMovement3P.Set(value);
			}
		}
	}

	public void StartTPCameraLockTimer()
	{
		if (this.bFirstPersonView)
		{
			this.tpCameraLockTimerActive = false;
			return;
		}
		if (this.vp_FPCamera.Locked3rdPerson)
		{
			return;
		}
		this.CameraRelativeMovement = false;
		this.tpCameraLockStartTime = Time.time;
		this.tpCameraLockTimerActive = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		this.playerUI = LocalPlayerUI.GetUIForPlayer(this);
		this.windowManager = this.playerUI.windowManager;
		this.nguiWindowManager = this.playerUI.nguiWindowManager;
		this.QuestJournal.OwnerPlayer = this;
		this.challengeJournal = new ChallengeJournal();
		this.challengeJournal.Player = this;
		base.Awake();
		this.dragAndDropItem = ItemStack.Empty;
		this.isEntityRemote = false;
		this.world.AddLocalPlayer(this);
		this.cameraContainerTransform = base.transform;
		this.cameraTransform = this.cameraContainerTransform.Find("Camera");
		this.playerCamera = this.cameraTransform.GetComponent<Camera>();
		this.finalCamera = this.playerCamera;
		this.ScreenEffectManager = NGUITools.AddMissingComponent<ScreenEffects>(this.cameraTransform.gameObject);
		Transform transform = this.cameraTransform.Find("ScreenEffectsWithDepth");
		if (transform != null)
		{
			this.uwEffectHaze = transform.Find("UnderwaterHaze");
		}
		this.uwEffectRefract = this.cameraTransform.Find("effect_refract_plane");
		this.uwEffectDebris = this.cameraTransform.Find("effect_underwater_debris");
		this.uwEffectDroplets = this.cameraTransform.Find("effect_dropletsParticle");
		this.uwEffectWaterFade = this.cameraTransform.Find("effect_water_fade");
		this.audioSourceBiomeActive = this.cameraTransform.gameObject.AddComponent<AudioSource>();
		this.audioSourceBiomeFadeOut = this.cameraTransform.gameObject.AddComponent<AudioSource>();
		this.overlayMaterial = new Material(Shader.Find("Game/UI/Screen Overlay"));
		this.CameraDOFInit();
		Shader.SetGlobalFloat("_UnderWater", 0f);
		this.renderManager = GameRenderManager.Create(this);
		this.bPlayingSpawnIn = true;
		this.spawnInTime = Time.time;
		SkyManager.SetFogDebug(-1f, float.MinValue, float.MinValue);
		WeatherManager.Instance.PushTransitions();
		this.ThreatLevel = Factory.CreateThreatLevel();
		this.ScreenEffectManager.SetScreenEffect("VibrantDeSat", 1f, 4f);
		GameManager.Instance.triggerEffectManager.EnableVibration();
		this.moveController = base.GetComponent<PlayerMoveController>();
		this.MoveController.Init();
		MumblePositionalAudio instance = SingletonMonoBehaviour<MumblePositionalAudio>.Instance;
		if (instance != null)
		{
			instance.SetPlayer(this);
		}
		this.lastWaypointUpdateTime = Time.time - 30f;
		this.characterMatrixOverride = base.transform.Find("Graphics/Model").gameObject.AddComponent<CharacterMatrixOverride>();
		this.characterMatrixOverride.Init(this);
	}

	public virtual Vector2 OnValue_InputMoveVector
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.m_MoveVector;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.m_MoveVector = (this.MovementRunning ? value.normalized : value);
		}
	}

	public virtual Vector2 OnValue_InputSmoothLook
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.m_SmoothLook;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.m_SmoothLook = value;
		}
	}

	public virtual Vector2 OnValue_InputRawLook
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.m_SmoothLook;
		}
	}

	public virtual Vector3 OnValue_CameraLookDirection
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.GetLookVector();
		}
	}

	public override void Init(int _entityClass, EntityInstanceAssets _assets, EModelInstanceAssets _eModelAssets)
	{
		base.Init(_entityClass, _assets, _eModelAssets);
		Transform modelTransform = this.emodel.GetModelTransform();
		for (int i = 0; i < modelTransform.childCount; i++)
		{
			Transform child = modelTransform.GetChild(i);
			for (int j = 0; j < child.childCount; j++)
			{
				if (child.GetChild(j).GetComponent<Renderer>() is SkinnedMeshRenderer)
				{
					((SkinnedMeshRenderer)child.GetChild(j).GetComponent<Renderer>()).updateWhenOffscreen = true;
				}
			}
		}
		this.SetFirstPersonView(true, false);
		this.bPreferFirstPerson = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxDefaultFirstPersonCamera);
		this.cameraDistanceMulti = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxCameraDistance3P);
		this.IsGodMode.Value = !GameStats.GetBool(EnumGameStats.IsPlayerDamageEnabled);
		this.IsGodMode.OnChangeDelegates += this.OnWeatherGodModeChanged;
		this.IsNoCollisionMode.Value = this.IsGodMode.Value;
		if (this.m_characterController != null)
		{
			this.PhysicsTransform.gameObject.layer = 20;
			this.m_characterController.enableOverlapRecovery = true;
		}
		if (this.vp_FPController != null)
		{
			this.vp_FPController.localPlayer = this;
			this.vp_FPController.Player.Register(this);
			this.vp_FPController.Player.FallImpact2.Register(this, "FallImpact", 0);
			this.vp_FPController.SyncCharacterController();
			this.vp_FPController.enabled = false;
		}
		GamePrefs.AddChangeListener(this);
		this.CameraRelativeMovement = true;
	}

	public void OnGamePrefChanged(EnumGamePrefs _enum)
	{
		if (_enum == EnumGamePrefs.OptionsGfxCameraDistance3P)
		{
			this.cameraDistanceMulti = GamePrefs.GetFloat(_enum);
		}
	}

	public bool NACommand(List<string> args)
	{
		if (args == null)
		{
			return false;
		}
		int count = args.Count;
		if (count > 0)
		{
			if (args[0] == "create")
			{
				if (count == 1)
				{
					return this.NAInit();
				}
			}
			else if (args[0] == "equip")
			{
				if (count != 1)
				{
					return this.NAEquip(args[1]);
				}
				return this.NAListEquipment();
			}
			else if (args[0] == "unequip")
			{
				if (count != 1)
				{
					return this.NAUnEquip(args[1]);
				}
			}
			else if (args[0] == "rot_x")
			{
				if (count != 1)
				{
					return this.NARotateX(args[1]);
				}
			}
			else
			{
				if (args[0] == "help")
				{
					this.NAHelp();
					return true;
				}
				if (args[0] == "parts")
				{
					return this.NAListParts();
				}
			}
		}
		this.NAHelp();
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerEquippedSlots _GetNASlots()
	{
		return base.transform.GetComponent<PlayerEquippedSlots>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform _GetNAOutfit()
	{
		return base.transform.Find("Graphics/Model/base");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NAHelp()
	{
		Log.Warning("New avatar command help.");
		Log.Warning("------------------------");
		Log.Warning("na create                Create new avatar.");
		Log.Warning("na equip                 List current equipment.");
		Log.Warning("na equip {partname}      Equip a part.");
		Log.Warning("na help                  This help.");
		Log.Warning("na parts                 List available parts.");
		Log.Warning("na rot_x {degrees}       Turn player (0 faces away, 180 towards).");
		Log.Warning("na unequip {partname}    Unequip a part.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool NAInit()
	{
		Log.Warning("New avatar init.");
		Transform transform = base.transform.Find("Graphics/Model");
		if (transform == null)
		{
			Log.Error("Entity does not have 'Graphics/Model' node!");
			return false;
		}
		if (transform.Find("base") != null)
		{
			return false;
		}
		Transform transform2 = DataLoader.LoadAsset<Transform>("@:Entities/Player/Male/maleTestPrefab.prefab", false);
		if (transform2 == null)
		{
			return false;
		}
		transform2 = Object.Instantiate<Transform>(transform2, transform);
		transform2.name = "base";
		transform2.localPosition = new Vector3(0f, 0f, 1f);
		transform2.localRotation = Quaternion.Euler(0f, 135f, 0f);
		PlayerEquippedSlots playerEquippedSlots = this._GetNASlots();
		if (playerEquippedSlots == null)
		{
			return false;
		}
		playerEquippedSlots.Init(this._GetNAOutfit());
		this.NAEquip("baseHead");
		this.NAEquip("baseBody");
		this.NAEquip("baseHands");
		this.NAEquip("baseFeet");
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool NAListParts()
	{
		PlayerEquippedSlots playerEquippedSlots = this._GetNASlots();
		if (playerEquippedSlots == null)
		{
			return false;
		}
		playerEquippedSlots.ListParts();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool NAListEquipment()
	{
		PlayerEquippedSlots playerEquippedSlots = this._GetNASlots();
		if (playerEquippedSlots == null)
		{
			return false;
		}
		playerEquippedSlots.ListEquipment();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool NAEquip(string partName)
	{
		Log.Warning("New avatar equip {0}", new object[] { partName });
		PlayerEquippedSlots playerEquippedSlots = this._GetNASlots();
		return !(playerEquippedSlots == null) && playerEquippedSlots.Equip(partName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool NAUnEquip(string partName)
	{
		Log.Warning("New avatar unequip {0}", new object[] { partName });
		PlayerEquippedSlots playerEquippedSlots = this._GetNASlots();
		return !(playerEquippedSlots == null) && playerEquippedSlots.UnEquip(partName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool NARotateX(string value)
	{
		Transform transform = this._GetNAOutfit();
		if (transform == null)
		{
			return false;
		}
		transform.localRotation = Quaternion.Euler(0f, Convert.ToSingle(value), 0f);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _IsEquipped(string partName)
	{
		PlayerEquippedSlots playerEquippedSlots = this._GetNASlots();
		return !(playerEquippedSlots == null) && playerEquippedSlots.IsEquipped(partName);
	}

	public override void PostInit()
	{
		base.PostInit();
		this.inventory.AddChangeListener(this);
		this.inventory.OnToolbeltItemsChangedInternal += this.callInventoryChanged;
		this.bag.OnBackpackItemsChangedInternal += this.callInventoryChanged;
		this.equipment.OnChanged += this.callInventoryChanged;
		this.DragAndDropItemChanged += this.callInventoryChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void callInventoryChanged()
	{
		if (this.InventoryChangedEvent != null)
		{
			this.InventoryChangedEvent();
		}
	}

	public override void OnAddedToWorld()
	{
		base.OnAddedToWorld();
	}

	public override void OnEntityUnload()
	{
		base.OnEntityUnload();
		this.InventoryChangedEvent = null;
		GamePrefs.RemoveChangeListener(this);
		if (this.QuestJournal != null)
		{
			this.QuestJournal.UnHookQuests();
		}
		if (this.challengeJournal != null)
		{
			this.challengeJournal.EndChallenges();
		}
		this.inventory.Cleanup();
		this.bag.Clear();
		this.renderManager.Destroy();
		this.renderManager = null;
		if (this.cameraTransform.parent == null)
		{
			Object.Destroy(this.cameraTransform.gameObject);
		}
		GameManager.Instance.World.RemoveLocalPlayer(this);
		if (this.ScreenEffectManager)
		{
			this.ClearScreenEffects();
		}
		this.playerUI = null;
		this.windowManager = null;
		this.nguiWindowManager = null;
		this.moveController = null;
	}

	public void OnInventoryChanged(Inventory _inventory)
	{
	}

	public bool IsMoveStateStill()
	{
		return this.moveState == EntityPlayerLocal.MoveState.Idle || this.moveState == EntityPlayerLocal.MoveState.Crouch || (this.moveState == EntityPlayerLocal.MoveState.Swim && !this.IsSwimmingMoving());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMoveStateToDefault()
	{
		this.SwimModeStop();
		if (!this.m_vp_FPController.enabled)
		{
			return;
		}
		vp_FPPlayerEventHandler player = this.m_vp_FPController.Player;
		if (base.IsCrouching)
		{
			if (this.MovementRunning && !player.Zoom.Active)
			{
				this.SetMoveState(EntityPlayerLocal.MoveState.CrouchRun, false);
				return;
			}
			if (player.InputMoveVector.Get() != Vector2.zero && this.m_vp_FPController.Velocity.sqrMagnitude > 0.01f)
			{
				this.SetMoveState(EntityPlayerLocal.MoveState.CrouchWalk, false);
				return;
			}
			this.SetMoveState(EntityPlayerLocal.MoveState.Crouch, false);
			return;
		}
		else
		{
			if (this.MovementRunning && !player.Zoom.Active)
			{
				this.SetMoveState(EntityPlayerLocal.MoveState.Run, false);
				return;
			}
			if (player.InputMoveVector.Get() != Vector2.zero && this.m_vp_FPController.Velocity.sqrMagnitude > 0.01f)
			{
				this.SetMoveState(EntityPlayerLocal.MoveState.Walk, false);
				return;
			}
			this.SetMoveState(EntityPlayerLocal.MoveState.Idle, false);
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMoveState(EntityPlayerLocal.MoveState _state, bool _isOverride = false)
	{
		vp_FPPlayerEventHandler player = this.m_vp_FPController.Player;
		bool aimingGun = this.AimingGun;
		int value = this.inventory.holdingItem.HoldType.Value;
		bool flag = (value == 27 || value == 53 || value == 68) && this.SpecialAttack;
		if (_state != this.moveState)
		{
			EntityPlayerLocal.MoveState moveState = this.moveState;
			if (moveState - EntityPlayerLocal.MoveState.Crouch <= 2 && _state != EntityPlayerLocal.MoveState.Crouch && _state != EntityPlayerLocal.MoveState.CrouchWalk && _state != EntityPlayerLocal.MoveState.CrouchRun && !_isOverride)
			{
				if (!this.vp_FPCamera.HasOverheadSpace && !this.IsGodMode.Value)
				{
					_state = this.moveState;
				}
				else if (player.Crouch.Active && !player.Crouch.TryStop(true))
				{
					_state = this.moveState;
				}
				if (_state != this.moveState)
				{
					this.FireEvent(MinEventTypes.onSelfStand, true);
				}
			}
		}
		bool flag2 = _state != this.moveState;
		if (!flag2 && aimingGun == this.moveStateAiming && flag == this.moveStateHoldBow)
		{
			return;
		}
		this.m_vp_FPController.MotorDamping = 0.346f;
		this.m_vp_FPController.PhysicsSlopeSlideLimit = 60f;
		this.m_vp_FPController.PhysicsCrouchHeightModifier = 0.7f;
		this.SetMoveStateWeaponDamping(0.08f, 0.75f);
		if (this.m_vp_FPWeapon != null)
		{
			this.m_vp_FPWeapon.RotationLookSway = new Vector3(0.25f, 0.17f, 0f);
			this.m_vp_FPWeapon.RetractionDistance = 0.1f;
			this.m_vp_FPWeapon.BobRate = new Vector4(0.9f, 0.45f, 0f, 0f);
			this.m_vp_FPWeapon.BobAmplitude = new Vector4(0.35f, 0.5f, 0f, 0f);
			this.m_vp_FPWeapon.BobInputVelocityScale = 1f;
			this.m_vp_FPWeapon.ShakeSpeed = 0f;
			this.m_vp_FPWeapon.ShakeAmplitude = new Vector3(0.25f, 0f, 2f);
		}
		if (this.m_vp_FPCamera != null)
		{
			this.m_vp_FPCamera.RotationKneeling = 0.05f;
		}
		switch (_state)
		{
		case EntityPlayerLocal.MoveState.Off:
			if (this.m_vp_FPController.enabled)
			{
				player.Crouch.Stop(0f);
				player.Jump.Stop(0f);
				this.m_vp_FPController.Stop();
				this.m_vp_FPController.enabled = false;
			}
			this.SwimModeStop();
			break;
		case EntityPlayerLocal.MoveState.Attached:
			player.Crouch.Stop(0f);
			player.Jump.Stop(0f);
			this.SwimModeStop();
			break;
		case EntityPlayerLocal.MoveState.Idle:
			this.m_vp_FPController.MotorAcceleration = 0.12f;
			this.m_vp_FPController.MotorBackwardsSpeed = 0.8f;
			this.m_vp_FPController.MotorSidewaysSpeed = 0.8f;
			this.m_vp_FPController.MotorSlopeSpeedDown = 1.2f;
			this.m_vp_FPController.MotorSlopeSpeedUp = 0.8f;
			if (flag2 && this.moveState != EntityPlayerLocal.MoveState.Walk)
			{
				this.FireEvent(MinEventTypes.onSelfWalk, true);
			}
			break;
		case EntityPlayerLocal.MoveState.Walk:
			this.m_vp_FPController.MotorAcceleration = 0.12f;
			this.m_vp_FPController.MotorBackwardsSpeed = 0.8f;
			this.m_vp_FPController.MotorSidewaysSpeed = 0.8f;
			this.m_vp_FPController.MotorSlopeSpeedDown = 1.2f;
			this.m_vp_FPController.MotorSlopeSpeedUp = 0.8f;
			this.SetMoveStateWeapon();
			if (this.m_vp_FPCamera != null)
			{
				this.m_vp_FPCamera.RotationKneeling = 0.065f;
			}
			if (flag2 && this.moveState != EntityPlayerLocal.MoveState.Idle)
			{
				this.FireEvent(MinEventTypes.onSelfWalk, true);
			}
			break;
		case EntityPlayerLocal.MoveState.Run:
			this.m_vp_FPController.MotorAcceleration = 0.35f;
			this.m_vp_FPController.MotorBackwardsSpeed = 0.8f;
			this.m_vp_FPController.MotorSidewaysSpeed = 0.5f;
			this.m_vp_FPController.MotorSlopeSpeedDown = 1.2f;
			this.m_vp_FPController.MotorSlopeSpeedUp = 0.8f;
			this.SetMoveStateWeapon();
			if (this.m_vp_FPWeapon != null)
			{
				this.m_vp_FPWeapon.BobRate = new Vector4(2f, 1f, 0f, 0f);
				this.m_vp_FPWeapon.BobAmplitude = new Vector4(1.5f, 1.2f, 0f, 0f);
			}
			if (this.m_vp_FPCamera != null)
			{
				this.m_vp_FPCamera.RotationKneeling = 0.075f;
			}
			if (flag2)
			{
				this.FireEvent(MinEventTypes.onSelfRun, true);
			}
			break;
		case EntityPlayerLocal.MoveState.Crouch:
			player.Crouch.Start(0f);
			this.m_vp_FPController.MotorAcceleration = 0.08f;
			this.m_vp_FPController.MotorBackwardsSpeed = 1f;
			this.m_vp_FPController.MotorSidewaysSpeed = 1f;
			this.m_vp_FPController.MotorSlopeSpeedDown = 1f;
			this.m_vp_FPController.MotorSlopeSpeedUp = 1f;
			if (flag2 && this.moveState != EntityPlayerLocal.MoveState.CrouchWalk && this.moveState != EntityPlayerLocal.MoveState.CrouchRun)
			{
				this.FireEvent(MinEventTypes.onSelfCrouch, true);
			}
			break;
		case EntityPlayerLocal.MoveState.CrouchWalk:
			player.Crouch.Start(0f);
			this.m_vp_FPController.MotorAcceleration = 0.08f;
			this.m_vp_FPController.MotorBackwardsSpeed = 1f;
			this.m_vp_FPController.MotorSidewaysSpeed = 1f;
			this.m_vp_FPController.MotorSlopeSpeedDown = 1f;
			this.m_vp_FPController.MotorSlopeSpeedUp = 1f;
			if (flag2)
			{
				this.FireEvent(MinEventTypes.onSelfCrouchWalk, true);
			}
			break;
		case EntityPlayerLocal.MoveState.CrouchRun:
			player.Crouch.Start(0f);
			this.m_vp_FPController.MotorAcceleration = 0.11f;
			this.m_vp_FPController.MotorBackwardsSpeed = 1f;
			this.m_vp_FPController.MotorSidewaysSpeed = 1f;
			this.m_vp_FPController.MotorSlopeSpeedDown = 1f;
			this.m_vp_FPController.MotorSlopeSpeedUp = 1f;
			if (flag2)
			{
				this.FireEvent(MinEventTypes.onSelfCrouchRun, true);
			}
			break;
		}
		if (flag)
		{
			this.SetMoveStateWeaponDamping(0.04f, 0.5f);
			if (this.m_vp_FPWeapon != null)
			{
				this.m_vp_FPWeapon.RotationLookSway = new Vector3(0.02f, 0.02f, 0f);
				this.m_vp_FPWeapon.RetractionDistance = 0f;
				this.m_vp_FPWeapon.BobAmplitude = new Vector4(0.1f, 0.05f, 0f, 0f);
				this.m_vp_FPWeapon.BobInputVelocityScale = 1f;
			}
		}
		if (aimingGun != this.moveStateAiming)
		{
			if (aimingGun)
			{
				this.FireEvent(MinEventTypes.onSelfAimingGunStart, true);
			}
			else
			{
				this.FireEvent(MinEventTypes.onSelfAimingGunStop, true);
			}
		}
		if (aimingGun)
		{
			this.SetMoveStateWeaponDamping(0.5f, 0.9f);
			if (this.m_vp_FPWeapon != null)
			{
				this.m_vp_FPWeapon.RotationLookSway = new Vector3(0.3f, 0.21f, 0f);
				this.m_vp_FPWeapon.RetractionDistance = 0f;
				this.m_vp_FPWeapon.ShakeSpeed = 0f;
				this.m_vp_FPWeapon.ShakeAmplitude = Vector3.zero;
				this.m_vp_FPWeapon.BobAmplitude = new Vector4(0.035f, 0.05f, 0f, 0f);
			}
		}
		this.m_vp_FPCamera.Refresh();
		if (this.m_vp_FPWeapon != null)
		{
			this.m_vp_FPWeapon.Refresh();
		}
		this.moveState = _state;
		this.moveStateAiming = aimingGun;
		this.moveStateHoldBow = flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMoveStateWeapon()
	{
		this.SetMoveStateWeaponDamping(0.01f, 0.25f);
		if (this.m_vp_FPWeapon != null)
		{
			this.m_vp_FPWeapon.RotationLookSway = new Vector3(0.3f, 0.21f, 0f);
			this.m_vp_FPWeapon.RetractionDistance = 0f;
			this.m_vp_FPWeapon.BobAmplitude = new Vector4(0.25f, 0.15f, 0f, 0f);
			this.m_vp_FPWeapon.BobInputVelocityScale = 100f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMoveStateWeaponDamping(float _stiffness, float _damping)
	{
		if (this.m_vp_FPWeapon != null)
		{
			this.m_vp_FPWeapon.PositionSpringStiffness = _stiffness;
			this.m_vp_FPWeapon.PositionSpringDamping = _damping;
			this.m_vp_FPWeapon.PositionPivotSpringStiffness = _stiffness;
			this.m_vp_FPWeapon.PositionPivotSpringDamping = _damping;
			this.m_vp_FPWeapon.RotationSpringStiffness = _stiffness;
			this.m_vp_FPWeapon.RotationSpringDamping = _damping;
			this.m_vp_FPWeapon.RotationPivotSpringStiffness = _stiffness;
			this.m_vp_FPWeapon.RotationPivotSpringDamping = _damping;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsMoveStateCrouch()
	{
		return this.moveState == EntityPlayerLocal.MoveState.Crouch || this.moveState == EntityPlayerLocal.MoveState.CrouchWalk || this.moveState == EntityPlayerLocal.MoveState.CrouchRun;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CalcIfSwimming()
	{
		float inWaterPercent = this.inWaterPercent;
		if (this.swimClimbing)
		{
			return inWaterPercent >= 0.04f && !this.onGround;
		}
		if (this.IsMoveStateCrouch())
		{
			return inWaterPercent >= 0.7f;
		}
		return inWaterPercent >= 0.6f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SwimModeTick()
	{
		vp_FPPlayerEventHandler player = this.m_vp_FPController.Player;
		this.SetMoveState(EntityPlayerLocal.MoveState.Swim, false);
		if (this.swimMode < 0)
		{
			this.swimMode = 1;
			this.swimExhaustedTicks = 0;
			this.swimClimbing = false;
			this.FireEvent(MinEventTypes.onSelfSwimStart, true);
			this.emodel.avatarController.SetSwim(true);
			this.m_vp_FPController.ScaleFallSpeed(0.2f);
		}
		this.m_vp_FPController.MotorFreeFly = true;
		this.m_vp_FPController.MotorJumpForce = 0f;
		this.m_vp_FPController.MotorJumpForceDamping = 0f;
		this.m_vp_FPController.MotorJumpForceHold = 0f;
		this.m_vp_FPController.MotorJumpForceHoldDamping = 1f;
		this.m_vp_FPController.MotorAcceleration = 0.00032f;
		if (!this.Jumping && !this.inputWasDown && player.InputMoveVector.Get().SqrMagnitude() < 0.001f)
		{
			this.m_vp_FPController.PhysicsGravityModifier = 0.003f;
			if (this.swimMode != 0)
			{
				this.swimMode = 0;
				this.FireEvent(MinEventTypes.onSelfSwimIdle, true);
			}
		}
		else
		{
			this.m_vp_FPController.PhysicsGravityModifier = 0f;
			if (this.MovementRunning)
			{
				this.m_vp_FPController.MotorAcceleration = 0.0024f;
				if (this.swimMode != 2)
				{
					this.swimMode = 2;
					this.FireEvent(MinEventTypes.onSelfSwimRun, true);
				}
			}
			else if (this.swimMode != 1)
			{
				this.swimMode = 1;
			}
		}
		if (this.Stamina <= 0f)
		{
			this.swimExhaustedTicks = 60;
		}
		if (this.swimExhaustedTicks > 0)
		{
			this.swimExhaustedTicks--;
			this.m_vp_FPController.PhysicsGravityModifier = 0.004f;
			if (!this.isHeadUnderwater)
			{
				this.m_vp_FPController.PhysicsGravityModifier = 0.08f;
			}
			this.m_vp_FPController.MotorAcceleration = 0.00025f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsSwimmingMoving()
	{
		return this.swimMode > 0;
	}

	public void SwimModeUpdateThrottle()
	{
		float timeScale = Time.timeScale;
		float num = timeScale;
		if (this.swimExhaustedTicks > 0)
		{
			num *= 0.45f;
		}
		float num2 = 0.79f;
		float num3 = 0f;
		this.swimClimbing = false;
		if (this.inputWasJump && (this.vp_FPCamera.HasOverheadSpace || this.IsGodMode.Value))
		{
			if (this.onGround)
			{
				vp_FPController vp_FPController = this.m_vp_FPController;
				vp_FPController.m_MotorThrottle.y = vp_FPController.m_MotorThrottle.y + 0.05f * num;
			}
			else
			{
				bool flag = false;
				if (this.swimExhaustedTicks == 0 && this.moveDirection.z > 0f)
				{
					Vector3 hipPosition = this.getHipPosition();
					Vector3 forwardVector = base.GetForwardVector();
					forwardVector.y = -0.25f;
					Ray ray;
					ray..ctor(hipPosition, forwardVector);
					float num4 = this.position.y + 0.12f;
					for (float num5 = hipPosition.y + 0.3f; num5 > num4; num5 -= 0.16f)
					{
						hipPosition.y = num5;
						ray.origin = hipPosition;
						if (Voxel.Raycast(this.world, ray, 0.45f, 1073807360, 65, 0.165f) && Voxel.phyxRaycastHit.normal.y > 0.3f)
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					this.swimClimbing = true;
					num2 = 0.1f;
					num3 = 0.02f;
				}
				else
				{
					vp_FPController vp_FPController2 = this.m_vp_FPController;
					vp_FPController2.m_MotorThrottle.y = vp_FPController2.m_MotorThrottle.y + 0.00052f * num;
				}
			}
		}
		else if (this.inputWasDown)
		{
			vp_FPController vp_FPController3 = this.m_vp_FPController;
			vp_FPController3.m_MotorThrottle.y = vp_FPController3.m_MotorThrottle.y + -0.00038f * num;
		}
		Vector3 lookVector = this.GetLookVector();
		float num6 = this.m_vp_FPController.MotorAcceleration * num;
		this.m_vp_FPController.m_MotorThrottle += lookVector * (this.moveDirection.z * num6);
		this.m_vp_FPController.m_MotorThrottle += base.transform.TransformDirection(Vector3.right) * (this.moveDirection.x * num6 * 0.7f);
		float num7 = 0.01f + Mathf.Pow(this.m_vp_FPController.m_MotorThrottle.magnitude * 5.4f, 2f);
		this.m_vp_FPController.m_MotorThrottle /= 1f + num7 * timeScale;
		if (this.swimClimbing)
		{
			this.m_vp_FPController.m_MotorThrottle.y = num3;
		}
		if (this.inWaterPercent < num2 && !base.IsInElevator() && this.m_vp_FPController.m_MotorThrottle.y > 0f)
		{
			vp_FPController vp_FPController4 = this.m_vp_FPController;
			vp_FPController4.m_MotorThrottle.y = vp_FPController4.m_MotorThrottle.y * 0.5f;
			if (this.inWaterPercent < num2 - 0.04f)
			{
				this.m_vp_FPController.m_MotorThrottle.y = 0f;
			}
		}
		this.m_vp_FPController.m_MotorThrottle = vp_MathUtility.SnapToZero(this.m_vp_FPController.m_MotorThrottle, 2E-05f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SwimModeStop()
	{
		if (this.swimMode >= 0)
		{
			this.swimMode = -1;
			this.FireEvent(MinEventTypes.onSelfSwimStop, true);
			this.emodel.avatarController.SetSwim(false);
			this.m_vp_FPController.MotorFreeFly = false;
			this.m_vp_FPController.PhysicsGravityModifier = 0.2f;
			this.m_vp_FPController.MotorJumpForce = 0.13f;
			this.m_vp_FPController.MotorJumpForceDamping = 0.08f;
			this.m_vp_FPController.MotorJumpForceHold = 0.003f;
			this.m_vp_FPController.MotorJumpForceHoldDamping = 0.5f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void StartJumpSwimMotion()
	{
	}

	public override void PhysicsPush(Vector3 forceVec, Vector3 forceWorldPos, bool affectLocalPlayerController = false)
	{
		if (this.IsGodMode.Value)
		{
			return;
		}
		if (affectLocalPlayerController && this.vp_FPController != null)
		{
			this.vp_FPController.AddForce(forceVec);
			return;
		}
		base.PhysicsPush(forceVec, forceWorldPos, affectLocalPlayerController);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void onSpawnStateChanged()
	{
		base.onSpawnStateChanged();
		if (this.vp_FPController && (!this.Spawned || this.moveController.respawnReason != RespawnType.Teleport || !this.IsFlyMode.Value))
		{
			this.vp_FPController.enabled = this.Spawned;
		}
		if (this.Spawned && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && PrefabEditModeManager.Instance.IsActive())
		{
			PrefabEditModeManager.Instance.LoadRecentlyUsedOrCreateNew();
		}
	}

	public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
	{
		vp_FPController vp_FPController = this.vp_FPController;
		bool flag = vp_FPController != null && !this.IsStuck && !this.IsFlyMode.Value;
		bool flag2 = true;
		bool flag3 = false;
		if (this.onGround)
		{
			this.isLadderAttached = false;
			this.canLadderAirAttach = true;
		}
		if (this.isLadderAttached)
		{
			this.speedStrafe = 0f;
		}
		bool flag4 = this.jumpTrigger;
		bool flag5 = flag4 != this.wasJumpTrigger;
		this.wasJumpTrigger = this.jumpTrigger;
		if (base.IsInElevator() && !this.IsFlyMode.Value)
		{
			flag3 = true;
			bool flag6 = false;
			if (flag4)
			{
				if (flag5)
				{
					if (this.isLadderAttached)
					{
						this.isLadderAttached = false;
						this.canLadderAirAttach = false;
						this.wasLadderAttachedJump = true;
					}
					else
					{
						this.canLadderAirAttach = true;
						this.wasLadderAttachedJump = false;
					}
				}
			}
			else
			{
				this.wasLadderAttachedJump = false;
			}
			if (!this.isLadderAttached)
			{
				if (vp_FPController != null)
				{
					bool flag7 = (vp_FPController.enabled ? vp_FPController.Grounded : this.onGround);
					if ((flag7 || this.isSwimming) && vp_FPController.IsCollidingWall && vp_FPController.ProjectedWallMove < 0.3f && _direction.z > 0f)
					{
						flag6 = true;
					}
					else if (!flag7 && this.canLadderAirAttach)
					{
						float y = vp_FPController.Velocity.y;
						if (y <= 0f)
						{
							if (y >= -3f)
							{
								this.isLadderAttached = true;
								this.wasLadderAttachedJump = false;
							}
							else
							{
								vp_FPController.ScaleFallSpeed(0.75f);
							}
						}
					}
				}
				else if (this.onGround && this.isCollidedHorizontally && this.projectedMove < 0.3f && _direction.z > 0f)
				{
					flag6 = true;
				}
				else if (!this.onGround)
				{
					this.isLadderAttached = true;
				}
			}
			Vector3 cameraLook = this.GetCameraLook(1f);
			if (flag6 && cameraLook.y > 0.1f)
			{
				this.isLadderAttached = true;
			}
			if (this.isLadderAttached)
			{
				this.SetMoveState(EntityPlayerLocal.MoveState.Off, true);
				float num = (this.MovementRunning ? 0.17f : 0.06f);
				num *= this.GetSpeedModifier();
				if (_direction.x != 0f || _direction.z > 0f)
				{
					Vector3 vector = _direction;
					if (vector.z < 0f)
					{
						vector.z = 0f;
					}
					float num2 = num * 0.65f;
					this.Move(vector, _isDirAbsolute, num2, num2);
				}
				if (this.motion.x < -0.11f)
				{
					this.motion.x = -0.11f;
				}
				if (this.motion.x > 0.11f)
				{
					this.motion.x = 0.11f;
				}
				if (this.motion.z < -0.11f)
				{
					this.motion.z = -0.11f;
				}
				if (this.motion.z > 0.11f)
				{
					this.motion.z = 0.11f;
				}
				cameraLook.y += 0.15f;
				cameraLook.y *= 2f;
				cameraLook.y = Mathf.Clamp(cameraLook.y, -1f, 1f);
				this.motion.y = _direction.z * cameraLook.y * num;
				this.fallDistance = 0f;
				this.entityCollision(this.motion);
				this.motion *= base.ScalePhysicsMulConstant(0.545f);
				this.distanceClimbed += this.motion.magnitude;
				if (this.distanceClimbed > 0.5f)
				{
					base.internalPlayStepSound(1f);
					this.distanceClimbed = 0f;
				}
			}
			flag2 = !this.isLadderAttached;
		}
		else
		{
			this.isLadderAttached = false;
		}
		if (flag && (!flag3 || flag2))
		{
			vp_FPController.enabled = true;
			this.motion = Vector3.zero;
			this.world.CheckEntityCollisionWithBlocks(this);
			if (vp_FPController.Grounded)
			{
				Transform groundTransform = vp_FPController.GroundTransform;
				if (groundTransform && groundTransform.CompareTag("LargeEntityBlocker"))
				{
					Vector2 randomOnUnitCircle = this.rand.RandomOnUnitCircle;
					vp_FPController.AddForce(randomOnUnitCircle.x * 0.008f, 0f, randomOnUnitCircle.y * 0.008f);
				}
			}
			flag2 = false;
		}
		if (flag2)
		{
			bool flag8 = base.IsInElevator();
			base.SetInElevator(false);
			base.MoveEntityHeaded(_direction, false);
			base.SetInElevator(flag8);
		}
	}

	public void SetCameraAttachedToPlayer(bool _b, bool _lockCamera)
	{
		if (_b)
		{
			this.cameraTransform.SetParent(this.cameraContainerTransform, false);
			this.cameraTransform.SetAsFirstSibling();
			this.cameraTransform.SetLocalPositionAndRotation(Constants.cDefaultCameraPlayerOffset, Quaternion.identity);
			this.vp_FPCamera.Locked3rdPerson = false;
			this.movementInput.bDetachedCameraMove = false;
			return;
		}
		this.cameraTransform.parent = null;
		this.vp_FPCamera.Locked3rdPerson = _lockCamera;
	}

	public bool IsCameraAttachedToPlayerOrScope()
	{
		return this.cameraTransform.parent != null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckNonSolidVertical(Vector3i blockPos, int maxY, int verticalSpace)
	{
		for (int i = 0; i < maxY; i++)
		{
			if (!this.world.GetBlock(blockPos.x, blockPos.y + i + 1, blockPos.z).Block.shape.IsSolidSpace)
			{
				bool flag = true;
				for (int j = 1; j < verticalSpace; j++)
				{
					if (this.world.GetBlock(blockPos.x, blockPos.y + i + 1 + j, blockPos.z).Block.shape.IsSolidSpace)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTransform()
	{
		if (this.AttachedToEntity != null)
		{
			return;
		}
		if (this.m_vp_FPController != null)
		{
			if (this.m_vp_FPController.enabled)
			{
				this.position = this.vp_FPController.SmoothPosition + Origin.position;
			}
			else
			{
				float elapsedPartialTicks = GameTimer.Instance.elapsedPartialTicks;
				if (elapsedPartialTicks < 1f)
				{
					base.transform.position = this.lastTickPos[0] + (this.position - this.lastTickPos[0]) * elapsedPartialTicks - Origin.position;
				}
				else
				{
					base.transform.position = this.position - Origin.position;
				}
				this.vp_FPController.SetPosition(base.transform.position);
			}
			this.rotation = this.PhysicsTransform.eulerAngles;
			if (this.vp_FPCamera != null)
			{
				this.rotation.x = -this.vp_FPCamera.Angle.x;
			}
			float num = base.width / 2f;
			float num2 = base.depth / 2f;
			this.boundingBox = BoundsUtils.BoundsForMinMax(this.position.x - num, this.position.y - this.yOffset + this.ySize, this.position.z - num2, this.position.x + num, this.position.y - this.yOffset + this.ySize + base.height, this.position.z + num2);
			return;
		}
		base.updateTransform();
	}

	public override void SetRotation(Vector3 _rot)
	{
		base.SetRotation(_rot);
		if (this.PhysicsTransform && !this.emodel.IsRagdollActive)
		{
			this.PhysicsTransform.eulerAngles = _rot;
		}
		if (this.m_vp_FPCamera)
		{
			this.m_vp_FPCamera.Angle = new Vector2(-_rot.x, _rot.y);
		}
	}

	public override void OnUpdatePosition(float _partialTicks)
	{
		if (GameManager.bPlayRecordedSession && this.Spawned)
		{
			PlayerInputRecordingSystem.Instance.Play(this, true);
		}
		if (this.m_vp_FPController != null)
		{
			this.ticksExisted++;
			this.prevPos = this.position;
			this.prevRotation = this.rotation;
			if (this.Spawned)
			{
				Vector3 vector = Vector3.zero;
				for (int i = 0; i < this.lastTickPos.Length - 1; i++)
				{
					vector += this.lastTickPos[i] - this.lastTickPos[i + 1];
				}
				vector /= (float)(this.lastTickPos.Length - 1);
				float num = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z);
				this.UpdateDistanceTravelledAchievement(num);
				if (this.AttachedToEntity == null)
				{
					this.updateStepSound(vector.x, vector.z, 0f);
					base.updatePlayerLandSound(num, vector.y);
				}
				else
				{
					this.distanceWalked += num;
				}
			}
			this.updateSpeedForwardAndStrafe(this.m_vp_FPController.Velocity, _partialTicks);
			base.ReplicateSpeeds();
		}
		else
		{
			base.OnUpdatePosition(_partialTicks);
		}
		if (this.Spawned)
		{
			if (this.position.y >= 2f && this.position.y < 4f)
			{
				IAchievementManager achievementManager = PlatformManager.NativePlatform.AchievementManager;
				if (achievementManager != null)
				{
					achievementManager.SetAchievementStat(EnumAchievementDataStat.DepthAchieved, 1);
				}
			}
			else if (this.position.y >= 255f)
			{
				IAchievementManager achievementManager2 = PlatformManager.NativePlatform.AchievementManager;
				if (achievementManager2 != null)
				{
					achievementManager2.SetAchievementStat(EnumAchievementDataStat.HeightAchieved, 1);
				}
			}
		}
		GameSenseManager instance = GameSenseManager.Instance;
		if (instance != null)
		{
			instance.UpdateEventCompass(this.rotation.y);
		}
		if (GameManager.bRecordNextSession && this.Spawned)
		{
			PlayerInputRecordingSystem.Instance.Record(this, GameTimer.Instance.ticks);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateDistanceTravelledAchievement(float distanceTravelled)
	{
		float num = distanceTravelled / 1000f;
		this.achievementDistanceAccu += num;
		if ((double)this.achievementDistanceAccu > 0.05)
		{
			IAchievementManager achievementManager = PlatformManager.NativePlatform.AchievementManager;
			if (achievementManager != null)
			{
				achievementManager.SetAchievementStat(EnumAchievementDataStat.KMTravelled, this.achievementDistanceAccu);
			}
			this.achievementDistanceAccu -= 0.05f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateSpeedForwardAndStrafe(Vector3 _dist, float _partialTicks)
	{
		this.speedForward = 0f;
		this.speedStrafe = 0f;
		this.speedVertical = 0f;
		if (this.isLadderAttached)
		{
			this.speedForward += _dist.y;
			this.speedStrafe = 1234f;
			this.speedVertical = 0f;
		}
		else
		{
			if (Mathf.Abs(_dist.x) > 0.001f || Mathf.Abs(_dist.z) > 0.001f)
			{
				Vector3 vector = base.transform.InverseTransformDirection(_dist).normalized * _dist.magnitude;
				this.speedForward = (float)((int)(vector.z * 100f)) / 100f;
				this.speedStrafe = (float)((int)(vector.x * 100f)) / 100f;
			}
			if (Mathf.Abs(_dist.y) > 0.001f)
			{
				this.speedVertical += _dist.y;
			}
		}
		this.SetMovementState();
	}

	public override void OnUpdateEntity()
	{
		if (this.DropTimeDelay > 0f)
		{
			this.DropTimeDelay -= 0.05f;
		}
		float value = base.Stats.Health.Value;
		float num = (this.oldHealth - value) / base.Stats.Health.Max;
		float time = Time.time;
		if (value != this.oldHealth || time > this.vibrationTimeout)
		{
			if (GamePrefs.GetInt(EnumGamePrefs.OptionsControllerVibrationStrength) > 0 && this.playerInput != null && this.moveController.GetControllerVibration())
			{
				InputDevice inputDevice = this.playerInput.Device;
				if (inputDevice == null && this.playerInput.LastInputType == 1)
				{
					inputDevice = InputManager.ActiveDevice;
				}
				if (inputDevice != null)
				{
					if (this.oldHealth > value)
					{
						if (value <= 0f)
						{
							inputDevice.Vibrate(0.5f);
							GameManager.Instance.triggerEffectManager.SetGamepadVibration(0.5f);
						}
						else if (num > 0.25f)
						{
							inputDevice.Vibrate(0.5f);
							GameManager.Instance.triggerEffectManager.SetGamepadVibration(0.5f);
						}
						else if (num > 0.1f)
						{
							inputDevice.Vibrate(0.35f);
							GameManager.Instance.triggerEffectManager.SetGamepadVibration(0.35f);
						}
						else
						{
							inputDevice.Vibrate(0.25f);
							GameManager.Instance.triggerEffectManager.SetGamepadVibration(0.25f);
						}
						this.vibrationTimeout = Time.time + 0.25f;
					}
					else
					{
						inputDevice.StopVibration();
						GameManager.Instance.triggerEffectManager.StopGamepadVibration(false);
						this.vibrationTimeout = float.MaxValue;
					}
				}
			}
			if (num > 0.02f)
			{
				GameSenseManager instance = GameSenseManager.Instance;
				if (instance != null)
				{
					instance.UpdateEventHit();
				}
			}
			GameSenseManager instance2 = GameSenseManager.Instance;
			if (instance2 != null)
			{
				instance2.UpdateEventHealth((int)(base.Stats.Health.ValuePercentUI * 100f));
			}
			this.oldHealth = value;
		}
		this.equipment.Update();
		base.OnUpdateEntity();
		this.WeatherStatusTick();
	}

	public override void OnUpdateLive()
	{
		if (this.IsSpawned())
		{
			if (Time.time - this.updateBedrollPositionChecks > 5f)
			{
				this.updateBedrollPositionChecks = Time.time;
				if (!this.CheckSpawnPointStillThere())
				{
					this.RemoveSpawnPoints(true);
				}
			}
			if (Time.time - this.updateRadiationChecks > 5f)
			{
				this.updateRadiationChecks = Time.time;
				IChunkProvider chunkProvider = this.world.ChunkCache.ChunkProvider;
				IBiomeProvider biomeProvider;
				if (chunkProvider != null && (biomeProvider = chunkProvider.GetBiomeProvider()) != null)
				{
					float radiationAt = biomeProvider.GetRadiationAt((int)this.position.x, (int)this.position.z);
					this.Buffs.SetCustomVar("_biomeradiation", radiationAt, true, CVarOperation.set);
				}
			}
			GameEventManager.Current.UpdateCurrentBossGroup(this);
		}
		if (this.AttachedToEntity != null)
		{
			this.SetMoveState(EntityPlayerLocal.MoveState.Attached, true);
			base.OnUpdateLive();
			if (!this.isEntityRemote)
			{
				this.BlockRadiusEffectsTick();
			}
			this.IsStuck = false;
			return;
		}
		bool isStuck = this.IsStuck;
		this.IsStuck = false;
		if (!this.IsFlyMode.Value)
		{
			float num = this.boundingBox.min.y + 0.5f;
			this.IsStuck = this.pushOutOfBlocks(this.position.x - base.width * 0.3f, num, this.position.z + base.depth * 0.3f);
			this.IsStuck = this.pushOutOfBlocks(this.position.x - base.width * 0.3f, num, this.position.z - base.depth * 0.3f) || this.IsStuck;
			this.IsStuck = this.pushOutOfBlocks(this.position.x + base.width * 0.3f, num, this.position.z - base.depth * 0.3f) || this.IsStuck;
			this.IsStuck = this.pushOutOfBlocks(this.position.x + base.width * 0.3f, num, this.position.z + base.depth * 0.3f) || this.IsStuck;
			if (!this.IsStuck)
			{
				int num2 = Utils.Fastfloor(this.position.x);
				int num3 = Utils.Fastfloor(num);
				int num4 = Utils.Fastfloor(this.position.z);
				if (this.shouldPushOutOfBlock(num2, num3, num4, true))
				{
					if (!this.shouldPushOutOfBlock(num2 - 1, num3, num4, true))
					{
						this.IsStuck = true;
						this.motion = new Vector3(-0.25f, 0f, 0f);
					}
					else if (!this.shouldPushOutOfBlock(num2 + 1, num3, num4, true))
					{
						this.IsStuck = true;
						this.motion = new Vector3(0.25f, 0f, 0f);
					}
					if (!this.shouldPushOutOfBlock(num2, num3, num4 - 1, true))
					{
						this.IsStuck = true;
						this.motion = new Vector3(0f, 0f, -0.25f);
					}
					else if (!this.shouldPushOutOfBlock(num2, num3, num4 + 1, true))
					{
						this.IsStuck = true;
						this.motion = new Vector3(0f, 0f, 0.25f);
					}
					else if (this.CheckNonSolidVertical(new Vector3i(num2, num3 + 1, num4), 4, 2))
					{
						this.IsStuck = true;
						this.motion = new Vector3(0f, 1.6f, 0f);
						Log.Warning("{0} Player is stuck, trying to unstick", new object[] { Time.frameCount });
					}
				}
			}
		}
		bool flag = true;
		bool flag2 = false;
		bool flag3 = this.InAir;
		this.InAir = !this.isLadderAttached && !this.onGround;
		if (!this.wasJumping && !this.jumpTrigger && flag3 && !this.InAir && !this.isLadderAttached)
		{
			this.EndJump();
		}
		if (this.m_vp_FPController != null)
		{
			if (this.IsStuck || this.IsFlyMode.Value)
			{
				this.SetMoveState(EntityPlayerLocal.MoveState.Off, true);
			}
			else
			{
				flag = false;
				base.Stats.Health.RegenerationAmount = 0f;
				base.Stats.Stamina.RegenerationAmount = 0f;
				if (isStuck != this.IsStuck)
				{
					this.m_vp_FPController.Stop();
				}
				if (this.m_vp_FPController.enabled)
				{
					this.onGround = this.m_vp_FPController.Grounded;
				}
				bool flag4 = this.jumpTrigger;
				if (this.isSwimming)
				{
					this.SwimModeTick();
					flag2 = true;
				}
				else
				{
					if (this.vp_FPCamera != null && this.m_vp_FPController != null && this.m_vp_FPWeapon != null)
					{
						this.SetMoveStateToDefault();
					}
					if (flag4 && (this.vp_FPCamera.HasOverheadSpace || this.IsGodMode.Value))
					{
						vp_Activity jump = this.m_vp_FPController.Player.Jump;
						bool active = jump.Active;
						this.m_vp_FPController.MotorJumpForce = Mathf.Max(EffectManager.GetValue(PassiveEffects.JumpStrength, null, this.m_vp_FPController.originalMotorJumpForce, this, null, this.CurrentStanceTag | this.CurrentMovementTag, true, true, true, true, true, 1, true, false), 0f) * SandboxOptionManager.GetFloat(global::SandboxOptions.SandboxOptions.JumpStrength);
						this.m_vp_FPController.MotorJumpForceHold = this.m_vp_FPController.MotorJumpForce / Mathf.Lerp(90f, 180f, Mathf.Clamp01(1f - this.m_vp_FPController.originalMotorJumpForce / this.m_vp_FPController.MotorJumpForce)) * Time.timeScale;
						if (base.IsInElevator())
						{
							if (!active && !this.wasJumping && (this.onGround || this.isLadderAttached))
							{
								jump.Start(0f);
							}
						}
						else if (!this.wasJumping)
						{
							jump.TryStart(true);
						}
						if (!active && jump.Active)
						{
							this.Jumping = true;
							this.Stamina -= Mathf.Max(EffectManager.GetValue(PassiveEffects.StaminaLoss, null, 4f, this, null, FastTags<TagGroup.Global>.Parse("jumping") | this.CurrentStanceTag | this.CurrentMovementTag, true, true, true, true, true, 1, true, false), 0f);
							this.FireEvent(MinEventTypes.onSelfJump, true);
							this.PlayOneShot(this.GetSoundJump(), false, false, false, null, 1f);
						}
						if (this.onGround && this.wasJumping)
						{
							this.Jumping = false;
							this.jumpTrigger = false;
						}
						if (this.isLadderAttached && this.wasJumping)
						{
							this.bJumping = false;
							this.jumpTrigger = false;
						}
					}
					else
					{
						this.m_vp_FPController.Player.Jump.Stop(0f);
					}
				}
				this.wasJumping = flag4;
			}
		}
		if (flag)
		{
			base.OnUpdateLive();
		}
		else
		{
			base.CheckSleeperTriggers();
			base.Stats.Tick(this.world.worldTime);
			this.m_vp_FPController.SpeedModifier = this.GetSpeedModifier() * Mathf.Clamp01(EffectManager.GetValue(PassiveEffects.Mobility, null, 1f, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false)) * (this.isMotionSlowedDown ? this.motionMultiplier : 1f);
			this.isMotionSlowedDown = false;
			base.updateCurrentBlockPosAndValue();
			if (this.canEntityMove())
			{
				this.MoveEntityHeaded(this.moveDirection, false);
			}
			base.checkForTeleportOutOfTraderArea();
		}
		if (this.challengeJournal != null)
		{
			this.challengeJournal.Update(this.world);
		}
		if (this.QuestJournal != null)
		{
			this.QuestJournal.Update(this.world.WorldDay);
		}
		if (!this.isEntityRemote)
		{
			this.BlockRadiusEffectsTick();
		}
		if (this.Stamina <= 0f)
		{
			this.bExhausted = true;
			this.Stamina = 0f;
		}
		float stamina = this.Stamina;
		if (this.bExhausted && stamina > base.Stats.Stamina.Max * 0.2f)
		{
			this.isExhaustedSoundAllowed = true;
			this.bExhausted = false;
		}
		if (this.bExhausted && this.isExhaustedSoundAllowed)
		{
			this.PlayOneShot(this.GetSoundStamina(), false, false, false, null, 1f);
			GameManager.ShowTooltip(this, "ttOutOfStamina", false, false, 0f);
			this.isExhaustedSoundAllowed = false;
		}
		if (this.prevStaminaValue >= stamina - 0.1f && this.moveState == EntityPlayerLocal.MoveState.Run && !flag2)
		{
			this.runTicks++;
			if (this.runTicks > 100 && stamina / base.Stats.Stamina.Max < 0.5f && this.sprintLoopSoundPlayId == -1 && !this.IsDead())
			{
				Manager.BroadcastPlay(this, "Player" + (this.IsMale ? "Male" : "Female") + "RunLoop", false, 1f);
				this.sprintLoopSoundPlayId = 0;
			}
			this.lerpCameraFastFOV += Time.deltaTime * Constants.cRunningFOVSpeedUp;
		}
		else
		{
			if (this.sprintLoopSoundPlayId != -1)
			{
				Manager.BroadcastStop(this.entityId, "Player" + (this.IsMale ? "Male" : "Female") + "RunLoop");
				this.sprintLoopSoundPlayId = -1;
				if (this.isExhaustedSoundAllowed)
				{
					this.PlayOneShot(this.GetSoundStamina(), false, false, false, null, 1f);
				}
			}
			this.lerpCameraFastFOV -= Time.deltaTime * Constants.cRunningFOVSpeedDown;
			this.runTicks = 0;
		}
		this.lerpCameraFastFOV = Mathf.Clamp01(this.lerpCameraFastFOV);
		this.prevStaminaValue = stamina;
		if (this.playerUI != null && this.playerUI.windowManager.IsModalWindowOpen())
		{
			this.TryCancelChargedAction();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BlockRadiusEffectsTick()
	{
		Vector3i blockPosition = base.GetBlockPosition();
		int num = World.toChunkXZ(blockPosition.x);
		int num2 = World.toChunkXZ(blockPosition.z);
		this.blockRadiusEffectsIndex = (this.blockRadiusEffectsIndex + 1) % 3;
		int num3 = num2 + this.blockRadiusEffectsIndex - 1;
		for (int i = -1; i <= 1; i++)
		{
			Chunk chunk = (Chunk)this.world.GetChunkSync(num + i, num3);
			if (chunk != null)
			{
				DictionaryList<Vector3i, TileEntity> tileEntities = chunk.GetTileEntities();
				for (int j = 0; j < tileEntities.list.Count; j++)
				{
					TileEntity tileEntity = tileEntities.list[j];
					if (tileEntity.IsActive(this.world))
					{
						Block block = tileEntity.block;
						if (block.RadiusEffects != null)
						{
							this.BlockRadiusEffectsApply(block, tileEntity.ToWorldPos().ToVector3());
						}
					}
				}
			}
		}
	}

	public void BlockRadiusEffectsApply(Block _block, Vector3 _pos)
	{
		if (_pos.y > this.position.y)
		{
			_pos.y = Utils.FastMoveTowards(_pos.y, this.position.y, 1f);
		}
		float distanceSq = base.GetDistanceSq(_pos);
		for (int i = 0; i < _block.RadiusEffects.Length; i++)
		{
			BlockRadiusEffect blockRadiusEffect = _block.RadiusEffects[i];
			if (distanceSq <= blockRadiusEffect.radiusSq && !this.Buffs.HasBuff(blockRadiusEffect.variable))
			{
				this.Buffs.AddBuff(blockRadiusEffect.variable, -1, true, false, -1f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleLootStageMaxCheck()
	{
		base.HandleLootStageMaxCheck();
		if (this.biomeStandingOn == null)
		{
			return;
		}
		if (GameStats.GetBool(EnumGameStats.BiomeProgression))
		{
			int lootStage = base.GetLootStage(0f, 0f);
			int num = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.LootStageMax, null, (float)this.biomeStandingOn.LootStageMax, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
			if (lootStage >= num)
			{
				if (!this.LootAtMax || this.lastLootStage == -1)
				{
					this.FireEvent(MinEventTypes.onSelfBiomeLootStageMaxEntered, true);
					this.LootAtMax = true;
				}
			}
			else if (lootStage < num && (this.LootAtMax || this.lastLootStage == -1))
			{
				this.FireEvent(MinEventTypes.onSelfBiomeLootStageMaxExited, true);
				this.LootAtMax = false;
			}
			this.lastLootStage = lootStage;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canEntityMove()
	{
		bool flag = true;
		if (!this.IsFlyMode.Value)
		{
			Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos((int)(this.position.x + this.moveDirection.x * 2f), (int)this.position.y, (int)(this.position.z + this.moveDirection.z * 2f));
			if (chunk == null || !chunk.IsCollisionMeshGenerated)
			{
				flag = false;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldPushOutOfBlock(int _x, int _y, int _z, bool pushOutOfTerrain)
	{
		BlockShape shape = this.world.GetBlock(_x, _y, _z).Block.shape;
		if (shape.IsSolidSpace && !shape.IsTerrain())
		{
			return true;
		}
		if (pushOutOfTerrain && shape.IsSolidSpace && shape.IsTerrain())
		{
			BlockShape shape2 = this.world.GetBlock(_x, _y + 1, _z).Block.shape;
			if (shape2.IsSolidSpace && shape2.IsTerrain())
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool pushOutOfBlocks(float _x, float _y, float _z)
	{
		int num = Utils.Fastfloor(_x);
		int num2 = Utils.Fastfloor(_y);
		int num3 = Utils.Fastfloor(_z);
		float num4 = _x - (float)num;
		float num5 = _z - (float)num3;
		bool flag = false;
		bool flag2 = base.IsCrouching || this.IsMoveStateCrouch();
		if (this.shouldPushOutOfBlock(num, num2, num3, false) || (!flag2 && this.shouldPushOutOfBlock(num, num2 + 1, num3, false)))
		{
			bool flag3 = !this.shouldPushOutOfBlock(num - 1, num2, num3, true) && !this.shouldPushOutOfBlock(num - 1, num2 + 1, num3, true);
			bool flag4 = !this.shouldPushOutOfBlock(num + 1, num2, num3, true) && !this.shouldPushOutOfBlock(num + 1, num2 + 1, num3, true);
			bool flag5 = !this.shouldPushOutOfBlock(num, num2, num3 - 1, true) && !this.shouldPushOutOfBlock(num, num2 + 1, num3 - 1, true);
			bool flag6 = !this.shouldPushOutOfBlock(num, num2, num3 + 1, true) && !this.shouldPushOutOfBlock(num, num2 + 1, num3 + 1, true);
			byte b = byte.MaxValue;
			float num6 = 9999f;
			if (flag3 && num4 < num6)
			{
				num6 = num4;
				b = 0;
			}
			if (flag4 && 1.0 - (double)num4 < (double)num6)
			{
				num6 = 1f - num4;
				b = 1;
			}
			if (flag5 && num5 < num6)
			{
				num6 = num5;
				b = 4;
			}
			if (flag6 && 1f - num5 < num6)
			{
				b = 5;
			}
			float num7 = 0.1f;
			if (b == 0)
			{
				this.motion.x = -num7;
			}
			if (b == 1)
			{
				this.motion.x = num7;
			}
			if (b == 4)
			{
				this.motion.z = -num7;
			}
			if (b == 5)
			{
				this.motion.z = num7;
			}
			if (b != 255)
			{
				flag = true;
			}
		}
		return flag;
	}

	public override void Move(Vector3 _direction, bool _isDirAbsolute, float _velocity, float _maxVelocity)
	{
		base.Move(_direction, _isDirAbsolute, _velocity, _maxVelocity);
	}

	public override void SetAlive()
	{
		base.SetAlive();
		if (this.PhysicsTransform != null)
		{
			this.PhysicsTransform.gameObject.layer = 20;
		}
		if (this.m_vp_FPController != null)
		{
			this.m_vp_FPController.Player.Dead.Stop(0f);
		}
		this.SetModelLayer(24, false, null);
		this.ShowHoldingItemLayer(true);
		this.bPlayerStatsChanged = true;
		this.bPlayerEquipmentChanged = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public new void LateUpdate()
	{
		if (this.bLerpCameraFlag)
		{
			this.lerpCameraLerpValue += Time.deltaTime * 4f;
			if (this.lerpCameraLerpValue >= 1f)
			{
				this.bLerpCameraFlag = false;
				this.playerCamera.fieldOfView = this.lerpCameraEndFOV;
			}
			else
			{
				this.playerCamera.fieldOfView = Mathf.Lerp(this.lerpCameraStartFOV, this.lerpCameraEndFOV, this.lerpCameraLerpValue);
			}
		}
		if (!this.AimingGun)
		{
			float num = (float)this.GetCameraFOV();
			float num2 = this.playerCamera.fieldOfView;
			if (this.lerpCameraLerpValue <= 0f || !this.bLerpCameraFlag)
			{
				num2 = num;
			}
			this.playerCamera.fieldOfView = Mathf.Lerp(num2, num * Constants.cRunningFOVMultiplier, this.lerpCameraFastFOV);
			if (this.OverrideFOV != -1f)
			{
				this.playerCamera.fieldOfView = this.OverrideFOV;
				this.playerCamera.transform.LookAt(this.OverrideLookAt - Origin.position);
			}
			else if (this.lastOverrideFOV != -1f)
			{
				this.playerCamera.fieldOfView = num;
			}
			this.lastOverrideFOV = this.OverrideFOV;
		}
		this.TPCameraCheckResult = this.CharacterCameraAngleValid();
		this.WorldBoundsUpdate();
	}

	public bool TPCameraCheckPassed
	{
		get
		{
			return this.TPCameraCheckResult == eTPCameraCheckResult.Pass;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldBoundsUpdate()
	{
		this.InWorldPercent = this.world.InBoundsForPlayersPercent(this.position);
		this.InWorldLookPercent = this.InWorldPercent;
		if (this.InWorldPercent < 1f && !this.AttachedToEntity && !this.IsFlyMode.Value)
		{
			Vector3 forward = this.playerCamera.transform.forward;
			this.InWorldLookPercent = this.world.InBoundsForPlayersPercent(this.position + forward * 80f * 0.3f);
			if (this.InWorldPercent <= 0.05f)
			{
				Vector3i vector3i;
				Vector3i vector3i2;
				this.world.GetWorldExtent(out vector3i, out vector3i2);
				Vector2 vector;
				vector.x = (float)(vector3i.x + vector3i2.x) * 0.5f;
				vector.y = (float)(vector3i.z + vector3i2.z) * 0.5f;
				Vector3 normalized = new Vector3(vector.x - this.position.x, 0f, vector.y - this.position.z).normalized;
				float num = (1f - this.InWorldPercent / 0.05f) * 0.8f * Time.deltaTime;
				this.m_vp_FPController.AddForce(normalized.x * num, 0f, normalized.z * num);
				GameManager.ShowTooltip(this, Localization.Get("ttWorldEnd", false, null), false, false, 0f);
			}
		}
	}

	public void UnderwaterCameraFrameUpdate()
	{
		bool flag = this.UnderwaterCameraCheck();
		if (this.IsUnderwaterCamera != flag)
		{
			this.IsUnderwaterCamera = flag;
			Shader.SetGlobalFloat("_UnderWater", (float)(flag ? 1 : 0));
			if (this.uwEffectHaze)
			{
				this.uwEffectHaze.gameObject.SetActive(flag);
			}
			if (this.uwEffectRefract)
			{
				this.uwEffectRefract.gameObject.SetActive(flag);
			}
			if (this.uwEffectDebris)
			{
				this.uwEffectDebris.gameObject.SetActive(flag);
			}
			if (!flag)
			{
				if (this.uwEffectDroplets)
				{
					this.uwEffectDroplets.gameObject.SetActive(true);
					this.uwEffectDroplets.GetComponent<ParticleSystem>().GetComponent<Renderer>().enabled = true;
					this.uwEffectDroplets.GetComponent<ParticleSystem>().Play();
					this.uwEffectDroplets.GetComponent<ParticleSystem>().Emit(this.rand.RandomRange(60, 120));
				}
				if (this.uwEffectWaterFade)
				{
					this.uwEffectWaterFade.gameObject.SetActive(true);
					this.uwEffectWaterFade.GetComponent<ParticleSystem>().GetComponent<Renderer>().enabled = true;
					this.uwEffectWaterFade.GetComponent<ParticleSystem>().Play();
					this.uwEffectWaterFade.GetComponent<ParticleSystem>().Emit(1);
				}
			}
		}
	}

	public bool UnderwaterCameraCheck()
	{
		Vector3 vector = this.cameraTransform.position + Origin.position;
		vector.y += 0.28f;
		if (this.UnderwaterCameraCheckPos(vector))
		{
			return true;
		}
		Vector2 forwardVector = base.GetForwardVector2();
		vector.x -= forwardVector.x * 0.3f;
		vector.z -= forwardVector.y * 0.3f;
		return this.UnderwaterCameraCheckPos(vector);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool UnderwaterCameraCheckPos(Vector3 pos)
	{
		Vector3i vector3i = World.worldToBlockPos(pos);
		float waterPercent = this.world.GetWaterPercent(vector3i);
		return waterPercent > 0f && (float)vector3i.y + waterPercent - pos.y > 0f;
	}

	public override bool IsHeadUnderwater()
	{
		return this.inWaterPercent >= 0.791f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHeadUnderwaterStateChanged(bool _bUnderwater)
	{
		base.OnHeadUnderwaterStateChanged(_bUnderwater);
		if (_bUnderwater)
		{
			this.Buffs.SetCustomVar("_underwater", 1f, true, CVarOperation.set);
		}
		else if (!this.IsDead())
		{
			this.Buffs.SetCustomVar("_underwater", 0f, true, CVarOperation.set);
			if (this.soundWaterSurface != null && Time.time - this.lastTimeUnderwater > 3f)
			{
				Manager.BroadcastPlay(this, this.soundWaterSurface, false, 1f);
			}
		}
		else
		{
			this.Buffs.SetCustomVar("_underwater", 0f, true, CVarOperation.set);
		}
		this.lastTimeUnderwater = Time.time;
	}

	public override void SetDead()
	{
		base.SetDead();
		this.lastHitDirection = Utils.EnumHitDirection.None;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CameraDOFInit()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CameraDOFFrameUpdate()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal.HolsterState getCurrentHolsterState()
	{
		EntityPlayerLocal.HolsterState holsterState = EntityPlayerLocal.HolsterState.Unholstered;
		AvatarMultiBodyController avatarMultiBodyController = this.emodel.avatarController as AvatarMultiBodyController;
		Animator animator = null;
		if (avatarMultiBodyController != null && avatarMultiBodyController.BodyAnimators.Count >= 2 && avatarMultiBodyController.BodyAnimators[1] != null)
		{
			animator = avatarMultiBodyController.BodyAnimators[1].Animator;
		}
		if (avatarMultiBodyController != null && animator != null)
		{
			AnimatorClipInfo[] currentAnimatorClipInfo = animator.GetCurrentAnimatorClipInfo(1);
			if (currentAnimatorClipInfo.Length != 0)
			{
				holsterState = EntityPlayerLocal.HolsterState.Undefined;
				foreach (AnimatorClipInfo animatorClipInfo in currentAnimatorClipInfo)
				{
					if (animatorClipInfo.clip.name == "FP_Unarmed_Additive_HolsteHold" && animatorClipInfo.weight == 1f)
					{
						holsterState = EntityPlayerLocal.HolsterState.Holstered;
						break;
					}
				}
			}
		}
		else
		{
			holsterState = this.lastHolsterState;
		}
		return holsterState;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (this.desiredHolsterState != EntityPlayerLocal.HolsterState.Undefined)
		{
			EntityPlayerLocal.HolsterState currentHolsterState = this.getCurrentHolsterState();
			if (currentHolsterState != EntityPlayerLocal.HolsterState.Undefined && currentHolsterState != this.desiredHolsterState)
			{
				this.lastHolsterState = currentHolsterState;
				this.emodel.avatarController.UpdateBool(AvatarController.holsteredHash, this.desiredHolsterState == EntityPlayerLocal.HolsterState.Holstered, true);
				this.desiredHolsterState = EntityPlayerLocal.HolsterState.Undefined;
			}
		}
		this.renderManager.FrameUpdate();
		if (this.bPlayingSpawnIn)
		{
			WeatherManager.Instance.PushTransitions();
		}
		this.CameraDOFFrameUpdate();
		float time = Time.time;
		bool flag = this.IsDead();
		if (this.Spawned != this.wasSpawned)
		{
			if (!flag && this.lastRespawnReason != RespawnType.Teleport)
			{
				this.bPlayingSpawnIn = true;
				this.spawnInTime = time;
				Manager.PlayInsidePlayerHead("spawnInStinger", -1, 0f, false, false);
				this.HolsterWeapon(false);
			}
			this.wasSpawned = this.Spawned;
		}
		if (flag)
		{
			if (this.deathTime == 0f)
			{
				this.deathTime = time;
				Manager.PlayInsidePlayerHead("player_death_stinger", this.entityId);
			}
		}
		else if (this.deathTime > 0f)
		{
			this.ClearScreenEffects();
			this.deathTime = 0f;
		}
		float num = (float)this.Health;
		if (num < this.dyingEffectHealthLast - 3f)
		{
			this.dyingEffectCur = Mathf.Clamp01(1f - Mathf.Clamp(num, 0f, 70f) / 70f);
			this.dyingEffectHitTime = time;
		}
		this.dyingEffectHealthLast = num;
		this.dyingEffectCur *= Mathf.Clamp01(1f - (time - this.dyingEffectHitTime) / 600f);
		if (this.dyingEffectCur < 0.01f)
		{
			this.dyingEffectCur = 0f;
		}
		if (this.dyingEffectLast != this.dyingEffectCur)
		{
			this.dyingEffectLast = this.dyingEffectCur;
			if (!flag)
			{
				this.ScreenEffectManager.SetScreenEffect("Dying", this.dyingEffectCur, 0f);
			}
		}
		if (this.bPlayingSpawnIn && time < this.spawnInTime + EntityPlayerLocal.spawnInEffectSpeed)
		{
			this.spawnInIntensity = Mathf.Clamp01(1f - (time - this.spawnInTime) / EntityPlayerLocal.spawnInEffectSpeed);
			this.ScreenEffectManager.SetScreenEffect("VibrantDeSat", this.spawnInIntensity, 4f);
			this.bPlayingSpawnIn = this.spawnInIntensity > 0f;
		}
		else if (this.spawnInIntensity > 0f)
		{
			this.spawnInIntensity = 0f;
			this.ScreenEffectManager.SetScreenEffect("VibrantDeSat", 0f, 4f);
			this.bPlayingSpawnIn = false;
		}
		if ((double)base.GetCVar("_underwater") > 0.1 && !this.equipment.HasAnyItems() && this.biomeStandingOn != null && this.biomeStandingOn.m_sBiomeName == "snow")
		{
			IAchievementManager achievementManager = PlatformManager.NativePlatform.AchievementManager;
			if (achievementManager != null)
			{
				achievementManager.SetAchievementStat(EnumAchievementDataStat.SubZeroNakedSwim, 1);
			}
		}
		ProgressionValue progressionValue = this.Progression.GetProgressionValue("attFortitude");
		if (progressionValue != null)
		{
			IAchievementManager achievementManager2 = PlatformManager.NativePlatform.AchievementManager;
			if (achievementManager2 != null)
			{
				achievementManager2.SetAchievementStat(EnumAchievementDataStat.HighestFortitude, progressionValue.Level);
			}
		}
		IAchievementManager achievementManager3 = PlatformManager.NativePlatform.AchievementManager;
		if (achievementManager3 != null)
		{
			achievementManager3.SetAchievementStat(EnumAchievementDataStat.HighestGamestage, base.gameStage);
		}
		IAchievementManager achievementManager4 = PlatformManager.NativePlatform.AchievementManager;
		if (achievementManager4 != null)
		{
			achievementManager4.SetAchievementStat(EnumAchievementDataStat.HighestPlayerLevel, this.Progression.Level);
		}
		if (base.RentedVMPosition != Vector3i.zero && this.RentalEndDay <= GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime))
		{
			base.RentedVMPosition = Vector3i.zero;
			this.RentalEndTime = 0UL;
			this.RentalEndDay = 0;
		}
		this.sneakDamageBlendTimer.Tick(Time.deltaTime);
		this.ThreatLevel.Numeric = ThreatLevelUtility.GetThreatLevelOn(this);
		if (this.Spawned)
		{
			AudioListener.volume = Mathf.Lerp(AudioListener.volume, GamePrefs.GetFloat(EnumGamePrefs.OptionsOverallAudioVolumeLevel), Time.deltaTime);
		}
		float num2 = GamePrefs.GetFloat(EnumGamePrefs.OptionsAmbientVolumeLevel) * this.biomeVolume;
		if (this.audioSourceBiomeActive.isPlaying && Utils.FastAbs(this.audioSourceBiomeActive.volume - num2 * 0.95f) > 0.01f)
		{
			this.audioSourceBiomeActive.volume = Mathf.Lerp(this.audioSourceBiomeActive.volume, num2, Time.deltaTime);
			if (this.audioSourceBiomeActive.volume > num2 * 0.95f)
			{
				this.audioSourceBiomeActive.volume = num2;
			}
		}
		if (this.audioSourceBiomeFadeOut.isPlaying && this.audioSourceBiomeFadeOut.volume > 0.001f)
		{
			this.audioSourceBiomeFadeOut.volume = Mathf.Lerp(this.audioSourceBiomeFadeOut.volume, 0f, Time.deltaTime);
			if ((double)this.audioSourceBiomeFadeOut.volume < 0.05)
			{
				this.audioSourceBiomeFadeOut.clip = null;
				this.audioSourceBiomeFadeOut.Stop();
			}
		}
		this.FrameUpdateCamera();
		if (!GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return;
		}
		this.ShelterFrameUpdate();
		if (this.emodel.IsRagdollActive || (this.IsDead() && this.bSwitchCameraBackAfterRespawn))
		{
			this.SelfCameraFrameUpdate();
		}
		if (this.inventory.IsHoldingGun() && this.inventory.GetHoldingGun() is ItemActionRanged)
		{
			float num3 = (float)Screen.width / this.cameraTransform.GetComponent<Camera>().fieldOfView;
			float num4 = EffectManager.GetValue(PassiveEffects.SpreadDegreesHorizontal, this.inventory.holdingItemData.itemValue, 90f, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) * num3;
			float num5 = EffectManager.GetValue(PassiveEffects.SpreadDegreesVertical, this.inventory.holdingItemData.itemValue, 90f, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) * num3;
			if (num4 > num5)
			{
				this.crossHairOpenArea = (float)((int)num4) * (this.inventory.holdingItemData.actionData[0] as ItemActionRanged.ItemActionDataRanged).lastAccuracy;
			}
			else
			{
				this.crossHairOpenArea = (float)((int)num5) * (this.inventory.holdingItemData.actionData[0] as ItemActionRanged.ItemActionDataRanged).lastAccuracy;
			}
		}
		else
		{
			Vector3 vector = this.prevPos - this.position;
			float num6 = Mathf.Max(20f, Mathf.Clamp01(Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z) * 3f) * 100f);
			this.crossHairOpenArea = Mathf.Lerp(this.crossHairOpenArea, num6, Time.deltaTime * 4f);
		}
		if (this.autoMove != null)
		{
			this.autoMove.Update();
		}
		List<Vector3i> list = this.backpackPositionsFromThread;
		lock (list)
		{
			if (this.backpackPositionsFromThread.Count > 0)
			{
				this.SetDroppedBackpackPositions(this.backpackPositionsFromThread);
				this.backpackPositionsFromThread.Clear();
			}
		}
		if (base.IsAlive())
		{
			this.recoveryPointTimer += Time.deltaTime;
			if (this.recoveryPointTimer > 30f && this.onGround)
			{
				this.TryAddRecoveryPosition(Vector3i.FromVector3Rounded(this.position));
				this.recoveryPointTimer = 0f;
			}
		}
		if (this.sharedQuestsToProcess.Count > 0 && this.IsSpawned())
		{
			foreach (NetPackageSharedQuest.SharedQuestData sharedQuestData in this.sharedQuestsToProcess)
			{
				if (this.QuestJournal.AddSharedQuestEntry(sharedQuestData) && !PartyQuests.AutoAccept)
				{
					string text = "?";
					PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(sharedQuestData.sharedByEntityID);
					if (playerDataFromEntityID != null)
					{
						text = playerDataFromEntityID.PlayerName.DisplayName;
					}
					GameManager.ShowTooltip(this, string.Format(Localization.Get("ttQuestShared", false, null), text, QuestClass.GetQuest(sharedQuestData.questID).Name), string.Empty, "ui_quest_invite", null, false, false, 0f);
				}
			}
			this.sharedQuestsToProcess.Clear();
		}
	}

	public void AddSharedQuestEntry(NetPackageSharedQuest.SharedQuestData sqd)
	{
		this.sharedQuestsToProcess.Add(sqd);
	}

	public void HandleHordeEvent(AIDirector.HordeEvent msg)
	{
		string text = null;
		if (msg != AIDirector.HordeEvent.Warn2)
		{
			if (msg == AIDirector.HordeEvent.Spawn)
			{
				GameManager.Instance.StartCoroutine(this.shakeCamera(Vector3.one, 1f, 50f, 5f));
				text = "Enemies/Horde/horde_spawn";
			}
		}
		else
		{
			text = "Enemies/Horde/horde_spawn_warning";
		}
		if (text != null)
		{
			Manager.PlayInsidePlayerHead(text, -1, 0f, false, false);
		}
	}

	public override void OnFired()
	{
		base.OnFired();
		Vector2 vector;
		vector..ctor(EffectManager.GetValue(PassiveEffects.KickDegreesHorizontalMin, this.inventory.holdingItemItemValue, 0f, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false), EffectManager.GetValue(PassiveEffects.KickDegreesHorizontalMax, this.inventory.holdingItemItemValue, 0f, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
		Vector2 vector2;
		vector2..ctor(EffectManager.GetValue(PassiveEffects.KickDegreesVerticalMin, this.inventory.holdingItemItemValue, 0f, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false), EffectManager.GetValue(PassiveEffects.KickDegreesVerticalMax, this.inventory.holdingItemItemValue, 0f, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
		if (vector.x != 0f || vector.y != 0f || vector2.x != 0f || vector2.y != 0f)
		{
			switch (this.inventory.holdingItem.GetCameraShakeType(this.inventory.holdingItemData))
			{
			case EnumCameraShake.Tiny:
				GameManager.Instance.StartCoroutine(this.shakeCamera(Vector3.one, 0.5f, 5f, 1f));
				break;
			case EnumCameraShake.Small:
				GameManager.Instance.StartCoroutine(this.shakeCamera(Vector3.one, 0.5f, 10f, 1f));
				break;
			case EnumCameraShake.Big:
				GameManager.Instance.StartCoroutine(this.shakeCamera(Vector3.one, 0.5f, 20f, 1f));
				break;
			}
		}
		TriggerEffectManager.ControllerTriggerEffect controllerTriggerEffectShoot = this.inventory.holdingItem.GetControllerTriggerEffectShoot();
		if (controllerTriggerEffectShoot.XboxTriggerEffect.Effect != TriggerEffectManager.EffectXbox.Off || controllerTriggerEffectShoot.DualsenseEffect.Effect != TriggerEffectManager.EffectDualsense.Off)
		{
			GameManager.Instance.triggerEffectManager.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.RightTrigger, controllerTriggerEffectShoot, false);
		}
		if (!this.AimingGun)
		{
			MovementInput movementInput = this.movementInput;
			movementInput.rotation.x = movementInput.rotation.x + this.rand.RandomRange(vector2.x, vector2.y) * 2f;
			MovementInput movementInput2 = this.movementInput;
			movementInput2.rotation.y = movementInput2.rotation.y + this.rand.RandomRange(vector.x, vector.y) * 2f;
			return;
		}
		MovementInput movementInput3 = this.movementInput;
		movementInput3.rotation.x = movementInput3.rotation.x + this.rand.RandomRange(vector2.x, vector2.y);
		MovementInput movementInput4 = this.movementInput;
		movementInput4.rotation.y = movementInput4.rotation.y + this.rand.RandomRange(vector.x, vector.y);
	}

	public int GetCrosshairOpenArea()
	{
		return (int)this.crossHairOpenArea;
	}

	public Vector2 GetCrosshairPosition2D()
	{
		Vector3 vector = this.finalCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0f));
		return new Vector2(vector.x, vector.y);
	}

	public Vector3 GetCrosshairPosition3D(float _z, float _attributeOffset2D, Vector3 _altStartPosition)
	{
		if (!this.playerCamera.enabled || this.movementInput.bCameraChange)
		{
			return _altStartPosition;
		}
		Vector2 crosshairPosition2D = this.GetCrosshairPosition2D();
		crosshairPosition2D.x += this.rand.RandomRange(-_attributeOffset2D, _attributeOffset2D) * 700f;
		crosshairPosition2D.y += this.rand.RandomRange(-_attributeOffset2D, _attributeOffset2D) * 700f;
		Vector3 vector = this.playerCamera.ScreenToWorldPoint(new Vector3(crosshairPosition2D.x, crosshairPosition2D.y, _z)) + Origin.position;
		if (!this.bFirstPersonView)
		{
			float num = this.vp_FPCamera.CurrentCameraDistance;
			if (Voxel.Raycast(GameManager.Instance.World, new Ray(vector, this.GetLookVector()), num + 1f, true, false))
			{
				num = Vector3.Distance(Voxel.phyxRaycastHit.point, vector - Origin.position) - 0.5f;
			}
			vector += this.GetLookVector() * num;
		}
		return vector;
	}

	public override void OnHoldingItemChanged()
	{
		if (!this.IsDead())
		{
			this.SetModelLayer(24, false, null);
		}
	}

	public override void SetModelLayer(int _layerId, bool force = false, string[] excludeTags = null)
	{
		if (this.emodel == null)
		{
			return;
		}
		Transform modelTransform = this.emodel.GetModelTransform();
		if (modelTransform == null)
		{
			return;
		}
		if (this.oldLayer != _layerId || force)
		{
			this.oldLayer = _layerId;
			Utils.SetLayerWithExclusionList(modelTransform.gameObject, _layerId, excludeTags);
			if (_layerId == 24 && modelTransform.childCount > 0)
			{
				Utils.SetLayerWithExclusionList(modelTransform.GetChild(0).gameObject, _layerId, excludeTags);
			}
			modelTransform.gameObject.GetComponentsInChildren<Collider>(true, EntityPlayerLocal.setLayerRecursivelyList);
			for (int i = EntityPlayerLocal.setLayerRecursivelyList.Count - 1; i >= 0; i--)
			{
				Utils.SetLayerWithExclusionList(EntityPlayerLocal.setLayerRecursivelyList[i].gameObject, _layerId, excludeTags);
			}
			EntityPlayerLocal.setLayerRecursivelyList.Clear();
		}
	}

	public virtual void MoveByInput()
	{
		bool isCrouching = base.IsCrouching;
		if (this.IsStuck || EffectManager.GetValue(PassiveEffects.DisableMovement, null, 0f, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) > 0f)
		{
			this.movementInput.Clear();
		}
		if (EffectManager.GetValue(PassiveEffects.FlipControls, null, 0f, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) > 0f)
		{
			this.movementInput.moveForward *= -1f;
			this.movementInput.moveStrafe *= -1f;
		}
		if (this.AttachedToEntity != null)
		{
			this.Crouching = false;
			this.CrouchingLocked = false;
			base.Climbing = false;
			this.MovementRunning = false;
			this.AimingGun = false;
			this.AttachedToEntity.MoveByAttachedEntity(this);
		}
		else
		{
			bool flag = false;
			this.moveDirection.x = this.movementInput.moveStrafe;
			this.moveDirection.z = this.movementInput.moveForward;
			if (this.moveDirection.x != 0f || this.moveDirection.z != 0f)
			{
				flag = true;
			}
			bool flag2 = ((!base.IsSwimming()) ? (!this.bExhausted && this.moveDirection.z > 0f) : (this.swimExhaustedTicks == 0));
			bool flag3 = this.movementInput.running && flag;
			if (!this.IsFlyMode.Value)
			{
				flag3 = flag3 && flag2;
			}
			this.MovementRunning = flag3;
			if (base.IsSwimming())
			{
				if (!this.IsSwimmingMoving() || this.swimExhaustedTicks > 0)
				{
					this.CurrentMovementTag = EntityAlive.MovementTagFloating;
				}
				else if (!this.MovementRunning)
				{
					this.CurrentMovementTag = EntityAlive.MovementTagSwimming;
				}
				else
				{
					this.CurrentMovementTag = EntityAlive.MovementTagSwimmingRun;
				}
			}
			else if (flag)
			{
				if (!this.MovementRunning)
				{
					this.CurrentMovementTag = EntityAlive.MovementTagWalking;
				}
				else
				{
					this.CurrentMovementTag = EntityAlive.MovementTagRunning;
				}
			}
			else
			{
				this.CurrentMovementTag = EntityAlive.MovementTagIdle;
			}
			if (this.movementInput.downToggle)
			{
				this.CrouchingLocked = !this.CrouchingLocked;
			}
			this.CrouchingLocked = this.CrouchingLocked && !this.isLadderAttached && !this.movementInput.down;
			this.Crouching = !this.IsFlyMode.Value && !this.isLadderAttached && (this.movementInput.down || this.CrouchingLocked);
			if (!this.AimingGun)
			{
				if (!this.IsFlyMode.Value)
				{
					if (!this.JetpackWearing)
					{
						if (this.movementInput.jump && this.vp_FPController && !this.inputWasJump)
						{
							this.vp_FPController.enabled = true;
						}
						if (!this.Jumping && !this.wasJumping && this.movementInput.jump && (this.onGround || this.isLadderAttached) && this.AttachedToEntity == null)
						{
							this.jumpTrigger = true;
						}
						else if (this.wasLadderAttachedJump && !this.isLadderAttached && this.movementInput.jump && !this.inputWasJump)
						{
							this.canLadderAirAttach = true;
						}
					}
					else
					{
						if (this.movementInput.jump)
						{
							this.motion.y = this.motion.y + 0.15f;
							flag = true;
						}
						if (this.movementInput.down)
						{
							this.motion.y = this.motion.y - 0.15f;
						}
					}
				}
				else
				{
					if (this.movementInput.jump)
					{
						if (this.movementInput.running)
						{
							this.motion.y = 0.9f;
						}
						else
						{
							this.motion.y = 0.3f * this.GodModeSpeedModifier;
						}
					}
					if (this.movementInput.down)
					{
						if (this.movementInput.running)
						{
							this.motion.y = -0.9f;
						}
						else
						{
							this.motion.y = -0.3f * this.GodModeSpeedModifier;
						}
					}
				}
			}
			this.JetpackActive = this.JetpackWearing && flag;
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			bool isCrouching2 = base.IsCrouching;
			if (isCrouching2 != isCrouching)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityStealth>().Setup(this, isCrouching2), false);
			}
		}
		if (!this.bFirstPersonView && this.vp_FPCamera.Locked3rdPerson)
		{
			Vector3 right = base.transform.right;
			Vector3 forward = base.transform.forward;
			right.y = 0f;
			forward.y = 0f;
			right.Normalize();
			forward.Normalize();
			this.moveDirection = forward * this.moveDirection.z + right * this.moveDirection.x;
		}
		else if (!this.bFirstPersonView && this.CameraRelativeMovement)
		{
			Vector3 right2 = this.playerCamera.transform.right;
			Vector3 forward2 = this.playerCamera.transform.forward;
			right2.y = 0f;
			forward2.y = 0f;
			right2.Normalize();
			forward2.Normalize();
			this.moveDirection = forward2 * this.moveDirection.z + right2 * this.moveDirection.x;
		}
		if (this.vp_FPController != null)
		{
			if (this.AttachedToEntity == null)
			{
				this.vp_FPController.Player.InputMoveVector.Set(new Vector2(this.moveDirection.x, this.moveDirection.z));
			}
			this.vp_FPController.Player.InputSmoothLook.Set(new Vector2(this.movementInput.rotation.y, -this.movementInput.rotation.x));
		}
		this.inputWasJump = this.movementInput.jump;
		this.inputWasDown = this.movementInput.down;
		this.movementInput.Clear();
	}

	public void SwitchFirstPersonViewFromInput()
	{
		if (this.vp_FPCamera != null && this.vp_FPCamera.Locked3rdPerson)
		{
			return;
		}
		if (this.AttachedToEntity != null)
		{
			return;
		}
		if (this.inventory.IsHoldingItemActionRunning())
		{
			return;
		}
		if (!GameManager.Instance.IsEditMode() && GameStats.GetInt(EnumGameStats.CameraRestrictionMode) > 0)
		{
			GameManager.ShowTooltip(this, Localization.Get("ttCameraRestricted", false, null), false, false, 0f);
			return;
		}
		bool flag = !this.bFirstPersonView;
		this.SetFirstPersonView(flag, true);
		if (!this.bFirstPersonView)
		{
			vp_FPCamera vp_FPCamera = this.vp_FPCamera;
			if (vp_FPCamera)
			{
				vp_FPCamera.m_Current3rdPersonBlend = 1f;
			}
		}
		this.bPreferFirstPerson = this.bFirstPersonView;
	}

	public void SwitchToPreferredCameraMode(bool _lerpPosition)
	{
		int @int = GameStats.GetInt(EnumGameStats.CameraRestrictionMode);
		if (GameManager.Instance.IsEditMode() || @int == 0)
		{
			this.SetFirstPersonView(this.bPreferFirstPerson, _lerpPosition);
			return;
		}
		this.SetFirstPersonView(@int == 1, _lerpPosition);
	}

	public void SetFirstPersonView(bool _bFirstPersonView, bool _bLerpPosition)
	{
		this.bFirstPersonView = _bFirstPersonView;
		base.SetCVar(".IsFPV", (float)(_bFirstPersonView ? 1 : 0));
		this.FireEvent(MinEventTypes.onSelfChangedView, true);
		if (this.bFirstPersonView)
		{
			this.SetCameraAttachedToPlayer(true, false);
			this.switchModelView(EnumEntityModelView.FirstPerson);
			this.lineOfSightObstructionPoint = Vector3.zero;
		}
		else
		{
			this.SetCameraAttachedToPlayer(false, false);
			this.switchModelView(EnumEntityModelView.ThirdPerson);
			this.vp_PlayerEventHandler.CameraRelativeMovement3P.Set(this.CameraRelativeMovement);
		}
		this.UpdateCameraFOV(_bLerpPosition);
		this.refreshHolsterState();
		this.characterMatrixOverride.enabled = !this.bFirstPersonView;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void switchModelView(EnumEntityModelView modelView)
	{
		base.switchModelView(modelView);
		this.SetModelLayer(24, true, null);
		if (this.vp_FPController != null)
		{
			this.vp_FPController.Player.IsFirstPerson.Set(modelView == EnumEntityModelView.FirstPerson);
		}
		this.emodel.avatarController.SetCrouching(base.IsCrouching);
		this.emodel.avatarController.SetSwim(base.IsSwimming());
	}

	public override void BeforePlayerRespawn(RespawnType _type)
	{
		base.BeforePlayerRespawn(_type);
		switch (_type)
		{
		case RespawnType.NewGame:
		case RespawnType.LoadedGame:
		case RespawnType.Teleport:
			break;
		case RespawnType.Died:
		{
			for (int i = 0; i < this.overlayDirectionTime.Length; i++)
			{
				this.overlayDirectionTime[i] = 0f;
			}
			break;
		}
		default:
			return;
		}
	}

	public override void AfterPlayerRespawn(RespawnType _type)
	{
		if (_type == RespawnType.Teleport)
		{
			this.RespawnTeleportTimeoutLogged = false;
		}
		this.desiredHolsterState = EntityPlayerLocal.HolsterState.Undefined;
		base.AfterPlayerRespawn(_type);
		this.m_vp_FPCamera.enabled = true;
		if (this.AttachedToEntity != null)
		{
			this.SetFirstPersonView(false, false);
		}
		else
		{
			this.SwitchToPreferredCameraMode(true);
		}
		if (!GameManager.Instance.IsEditMode() && (_type == RespawnType.NewGame || _type == RespawnType.EnterMultiplayer))
		{
			this.emodel.avatarController.PlayPlayerFPRevive();
		}
		if (this.world.IsEditor() || GameModeCreative.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode)))
		{
			if (GameManager.Instance.IsEditMode() && !PrefabEditModeManager.Instance.IsActive())
			{
				SkyManager.SetFogDebug(0f, float.MinValue, float.MinValue);
				World world = GameManager.Instance.World;
				if (((world != null) ? world.BiomeAtmosphereEffects : null) != null)
				{
					GameManager.Instance.World.BiomeAtmosphereEffects.ForceDefault = true;
				}
			}
			if (this.Buffs != null && !this.Buffs.HasBuff("god"))
			{
				this.Buffs.AddBuff("god", -1, true, false, -1f);
			}
		}
		switch (_type)
		{
		case RespawnType.NewGame:
		case RespawnType.EnterMultiplayer:
		{
			this.SetAlive();
			this.Score = 0;
			if (this.world.IsEditor())
			{
				this.inventory.SetItem(1, new ItemValue(1, false), 64, true);
				this.inventory.SetHoldingItemIdx(0);
			}
			else
			{
				this.SetupStartingItems();
			}
			this.Buffs.UnPauseAll();
			SandboxOptionManager sandboxOptionManager = SandboxOptionManager.Current;
			sandboxOptionManager.LoadOptionsFromCode(GameStats.GetString(EnumGameStats.SandboxCode));
			sandboxOptionManager.UpdateInGameValuesWithSandboxOptions(false);
			this.FireEvent(MinEventTypes.onSelfFirstSpawn, true);
			this.FireEvent(MinEventTypes.onSelfEnteredGame, true);
			return;
		}
		case RespawnType.LoadedGame:
		case RespawnType.JoinMultiplayer:
		{
			SandboxOptionManager sandboxOptionManager2 = SandboxOptionManager.Current;
			sandboxOptionManager2.LoadOptionsFromCode(GameStats.GetString(EnumGameStats.SandboxCode));
			sandboxOptionManager2.UpdateInGameValuesWithSandboxOptions(false);
			this.FireEvent(MinEventTypes.onSelfEnteredGame, true);
			this.HandleMapObjects(true);
			return;
		}
		case RespawnType.Died:
			this.SetAlive();
			this.Health = this.GetMaxHealth();
			this.Stamina = (float)this.GetMaxStamina();
			this.Water = (float)this.GetMaxWater();
			base.Stats.Stamina.MaxModifier = 0f;
			this.CrouchingLocked = (this.Crouching = false);
			this.FireEvent(MinEventTypes.onSelfRespawn, true);
			Manager.StopLoopInsidePlayerHead("player_death_stinger_lp", this.entityId, false);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				Manager.Instance.StopDistantLoopingPositionalSounds(base.transform.position);
				return;
			}
			break;
		case RespawnType.Teleport:
			this.HolsterWeapon(false);
			this.FireEvent(MinEventTypes.onSelfTeleported, true);
			break;
		default:
			return;
		}
	}

	public void SetupStartingItems()
	{
		for (int i = 0; i < this.itemsOnEnterGame.Count; i++)
		{
			ItemStack itemStack = this.itemsOnEnterGame[i];
			itemStack.itemValue.Meta = ItemClass.GetForId(itemStack.itemValue.type).GetInitialMetadata(itemStack.itemValue);
			this.inventory.SetItem(i + 1, itemStack);
		}
		this.inventory.SetHoldingItemIdx(0);
	}

	public override Vector3 GetCameraLook(float _t)
	{
		if (!this.bFirstPersonView)
		{
			return this.cameraTransform.forward.normalized;
		}
		return base.GetCameraLook(_t);
	}

	public override Ray GetLookRay()
	{
		Ray ray = this.playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
		ray.origin += Origin.position;
		if (this.bFirstPersonView)
		{
			ray.direction += this.playerCamera.transform.up * 0.0001f;
			return ray;
		}
		ray.origin += ray.direction * this.m_vp_FPCamera.CurrentCameraDistance;
		ray.direction += this.playerCamera.transform.up * 0.0001f;
		float num = Vector3.Distance(this.playerCamera.transform.position, this.LookPoint);
		float num2 = Vector3.Distance(this.playerCamera.transform.position, ray.origin - Origin.position);
		if (num < num2)
		{
			ray.origin = this.LookPoint;
			ray.origin += Origin.position;
		}
		ray.origin -= this.playerCamera.transform.forward * 0.15f;
		return ray;
	}

	public override Ray GetMeleeRay()
	{
		if (this.bFirstPersonView)
		{
			return this.GetLookRay();
		}
		Ray ray;
		if (this.LineOfSightObstructed)
		{
			ray = this.playerCamera.ViewportPointToRay(this.playerCamera.WorldToViewportPoint(this.lineOfSightObstructionPoint));
		}
		else
		{
			ray..ctor(this.cameraTransform.position + this.cameraTransform.forward * this.vp_FPCamera.CurrentCameraDistance, this.cameraTransform.forward);
		}
		ray.origin += Origin.position;
		return ray;
	}

	public override Vector3 GetLookVector()
	{
		if (this.playerCamera.enabled)
		{
			return this.cameraTransform.forward;
		}
		return base.GetLookVector();
	}

	public override Vector3 GetLookVector(Vector3 _altLookVector)
	{
		if (!this.playerCamera.enabled)
		{
			return _altLookVector;
		}
		return this.GetLookVector();
	}

	public static void CheckPos()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startDeathCamera()
	{
		if (this.bFirstPersonView)
		{
			this.SetFirstPersonView(false, true);
		}
		this.bSwitchCameraBackAfterRespawn = true;
		this.StartSelfCamera();
		this.selfCameraSeekPos = this.selfCameraPos - this.cameraTransform.forward * 2.8f;
		this.selfCameraSeekPos.y = this.selfCameraSeekPos.y + 2.2f;
		this.ScreenEffectManager.SetScreenEffect("Dying", 0.5f, 0.5f);
		this.ScreenEffectManager.SetScreenEffect("Dead", 1f, 0.5f);
		this.ScreenEffectManager.SetScreenEffect("FadeToBlack", 1f, (float)base.GetTimeStayAfterDeath() * 0.05f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartSelfCamera()
	{
		this.selfCameraPos = this.cameraTransform.position + Origin.position;
		this.m_vp_FPCamera.enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SelfCameraFrameUpdate()
	{
		Vector3 vector = this.emodel.GetChestPosition();
		vector.y += 0.2f;
		if (this.selfCameraSeekPos.y < vector.y + 0.1f)
		{
			this.selfCameraSeekPos.y = this.selfCameraSeekPos.y + 0.05f;
		}
		this.selfCameraPos = Vector3.MoveTowards(this.selfCameraPos, this.selfCameraSeekPos, Time.deltaTime * 2.5f);
		Vector3 vector2 = this.selfCameraPos - vector;
		float magnitude = vector2.magnitude;
		vector -= Origin.position;
		bool flag = false;
		float num = magnitude;
		float num2 = magnitude - 0.28f;
		RaycastHit raycastHit;
		if (num2 > 0f && Physics.SphereCast(vector, 0.28f, vector2, ref raycastHit, num2, 65536))
		{
			num = Utils.FastMin(raycastHit.distance, num);
			flag = true;
		}
		if (!flag && Physics.Raycast(vector, vector2, ref raycastHit, magnitude + 0.28f, 65536))
		{
			num = raycastHit.distance - 0.28f;
			flag = true;
		}
		if (flag)
		{
			this.selfCameraPos = vector + vector2.normalized * Utils.FastMax(0.01f, num) + Origin.position;
			this.selfCameraSeekPos = this.selfCameraPos;
		}
		Vector3 vector3 = this.selfCameraPos - Origin.position;
		this.cameraTransform.position = vector3;
		Quaternion quaternion = Quaternion.LookRotation(vector - vector3);
		this.cameraTransform.rotation = Quaternion.Slerp(this.cameraTransform.rotation, quaternion, 0.1f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearScreenEffects()
	{
		this.ScreenEffectManager.DisableScreenEffect("Dying");
		this.ScreenEffectManager.DisableScreenEffect("Dead");
		this.ScreenEffectManager.DisableScreenEffect("FadeToBlack");
	}

	public bool CancellingInventoryActions
	{
		get
		{
			return this.cancellingInventoryActions;
		}
	}

	public IEnumerator CancelInventoryActions(Action cancelCallback, bool holsterWeapon)
	{
		this.cancellingInventoryActions = true;
		if (this.inventory.holdingItem.Actions != null && this.inventory.holdingItem.Actions.Length != 0 && this.inventory.holdingItemData.actionData != null && this.inventory.holdingItemData.actionData.Count > 0)
		{
			for (int i = 0; i < this.inventory.holdingItem.Actions.Length; i++)
			{
				if (this.inventory.holdingItem.Actions[i] != null && this.inventory.holdingItemData.actionData[i] != null)
				{
					if (this.inventory.holdingItem.Actions[i].IsActionRunning(this.inventory.holdingItemData.actionData[i]))
					{
						this.inventory.holdingItem.Actions[i].CancelAction(this.inventory.holdingItemData.actionData[i]);
					}
					this.inventory.holdingItem.Actions[i].CancelReload(this.inventory.holdingItemData.actionData[i], holsterWeapon);
				}
			}
		}
		while (this.IsReloading())
		{
			yield return null;
		}
		for (;;)
		{
			EntityPlayerLocal.HolsterState currentHolsterState = this.getCurrentHolsterState();
			if (this.desiredHolsterState == EntityPlayerLocal.HolsterState.Undefined || this.desiredHolsterState == currentHolsterState)
			{
				break;
			}
			yield return null;
		}
		cancelCallback();
		this.cancellingInventoryActions = false;
		yield break;
	}

	public bool IsReloading()
	{
		if (this.inventory.holdingItemData.actionData != null)
		{
			foreach (ItemActionData itemActionData in this.inventory.holdingItemData.actionData)
			{
				ItemActionRanged.ItemActionDataRanged itemActionDataRanged = itemActionData as ItemActionRanged.ItemActionDataRanged;
				if (itemActionDataRanged != null)
				{
					return ItemActionRanged.Reloading(itemActionDataRanged);
				}
			}
			return false;
		}
		return false;
	}

	public override void OnEntityDeath()
	{
		GameManager.Instance.TriggerSendOfLocalPlayerDataFile(0f);
		base.StartCoroutine(this.CancelInventoryActions(delegate
		{
		}, true));
		this.inventory.ReleaseAll(this.playerInput);
		this.inventory.SetActiveItemIndexOff();
		Manager.BroadcastStop(this.entityId, "Player" + (this.IsMale ? "Male" : "Female") + "RunLoop");
		this.sprintLoopSoundPlayId = -1;
		this.windowManager.CloseAllOpenModalWindows(null, false);
		this.windowManager.Close("windowpaging");
		GameManager.Instance.ClearTooltips(this.nguiWindowManager);
		this.windowManager.Open("death", false);
		this.AimingGun = false;
		this.BloodMoonParticipation = false;
		base.OnEntityDeath();
		this.startDeathCamera();
	}

	public override void OnDeathUpdate()
	{
		base.OnDeathUpdate();
		if (this.Spawned && base.GetDeathTime() >= base.GetTimeStayAfterDeath())
		{
			this.windowManager.Close("death");
			if (this.ShouldRemoveEquipmentOnDeath((EntityPlayerLocal.DropOption)GameStats.GetInt(EnumGameStats.DropOnDeath)))
			{
				for (int i = 0; i < 12; i++)
				{
					ItemValue slotItem = this.equipment.GetSlotItem(i);
					if (this.dropValueCondition(slotItem))
					{
						this.equipment.SetSlotItem(i, null, true);
					}
				}
			}
			this.Respawn(RespawnType.Died);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FrameUpdateCamera()
	{
		this.UnderwaterCameraFrameUpdate();
		if (!this.bFirstPersonView)
		{
			if (!(this.AttachedToEntity is EntityVehicle))
			{
				bool flag = this.inventory.holdingItem.Actions[0] is ItemActionRanged || this.inventory.holdingItem.Actions[1] is ItemActionRanged;
				int @int = GamePrefs.GetInt(EnumGamePrefs.OptionsGfx3PCameraMode);
				if (@int == 1)
				{
					this.StartTPCameraLockTimer();
				}
				else if (@int == 2 && flag)
				{
					this.StartTPCameraLockTimer();
				}
				else if (this.IsGodMode.Value || this.IsFlyMode.Value || this.AttachedToEntity != null || this.isLadderAttached || this.isSwimming)
				{
					this.StartTPCameraLockTimer();
				}
				else if (this.tpCameraLockTimerActive && Time.time - this.tpCameraLockStartTime > 2f && !this.AimingGun)
				{
					this.tpCameraLockTimerActive = false;
					this.CameraRelativeMovement = true;
				}
				bool flag2 = false;
				float num = 0f;
				RaycastHit raycastHit;
				if (Physics.Raycast(new Ray(base.transform.position, Vector3.up), ref raycastHit, 8f, 1073807360))
				{
					flag2 = true;
					num = raycastHit.distance;
					if (this.overheadObstructionTime < 0.5f)
					{
						this.overheadObstructionTime += Time.deltaTime;
					}
				}
				else
				{
					this.overheadObstructionTime = 0f;
				}
				float num2 = 1f;
				if (flag2 && this.overheadObstructionTime >= 0.5f)
				{
					if (num < 3f)
					{
						num2 = 0.5f;
					}
					else if (num < 6f)
					{
						num2 = 0.65f;
					}
					else
					{
						num2 = 0.8f;
					}
				}
				if (this.MovementRunning)
				{
					num2 *= 1.5f;
				}
				this.camOverheadDistance = Utils.FastLerp(this.camOverheadDistance, num2, Time.deltaTime * 1.5f);
				vp_FPCamera vp_FPCamera = this.m_vp_FPCamera;
				if (this.Crouching && !this.isSwimming)
				{
					if (this.AimingGun)
					{
						vp_FPCamera.Position3rdPersonOffset = new Vector3(0.3f, 1.25f, 0.6f);
					}
					else
					{
						vp_FPCamera.Position3rdPersonOffset = new Vector3(0.3f, 1.25f, 1f);
					}
				}
				else if (this.AimingGun)
				{
					vp_FPCamera.Position3rdPersonOffset = new Vector3(0.4f, 1.65f, 0.8f);
				}
				else
				{
					vp_FPCamera.Position3rdPersonOffset = new Vector3(0.4f, 1.65f, 1.2f);
				}
				float num3 = 1f + 3f * this.cameraDistanceMulti;
				vp_FPCamera vp_FPCamera2 = vp_FPCamera;
				vp_FPCamera2.Position3rdPersonOffset.z = vp_FPCamera2.Position3rdPersonOffset.z * (this.camOverheadDistance * ((num2 >= 1f && !this.AimingGun) ? num3 : 1f));
				if (this.isAimingScoped)
				{
					vp_FPCamera.Position3rdPersonOffset.x = 0f;
				}
				else if (this.isLadderAttached && !this.AimingGun)
				{
					vp_FPCamera.Position3rdPersonOffset.x = 0.15f;
				}
				if (this.flipCameraSide)
				{
					vp_FPCamera vp_FPCamera3 = vp_FPCamera;
					vp_FPCamera3.Position3rdPersonOffset.x = vp_FPCamera3.Position3rdPersonOffset.x * -1f;
				}
			}
			this.characterMatrixOverride.Active = this.vp_FPCamera.CameraCollisionDistance < 0.55f;
			this.ThirdPersonLineOfSightCheck();
		}
	}

	public void RefreshDrivingCameraPositions()
	{
		if (this.AttachedToEntity != null)
		{
			EntityVehicle entityVehicle = this.AttachedToEntity as EntityVehicle;
			if (entityVehicle != null)
			{
				this.vp_FPCamera.Position3rdPersonOffset = entityVehicle.GetCameraOffset(Time.deltaTime);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateCameraFOV(bool _bLerpPosition)
	{
		if (!this.playerCamera.enabled)
		{
			return;
		}
		if (!this.IsCameraAttachedToPlayerOrScope() && this.bFirstPersonView)
		{
			return;
		}
		if (this.AimingGun)
		{
			this.inventory.holdingItem.GetIronSights(this.inventory.holdingItemData, out this.lerpCameraEndFOV);
			if (this.lerpCameraEndFOV != 0f)
			{
				this.bLerpCameraFlag = _bLerpPosition;
				this.lerpCameraLerpValue = 0f;
				this.lerpCameraStartFOV = this.playerCamera.fieldOfView;
				return;
			}
		}
		else
		{
			float num = (float)this.GetCameraFOV();
			this.bLerpCameraFlag = _bLerpPosition;
			if (this.bLerpCameraFlag)
			{
				this.lerpCameraLerpValue = 0f;
				this.lerpCameraStartFOV = this.playerCamera.fieldOfView;
				this.lerpCameraEndFOV = num;
				return;
			}
			this.playerCamera.fieldOfView = num;
		}
	}

	public bool TryUpdateCameraDistanceMultiplier(float amount)
	{
		if (amount == 0f)
		{
			this.lastCameraDistanceAmount = amount;
			return false;
		}
		if (amount > 0f && this.bFirstPersonView && this.lastCameraDistanceAmount == 0f)
		{
			if (GamePrefs.GetInt(EnumGamePrefs.CameraRestrictionMode) != 0)
			{
				return false;
			}
			this.cameraDistanceMulti = 0f;
			this.SwitchFirstPersonViewFromInput();
			this.lastCameraDistanceAmount = amount;
			return true;
		}
		else
		{
			if (amount > 0f && this.cameraDistanceMulti == 1f)
			{
				this.lastCameraDistanceAmount = amount;
				return false;
			}
			if (amount >= 0f || this.cameraDistanceMulti != 0f || this.bFirstPersonView)
			{
				this.lastCameraDistanceAmount = amount;
				this.cameraDistanceMulti = Mathf.Clamp(this.cameraDistanceMulti + amount, 0f, 1f);
				GamePrefs.Set(EnumGamePrefs.OptionsGfxCameraDistance3P, this.cameraDistanceMulti);
				return true;
			}
			if (GamePrefs.GetInt(EnumGamePrefs.CameraRestrictionMode) != 0)
			{
				return false;
			}
			if (this.lastCameraDistanceAmount == 0f)
			{
				this.SwitchFirstPersonViewFromInput();
				this.lastCameraDistanceAmount = amount;
				return true;
			}
			this.lastCameraDistanceAmount = amount;
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ThirdPersonLineOfSightCheck()
	{
		this.lineOfSightObstructionPoint = Vector3.zero;
		this.LineOfSightObstructed = false;
		Vector3 vector = base.transform.position + new Vector3(0f, this.Crouching ? (base.height * 0.7f) : (base.height * 0.95f), 0f) + (this.flipCameraSide ? (-base.transform.right) : base.transform.right) * 0.05f;
		Vector3 normalized = (this.LookPoint - vector).normalized;
		vector += Origin.position;
		this.lineOfSightRay.origin = vector;
		this.lineOfSightRay.direction = normalized;
		if (Voxel.Raycast(GameManager.Instance.World, this.lineOfSightRay, 1f, 1073807360, 8, 0f))
		{
			if (Vector3.Distance(Voxel.phyxRaycastHit.point, this.LookPoint) > 0.1f)
			{
				this.LineOfSightObstructed = true;
			}
			this.lineOfSightObstructionPoint = Voxel.phyxRaycastHit.point;
		}
	}

	public bool IsCameraFacingCharacter()
	{
		return !this.bFirstPersonView && Vector3.Dot(base.transform.forward, this.cameraTransform.forward) < 0f;
	}

	public virtual bool isAimingScoped
	{
		get
		{
			if (!this.AimingGun)
			{
				return false;
			}
			ItemActionZoom.ItemActionDataZoom itemActionDataZoom = this.inventory.holdingItemData.actionData[1] as ItemActionZoom.ItemActionDataZoom;
			return itemActionDataZoom != null && itemActionDataZoom.HasScope && itemActionDataZoom.ZoomOverlay != null;
		}
	}

	public override int GetCameraFOV()
	{
		if (!this.bFirstPersonView)
		{
			return GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV3P);
		}
		return GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV);
	}

	public override void PhysicsResume(Vector3 pos, float rotY)
	{
		Transform transform = this.cameraTransform;
		Vector3 position = transform.position;
		Quaternion rotation = transform.rotation;
		base.PhysicsResume(pos, rotY);
		transform.SetPositionAndRotation(position, rotation);
	}

	public override void OnRagdoll(bool isActive)
	{
		base.OnRagdoll(isActive);
		if (isActive)
		{
			this.SetFirstPersonView(false, true);
			this.emodel.InitRigidBodies();
			this.StartSelfCamera();
			Vector3 forwardVector = base.GetForwardVector();
			this.selfCameraSeekPos = this.emodel.GetChestPosition();
			this.selfCameraSeekPos.x = this.selfCameraSeekPos.x - forwardVector.x * 2.2f;
			this.selfCameraSeekPos.y = this.selfCameraSeekPos.y + 1.2f;
			this.selfCameraSeekPos.z = this.selfCameraSeekPos.z - forwardVector.z * 2.2f;
			Transform transform = this.cameraTransform;
			transform.position = Vector3.MoveTowards(transform.position, this.selfCameraSeekPos, 1.2f);
			return;
		}
		this.SetRotation(this.rotation);
		this.m_vp_FPCamera.enabled = true;
		this.SwitchToPreferredCameraMode(true);
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale = 1f)
	{
		_strength = base.DamageEntity(_damageSource, _strength, _criticalHit, impulseScale);
		if (_strength > 0)
		{
			GameManager.Instance.StartCoroutine(this.shakeCamera(_damageSource.getDirection(), 0.5f, (float)(_strength * 4), 1f));
		}
		return _strength;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator shakeCamera(Vector3 _direction, float time, float strength, float speed = 1f)
	{
		this.m_vp_FPCamera.ShakeSpeed2 = speed;
		this.m_vp_FPCamera.ShakeAmplitude2 = new Vector3(-_direction.x, _direction.y, 0f) * strength;
		yield return new WaitForSeconds(time);
		this.m_vp_FPCamera.ShakeSpeed2 = 0f;
		this.m_vp_FPCamera.ShakeAmplitude2 = Vector3.zero;
		yield break;
	}

	public override void Kill(DamageResponse _dmResponse)
	{
		if (!this.IsDead())
		{
			base.Kill(_dmResponse);
			GameManager.Instance.StartCoroutine(this.shakeCamera(Vector3.one, 0.5f, 20f, 1f));
			if (this.m_vp_FPController != null)
			{
				this.m_vp_FPController.Player.Dead.Start(0f);
			}
		}
	}

	public override void SetPosition(Vector3 _pos, bool _bUpdatePhysics = true)
	{
		base.SetPosition(_pos, _bUpdatePhysics);
		Origin.Instance.UpdateLocalPlayer(this);
		vp_FPController vp_FPController = this.vp_FPController;
		if (vp_FPController != null)
		{
			if (!this.emodel.IsRagdollActive)
			{
				vp_FPController.SetPosition(_pos - Origin.position);
			}
			vp_FPController.Stop();
			if (this.AttachedToEntity)
			{
				vp_FPController.Transform.localPosition = Vector3.zero;
			}
		}
		Manager.CameraChanged();
	}

	public override void Respawn(RespawnType _reason)
	{
		Log.Out("Respawning: " + _reason.ToStringCached<RespawnType>());
		base.Respawn(_reason);
		if (_reason == RespawnType.Teleport)
		{
			this.RespawnTeleportDuration = Time.realtimeSinceStartup;
			this.RespawnTeleportTimeoutLogged = false;
		}
		this.moveController.Respawn(_reason);
		Shader.SetGlobalFloat("_UnderWater", 0f);
		this.SetControllable(false);
		if (this.vp_FPController != null && !this.AttachedToEntity)
		{
			this.vp_FPController.ResetState();
			this.vp_FPController.SetPosition(base.GetPosition() - Origin.position);
			this.vp_FPController.Stop();
			this.vp_FPCamera.Locked3rdPerson = false;
		}
		if (_reason == RespawnType.Teleport)
		{
			this.windowManager.CloseAllOpenModalWindows("map");
		}
		this.isFallDeath = false;
	}

	public void TeleportToPosition(Vector3 _pos, bool _onlyIfNotFlying = false, Vector3? _viewDirection = null)
	{
		if (!_onlyIfNotFlying || !this.IsFlyMode.Value)
		{
			this.Teleport(_pos, (_viewDirection != null) ? _viewDirection.Value.y : float.MinValue);
			if (_pos.y >= 0f)
			{
				ThreadManager.StartCoroutine(this.setVerticalPosition(_pos, _viewDirection));
				return;
			}
			Log.Out("Teleported to {0}", new object[] { _pos.ToCultureInvariantString() });
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator setVerticalPosition(Vector3 _pos, Vector3? _viewDirection)
	{
		while (!this.Spawned)
		{
			yield return null;
		}
		if (this.AttachedToEntity != null)
		{
			this.AttachedToEntity.SetPosition(_pos, true);
		}
		else
		{
			this.SetPosition(_pos, true);
		}
		if (_viewDirection != null)
		{
			this.SetRotation(_viewDirection.Value);
		}
		Log.Out("Teleported to {0}", new object[] { _pos.ToCultureInvariantString() });
		yield break;
	}

	public void SetControllable(bool _b)
	{
		if (_b && this.bIntroAnimActive)
		{
			return;
		}
		base.transform.GetComponent<PlayerMoveController>().SetControllableOverride(_b);
	}

	public void NotifySneakDamage(float multiplier)
	{
		this.sneakDamageText = string.Format(Localization.Get("sneakDamageBonus", false, null), multiplier.ToCultureInvariantString("f1")).ToUpper();
		this.sneakDamageBlendTimer.FadeIn();
	}

	public void NotifyDamageMultiplier(float multiplier)
	{
		this.sneakDamageText = string.Format(Localization.Get("stunnedDamageBonus", false, null), multiplier.ToCultureInvariantString("f1")).ToUpper();
		this.sneakDamageBlendTimer.FadeIn();
	}

	public override void EnableCamera(bool _b)
	{
		this.playerCamera.enabled = _b;
	}

	public override bool IsAttackValid()
	{
		return true;
	}

	public override bool IsAimingGunPossible()
	{
		return (this.inventory.holdingItem.Actions[0] == null || this.inventory.holdingItem.Actions[0].IsAimingGunPossible(this.inventory.holdingItemData.actionData[0])) && (this.inventory.holdingItem.Actions[1] == null || this.inventory.holdingItem.Actions[1].IsAimingGunPossible(this.inventory.holdingItemData.actionData[1]));
	}

	public override bool IsDrawMapIcon()
	{
		return this.IsSpawned();
	}

	public override bool IsMapIconBlinking()
	{
		return true;
	}

	public override bool CanMapIconBeSelected()
	{
		return false;
	}

	public override Color GetMapIconColor()
	{
		return Color.white;
	}

	public override int GetLayerForMapIcon()
	{
		return 20;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool BreakLeg(float chance)
	{
		if (this.rand.RandomFloat <= chance && this.Buffs.AddBuff("injuryBrokenLeg", -1, true, false, -1f) == EntityBuffs.BuffStatus.Added)
		{
			this.PlayOneShot("breakleg", false, false, false, null, 1f);
			IAchievementManager achievementManager = PlatformManager.NativePlatform.AchievementManager;
			if (achievementManager != null)
			{
				achievementManager.SetAchievementStat(EnumAchievementDataStat.LegBroken, 1);
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool FractureLeg(float chance)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool SprainLeg(float chance)
	{
		return this.rand.RandomFloat <= chance && this.Buffs.AddBuff("injurySprainedLeg", -1, true, false, -1f) == EntityBuffs.BuffStatus.Added;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HasBrokenLeg()
	{
		return this.Buffs.HasBuff("injuryBrokenLeg");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HasFracturedLeg()
	{
		return this.Buffs.HasBuff("injuryBrokenLeg");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HasSprainedLeg()
	{
		return this.Buffs.HasBuff("injurySprainedLeg");
	}

	public override float GetSpeedModifier()
	{
		return base.GetSpeedModifier() * (this.IsGodMode.Value ? this.GodModeSpeedModifier : 1f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void FallImpact(float speed)
	{
		if (this.IsGodMode.Value || this.AttachedToEntity != null)
		{
			return;
		}
		if (speed <= 0f)
		{
			return;
		}
		Vector3i vector3i = World.worldToBlockPos(this.vp_FPController.Transform.position + Origin.position);
		BlockValue blockValue = this.world.GetBlock(vector3i);
		if (blockValue.isair || blockValue.Block.IsElevator((int)blockValue.rotation))
		{
			vector3i.y--;
			blockValue = this.world.GetBlock(vector3i);
		}
		float num = 1f;
		if (!blockValue.isair)
		{
			num = blockValue.Block.FallDamage;
			if (num <= 0f)
			{
				return;
			}
		}
		if (speed > 1f)
		{
			speed = 1f;
		}
		speed *= 1f;
		speed *= EntityPlayer.FallDamageModifier;
		speed *= num;
		speed = EffectManager.GetValue(PassiveEffects.FallDamageReduction, this.inventory.holdingItemItemValue, speed, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.fallHealth = this.Health;
		base.SetCVar("_fallSpeed", speed);
		this.FireEvent(MinEventTypes.onSelfFallImpact, true);
		if (speed > 0.05f)
		{
			this.PlayHitGroundSound(speed);
		}
	}

	public override void BuffAdded(BuffValue _buff)
	{
		if (_buff.BuffClass.NameTag.Test_Bit(EntityAlive.FallingBuffTagBit) && this.fallHealth > 0 && this.Health <= 0)
		{
			this.isFallDeath = true;
		}
	}

	public override void CopyPropertiesFromEntityClass()
	{
		base.CopyPropertiesFromEntityClass();
		EntityClass entityClass = EntityClass.list[this.entityClass];
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropDropInventoryBlock))
		{
			this.dropInventoryBlock = entityClass.Properties.Values[EntityClass.PropDropInventoryBlock];
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void dropItemOnDeath()
	{
		this.removeItemsOnDeath();
		this.degradeItemsOnDeath();
		this.dropBackpack(true);
		this.inventory.SetFlashlight(false);
	}

	public void dropItemOnQuit()
	{
		this.dropBackpack(false);
	}

	public override void SetDroppedBackpackPositions(List<Vector3i> _positions)
	{
		if (!ThreadManager.IsMainThread())
		{
			List<Vector3i> list = this.backpackPositionsFromThread;
			lock (list)
			{
				this.backpackPositionsFromThread.Clear();
				if (_positions != null)
				{
					this.backpackPositionsFromThread.AddRange(_positions);
				}
			}
			return;
		}
		base.SetDroppedBackpackPositions(_positions);
		if (this.backpackNavObjects != null)
		{
			foreach (NavObject navObject in this.backpackNavObjects)
			{
				NavObjectManager.Instance.UnRegisterNavObject(navObject);
			}
			this.backpackNavObjects.Clear();
		}
		else
		{
			this.backpackNavObjects = new List<NavObject>();
		}
		if (_positions != null)
		{
			for (int i = 0; i < _positions.Count; i++)
			{
				Vector3i vector3i = _positions[i];
				if (!vector3i.Equals(Vector3i.zero))
				{
					this.backpackNavObjects.Add(NavObjectManager.Instance.RegisterNavObject("backpack_distant", vector3i.ToVector3() + new Vector3(0.5f, 0f, 0.5f), "", false, -1, null));
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void removeItemsOnDeath()
	{
		EntityPlayerLocal.DropOption @int = (EntityPlayerLocal.DropOption)SandboxOptionManager.GetInt(global::SandboxOptions.SandboxOptions.LoseItemsOnDeathType);
		if (@int == EntityPlayerLocal.DropOption.None)
		{
			return;
		}
		bool flag = @int == EntityPlayerLocal.DropOption.All || @int == EntityPlayerLocal.DropOption.Backpack || @int == EntityPlayerLocal.DropOption.ToolbeltBackpack;
		bool flag2 = @int == EntityPlayerLocal.DropOption.All || @int == EntityPlayerLocal.DropOption.Toolbelt || @int == EntityPlayerLocal.DropOption.ToolbeltBackpack;
		bool flag3 = @int == EntityPlayerLocal.DropOption.All || @int == EntityPlayerLocal.DropOption.Equipment;
		if (this.DestroyLocationSlotList == null)
		{
			this.DestroyLocationSlotList = new List<EntityPlayerLocal.LocationSlotInfo>();
		}
		else
		{
			this.DestroyLocationSlotList.Clear();
		}
		if (flag)
		{
			ItemStack[] slots = this.bag.GetSlots();
			for (int i = 0; i < slots.Length; i++)
			{
				if (this.dropStackCondition(slots[i]))
				{
					this.DestroyLocationSlotList.Add(new EntityPlayerLocal.LocationSlotInfo(EntityPlayerLocal.LocationSlotInfo.LocationTypes.Backpack, i));
				}
			}
		}
		if (flag2)
		{
			for (int j = 0; j < this.inventory.GetItemCount(); j++)
			{
				if (j != this.inventory.DUMMY_SLOT_IDX)
				{
					ItemStack item = this.inventory.GetItem(j);
					if (this.dropStackCondition(item))
					{
						this.DestroyLocationSlotList.Add(new EntityPlayerLocal.LocationSlotInfo(EntityPlayerLocal.LocationSlotInfo.LocationTypes.Toolbelt, j));
					}
				}
			}
		}
		int num = 4;
		int num2 = 7;
		if (flag3)
		{
			for (int k = 0; k < this.equipment.GetSlotCount(); k++)
			{
				if (k < num || k > num2)
				{
					ItemValue slotItem = this.equipment.GetSlotItem(k);
					if (this.dropValueCondition(slotItem))
					{
						this.DestroyLocationSlotList.Add(new EntityPlayerLocal.LocationSlotInfo(EntityPlayerLocal.LocationSlotInfo.LocationTypes.Equipment, k));
					}
				}
			}
		}
		if (this.DestroyLocationSlotList.Count == 0)
		{
			return;
		}
		int num3 = Mathf.Min(this.rand.RandomRange(EntityPlayerLocal.LostItemOnDeathMin, EntityPlayerLocal.LostItemOnDeathMax + 1), this.DestroyLocationSlotList.Count);
		this.DestroyLocationSlotList.Shuffle(this.rand);
		if (this.DestroyLocationSlotList.Count > num3)
		{
			this.DestroyLocationSlotList.RemoveRange(num3, this.DestroyLocationSlotList.Count - num3);
		}
		if (flag)
		{
			ItemStack[] slots2 = this.bag.GetSlots();
			this.HandleRemoveRandomItems(slots2);
			this.bag.SetSlots(slots2);
		}
		if (flag3)
		{
			this.HandleRemoveRandomItems(this.equipment);
		}
		if (flag2)
		{
			this.HandleRemoveRandomItems(this.inventory);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void degradeItemsOnDeath()
	{
		if (EntityPlayerLocal.DegradeOnDeathType == EntityPlayerLocal.DegradeOnDeathTypes.None)
		{
			return;
		}
		bool flag = false;
		foreach (ItemStack itemStack in this.bag.GetSlots())
		{
			if (itemStack.itemValue.HasQuality)
			{
				flag = true;
				if (EntityPlayerLocal.DegradeOnDeathType == EntityPlayerLocal.DegradeOnDeathTypes.MaxDurability || EntityPlayerLocal.DegradeOnDeathType == EntityPlayerLocal.DegradeOnDeathTypes.Both)
				{
					float num = itemStack.itemValue.MaxDurabilityModifier;
					num -= EntityPlayerLocal.DegradeAmountOnDeath;
					if (num < EntityPlayerLocal.DegradeAmountOnDeath)
					{
						num = EntityPlayerLocal.DegradeAmountOnDeath;
					}
					itemStack.itemValue.MaxDurabilityModifier = num;
				}
				if (EntityPlayerLocal.DegradeOnDeathType == EntityPlayerLocal.DegradeOnDeathTypes.Durability || EntityPlayerLocal.DegradeOnDeathType == EntityPlayerLocal.DegradeOnDeathTypes.Both)
				{
					int maxUseTimes = itemStack.itemValue.MaxUseTimes;
					int num2 = (int)((float)maxUseTimes * EntityPlayerLocal.DegradeAmountOnDeath);
					itemStack.itemValue.UseTimes += (float)num2;
					if (itemStack.itemValue.UseTimes > (float)maxUseTimes)
					{
						itemStack.itemValue.UseTimes = (float)maxUseTimes;
					}
				}
			}
		}
		if (flag)
		{
			ItemStack[] slots;
			this.bag.SetSlots(slots);
		}
		for (int j = 0; j < this.equipment.GetSlotCount(); j++)
		{
			ItemValue slotItem = this.equipment.GetSlotItem(j);
			if (slotItem != null && !slotItem.IsEmpty() && slotItem.HasQuality)
			{
				flag = false;
				if (EntityPlayerLocal.DegradeOnDeathType == EntityPlayerLocal.DegradeOnDeathTypes.MaxDurability || EntityPlayerLocal.DegradeOnDeathType == EntityPlayerLocal.DegradeOnDeathTypes.Both)
				{
					float num3 = slotItem.MaxDurabilityModifier;
					num3 -= EntityPlayerLocal.DegradeAmountOnDeath;
					if (num3 < EntityPlayerLocal.DegradeAmountOnDeath)
					{
						num3 = EntityPlayerLocal.DegradeAmountOnDeath;
					}
					slotItem.MaxDurabilityModifier = num3;
					flag = true;
				}
				if (EntityPlayerLocal.DegradeOnDeathType == EntityPlayerLocal.DegradeOnDeathTypes.Durability || EntityPlayerLocal.DegradeOnDeathType == EntityPlayerLocal.DegradeOnDeathTypes.Both)
				{
					int maxUseTimes2 = slotItem.MaxUseTimes;
					int num4 = (int)((float)maxUseTimes2 * EntityPlayerLocal.DegradeAmountOnDeath);
					slotItem.UseTimes += (float)num4;
					if (slotItem.UseTimes > (float)maxUseTimes2)
					{
						slotItem.UseTimes = (float)maxUseTimes2;
					}
					flag = true;
				}
				if (flag)
				{
					this.equipment.SetSlotItem(j, slotItem, true);
				}
			}
		}
		flag = false;
		foreach (ItemStack itemStack2 in this.inventory.GetSlots())
		{
			if (itemStack2.itemValue.HasQuality)
			{
				flag = true;
				if (EntityPlayerLocal.DegradeOnDeathType == EntityPlayerLocal.DegradeOnDeathTypes.MaxDurability || EntityPlayerLocal.DegradeOnDeathType == EntityPlayerLocal.DegradeOnDeathTypes.Both)
				{
					float num5 = itemStack2.itemValue.MaxDurabilityModifier;
					num5 -= EntityPlayerLocal.DegradeAmountOnDeath;
					if (num5 < EntityPlayerLocal.DegradeAmountOnDeath)
					{
						num5 = EntityPlayerLocal.DegradeAmountOnDeath;
					}
					itemStack2.itemValue.MaxDurabilityModifier = num5;
				}
				if (EntityPlayerLocal.DegradeOnDeathType == EntityPlayerLocal.DegradeOnDeathTypes.Durability || EntityPlayerLocal.DegradeOnDeathType == EntityPlayerLocal.DegradeOnDeathTypes.Both)
				{
					int maxUseTimes3 = itemStack2.itemValue.MaxUseTimes;
					int num6 = (int)((float)maxUseTimes3 * EntityPlayerLocal.DegradeAmountOnDeath);
					itemStack2.itemValue.UseTimes += (float)num6;
					if (itemStack2.itemValue.UseTimes > (float)maxUseTimes3)
					{
						itemStack2.itemValue.UseTimes = (float)maxUseTimes3;
					}
				}
			}
		}
		if (flag)
		{
			ItemStack[] slots2;
			this.inventory.SetSlots(slots2, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void dropBackpack(bool _isDying)
	{
		EntityPlayerLocal.DropOption dropOption = (EntityPlayerLocal.DropOption)(_isDying ? GameStats.GetInt(EnumGameStats.DropOnDeath) : GameStats.GetInt(EnumGameStats.DropOnQuit));
		if (dropOption == EntityPlayerLocal.DropOption.None)
		{
			return;
		}
		if (string.IsNullOrEmpty(this.dropInventoryBlock))
		{
			return;
		}
		if (this.playerUI.xui != null)
		{
			this.playerUI.xui.CancelAllCrafting();
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		bool flag = dropOption == EntityPlayerLocal.DropOption.All || dropOption == EntityPlayerLocal.DropOption.Backpack || dropOption == EntityPlayerLocal.DropOption.ToolbeltBackpack;
		bool flag2 = dropOption == EntityPlayerLocal.DropOption.All || dropOption == EntityPlayerLocal.DropOption.Toolbelt || dropOption == EntityPlayerLocal.DropOption.ToolbeltBackpack;
		bool flag3 = dropOption == EntityPlayerLocal.DropOption.All || dropOption == EntityPlayerLocal.DropOption.Equipment;
		ItemStack[] slots = this.bag.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (this.dropStackCondition(slots[i]))
			{
				num++;
			}
		}
		for (int j = 0; j < this.inventory.GetItemCount(); j++)
		{
			if (j != this.inventory.DUMMY_SLOT_IDX)
			{
				ItemStack item = this.inventory.GetItem(j);
				if (this.dropStackCondition(item))
				{
					num2++;
				}
			}
		}
		int num4 = 4;
		int num5 = 7;
		for (int k = 0; k < this.equipment.GetSlotCount(); k++)
		{
			if (k < num4 || k > num5)
			{
				ItemValue slotItem = this.equipment.GetSlotItem(k);
				if (this.dropValueCondition(slotItem))
				{
					num3++;
				}
			}
		}
		if (num == 0 && num2 == 0 && num3 == 0)
		{
			return;
		}
		if (_isDying && dropOption == EntityPlayerLocal.DropOption.DeleteAll)
		{
			ItemStack[] slots2 = this.bag.GetSlots();
			for (int l = 0; l < slots2.Length; l++)
			{
				slots2[l] = ItemStack.Empty;
			}
			this.bag.SetSlots(slots2);
			for (int m = 0; m < this.inventory.GetItemCount(); m++)
			{
				this.inventory.SetItem(m, ItemStack.Empty);
			}
			for (int n = 0; n < 12; n++)
			{
				ItemValue slotItem2 = this.equipment.GetSlotItem(n);
				if (slotItem2 != null && !slotItem2.ItemClassOrMissing.KeepOnDeath())
				{
					this.equipment.SetSlotItem(n, null, true);
				}
			}
			return;
		}
		EntityBackpack entityBackpack = EntityFactory.CreateEntity("Backpack".GetHashCode(), this.position + base.transform.up * 2f) as EntityBackpack;
		PreferenceTracker preferenceTracker = new PreferenceTracker(this.entityId);
		if (flag)
		{
			ItemStack[] slots3 = this.bag.GetSlots();
			preferenceTracker.SetBag(slots3, this.dropStackCondition);
			for (int num6 = 0; num6 < slots3.Length; num6++)
			{
				if (this.dropStackCondition(slots3[num6]))
				{
					entityBackpack.bag.AddItem(slots3[num6]);
					slots3[num6] = ItemStack.Empty;
				}
			}
			this.bag.SetSlots(slots3);
		}
		if (flag3)
		{
			ItemValue[] array = new ItemValue[12];
			for (int num7 = 0; num7 < array.Length; num7++)
			{
				ItemValue slotItem3 = this.equipment.GetSlotItem(num7);
				ItemValue itemValue = ((slotItem3 != null) ? slotItem3.Clone() : null);
				if (this.dropValueCondition(itemValue))
				{
					array[num7] = itemValue;
					ItemStack itemStack = new ItemStack(itemValue, 1);
					entityBackpack.bag.AddItem(itemStack);
					if (!_isDying)
					{
						this.equipment.SetSlotItem(num7, null, true);
					}
				}
			}
			preferenceTracker.SetEquipment(array, this.dropValueCondition);
		}
		if (flag2)
		{
			ItemStack[] array2 = new ItemStack[this.inventory.GetItemCount()];
			for (int num8 = 0; num8 < array2.Length; num8++)
			{
				if (num8 != this.inventory.DUMMY_SLOT_IDX)
				{
					ItemStack item2 = this.inventory.GetItem(num8);
					array2[num8] = item2;
					if (this.dropStackCondition(item2))
					{
						entityBackpack.bag.AddItem(item2);
						this.inventory.SetItem(num8, ItemStack.Empty);
					}
				}
			}
			preferenceTracker.SetToolbelt(array2, this.dropStackCondition);
		}
		if (preferenceTracker.AnyPreferences)
		{
			entityBackpack.bag.preferences = preferenceTracker;
		}
		entityBackpack.RefPlayerId = this.entityId;
		EntityCreationData entityCreationData = new EntityCreationData(entityBackpack, true);
		entityCreationData.entityName = string.Format(Localization.Get("playersBackpack", false, null), this.EntityName);
		entityCreationData.id = -1;
		entityCreationData.bag = entityBackpack.bag.Clone();
		GameManager.Instance.RequestToSpawnEntityServer(entityCreationData);
		entityBackpack.OnEntityUnload();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.SetDroppedBackpackPositions(GameManager.Instance.persistentLocalPlayer.GetDroppedBackpackPositions());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleRemoveRandomItems(ItemStack[] items)
	{
		for (int i = this.DestroyLocationSlotList.Count - 1; i >= 0; i--)
		{
			if (this.DestroyLocationSlotList[i].Location == EntityPlayerLocal.LocationSlotInfo.LocationTypes.Backpack)
			{
				items[this.DestroyLocationSlotList[i].Slot].Clear();
				this.DestroyLocationSlotList.RemoveAt(i);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleRemoveRandomItems(Inventory inventory)
	{
		for (int i = this.DestroyLocationSlotList.Count - 1; i >= 0; i--)
		{
			if (this.DestroyLocationSlotList[i].Location == EntityPlayerLocal.LocationSlotInfo.LocationTypes.Toolbelt)
			{
				inventory.SetItem(this.DestroyLocationSlotList[i].Slot, ItemStack.Empty);
				this.DestroyLocationSlotList.RemoveAt(i);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleRemoveRandomItems(Equipment equipment)
	{
		for (int i = this.DestroyLocationSlotList.Count - 1; i >= 0; i--)
		{
			if (this.DestroyLocationSlotList[i].Location == EntityPlayerLocal.LocationSlotInfo.LocationTypes.Equipment)
			{
				equipment.SetSlotItem(this.DestroyLocationSlotList[i].Slot, null, true);
				this.DestroyLocationSlotList.RemoveAt(i);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool ShouldRemoveEquipmentOnDeath(EntityPlayerLocal.DropOption dropOption)
	{
		return dropOption == EntityPlayerLocal.DropOption.All || dropOption == EntityPlayerLocal.DropOption.DeleteAll;
	}

	public override int AttachToEntity(Entity _other, int slot = -1)
	{
		if (_other.IsAttached(this))
		{
			return -1;
		}
		vp_FPController vp_FPController = this.vp_FPController;
		vp_FPController.enabled = true;
		Transform transform = this.m_vp_FPCamera.Transform;
		Vector3 position = transform.position;
		Quaternion rotation = transform.rotation;
		this.SetFirstPersonView(false, true);
		slot = base.AttachToEntity(_other, slot);
		if (slot >= 0)
		{
			transform.position = position;
			transform.rotation = rotation;
			vp_FPController.Stop();
			vp_FPController.Player.Driving.Start(0f);
		}
		else
		{
			this.SwitchToPreferredCameraMode(false);
		}
		EntityVehicle entityVehicle = _other as EntityVehicle;
		if (entityVehicle && entityVehicle.LocalPlayerIsOwner())
		{
			this.Waypoints.UpdateEntityVehicleWayPoint(entityVehicle, false);
			this.Waypoints.SetWaypointHiddenOnMap(this.entityId, true);
		}
		Manager.BroadcastStop(this.entityId, "Player" + (this.IsMale ? "Male" : "Female") + "RunLoop");
		this.sprintLoopSoundPlayId = -1;
		return slot;
	}

	public override void Detach()
	{
		this.SwitchToPreferredCameraMode(true);
		this.moveController.isAutorun = false;
		Vector3 vector = Vector3.zero;
		EntityVehicle entityVehicle = this.AttachedToEntity as EntityVehicle;
		if (entityVehicle)
		{
			vector = entityVehicle.GetExitVelocity() * Time.fixedDeltaTime;
			if (entityVehicle.LocalPlayerIsOwner())
			{
				this.Waypoints.UpdateEntityVehicleWayPoint(entityVehicle, false);
			}
		}
		base.Detach();
		vp_FPController vp_FPController = this.vp_FPController;
		vp_FPController.Player.Driving.Stop(0f);
		vp_FPController.Stop();
		vp_FPController.m_MaxHeightInitialFallSpeed = Utils.FastMin(vector.y, 0f);
		vp_FPController.AddForce(vector);
	}

	public override void ProcessDamageResponseLocal(DamageResponse _dmResponse)
	{
		base.ProcessDamageResponseLocal(_dmResponse);
		if (_dmResponse.Source.damageType != EnumDamageTypes.Weather)
		{
			this.healthLostThisRound = _dmResponse.Strength > 0;
		}
	}

	public override bool CanUpdateEntity()
	{
		return this.IsFlyMode.Value || base.CanUpdateEntity();
	}

	public override void OnHUD()
	{
		if (Event.current.type != 7)
		{
			return;
		}
		NGuiWdwInGameHUD inGameHUD = this.nguiWindowManager.InGameHUD;
		if (!GameManager.Instance.gameStateManager.IsGameStarted() || GameStats.GetInt(EnumGameStats.GameState) != 1 || !this.Spawned)
		{
			return;
		}
		bool flag = this.windowManager.IsModalWindowOpen() || LocalPlayerUI.primaryUI.windowManager.IsModalWindowOpen();
		this.guiDrawOverlayTextures(inGameHUD, flag);
		if (this.windowManager.IsFullHUDDisabled())
		{
			return;
		}
		if (this.inventory != null && !flag)
		{
			this.inventory.holdingItem.OnHUD(this.inventory.holdingItemData, Screen.width - 10, Screen.height - 10);
		}
		if (!LocalPlayerUI.primaryUI.windowManager.IsModalWindowOpen() && !this.windowManager.IsWindowOpen("toolbelt") && !this.windowManager.IsWindowOpen(XUiC_InGameMenuWindow.ID) && !this.windowManager.IsWindowOpen("dialog") && !this.windowManager.IsWindowOpen("tipWindow") && !this.windowManager.IsWindowOpen("questOffer"))
		{
			this.windowManager.Open("toolbelt", false);
		}
		this.windowManager.Open(this.playerUI.xui.CalloutWindow.WindowGroup, false);
		if (!this.windowManager.IsModalWindowOpen() && !this.windowManager.IsWindowOpen(XUiC_CompassWindow.ID) && !LocalPlayerUI.primaryUI.windowManager.IsModalWindowOpen())
		{
			this.windowManager.Open(XUiC_CompassWindow.ID, false);
		}
		this.windowManager.Open(this.playerUI.xui.ToolTipWindow.WindowGroup, false);
		this.windowManager.Open(XUiC_ChatOutput.ID, false);
		if (Event.current.type == 7)
		{
			if (this.sneakDamageBlendTimer.Value > 0f)
			{
				this.nguiWindowManager.SetLabel(EnumNGUIWindow.CriticalHitText, this.sneakDamageText, new Color?(new Color(1f, 1f, 1f, this.sneakDamageBlendTimer.Value)), true);
			}
			else if (this.nguiWindowManager.IsShowing(EnumNGUIWindow.CriticalHitText))
			{
				this.nguiWindowManager.Show(EnumNGUIWindow.CriticalHitText, false);
			}
		}
		this.guiDrawCrosshair(inGameHUD, flag);
	}

	public void ForceBloodSplatter()
	{
		this.healthLostThisRound = true;
		this.lastHitDirection = Utils.EnumHitDirection.Front;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void guiDrawOverlayTextures(NGuiWdwInGameHUD _guiInGame, bool bModalWindowOpen)
	{
		if (Event.current.type != 7)
		{
			return;
		}
		this.inventory.holdingItem.OnScreenOverlay(this.inventory.holdingItemData);
		Vector3 vector = this.finalCamera.ViewportToScreenPoint(new Vector3(0f, 0f, 0f));
		Vector3 vector2 = this.finalCamera.ViewportToScreenPoint(new Vector3(1f, 1f, 0f));
		if (this.healthLostThisRound && !this.IsDead() && this.lastHitDirection != Utils.EnumHitDirection.None)
		{
			this.healthLostThisRound = false;
			int lastHitDirection = (int)this.lastHitDirection;
			this.overlayDirectionTime[lastHitDirection] = 6f;
			byte[] array = this.overlayAlternating;
			int num = lastHitDirection;
			array[num] += 1;
			int num2 = lastHitDirection * 2 + (int)(this.overlayAlternating[lastHitDirection] & 1);
			this.overlayDirectionTime[num2] = 6f;
			this.overlayBloodDropsPositions[num2 * 3] = new Vector2(this.rand.RandomRange(vector.x, vector2.x), this.rand.RandomRange(vector.y, vector2.y));
			this.overlayBloodDropsPositions[num2 * 3 + 1] = new Vector2(this.rand.RandomRange(vector.x, vector2.x), this.rand.RandomRange(vector.y, vector2.y));
			this.overlayBloodDropsPositions[num2 * 3 + 2] = new Vector2(this.rand.RandomRange(vector.x, vector2.x), this.rand.RandomRange(vector.y, vector2.y));
			this.lastHitDirection = Utils.EnumHitDirection.None;
		}
		for (int i = 0; i < 8; i++)
		{
			if (this.overlayDirectionTime[i] > 0f)
			{
				float num3 = Mathf.Pow(1f - this.overlayDirectionTime[i] / 6f, 0.28f);
				this.overlayMaterial.SetColor("_Color", new Color(num3, num3, num3));
				if (this.windowManager.IsHUDEnabled())
				{
					Vector3 vector3 = (vector2 + vector) * 0.5f;
					int pixelWidth = this.finalCamera.pixelWidth;
					int pixelHeight = this.finalCamera.pixelHeight;
					Texture2D texture2D = _guiInGame.overlayDamageTextures[i];
					float num4 = (float)pixelHeight / 512f;
					float num5 = (float)texture2D.width * num4;
					float num6 = (float)texture2D.height * num4;
					Rect rect;
					rect..ctor(vector3.x - num5 * 0.5f, 0f, num5, num6);
					int num7 = i >> 1;
					if (num7 == 1)
					{
						rect.y = (float)pixelHeight - num6;
					}
					else if (num7 >= 2)
					{
						rect.x = (float)pixelWidth - num5;
						rect.y = vector3.y - num6 * 0.5f;
						if (num7 == 3)
						{
							rect.x = 0f;
						}
					}
					Graphics.DrawTexture(rect, texture2D, this.overlayMaterial);
					int num8 = i * 3;
					Graphics.DrawTexture(new Rect(this.overlayBloodDropsPositions[num8].x, (float)pixelHeight - this.overlayBloodDropsPositions[num8].y, (float)_guiInGame.overlayDamageBloodDrops[0].width, (float)_guiInGame.overlayDamageBloodDrops[0].height), _guiInGame.overlayDamageBloodDrops[0], this.overlayMaterial);
					Graphics.DrawTexture(new Rect(this.overlayBloodDropsPositions[num8 + 1].x, (float)pixelHeight - this.overlayBloodDropsPositions[num8 + 1].y, (float)_guiInGame.overlayDamageBloodDrops[1].width, (float)_guiInGame.overlayDamageBloodDrops[1].height), _guiInGame.overlayDamageBloodDrops[1], this.overlayMaterial);
					Graphics.DrawTexture(new Rect(this.overlayBloodDropsPositions[num8 + 2].x, (float)pixelHeight - this.overlayBloodDropsPositions[num8 + 2].y, (float)_guiInGame.overlayDamageBloodDrops[2].width, (float)_guiInGame.overlayDamageBloodDrops[2].height), _guiInGame.overlayDamageBloodDrops[2], this.overlayMaterial);
				}
				if (this.Health > 0)
				{
					this.overlayDirectionTime[i] -= Time.deltaTime;
				}
				else
				{
					this.overlayDirectionTime[i] = 0f;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CrosshairAlpha(NGuiWdwInGameHUD _guiInGame)
	{
		return 1f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void guiDrawCrosshair(NGuiWdwInGameHUD _guiInGame, bool bModalWindowOpen)
	{
		if (!_guiInGame.showCrosshair)
		{
			return;
		}
		if (Event.current.type != 7)
		{
			return;
		}
		if (this.IsDead() || this.emodel.IsRagdollActive)
		{
			return;
		}
		if (this.AttachedToEntity != null)
		{
			return;
		}
		if (!GamePrefs.GetBool(EnumGamePrefs.OptionsCrosshairEnabled))
		{
			return;
		}
		ItemClass.EnumCrosshairType enumCrosshairType = this.inventory.holdingItem.GetCrosshairType(this.inventory.holdingItemData);
		if (enumCrosshairType == ItemClass.EnumCrosshairType.None)
		{
			if (this.bFirstPersonView || !GamePrefs.GetBool(EnumGamePrefs.OptionsCrosshairDot3P))
			{
				return;
			}
			enumCrosshairType = ItemClass.EnumCrosshairType.Dot;
		}
		if (enumCrosshairType == ItemClass.EnumCrosshairType.Crosshair || (enumCrosshairType == ItemClass.EnumCrosshairType.CrosshairOnAiming && !GamePrefs.GetBool(EnumGamePrefs.OptionsCrosshairRangedEnabled)))
		{
			enumCrosshairType = ItemClass.EnumCrosshairType.Plus;
		}
		if (!GamePrefs.GetBool(EnumGamePrefs.OptionsCrosshairADS) && ((this.bFirstPersonView && this.AimingGun) || this.isAimingScoped))
		{
			return;
		}
		if (!bModalWindowOpen && this.inventory != null)
		{
			float @float = GamePrefs.GetFloat(EnumGamePrefs.OptionsCrosshairScale);
			float float2 = GamePrefs.GetFloat(EnumGamePrefs.OptionsCrosshairOpacity);
			int num = GamePrefs.GetInt(EnumGamePrefs.OptionsCrosshairColor);
			if (num < 0 || num >= EntityPlayerLocal.crosshairColors.Count)
			{
				num = 0;
			}
			Color color = EntityPlayerLocal.crosshairColors[num];
			Vector2 crosshairPosition2D = this.GetCrosshairPosition2D();
			crosshairPosition2D.y = (float)Screen.height - crosshairPosition2D.y;
			float num2 = (float)Screen.height * 0.059f;
			float num3 = num2 * @float;
			Color color2 = GUI.color;
			switch (enumCrosshairType)
			{
			case ItemClass.EnumCrosshairType.Plus:
				GUI.color = new Color(color.r, color.g, color.b, float2);
				GUI.DrawTexture(new Rect(crosshairPosition2D.x - num3 / 2f, crosshairPosition2D.y - num3 / 2f, num3, num3), _guiInGame.CrosshairTexture, 0);
				GUI.color = color2;
				return;
			case ItemClass.EnumCrosshairType.Crosshair:
			case ItemClass.EnumCrosshairType.CrosshairOnAiming:
				if (enumCrosshairType != ItemClass.EnumCrosshairType.Crosshair || !this.AimingGun || ItemAction.ShowDistanceDebugInfo)
				{
					float num4 = EffectManager.GetValue(PassiveEffects.SpreadDegreesHorizontal, this.inventory.holdingItemData.itemValue, 90f, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
					num4 *= 0.5f;
					num4 *= (this.inventory.holdingItemData.actionData[0] as ItemActionRanged.ItemActionDataRanged).lastAccuracy;
					num4 *= (float)Screen.width / this.cameraTransform.GetComponent<Camera>().fieldOfView;
					float num5 = EffectManager.GetValue(PassiveEffects.SpreadDegreesVertical, this.inventory.holdingItemData.itemValue, 90f, this, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
					num5 *= 0.5f;
					num5 *= (this.inventory.holdingItemData.actionData[0] as ItemActionRanged.ItemActionDataRanged).lastAccuracy;
					num5 *= (float)Screen.width / this.cameraTransform.GetComponent<Camera>().fieldOfView;
					float x = crosshairPosition2D.x;
					float y = crosshairPosition2D.y;
					float num6 = num3 / 3f;
					EntityPlayerLocal.DrawDynamicCrosshair(x, y, num4, num5, num6, GamePrefs.GetFloat(EnumGamePrefs.OptionsCrosshairThickness), float2, color);
					return;
				}
				break;
			case ItemClass.EnumCrosshairType.Damage:
				if (this.playerUI.xui.BackgroundGlobalOpacity < 1f)
				{
					float num7 = color2.a * this.playerUI.xui.BackgroundGlobalOpacity;
					GUI.color = new Color(color2.r, color2.g, color2.b, num7);
				}
				else
				{
					GUI.color = new Color(color.r, color.g, color.b, this.CrosshairAlpha(_guiInGame));
				}
				GUI.DrawTexture(new Rect(crosshairPosition2D.x - num2 / 2f, crosshairPosition2D.y - num2 / 2f, num2, num2), _guiInGame.CrosshairDamage, 0);
				GUI.color = color2;
				return;
			case ItemClass.EnumCrosshairType.Upgrade:
				if (this.playerUI.xui.BackgroundGlobalOpacity < 1f)
				{
					float num8 = color2.a * this.playerUI.xui.BackgroundGlobalOpacity;
					GUI.color = new Color(color2.r, color2.g, color2.b, num8);
				}
				else
				{
					GUI.color = new Color(color2.r, color2.g, color2.b, this.CrosshairAlpha(_guiInGame));
				}
				GUI.DrawTexture(new Rect(crosshairPosition2D.x - num2 / 2f, crosshairPosition2D.y - num2 / 2f, num2, num2), _guiInGame.CrosshairUpgrade, 0);
				GUI.color = color2;
				return;
			case ItemClass.EnumCrosshairType.Repair:
				if (this.playerUI.xui.BackgroundGlobalOpacity < 1f)
				{
					float num9 = color2.a * this.playerUI.xui.BackgroundGlobalOpacity;
					GUI.color = new Color(color2.r, color2.g, color2.b, num9);
				}
				else
				{
					GUI.color = new Color(color2.r, color2.g, color2.b, this.CrosshairAlpha(_guiInGame));
				}
				GUI.DrawTexture(new Rect(crosshairPosition2D.x - num2 / 2f, crosshairPosition2D.y - num2 / 2f, num2, num2), _guiInGame.CrosshairRepair, 0);
				GUI.color = color2;
				return;
			case ItemClass.EnumCrosshairType.PowerSource:
				if (this.playerUI.xui.BackgroundGlobalOpacity < 1f)
				{
					float num10 = color2.a * this.playerUI.xui.BackgroundGlobalOpacity;
					GUI.color = new Color(color2.r, color2.g, color2.b, num10);
				}
				else
				{
					GUI.color = new Color(color2.r, color2.g, color2.b, this.CrosshairAlpha(_guiInGame));
				}
				GUI.DrawTexture(new Rect(crosshairPosition2D.x - num2 / 2f, crosshairPosition2D.y - num2 / 2f, num2, num2), _guiInGame.CrosshairPowerSource, 0);
				GUI.color = color2;
				return;
			case ItemClass.EnumCrosshairType.Heal:
				if (this.playerUI.xui.BackgroundGlobalOpacity < 1f)
				{
					float num11 = color2.a * this.playerUI.xui.BackgroundGlobalOpacity;
					GUI.color = new Color(color2.r, color2.g, color2.b, num11);
				}
				else
				{
					GUI.color = new Color(color2.r, color2.g, color2.b, this.CrosshairAlpha(_guiInGame));
				}
				GUI.DrawTexture(new Rect(crosshairPosition2D.x - num2 / 2f, crosshairPosition2D.y - num2 / 2f, num2, num2), _guiInGame.CrosshairRepair, 0);
				GUI.color = color2;
				return;
			case ItemClass.EnumCrosshairType.PowerItem:
				if (this.playerUI.xui.BackgroundGlobalOpacity < 1f)
				{
					float num12 = color2.a * this.playerUI.xui.BackgroundGlobalOpacity;
					GUI.color = new Color(color2.r, color2.g, color2.b, num12);
				}
				else
				{
					GUI.color = new Color(color2.r, color2.g, color2.b, this.CrosshairAlpha(_guiInGame));
				}
				GUI.DrawTexture(new Rect(crosshairPosition2D.x - num2 / 2f, crosshairPosition2D.y - num2 / 2f, num2, num2), _guiInGame.CrosshairPowerItem, 0);
				GUI.color = color2;
				return;
			case ItemClass.EnumCrosshairType.Blocked:
				GUI.color = new Color(1f, 0.5f, 0f, float2 * 0.5f);
				GUI.DrawTexture(new Rect(crosshairPosition2D.x - num2 / 4f, crosshairPosition2D.y - num2 / 4f, num2 / 2f, num2 / 2f), _guiInGame.CrosshairBlocked, 0);
				GUI.color = color2;
				return;
			case ItemClass.EnumCrosshairType.Dot:
				GUI.color = new Color(color.r, color.g, color.b, float2);
				GUI.DrawTexture(new Rect(crosshairPosition2D.x - num2 / 4f, crosshairPosition2D.y - num2 / 4f, num2 / 2f, num2 / 2f), _guiInGame.CrosshairDot, 0);
				GUI.color = color2;
				break;
			default:
				return;
			}
		}
	}

	public static void DrawDynamicCrosshair(float centerX, float centerY, float openAreaX, float openAreaY, float length, float thicknessMultiplier, float crosshairOpacity, Color crosshairColor)
	{
		NGuiWdwInGameHUD inGameHUD = LocalPlayerUI.primaryUI.nguiWindowManager.InGameHUD;
		float num = (float)Screen.height * 0.005f * thicknessMultiplier;
		float num2 = num / 3f;
		Color color = GUI.color;
		Color color2 = new Color(0f, 0f, 0f, crosshairOpacity * 0.5f);
		Color color3;
		color3..ctor(crosshairColor.r, crosshairColor.g, crosshairColor.b, crosshairOpacity);
		GUI.color = color2;
		GUI.DrawTexture(new Rect(centerX - openAreaX - length - num2, centerY - num / 2f + num2, length, num), inGameHUD.CrosshairSquare, 0);
		GUI.color = color3;
		GUI.DrawTexture(new Rect(centerX - openAreaX - length, centerY - num / 2f, length, num), inGameHUD.CrosshairSquare, 0);
		GUI.color = color2;
		GUI.DrawTexture(new Rect(centerX + openAreaX + num2, centerY - num / 2f + num2, length, num), inGameHUD.CrosshairSquare, 0);
		GUI.color = color3;
		GUI.DrawTexture(new Rect(centerX + openAreaX, centerY - num / 2f, length, num), inGameHUD.CrosshairSquare, 0);
		GUI.color = color2;
		GUI.DrawTexture(new Rect(centerX - num + num / 2f - num2, centerY + openAreaY + num2, num, length), inGameHUD.CrosshairSquare, 0);
		GUI.color = color3;
		GUI.DrawTexture(new Rect(centerX - num + num / 2f, centerY + openAreaY, num, length), inGameHUD.CrosshairSquare, 0);
		GUI.color = color2;
		GUI.DrawTexture(new Rect(centerX - num + num / 2f - num2, centerY - openAreaY - length + num2, num, length), inGameHUD.CrosshairSquare, 0);
		GUI.color = color3;
		GUI.DrawTexture(new Rect(centerX - num + num / 2f, centerY - openAreaY - length, num, length), inGameHUD.CrosshairSquare, 0);
		GUI.color = color;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShelterFrameUpdate()
	{
		if (!this.shelterIsUpdating)
		{
			this.shelterIsUpdating = true;
			this.shelterStartPos = this.position - Origin.position;
			this.shelterPos = this.shelterStartPos;
			this.shelterPos.y = this.shelterPos.y + 0.62f;
			this.shelterSideCount = -10;
		}
		if (this.shelterSideCount == -10)
		{
			this.shelterSideCount = -2;
			this.ShelterCheckSkyUp();
			this.shelterAbovePercent = 1f;
			if (!this.shelterIsUpdating)
			{
				this.shelterAbovePercent = 0f;
				this.shelterPercent = 0f;
				return;
			}
		}
		else if (this.shelterSideCount < 0)
		{
			this.shelterSideCount++;
			this.ShelterCheckSkyDiagonal();
			this.shelterPos.y = this.shelterPos.y + 0.1f;
			if (!this.shelterIsUpdating)
			{
				this.shelterPercent = 0f;
				return;
			}
			if (this.shelterSideCount == 0)
			{
				this.shelterStartPos.y = this.shelterStartPos.y + 0.48f;
				this.shelterStartPos.x = this.shelterStartPos.x - 0.13f;
				this.shelterStartPos.z = this.shelterStartPos.z - 0.13f;
				this.shelterPosOffset = Vector3.zero;
				return;
			}
		}
		else
		{
			if (this.shelterSideCount == 0)
			{
				this.shelterSideCount = 1;
				this.shelterPos = this.shelterStartPos + this.shelterPosOffset;
				this.shelterDir = 0f;
				this.shelterRadius = 1f;
			}
			this.ShelterCheckSides();
			if (!this.shelterIsUpdating)
			{
				this.shelterPercent = 0f;
				return;
			}
			if (this.shelterSideCount >= 4)
			{
				if (this.shelterPosOffset.y < 0.5f)
				{
					this.shelterPosOffset.y = this.shelterPosOffset.y + (base.IsCrouching ? 0.54f : 0.65f);
					this.shelterPosOffset.x = this.shelterPosOffset.x + 0.011f;
					this.shelterPosOffset.z = this.shelterPosOffset.z + 0.013f;
					this.shelterSideCount = 0;
					return;
				}
				this.shelterIsUpdating = false;
				this.shelterPercent = 1f;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShelterCheckSkyUp()
	{
		PhysicsScene defaultPhysicsScene = Physics.defaultPhysicsScene;
		for (int i = 0; i < 4; i++)
		{
			Vector3 vector = this.shelterPos;
			float num = this.shelterDir * 0.017453292f;
			vector.x += (float)Math.Cos((double)num) * 0.18f;
			vector.z += (float)Math.Sin((double)num) * 0.18f;
			RaycastHit raycastHit;
			if (!defaultPhysicsScene.Raycast(vector, Vector3.up, ref raycastHit, 253f, 1073807360, 0))
			{
				this.shelterIsUpdating = false;
				return;
			}
			this.shelterDir += 90f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShelterCheckSkyDiagonal()
	{
		PhysicsScene defaultPhysicsScene = Physics.defaultPhysicsScene;
		Vector3 vector;
		vector.y = 0.75f + (float)(this.shelterSideCount + 1) * 0.13f;
		for (int i = 0; i < 8; i++)
		{
			float num = this.shelterDir * 0.017453292f;
			vector.x = (float)Math.Cos((double)num);
			vector.z = (float)Math.Sin((double)num);
			RaycastHit raycastHit;
			if (!defaultPhysicsScene.Raycast(this.shelterPos, vector, ref raycastHit, 253f, 1073807360, 0))
			{
				this.shelterIsUpdating = false;
				return;
			}
			this.shelterDir += 45f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShelterCheckSides()
	{
		PhysicsScene defaultPhysicsScene = Physics.defaultPhysicsScene;
		Vector3 vector;
		vector.y = 0f;
		for (int i = 0; i < 8; i++)
		{
			float num = this.shelterDir * 0.017453292f;
			float num2 = (float)Math.Cos((double)num);
			float num3 = (float)Math.Sin((double)num);
			vector.x = num2 * this.shelterRadius;
			vector.z = num3 * this.shelterRadius;
			Vector3 vector2 = this.shelterPos + vector;
			RaycastHit raycastHit;
			if (defaultPhysicsScene.Raycast(this.shelterPos, vector, ref raycastHit, this.shelterRadius, 1073807360, 0))
			{
				vector2 = raycastHit.point;
				vector2.x -= num2 * 0.02f;
				vector2.z -= num3 * 0.02f;
			}
			if (!defaultPhysicsScene.Raycast(vector2, Vector3.up, ref raycastHit, 253f, 1073807360, 0))
			{
				this.shelterIsUpdating = false;
				break;
			}
			this.shelterDir += 45f;
		}
		this.shelterDir += -359f;
		this.shelterRadius += 1f;
		if (this.shelterRadius > 3.1f)
		{
			this.shelterRadius = 0f;
			this.shelterSideCount++;
			this.shelterPos.x = this.shelterPos.x + 0.13f;
			this.shelterPos.z = this.shelterPos.z + 0.13f;
		}
	}

	public void WeatherStatusFrameUpdate()
	{
		bool flag = this.AreaMessage != null && this.stormAlertBiomeStandingOn == this.biomeStandingOn;
		this.AreaMessage = null;
		this.isPlayerInStorm = false;
		if (!EntityPlayerLocal.StormWarning)
		{
			return;
		}
		if (this.biomeStandingOn != null)
		{
			int stormLevel = this.biomeStandingOn.currentWeatherGroup.stormLevel;
			if (stormLevel > 0 && this.biomeStandingOn.Difficulty >= 2)
			{
				if (stormLevel >= 2)
				{
					this.isPlayerInStorm = !this.isIndoorsCurrent;
				}
				this.AreaMessageAlpha = 1f;
				string text = "weatherStormBuild";
				if (stormLevel == 1)
				{
					if (this.isIndoorsCurrent)
					{
						text = "weatherStormBuildSafe";
						this.AreaMessageAlpha = 0.85f;
					}
				}
				else if (stormLevel >= 2)
				{
					text = "weatherStorm";
					if (this.isIndoorsCurrent)
					{
						text = "weatherStormSafe";
						this.AreaMessageAlpha = 0.75f;
					}
				}
				if (stormLevel == 1)
				{
					int remainingSeconds = (int)WeatherManager.currentWeather.remainingSeconds;
					string text2 = "";
					if (remainingSeconds <= 60)
					{
						if (!flag || (float)remainingSeconds > this.stormRemainingSeconds)
						{
							this.stormRemainingSeconds = (float)remainingSeconds;
						}
						this.stormRemainingSeconds = Mathf.MoveTowards(this.stormRemainingSeconds, (float)remainingSeconds, Time.deltaTime);
						text2 = string.Format("{0:d2}", (int)(this.stormRemainingSeconds + 0.99f));
					}
					this.AreaMessage = string.Format(Localization.Get(text, false, null), text2);
				}
				else
				{
					this.AreaMessage = Localization.Get(text, false, null);
				}
				if (!flag)
				{
					Manager.PlayInsidePlayerHead("ui_weather_alert", -1, 0f, false, true);
					this.stormAlertBiomeStandingOn = this.biomeStandingOn;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WeatherStatusTick()
	{
		if (this.SpawnedTicks > 100)
		{
			bool flag = false;
			BiomeDefinition biomeStandingOn = this.biomeStandingOn;
			BiomeDefinition.WeatherGroup weatherGroup = ((biomeStandingOn != null) ? biomeStandingOn.currentWeatherGroup : null);
			if (weatherGroup != null && weatherGroup != this.weatherGroup)
			{
				string text = "{0} WeatherStatusTick {1} to {2}";
				object[] array = new object[3];
				array[0] = base.GetDebugName();
				int num = 1;
				BiomeDefinition.WeatherGroup weatherGroup2 = this.weatherGroup;
				array[num] = ((weatherGroup2 != null) ? weatherGroup2.name : null);
				array[2] = weatherGroup.name;
				Log.Out(text, array);
				if (this.weatherBuff != null)
				{
					this.Buffs.RemoveBuff(this.weatherBuff, -1, true);
				}
				this.weatherGroup = weatherGroup;
				this.weatherBuff = weatherGroup.buffName;
				flag = true;
			}
			bool flag2 = this.shelterPercent > 0f;
			if (this.isIndoorsCurrent != flag2)
			{
				this.isIndoorsCurrent = flag2;
				flag = true;
			}
			if (flag)
			{
				this.WeatherBuffUpdate();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WeatherBuffUpdate()
	{
		Log.Out("{0} WeatherBuffUpdate {1}, indoors {2}", new object[]
		{
			base.GetDebugName(),
			this.weatherBuff,
			this.isIndoorsCurrent
		});
		if (this.weatherBuff != null)
		{
			if (this.isIndoorsCurrent)
			{
				this.Buffs.RemoveBuff(this.weatherBuff, -1, true);
				return;
			}
			this.Buffs.AddBuff(this.weatherBuff, -1, true, false, -1f);
		}
	}

	public void OnWeatherGodModeChanged(bool _oldValue, bool _newValue)
	{
		if (!_newValue)
		{
			this.WeatherBuffUpdate();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void onNewBiomeEntered(BiomeDefinition _biome)
	{
		if (_biome != null)
		{
			EnvironmentAudioManager.Instance.EnterBiome(_biome);
			this.MinEventContext.Biome = _biome;
			this.FireEvent(MinEventTypes.onSelfEnteredBiome, true);
			QuestEventManager.Current.BiomeEntered(_biome);
			this.lastLootStage = -1;
		}
		if (this.biomeStandingOn != null && this.biomeStandingOn.Buff != null)
		{
			Log.Out("{0} onNewBiomeEntered -{1}", new object[]
			{
				base.GetDebugName(),
				this.biomeStandingOn.Buff
			});
			this.Buffs.RemoveBuff(this.biomeStandingOn.Buff, -1, true);
		}
		if (_biome != null && EntityStats.NewWeatherSurvivalEnabled && _biome.Buff != null)
		{
			Log.Out("{0} onNewBiomeEntered +{1}", new object[]
			{
				base.GetDebugName(),
				_biome.Buff
			});
			this.Buffs.AddBuff(_biome.Buff, -1, true, false, -1f);
		}
		this.biomeStandingOn = _biome;
	}

	public void ResetBiomeWeatherOnDeath()
	{
		this.onNewBiomeEntered(null);
		this.isIndoorsCurrent = true;
		this.WeatherBuffUpdate();
		this.weatherBuff = null;
		this.weatherGroup = null;
	}

	public override float GetWetnessRate()
	{
		float num = Mathf.Pow(this.inWaterPercent, 2f);
		if (this.shelterAbovePercent == 0f)
		{
			float num2 = WeatherManager.Instance.GetCurrentWetPercent(this) * 0.05f;
			num = Utils.FastMax(num, num2);
		}
		return num;
	}

	public SpawnPosition GetSpawnPoint()
	{
		if (this.SpawnPoints == null || this.SpawnPoints.Count == 0)
		{
			return SpawnPosition.Undef;
		}
		return new SpawnPosition(this.SpawnPoints[0].ToVector3() + new Vector3(0.5f, 0f, 0.5f), 0f);
	}

	public override void AddUIHarvestingItem(ItemStack itemStack, bool _bAddOnlyIfNotExisting = false)
	{
		this.playerUI.xui.CollectedItemList.AddItemStack(itemStack, _bAddOnlyIfNotExisting);
	}

	public override Vector3 GetDropPosition()
	{
		Vector3 vector = base.GetDropPosition();
		Vector3 vector2 = vector - this.getHeadPosition();
		RaycastHit raycastHit;
		if (Physics.Raycast(new Ray(this.getHeadPosition() - Origin.position, vector2), ref raycastHit, vector2.magnitude, 1073807360))
		{
			vector = raycastHit.point - vector2.normalized * 0.5f + Origin.position;
		}
		return vector;
	}

	public bool CheckSpawnPointStillThere()
	{
		SpawnPosition spawnPoint = this.GetSpawnPoint();
		return spawnPoint.IsUndef() || this.world.GetChunkFromWorldPos(spawnPoint.ToBlockPos()) == null || this.world.GetBlock(spawnPoint.ToBlockPos()).Block is BlockSleepingBag;
	}

	public void RemoveSpawnPoints(bool showTooltip = true)
	{
		this.SpawnPoints.Clear();
		if (showTooltip)
		{
			GameManager.ShowTooltip(this, Localization.Get("ttBedrollGone", false, null), false, false, 0f);
		}
		this.selectedSpawnPointKey = -1L;
	}

	public void EmptyBackpackAndToolbelt()
	{
		ItemStack[] slots = this.bag.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			slots[i] = ItemStack.Empty;
		}
		this.bag.SetSlots(slots);
		for (int j = 0; j < this.inventory.GetItemCount(); j++)
		{
			if (j != this.inventory.DUMMY_SLOT_IDX)
			{
				this.inventory.SetItem(j, ItemStack.Empty);
			}
		}
	}

	public void EmptyBackpack()
	{
		ItemStack[] slots = this.bag.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			slots[i] = ItemStack.Empty;
		}
		this.bag.SetSlots(slots);
	}

	public void EmptyToolbelt(int start, int end)
	{
		for (int i = start; i < end; i++)
		{
			if (i != this.inventory.DUMMY_SLOT_IDX)
			{
				this.inventory.SetItem(i, ItemStack.Empty);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleMapObjects(bool usePersistantBackpackPositions)
	{
		if (this.persistentPlayerData != null)
		{
			List<Vector3i> landProtectionBlocks = this.persistentPlayerData.GetLandProtectionBlocks();
			for (int i = 0; i < landProtectionBlocks.Count; i++)
			{
				NavObject navObject = NavObjectManager.Instance.RegisterNavObject("land_claim", landProtectionBlocks[i].ToVector3(), "", false, -1, null);
				if (navObject != null)
				{
					navObject.OwnerEntity = this;
				}
			}
			this.persistentPlayerData.ShowBedrollOnMap();
			this.SetDroppedBackpackPositions(usePersistantBackpackPositions ? this.persistentPlayerData.GetDroppedBackpackPositions() : this.droppedBackpackPositions);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AnalyticsSendDeath(DamageResponse _dmResponse)
	{
		DamageSource source = _dmResponse.Source;
		string text;
		if (this.isFallDeath)
		{
			text = "fall";
		}
		else if (source.BuffClass != null)
		{
			text = source.BuffClass.Name;
		}
		else if (source.ItemClass != null)
		{
			text = source.ItemClass.Name;
		}
		else
		{
			text = source.damageType.ToStringCached<EnumDamageTypes>();
		}
		if (this.entityThatKilledMe)
		{
			text += "_";
			if (this.entityThatKilledMe == this)
			{
				text += "self";
			}
			else if (this.entityThatKilledMe is EntityPlayer)
			{
				text += "player";
			}
			else
			{
				text += this.entityThatKilledMe.EntityName;
			}
		}
		GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.PlayerDeathCauses, text, 1, true, GameSparksCollector.GSDataCollection.SessionUpdates);
	}

	public EntityPlayerLocal.AutoMove EnableAutoMove(bool _enable)
	{
		if (!_enable)
		{
			this.autoMove = null;
		}
		else
		{
			this.autoMove = new EntityPlayerLocal.AutoMove(this);
		}
		return this.autoMove;
	}

	public void TryCancelChargedAction()
	{
		ItemAction itemAction = this.inventory.holdingItem.Actions[0];
		if (itemAction is ItemActionCatapult || itemAction is ItemActionThrowAway)
		{
			itemAction.CancelAction(this.inventory.holdingItemData.actionData[0]);
			this.inventory.holdingItemData.actionData[0].HasExecuted = false;
		}
	}

	public bool TryAddRecoveryPosition(Vector3i _position)
	{
		if (this.recoveryPositions.Contains(_position))
		{
			return false;
		}
		if (!GameManager.Instance.World.CanPlayersSpawnAtPos(_position, false) || GameManager.Instance.World.GetPOIAtPosition(_position.ToVector3(), null, null) != null)
		{
			return false;
		}
		if (this.recoveryPositions.Count == 0)
		{
			this.recoveryPositions.Add(_position);
			return true;
		}
		if ((this.recoveryPositions[this.recoveryPositions.Count - 1] - _position).ToVector3().sqrMagnitude < 10000f)
		{
			return false;
		}
		if (this.recoveryPositions.Count >= 5)
		{
			this.recoveryPositions.RemoveAt(0);
		}
		this.recoveryPositions.Add(_position);
		return true;
	}

	public void GiveExp(CraftCompleteData data)
	{
		int num = (int)this.Buffs.GetCustomVar("_craftCount_" + data.RecipeName);
		int recipeUsedCount = (int)data.RecipeUsedCount;
		this.Buffs.SetCustomVar("_craftCount_" + data.RecipeName, (float)(num + recipeUsedCount), true, CVarOperation.set);
		this.Progression.AddLevelExp(data.CraftExpGain / (num + recipeUsedCount), "_xpFromCrafting", Progression.XPTypes.Crafting, true, true, -1, null);
		this.totalItemsCrafted += (uint)recipeUsedCount;
		QuestEventManager.Current.CraftedItem(data.CraftedItemStack);
		XUiC_RecipeStack.HandleCraftXPGained();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGUI()
	{
		this.renderManager.OnGUI();
	}

	public override void PlayStepSound(float _volume)
	{
		if (!this.bFirstPersonView)
		{
			base.PlayStepSound(_volume);
		}
	}

	public void HolsterWeapon(bool holster)
	{
		EntityPlayerLocal.HolsterState holsterState = (holster ? EntityPlayerLocal.HolsterState.Holstered : EntityPlayerLocal.HolsterState.Unholstered);
		if (this.desiredHolsterState != holsterState)
		{
			this.desiredHolsterState = holsterState;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void refreshHolsterState()
	{
		this.desiredHolsterState = EntityPlayerLocal.HolsterState.Undefined;
	}

	public eTPCameraCheckResult CharacterCameraAngleValid()
	{
		if (this.bFirstPersonView || this.vp_FPCamera.Locked3rdPerson)
		{
			return eTPCameraCheckResult.Pass;
		}
		if (this.LineOfSightObstructed)
		{
			return eTPCameraCheckResult.LineOfSightCheckFailed;
		}
		Vector3 vector;
		vector..ctor(this.cameraTransform.forward.x, 0f, this.cameraTransform.forward.z);
		if (Vector3.Angle(base.transform.forward, vector) > 20f)
		{
			return eTPCameraCheckResult.AngleCheckFailed;
		}
		return eTPCameraCheckResult.Pass;
	}

	public void UpdateRespawn()
	{
		if (!this.RespawnTeleportTimeoutLogged && Time.realtimeSinceStartup - this.RespawnTeleportDuration > 5f)
		{
			this.LogFellThroughWorldDebugInfo();
			BacktraceUtils.SendErrorReport("fellthroughworld", "Player fell through world", null);
			this.RespawnTeleportTimeoutLogged = true;
		}
	}

	public void LogFellThroughWorldDebugInfo()
	{
		Vector3i vector3i = World.worldToBlockPos(this.position);
		Chunk chunk = (Chunk)this.world.GetChunkFromWorldPos(vector3i.x, vector3i.y, vector3i.z);
		if (chunk == null)
		{
			Log.Error(string.Format("[FELLTHROUGHWORLD] Teleport respawn failed. Local player blockPos: {0}, Chunk is null.", vector3i));
		}
		else
		{
			long key = chunk.Key;
			Log.Out(string.Format("[FELLTHROUGHWORLD] Teleport respawn failed. Local player blockPos: {0}, Chunk Key: {1}, Chunk Pos X:{2}, Z:{3}", new object[]
			{
				vector3i,
				(chunk != null) ? chunk.Key : "null",
				WorldChunkCache.extractX(chunk.Key) << 4,
				WorldChunkCache.extractZ(chunk.Key) << 4
			}));
			Log.Out(string.Format("[FELLTHROUGHWORLD] Time since chunk {0} selected for data generation: {1}", key, ChunkManager.SecondsSinceChunkSelectedForGeneration(chunk.Key)));
			ValueTuple<int, double> valueTuple = ChunkCluster.SecondsSinceChunkStartedRegeneration(chunk.Key);
			if (valueTuple.Item1 == -1)
			{
				Log.Out(string.Format("[FELLTHROUGHWORLD] Chunk {0} has never started mesh regeneration.", key));
			}
			else
			{
				Log.Out(string.Format("[FELLTHROUGHWORLD] Time since a layer on chunk {0} started mesh regeneration: Layer {1}, {2}", key, valueTuple.Item1, valueTuple.Item2));
			}
			valueTuple = ChunkCluster.SecondsSinceChunkEndedRegeneration(chunk.Key);
			if (valueTuple.Item1 == -1)
			{
				Log.Out(string.Format("[FELLTHROUGHWORLD] Chunk {0} has never finished mesh regeneration.", key));
			}
			else
			{
				Log.Out(string.Format("[FELLTHROUGHWORLD] Time since a layer on chunk {0} finished mesh regeneration: Layer {1}, {2}", key, valueTuple.Item1, valueTuple.Item2));
			}
		}
		ChunkCluster chunkCache = this.world.ChunkCache;
		if (chunk != null)
		{
			chunk.LogChunkState();
		}
		ChunkProviderGenerateWorld chunkProviderGenerateWorld = chunkCache.ChunkProvider as ChunkProviderGenerateWorld;
		if (chunkProviderGenerateWorld != null)
		{
			chunkProviderGenerateWorld.LogCurrentChunkGeneration();
		}
		World world = this.world;
		if (world != null)
		{
			ChunkManager chunkManager = world.m_ChunkManager;
			if (chunkManager != null)
			{
				chunkManager.LogCurrentGenerationState();
			}
		}
		ChunkCluster.LogCurrentChunkRegenerationState(chunk.Key);
		this.moveController.LogCurrentRespawnState();
	}

	public void ClearMovementInputs()
	{
		this.movementInput.moveForward = 0f;
		this.movementInput.moveStrafe = 0f;
		this.movementInput.rotation = Vector3.zero;
		this.vp_FPController.Player.InputMoveVector.Set(Vector2.zero);
		this.vp_FPController.Player.InputSmoothLook.Set(Vector2.zero);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cHolsterLayerIndex = 1;

	public UnityEvent<bool> MountEvent = new UnityEvent<bool>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float prevStaminaValue;

	public float weaponCrossHairAlpha = 0.8f;

	public const float cCameraTPVBaseDistance = 0.94f;

	public const float cCameraTPVOffsetMin = -0.2f;

	public const float cCameraTPVOffsetMax = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform cameraContainerTransform;

	public Transform cameraTransform;

	public Camera playerCamera;

	public Camera finalCamera;

	public bool IsUnderwaterCamera;

	public GameRenderManager renderManager;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_FPController m_vp_FPController;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_FPCamera m_vp_FPCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_FPWeapon m_vp_FPWeapon;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_PlayerEventHandler m_vp_FPPlayerEventHandler;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool m_checked_vp_FPController;

	public WorldRayHitInfo HitInfo = new WorldRayHitInfo();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float SPRINT_GRACE_PERIOD = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timeInSprintGrace;

	public float RespawnTeleportDuration;

	public const float RespawnTeleportTimeout = 5f;

	public bool RespawnTeleportTimeoutLogged;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float VIBRATION_LOW = 0.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float VIBRATION_MEDIUM = 0.35f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float VIBRATION_HIGH = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float VIBRATION_DURATION = 0.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float oldHealth;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float vibrationTimeout = float.MaxValue;

	public static int LostItemOnDeathMin = 0;

	public static int LostItemOnDeathMax = 0;

	public static EntityPlayerLocal.DegradeOnDeathTypes DegradeOnDeathType = EntityPlayerLocal.DegradeOnDeathTypes.None;

	public static float DegradeAmountOnDeath = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityPlayerLocal.HolsterState desiredHolsterState = EntityPlayerLocal.HolsterState.Undefined;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityPlayerLocal.HolsterState lastHolsterState = EntityPlayerLocal.HolsterState.Unholstered;

	public bool flipCameraSide;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const float cMaxLineOfSightCheckRange = 1f;

	public bool LineOfSightObstructed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 lineOfSightObstructionPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Ray lineOfSightRay;

	public static bool StormWarning = true;

	public PersistentPlayerData persistentPlayerData;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float lastTimeJetpackDecreased;

	public bool bFirstPersonView = true;

	public bool bPreferFirstPerson = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float crossHairOpenArea;

	public MovementInput movementInput = new MovementInput();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool inputWasJump;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool inputWasDown;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool jumpTrigger;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bSwitchCameraBackAfterRespawn;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bSwitchTo3rdPersonAfterAiming;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 selfCameraPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 selfCameraSeekPos;

	public bool bExhausted;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isExhaustedSoundAllowed = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ItemStack dragAndDropItem;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isReloadCancelling;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isLadderAttached;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool canLadderAirAttach = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool wasJumping;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool wasJumpTrigger;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool wasLadderAttachedJump;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int swimMode = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int swimExhaustedTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool swimClimbing;

	public bool bLerpCameraFlag;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lerpCameraLerpValue;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lerpCameraStartFOV;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lerpCameraEndFOV;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lerpCameraFastFOV;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastOverrideFOV = -1f;

	public float OverrideFOV = -1f;

	public Vector3 OverrideLookAt = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float biomeVolume;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AudioSource audioSourceBiomeActive;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AudioSource audioSourceBiomeFadeOut;

	public BlendCycleTimer sneakDamageBlendTimer = new BlendCycleTimer(0.5f, 2f, 0.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string sneakDamageText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string dropInventoryBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int runTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int sprintLoopSoundPlayId = -1;

	public DynamicMusicManager DynamicMusicManager;

	public IThreatLevel ThreatLevel;

	public float LastTargetEventTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material overlayMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public byte[] overlayAlternating = new byte[4];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float[] overlayDirectionTime = new float[8];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2[] overlayBloodDropsPositions = new Vector2[24];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform uwEffectRefract;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform uwEffectDebris;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform uwEffectDroplets;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform uwEffectWaterFade;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform uwEffectHaze;

	public bool bIntroAnimActive;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LocalPlayerUI playerUI;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GUIWindowManager windowManager;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public NGUIWindowManager nguiWindowManager;

	public string AreaMessage;

	public float AreaMessageAlpha;

	public ScreenEffects ScreenEffectManager;

	public float GodModeSpeedModifier = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public PlayerMoveController moveController;

	public bool isStunned;

	public bool isDeafened;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int oldLayer = -1;

	public Rect ZombieCompassBounds;

	public float DropTimeDelay;

	public float InteractTimeDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float recoveryPointTimer;

	public List<Vector3i> recoveryPositions = new List<Vector3i>();

	public bool DebugDismembermentChance;

	public bool BloodMoonParticipation;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool inAir;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool _cameraRelativeMovement;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cAimingCameraResetTime = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool tpCameraLockTimerActive;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float tpCameraLockStartTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cMinOvereadObstructionTime = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float camOverheadDistance = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float overheadObstructionTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cMinCameraDistanceMultiplier = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cMaxCameraDistanceMultiplier = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float cameraDistanceMulti = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CharacterMatrixOverride characterMatrixOverride;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_MoveVector;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 m_SmoothLook;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityPlayerLocal.MoveState moveState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool moveStateAiming;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool moveStateHoldBow;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cSwimDragBase = 0.01f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cSwimDragScale = 5.4f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cSwimDragPow = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cSwimAccelExhausted = 0.00025f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cSwimAccel = 0.00032f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cSwimAccelRun = 0.0024f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cSwimAccelUp = 0.00052f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cSwimAccelUpGrounded = 0.05f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cSwimAccelDown = -0.00038f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float achievementDistanceAccu;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastWaypointUpdateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cWaypointUpdateTime = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float updateBedrollPositionChecks;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float updateRadiationChecks;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int blockRadiusEffectsIndex;

	public bool LootAtMax;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int lastLootStage = -1;

	public eTPCameraCheckResult TPCameraCheckResult;

	public float InWorldPercent;

	public float InWorldLookPercent;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastTimeUnderwater;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool wasSpawned;

	public float spawnEffectPow = 4.19f;

	public static float spawnInEffectSpeed = 3f;

	public bool bPlayingSpawnIn;

	public float spawnInTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float spawnInIntensity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float deathTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Predicate<ItemStack> dropStackCondition = (ItemStack s) => s != null && !s.IsEmpty() && !s.itemValue.ItemClassOrMissing.KeepOnDeath();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Predicate<ItemValue> dropValueCondition = (ItemValue v) => v != null && !v.IsEmpty() && !v.ItemClassOrMissing.KeepOnDeath();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cDyingEffectSpeed = 600f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cDyingEffectStartHealth = 70f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cDyingEffectHealthThreshold = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float dyingEffectHitTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float dyingEffectCur;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float dyingEffectLast;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float dyingEffectHealthLast;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int fallHealth;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isFallDeath;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<NetPackageSharedQuest.SharedQuestData> sharedQuestsToProcess = new List<NetPackageSharedQuest.SharedQuestData>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<Collider> setLayerRecursivelyList = new List<Collider>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool cancellingInventoryActions;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastCameraDistanceAmount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public NavObject backpackNavObject;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<NavObject> backpackNavObjects;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Vector3i> backpackPositionsFromThread = new List<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<EntityPlayerLocal.LocationSlotInfo> DestroyLocationSlotList;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject[] screenBloodEffect;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material[] screenBloodMtrl;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool healthLostThisRound;

	public static List<Color> crosshairColors = new List<Color>
	{
		new Color(1f, 1f, 1f, 1f),
		new Color(1f, 0.1f, 0f, 1f),
		new Color(1f, 0.4f, 0f, 1f),
		new Color(1f, 0.8f, 0f, 1f),
		new Color(0f, 1f, 0.3f, 1f),
		new Color(0f, 1f, 1f, 1f),
		new Color(0f, 0.5f, 1f, 1f),
		new Color(1f, 0f, 0.9f, 1f),
		new Color(1f, 0.6f, 0.9f, 1f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cCrosshairScreenHeightFactor = 0.059f;

	public float shelterPercent;

	public float shelterAbovePercent;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool shelterIsUpdating;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int shelterSideCount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 shelterStartPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 shelterPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 shelterPosOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float shelterDir;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float shelterRadius;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cShelterSideStep = 0.13f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cShelterUpDistance = 253f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cShelterAngles = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cShelterAngleStep = 45f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cShelterRadiusStep = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cShelterRadiusMax = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cShelterMask = 1073807360;

	public bool isPlayerInStorm;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BiomeDefinition stormAlertBiomeStandingOn;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float stormRemainingSeconds;

	public bool isIndoorsCurrent = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BiomeDefinition.WeatherGroup weatherGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string weatherBuff;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityPlayerLocal.AutoMove autoMove;

	public enum DegradeOnDeathTypes
	{
		None,
		Durability,
		MaxDurability,
		Both
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum HolsterState
	{
		Holstered,
		Unholstered,
		Undefined
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum MoveState
	{
		None,
		Off,
		Attached,
		Idle,
		Walk,
		Run,
		Swim,
		Crouch,
		CrouchWalk,
		CrouchRun,
		Jump
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct LocationSlotInfo
	{
		public LocationSlotInfo(EntityPlayerLocal.LocationSlotInfo.LocationTypes location, int slot)
		{
			this.Location = location;
			this.Slot = slot;
		}

		public EntityPlayerLocal.LocationSlotInfo.LocationTypes Location;

		public int Slot;

		public enum LocationTypes
		{
			Backpack,
			Toolbelt,
			Equipment
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum DropOption
	{
		None,
		All,
		Toolbelt,
		Backpack,
		Equipment,
		ToolbeltBackpack,
		DeleteAll
	}

	public class AutoMove
	{
		public AutoMove(Entity _entity)
		{
			this.entity = _entity;
		}

		public void StartLine(float _duration, int _loopCount, Vector3 _endPos, Action _onMoveComplete = null)
		{
			this.mode = EntityPlayerLocal.AutoMove.Mode.Line;
			this.endTime = _duration;
			this.loopCount = _loopCount;
			this.startPos = this.entity.position;
			this.targetPos = _endPos;
			this.isPingPong = false;
			if (this.loopCount < 0)
			{
				this.loopCount *= -2;
				this.isPingPong = true;
			}
			this.onMoveComplete = _onMoveComplete;
		}

		public void StartOrbit(float _duration, int _loopCount, Vector3 _orbitPos, bool _lookForward = false, bool _isFlipped = false, Action _onMoveComplete = null)
		{
			this.mode = EntityPlayerLocal.AutoMove.Mode.Orbit;
			this.endTime = _duration;
			this.loopCount = _loopCount;
			this.startPos = this.entity.position;
			this.targetPos = _orbitPos;
			this.orbitLookForward = _lookForward;
			this.isFlipped = _isFlipped;
			this.isPingPong = false;
			if (this.loopCount < 0)
			{
				this.loopCount *= -2;
				this.isPingPong = true;
			}
			this.onMoveComplete = _onMoveComplete;
		}

		public void StartRelative(float velX, float velZ, float rotVel)
		{
			this.mode = EntityPlayerLocal.AutoMove.Mode.Relative;
			this.vel.x = velX;
			this.vel.z = velZ;
			this.rotY = this.entity.rotation.y;
			this.rotYVel = rotVel;
		}

		public void SetLookAt(Vector3 _pos)
		{
			this.lookAtPos = _pos;
		}

		public void Update()
		{
			if (this.mode == EntityPlayerLocal.AutoMove.Mode.Off)
			{
				return;
			}
			if (this.mode == EntityPlayerLocal.AutoMove.Mode.Line)
			{
				this.curTime += Time.deltaTime;
				float num = this.curTime / this.endTime;
				if (num > 1f)
				{
					this.curTime = 0f;
					num = 0f;
					if (this.isPingPong)
					{
						Vector3 vector = this.startPos;
						Vector3 vector2 = this.targetPos;
						this.targetPos = vector;
						this.startPos = vector2;
					}
					int num2 = this.loopCount - 1;
					this.loopCount = num2;
					if (num2 <= 0)
					{
						num = (float)(this.isPingPong ? 0 : 1);
						Action action = this.onMoveComplete;
						if (action != null)
						{
							action();
						}
						this.mode = EntityPlayerLocal.AutoMove.Mode.Off;
					}
				}
				Vector3 vector3 = Vector3.Lerp(this.startPos, this.targetPos, num);
				this.entity.SetPosition(vector3, true);
			}
			if (this.mode == EntityPlayerLocal.AutoMove.Mode.Orbit)
			{
				this.curTime += Time.deltaTime;
				float num3 = this.curTime / this.endTime;
				if (num3 > 1f)
				{
					this.curTime -= this.endTime;
					num3 = this.curTime / this.endTime;
					if (this.isPingPong)
					{
						this.isFlipped = !this.isFlipped;
					}
					int num2 = this.loopCount - 1;
					this.loopCount = num2;
					if (num2 <= 0)
					{
						num3 = 1f;
						Action action2 = this.onMoveComplete;
						if (action2 != null)
						{
							action2();
						}
						this.mode = EntityPlayerLocal.AutoMove.Mode.Off;
					}
				}
				Vector3 vector4 = this.startPos - this.targetPos;
				float num4 = 360f * num3;
				num4 *= (float)(this.isFlipped ? (-1) : 1);
				this.entity.SetPosition(this.targetPos + Quaternion.Euler(0f, num4, 0f) * vector4, true);
				Quaternion quaternion;
				if (this.orbitLookForward)
				{
					quaternion = Quaternion.Euler(0f, num4 - 90f, 0f);
				}
				else
				{
					quaternion = Quaternion.LookRotation(this.targetPos - this.entity.position, Vector3.up);
				}
				Vector3 eulerAngles = quaternion.eulerAngles;
				eulerAngles.x *= -1f;
				this.entity.SetRotation(eulerAngles);
			}
			if (this.mode == EntityPlayerLocal.AutoMove.Mode.Relative)
			{
				this.rotY += this.rotYVel * Time.deltaTime;
				this.entity.SetRotation(new Vector3(0f, this.rotY, 0f));
				this.entity.SetPosition(this.entity.position + Quaternion.Euler(0f, this.rotY, 0f) * this.vel * Time.deltaTime, true);
			}
			if (this.lookAtPos.sqrMagnitude > 0f)
			{
				Vector3 eulerAngles2 = Quaternion.LookRotation(this.lookAtPos - this.entity.position, Vector3.up).eulerAngles;
				eulerAngles2.x *= -1f;
				this.entity.SetRotation(eulerAngles2);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Entity entity;

		[PublicizedFrom(EAccessModifier.Private)]
		public EntityPlayerLocal.AutoMove.Mode mode;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 startPos;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 targetPos;

		[PublicizedFrom(EAccessModifier.Private)]
		public float curTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public float endTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public int loopCount;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isPingPong;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isFlipped;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool orbitLookForward;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 vel;

		[PublicizedFrom(EAccessModifier.Private)]
		public float rotY;

		[PublicizedFrom(EAccessModifier.Private)]
		public float rotYVel;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 lookAtPos;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action onMoveComplete;

		[PublicizedFrom(EAccessModifier.Private)]
		public enum Mode
		{
			Off,
			Line,
			Orbit,
			Relative
		}
	}
}
