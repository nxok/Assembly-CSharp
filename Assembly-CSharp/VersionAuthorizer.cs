using System;
using UnityEngine.Scripting;

[Preserve]
public class VersionAuthorizer : AuthorizerAbs
{
	public override int Order
	{
		get
		{
			return 70;
		}
	}

	public override string AuthorizerName
	{
		get
		{
			return "VersionCheck";
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
		if (!string.Equals(Constants.cVersionInformation.LongStringNoBuild, _clientInfo.compatibilityVersion, StringComparison.Ordinal))
		{
			EAuthorizerSyncResult eauthorizerSyncResult = EAuthorizerSyncResult.SyncDeny;
			GameUtils.EKickReason ekickReason = GameUtils.EKickReason.VersionMismatch;
			int num = 0;
			string longStringNoBuild = Constants.cVersionInformation.LongStringNoBuild;
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(eauthorizerSyncResult, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(ekickReason, num, default(DateTime), longStringNoBuild)));
		}
		return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
	}
}
