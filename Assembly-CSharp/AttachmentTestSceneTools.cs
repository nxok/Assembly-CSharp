using System;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentTestSceneTools : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.animator = base.GetComponent<Animator>();
		if (this.animator != null)
		{
			AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(this.animator.runtimeAnimatorController);
			List<KeyValuePair<AnimationClip, AnimationClip>> list = new List<KeyValuePair<AnimationClip, AnimationClip>>();
			foreach (AnimationClip animationClip in animatorOverrideController.animationClips)
			{
				list.Add(new KeyValuePair<AnimationClip, AnimationClip>(animationClip, this.anim));
			}
			animatorOverrideController.ApplyOverrides(list);
			this.animator.runtimeAnimatorController = animatorOverrideController;
		}
		if (this.attached != null)
		{
			this.attached = Object.Instantiate<GameObject>(this.prefabAttachment);
			this.attached.transform.parent = this.attachPoint.transform;
			this.attached.transform.localPosition = Vector3.zero;
			this.attached.transform.localEulerAngles = Vector3.zero;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Input.GetKey(97))
		{
			base.transform.Rotate(0f, this.turnRate * Time.deltaTime * -1f, 0f);
		}
		if (Input.GetKey(100))
		{
			base.transform.Rotate(0f, this.turnRate * Time.deltaTime, 0f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Animator animator;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int layerIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int maxLayers;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float turnRate = 600f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int totalModels;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int totalBodyParts;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int randomMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float targetLayerWeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float endLayerWeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int currentModel;

	public AnimationClip anim;

	public GameObject attachPoint;

	public GameObject prefabAttachment;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject attached;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Renderer meshRenderer;
}
