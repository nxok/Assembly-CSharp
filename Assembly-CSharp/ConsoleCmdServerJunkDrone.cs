using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdServerJunkDrone : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "jds" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Server junk drone commands.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return this.buildHelpLog();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string buildHelpLog()
	{
		return "JunkDrone help:" + Environment.NewLine + "[Server/Host Commands]" + Environment.NewLine + "jds, log man - logs out drone manager data" + Environment.NewLine + "clear - clears drone data for player (ex. clear [PlayerName])" + Environment.NewLine + "unstuck [playerId] - triggers teleport to player" + Environment.NewLine;
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			Log.Out(this.logDroneManager("Manager Drones"));
			return;
		}
		string text = _params[0];
		if (text.Equals("help"))
		{
			Log.Out(this.buildHelpLog());
			return;
		}
		if (text.Equals("save"))
		{
			DroneManager.Instance.TriggerSave();
			return;
		}
		if (text.Equals("log") && _params.Count > 1)
		{
			string text2 = _params[1];
			if (text2.ContainsCaseInsensitive("man"))
			{
				Log.Out(this.logDroneManager("Manager Drones"));
			}
			if (text2.ContainsCaseInsensitive("active"))
			{
				Log.Out("Active Drones" + Environment.NewLine + DroneManager.Instance.LogActiveDrones() + Environment.NewLine);
			}
			if (text2.ContainsCaseInsensitive("unloaded"))
			{
				Log.Out("Unloaded Drones" + Environment.NewLine + DroneManager.Instance.LogUnloadedDrones() + Environment.NewLine);
			}
			return;
		}
		if (text.Equals("clearall"))
		{
			if (_senderInfo.RemoteClientInfo == null)
			{
				foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in GameManager.Instance.GetPersistentPlayerList().Players)
				{
					this.removeDronesForPlayer(keyValuePair.Value.EntityId);
				}
				Log.Out("JunkDrone data cleared for all players.");
				return;
			}
			Log.Out("This command can only be run from the host or server.");
			return;
		}
		else
		{
			if (text.Equals("clear") && _params.Count > 1)
			{
				string text3 = _params[1];
				List<EntityPlayer> list = GameManager.Instance.World.Players.list;
				for (int i = 0; i < list.Count; i++)
				{
					EntityPlayer entityPlayer = list[i];
					if (entityPlayer.EntityName.ContainsCaseInsensitive(text3))
					{
						this.removeDronesForPlayer(entityPlayer);
						Log.Out("JunkDrone data cleared for {0}.", new object[] { entityPlayer.EntityName });
						return;
					}
				}
				return;
			}
			if (text.Equals("unstuck"))
			{
				EntityPlayer entityPlayer2 = ((_senderInfo.RemoteClientInfo == null) ? GameManager.Instance.World.GetPrimaryPlayer() : (GameManager.Instance.World.GetEntity(_senderInfo.RemoteClientInfo.entityId) as EntityPlayer));
				if (entityPlayer2 == null)
				{
					return;
				}
				OwnedEntityData[] ownedEntityClass = entityPlayer2.GetOwnedEntityClass("entityJunkDrone");
				if (ownedEntityClass.Length != 0)
				{
					EntityDrone entityDrone = GameManager.Instance.World.GetEntity(ownedEntityClass[0].Id) as EntityDrone;
					if (entityDrone == null)
					{
						entityDrone = DroneManager.Instance.LoadDrone(ownedEntityClass[0].Id, GameManager.Instance.World);
					}
					if (entityDrone != null)
					{
						entityDrone.DebugTeleportUnstuck();
						Log.Out("drone unstuck complete");
					}
					else
					{
						Log.Warning("Server unstuck drone failed");
					}
				}
			}
			if (text.Equals("teleport") || text.Equals("tele"))
			{
				EntityPlayer entityPlayer3 = ((_senderInfo.RemoteClientInfo == null) ? GameManager.Instance.World.GetPrimaryPlayer() : (GameManager.Instance.World.GetEntity(_senderInfo.RemoteClientInfo.entityId) as EntityPlayer));
				if (entityPlayer3 == null)
				{
					return;
				}
				int num = -1;
				if (int.TryParse(_params[1], out num))
				{
					EntityDrone activeDronesWithId = DroneManager.Instance.GetActiveDronesWithId(num);
					if (activeDronesWithId)
					{
						activeDronesWithId.DebugTeleportTo(entityPlayer3.position);
						Log.Out("drone debug teleport complete for {0}", new object[] { num });
					}
				}
			}
			if (text.Equals("remove"))
			{
				int num2 = -1;
				if (int.TryParse(_params[1], out num2))
				{
					DroneManager.Instance.RemoveActiveDrone(num2);
				}
			}
			if (text.Equals("unassign"))
			{
				string playerName2 = _params[1];
				int num3 = -1;
				if (int.TryParse(_params[2], out num3))
				{
					EntityPlayer entityPlayer4 = GameManager.Instance.World.Players.list.Find((EntityPlayer p) => p.EntityName.ContainsCaseInsensitive(playerName2));
					if (entityPlayer4)
					{
						EntityDrone entityDrone2 = GameManager.Instance.World.GetEntity(num3) as EntityDrone;
						if (entityDrone2)
						{
							entityDrone2.belongsPlayerId = -1;
							entityDrone2.Owner = null;
							entityPlayer4.RemoveOwnedEntity(entityDrone2);
							entityDrone2.OwnerID = null;
							Log.Out("unassigned drone {0} from player {1}", new object[] { num3, entityPlayer4.EntityName });
							return;
						}
					}
				}
			}
			if (text.Equals("assign"))
			{
				string playerName3 = _params[1];
				int num4 = -1;
				if (int.TryParse(_params[2], out num4))
				{
					EntityPlayer entityPlayer5 = GameManager.Instance.World.Players.list.Find((EntityPlayer p) => p.EntityName.ContainsCaseInsensitive(playerName3));
					if (entityPlayer5)
					{
						bool flag = EntityDrone.IsValidForLocalPlayer();
						if (!flag)
						{
							Log.Out("please pick up, or clear the currently deployed drone");
							return;
						}
						if (DroneManager.Instance.AssignUnloadedDrone(entityPlayer5, num4))
						{
							Log.Out("assigned unloaded drone {0} to player {1}", new object[] { num4, entityPlayer5.EntityName });
						}
						else if (flag)
						{
							EntityDrone entityDrone3 = GameManager.Instance.World.GetEntity(num4) as EntityDrone;
							if (entityDrone3)
							{
								entityDrone3.belongsPlayerId = entityPlayer5.entityId;
								entityDrone3.Owner = entityPlayer5;
								entityPlayer5.AddOwnedEntity(entityDrone3);
								entityDrone3.OwnerID = PlatformManager.InternalLocalUserIdentifier;
								Log.Warning(entityDrone3.OwnerID.ReadablePlatformUserIdentifier.ToString());
								Log.Out("assigned drone {0} to player {1}", new object[] { num4, entityPlayer5.EntityName });
								return;
							}
							Log.Out("assign drone failed, id is not a drone");
						}
						else
						{
							Log.Out("assign drone failed");
						}
					}
					else
					{
						Log.Out("unknown player name: " + playerName3);
					}
				}
				else
				{
					Log.Out("assign drone parse failed");
				}
			}
			if (text.Equals("eir"))
			{
				string playerName = _params[1];
				int num5 = -1;
				if (int.TryParse(_params[2], out num5) && GameManager.Instance.World.Players.list.Find((EntityPlayer p) => p.EntityName.ContainsCaseInsensitive(playerName)))
				{
					EntityDrone entityDrone4 = GameManager.Instance.World.GetEntity(num5) as EntityDrone;
					if (entityDrone4)
					{
						entityDrone4.DebugEnemiesInRange = !entityDrone4.DebugEnemiesInRange;
						Log.Out("JunkDrone DebugEnemiesInRange {0}", new object[] { entityDrone4.DebugEnemiesInRange });
						return;
					}
				}
			}
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string logDroneManager(string header)
	{
		string text = header + Environment.NewLine;
		List<EntityCreationData> allDronesECD = DroneManager.Instance.GetAllDronesECD();
		for (int i = 0; i < allDronesECD.Count; i++)
		{
			EntityCreationData entityCreationData = allDronesECD[i];
			text = text + string.Format("#{0}, id {1}, {2}, {3}, chunk {4}, ownerId {5}", new object[]
			{
				i,
				entityCreationData.id,
				EntityClass.GetEntityClassName(entityCreationData.entityClass),
				entityCreationData.pos.ToCultureInvariantString(),
				World.toChunkXZ(entityCreationData.pos),
				entityCreationData.belongsPlayerId
			}) + Environment.NewLine;
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeDronesForPlayer(EntityPlayer player)
	{
		OwnedEntityData[] ownedEntityClass = player.GetOwnedEntityClass("entityJunkDrone");
		for (int i = 0; i < ownedEntityClass.Length; i++)
		{
			player.RemoveOwnedEntity(ownedEntityClass[i]);
		}
		DroneManager.Instance.ClearAllDronesForPlayer(player);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeDronesForPlayer(int entityId)
	{
		EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(entityId) as EntityPlayer;
		if (entityPlayer)
		{
			OwnedEntityData[] ownedEntityClass = entityPlayer.GetOwnedEntityClass("entityJunkDrone");
			for (int i = 0; i < ownedEntityClass.Length; i++)
			{
				entityPlayer.RemoveOwnedEntity(ownedEntityClass[i]);
			}
		}
		DroneManager.Instance.ClearAllDronesForPlayer(entityId);
	}
}
