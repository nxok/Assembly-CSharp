using System;
using System.IO;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityTurret : EntityAlive
{
	public override bool IsValidAimAssistSnapTarget
	{
		get
		{
			return false;
		}
	}

	public override bool IsValidAimAssistSlowdownTarget
	{
		get
		{
			return false;
		}
	}

	public override string LocalizedEntityName
	{
		get
		{
			return Localization.Get(this.EntityName, false, null);
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

	public bool IsTurning
	{
		get
		{
			return this.IsOn && (this.YawController.IsTurning || this.PitchController.IsTurning);
		}
	}

	public override int Health
	{
		get
		{
			return (int)Mathf.Max((float)this.OriginalItemValue.MaxUseTimes - this.OriginalItemValue.UseTimes, 1f);
		}
		set
		{
			this.OriginalItemValue.UseTimes = (float)(this.OriginalItemValue.MaxUseTimes - value);
		}
	}

	public override void Init(int _entityClass, EntityInstanceAssets _assets, EModelInstanceAssets _eModelAssets)
	{
		base.Init(_entityClass, _assets, _eModelAssets);
		EntityClass entityClass = EntityClass.list[this.entityClass];
		base.transform.tag = "E_Vehicle";
		Transform transform = base.transform;
		this.thisRigidBody = transform.GetComponent<Rigidbody>();
		if (this.thisRigidBody)
		{
			this.thisRigidBody.centerOfMass = new Vector3(0f, 0.1f, 0f);
			this.thisRigidBody.sleepThreshold = this.thisRigidBody.mass * 0.01f * 0.01f * 0.5f;
			transform.gameObject.AddComponent<CollisionCallForward>().Entity = this;
			transform.gameObject.layer = 21;
			Utils.SetTagsRecursively(transform, "E_Vehicle");
		}
		this.alertEnabled = false;
	}

	public override void Kill(DamageResponse _dmResponse)
	{
		_dmResponse.Fatal = false;
	}

	public override void SetDead()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ClientKill(DamageResponse _dmResponse)
	{
		_dmResponse.Fatal = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override DamageResponse damageEntityLocal(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale)
	{
		DamageResponse damageResponse = base.damageEntityLocal(_damageSource, _strength, _criticalHit, impulseScale);
		damageResponse.Fatal = false;
		return damageResponse;
	}

	public override void OnEntityUnload()
	{
		base.OnEntityUnload();
		this.IsOn = false;
		if (GameManager.Instance != null && GameManager.Instance.World != null && this.belongsPlayerId != -1)
		{
			EntityAlive entityAlive = (EntityAlive)GameManager.Instance.World.GetEntity(this.belongsPlayerId);
			if (entityAlive != null)
			{
				entityAlive.RemoveOwnedEntity(this.entityId);
			}
		}
		this.FireController.Update();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AddCharacterController()
	{
	}

	public override void PostInit()
	{
		Transform transform = base.transform;
		transform.rotation = this.qrotation;
		this.StaticPosition = this.position;
		this.fallPos = this.position;
		this.YawController = transform.GetComponentInChildren<AutoTurretYawLerp>();
		this.PitchController = transform.GetComponentInChildren<AutoTurretPitchLerp>();
		this.FireController = transform.GetComponentInChildren<MiniTurretFireController>();
		this.Laser = transform.FindInChilds("turret_laser", false);
		this.Cone = transform.FindInChilds("turret_cone", false);
		PersistentPlayerData playerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(this.OwnerID);
		if (playerData != null)
		{
			this.belongsPlayerId = playerData.EntityId;
		}
		this.HandleNavObject();
		this.InitTurret();
	}

	public override void InitInventory()
	{
		this.inventory = new EntityTurret.TurretInventory(GameManager.Instance, this);
	}

	public void InitTurret()
	{
		this.FireController.Init(base.EntityClass.Properties, this);
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
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
			if (this.Owner != null)
			{
				this.Owner.AddOwnedEntity(this);
			}
		}
		if (this.uloam == null && this.OriginalItemValue.ItemClass != null)
		{
			this.uloam = NGUITools.AddMissingComponent<UpdateLightOnAllMaterials>(base.gameObject);
			this.uloam.AddRendererNameToIgnore("turret_laser");
			this.uloam.SetTintColorForItem(Vector3.one);
			if (this.OriginalItemValue.ItemClass.Properties.Values.ContainsKey(Block.PropTintColor))
			{
				this.uloam.SetTintColorForItem(Block.StringToVector3(this.OriginalItemValue.GetPropertyOverride(Block.PropTintColor, this.OriginalItemValue.ItemClass.Properties.Values[Block.PropTintColor])));
			}
			else
			{
				this.uloam.SetTintColorForItem(Block.StringToVector3(this.OriginalItemValue.GetPropertyOverride(Block.PropTintColor, "255,255,255")));
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.IsOn = this.OriginalItemValue.PercentUsesLeft > 0f;
			if ((int)EffectManager.GetValue(PassiveEffects.MagazineSize, this.OriginalItemValue, 0f, null, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) > 0)
			{
				this.IsOn &= this.OriginalItemValue.Meta > 0;
			}
			if (GameManager.Instance != null && GameManager.Instance.World != null && this.belongsPlayerId != -1)
			{
				this.IsOn &= this.Owner != null;
				if (this.Owner != null)
				{
					if (EffectManager.GetValue(PassiveEffects.DisableItem, this.OriginalItemValue, 0f, this.Owner, null, this.OriginalItemValue.ItemClass.ItemTags, true, true, true, true, true, 1, true, false) > 0f)
					{
						this.IsOn = false;
					}
					else
					{
						this.maxOwnerDistance = (int)EffectManager.GetValue(PassiveEffects.JunkTurretActiveRange, this.OriginalItemValue, 10f, this.Owner, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
						if (this.IsOn)
						{
							this.DistanceToOwner = base.GetDistanceSq(this.Owner);
							this.IsOn &= this.DistanceToOwner < (float)(this.maxOwnerDistance * this.maxOwnerDistance);
						}
						if (this.IsOn)
						{
							int num = (int)EffectManager.GetValue(PassiveEffects.JunkTurretActiveCount, this.OriginalItemValue, 1f, this.Owner, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
							int num2 = 0;
							OwnedEntityData[] ownedEntities = this.Owner.GetOwnedEntities();
							for (int i = 0; i < ownedEntities.Length; i++)
							{
								EntityTurret entityTurret = GameManager.Instance.World.GetEntity(ownedEntities[i].Id) as EntityTurret;
								if (!(entityTurret == null) && entityTurret.entityId != this.entityId)
								{
									if (entityTurret.IsOn)
									{
										num2++;
									}
									this.IsOn &= num2 <= num || this.DistanceToOwner < entityTurret.DistanceToOwner || this.ForceOn;
									if (!this.IsOn)
									{
										break;
									}
								}
							}
						}
					}
				}
			}
			else if (this.IsOn)
			{
				this.IsOn &= this.belongsPlayerId == -1 && this.OwnerID == null;
			}
			this.ForceOn = false;
			if (this.TargetEntityId != this.lastTargetEntityId || this.IsOn != this.lastIsOn || this.OriginalItemValue.Equals(this.lastOriginalItemValue))
			{
				this.lastOriginalItemValue = this.OriginalItemValue.Clone();
				this.lastTargetEntityId = this.TargetEntityId;
				this.lastIsOn = this.IsOn;
				NetPackageTurretSync netPackageTurretSync = NetPackageManager.GetPackage<NetPackageTurretSync>().Setup(this.entityId, this.TargetEntityId, this.IsOn, this.OriginalItemValue);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(netPackageTurretSync, true, -1, -1, -1, null, 192, false);
			}
		}
		if (this.Laser != null && this.IsOn != this.Laser.gameObject.activeSelf)
		{
			this.Laser.gameObject.SetActive(this.IsOn);
		}
	}

	public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
	{
	}

	public void InitDynamicSpawn()
	{
		for (int i = 1; i < ItemClass.list.Length - 1; i++)
		{
			if (ItemClass.list[i] != null)
			{
				string name = ItemClass.list[i].Name;
				if (name == "gunBotT1JunkSledge" || name == "gunBotT2JunkTurret")
				{
					this.OwnerID = PlatformManager.InternalLocalUserIdentifier;
					this.OriginalItemValue = new ItemValue(ItemClass.list[i].Id, false);
					this.AmmoCount = ItemClass.GetForId(ItemClass.list[i].Id).GetInitialMetadata(this.OriginalItemValue);
					this.ForceOn = true;
					PersistentPlayerData playerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(this.OwnerID);
					if (playerData != null)
					{
						(GameManager.Instance.World.GetEntity(playerData.EntityId) as EntityAlive).AddOwnedEntity(this);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTransform()
	{
		Vector3 vector = base.transform.position + Origin.position;
		this.position = vector;
		this.StaticPosition = vector;
		Chunk chunk = GameManager.Instance.World.GetChunkFromWorldPos((int)vector.x, (int)vector.y, (int)vector.z) as Chunk;
		if (chunk != null && chunk.IsCollisionMeshGenerated)
		{
			if (this.posCheckTimer <= 0f)
			{
				this.posCheckTimer = 0.5f;
				int modelLayer = base.GetModelLayer();
				this.SetModelLayer(2, false, null);
				float y = this.fallPos.y;
				this.fallPos = vector;
				Ray ray;
				ray..ctor(vector + Vector3.up * 0.375f, Vector3.down);
				if (Voxel.Raycast(GameManager.Instance.World, ray, 255f, 1082195969, 128, 0.25f))
				{
					this.groundUpDirection = Voxel.phyxRaycastHit.normal;
					this.fallPos.y = Voxel.voxelRayHitInfo.fmcHit.pos.y;
					if (Vector3.Dot(Vector3.up, this.groundUpDirection) < 0.7f)
					{
						this.fallPos.y = this.fallPos.y - 0.1f;
					}
					if (this.fallPos.y < y)
					{
						this.fallDelay = 5;
					}
				}
				this.SetModelLayer(modelLayer, false, null);
			}
			float deltaTime = Time.deltaTime;
			this.posCheckTimer -= deltaTime;
			this.isFalling = false;
			if (vector != this.fallPos)
			{
				this.posCheckTimer = 0f;
				int num = this.fallDelay - 1;
				this.fallDelay = num;
				if (num < 0)
				{
					this.isFalling = true;
					base.transform.position = Vector3.MoveTowards(base.transform.position, this.fallPos - Origin.position, 5f * deltaTime);
					return;
				}
			}
		}
		else
		{
			this.posCheckTimer = 0.5f;
		}
	}

	public override void OnCollectServer(int _playerId)
	{
		this.OriginalItemValue = ItemValue.None;
		this.PickedUpWaitingToDelete = true;
		this.bPlayerStatsChanged = true;
		base.transform.gameObject.SetActive(false);
		this.world.RemoveEntity(this.entityId, EnumRemoveEntityReason.Killed);
	}

	public override void OnCollectLocal(int _playerId)
	{
		EntityPlayerLocal entityPlayerLocal = this.world.GetEntity(_playerId) as EntityPlayerLocal;
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		ItemStack itemStack = new ItemStack(this.OriginalItemValue, 1);
		if (!uiforPlayer.xui.PlayerInventory.AddItem(itemStack))
		{
			GameManager.Instance.ItemDropServer(itemStack, entityPlayerLocal.GetPosition(), Vector3.zero, _playerId, 60f, false);
		}
	}

	public bool CanInteract(int _interactingEntityId)
	{
		return !this.isFalling && !this.PickedUpWaitingToDelete && this.OriginalItemValue.type != 0 && (this.belongsPlayerId == _interactingEntityId || this.Health <= 1);
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
		PlayerActionsLocal playerInput = entityPlayerLocal2.playerInput;
		string text = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		return string.Format(Localization.Get("turretPickUp", false, null), text, this.LocalizedEntityName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitLocalActivationCommands(Action<EntityActivationCommand> _addCallback)
	{
		_addCallback(new EntityActivationCommand("take", "hand", null, null));
	}

	public override bool AllowActivationCommand(ReadOnlySpan<char> _commandName, EntityPlayerLocal _playerFocusing)
	{
		if (base.CommandIs(_commandName, "take"))
		{
			return this.CanInteract(_playerFocusing.entityId);
		}
		return base.AllowActivationCommand(_commandName, _playerFocusing);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEntityActivated(EntityActivationCommand _command, EntityPlayerLocal _playerFocusing)
	{
		if (base.CommandIs(_command.commandId, "take"))
		{
			ItemStack itemStack = new ItemStack(this.OriginalItemValue, 1);
			if (_playerFocusing.inventory.CanTakeItem(itemStack) || _playerFocusing.bag.CanTakeItem(itemStack))
			{
				base.Collect(_playerFocusing.entityId);
				return;
			}
			GameManager.ShowTooltip(_playerFocusing, Localization.Get("xuiInventoryFullForPickup", false, null), string.Empty, "ui_denied", null, false, false, 0f);
		}
	}

	public override bool IsDead()
	{
		return false;
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		base.Write(_bw, _bNetworkWrite);
		_bw.Write(1);
		this.OwnerID.ToStream(_bw, false);
		this.OriginalItemValue.Write(_bw);
		StreamUtils.Write(_bw, this.StaticPosition);
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		int num = _br.ReadInt32();
		this.OwnerID = PlatformUserIdentifierAbs.FromStream(_br, false, false);
		this.OriginalItemValue = ItemValue.None;
		this.OriginalItemValue.Read(_br);
		if (num > 0)
		{
			this.StaticPosition = StreamUtils.ReadVector3(_br);
		}
	}

	public const int SaveVersion = 1;

	public const string JunkTurretSledgeItem = "gunBotT1JunkSledge";

	public const string JunkTurretRangedItem = "gunBotT2JunkTurret";

	public AutoTurretYawLerp YawController;

	public AutoTurretPitchLerp PitchController;

	public MiniTurretFireController FireController;

	public Transform Laser;

	public Transform Cone;

	public Material ConeMaterial;

	public Color ConeColor;

	public float CenteredYaw;

	public float CenteredPitch;

	public bool TargetOwner;

	public bool TargetAllies;

	public bool TargetStrangers = true;

	public bool TargetEnemies = true;

	public int maxOwnerDistance = 10;

	public ItemValue OriginalItemValue = ItemValue.None;

	public bool PickedUpWaitingToDelete;

	public PlatformUserIdentifierAbs OwnerID;

	public bool IsOn;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Rigidbody thisRigidBody;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public UpdateLightOnAllMaterials uloam;

	public EntityAlive Owner;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int lastTargetEntityId = -2;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool lastIsOn;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ItemValue lastOriginalItemValue = ItemValue.None;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cPOSITION_UPDATE_CHECK_TIME = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float posCheckTimer = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int fallDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 fallPos;

	public int TargetEntityId = -1;

	public bool ForceOn;

	public float DistanceToOwner = float.MaxValue;

	public Vector3 groundPosition;

	public Vector3 groundUpDirection;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isFalling;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 StaticPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cLerpTimeScale = 8f;

	public int tmpBelongsPlayerID;

	public class TurretInventory : Inventory
	{
		public TurretInventory(IGameManager _gameManager, EntityAlive _entity)
			: base(_gameManager, _entity)
		{
			this.cSlotCount = base.PUBLIC_SLOTS + 1;
			this.SetupSlots();
		}

		public override void Execute(int _actionIdx, bool _bReleased, PlayerActionsLocal _playerActions = null)
		{
		}

		public void SetupSlots()
		{
			this.slots = new ItemInventoryData[this.cSlotCount];
			this.models = new Transform[this.cSlotCount];
			this.m_HoldingItemIdx = 0;
			base.Clear();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void updateHoldingItem()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int cSlotCount;
	}
}
