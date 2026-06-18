using System;

public class TimerEventData
{
	public event TimerEventHandler FullTimeFinishEvent;

	public event TimerEventHandler CloseEvent;

	public event TimerEventHandler AlternateEvent;

	public void HandleFullTimeFinished()
	{
		TimerEventHandler fullTimeFinishEvent = this.FullTimeFinishEvent;
		if (fullTimeFinishEvent == null)
		{
			return;
		}
		fullTimeFinishEvent(this);
	}

	public void HandleAlternateEvent()
	{
		TimerEventHandler alternateEvent = this.AlternateEvent;
		if (alternateEvent == null)
		{
			return;
		}
		alternateEvent(this);
	}

	public void HandleClosed()
	{
		TimerEventHandler closeEvent = this.CloseEvent;
		if (closeEvent == null)
		{
			return;
		}
		closeEvent(this);
	}

	public object Data;

	public bool CancelWithActivateButton;

	public bool CloseOnHit;

	public float AlternateTime = -1f;

	public float Completion = -1f;
}
