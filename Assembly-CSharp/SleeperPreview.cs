using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SleeperPreview : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.animator = base.GetComponent<Animator>();
	}

	public void SetPose(int pose)
	{
	}

	public void SetRotation(float rot)
	{
		base.transform.rotation = Quaternion.AngleAxis(rot, Vector3.up);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Animator animator;
}
