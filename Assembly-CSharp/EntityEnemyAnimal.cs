using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityEnemyAnimal : EntityEnemy
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		if (this.ModelTransform)
		{
			this.animator = this.ModelTransform.GetComponentInChildren<Animator>();
		}
	}

	public override Color GetMapIconColor()
	{
		return new Color(1f, 0.8235294f, 0.34117648f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float getNextStepSoundDistance()
	{
		return 0.8f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isGameMessageOnDeath()
	{
		return false;
	}

	public override bool CanDamageEntity(int _sourceEntityId)
	{
		Entity entity = this.world.GetEntity(_sourceEntityId);
		return !entity || entity.entityClass != this.entityClass;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTasks()
	{
		if (this.Electrocuted)
		{
			base.SetMoveForward(0f);
			if (this.animator)
			{
				this.animator.enabled = false;
			}
			return;
		}
		if (this.animator)
		{
			this.animator.enabled = true;
		}
		base.updateTasks();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Animator animator;
}
