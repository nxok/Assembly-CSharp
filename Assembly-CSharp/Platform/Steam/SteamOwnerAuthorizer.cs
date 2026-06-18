using System;
using UnityEngine.Scripting;

namespace Platform.Steam
{
	[Preserve]
	public class SteamOwnerAuthorizer : AuthorizerAbs
	{
		public override int Order
		{
			get
			{
				return 430;
			}
		}

		public override string AuthorizerName
		{
			get
			{
				return "SteamFamily";
			}
		}

		public override string StateLocalizationKey
		{
			get
			{
				return null;
			}
		}

		public override EPlatformIdentifier PlatformRestriction
		{
			get
			{
				return EPlatformIdentifier.Steam;
			}
		}

		public override ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?> Authorize(ClientInfo _clientInfo)
		{
			UserIdentifierSteam userIdentifierSteam = (UserIdentifierSteam)_clientInfo.PlatformId;
			UserIdentifierSteam ownerId = userIdentifierSteam.OwnerId;
			DateTime dateTime;
			string text;
			if (GameManager.Instance.adminTools != null && ownerId != null && !userIdentifierSteam.Equals(ownerId) && GameManager.Instance.adminTools.Blacklist.IsBanned(ownerId, out dateTime, out text))
			{
				return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(GameUtils.EKickReason.Banned, 0, dateTime, text)));
			}
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
		}
	}
}
