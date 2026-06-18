using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerQuestPositions : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public NetPackagePlayerQuestPositions Setup(int entityId, PersistentPlayerData ppd)
	{
		this.entityId = entityId;
		this.questPositions = new List<QuestPositionData>(ppd.QuestPositions);
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.entityId = _reader.ReadInt32();
		this.questPositions = new List<QuestPositionData>();
		int num = _reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			this.questPositions.Add(QuestPositionData.Read(_reader));
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.entityId);
		_writer.Write(this.questPositions.Count);
		foreach (QuestPositionData questPositionData in this.questPositions)
		{
			questPositionData.Write(_writer);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!base.ValidEntityIdForSender(this.entityId, false))
		{
			return;
		}
		PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(this.entityId);
		if (playerDataFromEntityID != null)
		{
			playerDataFromEntityID.QuestPositions.Clear();
			playerDataFromEntityID.QuestPositions.AddRange(this.questPositions);
		}
	}

	public override int GetLength()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<QuestPositionData> questPositions;
}
