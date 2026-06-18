using System;
using UnityEngine.Scripting;
using Utf8Json;
using Webserver.UrlHandlers;
using Webserver.WebAPI.APIs;

namespace Webserver.SSE
{
	[Preserve]
	public class EventLog : AbsEvent
	{
		public EventLog(SseHandler _parent)
			: base(_parent, true, "log")
		{
			LogBuffer.EntryAdded += this.LogCallback;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void LogCallback(LogBuffer.LogEntry _logEntry)
		{
			JsonWriter jsonWriter = default(JsonWriter);
			LogApi.WriteLogMessageObject(ref jsonWriter, _logEntry);
			base.SendData("logLine", jsonWriter.ToString());
		}
	}
}
