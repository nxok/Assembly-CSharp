using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Scripting;
using Utf8Json;

namespace Webserver.WebAPI.APIs
{
	[Preserve]
	public class LogApi : AbsRestApi
	{
		public LogApi()
			: base("Log")
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleRestGet(RequestContext _context)
		{
			int num;
			if (_context.QueryParameters["count"] == null || !int.TryParse(_context.QueryParameters["count"], out num))
			{
				num = 50;
			}
			if (num == 0)
			{
				num = 1;
			}
			if (num > 1000)
			{
				num = 1000;
			}
			if (num < -1000)
			{
				num = -1000;
			}
			int num2;
			if (_context.QueryParameters["firstLine"] == null || !int.TryParse(_context.QueryParameters["firstLine"], out num2))
			{
				num2 = ((num > 0) ? LogBuffer.Instance.OldestLine : LogBuffer.Instance.LatestLine);
			}
			JsonWriter jsonWriter;
			AbsRestApi.PrepareEnvelopedResult(out jsonWriter);
			jsonWriter.WriteRaw(LogApi.jsonKeyEntries);
			int num3;
			List<LogBuffer.LogEntry> range = LogBuffer.Instance.GetRange(ref num2, num, out num3);
			jsonWriter.WriteBeginArray();
			for (int i = 0; i < range.Count; i++)
			{
				LogBuffer.LogEntry logEntry = range[i];
				if (i > 0)
				{
					jsonWriter.WriteValueSeparator();
				}
				LogApi.WriteLogMessageObject(ref jsonWriter, logEntry);
			}
			jsonWriter.WriteEndArray();
			jsonWriter.WriteRaw(LogApi.jsonKeyFirstLine);
			jsonWriter.WriteInt32(num2);
			jsonWriter.WriteRaw(LogApi.jsonKeyLastLine);
			jsonWriter.WriteInt32(num3);
			jsonWriter.WriteEndObject();
			AbsRestApi.SendEnvelopedResult(_context, ref jsonWriter, HttpStatusCode.OK, null, null, null);
		}

		public static void WriteLogMessageObject(ref JsonWriter _writer, LogBuffer.LogEntry _logEntry)
		{
			_writer.WriteRaw(LogApi.jsonIdKey);
			_writer.WriteInt32(_logEntry.MessageId);
			_writer.WriteRaw(LogApi.jsonMsgKey);
			_writer.WriteString(_logEntry.Message);
			_writer.WriteRaw(LogApi.jsonTypeKey);
			_writer.WriteString(_logEntry.Type.ToStringCached<LogType>());
			_writer.WriteRaw(LogApi.jsonTraceKey);
			_writer.WriteString(_logEntry.Trace);
			_writer.WriteRaw(LogApi.jsonIsotimeKey);
			_writer.WriteString(_logEntry.IsoTime);
			_writer.WriteRaw(LogApi.jsonUptimeKey);
			_writer.WriteString(_logEntry.Uptime.ToString());
			_writer.WriteEndObject();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int maxCount = 1000;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyEntries = JsonWriter.GetEncodedPropertyNameWithBeginObject("entries");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyFirstLine = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("firstLine");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonKeyLastLine = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("lastLine");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonIdKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("id");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonMsgKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("msg");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonTypeKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("type");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonTraceKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("trace");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonIsotimeKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("isotime");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonUptimeKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("uptime");
	}
}
