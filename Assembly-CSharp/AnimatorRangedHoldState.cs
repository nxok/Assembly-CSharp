using System;
using Audio;
using UnityEngine;

public class AnimatorRangedHoldState : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		EntityAlive componentInParent = animator.GetComponentInParent<EntityAlive>();
		if (componentInParent == null)
		{
			return;
		}
		componentInParent.emodel.avatarController.UpdateInt("CurrentAnim", 0, true);
		this.actionData = componentInParent.inventory.holdingItemData.actionData[0] as ItemActionRanged.ItemActionDataRanged;
		if (this.actionData != null)
		{
			this.itemClass = this.actionData.invData.itemValue.ItemClass;
			if (this.itemClass != null && this.itemClass.Properties.Values.ContainsKey(ItemClass.PropSoundIdle))
			{
				if (this.actionData.invData.itemValue.Meta > 0)
				{
					Manager.Play(this.actionData.invData.holdingEntity, this.itemClass.Properties.Values[ItemClass.PropSoundIdle], 1f, false);
					this.actionData.invData.holdingEntitySoundID = 0;
					return;
				}
				Manager.Stop(this.actionData.invData.holdingEntity.entityId, this.itemClass.Properties.Values[ItemClass.PropSoundIdle]);
				this.actionData.invData.holdingEntitySoundID = -1;
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.itemClass != null && this.itemClass.Properties.Values.ContainsKey(ItemClass.PropSoundIdle))
		{
			if (this.actionData.invData.holdingEntitySoundID != -1 && this.actionData.invData.itemValue.Meta == 0)
			{
				Manager.Stop(this.actionData.invData.holdingEntity.entityId, this.itemClass.Properties.Values[ItemClass.PropSoundIdle]);
				this.actionData.invData.holdingEntitySoundID = -1;
				return;
			}
			if (this.actionData.invData.holdingEntitySoundID == -1 && this.actionData.invData.itemValue.Meta > 0)
			{
				Manager.Play(this.actionData.invData.holdingEntity, this.itemClass.Properties.Values[ItemClass.PropSoundIdle], 1f, false);
				this.actionData.invData.holdingEntitySoundID = 0;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ItemActionRanged.ItemActionDataRanged actionData;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ItemClass itemClass;
}
