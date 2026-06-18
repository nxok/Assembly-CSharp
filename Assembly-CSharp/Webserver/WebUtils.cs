using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using SpaceWizards.HttpListener;
using UnityEngine.Profiling;
using Utf8Json;
using Webserver.WebAPI;

namespace Webserver
{
	public static class WebUtils
	{
		[PublicizedFrom(EAccessModifier.Private)]
		static WebUtils()
		{
			JsonWriter jsonWriter = default(JsonWriter);
			jsonWriter.WriteBeginArray();
			jsonWriter.WriteEndArray();
			WebUtils.JsonEmptyData = jsonWriter.ToUtf8ByteArray();
		}

		public static void WriteText(HttpListenerResponse _resp, string _text, HttpStatusCode _statusCode = HttpStatusCode.OK, string _mimeType = null)
		{
			_resp.StatusCode = (int)_statusCode;
			_resp.ContentType = _mimeType ?? "text/plain";
			_resp.ContentEncoding = Encoding.UTF8;
			if (string.IsNullOrEmpty(_text))
			{
				_resp.ContentLength64 = 0L;
				return;
			}
			byte[] bytes = Encoding.UTF8.GetBytes(_text);
			_resp.ContentLength64 = (long)bytes.Length;
			_resp.OutputStream.Write(bytes, 0, bytes.Length);
		}

		public static bool IsSslRedirected(HttpListenerRequest _req)
		{
			string text = _req.Headers["X-Forwarded-Proto"];
			return !string.IsNullOrEmpty(text) && text.Equals("https", StringComparison.OrdinalIgnoreCase);
		}

		public static string GenerateGuid()
		{
			return Guid.NewGuid().ToString();
		}

		public static void WriteJsonData(HttpListenerResponse _resp, ref JsonWriter _jsonWriter, HttpStatusCode _statusCode = HttpStatusCode.OK)
		{
			ArraySegment<byte> buffer = _jsonWriter.GetBuffer();
			_resp.StatusCode = (int)_statusCode;
			_resp.ContentType = "application/json";
			_resp.ContentEncoding = Encoding.UTF8;
			_resp.ContentLength64 = (long)buffer.Count;
			_resp.OutputStream.Write(buffer.Array, 0, buffer.Count);
		}

		public static void PrepareEnvelopedResult(out JsonWriter _writer)
		{
			_writer = default(JsonWriter);
			_writer.WriteRaw(WebUtils.jsonRawDataKey);
		}

		public static void SendEnvelopedResult(RequestContext _context, ref JsonWriter _writer, HttpStatusCode _statusCode = HttpStatusCode.OK, byte[] _jsonInputData = null, string _errorCode = null, Exception _exception = null)
		{
			_writer.WriteRaw(WebUtils.jsonRawMetaKey);
			_writer.WriteRaw(WebUtils.jsonRawMetaServertimeKey);
			JsonCommons.WriteDateTime(ref _writer, DateTime.Now);
			if (!string.IsNullOrEmpty(_errorCode))
			{
				_writer.WriteRaw(WebUtils.jsonRawMetaRequestMethodKey);
				_writer.WriteString(_context.Request.HttpMethod);
				_writer.WriteRaw(WebUtils.jsonRawMetaRequestSubpathKey);
				_writer.WriteString(_context.RequestPath);
				_writer.WriteRaw(WebUtils.jsonRawMetaRequestBodyKey);
				if (_jsonInputData != null)
				{
					_writer.WriteRaw(_jsonInputData);
				}
				else
				{
					_writer.WriteNull();
				}
				_writer.WriteRaw(WebUtils.jsonRawMetaErrorCodeKey);
				_writer.WriteString(_errorCode);
				if (_exception != null)
				{
					_writer.WriteRaw(WebUtils.jsonRawMetaExceptionMessageKey);
					_writer.WriteString(_exception.Message);
					_writer.WriteRaw(WebUtils.jsonRawMetaExceptionTraceKey);
					_writer.WriteString(_exception.StackTrace);
				}
			}
			_writer.WriteEndObject();
			_writer.WriteEndObject();
			WebUtils.WriteJsonData(_context.Response, ref _writer, _statusCode);
		}

		public static void SendEmptyResponse(RequestContext _context, HttpStatusCode _statusCode = HttpStatusCode.OK, byte[] _jsonInputData = null, string _errorCode = null, Exception _exception = null)
		{
			JsonWriter jsonWriter;
			WebUtils.PrepareEnvelopedResult(out jsonWriter);
			jsonWriter.WriteRaw(WebUtils.JsonEmptyData);
			WebUtils.SendEnvelopedResult(_context, ref jsonWriter, _statusCode, _jsonInputData, _errorCode, _exception);
		}

		public static bool TryGetValue(this NameValueCollection _nameValueCollection, string _name, out string _result)
		{
			_result = _nameValueCollection[_name];
			return _result != null;
		}

		public const string MimePlain = "text/plain";

		public const string MimeHtml = "text/html";

		public const string MimeJson = "application/json";

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly CustomSampler envelopeBuildSampler = CustomSampler.Create("JSON_EnvelopeBuilding", false);

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly CustomSampler netWriteSampler = CustomSampler.Create("JSON_Write", false);

		public static readonly byte[] JsonEmptyData;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonRawDataKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("data");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonRawMetaKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("meta");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonRawMetaServertimeKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("serverTime");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonRawMetaRequestMethodKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("requestMethod");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonRawMetaRequestSubpathKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("requestSubpath");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonRawMetaRequestBodyKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("requestBody");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonRawMetaErrorCodeKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("errorCode");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonRawMetaExceptionMessageKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("exceptionMessage");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonRawMetaExceptionTraceKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("exceptionTrace");
	}
}
