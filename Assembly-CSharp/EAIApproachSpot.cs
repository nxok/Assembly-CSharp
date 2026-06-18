using System;
using GamePath;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIApproachSpot : EAIBase
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		this.MutexBits = 3;
		this.executeDelay = 0.1f;
	}

	public override bool CanExecute()
	{
		if (!this.theEntity.HasInvestigatePosition)
		{
			return false;
		}
		if (this.theEntity.IsSleeping)
		{
			return false;
		}
		this.investigatePos = this.theEntity.InvestigatePosition;
		this.seekPos = this.theEntity.world.FindSupportingBlockPos(this.investigatePos);
		return true;
	}

	public override void Start()
	{
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
		int num = this.investigateTicks + 1;
		this.investigateTicks = num;
		if (num > 40)
		{
			this.investigateTicks = 0;
			if (!this.theEntity.HasInvestigatePosition)
			{
				return false;
			}
			if ((this.investigatePos - this.theEntity.InvestigatePosition).sqrMagnitude >= 4f)
			{
				return false;
			}
		}
		if ((this.seekPos - this.theEntity.position).sqrMagnitude <= 4f || (path != null && path.isFinished()))
		{
			this.theEntity.ClearInvestigatePosition();
			return false;
		}
		return true;
	}

	public override void Update()
	{
		if (this.theEntity.navigator.getPath() != null)
		{
			this.hadPath = true;
			this.theEntity.moveHelper.CalcIfUnreachablePos();
		}
		Vector3 vector = this.investigatePos;
		vector.y += 0.8f;
		this.theEntity.SetLookPosition(vector);
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
		if (this.theEntity.IsScoutZombie)
		{
			AstarManager.Instance.AddLocationLine(this.theEntity.position, this.seekPos, 32);
		}
		if (PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
		{
			return;
		}
		this.pathRecalculateTicks = 40 + base.GetRandom(20);
		this.theEntity.FindPath(this.seekPos, this.theEntity.GetMoveSpeedAggro(), true, this);
	}

	public override void Reset()
	{
		this.theEntity.moveHelper.Stop();
		this.theEntity.SetLookPosition(Vector3.zero);
		this.manager.lookTime = 5f + base.RandomFloat * 3f;
		this.manager.interestDistance = 2f;
	}

	public override string ToString()
	{
		return string.Format("{0}, {1} dist{2}", base.ToString(), this.theEntity.navigator.noPathAndNotPlanningOne() ? "(-path)" : (this.theEntity.navigator.noPath() ? "(!path)" : ""), (this.theEntity.position - this.seekPos).magnitude.ToCultureInvariantString());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cInvestigateChangeDist = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCloseDist = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cLookTimeMin = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cLookTimeMax = 8f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 investigatePos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 seekPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hadPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public int investigateTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int pathRecalculateTicks;
}
