using System;
using UnityEngine;

public class GameTimer
{
	public static GameTimer Instance
	{
		get
		{
			if (GameTimer.m_Instance == null)
			{
				GameTimer.m_Instance = new GameTimer(20f);
			}
			return GameTimer.m_Instance;
		}
	}

	public GameTimer(float _t)
	{
		this.ticksPerSecond = _t;
		this.ms = new MicroStopwatch();
		this.Reset(0UL);
	}

	public void Reset(ulong _ticks = 0UL)
	{
		this.elapsedPartialTicks = 0f;
		this.ticks = _ticks;
		this.ticksSincePlayfieldLoaded = 0UL;
		this.elapsedTicksD = 0.0;
		this.lastMillis = 0L;
		this.ms.ResetAndRestart();
	}

	public void updateTimer(bool _bServerIsStopped)
	{
		if (_bServerIsStopped)
		{
			this.Reset(this.ticks);
			return;
		}
		long elapsedMilliseconds = this.ms.ElapsedMilliseconds;
		long num = elapsedMilliseconds - this.lastMillis;
		this.lastMillis = elapsedMilliseconds;
		this.elapsedTicksD += (double)(Time.timeScale * (float)num) / 1000.0 * (double)this.ticksPerSecond;
		this.elapsedTicks = (int)this.elapsedTicksD;
		this.elapsedPartialTicks = (float)(this.elapsedTicksD - (double)this.elapsedTicks);
		this.elapsedTicksD -= (double)this.elapsedTicks;
		this.ticks += (ulong)((long)this.elapsedTicks);
		this.ticksSincePlayfieldLoaded += (ulong)((long)this.elapsedTicks);
	}

	public ulong ticks;

	public ulong ticksSincePlayfieldLoaded;

	public int elapsedTicks;

	public float elapsedPartialTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public double elapsedTicksD;

	[PublicizedFrom(EAccessModifier.Private)]
	public float ticksPerSecond;

	[PublicizedFrom(EAccessModifier.Private)]
	public long lastMillis;

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameTimer m_Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public MicroStopwatch ms;
}
