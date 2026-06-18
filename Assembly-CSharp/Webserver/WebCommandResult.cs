using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SpaceWizards.HttpListener;
using UnityEngine;
using Utf8Json;

namespace Webserver
{
	public class WebCommandResult : IConsoleConnection
	{
		public WebCommandResult(string _command, string _parameters, WebCommandResult.ResultType _responseType, RequestContext _context)
		{
			this.context = _context;
			this.command = _command;
			this.parameters = _parameters;
			this.responseType = _responseType;
			WebConnection connection = _context.Connection;
			this.sourceName = ((connection != null) ? connection.Username : null) ?? ("Unauth-PermLevel-" + _context.PermissionLevel.ToString());
		}

		public void SendLines(List<string> _output)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string text in _output)
			{
				stringBuilder.AppendLine(text);
			}
			string text2 = stringBuilder.ToString();
			try
			{
				if (this.responseType == WebCommandResult.ResultType.Raw)
				{
					WebUtils.WriteText(this.context.Response, text2, HttpStatusCode.OK, null);
				}
				else
				{
					JsonWriter jsonWriter;
					WebUtils.PrepareEnvelopedResult(out jsonWriter);
					if (this.responseType == WebCommandResult.ResultType.ResultOnly)
					{
						jsonWriter.WriteRaw(WebCommandResult.jsonRawKey);
						jsonWriter.WriteString(text2);
						jsonWriter.WriteEndObject();
					}
					else
					{
						jsonWriter.WriteRaw(WebCommandResult.jsonCommandKey);
						jsonWriter.WriteString(this.command);
						jsonWriter.WriteRaw(WebCommandResult.jsonParametersKey);
						jsonWriter.WriteString(this.parameters);
						jsonWriter.WriteRaw(WebCommandResult.jsonResultKey);
						jsonWriter.WriteString(text2);
						jsonWriter.WriteEndObject();
					}
					WebUtils.SendEnvelopedResult(this.context, ref jsonWriter, HttpStatusCode.OK, null, null, null);
				}
			}
			catch (IOException ex)
			{
				if (ex.InnerException is SocketException)
				{
					Log.Warning("[Web] Error in WebCommandResult.SendLines(): Remote host closed connection: " + ex.InnerException.Message);
				}
				else
				{
					Log.Warning(string.Format("[Web] Error (IO) in WebCommandResult.SendLines(): {0}", ex));
				}
			}
			catch (Exception ex2)
			{
				Log.Warning(string.Format("[Web] Error in WebCommandResult.SendLines(): {0}", ex2));
			}
			finally
			{
				RequestContext requestContext = this.context;
				if (requestContext != null)
				{
					HttpListenerResponse response = requestContext.Response;
					if (response != null)
					{
						response.Close();
					}
				}
			}
		}

		public void SendLine(string _text)
		{
		}

		public void SendLog(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime)
		{
		}

		public void EnableLogLevel(LogType _type, bool _enable)
		{
		}

		public string GetDescription()
		{
			return "WebCommandResult_for_" + this.command + "_by_" + this.sourceName;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string command;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string parameters;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string sourceName;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly RequestContext context;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WebCommandResult.ResultType responseType;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonRawKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("resultRaw");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonCommandKey = JsonWriter.GetEncodedPropertyNameWithBeginObject("command");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonParametersKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("parameters");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] jsonResultKey = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("result");

		public enum ResultType
		{
			Full,
			ResultOnly,
			Raw
		}
	}
}
