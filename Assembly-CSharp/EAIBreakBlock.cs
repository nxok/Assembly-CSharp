using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIBreakBlock : EAIBase
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		this.MutexBits = 8;
		this.executeDelay = 0.15f;
	}

	public override bool CanExecute()
	{
		if (this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
			return false;
		}
		EntityMoveHelper moveHelper = this.theEntity.moveHelper;
		if (moveHelper.BlockedTime < 0.35f || !moveHelper.CanBreakBlocks)
		{
			return false;
		}
		if (this.theEntity.Jumping && !moveHelper.IsDestroyArea)
		{
			return false;
		}
		int num = ((this.theEntity.crouchType == 0 && this.theEntity.physicsHeight >= 1f) ? 7 : 5);
		if ((moveHelper.BlockedFlags & num) > 0)
		{
			Vector3i blockPos = moveHelper.HitInfo.hit.blockPos;
			if (this.theEntity.world.GetBlock(blockPos).isair)
			{
				return false;
			}
			float num2 = moveHelper.CalcBlockedDistanceSq();
			float num3 = this.theEntity.m_characterController.GetRadius() + 0.7f;
			if (num2 <= num3 * num3)
			{
				return true;
			}
		}
		return false;
	}

	public override void Start()
	{
		this.attackDelay = 1;
		Vector3i blockPos = this.theEntity.moveHelper.HitInfo.hit.blockPos;
		Block block = this.theEntity.world.GetBlock(blockPos).Block;
		if (block.HasTag(BlockTags.Door) || block.HasTag(BlockTags.ClosetDoor))
		{
			this.theEntity.IsBreakingDoors = true;
		}
	}

	public override bool Continue()
	{
		return (this.theEntity.onGround || this.theEntity.IsInElevator()) && this.CanExecute();
	}

	public override void Update()
	{
		EntityMoveHelper moveHelper = this.theEntity.moveHelper;
		if (this.attackDelay > 0)
		{
			this.attackDelay--;
		}
		if (this.attackDelay <= 0)
		{
			this.AttackBlock();
		}
	}

	public override void Reset()
	{
		this.theEntity.IsBreakingBlocks = false;
		this.theEntity.IsBreakingDoors = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AttackBlock()
	{
		this.theEntity.SetLookPosition(Vector3.zero);
		ItemActionAttackData itemActionAttackData = this.theEntity.inventory.holdingItemData.actionData[0] as ItemActionAttackData;
		if (itemActionAttackData == null)
		{
			return;
		}
		this.damageBoostPercent = 0f;
		if (this.theEntity is EntityZombie)
		{
			Bounds bounds;
			bounds..ctor(this.theEntity.position, new Vector3(1.7f, 1.5f, 1.7f));
			this.theEntity.world.GetEntitiesInBounds(typeof(EntityZombie), bounds, this.allies);
			for (int i = this.allies.Count - 1; i >= 0; i--)
			{
				if ((EntityZombie)this.allies[i] != this.theEntity)
				{
					this.damageBoostPercent += 0.2f;
				}
			}
			this.allies.Clear();
		}
		if (this.theEntity.Attack(false))
		{
			this.theEntity.IsBreakingBlocks = true;
			float num = 0.25f + base.RandomFloat * 0.8f;
			if (this.theEntity.moveHelper.IsUnreachableAbove)
			{
				num *= 0.5f;
			}
			this.attackDelay = (int)((num + 0.75f) * 20f);
			itemActionAttackData.hitDelegate = new ItemActionAttackData.HitDelegate(this.GetHitInfo);
			this.theEntity.Attack(true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldRayHitInfo GetHitInfo(out float damageScale)
	{
		EntityMoveHelper moveHelper = this.theEntity.moveHelper;
		damageScale = moveHelper.DamageScale + this.damageBoostPercent;
		return moveHelper.HitInfo;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDamageBoostPerAlly = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int attackDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public float damageBoostPercent;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> allies = new List<Entity>();
}
