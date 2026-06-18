using System;
using UnityEngine;

public class AnimationStateRandomBlend : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		int integer = animator.GetInteger("RandomSelector");
		if (this.ChoiceCount > 0)
		{
			animator.SetFloat("RandomVariationQuantized", (float)(integer % this.ChoiceCount));
			return;
		}
		animator.SetFloat("RandomVariationQuantized", 0f);
	}

	[Tooltip("The number of options to randomly select from")]
	public int ChoiceCount = 1;
}
