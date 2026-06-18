using System;

public class ToolTipEvent
{
	public object Parameter { get; set; }

	public event ToolTipEventHandler EventHandler;

	public void HandleEvent()
	{
		ToolTipEventHandler eventHandler = this.EventHandler;
		if (eventHandler == null)
		{
			return;
		}
		eventHandler(this.Parameter);
	}
}
