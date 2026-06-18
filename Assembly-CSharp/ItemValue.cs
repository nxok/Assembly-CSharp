using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ItemValue : IEquatable<ItemValue>
{
	public static ItemValue None
	{
		get
		{
			return new ItemValue(0, false);
		}
	}

	public int MaxUseTimes
	{
		get
		{
			return ItemValue.ModMaxUseTimes(this.MaxUseTimesBase, this);
		}
	}

	public int MaxUseTimesUI
	{
		get
		{
			return this.MaxUseTimesBase;
		}
	}

	public int MaxUseTimesBase
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			ItemClass itemClass = this.ItemClass;
			return (int)EffectManager.GetValue(PassiveEffects.DegradationMax, this, 0f, null, null, (itemClass != null) ? itemClass.ItemTags : FastTags<TagGroup.Global>.none, true, true, true, true, true, 1, true, false);
		}
	}

	public static int ModMaxUseTimes(int _value, ItemValue _iv)
	{
		if (_value <= 0)
		{
			return _value;
		}
		float num;
		if (!_iv.TryGetMetadata("DurabilityModifier", out num))
		{
			return _value;
		}
		_value = Mathf.RoundToInt((float)_value * num);
		_value = Mathf.Max(_value, 1);
		return _value;
	}

	public float MaxDurabilityModifier
	{
		get
		{
			float num;
			if (!this.TryGetMetadata("DurabilityModifier", out num))
			{
				return 1f;
			}
			return num;
		}
		set
		{
			if (value == 1f)
			{
				this.RemoveMetaData("DurabilityModifier");
				return;
			}
			this.SetMetadata("DurabilityModifier", value);
		}
	}

	public float PercentUsesLeft
	{
		get
		{
			int maxUseTimes = this.MaxUseTimes;
			if (maxUseTimes > 0)
			{
				return 1f - Utils.FastClamp01(this.UseTimes / (float)maxUseTimes);
			}
			return 1f;
		}
	}

	public bool TryGetMetadata(string key, out int value)
	{
		object obj;
		if (!this.TryGetMetadata(key, out obj, TypedMetadataValue.TypeTag.Integer))
		{
			value = 0;
			return false;
		}
		value = (int)obj;
		return true;
	}

	public bool TryGetMetadata(string key, out float value)
	{
		object obj;
		if (!this.TryGetMetadata(key, out obj, TypedMetadataValue.TypeTag.Float))
		{
			value = 0f;
			return false;
		}
		value = (float)obj;
		return true;
	}

	public bool TryGetMetadata(string key, out string value)
	{
		object obj;
		if (!this.TryGetMetadata(key, out obj, TypedMetadataValue.TypeTag.String))
		{
			value = null;
			return false;
		}
		value = (string)obj;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetMetadata(string key, out object value, TypedMetadataValue.TypeTag typeTag = TypedMetadataValue.TypeTag.None)
	{
		if (this.Metadata == null)
		{
			value = null;
			return false;
		}
		TypedMetadataValue typedMetadataValue;
		if (!this.Metadata.TryGetValue(key, out typedMetadataValue))
		{
			value = null;
			return false;
		}
		if (typeTag != TypedMetadataValue.TypeTag.None && typedMetadataValue.GetTypeTag() != typeTag)
		{
			value = null;
			return false;
		}
		value = typedMetadataValue.GetValue();
		return true;
	}

	public bool HasMetadata(string key, TypedMetadataValue.TypeTag typeTag = TypedMetadataValue.TypeTag.None)
	{
		if (this.Metadata == null)
		{
			return false;
		}
		if (typeTag == TypedMetadataValue.TypeTag.None)
		{
			return this.Metadata.ContainsKey(key);
		}
		TypedMetadataValue typedMetadataValue;
		return this.Metadata.TryGetValue(key, out typedMetadataValue) && typedMetadataValue.GetTypeTag() == typeTag;
	}

	public object GetMetadata(string key)
	{
		if (this.Metadata == null)
		{
			return false;
		}
		TypedMetadataValue typedMetadataValue;
		if (this.Metadata.TryGetValue(key, out typedMetadataValue))
		{
			return typedMetadataValue.GetValue();
		}
		return null;
	}

	public void SetMetadata(string key, int value)
	{
		this.SetMetadata(key, value, TypedMetadataValue.TypeTag.Integer);
	}

	public void SetMetadata(string key, float value)
	{
		this.SetMetadata(key, value, TypedMetadataValue.TypeTag.Float);
	}

	public void SetMetadata(string key, string value)
	{
		this.SetMetadata(key, value, TypedMetadataValue.TypeTag.String);
	}

	public void SetMetadata(string key, object value, string typeTag)
	{
		this.SetMetadata(key, value, TypedMetadataValue.StringToTag(typeTag));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMetadata(string key, object value, TypedMetadataValue.TypeTag typeTag)
	{
		if (this.Metadata == null)
		{
			this.Metadata = new Dictionary<string, TypedMetadataValue>();
		}
		TypedMetadataValue typedMetadataValue;
		if (this.Metadata.TryGetValue(key, out typedMetadataValue))
		{
			if (!typedMetadataValue.SetValue(value))
			{
				Log.Warning(string.Format("Can not update Metadata value '{0}' on ItemValue, type of value '{1}' ({2}) does not match existing TypeTag ({3}). From: {4}", new object[]
				{
					key,
					value,
					value.GetType().Name,
					typedMetadataValue.GetTypeTag(),
					StackTraceUtility.ExtractStackTrace()
				}));
				return;
			}
		}
		else
		{
			TypedMetadataValue typedMetadataValue2;
			if (TypedMetadataValue.TryCreate(value, typeTag, out typedMetadataValue2))
			{
				this.Metadata.Add(key, typedMetadataValue2);
				return;
			}
			Log.Warning(string.Format("Can not set Metadata key '{0}' on ItemValue, type of value '{1}' ({2}) does not match TypeTag ({3}). From: {4}", new object[]
			{
				key,
				value,
				value.GetType().Name,
				typeTag,
				StackTraceUtility.ExtractStackTrace()
			}));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMetadata(string key, TypedMetadataValue tmv)
	{
		if (this.Metadata == null)
		{
			this.Metadata = new Dictionary<string, TypedMetadataValue>();
		}
		TypedMetadataValue typedMetadataValue;
		if (this.Metadata.TryGetValue(key, out typedMetadataValue))
		{
			object value = tmv.GetValue();
			if (!typedMetadataValue.SetValue(value))
			{
				Log.Warning(string.Format("Can not update Metadata value '{0}' on ItemValue, type of value '{1}' ({2}) does not match existing TypeTag ({3}). From: {4}", new object[]
				{
					key,
					value,
					value.GetType().Name,
					typedMetadataValue.GetTypeTag(),
					StackTraceUtility.ExtractStackTrace()
				}));
				return;
			}
		}
		else
		{
			this.Metadata.Add(key, tmv);
		}
	}

	public bool RemoveMetaData(string name)
	{
		bool flag = false;
		if (this.Metadata != null)
		{
			flag = this.Metadata.Remove(name);
		}
		return flag;
	}

	public bool HasQuality
	{
		get
		{
			ItemClass itemClass = this.ItemClass;
			return itemClass != null && (itemClass.HasQuality || itemClass is ItemClassModifier);
		}
	}

	public bool HasModSlots
	{
		get
		{
			return this.Modifications.Length != 0;
		}
	}

	public bool IsMod
	{
		get
		{
			ItemClass itemClass = this.ItemClass;
			return itemClass != null && itemClass is ItemClassModifier;
		}
	}

	public bool IsShapeHelperBlock
	{
		get
		{
			ItemClassBlock itemClassBlock = this.ItemClass as ItemClassBlock;
			return itemClassBlock != null && itemClassBlock.GetBlock().SelectAlternates;
		}
	}

	public ItemClass ItemClass
	{
		get
		{
			if (this.type < 0 || ItemClass.list == null || this.type >= ItemClass.list.Length)
			{
				return null;
			}
			ItemClass itemClass = ItemClass.list[this.type];
			if (itemClass is ItemClassQuest)
			{
				return ItemClassQuest.GetItemQuestById(this.Seed);
			}
			return itemClass;
		}
	}

	public ItemClass ItemClassOrMissing
	{
		get
		{
			ItemClass itemClass = this.ItemClass;
			if (itemClass != null)
			{
				return itemClass;
			}
			return ItemClass.MissingItem;
		}
	}

	public ItemValue()
	{
		this.Modifications = ItemValue.emptyItemValueArray;
		this.CosmeticMods = ItemValue.emptyItemValueArray;
	}

	public ItemValue(int _type, bool _bCreateDefaultParts = false)
		: this(_type, 1, 6, _bCreateDefaultParts, null, 1f)
	{
	}

	public ItemValue(int _type, int minQuality, int maxQuality, bool _bCreateDefaultModItems = false, string[] modsToInstall = null, float modInstallDescendingChance = 1f)
	{
		this.type = _type;
		this.Modifications = ItemValue.emptyItemValueArray;
		this.CosmeticMods = ItemValue.emptyItemValueArray;
		if (this.type == 0)
		{
			return;
		}
		DateTime utcNow = DateTime.UtcNow;
		this.Seed = (ushort)((utcNow - ItemValue.baseDate).Seconds + utcNow.Millisecond + this.type);
		if (!ThreadManager.IsMainThread())
		{
			return;
		}
		ItemClass itemClass = this.ItemClass;
		if (itemClass == null)
		{
			return;
		}
		GameRandom gameRandom = null;
		if (itemClass.HasQuality)
		{
			if (minQuality == maxQuality)
			{
				this.Quality = (ushort)minQuality;
			}
			else
			{
				gameRandom = GameRandomManager.Instance.CreateGameRandom((int)this.Seed);
				this.Quality = (ushort)gameRandom.RandomRange(minQuality, maxQuality + 1);
			}
		}
		if (itemClass is ItemClassModifier)
		{
			GameRandomManager.Instance.FreeGameRandom(gameRandom);
			return;
		}
		if (itemClass.Stacknumber.Value > 1)
		{
			GameRandomManager.Instance.FreeGameRandom(gameRandom);
			return;
		}
		this.Modifications = new ItemValue[this.CalcModSlotCount()];
		this.CosmeticMods = new ItemValue[itemClass.HasAnyTags(ItemClassModifier.CosmeticItemTags) ? 1 : 0];
		if (_bCreateDefaultModItems)
		{
			if (gameRandom == null)
			{
				gameRandom = GameRandomManager.Instance.CreateGameRandom((int)this.Seed);
			}
			this.createDefaultModItems(itemClass, gameRandom, modsToInstall, modInstallDescendingChance);
		}
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
	}

	public void Clear()
	{
		this.type = 0;
		this.UseTimes = 0f;
		this.Quality = 0;
		this.Meta = 0;
		this.Seed = 0;
		this.Stats = null;
		this.Modifications = new ItemValue[0];
		this.CosmeticMods = new ItemValue[0];
		this.Metadata = null;
		this.SelectedAmmoTypeIndex = 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CalcModSlotCount()
	{
		return Utils.FastMin(255, (int)EffectManager.GetValue(PassiveEffects.ModSlots, this, (float)Utils.FastMax(0, (int)(this.Quality - 1)), null, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createDefaultModItems(ItemClass ic, GameRandom random, string[] modsToInstall, float modInstallDescendingChance)
	{
		FastTags<TagGroup.Global> fastTags = FastTags<TagGroup.Global>.none;
		bool flag = false;
		bool flag2 = false;
		if (modsToInstall != null && modsToInstall.Length != 0)
		{
			float num = modInstallDescendingChance;
			if (!ic.ItemTags.IsEmpty)
			{
				int num2 = 0;
				for (int i = 0; i < modsToInstall.Length; i++)
				{
					ItemClassModifier itemClassModifier = ItemClass.GetItemClass(modsToInstall[i], true) as ItemClassModifier;
					if (itemClassModifier == null)
					{
						itemClassModifier = ItemClassModifier.GetDesiredItemModWithAnyTags(ic.ItemTags, fastTags, FastTags<TagGroup.Global>.Parse(modsToInstall[i]), random);
					}
					if (itemClassModifier != null)
					{
						if (itemClassModifier.HasAnyTags(ItemClassModifier.CosmeticModTypes))
						{
							flag = true;
							if (!flag2 && random.RandomFloat <= modInstallDescendingChance)
							{
								this.CosmeticMods[0] = new ItemValue(itemClassModifier.Id, false);
								fastTags |= itemClassModifier.ItemTags;
								flag2 = true;
								Log.Warning("ItemValue createDefaultModItems cosmetic {0}", new object[] { this.CosmeticMods[0] });
							}
						}
						else if (num2 < this.Modifications.Length && random.RandomFloat <= num)
						{
							this.Modifications[num2] = new ItemValue(itemClassModifier.Id, false);
							fastTags |= itemClassModifier.ItemTags;
							num2++;
							num *= 0.5f;
						}
					}
				}
				for (int j = num2; j < this.Modifications.Length; j++)
				{
					this.Modifications[j] = ItemValue.None;
				}
			}
		}
		if (!flag && !ic.HasAnyTags(ItemValue.noPreinstallCosmeticItemTags))
		{
			for (int k = 0; k < this.CosmeticMods.Length; k++)
			{
				ItemClassModifier cosmeticItemMod = ItemClassModifier.GetCosmeticItemMod(ic.ItemTags, fastTags, random);
				if (cosmeticItemMod != null)
				{
					this.CosmeticMods[k] = new ItemValue(cosmeticItemMod.Id, false);
					fastTags |= cosmeticItemMod.ItemTags;
				}
				else
				{
					this.CosmeticMods[k] = ItemValue.None;
				}
			}
		}
	}

	public void ModifyValue(EntityAlive _entity, ItemValue _originalItemValue, PassiveEffects _passiveEffect, ref float _originalValue, ref float _perc_value, FastTags<TagGroup.Global> _tags, bool _useMods = true, bool _useDurability = false)
	{
		if (_originalItemValue == null || !_originalItemValue.Equals(this))
		{
			int num = MinEventParams.CachedEventParam.Seed;
			if (_entity != null)
			{
				num = _entity.MinEventContext.Seed;
			}
			ItemClass itemClass = this.ItemClass;
			if (itemClass != null)
			{
				if (itemClass.Actions != null && itemClass.Actions.Length != 0 && itemClass.Actions[0] is ItemActionRanged)
				{
					string[] magazineItemNames = (itemClass.Actions[0] as ItemActionRanged).MagazineItemNames;
					if (magazineItemNames != null)
					{
						ItemClass itemClass2 = ItemClass.GetItemClass(magazineItemNames[(int)this.SelectedAmmoTypeIndex], false);
						if (itemClass2 != null && itemClass2.Effects != null)
						{
							itemClass2.Effects.ModifyValue(_entity, _passiveEffect, ref _originalValue, ref _perc_value, 0f, _tags, 1);
						}
					}
				}
				if (itemClass.Effects != null)
				{
					ItemValue itemValue = MinEventParams.CachedEventParam.ItemValue;
					ItemValue itemValue2 = ((_entity != null) ? _entity.MinEventContext.ItemValue : null);
					MinEventParams.CachedEventParam.Seed = (int)((byte)this.Seed + ((this.Seed != 0) ? _passiveEffect : PassiveEffects.None));
					MinEventParams.CachedEventParam.ItemValue = this;
					if (_entity != null)
					{
						_entity.MinEventContext.Seed = MinEventParams.CachedEventParam.Seed;
						_entity.MinEventContext.ItemValue = this;
					}
					float num2 = _originalValue;
					itemClass.Effects.ModifyValue(_entity, _passiveEffect, ref _originalValue, ref _perc_value, (float)this.Quality, _tags, 1);
					if (_useDurability)
					{
						if (_passiveEffect != PassiveEffects.PhysicalDamageResist)
						{
							if (_passiveEffect != PassiveEffects.ElementalDamageResist)
							{
								if (_passiveEffect == PassiveEffects.BuffResistance)
								{
									if (this.PercentUsesLeft < 0.5f)
									{
										float num3 = _originalValue - num2;
										_originalValue = num2 + num3 * this.PercentUsesLeft * 2f;
									}
								}
							}
							else if (this.PercentUsesLeft < 0.5f)
							{
								float num4 = _originalValue - num2;
								_originalValue = num2 + num4 * this.PercentUsesLeft * 2f;
							}
						}
						else if (this.PercentUsesLeft < 0.5f)
						{
							float num5 = _originalValue - num2;
							_originalValue = num2 + num5 * this.PercentUsesLeft * 2f;
						}
					}
					MinEventParams.CachedEventParam.ItemValue = itemValue;
					if (_entity != null)
					{
						_entity.MinEventContext.ItemValue = itemValue2;
					}
				}
			}
			if (this.Stats != null)
			{
				this.StatModifyValue(_passiveEffect, ref _originalValue, false);
			}
			if (_useMods)
			{
				for (int i = 0; i < this.CosmeticMods.Length; i++)
				{
					if (this.CosmeticMods[i] != null && this.CosmeticMods[i].ItemClass is ItemClassModifier)
					{
						this.CosmeticMods[i].ModifyValue(_entity, _originalItemValue, _passiveEffect, ref _originalValue, ref _perc_value, _tags, true, false);
					}
				}
				for (int j = 0; j < this.Modifications.Length; j++)
				{
					if (this.Modifications[j] != null && this.Modifications[j].ItemClass is ItemClassModifier)
					{
						this.Modifications[j].ModifyValue(_entity, _originalItemValue, _passiveEffect, ref _originalValue, ref _perc_value, _tags, true, false);
					}
				}
			}
			MinEventParams.CachedEventParam.Seed = num;
			if (_entity != null)
			{
				_entity.MinEventContext.Seed = num;
			}
		}
	}

	public void GetModifiedValueData(List<EffectManager.ModifierValuesAndSources> _modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType _sourceType, EntityAlive _entity, ItemValue _originalItemValue, PassiveEffects _passiveEffect, ref float _originalValue, ref float _perc_value, FastTags<TagGroup.Global> _tags)
	{
		if (_originalItemValue == null || !_originalItemValue.Equals(this))
		{
			ItemClass itemClass = this.ItemClass;
			if (itemClass != null)
			{
				if (itemClass.Actions != null && itemClass.Actions.Length != 0 && itemClass.Actions[0] is ItemActionRanged)
				{
					string[] magazineItemNames = (itemClass.Actions[0] as ItemActionRanged).MagazineItemNames;
					if (magazineItemNames != null)
					{
						ItemClass.GetItem(magazineItemNames[(int)this.SelectedAmmoTypeIndex], false).GetModifiedValueData(_modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType.Ammo, _entity, _originalItemValue, _passiveEffect, ref _originalValue, ref _perc_value, _tags);
					}
				}
				if (itemClass.Effects != null)
				{
					itemClass.Effects.GetModifiedValueData(_modValueSources, _sourceType, _entity, _passiveEffect, ref _originalValue, ref _perc_value, (float)this.Quality, _tags, 1);
				}
			}
			for (int i = 0; i < this.CosmeticMods.Length; i++)
			{
				if (this.CosmeticMods[i] != null && this.CosmeticMods[i].ItemClass is ItemClassModifier)
				{
					this.CosmeticMods[i].GetModifiedValueData(_modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType.CosmeticMod, _entity, _originalItemValue, _passiveEffect, ref _originalValue, ref _perc_value, _tags);
				}
			}
			for (int j = 0; j < this.Modifications.Length; j++)
			{
				if (this.Modifications[j] != null && this.Modifications[j].ItemClass is ItemClassModifier)
				{
					this.Modifications[j].GetModifiedValueData(_modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType.Mod, _entity, _originalItemValue, _passiveEffect, ref _originalValue, ref _perc_value, _tags);
				}
			}
		}
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _eventParms)
	{
		ItemClass itemClass = this.ItemClass;
		if (itemClass != null)
		{
			if (itemClass is ItemClassModifier && itemClass.Effects != null)
			{
				itemClass.Effects.FireEvent(_eventType, _eventParms);
				return;
			}
			if (itemClass.Actions != null && itemClass.Actions.Length != 0 && itemClass.Actions[0] is ItemActionRanged)
			{
				string[] magazineItemNames = (itemClass.Actions[0] as ItemActionRanged).MagazineItemNames;
				if (magazineItemNames != null)
				{
					ItemClass.GetItem(magazineItemNames[(int)this.SelectedAmmoTypeIndex], false).FireEvent(_eventType, _eventParms);
				}
			}
			itemClass.FireEvent(_eventType, _eventParms);
		}
		if (!this.HasQuality)
		{
			return;
		}
		for (int i = 0; i < this.Modifications.Length; i++)
		{
			if (this.Modifications[i] != null)
			{
				this.Modifications[i].FireEvent(_eventType, _eventParms);
			}
		}
		for (int j = 0; j < this.CosmeticMods.Length; j++)
		{
			if (this.CosmeticMods[j] != null)
			{
				this.CosmeticMods[j].FireEvent(_eventType, _eventParms);
			}
		}
	}

	public ItemValue Clone()
	{
		ItemValue itemValue = new ItemValue(this.type, false);
		itemValue.Meta = this.Meta;
		itemValue.UseTimes = this.UseTimes;
		itemValue.Quality = this.Quality;
		itemValue.SelectedAmmoTypeIndex = this.SelectedAmmoTypeIndex;
		if (this.Stats != null)
		{
			itemValue.Stats = new ItemValue.Stat[this.Stats.Length];
			Array.Copy(this.Stats, itemValue.Stats, this.Stats.Length);
		}
		this.CloneModsTo(itemValue);
		if (this.Metadata != null)
		{
			itemValue.Metadata = new Dictionary<string, TypedMetadataValue>();
			foreach (KeyValuePair<string, TypedMetadataValue> keyValuePair in this.Metadata)
			{
				Dictionary<string, TypedMetadataValue> metadata = itemValue.Metadata;
				string key = keyValuePair.Key;
				TypedMetadataValue value = keyValuePair.Value;
				metadata.Add(key, (value != null) ? value.Clone() : null);
			}
		}
		this.CloneCosmeticModsTo(itemValue);
		itemValue.Activated = this.Activated;
		if (itemValue.type == 0)
		{
			this.Seed = 0;
		}
		itemValue.Seed = this.Seed;
		itemValue.TextureFullArray = this.TextureFullArray;
		return itemValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CloneModsTo(ItemValue _iv)
	{
		int num = this.Modifications.Length;
		_iv.Modifications = new ItemValue[num];
		for (int i = 0; i < num; i++)
		{
			ItemValue[] modifications = _iv.Modifications;
			int num2 = i;
			ItemValue itemValue = this.Modifications[i];
			modifications[num2] = ((itemValue != null) ? itemValue.Clone() : null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CloneCosmeticModsTo(ItemValue _iv)
	{
		int num = this.CosmeticMods.Length;
		_iv.CosmeticMods = new ItemValue[num];
		for (int i = 0; i < num; i++)
		{
			ItemValue[] cosmeticMods = _iv.CosmeticMods;
			int num2 = i;
			ItemValue itemValue = this.CosmeticMods[i];
			cosmeticMods[num2] = ((itemValue != null) ? itemValue.Clone() : null);
		}
	}

	public bool IsEmpty()
	{
		return this.type == 0;
	}

	public BlockValue ToBlockValue(bool _allowAlternates = false)
	{
		if (this.type >= Block.ItemsStartHere)
		{
			return BlockValue.Air;
		}
		BlockValue blockValue = default(BlockValue);
		blockValue.type = this.type;
		if (_allowAlternates && blockValue.Block.SelectAlternates)
		{
			return blockValue.Block.GetAltBlockValue(this.Meta);
		}
		return blockValue;
	}

	public void MergeBest(ItemValue _iv)
	{
		int maxUseTimes = this.MaxUseTimes;
		int maxUseTimes2 = _iv.MaxUseTimes;
		float num = (float)maxUseTimes - this.UseTimes;
		float num2 = (float)maxUseTimes2 - _iv.UseTimes;
		if (ItemAction.RepairType == ItemAction.RepairTypes.Both || ItemAction.RepairType == ItemAction.RepairTypes.CombineOnly)
		{
			int num3 = Utils.FastMax(maxUseTimes, maxUseTimes2);
			float num4 = num + num2;
			this.UseTimes = Utils.FastMax(0f, (float)num3 - num4);
		}
		else
		{
			bool flag = false;
			if (_iv.Quality == this.Quality)
			{
				flag = num2 > num;
				if (flag)
				{
					this.MaxDurabilityModifier = _iv.MaxDurabilityModifier;
				}
			}
			else if (_iv.Quality > this.Quality)
			{
				flag = true;
			}
			if (flag)
			{
				this.UseTimes = Utils.FastMax(0f, (float)maxUseTimes2 - num2);
			}
		}
		if (_iv.Quality > this.Quality)
		{
			this.Quality = _iv.Quality;
			this.MaxDurabilityModifier = _iv.MaxDurabilityModifier;
		}
		this.MergeBestStats(_iv);
		if (_iv.HasMods())
		{
			_iv.CloneModsTo(this);
		}
		if (_iv.HasCosmetics())
		{
			_iv.CloneCosmeticModsTo(this);
		}
		int num5 = this.CalcModSlotCount();
		Array.Resize<ItemValue>(ref this.Modifications, num5);
	}

	public void InitStats(int _count)
	{
		this.Stats = new ItemValue.Stat[_count];
	}

	public void ClearStats()
	{
		this.Stats = null;
	}

	public void RemoveUnusedStats()
	{
		if (this.Stats != null)
		{
			int num = 0;
			for (int i = 0; i < this.Stats.Length; i++)
			{
				if (this.Stats[i].value != 0)
				{
					num++;
				}
			}
			if (num == 0)
			{
				this.ClearStats();
				return;
			}
			if (num < this.Stats.Length)
			{
				ItemValue.Stat[] stats = this.Stats;
				this.Stats = new ItemValue.Stat[num];
				int num2 = 0;
				for (int j = 0; j < stats.Length; j++)
				{
					if (stats[j].value != 0)
					{
						this.Stats[num2++] = stats[j];
					}
				}
			}
		}
	}

	public bool HasStats()
	{
		return this.Stats != null;
	}

	public bool HasAnyBoostedStats()
	{
		if (this.Stats != null)
		{
			for (int i = 0; i < this.Stats.Length; i++)
			{
				if (this.Stats[i].isBoosted)
				{
					return true;
				}
			}
		}
		return false;
	}

	public float GetStatPercent(PassiveEffects _type, bool _onlyBoosted)
	{
		float num = 1f;
		if (this.Stats != null)
		{
			this.StatModifyValue(_type, ref num, _onlyBoosted);
		}
		return num;
	}

	public void SetStat(int _index, PassiveEffects _type, int _base, int _added)
	{
		this.Stats[_index] = new ItemValue.Stat(_type, _base, _added);
	}

	public void MergeBestStats(ItemValue _iv)
	{
		if (_iv.Stats == null)
		{
			return;
		}
		if (this.Stats == null)
		{
			this.Stats = new ItemValue.Stat[_iv.Stats.Length];
			Array.Copy(_iv.Stats, this.Stats, _iv.Stats.Length);
			return;
		}
		List<ItemValue.Stat> list = new List<ItemValue.Stat>(this.Stats);
		for (int i = 0; i < _iv.Stats.Length; i++)
		{
			ItemValue.Stat stat = _iv.Stats[i];
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				ItemValue.Stat stat2 = list[j];
				if (stat.type == stat2.type)
				{
					bool flag2 = ItemValue.IsStatLowerBetter(stat2.type);
					if (stat.value != stat2.value && stat.value > stat2.value != flag2)
					{
						stat2.value = stat.value;
						stat2.isBoosted = stat.isBoosted;
						list[j] = stat2;
					}
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(stat);
			}
		}
		this.Stats = list.ToArray();
	}

	public static bool IsStatLowerBetter(PassiveEffects _type)
	{
		return _type == PassiveEffects.StaminaLoss || _type == PassiveEffects.TargetArmor;
	}

	public void StatModifyValue(PassiveEffects _passiveEffect, ref float _originalValue, bool onlyBoosted = false)
	{
		for (int i = 0; i < this.Stats.Length; i++)
		{
			ItemValue.Stat stat = this.Stats[i];
			if (stat.type == _passiveEffect)
			{
				int num = (int)((onlyBoosted && !stat.isBoosted) ? 0 : stat.value);
				float num2 = 1f + (float)num * 0.005f;
				_originalValue *= num2;
				return;
			}
		}
	}

	public void ReadOld(BinaryReader _br)
	{
	}

	public static ItemValue ReadOrNull(BinaryReader _br)
	{
		byte b = _br.ReadByte();
		if (b == 0)
		{
			return null;
		}
		ItemValue itemValue = new ItemValue();
		itemValue.ReadData(_br, (int)b);
		return itemValue;
	}

	public void Read(BinaryReader _br)
	{
		byte b = _br.ReadByte();
		if (b == 0)
		{
			this.type = 0;
			return;
		}
		this.ReadData(_br, (int)b);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReadData(BinaryReader _br, int version)
	{
		int num = 0;
		if (version >= 8)
		{
			num = (int)_br.ReadByte();
		}
		this.type = (int)_br.ReadUInt16();
		if ((num & 1) > 0)
		{
			this.type += Block.ItemsStartHere;
		}
		if (version < 8 && this.type >= 32768)
		{
			this.type += 32768;
		}
		ItemClass itemClass = this.ItemClass;
		if (version > 5)
		{
			this.UseTimes = _br.ReadSingle();
		}
		else
		{
			this.UseTimes = (float)_br.ReadUInt16();
		}
		this.Quality = _br.ReadUInt16();
		if (itemClass != null && itemClass.QualityMin > 0)
		{
			this.Quality = (ushort)((byte)Utils.FastMax((int)this.Quality, (int)itemClass.QualityMin));
		}
		this.Meta = (int)_br.ReadUInt16();
		if (this.Meta >= 65535)
		{
			this.Meta = -1;
		}
		if (version > 6)
		{
			int num2 = (int)_br.ReadByte();
			for (int i = 0; i < num2; i++)
			{
				string text = _br.ReadString();
				TypedMetadataValue typedMetadataValue = TypedMetadataValue.Read(_br);
				if (EntityPlayerLocal.PermaDegrationOn || text != "DurabilityModifier")
				{
					this.SetMetadata(text, typedMetadataValue);
				}
			}
		}
		if ((num & 2) > 0)
		{
			int num3 = (int)_br.ReadByte();
			this.Stats = new ItemValue.Stat[num3];
			for (int j = 0; j < num3; j++)
			{
				PassiveEffects passiveEffects = (PassiveEffects)_br.ReadByte();
				int num4 = (int)_br.ReadInt16();
				int num5 = (int)_br.ReadInt16();
				this.Stats[j] = new ItemValue.Stat(passiveEffects, num4, num5);
			}
			this.RemoveUnusedStats();
		}
		if ((version > 4 || this.HasQuality) && !(itemClass is ItemClassModifier))
		{
			byte b = _br.ReadByte();
			this.Modifications = new ItemValue[(int)b];
			if (b != 0)
			{
				for (int k = 0; k < (int)b; k++)
				{
					if (_br.ReadBoolean())
					{
						this.Modifications[k] = new ItemValue();
						this.Modifications[k].Read(_br);
					}
					else
					{
						this.Modifications[k] = ItemValue.None;
					}
				}
			}
			b = _br.ReadByte();
			this.CosmeticMods = new ItemValue[(int)b];
			if (b != 0)
			{
				for (int l = 0; l < (int)b; l++)
				{
					if (_br.ReadBoolean())
					{
						this.CosmeticMods[l] = new ItemValue();
						this.CosmeticMods[l].Read(_br);
					}
					else
					{
						this.CosmeticMods[l] = ItemValue.None;
					}
				}
			}
		}
		if (version > 1)
		{
			this.Activated = _br.ReadByte();
		}
		if (version > 2)
		{
			this.SelectedAmmoTypeIndex = _br.ReadByte();
		}
		if (version > 3)
		{
			this.Seed = _br.ReadUInt16();
			if (this.type == 0)
			{
				this.Seed = 0;
			}
		}
		if (version > 8)
		{
			if (_br.ReadBoolean())
			{
				this.TextureFullArray.Read(_br, 1);
				return;
			}
			this.TextureFullArray.Fill(0L);
		}
	}

	public static void Write(ItemValue _iv, BinaryWriter _bw)
	{
		if (_iv == null)
		{
			_bw.Write(0);
			return;
		}
		_iv.Write(_bw);
	}

	public void Write(BinaryWriter _bw)
	{
		if (this.IsEmpty())
		{
			_bw.Write(0);
			return;
		}
		_bw.Write(9);
		int num = this.type;
		byte b = 0;
		if (this.type >= Block.ItemsStartHere)
		{
			b = 1;
			num -= Block.ItemsStartHere;
		}
		if (this.Stats != null)
		{
			b |= 2;
		}
		_bw.Write(b);
		_bw.Write((ushort)num);
		_bw.Write(this.UseTimes);
		_bw.Write(this.Quality);
		_bw.Write((ushort)this.Meta);
		int num2 = ((this.Metadata != null) ? this.Metadata.Count : 0);
		_bw.Write((byte)num2);
		if (this.Metadata != null)
		{
			foreach (string text in this.Metadata.Keys)
			{
				TypedMetadataValue typedMetadataValue = this.Metadata[text];
				if (((typedMetadataValue != null) ? typedMetadataValue.GetValue() : null) != null)
				{
					_bw.Write(text);
					TypedMetadataValue.Write(this.Metadata[text], _bw);
				}
			}
		}
		if (this.Stats != null)
		{
			int num3 = this.Stats.Length;
			_bw.Write((byte)num3);
			for (int i = 0; i < num3; i++)
			{
				ItemValue.Stat stat = this.Stats[i];
				_bw.Write((byte)stat.type);
				short num4 = ((!stat.isBoosted) ? stat.value : 0);
				short num5 = (stat.isBoosted ? stat.value : 0);
				_bw.Write(num4);
				_bw.Write(num5);
			}
		}
		if (!(this.ItemClass is ItemClassModifier))
		{
			_bw.Write((byte)this.Modifications.Length);
			for (int j = 0; j < this.Modifications.Length; j++)
			{
				bool flag = this.Modifications[j] != null && !this.Modifications[j].IsEmpty();
				_bw.Write(flag);
				if (flag)
				{
					this.Modifications[j].Write(_bw);
				}
			}
			_bw.Write((byte)this.CosmeticMods.Length);
			for (int k = 0; k < this.CosmeticMods.Length; k++)
			{
				bool flag2 = this.CosmeticMods[k] != null && !this.CosmeticMods[k].IsEmpty();
				_bw.Write(flag2);
				if (flag2)
				{
					this.CosmeticMods[k].Write(_bw);
				}
			}
		}
		_bw.Write(this.Activated);
		_bw.Write(this.SelectedAmmoTypeIndex);
		if (this.type == 0)
		{
			this.Seed = 0;
		}
		_bw.Write(this.Seed);
		if (this.TextureFullArray.IsDefault)
		{
			_bw.Write(false);
		}
		else
		{
			_bw.Write(true);
			this.TextureFullArray.Write(_bw);
		}
		ItemClass itemClass = ItemClass.list[this.type];
		if (itemClass == null)
		{
			if (this.type != 0)
			{
				Log.Error("No ItemClass entry for type " + this.type.ToString());
				return;
			}
		}
		else
		{
			NameIdMapping nameIdMapping;
			if (itemClass.IsBlock())
			{
				nameIdMapping = Block.nameIdMapping;
			}
			else
			{
				nameIdMapping = ItemClass.nameIdMapping;
			}
			if (nameIdMapping != null)
			{
				nameIdMapping.AddMapping(this.type, itemClass.Name, false);
			}
		}
	}

	public bool Equals(ItemValue _other)
	{
		return _other != null && (_other.type == this.type && _other.UseTimes == this.UseTimes && _other.Meta == this.Meta && _other.Seed == this.Seed && _other.Quality == this.Quality && _other.SelectedAmmoTypeIndex == this.SelectedAmmoTypeIndex && _other.Activated == this.Activated && ItemValue.Equals(_other.Metadata, this.Metadata) && ItemValue.Equals(_other.Stats, this.Stats) && ItemValue.Equals(_other.CosmeticMods, this.CosmeticMods)) && ItemValue.Equals(_other.Modifications, this.Modifications);
	}

	public override bool Equals(object _other)
	{
		return _other is ItemValue && this.Equals((ItemValue)_other);
	}

	public bool EqualsExceptUseTimesAndAmmo(ItemValue _other)
	{
		if (_other == null)
		{
			return false;
		}
		bool flag = this.ItemClass != null && this.ItemClass.Actions != null && this.ItemClass.Actions.Length != 0 && this.ItemClass.Actions[0] is ItemActionRanged;
		return _other.type == this.type && (flag || _other.Meta == this.Meta) && _other.Seed == this.Seed && _other.Quality == this.Quality && _other.SelectedAmmoTypeIndex == this.SelectedAmmoTypeIndex && _other.Activated == this.Activated && ItemValue.Equals(_other.Metadata, this.Metadata) && ItemValue.Equals(_other.Stats, this.Stats) && ItemValue.Equals(_other.CosmeticMods, this.CosmeticMods) && ItemValue.Equals(_other.Modifications, this.Modifications);
	}

	public bool EqualsForMerging(ItemValue _other)
	{
		if (_other == null)
		{
			return false;
		}
		bool flag = ItemAction.RepairType == ItemAction.RepairTypes.Both || ItemAction.RepairType == ItemAction.RepairTypes.CombineOnly;
		return _other.type == this.type && (!flag || this.UseTimes + _other.UseTimes == 0f || ((float)this.MaxUseTimes == this.UseTimes && (float)_other.MaxUseTimes == _other.UseTimes)) && ItemValue.Equals(_other.Stats, this.Stats);
	}

	public static bool Equals(ItemValue[] _a, ItemValue[] _b)
	{
		if (_a == null && _b == null)
		{
			return true;
		}
		if (_a == null || _b == null)
		{
			return false;
		}
		if (_a.Length != _b.Length)
		{
			return false;
		}
		if (_a.Length == 0)
		{
			return true;
		}
		for (int i = 0; i < _a.Length; i++)
		{
			if (_a[i] != null || _b[i] != null)
			{
				if (_a[i] == null || _b[i] == null)
				{
					return false;
				}
				if (!_a[i].Equals(_b[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool Equals(ItemValue.Stat[] _a, ItemValue.Stat[] _b)
	{
		return (_a == null && _b == null) || (_a != null && _b != null && _a.Length == _b.Length && (_a.Length == 0 || _a.SequenceEqual(_b)));
	}

	public static bool Equals(object[] _a, object[] _b)
	{
		return _a is ItemValue[] && _b is ItemValue[] && ItemValue.Equals((ItemValue[])_a, (ItemValue[])_b);
	}

	public static bool Equals(Dictionary<string, TypedMetadataValue> _a, Dictionary<string, TypedMetadataValue> _b)
	{
		if (_a == null && _b == null)
		{
			return true;
		}
		if (_a == null || _b == null)
		{
			return false;
		}
		if (_a.Count != _b.Count)
		{
			return false;
		}
		if (_a.Count == 0)
		{
			return true;
		}
		foreach (string text in _a.Keys)
		{
			if (!_b.ContainsKey(text))
			{
				return false;
			}
			if (!_a[text].Equals(_b[text]))
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		return this.type;
	}

	public int GetItemId()
	{
		return this.type - Block.ItemsStartHere;
	}

	public int GetItemOrBlockId()
	{
		if (this.type < Block.ItemsStartHere)
		{
			return this.type;
		}
		return this.type - Block.ItemsStartHere;
	}

	public override string ToString()
	{
		return string.Concat(new string[]
		{
			(this.type >= Block.ItemsStartHere) ? ("item=" + (this.type - Block.ItemsStartHere).ToString()) : ("block=" + this.type.ToString()),
			" m=",
			this.Meta.ToString(),
			" ut=",
			this.UseTimes.ToString()
		});
	}

	public string GetPropertyOverride(string _propertyName, string _originalValue)
	{
		if (this.Modifications.Length == 0 && this.CosmeticMods.Length == 0)
		{
			return _originalValue;
		}
		string text = "";
		string itemName = this.ItemClass.GetItemName();
		for (int i = 0; i < this.Modifications.Length; i++)
		{
			ItemValue itemValue = this.Modifications[i];
			if (itemValue != null)
			{
				ItemClassModifier itemClassModifier = itemValue.ItemClass as ItemClassModifier;
				if (itemClassModifier != null && itemClassModifier.GetPropertyOverride(_propertyName, itemName, ref text))
				{
					return text;
				}
			}
		}
		text = "";
		for (int j = 0; j < this.CosmeticMods.Length; j++)
		{
			ItemValue itemValue2 = this.CosmeticMods[j];
			if (itemValue2 != null)
			{
				ItemClassModifier itemClassModifier2 = itemValue2.ItemClass as ItemClassModifier;
				if (itemClassModifier2 != null && itemClassModifier2.GetPropertyOverride(_propertyName, itemName, ref text))
				{
					return text;
				}
			}
		}
		return _originalValue;
	}

	public bool HasCosmetics()
	{
		if (this.CosmeticMods != null)
		{
			for (int i = 0; i < this.CosmeticMods.Length; i++)
			{
				ItemValue itemValue = this.CosmeticMods[i];
				if (itemValue != null && !itemValue.IsEmpty())
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasMods()
	{
		if (this.Modifications != null)
		{
			for (int i = 0; i < this.Modifications.Length; i++)
			{
				ItemValue itemValue = this.Modifications[i];
				if (itemValue != null && !itemValue.IsEmpty())
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int EmptySaveVersion = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CurrentSaveVersion = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte MaxModifications = 255;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ItemValue[] emptyItemValueArray = new ItemValue[0];

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> noPreinstallCosmeticItemTags = FastTags<TagGroup.Global>.Parse("weapon,tool,armor");

	public int type;

	public byte Activated;

	public byte SelectedAmmoTypeIndex;

	public float UseTimes;

	public int Meta;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, TypedMetadataValue> Metadata;

	public ushort Quality;

	public ItemValue.Stat[] Stats;

	public ItemValue[] Modifications;

	public ItemValue[] CosmeticMods;

	public ushort Seed;

	public TextureFullArray TextureFullArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cDurabilityMetaName = "DurabilityModifier";

	[PublicizedFrom(EAccessModifier.Private)]
	public static DateTime baseDate = new DateTime(2013, 10, 1);

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cFlagsItem = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cFlagsStats = 2;

	public struct Stat
	{
		public Stat(PassiveEffects _type, int _base, int _added)
		{
			this.type = _type;
			this.isBoosted = _added != 0;
			this.value = (short)(_base + _added);
		}

		public PassiveEffects type;

		public bool isBoosted;

		public short value;
	}
}
