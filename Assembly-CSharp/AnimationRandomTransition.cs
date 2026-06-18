using System;
using UnityEngine;

public class AnimationRandomTransition : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.numberOfAnimations > 0)
		{
			int num = Random.Range(0, this.numberOfAnimations);
			animator.SetInteger(this.animationParameter, num);
		}
	}

	public string animationParameter = "RandomIndex";

	public int numberOfAnimations;
}
