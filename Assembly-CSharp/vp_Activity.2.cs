using System;

public class vp_Activity<V> : vp_Activity
{
	public vp_Activity(string name)
		: base(name)
	{
	}

	public bool TryStart<T>(T argument)
	{
		if (this.m_Active)
		{
			return false;
		}
		this.m_Argument = argument;
		return base.TryStart(true);
	}
}
