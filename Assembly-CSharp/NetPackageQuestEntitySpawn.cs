using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageQuestEntitySpawn : NetPackage
{
	public NetPackageQuestEntitySpawn Setup(int _entityType, int _entityThatPlaced = -1)
	{
		this.entityType = _entityType;
		this.gamestageGroup = "";
		this.entityIDQuestHolder = _entityThatPlaced;
		return this;
	}

	public NetPackageQuestEntitySpawn Setup(string _gamestageGroup, int _entityThatPlaced = -1)
	{
		this.entityType = -1;
		this.gamestageGroup = _gamestageGroup;
		this.entityIDQuestHolder = _entityThatPlaced;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.entityType = _reader.ReadInt32();
		this.gamestageGroup = _reader.ReadString();
		this.entityIDQuestHolder = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.entityType);
		_writer.Write(this.gamestageGroup);
		_writer.Write(this.entityIDQuestHolder);
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (this.entityType == -1)
		{
			EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(this.entityIDQuestHolder) as EntityPlayer;
			GameStageDefinition gameStage = GameStageDefinition.GetGameStage(this.gamestageGroup);
			this.entityType = EntityGroups.GetRandomFromGroup(gameStage.GetStage(entityPlayer.PartyGameStage).GetSpawnGroup(0).groupName, ref NetPackageQuestEntitySpawn.lastClassId, null);
		}
		QuestActionSpawnEnemy.SpawnQuestEntity(this.entityType, this.entityIDQuestHolder, null);
	}

	public override int GetLength()
	{
		return 20;
	}

	public int entityType = -1;

	public string gamestageGroup;

	public ItemValue itemValue;

	public int entityIDQuestHolder;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int lastClassId = -1;
}
