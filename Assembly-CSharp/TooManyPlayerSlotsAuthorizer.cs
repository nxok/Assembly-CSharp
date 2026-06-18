using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class TooManyPlayerSlotsAuthorizer : AuthorizerAbs
{
	public override int Order
	{
		get
		{
			return 81;
		}
	}

	public override string AuthorizerName
	{
		get
		{
			return "PlayerSlots";
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
		int @int = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
		bool flag = true;
		EPlayGroup eplayGroup = _clientInfo.device.ToPlayGroup();
		if (eplayGroup == EPlayGroup.XBS || eplayGroup == EPlayGroup.PS5)
		{
			flag = @int <= 8;
		}
		if (!flag)
		{
			EAuthorizerSyncResult eauthorizerSyncResult = EAuthorizerSyncResult.SyncDeny;
			GameUtils.EKickReason ekickReason = GameUtils.EKickReason.PlatformPlayerLimitExceeded;
			int num = 0;
			string text = 8.ToString();
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(eauthorizerSyncResult, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(ekickReason, num, default(DateTime), text)));
		}
		return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
	}
}
