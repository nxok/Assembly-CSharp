using System;
using System.Collections;
using System.Collections.Generic;
using Platform;
using UnityEngine;

public class ProtocolManager : IProtocolManagerProtocolInterface
{
	public bool HasRunningServers
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public ProtocolManager.NetworkType CurrentMode
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsServer
	{
		get
		{
			return this.CurrentMode == ProtocolManager.NetworkType.Server || this.CurrentMode == ProtocolManager.NetworkType.OfflineServer;
		}
	}

	public bool IsClient
	{
		get
		{
			return this.CurrentMode == ProtocolManager.NetworkType.Client;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupProtocols()
	{
		if (this.servers.Count == 0)
		{
			string @string = GamePrefs.GetString(EnumGamePrefs.ServerDisabledNetworkProtocols);
			List<string> list = new List<string>();
			if (!string.IsNullOrEmpty(@string))
			{
				list.AddRange(@string.ToLower().Split(',', StringSplitOptions.None));
			}
			if (GameUtils.GetLaunchArgument("nounet") != null)
			{
				list.Add("unet");
			}
			if (GameUtils.GetLaunchArgument("noraknet") != null)
			{
				list.Add("raknet");
			}
			if (GameUtils.GetLaunchArgument("nolitenetlib") != null)
			{
				list.Add("litenetlib");
			}
			if (!list.Contains("litenetlib"))
			{
				this.servers.Add(new NetworkServerLiteNetLib(this));
				this.clients.Add(new NetworkClientLiteNetLib(this));
			}
			else
			{
				Log.Out("[NET] Disabling protocol: LiteNetLib");
			}
			if (PlatformManager.NativePlatform.HasNetworkingEnabled(list))
			{
				this.servers.Add(PlatformManager.NativePlatform.GetNetworkingServer(this));
				if (!GameManager.IsDedicatedServer)
				{
					this.clients.Add(PlatformManager.NativePlatform.GetNetworkingClient(this));
				}
			}
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			if (crossplatformPlatform != null && crossplatformPlatform.HasNetworkingEnabled(list))
			{
				this.servers.Add(PlatformManager.CrossplatformPlatform.GetNetworkingServer(this));
				if (!GameManager.IsDedicatedServer)
				{
					this.clients.Add(PlatformManager.CrossplatformPlatform.GetNetworkingClient(this));
				}
			}
			foreach (KeyValuePair<EPlatformIdentifier, IPlatform> keyValuePair in PlatformManager.ServerPlatforms)
			{
				if (keyValuePair.Value.AsServerOnly && keyValuePair.Value.HasNetworkingEnabled(list))
				{
					this.servers.Add(keyValuePair.Value.GetNetworkingServer(this));
				}
			}
		}
	}

	public string GetGamePortsString()
	{
		string text = "";
		string serverPorts = ServerInformationTcpProvider.Instance.GetServerPorts();
		if (!string.IsNullOrEmpty(serverPorts))
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += ", ";
			}
			text += serverPorts;
		}
		IMasterServerAnnouncer serverListAnnouncer = PlatformManager.MultiPlatform.ServerListAnnouncer;
		string text2 = ((serverListAnnouncer != null) ? serverListAnnouncer.GetServerPorts() : null);
		if (!string.IsNullOrEmpty(text2))
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += ", ";
			}
			text += text2;
		}
		int @int = GamePrefs.GetInt(EnumGamePrefs.ServerPort);
		for (int i = 0; i < this.servers.Count; i++)
		{
			string serverPorts2 = this.servers[i].GetServerPorts(@int);
			if (!string.IsNullOrEmpty(serverPorts2))
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += ", ";
				}
				text += serverPorts2;
			}
		}
		return text;
	}

	public void Update()
	{
		for (int i = 0; i < this.servers.Count; i++)
		{
			this.servers[i].Update();
		}
		for (int j = 0; j < this.clients.Count; j++)
		{
			this.clients[j].Update();
		}
	}

	public void LateUpdate()
	{
		for (int i = 0; i < this.servers.Count; i++)
		{
			this.servers[i].LateUpdate();
		}
		for (int j = 0; j < this.clients.Count; j++)
		{
			this.clients[j].LateUpdate();
		}
	}

	public void StartOfflineServer()
	{
		Log.Out("NET: Starting offline server.");
		this.CurrentMode = ProtocolManager.NetworkType.OfflineServer;
	}

	public NetworkConnectionError StartServers(string _password)
	{
		if (PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.OfflineMode || !PermissionsManager.IsMultiplayerAllowed() || !PermissionsManager.CanHostMultiplayer())
		{
			Log.Warning(string.Format("NET: User unable to create online server. User status: {0}, Multiplayer allowed: {1}, Host Multiplayer allowed: {2}", PlatformManager.MultiPlatform.User.UserStatus, PermissionsManager.IsMultiplayerAllowed(), PermissionsManager.CanHostMultiplayer()));
			this.StartOfflineServer();
			return NetworkConnectionError.NoError;
		}
		Log.Out("NET: Starting server protocols");
		this.SetupProtocols();
		this.CurrentMode = ProtocolManager.NetworkType.Server;
		NetworkConnectionError networkConnectionError = NetworkConnectionError.NoError;
		int @int = GamePrefs.GetInt(EnumGamePrefs.ServerPort);
		if (@int < 1024 || @int > 65530)
		{
			Log.Error(string.Format("NET: Starting server protocols failed: Invalid ServerPort {0}, must be within 1024 and 65530", @int));
			return NetworkConnectionError.InvalidPort;
		}
		for (int i = 0; i < this.servers.Count; i++)
		{
			networkConnectionError = this.servers[i].StartServer(@int, _password);
			if (networkConnectionError != NetworkConnectionError.NoError)
			{
				break;
			}
			this.HasRunningServers = true;
		}
		if (networkConnectionError != NetworkConnectionError.NoError)
		{
			for (int j = 0; j < this.servers.Count; j++)
			{
				this.servers[j].StopServer();
			}
			this.HasRunningServers = false;
			this.CurrentMode = ProtocolManager.NetworkType.None;
			Log.Error("NET: Starting server protocols failed: " + networkConnectionError.ToStringCached<NetworkConnectionError>());
		}
		return networkConnectionError;
	}

	public void MakeServerOffline()
	{
		if (this.CurrentMode != ProtocolManager.NetworkType.Server)
		{
			return;
		}
		this.StopServersOnly();
		this.CurrentMode = ProtocolManager.NetworkType.OfflineServer;
	}

	public void SetServerPassword(string _password)
	{
		if (this.CurrentMode != ProtocolManager.NetworkType.Server)
		{
			return;
		}
		foreach (INetworkServer networkServer in this.servers)
		{
			networkServer.SetServerPassword(_password);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StopServersOnly()
	{
		Log.Out("NET: Stopping server protocols");
		foreach (INetworkServer networkServer in this.servers)
		{
			networkServer.StopServer();
		}
		this.HasRunningServers = false;
	}

	public void StopServers()
	{
		this.StopServersOnly();
		ThreadManager.StartCoroutine(this.resetStateLater(0.25f));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator resetStateLater(float _delay)
	{
		yield return new WaitForSeconds(_delay);
		this.CurrentMode = ProtocolManager.NetworkType.None;
		yield break;
	}

	public void ConnectToServer(GameServerInfo _gameServerInfo)
	{
		this.SetupProtocols();
		this.CurrentMode = ProtocolManager.NetworkType.Client;
		this.currentGameServerInfo = _gameServerInfo;
		this.clients[this.currentConnectionAttemptIndex].Connect(_gameServerInfo);
	}

	public void InvalidPasswordEv()
	{
		this.CurrentMode = ProtocolManager.NetworkType.None;
		this.currentGameServerInfo = null;
		SingletonMonoBehaviour<ConnectionManager>.Instance.Net_InvalidPassword();
	}

	public void ConnectionFailedEv(string _msg)
	{
		this.currentConnectionAttemptIndex++;
		if (this.currentConnectionAttemptIndex < this.clients.Count)
		{
			this.clients[this.currentConnectionAttemptIndex].Connect(this.currentGameServerInfo);
			return;
		}
		this.CurrentMode = ProtocolManager.NetworkType.None;
		this.currentConnectionAttemptIndex = 0;
		this.currentGameServerInfo = null;
		SingletonMonoBehaviour<ConnectionManager>.Instance.Net_ConnectionFailed(_msg);
	}

	public void DisconnectedFromServerEv(string _msg)
	{
		this.CurrentMode = ProtocolManager.NetworkType.None;
		SingletonMonoBehaviour<ConnectionManager>.Instance.Net_DisconnectedFromServer(_msg);
	}

	public void Disconnect()
	{
		this.currentConnectionAttemptIndex = 0;
		for (int i = 0; i < this.clients.Count; i++)
		{
			this.clients[i].Disconnect();
		}
		if (this.IsClient)
		{
			this.CurrentMode = ProtocolManager.NetworkType.None;
			SingletonMonoBehaviour<ConnectionManager>.Instance.DisconnectFromServer();
		}
	}

	public void SetLatencySimulation(bool _enable, int _min, int _max)
	{
		for (int i = 0; i < this.clients.Count; i++)
		{
			this.clients[i].SetLatencySimulation(_enable, _min, _max);
		}
		for (int j = 0; j < this.servers.Count; j++)
		{
			this.servers[j].SetLatencySimulation(_enable, _min, _max);
		}
	}

	public void SetPacketLossSimulation(bool _enable, int _chance)
	{
		for (int i = 0; i < this.clients.Count; i++)
		{
			this.clients[i].SetPacketLossSimulation(_enable, _chance);
		}
		for (int j = 0; j < this.servers.Count; j++)
		{
			this.servers[j].SetPacketLossSimulation(_enable, _chance);
		}
	}

	public void EnableNetworkStatistics()
	{
		for (int i = 0; i < this.clients.Count; i++)
		{
			this.clients[i].EnableStatistics();
		}
		for (int j = 0; j < this.servers.Count; j++)
		{
			this.servers[j].EnableStatistics();
		}
	}

	public void DisableNetworkStatistics()
	{
		for (int i = 0; i < this.clients.Count; i++)
		{
			this.clients[i].DisableStatistics();
		}
		for (int j = 0; j < this.servers.Count; j++)
		{
			this.servers[j].DisableStatistics();
		}
	}

	public string PrintNetworkStatistics()
	{
		string text = "";
		for (int i = 0; i < this.clients.Count; i++)
		{
			text = text + "CLIENT " + i.ToString() + "\n";
			text = text + this.clients[i].PrintNetworkStatistics() + "\n";
		}
		for (int j = 0; j < this.servers.Count; j++)
		{
			text = text + "SERVER " + j.ToString() + "\n";
			text = text + this.servers[j].PrintNetworkStatistics() + "\n";
		}
		return text;
	}

	public void ResetNetworkStatistics()
	{
		for (int i = 0; i < this.clients.Count; i++)
		{
			this.clients[i].ResetNetworkStatistics();
		}
		for (int j = 0; j < this.servers.Count; j++)
		{
			this.servers[j].ResetNetworkStatistics();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<INetworkClient> clients = new List<INetworkClient>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<INetworkServer> servers = new List<INetworkServer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public GameServerInfo currentGameServerInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentConnectionAttemptIndex;

	public enum NetworkType
	{
		None,
		Client,
		Server,
		OfflineServer
	}
}
