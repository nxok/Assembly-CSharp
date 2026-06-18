using System;
using UnityEngine;

[PublicizedFrom(EAccessModifier.Internal)]
public class AnimatorStateTPVHolsterUnholster : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.GetComponentInParent<EntityAlive>() != null;
	}
}
