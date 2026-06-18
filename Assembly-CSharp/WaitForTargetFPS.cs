using System;
using System.Threading;
using UnityEngine;

public class WaitForTargetFPS : MonoBehaviour
{
	public int TargetFPS
	{
		get
		{
			return this.m_targetFPS;
		}
		set
		{
			if (value != this.m_targetFPS)
			{
				this.m_targetFPS = value;
				this.timePerFrame = 1f / (float)this.m_targetFPS;
				base.enabled = this.m_targetFPS > 0;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		this.timePerFrame = 1f / (float)this.TargetFPS;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void LateUpdate()
	{
		float num = 0f;
		float num2 = Time.realtimeSinceStartup - this.lastUpdateTime;
		this.lastUpdateTime = Time.realtimeSinceStartup;
		if (!this.SkipSleepThisFrame)
		{
			float num3 = num2 - this.sleepLastFrame;
			if (num3 < this.timePerFrame)
			{
				num = Math.Min(this.timePerFrame - num3, 1f);
				Thread.Sleep((int)(num * 1000f));
			}
		}
		this.sleepLastFrame = num;
		this.SkipSleepThisFrame = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int m_targetFPS = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timePerFrame;

	public bool SkipSleepThisFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float sleepLastFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastUpdateTime;
}
