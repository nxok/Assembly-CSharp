using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class CrossplayAuthorizer : AuthorizerAbs
{
	public override int Order
	{
		get
		{
			return 550;
		}
	}

	public override string AuthorizerName
	{
		get
		{
			return "Crossplay";
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
		bool @bool = GamePrefs.GetBool(EnumGamePrefs.ServerAllowCrossplay);
		IUserClient user = PlatformManager.MultiPlatform.User;
		bool flag = user == null || user.Permissions.HasCrossplay();
		if (@bool && flag)
		{
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
		}
		if (_clientInfo.device.ToPlayGroup() != DeviceFlag.StandaloneWindows.ToPlayGroup())
		{
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(GameUtils.EKickReason.CrossplayDisabled, 0, default(DateTime), "")));
		}
		return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
	}
}
