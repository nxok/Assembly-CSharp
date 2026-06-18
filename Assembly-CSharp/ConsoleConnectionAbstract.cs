using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ConsoleConnectionAbstract : IConsoleConnection
{
	public abstract void SendLines(List<string> _output);

	public abstract void SendLine(string _text);

	public abstract void SendLog(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime);

	public abstract string GetDescription();

	public void EnableLogLevel(LogType _type, bool _enable)
	{
		if (_enable)
		{
			this.enabledLogLevels.Add(_type);
			return;
		}
		this.enabledLogLevels.Remove(_type);
	}

	public bool IsLogLevelEnabled(LogType _type)
	{
		return this.enabledLogLevels.Contains(_type);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ConsoleConnectionAbstract()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<LogType> enabledLogLevels = new HashSet<LogType> { 3, 2, 0, 4, 1 };
}
