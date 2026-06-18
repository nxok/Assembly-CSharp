using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionThrownWeapon : ItemActionThrowAway
{
	public new ExplosionData Explosion { get; set; }

	public new int Velocity { get; set; }

	public new float FlyTime { get; set; }

	public new float LifeTime { get; set; }

	public new float CollisionRadius { get; set; }

	public float Gravity { get; set; }

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		this.Explosion = new ExplosionData(this.Properties, this.item.Effects);
		if (this.Properties.Values.ContainsKey("Velocity"))
		{
			this.Velocity = (int)StringParsers.ParseFloat(this.Properties.Values["Velocity"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.Velocity = 1;
		}
		if (this.Properties.Values.ContainsKey("FlyTime"))
		{
			this.FlyTime = StringParsers.ParseFloat(this.Properties.Values["FlyTime"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.FlyTime = 20f;
		}
		if (this.Properties.Values.ContainsKey("LifeTime"))
		{
			this.LifeTime = StringParsers.ParseFloat(this.Properties.Values["LifeTime"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.LifeTime = 100f;
		}
		if (this.Properties.Values.ContainsKey("CollisionRadius"))
		{
			this.CollisionRadius = StringParsers.ParseFloat(this.Properties.Values["CollisionRadius"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.CollisionRadius = 0.05f;
		}
		if (this.Properties.Values.ContainsKey("Gravity"))
		{
			this.Gravity = StringParsers.ParseFloat(this.Properties.Values["Gravity"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.Gravity = -9.81f;
		}
		if (_props.Values.ContainsKey("DamageEntity"))
		{
			this.damageEntity = StringParsers.ParseFloat(_props.Values["DamageEntity"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.damageEntity = 0f;
		}
		if (_props.Values.ContainsKey("DamageBlock"))
		{
			this.damageBlock = StringParsers.ParseFloat(_props.Values["DamageBlock"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.damageBlock = 0f;
		}
		this.hitmaskOverride = Voxel.ToHitMask(_props.GetString("Hitmask_override"));
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		return ((ItemActionThrowAway.MyInventoryData)_actionData).m_bActivated;
	}

	public override void StartHolding(ItemActionData _actionData)
	{
		base.StartHolding(_actionData);
		ItemActionThrowAway.MyInventoryData myInventoryData = (ItemActionThrowAway.MyInventoryData)_actionData;
		myInventoryData.m_bActivated = false;
		myInventoryData.m_ActivateTime = 0f;
		myInventoryData.m_LastThrowTime = 0f;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		ItemActionThrowAway.MyInventoryData myInventoryData = (ItemActionThrowAway.MyInventoryData)_actionData;
		float num = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient ? 0.1f : AnimationDelayData.AnimationDelay[myInventoryData.invData.item.HoldType.Value].RayCast);
		if (myInventoryData.m_LastThrowTime <= 0f || Time.time - myInventoryData.m_LastThrowTime < num)
		{
			return;
		}
		GameManager.Instance.ItemActionEffectsServer(myInventoryData.invData.holdingEntity.entityId, myInventoryData.invData.slotIdx, myInventoryData.indexInEntityOfAction, 0, Vector3.zero, Vector3.zero, 0);
	}

	public override void ItemActionEffects(GameManager _gameManager, ItemActionData _actionData, int _firingState, Vector3 _startPos, Vector3 _direction, int _userData = 0)
	{
		_actionData.invData.holdingEntity.emodel.avatarController.TriggerEvent("WeaponFire");
		(_actionData as ItemActionThrowAway.MyInventoryData).m_LastThrowTime = 0f;
		if (!_actionData.invData.holdingEntity.isEntityRemote)
		{
			this.throwAway(_actionData as ItemActionThrowAway.MyInventoryData);
		}
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		ItemActionThrowAway.MyInventoryData myInventoryData = (ItemActionThrowAway.MyInventoryData)_actionData;
		if (!myInventoryData.m_bActivated && Time.time - myInventoryData.m_LastThrowTime < this.Delay)
		{
			return;
		}
		if (_actionData.invData.itemValue.PercentUsesLeft == 0f)
		{
			if (_bReleased)
			{
				EntityPlayerLocal entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;
				base.HandleJamSound(_actionData.invData.itemValue, this.item, entityPlayerLocal);
			}
			return;
		}
		if (!_bReleased)
		{
			if (!myInventoryData.m_bActivated && !myInventoryData.m_bCanceled)
			{
				myInventoryData.m_bActivated = true;
				myInventoryData.m_ActivateTime = Time.time;
				myInventoryData.invData.holdingEntity.emodel.avatarController.TriggerEvent("WeaponPreFire");
			}
			return;
		}
		if (myInventoryData.m_bCanceled)
		{
			myInventoryData.m_bCanceled = false;
			return;
		}
		if (!myInventoryData.m_bActivated)
		{
			return;
		}
		myInventoryData.m_ThrowStrength = Mathf.Min(this.maxStrainTime, Time.time - myInventoryData.m_ActivateTime) / this.maxStrainTime * this.maxThrowStrength;
		myInventoryData.m_LastThrowTime = Time.time;
		myInventoryData.m_bActivated = false;
		if (!myInventoryData.invData.holdingEntity.isEntityRemote)
		{
			myInventoryData.invData.holdingEntity.emodel.avatarController.TriggerEvent("WeaponFire");
		}
		if (this.soundStart != null)
		{
			myInventoryData.invData.holdingEntity.PlayOneShot(this.soundStart, false, false, false, null, 1f);
		}
	}

	public override void CancelAction(ItemActionData _actionData)
	{
		ItemActionThrowAway.MyInventoryData myInventoryData = (ItemActionThrowAway.MyInventoryData)_actionData;
		myInventoryData.invData.holdingEntity.emodel.avatarController.TriggerEvent("WeaponPreFireCancel");
		myInventoryData.m_bActivated = false;
		myInventoryData.m_bCanceled = true;
		myInventoryData.m_ActivateTime = 0f;
		myInventoryData.m_LastThrowTime = 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void throwAway(ItemActionThrowAway.MyInventoryData _actionData)
	{
		ThrownWeaponMoveScript thrownWeaponMoveScript = this.instantiateProjectile(_actionData);
		ItemInventoryData invData = _actionData.invData;
		Vector3 lookVector = _actionData.invData.holdingEntity.GetLookVector();
		_actionData.invData.holdingEntity.getHeadPosition();
		Vector3 origin = _actionData.invData.holdingEntity.GetLookRay().origin;
		if (_actionData.invData.holdingEntity as EntityPlayerLocal != null)
		{
			float num = EffectManager.GetValue(PassiveEffects.StaminaLoss, _actionData.invData.holdingEntity.inventory.holdingItemItemValue, 2f, _actionData.invData.holdingEntity, null, _actionData.invData.holdingEntity.inventory.holdingItem.ItemTags | FastTags<TagGroup.Global>.Parse((_actionData.indexInEntityOfAction == 0) ? "primary" : "secondary"), true, true, true, true, true, 1, true, false) * ItemActionAttack.StaminaUsageMultiplier;
			_actionData.invData.holdingEntity.Stats.Stamina.Value -= num;
		}
		thrownWeaponMoveScript.Fire(origin, lookVector, _actionData.invData.holdingEntity, this.hitmaskOverride, _actionData.m_ThrowStrength);
		_actionData.invData.holdingEntity.inventory.DecHoldingItem(1);
	}

	public ThrownWeaponMoveScript instantiateProjectile(ItemActionData _actionData)
	{
		ItemValue holdingItemItemValue = _actionData.invData.holdingEntity.inventory.holdingItemItemValue;
		int entityId = _actionData.invData.holdingEntity.entityId;
		new ItemValue(_actionData.invData.item.Id, false);
		Transform transform = Object.Instantiate<GameObject>(_actionData.invData.holdingEntity.inventory.models[_actionData.invData.holdingEntity.inventory.holdingItemIdx].gameObject).transform;
		Utils.ForceMaterialsInstance(transform.gameObject);
		transform.parent = null;
		transform.position = _actionData.invData.model.transform.position;
		transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		Utils.SetLayerRecursively(transform.gameObject, 0);
		ThrownWeaponMoveScript thrownWeaponMoveScript = NGUITools.AddMissingComponent<ThrownWeaponMoveScript>(transform.gameObject);
		thrownWeaponMoveScript.itemActionThrownWeapon = this;
		thrownWeaponMoveScript.itemWeapon = _actionData.invData.item;
		thrownWeaponMoveScript.itemValueWeapon = _actionData.invData.itemValue;
		thrownWeaponMoveScript.actionData = _actionData as ItemActionThrowAway.MyInventoryData;
		thrownWeaponMoveScript.ProjectileOwnerID = _actionData.invData.holdingEntity.entityId;
		transform.gameObject.SetActive(true);
		_actionData.invData.model.gameObject.SetActive(false);
		_actionData.invData.holdingEntity.MinEventContext.Self = _actionData.invData.holdingEntity;
		_actionData.invData.holdingEntity.MinEventContext.Transform = transform;
		_actionData.invData.holdingEntity.MinEventContext.Tags = this.usePassedInTransformTag;
		thrownWeaponMoveScript.itemValueWeapon.FireEvent(MinEventTypes.onSelfHoldingItemThrown, _actionData.invData.holdingEntity.MinEventContext);
		return thrownWeaponMoveScript;
	}

	public float GetDamageEntity(ItemValue _itemValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
	{
		this.tmpTag = ((actionIndex == 0) ? ItemActionAttack.PrimaryTag : ItemActionAttack.SecondaryTag);
		this.tmpTag |= ((_itemValue.ItemClass == null) ? ItemActionAttack.MeleeTag : _itemValue.ItemClass.ItemTags);
		if (_holdingEntity != null)
		{
			this.tmpTag |= _holdingEntity.CurrentStanceTag | _holdingEntity.CurrentMovementTag;
		}
		return EffectManager.GetValue(PassiveEffects.EntityDamage, _itemValue, this.damageEntity, _holdingEntity, null, this.tmpTag, true, true, true, true, true, 1, true, false);
	}

	public float GetDamageBlock(ItemValue _itemValue, BlockValue _blockValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
	{
		this.tmpTag = ((actionIndex == 0) ? ItemActionAttack.PrimaryTag : ItemActionAttack.SecondaryTag);
		this.tmpTag |= ((_itemValue.ItemClass == null) ? ItemActionAttack.MeleeTag : _itemValue.ItemClass.ItemTags);
		if (_holdingEntity != null)
		{
			this.tmpTag |= _holdingEntity.CurrentStanceTag | _holdingEntity.CurrentMovementTag;
		}
		this.tmpTag |= _blockValue.Block.Tags;
		float value = EffectManager.GetValue(PassiveEffects.BlockDamage, _itemValue, this.damageBlock, _holdingEntity, null, this.tmpTag, true, true, true, true, true, 1, true, false);
		return Utils.FastMin((float)_blockValue.Block.blockMaterial.MaxIncomingDamage, value);
	}

	public float damageEntity;

	public float damageBlock;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int hitmaskOverride;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> usePassedInTransformTag = FastTags<TagGroup.Global>.Parse("usePassedInTransform");

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> tmpTag;
}
