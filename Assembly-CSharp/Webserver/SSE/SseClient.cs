using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SpaceWizards.HttpListener;
using Webserver.UrlHandlers;

namespace Webserver.SSE
{
	public class SseClient
	{
		public SseClient(SseHandler _parent, RequestContext _context)
		{
			this.parent = _parent;
			this.response = _context.Response;
			this.RemoteEndpoint = _context.Request.RemoteEndPoint;
			this.response.SendChunked = true;
			this.response.AddHeader("Content-Type", "text/event-stream");
			this.response.OutputStream.Flush();
		}

		public ESseClientWriteResult Write(byte[] _bytes, int _bytesToSend)
		{
			HttpListenerResponse httpListenerResponse = this.response;
			ESseClientWriteResult esseClientWriteResult;
			try
			{
				if (!httpListenerResponse.OutputStream.CanWrite)
				{
					this.parent.ClientClosed(this);
					httpListenerResponse.Close();
					esseClientWriteResult = ESseClientWriteResult.Closed;
				}
				else
				{
					httpListenerResponse.OutputStream.Write(_bytes, 0, _bytesToSend);
					httpListenerResponse.OutputStream.Flush();
					this.lastMessageSent = DateTime.Now;
					esseClientWriteResult = ESseClientWriteResult.Ok;
				}
			}
			catch (IOException ex)
			{
				this.parent.ClientClosed(this);
				SocketException ex2 = ex.InnerException as SocketException;
				if (ex2 != null)
				{
					if (ex2.SocketErrorCode == SocketError.ConnectionAborted || ex2.SocketErrorCode == SocketError.Shutdown)
					{
						esseClientWriteResult = ESseClientWriteResult.Closed;
					}
					else
					{
						Log.Error("[Web] [SSE] SocketError (" + ex2.SocketErrorCode.ToStringCached<SocketError>() + ") while trying to write", new object[] { true });
						esseClientWriteResult = ESseClientWriteResult.Error;
					}
				}
				else
				{
					Log.Error("[Web] [SSE] IOException while trying to write:", new object[] { true });
					Log.Exception(ex);
					esseClientWriteResult = ESseClientWriteResult.Error;
				}
			}
			catch (Exception ex3)
			{
				this.parent.ClientClosed(this);
				httpListenerResponse.Close();
				Log.Error("[Web] [SSE] Exception while trying to write:", new object[] { true });
				Log.Exception(ex3);
				esseClientWriteResult = ESseClientWriteResult.Error;
			}
			return esseClientWriteResult;
		}

		public void HandleKeepAlive()
		{
			DateTime now = DateTime.Now;
			if ((now - this.lastMessageSent).TotalSeconds < 10.0)
			{
				return;
			}
			this.Write(SseClient.keepAliveData, SseClient.keepAliveData.Length);
			this.lastMessageSent = now;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int keepAliveIntervalSeconds = 10;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly byte[] keepAliveData = Encoding.UTF8.GetBytes(": KeepAlive\n\n");

		public readonly IPEndPoint RemoteEndpoint;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly SseHandler parent;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HttpListenerResponse response;

		[PublicizedFrom(EAccessModifier.Private)]
		public DateTime lastMessageSent = DateTime.Now;
	}
}
