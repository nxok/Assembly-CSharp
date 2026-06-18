using System;
using GamePath;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIPathTest : EAIBase
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		this.MutexBits = 1;
		this.banditEntity = _theEntity as EntityBandit;
	}

	public void SetTargetMove(Vector3 _targetWorld, EntityAlive _targetYaw, bool _canBreakBlocks)
	{
		if (this.banditEntity != null)
		{
			this.targetWorldPosition = _targetWorld;
			this.targetYaw = _targetYaw;
			this.newTarget = true;
			this.cancel = false;
			this.canBreakBlocks = _canBreakBlocks;
		}
	}

	public void CancelTargetMove()
	{
		this.targetWorldPosition = Vector3.zero;
		this.newTarget = false;
		this.cancel = true;
	}

	public override bool CanExecute()
	{
		return this.newTarget;
	}

	public override bool Continue()
	{
		return !this.cancel;
	}

	public override void Update()
	{
		base.Update();
		if (this.newTarget)
		{
			this.newTarget = false;
			PathInfoSingleTarget pathInfoSingleTarget = new PathInfoSingleTarget(this.theEntity, this.targetWorldPosition, this.canBreakBlocks, this.theEntity.GetMoveSpeed(), null);
			PathFinderThread.Instance.FindPath(pathInfoSingleTarget);
			if (this.banditEntity != null)
			{
				this.banditEntity.focusBody.SetFocus(FocusPriority.Highest, new AIFocusBody(this.targetYaw)
				{
					ConditionDistance = new AIFocusConditionDistance(5f, this.targetWorldPosition)
				});
			}
		}
		Vector3 vector = this.targetWorldPosition - Origin.position;
		Debug.DrawLine(vector, vector + Vector3.up * 2f, Color.blue);
		PathEntity path = this.theEntity.navigator.getPath();
		if (path != null)
		{
			for (int i = 0; i < path.getCurrentPathLength() - 1; i++)
			{
				Vector3 vector2 = path.getPathPointFromIndex(i).projectedLocation - Origin.position;
				Color color;
				color..ctor(1f, 1f, 0.1f);
				if (i < path.getCurrentPathIndex())
				{
					color..ctor(0.3f, 0.3f, 0f);
				}
				Utils.DrawLine(vector2, path.getPathPointFromIndex(i + 1).projectedLocation - Origin.position, color, color * 0.5f, 3, 0.1f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityBandit banditEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 targetWorldPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive targetYaw;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool newTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canBreakBlocks;
}
