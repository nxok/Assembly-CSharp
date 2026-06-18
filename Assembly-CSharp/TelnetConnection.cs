using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class TelnetConnection : ConsoleConnectionAbstract
{
	public bool IsClosed
	{
		get
		{
			return this.closed;
		}
	}

	public bool IsAuthenticated
	{
		get
		{
			return !this.authEnabled || this.authenticated;
		}
	}

	public int EndPointHash { get; }

	public TelnetConnection(TelnetConsole _owner, TcpClient _client, int _addressHash, bool _authEnabled)
	{
		this.telnet = _owner;
		this.authEnabled = _authEnabled;
		this.client = _client;
		this.endpoint = _client.Client.RemoteEndPoint;
		this.EndPointHash = _addressHash;
		string text = "Telnet connection from: ";
		EndPoint endPoint = this.endpoint;
		Log.Out(text + ((endPoint != null) ? endPoint.ToString() : null));
		this.clientStream = _client.GetStream();
		this.reader = new StreamReader(this.clientStream, Encoding.UTF8);
		string text2 = "TelnetClient_";
		EndPoint endPoint2 = this.endpoint;
		ThreadManager.StartThread(text2 + ((endPoint2 != null) ? endPoint2.ToString() : null), null, new ThreadManager.ThreadFunctionLoopDelegate(this.HandlerThread), new ThreadManager.ThreadFunctionEndDelegate(this.ThreadEnd), null, null, false, false);
		if (_authEnabled)
		{
			this.toClientQueue.Enqueue("Please enter password:");
			return;
		}
		this.LoginMessage();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LoginMessage()
	{
		this.toClientQueue.Enqueue("*** Connected with 7DTD server.");
		this.toClientQueue.Enqueue("*** Server version: " + Constants.cVersionInformation.LongString + " Compatibility Version: " + Constants.cVersionInformation.LongStringNoBuild);
		this.toClientQueue.Enqueue(string.Empty);
		this.toClientQueue.Enqueue("Server IP:   " + (string.IsNullOrEmpty(GamePrefs.GetString(EnumGamePrefs.ServerIP)) ? "Any" : GamePrefs.GetString(EnumGamePrefs.ServerIP)));
		this.toClientQueue.Enqueue("Server port: " + GamePrefs.GetInt(EnumGamePrefs.ServerPort).ToString());
		this.toClientQueue.Enqueue("Max players: " + GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount).ToString());
		this.toClientQueue.Enqueue("Game mode:   " + GamePrefs.GetString(EnumGamePrefs.GameMode));
		this.toClientQueue.Enqueue("World:       " + GamePrefs.GetString(EnumGamePrefs.GameWorld));
		this.toClientQueue.Enqueue("Game name:   " + GamePrefs.GetString(EnumGamePrefs.GameName));
		this.toClientQueue.Enqueue("Difficulty:  " + GamePrefs.GetInt(EnumGamePrefs.GameDifficulty).ToString());
		this.toClientQueue.Enqueue(string.Empty);
		this.toClientQueue.Enqueue("Press 'help' to get a list of all commands. Press 'exit' to end session.");
		this.toClientQueue.Enqueue(string.Empty);
	}

	public bool ConnectionUsable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.client.Connected && !this.closed;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int HandlerThread(ThreadManager.ThreadInfo _tInfo)
	{
		if (!this.ConnectionUsable || this.closeConnection)
		{
			return -1;
		}
		try
		{
			if (!this.handleReading())
			{
				return -1;
			}
			this.handleWriting();
		}
		catch (IOException ex)
		{
			SocketException ex2 = ex.InnerException as SocketException;
			if (ex2 != null && ex2.SocketErrorCode == SocketError.ConnectionAborted)
			{
				string text = "Connection closed by host in TelnetClient_";
				EndPoint endPoint = this.endpoint;
				Log.Warning(text + ((endPoint != null) ? endPoint.ToString() : null));
				return -1;
			}
			string text2 = "IOException in TelnetClient_";
			EndPoint endPoint2 = this.endpoint;
			Log.Error(text2 + ((endPoint2 != null) ? endPoint2.ToString() : null) + ": " + ex.Message);
			Log.Exception(ex);
			return -1;
		}
		return 25;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool handleReading()
	{
		int num;
		while (this.ConnectionUsable && this.clientStream.CanRead && this.client.Available > 0 && (num = this.reader.Read(this.charBuffer, 0, this.charBuffer.Length)) > 0)
		{
			for (int i = 0; i < num; i++)
			{
				char c = this.charBuffer[i];
				if (c == '\r' || c == '\n')
				{
					if (!this.submitInput())
					{
						return false;
					}
				}
				else
				{
					this.readStringBuilder.Append(c);
				}
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool submitInput()
	{
		this.readStringBuilder.Trim();
		if (this.readStringBuilder.Length <= 0)
		{
			return true;
		}
		string text = this.readStringBuilder.ToString();
		if (!this.IsAuthenticated)
		{
			this.authenticate(text);
		}
		else
		{
			if (text.EqualsCaseInsensitive("exit"))
			{
				return false;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteAsync(text, this);
		}
		this.readStringBuilder.Length = 0;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void authenticate(string _line)
	{
		if (_line.Equals(GamePrefs.GetString(EnumGamePrefs.TelnetPassword)))
		{
			this.authenticated = true;
			this.toClientQueue.Enqueue("Logon successful.");
			this.toClientQueue.Enqueue(string.Empty);
			this.toClientQueue.Enqueue(string.Empty);
			this.toClientQueue.Enqueue(string.Empty);
			this.LoginMessage();
			return;
		}
		if (this.telnet.RegisterFailedLogin(this))
		{
			this.toClientQueue.Enqueue("Password incorrect, please enter password:");
			return;
		}
		this.toClientQueue.Enqueue("Too many failed login attempts!");
		this.closeConnection = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleWriting()
	{
		while (this.ConnectionUsable && this.clientStream.CanWrite && this.toClientQueue.HasData())
		{
			string text = this.toClientQueue.Dequeue();
			if (text == null)
			{
				this.clientStream.WriteByte(0);
			}
			else
			{
				int num;
				for (int i = 0; i < text.Length; i += num)
				{
					num = Math.Min(64, text.Length - i);
					int bytes = Encoding.UTF8.GetBytes(text, i, num, this.byteBuffer, 0);
					this.clientStream.Write(this.byteBuffer, 0, bytes);
				}
				this.clientStream.WriteByte(13);
				this.clientStream.WriteByte(10);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ThreadEnd(ThreadManager.ThreadInfo _threadInfo, bool _exitForException)
	{
		this.Close(false);
	}

	public void Close(bool _kickedForLogins = false)
	{
		if (this.closed)
		{
			return;
		}
		this.closed = true;
		this.toClientQueue.Close();
		if (this.client.Connected)
		{
			this.client.GetStream().Close();
			this.client.Close();
		}
		this.telnet.ConnectionClosed(this);
		if (_kickedForLogins)
		{
			string text = "Telnet connection closed for too many login attempts: ";
			EndPoint endPoint = this.endpoint;
			Log.Out(text + ((endPoint != null) ? endPoint.ToString() : null));
			return;
		}
		string text2 = "Telnet connection closed: ";
		EndPoint endPoint2 = this.endpoint;
		Log.Out(text2 + ((endPoint2 != null) ? endPoint2.ToString() : null));
	}

	public override void SendLine(string _line)
	{
		if (!this.closed && this.IsAuthenticated)
		{
			this.toClientQueue.Enqueue(_line);
			return;
		}
		this.toClientQueue.Enqueue(null);
	}

	public override void SendLines(List<string> _output)
	{
		for (int i = 0; i < _output.Count; i++)
		{
			this.SendLine(_output[i]);
		}
	}

	public override void SendLog(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime)
	{
		if (base.IsLogLevelEnabled(_type))
		{
			this.SendLine(_formattedMessage);
		}
	}

	public override string GetDescription()
	{
		string text;
		if ((text = this.cachedDescription) == null)
		{
			string text2 = "Telnet from ";
			EndPoint endPoint = this.endpoint;
			text = (this.cachedDescription = text2 + ((endPoint != null) ? endPoint.ToString() : null));
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MAX_CHARS_PER_CONVERSION = 64;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BlockingQueue<string> toClientQueue = new BlockingQueue<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly TelnetConsole telnet;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool authenticated;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool authEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly TcpClient client;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly EndPoint endpoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile bool closed;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedDescription;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly NetworkStream clientStream;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StreamReader reader;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringBuilder readStringBuilder = new StringBuilder();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly char[] charBuffer = new char[64];

	[PublicizedFrom(EAccessModifier.Private)]
	public bool closeConnection;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly byte[] byteBuffer = new byte[256];
}
