using System;
using UnityEngine;

public class FPS
{
	public FPS(float _restartTime)
	{
		this.restartTime = _restartTime;
		this.timeleft = _restartTime;
	}

	public bool Update()
	{
		this.timeleft -= Time.unscaledDeltaTime;
		this.accum += Time.unscaledDeltaTime;
		this.frames++;
		if ((double)this.timeleft <= 0.0)
		{
			this.Counter = (float)this.frames / this.accum;
			this.timeleft = this.restartTime;
			this.accum = 0f;
			this.frames = 0;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float accum;

	[PublicizedFrom(EAccessModifier.Private)]
	public int frames;

	[PublicizedFrom(EAccessModifier.Private)]
	public float timeleft;

	[PublicizedFrom(EAccessModifier.Private)]
	public float restartTime = 0.5f;

	public float Counter;
}
