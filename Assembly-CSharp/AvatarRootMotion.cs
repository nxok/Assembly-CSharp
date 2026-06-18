using System;
using UnityEngine;

public class AvatarRootMotion : MonoBehaviour
{
	public void Init(AvatarController _mainController, Animator _root)
	{
		this.mainController = _mainController;
		this.root = _root;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnAnimatorMove()
	{
		if (this.mainController != null && this.root != null)
		{
			this.mainController.NotifyAnimatorMove(this.root);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AvatarController mainController;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Animator root;
}
