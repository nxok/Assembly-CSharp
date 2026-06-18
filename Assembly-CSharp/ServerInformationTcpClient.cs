using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public static class ServerInformationTcpClient
{
	public static void RequestRules(GameServerInfo _gsi, bool _ignoreTimeouts, ServerInformationTcpClient.RulesRequestDone _callback)
	{
		ThreadManager.StartCoroutine(ServerInformationTcpClient.RequestRulesTcpCo(_gsi, _ignoreTimeouts, _callback));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator RequestRulesTcpCo(GameServerInfo _gsi, bool _ignoreTimeoutsAndRefusedConnections, ServerInformationTcpClient.RulesRequestDone _callback)
	{
		string ip = _gsi.GetValue(GameInfoString.IP);
		int port = _gsi.GetValue(GameInfoInt.Port);
		CountdownTimer timeout = new CountdownTimer(3f, true);
		TcpClient c = new TcpClient();
		Task connectAsync;
		try
		{
			connectAsync = c.ConnectAsync(ip, port);
			goto IL_00E4;
		}
		catch (Exception ex)
		{
			Log.Warning(string.Format("NET: Requesting rules from TCP ({0}:{1}) failed due to connection problems ({2})", ip, port, ex.Message));
			_callback(false, Localization.Get("netNoServerInformation", false, null), _gsi);
			yield break;
		}
		IL_00CD:
		yield return null;
		IL_00E4:
		if (!connectAsync.IsCompleted && !timeout.HasPassed())
		{
			goto IL_00CD;
		}
		if (timeout.HasPassed() && !connectAsync.IsCompleted)
		{
			c.Close();
			c.Dispose();
			if (!_ignoreTimeoutsAndRefusedConnections)
			{
				Log.Warning(string.Format("NET: Requesting rules from TCP ({0}:{1}) failed due to connection problems (Timeout)", ip, port));
			}
			_callback(false, Localization.Get("netNoServerInformation", false, null), _gsi);
			yield break;
		}
		if (connectAsync.IsFaulted)
		{
			AggregateException exception = connectAsync.Exception;
			Exception ex2 = ((exception != null) ? exception.InnerException : null);
			if (_ignoreTimeoutsAndRefusedConnections)
			{
				SocketException ex3 = ex2 as SocketException;
				if (ex3 != null && ex3.SocketErrorCode == SocketError.ConnectionRefused)
				{
					goto IL_0216;
				}
			}
			string text = ((ex2 != null) ? ex2.Message.Replace("\r\n", " ").Replace("\n", " ") : null);
			Log.Warning(string.Format("NET: Requesting rules from TCP ({0}:{1}) failed due to connection problems ({2})", ip, port, text));
			IL_0216:
			_callback(false, Localization.Get("netNoServerInformation", false, null), _gsi);
			c.Close();
			c.Dispose();
			yield break;
		}
		NetworkStream ns = c.GetStream();
		byte[] buf = MemoryPools.poolByte.Alloc(32768);
		int size = 0;
		int received = 0;
		bool legacyFormat = false;
		ServerInformationTcpClient.EGameServerInfoReadState state = ServerInformationTcpClient.EGameServerInfoReadState.Size1;
		while (!timeout.HasPassed() && state != ServerInformationTcpClient.EGameServerInfoReadState.Done && state != ServerInformationTcpClient.EGameServerInfoReadState.Error)
		{
			if (ns.CanRead)
			{
				while (ns.DataAvailable)
				{
					int num = ns.ReadByte();
					if (num < 0)
					{
						state = ServerInformationTcpClient.EGameServerInfoReadState.Error;
					}
					switch (state)
					{
					case ServerInformationTcpClient.EGameServerInfoReadState.Size1:
						if (num < 48 || num > 57)
						{
							legacyFormat = true;
							size = num << 8;
						}
						else
						{
							size = (num - 48) * 10000;
						}
						state = ServerInformationTcpClient.EGameServerInfoReadState.Size2;
						break;
					case ServerInformationTcpClient.EGameServerInfoReadState.Size2:
						if (legacyFormat)
						{
							size += num;
							state = ((size > 0) ? ServerInformationTcpClient.EGameServerInfoReadState.Data : ServerInformationTcpClient.EGameServerInfoReadState.Done);
						}
						else
						{
							size += (num - 48) * 1000;
							state = ServerInformationTcpClient.EGameServerInfoReadState.Size3;
						}
						break;
					case ServerInformationTcpClient.EGameServerInfoReadState.Size3:
						size += (num - 48) * 100;
						state = ServerInformationTcpClient.EGameServerInfoReadState.Size4;
						break;
					case ServerInformationTcpClient.EGameServerInfoReadState.Size4:
						size += (num - 48) * 10;
						state = ServerInformationTcpClient.EGameServerInfoReadState.Size5;
						break;
					case ServerInformationTcpClient.EGameServerInfoReadState.Size5:
						size += num - 48;
						state = ServerInformationTcpClient.EGameServerInfoReadState.Break1;
						break;
					case ServerInformationTcpClient.EGameServerInfoReadState.Break1:
						state = ServerInformationTcpClient.EGameServerInfoReadState.Break2;
						break;
					case ServerInformationTcpClient.EGameServerInfoReadState.Break2:
						state = ((size > 0) ? ServerInformationTcpClient.EGameServerInfoReadState.Data : ServerInformationTcpClient.EGameServerInfoReadState.Done);
						break;
					case ServerInformationTcpClient.EGameServerInfoReadState.Data:
					{
						buf[received] = (byte)num;
						int num2 = received;
						received = num2 + 1;
						if (received >= size)
						{
							state = ServerInformationTcpClient.EGameServerInfoReadState.Done;
						}
						break;
					}
					}
				}
			}
			else
			{
				state = ServerInformationTcpClient.EGameServerInfoReadState.Error;
			}
			yield return null;
		}
		long elapsedMilliseconds = timeout.ElapsedMilliseconds;
		switch (state)
		{
		case ServerInformationTcpClient.EGameServerInfoReadState.Size1:
		case ServerInformationTcpClient.EGameServerInfoReadState.Size2:
		case ServerInformationTcpClient.EGameServerInfoReadState.Size3:
		case ServerInformationTcpClient.EGameServerInfoReadState.Size4:
		case ServerInformationTcpClient.EGameServerInfoReadState.Size5:
		case ServerInformationTcpClient.EGameServerInfoReadState.Break1:
		case ServerInformationTcpClient.EGameServerInfoReadState.Break2:
		case ServerInformationTcpClient.EGameServerInfoReadState.Data:
			Log.Warning(string.Format("NET: Requesting rules from TCP ({0}:{1}) timed out", ip, port));
			_callback(false, Localization.Get("netLiteNetLibDisconnectReason_Timeout", false, null), _gsi);
			break;
		case ServerInformationTcpClient.EGameServerInfoReadState.Done:
		{
			GameServerInfo gameServerInfo = new GameServerInfo(Encoding.UTF8.GetString(buf, 0, received));
			_gsi.Merge(gameServerInfo, EServerRelationType.Internet);
			_gsi.SetValue(GameInfoInt.Ping, (int)elapsedMilliseconds);
			_callback(true, null, _gsi);
			break;
		}
		case ServerInformationTcpClient.EGameServerInfoReadState.Error:
			Log.Warning(string.Format("NET: Requesting rules from TCP ({0}:{1}) failed", ip, port));
			_callback(false, Localization.Get("netRequestingServerInformationFailed", false, null), _gsi);
			break;
		}
		MemoryPools.poolByte.Free(buf);
		c.Close();
		c.Dispose();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EGameServerInfoReadState
	{
		Size1,
		Size2,
		Size3,
		Size4,
		Size5,
		Break1,
		Break2,
		Data,
		Done,
		Error
	}

	public delegate void RulesRequestDone(bool _success, string _message, GameServerInfo _gsi);
}
