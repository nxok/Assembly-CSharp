using System;
using System.Collections.Generic;
using UnityEngine;

public class LootContainer
{
	public static void InitStatic()
	{
		LootContainer.Cleanup();
	}

	public void Init()
	{
		LootContainer.lootContainers[this.Name] = this;
	}

	public static void Cleanup()
	{
		LootContainer.lootContainers.Clear();
		LootContainer.lootGroups.Clear();
		LootContainer.lootQualityTemplates.Clear();
		LootContainer.lootProbTemplates.Clear();
	}

	public static bool IsLoaded()
	{
		return LootContainer.lootContainers.Count > 0;
	}

	public static LootContainer GetLootContainer(string _name, bool _errorOnMiss = true)
	{
		if (string.IsNullOrEmpty(_name))
		{
			return null;
		}
		LootContainer lootContainer;
		if (LootContainer.lootContainers.TryGetValue(_name, out lootContainer))
		{
			return lootContainer;
		}
		if (_errorOnMiss)
		{
			Log.Error("LootContainer '" + _name + "' unknown");
		}
		return null;
	}

	public static ItemStack GetRewardItem(string lootGroup, float questDifficulty)
	{
		if (!LootContainer.lootGroups.ContainsKey(lootGroup))
		{
			return ItemStack.Empty;
		}
		List<ItemStack> list = new List<ItemStack>();
		int num = 1;
		LootContainer.SpawnItemsFromGroup(GameManager.Instance.lootManager.Random, LootContainer.lootGroups[lootGroup], 1, 1f, list, ref num, questDifficulty, 0f, LootContainer.lootGroups[lootGroup].lootQualityTemplate, null, FastTags<TagGroup.Global>.none, true, true, false, true, null, true);
		if (list.Count == 0)
		{
			return ItemStack.Empty;
		}
		return list[0];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool SpawnItem(GameRandom random, LootContainer.LootEntry template, ItemValue lootItemValue, int countToSpawn, List<ItemStack> spawnedItems, ref int slotsLeft, float gameStage, string lootQualityTemplate, EntityPlayer player, FastTags<TagGroup.Global> containerTags, bool _forceStacking)
	{
		if (lootItemValue.ItemClass == null)
		{
			return false;
		}
		if (player != null)
		{
			countToSpawn = Math.Min((int)EffectManager.GetValue(PassiveEffects.LootQuantity, player.inventory.holdingItemItemValue, (float)countToSpawn, player, null, lootItemValue.ItemClass.ItemTags | containerTags, true, true, true, true, true, 1, true, false), lootItemValue.ItemClass.Stacknumber.Value);
		}
		if (countToSpawn < 1 || (player != null && LootContainer.GlobalCountModifier == 0f))
		{
			return false;
		}
		if (lootItemValue.HasQuality && LootContainer.LootMaxTier == 0)
		{
			return false;
		}
		ItemClass itemClass = ItemClass.HandleSandboxTechType(lootItemValue.ItemClass);
		if (itemClass != lootItemValue.ItemClass)
		{
			if (itemClass == null)
			{
				return false;
			}
			lootItemValue = ItemClass.GetItem(itemClass.Name, false);
		}
		if (lootItemValue.ItemClass.CanStack())
		{
			int value = lootItemValue.ItemClass.Stacknumber.Value;
			for (int i = 0; i < spawnedItems.Count; i++)
			{
				ItemStack itemStack = spawnedItems[i];
				if (itemStack.itemValue.type == lootItemValue.type)
				{
					if (itemStack.CanStack(countToSpawn) || _forceStacking)
					{
						itemStack.count += countToSpawn;
						return true;
					}
					int num = value - itemStack.count;
					itemStack.count = value;
					countToSpawn -= num;
				}
			}
		}
		if (slotsLeft < 1)
		{
			return false;
		}
		int num2 = template.minQuality;
		int num3 = template.maxQuality;
		string text = lootQualityTemplate;
		if (!string.IsNullOrEmpty(text))
		{
			LootContainer.LootGroup parentGroup = template.parentGroup;
			if (((parentGroup != null) ? parentGroup.lootQualityTemplate : null) == null)
			{
				goto IL_018B;
			}
		}
		LootContainer.LootGroup parentGroup2 = template.parentGroup;
		text = ((parentGroup2 != null) ? parentGroup2.lootQualityTemplate : null);
		IL_018B:
		if (!string.IsNullOrEmpty(text))
		{
			bool flag = false;
			for (int j = 0; j < LootContainer.lootQualityTemplates[text].templates.Count; j++)
			{
				float randomFloat = random.RandomFloat;
				LootContainer.LootGroup lootGroup = LootContainer.lootQualityTemplates[text].templates[j];
				num2 = lootGroup.minQuality;
				num3 = lootGroup.maxQuality;
				if (lootGroup.minLevel <= gameStage && lootGroup.maxLevel >= gameStage)
				{
					for (int k = 0; k < lootGroup.items.Count; k++)
					{
						LootContainer.LootEntry lootEntry = lootGroup.items[k];
						if (random.RandomFloat <= lootEntry.prob)
						{
							num2 = lootEntry.minQuality;
							num3 = lootEntry.maxQuality;
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
		}
		string[] array = template.modsToInstall;
		float num4 = template.modChance;
		if (template.parentGroup != null && template.parentGroup.modsToInstall.Length != 0)
		{
			array = template.parentGroup.modsToInstall;
			num4 = template.parentGroup.modChance;
		}
		ItemValue itemValue;
		if (lootItemValue.HasQuality)
		{
			if (num2 <= -1)
			{
				num2 = 1;
				num3 = 6;
			}
			if (num3 > LootContainer.LootMaxTier)
			{
				num3 = LootContainer.LootMaxTier;
			}
			if (num2 > num3)
			{
				num2 = num3;
			}
			itemValue = new ItemValue(lootItemValue.type, num2, num3, true, array, num4);
		}
		else
		{
			itemValue = new ItemValue(lootItemValue.type, 1, 6, true, array, num4);
		}
		itemClass = itemValue.ItemClass;
		if (itemClass != null)
		{
			itemClass.AddGSStats(itemValue, (int)gameStage, random);
			if (itemClass.Actions != null && itemClass.Actions.Length != 0 && itemClass.Actions[0] != null)
			{
				itemValue.Meta = 0;
			}
			if (itemValue.MaxUseTimes > 0)
			{
				if (template.randomDurability)
				{
					itemValue.UseTimes = (float)((int)((float)itemValue.MaxUseTimes * random.RandomRange(0.2f, 0.8f)));
				}
				else
				{
					itemValue.UseTimes = 0f;
				}
			}
		}
		ItemStack itemStack2;
		if (player != null)
		{
			if (!LootContainer.OverrideItems.ContainsKey(player))
			{
				itemStack2 = new ItemStack(itemValue, countToSpawn);
			}
			else
			{
				string[] array2 = LootContainer.OverrideItems[player];
				itemStack2 = new ItemStack(ItemClass.GetItem(array2[random.RandomRange(array2.Length)], false), 1);
			}
		}
		else
		{
			itemStack2 = new ItemStack(itemValue, countToSpawn);
		}
		spawnedItems.Add(itemStack2);
		slotsLeft--;
		return true;
	}

	public static int RandomSpawnCount(GameRandom random, int min, int max, float abundance)
	{
		if (min < 0)
		{
			return -1;
		}
		float num = random.RandomRange((float)min - 0.49f, (float)max + 0.49f);
		if (num < (float)min)
		{
			num = (float)min;
		}
		if (num > (float)max)
		{
			num = (float)max;
		}
		num *= abundance;
		int num2 = (int)num;
		float num3 = num - (float)num2;
		if (random.RandomFloat < num3)
		{
			num2++;
		}
		return num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool SpawnAllItemsFromList(GameRandom random, List<LootContainer.LootEntry> itemSet, float abundance, List<ItemStack> spawnedItems, ref int slotsLeft, float playerLevelPercentage, float rareLootChance, string lootQualityTemplate, EntityPlayer player, FastTags<TagGroup.Global> containerTags, bool uniqueItems, bool ignoreLootProb, bool _forceStacking, bool _ignoreLootAbundance, List<string> _buffsToAdd)
	{
		bool flag = false;
		for (int i = 0; i < itemSet.Count; i++)
		{
			LootContainer.LootEntry lootEntry = itemSet[i];
			bool flag2 = false;
			if (!(player != null) || lootEntry.HasRequirements(player))
			{
				if (!lootEntry.forceProb || random.RandomFloat <= LootContainer.getProbability(player, lootEntry, playerLevelPercentage, ignoreLootProb))
				{
					float num = ((lootEntry.group != null) ? LootContainer.GetCountMultiplierFromSandbox(lootEntry.group.abundanceType) : 1f);
					ValueTuple<int, int, bool> modifiedCountFromSandbox = LootContainer.GetModifiedCountFromSandbox(lootEntry.minCount, lootEntry.maxCount, num);
					int item = modifiedCountFromSandbox.Item1;
					int item2 = modifiedCountFromSandbox.Item2;
					bool item3 = modifiedCountFromSandbox.Item3;
					int num2 = LootContainer.RandomSpawnCount(random, item, item2, (lootEntry.group == null) ? abundance : 1f);
					if (lootEntry.group != null)
					{
						if (lootEntry.group.minLevel <= playerLevelPercentage && lootEntry.group.maxLevel >= playerLevelPercentage)
						{
							flag2 = LootContainer.SpawnItemsFromGroup(random, lootEntry.group, num2, abundance, spawnedItems, ref slotsLeft, playerLevelPercentage, rareLootChance, lootQualityTemplate, player, containerTags, uniqueItems, ignoreLootProb, _forceStacking, _ignoreLootAbundance, _buffsToAdd, item3);
						}
					}
					else
					{
						if (!_ignoreLootAbundance)
						{
							num2 = LootContainer.GetCountFromSandbox(lootEntry.item.itemValue, num2);
						}
						flag2 = LootContainer.SpawnItem(random, lootEntry, lootEntry.item.itemValue, num2, spawnedItems, ref slotsLeft, playerLevelPercentage, lootQualityTemplate, player, containerTags, _forceStacking);
					}
				}
				flag = flag || flag2;
				if (flag2 && lootEntry.buffsToAdd != null)
				{
					_buffsToAdd.AddRange(lootEntry.buffsToAdd);
				}
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static ValueTuple<int, int, bool> GetModifiedCountFromSandbox(int oldMin, int oldMax, float modifier)
	{
		int num = Utils.Fastfloor((float)oldMin * modifier);
		int num2 = Utils.Fastfloor((float)oldMax * modifier);
		return new ValueTuple<int, int, bool>(num, num2, num != oldMin || num2 != oldMax);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool SpawnItemsFromGroup(GameRandom random, LootContainer.LootGroup group, int numToSpawn, float abundance, List<ItemStack> spawnedItems, ref int slotsLeft, float gameStage, float rareLootChance, string lootQualityTemplate, EntityPlayer player, FastTags<TagGroup.Global> containerTags, bool uniqueItems, bool ignoreLootProb, bool _forceStacking, bool _ignoreLootAbundance, List<string> _buffsToAdd, bool _sandboxModifierUsed)
	{
		bool flag = false;
		int num = 0;
		while (num < numToSpawn && slotsLeft > 0)
		{
			if (_sandboxModifierUsed)
			{
				flag |= LootContainer.SpawnLootItemsFromList(random, group.items, LootContainer.RandomSpawnCount(random, group.minCount, group.maxCount, 1f), abundance, spawnedItems, ref slotsLeft, gameStage, rareLootChance, lootQualityTemplate, player, containerTags, uniqueItems, ignoreLootProb, _forceStacking, _ignoreLootAbundance, _buffsToAdd, _sandboxModifierUsed);
			}
			else
			{
				float countMultiplierFromSandbox = LootContainer.GetCountMultiplierFromSandbox(group.abundanceType);
				ValueTuple<int, int, bool> modifiedCountFromSandbox = LootContainer.GetModifiedCountFromSandbox(group.minCount, group.maxCount, countMultiplierFromSandbox);
				int item = modifiedCountFromSandbox.Item1;
				int item2 = modifiedCountFromSandbox.Item2;
				bool item3 = modifiedCountFromSandbox.Item3;
				flag |= LootContainer.SpawnLootItemsFromList(random, group.items, LootContainer.RandomSpawnCount(random, item, item2, 1f), abundance, spawnedItems, ref slotsLeft, gameStage, rareLootChance, lootQualityTemplate, player, containerTags, uniqueItems, ignoreLootProb, _forceStacking, _ignoreLootAbundance, _buffsToAdd, item3);
			}
			num++;
		}
		return flag;
	}

	public static bool SpawnLootItemsFromList(GameRandom random, List<LootContainer.LootEntry> itemSet, int numToSpawn, float abundance, List<ItemStack> spawnedItems, ref int slotsLeft, float lootStage, float rareLootChance, string lootQualityTemplate, EntityPlayer player, FastTags<TagGroup.Global> containerTags, bool uniqueItems, bool ignoreLootProb, bool _forceStacking, bool _ignoreLootAbundance, List<string> _buffsToAdd, bool _sandboxModifierUsed)
	{
		if (numToSpawn < 1)
		{
			return numToSpawn == -1 && LootContainer.SpawnAllItemsFromList(random, itemSet, abundance, spawnedItems, ref slotsLeft, lootStage, rareLootChance, lootQualityTemplate, player, containerTags, uniqueItems, ignoreLootProb, _forceStacking, _ignoreLootAbundance, _buffsToAdd);
		}
		float num = 0f;
		for (int i = 0; i < itemSet.Count; i++)
		{
			LootContainer.LootEntry lootEntry = itemSet[i];
			if (!lootEntry.forceProb)
			{
				num += LootContainer.getProbability(player, lootEntry, lootStage, ignoreLootProb);
			}
		}
		if (num == 0f)
		{
			return false;
		}
		List<int> list = new List<int>();
		bool flag = false;
		for (int j = 0; j < numToSpawn; j++)
		{
			float num2 = 0f;
			float randomFloat = random.RandomFloat;
			for (int k = 0; k < itemSet.Count; k++)
			{
				bool flag2 = false;
				LootContainer.LootEntry lootEntry2 = itemSet[k];
				if (!list.Contains(k) || (!lootEntry2.forceProb && !uniqueItems))
				{
					float probability = LootContainer.getProbability(player, lootEntry2, lootStage, ignoreLootProb);
					bool flag3;
					if (lootEntry2.forceProb)
					{
						flag3 = random.RandomFloat <= probability;
					}
					else
					{
						num2 += probability / num;
						flag3 = randomFloat <= num2 + rareLootChance;
					}
					if (flag3)
					{
						list.Add(k);
						if (uniqueItems)
						{
							num -= LootContainer.getProbability(player, lootEntry2, lootStage, ignoreLootProb);
						}
						bool flag4 = _sandboxModifierUsed;
						int num3;
						if (_sandboxModifierUsed)
						{
							num3 = LootContainer.RandomSpawnCount(random, lootEntry2.minCount, lootEntry2.maxCount, (lootEntry2.group == null) ? abundance : 1f);
							num3 += Mathf.RoundToInt((float)num3 * (lootEntry2.lootstageCountMod * lootStage));
						}
						else
						{
							float num4 = ((lootEntry2.group != null) ? LootContainer.GetCountMultiplierFromSandbox(lootEntry2.group.abundanceType) : 1f);
							ValueTuple<int, int, bool> modifiedCountFromSandbox = LootContainer.GetModifiedCountFromSandbox(lootEntry2.minCount, lootEntry2.maxCount, num4);
							int item = modifiedCountFromSandbox.Item1;
							int item2 = modifiedCountFromSandbox.Item2;
							bool item3 = modifiedCountFromSandbox.Item3;
							num3 = LootContainer.RandomSpawnCount(random, item, item2, (lootEntry2.group == null) ? abundance : 1f);
							num3 += Mathf.RoundToInt((float)num3 * (lootEntry2.lootstageCountMod * lootStage));
							flag4 = item3;
						}
						if (lootEntry2.group != null)
						{
							if (lootEntry2.group.minLevel <= lootStage && lootEntry2.group.maxLevel >= lootStage)
							{
								flag2 = LootContainer.SpawnItemsFromGroup(random, lootEntry2.group, num3, abundance, spawnedItems, ref slotsLeft, lootStage, rareLootChance, lootQualityTemplate, player, containerTags, uniqueItems, ignoreLootProb, _forceStacking, _ignoreLootAbundance, _buffsToAdd, flag4);
							}
						}
						else
						{
							if (!_ignoreLootAbundance && !_sandboxModifierUsed)
							{
								num3 = LootContainer.GetCountFromSandbox(lootEntry2.item.itemValue, num3);
							}
							flag2 = LootContainer.SpawnItem(random, lootEntry2, lootEntry2.item.itemValue, num3, spawnedItems, ref slotsLeft, lootStage, lootQualityTemplate, player, containerTags, _forceStacking);
						}
						flag = flag || flag2;
						if (flag2 && lootEntry2.buffsToAdd != null)
						{
							_buffsToAdd.AddRange(lootEntry2.buffsToAdd);
							break;
						}
						break;
					}
				}
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int GetItemCountPerSetting(ItemValue itemValue, int itemSpawnCount, float modifier)
	{
		if (modifier == 0f)
		{
			return 0;
		}
		if (modifier != 1f)
		{
			return (int)Math.Ceiling((double)((float)itemSpawnCount * modifier));
		}
		return itemSpawnCount;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int GetCountFromSandbox(ItemValue itemValue, int itemSpawnCount)
	{
		if (itemValue.ItemClass.HasAnyTags(LootContainer.foodTags))
		{
			itemSpawnCount = LootContainer.GetItemCountPerSetting(itemValue, itemSpawnCount, LootContainer.FoodCountModifier);
		}
		else if (itemValue.ItemClass.HasAnyTags(LootContainer.drinksTags))
		{
			itemSpawnCount = LootContainer.GetItemCountPerSetting(itemValue, itemSpawnCount, LootContainer.DrinkCountModifier);
		}
		else if (itemValue.ItemClass.HasAnyTags(LootContainer.ammoTags))
		{
			itemSpawnCount = LootContainer.GetItemCountPerSetting(itemValue, itemSpawnCount, LootContainer.AmmoCountModifier);
		}
		else if (itemValue.ItemClass.HasAnyTags(LootContainer.medicalTags))
		{
			itemSpawnCount = LootContainer.GetItemCountPerSetting(itemValue, itemSpawnCount, LootContainer.MedicalCountModifier);
		}
		else if (itemValue.ItemClass.HasAnyTags(LootContainer.junkTags))
		{
			itemSpawnCount = LootContainer.GetItemCountPerSetting(itemValue, itemSpawnCount, LootContainer.ResourceCountModifier);
		}
		else if (itemValue.ItemClass.HasAnyTags(LootContainer.armorTags))
		{
			itemSpawnCount = LootContainer.GetItemCountPerSetting(itemValue, itemSpawnCount, LootContainer.ArmorCountModifier);
		}
		else if (itemValue.ItemClass.HasAnyTags(LootContainer.meleeTags))
		{
			itemSpawnCount = LootContainer.GetItemCountPerSetting(itemValue, itemSpawnCount, LootContainer.MeleeCountModifier);
		}
		else if (itemValue.ItemClass.HasAnyTags(LootContainer.rangedTags))
		{
			itemSpawnCount = LootContainer.GetItemCountPerSetting(itemValue, itemSpawnCount, LootContainer.RangedCountModifier);
		}
		else if (itemValue.ItemClass.HasAnyTags(LootContainer.dukesTags))
		{
			itemSpawnCount = LootContainer.GetItemCountPerSetting(itemValue, itemSpawnCount, LootContainer.DukesCountModifier);
		}
		else if (itemValue.ItemClass.HasAnyTags(LootContainer.magazineTags))
		{
			itemSpawnCount = LootContainer.GetItemCountPerSetting(itemValue, itemSpawnCount, LootContainer.MagazinesCountModifier);
		}
		return itemSpawnCount;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float GetCountMultiplierFromSandbox(LootContainer.LootGroup.AbundanceLootModTypes abundanceType)
	{
		switch (abundanceType)
		{
		case LootContainer.LootGroup.AbundanceLootModTypes.Food:
			return LootContainer.FoodCountModifier;
		case LootContainer.LootGroup.AbundanceLootModTypes.Drinks:
			return LootContainer.DrinkCountModifier;
		case LootContainer.LootGroup.AbundanceLootModTypes.Ammo:
			return LootContainer.AmmoCountModifier;
		case LootContainer.LootGroup.AbundanceLootModTypes.Meds:
			return LootContainer.MedicalCountModifier;
		case LootContainer.LootGroup.AbundanceLootModTypes.Resources:
			return LootContainer.ResourceCountModifier;
		case LootContainer.LootGroup.AbundanceLootModTypes.Armor:
			return LootContainer.ArmorCountModifier;
		case LootContainer.LootGroup.AbundanceLootModTypes.Melee:
			return LootContainer.MeleeCountModifier;
		case LootContainer.LootGroup.AbundanceLootModTypes.Ranged:
			return LootContainer.RangedCountModifier;
		case LootContainer.LootGroup.AbundanceLootModTypes.Dukes:
			return LootContainer.DukesCountModifier;
		case LootContainer.LootGroup.AbundanceLootModTypes.Magazines:
			return LootContainer.MagazinesCountModifier;
		default:
			return 1f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float GetSandboxProb(ItemClass _item)
	{
		if (_item.ItemTags.Test_AnySet(LootContainer.treasureTags))
		{
			return LootContainer.TreasureMapChance;
		}
		return 1f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float getProbability(EntityPlayer _player, LootContainer.LootEntry _item, float _lootstage, bool _ignoreLootProb)
	{
		if (_player != null && !_item.HasRequirements(_player))
		{
			return 0f;
		}
		if (_item.lootProbTemplate != string.Empty && LootContainer.lootProbTemplates.ContainsKey(_item.lootProbTemplate))
		{
			LootContainer.LootProbabilityTemplate lootProbabilityTemplate = LootContainer.lootProbTemplates[_item.lootProbTemplate];
			int i = 0;
			while (i < lootProbabilityTemplate.templates.Count)
			{
				LootContainer.LootEntry lootEntry = lootProbabilityTemplate.templates[i];
				if (lootEntry.minLevel <= _lootstage && lootEntry.maxLevel >= _lootstage)
				{
					if (_item.item != null && !_item.item.itemValue.ItemClass.ItemTags.IsEmpty)
					{
						float sandboxProb = LootContainer.GetSandboxProb(_item.item.itemValue.ItemClass);
						if (_ignoreLootProb)
						{
							return lootEntry.prob * sandboxProb;
						}
						return EffectManager.GetValue(PassiveEffects.LootProb, null, lootEntry.prob, _player, null, _item.item.itemValue.ItemClass.ItemTags, true, true, true, true, true, 1, true, false) * sandboxProb;
					}
					else
					{
						if (_item.tags.IsEmpty)
						{
							return lootEntry.prob;
						}
						return EffectManager.GetValue(PassiveEffects.LootProb, null, lootEntry.prob, _player, null, _item.tags, true, true, true, true, true, 1, true, false);
					}
				}
				else
				{
					i++;
				}
			}
		}
		if (_item.item != null && !_item.item.itemValue.ItemClass.ItemTags.IsEmpty)
		{
			float sandboxProb2 = LootContainer.GetSandboxProb(_item.item.itemValue.ItemClass);
			if (_ignoreLootProb)
			{
				return _item.prob * sandboxProb2;
			}
			return EffectManager.GetValue(PassiveEffects.LootProb, null, _item.prob, _player, null, _item.item.itemValue.ItemClass.ItemTags, true, true, true, true, true, 1, true, false) * sandboxProb2;
		}
		else
		{
			if (_item.tags.IsEmpty)
			{
				return _item.prob;
			}
			return EffectManager.GetValue(PassiveEffects.LootProb, null, _item.prob, _player, null, _item.tags, true, true, true, true, true, 1, true, false);
		}
	}

	public void ExecuteBuffActions(int instigatorId, EntityAlive target)
	{
		if (this.BuffActions != null)
		{
			for (int i = 0; i < this.BuffActions.Count; i++)
			{
				target.Buffs.AddBuff(this.BuffActions[i], -1, true, false, -1f);
			}
		}
	}

	public IList<ItemStack> Spawn(GameRandom random, int _maxItems, float playerLevelPercentage, float rareLootChance, EntityPlayer player, FastTags<TagGroup.Global> containerTags, bool uniqueItems, bool ignoreLootProb, bool ignoreLootAbundance)
	{
		List<ItemStack> list = new List<ItemStack>();
		float num = 1f;
		int num2;
		if (ignoreLootAbundance)
		{
			num2 = Mathf.Min(LootContainer.RandomSpawnCount(random, this.minCount, this.maxCount, 1f), _maxItems);
			num = 1f;
		}
		else if (LootContainer.GlobalCountModifier > 1f)
		{
			ValueTuple<int, int, bool> modifiedCountFromSandbox = LootContainer.GetModifiedCountFromSandbox(this.minCount, this.maxCount, LootContainer.GlobalCountModifier);
			int item = modifiedCountFromSandbox.Item1;
			int item2 = modifiedCountFromSandbox.Item2;
			bool item3 = modifiedCountFromSandbox.Item3;
			num2 = Mathf.Min(LootContainer.RandomSpawnCount(random, item, item2, 1f), _maxItems);
			if (!item3)
			{
				num = LootContainer.GlobalCountModifier;
			}
		}
		else
		{
			num2 = Mathf.Min(LootContainer.RandomSpawnCount(random, this.minCount, this.maxCount, 1f), _maxItems);
			num = LootContainer.GlobalCountModifier;
		}
		List<string> list2 = new List<string>();
		if (LootContainer.SpawnLootItemsFromList(random, this.itemsToSpawn, num2, num, list, ref _maxItems, playerLevelPercentage, rareLootChance, this.lootQualityTemplate, player, containerTags, uniqueItems, ignoreLootProb, false, ignoreLootAbundance, list2, ignoreLootAbundance))
		{
			foreach (string text in list2)
			{
				player.Buffs.AddBuff(text, -1, true, false, -1f);
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, LootContainer> lootContainers = new CaseInsensitiveStringDictionary<LootContainer>();

	public static readonly Dictionary<string, LootContainer.LootGroup> lootGroups = new Dictionary<string, LootContainer.LootGroup>();

	public static readonly Dictionary<string, LootContainer.LootQualityTemplate> lootQualityTemplates = new Dictionary<string, LootContainer.LootQualityTemplate>();

	public static readonly Dictionary<string, LootContainer.LootProbabilityTemplate> lootProbTemplates = new Dictionary<string, LootContainer.LootProbabilityTemplate>();

	public string Name;

	public string soundOpen;

	public string soundClose;

	public Vector2i size;

	public float openTime;

	public int minCount;

	public int maxCount;

	public LootContainer.DestroyOnClose destroyOnClose;

	public string lootQualityTemplate;

	public List<string> BuffActions;

	public string OnOpenEvent = "";

	public bool ignoreLootAbundance;

	public bool useUnmodifiedLootstage;

	public bool UniqueItems;

	public bool IgnoreLootProb;

	public readonly List<LootContainer.LootEntry> itemsToSpawn = new List<LootContainer.LootEntry>();

	public static Dictionary<EntityPlayer, string[]> OverrideItems = new Dictionary<EntityPlayer, string[]>();

	public static float GlobalCountModifier = 1f;

	public static float FoodCountModifier = 1f;

	public static float DrinkCountModifier = 1f;

	public static float AmmoCountModifier = 1f;

	public static float MedicalCountModifier = 1f;

	public static float ResourceCountModifier = 1f;

	public static float ArmorCountModifier = 1f;

	public static float MeleeCountModifier = 1f;

	public static float RangedCountModifier = 1f;

	public static float DukesCountModifier = 1f;

	public static float MagazinesCountModifier = 1f;

	public static float TreasureMapChance = 1f;

	public static int LootMaxTier = 6;

	public static float LootTimerModifier = 1f;

	public static float LootBagChance = 1f;

	public static bool NoLoot = false;

	public static FastTags<TagGroup.Global> foodTags = FastTags<TagGroup.Global>.Parse("food");

	public static FastTags<TagGroup.Global> drinksTags = FastTags<TagGroup.Global>.Parse("drinks");

	public static FastTags<TagGroup.Global> ammoTags = FastTags<TagGroup.Global>.Parse("ammo");

	public static FastTags<TagGroup.Global> medicalTags = FastTags<TagGroup.Global>.Parse("medical");

	public static FastTags<TagGroup.Global> junkTags = FastTags<TagGroup.Global>.Parse("junk");

	public static FastTags<TagGroup.Global> armorTags = FastTags<TagGroup.Global>.Parse("armor,armorParts");

	public static FastTags<TagGroup.Global> meleeTags = FastTags<TagGroup.Global>.Parse("melee,meleeParts");

	public static FastTags<TagGroup.Global> rangedTags = FastTags<TagGroup.Global>.Parse("ranged,rangedParts");

	public static FastTags<TagGroup.Global> dukesTags = FastTags<TagGroup.Global>.Parse("dukes");

	public static FastTags<TagGroup.Global> magazineTags = FastTags<TagGroup.Global>.Parse("csm");

	public static FastTags<TagGroup.Global> treasureTags = FastTags<TagGroup.Global>.Parse("treasureMap");

	public enum DestroyOnClose
	{
		False,
		True,
		Empty
	}

	public class LootItem
	{
		public ItemValue itemValue;
	}

	public class LootGroup
	{
		public string name;

		public string lootQualityTemplate;

		public int minCount;

		public int maxCount;

		public int minQuality = -1;

		public int maxQuality = -1;

		public float minLevel;

		public float maxLevel;

		public string[] modsToInstall;

		public float modChance = 1f;

		public readonly List<LootContainer.LootEntry> items = new List<LootContainer.LootEntry>();

		public LootContainer.LootGroup.AbundanceLootModTypes abundanceType;

		public enum AbundanceLootModTypes
		{
			None,
			Food,
			Drinks,
			Ammo,
			Meds,
			Resources,
			Armor,
			Melee,
			Ranged,
			Dukes,
			Magazines
		}
	}

	public class LootEntry
	{
		public bool HasRequirements(EntityPlayer player)
		{
			if (this.Requirements != null)
			{
				for (int i = 0; i < this.Requirements.Count; i++)
				{
					if (!this.Requirements[i].CheckRequirement(player))
					{
						return false;
					}
				}
			}
			return true;
		}

		public string lootProbTemplate;

		public int minCount;

		public int maxCount;

		public int minQuality;

		public int maxQuality;

		public float minLevel;

		public float maxLevel;

		public float prob;

		public bool forceProb;

		public string[] modsToInstall;

		public float modChance = 1f;

		public float lootstageCountMod;

		public LootContainer.LootItem item;

		public LootContainer.LootGroup group;

		public LootContainer.LootGroup parentGroup;

		public FastTags<TagGroup.Global> tags;

		public string[] buffsToAdd;

		public bool randomDurability = true;

		public List<BaseLootEntryRequirement> Requirements;
	}

	public class LootProbabilityTemplate
	{
		public string name;

		public readonly List<LootContainer.LootEntry> templates = new List<LootContainer.LootEntry>();
	}

	public class LootQualityTemplate
	{
		public string name;

		public readonly List<LootContainer.LootGroup> templates = new List<LootContainer.LootGroup>();
	}
}
