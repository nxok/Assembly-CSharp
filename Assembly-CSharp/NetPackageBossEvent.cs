using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageBossEvent : NetPackage
{
	public NetPackageBossEvent Setup(NetPackageBossEvent.BossEventTypes _eventType, int _bossGroupID)
	{
		this.bossGroupID = _bossGroupID;
		this.entityID = -1;
		this.minionIDs = null;
		this.eventType = _eventType;
		this.bossIcon1 = "";
		return this;
	}

	public NetPackageBossEvent Setup(NetPackageBossEvent.BossEventTypes _eventType, int _bossGroupID, BossGroup.BossGroupTypes _bossGroupType)
	{
		this.bossGroupID = _bossGroupID;
		this.bossGroupType = _bossGroupType;
		this.entityID = -1;
		this.minionIDs = null;
		this.eventType = _eventType;
		this.bossIcon1 = "";
		return this;
	}

	public NetPackageBossEvent Setup(NetPackageBossEvent.BossEventTypes _eventType, int _bossGroupID, int _entityID)
	{
		this.bossGroupID = _bossGroupID;
		this.entityID = _entityID;
		this.minionIDs = null;
		this.eventType = _eventType;
		this.bossIcon1 = "";
		return this;
	}

	public NetPackageBossEvent Setup(NetPackageBossEvent.BossEventTypes _eventType, int _bossGroupID, BossGroup.BossGroupTypes _bossGroupType, int _bossID, List<int> _minionIDs, string _bossIcon1)
	{
		this.bossGroupID = _bossGroupID;
		this.bossGroupType = _bossGroupType;
		this.entityID = _bossID;
		this.minionIDs = _minionIDs;
		this.eventType = _eventType;
		this.bossIcon1 = _bossIcon1;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.bossGroupID = _reader.ReadInt32();
		this.eventType = (NetPackageBossEvent.BossEventTypes)_reader.ReadByte();
		this.bossGroupType = (BossGroup.BossGroupTypes)_reader.ReadByte();
		this.entityID = _reader.ReadInt32();
		this.bossIcon1 = _reader.ReadString();
		if (this.eventType == NetPackageBossEvent.BossEventTypes.AddGroup)
		{
			int num = _reader.ReadInt32();
			this.minionIDs = new List<int>();
			for (int i = 0; i < num; i++)
			{
				this.minionIDs.Add(_reader.ReadInt32());
			}
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.bossGroupID);
		_writer.Write((byte)this.eventType);
		_writer.Write((byte)this.bossGroupType);
		_writer.Write(this.entityID);
		_writer.Write(this.bossIcon1);
		if (this.eventType == NetPackageBossEvent.BossEventTypes.AddGroup)
		{
			_writer.Write(this.minionIDs.Count);
			for (int i = 0; i < this.minionIDs.Count; i++)
			{
				_writer.Write(this.minionIDs[i]);
			}
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		switch (this.eventType)
		{
		case NetPackageBossEvent.BossEventTypes.RequestGroups:
			GameEventManager.Current.SendBossGroups(base.Sender.entityId);
			return;
		case NetPackageBossEvent.BossEventTypes.AddGroup:
			GameEventManager.Current.SetupClientBossGroup(this.bossGroupID, this.bossGroupType, this.entityID, this.minionIDs, this.bossIcon1);
			return;
		case NetPackageBossEvent.BossEventTypes.UpdateGroupType:
			GameEventManager.Current.UpdateBossGroupType(this.bossGroupID, this.bossGroupType);
			return;
		case NetPackageBossEvent.BossEventTypes.RemoveGroup:
			GameEventManager.Current.RemoveClientBossGroup(this.bossGroupID);
			return;
		case NetPackageBossEvent.BossEventTypes.RemoveMinion:
			GameEventManager.Current.RemoveEntityFromBossGroup(this.bossGroupID, this.entityID);
			return;
		case NetPackageBossEvent.BossEventTypes.RequestStats:
			GameEventManager.Current.RequestBossGroupStatRefresh(this.bossGroupID, base.Sender.entityId);
			return;
		default:
			return;
		}
	}

	public override int GetLength()
	{
		return 20;
	}

	public int bossGroupID;

	public int entityID;

	public List<int> minionIDs;

	public string bossIcon1;

	public BossGroup.BossGroupTypes bossGroupType;

	public NetPackageBossEvent.BossEventTypes eventType;

	public enum BossEventTypes
	{
		RequestGroups,
		AddGroup,
		UpdateGroupType,
		RemoveGroup,
		RemoveMinion,
		RequestStats
	}
}
