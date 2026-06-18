using System;
using UnityEngine;

public class FirstPersonAnimator : BodyAnimator
{
	public FirstPersonAnimator(EntityAlive _entity, AvatarCharacterController.AnimationStates _animStates, Transform _bodyTransform, BodyAnimator.EnumState _defaultState)
	{
		BodyAnimator.BodyParts bodyParts = new BodyAnimator.BodyParts(_bodyTransform, _bodyTransform.FindInChilds((_entity.emodel is EModelSDCS) ? "RightWeapon" : "Gunjoint", false));
		base.initBodyAnimator(_entity, bodyParts, _defaultState);
	}

	public override void SetDrunk(float _numBeers)
	{
		Animator animator = base.Animator;
		if (animator)
		{
			animator.SetFloat("drunk", _numBeers);
		}
	}
}
