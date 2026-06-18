using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Webserver.UrlHandlers;

namespace Webserver.SSE
{
	public abstract class AbsEvent
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public AbsEvent(SseHandler _parent, bool _reuseEncodingBuffer = true, string _name = null)
		{
			this.Name = _name ?? base.GetType().Name;
			this.parent = _parent;
			if (_reuseEncodingBuffer)
			{
				this.encodingBuffer = new byte[1048576];
			}
		}

		public void AddListener(SseClient _client)
		{
			this.totalOpened++;
			this.currentlyOpen++;
			this.openClients.Add(_client);
			this.logConnectionState("Connection opened", _client);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void SendData(string _eventName, string _data)
		{
			this.sendQueue.Enqueue(new ValueTuple<string, string>(_eventName, _data));
			this.parent.SignalSendQueue();
		}

		public void ProcessSendQueue()
		{
			while (this.sendQueue.HasData())
			{
				ValueTuple<string, string> valueTuple = this.sendQueue.Dequeue();
				string item = valueTuple.Item1;
				string item2 = valueTuple.Item2;
				this.stringBuilder.Append("event: ");
				this.stringBuilder.AppendLine(item);
				this.stringBuilder.Append("data: ");
				this.stringBuilder.AppendLine(item2);
				this.stringBuilder.AppendLine("");
				string text = this.stringBuilder.ToString();
				this.stringBuilder.Clear();
				byte[] bytes;
				int num;
				if (this.encodingBuffer != null)
				{
					bytes = this.encodingBuffer;
					try
					{
						num = Encoding.UTF8.GetBytes(text, 0, text.Length, bytes, 0);
						goto IL_00DB;
					}
					catch (ArgumentException ex)
					{
						Log.Error("[Web] [SSE] '" + this.Name + "': Exception while encoding data for output, most likely exceeding buffer size:");
						Log.Exception(ex);
						break;
					}
					goto IL_00CA;
				}
				goto IL_00CA;
				IL_00DB:
				this.sendBufToListeners(bytes, num);
				continue;
				IL_00CA:
				bytes = Encoding.UTF8.GetBytes(text);
				num = bytes.Length;
				goto IL_00DB;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void sendBufToListeners(byte[] _bytes, int _bytesToSend)
		{
			for (int i = this.openClients.Count - 1; i >= 0; i--)
			{
				this.openClients[i].Write(_bytes, _bytesToSend);
			}
		}

		public virtual int DefaultPermissionLevel()
		{
			return 0;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void logConnectionState(string _message, SseClient _client)
		{
			Log.Out(string.Format("[Web] [SSE] '{0}': {1} from {2} (Left open: {3}, total opened: {4}, closed: {5})", new object[] { this.Name, _message, _client.RemoteEndpoint, this.currentlyOpen, this.totalOpened, this.totalClosed }));
		}

		public void ClientClosed(SseClient _client)
		{
			if (this.openClients.Remove(_client))
			{
				this.currentlyOpen--;
				this.totalClosed++;
				this.logConnectionState("Closed connection", _client);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int encodingBufferSize = 1048576;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly SseHandler parent;

		public readonly string Name;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly byte[] encodingBuffer;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly StringBuilder stringBuilder = new StringBuilder();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<SseClient> openClients = new List<SseClient>();

		[TupleElementNames(new string[] { "_eventName", "_data" })]
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly BlockingQueue<ValueTuple<string, string>> sendQueue = new BlockingQueue<ValueTuple<string, string>>();

		[PublicizedFrom(EAccessModifier.Private)]
		public int currentlyOpen;

		[PublicizedFrom(EAccessModifier.Private)]
		public int totalOpened;

		[PublicizedFrom(EAccessModifier.Private)]
		public int totalClosed;
	}
}
