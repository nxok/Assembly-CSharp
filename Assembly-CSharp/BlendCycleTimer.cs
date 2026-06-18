using System;

public class BlendCycleTimer
{
	public BlendCycleTimer(float inTime, float holdTime, float outTime)
	{
		this.m_inTime = inTime;
		this.m_outTime = outTime;
		this.m_holdTime = holdTime;
		this.m_time = 0f;
		this.m_dir = BlendCycleTimer.Dir.Done;
	}

	public void Tick(float dt)
	{
		this.m_blendTimer.Tick(dt);
		switch (this.m_dir)
		{
		case BlendCycleTimer.Dir.In:
			this.m_time += dt;
			if (this.m_time >= this.m_inTime)
			{
				this.m_dir = BlendCycleTimer.Dir.Hold;
				this.m_time = 0f;
				return;
			}
			break;
		case BlendCycleTimer.Dir.Hold:
			if (this.m_holdTime != -1f)
			{
				this.m_time += dt;
				if (this.m_time >= this.m_holdTime)
				{
					this.m_dir = BlendCycleTimer.Dir.Out;
					this.m_time = 0f;
					this.m_blendTimer.BlendTo(0f, this.m_outTime);
				}
			}
			break;
		case BlendCycleTimer.Dir.Out:
			this.m_time += dt;
			if (this.m_time >= this.m_outTime)
			{
				this.m_dir = BlendCycleTimer.Dir.Done;
				return;
			}
			break;
		case BlendCycleTimer.Dir.Done:
			break;
		default:
			return;
		}
	}

	public void FadeIn()
	{
		this.m_dir = BlendCycleTimer.Dir.In;
		this.m_time = this.Value * this.m_inTime;
		this.m_blendTimer.BlendTo(1f, this.m_inTime);
	}

	public void FadeOut()
	{
		this.m_dir = BlendCycleTimer.Dir.Out;
		this.m_time = (1f - this.Value) * this.m_outTime;
		this.m_blendTimer.BlendTo(0f, this.m_outTime);
	}

	public void Restart()
	{
		this.m_time = 0f;
		this.m_dir = BlendCycleTimer.Dir.In;
		this.m_blendTimer.BlendTo(0f, 0f);
		this.m_blendTimer.BlendTo(1f, this.m_inTime);
	}

	public BlendCycleTimer.Dir Direction
	{
		get
		{
			return this.m_dir;
		}
	}

	public float Value
	{
		get
		{
			return this.m_blendTimer.Value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlendTimer m_blendTimer = new BlendTimer(0f);

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_inTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_outTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_holdTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_time;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlendCycleTimer.Dir m_dir;

	public enum Dir
	{
		In,
		Hold,
		Out,
		Done
	}
}
