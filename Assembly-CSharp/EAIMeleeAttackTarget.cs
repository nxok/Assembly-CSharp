using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIMeleeAttackTarget : EAIBase
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		this.MutexBits = 11;
		this.cooldown = 3f;
		this.attackDuration = 20f;
	}

	public override void SetData(Dictionary<string, string> data)
	{
		base.SetData(data);
		base.GetData(data, "slot", ref this.inventorySlot);
		base.GetData(data, "itemType", ref this.itemActionType);
		base.GetData(data, "startAnimType", ref this.startAnimType);
		base.GetData(data, "releaseDelay", ref this.releaseDelay);
		base.GetData(data, "cooldown", ref this.baseCooldown);
		base.GetData(data, "duration", ref this.attackDuration);
		base.GetData(data, "minRange", ref this.minRange);
		base.GetData(data, "maxRange", ref this.maxRange);
		base.GetData(data, "unreachableRange", ref this.unreachableRange);
		data.TryGetValue("sndStart", out this.soundStartName);
		data.TryGetValue("sndRelease", out this.soundReleaseName);
	}

	public override bool CanExecute()
	{
		if (this.theEntity.IsDancing)
		{
			return false;
		}
		if (this.cooldown > 0f)
		{
			this.cooldown -= this.executeWaitTime;
			return false;
		}
		if (!this.theEntity.IsAttackValid())
		{
			return false;
		}
		this.entityTarget = this.theEntity.GetAttackTarget();
		return this.entityTarget && !this.entityTarget.IsDead() && !this.theEntity.bodyDamage.IsAnyLegMissing && (this.startAnimType < 0 || !this.theEntity.bodyDamage.IsAnyArmOrLegMissing) && this.InRange() && this.theEntity.CanSee(this.entityTarget);
	}

	public override void Start()
	{
		this.theEntity.emodel.avatarController.hitWeightMax = 0.5f;
		this.painHitsFelt = this.theEntity.painHitsFelt;
		this.elapsedTime = 0f;
		this.state = EAIMeleeAttackTarget.State.Attack;
		this.stateTime = 0f;
		if (this.startAnimType >= 0)
		{
			this.state = EAIMeleeAttackTarget.State.StartAnim;
			this.theEntity.StartAnimAction(this.startAnimType + 3000);
		}
		if (!string.IsNullOrEmpty(this.soundStartName))
		{
			Manager.BroadcastPlay(this.theEntity, this.soundStartName, false, 1f);
		}
		this.theEntity.inventory.SetHoldingItemIdx(this.inventorySlot);
	}

	public override bool Continue()
	{
		return this.entityTarget && !this.entityTarget.IsDead() && this.elapsedTime < this.attackDuration && this.theEntity.bodyDamage.CurrentStun == EnumEntityStunType.None && !this.theEntity.Electrocuted && (this.startAnimType < 0 || !this.theEntity.bodyDamage.IsAnyArmOrLegMissing) && this.InRange() && this.theEntity.painHitsFelt - this.painHitsFelt < 1f;
	}

	public override void Update()
	{
		this.elapsedTime += 0.05f;
		if (this.elapsedTime < this.attackDuration * 0.5f)
		{
			Vector3 headPosition = this.entityTarget.getHeadPosition();
			if (this.theEntity.IsInFrontOfMe(headPosition))
			{
				this.theEntity.SetLookPosition(headPosition);
			}
			this.theEntity.SeekYawToPos(this.entityTarget.position, 30f);
		}
		this.stateTime += 0.05f;
		if (this.state == EAIMeleeAttackTarget.State.StartAnim)
		{
			if (this.theEntity.GetAnimActionState() != AvatarController.ActionState.Ready)
			{
				return;
			}
			this.theEntity.ContinueAnimAction(this.startAnimType + 1 + 3000);
			this.state = EAIMeleeAttackTarget.State.ReleaseAnim;
			this.stateTime = 0f;
			if (!string.IsNullOrEmpty(this.soundReleaseName))
			{
				Manager.BroadcastPlay(this.theEntity, this.soundReleaseName, false, 1f);
			}
		}
		if (this.state == EAIMeleeAttackTarget.State.ReleaseAnim)
		{
			if (this.stateTime < this.releaseDelay)
			{
				return;
			}
			this.state = EAIMeleeAttackTarget.State.Attack;
			this.stateTime = 0f;
		}
		this.theEntity.UseHoldingItem(this.itemActionType, false);
		if (!this.theEntity.IsHoldingItemInUse(this.itemActionType))
		{
			this.elapsedTime = float.MaxValue;
		}
	}

	public override void Reset()
	{
		this.theEntity.UseHoldingItem(this.itemActionType, true);
		this.theEntity.StartAnimAction(9999);
		this.theEntity.SetLookPosition(Vector3.zero);
		this.theEntity.emodel.avatarController.hitWeightMax = 1f;
		this.entityTarget = null;
		this.cooldown = this.baseCooldown + this.baseCooldown * 0.5f * base.RandomFloat;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool InRange()
	{
		float distanceSq = this.entityTarget.GetDistanceSq(this.theEntity);
		if (this.unreachableRange > 0f)
		{
			EntityMoveHelper moveHelper = this.theEntity.moveHelper;
			if (moveHelper.IsUnreachableAbove || moveHelper.IsUnreachableSide)
			{
				return distanceSq <= this.unreachableRange * this.unreachableRange;
			}
		}
		return distanceSq >= this.minRange * this.minRange && distanceSq <= this.maxRange * this.maxRange;
	}

	public override string ToString()
	{
		bool flag = this.entityTarget && this.InRange();
		return string.Format("{0} {1}, inRange{2}, Time {3}", new object[]
		{
			base.ToString(),
			this.entityTarget ? this.entityTarget.EntityName : "",
			flag,
			this.elapsedTime.ToCultureInvariantString("0.00")
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int inventorySlot;

	[PublicizedFrom(EAccessModifier.Private)]
	public int itemActionType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int startAnimType = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public float releaseDelay = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float baseCooldown;

	[PublicizedFrom(EAccessModifier.Private)]
	public float cooldown;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entityTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public float attackDuration;

	[PublicizedFrom(EAccessModifier.Private)]
	public float elapsedTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float minRange = 4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float maxRange = 25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float unreachableRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public string soundStartName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string soundReleaseName;

	[PublicizedFrom(EAccessModifier.Private)]
	public EAIMeleeAttackTarget.State state;

	[PublicizedFrom(EAccessModifier.Private)]
	public float stateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float painHitsFelt;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum State
	{
		StartAnim,
		ReleaseAnim,
		Attack,
		Release
	}
}
