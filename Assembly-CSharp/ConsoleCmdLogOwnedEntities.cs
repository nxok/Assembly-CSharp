using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdLogOwnedEntities : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	public override DeviceFlag AllowedDeviceTypes
	{
		get
		{
			return DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;
		}
	}

	public override DeviceFlag AllowedDeviceTypesClient
	{
		get
		{
			return DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "playerOwnedEntities", "poe" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Lists player owned entities.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return this.buildHelpLog();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string buildHelpLog()
	{
		return "Player Owned Entities help:" + Environment.NewLine + "[Client Commands]" + Environment.NewLine + "poe - Lists owned entities for the local player" + Environment.NewLine + "[Server/Host Commands]" + Environment.NewLine + "poe - Lists owned entities for all players online" + Environment.NewLine + "[PlayerName] - Lists the owned entities for passed online player (ex. poe [PlayerName])" + Environment.NewLine;
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
				if (primaryPlayer)
				{
					Log.Out("Client Player Owned Entities" + Environment.NewLine + this.logPlayerOwnedEntities(primaryPlayer.entityId));
					return;
				}
			}
			else
			{
				string text = "Game Player Owned Entities" + Environment.NewLine;
				Log.Out(this.logPlayerOwnedEntities(text));
			}
			return;
		}
		if (_params[0].ContainsCaseInsensitive("help"))
		{
			Log.Out(this.buildHelpLog());
			return;
		}
		if (_params.Count > 0)
		{
			List<EntityPlayer> list = GameManager.Instance.World.Players.list;
			for (int i = 0; i < list.Count; i++)
			{
				EntityPlayer entityPlayer = list[i];
				if (entityPlayer.EntityName.ContainsCaseInsensitive(_params[0]))
				{
					Log.Out(entityPlayer.EntityName + " Owned Entities" + Environment.NewLine + this.logPlayerOwnedEntities(entityPlayer.entityId));
					return;
				}
			}
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string logPlayerOwnedEntities(string header)
	{
		string text = header + Environment.NewLine;
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in GameManager.Instance.GetPersistentPlayerList().Players)
		{
			text += this.logPlayerOwnedEntities(keyValuePair.Value.EntityId);
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string logPlayerOwnedEntities(int entityId)
	{
		string text = string.Empty;
		EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(entityId) as EntityPlayer;
		if (entityPlayer)
		{
			OwnedEntityData[] ownedEntities = entityPlayer.GetOwnedEntities();
			string text2 = string.Empty;
			foreach (OwnedEntityData ownedEntityData in ownedEntities)
			{
				text2 = text2 + string.Format("entityId: {0}, classId: {1}, lastKnownPosition: {2}", ownedEntityData.Id, EntityClass.GetEntityClassName(ownedEntityData.ClassId), ownedEntityData.hasLastKnownPosition ? ownedEntityData.LastKnownPosition.ToString() : "none") + Environment.NewLine;
			}
			text += string.Format("[{0} - ({1})]" + Environment.NewLine + "{2}", entityPlayer.EntityName, ownedEntities.Length, text2);
		}
		return text;
	}
}
