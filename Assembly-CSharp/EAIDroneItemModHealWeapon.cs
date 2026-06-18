using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;

public class EAIDroneItemModHealWeapon : EAIDroneItemTask
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
		if (this.drone.Owner && LockManager.Instance.IsLockedServer(this.drone, 0))
		{
			return false;
		}
		EntityAlive nearestHealTargetInRange = this.drone.GetNearestHealTargetInRange(this.healTargetRange);
		if (!nearestHealTargetInRange)
		{
			return false;
		}
		if (!this.drone.TargetCanBeHealed(nearestHealTargetInRange))
		{
			return false;
		}
		if (!this.drone.IsTargetInNeedOfMedical(nearestHealTargetInRange))
		{
			return false;
		}
		DroneWeapons.Weapon installedWeapon = this.drone.GetInstalledWeapon(this.ItemKey);
		if (installedWeapon.canFire())
		{
			this.drone.SetAttackTarget(nearestHealTargetInRange, 1200);
			if (this.drone.GetAttackTarget())
			{
				this.drone.SetActiveWeapon(installedWeapon);
				this.drone.SetState(EntityDrone.State.Heal, true);
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
		DroneWeapons.Weapon installedWeapon = this.drone.GetInstalledWeapon(this.ItemKey);
		EntityAlive attackTarget2 = this.drone.GetAttackTarget();
		if (attackTarget2 && attackTarget2.AttachedToEntity as EntityVehicle != null)
		{
			if (installedWeapon != null)
			{
				installedWeapon.Fire(attackTarget);
			}
			this.OnAttackExit();
			return false;
		}
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
			else if (this.actionTimer <= 0f)
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
		base.OnAttackEnter();
		if (GameManager.Instance.GetPersistentLocalPlayer() != null && this.drone.initSuppressVOTimer <= 0f)
		{
			Manager.Play(this.drone, "drone_healeffect", 1f, false);
		}
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

	[PublicizedFrom(EAccessModifier.Private)]
	public float healTargetRange = 15f;
}
