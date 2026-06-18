using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntitySupplyCrate : EntityAlive
{
	public override bool IsValidAimAssistSnapTarget
	{
		get
		{
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.hasAI = false;
	}

	public override void PostInit()
	{
		base.PostInit();
		this.ValidateResources();
		base.gameObject.layer = 21;
		Collider component = base.GetComponent<Collider>();
		if (component)
		{
			component.enabled = false;
			component.enabled = true;
		}
		if (this.wasOnGround)
		{
			this.StopSmokeAndLights();
			if (this.parachuteT)
			{
				this.parachuteT.gameObject.SetActive(false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitLocalActivationCommands(Action<EntityActivationCommand> _addCallback)
	{
		_addCallback(new EntityActivationCommand("search", "search", null, null));
	}

	public override bool AllowActivationCommand(ReadOnlySpan<char> _commandName, EntityPlayerLocal _playerFocusing)
	{
		if (base.CommandIs(_commandName, "search"))
		{
			return this.bag != null && !this.IsDead();
		}
		return base.AllowActivationCommand(_commandName, _playerFocusing);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEntityActivated(EntityActivationCommand _command, EntityPlayerLocal _playerFocusing)
	{
		if (base.CommandIs(_command.commandId, "search"))
		{
			LockManager.Instance.LockRequestLocal(this, new Entity.EntityLockContext(_command.commandId.ToString(), this.bag), 0);
		}
	}

	public override string GetActivationText()
	{
		if (this.bag == null)
		{
			return string.Empty;
		}
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
		string text2 = Localization.Get(this.LocalizedEntityName, false, null);
		if (!this.bag.Touched)
		{
			return string.Format(Localization.Get("lootTooltipNew", false, null), text, text2);
		}
		if (this.bag.IsEmpty())
		{
			return string.Format(Localization.Get("lootTooltipEmpty", false, null), text, text2);
		}
		return string.Format(Localization.Get("lootTooltipTouched", false, null), text, text2);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleNavObject()
	{
		if (GameStats.GetBool(EnumGameStats.AirDropMarker))
		{
			NavObjectManager.Instance.UnRegisterNavObjectByEntityID(this.entityId);
			if (EntityClass.list[this.entityClass].NavObject != "")
			{
				this.NavObject = NavObjectManager.Instance.RegisterNavObject(EntityClass.list[this.entityClass].NavObject, this, "", false);
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				Vector3 vector = this.NavObject.GetPosition() + Origin.position;
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageNavObject>().Setup(this.NavObject.NavObjectClass.NavObjectClassName, this.NavObject.DisplayName, vector, true, this.NavObject.usingLocalizationId, this.entityId), false, -1, -1, -1, null, 192, false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		this.startRotY = this.rotation.y;
	}

	public override void OnEntityUnload()
	{
		base.OnEntityUnload();
		if (this.unloadReason == EnumRemoveEntityReason.Killed && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			GameManager.Instance.World.aiDirector.GetComponent<AIDirectorAirDropComponent>().RemoveSupplyCrate(this.entityId);
		}
	}

	public override EnumMapObjectType GetMapObjectType()
	{
		return EnumMapObjectType.SupplyDrop;
	}

	public override void SetMotionMultiplier(float _motionMultiplier)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void fallHitGround(float _v, Vector3 _fallMotion)
	{
		base.fallHitGround(Mathf.Min(_v, 5f), new Vector3(_fallMotion.x, Mathf.Max(-0.75f, _fallMotion.y), _fallMotion.z));
	}

	public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
	{
		base.MoveEntityHeaded(_direction, _isDirAbsolute);
		if (this.AttachedToEntity != null)
		{
			return;
		}
		if (((EModelSupplyCrate)this.emodel).parachute.gameObject.activeSelf && !base.IsInWater())
		{
			this.motion.y = this.motion.y + base.ScalePhysicsAddConstant(this.world.Gravity * 0.95f);
		}
	}

	public bool RequiresChunkObserver()
	{
		return !this.onGround || this.isSmokeOn;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ValidateResources()
	{
		if (!this.crateT)
		{
			this.crateT = base.transform.FindInChilds("SupplyCrateEntityPrefab", false);
		}
		if (!this.parachuteT)
		{
			this.parachuteT = base.transform.FindInChilds("parachute_supplies", false);
		}
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		if (this.showParachuteInTicks > 0)
		{
			this.showParachuteInTicks--;
		}
		if (this.closeParachuteInTicks > 0)
		{
			this.closeParachuteInTicks--;
		}
		if (!this.onGround && this.wasOnGround)
		{
			this.showParachuteInTicks = 10;
		}
		if (this.onGround && !this.wasOnGround)
		{
			this.closeParachuteInTicks = 10;
		}
		if ((this.onGround || base.IsInWater()) && this.closeParachuteInTicks <= 0)
		{
			((EModelSupplyCrate)this.emodel).parachute.gameObject.SetActive(false);
		}
		if (this.onGround && !this.wasOnGround)
		{
			float lightBrightness = this.world.GetLightBrightness(base.GetBlockPosition());
			GameManager.Instance.SpawnParticleEffectClient(new ParticleEffect("supply_crate_impact", base.GetPosition(), Quaternion.identity, lightBrightness, Color.white), this.entityId, false, false);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				AIDirectorAirDropComponent component = GameManager.Instance.World.aiDirector.GetComponent<AIDirectorAirDropComponent>();
				component.SetSupplyCratePosition(this.entityId, World.worldToBlockPos(this.position));
				component.RefreshCrates(-1);
			}
		}
		this.wasOnGround = this.onGround;
	}

	public override bool CanUpdateEntity()
	{
		return this.isEntityRemote || base.CanUpdateEntity();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		float time = Time.time;
		if (!GameManager.IsDedicatedServer)
		{
			if (this.wasOnGround && this.isSmokeOn)
			{
				if (this.smokeTimeOnGround == 0f)
				{
					this.smokeTimer = time;
				}
				this.smokeTimeOnGround = time - this.smokeTimer + 0.0001f;
				if (time > this.smokeTimer + this.smokeTimeAfterLanding)
				{
					this.StopSmokeAndLights();
				}
			}
			this.ValidateResources();
		}
		if (!this.onGround)
		{
			Vector3 vector;
			vector.x = Mathf.Sin(time) * 8f - 4f;
			vector.y = Mathf.Sin(time + 0.3f) * 8f - 4f + this.startRotY;
			vector.z = 0f;
			this.ModelTransform.localEulerAngles = vector;
			this.SetRotation(vector);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StopSmokeAndLights()
	{
		this.isSmokeOn = false;
		Transform modelTransform = this.emodel.GetModelTransform();
		List<Transform> list = new List<Transform>();
		GameUtils.FindTagInChilds(modelTransform, "SupplySmoke", list);
		for (int i = list.Count - 1; i >= 0; i--)
		{
			ParticleSystem[] componentsInChildren = list[i].GetComponentsInChildren<ParticleSystem>();
			for (int j = componentsInChildren.Length - 1; j >= 0; j--)
			{
				componentsInChildren[j].main.loop = false;
			}
		}
		list.Clear();
		GameUtils.FindTagInChilds(modelTransform, "SupplyLit", list);
		for (int k = 0; k < list.Count; k++)
		{
			list[k].gameObject.SetActive(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isRadiationSensitive()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool canDespawn()
	{
		return false;
	}

	public override bool IsSavedToFile()
	{
		return true;
	}

	public override bool CanCollideWithBlocks()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isGameMessageOnDeath()
	{
		return false;
	}

	public override void OnEntityDeath()
	{
		base.OnEntityDeath();
		GameManager.Instance.World.ObjectOnMapRemove(EnumMapObjectType.SupplyDrop, this.entityId);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.SupplyDrop, this.entityId), false, -1, -1, -1, null, 192, false);
			base.DropBagServer();
		}
	}

	public override bool CanBePushed()
	{
		return false;
	}

	public override bool CanCollideWith(Entity _other)
	{
		return false;
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		if (_version > 11)
		{
			this.wasOnGround = _br.ReadBoolean();
			this.closeParachuteInTicks = _br.ReadInt32();
			this.showParachuteInTicks = _br.ReadInt32();
		}
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		base.Write(_bw, _bNetworkWrite);
		_bw.Write(this.wasOnGround);
		_bw.Write(this.closeParachuteInTicks);
		_bw.Write(this.showParachuteInTicks);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float startRotY;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public new bool wasOnGround;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int showParachuteInTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int closeParachuteInTicks;

	public bool isSmokeOn = true;

	public float smokeTimeAfterLanding = 240f;

	public float smokeTimeOnGround;

	public float smokeTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform crateT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform parachuteT;
}
