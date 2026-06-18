using System;
using System.Collections.Generic;

namespace Platform
{
	public class ClientLobbyManager
	{
		public ClientLobbyManager()
		{
			ConnectionManager.OnClientDisconnected += this.OnClientDisconnected;
		}

		public bool TryGetLobbyId(EPlatformIdentifier platform, out PlatformLobbyId lobbyId)
		{
			object obj = this.lockObj;
			bool flag2;
			lock (obj)
			{
				ClientLobbyManager.Lobby lobby;
				if (this.lobbies.TryGetValue(platform, out lobby))
				{
					lobbyId = lobby.Id;
					flag2 = true;
				}
				else
				{
					lobbyId = null;
					flag2 = false;
				}
			}
			return flag2;
		}

		public void RegisterLobbyClient(PlatformLobbyId platformLobbyId, ClientInfo client, bool overwrite = false)
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.Contains(client))
			{
				Log.Warning(string.Format("[ClientLobbyManager] could not register {0} for client lobby {1} : {2} as they are no longer connected", client.playerName, platformLobbyId.PlatformIdentifier, platformLobbyId.LobbyId));
				return;
			}
			object obj = this.lockObj;
			lock (obj)
			{
				ClientLobbyManager.Lobby lobby;
				if (!this.lobbies.TryGetValue(platformLobbyId.PlatformIdentifier, out lobby))
				{
					Log.Out(string.Format("[ClientLobbyManager] registering new lobby for client platform {0} : {1}", platformLobbyId.PlatformIdentifier, platformLobbyId.LobbyId));
					lobby = new ClientLobbyManager.Lobby(platformLobbyId);
					lobby.AddClient(client);
					this.lobbies.Add(platformLobbyId.PlatformIdentifier, lobby);
				}
				else if (lobby.Id.LobbyId.Equals(platformLobbyId.LobbyId))
				{
					lobby.AddClient(client);
				}
				else if (overwrite)
				{
					Log.Warning(string.Format("[ClientLobbyManager] overwriting existing lobby for {0}", platformLobbyId.PlatformIdentifier));
					ClientLobbyManager.Lobby lobby2 = new ClientLobbyManager.Lobby(platformLobbyId);
					lobby2.AddClient(client);
					foreach (ClientInfo clientInfo in lobby.Clients)
					{
						clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageLobbyJoin>().Setup(platformLobbyId));
						lobby2.AddClient(clientInfo);
					}
					this.lobbies[platformLobbyId.PlatformIdentifier] = lobby2;
				}
				else
				{
					Log.Warning(string.Format("[ClientLobbyManager] a different client lobby already registered for {0}, sending to client", platformLobbyId.PlatformIdentifier));
					client.SendPackage(NetPackageManager.GetPackage<NetPackageLobbyJoin>().Setup(lobby.Id));
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnClientDisconnected(ClientInfo client)
		{
			object obj = this.lockObj;
			lock (obj)
			{
				ClientLobbyManager.Lobby lobby;
				if (this.lobbies.TryGetValue(client.PlatformId.PlatformIdentifier, out lobby))
				{
					lobby.RemoveClient(client);
					if (lobby.IsEmpty)
					{
						Log.Out(string.Format("[ClientLobbyManager] removing registered lobby {0} : {1}", lobby.Id.PlatformIdentifier, lobby.Id.LobbyId));
						this.lobbies.Remove(client.PlatformId.PlatformIdentifier);
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public object lockObj = new object();

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<EPlatformIdentifier, ClientLobbyManager.Lobby> lobbies = new Dictionary<EPlatformIdentifier, ClientLobbyManager.Lobby>();

		[PublicizedFrom(EAccessModifier.Private)]
		public class Lobby
		{
			public PlatformLobbyId Id
			{
				get
				{
					return this.id;
				}
			}

			public bool IsEmpty
			{
				get
				{
					return this.clients.Count == 0;
				}
			}

			public IReadOnlyList<ClientInfo> Clients
			{
				get
				{
					return this.clients;
				}
			}

			public Lobby(PlatformLobbyId id)
			{
				this.id = id;
			}

			public Lobby(EPlatformIdentifier platform, string lobbyId)
			{
				this.id = new PlatformLobbyId(platform, lobbyId);
			}

			public void AddClient(ClientInfo client)
			{
				this.clients.Add(client);
				Log.Out(string.Format("[ClientLobbyManager] registered member {0} for client lobby {1} : {2}. Total members: {3}", new object[]
				{
					client.playerName,
					this.id.PlatformIdentifier,
					this.id.LobbyId,
					this.clients.Count
				}));
			}

			public void RemoveClient(ClientInfo client)
			{
				if (this.clients.Remove(client))
				{
					Log.Out(string.Format("[ClientLobbyManager] removed member {0} from client lobby {1} : {2}. Total members: {3}", new object[]
					{
						client.playerName,
						this.id.PlatformIdentifier,
						this.id.LobbyId,
						this.clients.Count
					}));
					return;
				}
				Log.Warning(string.Format("[ClientLobbyManager] remove member {0} from client lobby {1} : {2} failed. They are not a member", client.playerName, this.id.PlatformIdentifier, this.id.LobbyId));
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly PlatformLobbyId id;

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly List<ClientInfo> clients = new List<ClientInfo>();
		}
	}
}
