using System;
using GamePath;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIApproachDistraction : EAIBase
{
	public EAIApproachDistraction()
	{
		this.MutexBits = 3;
	}

	public override bool CanExecute()
	{
		EntityItem pendingDistraction = this.theEntity.pendingDistraction;
		if (!pendingDistraction || pendingDistraction.itemClass == null)
		{
			return false;
		}
		if (this.theEntity.GetAttackTarget())
		{
			if (!pendingDistraction.itemClass.IsEatDistraction)
			{
				this.theEntity.pendingDistraction = null;
			}
			return false;
		}
		if ((this.theEntity.position - pendingDistraction.position).sqrMagnitude < 2.25f && !pendingDistraction.itemClass.IsEatDistraction)
		{
			this.theEntity.pendingDistraction = null;
			return false;
		}
		return true;
	}

	public override void Start()
	{
		this.theEntity.SetAttackTarget(null, 0);
		this.theEntity.IsEating = false;
		this.theEntity.distraction = this.theEntity.pendingDistraction;
		this.theEntity.pendingDistraction = null;
		this.hadPath = false;
		this.updatePath();
	}

	public override bool Continue()
	{
		PathEntity path = this.theEntity.navigator.getPath();
		if (this.hadPath && path == null)
		{
			return false;
		}
		EntityItem distraction = this.theEntity.distraction;
		return !(distraction == null) && distraction.itemClass != null && (((this.theEntity.position - distraction.position).sqrMagnitude > 2.25f && (path == null || !path.isFinished())) || (distraction.itemClass.IsEatDistraction && distraction.IsDistractionActive));
	}

	public override void Update()
	{
		EntityItem distraction = this.theEntity.distraction;
		if (!distraction)
		{
			return;
		}
		PathEntity path = this.theEntity.getNavigator().getPath();
		if (path != null)
		{
			this.hadPath = true;
		}
		bool flag = false;
		if (path != null && !path.isFinished() && !this.theEntity.isCollidedHorizontally)
		{
			flag = true;
		}
		if (this.theEntity.IsSwimming())
		{
			flag = true;
		}
		if (Mathf.Abs(this.theEntity.speedForward) > 0.01f || Mathf.Abs(this.theEntity.speedStrafe) > 0.01f)
		{
			flag = true;
		}
		if (flag)
		{
			this.theEntity.SetLookPosition(distraction.position);
		}
		if ((this.theEntity.GetPosition() - distraction.position).sqrMagnitude <= 2.25f)
		{
			this.theEntity.IsEating = true;
			distraction.distractionEatTicks--;
			return;
		}
		int num = this.pathRecalculateTicks - 1;
		this.pathRecalculateTicks = num;
		if (num <= 0)
		{
			this.updatePath();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updatePath()
	{
		if (PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
		{
			return;
		}
		this.pathRecalculateTicks = 20 + base.GetRandom(20);
		this.theEntity.FindPath(this.theEntity.distraction.position, this.theEntity.GetMoveSpeedAggro(), true, this);
	}

	public override void Reset()
	{
		this.theEntity.moveHelper.Stop();
		this.theEntity.SetLookPosition(Vector3.zero);
		this.theEntity.IsEating = false;
		this.theEntity.distraction = null;
		this.manager.lookTime = 2f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCloseDist = 1.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cLookTime = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hadPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public int pathRecalculateTicks;
}
