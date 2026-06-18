using System;

public class GUIBlinker
{
	public GUIBlinker(float _ms)
	{
		this.ms = _ms;
	}

	public bool Draw(float _curTime)
	{
		if (_curTime - this.lastBlinkTime > this.ms)
		{
			this.lastBlinkTime = _curTime;
			this.bResult = !this.bResult;
		}
		return this.bResult;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float ms;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastBlinkTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bResult = true;
}
