using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageGameEventResponse : NetPackage
{
	public NetPackageGameEventResponse Setup(string _event, int _targetEntityID, string _extraData, string _tag, NetPackageGameEventResponse.ResponseTypes _responseType, int _entitySpawnedID = -1, int _index = -1, bool _isDespawn = false, string _actionKey = "")
	{
		this.eventName = _event;
		this.targetEntityID = _targetEntityID;
		this.extraData = _extraData;
		this.tag = _tag;
		this.responseType = _responseType;
		this.entitySpawnedID = _entitySpawnedID;
		this.index = _index;
		this.isDespawn = _isDespawn;
		this.actionKey = _actionKey;
		return this;
	}

	public NetPackageGameEventResponse Setup(NetPackageGameEventResponse.ResponseTypes _responseType, int _entitySpawnedID = -1, int _index = -1, string _tag = "", bool _isDespawn = false)
	{
		this.eventName = "";
		this.targetEntityID = -1;
		this.extraData = "";
		this.tag = _tag;
		this.responseType = _responseType;
		this.entitySpawnedID = _entitySpawnedID;
		this.index = _index;
		this.isDespawn = _isDespawn;
		return this;
	}

	public NetPackageGameEventResponse Setup(NetPackageGameEventResponse.ResponseTypes _responseType, string _event, int _blockGroupID, List<Vector3i> _blockList, string _tag = "", bool _isDespawn = false)
	{
		this.eventName = _event;
		this.targetEntityID = -1;
		this.extraData = "";
		this.tag = _tag;
		this.index = _blockGroupID;
		this.responseType = _responseType;
		this.blockList = _blockList;
		this.isDespawn = _isDespawn;
		return this;
	}

	public NetPackageGameEventResponse Setup(NetPackageGameEventResponse.ResponseTypes _responseType, Vector3i _blockPos)
	{
		this.eventName = "";
		this.targetEntityID = -1;
		this.extraData = "";
		this.tag = "";
		this.responseType = _responseType;
		this.blockPos = _blockPos;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.eventName = _br.ReadString();
		this.targetEntityID = _br.ReadInt32();
		this.extraData = _br.ReadString();
		this.tag = _br.ReadString();
		this.responseType = (NetPackageGameEventResponse.ResponseTypes)_br.ReadByte();
		this.entitySpawnedID = _br.ReadInt32();
		if (this.responseType == NetPackageGameEventResponse.ResponseTypes.ClientSequenceAction)
		{
			this.actionKey = _br.ReadString();
			return;
		}
		if (this.responseType == NetPackageGameEventResponse.ResponseTypes.BlocksAdded)
		{
			this.index = _br.ReadInt32();
			int num = _br.ReadInt32();
			this.blockList = new List<Vector3i>();
			for (int i = 0; i < num; i++)
			{
				this.blockList.Add(StreamUtils.ReadVector3i(_br));
			}
			return;
		}
		if (this.responseType == NetPackageGameEventResponse.ResponseTypes.BlocksRemoved)
		{
			this.index = _br.ReadInt32();
			this.isDespawn = _br.ReadBoolean();
			return;
		}
		if (this.responseType == NetPackageGameEventResponse.ResponseTypes.BlocksRemoved || this.responseType == NetPackageGameEventResponse.ResponseTypes.BlockDamaged)
		{
			this.blockPos = StreamUtils.ReadVector3i(_br);
		}
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.eventName);
		_bw.Write(this.targetEntityID);
		_bw.Write(this.extraData);
		_bw.Write(this.tag);
		_bw.Write((byte)this.responseType);
		_bw.Write(this.entitySpawnedID);
		if (this.responseType == NetPackageGameEventResponse.ResponseTypes.ClientSequenceAction)
		{
			_bw.Write(this.actionKey);
			return;
		}
		if (this.responseType == NetPackageGameEventResponse.ResponseTypes.BlocksAdded)
		{
			_bw.Write(this.index);
			if (this.blockList == null)
			{
				_bw.Write(0);
				return;
			}
			_bw.Write(this.blockList.Count);
			for (int i = 0; i < this.blockList.Count; i++)
			{
				StreamUtils.Write(_bw, this.blockList[i]);
			}
			return;
		}
		else
		{
			if (this.responseType == NetPackageGameEventResponse.ResponseTypes.BlocksRemoved)
			{
				_bw.Write(this.index);
				_bw.Write(this.isDespawn);
				return;
			}
			if (this.responseType == NetPackageGameEventResponse.ResponseTypes.BlocksRemoved || this.responseType == NetPackageGameEventResponse.ResponseTypes.BlockDamaged)
			{
				StreamUtils.Write(_bw, this.blockPos);
			}
			return;
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		switch (this.responseType)
		{
		case NetPackageGameEventResponse.ResponseTypes.Denied:
			GameEventManager.Current.HandleGameEventDenied(this.eventName, this.targetEntityID, this.extraData, this.tag);
			return;
		case NetPackageGameEventResponse.ResponseTypes.Approved:
			GameEventManager.Current.HandleGameEventApproved(this.eventName, this.targetEntityID, this.extraData, this.tag);
			return;
		case NetPackageGameEventResponse.ResponseTypes.TwitchPartyActionApproved:
			GameEventManager.Current.HandleTwitchPartyGameEventApproved(this.eventName, this.targetEntityID, this.extraData, this.tag);
			return;
		case NetPackageGameEventResponse.ResponseTypes.TwitchRefundNeeded:
			GameEventManager.Current.HandleTwitchRefundNeeded(this.eventName, this.targetEntityID, this.extraData, this.tag);
			return;
		case NetPackageGameEventResponse.ResponseTypes.TwitchSetOwner:
			GameEventManager.Current.HandleGameEntitySpawned(this.eventName, this.entitySpawnedID, this.tag);
			GameEventManager.Current.HandleTwitchSetOwner(this.targetEntityID, this.entitySpawnedID, this.extraData);
			return;
		case NetPackageGameEventResponse.ResponseTypes.EntitySpawned:
			GameEventManager.Current.HandleGameEntitySpawned(this.eventName, this.entitySpawnedID, this.tag);
			return;
		case NetPackageGameEventResponse.ResponseTypes.EntityDespawned:
			GameEventManager.Current.HandleGameEntityDespawned(this.entitySpawnedID);
			return;
		case NetPackageGameEventResponse.ResponseTypes.EntityKilled:
			GameEventManager.Current.HandleGameEntityKilled(this.entitySpawnedID);
			return;
		case NetPackageGameEventResponse.ResponseTypes.BlocksAdded:
			GameEventManager.Current.HandleGameBlocksAdded(this.eventName, this.index, this.blockList, this.tag);
			return;
		case NetPackageGameEventResponse.ResponseTypes.BlocksRemoved:
			GameEventManager.Current.HandleGameBlocksRemoved(this.index, this.isDespawn);
			return;
		case NetPackageGameEventResponse.ResponseTypes.BlockRemoved:
			GameEventManager.Current.HandleGameBlockRemoved(this.blockPos);
			return;
		case NetPackageGameEventResponse.ResponseTypes.BlockDamaged:
			GameEventManager.Current.SendBlockDamageUpdate(this.blockPos);
			return;
		case NetPackageGameEventResponse.ResponseTypes.ClientSequenceAction:
			GameEventManager.Current.HandleGameEventSequenceItemForClient(this.eventName, this.actionKey);
			return;
		case NetPackageGameEventResponse.ResponseTypes.Completed:
			GameEventManager.Current.HandleGameEventCompleted(this.eventName, this.targetEntityID, this.extraData, this.tag);
			return;
		default:
			return;
		}
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.Both;
		}
	}

	public override int GetLength()
	{
		return 30;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string eventName;

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackageGameEventResponse.ResponseTypes responseType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int entitySpawnedID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int targetEntityID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string extraData;

	[PublicizedFrom(EAccessModifier.Private)]
	public string tag = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int index = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string actionKey = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDespawn;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3i> blockList;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i blockPos;

	public enum ResponseTypes
	{
		Denied,
		Approved,
		TwitchPartyActionApproved,
		TwitchRefundNeeded,
		TwitchSetOwner,
		EntitySpawned,
		EntityDespawned,
		EntityKilled,
		BlocksAdded,
		BlocksRemoved,
		BlockRemoved,
		BlockDamaged,
		ClientSequenceAction,
		Completed
	}
}
