using System;
using UnityEngine.Scripting;

[Preserve]
public class LegacyModAuthorizer : AuthorizerAbs
{
	public override int Order
	{
		get
		{
			return 150;
		}
	}

	public override string AuthorizerName
	{
		get
		{
			return "LegacyModAuthorizations";
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
		ModEvents.SPlayerLoginData splayerLoginData = new ModEvents.SPlayerLoginData(_clientInfo, _clientInfo.compatibilityVersion);
		ValueTuple<ModEvents.EModEventResult, Mod> valueTuple = ModEvents.PlayerLogin.Invoke(ref splayerLoginData);
		ModEvents.EModEventResult item = valueTuple.Item1;
		Mod item2 = valueTuple.Item2;
		if (item == ModEvents.EModEventResult.StopHandlersAndVanilla)
		{
			Log.Out("Denying login from mod: " + item2.Name);
			EAuthorizerSyncResult eauthorizerSyncResult = EAuthorizerSyncResult.SyncDeny;
			GameUtils.EKickReason ekickReason = GameUtils.EKickReason.ModDecision;
			int num = 0;
			string customMessage = splayerLoginData.CustomMessage;
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(eauthorizerSyncResult, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(ekickReason, num, default(DateTime), customMessage)));
		}
		return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
	}
}
