using System;
using UnityEngine;

public class AnimationStateRagdoll : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		EntityAlive componentInParent = animator.GetComponentInParent<EntityAlive>();
		if (componentInParent != null)
		{
			componentInParent.BeginDynamicRagdoll(this.RagdollFlags, this.StunTime);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		EntityAlive componentInParent = animator.GetComponentInParent<EntityAlive>();
		if (componentInParent != null)
		{
			componentInParent.ActivateDynamicRagdoll();
		}
	}

	public DynamicRagdollFlags RagdollFlags = DynamicRagdollFlags.Active | DynamicRagdollFlags.RagdollOnFall | DynamicRagdollFlags.UseBoneVelocities;

	[Tooltip("Time period to stun")]
	public FloatRange StunTime = new FloatRange(1f, 1f);
}
