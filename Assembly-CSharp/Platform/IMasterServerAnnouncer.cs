using System;

namespace Platform
{
	public interface IMasterServerAnnouncer
	{
		void Init(IPlatform _owner);

		void Update();

		bool GameServerInitialized { get; }

		string GetServerPorts();

		void AdvertiseServer(Action _onServerRegistered);

		void StopServer();
	}
}
