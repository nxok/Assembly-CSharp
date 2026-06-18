using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIDodge : EAIBase
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		this.executeDelay = 0.1f;
		this.cooldown = 3f;
		this.actionkDuration = 1f;
	}

	public override void SetData(Dictionary<string, string> data)
	{
		base.SetData(data);
		string text;
		if (data.TryGetValue("tags", out text))
		{
			this.tags = FastTags<TagGroup.Global>.Parse(text);
		}
		base.GetData(data, "maxXZDistance", ref this.maxXZDistance);
		base.GetData(data, "cooldown", ref this.baseCooldown);
		base.GetData(data, "duration", ref this.actionkDuration);
		base.GetData(data, "minRange", ref this.minRange);
		base.GetData(data, "maxRange", ref this.maxRange);
		base.GetData(data, "unreachableRange", ref this.unreachableRange);
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
		this.theEntity.world.GetEntitiesInBounds(this.tags, BoundsUtils.ExpandBounds(this.theEntity.boundingBox, this.maxXZDistance, 8f, this.maxXZDistance), EAIDodge.entityList);
		this.entityTarget = null;
		for (int i = 0; i < EAIDodge.entityList.Count; i++)
		{
			EntityAlive entityAlive = EAIDodge.entityList[i] as EntityAlive;
			if (entityAlive && !entityAlive.IsDead() && entityAlive.emodel.avatarController.IsAnimationToDodge())
			{
				this.entityTarget = entityAlive;
				break;
			}
		}
		EAIDodge.entityList.Clear();
		return !(this.entityTarget == null) && this.InRange() && this.theEntity.CanSee(this.entityTarget);
	}

	public override void Start()
	{
		this.actionTime = 0f;
		this.theEntity.emodel.avatarController.StartAnimationDodge(base.Random.RandomFloat);
	}

	public override bool Continue()
	{
		return this.entityTarget && this.entityTarget.IsAlive() && this.actionTime < this.actionkDuration && this.theEntity.hasBeenAttackedTime <= 0;
	}

	public override void Update()
	{
		this.actionTime += 0.05f;
		if (this.actionTime < this.actionkDuration * 0.5f)
		{
			Vector3 headPosition = this.entityTarget.getHeadPosition();
			if (this.theEntity.IsInFrontOfMe(headPosition))
			{
				this.theEntity.SetLookPosition(headPosition);
			}
		}
	}

	public override void Reset()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool InRange()
	{
		float distanceSq = this.entityTarget.GetDistanceSq(this.theEntity);
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
			this.actionTime.ToCultureInvariantString("0.00")
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> tags;

	[PublicizedFrom(EAccessModifier.Private)]
	public float maxXZDistance = 100f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float baseCooldown;

	[PublicizedFrom(EAccessModifier.Private)]
	public float cooldown;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive entityTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public float actionTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float actionkDuration;

	[PublicizedFrom(EAccessModifier.Private)]
	public float minRange = 4f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float maxRange = 25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float unreachableRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Entity> entityList = new List<Entity>();
}
