using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerEntityStats : EntityStats
{
	public PlayerEntityStats()
	{
		this.Stamina = new Stat();
		this.Water = new Stat();
		this.Food = new Stat();
	}

	public PlayerEntityStats(EntityPlayer _ea)
		: base(_ea)
	{
		this.localPlayer = _ea as EntityPlayerLocal;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Init()
	{
		int num = (int)EffectManager.GetValue(PassiveEffects.HealthMax, null, 100f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.Health = new Stat(Stat.StatTypes.Health, this.m_entity, (float)num, (float)num)
		{
			GainPassive = PassiveEffects.HealthGain,
			MaxPassive = PassiveEffects.HealthMax,
			LossPassive = PassiveEffects.HealthLoss
		};
		int num2 = (int)EffectManager.GetValue(PassiveEffects.StaminaMax, null, 100f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.Stamina = new Stat(Stat.StatTypes.Stamina, this.m_entity, (float)num2, (float)num2)
		{
			GainPassive = PassiveEffects.StaminaGain,
			MaxPassive = PassiveEffects.StaminaMax,
			LossPassive = PassiveEffects.StaminaLoss
		};
		int num3 = (int)EffectManager.GetValue(PassiveEffects.WaterMax, null, 100f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.Water = new Stat(Stat.StatTypes.Water, this.m_entity, (float)num3, (float)num3)
		{
			GainPassive = PassiveEffects.WaterGain,
			MaxPassive = PassiveEffects.WaterMax,
			LossPassive = PassiveEffects.WaterLoss
		};
		int num4 = (int)EffectManager.GetValue(PassiveEffects.FoodMax, null, 100f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.Food = new Stat(Stat.StatTypes.Food, this.m_entity, (float)num4, (float)num4)
		{
			GainPassive = PassiveEffects.FoodGain,
			MaxPassive = PassiveEffects.FoodMax,
			LossPassive = PassiveEffects.FoodLoss
		};
		this.buffChangedDelegates = new List<IEntityBuffsChanged>();
		this.notificationChangedDelegates = new List<IEntityUINotificationChanged>();
		this.m_notifications = new List<EntityUINotification>();
	}

	public override void CopyFrom(EntityStats _stats)
	{
		PlayerEntityStats playerEntityStats = (PlayerEntityStats)_stats;
		this.Health.CopyFrom(playerEntityStats.Health);
		this.Stamina.CopyFrom(playerEntityStats.Stamina);
		this.Water.CopyFrom(playerEntityStats.Water);
		this.Food.CopyFrom(playerEntityStats.Food);
		this.CoreTemp = playerEntityStats.CoreTemp;
	}

	public override EntityStats SimpleClone()
	{
		PlayerEntityStats playerEntityStats = new PlayerEntityStats();
		playerEntityStats.Health.CopyFrom(this.Health);
		playerEntityStats.Stamina.CopyFrom(this.Stamina);
		playerEntityStats.Water.CopyFrom(this.Water);
		playerEntityStats.Food.CopyFrom(this.Food);
		playerEntityStats.CoreTemp = this.CoreTemp;
		return playerEntityStats;
	}

	public void AddUINotificationChangedDelegate(IEntityUINotificationChanged _uiChangedDelegate)
	{
		if (!this.notificationChangedDelegates.Contains(_uiChangedDelegate))
		{
			this.notificationChangedDelegates.Add(_uiChangedDelegate);
		}
	}

	public void RemoveUINotificationChangedDelegate(IEntityUINotificationChanged _uiChangedDelegate)
	{
		this.notificationChangedDelegates.Remove(_uiChangedDelegate);
	}

	public void AddBuffChangedDelegate(IEntityBuffsChanged _buffChangedDelegate)
	{
		if (!this.buffChangedDelegates.Contains(_buffChangedDelegate))
		{
			this.buffChangedDelegates.Add(_buffChangedDelegate);
		}
	}

	public void RemoveBuffChangedDelegate(IEntityBuffsChanged _buffChangedDelegate)
	{
		this.buffChangedDelegates.Remove(_buffChangedDelegate);
	}

	public override void EntityBuffAdded(BuffValue _buff)
	{
		for (int i = 0; i < this.buffChangedDelegates.Count; i++)
		{
			this.buffChangedDelegates[i].EntityBuffAdded(_buff);
		}
		BuffClass buffClass = _buff.BuffClass;
		if (!buffClass.Hidden && buffClass.Icon != null)
		{
			BuffEntityUINotification buffEntityUINotification = new BuffEntityUINotification(this.m_entity, _buff);
			this.m_notifications.Add(buffEntityUINotification);
			for (int j = 0; j < this.notificationChangedDelegates.Count; j++)
			{
				this.notificationChangedDelegates[j].EntityUINotificationAdded(buffEntityUINotification);
			}
		}
	}

	public override void EntityBuffRemoved(BuffValue _buff)
	{
		for (int i = 0; i < this.buffChangedDelegates.Count; i++)
		{
			this.buffChangedDelegates[i].EntityBuffRemoved(_buff);
		}
		int j = 0;
		while (j < this.m_notifications.Count)
		{
			EntityUINotification entityUINotification = this.m_notifications[j];
			if (entityUINotification.Buff == _buff)
			{
				this.m_notifications.RemoveAt(j);
				for (int k = 0; k < this.notificationChangedDelegates.Count; k++)
				{
					this.notificationChangedDelegates[k].EntityUINotificationRemoved(entityUINotification);
				}
			}
			else
			{
				j++;
			}
		}
	}

	public List<EntityUINotification> Notifications
	{
		get
		{
			return this.m_notifications;
		}
	}

	public override void TickWait(ulong worldTime)
	{
		float num = 0.5f;
		if (this.waitTicks == 1)
		{
			this.UpdateWeatherStats(num, worldTime, this.m_entity.IsGodMode.Value);
		}
		if (this.waitTicks == 2)
		{
			this.UpdatePlayerFoodOT(num);
			this.UpdatePlayerWaterOT(num);
		}
		if (this.waitTicks == 3)
		{
			this.UpdatePlayerHealthOT(num);
		}
		if (this.waitTicks == 4)
		{
			this.UpdatePlayerStaminaOT(num);
		}
		if (this.waitTicks == 5)
		{
			if (this.Health.Changed)
			{
				this.SendStatChangePacket(NetPackageEntityStatChanged.EnumStat.Health);
				this.Health.Changed = false;
			}
			if (this.Stamina.Changed)
			{
				this.SendStatChangePacket(NetPackageEntityStatChanged.EnumStat.Stamina);
				this.Stamina.Changed = false;
			}
			if (this.Water.Changed)
			{
				this.SendStatChangePacket(NetPackageEntityStatChanged.EnumStat.Water);
				this.Water.Changed = false;
			}
			if (this.Food.Changed)
			{
				this.SendStatChangePacket(NetPackageEntityStatChanged.EnumStat.Food);
				this.Food.Changed = false;
			}
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateWeatherStats(float dt, ulong worldTime, bool godMode)
	{
		this.m_amountEnclosed = this.m_entity.GetAmountEnclosed();
		this.LightInsidePer = this.m_amountEnclosed;
		this.shadePer = this.CalcShadeFromSunPercent();
		EntityAlive entity = this.m_entity;
		EntityBuffs buffs = entity.Buffs;
		float wetnessRate = entity.GetWetnessRate();
		buffs.SetCustomVar("_wetnessrate", wetnessRate, true, CVarOperation.set);
		float num = this.GetOutsideTemperature();
		buffs.SetCustomVar("_outsidetemp", num, true, CVarOperation.set);
		float num2 = (this.localPlayer ? this.localPlayer.shelterPercent : 0f);
		buffs.SetCustomVar("_sheltered", num2, true, CVarOperation.set);
		if (!World.TemperatureSurvival || !EntityStats.WeatherSurvivalEnabled || WeatherManager.inWeatherGracePeriod || entity.biomeStandingOn == null || entity.IsGodMode.Value || buffs.HasBuff("god"))
		{
			buffs.SetCustomVar("_degreesabsorbed", 0f, true, CVarOperation.set);
			buffs.SetCustomVar("_coretemp", 70f, true, CVarOperation.set);
			buffs.SetCustomVar("_shaded", this.shadePer, true, CVarOperation.set);
			return;
		}
		float num3;
		if (num < 70f)
		{
			num3 = EffectManager.GetValue(PassiveEffects.HypothermalResist, null, 0f, entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			num = Utils.FastMin(70f, num + num3);
		}
		else
		{
			num3 = EffectManager.GetValue(PassiveEffects.HyperthermalResist, null, 0f, entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			num = Utils.FastMax(70f, num - num3);
		}
		this.CoreTemp = Utils.FastMoveTowards(this.CoreTemp, num, 1f);
		buffs.SetCustomVar("_degreesabsorbed", num3, true, CVarOperation.set);
		buffs.SetCustomVar("_coretemp", this.CoreTemp, true, CVarOperation.set);
		buffs.SetCustomVar("_shaded", this.shadePer, true, CVarOperation.set);
	}

	public void UpdatePlayerFoodOT(float dt)
	{
		this.Food.RegenerationAmount += EffectManager.GetValue(PassiveEffects.FoodChangeOT, null, 0f, this.m_entity, null, this.m_entity.CurrentMovementTag, true, true, true, true, true, 1, true, false) * dt;
		this.Food.MaxModifier = -EffectManager.GetValue(PassiveEffects.FoodMaxBlockage, null, 0f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.Food.Tick(dt);
		this.Food.RegenerationAmount = 0f;
	}

	public void UpdatePlayerWaterOT(float dt)
	{
		this.Water.RegenerationAmount += EffectManager.GetValue(PassiveEffects.WaterChangeOT, null, 0f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) * dt;
		this.Water.MaxModifier = -EffectManager.GetValue(PassiveEffects.WaterMaxBlockage, null, 0f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.Water.Tick(dt);
		this.Water.RegenerationAmount = 0f;
	}

	public void UpdatePlayerHealthOT(float dt)
	{
		float num = EffectManager.GetValue(PassiveEffects.HealthChangeOT, null, 0f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		if (this.Health.ValuePercent < 1f && num > 0f)
		{
			float waterPercent = this.GetWaterPercent();
			this.Health.RegenerationAmount = num * waterPercent * dt;
		}
		else if (num < 0f)
		{
			List<EffectManager.ModifierValuesAndSources> valuesAndSources = EffectManager.GetValuesAndSources(PassiveEffects.HealthChangeOT, null, 0f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true);
			float num2 = 0f;
			float num3 = 1f;
			for (int i = 0; i < valuesAndSources.Count; i++)
			{
				if (valuesAndSources[i].ParentType == MinEffectController.SourceParentType.BuffClass)
				{
					BuffValue buff = this.m_entity.Buffs.GetBuff((string)valuesAndSources[i].Source);
					if (buff != null && buff.BuffClass != null && !buff.Remove)
					{
						BuffClass buffClass = buff.BuffClass;
						num2 = 0f;
						num3 = 1f;
						buffClass.ModifyValue(this.m_entity, PassiveEffects.HealthChangeOT, buff, ref num2, ref num3, FastTags<TagGroup.Global>.none);
						num = num2 * num3 * dt;
						if (num < 0f)
						{
							DamageSourceEntity damageSourceEntity = new DamageSourceEntity(buffClass.DamageSource, buffClass.DamageType, buff.InstigatorId);
							damageSourceEntity.BuffClass = buffClass;
							this.m_entity.DamageEntity(damageSourceEntity, (int)(-num + 0.5f), false, 0f);
						}
					}
				}
				else
				{
					this.Health.RegenerationAmount = valuesAndSources[i].Value * dt;
				}
			}
		}
		this.Health.MaxModifier = -EffectManager.GetValue(PassiveEffects.HealthMaxBlockage, null, 0f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.Health.Tick(dt);
	}

	public void UpdatePlayerStaminaOT(float _dt)
	{
		float value = EffectManager.GetValue(PassiveEffects.StaminaChangeOT, null, 0f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		if (this.Stamina.ValuePercent < 1f && value > 0f)
		{
			this.Stamina.RegenerationAmount = value * _dt;
		}
		else if (value < 0f)
		{
			this.Stamina.RegenerationAmount = value * _dt;
		}
		float num = EffectManager.GetValue(PassiveEffects.StaminaChangeOT, null, this.Stamina.RegenerationAmount, this.m_entity, null, this.m_entity.CurrentMovementTag | this.m_entity.CurrentStanceTag, true, true, true, true, true, 1, true, false);
		if (num > 0f)
		{
			float num2 = this.GetWaterPercent();
			num2 = Utils.FastMax(0.2f, num2);
			num *= num2;
		}
		this.Stamina.RegenerationAmount = num * _dt;
		if (this.Stamina.RegenerationAmount > 0f)
		{
			this.Stamina.RegenerationAmount *= this.Stamina.GainSandboxModifier;
		}
		this.Stamina.MaxModifier = -EffectManager.GetValue(PassiveEffects.StaminaMaxBlockage, null, 0f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.Stamina.Tick(_dt);
	}

	public float GetOutsideTemperature()
	{
		float num = WeatherManager.GetTemperature();
		num += Utils.FastLerp(0f, -20f, (this.m_entity.position.y - 130f) / 100f);
		float sunPercent = SkyManager.GetSunPercent();
		if (sunPercent > 0.25f)
		{
			num += this.shadePer * sunPercent * -8f;
		}
		if (this.localPlayer.shelterPercent < 0.5f)
		{
			float num2 = WeatherManager.GetWindSpeed() * 0.01f * -24f;
			num += num2;
		}
		float num3;
		if (num < 70f)
		{
			num3 = Utils.FastMin(70f, num + 30f);
		}
		else
		{
			num3 = Utils.FastMax(70f, num - 30f);
		}
		return Utils.FastLerpUnclamped(num, num3, this.localPlayer.shelterPercent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalcShadeFromSunPercent()
	{
		Vector3 sunLightDirection = SkyManager.GetSunLightDirection();
		if (sunLightDirection.y > -0.25f)
		{
			return 1f;
		}
		RaycastHit raycastHit;
		if (Physics.SphereCast(new Ray(this.m_entity.getHeadPosition() - Origin.position + sunLightDirection * 0.15f, -sunLightDirection), 0.15f, ref raycastHit, float.PositiveInfinity, 65809))
		{
			return 1f;
		}
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetWaterPercent()
	{
		float num = this.Water.ValuePercentUI * (this.Water.Max * 0.01f);
		if (num != 0f)
		{
			if (num < 0.25f)
			{
				num = 0.25f;
			}
			else if (num < 0.5f)
			{
				num = 0.5f;
			}
			else
			{
				num = 1f;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new void SendStatChangePacket(NetPackageEntityStatChanged.EnumStat enumStat)
	{
		NetPackageEntityStatChanged netPackageEntityStatChanged = NetPackageManager.GetPackage<NetPackageEntityStatChanged>().Setup(this.m_entity, this.localPlayer.entityId, enumStat);
		if (this.m_entity.world.IsRemote())
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(netPackageEntityStatChanged, false);
			return;
		}
		this.m_entity.world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(this.m_entity.entityId, -1, netPackageEntityStatChanged, enumStat > NetPackageEntityStatChanged.EnumStat.Health);
	}

	public override void Read(BinaryReader stream)
	{
		int num = stream.ReadInt32();
		this.Health.Read(stream);
		this.Stamina.Read(stream);
		if (num <= 10)
		{
			new Stat(Stat.StatTypes.Health, null, 0f, 0f).Read(stream);
		}
		this.Water.Read(stream);
		this.Food.Read(stream);
		if (num >= 11)
		{
			this.CoreTemp = (float)(stream.ReadSByte() * 2);
		}
	}

	public override void Write(BinaryWriter stream)
	{
		stream.Write(11);
		this.Health.Write(stream);
		this.Stamina.Write(stream);
		this.Water.Write(stream);
		this.Food.Write(stream);
		stream.Write((sbyte)(this.CoreTemp / 2f));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<IEntityBuffsChanged> buffChangedDelegates;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<IEntityUINotificationChanged> notificationChangedDelegates;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityUINotification> m_notifications;

	public float CoreTemp = 70f;

	public float LightInsidePer;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float shadePer;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRaycastMask = 65809;
}
