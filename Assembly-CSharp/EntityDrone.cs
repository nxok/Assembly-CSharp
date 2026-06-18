using System;
using System.Collections.Generic;
using System.IO;
using Audio;
using GamePath;
using Platform;
using RaycastPathing;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityDrone : EntityNPC, ILockable
{
	public override bool IsValidAimAssistSlowdownTarget
	{
		get
		{
			return false;
		}
	}

	public override bool IsValidAimAssistSnapTarget
	{
		get
		{
			return false;
		}
	}

	public DroneLightManager lightManager
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!this._lm)
			{
				this._lm = base.transform.GetComponentInChildren<DroneLightManager>();
			}
			return this._lm;
		}
	}

	public float TimeSinceCreation
	{
		get
		{
			if (this.creationTime == 0f)
			{
				return 0f;
			}
			return Time.time - this.creationTime;
		}
	}

	public static bool IsValidForLocalPlayer()
	{
		PersistentPlayerData playerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(PlatformManager.InternalLocalUserIdentifier);
		return playerData != null && EntityDrone.IsValidForPlayer(GameManager.Instance.World.GetEntity(playerData.EntityId) as EntityPlayerLocal);
	}

	public static bool IsValidForPlayer(EntityPlayerLocal localPlayer)
	{
		foreach (OwnedEntityData ownedEntityData in localPlayer.GetOwnedEntities())
		{
			if (ownedEntityData.ClassId != -1 && EntityClass.list[ownedEntityData.ClassId].entityClassName == "entityJunkDrone")
			{
				GameManager.ShowTooltip(localPlayer, Localization.Get("xuiMaxDeployedDronesReached", false, null), string.Empty, "ui_denied", null, false, false, 0f);
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetItemClassId()
	{
		for (int i = 1; i < ItemClass.list.Length - 1; i++)
		{
			ItemClass itemClass = ItemClass.list[i];
			if (itemClass != null && itemClass.Name == "gunBotT3JunkDrone")
			{
				return itemClass.Id;
			}
		}
		return -1;
	}

	public void PrepareToSpawn()
	{
		this.PlayWakeupAnim = true;
	}

	public void OnApplyToEntity(int orderState)
	{
		if (orderState < 0)
		{
			return;
		}
		if (orderState != 0)
		{
			if (orderState != 1)
			{
				return;
			}
			this.SentryMode();
		}
		else
		{
			this.setOrders(EntityDrone.Orders.Follow);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.SendSyncData(16384);
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DebugDroneLog(EntityDrone.LoggingTypes logType, string format, params object[] args)
	{
		int num = 0;
		switch (logType)
		{
		case EntityDrone.LoggingTypes.Init:
			num = (int)EntityDrone.debugDroneInitLogPriority;
			break;
		case EntityDrone.LoggingTypes.Animation:
			num = (int)EntityDrone.debugDroneAnimationLogPriority;
			break;
		case EntityDrone.LoggingTypes.Shutdown:
			num = (int)EntityDrone.debugDroneShutdownLogPriority;
			break;
		}
		if (num == 1)
		{
			Log.Warning(format, args);
			return;
		}
		if (num != 2)
		{
			Log.Out(format, args);
			return;
		}
		Log.Error(format, args);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DebugDroneLog(string format, params object[] args)
	{
		this.DebugDroneLog(EntityDrone.LoggingTypes.Any, format, args);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.steering = new EntityDrone.EntitySteering(this);
		this.isLocked = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new void LateUpdate()
	{
		if (DroneManager.Debug_LocalControl)
		{
			this.debugInputRotX += Input.GetAxis("Mouse X") * 30f * 0.05f;
			this.debugInputRotY += Input.GetAxis("Mouse Y") * 30f * 0.05f;
			this.debugInputRotY = Mathf.Clamp(this.debugInputRotY, -90f, 90f);
			this.reconCam.transform.localRotation = Quaternion.AngleAxis(this.debugInputRotX, Vector3.up);
			this.reconCam.transform.localRotation *= Quaternion.AngleAxis(this.debugInputRotY, Vector3.left);
			RaycastHit raycastHit;
			if (Input.GetMouseButtonDown(0) && RaycastPathUtils.IsPositionBlocked(this.reconCam.ScreenPointToRay(Input.mousePosition), out raycastHit, 65536, true, 100f))
			{
				RaycastPathUtils.DrawBounds(World.worldToBlockPos(raycastHit.point + Origin.position), Color.yellow, 1f, 1f);
				this.pathMan.CreatePath(this.Owner.position, raycastHit.point + Origin.position, this.currentSpeedFlying, false, this.FollowHoverHeight);
			}
		}
	}

	public override void Init(int _entityClass, EntityInstanceAssets _assets, EModelInstanceAssets _eModelAssets)
	{
		base.Init(_entityClass, _assets, _eModelAssets);
	}

	public override void InitInventory()
	{
		this.inventory = new EntityDrone.DroneInventory(GameManager.Instance, this);
	}

	public override void PostInit()
	{
		float num = 1f / base.transform.localScale.x;
		this.interactionCollider = base.gameObject.GetComponent<BoxCollider>();
		if (this.interactionCollider)
		{
			this.interactionCollider.center = new Vector3(0f, 0.5f, 0.25f);
			this.interactionCollider.size = new Vector3(2.5f, 2f, 2f);
		}
		this.sensors = new EntityDrone.DroneSensors(this);
		this.sensors.Init();
		this.initWorldValues(this.orderState == EntityDrone.Orders.Follow);
		this.IsFlyMode.Value = true;
		this.bCanClimbLadders = true;
		this.bCanClimbVertical = true;
		this.prefabColor = this.GetPaintColor();
	}

	public override void OnAddedToWorld()
	{
		base.OnAddedToWorld();
		if (this.itemvalueToLoad != null)
		{
			this.OriginalItemValue = this.itemvalueToLoad;
		}
		if (this.OriginalItemValue == null)
		{
			this.isSystemSpawn = true;
			int itemClassId = this.GetItemClassId();
			if (itemClassId == -1)
			{
				Log.Warning("Failed to load junk drone from system spawn");
				return;
			}
			this.OriginalItemValue = new ItemValue(itemClassId, false);
		}
		this.isOwnerSyncPending = true;
		this.LoadMods();
		if (this.nativeCollider)
		{
			this.nativeCollider.enabled = true;
		}
		float value = EffectManager.GetValue(PassiveEffects.DegradationMax, this.OriginalItemValue, 0f, null, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		base.Stats.Health.BaseMax = value;
		base.Stats.Health.OriginalMax = value;
		this.Health = Mathf.RoundToInt(value * (1f - this.OriginalItemValue.UseTimes / value));
		this.animator = base.GetComponentInChildren<Animator>();
		this.pathMan = new FloodFillEntityPathGenerator(this.world, this);
		Origin.OriginChanged = (Action<Vector3>)Delegate.Combine(Origin.OriginChanged, new Action<Vector3>(this.OnOriginChanged));
		this.creationTime = Time.time;
	}

	public override void OnEntityUnload()
	{
		Origin.OriginChanged = (Action<Vector3>)Delegate.Remove(Origin.OriginChanged, new Action<Vector3>(this.OnOriginChanged));
		this.unRegsiterMovingLights();
		this.registeredPartyMembers = null;
		EntityPlayer entityPlayer = this.Owner as EntityPlayer;
		if (entityPlayer)
		{
			Party party = entityPlayer.Party;
			if (party != null)
			{
				party.PartyMemberAdded -= this.onPartyMemberAdded;
				party.PartyMemberRemoved -= this.onPartyMemberRemoved;
			}
			entityPlayer.PlayerTeleportedDelegates -= this.TeleportIfFollowing;
		}
		base.OnEntityUnload();
	}

	public override bool CanUpdateEntity()
	{
		return base.CanUpdateEntity();
	}

	public override bool CanNavigatePath()
	{
		return true;
	}

	public override void SetPosition(Vector3 _pos, bool _bUpdatePhysics = true)
	{
		base.SetPosition(_pos, _bUpdatePhysics);
	}

	public override float GetEyeHeight()
	{
		if (this.head == null)
		{
			this.head = base.transform.FindInChilds("Head", false);
		}
		return this.head.position.y - base.transform.position.y;
	}

	public override Ray GetLookRay()
	{
		return new Ray(this.position + new Vector3(0f, this.GetEyeHeight(), 0f), (base.GetAttackTarget() == null) ? this.GetLookVector() : (base.GetAttackTarget().getChestPosition() - this.position).normalized);
	}

	public override bool CanBePushed()
	{
		return true;
	}

	public override float GetWeight()
	{
		return base.GetWeight();
	}

	public override bool IsDead()
	{
		return false;
	}

	public override bool IsAttackValid()
	{
		return this.activeWeapon != null && this.activeWeapon.canFire();
	}

	public override int Health
	{
		get
		{
			return (int)base.Stats.Health.Value;
		}
		set
		{
			float num = (float)Mathf.Max(value, 1);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && num == 1f && this.state != EntityDrone.State.Shutdown)
			{
				this.isShutdownPending = true;
			}
			base.Stats.Health.Value = num;
		}
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale)
	{
		int num = Mathf.RoundToInt((float)_strength * this.armorDamageReduction);
		EntityAlive entityAlive = (EntityAlive)this.world.GetEntity(_damageSource.getEntityId());
		if (this.Owner && entityAlive && !this.debugFriendlyFire && entityAlive && this.isAlly(entityAlive))
		{
			num = 0;
		}
		return base.DamageEntity(_damageSource, num, _criticalHit, _impulseScale);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleNavObject()
	{
		NavObjectManager instance = NavObjectManager.Instance;
		EntityClass eClass = EntityClass.list[this.entityClass];
		if (eClass.NavObject != "")
		{
			NavObject navObject = instance.NavObjectList.Find(delegate(NavObject n)
			{
				NavObjectClass navObjectClass = n.NavObjectClass;
				return ((navObjectClass != null) ? navObjectClass.NavObjectClassName : null) == eClass.NavObject;
			});
			if (navObject != null)
			{
				instance.UnRegisterNavObject(navObject);
			}
			this.NavObject = instance.RegisterNavObject(eClass.NavObject, this, "", false);
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer != null)
		{
			primaryPlayer.Waypoints.UpdateEntityDroneWayPoint(this, this.OrderState == EntityDrone.Orders.Follow, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AddCharacterController()
	{
		base.AddCharacterController();
		if (this.PhysicsTransform == null)
		{
			return;
		}
		if (this.m_characterController == null)
		{
			return;
		}
		this.RootMotion = false;
		this.m_characterController.SetSize(Vector3.zero, this.physColHeight, this.physColHeight * 0.5f);
		this.setNoClip(true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float GetPushBoundsVertical()
	{
		return 1f;
	}

	public override void PlayStepSound(float _volume)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateStepSound(float _distX, float _distZ, float _rotYDelta)
	{
	}

	public override bool IsIgnoredByAI()
	{
		return true;
	}

	public override string ToString()
	{
		return string.Format("[type={0}, id={1}, belongsPlayerId={2}]", base.GetType(), this.entityId, this.belongsPlayerId);
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		base.Write(_bw, _bNetworkWrite);
		_bw.Write(1);
		this.OwnerID.ToStream(_bw, false);
		this.OriginalItemValue = this.GetUpdatedItemValue();
		this.OriginalItemValue.Write(_bw);
		ushort num = 49515;
		_bw.Write(num);
		this.WriteSyncData(_bw, num);
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		_br.ReadInt32();
		this.OwnerID = PlatformUserIdentifierAbs.FromStream(_br, false, false);
		this.OriginalItemValue = ItemValue.None;
		this.OriginalItemValue.Read(_br);
		ushort num = _br.ReadUInt16();
		this.ReadSyncData(_br, num, 0);
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		if (DroneManager.Debug_LocalControl)
		{
			return;
		}
		this.SyncOwnerData();
		this.updateTransitionState();
		this.updateAnimStates();
		if (this.isShutdownPending || (!this.Owner && this.state != EntityDrone.State.Shutdown))
		{
			this.performShutdown();
		}
		this.updateShutdownState();
		if (!this.isQuietMode && this.idleLoop == null && this.state == EntityDrone.State.Idle && !GameManager.IsDedicatedServer && GameManager.Instance.World.IsLocalPlayer(this.belongsPlayerId))
		{
			this.idleLoop = this.PlaySoundLoop("drone_idle_hover", 0.2f);
		}
		if ((this.state == EntityDrone.State.Idle || this.state == EntityDrone.State.Sentry || this.state == EntityDrone.State.Follow) && this.areaScanTimer > 0f)
		{
			this.areaScanTimer -= Time.deltaTime;
			if (this.areaScanTimer <= 0f)
			{
				this.isInConfinedSpace = this.pathMan.IsConfinedSpace(this.position, 3f, false);
				this.areaScanTimer = this.areaScanTime;
			}
		}
		this.updatePartyBuffs();
		this.updateDroneSystems();
		EntityPlayerLocal entityPlayerLocal = this.Owner as EntityPlayerLocal;
		if (entityPlayerLocal && this.state == EntityDrone.State.Idle && this.initSuppressVOTimer <= 0f)
		{
			if (this.focusBoxNode == null)
			{
				if (entityPlayerLocal.MoveController.FocusBoxPosition == World.worldToBlockPos(this.position))
				{
					RaycastNode raycastNode = RaycastPathWorldUtils.FindNodeType(RaycastPathWorldUtils.ScanVolume(this.world, this.position, false, false, false, 0f), cPathNodeType.Air);
					if (raycastNode != null)
					{
						this.focusBoxNode = raycastNode;
					}
				}
			}
			else
			{
				Vector3 vector = this.focusBoxNode.Center - this.position;
				RaycastPathUtils.DrawLine(this.position, this.focusBoxNode.Center, Color.yellow, 1f);
				if (this.isOutOfRange(this.focusBoxNode.Center, 0.25f))
				{
					this.move(vector.normalized, false);
				}
				else
				{
					this.focusBoxNode = null;
				}
			}
		}
		else if (this.state != EntityDrone.State.Idle && this.focusBoxNode != null)
		{
			this.focusBoxNode = null;
		}
		this.updateDroneServiceMenu();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTasks()
	{
		if (DroneManager.Debug_LocalControl)
		{
			float num = this.debugInputSpeed;
			if (InputUtils.ShiftKeyPressed)
			{
				num *= 10f;
			}
			this.debugInputFwd = this.reconCam.transform.forward;
			this.debugInputFwd.y = 0f;
			if (Input.GetKey(119))
			{
				this.move(this.debugInputFwd, num, false);
			}
			if (Input.GetKey(115))
			{
				this.move(-this.debugInputFwd, num, false);
			}
			this.debugInputRgt = this.reconCam.transform.right;
			this.debugInputRgt.y = 0f;
			if (Input.GetKey(97))
			{
				this.move(-this.debugInputRgt, num, false);
			}
			if (Input.GetKey(100))
			{
				this.move(this.debugInputRgt, num, false);
			}
			this.debugInputUp = this.reconCam.transform.up;
			this.debugInputUp.x = 0f;
			this.debugInputUp.z = 0f;
			if (Input.GetKey(32))
			{
				this.move(this.debugInputUp, num * 0.5f, false);
			}
			if (Input.GetKey(99))
			{
				this.move(-this.debugInputUp, num * 0.5f, false);
			}
			RaycastPathUtils.DrawBounds(this.Owner.GetBlockPosition().ToVector3CenterXZ() - new Vector3(0.5f, 0f, 0.5f), Color.cyan, 1f, 1f);
			return;
		}
		base.GetEntitySenses().ClearIfExpired();
		if (this.Owner != null)
		{
			this.updateState();
			this.debugUpdate();
		}
		if (PathFinderThread.Instance != null)
		{
			base.updateTasks();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitLocalActivationCommands(Action<EntityActivationCommand> _addCallback)
	{
		_addCallback(new EntityActivationCommand("talk", "talk", null, null));
		_addCallback(new EntityActivationCommand("service", "service", null, null));
		_addCallback(new EntityActivationCommand("repair", "wrench", null, null));
		_addCallback(new EntityActivationCommand("lock", "lock", null, null));
		_addCallback(new EntityActivationCommand("unlock", "unlock", null, null));
		_addCallback(new EntityActivationCommand("storage", "loot_sack", null, null));
		_addCallback(new EntityActivationCommand("keypad", "keypad", null, null));
		_addCallback(new EntityActivationCommand("take", "hand", null, null));
		_addCallback(new EntityActivationCommand("drone_command_stay", "run_and_gun", null, null));
		_addCallback(new EntityActivationCommand("drone_command_follow", "run", null, null));
		_addCallback(new EntityActivationCommand("drone_dont_heal_allies", "player", null, null));
		_addCallback(new EntityActivationCommand("drone_heal_allies", "allies", null, null));
		_addCallback(new EntityActivationCommand("drone_attack_mode_passive", "clock", null, null));
		_addCallback(new EntityActivationCommand("drone_attack_mode_aggressive", "fire", null, null));
		_addCallback(new EntityActivationCommand("drone_light_on", "lightbulb", null, null));
		_addCallback(new EntityActivationCommand("drone_light_off", "electric_switch", null, null));
		_addCallback(new EntityActivationCommand("drone_silent_on", "stealth", null, "drone_silent"));
		_addCallback(new EntityActivationCommand("drone_silent_off", "sight", null, "drone_silent"));
		_addCallback(new EntityActivationCommand("drone_command_heal", "cardio", null, null));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ReorderActivationCommands(List<EntityActivationCommand> _commands)
	{
		if (this.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			Entity.MoveActivationCommandAfter(_commands, "storage", "heal");
		}
	}

	public override bool AllowActivationCommand(ReadOnlySpan<char> _commandName, EntityPlayerLocal _playerFocusing)
	{
		if (this.IsDead())
		{
			return false;
		}
		if (this.belongsToPlayerId(_playerFocusing.entityId))
		{
			if (base.CommandIs(_commandName, "talk"))
			{
				return this.state != EntityDrone.State.Shutdown;
			}
			if (base.CommandIs(_commandName, "service"))
			{
				return true;
			}
			if (base.CommandIs(_commandName, "repair"))
			{
				return (float)this.Health < base.Stats.Health.Max;
			}
			if (base.CommandIs(_commandName, "lock"))
			{
				return !this.isLocked;
			}
			if (base.CommandIs(_commandName, "unlock"))
			{
				return this.isLocked;
			}
			if (base.CommandIs(_commandName, "keypad"))
			{
				return true;
			}
			if (base.CommandIs(_commandName, "take"))
			{
				return true;
			}
			if (base.CommandIs(_commandName, "drone_command_stay"))
			{
				return this.OrderState != EntityDrone.Orders.Stay && this.state != EntityDrone.State.Shutdown;
			}
			if (base.CommandIs(_commandName, "drone_command_follow"))
			{
				return this.OrderState != EntityDrone.Orders.Follow && this.state != EntityDrone.State.Shutdown;
			}
			if (base.CommandIs(_commandName, "drone_command_heal"))
			{
				return this.state != EntityDrone.State.Shutdown && this.TargetCanBeHealed(_playerFocusing);
			}
			if (base.CommandIs(_commandName, "storage"))
			{
				return this.bag != null;
			}
			if (base.CommandIs(_commandName, "drone_silent_on"))
			{
				return !this.isQuietMode;
			}
			if (base.CommandIs(_commandName, "drone_silent_off"))
			{
				return this.isQuietMode;
			}
			if (base.CommandIs(_commandName, "drone_light_on"))
			{
				return this.IsFlashlightAttached && !this.IsFlashlightOn;
			}
			if (base.CommandIs(_commandName, "drone_light_off"))
			{
				return this.IsFlashlightAttached && this.IsFlashlightOn;
			}
			if (base.CommandIs(_commandName, "drone_dont_heal_allies"))
			{
				return this.IsHealModAttached && this.allyHealMode == EntityDrone.AllyHealMode.HealAllies;
			}
			if (base.CommandIs(_commandName, "drone_heal_allies"))
			{
				return this.IsHealModAttached && this.allyHealMode == EntityDrone.AllyHealMode.DoNotHeal;
			}
			if (base.CommandIs(_commandName, "drone_attack_mode_passive"))
			{
				return this.IsWeaponAttached && this.attackMode == EntityDrone.AttackMode.Aggressive && GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled);
			}
			if (base.CommandIs(_commandName, "drone_attack_mode_aggressive"))
			{
				return this.IsWeaponAttached && this.attackMode == EntityDrone.AttackMode.Passive && GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled);
			}
			return base.AllowActivationCommand(_commandName, _playerFocusing);
		}
		else
		{
			bool flag = this.isLocked && !this.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier) && this.HasPassword();
			bool flag2 = (float)this.Health < base.Stats.Health.Max;
			if (base.CommandIs(_commandName, "storage"))
			{
				return this.bag != null;
			}
			if (base.CommandIs(_commandName, "keypad"))
			{
				return flag;
			}
			return base.CommandIs(_commandName, "repair") && flag2;
		}
	}

	public override string GetActivationText()
	{
		GameManager instance = GameManager.Instance;
		EntityPlayerLocal entityPlayerLocal;
		if (instance == null)
		{
			entityPlayerLocal = null;
		}
		else
		{
			World world = instance.World;
			entityPlayerLocal = ((world != null) ? world.GetPrimaryPlayer() : null);
		}
		EntityPlayerLocal entityPlayerLocal2 = entityPlayerLocal;
		if (entityPlayerLocal2 == null)
		{
			return string.Empty;
		}
		string text = entityPlayerLocal2.playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + entityPlayerLocal2.playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		string text2 = string.Format(Localization.Get("npcTooltipTalk", false, null), text, this.LocalizedEntityName);
		if (this.IsLocked() && !this.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			text2 = Localization.Get("ttLocked", false, null) + "\n" + text2;
		}
		return text2;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEntityActivated(EntityActivationCommand _command, EntityPlayerLocal _playerFocusing)
	{
		if (base.CommandIs(_command.commandId, "storage") && this.isLocked && !this.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			Manager.Play(this, "locked", 1f, false);
			return;
		}
		Entity.EntityLockContext entityLockContext = new Entity.EntityLockContext(_command.commandId.ToString(), this.bag);
		LockManager.Instance.LockRequestLocal(this, entityLockContext, 0);
	}

	public void StopUIInteraction()
	{
		this.stopInteraction(234);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startInteraction(ReadOnlySpan<char> _commandName)
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		LocalPlayerUI uiforPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (base.CommandIs(_commandName, "talk"))
		{
			this.startDialog(primaryPlayer);
			return;
		}
		if (base.CommandIs(_commandName, "service"))
		{
			((XUiC_DroneWindowGroup)((XUiWindowGroup)uiforPrimaryPlayer.windowManager.GetWindow(XUiC_DroneWindowGroup.ID)).Controller).CurrentVehicleEntity = this;
			uiforPrimaryPlayer.windowManager.Open(XUiC_DroneWindowGroup.ID, true);
			Manager.BroadcastPlayByLocalPlayer(this.position, "UseActions/service_vehicle");
			return;
		}
		if (base.CommandIs(_commandName, "repair"))
		{
			this.DoRepairAction(uiforPrimaryPlayer);
			this.stopInteraction(0);
			return;
		}
		if (base.CommandIs(_commandName, "lock"))
		{
			this.PlaySound("misc/locking", 1f);
			this.isLocked = !this.isLocked;
			this.stopInteraction(2);
			return;
		}
		if (base.CommandIs(_commandName, "unlock"))
		{
			this.PlaySound("misc/unlocking", 1f);
			this.isLocked = !this.isLocked;
			this.stopInteraction(2);
			return;
		}
		if (base.CommandIs(_commandName, "keypad"))
		{
			this.doKeypadAction(uiforPrimaryPlayer);
			return;
		}
		if (base.CommandIs(_commandName, "take") || base.CommandIs(_commandName, "force_pickup"))
		{
			this.pickup(primaryPlayer);
			return;
		}
		if (base.CommandIs(_commandName, "drone_command_stay"))
		{
			this.SentryMode();
			this.stopInteraction(0);
			return;
		}
		if (base.CommandIs(_commandName, "drone_command_follow"))
		{
			this.FollowMode(true);
			this.stopInteraction(0);
			return;
		}
		if (base.CommandIs(_commandName, "drone_command_heal"))
		{
			this.HealRequest();
			this.stopInteraction(0);
			return;
		}
		if (base.CommandIs(_commandName, "storage"))
		{
			this.openStorageWindow(uiforPrimaryPlayer);
			return;
		}
		if (base.CommandIs(_commandName, "drone_silent_on") || base.CommandIs(_commandName, "drone_silent_off"))
		{
			this.isQuietMode = !this.isQuietMode;
			Handle handle = this.idleLoop;
			if (handle != null)
			{
				handle.Stop(this.entityId);
			}
			this.idleLoop = null;
			this.stopInteraction(32);
			return;
		}
		if (base.CommandIs(_commandName, "drone_light_on") || base.CommandIs(_commandName, "drone_light_off"))
		{
			this.ToggleLightAction();
			this.stopInteraction(0);
			return;
		}
		if (base.CommandIs(_commandName, "drone_heal_allies") || base.CommandIs(_commandName, "drone_dont_heal_allies"))
		{
			this.ToggleHealAllies();
			this.stopInteraction(0);
			return;
		}
		if (base.CommandIs(_commandName, "drone_attack_mode_passive") || base.CommandIs(_commandName, "drone_attack_mode_aggressive"))
		{
			this.ToggleAttackMode();
			this.stopInteraction(0);
			return;
		}
		this.stopInteraction(0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void stopInteraction(ushort syncFlags = 0)
	{
		if (syncFlags != 0)
		{
			this.SendSyncData(syncFlags);
		}
		LockManager.Instance.UnlockRequestLocal();
	}

	public void OpenStorageFromDialog(Entity _entityFocusing)
	{
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_entityFocusing as EntityPlayerLocal);
		this.openStorageWindow(uiforPlayer);
		Entity.EntityLockContext entityLockContext = new Entity.EntityLockContext("storage", this.bag);
		LockManager.Instance.LockRequestLocal(this, entityLockContext, 0);
	}

	public void ToggleOrderState()
	{
		EntityDrone.Orders orders = this.orderState;
		if (orders == EntityDrone.Orders.Follow)
		{
			this.SentryMode();
			return;
		}
		if (orders != EntityDrone.Orders.Stay)
		{
			return;
		}
		this.FollowMode(true);
	}

	public void ToggleHealAllies()
	{
		this.setHealAllies(!this.IsHealingAllies);
		this.SendSyncData(256);
	}

	public void ToggleAttackMode()
	{
		EntityDrone.AttackMode attackMode = this.attackMode;
		if (attackMode == EntityDrone.AttackMode.Passive)
		{
			this.SetAttacKMode(EntityDrone.AttackMode.Aggressive);
			return;
		}
		if (attackMode != EntityDrone.AttackMode.Aggressive)
		{
			return;
		}
		this.SetAttacKMode(EntityDrone.AttackMode.Passive);
	}

	public void ToggleLightAction()
	{
		this.IsFlashlightOn = !this.IsFlashlightOn;
		this.setFlashlightOn(this.IsFlashlightOn);
		this.SendSyncData(64);
	}

	public void DoRepairAction(LocalPlayerUI playerUI)
	{
		string text = "resourceRepairKit";
		if (this.HasStoredItem(playerUI.entityPlayer, text, EntityDrone.repairKitTags))
		{
			if (this.GetRepairAmountNeeded() > 0)
			{
				playerUI.xui.CollectedItemList.RemoveItemStack(new ItemStack(ItemClass.GetItem(text, false), 1));
				this.PlaySound("crafting/craft_repair_item", 1f);
				this.TakeStoredItem(playerUI.entityPlayer, text, EntityDrone.repairKitTags);
				this.performRepair();
				this.SendSyncData(16);
				return;
			}
		}
		else
		{
			Manager.PlayInsidePlayerHead("misc/missingitemtorepair", -1, 0f, false, false);
		}
	}

	public void HealRequest()
	{
		if (!this.healWeapon.hasHealingItem())
		{
			GameManager.ShowTooltip(this.Owner as EntityPlayerLocal, Localization.Get("xuiDroneNeedsHealItemsStored", false, null), string.Empty, "ui_denied", null, false, false, 0f);
			this.PlaySound("drone_empty", 1f);
			return;
		}
		if (this.state != EntityDrone.State.Heal && this.healWeapon.canFire())
		{
			this.userRequestedHeal = true;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.healTargetServer(this.Owner, this.userRequestedHeal);
			}
			else
			{
				this.healRequestClient();
			}
			this.userRequestedHeal = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void startDialog(Entity _entityFocusing)
	{
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_entityFocusing as EntityPlayerLocal);
		uiforPlayer.xui.Dialog.Respondent = this;
		uiforPlayer.windowManager.CloseAllOpenModalWindows(null, false);
		XUiC_DialogWindowGroup.Open(uiforPlayer.xui, new Action(this.StopUIInteraction));
		this.PlayVO("drone_greeting", false, 1f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openStorageWindow(LocalPlayerUI playerUI)
	{
		XUiC_BagStorageWindowGroup.Open(playerUI.xui, this, this.bag, LootContainer.GetLootContainer("roboticDrone", true), Localization.Get("xuiStorage", false, null), delegate
		{
			this.SendSyncData(8);
		}, new Action(this.StopUIInteraction), delegate
		{
			float distanceSq = this.GetDistanceSq(playerUI.entityPlayer);
			float num = Constants.cDigAndBuildDistance + 1f;
			return distanceSq <= num * num;
		}, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doKeypadAction(LocalPlayerUI playerUI)
	{
		XUiC_KeypadWindow.Open(playerUI, this, new Action(this.StopUIInteraction), delegate
		{
			float distanceSq = this.GetDistanceSq(playerUI.entityPlayer);
			float num = Constants.cDigAndBuildDistance + 1f;
			return distanceSq <= num * num;
		});
	}

	public void OnWakeUp()
	{
	}

	public void SetItemValueToLoad(ItemValue itemValue)
	{
		this.itemvalueToLoad = itemValue.Clone();
	}

	public void LoadMods()
	{
		Vector2i size = LootContainer.GetLootContainer("roboticDrone", true).size;
		int num = size.x * size.y;
		this.lightManager.DisableMaterials("junkDroneLamp");
		GameObject gameObject = base.transform.FindInChilds("freightBox", false).gameObject;
		GameObject gameObject2 = base.transform.FindInChilds("armor", false).gameObject;
		GameObject gameObject3 = base.transform.FindInChilds("machineGun", false).gameObject;
		GameObject gameObject4 = base.transform.FindInChilds("teddyBear", false).gameObject;
		GameObject gameObject5 = base.transform.FindInChilds("junkDroneArmRight", false).gameObject;
		if (gameObject != null)
		{
			gameObject.SetActive(false);
		}
		if (gameObject2 != null)
		{
			gameObject2.SetActive(false);
		}
		if (gameObject3 != null)
		{
			gameObject3.SetActive(false);
		}
		if (gameObject4 != null)
		{
			gameObject4.SetActive(false);
		}
		if (gameObject5 != null)
		{
			gameObject5.SetActive(true);
		}
		for (int i = 0; i < this.installedWeapons.Count; i++)
		{
			DroneWeapons.Weapon weapon = this.installedWeapons[i];
			this.installedWeapons.Remove(weapon);
			weapon.Unequip();
		}
		this.stunWeapon = null;
		this.healWeapon = null;
		if (this.state == EntityDrone.State.Attack)
		{
			this.SetState(EntityDrone.State.Idle, true);
		}
		this.IsFlashlightAttached = false;
		this.setFlashlightOn(false);
		if (this.OriginalItemValue.HasMods())
		{
			for (int j = 0; j < this.OriginalItemValue.Modifications.Length; j++)
			{
				ItemValue itemValue = this.OriginalItemValue.Modifications[j];
				if (itemValue.ItemClass != null)
				{
					string name = itemValue.ItemClass.Name;
					uint num2 = <PrivateImplementationDetails>.ComputeStringHash(name);
					if (num2 <= 2400030839U)
					{
						if (num2 != 1912183181U)
						{
							if (num2 != 2266484491U)
							{
								if (num2 == 2400030839U)
								{
									if (name == "modRoboticDroneWeaponMod")
									{
										if (gameObject3)
										{
											gameObject3.SetActive(true);
										}
									}
								}
							}
							else if (name == "modRoboticDroneMoraleBoosterMod")
							{
								this.isSupportModAttached = true;
								if (gameObject4)
								{
									gameObject4.SetActive(true);
								}
							}
						}
						else if (name == "modRoboticDroneCargoMod")
						{
							num += 8;
							if (gameObject)
							{
								gameObject.SetActive(true);
							}
						}
					}
					else if (num2 <= 3474526689U)
					{
						if (num2 != 2404831999U)
						{
							if (num2 == 3474526689U)
							{
								if (name == "modRoboticDroneArmorPlatingMod")
								{
									this.armorDamageReduction = 0.5f;
									if (gameObject2)
									{
										gameObject2.SetActive(true);
									}
								}
							}
						}
						else if (name == "modRoboticDroneMedicMod")
						{
							this.healWeapon = new DroneWeapons.HealBeamWeapon(this);
							this.healWeapon.Init();
							this.healWeapon.Equip(itemValue);
							this.installedWeapons.Add(this.healWeapon);
						}
					}
					else if (num2 != 3914512375U)
					{
						if (num2 == 4027736419U)
						{
							if (name == "modRoboticDroneStunWeaponMod")
							{
								this.stunWeapon = new DroneWeapons.StunBeamWeapon(this);
								this.stunWeapon.Init();
								this.stunWeapon.Equip(itemValue);
								this.installedWeapons.Add(this.stunWeapon);
								if (gameObject3)
								{
									gameObject3.SetActive(true);
								}
								if (gameObject5)
								{
									gameObject5.SetActive(false);
								}
							}
						}
					}
					else if (name == "modRoboticDroneHeadlampMod")
					{
						this.IsFlashlightAttached = true;
						DroneLightManager.LightEffect[] lightEffects = this.lightManager.LightEffects;
						if (lightEffects.Length != 0)
						{
							LightManager.RegisterMovingLight(this, lightEffects[0].linkedObjects[0].GetComponent<Light>());
						}
						if (this.IsFlashlightOn)
						{
							this.setFlashlightOn(true);
						}
					}
				}
			}
		}
		ItemStack[] slots = this.bag.GetSlots();
		if (slots == null || slots.Length != num)
		{
			ItemStack[] array = ItemStack.CreateArray(num);
			if (slots != null)
			{
				int num3 = Mathf.Min(slots.Length, num);
				for (int k = 0; k < num3; k++)
				{
					array[k] = slots[k];
				}
			}
			this.bag.SetSlots(array);
		}
		Color color = this.prefabColor;
		ItemValue itemValue2 = this.OriginalItemValue.CosmeticMods[0];
		if (this.OriginalItemValue.CosmeticMods.Length != 0 && itemValue2 != null && !itemValue2.IsEmpty())
		{
			Vector3 vector = Block.StringToVector3(this.OriginalItemValue.GetPropertyOverride(Block.PropTintColor, "255,255,255"));
			color.r = vector.x;
			color.g = vector.y;
			color.b = vector.z;
		}
		for (int l = 0; l < this.paintableParts.Length; l++)
		{
			this.SetPaint(this.paintableParts[l], color);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initWorldValues(bool value)
	{
		this.bWillRespawn = value;
	}

	public void SyncOwnerData()
	{
		if (this.isOwnerSyncPending)
		{
			this.notifySyncOwner();
			this.isOwnerSyncPending = false;
		}
		if (this.belongsPlayerId == -1)
		{
			PersistentPlayerList persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
			if (persistentPlayerList != null)
			{
				PersistentPlayerData playerData = persistentPlayerList.GetPlayerData(this.OwnerID);
				if (playerData != null)
				{
					this.belongsPlayerId = playerData.EntityId;
				}
			}
		}
		if (!this.Owner)
		{
			this.Owner = (EntityAlive)GameManager.Instance.World.GetEntity(this.belongsPlayerId);
			if (this.Owner)
			{
				this.Owner.AddOwnedEntity(this);
				if (!this.hasNavObjectsEnabled && GameManager.Instance.World.IsLocalPlayer(this.belongsPlayerId))
				{
					this.HandleNavObject();
					this.hasNavObjectsEnabled = true;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void notifySyncOwner()
	{
		PersistentPlayerData playerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(this.OwnerID);
		if (playerData != null)
		{
			this.belongsPlayerId = playerData.EntityId;
			this.Owner = GameManager.Instance.World.GetEntity(this.belongsPlayerId) as EntityAlive;
		}
		if (this.Owner)
		{
			this.rotation = Quaternion.LookRotation(this.Owner.position - this.position).eulerAngles;
			if (GameManager.Instance.World.IsLocalPlayer(this.belongsPlayerId))
			{
				this.HandleNavObject();
				this.hasNavObjectsEnabled = true;
				this.SetOwner(this.OwnerID);
				this.SendSyncData(3);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool belongsToPlayerId(int id)
	{
		return this.belongsPlayerId == id;
	}

	public Color GetPaintColor()
	{
		return base.transform.FindRecursive("BaseMesh").GetComponentInChildren<Renderer>().sharedMaterial.color;
	}

	public void SetPaint(string childName, Color color)
	{
		Transform transform = base.transform.FindRecursive(childName);
		if (transform && transform.gameObject.activeSelf)
		{
			Renderer[] componentsInChildren = transform.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material.color = color;
			}
		}
	}

	public bool PlayWakeupAnim { get; set; }

	[PublicizedFrom(EAccessModifier.Private)]
	public void playWakeupAnim()
	{
		this.animator = base.GetComponentInChildren<Animator>();
		if (this.animator)
		{
			this.animator.enabled = true;
			this.animator.Play("Base Layer.SpawnIn");
			this.WakeupAnimTime = 2.5f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playIdleAnim()
	{
		this.animator = base.GetComponentInChildren<Animator>();
		if (this.animator)
		{
			this.animator.enabled = true;
			this.animator.Play("Base Layer.Idle", 0, 0f);
			this.animator.Update(0f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setShutdownAnim()
	{
		this.animator = base.GetComponentInChildren<Animator>();
		if (this.animator)
		{
			this.animator.Play("Base Layer.SpawnIn", 0, 0f);
			this.animator.Update(0f);
			this.animator.StopPlayback();
			this.animator.enabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateAnimStates()
	{
		if (!this.isAnimationStateSet)
		{
			if (this.Health > 1 && this.Owner)
			{
				if (this.PlayWakeupAnim)
				{
					this.playWakeupAnim();
					this.PlayWakeupAnim = false;
				}
				else
				{
					this.playIdleAnim();
					this.WakeupAnimTime = 2.5f;
				}
			}
			else
			{
				this.setShutdownAnim();
			}
			this.isAnimationStateSet = true;
		}
		if (this.WakeupAnimTime > 0f)
		{
			this.WakeupAnimTime -= 0.05f;
			if (this.WakeupAnimTime <= 0f && !GameManager.IsDedicatedServer)
			{
				if (this.Owner)
				{
					Manager.Stop(this.Owner.entityId, "drone_take");
				}
				this.PlayVO("drone_wakeup", false, 1f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void unRegsiterMovingLights()
	{
		DroneLightManager.LightEffect[] lightEffects = this.lightManager.LightEffects;
		if (lightEffects.Length != 0)
		{
			LightManager.UnRegisterMovingLight(this, lightEffects[0].linkedObjects[0].GetComponent<Light>());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setFlashlightOn(bool value)
	{
		if (value)
		{
			this.lightManager.InitMaterials("junkDroneLamp");
			return;
		}
		this.lightManager.DisableMaterials("junkDroneLamp");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Handle PlaySoundLoop(string sound_path, float _vol = 1f)
	{
		return Manager.Play(this, sound_path, _vol, true);
	}

	public void PlaySound(string sound_path, float _vol = 1f)
	{
		this.PlaySound(this, sound_path, false, false, _vol);
	}

	public void PlayVO(string sound_path, bool _hasPriority = false, float _vol = 1f)
	{
		if (GameManager.Instance.World.IsLocalPlayer(this.belongsPlayerId))
		{
			this.PlaySound(this, sound_path, true, _hasPriority, _vol);
		}
	}

	public void PlaySound(Entity entity, string sound_path, bool _isVO = false, bool _hasPriority = false, float _vol = 1f)
	{
		if (!this.isQuietMode)
		{
			if (_isVO)
			{
				if (_hasPriority)
				{
					Handle handle = this.voHandle;
					if (handle != null)
					{
						handle.Stop(this.entityId);
					}
				}
				this.voHandle = Manager.Play(entity, sound_path, _vol, true);
				return;
			}
			Manager.Play(entity, sound_path, _vol, false);
		}
	}

	public bool isAlly(EntityAlive _target)
	{
		if (this.debugFriendlyFire)
		{
			return false;
		}
		if (this.Owner && this.Owner == _target)
		{
			return true;
		}
		PersistentPlayerList persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
		PersistentPlayerData playerData = persistentPlayerList.GetPlayerData(this.OwnerID);
		if (playerData != null && persistentPlayerList.EntityToPlayerMap.ContainsKey(_target.entityId))
		{
			PersistentPlayerData persistentPlayerData = persistentPlayerList.EntityToPlayerMap[_target.entityId];
			if (persistentPlayerData != null && playerData.IsAlly(persistentPlayerData))
			{
				return true;
			}
			EntityPlayer entityPlayer = this.Owner as EntityPlayer;
			EntityPlayer entityPlayer2 = _target as EntityPlayer;
			if (entityPlayer && entityPlayer2 && entityPlayer.Party != null && entityPlayer.Party.ContainsMember(entityPlayer2))
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onPartyMemberAdded(EntityPlayer player)
	{
		this.registeredPartyMembers.Add(player.entityId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onPartyMemberRemoved(EntityPlayer player)
	{
		this.registeredPartyMembers.Remove(player.entityId);
		this.removeSupportBuff(player);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removePartyBuffs(EntityPlayer owner)
	{
		if (owner.Party != null)
		{
			for (int i = 0; i < owner.Party.MemberList.Count; i++)
			{
				EntityAlive entityAlive = owner.Party.MemberList[i];
				this.removeSupportBuff(entityAlive);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void procBuffRange(EntityAlive entity)
	{
		if (entity)
		{
			if ((this.position - entity.position).magnitude < 32f)
			{
				this.addSupportBuff(entity);
				return;
			}
			this.removeSupportBuff(entity);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addSupportBuff(EntityAlive entity)
	{
		if (this.state != EntityDrone.State.Shutdown && !entity.Buffs.HasBuff("buffJunkDroneSupportEffect"))
		{
			entity.Buffs.AddBuff("buffJunkDroneSupportEffect", -1, true, false, -1f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeSupportBuff(EntityAlive entity)
	{
		if (entity && entity.Buffs.HasBuff("buffJunkDroneSupportEffect") && !this.doesEntityHaveSupport(entity))
		{
			entity.Buffs.RemoveBuff("buffJunkDroneSupportEffect", -1, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool doesEntityHaveSupport(EntityAlive entity)
	{
		OwnedEntityData[] ownedEntities = entity.GetOwnedEntities();
		for (int i = 0; i < ownedEntities.Length; i++)
		{
			EntityDrone entityDrone = this.world.GetEntity(ownedEntities[i].Id) as EntityDrone;
			if (entityDrone && entityDrone.isSupportModAttached)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void buffAllies()
	{
		EntityPlayer entityPlayer = this.Owner as EntityPlayer;
		if (entityPlayer)
		{
			if (entityPlayer.Party != null)
			{
				this.knownPartyMembers = entityPlayer.Party.GetMemberIdArray();
				for (int i = 0; i < this.knownPartyMembers.Length; i++)
				{
					EntityAlive entityAlive = this.world.GetEntity(this.knownPartyMembers[i]) as EntityAlive;
					this.procBuffRange(entityAlive);
				}
				return;
			}
			if (this.knownPartyMembers != null && this.knownPartyMembers.Length != 0)
			{
				for (int j = 0; j < this.knownPartyMembers.Length; j++)
				{
					EntityAlive entityAlive2 = this.world.GetEntity(this.knownPartyMembers[j]) as EntityAlive;
					this.removeSupportBuff(entityAlive2);
				}
				this.knownPartyMembers = null;
			}
			this.procBuffRange(entityPlayer);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePartyBuffs()
	{
		if (this.Owner && this.isSupportModAttached && !this.isEntityRemote)
		{
			this.buffAllies();
		}
	}

	public bool HasStoredItem(EntityAlive entity, string itemGroupOrName, FastTags<TagGroup.Global> fastTags)
	{
		ItemValue item = ItemClass.GetItem(itemGroupOrName, false);
		bool itemClass = item.ItemClass != null;
		int num = 0;
		int num2 = 0;
		if (itemClass)
		{
			num = entity.bag.GetItemCount(item, -1, -1, true);
			num2 = entity.inventory.GetItemCount(item, false, -1, -1, true);
		}
		return num + num2 > 0;
	}

	public ItemStack TakeStoredItem(EntityAlive entity, string itemGroupOrName, FastTags<TagGroup.Global> fastTags)
	{
		ItemValue item = ItemClass.GetItem(itemGroupOrName, false);
		if (item.ItemClass != null)
		{
			entity.bag.GetItemCount(item, -1, -1, true);
			if (entity.inventory.GetItemCount(item, false, -1, -1, true) > 0)
			{
				entity.inventory.DecItem(item, 1, false, null);
			}
			else
			{
				entity.bag.DecItem(item, 1, false, null);
			}
			return new ItemStack(item.Clone(), 1);
		}
		return null;
	}

	public ItemValue GetUpdatedItemValue()
	{
		this.OriginalItemValue.UseTimes = (float)this.OriginalItemValue.MaxUseTimes * (1f - (float)this.Health / base.Stats.Health.BaseMax);
		return this.OriginalItemValue;
	}

	public override void OnCollectServer(int _playerId)
	{
		this.OriginalItemValue = this.GetUpdatedItemValue();
		base.transform.gameObject.SetActive(false);
		if (this.Owner)
		{
			this.Owner.RemoveOwnedEntity(this.entityId);
			if (DroneManager.Instance != null)
			{
				DroneManager.Instance.RemoveTrackedDrone(this, EnumRemoveEntityReason.Despawned);
			}
		}
		this.world.RemoveEntity(this.entityId, EnumRemoveEntityReason.Killed);
	}

	public override void OnCollectLocal(int _playerId)
	{
		EntityPlayerLocal entityPlayerLocal = this.world.GetEntity(_playerId) as EntityPlayerLocal;
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		ItemStack itemStack = new ItemStack(this.GetUpdatedItemValue(), 1);
		if (!uiforPlayer.xui.PlayerInventory.Toolbelt.AddItem(itemStack) && !uiforPlayer.xui.PlayerInventory.AddItem(itemStack))
		{
			GameManager.Instance.ItemDropServer(itemStack, entityPlayerLocal.GetPosition(), Vector3.zero, _playerId, 60f, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void pickup(Entity _entityFocusing)
	{
		if (!this.bag.IsEmpty())
		{
			this.PlayVO("drone_takefail", true, 1f);
			GameManager.ShowTooltip(this.Owner as EntityPlayerLocal, Localization.Get("ttEmptyDroneBeforePickup", false, null), string.Empty, "ui_denied", null, false, false, 0f);
			this.stopInteraction(0);
			return;
		}
		ItemStack itemStack = new ItemStack(this.GetUpdatedItemValue(), 1);
		EntityPlayer entityPlayer = _entityFocusing as EntityPlayer;
		if (entityPlayer.inventory.CanTakeItem(itemStack) || entityPlayer.bag.CanTakeItem(itemStack))
		{
			this.isBeingPickedUp = true;
			this.PlaySound(entityPlayer, "drone_take", true, true, 1f);
			this.initWorldValues(false);
			this.nativeCollider.enabled = false;
			base.Collect(entityPlayer.entityId);
			if (entityPlayer.Buffs.HasBuff("buffJunkDroneSupportEffect"))
			{
				entityPlayer.Buffs.RemoveBuff("buffJunkDroneSupportEffect", -1, true);
			}
			this.removePartyBuffs(entityPlayer);
			this.unRegsiterMovingLights();
		}
		else
		{
			GameManager.ShowTooltip(entityPlayer as EntityPlayerLocal, Localization.Get("xuiInventoryFullForPickup", false, null), string.Empty, "ui_denied", null, false, false, 0f);
		}
		this.stopInteraction(0);
	}

	public int StorageCapacity
	{
		get
		{
			return this.bag.SlotCount;
		}
	}

	public int GetStoredItemCount()
	{
		return this.bag.GetUsedSlotCount();
	}

	public bool CanRemoveExtraStorage()
	{
		return this.GetStoredItemCount() < this.StorageCapacity - 8;
	}

	public void NotifyToManyStoredItems()
	{
		if (this.overItemLimitCooldown > 0f)
		{
			return;
		}
		this.overItemLimitCooldown = 5f;
		if (!this.CanRemoveExtraStorage())
		{
			GameManager.ShowTooltip(this.Owner as EntityPlayerLocal, Localization.Get("ttJunkDroneEmptySomeStorage", false, null), string.Empty, "ui_denied", null, false, false, 0f);
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDroneServiceMenu()
	{
		if (this.overItemLimitCooldown > 0f)
		{
			this.overItemLimitCooldown -= 0.05f;
		}
	}

	public int GetRepairAmountNeeded()
	{
		return this.GetMaxHealth() - this.Health;
	}

	public void RepairParts(int _amount)
	{
		this.Health += _amount;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void performRepair()
	{
		this.Health = (int)base.Stats.Health.Max;
		this.OriginalItemValue.UseTimes = 0f;
		this.setShutdown(false);
		this.playWakeupAnim();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.SendSyncData(16);
		}
	}

	public float EnemyDetectionRadius
	{
		get
		{
			return this.sensors.EnemyDetectionRadius;
		}
	}

	public EntityAlive GetNearestEnemyInRange(Vector3 targetPos)
	{
		return this.sensors.GetNearestEnemyInRange(targetPos);
	}

	public bool IsOwnerSneaking()
	{
		return this.Owner && this.Owner.IsCrouching && !this.sensors.IsOwnerAttackTarget();
	}

	public EntityDrone.State GetState()
	{
		return this.state;
	}

	public void SetState(EntityDrone.State next, bool sync = false)
	{
		this.setState(next);
		if (sync)
		{
			this.SendSyncData(32768);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setState(EntityDrone.State next)
	{
		this.lastState = this.state;
		this.state = next;
		this.stateTime = 0f;
		switch (this.state)
		{
		case EntityDrone.State.Idle:
		case EntityDrone.State.Sentry:
			break;
		case EntityDrone.State.Follow:
			if (this.lastState == EntityDrone.State.Sentry && this.Owner && this.Owner.HasOwnedEntity(this.entityId))
			{
				this.Owner.GetOwnedEntity(this.entityId).ClearLastKnownPostition();
				return;
			}
			break;
		case EntityDrone.State.Heal:
			this.clearNeedsHealItemCheck();
			break;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateState()
	{
		this.stateTime += 0.05f;
		switch (this.state)
		{
		case EntityDrone.State.Idle:
			this.idleState();
			return;
		case EntityDrone.State.Sentry:
			this.sentryState();
			break;
		case EntityDrone.State.Follow:
			this.followState();
			return;
		case EntityDrone.State.Heal:
			this.healState();
			return;
		case EntityDrone.State.Attack:
			this.attackState();
			return;
		case EntityDrone.State.Shutdown:
		case EntityDrone.State.NoClip:
			break;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateTransitionState()
	{
		if (this.transitionState != EntityDrone.State.None)
		{
			if (this.state == this.transitionState)
			{
				this.transitionState = EntityDrone.State.None;
				return;
			}
			if ((this.state == EntityDrone.State.Attack || this.state == EntityDrone.State.Heal) && this.transitionState == EntityDrone.State.Idle)
			{
				for (int i = 0; i < this.installedWeapons.Count; i++)
				{
					this.installedWeapons[i].RefreshCooldown();
				}
			}
			EntityDrone.State state = this.transitionState;
			if (state != EntityDrone.State.Idle)
			{
				if (state != EntityDrone.State.Heal)
				{
					if (state == EntityDrone.State.Shutdown)
					{
						this.isShutdownPending = true;
					}
					else
					{
						this.setState(this.transitionState);
					}
				}
				else
				{
					if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						this.healTargetServer(base.GetAttackTarget(), this.userRequestedHeal);
					}
					else
					{
						this.setState(this.transitionState);
						this.initSuppressVOTimer = 0f;
					}
					this.userRequestedHeal = false;
				}
			}
			else if (this.state == EntityDrone.State.Shutdown)
			{
				this.setShutdown(false);
			}
			else
			{
				this.setState(this.transitionState);
			}
			this.transitionState = EntityDrone.State.None;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void idleState()
	{
		EntityAlive owner = this.Owner;
		if (owner)
		{
			Vector3 chestPosition = owner.getChestPosition();
			if (this.onUnderWaterState(chestPosition))
			{
				return;
			}
			bool flag = this.sensors.IsEnemyInRange();
			if (flag && !this.steering.IsInRange(chestPosition, this.sensors.EnemyDetectionRadius))
			{
				this.setState(EntityDrone.State.Follow);
				return;
			}
			if (!this.steering.IsInRange(chestPosition, this.FollowDistance + 2f) && !flag)
			{
				this.setState(EntityDrone.State.Follow);
				return;
			}
			if (!this.isEntityAboveOrBelow(owner))
			{
				this.rotateTo(this.steering.GetDir2D(this.position, chestPosition));
				float num = 0f;
				if (this.position.y - chestPosition.y > num || this.position.y - chestPosition.y < num)
				{
					Vector3 position = this.position;
					position.y = chestPosition.y;
					this.move(this.steering.Seek(this.position, position, this.SpeedFlying * 0.5f), this.SpeedFlying, false);
				}
			}
		}
	}

	public EntityDrone.Orders OrderState
	{
		get
		{
			return this.orderState;
		}
	}

	public void SentryMode()
	{
		this.PlayVO("drone_command", true, 1f);
		this.SentryPos = this.position;
		this.setOrders(EntityDrone.Orders.Stay);
		this.setState(EntityDrone.State.Sentry);
		this.SendSyncData(49152);
		if (this.Owner && this.Owner.HasOwnedEntity(this.entityId))
		{
			this.Owner.GetOwnedEntity(this.entityId).SetLastKnownPosition(this.position);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setOrders(EntityDrone.Orders orders)
	{
		this.orderState = orders;
		this.initWorldValues(this.orderState == EntityDrone.Orders.Follow);
		if (GameManager.Instance.World.IsLocalPlayer(this.belongsPlayerId))
		{
			this.HandleNavObject();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void sentryState()
	{
		Vector3 sentryPos = this.SentryPos;
		if (this.world.IsChunkAreaLoaded(sentryPos))
		{
			if ((sentryPos - this.position).magnitude > 5f)
			{
				if (!this.DoMoveIntoFollowPos(sentryPos, 1.414f, base.transform.forward, 0.1f, true, 10f))
				{
					return;
				}
				this.clearCurrentPath();
			}
			if (!this.steering.IsInRange(sentryPos, 0.25f))
			{
				Vector3 vector = this.steering.Seek(this.position, sentryPos, 0.25f);
				this.rotateTo((sentryPos - this.position).normalized);
				this.move(vector, false);
				return;
			}
		}
	}

	public bool CanInterruptFollow
	{
		get
		{
			return true;
		}
	}

	public void FollowMode(bool playVO = true)
	{
		if (playVO)
		{
			this.PlayVO("drone_command", true, 1f);
		}
		this.setOrders(EntityDrone.Orders.Follow);
		this.setState(EntityDrone.State.Follow);
		this.SendSyncData(49152);
	}

	public bool IsAttachedToVehicle(Entity entity)
	{
		return entity && entity.AttachedToEntity as EntityVehicle != null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInteractionFocusOwner()
	{
		if (this.Owner)
		{
			Ray lookRay = this.Owner.GetLookRay();
			RaycastHit raycastHit;
			if (Physics.Raycast(lookRay.origin - Origin.position, lookRay.direction, ref raycastHit, 1000f, 16384) && raycastHit.transform && raycastHit.transform.tag != "Physics")
			{
				Entity component = raycastHit.transform.GetComponent<Entity>();
				if (component && component.EntityClass.entityClassName == "entityJunkDrone")
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool onInterruptState()
	{
		if (this.isInteractionFocusOwner())
		{
			this.ownerFocusTimer += 0.05f;
			if (this.ownerFocusTimer >= 0.2f)
			{
				this.ownerFocusTimer = 0f;
				this.setState(EntityDrone.State.Idle);
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool onVehicleState(EntityAlive entity, Vector3 followPoint)
	{
		if (this.IsAttachedToVehicle(entity))
		{
			if (!this.ownerIsOnVehicle)
			{
				this.ownerIsOnVehicle = true;
				this.setNoClip(true);
				this.clearCurrentPath();
			}
			Entity attachedToEntity = entity.AttachedToEntity;
			Vector3 vector = attachedToEntity.position - attachedToEntity.transform.forward * 5f * 2f + Vector3.up * 5f * 2f;
			this.steerFollow(entity, vector);
			return true;
		}
		if (this.ownerIsOnVehicle)
		{
			this.ownerIsOnVehicle = false;
			this.SetPosition(followPoint, true);
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool onUnderWaterState(Vector3 chestPos)
	{
		if (this.isTargetUnderWater(chestPos))
		{
			if (this.currentPath.Count > 0)
			{
				this.clearCurrentPath();
			}
			Vector3 vector = this.steering.Seek(this.position, this.findOpenBlockAbove(chestPos, 256), 0.2f);
			this.rotateTo(vector);
			this.move(vector, false);
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool DoMoveIntoFollowPos(Vector3 targetPos, float seekDist, Vector3 seekForward, float pointRadius, bool debugDraw = false, float duration = 0f)
	{
		Utils.DrawCircleLinesHorzontal(this.position - Origin.position, seekDist, Color.white, Color.red, 24, 0.05f);
		Vector3 normalized = (targetPos - this.position).normalized;
		float magnitude = (targetPos - this.position).magnitude;
		if (this.currentPath.Count == 0)
		{
			EntityDrone.GetPath(this.currentPath, this, this.position, targetPos, this.SpeedFlying, null, seekDist, pointRadius, debugDraw, duration);
			if (this.currentPath.Count > 0)
			{
				this.currentPathDest = this.currentPath[this.currentPath.Count - 1];
			}
			else
			{
				this.currentPathDest = targetPos;
			}
		}
		else if (EntityDrone.IsPositionBlocked(this.position, targetPos, 1073807360, false, 0f) || (float)this.currentPath.Count > seekDist + 1f || (this.currentPath.Count > 0 && magnitude > seekDist + 1.414f))
		{
			this.followPlannedPath(this.SpeedFlying, pointRadius, debugDraw, duration);
		}
		else if (magnitude >= seekDist)
		{
			this.RotateTo(normalized);
			this.Move(targetPos, pointRadius);
		}
		return !EntityDrone.IsPositionBlocked(this.position, targetPos, 1073807360, false, 0f) && magnitude <= seekDist;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool DoMoveIntoFollowPos(EntityAlive avaliableTarget, float seekDist, Vector3 seekForward, float pointRadius, bool debugDraw = false, float duration = 0f)
	{
		Vector3 chestPosition = avaliableTarget.getChestPosition();
		return this.DoMoveIntoFollowPos(chestPosition, seekDist, seekForward, pointRadius, debugDraw, duration);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void steerFollow(EntityAlive entity, Vector3 followPoint)
	{
		if (entity)
		{
			if (!this.steering.IsInRange(entity.position, 10f))
			{
				if (this.decelerationTime > 0f)
				{
					this.decelerationTime = 0f;
				}
				this.accelerationTime += 0.05f;
				this.currentSpeedFlying = Mathf.Lerp(this.currentSpeedFlying, Mathf.Max(15f, (entity.position - this.position).magnitude), Mathf.Clamp01(this.accelerationTime / this.SpeedFlying));
			}
			else
			{
				if (this.accelerationTime > 0f)
				{
					this.accelerationTime = 0f;
				}
				this.decelerationTime += 0.05f;
				this.currentSpeedFlying = Mathf.Lerp(this.currentSpeedFlying, this.SpeedFlying, Mathf.Clamp01(this.decelerationTime / (this.SpeedFlying * 0.5f)));
			}
		}
		Vector3 vector = this.steering.Seek(this.position, followPoint, this.SpeedFlying);
		if (!this.steering.IsInRange(followPoint, 0.1f))
		{
			Vector3 chestPosition = entity.getChestPosition();
			float magnitude = (chestPosition - this.position).magnitude;
			if (magnitude > 5f && magnitude < 24f && !RaycastPathUtils.IsPointBlocked(this.position, chestPosition, 1073807360, false, 0f) && Vector3.Angle(entity.GetLookVector(), this.position - chestPosition) < 45f)
			{
				float num = 0.5f;
				Vector3 vector2 = this.steering.Flee(this.position, chestPosition, this.SpeedFlying);
				if (!RaycastPathUtils.IsPositionBlocked(this.position, this.position + (vector + vector2), 1073807360, false))
				{
					vector += vector2 * num;
				}
			}
			if (this.steering.GetAltitude(this.position) < magnitude * 0.33f && !RaycastPathUtils.IsPositionBlocked(this.position, this.position + Vector3.up, 1073807360, false))
			{
				float num2 = 0.75f;
				Vector3 vector3 = this.steering.Seek(this.position, this.position + Vector3.up, this.SpeedFlying);
				vector += vector3 * num2;
			}
			this.rotateTo((chestPosition - this.position).normalized);
			this.move(vector.normalized, followPoint, this.currentSpeedFlying, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void followState()
	{
		EntityAlive owner = this.Owner;
		if (!owner)
		{
			return;
		}
		if (this.onInterruptState())
		{
			return;
		}
		Vector3 chestPosition = owner.getChestPosition();
		if (this.onUnderWaterState(chestPosition))
		{
			return;
		}
		Vector3[] groupPositions = EntityDrone.GetGroupPositions(owner, 5f, false, 0f);
		Array.Sort<Vector3>(groupPositions, (Vector3 x, Vector3 y) => Vector3.Distance(this.position, x).CompareTo(Vector3.Distance(this.position, y)));
		Vector3 vector = groupPositions[0];
		if (this.onVehicleState(owner, vector))
		{
			return;
		}
		if (PathFinderThread.Instance != null)
		{
			if (!this.DoMoveIntoFollowPos(owner, 5f, base.transform.forward, 0.1f, true, 10f))
			{
				return;
			}
			this.clearCurrentPath();
		}
		this.steerFollow(owner, vector);
		if (this.steering.IsInRange(vector, 0.5f) || this.steering.IsInRange(chestPosition, this.FollowDistance))
		{
			this.setState(EntityDrone.State.Idle);
		}
	}

	public bool IsWeaponAttached
	{
		get
		{
			return this.stunWeapon != null;
		}
	}

	public EntityDrone.AttackMode AttackState
	{
		get
		{
			return this.attackMode;
		}
	}

	public bool CanAttack
	{
		get
		{
			return this.state != EntityDrone.State.Shutdown && this.state != EntityDrone.State.Heal && this.state != EntityDrone.State.Attack && this.WakeupAnimTime <= 0f;
		}
	}

	public void SetAttacKMode(EntityDrone.AttackMode mode)
	{
		this.attackMode = mode;
	}

	public List<DroneWeapons.Weapon> GetInstalledWeapons()
	{
		return this.installedWeapons;
	}

	public DroneWeapons.Weapon GetInstalledWeapon(string itemKey)
	{
		for (int i = 0; i < this.installedWeapons.Count; i++)
		{
			DroneWeapons.Weapon weapon = this.installedWeapons[i];
			if (weapon.ItemName.Equals(itemKey))
			{
				return weapon;
			}
		}
		return null;
	}

	public void SetActiveWeapon(DroneWeapons.Weapon _weapon)
	{
		this.activeWeapon = _weapon;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void attackState()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void exitAttackState()
	{
	}

	public bool IsHealModAttached
	{
		get
		{
			return this.healWeapon != null;
		}
	}

	public Vector3 GetHealArmPosition()
	{
		if (this.healWeapon != null)
		{
			return this.healWeapon.WeaponJoint.position + Origin.position;
		}
		return this.position + Origin.position;
	}

	public EntityDrone.AllyHealMode HealAllyMode
	{
		get
		{
			return this.allyHealMode;
		}
	}

	public bool TargetCanBeHealed(EntityAlive entity)
	{
		return this.healWeapon != null && this.healWeapon.targetCanBeHealed(entity) && this.healWeapon.hasHealingItem();
	}

	public bool IsTargetInNeedOfMedical(EntityAlive target)
	{
		return this.healWeapon != null && this.healWeapon.isTargetInNeedOfMedical(target);
	}

	public EntityAlive GetNearestHealTargetInRange(float range)
	{
		if (!this.IsHealingAllies || this.registeredPartyMembers == null)
		{
			return this.Owner;
		}
		List<EntityAlive> healingTargetsInRange = this.getHealingTargetsInRange(this.registeredPartyMembers, range);
		if (healingTargetsInRange.Count > 0)
		{
			if (healingTargetsInRange.Count > 1)
			{
				healingTargetsInRange.Sort((EntityAlive x, EntityAlive y) => x.Health.CompareTo(y.Health));
			}
			return healingTargetsInRange[0];
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setHealAllies(bool value)
	{
		this.IsHealingAllies = value;
		this.allyHealMode = (value ? EntityDrone.AllyHealMode.HealAllies : EntityDrone.AllyHealMode.DoNotHeal);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityAlive> getHealingTargetsInRange(List<int> playerIds, float range)
	{
		EntityDrone.healTargetsInRange.Clear();
		for (int i = 0; i < playerIds.Count; i++)
		{
			int num = playerIds[i];
			EntityAlive entityAlive = this.world.GetEntity(num) as EntityAlive;
			if (entityAlive)
			{
				float magnitude = (this.position - entityAlive.position).magnitude;
				bool flag = !RaycastPathUtils.IsPositionBlocked(this.position, entityAlive.getHeadPosition(), 65536, false);
				if (magnitude < range && this.healWeapon.targetNeedsHealing(entityAlive) && flag)
				{
					EntityDrone.healTargetsInRange.Add(entityAlive);
				}
			}
		}
		return EntityDrone.healTargetsInRange;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void healTargetServer(EntityAlive target, bool healReq = false)
	{
		if (this.state != EntityDrone.State.Heal && this.healWeapon.canFire() && (healReq || this.healWeapon.isTargetInNeedOfMedical(target)))
		{
			this.healTarget(target);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void healRequestClient()
	{
		this.SetState(EntityDrone.State.Heal, true);
		this.SetState(EntityDrone.State.Idle, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void healTarget(EntityAlive target)
	{
		base.SetAttackTarget(target, 1200);
		if (base.GetAttackTarget())
		{
			this.SetActiveWeapon(this.healWeapon);
			this.SetState(EntityDrone.State.Heal, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkNotifityNeedsHealItem()
	{
		if (!this.healWeapon.hasHealingItem())
		{
			GameManager.ShowTooltip(this.Owner as EntityPlayerLocal, Localization.Get("xuiDroneNeedsHealItemsStored", false, null), string.Empty, "ui_denied", null, false, false, 0f);
			this.PlaySound("drone_empty", 1f);
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateNeedsHealItemCheck()
	{
		if (this.needsHealItemTimer > 0f)
		{
			this.needsHealItemTimer -= 0.05f;
		}
		if (this.needsHealItemTimer <= 0f && this.needsHealNotifyCount < 2 && this.checkNotifityNeedsHealItem())
		{
			this.needsHealItemTimer = 30f;
			this.needsHealNotifyCount++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearNeedsHealItemCheck()
	{
		this.needsHealItemTimer = 0f;
		this.needsHealNotifyCount = 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void healState()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onHealDone()
	{
	}

	public bool IsOnTeleportCooldown()
	{
		return this.teleportAtkCooldownTimer > 0f;
	}

	public void TeleportIfFollowing()
	{
		if (this.orderState == EntityDrone.Orders.Follow && !this.isShutdown)
		{
			this.teleportState();
		}
	}

	public void TeleportOutOfRange()
	{
		if (this.state == EntityDrone.State.Attack)
		{
			this.exitAttackState();
		}
		if (this.state == EntityDrone.State.Heal)
		{
			this.onHealDone();
		}
		this.teleportState();
	}

	public void TeleportToPosition(Vector3 telePos)
	{
		this.teleportToPosition(telePos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void teleportToPosition(Vector3 telePos)
	{
		this.motion = Vector3.zero;
		this.SetPosition(telePos, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkTeleportPos(Vector3 target)
	{
		if (this.Owner)
		{
			if (this.isOutOfRange(this.Owner.position, 32f))
			{
				Log.Out("teleport failed");
				return;
			}
			Log.Out("teleport success!");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void teleportState()
	{
		this.teleportAtkCooldownTimer = 5f;
		this.setState(EntityDrone.State.Teleport);
		this.clearCurrentPath();
		this.motion = Vector3.zero;
		Vector3 chestPosition = this.Owner.getChestPosition();
		Vector3 lookVector = this.Owner.GetLookVector();
		Vector3 vector = chestPosition - new Vector3(lookVector.x, 0f, lookVector.z) * 5f;
		Vector3[] groupPositions = EntityDrone.GetGroupPositions(this.Owner, 5f, false, 0f);
		Array.Sort<Vector3>(groupPositions, (Vector3 x, Vector3 y) => Vector3.Distance(this.position, x).CompareTo(Vector3.Distance(this.position, y)));
		foreach (Vector3 vector2 in groupPositions)
		{
			RaycastHit raycastHit;
			bool flag = EntityDrone.IsPositionBlocked(chestPosition, vector2, out raycastHit, 1073807360, false, 0f);
			bool flag2 = EntityDrone.IsPositionBlocked(vector2, chestPosition, 1073807360, false, 0f);
			if (!flag && !flag2)
			{
				vector = vector2;
				break;
			}
			if (flag)
			{
				vector = World.worldToBlockPos(raycastHit.point + Origin.position).ToVector3Center();
				break;
			}
		}
		this.SetPosition(vector, true);
		this.ModelTransform.position = vector - Origin.position;
		this.setState(EntityDrone.State.Idle);
		this.checkTeleportPos(vector);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void performShutdown()
	{
		this.DebugDroneLog(EntityDrone.LoggingTypes.Shutdown, "performShutdown() {0}", new object[] { this });
		if (this.Owner)
		{
			Manager.Stop(this.Owner.entityId, "drone_take");
		}
		this.PlayVO("drone_shutdown", true, 1f);
		if (this.Owner && this.Owner.HasOwnedEntity(this.entityId))
		{
			this.Owner.GetOwnedEntity(this.entityId).SetLastKnownPosition(this.position);
		}
		this.setShutdown(true);
		this.isShutdownPending = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setShutdown(bool value)
	{
		this.DebugDroneLog(EntityDrone.LoggingTypes.Shutdown, "setShutdown({0}) {1}", new object[] { value, this });
		this.animator = base.GetComponentInChildren<Animator>();
		if (this.animator)
		{
			this.animator.enabled = !value;
		}
		this.PhysicsTransform.gameObject.SetActive(!value);
		this.IsNoCollisionMode.Value = value;
		this.setShutdownDestruction(value);
		this.isShutdown = value;
		if (value)
		{
			base.SetRevengeTarget(null);
			base.SetAttackTarget(null, 0);
			this.setShutdownAnim();
			this.setState(EntityDrone.State.Shutdown);
			Handle handle = this.idleLoop;
			if (handle != null)
			{
				handle.Stop(this.entityId);
			}
			this.idleLoop = null;
		}
		else
		{
			this.isGrounded = value;
			if (this.orderState == EntityDrone.Orders.Stay)
			{
				this.setState(EntityDrone.State.Sentry);
			}
			else
			{
				this.setState(EntityDrone.State.Idle);
			}
			if (this.Owner && this.Owner.HasOwnedEntity(this.entityId))
			{
				this.Owner.GetOwnedEntity(this.entityId).ClearLastKnownPostition();
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.SendSyncData(32768);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setShutdownDestruction(bool value)
	{
		if (value && this.Health > 1)
		{
			return;
		}
		Transform transform = base.transform.FindInChilds("p_smokeLeft", false);
		if (transform)
		{
			transform.gameObject.SetActive(value);
		}
		Transform transform2 = base.transform.FindInChilds("p_smokeRight", false);
		if (transform2)
		{
			transform2.gameObject.SetActive(value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void processShutdown()
	{
		if (this.isGrounded)
		{
			return;
		}
		this.fallBlockPos.RoundToInt(this.position - EntityDrone.blockHeightOffset);
		RaycastHit raycastHit;
		if ((!this.hasFallPoint || this.world.GetBlock(this.fallBlockPos).isair) && Physics.Raycast(this.position - Origin.position + EntityDrone.blockHeightOffset, Vector3.down, ref raycastHit, 999f, 268500992))
		{
			this.fallPoint = raycastHit.point;
			this.isGrounded = false;
			this.hasFallPoint = true;
		}
		if (this.isShutdown)
		{
			Vector3 position = this.position;
			float num = Vector3.Distance(this.position, this.fallPoint + Origin.position);
			if (num < 0.01f)
			{
				this.isGrounded = true;
				return;
			}
			position.y -= num * this.SpeedFlying * 0.05f;
			position.y = Mathf.Max(position.y, this.fallPoint.y);
			this.SetPosition(position, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateShutdownState()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (this.Owner)
			{
				if (this.Owner.Health <= 0 && this.state != EntityDrone.State.Shutdown && this.state != EntityDrone.State.Sentry)
				{
					this.performShutdown();
				}
				if (this.Health > 1 && this.Owner.Health > 1 && Vector3.Distance(this.position, this.Owner.position) < 10f && this.state == EntityDrone.State.Shutdown)
				{
					this.setShutdown(false);
				}
			}
			if (this.state == EntityDrone.State.Shutdown)
			{
				this.processShutdown();
			}
		}
	}

	public int AmmoCount
	{
		get
		{
			return this.OriginalItemValue.Meta;
		}
		set
		{
			this.OriginalItemValue.Meta = value;
		}
	}

	public bool IsVisible
	{
		get
		{
			return this.isVisible;
		}
		set
		{
			if (value != this.isVisible)
			{
				Renderer[] componentsInChildren = this.emodel.gameObject.GetComponentsInChildren<Renderer>(true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = value;
				}
				this.isVisible = value;
			}
		}
	}

	public void SetRenderersEnabled(bool value)
	{
		Renderer[] componentsInChildren = base.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setNoClip(bool value)
	{
		this.IsNoCollisionMode.Value = value;
		this.PhysicsTransform.gameObject.layer = (value ? 14 : 15);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOutOfRange(Vector3 _target, float _distance)
	{
		return (this.position - _target).sqrMagnitude > _distance * _distance;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsAnyPlayerWithingDist(float dist)
	{
		PersistentPlayerList persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
		if (((persistentPlayerList != null) ? persistentPlayerList.Players : null) != null)
		{
			foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in persistentPlayerList.Players)
			{
				EntityPlayer entityPlayer = this.world.GetEntity(keyValuePair.Value.EntityId) as EntityPlayer;
				if (entityPlayer && (entityPlayer.getChestPosition() - this.position).magnitude <= dist)
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float getTargetView(EntityAlive target, float degrees, float weight)
	{
		Vector3 lookVector = target.GetLookVector();
		Vector3 vector = this.position - target.position;
		float num = Vector3.Angle(lookVector, vector);
		if (num < degrees * 0.5f)
		{
			return (1f - num / degrees * 0.5f) * weight;
		}
		return 0f;
	}

	public void NotifyOffTheWorld()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnOriginChanged(Vector3 _origin)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDroneSystems()
	{
		if (this.initSuppressVOTimer > 0f)
		{
			this.initSuppressVOTimer -= 0.05f;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.Owner && !this.registeredOwnerHandlers)
		{
			EntityPlayer entityPlayer = this.Owner as EntityPlayer;
			if (entityPlayer)
			{
				entityPlayer.PlayerTeleportedDelegates += this.TeleportIfFollowing;
				this.registeredOwnerHandlers = true;
			}
		}
		if (this.registeredPartyMembers == null)
		{
			EntityPlayer entityPlayer2 = this.Owner as EntityPlayer;
			if (entityPlayer2)
			{
				Party party = entityPlayer2.Party;
				if (party != null)
				{
					this.registeredPartyMembers = new List<int>();
					int[] memberIdArray = party.GetMemberIdArray();
					for (int i = 0; i < memberIdArray.Length; i++)
					{
						this.registeredPartyMembers.Add(memberIdArray[i]);
					}
					party.PartyMemberAdded += this.onPartyMemberAdded;
					party.PartyMemberRemoved += this.onPartyMemberRemoved;
				}
			}
		}
		if (this.sensors != null)
		{
			this.sensors.Update();
			if (this.sensors.IsEnemyInRange())
			{
				this.FollowDistance = 10f;
			}
			else
			{
				this.FollowDistance = 5f;
			}
		}
		bool flag = this.state != EntityDrone.State.Shutdown && this.state != EntityDrone.State.Sentry && this.state != EntityDrone.State.Attack && this.state != EntityDrone.State.Heal;
		if (this.healWeapon != null && flag && this.healWeapon.targetCanBeHealed(this.Owner) && this.initSuppressVOTimer <= 0f)
		{
			this.updateNeedsHealItemCheck();
		}
		if (this.teleportAtkCooldownTimer >= 0f)
		{
			this.teleportAtkCooldownTimer -= 0.05f;
		}
		for (int j = 0; j < this.installedWeapons.Count; j++)
		{
			this.installedWeapons[j].Update();
		}
	}

	public bool IsInRange(Vector3 target, float range)
	{
		return this.steering.IsInRange(target, range);
	}

	public void Move(Vector3 targetPos, float pointRadius)
	{
		Vector3 vector = this.steering.Seek(this.position, targetPos, pointRadius);
		this.move(vector, this.SpeedFlying, false);
	}

	public void RotateTo(Vector3 dir)
	{
		this.rotateTo(dir);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canMove(Vector3 dir)
	{
		Vector3 vector = this.position + dir.normalized * this.physColHeight;
		return !RaycastPathUtils.IsPositionBlocked(this.position, vector, 1073807360, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void move(Vector3 dir, float speedFlying, bool ignoreObsticles = false)
	{
		Vector3 vector = this.position + dir.normalized * this.physColHeight;
		if (this.ownerIsOnVehicle || !EntityDrone.IsPositionBlocked(this.position, vector, 1073807360, true, 0f) || ignoreObsticles)
		{
			this.motion += dir * speedFlying * 0.05f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void move(Vector3 dir, Vector3 target, float speedFlying, bool ignoreObsticles = false)
	{
		Vector3 vector = this.position + dir.normalized * this.physColHeight;
		if (this.ownerIsOnVehicle || !EntityDrone.IsPositionBlocked(this.position, vector, 1073807360, true, 0f) || ignoreObsticles)
		{
			this.motion += Vector3.ClampMagnitude(dir * speedFlying * 0.05f, (target - this.position).magnitude);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void move(Vector3 dir, bool ignoreObsticles = false)
	{
		this.move(dir, this.currentSpeedFlying, ignoreObsticles);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void rotateTo(Vector3 dir)
	{
		if (dir != Vector3.zero)
		{
			this.rotation = this.rotateToDir(dir);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 rotateToDir(Vector3 dir)
	{
		return Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(dir), (1f - Vector3.Angle(base.transform.forward, dir) / 180f) * this.RotationSpeed * 0.05f).eulerAngles;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 rotateToEuler(Vector3 rot)
	{
		return Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(rot), (1f - Vector3.Angle(base.transform.forward, (rot - base.transform.eulerAngles).normalized) / 180f) * this.RotationSpeed * 0.05f).eulerAngles;
	}

	public static bool GetPath(List<Vector3> currentPath, EntityAlive entity, Vector3 start, Vector3 end, float speed, EAIBase aiTask = null, float seekDist = 0f, float pointRadius = 0.1f, bool debugDraw = false, float duration = 0f)
	{
		RaycastPathUtils.DrawBounds(start, Color.yellow, duration, 1f);
		RaycastPathUtils.DrawBounds(end, Color.green, duration, 1f);
		Vector3 projectedGroundPoint = EntityDrone.GetProjectedGroundPoint(start, debugDraw, duration);
		Vector3 projectedGroundPoint2 = EntityDrone.GetProjectedGroundPoint(end, debugDraw, duration);
		if (EntityDrone.GetProjectedPath(currentPath, entity, projectedGroundPoint, projectedGroundPoint2, speed, aiTask, debugDraw, duration))
		{
			Utils.DrawCircleLinesHorzontal(end - Origin.position, seekDist, Color.white, Color.green, 12, duration);
			for (int i = 1; i < currentPath.Count; i++)
			{
				Vector3 vector = currentPath[i];
				vector += EntityDrone.blockHeightOffset;
				vector.y += 1f;
				currentPath[i] = vector;
			}
			for (int j = 0; j < currentPath.Count - 1; j++)
			{
				if (EntityDrone.IsPositionBlocked(currentPath[j], currentPath[j + 1], 1073807360, false, 0f))
				{
					if (debugDraw)
					{
						Utils.DrawLine(currentPath[j] - Origin.position, currentPath[j + 1] - Origin.position, Color.white, Color.magenta, 2, duration);
						Utils.DrawCircleLinesHorzontal(currentPath[j] - Origin.position, pointRadius, Color.white, Color.green, 12, duration);
					}
					Vector3 vector2 = currentPath[j];
					vector2.y -= 1f;
					Vector3 vector3 = currentPath[j + 1];
					vector3.y -= 1f;
					if (!EntityDrone.IsPositionBlocked(currentPath[j], vector2, 1073807360, false, 0f) && !EntityDrone.IsPositionBlocked(vector2, vector3, 1073807360, false, 0f))
					{
						Utils.DrawLine(vector2 - Origin.position, vector3 - Origin.position, Color.white, Color.yellow, 2, duration);
						currentPath[j] = vector2;
						currentPath[j + 1] = vector3;
						if (j + 2 < currentPath.Count - 1 && !EntityDrone.IsPositionBlocked(vector3, currentPath[j + 2] - EntityDrone.blockHeightOffset, 1073807360, false, 0f))
						{
							currentPath[j + 2] = currentPath[j + 2] - EntityDrone.blockHeightOffset;
						}
						j++;
					}
					if (j + 2 >= currentPath.Count - 1 || EntityDrone.IsPositionBlocked(currentPath[j], currentPath[j + 2], 0, false, 0f))
					{
						currentPath.RemoveRange(j + 1, currentPath.Count - (j + 1));
						break;
					}
					Utils.DrawLine(currentPath[j] - Origin.position, currentPath[j + 2] - Origin.position, Color.white, Color.blue, 2, duration);
					currentPath.RemoveAt(j + 1);
				}
				else if (debugDraw)
				{
					Utils.DrawLine(currentPath[j] - Origin.position, currentPath[j + 1] - Origin.position, Color.white, Color.cyan, 2, duration);
					Utils.DrawCircleLinesHorzontal(currentPath[j] - Origin.position, pointRadius, Color.white, Color.cyan, 12, duration);
				}
			}
			currentPath.RemoveAt(0);
			return true;
		}
		return false;
	}

	public static bool GetProjectedPath(List<Vector3> projectedPath, EntityAlive entity, Vector3 start, Vector3 end, float speed, EAIBase aiTask = null, bool debugDraw = false, float duration = 0f)
	{
		projectedPath.Clear();
		int entityId = entity.entityId;
		PathFinderThread instance = PathFinderThread.Instance;
		PathInfo path = instance.GetPath(entityId);
		PathEntity pathEntity = ((path != null) ? path.path : null);
		if (pathEntity == null && !instance.IsCalculatingPath(entityId))
		{
			instance.FindPath(entity, start, end, speed, false, aiTask);
			return false;
		}
		if (pathEntity == null)
		{
			return false;
		}
		for (int i = 0; i < pathEntity.points.Length; i++)
		{
			Vector3 projectedLocation = pathEntity.points[i].projectedLocation;
			projectedPath.Add(projectedLocation);
		}
		if (debugDraw)
		{
			for (int j = 0; j < projectedPath.Count - 1; j++)
			{
				Utils.DrawLine(projectedPath[j] - Origin.position, projectedPath[j + 1] - Origin.position, Color.white, Color.cyan, 2, duration);
			}
		}
		return true;
	}

	public static Vector3 GetProjectedGroundPoint(Vector3 currentPos, bool debugDraw = false, float duration = 0f)
	{
		Vector3 vector = currentPos;
		RaycastHit raycastHit;
		if (Physics.Raycast(vector - Origin.position + EntityDrone.blockHeightOffset, Vector3.down, ref raycastHit, 100f, 1073807360))
		{
			vector = raycastHit.point + EntityDrone.blockHeightOffset + Origin.position;
			if (debugDraw)
			{
				RaycastPathUtils.DrawBounds(vector, Color.white, duration, 1f);
			}
		}
		else
		{
			vector -= EntityDrone.blockHeightOffset;
			vector.y -= 1f;
			if (debugDraw)
			{
				RaycastPathUtils.DrawBounds(vector, Color.white, duration, 1f);
			}
		}
		return vector;
	}

	public static Vector3[] GetGroupPositions(EntityAlive _entity, float followDist, bool debugDraw = false, float duration = 0f)
	{
		Vector3[] array = new Vector3[5];
		float num = 1f;
		Vector3 chestPosition = _entity.getChestPosition();
		Vector3 lookVector = _entity.GetLookVector();
		lookVector.y = 0f;
		array[0] = chestPosition - lookVector * followDist;
		Vector3 normalized = (_entity.transform.right - lookVector).normalized;
		normalized.y = 0f;
		array[1] = chestPosition + normalized * followDist * num;
		Vector3 normalized2 = (_entity.transform.right + lookVector).normalized;
		normalized2.y = 0f;
		array[2] = chestPosition - normalized2 * followDist * num;
		Vector3 normalized3 = (normalized - lookVector).normalized;
		normalized3.y = 0f;
		array[3] = chestPosition + normalized3 * followDist * num;
		Vector3 normalized4 = (normalized2 + lookVector).normalized;
		normalized4.y = 0f;
		array[4] = chestPosition - normalized4 * followDist * num;
		World world = GameManager.Instance.World;
		foreach (Vector3 vector in array)
		{
			bool flag = EntityDrone.IsPositionBlocked(chestPosition, vector, 1073807360, false, 0f);
			bool flag2 = EntityDrone.IsPositionBlocked(vector, chestPosition, 1073807360, false, 0f);
			Vector3i vector3i = World.worldToBlockPos(vector);
			if (!flag && !flag2)
			{
				if (RaycastPathWorldUtils.FindNodeType(RaycastPathWorldUtils.ScanVolume(world, vector3i.ToVector3Center(), false, false, false, 0f), cPathNodeType.Air) != null && debugDraw)
				{
					RaycastPathUtils.DrawNode(new RaycastNode(vector3i.ToVector3Center(), 1f, 0), Color.yellow, duration);
				}
			}
			else if (debugDraw)
			{
				RaycastPathUtils.DrawNode(new RaycastNode(vector3i.ToVector3Center(), 1f, 0), Color.red, duration);
			}
		}
		return array;
	}

	public static bool IsPositionBlocked(Vector3 start, Vector3 end, int layerMask = 0, bool debugDraw = false, float duration = 0f)
	{
		RaycastHit raycastHit;
		return EntityDrone.IsPositionBlocked(start, end, out raycastHit, layerMask, debugDraw, duration);
	}

	public static bool IsPositionBlocked(Vector3 start, Vector3 end, out RaycastHit hit, int layerMask = 0, bool debugDraw = false, float duration = 0f)
	{
		Vector3 vector = end - start;
		return EntityDrone.IsPositionBlocked(new Ray(start - Origin.position, vector), out hit, layerMask, vector.magnitude, debugDraw, duration);
	}

	public static bool IsPositionBlocked(Ray ray, out RaycastHit hit, int layerMask = 0, float maxDist = 100f, bool debugDraw = false, float duration = 0f)
	{
		bool flag = Physics.Raycast(ray, ref hit, maxDist, layerMask);
		if (debugDraw)
		{
			if (flag)
			{
				Utils.DrawLine(ray.origin, hit.point, Color.magenta, Color.magenta, 2, duration);
			}
			else
			{
				Utils.DrawLine(ray.origin, ray.origin + ray.direction * maxDist, Color.cyan, Color.cyan, 2, duration);
			}
		}
		return flag;
	}

	public static EntityDrone FindCollisionEntity(Transform t)
	{
		if (t)
		{
			EntityDrone component = t.GetComponent<EntityDrone>();
			if (component)
			{
				return component;
			}
		}
		return null;
	}

	public bool IgnoreCollisionEntity(Ray ray, float seeDist)
	{
		bool flag = false;
		int layer = base.gameObject.layer;
		GameObject gameObject = this.PhysicsTransform.gameObject;
		int layer2 = gameObject.layer;
		Utils.SetLayerRecursively(base.gameObject, 2);
		Utils.SetLayerRecursively(gameObject, 2);
		if (Voxel.Raycast(this.world, ray, seeDist, -1612492829, 64, 0f))
		{
			flag = true;
		}
		Utils.SetLayerRecursively(base.gameObject, layer);
		Utils.SetLayerRecursively(gameObject, layer2);
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i getBlockPosition(Vector3 worldPos)
	{
		Vector3i vector3i = new Vector3i(worldPos);
		Vector3 vector = worldPos - vector3i.ToVector3Center();
		return vector3i + Vector3i.FromVector3Rounded(vector);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 findOpenBlockAbove(Vector3 targetPosition, int maxHeight = 256)
	{
		Vector3i vector3i = this.getBlockPosition(targetPosition);
		vector3i += Vector3i.up;
		int num = 1;
		BlockValue blockValue = this.world.GetBlock(vector3i);
		while (!blockValue.isair && num < maxHeight)
		{
			num++;
			vector3i += Vector3i.up;
			blockValue = this.world.GetBlock(vector3i);
		}
		return vector3i.ToVector3Center();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isEntityAboveOrBelow(Entity entity)
	{
		bool flag = false;
		Vector3 chestPosition = entity.getChestPosition();
		float num = this.position.x - chestPosition.x;
		float num2 = this.position.z - chestPosition.z;
		float num3 = this.position.y - chestPosition.y;
		if (num > -0.85f && num < 0.85f && num2 > -0.85f && num2 < 0.85f && (num3 < -1.2f || num3 > 1.2f))
		{
			flag = true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTargetUnderWater(Vector3 targetPosition)
	{
		Vector3i blockPosition = this.getBlockPosition(targetPosition);
		return this.world.GetBlock(blockPosition).type == 240;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void followPlannedPath(float speed, float pointRadius = 0.1f, bool debugDraw = false, float duration = 0f)
	{
		if (this.currentPath.Count > 0)
		{
			this.currentPathTarget = this.currentPath[0];
			this.RotateTo((this.currentPathTarget - this.position).normalized);
			this.Move(this.currentPathTarget, pointRadius);
			if (this.IsInRange(this.currentPathTarget, pointRadius))
			{
				this.currentPath.RemoveAt(0);
				return;
			}
			if (debugDraw && this.currentPath.Count > 1)
			{
				RaycastPathUtils.DrawLine(this.currentPath[0], this.currentPath[1], Color.green, 1f);
			}
			if (this.pathTracker.IsStuck(this.position, this.currentPath[0], 0.5f, false))
			{
				if (this.currentPath.Count > 1)
				{
					this.TeleportToPosition(this.currentPath[1]);
					this.currentPath.RemoveRange(0, 2);
					return;
				}
				this.TeleportToPosition(this.currentPath[0]);
				this.currentPath.RemoveAt(0);
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearCurrentPath()
	{
		this.currentPath.Clear();
		this.OnPathInterupted();
	}

	public void OnPathInterupted()
	{
		this.moveHelper.Stop();
		this.navigator.clearPath();
		if (PathFinderThread.Instance != null)
		{
			PathFinderThread.Instance.RemovePathsFor(this.entityId);
		}
	}

	public override string MakeDebugNameInfo()
	{
		return string.Format("\nState: {0}", this.state.ToStringCached<EntityDrone.State>());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDebugName()
	{
		this.aiManager.UpdateDebugName();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void debugUpdate()
	{
		this.updateDebugName();
		if (this.debugCamera)
		{
			if (this.Owner && this.currentPath.Count == 0)
			{
				this.debugCamera.transform.LookAt(this.Owner.getHeadPosition() - Origin.position);
				return;
			}
			this.debugCamera.transform.forward = base.transform.forward;
		}
	}

	public void DebugTeleportUnstuck()
	{
		this.teleportState();
	}

	public void DebugTeleportTo(Vector3 pos)
	{
		this.clearCurrentPath();
		this.motion = Vector3.zero;
		this.SetPosition(pos, true);
	}

	public bool DebugFrendlyFireEnabled
	{
		get
		{
			return this.debugFriendlyFire;
		}
	}

	public void DebugToggleFriendlyFire()
	{
		this.debugFriendlyFire = !this.debugFriendlyFire;
	}

	public bool IsDebugCameraEnabled
	{
		get
		{
			return this.debugShowCamera;
		}
	}

	public void DebugToggleDebugCamera()
	{
		this._prepareDebugCamera();
		this.debugShowCamera = !this.debugShowCamera;
	}

	public void SetDebugCameraEnabled(bool value)
	{
		this._prepareDebugCamera();
		this.debugShowCamera = value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void _prepareDebugCamera()
	{
		if (this.debugShowCamera && this.debugCamera)
		{
			Object.Destroy(this.debugCamera);
			return;
		}
		this.debugCamera = new GameObject("Camera");
		this.debugCamera.transform.SetParent(base.transform);
		this.debugCamera.transform.localPosition = Vector3.zero;
		this.debugCamera.transform.localRotation = Quaternion.identity;
		Camera camera = this.debugCamera.AddComponent<Camera>();
		Rect rect = camera.rect;
		float num = 0.35f;
		rect.width = num;
		rect.height = num;
		float num2 = 1f - num;
		rect.x = num2;
		rect.y = num2;
		camera.rect = rect;
		camera.farClipPlane = 64f;
	}

	public void Debug_ToggleReconMode()
	{
		this._prepareReconCam();
		DroneManager.Debug_LocalControl = !DroneManager.Debug_LocalControl;
		EntityPlayerLocal entityPlayerLocal = this.Owner as EntityPlayerLocal;
		entityPlayerLocal.PlayerUI.windowManager.SetHUDEnabled(DroneManager.Debug_LocalControl ? GUIWindowManager.HudEnabledStates.FullHide : GUIWindowManager.HudEnabledStates.Enabled);
		entityPlayerLocal.bEntityAliveFlagsChanged = true;
		entityPlayerLocal.IsGodMode.Value = DroneManager.Debug_LocalControl;
		entityPlayerLocal.IsNoCollisionMode.Value = DroneManager.Debug_LocalControl;
		entityPlayerLocal.IsFlyMode.Value = DroneManager.Debug_LocalControl;
		if (entityPlayerLocal.IsGodMode.Value)
		{
			entityPlayerLocal.Buffs.AddBuff("god", -1, true, false, -1f);
		}
		else if (!GameManager.Instance.World.IsEditor() && !GameModeCreative.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode)))
		{
			entityPlayerLocal.Buffs.RemoveBuff("god", -1, true);
		}
		entityPlayerLocal.IsSpectator = DroneManager.Debug_LocalControl;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void _prepareReconCam()
	{
		if (DroneManager.Debug_LocalControl && this.reconCam)
		{
			Object.Destroy(this.reconCam.gameObject);
			return;
		}
		GameObject gameObject = new GameObject(this.Owner.EntityName + "-Drone|Recon");
		gameObject.transform.SetParent(base.transform);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		this.reconCam = gameObject.AddComponent<Camera>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool procEnemiesInRange()
	{
		if (this.DebugEnemiesInRange && this.sensors.IsEnemyInRange() && this.Owner)
		{
			EntityPlayerLocal entityPlayerLocal = this.Owner as EntityPlayerLocal;
			if (entityPlayerLocal)
			{
				this.rotateTo((this.Owner.position - this.position).normalized);
				Vector3 vector = entityPlayerLocal.vp_FPCamera.transform.position + Origin.position;
				if (entityPlayerLocal.bFirstPersonView)
				{
					vector = this.Owner.getHeadPosition() - this.Owner.transform.forward;
					vector.y = Mathf.Max(vector.y, this.Owner.getHeadPosition().y) + 0.5f;
				}
				else
				{
					vector.y = Mathf.Max(vector.y, this.Owner.getHeadPosition().y) + 1f;
				}
				float magnitude = (this.position - vector).magnitude;
				this.move(this.steering.Seek(this.position, vector, 1f), magnitude * 15f, true);
			}
			return true;
		}
		return false;
	}

	public ushort GetSyncFlagsReplicated(ushort syncFlags)
	{
		return syncFlags & 2;
	}

	public void SendSyncData(ushort syncFlags)
	{
		int primaryPlayerId = GameManager.Instance.World.GetPrimaryPlayerId();
		this.SendSyncData(syncFlags, primaryPlayerId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendSyncData(ushort syncFlags, int playerId)
	{
		EntityDrone.NetPackageDroneDataSync netPackageDroneDataSync = NetPackageManager.GetPackage<EntityDrone.NetPackageDroneDataSync>().Setup(this, playerId, syncFlags);
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(netPackageDroneDataSync, false);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(netPackageDroneDataSync, false, -1, -1, -1, null, 192, false);
	}

	public void WriteSyncData(BinaryWriter _bw, ushort syncFlags)
	{
		_bw.Write(3);
		if ((syncFlags & 1) > 0)
		{
			this.OwnerID.ToStream(_bw, false);
			_bw.Write(this.Health);
		}
		if ((syncFlags & 16384) > 0)
		{
			_bw.Write((byte)this.OrderState);
			if (this.OrderState == EntityDrone.Orders.Stay)
			{
				float[] array = new float[]
				{
					this.SentryPos.x,
					this.SentryPos.y,
					this.SentryPos.z
				};
				for (int i = 0; i < array.Length; i++)
				{
					_bw.Write(array[i]);
				}
			}
		}
		if ((syncFlags & 32768) > 0)
		{
			_bw.Write((byte)this.state);
			if (this.state == EntityDrone.State.Heal)
			{
				_bw.Write(this.userRequestedHeal);
				this.userRequestedHeal = false;
			}
		}
		if ((syncFlags & 2) > 0)
		{
			byte b = 0;
			if (this.isLocked)
			{
				b |= 2;
			}
			_bw.Write(b);
			this.ownerSteamId.ToStream(_bw, false);
			_bw.Write(this.passwordHash);
			_bw.Write((byte)this.allowedUsers.Count);
			for (int j = 0; j < this.allowedUsers.Count; j++)
			{
				this.allowedUsers[j].ToStream(_bw, false);
			}
		}
		if ((syncFlags & 8) > 0)
		{
			this.bag.Write(_bw);
		}
		if ((syncFlags & 32) > 0)
		{
			_bw.Write(this.isQuietMode);
		}
		if ((syncFlags & 64) > 0)
		{
			_bw.Write(this.IsFlashlightOn);
		}
		if ((syncFlags & 256) > 0)
		{
			_bw.Write(this.IsHealingAllies);
		}
		if ((syncFlags & 128) > 0)
		{
			this.OriginalItemValue.Write(_bw);
		}
	}

	public void ReadSyncData(BinaryReader _br, ushort syncFlags, int senderId)
	{
		byte b = _br.ReadByte();
		if ((syncFlags & 1) > 0)
		{
			this.OwnerID = PlatformUserIdentifierAbs.FromStream(_br, false, false);
			this.Health = _br.ReadInt32();
		}
		if ((syncFlags & 16384) > 0)
		{
			EntityDrone.Orders orders = (EntityDrone.Orders)_br.ReadByte();
			if (orders == EntityDrone.Orders.Stay)
			{
				this.SentryPos.x = _br.ReadSingle();
				this.SentryPos.y = _br.ReadSingle();
				this.SentryPos.z = _br.ReadSingle();
			}
			this.setOrders(orders);
			if (GameManager.IsDedicatedServer)
			{
				this.SendSyncData(16384, senderId);
			}
		}
		if ((syncFlags & 32768) > 0)
		{
			byte b2 = _br.ReadByte();
			this.transitionState = (EntityDrone.State)b2;
			if (b >= 1 && this.transitionState == EntityDrone.State.Heal)
			{
				this.userRequestedHeal = _br.ReadBoolean();
			}
			this.DebugDroneLog("{0} read transition {1} > {2}", new object[] { this, this.state, this.transitionState });
		}
		if ((syncFlags & 2) > 0)
		{
			byte b3 = _br.ReadByte();
			this.isLocked = (b3 & 2) > 0;
			this.ownerSteamId = PlatformUserIdentifierAbs.FromStream(_br, false, false);
			if (b > 1)
			{
				this.passwordHash = _br.ReadString();
			}
			else
			{
				this.passwordHash = _br.ReadInt32().ToString();
			}
			this.allowedUsers.Clear();
			int num = (int)_br.ReadByte();
			for (int i = 0; i < num; i++)
			{
				this.allowedUsers.Add(PlatformUserIdentifierAbs.FromStream(_br, true, false));
			}
		}
		if ((syncFlags & 8) > 0)
		{
			if (b >= 3)
			{
				this.bag = Bag.Read(_br);
			}
			else
			{
				int num2 = (int)_br.ReadByte();
				ItemStack[] array = new ItemStack[num2];
				for (int j = 0; j < num2; j++)
				{
					ItemStack itemStack = new ItemStack();
					array[j] = itemStack.Read(_br);
				}
				this.bag.SetSlots(array);
			}
		}
		if ((syncFlags & 16) > 0)
		{
			this.performRepair();
		}
		if ((syncFlags & 32) > 0)
		{
			this.isQuietMode = _br.ReadBoolean();
			if (this.isQuietMode)
			{
				Handle handle = this.idleLoop;
				if (handle != null)
				{
					handle.Stop(this.entityId);
				}
				this.idleLoop = null;
			}
		}
		if ((syncFlags & 64) > 0)
		{
			this.IsFlashlightOn = _br.ReadBoolean();
			this.setFlashlightOn(this.IsFlashlightOn);
		}
		if ((syncFlags & 256) > 0)
		{
			this.setHealAllies(_br.ReadBoolean());
		}
		if ((syncFlags & 128) > 0)
		{
			this.OriginalItemValue.Read(_br);
			this.LoadMods();
		}
	}

	public override bool IsSharedLock(ushort _channel)
	{
		return false;
	}

	public override void OnLockedServer(bool _success, int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
		base.OnLockedServer(_success, _lockingPlayerID, _context, _channel);
	}

	public override void OnUnlockedServer(int _unlockingPlayerId, ushort _channel)
	{
		base.OnUnlockedServer(_unlockingPlayerId, _channel);
	}

	public override void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		if (!_success)
		{
			GameManager.ShowTooltip(GameManager.Instance.World.GetPrimaryPlayer(), Localization.Get("ttVehicleInUse", false, null), string.Empty, "ui_denied", null, false, false, 0f);
			return;
		}
		Entity.EntityLockContext entityLockContext = _context as Entity.EntityLockContext;
		if (entityLockContext == null)
		{
			Log.Warning("[EntityDrone] Missing or invalid lock context.");
			LockManager.Instance.UnlockRequestLocal();
			return;
		}
		this.bag = entityLockContext.Bag.Clone();
		this.startInteraction(entityLockContext.Command);
	}

	public int EntityId
	{
		get
		{
			return this.entityId;
		}
		set
		{
			this.entityId = value;
		}
	}

	public bool IsLocked()
	{
		return this.isLocked;
	}

	public void SetLocked(bool _isLocked)
	{
		this.isLocked = _isLocked;
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return this.ownerSteamId;
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		this.ownerSteamId = _userIdentifier;
	}

	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		return (_userIdentifier != null && _userIdentifier.Equals(this.ownerSteamId)) || this.allowedUsers.Contains(_userIdentifier) || this.IsOwner(_userIdentifier);
	}

	public List<PlatformUserIdentifierAbs> GetUsers()
	{
		return new List<PlatformUserIdentifierAbs>();
	}

	public bool LocalPlayerIsOwner()
	{
		return this.IsOwner(PlatformManager.InternalLocalUserIdentifier);
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		if (this.ownerSteamId == null && this.OwnerID != null)
		{
			return this.OwnerID.Equals(_userIdentifier);
		}
		return this.ownerSteamId != null && this.ownerSteamId.Equals(_userIdentifier);
	}

	public bool HasPassword()
	{
		return !string.IsNullOrEmpty(this.passwordHash);
	}

	public string GetHashForPassword(string _password)
	{
		return Utils.HashString(_password);
	}

	public bool SetPasswordHash(string _passwordHash, PlatformUserIdentifierAbs _userIdentifier)
	{
		if (this.LocalPlayerIsOwner() && _passwordHash != null)
		{
			if (_passwordHash != this.passwordHash)
			{
				this.passwordHash = _passwordHash;
				this.allowedUsers.Clear();
				if (this.ownerSteamId == null)
				{
					this.SetOwner(_userIdentifier);
				}
			}
			return true;
		}
		return false;
	}

	public bool CheckPasswordHash(string _passwordHash, PlatformUserIdentifierAbs _userIdentifier)
	{
		if (this.LocalPlayerIsOwner() || !this.HasPassword())
		{
			this.SendSyncData(2);
			return true;
		}
		if (_passwordHash == this.passwordHash)
		{
			this.allowedUsers.Add(_userIdentifier);
			this.SendSyncData(2);
			return true;
		}
		return false;
	}

	public string GetPasswordHash()
	{
		return this.passwordHash;
	}

	public const string ClassName = "entityJunkDrone";

	public const string ItemName = "gunBotT3JunkDrone";

	public const int SaveVersion = 1;

	public const string cSupportModBuff = "buffJunkDroneSupportEffect";

	public static readonly FastTags<TagGroup.Global> StorageModifierTags = FastTags<TagGroup.Global>.Parse("droneStorage");

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cIdleAnimName = "Base Layer.Idle";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cSpawnAnimName = "Base Layer.SpawnIn";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static FastTags<TagGroup.Global> repairKitTags = FastTags<TagGroup.Global>.Parse("junk");

	public static bool DebugModeEnabled;

	public ItemValue OriginalItemValue;

	public PlatformUserIdentifierAbs OwnerID;

	public EntityAlive Owner;

	public const float cBaseFollowDistance = 5f;

	public const float cCombatFollowRange = 10f;

	public float FollowDistance = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cAvoidRange = 2.5f;

	public float FollowHoverHeight = 1f;

	public float StayHoverHeight = 2f;

	public float SpeedPathing = 2f;

	public float SpeedFlying = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cMaxSpeedFlying = 15f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cVehicleInventorySize = 45;

	public float RotationSpeed = 30f;

	public float AttackActionTime = 3f;

	public float HealActionTime = 7f;

	public DroneWeapons.HealBeamWeapon healWeapon;

	public DroneWeapons.StunBeamWeapon stunWeapon;

	public DroneWeapons.Weapon activeWeapon;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<DroneWeapons.Weapon> installedWeapons = new List<DroneWeapons.Weapon>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool weaponDischarged;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cAttackEnterTime = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cAttackExitTime = 1.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float attackEnterTimer = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float attackExitTimer = 1.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float accelerationTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float decelerationTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float armorDamageReduction = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float currentSpeedFlying = 3f;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityDrone.State state;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityDrone.State lastState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityDrone.State transitionState = EntityDrone.State.None;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float stateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float stateMaxTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 lastPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timeSpentAtLocation;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isVisible = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityAlive currentTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Vector3> currentPath = new List<Vector3>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityDrone.EntitySteering steering;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityDrone.DroneSensors sensors;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public FloodFillEntityPathGenerator pathMan;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cInitSuppressVOTime = 5f;

	public float initSuppressVOTimer = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform head;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Color prefabColor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public DroneLightManager _lm;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float creationTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool hasNavObjectsEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isOwnerSyncPending;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ItemValue itemvalueToLoad;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isBeingPickedUp;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BoxCollider interactionCollider;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string[] paintableParts = new string[] { "BaseMesh", "junkDroneArmRight", "armor" };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool debugFriendlyFire;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool debugShowCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject debugCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Camera reconCam;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isQuietMode;

	public bool IsFlashlightAttached;

	public bool IsFlashlightOn;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isSupportModAttached;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Handle voHandle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Handle idleLoop;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool partyEventsSet;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int[] knownPartyMembers;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float areaScanTime = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float areaScanTimer = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isInConfinedSpace;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float debugInputRotX;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float debugInputRotY;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 debugInputFwd;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 debugInputRgt;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 debugInputUp;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float debugInputSpeed = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3i debugOwnerPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float overItemLimitCooldown;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float retryPathTime = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isTryingToFindPath;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool userRequestedHeal;

	public bool isSystemSpawn;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static byte debugDroneInitLogPriority = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static byte debugDroneAnimationLogPriority = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static byte debugDroneShutdownLogPriority = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float physColHeight = 0.6f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public RaycastNode focusBoxNode;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cActivationStay = "drone_command_stay";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cActivationFollow = "drone_command_follow";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cActivationDontHealAllies = "drone_dont_heal_allies";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cActivationHealAllies = "drone_heal_allies";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cActivationLightOn = "drone_light_on";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cActivationLightOff = "drone_light_off";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cActivationSilentOn = "drone_silent_on";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cActivationSilentOff = "drone_silent_off";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cActivationHealMe = "drone_command_heal";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cActivationModPassive = "drone_attack_mode_passive";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cActivationModAgressive = "drone_attack_mode_aggressive";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Animator animator;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isAnimationStateSet;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cWakeUpTime = 2.5f;

	public float WakeupAnimTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool registeredOwnerHandlers;

	public List<int> registeredPartyMembers;

	public Vector3 SentryPos;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityDrone.Orders orderState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cOwnerFocusTime = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float ownerFocusTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool ownerIsOnVehicle;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityDrone.AttackMode attackMode;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cNotifyNeedsHealItemCooldown = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cNotifyNeedsHealMaxNotifyCount = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float needsHealItemTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int needsHealNotifyCount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool IsHealingAllies;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityDrone.AllyHealMode allyHealMode;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<EntityAlive> healTargetsInRange = new List<EntityAlive>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cTeleportAtkCooldownTime = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float teleportAtkCooldownTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isShutdown;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isGrounded;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 fallPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool hasFallPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3i fallBlockPos;

	public const int cPathLayer = 1073807360;

	public const float cFollowHoverHeight = 1f;

	public const float cAddPathDist = 1.414f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Vector3 blockHeightOffset = new Vector3(0f, 0.5f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityDrone.PathTracker pathTracker = new EntityDrone.PathTracker();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 currentPathTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 currentPathDest;

	public bool DebugEnemiesInRange;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncReplicate = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const byte cSyncVersion = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncOwnerKey = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncInteractAndSecurity = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncAction = 4;

	public const ushort cSyncStorage = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncOrderState = 16384;

	public const ushort cSyncState = 32768;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const byte cSyncInteractAndSecurityFLocked = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncRepairAction = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncQuietMode = 32;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncLightMod = 64;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncService = 128;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncHealAllies = 256;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isShutdownPending;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string passwordHash = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<PlatformUserIdentifierAbs> allowedUsers = new List<PlatformUserIdentifierAbs>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public PlatformUserIdentifierAbs ownerSteamId;

	public class SoundKeys
	{
		public const string cIdleHover = "drone_idle_hover";

		public const string cFly = "drone_fly";

		public const string cStorageOpen = "vehicle_storage_open";

		public const string cStorageClose = "vehicle_storage_close";

		public const string cHealEffect = "drone_healeffect";

		public const string cAttackEffect = "drone_attackeffect";

		public const string cStunCharged = "stunbaton_hit4";

		public const string cCommand = "drone_command";

		public const string cEmpty = "drone_empty";

		public const string cEnemySense = "drone_enemy_sense";

		public const string cEnemyEngauge = "drone_enemy_engauge";

		public const string cDroneOther = "drone_other";

		public const string cShutDown = "drone_shutdown";

		public const string cTake = "drone_take";

		public const string cTakeFail = "drone_takefail";

		public const string cWakeUp = "drone_wakeup";

		public const string cGreeting = "drone_greeting";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class ModKeys
	{
		public const string cStorageMod = "modRoboticDroneCargoMod";

		public const string cArmorMod = "modRoboticDroneArmorPlatingMod";

		public const string cHealMod = "modRoboticDroneMedicMod";

		public const string cStunMod = "modRoboticDroneStunWeaponMod";

		public const string cGunMod = "modRoboticDroneWeaponMod";

		public const string cMoraleMod = "modRoboticDroneMoraleBoosterMod";

		public const string cHeadlampMod = "modRoboticDroneHeadlampMod";

		public const string cHeadlampLightName = "junkDroneLamp";
	}

	public enum State
	{
		Idle,
		Sentry,
		Follow,
		Heal,
		Attack,
		Shutdown,
		NoClip,
		Teleport,
		None
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum LoggingTypes
	{
		Any,
		Init,
		Animation,
		Shutdown
	}

	public enum Orders
	{
		Follow,
		Stay
	}

	public enum AttackMode
	{
		Passive,
		Aggressive
	}

	public enum AllyHealMode
	{
		DoNotHeal,
		HealAllies
	}

	[Preserve]
	public class PathTracker
	{
		public bool IsStuck(Vector3 pos, Vector3 target, float time = 0.5f, bool debugDraw = false)
		{
			return this.IsStuckInBlock(pos, time, debugDraw) || this.IsNotAbleToReachTarget(target);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool IsStuckInBlock(Vector3 position, float time = 0.5f, bool debugDraw = false)
		{
			this.timeInBlock += Time.deltaTime;
			this.currentBlockPosition.FloorToInt(position);
			if (debugDraw)
			{
				RaycastPathUtils.DrawBounds(this.currentBlockPosition, Color.green, 0.05f, 1f);
			}
			if (this.currentBlockPosition != this.lastBlockPosition)
			{
				this.lastBlockPosition = this.currentBlockPosition;
				this.timeInBlock = 0f;
			}
			return this.timeInBlock > time;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool IsNotAbleToReachTarget(Vector3 currentTarget)
		{
			this.timeSpentToNextTarget += Time.deltaTime;
			if (this.targetDestination != currentTarget)
			{
				this.targetDestination = currentTarget;
				this.timeSpentToNextTarget = 0f;
			}
			if (this.timeSpentToNextTarget > 1f)
			{
				this.timeSpentToNextTarget = 0f;
				return true;
			}
			return false;
		}

		public Vector3i currentBlockPosition;

		public Vector3i lastBlockPosition;

		public float timeInBlock;

		[PublicizedFrom(EAccessModifier.Private)]
		public float timeSpentToNextTarget;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 targetDestination;
	}

	[Preserve]
	public class NetPackageDroneDataSync : NetPackage
	{
		public EntityDrone.NetPackageDroneDataSync Setup(EntityDrone _ev, int _senderId, ushort _syncFlags)
		{
			this.senderId = _senderId;
			this.vehicleId = _ev.entityId;
			this.syncFlags = _syncFlags;
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
			{
				pooledBinaryWriter.SetBaseStream(this.entityData);
				_ev.WriteSyncData(pooledBinaryWriter, _syncFlags);
			}
			return this;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ~NetPackageDroneDataSync()
		{
			MemoryPools.poolMemoryStream.FreeSync(this.entityData);
		}

		public override void read(PooledBinaryReader _br)
		{
			this.senderId = _br.ReadInt32();
			this.vehicleId = _br.ReadInt32();
			this.syncFlags = _br.ReadUInt16();
			int num = (int)_br.ReadUInt16();
			StreamUtils.StreamCopy(_br.BaseStream, this.entityData, num, null, true);
		}

		public override void write(PooledBinaryWriter _bw)
		{
			base.write(_bw);
			_bw.Write(this.senderId);
			_bw.Write(this.vehicleId);
			_bw.Write(this.syncFlags);
			_bw.Write((ushort)this.entityData.Length);
			this.entityData.WriteTo(_bw.BaseStream);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			if (_world == null)
			{
				return;
			}
			EntityDrone entityDrone = GameManager.Instance.World.GetEntity(this.vehicleId) as EntityDrone;
			if (entityDrone == null)
			{
				return;
			}
			if (this.entityData.Length > 0L)
			{
				PooledExpandableMemoryStream pooledExpandableMemoryStream = this.entityData;
				lock (pooledExpandableMemoryStream)
				{
					this.entityData.Position = 0L;
					try
					{
						using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
						{
							pooledBinaryReader.SetBaseStream(this.entityData);
							entityDrone.ReadSyncData(pooledBinaryReader, this.syncFlags, this.senderId);
						}
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
						string text = "Error syncing data for entity ";
						EntityDrone entityDrone2 = entityDrone;
						Log.Error(text + ((entityDrone2 != null) ? entityDrone2.ToString() : null) + "; Sender id = " + this.senderId.ToString());
					}
				}
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				ushort syncFlagsReplicated = entityDrone.GetSyncFlagsReplicated(this.syncFlags);
				if (syncFlagsReplicated != 0)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<EntityDrone.NetPackageDroneDataSync>().Setup(entityDrone, this.senderId, syncFlagsReplicated), false, -1, this.senderId, -1, null, 192, false);
				}
			}
		}

		public override int GetLength()
		{
			return (int)(12L + this.entityData.Length);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int senderId;

		[PublicizedFrom(EAccessModifier.Private)]
		public int vehicleId;

		[PublicizedFrom(EAccessModifier.Private)]
		public ushort syncFlags;

		[PublicizedFrom(EAccessModifier.Private)]
		public PooledExpandableMemoryStream entityData = MemoryPools.poolMemoryStream.AllocSync(true);
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class DroneInventory : Inventory
	{
		public DroneInventory(IGameManager _gameManager, EntityAlive _entity)
			: base(_gameManager, _entity)
		{
			this.SetupSlots();
		}

		public void SetupSlots()
		{
			int num = base.PUBLIC_SLOTS + 1;
			this.slots = new ItemInventoryData[num];
			this.models = new Transform[num];
			this.m_HoldingItemIdx = 0;
			base.Clear();
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class DroneSensors
	{
		public DroneSensors(EntityDrone _drone)
		{
			this.drone = _drone;
		}

		public void Init()
		{
		}

		public void Update()
		{
			if (this.enemyDetectedBarkTimer > 0f)
			{
				this.enemyDetectedBarkTimer -= 0.05f;
				if (this.enemyDetectedBarkTimer <= 0f)
				{
					this.canBarkEnemyDetected = true;
				}
			}
			if (this.IsEnemyInRange() && this.canBarkEnemyDetected)
			{
				this.barkEnemyDetected();
			}
		}

		public bool IsEnemyInRange()
		{
			return this.drone && this.GetNearestEnemyInRange(this.drone.position) != null;
		}

		public EntityAlive GetNearestEnemyInRange(Vector3 targetPos)
		{
			return this.GetNearestEnemyInRange(targetPos, this.EnemyDetectionRadius);
		}

		public EntityAlive GetNearestEnemyInRange(Vector3 targetPos, float weaponRange)
		{
			EntityAlive revengeTarget = this.drone.GetRevengeTarget();
			if (revengeTarget && !revengeTarget.Buffs.HasBuff("buffShocked"))
			{
				return revengeTarget;
			}
			EntityEnemy entityEnemy = null;
			float num = float.MaxValue;
			this.entitiesInRange.Clear();
			GameManager.Instance.World.GetEntitiesAround(EntityFlags.Player | EntityFlags.Zombie | EntityFlags.Animal | EntityFlags.Bandit, targetPos, this.EnemyDetectionRadius, this.entitiesInRange);
			for (int i = 0; i < this.entitiesInRange.Count; i++)
			{
				EntityEnemy entityEnemy2 = this.entitiesInRange[i] as EntityEnemy;
				if (entityEnemy2 && entityEnemy2.EntityClass != null && entityEnemy2.EntityClass.bIsEnemyEntity && this.canAttackTarget(entityEnemy2))
				{
					float sqrMagnitude = (targetPos - entityEnemy2.position).sqrMagnitude;
					if (sqrMagnitude < num && sqrMagnitude < weaponRange * weaponRange)
					{
						num = sqrMagnitude;
						entityEnemy = entityEnemy2;
					}
				}
			}
			return entityEnemy;
		}

		public bool IsOwnerAttackTarget()
		{
			if (this.drone && this.drone.Owner)
			{
				this.entitiesInRange.Clear();
				GameManager.Instance.World.GetEntitiesAround(EntityFlags.Player | EntityFlags.Zombie | EntityFlags.Animal | EntityFlags.Bandit, this.drone.position, this.EnemyDetectionRadius, this.entitiesInRange);
				for (int i = 0; i < this.entitiesInRange.Count; i++)
				{
					EntityEnemy entityEnemy = this.entitiesInRange[i] as EntityEnemy;
					if (entityEnemy && this.canAttackTarget(entityEnemy) && entityEnemy.GetAttackTarget() == this.drone.Owner)
					{
						return true;
					}
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool canAttackTarget(EntityAlive enemy)
		{
			return !enemy.IsDead() && (!enemy.IsSleeper || !enemy.IsSleeping) && !enemy.Buffs.HasBuff("buffShocked") && !EntityDrone.IsPositionBlocked(this.drone.position, enemy.getChestPosition(), 1073807360, false, 0f);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void barkEnemyDetected()
		{
			if (this.drone)
			{
				if (this.drone.Owner)
				{
					Manager.Stop(this.drone.Owner.entityId, "drone_take");
				}
				if (this.drone.state == EntityDrone.State.Shutdown)
				{
					return;
				}
				this.drone.PlayVO("drone_enemy_sense", true, 1f);
				this.enemyDetectedBarkTimer = this.EnemyDetectedBarkCooldown;
				this.canBarkEnemyDetected = false;
			}
		}

		public float EnemyDetectionRadius = 22f;

		public float EnemyDetectedBarkCooldown = 90f;

		[PublicizedFrom(EAccessModifier.Private)]
		public float enemyDetectedBarkTimer = 10f;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool canBarkEnemyDetected;

		[PublicizedFrom(EAccessModifier.Private)]
		public EntityDrone drone;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<Entity> entitiesInRange = new List<Entity>();
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class SteeringMan
	{
		public Vector3 Seek(Vector3 pos, Vector3 target, float slowingRadius)
		{
			return this.doSeek(pos, target, slowingRadius);
		}

		public Vector3 Seek2D(Vector3 pos, Vector3 target, float slowingRadius)
		{
			return this.doSeek2D(pos, target, slowingRadius);
		}

		public Vector3 Flee(Vector3 pos, Vector3 target, float avoidRadius)
		{
			return this.doFlee(pos, target, avoidRadius);
		}

		public Vector3 Flee2D(Vector3 pos, Vector3 target, float avoidRadius)
		{
			return this.doFlee2D(pos, target, avoidRadius);
		}

		public Vector3 GetDir(Vector3 from, Vector3 to)
		{
			return this.getDirVector(from, to);
		}

		public Vector3 GetDir2D(Vector3 from, Vector3 to)
		{
			Vector3 vector;
			vector..ctor(from.x, 0f, from.z);
			Vector3 vector2;
			vector2..ctor(to.x, 0f, to.z);
			return this.getDirVector(vector, vector2);
		}

		public bool IsInRange(Vector3 from, Vector3 to, float dist)
		{
			return this.isInRange(from, to, dist);
		}

		public bool IsInRange2D(Vector3 from, Vector3 to, float dist)
		{
			return this.isInRange2D(from, to, dist);
		}

		public Vector3 GetPointAround(Vector3 lhs, Vector3 rhs, float radius)
		{
			return this.getPointAround(lhs, rhs, radius);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 getVec(Vector3 pos, Vector3 target)
		{
			return target - pos;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 getDirVector(Vector3 pos, Vector3 target)
		{
			return this.getVec(pos, target).normalized;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float getDist(Vector3 pos, Vector3 target)
		{
			return this.getVec(pos, target).magnitude;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isInRange(Vector3 from, Vector3 to, float dist)
		{
			return (from - to).sqrMagnitude < dist * dist;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isInRange2D(Vector3 from, Vector3 to, float dist)
		{
			Vector3 vector;
			vector..ctor(from.x, 0f, from.z);
			Vector3 vector2;
			vector2..ctor(to.x, 0f, to.z);
			return this.isInRange(vector, vector2, dist);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 getPointAround(Vector3 lhs, Vector3 rhs, float radius)
		{
			return Vector3.Cross(lhs, rhs) * radius * 0.5f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doSeek(Vector3 pos, Vector3 target, float radius)
		{
			float dist = this.getDist(pos, target);
			if (dist < radius)
			{
				return this.getDirVector(pos, target) * (dist / radius);
			}
			return this.getDirVector(pos, target);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doSeek2D(Vector3 pos, Vector3 target, float radius)
		{
			Vector3 vector = this.doSeek(pos, target, radius);
			vector.y = 0f;
			return vector;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doFlee(Vector3 pos, Vector3 target, float radius)
		{
			return -this.doSeek(pos, target, radius);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doFlee2D(Vector3 pos, Vector3 target, float radius)
		{
			Vector3 vector = this.doFlee(pos, target, radius);
			vector.y = 0f;
			return vector;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public const int kMaxDistance = 1000;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class EntitySteering : EntityDrone.SteeringMan
	{
		public EntitySteering(EntityAlive _entity)
		{
			this.entity = _entity;
		}

		public Vector3 Hover(float height, float slowingRadius = 1f)
		{
			return this.doHover(this.entity.position, height, slowingRadius);
		}

		public Vector3 FollowPlayer(Vector3 playerPos, Vector3 playerLookDir, float followDist, float degrees = 90f, float maxDist = 15f)
		{
			return this.followTarget(this.entity.position, playerPos, playerLookDir, followDist, degrees, maxDist);
		}

		public Vector3 AvoidArc(Vector3 fromPos, Vector3 toPos, Vector3 dir, Vector3 up, bool subtract, float degrees, float maxDist = 1000f)
		{
			return this.doAvoidArc(fromPos, toPos, dir, up, subtract, degrees, maxDist);
		}

		public Vector3 AvoidArc2D(Vector3 fromPos, Vector3 toPos, Vector3 dir, bool subtract, float degrees, float maxDist = 1000f)
		{
			return this.doAvoidArc2D(fromPos, toPos, dir, subtract, degrees, maxDist);
		}

		public Vector3 AvoidTargetView(EntityAlive target, float followDist, bool subtract, float degrees = 90f, float maxDist = 15f)
		{
			return this.avoidTargetView(this.entity.position, target.getHeadPosition(), target.GetLookVector(), followDist, subtract, degrees, maxDist);
		}

		public Vector3 FollowTarget(EntityAlive target, Vector3 viewDir, float followDist, bool subtract, float degrees = 90f, float maxDist = 15f)
		{
			return this.pursueAvoidOwnerView(this.entity.position, target.getHeadPosition(), viewDir, Vector3.zero, followDist, subtract, degrees, maxDist);
		}

		public bool IsInRange(Vector3 target, float dist)
		{
			return base.IsInRange(this.entity.position, target, dist);
		}

		public bool IsInRange2D(Vector3 target, float dist)
		{
			return base.IsInRange2D(this.entity.position, target, dist);
		}

		public bool IsInRange2D(EntityAlive target, float dist)
		{
			return base.IsInRange2D(this.entity.position, target.position, dist);
		}

		public float GetYPos(float height)
		{
			return this.getYPos(this.entity.position, height);
		}

		public float GetAltitude(Vector3 pos)
		{
			return this.getAltitude(pos);
		}

		public bool IsAboveGround(Vector3 pos)
		{
			return this.getAltitude(pos) > -1f;
		}

		public float GetCeiling(Vector3 pos)
		{
			return this.getCeiling(pos);
		}

		public bool IsBelowCeiling(Vector3 pos)
		{
			return this.getCeiling(pos) > -1f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float getAltitude(Vector3 pos)
		{
			RaycastHit raycastHit;
			if (Physics.Raycast(pos - Origin.position, Vector3.down, ref raycastHit, 1000f, 65536))
			{
				return raycastHit.distance;
			}
			return -1f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float getCeiling(Vector3 pos)
		{
			RaycastHit raycastHit;
			if (Physics.Raycast(pos - Origin.position, Vector3.up, ref raycastHit, 1000f, 65536))
			{
				return raycastHit.distance;
			}
			return -1f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float getYPos(Vector3 pos, float height)
		{
			float altitude = this.getAltitude(pos);
			if (altitude >= 0f)
			{
				return pos.y - altitude + height;
			}
			return -1f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doHover(Vector3 pos, float height, float radius)
		{
			float altitude = this.getAltitude(pos);
			if (altitude <= 0f)
			{
				return Vector3.zero;
			}
			Vector3 vector = ((altitude < height) ? Vector3.up : Vector3.down);
			float num = Mathf.Abs(height - altitude);
			if (num < radius)
			{
				return vector * (num / radius);
			}
			return vector;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 followTarget(Vector3 pos, Vector3 target, Vector3 lookDir, float followDist, float degrees, float maxDist)
		{
			return base.Seek(pos, target, followDist).normalized;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 avoidTargetView(Vector3 pos, Vector3 target, Vector3 lookDir, float followDist, bool subtract, float degrees, float maxDist)
		{
			return this.AvoidArc2D(pos, target, lookDir, subtract, degrees, maxDist).normalized;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 pursueAvoidOwnerView(Vector3 pos, Vector3 target, Vector3 lookDir, Vector3 offSet, float followDist, bool subtract, float degrees, float maxDist)
		{
			Vector3 vector = base.Seek(pos, target, followDist);
			Vector3 vector2 = this.AvoidArc2D(pos, target, lookDir, subtract, degrees, maxDist);
			return (vector + vector2).normalized;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doAvoidArc(Vector3 from, Vector3 to, Vector3 dir, Vector3 up, bool subtract, float degrees, float maxDist)
		{
			Vector3 vector = from - to;
			if (Vector3.Angle(dir, vector) < degrees * 0.5f)
			{
				Vector3 vector2 = base.GetPointAround((to - from).normalized, up, maxDist);
				vector2 = (subtract ? (to - vector2) : (to + vector2));
				if (base.IsInRange(from, vector2, maxDist))
				{
					return base.Flee(from, vector2 + dir * vector.magnitude, 0f);
				}
			}
			return Vector3.zero;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doAvoidArc2D(Vector3 from, Vector3 to, Vector3 dir, bool subtract, float degrees, float maxDist)
		{
			Vector3 vector = this.doAvoidArc(from, to, dir, Vector3.up, subtract, degrees, maxDist);
			vector.y = 0f;
			return vector;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public EntityAlive entity;
	}
}
