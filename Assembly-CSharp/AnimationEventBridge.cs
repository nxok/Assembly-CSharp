using System;
using Audio;
using UnityEngine;

public class AnimationEventBridge : RootTransformRefEntity
{
	public EntityAlive entity
	{
		get
		{
			if (!this._entity && this.RootTransform)
			{
				this._entity = this.RootTransform.GetComponent<EntityAlive>();
			}
			return this._entity;
		}
	}

	public void PlaySound(AnimationEvent ae)
	{
		this.playSound(ae.stringParameter, ae);
	}

	public void playSound(string name, AnimationEvent ae = null)
	{
		if (this.entity != null)
		{
			EntityPlayer entityPlayer = this.entity as EntityPlayer;
			if (entityPlayer != null && entityPlayer.IsReloadCancelled())
			{
				return;
			}
			this.entity.PlayOneShot(name, false, true, false, ae, 1f);
		}
	}

	public void PlayWeaponSound(AnimationEvent ae)
	{
		EntityPlayer entityPlayer = this.entity as EntityPlayer;
		if (entityPlayer != null && entityPlayer.inventory.holdingItem.HoldType.Value == ae.intParameter)
		{
			this.playSound(ae.stringParameter, ae);
		}
	}

	public void PlayLocalSound(string name)
	{
		this.playSound(name, null);
	}

	public void PlayStepSound(AnimationEvent _animEvent)
	{
		if (this.entity && _animEvent.animatorClipInfo.weight >= 0.3f)
		{
			float time = Time.time;
			if (time - this.lastTimeStepPlayed >= 0.1f)
			{
				this.lastTimeStepPlayed = time;
				float num = _animEvent.floatParameter;
				if (num == 0f)
				{
					num = 1f;
				}
				this.entity.PlayStepSound(num);
			}
		}
	}

	public void DeathImpactLight()
	{
		if (this.entity != null && this.entity.IsDead())
		{
			Manager.Play(this.entity, "impactbodylight", 1f, false);
		}
	}

	public void DeathImpactHeavy()
	{
		if (this.entity != null && this.entity.IsDead())
		{
			Manager.Play(this.entity, "impactbodyheavy", 1f, false);
		}
	}

	public void HideHoldingItem()
	{
		EntityAlive entity = this.entity;
		if (entity && entity.emodel)
		{
			Transform holdingItemTransform = entity.inventory.GetHoldingItemTransform();
			if (holdingItemTransform)
			{
				holdingItemTransform.gameObject.SetActive(false);
			}
		}
	}

	public void ShowHoldingItem()
	{
		EntityAlive entity = this.entity;
		if (entity && entity.emodel)
		{
			Transform holdingItemTransform = entity.inventory.GetHoldingItemTransform();
			if (holdingItemTransform)
			{
				holdingItemTransform.gameObject.SetActive(true);
			}
		}
	}

	public void Hit()
	{
		Entity entity = this.entity;
		if (entity && entity.emodel && entity.emodel.avatarController)
		{
			entity.emodel.avatarController.SetAttackImpact();
		}
	}

	public void ConsumeComplete()
	{
		EntityPlayerLocal entityPlayerLocal = this.entity as EntityPlayerLocal;
		if (entityPlayerLocal != null)
		{
			ItemClass holdingItem = entityPlayerLocal.inventory.holdingItem;
			for (int i = 0; i < holdingItem.Actions.Length; i++)
			{
				ItemActionEat itemActionEat = holdingItem.Actions[i] as ItemActionEat;
				if (itemActionEat != null && itemActionEat.PercentDone(entityPlayerLocal.inventory.holdingItemData.actionData[i]) > 0f)
				{
					itemActionEat.Completed(entityPlayerLocal.inventory.holdingItemData.actionData[i]);
					return;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		EntityAlive entity = this.entity;
		if (entity != null && entity.emodel != null)
		{
			entity.emodel.avatarController.SetCrouching(entity.IsCrouching);
			entity.SetVehiclePoseMode(entity.vehiclePoseMode);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityAlive _entity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastTimeStepPlayed;
}
