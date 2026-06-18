using System;
using UnityEngine;

public class AnimatorSpeedSetter : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.applyOnStateEnter)
		{
			animator.speed = this.animatorSpeed;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.applyOnStateExit)
		{
			animator.speed = this.animatorSpeed;
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	}

	public bool applyOnStateEnter = true;

	public bool applyOnStateExit;

	public float animatorSpeed = 1f;
}
