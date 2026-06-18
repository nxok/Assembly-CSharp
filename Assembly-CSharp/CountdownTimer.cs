using System;

public class CountdownTimer
{
	public long ElapsedMilliseconds
	{
		get
		{
			return (long)this.Elapsed.TotalMilliseconds;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
		}
	}

	public CountdownTimer(float _seconds, bool _start = true)
	{
		this.ms = (long)((int)(_seconds * 1000f));
		this.IsRunning = _start;
		this.offset = 0L;
		if (this.IsRunning)
		{
			this.ResetAndRestart();
			return;
		}
		this.Reset();
	}

	public void SetTimeout(float _seconds)
	{
		this.ms = (long)((int)(_seconds * 1000f));
	}

	public bool HasPassed()
	{
		bool flag = false;
		if (this.IsRunning)
		{
			this.Update();
			flag = ((this.offset == 0L) ? (this.ElapsedMilliseconds > this.ms) : (this.ElapsedMilliseconds + this.offset > this.ms));
			if (flag)
			{
				this.offset = 0L;
			}
		}
		return flag;
	}

	public void SetPassedIn(float _seconds)
	{
		this.offset = (long)((float)this.ms - _seconds * 1000f) - this.ElapsedMilliseconds;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		this.Elapsed = DateTime.Now.Subtract(this.StartTime);
	}

	public void Reset()
	{
		this.Elapsed = TimeSpan.Zero;
		this.StartTime = DateTime.Now;
		this.IsRunning = false;
	}

	public void ResetAndRestart()
	{
		this.Reset();
		this.IsRunning = true;
	}

	public void Stop()
	{
		this.IsRunning = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public long ms;

	[PublicizedFrom(EAccessModifier.Private)]
	public long offset;

	public TimeSpan Elapsed;

	public bool IsRunning;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime StartTime;
}
