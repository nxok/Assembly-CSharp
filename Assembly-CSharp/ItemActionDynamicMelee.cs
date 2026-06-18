using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionDynamicMelee : ItemActionDynamic
{
	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		ItemActionDynamicMelee.ItemActionDynamicMeleeData itemActionDynamicMeleeData = _actionData as ItemActionDynamicMelee.ItemActionDynamicMeleeData;
		if (_bReleased)
		{
			itemActionDynamicMeleeData.HasReleased = true;
			this.SetAttackFinished(itemActionDynamicMeleeData);
			itemActionDynamicMeleeData.HasExecuted = false;
			itemActionDynamicMeleeData.HasFinished = true;
			return;
		}
		if (!this.canStartAttack(itemActionDynamicMeleeData))
		{
			itemActionDynamicMeleeData.HasReleased = false;
			return;
		}
		if (itemActionDynamicMeleeData.HasExecuted)
		{
			this.SetAttackFinished(itemActionDynamicMeleeData);
			itemActionDynamicMeleeData.HasExecuted = false;
		}
		itemActionDynamicMeleeData.lastUseTime = Time.time;
		itemActionDynamicMeleeData.lastWeaponHeadPosition = Vector3.zero;
		itemActionDynamicMeleeData.lastWeaponHeadPositionDebug = Vector3.zero;
		itemActionDynamicMeleeData.lastClipPercentage = -1f;
		itemActionDynamicMeleeData.alreadyHitEnts.Clear();
		itemActionDynamicMeleeData.alreadyHitBlocks.Clear();
		itemActionDynamicMeleeData.EventParms.Self = itemActionDynamicMeleeData.invData.holdingEntity;
		itemActionDynamicMeleeData.EventParms.Other = null;
		itemActionDynamicMeleeData.EventParms.ItemActionData = itemActionDynamicMeleeData;
		itemActionDynamicMeleeData.EventParms.ItemValue = itemActionDynamicMeleeData.invData.itemValue;
		_actionData.invData.holdingEntity.MinEventContext.Other = null;
		for (int i = 0; i < ItemActionDynamic.DebugDisplayHits.Count; i++)
		{
			Object.DestroyImmediate(ItemActionDynamic.DebugDisplayHits[i]);
		}
		ItemActionDynamic.DebugDisplayHits.Clear();
		ItemActionAttack.AttackHitInfo attackHitInfo;
		itemActionDynamicMeleeData.IsHarvesting = this.checkHarvesting(_actionData, out attackHitInfo);
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		AvatarController avatarController = holdingEntity.emodel.avatarController;
		if (avatarController)
		{
			avatarController.UpdateBool(AvatarController.harvestingHash, itemActionDynamicMeleeData.IsHarvesting, true);
			avatarController.UpdateBool("IsHarvesting", itemActionDynamicMeleeData.IsHarvesting, true);
			avatarController.UpdateInt(AvatarController.itemActionIndexHash, itemActionDynamicMeleeData.indexInEntityOfAction, true);
		}
		string soundStart = this.soundStart;
		if (soundStart != null && !itemActionDynamicMeleeData.IsHarvesting)
		{
			holdingEntity.PlayOneShot(soundStart, false, false, false, null, 1f);
		}
		if (this.UsePowerAttackAnimation)
		{
			if (avatarController)
			{
				avatarController.TriggerEvent("PowerAttack");
			}
		}
		else if (!itemActionDynamicMeleeData.IsHarvesting)
		{
			holdingEntity.RightArmAnimationAttack = true;
		}
		if (itemActionDynamicMeleeData.IsHarvesting)
		{
			holdingEntity.StartHarvestingAnim(this.HarvestLength, !this.UsePowerAttackAnimation);
		}
		if (!this.UsePowerAttackTriggers)
		{
			holdingEntity.FireEvent(MinEventTypes.onSelfPrimaryActionStart, true);
		}
		else
		{
			holdingEntity.FireEvent(MinEventTypes.onSelfSecondaryActionStart, true);
		}
		EntityPlayerLocal entityPlayerLocal = holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal != null && entityPlayerLocal.movementInput.lastInputController)
		{
			entityPlayerLocal.MoveController.FindCameraSnapTarget(eCameraSnapMode.MeleeAttack, this.Range + 1f);
		}
		itemActionDynamicMeleeData.Attacking = true;
		itemActionDynamicMeleeData.HasExecuted = true;
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		bool flag = false;
		ItemActionDynamicMelee.ItemActionDynamicMeleeData itemActionDynamicMeleeData = _actionData as ItemActionDynamicMelee.ItemActionDynamicMeleeData;
		if (itemActionDynamicMeleeData != null)
		{
			flag = itemActionDynamicMeleeData.IsHarvesting || (itemActionDynamicMeleeData.invData.holdingEntity.emodel.avatarController != null && itemActionDynamicMeleeData.invData.holdingEntity.emodel.avatarController.IsAnimationHarvestingPlaying()) || itemActionDynamicMeleeData.Attacking || !itemActionDynamicMeleeData.HasFinished;
		}
		return flag;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		ItemActionDynamicMelee.ItemActionDynamicMeleeData itemActionDynamicMeleeData = _actionData as ItemActionDynamicMelee.ItemActionDynamicMeleeData;
		if (!itemActionDynamicMeleeData.Attacking)
		{
			return;
		}
		FastTags<TagGroup.Global> fastTags = ((_actionData.indexInEntityOfAction == 0) ? FastTags<TagGroup.Global>.Parse("primary") : FastTags<TagGroup.Global>.Parse("secondary"));
		ItemValue itemValue = _actionData.invData.itemValue;
		ItemClass itemClass = itemValue.ItemClass;
		if (itemClass != null)
		{
			fastTags |= itemClass.ItemTags;
		}
		float num = EffectManager.GetValue(PassiveEffects.AttacksPerMinute, itemValue, 60f, _actionData.invData.holdingEntity, null, fastTags, true, true, true, true, true, 1, true, false);
		if (num == 0f)
		{
			num = 0.0001f;
		}
		float num2 = 60f / num;
		itemActionDynamicMeleeData.attackTime = num2;
		if (Time.time - itemActionDynamicMeleeData.lastUseTime > num2 + 0.1f)
		{
			this.SetAttackFinished(_actionData);
			return;
		}
		if (itemActionDynamicMeleeData.invData == null || itemActionDynamicMeleeData.invData.holdingEntity == null || itemActionDynamicMeleeData.invData.holdingEntity.emodel == null)
		{
			this.SetAttackFinished(_actionData);
			return;
		}
		if (itemActionDynamicMeleeData.invData.holdingEntity.emodel.avatarController == null || itemActionDynamicMeleeData.invData.holdingEntity.IsDead())
		{
			this.SetAttackFinished(_actionData);
			return;
		}
		if (this.UseGrazingHits && itemActionDynamicMeleeData.invData.holdingEntity as EntityPlayerLocal != null)
		{
			float num3 = (Time.time - itemActionDynamicMeleeData.lastUseTime) / num2;
			if (num3 > this.GrazeStart - 0.1f && num3 < this.GrazeEnd + 0.1f)
			{
				this.GrazeCast(itemActionDynamicMeleeData, num3);
			}
		}
	}

	public void SetAttackFinished(ItemActionData _actionData)
	{
		ItemActionDynamicMelee.ItemActionDynamicMeleeData itemActionDynamicMeleeData = _actionData as ItemActionDynamicMelee.ItemActionDynamicMeleeData;
		if (itemActionDynamicMeleeData != null)
		{
			if (itemActionDynamicMeleeData.Attacking)
			{
				if (!this.UsePowerAttackTriggers)
				{
					itemActionDynamicMeleeData.invData.holdingEntity.FireEvent(MinEventTypes.onSelfPrimaryActionEnd, true);
				}
				else
				{
					itemActionDynamicMeleeData.invData.holdingEntity.FireEvent(MinEventTypes.onSelfSecondaryActionEnd, true);
				}
				if (_actionData.invData.holdingEntity.MinEventContext.Other == null)
				{
					_actionData.invData.holdingEntity.FireEvent((_actionData.indexInEntityOfAction == 0) ? MinEventTypes.onSelfPrimaryActionMissEntity : MinEventTypes.onSelfSecondaryActionMissEntity, true);
				}
			}
			itemActionDynamicMeleeData.Attacking = false;
			itemActionDynamicMeleeData.IsHarvesting = false;
			itemActionDynamicMeleeData.HasFinished = true;
		}
	}

	public override bool GrazeCast(ItemActionDynamic.ItemActionDynamicData _actionData, float normalizedClipTime = -1f)
	{
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (holdingEntity is EntityVehicle)
		{
			return false;
		}
		WorldRayHitInfo[] executeActionGrazeTarget = base.GetExecuteActionGrazeTarget(_actionData, normalizedClipTime);
		bool flag = false;
		for (int i = 0; i < executeActionGrazeTarget.Length; i++)
		{
			float num = (_actionData as ItemActionDynamicMelee.ItemActionDynamicMeleeData).StaminaUsage * EffectManager.GetValue(PassiveEffects.GrazeStaminaMultiplier, _actionData.invData.holdingEntity.inventory.holdingItemItemValue, this.GrazeStaminaPercentage * ItemActionAttack.StaminaUsageMultiplier, _actionData.invData.holdingEntity, null, _actionData.invData.holdingEntity.inventory.holdingItem.ItemTags, true, true, true, true, true, 1, true, false);
			if (holdingEntity.Stats.Stamina.Value >= num)
			{
				if (holdingEntity as EntityPlayerLocal != null)
				{
					holdingEntity.Stats.Stamina.Value -= num;
				}
				this.hitTarget(_actionData, executeActionGrazeTarget[i], true);
				flag = true;
			}
		}
		return flag;
	}

	public override bool Raycast(ItemActionDynamic.ItemActionDynamicData _actionData)
	{
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		if (holdingEntity is EntityVehicle)
		{
			return false;
		}
		if (holdingEntity as EntityPlayerLocal != null)
		{
			holdingEntity.Stats.Stamina.Value -= (_actionData as ItemActionDynamicMelee.ItemActionDynamicMeleeData).StaminaUsage;
		}
		_actionData.waterCollisionParticles.Reset();
		int num = 1;
		ItemValue itemValue = _actionData.invData.itemValue;
		FastTags<TagGroup.Global> fastTags = ((itemValue.ItemClass != null) ? itemValue.ItemClass.ItemTags : FastTags<TagGroup.Global>.none);
		num += Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.EntityPenetrationCount, itemValue, (float)this.EntityPenetrationCount, holdingEntity, null, fastTags, true, true, true, true, true, 1, true, false));
		int num2 = 0;
		int num3 = 0;
		EntityAlive entityAlive = null;
		do
		{
			WorldRayHitInfo executeActionTarget = this.GetExecuteActionTarget(_actionData);
			_actionData.waterCollisionParticles.CheckCollision(_actionData.ray.origin, _actionData.ray.direction, Utils.FastMax(this.Range, this.BlockRange) + this.SphereRadius, holdingEntity.entityId);
			EntityAlive entityAlive2;
			if (!base.isHitValid(executeActionTarget, _actionData, out entityAlive2))
			{
				break;
			}
			if (!entityAlive2 || entityAlive2 != entityAlive)
			{
				entityAlive = entityAlive2;
				if (ItemAction.ShowDebugDisplayHit)
				{
					DebugLines.Create(null, holdingEntity.RootTransform, holdingEntity.position, executeActionTarget.hit.pos, new Color(0.7f, 0f, 0f), new Color(1f, 1f, 0f), 0.05f, 0.02f, 2f);
				}
				this.hitTarget(_actionData, executeActionTarget, false);
				num2++;
			}
			if (++num3 >= 20 || !entityAlive2)
			{
				break;
			}
			_actionData.ray.origin = _actionData.hitInfo.hit.pos + _actionData.ray.direction * 0.1f;
			_actionData.useExistingRay = true;
		}
		while (num2 < num);
		AvatarController avatarController = holdingEntity.emodel.avatarController;
		if (num2 == 0)
		{
			if (avatarController)
			{
				avatarController.UpdateBool("RayHit", false, true);
				holdingEntity.FireEvent((_actionData.indexInEntityOfAction == 0) ? MinEventTypes.onSelfPrimaryActionRayMiss : MinEventTypes.onSelfSecondaryActionRayMiss, true);
			}
			return false;
		}
		if (avatarController)
		{
			avatarController.UpdateBool("RayHit", true, true);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkHarvesting(ItemActionData _actionData, out ItemActionAttack.AttackHitInfo myAttackHitInfo)
	{
		WorldRayHitInfo executeActionTarget = this.GetExecuteActionTarget(_actionData);
		ItemValue itemValue = _actionData.invData.itemStack.itemValue;
		myAttackHitInfo = new ItemActionAttack.AttackHitInfo
		{
			WeaponTypeTag = ItemActionAttack.MeleeTag
		};
		float num = base.GetDamageEntity(_actionData.invData.itemValue, _actionData.invData.holdingEntity, _actionData.indexInEntityOfAction);
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
		WorldRayHitInfo worldRayHitInfo = executeActionTarget;
		int entityId = _actionData.invData.holdingEntity.entityId;
		EnumDamageTypes enumDamageTypes = ((this.DamageType == EnumDamageTypes.None) ? EnumDamageTypes.Bashing : this.DamageType);
		float damageBlock = base.GetDamageBlock(_actionData.invData.itemValue, ItemActionAttack.GetBlockHit(_actionData.invData.world, executeActionTarget), _actionData.invData.holdingEntity, _actionData.indexInEntityOfAction);
		float num2 = num;
		float num3 = 1f;
		float num4 = 1f;
		float num5 = 0f;
		float dismemberChance = ItemAction.GetDismemberChance(_actionData, executeActionTarget);
		string id = this.item.MadeOfMaterial.id;
		DamageMultiplier damageMultiplier = new DamageMultiplier();
		List<string> list = new List<string>();
		ItemActionAttack.AttackHitInfo attackHitInfo = myAttackHitInfo;
		int num6 = 1;
		int actionExp = this.ActionExp;
		float actionExpBonusMultiplier = this.ActionExpBonusMultiplier;
		ItemActionAttack itemActionAttack = null;
		Dictionary<string, ItemActionAttack.Bonuses> toolBonuses = this.ToolBonuses;
		ItemActionAttack.EnumAttackMode enumAttackMode = ItemActionAttack.EnumAttackMode.Simulate;
		Dictionary<string, string> dictionary = null;
		int num7 = -1;
		bool flag2 = flag;
		ItemActionAttack.Hit(worldRayHitInfo, entityId, enumDamageTypes, damageBlock, num2, num3, num4, num5, dismemberChance, id, damageMultiplier, list, attackHitInfo, num6, actionExp, actionExpBonusMultiplier, itemActionAttack, toolBonuses, enumAttackMode, dictionary, num7, itemValue, false, flag2, true, null);
		if (myAttackHitInfo.bKilled)
		{
			return false;
		}
		if (Voxel.voxelRayHitInfo.tag.StartsWith("E_") && executeActionTarget.hit.distanceSq > this.Range * this.Range)
		{
			return false;
		}
		if (executeActionTarget.hit.distanceSq > this.BlockRange * this.BlockRange)
		{
			return false;
		}
		if (myAttackHitInfo.itemsToDrop != null && myAttackHitInfo.itemsToDrop.ContainsKey(EnumDropEvent.Harvest))
		{
			List<Block.SItemDropProb> list2 = myAttackHitInfo.itemsToDrop[EnumDropEvent.Harvest];
			for (int i = 0; i < list2.Count; i++)
			{
				if (list2[i].toolCategory != null && this.ToolBonuses != null && this.ToolBonuses.ContainsKey(list2[i].toolCategory))
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canStartAttack(ItemActionDynamicMelee.ItemActionDynamicMeleeData _actionData)
	{
		if (_actionData.Attacking)
		{
			return false;
		}
		EntityPlayerLocal entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal != null && !entityPlayerLocal.bFirstPersonView)
		{
			entityPlayerLocal.StartTPCameraLockTimer();
			if (entityPlayerLocal.TPCameraCheckResult == eTPCameraCheckResult.AngleCheckFailed)
			{
				return false;
			}
		}
		FastTags<TagGroup.Global> fastTags = ((_actionData.indexInEntityOfAction == 0) ? FastTags<TagGroup.Global>.Parse("primary") : FastTags<TagGroup.Global>.Parse("secondary"));
		ItemClass itemClass = _actionData.invData.itemValue.ItemClass;
		if (itemClass != null)
		{
			fastTags |= itemClass.ItemTags;
		}
		EntityAlive holdingEntity = _actionData.invData.holdingEntity;
		float num = EffectManager.GetValue(PassiveEffects.AttacksPerMinute, _actionData.invData.itemValue, 60f, holdingEntity, null, fastTags, true, true, true, true, true, 1, true, false);
		if (num == 0f)
		{
			num = 0.0001f;
		}
		num = 60f / num;
		if (Time.time - _actionData.lastUseTime < num + 0.1f)
		{
			return false;
		}
		if (EffectManager.GetValue(PassiveEffects.DisableItem, holdingEntity.inventory.holdingItemItemValue, 0f, holdingEntity, null, _actionData.invData.item.ItemTags, true, true, true, true, true, 1, true, false) > 0f)
		{
			_actionData.lastUseTime = Time.time;
			Manager.PlayInsidePlayerHead("twitch_no_attack", -1, 0f, false, false);
			return false;
		}
		if (_actionData.invData.itemValue.PercentUsesLeft == 0f)
		{
			if (_actionData.HasReleased)
			{
				EntityPlayerLocal entityPlayerLocal2 = holdingEntity as EntityPlayerLocal;
				base.HandleJamSound(_actionData.invData.itemValue, this.item, entityPlayerLocal2);
			}
			return false;
		}
		if (holdingEntity.Stats.Stamina != null)
		{
			_actionData.StaminaUsage = EffectManager.GetValue(PassiveEffects.StaminaLoss, _actionData.invData.itemValue, 2f, holdingEntity, null, _actionData.ActionTags, true, true, true, true, true, 1, true, false) * ItemActionAttack.StaminaUsageMultiplier;
			if (holdingEntity.Stats.Stamina.Value < _actionData.StaminaUsage)
			{
				if (_actionData.HasReleased)
				{
					Manager.Play(holdingEntity, holdingEntity.IsMale ? "player1stamina" : "player2stamina", 1f, false);
					GameManager.ShowTooltip(holdingEntity as EntityPlayerLocal, "ttOutOfStamina", true, false, 0f);
				}
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canContinueAttack(ItemActionDynamicMelee.ItemActionDynamicMeleeData _actionData)
	{
		return _actionData.invData.holdingEntity.IsAttackValid();
	}

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ItemActionDynamicMelee.ItemActionDynamicMeleeData(_invData, _indexInEntityOfAction);
	}

	public override ItemClass.EnumCrosshairType GetCrosshairType(ItemActionData _actionData)
	{
		EntityPlayerLocal entityPlayerLocal = _actionData.invData.holdingEntity as EntityPlayerLocal;
		if (entityPlayerLocal != null && !entityPlayerLocal.TPCameraCheckPassed)
		{
			return ItemClass.EnumCrosshairType.None;
		}
		if (this.isShowOverlay(_actionData))
		{
			return ItemClass.EnumCrosshairType.Damage;
		}
		return ItemClass.EnumCrosshairType.Plus;
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		this.SetAttackFinished(_data);
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_data.invData.holdingEntity as EntityPlayerLocal);
		if (uiforPlayer != null && XUiC_FocusedBlockHealth.IsWindowOpen(uiforPlayer))
		{
			XUiC_FocusedBlockHealth.SetData(uiforPlayer, null, 0f);
			((ItemActionDynamicMelee.ItemActionDynamicMeleeData)_data).uiOpenedByMe = false;
		}
	}

	public override void StartHolding(ItemActionData _data)
	{
		base.StartHolding(_data);
		this.SetAttackFinished(_data);
	}

	public class ItemActionDynamicMeleeData : ItemActionDynamic.ItemActionDynamicData
	{
		public ItemActionDynamicMeleeData(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}

		public float StaminaUsage;

		public bool Attacking;

		public bool HasReleased = true;

		public bool HasFinished = true;
	}
}
