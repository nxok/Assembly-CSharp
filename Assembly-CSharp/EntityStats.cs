using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using SandboxOptions;

public class EntityStats
{
	public float AmountEnclosed
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.m_amountEnclosed;
		}
	}

	public EntityStats()
	{
		this.Health = new Stat();
	}

	public EntityStats(EntityAlive _ea)
	{
		this.m_entity = _ea;
		this.Init();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Init()
	{
		int num = (int)EffectManager.GetValue(PassiveEffects.HealthMax, null, 100f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.Health = new Stat(Stat.StatTypes.Health, this.m_entity, (float)num, (float)num)
		{
			MaxPassive = PassiveEffects.HealthMax,
			GainPassive = PassiveEffects.HealthGain,
			LossPassive = PassiveEffects.HealthLoss
		};
	}

	public virtual void CopyFrom(EntityStats _newStats)
	{
		this.Health.CopyFrom(_newStats.Health);
	}

	public virtual EntityStats SimpleClone()
	{
		EntityStats entityStats = new EntityStats();
		entityStats.Health.CopyFrom(this.Health);
		return entityStats;
	}

	public virtual void EntityBuffAdded(BuffValue _buff)
	{
	}

	public virtual void EntityBuffRemoved(BuffValue _buff)
	{
	}

	public void Tick(ulong worldTime)
	{
		if (this.m_entity.isEntityRemote || this.m_entity.IsDead())
		{
			return;
		}
		int num = this.waitTicks + 1;
		this.waitTicks = num;
		if (num >= 10)
		{
			this.waitTicks = 0;
		}
		this.TickWait(worldTime);
	}

	public virtual void TickWait(ulong worldTime)
	{
		float num = 0.5f;
		if (this.waitTicks == 1)
		{
			this.UpdateNPCStatsOverTime(num);
			this.Health.Tick(num);
		}
		if (this.waitTicks == 2 && this.Health.Changed)
		{
			this.SendStatChangePacket(NetPackageEntityStatChanged.EnumStat.Health);
			this.Health.Changed = false;
		}
		if (this.waitTicks == 6)
		{
			if (this.netSyncWaitTicks > 0)
			{
				this.netSyncWaitTicks--;
				return;
			}
			this.netSyncWaitTicks = 10;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityStatsBuff>().Setup(this.m_entity, null), false, -1, -1, -1, null, 192, false);
				return;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityStatsBuff>().Setup(this.m_entity, null), false);
		}
	}

	public void UpdateNPCStatsOverTime(float dt)
	{
		List<EffectManager.ModifierValuesAndSources> valuesAndSources = EffectManager.GetValuesAndSources(PassiveEffects.HealthChangeOT, null, 0f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true);
		for (int i = 0; i < valuesAndSources.Count; i++)
		{
			EffectManager.ModifierValuesAndSources modifierValuesAndSources = valuesAndSources[i];
			if (modifierValuesAndSources.ParentType == MinEffectController.SourceParentType.BuffClass)
			{
				BuffValue buff = this.m_entity.Buffs.GetBuff((string)modifierValuesAndSources.Source);
				if (buff != null && buff.BuffClass != null)
				{
					BuffClass buffClass = buff.BuffClass;
					float num = 0f;
					float num2 = 1f;
					buffClass.ModifyValue(this.m_entity, PassiveEffects.HealthChangeOT, buff, ref num, ref num2, FastTags<TagGroup.Global>.none);
					float num3 = num * num2 * dt;
					if (num3 < 0f)
					{
						float num4 = -num3 + this.buffDamageRemainder;
						int num5 = (int)num4;
						this.buffDamageRemainder = num4 - (float)num5;
						if (num5 > 0)
						{
							DamageSource damageSource = new DamageSource(buffClass.DamageSource, buffClass.DamageType);
							damageSource.BuffClass = buffClass;
							this.m_entity.DamageEntity(damageSource, num5, false, 0f);
						}
					}
					else if (num3 > 0f)
					{
						this.Health.Value += num3;
					}
				}
			}
			else
			{
				this.Health.Value += modifierValuesAndSources.Value * dt;
			}
		}
	}

	public void ResetStats()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendStatChangePacket(NetPackageEntityStatChanged.EnumStat enumStat)
	{
		int num;
		if (GameManager.IsDedicatedServer)
		{
			num = -1;
		}
		else
		{
			num = this.m_entity.world.GetPrimaryPlayer().entityId;
		}
		NetPackageEntityStatChanged netPackageEntityStatChanged = NetPackageManager.GetPackage<NetPackageEntityStatChanged>().Setup(this.m_entity, num, enumStat);
		if (this.m_entity.world.IsRemote())
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(netPackageEntityStatChanged, false);
			return;
		}
		this.m_entity.world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(this.m_entity.entityId, num, netPackageEntityStatChanged, enumStat > NetPackageEntityStatChanged.EnumStat.Health);
	}

	public virtual void Read(BinaryReader stream)
	{
		stream.ReadInt32();
		this.Health.Read(stream);
	}

	public virtual void Write(BinaryWriter stream)
	{
		stream.Write(11);
		this.Health.Write(stream);
	}

	public void UpdateSandboxOptions()
	{
		this.Stamina.GainSandboxModifier = SandboxOptionManager.GetFloat(global::SandboxOptions.SandboxOptions.StaminaRegen);
		this.Stamina.LossSandboxModifier = SandboxOptionManager.GetFloat(global::SandboxOptions.SandboxOptions.StaminaUsage);
	}

	public const int cVersion = 11;

	public static bool WeatherSurvivalEnabled;

	public static bool NewWeatherSurvivalEnabled = true;

	public Stat Health;

	public Stat Stamina;

	public Stat Water;

	public Stat Food;

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityAlive m_entity;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_amountEnclosed;

	[PublicizedFrom(EAccessModifier.Private)]
	public float buffDamageRemainder;

	[PublicizedFrom(EAccessModifier.Protected)]
	public const int cWaitTicks = 10;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int waitTicks;

	[PublicizedFrom(EAccessModifier.Protected)]
	public const int cNetSyncWaitTicks = 10;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int netSyncWaitTicks = 10;
}
