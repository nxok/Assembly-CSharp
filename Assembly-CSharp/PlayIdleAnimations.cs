using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Play Idle Animations")]
public class PlayIdleAnimations : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.mAnim = base.GetComponentInChildren<Animation>();
		if (this.mAnim == null)
		{
			Debug.LogWarning(NGUITools.GetHierarchy(base.gameObject) + " has no Animation component");
			Object.Destroy(this);
			return;
		}
		foreach (object obj in this.mAnim)
		{
			AnimationState animationState = (AnimationState)obj;
			if (animationState.clip.name == "idle")
			{
				animationState.layer = 0;
				this.mIdle = animationState.clip;
				this.mAnim.Play(this.mIdle.name);
			}
			else if (animationState.clip.name.StartsWith("idle"))
			{
				animationState.layer = 1;
				this.mBreaks.Add(animationState.clip);
			}
		}
		if (this.mBreaks.Count == 0)
		{
			Object.Destroy(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.mNextBreak < Time.time)
		{
			if (this.mBreaks.Count == 1)
			{
				AnimationClip animationClip = this.mBreaks[0];
				this.mNextBreak = Time.time + animationClip.length + Random.Range(5f, 15f);
				this.mAnim.CrossFade(animationClip.name);
				return;
			}
			int num = Random.Range(0, this.mBreaks.Count - 1);
			if (this.mLastIndex == num)
			{
				num++;
				if (num >= this.mBreaks.Count)
				{
					num = 0;
				}
			}
			this.mLastIndex = num;
			AnimationClip animationClip2 = this.mBreaks[num];
			this.mNextBreak = Time.time + animationClip2.length + Random.Range(2f, 8f);
			this.mAnim.CrossFade(animationClip2.name);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Animation mAnim;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AnimationClip mIdle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<AnimationClip> mBreaks = new List<AnimationClip>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float mNextBreak;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int mLastIndex;
}
