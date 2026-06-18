using System;
using System.Collections.Generic;
using UnityEngine;

public static class EffectManager
{
	public static float GetValue(PassiveEffects _passiveEffect, ItemValue _originalItemValue = null, float _originalValue = 0f, EntityAlive _entity = null, Recipe _recipe = null, FastTags<TagGroup.Global> tags = default(FastTags<TagGroup.Global>), bool calcEquipment = true, bool calcHoldingItem = true, bool calcProgression = true, bool calcBuffs = true, bool calcChallenges = true, int craftingTier = 1, bool useMods = true, bool _useDurability = false)
	{
		float num = 1f;
		if (_entity != null)
		{
			MinEventParams.CopyTo(_entity.MinEventContext, MinEventParams.CachedEventParam);
		}
		if (_originalItemValue != null)
		{
			if (_entity != null && _entity.MinEventContext.ItemValue == null)
			{
				_entity.MinEventContext.ItemValue = _originalItemValue;
			}
			MinEventParams.CachedEventParam.ItemValue = _originalItemValue;
			if (_originalItemValue.type != 0 && tags.IsEmpty)
			{
				ItemClass itemClass = _originalItemValue.ItemClass;
				if (itemClass != null)
				{
					tags = itemClass.ItemTags;
				}
			}
		}
		if (_entity == null)
		{
			if (_recipe != null)
			{
				_recipe.ModifyValue(_passiveEffect, ref _originalValue, ref num, tags, craftingTier);
			}
			if (_originalItemValue != null && _originalItemValue.type != 0 && _originalItemValue.ItemClass != null)
			{
				_originalItemValue.ModifyValue(_entity, null, _passiveEffect, ref _originalValue, ref num, tags, true, false);
			}
		}
		else
		{
			if (GameManager.Instance == null || GameManager.Instance.gameStateManager == null || !GameManager.Instance.gameStateManager.IsGameStarted())
			{
				return _originalValue;
			}
			EntityClass entityClass;
			if (EntityClass.list.TryGetValue(_entity.entityClass, out entityClass) && entityClass.Effects != null)
			{
				entityClass.Effects.ModifyValue(_entity, _passiveEffect, ref _originalValue, ref num, 0f, tags, 1);
			}
			if (_originalItemValue != null && _originalItemValue.type != 0 && _originalItemValue.ItemClass != null)
			{
				_originalItemValue.ModifyValue(_entity, null, _passiveEffect, ref _originalValue, ref num, tags, useMods, false);
			}
			else
			{
				EntityVehicle entityVehicle = _entity as EntityVehicle;
				if (entityVehicle != null)
				{
					Vehicle vehicle = entityVehicle.GetVehicle();
					if (vehicle != null)
					{
						vehicle.GetUpdatedItemValue().ModifyValue(_entity, null, _passiveEffect, ref _originalValue, ref num, tags, true, false);
					}
				}
				else if (calcHoldingItem && _entity.inventory != null && _entity.inventory.holdingItemItemValue != _originalItemValue && !_entity.inventory.holdingItemItemValue.IsMod)
				{
					_entity.inventory.ModifyValue(_originalItemValue, _passiveEffect, ref _originalValue, ref num, tags);
				}
			}
			if (calcEquipment && _entity.equipment != null)
			{
				_entity.equipment.ModifyValue(_originalItemValue, _passiveEffect, ref _originalValue, ref num, tags, _useDurability);
			}
			if (_originalItemValue != null)
			{
				if (_entity != null)
				{
					_entity.MinEventContext.ItemValue = _originalItemValue;
				}
				MinEventParams.CachedEventParam.ItemValue = _originalItemValue;
			}
			if (calcProgression && _entity.Progression != null)
			{
				_entity.Progression.ModifyValue(_passiveEffect, ref _originalValue, ref num, tags);
			}
			if (calcChallenges && _entity.challengeJournal != null)
			{
				_entity.challengeJournal.ModifyValue(_passiveEffect, ref _originalValue, ref num, tags);
			}
			if (_recipe != null)
			{
				_recipe.ModifyValue(_passiveEffect, ref _originalValue, ref num, tags, craftingTier);
			}
			EntityPlayerLocal entityPlayerLocal = _entity as EntityPlayerLocal;
			if (entityPlayerLocal != null)
			{
				if (EffectManager.slotsCached == null || _entity.entityId != EffectManager.slotsQueriedForEntity || EffectManager.slotsQueriedFrame != Time.frameCount)
				{
					LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
					EffectManager.slotsCached = ((uiforPlayer.xui.CurrentWorkstationToolGrid != null) ? uiforPlayer.xui.CurrentWorkstationToolGrid.GetSlots() : null);
					EffectManager.slotsQueriedFrame = Time.frameCount;
					EffectManager.slotsQueriedForEntity = _entity.entityId;
				}
				if (EffectManager.slotsCached != null)
				{
					for (int i = 0; i < EffectManager.slotsCached.Length; i++)
					{
						if (!EffectManager.slotsCached[i].IsEmpty())
						{
							EffectManager.slotsCached[i].itemValue.ModifyValue(_entity, null, _passiveEffect, ref _originalValue, ref num, tags, true, false);
						}
					}
				}
			}
			if (calcBuffs && _entity.Buffs != null)
			{
				_entity.Buffs.ModifyValue(_passiveEffect, ref _originalValue, ref num, tags);
			}
		}
		if (_originalItemValue != null && _originalItemValue.ItemClass != null && _originalItemValue.Quality > 0 && useMods && _originalItemValue.ItemClass.Effects != null)
		{
			for (int j = 0; j < _originalItemValue.Modifications.Length; j++)
			{
				if (_originalItemValue.Modifications[j] != null && _originalItemValue.Modifications[j].ItemClass is ItemClassModifier)
				{
					_originalItemValue.ItemClass.Effects.ModifyValue(_entity, PassiveEffects.ModPowerBonus, ref _originalValue, ref num, (float)_originalItemValue.Quality, FastTags<TagGroup.Global>.Parse(_passiveEffect.ToStringCached<PassiveEffects>()), 1);
				}
			}
		}
		return _originalValue * num;
	}

	public static float GetItemValue(PassiveEffects _passiveEffect, ItemValue _originalItemValue, float _originalValue = 0f)
	{
		float num = 1f;
		if (_originalItemValue != null && _originalItemValue.type != 0 && _originalItemValue.ItemClass != null)
		{
			MinEventParams.CachedEventParam.ItemValue = _originalItemValue;
			_originalItemValue.ModifyValue(null, null, _passiveEffect, ref _originalValue, ref num, _originalItemValue.ItemClass.ItemTags, true, false);
		}
		return _originalValue * num;
	}

	public static float GetDisplayValues(PassiveEffects _passiveEffect, out float baseValueChange, out float percValueMultiplier, ItemValue _originalItemValue = null, float _originalValue = 0f, EntityAlive _entity = null, Recipe _recipe = null, FastTags<TagGroup.Global> tags = default(FastTags<TagGroup.Global>), int craftingTier = 1)
	{
		float num = _originalValue;
		baseValueChange = 0f;
		percValueMultiplier = 1f;
		if (GameManager.Instance == null || GameManager.Instance.gameStateManager == null || !GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return _originalValue;
		}
		if (_entity == null)
		{
			if (_recipe != null)
			{
				_recipe.ModifyValue(_passiveEffect, ref baseValueChange, ref percValueMultiplier, tags, craftingTier);
			}
			if (_originalItemValue != null && _originalItemValue.type != 0 && _originalItemValue.ItemClass != null)
			{
				_originalItemValue.ModifyValue(_entity, null, _passiveEffect, ref _originalValue, ref percValueMultiplier, tags, true, false);
			}
		}
		else
		{
			if (EntityClass.list.ContainsKey(_entity.entityClass) && EntityClass.list[_entity.entityClass].Effects != null)
			{
				EntityClass.list[_entity.entityClass].Effects.ModifyValue(_entity, _passiveEffect, ref _originalValue, ref percValueMultiplier, 0f, tags, 1);
			}
			if (_originalItemValue != null && _originalItemValue.type != 0 && _originalItemValue.ItemClass != null)
			{
				_originalItemValue.ModifyValue(_entity, null, _passiveEffect, ref _originalValue, ref percValueMultiplier, tags, true, false);
			}
			else
			{
				if (_entity.inventory != null && _entity.inventory.holdingItemItemValue != _originalItemValue)
				{
					_entity.inventory.ModifyValue(_originalItemValue, _passiveEffect, ref _originalValue, ref percValueMultiplier, tags);
				}
				if (_entity.equipment != null)
				{
					_entity.equipment.ModifyValue(_originalItemValue, _passiveEffect, ref _originalValue, ref percValueMultiplier, tags, false);
				}
			}
			if (_entity.Progression != null)
			{
				_entity.Progression.ModifyValue(_passiveEffect, ref _originalValue, ref percValueMultiplier, tags);
			}
			if (_recipe != null)
			{
				_recipe.ModifyValue(_passiveEffect, ref baseValueChange, ref percValueMultiplier, tags, craftingTier);
			}
			if (_entity.Buffs != null)
			{
				_entity.Buffs.ModifyValue(_passiveEffect, ref _originalValue, ref percValueMultiplier, tags);
			}
		}
		if (_originalItemValue != null && _originalItemValue.ItemClass != null && _originalItemValue.Quality > 0 && _originalItemValue.ItemClass.Effects != null)
		{
			for (int i = 0; i < _originalItemValue.Modifications.Length; i++)
			{
				if (_originalItemValue.Modifications[i] != null && _originalItemValue.Modifications[i].ItemClass is ItemClassModifier)
				{
					_originalItemValue.ItemClass.Effects.ModifyValue(_entity, PassiveEffects.ModPowerBonus, ref _originalValue, ref percValueMultiplier, (float)_originalItemValue.Quality, FastTags<TagGroup.Global>.Parse(_passiveEffect.ToStringCached<PassiveEffects>()), 1);
				}
			}
		}
		baseValueChange = _originalValue - num;
		return _originalValue * percValueMultiplier;
	}

	public static string GetInfoString(PassiveEffects _gAttribute, ItemValue _itemValue, EntityAlive _ea = null, float modAmount = 0f)
	{
		return string.Format("{0}: {1}\n", _gAttribute.ToStringCached<PassiveEffects>(), (modAmount + EffectManager.GetValue(_gAttribute, _itemValue, 0f, _ea, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false)).ToCultureInvariantString("0.0"));
	}

	public static string GetColoredInfoString(PassiveEffects _passiveEffect, ItemValue _itemValue, EntityAlive _ea = null)
	{
		EffectManager.GetDisplayValues(_passiveEffect, out EffectManager.cInfoStringBaseValue, out EffectManager.cInfoStringPercValue, _itemValue, 0f, _ea, null, default(FastTags<TagGroup.Global>), 1);
		return string.Format("{0}: [REPLACE_COLOR]{1}*{2}[-]\n", _passiveEffect.ToStringCached<PassiveEffects>(), EffectManager.cInfoStringBaseValue.ToCultureInvariantString("0.0"), EffectManager.cInfoStringPercValue.ToCultureInvariantString("0.0"));
	}

	public static List<EffectManager.ModifierValuesAndSources> GetValuesAndSources(PassiveEffects _passiveEffect, ItemValue _originalItemValue = null, float _originalValue = 0f, EntityAlive _entity = null, Recipe _recipe = null, FastTags<TagGroup.Global> tags = default(FastTags<TagGroup.Global>), bool calcEquipment = true, bool calcHoldingItem = true)
	{
		float num = 1f;
		List<EffectManager.ModifierValuesAndSources> list = new List<EffectManager.ModifierValuesAndSources>();
		if (_entity == null)
		{
			if (_originalItemValue != null && _originalItemValue.type != 0 && _originalItemValue.ItemClass != null)
			{
				_originalItemValue.GetModifiedValueData(list, EffectManager.ModifierValuesAndSources.ValueSourceType.Self, _entity, null, _passiveEffect, ref _originalValue, ref num, tags);
			}
		}
		else
		{
			if (GameManager.Instance == null || GameManager.Instance.gameStateManager == null || !GameManager.Instance.gameStateManager.IsGameStarted())
			{
				return list;
			}
			if (EntityClass.list.ContainsKey(_entity.entityClass) && EntityClass.list[_entity.entityClass].Effects != null)
			{
				EntityClass.list[_entity.entityClass].Effects.GetModifiedValueData(list, EffectManager.ModifierValuesAndSources.ValueSourceType.Base, _entity, _passiveEffect, ref _originalValue, ref num, 0f, tags, 1);
			}
			if (_originalItemValue != null && _originalItemValue.type != 0 && _originalItemValue.ItemClass != null)
			{
				_originalItemValue.GetModifiedValueData(list, EffectManager.ModifierValuesAndSources.ValueSourceType.Self, _entity, null, _passiveEffect, ref _originalValue, ref num, tags);
			}
			else
			{
				if (calcHoldingItem && _entity.inventory != null && _entity.inventory.holdingItemItemValue != _originalItemValue && !_entity.inventory.holdingItemItemValue.IsMod)
				{
					_entity.inventory.holdingItemItemValue.GetModifiedValueData(list, EffectManager.ModifierValuesAndSources.ValueSourceType.Held, _entity, _originalItemValue, _passiveEffect, ref _originalValue, ref num, tags);
				}
				if (calcEquipment && _entity.equipment != null)
				{
					_entity.equipment.GetModifiedValueData(list, EffectManager.ModifierValuesAndSources.ValueSourceType.Worn, _originalItemValue, _passiveEffect, ref _originalValue, ref num, tags);
				}
			}
			if (_entity.Progression != null)
			{
				_entity.Progression.GetModifiedValueData(list, EffectManager.ModifierValuesAndSources.ValueSourceType.Progression, _passiveEffect, ref _originalValue, ref num, tags);
			}
			if (_entity.Buffs != null)
			{
				_entity.Buffs.GetModifiedValueData(list, EffectManager.ModifierValuesAndSources.ValueSourceType.Buff, _passiveEffect, ref _originalValue, ref num, tags);
			}
		}
		if (_originalItemValue != null && _originalItemValue.ItemClass != null && _originalItemValue.Quality > 0 && _originalItemValue.ItemClass.Effects != null)
		{
			for (int i = 0; i < _originalItemValue.Modifications.Length; i++)
			{
				if (_originalItemValue.Modifications[i] != null && _originalItemValue.Modifications[i].ItemClass is ItemClassModifier)
				{
					_originalItemValue.ItemClass.Effects.GetModifiedValueData(list, EffectManager.ModifierValuesAndSources.ValueSourceType.ModBonus, _entity, PassiveEffects.ModPowerBonus, ref _originalValue, ref num, (float)_originalItemValue.Quality, FastTags<TagGroup.Global>.Parse(_passiveEffect.ToStringCached<PassiveEffects>()), 1);
				}
			}
		}
		return list;
	}

	public static FastEnumIntEqualityComparer<PassiveEffects> PassiveEffectsComparer = new FastEnumIntEqualityComparer<PassiveEffects>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static int slotsQueriedFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int slotsQueriedForEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ItemStack[] slotsCached;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float cInfoStringBaseValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float cInfoStringPercValue;

	public class ModifierValuesAndSources
	{
		public PassiveEffects PassiveEffectName;

		public MinEffectController.SourceParentType ParentType;

		public EffectManager.ModifierValuesAndSources.ValueSourceType ValueSource;

		public EffectManager.ModifierValuesAndSources.ValueTypes ValueType;

		public FastTags<TagGroup.Global> Tags;

		public object Source;

		public float Value;

		public PassiveEffect.ValueModifierTypes ModifierType;

		public int ModItemSource;

		public enum ValueSourceType
		{
			None,
			Self,
			Held,
			Worn,
			Attribute,
			Skill,
			Perk,
			Mod,
			CosmeticMod,
			Fault,
			Buff,
			Progression,
			Base,
			Ammo,
			ModBonus
		}

		public enum ValueTypes
		{
			None,
			BaseValue,
			PercentValue
		}

		public enum ModTypes
		{
			None,
			Base,
			Percentage
		}
	}
}
