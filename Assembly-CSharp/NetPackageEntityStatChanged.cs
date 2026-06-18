using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityStatChanged : NetPackageEntityTargeted
{
	public NetPackageEntityStatChanged Setup(EntityAlive entity, int instigatorId, NetPackageEntityStatChanged.EnumStat Estat)
	{
		base.Setup(entity.entityId);
		this.m_instigatorId = instigatorId;
		this.m_enumStat = Estat;
		Stat stat = NetPackageEntityStatChanged.GetStat(entity, Estat);
		this.m_value = stat.Value;
		this.m_max = stat.BaseMax;
		this.m_maxModifier = stat.MaxModifier;
		return this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Stat GetStat(EntityAlive entity, NetPackageEntityStatChanged.EnumStat stat)
	{
		if (stat <= NetPackageEntityStatChanged.EnumStat.Stamina)
		{
			if (stat == NetPackageEntityStatChanged.EnumStat.Health)
			{
				return entity.Stats.Health;
			}
			if (stat == NetPackageEntityStatChanged.EnumStat.Stamina)
			{
				return entity.Stats.Stamina;
			}
		}
		else
		{
			if (stat == NetPackageEntityStatChanged.EnumStat.Food)
			{
				return entity.Stats.Food;
			}
			if (stat == NetPackageEntityStatChanged.EnumStat.Water)
			{
				return entity.Stats.Water;
			}
		}
		return entity.Stats.Health;
	}

	public override void read(PooledBinaryReader _reader)
	{
		base.read(_reader);
		this.m_instigatorId = _reader.ReadInt32();
		this.m_enumStat = (NetPackageEntityStatChanged.EnumStat)_reader.ReadByte();
		this.m_value = _reader.ReadSingle();
		this.m_max = _reader.ReadSingle();
		this.m_maxModifier = _reader.ReadSingle();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.m_instigatorId);
		_writer.Write((byte)this.m_enumStat);
		_writer.Write(this.m_value);
		_writer.Write(this.m_max);
		_writer.Write(this.m_maxModifier);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (this.entityId == _world.GetPrimaryPlayerId() && this.entityId == this.m_instigatorId)
		{
			return;
		}
		if (!base.ValidEntityIdForSender(this.m_instigatorId, false))
		{
			return;
		}
		EntityAlive entityAlive = _world.GetEntity(this.entityId) as EntityAlive;
		if (entityAlive)
		{
			Stat stat = NetPackageEntityStatChanged.GetStat(entityAlive, this.m_enumStat);
			stat.BaseMax = this.m_max;
			stat.MaxModifier = this.m_maxModifier;
			stat.Value = this.m_value;
			stat.Changed = false;
			if (!entityAlive.isEntityRemote && this.m_enumStat == NetPackageEntityStatChanged.EnumStat.Health)
			{
				entityAlive.MinEventContext.Other = _world.GetEntity(this.m_instigatorId) as EntityAlive;
				entityAlive.FireEvent(MinEventTypes.onOtherHealedSelf, true);
			}
			if (!_world.IsRemote())
			{
				_world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(entityAlive.entityId, this.m_instigatorId, NetPackageManager.GetPackage<NetPackageEntityStatChanged>().Setup(entityAlive, this.m_instigatorId, this.m_enumStat), this.m_enumStat > NetPackageEntityStatChanged.EnumStat.Health);
			}
		}
	}

	public override int GetLength()
	{
		return 21;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_instigatorId;

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPackageEntityStatChanged.EnumStat m_enumStat;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_value;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_max;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_maxModifier;

	public enum EnumStat
	{
		Health,
		Stamina,
		Sickness,
		Gassiness,
		SpeedModifier,
		Wellness,
		CoreTempOLD,
		Food,
		Water
	}
}
