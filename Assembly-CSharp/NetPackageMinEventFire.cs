using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageMinEventFire : NetPackage
{
	public NetPackageMinEventFire Setup(int _selfEntityID, int _otherEntityID, MinEventTypes _eventType, ItemValue _itemValue)
	{
		this.selfEntityID = _selfEntityID;
		this.otherEntityID = _otherEntityID;
		this.eventType = _eventType;
		this.itemValue = _itemValue;
		this.eventPackageType = NetPackageMinEventFire.EventPackageTypes.ItemEvent;
		return this;
	}

	public NetPackageMinEventFire Setup(int _selfEntityID, int _otherEntityID, MinEventTypes _eventType, BlockValue _blockValue)
	{
		this.selfEntityID = _selfEntityID;
		this.otherEntityID = _otherEntityID;
		this.eventType = _eventType;
		this.blockValue = _blockValue;
		this.eventPackageType = NetPackageMinEventFire.EventPackageTypes.BlockEvent;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.selfEntityID = _br.ReadInt32();
		this.otherEntityID = _br.ReadInt32();
		this.eventType = (MinEventTypes)_br.ReadByte();
		this.eventPackageType = (NetPackageMinEventFire.EventPackageTypes)_br.ReadByte();
		if (this.eventPackageType == NetPackageMinEventFire.EventPackageTypes.ItemEvent)
		{
			this.itemValue = new ItemValue();
			this.itemValue.Read(_br);
			this.blockValue = BlockValue.Air;
			return;
		}
		this.blockValue = new BlockValue(_br.ReadUInt32());
		this.itemValue = ItemValue.None;
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.selfEntityID);
		_bw.Write(this.otherEntityID);
		_bw.Write((byte)this.eventType);
		_bw.Write((byte)this.eventPackageType);
		if (this.eventPackageType == NetPackageMinEventFire.EventPackageTypes.ItemEvent)
		{
			this.itemValue.Write(_bw);
			return;
		}
		_bw.Write(this.blockValue.rawData);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityAlive entityAlive = _world.GetEntity(this.selfEntityID) as EntityAlive;
		if (entityAlive != null)
		{
			entityAlive.MinEventContext.Self = entityAlive;
			entityAlive.MinEventContext.Other = ((this.otherEntityID == -1) ? null : (_world.GetEntity(this.otherEntityID) as EntityAlive));
			entityAlive.MinEventContext.ItemValue = this.itemValue;
			entityAlive.MinEventContext.BlockValue = this.blockValue;
			entityAlive.FireEvent(this.eventType, true);
		}
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public override int GetLength()
	{
		return 32;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int selfEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public int otherEntityID;

	[PublicizedFrom(EAccessModifier.Private)]
	public MinEventTypes eventType;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue itemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackageMinEventFire.EventPackageTypes eventPackageType;

	public enum EventPackageTypes
	{
		ItemEvent,
		BlockEvent
	}
}
