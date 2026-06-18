using System;
using System.Threading;
using UnityEngine;

public class FrameRateLimiter : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.MaxFrames < 4f)
		{
			this.MaxFrames = 4f;
		}
		if (this.MaxFrames < 60f)
		{
			int num = (int)(1000.0 / (double)this.MaxFrames - (double)(Time.deltaTime * 1000f));
			if (num > 0)
			{
				Thread.Sleep(num);
			}
		}
	}

	public float MaxFrames = 9999f;
}
