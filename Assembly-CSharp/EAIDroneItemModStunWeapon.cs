using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;

public class EAIDroneItemModStunWeapon : EAIDroneItemTask
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
	}

	public override void SetData(Dictionary<string, string> data)
	{
		base.SetData(data);
	}

	public override bool CanExecute()
	{
		if (!base.CanExecute())
		{
			return false;
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return false;
		}
		if (this.drone.IsOnTeleportCooldown())
		{
			if (this.currentPath.Count > 0)
			{
				this.OnAttackExit();
			}
			return false;
		}
		if (!this.drone.CanAttack)
		{
			return false;
		}
		if (!this.drone.CanInterruptFollow)
		{
			return false;
		}
		EntityAlive owner = this.drone.Owner;
		if (owner && (this.drone.IsAttachedToVehicle(owner) || LockManager.Instance.IsLockedServer(this.drone, 0)))
		{
			return false;
		}
		if (this.drone.IsOwnerSneaking())
		{
			return false;
		}
		EntityAlive nearestEnemyInRange = this.drone.GetNearestEnemyInRange(owner ? owner.position : this.drone.position);
		if (!nearestEnemyInRange)
		{
			return false;
		}
		DroneWeapons.Weapon installedWeapon = this.drone.GetInstalledWeapon(this.ItemKey);
		if (installedWeapon.canFire())
		{
			this.drone.SetAttackTarget(nearestEnemyInRange, 1200);
			if (this.drone.GetAttackTarget())
			{
				this.drone.SetActiveWeapon(installedWeapon);
				this.drone.SetState(EntityDrone.State.Attack, true);
				return true;
			}
		}
		return false;
	}

	public override bool Continue()
	{
		if (!this.drone)
		{
			return false;
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return false;
		}
		EntityAlive attackTarget = this.drone.GetAttackTarget();
		if (!attackTarget || attackTarget.IsDead())
		{
			this.OnAttackExit();
			return false;
		}
		if (this.drone.WakeupAnimTime > 0f)
		{
			return false;
		}
		if (!this.drone.CanInterruptFollow)
		{
			return false;
		}
		EntityAlive owner = this.drone.Owner;
		if (owner && (this.drone.IsAttachedToVehicle(owner) || LockManager.Instance.IsLockedServer(this.drone, 0)))
		{
			this.OnAttackExit();
			return false;
		}
		DroneWeapons.Weapon installedWeapon = this.drone.GetInstalledWeapon(this.ItemKey);
		if (installedWeapon != null && this.attackEnterTimer <= 0f)
		{
			Vector3 position = this.drone.position;
			Transform transform = this.drone.transform;
			Utils.DrawCircleLinesHorzontal(position - Origin.position, this.drone.EnemyDetectionRadius, Color.white, Color.yellow, 24, 0.05f);
			float num = 0.1f;
			float num2 = 10f;
			if (!this.weaponDischarged)
			{
				Vector3 chestPosition = attackTarget.getChestPosition();
				if (base.DoMoveIntoAtkPos(attackTarget, installedWeapon.Range, transform.forward, num, true, num2) && installedWeapon.canFire())
				{
					Utils.DrawCircleLinesHorzontal(position - Origin.position, installedWeapon.Range, Color.white, Color.red, 24, 5f);
					Utils.DrawLine(position - Origin.position, chestPosition - Origin.position, Color.red, Color.red, 1, 5f);
					installedWeapon.Fire(attackTarget);
					this.weaponDischarged = true;
					base.ClearPath();
				}
			}
			else if (this.actionTimer <= 0f && base.DoFleeFromTargetEntity(attackTarget, 1.414f, num, true, num2))
			{
				this.OnAttackExit();
				return false;
			}
		}
		return true;
	}

	public override void Update()
	{
		base.Update();
	}

	public override void Reset()
	{
		base.Reset();
		this.OnAttackExit();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnAttackEnter()
	{
		if (this.drone.Owner)
		{
			Manager.Stop(this.drone.Owner.entityId, "drone_enemy_sense");
		}
		base.OnAttackEnter();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnAttackAction()
	{
		base.OnAttackAction();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnAttackExit()
	{
		base.OnAttackExit();
	}
}
