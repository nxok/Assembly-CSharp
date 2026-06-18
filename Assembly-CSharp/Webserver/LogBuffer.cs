using System;
using System.Collections.Generic;
using UnityEngine;

namespace Webserver
{
	public class LogBuffer
	{
		public static event Action<LogBuffer.LogEntry> EntryAdded;

		public static void Init()
		{
			if (LogBuffer.instance == null)
			{
				LogBuffer.instance = new LogBuffer();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public LogBuffer()
		{
			Log.LogCallbacksExtended += new Log.LogCallbackExtendedDelegate(this.LogCallback);
		}

		public static LogBuffer Instance
		{
			get
			{
				LogBuffer logBuffer;
				if ((logBuffer = LogBuffer.instance) == null)
				{
					logBuffer = (LogBuffer.instance = new LogBuffer());
				}
				return logBuffer;
			}
		}

		public int OldestLine
		{
			get
			{
				List<LogBuffer.LogEntry> list = this.logEntries;
				int num;
				lock (list)
				{
					num = this.listOffset;
				}
				return num;
			}
		}

		public int LatestLine
		{
			get
			{
				List<LogBuffer.LogEntry> list = this.logEntries;
				int num;
				lock (list)
				{
					num = this.listOffset + this.logEntries.Count - 1;
				}
				return num;
			}
		}

		public int StoredLines
		{
			get
			{
				List<LogBuffer.LogEntry> list = this.logEntries;
				int count;
				lock (list)
				{
					count = this.logEntries.Count;
				}
				return count;
			}
		}

		public LogBuffer.LogEntry this[int _index]
		{
			get
			{
				List<LogBuffer.LogEntry> list = this.logEntries;
				lock (list)
				{
					if (_index >= this.listOffset && _index < this.listOffset + this.logEntries.Count)
					{
						return this.logEntries[_index];
					}
				}
				return null;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void LogCallback(string _formattedMsg, string _plainMsg, string _trace, LogType _type, DateTime _timestamp, long _uptime)
		{
			List<LogBuffer.LogEntry> list = this.logEntries;
			lock (list)
			{
				LogBuffer.LogEntry logEntry = new LogBuffer.LogEntry(this.listOffset + this.logEntries.Count, _timestamp, _plainMsg, _trace, _type, _uptime);
				this.logEntries.Add(logEntry);
				Action<LogBuffer.LogEntry> entryAdded = LogBuffer.EntryAdded;
				if (entryAdded != null)
				{
					entryAdded(logEntry);
				}
				if (this.logEntries.Count > 3000)
				{
					this.listOffset += this.logEntries.Count - 3000;
					this.logEntries.RemoveRange(0, this.logEntries.Count - 3000);
				}
			}
		}

		public List<LogBuffer.LogEntry> GetRange(ref int _start, int _count, out int _end)
		{
			List<LogBuffer.LogEntry> list = this.logEntries;
			List<LogBuffer.LogEntry> range;
			lock (list)
			{
				int num;
				if (_count < 0)
				{
					_count = -_count;
					if (_start >= this.listOffset + this.logEntries.Count)
					{
						_start = this.listOffset + this.logEntries.Count - 1;
					}
					_end = _start;
					if (_start < this.listOffset)
					{
						return this.emptyList;
					}
					_start -= _count - 1;
					if (_start < this.listOffset)
					{
						_start = this.listOffset;
					}
					num = _start - this.listOffset;
					_end++;
					_count = _end - _start;
				}
				else
				{
					if (_start < this.listOffset)
					{
						_start = this.listOffset;
					}
					if (_start >= this.listOffset + this.logEntries.Count)
					{
						_end = _start;
						return this.emptyList;
					}
					num = _start - this.listOffset;
					if (num + _count > this.logEntries.Count)
					{
						_count = this.logEntries.Count - num;
					}
					_end = _start + _count;
				}
				range = this.logEntries.GetRange(num, _count);
			}
			return range;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int maxEntries = 3000;

		[PublicizedFrom(EAccessModifier.Private)]
		public static LogBuffer instance;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<LogBuffer.LogEntry> logEntries = new List<LogBuffer.LogEntry>();

		[PublicizedFrom(EAccessModifier.Private)]
		public int listOffset;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<LogBuffer.LogEntry> emptyList = new List<LogBuffer.LogEntry>();

		public class LogEntry
		{
			public LogEntry(int _messageId, DateTime _timestamp, string _message, string _trace, LogType _type, long _uptime)
			{
				this.MessageId = _messageId;
				this.Timestamp = _timestamp;
				this.IsoTime = _timestamp.ToString("o");
				this.Message = _message;
				this.Trace = _trace;
				this.Type = _type;
				this.Uptime = _uptime;
			}

			public readonly int MessageId;

			public readonly DateTime Timestamp;

			public readonly string IsoTime;

			public readonly string Message;

			public readonly string Trace;

			public readonly LogType Type;

			public readonly long Uptime;
		}
	}
}
