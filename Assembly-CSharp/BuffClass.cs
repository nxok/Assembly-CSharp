using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BuffClass
{
	public float DurationMax
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.durationMax;
		}
		set
		{
			if (this.initialDurationMax == 0f && value > 0f)
			{
				this.initialDurationMax = value;
			}
			this.durationMax = value;
		}
	}

	public float InitialDurationMax
	{
		get
		{
			return this.initialDurationMax;
		}
	}

	public BuffClass(string _name = "")
	{
		this.Name = _name.ToLower();
		this.LocalizedName = string.Empty;
		this.DescriptionKey = string.Empty;
		this.TooltipKey = string.Empty;
		this.Icon = string.Empty;
		this.IconBlink = false;
		this.Requirements = null;
		this.Hidden = false;
		this.DamageType = EnumDamageTypes.None;
		this.StackType = BuffEffectStackTypes.Replace;
		this.durationMax = 0f;
		this.initialDurationMax = 0f;
	}

	public void Tick(BuffValue _ev)
	{
		_ev.DurationTick();
		if (this.DurationMax > 0f && _ev.DurationInSeconds >= this.DurationMax)
		{
			_ev.Finished = true;
		}
	}

	public void ModifyValue(EntityAlive _self, PassiveEffects _effect, BuffValue _bv, ref float _base_value, ref float _perc_value, FastTags<TagGroup.Global> _tags)
	{
		if (_bv.Remove)
		{
			return;
		}
		_self.MinEventContext.Tags |= _tags;
		if (!this.canRun(_self.MinEventContext))
		{
			return;
		}
		if (this.Effects != null)
		{
			this.Effects.ModifyValue(_self, _effect, ref _base_value, ref _perc_value, _bv.DurationInSeconds, _tags, (this.StackType == BuffEffectStackTypes.Effect) ? _bv.StackEffectMultiplier : 1);
		}
	}

	public void GetModifiedValueData(List<EffectManager.ModifierValuesAndSources> _modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType _sourceType, EntityAlive _self, PassiveEffects _effect, BuffValue _bv, ref float _base_value, ref float _perc_value, FastTags<TagGroup.Global> _tags)
	{
		if (_bv.Remove)
		{
			return;
		}
		_self.MinEventContext.Tags |= _tags;
		if (!this.canRun(_self.MinEventContext))
		{
			return;
		}
		if (this.Effects != null)
		{
			this.Effects.GetModifiedValueData(_modValueSources, _sourceType, _self, _effect, ref _base_value, ref _perc_value, _bv.DurationInSeconds, _tags, (this.StackType == BuffEffectStackTypes.Effect) ? _bv.StackEffectMultiplier : 1);
		}
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _params)
	{
		if (this.Effects == null)
		{
			return;
		}
		if (!this.canRun(_params))
		{
			return;
		}
		this.Effects.FireEvent(_eventType, _params);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canRun(MinEventParams _params)
	{
		return this.Requirements == null || this.Requirements.IsValid(_params);
	}

	public string Name;

	public string LocalizedName;

	public string Description;

	public string DescriptionKey;

	public string Tooltip;

	public string TooltipKey;

	public string Icon;

	public string DisplayValueCVar;

	public string DisplayValueKey;

	public BuffClass.CVarDisplayFormat DisplayValueFormat;

	public Color IconColor;

	public bool IconBlink;

	public EnumEntityUINotificationDisplayMode DisplayType;

	public RequirementGroup Requirements;

	public MinEffectController Effects;

	public bool Hidden;

	public bool ShowOnHUD = true;

	public bool AllowInEditor;

	public EnumGameStats RequiredGameStat = EnumGameStats.Last;

	[PublicizedFrom(EAccessModifier.Private)]
	public float durationMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public float initialDurationMax;

	public int UpdateRateTicks = 20;

	public EnumDamageTypes DamageType;

	public EnumDamageSource DamageSource;

	public BuffEffectStackTypes StackType;

	public bool RemoveOnDeath = true;

	public FastTags<TagGroup.Global> NameTag;

	public FastTags<TagGroup.Global> Tags = FastTags<TagGroup.Global>.none;

	public enum CVarDisplayFormat
	{
		None,
		Float,
		FlooredToInt,
		RoundedToInt,
		CeiledToInt,
		Time,
		Percentage,
		Degrees
	}
}
