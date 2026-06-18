using System;
using System.Collections.ObjectModel;
using UnityEngine.Scripting;

[Preserve]
public class DuplicateUserIdAuthorizer : AuthorizerAbs
{
	public override int Order
	{
		get
		{
			return 60;
		}
	}

	public override string AuthorizerName
	{
		get
		{
			return "DuplicateUserId";
		}
	}

	public override string StateLocalizationKey
	{
		get
		{
			return null;
		}
	}

	public override ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?> Authorize(ClientInfo _clientInfo)
	{
		ReadOnlyCollection<ClientInfo> list = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List;
		for (int i = 0; i < list.Count; i++)
		{
			ClientInfo clientInfo = list[i];
			if (clientInfo != _clientInfo)
			{
				if (!_clientInfo.PlatformId.Equals(clientInfo.PlatformId))
				{
					PlatformUserIdentifierAbs crossplatformId = _clientInfo.CrossplatformId;
					if (crossplatformId == null || !crossplatformId.Equals(clientInfo.CrossplatformId))
					{
						goto IL_0089;
					}
				}
				GameUtils.KickPlayerForClientInfo(clientInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.DuplicatePlayerID, 0, default(DateTime), ""));
				return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(GameUtils.EKickReason.DuplicatePlayerID, 0, default(DateTime), "")));
			}
			IL_0089:;
		}
		return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
	}
}
