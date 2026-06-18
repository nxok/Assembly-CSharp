using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionMelee : ItemActionAttack
{
	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionMelee.InventoryDataMelee(_invData, _indexInEntityOfAction);
	}

	public override ItemClass.EnumCrosshairType GetCrosshairType(ItemActionData _actionData)
	{
		if (this.isShowOverlay((ItemActionAttackData)_actionData))
		{
			return ItemClass.EnumCrosshairType.Damage;
		}
		return ItemClass.EnumCrosshairType.Plus;
	}

	public override WorldRayHitInfo GetExecuteActionTarget(ItemActionData _actionData)
	{
		ItemActionMelee.InventoryDataMelee inventoryDataMelee = (ItemActionMelee.InventoryDataMelee)_actionData;
		EntityAlive holdingEntity = inventoryDataMelee.invData.holdingEntity;
		inventoryDataMelee.ray = holdingEntity.GetMeleeRay();
		if (holdingEntity.IsBreakingBlocks)
		{
			if (inventoryDataMelee.ray.direction.y < 0f)
			{
				inventoryDataMelee.ray.direction = new Vector3(inventoryDataMelee.ray.direction.x, 0f, inventoryDataMelee.ray.direction.z);
				ItemActionMelee.InventoryDataMelee inventoryDataMelee2 = inventoryDataMelee;
				inventoryDataMelee2.ray.origin = inventoryDataMelee2.ray.origin + new Vector3(0f, -0.7f, 0f);
			}
		}
		else if (holdingEntity.GetAttackTarget() != null)
		{
			Vector3 vector = holdingEntity.GetAttackTargetHitPosition() - inventoryDataMelee.ray.origin;
			inventoryDataMelee.ray = new Ray(inventoryDataMelee.ray.origin, vector);
		}
		ItemActionMelee.InventoryDataMelee inventoryDataMelee3 = inventoryDataMelee;
		inventoryDataMelee3.ray.origin = inventoryDataMelee3.ray.origin - 0.15f * inventoryDataMelee.ray.direction;
		int modelLayer = holdingEntity.GetModelLayer();
		holdingEntity.SetModelLayer(2, false, null);
		float num = Utils.FastMax(this.Range, this.BlockRange) + 0.15f;
		if (holdingEntity is EntityEnemy && holdingEntity.IsBreakingBlocks)
		{
			Voxel.Raycast(inventoryDataMelee.invData.world, inventoryDataMelee.ray, num, 1073807360, 128, 0.4f);
		}
		else
		{
			EntityAlive entityAlive = null;
			int num2 = -538767389;
			if (Voxel.Raycast(inventoryDataMelee.invData.world, inventoryDataMelee.ray, num, num2, 128, this.SphereRadius))
			{
				entityAlive = ItemActionAttack.GetEntityFromHit(Voxel.voxelRayHitInfo) as EntityAlive;
			}
			if (entityAlive == null)
			{
				Voxel.Raycast(inventoryDataMelee.invData.world, inventoryDataMelee.ray, num, -538488845, 128, this.SphereRadius);
			}
		}
		holdingEntity.SetModelLayer(modelLayer, false, null);
		return _actionData.GetUpdatedHitInfo();
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		ItemActionMelee.InventoryDataMelee inventoryDataMelee = (ItemActionMelee.InventoryDataMelee)_actionData;
		if (_bReleased)
		{
			inventoryDataMelee.bFirstHitInARow = true;
			return;
		}
		if (Time.time - inventoryDataMelee.lastUseTime < this.Delay)
		{
			return;
		}
		inventoryDataMelee.lastUseTime = Time.time;
		if (inventoryDataMelee.invData.itemValue.MaxUseTimes > 0 && inventoryDataMelee.invData.itemValue.UseTimes >= (float)inventoryDataMelee.invData.itemValue.MaxUseTimes)
		{
			EntityPlayerLocal entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;
			base.HandleJamSound(_actionData.invData.itemValue, this.item, entityPlayerLocal);
			return;
		}
		_actionData.invData.holdingEntity.RightArmAnimationAttack = true;
		ItemActionAttack.AttackHitInfo attackHitInfo;
		inventoryDataMelee.bHarvesting = this.checkHarvesting(_actionData, out attackHitInfo);
		if (inventoryDataMelee.bHarvesting)
		{
			_actionData.invData.holdingEntity.HarvestingAnimation = true;
		}
		string soundStart = this.soundStart;
		if (soundStart != null)
		{
			_actionData.invData.holdingEntity.PlayOneShot(soundStart, false, false, false, null, 1f);
		}
		inventoryDataMelee.bAttackStarted = true;
		if ((double)inventoryDataMelee.invData.holdingEntity.speedForward > 0.009)
		{
			this.rayCastDelay = AnimationDelayData.AnimationDelay[inventoryDataMelee.invData.item.HoldType.Value].RayCastMoving;
			return;
		}
		this.rayCastDelay = AnimationDelayData.AnimationDelay[inventoryDataMelee.invData.item.HoldType.Value].RayCast;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isShowOverlay(ItemActionData actionData)
	{
		if (!base.isShowOverlay(actionData))
		{
			return false;
		}
		if (((ItemActionMelee.InventoryDataMelee)actionData).bFirstHitInARow && Time.time - actionData.lastUseTime <= this.rayCastDelay)
		{
			return false;
		}
		WorldRayHitInfo executeActionTarget = this.GetExecuteActionTarget(actionData);
		return executeActionTarget.bHitValid && (executeActionTarget.tag == null || !GameUtils.IsBlockOrTerrain(executeActionTarget.tag) || executeActionTarget.hit.distanceSq <= this.BlockRange * this.BlockRange) && (executeActionTarget.tag == null || !executeActionTarget.tag.StartsWith("E_") || executeActionTarget.hit.distanceSq <= this.Range * this.Range);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkHarvesting(ItemActionData _actionData, out ItemActionAttack.AttackHitInfo myAttackHitInfo)
	{
		WorldRayHitInfo executeActionTarget = this.GetExecuteActionTarget(_actionData);
		ItemValue itemValue = _actionData.invData.itemValue;
		float num = base.GetDamageEntity(itemValue, _actionData.invData.holdingEntity, _actionData.indexInEntityOfAction);
		bool flag;
		if (_actionData.invData.holdingEntity is EntityPlayer)
		{
			num *= ItemActionAttack.MeleeDamagePercent;
			flag = ItemActionAttack.MeleeDamagePercent == 0f;
		}
		else
		{
			num *= ItemActionAttack.EntityPlayerDamagePercent;
			flag = ItemActionAttack.EntityPlayerDamagePercent == 0f;
		}
		myAttackHitInfo = new ItemActionAttack.AttackHitInfo
		{
			WeaponTypeTag = ItemActionAttack.MeleeTag
		};
		ItemActionAttack.Hit(executeActionTarget, _actionData.invData.holdingEntity.entityId, (this.DamageType == EnumDamageTypes.None) ? EnumDamageTypes.Bashing : this.DamageType, base.GetDamageBlock(itemValue, ItemActionAttack.GetBlockHit(_actionData.invData.world, executeActionTarget), _actionData.invData.holdingEntity, _actionData.indexInEntityOfAction), num, 1f, 1f, 0f, ItemAction.GetDismemberChance(_actionData, executeActionTarget), this.item.MadeOfMaterial.id, this.damageMultiplier, this.getBuffActions(_actionData), myAttackHitInfo, 1, this.ActionExp, this.ActionExpBonusMultiplier, this, this.ToolBonuses, ItemActionAttack.EnumAttackMode.Simulate, null, -1, null, false, flag, true, null);
		if (myAttackHitInfo.bKilled)
		{
			return false;
		}
		if (myAttackHitInfo.itemsToDrop != null && myAttackHitInfo.itemsToDrop.ContainsKey(EnumDropEvent.Harvest))
		{
			List<Block.SItemDropProb> list = myAttackHitInfo.itemsToDrop[EnumDropEvent.Harvest];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].toolCategory != null && this.ToolBonuses != null && this.ToolBonuses.ContainsKey(list[i].toolCategory))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		ItemActionMelee.InventoryDataMelee inventoryDataMelee = (ItemActionMelee.InventoryDataMelee)_actionData;
		return Time.time - inventoryDataMelee.lastUseTime < this.Delay + 0.1f;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		ItemActionMelee.InventoryDataMelee inventoryDataMelee = (ItemActionMelee.InventoryDataMelee)_actionData;
		if (!inventoryDataMelee.bAttackStarted || Time.time - inventoryDataMelee.lastUseTime < this.rayCastDelay)
		{
			return;
		}
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (this.rayCastDelay <= 0f && !holdingEntity.IsAttackImpact())
		{
			return;
		}
		inventoryDataMelee.bAttackStarted = false;
		ItemActionAttackData.HitDelegate hitDelegate = inventoryDataMelee.hitDelegate;
		inventoryDataMelee.hitDelegate = null;
		if (!holdingEntity.IsAttackValid())
		{
			return;
		}
		float num = EffectManager.GetValue(PassiveEffects.StaminaLoss, inventoryDataMelee.invData.itemValue, 0f, holdingEntity, null, (_actionData.indexInEntityOfAction == 0) ? FastTags<TagGroup.Global>.Parse("primary") : FastTags<TagGroup.Global>.Parse("secondary"), true, true, true, true, true, 1, true, false) * ItemActionAttack.StaminaUsageMultiplier;
		holdingEntity.AddStamina(-num);
		float num2 = 1f;
		WorldRayHitInfo worldRayHitInfo;
		if (hitDelegate != null)
		{
			worldRayHitInfo = hitDelegate(out num2);
		}
		else
		{
			worldRayHitInfo = this.GetExecuteActionTarget(_actionData);
		}
		if (worldRayHitInfo == null || !worldRayHitInfo.bHitValid)
		{
			return;
		}
		if (worldRayHitInfo.tag != null && GameUtils.IsBlockOrTerrain(worldRayHitInfo.tag) && worldRayHitInfo.hit.distanceSq > this.BlockRange * this.BlockRange)
		{
			return;
		}
		if (worldRayHitInfo.tag != null && worldRayHitInfo.tag.StartsWith("E_") && worldRayHitInfo.hit.distanceSq > this.Range * this.Range)
		{
			return;
		}
		if (inventoryDataMelee.invData.itemValue.MaxUseTimes > 0)
		{
			_actionData.invData.itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, inventoryDataMelee.invData.itemValue, 1f, holdingEntity, null, _actionData.invData.itemValue.ItemClass.ItemTags, true, true, true, true, true, 1, true, false) * ItemAction.ItemDegradationModifier;
			base.HandleItemBreak(_actionData);
		}
		if (ItemAction.ShowDebugDisplayHit)
		{
			DebugLines.Create("MeleeHit", holdingEntity.RootTransform, holdingEntity.position, worldRayHitInfo.hit.pos, new Color(0.7f, 0f, 0f), new Color(1f, 1f, 0f), 0.05f, 0.02f, 1f);
		}
		this.hitTheTarget(inventoryDataMelee, worldRayHitInfo, num2);
		if (inventoryDataMelee.bFirstHitInARow)
		{
			inventoryDataMelee.bFirstHitInARow = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void hitTheTarget(ItemActionMelee.InventoryDataMelee _actionData, WorldRayHitInfo hitInfo, float damageScale)
	{
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		ItemValue itemValue = _actionData.invData.itemValue;
		float num = 1f;
		if (itemValue.MaxUseTimes > 0)
		{
			num = ((float)itemValue.MaxUseTimes - itemValue.UseTimes) / (float)itemValue.MaxUseTimes;
		}
		_actionData.attackDetails.WeaponTypeTag = ItemActionAttack.MeleeTag;
		int num2 = 1;
		if (this.bUseParticleHarvesting && (this.particleHarvestingCategory == null || this.particleHarvestingCategory == this.item.MadeOfMaterial.id))
		{
			num2 |= 4;
		}
		float num3 = base.GetDamageBlock(itemValue, ItemActionAttack.GetBlockHit(_actionData.invData.world, hitInfo), holdingEntity, _actionData.indexInEntityOfAction) * damageScale;
		float num4 = base.GetDamageEntity(itemValue, holdingEntity, _actionData.indexInEntityOfAction);
		bool flag;
		if (holdingEntity is EntityPlayer)
		{
			num4 *= ItemActionAttack.MeleeDamagePercent;
			flag = ItemActionAttack.MeleeDamagePercent == 0f;
		}
		else
		{
			num4 *= ItemActionAttack.EntityPlayerDamagePercent;
			flag = ItemActionAttack.EntityPlayerDamagePercent == 0f;
		}
		int entityId = holdingEntity.entityId;
		EnumDamageTypes enumDamageTypes = ((this.DamageType == EnumDamageTypes.None) ? EnumDamageTypes.Bashing : this.DamageType);
		float num5 = num3;
		float num6 = num4;
		Stat stamina = holdingEntity.Stats.Stamina;
		ItemActionAttack.Hit(hitInfo, entityId, enumDamageTypes, num5, num6, (stamina != null) ? stamina.ValuePercent : 1f, num, 0f, ItemAction.GetDismemberChance(_actionData, hitInfo), this.item.MadeOfMaterial.SurfaceCategory, this.damageMultiplier, this.getBuffActions(_actionData), _actionData.attackDetails, num2, this.ActionExp, this.ActionExpBonusMultiplier, this, this.ToolBonuses, _actionData.bHarvesting ? ItemActionAttack.EnumAttackMode.RealAndHarvesting : ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, -1, null, false, flag, true, null);
		GameUtils.HarvestOnAttack(_actionData, this.ToolBonuses);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float rayCastDelay;

	[PublicizedFrom(EAccessModifier.Protected)]
	public class InventoryDataMelee : ItemActionAttackData
	{
		public InventoryDataMelee(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}

		public bool bAttackStarted;

		public Ray ray;

		public bool bHarvesting;

		public bool bFirstHitInARow;
	}
}
