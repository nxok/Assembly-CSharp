using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdListPlayers : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[] { "listplayers", "lp" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		List<EntityPlayer> list = GameManager.Instance.World.Players.list;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			EntityPlayer entityPlayer = list[i];
			ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(entityPlayer.entityId);
			stringBuilder.Append(i);
			stringBuilder.Append(". id=");
			stringBuilder.Append(entityPlayer.entityId);
			stringBuilder.Append(", ");
			stringBuilder.Append(entityPlayer.EntityName);
			stringBuilder.Append(", pos=");
			stringBuilder.Append(entityPlayer.GetPosition().ToCultureInvariantString());
			stringBuilder.Append(", rot=");
			stringBuilder.Append(entityPlayer.rotation.ToCultureInvariantString());
			stringBuilder.Append(", remote=");
			stringBuilder.Append(entityPlayer.isEntityRemote);
			stringBuilder.Append(", health=");
			stringBuilder.Append(entityPlayer.Health);
			stringBuilder.Append(", deaths=");
			stringBuilder.Append(entityPlayer.Died);
			stringBuilder.Append(", zombies=");
			stringBuilder.Append(entityPlayer.KilledZombies);
			stringBuilder.Append(", players=");
			stringBuilder.Append(entityPlayer.KilledPlayers);
			stringBuilder.Append(", score=");
			stringBuilder.Append(entityPlayer.Score);
			stringBuilder.Append(", level=");
			stringBuilder.Append(entityPlayer.Progression.GetLevel());
			stringBuilder.Append(", pltfmid=");
			StringBuilder stringBuilder2 = stringBuilder;
			string text;
			if (clientInfo == null)
			{
				text = null;
			}
			else
			{
				PlatformUserIdentifierAbs platformId = clientInfo.PlatformId;
				text = ((platformId != null) ? platformId.CombinedString : null);
			}
			stringBuilder2.Append(text ?? "<unknown>");
			stringBuilder.Append(", crossid=");
			StringBuilder stringBuilder3 = stringBuilder;
			string text2;
			if (clientInfo == null)
			{
				text2 = null;
			}
			else
			{
				PlatformUserIdentifierAbs crossplatformId = clientInfo.CrossplatformId;
				text2 = ((crossplatformId != null) ? crossplatformId.CombinedString : null);
			}
			stringBuilder3.Append(text2 ?? "<unknown>");
			stringBuilder.Append(", ip=");
			stringBuilder.Append(((clientInfo != null) ? clientInfo.ip : null) ?? "<unknown>");
			stringBuilder.Append(", ping=");
			stringBuilder.Append(entityPlayer.pingToServer);
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(stringBuilder.ToString());
			stringBuilder.Length = 0;
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Total of " + list.Count.ToString() + " in the game");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "lists all players";
	}
}
