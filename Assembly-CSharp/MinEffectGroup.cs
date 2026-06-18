using System;
using System.Collections.Generic;
using System.Xml.Linq;

public class MinEffectGroup
{
	public MinEffectGroup()
	{
		this.Requirements = null;
		this.PassiveEffects = new List<PassiveEffect>();
		this.TriggeredEffects = new Dictionary<MinEventTypes, List<MinEventActionBase>>();
		this.EffectDescriptions = new List<EffectGroupDescription>();
		this.OwnerTiered = true;
		this.PassivesIndices = new List<PassiveEffects>();
		this.EffectDisplayValues = new CaseInsensitiveStringDictionary<EffectDisplayValue>();
	}

	public void ModifyValue(MinEventParams _params, EntityAlive _self, PassiveEffects _effect, ref float _base_value, ref float _perc_value, float level, FastTags<TagGroup.Global> _tags, int _multiplier = 1)
	{
		if (!this.canRun(_params))
		{
			return;
		}
		int count = this.PassiveEffects.Count;
		for (int i = 0; i < count; i++)
		{
			PassiveEffect passiveEffect = this.PassiveEffects[i];
			if (passiveEffect.Type == _effect && passiveEffect.RequirementsMet(_params))
			{
				passiveEffect.ModifyValue(_self, level, ref _base_value, ref _perc_value, _tags, _multiplier);
			}
		}
	}

	public void GetModifiedValueData(List<EffectManager.ModifierValuesAndSources> _modValueSource, EffectManager.ModifierValuesAndSources.ValueSourceType _sourceType, MinEffectController.SourceParentType _parentType, EntityAlive _self, PassiveEffects _effect, ref float _base_value, ref float _perc_value, float level, FastTags<TagGroup.Global> _tags, int _multiplier = 1, object _parentPointer = null)
	{
		MinEventParams minEventParams;
		if (_self == null)
		{
			minEventParams = MinEventParams.CachedEventParam;
			minEventParams.Self = null;
		}
		else
		{
			minEventParams = _self.MinEventContext;
		}
		minEventParams.Tags = _tags;
		if (!this.canRun(minEventParams))
		{
			return;
		}
		for (int i = 0; i < this.PassiveEffects.Count; i++)
		{
			PassiveEffect passiveEffect = this.PassiveEffects[i];
			if (passiveEffect.Type == _effect && passiveEffect.RequirementsMet(minEventParams))
			{
				passiveEffect.GetModifiedValueData(_modValueSource, _sourceType, _parentType, _self, level, ref _base_value, ref _perc_value, _tags, _multiplier, _parentPointer);
			}
		}
	}

	public IReadOnlyList<MinEventActionBase> GetTriggeredEffects(MinEventTypes _eventType)
	{
		List<MinEventActionBase> list;
		if (!this.TriggeredEffects.TryGetValue(_eventType, out list))
		{
			return Array.Empty<MinEventActionBase>();
		}
		return list;
	}

	public void AddTriggeredEffect(MinEventActionBase triggeredEffect)
	{
		MinEventTypes eventType = triggeredEffect.EventType;
		List<MinEventActionBase> list;
		if (!this.TriggeredEffects.TryGetValue(eventType, out list))
		{
			list = new List<MinEventActionBase>();
			this.TriggeredEffects.Add(eventType, list);
		}
		list.Add(triggeredEffect);
	}

	public bool HasEvents()
	{
		return this.TriggeredEffects.Count > 0;
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _eventParms)
	{
		if (this.TriggeredEffects.Count <= 0)
		{
			return;
		}
		IReadOnlyList<MinEventActionBase> triggeredEffects = this.GetTriggeredEffects(_eventType);
		if (triggeredEffects.Count <= 0)
		{
			return;
		}
		if (!this.canRun(_eventParms))
		{
			return;
		}
		for (int i = 0; i < triggeredEffects.Count; i++)
		{
			MinEventActionBase minEventActionBase = triggeredEffects[i];
			if (minEventActionBase.CanExecute(_eventType, _eventParms))
			{
				minEventActionBase.Execute(_eventParms);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canRun(MinEventParams _params)
	{
		return this.Requirements == null || this.Requirements.IsValid(_params);
	}

	public bool HasTrigger(MinEventTypes _eventType)
	{
		return this.GetTriggeredEffects(_eventType).Count > 0;
	}

	public static MinEffectGroup ParseXml(XElement _element)
	{
		MinEffectGroup minEffectGroup = new MinEffectGroup();
		if (_element.HasAttribute("tiered"))
		{
			minEffectGroup.OwnerTiered = StringParsers.ParseBool(_element.GetAttribute("tiered"), 0, -1, true);
		}
		minEffectGroup.Requirements = RequirementBase.ParseRequirementGroup(_element);
		foreach (XElement xelement in _element.Elements())
		{
			if (xelement.Name == "passive_effect")
			{
				PassiveEffect passiveEffect = PassiveEffect.ParsePassiveEffect(xelement);
				if (passiveEffect != null)
				{
					MinEffectGroup.AddPassiveEffectToGroup(minEffectGroup, passiveEffect);
				}
			}
			else if (xelement.Name == "triggered_effect")
			{
				MinEventActionBase minEventActionBase = MinEventActionBase.ParseAction(xelement);
				if (minEventActionBase != null)
				{
					minEffectGroup.AddTriggeredEffect(minEventActionBase);
				}
			}
			else if (xelement.Name == "effect_description")
			{
				EffectGroupDescription effectGroupDescription = EffectGroupDescription.ParseDescription(xelement);
				if (effectGroupDescription != null)
				{
					minEffectGroup.EffectDescriptions.Add(effectGroupDescription);
				}
			}
			else if (xelement.Name == "display_value")
			{
				EffectDisplayValue effectDisplayValue = EffectDisplayValue.ParseDisplayValue(xelement);
				if (effectDisplayValue != null)
				{
					minEffectGroup.EffectDisplayValues.Add(effectDisplayValue.Name, effectDisplayValue);
				}
			}
		}
		return minEffectGroup;
	}

	public static void AddPassiveEffectToGroup(MinEffectGroup _effectGroup, PassiveEffect _pe)
	{
		_effectGroup.PassivesIndices.Add(_pe.Type);
		_effectGroup.PassiveEffects.Add(_pe);
	}

	public RequirementGroup Requirements;

	public List<PassiveEffect> PassiveEffects;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<MinEventTypes, List<MinEventActionBase>> TriggeredEffects;

	public List<EffectGroupDescription> EffectDescriptions;

	public CaseInsensitiveStringDictionary<EffectDisplayValue> EffectDisplayValues;

	public bool OwnerTiered;

	public List<PassiveEffects> PassivesIndices;
}
