using System;
using System.Collections.Generic;
using GamePath;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class EAIRunAway : EAIBase
{
	public EAIRunAway()
	{
	}

	public override void SetData(Dictionary<string, string> data)
	{
		base.SetData(data);
		base.GetData(data, "fleeDistance", ref this.fleeDistance);
	}

	public override bool CanExecute()
	{
		return this.FindFleePos(this.GetFleeFromPos());
	}

	public override void Start()
	{
		this.timeoutTicks = (30 + base.GetRandom(20)) * 20;
		PathFinderThread.Instance.RemovePathsFor(this.theEntity.entityId);
		string soundAlert = this.theEntity.GetSoundAlert();
		if (soundAlert != null)
		{
			this.theEntity.PlayOneShot(soundAlert, false, false, false, null, 1f);
		}
	}

	public override bool Continue()
	{
		return this.timeoutTicks > 0;
	}

	public override void Update()
	{
		this.timeoutTicks--;
		PathEntity path = this.theEntity.navigator.getPath();
		if (!this.checkedPath && !PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
		{
			this.checkedPath = true;
			if (path != null)
			{
				Vector3 rawEndPos = path.rawEndPos;
				if (this.theEntity.GetDistanceSq(rawEndPos) < 1.21f)
				{
					this.FindRandomPos();
				}
			}
		}
		if (this.checkedPath && path != null && path.getCurrentPathLength() >= 2 && path.NodeCountRemaining() <= 2)
		{
			this.fleeTicks = 0;
		}
		int num = this.fleeTicks - 1;
		this.fleeTicks = num;
		if (num <= 0)
		{
			Vector3 fleeFromPos = this.GetFleeFromPos();
			this.FindFleePos(fleeFromPos);
		}
		num = this.pathTicks - 1;
		this.pathTicks = num;
		if (num <= 0 && !PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
		{
			this.pathTicks = 60;
			this.theEntity.FindPath(this.targetPos, this.theEntity.GetMoveSpeed(), false, this);
			this.checkedPath = false;
		}
	}

	public override void Reset()
	{
		this.enemy = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool FindFleePos(Vector3 fleeFromPos)
	{
		Vector3 vector = this.theEntity.position - fleeFromPos;
		Vector3 vector2 = RandomPositionGenerator.CalcPositionInDirection(this.theEntity, this.theEntity.position, vector, (float)this.fleeDistance, 80f);
		if (vector2.y == 0f)
		{
			return false;
		}
		this.targetPos = vector2;
		this.fleeTicks = 80;
		this.pathTicks = 0;
		this.checkedPath = false;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool FindRandomPos()
	{
		Vector3 vector = RandomPositionGenerator.CalcAround(this.theEntity, this.fleeDistance, 0);
		if (vector.y == 0f)
		{
			this.fleeTicks = 0;
			return false;
		}
		this.targetPos = vector;
		this.fleeTicks = 80;
		this.pathTicks = 0;
		this.checkedPath = false;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract Vector3 GetFleeFromPos();

	public override string ToString()
	{
		return string.Format("{0}, flee {1}, timeout {2}, {3}", new object[]
		{
			base.ToString(),
			this.fleeTicks,
			this.timeoutTicks,
			(this.enemy != null) ? this.enemy.GetDebugName() : ""
		});
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityAlive enemy;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 targetPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int timeoutTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fleeTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int pathTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkedPath;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fleeDistance = 20;
}
