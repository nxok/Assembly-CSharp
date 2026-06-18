using System;
using System.Runtime.CompilerServices;

namespace Platform
{
	public interface IAntiCheatServer : IAntiCheatEncryption, IEncryptionModule
	{
		void Init(IPlatform _owner);

		void Update();

		bool StartServer(AuthenticationSuccessfulCallbackDelegate _authSuccessfulDelegate, KickPlayerDelegate _kickPlayerDelegate);

		bool RegisterUser(ClientInfo _client);

		void FreeUser(ClientInfo _client);

		void HandleMessageFromClient(ClientInfo _cInfo, byte[] _data);

		void StopServer();

		void Destroy();

		bool ServerEacEnabled();

		bool ServerEacAvailable();

		bool GetHostUserIdAndToken([TupleElementNames(new string[] { "userId", "token" })] out ValueTuple<PlatformUserIdentifierAbs, string> _hostUserIdAndToken);
	}
}
