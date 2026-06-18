using System;
using System.Collections.ObjectModel;
using UnityEngine.Scripting;

[Preserve]
public class PlayerSlotsAuthorizer : AuthorizerAbs
{
	public override int Order
	{
		get
		{
			return 80;
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
		AdminTools adminTools = GameManager.Instance.adminTools;
		int num = 0;
		int num2 = 0;
		int @int = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
		int int2 = GamePrefs.GetInt(EnumGamePrefs.ServerReservedSlots);
		int int3 = GamePrefs.GetInt(EnumGamePrefs.ServerReservedSlotsPermission);
		ReadOnlyCollection<ClientInfo> list = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List;
		for (int i = 0; i < list.Count; i++)
		{
			ClientInfo clientInfo = list[i];
			if (clientInfo != _clientInfo)
			{
				if (((adminTools != null) ? adminTools.Users.GetUserPermissionLevel(clientInfo) : 1000) <= int3)
				{
					num++;
				}
				else
				{
					num2++;
				}
			}
		}
		if (!GameManager.IsDedicatedServer)
		{
			num++;
		}
		bool flag = ((adminTools != null) ? adminTools.Users.GetUserPermissionLevel(_clientInfo) : 1000) <= int3;
		bool flag2;
		if (flag)
		{
			flag2 = num2 + num < @int;
		}
		else
		{
			flag2 = num2 + num < @int && num2 < @int - int2;
		}
		if (!flag2)
		{
			int int4 = GamePrefs.GetInt(EnumGamePrefs.ServerAdminSlots);
			if (int4 > 0 && num2 + num < @int + int4)
			{
				flag2 = ((adminTools != null) ? adminTools.Users.GetUserPermissionLevel(_clientInfo) : 1000) <= GamePrefs.GetInt(EnumGamePrefs.ServerAdminSlotsPermission);
			}
		}
		if (flag2)
		{
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
		}
		string text;
		if (!flag && num2 + num < @int)
		{
			EAuthorizerSyncResult eauthorizerSyncResult = EAuthorizerSyncResult.SyncDeny;
			GameUtils.EKickReason ekickReason = GameUtils.EKickReason.PlayerLimitExceededNonVIP;
			int num3 = 0;
			text = (@int - int2).ToString();
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(eauthorizerSyncResult, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(ekickReason, num3, default(DateTime), text)));
		}
		EAuthorizerSyncResult eauthorizerSyncResult2 = EAuthorizerSyncResult.SyncDeny;
		GameUtils.EKickReason ekickReason2 = GameUtils.EKickReason.PlayerLimitExceeded;
		int num4 = 0;
		text = @int.ToString();
		return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(eauthorizerSyncResult2, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(ekickReason2, num4, default(DateTime), text)));
	}
}
