using System;
using System.IO;
using System.Runtime.CompilerServices;

public sealed class Stat
{
	public Stat()
	{
	}

	public Stat(Stat.StatTypes _statType, EntityAlive _entity, float _value, float _baseMax)
	{
		this.StatType = _statType;
		this.Entity = _entity;
		this.m_value = _value;
		this.m_originalValue = _value;
		this.m_lastValue = _value;
		this.m_baseMax = _baseMax;
		this.m_originalBaseMax = _baseMax;
		this.m_maxModifier = 0f;
		this.GainSandboxModifier = 1f;
		this.LossSandboxModifier = 1f;
		this.m_changed = false;
	}

	public void Tick(float dt)
	{
		if (this.MaxPassive != PassiveEffects.None)
		{
			this.BaseMax = EffectManager.GetValue(this.MaxPassive, null, this.m_originalBaseMax, this.Entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		}
		if ((this.StatType == Stat.StatTypes.Health || this.StatType == Stat.StatTypes.Stamina) && Utils.FastAbs(this.m_lastValue - this.m_value) >= 1f)
		{
			if (this.m_value > this.m_lastValue && this.GainPassive != PassiveEffects.None)
			{
				this.m_value = Utils.FastClamp(this.m_lastValue + EffectManager.GetValue(this.GainPassive, null, this.m_value - this.m_lastValue, this.Entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false), 0f, this.m_baseMax);
			}
			else if (this.m_value < this.m_lastValue && this.LossPassive != PassiveEffects.None)
			{
				this.m_value = Utils.FastClamp(this.m_lastValue - EffectManager.GetValue(this.LossPassive, null, this.m_lastValue - this.m_value, this.Entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false), 0f, this.m_baseMax);
			}
		}
		if (this.m_value + this.regenAmount > this.ModifiedMax)
		{
			this.regenAmount = this.ModifiedMax - this.m_value;
		}
		this.RegenerationAmountUI = this.m_value - this.m_lastValue + this.regenAmount / dt;
		this.m_value += this.regenAmount;
		if (this.regenAmount > 0f)
		{
			if (this.StatType == Stat.StatTypes.Stamina)
			{
				this.Entity.Stats.Water.RegenerationAmount -= this.regenAmount * EffectManager.GetValue(PassiveEffects.WaterLossPerStaminaPointGained, null, 1f, this.Entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
				this.Entity.Stats.Food.RegenerationAmount -= this.regenAmount * EffectManager.GetValue(PassiveEffects.FoodLossPerStaminaPointGained, null, 1f, this.Entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			}
			else if (this.StatType == Stat.StatTypes.Health)
			{
				this.Entity.Stats.Water.RegenerationAmount -= this.regenAmount * EffectManager.GetValue(PassiveEffects.WaterLossPerHealthPointGained, null, 1f, this.Entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
				this.Entity.Stats.Food.RegenerationAmount -= this.regenAmount * EffectManager.GetValue(PassiveEffects.FoodLossPerHealthPointGained, null, 1f, this.Entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			}
		}
		this.regenAmount = this.m_value - this.m_lastValue;
		this.SetChangedFlag(this.m_value, this.m_lastValue);
		this.m_lastValue = this.m_value;
	}

	public void ResetValue()
	{
		this.m_value = this.m_originalValue;
		this.m_baseMax = this.m_originalBaseMax;
		this.m_maxModifier = 0f;
		this.m_changed = true;
	}

	public void CopyFrom(Stat _stat)
	{
		this.m_value = _stat.m_value;
		this.m_originalValue = _stat.m_originalValue;
		this.m_baseMax = _stat.m_baseMax;
		this.m_originalBaseMax = _stat.m_originalBaseMax;
		this.m_maxModifier = _stat.m_maxModifier;
		this.m_changed = false;
	}

	public float RegenerationAmount
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.regenAmount;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			this.regenAmount = value;
		}
	}

	public float RegenerationAmountUI
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public float Max
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.m_baseMax;
		}
	}

	public float BaseMax
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.m_baseMax;
		}
		set
		{
			if (this.m_baseMax != value)
			{
				this.SetChangedFlag(this.m_baseMax, value);
				this.m_baseMax = value;
			}
		}
	}

	public float ModifiedMax
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.m_baseMax + this.m_maxModifier;
		}
	}

	public float Value
	{
		get
		{
			if (this.GodModeEntity())
			{
				return this.ModifiedMax;
			}
			return Utils.FastClamp(this.m_value, 0f, this.ModifiedMax);
		}
		set
		{
			if (this.m_value != value)
			{
				float value2 = this.m_value;
				this.m_value = Utils.FastClamp(value, 0f, this.ModifiedMax);
				this.SetChangedFlag(value2, value);
			}
		}
	}

	public float ValuePercent
	{
		get
		{
			return Utils.FastClamp01(this.Value / this.ModifiedMax);
		}
	}

	public float ValuePercentUI
	{
		get
		{
			return Utils.FastClamp01(this.Value / this.Max);
		}
	}

	public float ModifiedMaxPercent
	{
		get
		{
			return Utils.FastClamp01(this.ModifiedMax / this.Max);
		}
	}

	public float MaxModifier
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.m_maxModifier;
		}
		set
		{
			this.m_maxModifier = Utils.FastClamp(value, -(this.Max * 0.75f), 0f);
		}
	}

	public float OriginalValue
	{
		get
		{
			return this.m_originalValue;
		}
		set
		{
			this.m_originalValue = value;
		}
	}

	public float OriginalMax
	{
		get
		{
			return this.m_originalBaseMax;
		}
		set
		{
			this.m_originalBaseMax = value;
		}
	}

	public bool Changed
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.m_changed;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			this.m_changed = value;
		}
	}

	public void Write(BinaryWriter stream)
	{
		stream.Write(6);
		stream.Write(this.m_value);
		stream.Write(this.m_maxModifier);
		stream.Write(this.m_baseMax);
		stream.Write(this.m_originalBaseMax);
		stream.Write(this.m_originalValue);
	}

	public void Read(BinaryReader stream)
	{
		int num = stream.ReadInt32();
		this.m_value = stream.ReadSingle();
		this.m_maxModifier = stream.ReadSingle();
		if (num <= 5)
		{
			stream.ReadSingle();
		}
		this.m_baseMax = stream.ReadSingle();
		this.m_originalBaseMax = stream.ReadSingle();
		this.m_originalValue = stream.ReadSingle();
		this.m_lastValue = this.m_value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool GodModeEntity()
	{
		return this.Entity && this.Entity.entityId == this.Entity.world.GetPrimaryPlayerId() && !GameStats.GetBool(EnumGameStats.IsPlayerDamageEnabled);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetChangedFlag(float newValue, float oldValue)
	{
		this.m_changed = this.m_changed || Utils.Fastfloor(newValue) != Utils.Fastfloor(oldValue);
	}

	public const int cBinaryVersion = 6;

	public Stat.StatTypes StatType;

	public PassiveEffects GainPassive;

	public PassiveEffects LossPassive;

	public PassiveEffects MaxPassive;

	public EntityAlive Entity;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_value;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_originalValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_lastValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_baseMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_originalBaseMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_maxModifier;

	public float GainSandboxModifier;

	public float LossSandboxModifier;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_changed;

	[PublicizedFrom(EAccessModifier.Private)]
	public float regenAmount;

	public enum StatTypes
	{
		Health,
		Stamina,
		Food,
		Water,
		CoreTemp
	}
}
