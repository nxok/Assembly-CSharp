using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Scripting;

namespace Platform.Steam
{
	[Preserve]
	public class SteamGroupsAuthorizer : AuthorizerAbs
	{
		public override int Order
		{
			get
			{
				return 470;
			}
		}

		public override string AuthorizerName
		{
			get
			{
				return "SteamGroups";
			}
		}

		public override string StateLocalizationKey
		{
			get
			{
				return "authstate_steamgroups";
			}
		}

		public override EPlatformIdentifier PlatformRestriction
		{
			get
			{
				return EPlatformIdentifier.Steam;
			}
		}

		public override bool AuthorizerActive
		{
			get
			{
				return GameManager.Instance.adminTools != null && PlatformManager.InstanceForPlatformIdentifier(EPlatformIdentifier.Steam) != null;
			}
		}

		public override void ServerStart()
		{
			base.ServerStart();
			IAuthenticationServer authenticationServer = PlatformManager.NativePlatform.AuthenticationServer;
			if (authenticationServer == null)
			{
				return;
			}
			authenticationServer.StartServerSteamGroups(new SteamGroupStatusResponse(this.groupStatusCallback));
		}

		public override ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?> Authorize(ClientInfo _clientInfo)
		{
			Dictionary<string, AdminWhitelist.WhitelistGroup> groups = GameManager.Instance.adminTools.Whitelist.GetGroups();
			Dictionary<string, AdminUsers.GroupPermission> groups2 = GameManager.Instance.adminTools.Users.GetGroups();
			if (groups.Count == 0 && groups2.Count == 0)
			{
				return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
			}
			EPlatformIdentifier platformIdentifier = _clientInfo.PlatformId.PlatformIdentifier;
			IPlatform platform = PlatformManager.InstanceForPlatformIdentifier(platformIdentifier);
			if (platform == null)
			{
				EAuthorizerSyncResult eauthorizerSyncResult = EAuthorizerSyncResult.SyncDeny;
				GameUtils.EKickReason ekickReason = GameUtils.EKickReason.UnsupportedPlatform;
				int num = 0;
				string text = platformIdentifier.ToStringCached<EPlatformIdentifier>();
				return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(eauthorizerSyncResult, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(ekickReason, num, default(DateTime), text)));
			}
			HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
			groups.CopyKeysTo(hashSet);
			groups2.CopyKeysTo(hashSet);
			_clientInfo.groupMembershipsWaiting = hashSet.Count;
			foreach (string text2 in hashSet)
			{
				if (!platform.AuthenticationServer.RequestUserInGroupStatus(_clientInfo, text2))
				{
					Interlocked.Decrement(ref _clientInfo.groupMembershipsWaiting);
				}
			}
			if (_clientInfo.groupMembershipsWaiting == 0)
			{
				return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
			}
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.WaitAsync, null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void groupStatusCallback(ClientInfo _clientInfo, ulong _groupId, bool _member, bool _officer)
		{
			bool flag = Interlocked.Decrement(ref _clientInfo.groupMembershipsWaiting) != 0;
			if (_member)
			{
				_clientInfo.groupMemberships[_groupId.ToString()] = (_officer ? 2 : 1);
			}
			if (!flag)
			{
				this.authResponsesHandler.AuthorizationAccepted(this, _clientInfo);
			}
		}
	}
}
