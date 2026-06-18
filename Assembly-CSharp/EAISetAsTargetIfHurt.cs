using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAISetAsTargetIfHurt : EAITarget
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity, 0f, false);
		this.MutexBits = 1;
	}

	public override void SetData(Dictionary<string, string> data)
	{
		base.SetData(data);
		this.targetClasses = new List<EAISetAsTargetIfHurt.TargetClass>();
		string text;
		if (data.TryGetValue("class", out text))
		{
			string[] array = text.Split(',', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				EAISetAsTargetIfHurt.TargetClass targetClass = default(EAISetAsTargetIfHurt.TargetClass);
				targetClass.type = EntityFactory.GetEntityType(array[i]);
				this.targetClasses.Add(targetClass);
			}
		}
	}

	public override bool CanExecute()
	{
		EntityAlive revengeTarget = this.theEntity.GetRevengeTarget();
		EntityAlive attackTarget = this.theEntity.GetAttackTarget();
		if (revengeTarget && revengeTarget != attackTarget && revengeTarget.entityType != this.theEntity.entityType)
		{
			if (this.targetClasses != null)
			{
				bool flag = false;
				Type type = revengeTarget.GetType();
				for (int i = 0; i < this.targetClasses.Count; i++)
				{
					EAISetAsTargetIfHurt.TargetClass targetClass = this.targetClasses[i];
					if (targetClass.type != null && targetClass.type.IsAssignableFrom(type))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			if (attackTarget != null && attackTarget.IsAlive() && base.RandomFloat < 0.66f)
			{
				this.theEntity.SetRevengeTarget(null);
				return false;
			}
			if (base.check(revengeTarget))
			{
				return true;
			}
			Vector3 vector = this.theEntity.position - revengeTarget.position;
			float searchRadius = EntityClass.list[this.theEntity.entityClass].SearchRadius;
			vector = revengeTarget.position + vector.normalized * (searchRadius * 0.35f);
			Vector2 vector2 = this.manager.random.RandomInsideUnitCircle * searchRadius;
			vector.x += vector2.x;
			vector.z += vector2.y;
			Vector3i vector3i = World.worldToBlockPos(vector);
			int height = (int)this.theEntity.world.GetHeight(vector3i.x, vector3i.z);
			if (height > 0)
			{
				vector.y = (float)height;
			}
			int num = this.theEntity.CalcInvestigateTicks(1200, revengeTarget);
			this.theEntity.SetInvestigatePosition(vector, num, true);
			this.theEntity.SetRevengeTarget(null);
		}
		return false;
	}

	public override void Start()
	{
		this.theEntity.SetAttackTarget(this.theEntity.GetRevengeTarget(), 400);
		this.viewAngleSave = this.theEntity.GetMaxViewAngle();
		this.theEntity.SetMaxViewAngle(270f);
		this.viewAngleRestoreCounter = 100;
		base.Start();
	}

	public override void Update()
	{
		if (this.viewAngleRestoreCounter > 0)
		{
			this.viewAngleRestoreCounter--;
			if (this.viewAngleRestoreCounter == 0)
			{
				this.restoreViewAngle();
			}
		}
	}

	public override bool Continue()
	{
		return (!(this.theEntity.GetRevengeTarget() != null) || !(this.theEntity.GetAttackTarget() != this.theEntity.GetRevengeTarget())) && base.Continue();
	}

	public override void Reset()
	{
		base.Reset();
		this.restoreViewAngle();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void restoreViewAngle()
	{
		if (this.viewAngleSave > 0f)
		{
			this.theEntity.SetMaxViewAngle(this.viewAngleSave);
			this.viewAngleSave = 0f;
			this.viewAngleRestoreCounter = 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EAISetAsTargetIfHurt.TargetClass> targetClasses;

	[PublicizedFrom(EAccessModifier.Private)]
	public float viewAngleSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public int viewAngleRestoreCounter;

	[PublicizedFrom(EAccessModifier.Private)]
	public struct TargetClass
	{
		public Type type;
	}
}
