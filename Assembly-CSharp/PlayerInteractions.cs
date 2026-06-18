using System;
using System.Collections.Generic;
using System.Linq;
using Platform;

public class PlayerInteractions
{
	public static PlayerInteractions Instance { get; } = new PlayerInteractions();

	public event PlayerIteractionEvent OnNewPlayerInteraction;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerInteractions()
	{
	}

	public void JoinedMultiplayerServer(PersistentPlayerList ppl)
	{
		Log.Out("[PlayerInteractions] JoinedMultplayerServer");
		this.RecordInteractionForActivePersistentPlayers(ppl, PlayerInteractionType.Login);
		this.SetPlayersList(ppl);
	}

	public void PlayerSpawnedInMultiplayerServer(PersistentPlayerList ppl, int spawningEntityId, RespawnType respawnReason)
	{
		this.SetPlayersList(ppl);
		if (respawnReason != RespawnType.NewGame && respawnReason != RespawnType.EnterMultiplayer && respawnReason != RespawnType.JoinMultiplayer && respawnReason != RespawnType.LoadedGame)
		{
			return;
		}
		LocalPlayerUI uiforPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		EntityPlayerLocal entityPlayerLocal = ((uiforPrimaryPlayer != null) ? uiforPrimaryPlayer.entityPlayer : null);
		int? num = ((entityPlayerLocal != null) ? new int?(entityPlayerLocal.entityId) : null);
		if (((spawningEntityId == num.GetValueOrDefault()) & (num != null)) && spawningEntityId != -1)
		{
			this.RecordInteractionForActivePersistentPlayers(ppl, PlayerInteractionType.FirstSpawn);
			return;
		}
		PersistentPlayerData playerDataFromEntityID = ppl.GetPlayerDataFromEntityID(spawningEntityId);
		PlayerIteractionEvent onNewPlayerInteraction = this.OnNewPlayerInteraction;
		if (onNewPlayerInteraction == null)
		{
			return;
		}
		onNewPlayerInteraction(this.CreateInteraction(playerDataFromEntityID, PlayerInteractionType.FirstSpawn));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetPlayersList(PersistentPlayerList ppl)
	{
		if (ppl == this.playerList)
		{
			return;
		}
		if (this.playerList != null)
		{
			this.playerList.RemovePlayerEventHandler(new PersistentPlayerData.PlayerEventHandler(this.OnPersistentPlayerEvent));
		}
		this.playerList = ppl;
		this.playerList.AddPlayerEventHandler(new PersistentPlayerData.PlayerEventHandler(this.OnPersistentPlayerEvent));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPersistentPlayerEvent(PersistentPlayerData ppData, PersistentPlayerData otherPlayer, EnumPersistentPlayerDataReason reason)
	{
		PlayerInteraction playerInteraction = default(PlayerInteraction);
		if (reason != EnumPersistentPlayerDataReason.Login)
		{
			if (reason == EnumPersistentPlayerDataReason.Disconnected)
			{
				Log.Out("[PlayerInteractions] persistent player disconnect");
				playerInteraction = this.CreateInteraction(ppData, PlayerInteractionType.Disconnect);
				IPlayerInteractionsRecorder playerInteractionsRecorder = PlatformManager.MultiPlatform.PlayerInteractionsRecorder;
				if (playerInteractionsRecorder != null)
				{
					playerInteractionsRecorder.RecordPlayerInteraction(playerInteraction);
				}
			}
		}
		else
		{
			Log.Out("[PlayerInteractions] persistent player login");
			playerInteraction = this.CreateInteraction(ppData, PlayerInteractionType.Login);
			IPlayerInteractionsRecorder playerInteractionsRecorder2 = PlatformManager.MultiPlatform.PlayerInteractionsRecorder;
			if (playerInteractionsRecorder2 != null)
			{
				playerInteractionsRecorder2.RecordPlayerInteraction(playerInteraction);
			}
		}
		PlayerIteractionEvent onNewPlayerInteraction = this.OnNewPlayerInteraction;
		if (onNewPlayerInteraction == null)
		{
			return;
		}
		onNewPlayerInteraction(playerInteraction);
	}

	public void Shutdown()
	{
		if (this.playerList != null)
		{
			Log.Out("[PlayerInteractions] Shutdown, record disconnect for all currently connected players");
			this.RecordInteractionForActivePersistentPlayers(this.playerList, PlayerInteractionType.Disconnect);
			this.playerList.RemovePlayerEventHandler(new PersistentPlayerData.PlayerEventHandler(this.OnPersistentPlayerEvent));
			this.playerList = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RecordInteractionForActivePersistentPlayers(PersistentPlayerList ppl, PlayerInteractionType interactionType)
	{
		IEnumerable<PlayerInteraction> enumerable = from ppd in ppl.Players.Values.ToList<PersistentPlayerData>()
			where ppd.EntityId != -1
			select this.CreateInteraction(ppd, interactionType);
		IPlayerInteractionsRecorder playerInteractionsRecorder = PlatformManager.MultiPlatform.PlayerInteractionsRecorder;
		if (playerInteractionsRecorder != null)
		{
			playerInteractionsRecorder.RecordPlayerInteractions(enumerable);
		}
		foreach (PlayerInteraction playerInteraction in enumerable)
		{
			PlayerIteractionEvent onNewPlayerInteraction = this.OnNewPlayerInteraction;
			if (onNewPlayerInteraction != null)
			{
				onNewPlayerInteraction(playerInteraction);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerInteraction CreateInteraction(PersistentPlayerData ppd, PlayerInteractionType type)
	{
		return new PlayerInteraction(ppd.PlayerData, type);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PersistentPlayerList playerList;
}
