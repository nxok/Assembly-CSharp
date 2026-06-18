using System;

public struct ServerDateTimeResult
{
	public bool RequestComplete
	{
		readonly get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool HasError
	{
		readonly get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int SecondsOffset
	{
		readonly get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public ServerDateTimeResult(bool _requestComplete, bool _hasError, int _secondsOffset)
	{
		this.RequestComplete = _requestComplete;
		this.HasError = _hasError;
		this.SecondsOffset = _secondsOffset;
	}
}
