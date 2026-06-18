using System;
using System.Collections.Generic;
using GamePath;
using RaycastPathing;
using UnityEngine;

public class EAIDroneItemTask : EAIItemTask
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		this.drone = _theEntity as EntityDrone;
	}

	public override void SetData(Dictionary<string, string> data)
	{
		base.SetData(data);
		base.GetData(data, "enterTime", ref this.attackEnterTime);
		base.GetData(data, "actionTime", ref this.actionTime);
		base.GetData(data, "exitTime", ref this.attackExitTime);
		this.attackEnterTimer = this.attackEnterTime;
		base.GetData(data, "fleeDist", ref this.fleeDist);
	}

	public override bool CanExecute()
	{
		return this.drone && PathFinderThread.Instance != null && base.CanExecute();
	}

	public override bool Continue()
	{
		return this.drone && base.Continue();
	}

	public override void Update()
	{
		if (!this.drone)
		{
			return;
		}
		base.Update();
		if (this.attackEnterTimer > 0f)
		{
			this.attackEnterTimer -= 0.05f;
			if (this.attackEnterTimer <= 0f)
			{
				this.OnAttackEnter();
			}
		}
		if (this.actionTimer > 0f)
		{
			this.actionTimer -= 0.05f;
			if (this.actionTimer <= 0f)
			{
				this.OnAttackAction();
			}
		}
		if (this.attackExitTimer > 0f)
		{
			this.attackExitTimer -= 0.05f;
			if (this.attackExitTimer <= 0f)
			{
				this.OnAttackExit();
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnAttackEnter()
	{
		this.actionTimer = this.actionTime;
		if (this.drone.activeWeapon != null && !string.IsNullOrEmpty(this.drone.activeWeapon.soundWeaponUseBark))
		{
			this.drone.PlayVO(this.drone.activeWeapon.soundWeaponUseBark, true, 1f);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnAttackAction()
	{
		this.attackExitTimer = this.attackExitTime;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnAttackExit()
	{
		this.attackEnterTimer = this.attackEnterTime;
		this.actionTimer = 0f;
		this.attackExitTimer = 0f;
		this.drone.SetRevengeTarget(null);
		this.drone.SetAttackTarget(null, 0);
		this.weaponDischarged = false;
		this.ClearPath();
		this.drone.SetState((this.drone.OrderState == EntityDrone.Orders.Stay) ? EntityDrone.State.Sentry : EntityDrone.State.Idle, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool GetPath(Vector3 seekPos, float seekDist, float pointRadius, bool debugDraw = false, float duration = 0f)
	{
		return EntityDrone.GetPath(this.currentPath, this.drone, this.drone.position, seekPos, this.drone.SpeedFlying, this, seekDist, pointRadius, debugDraw, duration);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3[] GetGroupPositions(EntityAlive target, float dist, bool debugDraw = false, float duration = 0f)
	{
		return EntityDrone.GetGroupPositions(target, dist, debugDraw, duration);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool IsPositionBlocked(Vector3 start, Vector3 end, int layerMask = 1073807360, bool debugDraw = false, float duration = 0f)
	{
		return EntityDrone.IsPositionBlocked(start, end, layerMask, debugDraw, 0f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ClearPath()
	{
		this.currentPath.Clear();
		this.drone.OnPathInterupted();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void FollowPlannedPath(float speed, float pointRadius = 0.1f, bool debugDraw = false, float duration = 0f)
	{
		if (this.currentPath.Count > 0)
		{
			this.currentPathTarget = this.currentPath[0];
			this.drone.RotateTo((this.currentPathTarget - this.drone.position).normalized);
			this.drone.Move(this.currentPathTarget, pointRadius);
			if (this.drone.IsInRange(this.currentPathTarget, pointRadius))
			{
				this.currentPath.RemoveAt(0);
				return;
			}
			if (debugDraw && this.currentPath.Count > 1)
			{
				RaycastPathUtils.DrawLine(this.currentPath[0], this.currentPath[1], Color.green, 1f);
			}
			if (this.pathTracker.IsStuck(this.drone.position, this.currentPath[0], 0.5f, false))
			{
				if (this.currentPath.Count > 1)
				{
					this.drone.TeleportToPosition(this.currentPath[1]);
					this.currentPath.RemoveRange(0, 2);
					return;
				}
				this.drone.TeleportToPosition(this.currentPath[0]);
				this.currentPath.RemoveAt(0);
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool DoMoveIntoAtkPos(EntityAlive avaliableTarget, float seekDist, Vector3 seekForward, float pointRadius, bool debugDraw = false, float duration = 0f)
	{
		Vector3 position = this.drone.position;
		Vector3 chestPosition = avaliableTarget.getChestPosition();
		Vector3 normalized = (chestPosition - position).normalized;
		bool flag = Vector3.Dot(seekForward, normalized) >= 0.9f;
		float magnitude = (chestPosition - position).magnitude;
		Utils.DrawCircleLinesHorzontal(position - Origin.position, seekDist, Color.white, Color.red, 24, 0.05f);
		if (this.currentPath.Count == 0)
		{
			Vector3[] groupPositions = this.GetGroupPositions(avaliableTarget, 1.414f, false, 0f);
			Array.Sort<Vector3>(groupPositions, (Vector3 x, Vector3 y) => Vector3.Distance(position, x).CompareTo(Vector3.Distance(position, y)));
			Vector3 vector = groupPositions[0];
			this.GetPath(vector, seekDist, pointRadius, debugDraw, duration);
			if (this.currentPath.Count > 0)
			{
				this.currentPathDest = this.currentPath[this.currentPath.Count - 1];
			}
			else
			{
				this.currentPathDest = vector;
			}
		}
		else
		{
			if ((chestPosition - this.currentPathDest).magnitude > seekDist)
			{
				this.ClearPath();
			}
			if (this.IsPositionBlocked(position, chestPosition, 1073807360, false, 0f) || (float)this.currentPath.Count > seekDist + 1f || (this.currentPath.Count > 0 && magnitude > seekDist + 1.414f))
			{
				this.FollowPlannedPath(this.drone.SpeedFlying, pointRadius, false, 0f);
			}
			else if (magnitude >= seekDist)
			{
				this.drone.RotateTo(normalized);
				this.drone.Move(chestPosition, pointRadius);
			}
		}
		if (!this.IsPositionBlocked(position, chestPosition, 1073807360, false, 0f) && magnitude <= seekDist)
		{
			this.drone.RotateTo(normalized);
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool DoFleeFromTargetEntity(EntityAlive avaliableTarget, float seekDist, float pointRadius, bool debugDraw = false, float duration = 0f)
	{
		Vector3 position = this.drone.position;
		avaliableTarget.getChestPosition();
		if (this.currentPath.Count == 0)
		{
			Vector3[] groupPositions = this.GetGroupPositions(avaliableTarget, (this.fleeDist > 0f) ? this.fleeDist : 10f, false, 0f);
			Vector3 vector = groupPositions[this.drone.rand.RandomRange(0, groupPositions.Length)];
			this.GetPath(vector, seekDist, pointRadius, debugDraw, duration);
			if (this.currentPath.Count > 0)
			{
				this.currentPathDest = this.currentPath[this.currentPath.Count - 1];
			}
			else
			{
				this.currentPathDest = vector;
			}
		}
		else
		{
			bool flag = !EntityDrone.IsPositionBlocked(position, this.currentPathDest, 1073807360, false, 0f);
			float magnitude = (this.currentPathDest - position).magnitude;
			if (magnitude > 5f || !flag)
			{
				this.FollowPlannedPath(this.drone.SpeedFlying, pointRadius, false, 0f);
			}
			else
			{
				this.drone.RotateTo((this.currentPathDest - position).normalized);
				this.drone.Move(this.currentPathDest, pointRadius);
			}
			if (magnitude <= seekDist)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityDrone drone;

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<Vector3> currentPath = new List<Vector3>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 currentPathTarget;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 currentPathDest;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool weaponDischarged;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float attackEnterTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float actionTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float attackExitTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float attackEnterTimer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float actionTimer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float attackExitTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fleeDist;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityDrone.PathTracker pathTracker = new EntityDrone.PathTracker();
}
