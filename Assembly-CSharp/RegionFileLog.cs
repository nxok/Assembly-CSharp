using System;
using System.Diagnostics;

public static class RegionFileLog
{
	[Conditional("DEBUG_REGIONLOG")]
	public static void Region(string _format = "", params object[] _args)
	{
		_format = string.Format("{0} Region {1}", GameManager.frameCount, _format);
		Log.Warning(_format, _args);
	}
}
