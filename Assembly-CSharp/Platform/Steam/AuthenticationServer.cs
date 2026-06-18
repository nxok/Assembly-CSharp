using System;
using Steamworks;

namespace Platform.Steam
{
	public class AuthenticationServer : IAuthenticationServer
	{
		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
			this.owner.Api.ClientApiInitialized += delegate
			{
				Log.Out("[Steamworks.NET] Registering auth callbacks");
				if (this.m_validateAuthTicketResponse == null)
				{
					this.m_validateAuthTicketResponse = Callback<ValidateAuthTicketResponse_t>.CreateGameServer(new Callback<ValidateAuthTicketResponse_t>.DispatchDelegate(this.ValidateAuthTicketResponse));
				}
				if (this.m_gsClientGroupStatus == null)
				{
					this.m_gsClientGroupStatus = Callback<GSClientGroupStatus_t>.CreateGameServer(new Callback<GSClientGroupStatus_t>.DispatchDelegate(this.GsClientGroupStatus));
				}
			};
		}

		public EBeginUserAuthenticationResult AuthenticateUser(ClientInfo _cInfo)
		{
			Log.Out("[Steamworks.NET] Auth.AuthenticateUser()");
			UserIdentifierSteam userIdentifierSteam = (UserIdentifierSteam)_cInfo.PlatformId;
			CSteamID csteamID;
			csteamID..ctor(userIdentifierSteam.SteamId);
			byte[] ticket = userIdentifierSteam.Ticket;
			if (ticket == null || ticket.Length == 0)
			{
				return EBeginUserAuthenticationResult.InvalidTicket;
			}
			EBeginAuthSessionResult ebeginAuthSessionResult = SteamGameServer.BeginAuthSession(ticket, ticket.Length, csteamID);
			string[] array = new string[8];
			array[0] = "[Steamworks.NET] Authenticating player: ";
			array[1] = _cInfo.playerName;
			array[2] = " SteamId: ";
			int num = 3;
			CSteamID csteamID2 = csteamID;
			array[num] = csteamID2.ToString();
			array[4] = " TicketLen: ";
			array[5] = ticket.Length.ToString();
			array[6] = " Result: ";
			array[7] = ebeginAuthSessionResult.ToStringCached<EBeginAuthSessionResult>();
			Log.Out(string.Concat(array));
			if (ebeginAuthSessionResult == null)
			{
				return EBeginUserAuthenticationResult.Ok;
			}
			SteamGameServer.EndAuthSession(csteamID);
			return ebeginAuthSessionResult;
		}

		public void RemoveUser(ClientInfo _cInfo)
		{
			if (this.owner.ServerListAnnouncer.GameServerInitialized)
			{
				SteamGameServer.EndAuthSession(new CSteamID(((UserIdentifierSteam)_cInfo.PlatformId).SteamId));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ValidateAuthTicketResponse(ValidateAuthTicketResponse_t _resp)
		{
			CSteamID steamID = _resp.m_SteamID;
			CSteamID ownerSteamID = _resp.m_OwnerSteamID;
			PlatformUserIdentifierAbs platformUserIdentifierAbs = new UserIdentifierSteam(steamID);
			ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForUserId(platformUserIdentifierAbs);
			if (clientInfo == null)
			{
				Log.Warning(string.Format("[Steamworks.NET] Authentication callback failed: User not found. ID: {0}", steamID));
				return;
			}
			string playerName = clientInfo.playerName;
			((UserIdentifierSteam)clientInfo.PlatformId).OwnerId = new UserIdentifierSteam(ownerSteamID);
			Log.Out(string.Format("[Steamworks.NET] Authentication callback. ID: {0}, owner: {1}, result: {2}", steamID, ownerSteamID, _resp.m_eAuthSessionResponse.ToStringCached<EAuthSessionResponse>()));
			if (_resp.m_eAuthSessionResponse == null)
			{
				this.authSuccessfulDelegate(clientInfo);
				return;
			}
			Log.Out(string.Format("[Steamworks.NET] Kick player for invalid login: {0} {1}", steamID, playerName));
			KickPlayerDelegate kickPlayerDelegate = this.kickPlayerDelegate;
			if (kickPlayerDelegate == null)
			{
				return;
			}
			kickPlayerDelegate(clientInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.PlatformAuthenticationFailed, _resp.m_eAuthSessionResponse, default(DateTime), ""));
		}

		public void StartServer(AuthenticationSuccessfulCallbackDelegate _authSuccessfulDelegate, KickPlayerDelegate _kickPlayerDelegate)
		{
			this.authSuccessfulDelegate = _authSuccessfulDelegate;
			this.kickPlayerDelegate = _kickPlayerDelegate;
		}

		public void StartServerSteamGroups(SteamGroupStatusResponse _groupStatusResponseDelegate)
		{
			this.groupStatusResponseDelegate = _groupStatusResponseDelegate;
		}

		public void StopServer()
		{
			this.authSuccessfulDelegate = null;
			this.kickPlayerDelegate = null;
		}

		public bool RequestUserInGroupStatus(ClientInfo _cInfo, string _steamIdGroup)
		{
			UserIdentifierSteam userIdentifierSteam = (UserIdentifierSteam)_cInfo.PlatformId;
			CSteamID csteamID;
			csteamID..ctor(userIdentifierSteam.SteamId);
			ulong num;
			if (!ulong.TryParse(_steamIdGroup, out num))
			{
				Log.Warning("Invalid Steam group ID '" + _steamIdGroup + "' (value out of range) in serveradmin.xml, ignoring");
				return false;
			}
			CSteamID csteamID2;
			csteamID2..ctor(num);
			EAccountType eaccountType = csteamID2.GetEAccountType();
			if (eaccountType != 7)
			{
				if (eaccountType == 1)
				{
					Log.Warning("Invalid Steam group ID '" + _steamIdGroup + "' (SteamID is for a user, not for a group) in serveradmin.xml, ignoring");
				}
				else
				{
					Log.Warning(string.Concat(new string[]
					{
						"Invalid Steam group ID '",
						_steamIdGroup,
						"' (SteamID not valid for a Steam group but for ",
						eaccountType.ToStringCached<EAccountType>(),
						") in serveradmin.xml, ignoring"
					}));
				}
				return false;
			}
			bool flag = SteamGameServer.RequestUserGroupStatus(csteamID, csteamID2);
			if (!flag)
			{
				Log.Warning(string.Format("Failed requesting Steam group membership for user {0} and group {1}", csteamID, csteamID2));
			}
			return flag;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void GsClientGroupStatus(GSClientGroupStatus_t _response)
		{
			CSteamID steamIDUser = _response.m_SteamIDUser;
			ulong steamID = _response.m_SteamIDGroup.m_SteamID;
			PlatformUserIdentifierAbs platformUserIdentifierAbs = new UserIdentifierSteam(steamIDUser);
			ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForUserId(platformUserIdentifierAbs);
			if (clientInfo != null)
			{
				this.groupStatusResponseDelegate(clientInfo, steamID, _response.m_bMember, _response.m_bOfficer);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public Callback<ValidateAuthTicketResponse_t> m_validateAuthTicketResponse;

		[PublicizedFrom(EAccessModifier.Private)]
		public Callback<GSClientGroupStatus_t> m_gsClientGroupStatus;

		[PublicizedFrom(EAccessModifier.Private)]
		public AuthenticationSuccessfulCallbackDelegate authSuccessfulDelegate;

		[PublicizedFrom(EAccessModifier.Private)]
		public KickPlayerDelegate kickPlayerDelegate;

		[PublicizedFrom(EAccessModifier.Private)]
		public SteamGroupStatusResponse groupStatusResponseDelegate;
	}
}
