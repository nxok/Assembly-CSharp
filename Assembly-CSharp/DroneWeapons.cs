using System;
using System.Collections.Generic;
using System.Globalization;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class DroneWeapons
{
	public const string cSHOCK_BUFF_NAME = "buffShocked";

	public const string cShockBuffDmgReq = "_droneStunDamage";

	public const string cBuffHealCooldown = "buffJunkDroneHealCooldownEffect";

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> healingItemTags = FastTags<TagGroup.Global>.Parse("medical");

	public const string cHealWeaponJoint = "WristLeft";

	[Preserve]
	public class Weapon
	{
		public Weapon(EntityAlive _entity)
		{
			this.entity = _entity;
			this.belongsPlayerId = this.entity.belongsPlayerId;
			this.entityProperties = _entity.EntityClass.Properties;
		}

		public virtual void Init()
		{
		}

		public virtual void Update()
		{
			if (this.cooldownTimer > 0f)
			{
				this.cooldownTimer -= 0.05f;
				if (this.cooldownTimer <= 0f)
				{
					this.OnReadyToFire();
				}
				if (this.target && this.target.IsDead())
				{
					this.InvokeFireComplete();
				}
			}
		}

		public virtual void SetModProps(ItemValue itemValue)
		{
			this.modItem = itemValue;
			DynamicProperties properties = this.modItem.ItemClass.Properties;
			properties.ParseString("SoundWeaponCharged", ref this.soundWeaponCharged);
			properties.ParseString("SoundWeaponUseBark", ref this.soundWeaponUseBark);
			float value = EffectManager.GetValue(PassiveEffects.MaxRange, itemValue, 0f, null, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			if (value != 0f)
			{
				this.range = value;
			}
			float value2 = EffectManager.GetValue(PassiveEffects.JunkDroneModCooldown, itemValue, 0f, null, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			if (value2 != 0f)
			{
				this.cooldown = value2;
			}
		}

		public string ItemName
		{
			get
			{
				if (this.modItem == null)
				{
					return string.Empty;
				}
				return this.modItem.ItemClass.Name;
			}
		}

		public float TimeRemaning
		{
			get
			{
				return this.cooldownTimer;
			}
		}

		public float TimeLength
		{
			get
			{
				return this.actionTime + this.cooldown;
			}
		}

		public float Range
		{
			get
			{
				return this.range;
			}
		}

		public float Cooldown
		{
			get
			{
				return this.cooldown;
			}
		}

		public virtual bool canFire()
		{
			return this.cooldownTimer <= 0f;
		}

		public virtual void Fire(EntityAlive _target)
		{
			this.target = _target;
			this.RefreshCooldown();
		}

		public void RefreshCooldown()
		{
			this.cooldownTimer = this.actionTime + this.cooldown;
		}

		public virtual bool hasActionCompleted()
		{
			return this.cooldownTimer < this.cooldown;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void OnReadyToFire()
		{
			if (!string.IsNullOrEmpty(this.soundWeaponCharged))
			{
				this.PlaySound(this.entity, this.soundWeaponCharged);
			}
		}

		public void RegisterOnFireComplete(Action _onFireComplete)
		{
			this.onFireComplete = _onFireComplete;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void OnFireComplete()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void InvokeFireComplete()
		{
			this.OnFireComplete();
			Action action = this.onFireComplete;
			if (action != null)
			{
				action();
			}
			this.onFireComplete = null;
			this.target = null;
		}

		public virtual void Equip(ItemValue itemValue)
		{
			this.entity.MinEventContext.ItemValue = itemValue;
			itemValue.FireEvent(MinEventTypes.onSelfEquipStart, this.entity.MinEventContext);
			this.SetModProps(itemValue);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.entity.aiManager.AddItemTasks(itemValue.ItemClass);
			}
		}

		public void Unequip()
		{
			this.modItem.FireEvent(MinEventTypes.onSelfEquipStop, this.entity.MinEventContext);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.entity.aiManager.RemoveItemTasks(this.modItem.ItemClass.Name);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void PlaySound(Entity _entity, string soundGroupKey)
		{
			Manager.Play(_entity, soundGroupKey, 1f, false);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void PlaySound(string soundGroupKey)
		{
			this.PlaySound(this.entity, soundGroupKey);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void TargetApplyBuff(string _buff, bool _fromElectrical)
		{
			this.target.Buffs.AddBuff(_buff, World.worldToBlockPos(this.entity.position), (this.belongsPlayerId != -1) ? this.belongsPlayerId : this.entity.entityId, true, _fromElectrical, -1f);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void SpawnParticleEffect(ParticleEffect _pe, int _entityId)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (!GameManager.IsDedicatedServer)
				{
					GameManager.Instance.SpawnParticleEffectClient(_pe, _entityId, false, false);
				}
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(_pe, _entityId, false, false), false, -1, _entityId, -1, null, 192, false);
				return;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(_pe, _entityId, false, false), false);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public Transform SpawnDroneParticleEffect(ParticleEffect _pe, int _entityId, DroneWeapons.NetPackageDroneParticleEffect.cActionType actionType)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<DroneWeapons.NetPackageDroneParticleEffect>().Setup(_pe, _entityId, actionType), false, -1, -1, -1, null, 192, false);
				if (!GameManager.IsDedicatedServer)
				{
					return GameManager.Instance.SpawnParticleEffectClientForceCreation(_pe, _entityId, false);
				}
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<DroneWeapons.NetPackageDroneParticleEffect>().Setup(_pe, _entityId, actionType), false);
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public EntityAlive entity;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int belongsPlayerId;

		[PublicizedFrom(EAccessModifier.Protected)]
		public DynamicProperties entityProperties;

		public Transform WeaponJoint;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float actionTime;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float cooldown = 1f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float range = 10f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public EntityAlive target;

		[PublicizedFrom(EAccessModifier.Private)]
		public float cooldownTimer;

		[PublicizedFrom(EAccessModifier.Protected)]
		public ItemValue modItem;

		public string soundWeaponCharged;

		public string soundWeaponUseBark;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action onFireComplete;

		public const string cSoundWeaponCharged = "SoundWeaponCharged";

		public const string cSoundWeaponUseBark = "SoundWeaponUseBark";
	}

	[Preserve]
	public class HealBeamWeapon : DroneWeapons.Weapon
	{
		public HealBeamWeapon(EntityAlive _entity)
			: base(_entity)
		{
		}

		public override void Init()
		{
			this.WeaponJoint = this.entity.transform.FindInChilds("WristLeft", false);
		}

		public override void SetModProps(ItemValue itemValue)
		{
			DynamicProperties properties = itemValue.ItemClass.Properties;
			properties.ParseFloat("HealActionTime", ref this.actionTime);
			properties.ParseFloat("HealDamageThreshold", ref this.HealDamageThreshold);
			base.SetModProps(itemValue);
		}

		public override void Equip(ItemValue itemValue)
		{
			base.Equip(itemValue);
			EntityDrone entityDrone = this.entity as EntityDrone;
			EntityAlive entityAlive = GameManager.Instance.World.GetEntity(entityDrone.belongsPlayerId) as EntityAlive;
			if (entityAlive && entityDrone && entityDrone.TimeSinceCreation != 0f)
			{
				entityAlive.Buffs.AddBuff("buffJunkDroneHealCooldownEffect", -1, true, false, -1f);
			}
		}

		public override void Fire(EntityAlive _target)
		{
			base.Fire(_target);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				DroneWeapons.HealBeamWeapon.HealItemType healItemType = this.findNeededHealType(_target);
				if (healItemType == DroneWeapons.HealBeamWeapon.HealItemType.None)
				{
					base.InvokeFireComplete();
					Log.Out("HealBeamWeapon: failed to determine heal type");
					return;
				}
				ItemStack healingItemStack = this.getHealingItemStack(healItemType);
				if (healingItemStack == null)
				{
					base.InvokeFireComplete();
					return;
				}
				this.entity.inventory.SetItem(0, healingItemStack);
				this.entity.inventory.SetHoldingItemIdx(0);
				this.entity.inventory.ForceHoldingItemUpdate();
				ItemAction itemAction = this.entity.inventory.holdingItem.Actions[1];
				ItemActionData itemActionData = this.entity.inventory.holdingItemData.actionData[1];
				ItemActionUseOther.FeedInventoryData feedInventoryData = itemActionData as ItemActionUseOther.FeedInventoryData;
				EntityDrone entityDrone = this.entity as EntityDrone;
				EntityAlive attackTarget = entityDrone.GetAttackTarget();
				if (feedInventoryData != null)
				{
					feedInventoryData.TargetEntity = attackTarget;
					itemActionData = feedInventoryData;
				}
				if (itemAction != null && itemAction.CanExecute(itemActionData))
				{
					itemAction.ExecuteAction(itemActionData, false);
					itemAction.ExecuteAction(itemActionData, true);
				}
				EntityAlive owner = entityDrone.Owner;
				if (owner)
				{
					owner.Buffs.AddBuff("buffJunkDroneHealCooldownEffect", -1, true, false, -1f);
				}
				ParticleEffect particleEffect = new ParticleEffect("drone_heal_beam", Vector3.zero, Quaternion.LookRotation(_target.getHeadPosition() - this.entity.position), 1f, Color.clear, null, this.entity.transform, 1f, "");
				Transform transform = base.SpawnDroneParticleEffect(particleEffect, this.entity.entityId, DroneWeapons.NetPackageDroneParticleEffect.cActionType.Heal);
				if (transform && !GameManager.IsDedicatedServer)
				{
					transform.GetComponent<DroneBeamParticle>().SetDisplayTime(this.actionTime);
				}
				ParticleEffect particleEffect2 = new ParticleEffect("drone_heal_player", Vector3.zero, Quaternion.identity, 1f, Color.clear, null, _target.transform, 1f, "");
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(particleEffect2, _target.entityId, false, false), false, -1, -1, -1, null, 192, false);
					if (!GameManager.IsDedicatedServer)
					{
						GameManager.Instance.SpawnParticleEffectClient(particleEffect2, _target.entityId, false, false);
					}
				}
			}
		}

		public override bool canFire()
		{
			return base.canFire() && this.hasHealingItem();
		}

		public bool hasHealingItem()
		{
			return this.hasSupportedHealingItem();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasSupportedHealingItem()
		{
			for (int i = 0; i < this.supportedItems.Length; i++)
			{
				if (this.hasItem(this.supportedItems[i]))
				{
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasMedicalTreatment()
		{
			bool flag = this.hasItem(DroneWeapons.HealBeamWeapon.HealItemType.MedicalBandage);
			bool flag2 = this.hasItem(DroneWeapons.HealBeamWeapon.HealItemType.FirstAidKit);
			return flag || flag2;
		}

		public bool targetCanBeHealed(EntityAlive _target)
		{
			return _target.IsAlive() && !_target.Buffs.HasBuff("buffHealHealth") && _target.GetCVar("medicalRegHealthAmount") == 0f && (float)_target.Health < _target.Stats.Health.ModifiedMax;
		}

		public bool isTargetInNeedOfMedical(EntityAlive _target)
		{
			if (!_target)
			{
				return false;
			}
			float num = (float)_target.GetMaxHealth();
			float modifiedMax = _target.Stats.Health.ModifiedMax;
			return ((num == modifiedMax && (float)_target.Health < num - this.HealDamageThreshold) || (float)_target.Health < modifiedMax * 0.67f) && _target.GetCVar("medicalRegHealthAmount") == 0f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool checkNeedsHealing(EntityAlive _target)
		{
			return this.targetCanBeHealed(_target) && this.isTargetInNeedOfMedical(_target);
		}

		public bool targetNeedsHealing(EntityAlive _target)
		{
			return this.findNeededHealType(_target) > DroneWeapons.HealBeamWeapon.HealItemType.None;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasBleedingTreatment()
		{
			bool flag = this.hasItem(DroneWeapons.HealBeamWeapon.HealItemType.Bandage);
			bool flag2 = this.hasItem(DroneWeapons.HealBeamWeapon.HealItemType.MedicalBandage);
			bool flag3 = this.hasItem(DroneWeapons.HealBeamWeapon.HealItemType.FirstAidKit);
			return flag || flag2 || flag3;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isTargetBleeding(EntityAlive _target)
		{
			return _target.Buffs.ActiveBuffs.Find((BuffValue b) => b.BuffName.ContainsCaseInsensitive("buffInjuryBleeding")) != null;
		}

		public bool targetNeedsTreatment(EntityAlive _target)
		{
			bool flag = this.isTargetBleeding(_target);
			return this.hasBleedingTreatment() && flag;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public DroneWeapons.HealBeamWeapon.HealItemType findNeededHealType(EntityAlive _entity)
		{
			bool flag = this.hasItem(DroneWeapons.HealBeamWeapon.HealItemType.Bandage);
			bool flag2 = this.hasItem(DroneWeapons.HealBeamWeapon.HealItemType.MedicalBandage);
			bool flag3 = this.hasItem(DroneWeapons.HealBeamWeapon.HealItemType.FirstAidKit);
			if (this.isTargetInNeedOfMedical(_entity) || this.targetCanBeHealed(_entity))
			{
				if (flag2)
				{
					return DroneWeapons.HealBeamWeapon.HealItemType.MedicalBandage;
				}
				if (flag3)
				{
					return DroneWeapons.HealBeamWeapon.HealItemType.FirstAidKit;
				}
				if (this.isTargetBleeding(_entity))
				{
					return DroneWeapons.HealBeamWeapon.HealItemType.Bandage;
				}
			}
			else if (this.isTargetBleeding(_entity))
			{
				if (flag)
				{
					return DroneWeapons.HealBeamWeapon.HealItemType.Bandage;
				}
				if (flag2)
				{
					return DroneWeapons.HealBeamWeapon.HealItemType.MedicalBandage;
				}
				if (flag3)
				{
					return DroneWeapons.HealBeamWeapon.HealItemType.FirstAidKit;
				}
			}
			return DroneWeapons.HealBeamWeapon.HealItemType.None;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ItemStack getHealingItemStack(DroneWeapons.HealBeamWeapon.HealItemType healType)
		{
			Bag bag = this.entity.bag;
			foreach (ItemStack itemStack in bag.GetSlots())
			{
				if (itemStack != null && this.isItem(itemStack.itemValue, healType))
				{
					ItemStack itemStack2 = new ItemStack(itemStack.itemValue.Clone(), 1);
					bag.DecItem(itemStack.itemValue, 1, false, null);
					return itemStack2;
				}
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasItem(string itemGroupOrName)
		{
			Bag bag = this.entity.bag;
			foreach (ItemStack itemStack in bag.GetSlots())
			{
				if (itemStack != null && this.isItem(itemStack.itemValue, itemGroupOrName) && bag.CanTakeItem(itemStack))
				{
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasItem(DroneWeapons.HealBeamWeapon.HealItemType itemType)
		{
			return this.hasItem(this.supportedItems[(int)itemType]);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isItem(ItemValue iv, string itemGroupOrName)
		{
			return iv.ItemClass != null && iv.ItemClass.Name.Equals(itemGroupOrName);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isItem(ItemValue iv, DroneWeapons.HealBeamWeapon.HealItemType itemType)
		{
			return this.isItem(iv, this.supportedItems[(int)itemType]);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string cHealBeam = "drone_heal_beam";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string cHealPlayer = "drone_heal_player";

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cIdxHealing = 0;

		public float HealDamageThreshold = 35f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const string cAbrasionInjury = "buffInjuryAbrasion";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string cHealingBuff = "buffHealHealth";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string cHealCVar = "medicalRegHealthAmount";

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cModifiedHealthCutoff = 0.67f;

		[PublicizedFrom(EAccessModifier.Private)]
		public string[] supportedItems = new string[] { "none", "medicalAloeCream", "medicalBandage", "medicalFirstAidBandage", "medicalFirstAidKit" };

		[PublicizedFrom(EAccessModifier.Private)]
		public const string cBuffInjuryBleeding = "buffInjuryBleeding";

		[PublicizedFrom(EAccessModifier.Private)]
		public enum HealItemType
		{
			None,
			AloeCream,
			Bandage,
			MedicalBandage,
			FirstAidKit
		}
	}

	[Preserve]
	public class StunBeamWeapon : DroneWeapons.Weapon
	{
		public StunBeamWeapon(EntityAlive _entity)
			: base(_entity)
		{
		}

		public override void Init()
		{
			this.WeaponJoint = this.entity.transform.FindInChilds("WristRight", false);
			this.cooldown = 10f;
			this.range = 2f;
		}

		public override void Fire(EntityAlive _target)
		{
			base.Fire(_target);
			this.target.SetCVar("_droneStunDamage", (float)this.modItem.Quality);
			base.TargetApplyBuff("buffShocked", true);
			base.PlaySound(this.entity, "drone_attackeffect");
			FireControllerUtils.SpawnParticleEffect(new ParticleEffect("nozzleflashuzi", this.WeaponJoint.position + Origin.position, Quaternion.Euler(0f, 180f, 0f), 1f, Color.white, "Electricity/Turret/turret_fire", this.WeaponJoint, 1f, ""), -1);
			float num = GameManager.Instance.World.GetLightBrightness(World.worldToBlockPos(this.entity.position)) / 2f;
			FireControllerUtils.SpawnParticleEffect(new ParticleEffect("nozzlesmokeuzi", this.WeaponJoint.position + Origin.position, num, new Color(1f, 1f, 1f, 0.3f), null, this.WeaponJoint, false, 1f, ""), -1);
		}
	}

	[Preserve]
	public class MachineGunWeapon : DroneWeapons.Weapon
	{
		public MachineGunWeapon(EntityAlive _entity)
			: base(_entity)
		{
		}

		public override void Init()
		{
			this.damageMultiplier = new DamageMultiplier(this.entityProperties);
			this.WeaponJoint = this.entity.transform.FindInChilds("WristRight", false);
			if (this.entityProperties.Values.ContainsKey("MaxDistance"))
			{
				this.range = StringParsers.ParseFloat(this.entityProperties.Values["MaxDistance"], 0, -1, NumberStyles.Any);
			}
			this.spreadHorizontal = new Vector2(-1f, 1f);
			this.spreadVertical = new Vector2(-1f, 1f);
			if (this.entityProperties.Values.ContainsKey("RaySpread"))
			{
				float num = StringParsers.ParseFloat(this.entityProperties.Values["RaySpread"], 0, -1, NumberStyles.Any);
				num *= 0.5f;
				this.spreadHorizontal = new Vector2(-num, num);
				this.spreadVertical = new Vector2(-num, num);
			}
			if (this.entityProperties.Values.ContainsKey("RayCount"))
			{
				this.RayCount = (float)int.Parse(this.entityProperties.Values["RayCount"]);
			}
			if (this.entityProperties.Values.ContainsKey("BurstRoundCount"))
			{
				this.burstRoundCountMax = int.Parse(this.entityProperties.Values["BurstRoundCount"]);
			}
			if (this.entityProperties.Values.ContainsKey("BurstFireRate"))
			{
				this.burstFireRate = Mathf.Max(StringParsers.ParseFloat(this.entityProperties.Values["BurstFireRate"], 0, -1, NumberStyles.Any), 0.1f);
			}
			this.actionTime = this.burstFireRate * (float)this.burstRoundCountMax;
			if (this.entityProperties.Values.ContainsKey("CooldownTime"))
			{
				this.cooldown = StringParsers.ParseFloat(this.entityProperties.Values["CooldownTime"], 0, -1, NumberStyles.Any);
			}
			if (this.entityProperties.Values.ContainsKey("EntityDamage"))
			{
				this.entityDamage = int.Parse(this.entityProperties.Values["EntityDamage"]);
			}
			this.buffActions = new List<string>();
			if (this.entityProperties.Values.ContainsKey("Buff"))
			{
				string[] array = this.entityProperties.Values["Buff"].Replace(" ", "").Split(',', StringSplitOptions.None);
				this.buffActions.AddRange(array);
			}
		}

		public override void Update()
		{
			base.Update();
			if (this.target != null && !this.target.IsDead() && this.burstRoundCount < this.burstRoundCountMax && base.TimeRemaning > 0f && base.TimeRemaning < base.TimeLength - this.burstFireRate * (float)this.burstRoundCount)
			{
				this._fireWeapon();
			}
		}

		public override void Fire(EntityAlive _target)
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return;
			}
			base.Fire(_target);
			this._fireWeapon();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnFireComplete()
		{
			base.OnFireComplete();
			this.burstRoundCount = 0;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void _fireWeapon()
		{
			EntityDrone entityDrone = this.entity as EntityDrone;
			Vector3 position = this.WeaponJoint.transform.position;
			Vector3 vector = this.target.getChestPosition() - Origin.position;
			EntityAlive entityAlive = GameManager.Instance.World.GetEntity(entityDrone.belongsPlayerId) as EntityAlive;
			GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
			FastTags<TagGroup.Global> itemTags = entityDrone.OriginalItemValue.ItemClass.ItemTags;
			int num = (int)EffectManager.GetValue(PassiveEffects.RoundRayCount, entityDrone.OriginalItemValue, this.RayCount, entityAlive, null, entityDrone.OriginalItemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false);
			float value = EffectManager.GetValue(PassiveEffects.MaxRange, entityDrone.OriginalItemValue, this.range, entityAlive, null, entityDrone.OriginalItemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false);
			for (int i = 0; i < num; i++)
			{
				Vector3 vector2 = (vector - position).normalized;
				vector2 = Quaternion.Euler(gameRandom.RandomRange(this.spreadHorizontal.x, this.spreadHorizontal.y), gameRandom.RandomRange(this.spreadVertical.x, this.spreadVertical.y), 0f) * vector2;
				Ray ray;
				ray..ctor(position + Origin.position, vector2);
				int num2 = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.EntityPenetrationCount, entityDrone.OriginalItemValue, 0f, entityAlive, null, entityDrone.OriginalItemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false));
				num2++;
				int num3 = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.BlockPenetrationFactor, entityDrone.OriginalItemValue, 1f, entityAlive, null, entityDrone.OriginalItemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false));
				EntityAlive entityAlive2 = null;
				for (int j = 0; j < num2; j++)
				{
					if (Voxel.Raycast(GameManager.Instance.World, ray, value, -538751005, 8, 0f))
					{
						WorldRayHitInfo worldRayHitInfo = Voxel.voxelRayHitInfo.Clone();
						if (worldRayHitInfo.tag.StartsWith("E_"))
						{
							string text;
							EntityAlive entityAlive3 = ItemActionAttack.FindHitEntityNoTagCheck(worldRayHitInfo, out text) as EntityAlive;
							if (entityAlive2 == entityAlive3)
							{
								ray.origin = worldRayHitInfo.hit.pos + ray.direction * 0.1f;
								j--;
								goto IL_0338;
							}
							entityAlive2 = entityAlive3;
						}
						else
						{
							j += Mathf.FloorToInt((float)ItemActionAttack.GetBlockHit(GameManager.Instance.World, worldRayHitInfo).Block.MaxDamage / (float)num3);
						}
						ItemActionAttack.Hit(worldRayHitInfo, entityDrone.belongsPlayerId, EnumDamageTypes.Piercing, this.GetDamageBlock(entityDrone.OriginalItemValue, BlockValue.Air, GameManager.Instance.World.GetEntity(entityDrone.belongsPlayerId) as EntityAlive, 1), this.GetDamageEntity(entityDrone.OriginalItemValue, GameManager.Instance.World.GetEntity(entityDrone.belongsPlayerId) as EntityAlive, 1), 1f, entityDrone.OriginalItemValue.PercentUsesLeft, 0f, 0f, "bullet", this.damageMultiplier, this.buffActions, new ItemActionAttack.AttackHitInfo(), 1, 0, 0f, null, null, ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, entityDrone.entityId, entityDrone.OriginalItemValue, false, false, true, null);
					}
					IL_0338:;
				}
			}
			ParticleEffect particleEffect = new ParticleEffect("nozzleflashuzi", this.WeaponJoint.position + Origin.position, Quaternion.Euler(0f, 180f, 0f), 1f, Color.white, "Electricity/Turret/turret_fire", this.WeaponJoint, 1f, "");
			base.SpawnParticleEffect(particleEffect, -1);
			float num4 = GameManager.Instance.World.GetLightBrightness(World.worldToBlockPos(this.entity.position)) / 2f;
			ParticleEffect particleEffect2 = new ParticleEffect("nozzlesmokeuzi", this.WeaponJoint.position + Origin.position, num4, new Color(1f, 1f, 1f, 0.3f), null, this.WeaponJoint, false, 1f, "");
			base.SpawnParticleEffect(particleEffect2, -1);
			this.burstRoundCount++;
			if ((int)EffectManager.GetValue(PassiveEffects.MagazineSize, entityDrone.OriginalItemValue, 0f, null, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) > 0)
			{
				EntityDrone entityDrone2 = entityDrone;
				int ammoCount = entityDrone2.AmmoCount;
				entityDrone2.AmmoCount = ammoCount - 1;
			}
			entityDrone.OriginalItemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, entityDrone.OriginalItemValue, 1f, entityAlive, null, entityDrone.OriginalItemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false) * ItemAction.ItemDegradationModifier;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float GetDamageEntity(ItemValue _itemValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
		{
			return EffectManager.GetValue(PassiveEffects.EntityDamage, _itemValue, (float)this.entityDamage, _holdingEntity, null, _itemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float GetDamageBlock(ItemValue _itemValue, BlockValue _blockValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
		{
			this.tmpTag = _itemValue.ItemClass.ItemTags;
			this.tmpTag |= _blockValue.Block.Tags;
			float value = EffectManager.GetValue(PassiveEffects.BlockDamage, _itemValue, (float)this.blockDamage, _holdingEntity, null, this.tmpTag, true, false, true, true, true, 1, true, false);
			return Utils.FastMin((float)_blockValue.Block.blockMaterial.MaxIncomingDamage, value);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float RayCount = 1f;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector2 spreadHorizontal;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector2 spreadVertical;

		[PublicizedFrom(EAccessModifier.Private)]
		public int burstRoundCountMax = 1;

		[PublicizedFrom(EAccessModifier.Private)]
		public int burstRoundCount;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float burstFireRateMax = 0.1f;

		[PublicizedFrom(EAccessModifier.Private)]
		public float burstFireRate = 1f;

		[PublicizedFrom(EAccessModifier.Private)]
		public int entityDamage;

		[PublicizedFrom(EAccessModifier.Private)]
		public int blockDamage;

		[PublicizedFrom(EAccessModifier.Private)]
		public DamageMultiplier damageMultiplier;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<string> buffActions;

		[PublicizedFrom(EAccessModifier.Private)]
		public FastTags<TagGroup.Global> tmpTag;
	}

	[Preserve]
	public class NetPackageDroneParticleEffect : NetPackage
	{
		public DroneWeapons.NetPackageDroneParticleEffect Setup(ParticleEffect _pe, int _entityThatCausedIt, DroneWeapons.NetPackageDroneParticleEffect.cActionType _actionType)
		{
			this.pe = _pe;
			this.entityThatCausedIt = _entityThatCausedIt;
			this.actionType = _actionType;
			return this;
		}

		public override void read(PooledBinaryReader _br)
		{
			this.pe = new ParticleEffect();
			this.pe.Read(_br);
			this.entityThatCausedIt = _br.ReadInt32();
		}

		public override void write(PooledBinaryWriter _bw)
		{
			base.write(_bw);
			this.pe.Write(_bw);
			_bw.Write(this.entityThatCausedIt);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			if (_world == null)
			{
				return;
			}
			if (!_world.IsRemote())
			{
				_world.GetGameManager().SpawnParticleEffectServer(this.pe, this.entityThatCausedIt, false, false);
				return;
			}
			Transform transform = _world.GetGameManager().SpawnParticleEffectClientForceCreation(this.pe, this.entityThatCausedIt, false);
			if (transform != null)
			{
				EntityDrone entityDrone = _world.GetEntity(this.entityThatCausedIt) as EntityDrone;
				if (entityDrone != null)
				{
					DroneBeamParticle component = transform.GetComponent<DroneBeamParticle>();
					if (this.actionType == DroneWeapons.NetPackageDroneParticleEffect.cActionType.Attack && entityDrone.activeWeapon != null)
					{
						transform.parent = entityDrone.activeWeapon.WeaponJoint;
						component.SetDisplayTime(entityDrone.AttackActionTime);
					}
					else if (this.actionType == DroneWeapons.NetPackageDroneParticleEffect.cActionType.Heal)
					{
						transform.parent = entityDrone.healWeapon.WeaponJoint;
						component.SetDisplayTime(entityDrone.HealActionTime);
					}
					transform.localPosition = Vector3.zero;
					transform.localRotation = Quaternion.identity;
				}
			}
		}

		public override int GetLength()
		{
			return 20;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ParticleEffect pe;

		[PublicizedFrom(EAccessModifier.Private)]
		public int entityThatCausedIt;

		[PublicizedFrom(EAccessModifier.Private)]
		public DroneWeapons.NetPackageDroneParticleEffect.cActionType actionType;

		public enum cActionType
		{
			Attack,
			Heal
		}
	}
}
