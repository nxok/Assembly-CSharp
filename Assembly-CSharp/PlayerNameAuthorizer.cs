using System;
using UnityEngine.Scripting;

[Preserve]
public class PlayerNameAuthorizer : AuthorizerAbs
{
	public override int Order
	{
		get
		{
			return 20;
		}
	}

	public override string AuthorizerName
	{
		get
		{
			return "PlayerName";
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
		if (string.IsNullOrEmpty(_clientInfo.playerName))
		{
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(GameUtils.EKickReason.EmptyNameOrPlayerID, 0, default(DateTime), "")));
		}
		return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
	}
}
