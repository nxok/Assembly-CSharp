using System;
using Steamworks;

namespace Platform.Steam
{
	public class AuthenticationClient : IAuthenticationClient
	{
		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
		}

		public string GetAuthTicket()
		{
			if (!this.registeredDisconnectEvent)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.OnDisconnectFromServer += this.OnDisconnectFromServer;
				this.registeredDisconnectEvent = true;
			}
			byte[] array = new byte[1024];
			Log.Out("[Steamworks.NET] Auth.GetAuthTicket()");
			if (this.ticketHandle != HAuthTicket.Invalid)
			{
				SteamUser.CancelAuthTicket(this.ticketHandle);
				this.ticketHandle = HAuthTicket.Invalid;
			}
			SteamNetworkingIdentity steamNetworkingIdentity = default(SteamNetworkingIdentity);
			steamNetworkingIdentity.m_eType = 0;
			SteamNetworkingIdentity steamNetworkingIdentity2 = steamNetworkingIdentity;
			uint num;
			this.ticketHandle = SteamUser.GetAuthSessionTicket(array, array.Length, ref num, ref steamNetworkingIdentity2);
			return Convert.ToBase64String(array);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnDisconnectFromServer()
		{
			if (this.ticketHandle != HAuthTicket.Invalid)
			{
				SteamUser.CancelAuthTicket(this.ticketHandle);
				this.ticketHandle = HAuthTicket.Invalid;
			}
		}

		public void AuthenticateServer(ClientAuthenticateServerContext _context)
		{
			_context.Success();
		}

		public void Destroy()
		{
			this.OnDisconnectFromServer();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public HAuthTicket ticketHandle = HAuthTicket.Invalid;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool registeredDisconnectEvent;
	}
}
