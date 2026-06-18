using System;
using System.Net;
using System.Net.Sockets;

namespace Platform.LAN
{
	public class UdpClientSendHandler
	{
		public UdpClientSendHandler(UdpClient _udpClient)
		{
			this.udpClient = _udpClient;
		}

		public bool BeginSend(byte[] _message, int _length, IPEndPoint _endPoint)
		{
			try
			{
				this.isComplete = false;
				this.udpClient.BeginSend(_message, _length, _endPoint, new AsyncCallback(UdpClientSendHandler.CompleteSendAsync), this);
				return true;
			}
			catch (SocketException ex)
			{
				Log.Warning(string.Format("LAN send handler unable to start send. {0} ErrorCode: {1}", "SocketException", ex.ErrorCode));
			}
			catch (Exception ex2)
			{
				Log.Exception(ex2);
			}
			this.isComplete = true;
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void CompleteSendAsync(IAsyncResult _result)
		{
			((UdpClientSendHandler)_result.AsyncState).CompleteSend(_result);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CompleteSend(IAsyncResult _result)
		{
			try
			{
				this.udpClient.EndSend(_result);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException ex)
			{
				Log.Warning(string.Format("LAN send handler unable to complete send. {0} ErrorCode: {1}", "SocketException", ex.ErrorCode));
			}
			catch (Exception ex2)
			{
				Log.Exception(ex2);
			}
			this.isComplete = true;
		}

		public readonly UdpClient udpClient;

		public bool isComplete;
	}
}
