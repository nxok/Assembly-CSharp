using System;
using System.Collections.Generic;
using UnityEngine;

public class TraderInfo
{
	public static float BuyMarkup
	{
		get
		{
			return TraderInfo.buyMarkup;
		}
		set
		{
			TraderInfo.buyMarkup = value;
		}
	}

	public static float SellMarkdown
	{
		get
		{
			return TraderInfo.sellMarkdown;
		}
		set
		{
			TraderInfo.sellMarkdown = value;
		}
	}

	public static float QualityMinMod
	{
		get
		{
			return TraderInfo.qualityMinMod;
		}
		set
		{
			TraderInfo.qualityMinMod = value;
		}
	}

	public static float QualityMaxMod
	{
		get
		{
			return TraderInfo.qualityMaxMod;
		}
		set
		{
			TraderInfo.qualityMaxMod = value;
		}
	}

	public static string CurrencyItem { get; set; }

	public int ResetInterval
	{
		get
		{
			if (this.IsVendingMachine)
			{
				if (TraderInfo.VendingResetInterval != -1)
				{
					return TraderInfo.VendingResetInterval;
				}
			}
			else if (TraderInfo.GlobalResetInterval != -1)
			{
				return TraderInfo.GlobalResetInterval;
			}
			return this.resetInterval;
		}
		set
		{
			this.resetInterval = value;
		}
	}

	public int ResetIntervalInTicks
	{
		get
		{
			if (this.IsVendingMachine)
			{
				if (TraderInfo.VendingResetIntervalInTicks != -1)
				{
					return TraderInfo.VendingResetIntervalInTicks;
				}
			}
			else if (TraderInfo.GlobalResetIntervalInTicks != -1)
			{
				return TraderInfo.GlobalResetIntervalInTicks;
			}
			return this.resetIntervalInTicks;
		}
		set
		{
			this.resetIntervalInTicks = value;
		}
	}

	public static TraderInfo.TraderHourPresets TraderHoursPreset
	{
		get
		{
			return TraderInfo.traderHoursPreset;
		}
		set
		{
			TraderInfo.traderHoursPreset = value;
			if (TraderInfo.traderHoursPreset != TraderInfo.TraderHourPresets.Default)
			{
				switch (TraderInfo.traderHoursPreset)
				{
				case TraderInfo.TraderHourPresets.MorningOnly:
					TraderInfo.GlobalOpenTime = GameUtils.DayTimeToWorldTime(1, 4, 1);
					TraderInfo.GlobalCloseTime = GameUtils.DayTimeToWorldTime(1, 11, 50);
					break;
				case TraderInfo.TraderHourPresets.MidDayOnly:
					TraderInfo.GlobalOpenTime = GameUtils.DayTimeToWorldTime(1, 10, 1);
					TraderInfo.GlobalCloseTime = GameUtils.DayTimeToWorldTime(1, 17, 50);
					break;
				case TraderInfo.TraderHourPresets.EveningOnly:
					TraderInfo.GlobalOpenTime = GameUtils.DayTimeToWorldTime(1, 16, 1);
					TraderInfo.GlobalCloseTime = GameUtils.DayTimeToWorldTime(1, 21, 50);
					break;
				case TraderInfo.TraderHourPresets.NightOnly:
					TraderInfo.GlobalOpenTime = GameUtils.DayTimeToWorldTime(1, 22, 1);
					TraderInfo.GlobalCloseTime = GameUtils.DayTimeToWorldTime(1, 3, 50);
					break;
				case TraderInfo.TraderHourPresets.OnlyClosedOnBM:
				{
					ValueTuple<int, int> valueTuple = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
					int item = valueTuple.Item1;
					int item2 = valueTuple.Item2;
					TraderInfo.GlobalOpenTime = GameUtils.DayTimeToWorldTime(1, item2, 0);
					TraderInfo.GlobalCloseTime = GameUtils.DayTimeToWorldTime(1, item - 1, 50);
					break;
				}
				}
				TraderInfo.GlobalWarningTime = TraderInfo.GlobalCloseTime - 300UL;
			}
		}
	}

	public int RentTimeInSeconds
	{
		get
		{
			return this.RentTimeInDays * 60 * GamePrefs.GetInt(EnumGamePrefs.DayNightLength);
		}
	}

	public int RentTimeInTicks
	{
		get
		{
			return this.RentTimeInDays * 24000;
		}
	}

	public ulong GetOpenTime()
	{
		if (TraderInfo.traderHoursPreset != TraderInfo.TraderHourPresets.Default)
		{
			return TraderInfo.GlobalOpenTime;
		}
		return this.OpenTime;
	}

	public ulong GetCloseTime()
	{
		if (TraderInfo.traderHoursPreset != TraderInfo.TraderHourPresets.Default)
		{
			return TraderInfo.GlobalCloseTime;
		}
		return this.CloseTime;
	}

	public ulong GetWarningTime()
	{
		if (TraderInfo.traderHoursPreset != TraderInfo.TraderHourPresets.Default)
		{
			return TraderInfo.GlobalWarningTime;
		}
		return this.WarningTime;
	}

	public bool IsOpen
	{
		get
		{
			if (TraderInfo.traderHoursPreset == TraderInfo.TraderHourPresets.OnlyClosedOnBM)
			{
				int @int = GameStats.GetInt(EnumGameStats.BloodMoonDay);
				int num = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
				if (num < @int || num > @int + 1)
				{
					return true;
				}
			}
			if (!this.UseOpenHours || TraderInfo.traderHoursPreset == TraderInfo.TraderHourPresets.AlwaysOpen || World.SandboxUseTraderArea != TraderAreaStates.Default)
			{
				return true;
			}
			ulong num2 = GameManager.Instance.World.worldTime % 24000UL;
			ulong openTime = this.GetOpenTime();
			ulong closeTime = this.GetCloseTime();
			if (openTime < closeTime)
			{
				return openTime < num2 && num2 < closeTime;
			}
			return num2 > openTime || num2 < closeTime;
		}
	}

	public bool IsTraderActivitiesOpen
	{
		get
		{
			if (TraderInfo.traderHoursPreset == TraderInfo.TraderHourPresets.OnlyClosedOnBM)
			{
				return !GameManager.Instance.World.isEventBloodMoon;
			}
			ulong num = GameManager.Instance.World.worldTime % 24000UL;
			ulong openTime = this.GetOpenTime();
			ulong closeTime = this.GetCloseTime();
			if (openTime < closeTime)
			{
				return openTime < num && num < closeTime;
			}
			return num > openTime || num < closeTime;
		}
	}

	public bool ShouldPlayOpenSound
	{
		get
		{
			if (TraderInfo.traderHoursPreset == TraderInfo.TraderHourPresets.OnlyClosedOnBM)
			{
				bool isEventBloodMoon = GameManager.Instance.World.isEventBloodMoon;
				if (this.lastClosedBM != isEventBloodMoon)
				{
					if (!this.lastClosedBM)
					{
						this.lastClosedBM = isEventBloodMoon;
						return false;
					}
					this.lastClosedBM = isEventBloodMoon;
				}
			}
			ulong openTime = this.GetOpenTime();
			ulong num = GameManager.Instance.World.worldTime % 24000UL;
			return num > openTime && num < openTime + 100UL;
		}
	}

	public bool ShouldPlayCloseSound
	{
		get
		{
			if (TraderInfo.traderHoursPreset == TraderInfo.TraderHourPresets.OnlyClosedOnBM && GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime) != GameStats.GetInt(EnumGameStats.BloodMoonDay))
			{
				return false;
			}
			ulong closeTime = this.GetCloseTime();
			ulong num = GameManager.Instance.World.worldTime % 24000UL;
			return num > closeTime && num < closeTime + 100UL;
		}
	}

	public bool IsWarningTime
	{
		get
		{
			if (TraderInfo.traderHoursPreset == TraderInfo.TraderHourPresets.OnlyClosedOnBM && GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime) != GameStats.GetInt(EnumGameStats.BloodMoonDay))
			{
				return false;
			}
			if (!this.UseOpenHours || TraderInfo.traderHoursPreset == TraderInfo.TraderHourPresets.AlwaysOpen || World.SandboxUseTraderArea != TraderAreaStates.Default)
			{
				return false;
			}
			ulong num = GameManager.Instance.World.worldTime % 24000UL;
			ulong openTime = this.GetOpenTime();
			ulong warningTime = this.GetWarningTime();
			if (openTime < warningTime)
			{
				return warningTime < num && num < warningTime + 100UL;
			}
			return warningTime > openTime || num < warningTime + 100UL;
		}
	}

	public static void InitStatic()
	{
		TraderInfo.traderInfoList = new TraderInfo[256];
		TraderInfo.traderItemGroups = new Dictionary<string, TraderInfo.TraderItemGroup>();
	}

	public void Init()
	{
		TraderInfo.traderInfoList[this.Id] = this;
	}

	public static void Cleanup()
	{
		TraderInfo.traderInfoList = null;
		TraderInfo.traderItemGroups = null;
	}

	public static string GetNextTraderTime()
	{
		if (TraderInfo.traderDisplayInfo == null)
		{
			if (TraderInfo.traderInfoList.Length == 0)
			{
				return "";
			}
			TraderInfo.traderDisplayInfo = new TraderInfo.TraderDisplayInfo();
		}
		TraderInfo.traderDisplayInfo.Refresh();
		return TraderInfo.traderDisplayInfo.GetTimeText();
	}

	public static string GetNextTraderText()
	{
		if (TraderInfo.traderDisplayInfo == null)
		{
			if (TraderInfo.traderInfoList.Length == 0)
			{
				return "";
			}
			TraderInfo.traderDisplayInfo = new TraderInfo.TraderDisplayInfo();
		}
		TraderInfo.traderDisplayInfo.Refresh();
		return TraderInfo.traderDisplayInfo.GetTimeTitle();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyRandomDegradation(ref ItemValue _itemValue)
	{
		_itemValue.Meta = ItemClass.GetForId(_itemValue.type).GetInitialMetadata(_itemValue);
		int maxUseTimes = _itemValue.MaxUseTimes;
		if (maxUseTimes == 0)
		{
			return;
		}
		float num = GameManager.Instance.World.GetGameRandom().RandomFloat * 0.6f + 0.2f;
		_itemValue.UseTimes = (float)((int)((float)maxUseTimes * num));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyQuality(ref ItemValue _itemValue, int minQuality = 1, int maxQuality = 6)
	{
		if (ItemClass.list[_itemValue.type].HasQuality || ItemClass.list[_itemValue.type].HasSubItems)
		{
			_itemValue = new ItemValue(_itemValue.type, Mathf.Clamp(minQuality, 1, maxQuality), maxQuality, false, null, 1f);
		}
	}

	public List<ItemStack> Spawn(GameRandom random)
	{
		List<ItemStack> list = new List<ItemStack>();
		this.SpawnLootItemsFromList(random, this.traderItems, -1, list, null);
		return list;
	}

	public List<ItemStack> SpawnTierGroup(GameRandom random, int tierGroupIndex)
	{
		List<ItemStack> list = new List<ItemStack>();
		this.TierItemGroups[tierGroupIndex].traderItems.Shuffle<TraderInfo.TraderItemEntry>();
		int num = this.RandomSpawnCount(random, this.minCount, this.maxCount);
		this.SpawnLootItemsFromList(random, this.TierItemGroups[tierGroupIndex].traderItems, num, list, null);
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnItem(GameRandom random, TraderInfo.TraderItemEntry template, ItemValue item, int countToSpawn, List<ItemStack> spawnedItems)
	{
		if (countToSpawn < 1)
		{
			return;
		}
		ItemClass itemClass = item.ItemClass;
		if (itemClass == null)
		{
			return;
		}
		if (item.HasQuality && TraderInfo.TraderMaxTier == 0)
		{
			return;
		}
		ItemClass itemClass2 = ItemClass.HandleSandboxTechType(itemClass);
		if (itemClass2 != itemClass)
		{
			if (itemClass2 == null)
			{
				return;
			}
			item = ItemClass.GetItem(itemClass2.Name, false);
			itemClass = itemClass2;
		}
		countToSpawn = Math.Min(countToSpawn, itemClass.Stacknumber.Value);
		int num = (itemClass.IsBlock() ? Block.list[item.type].EconomicBundleSize : itemClass.EconomicBundleSize);
		if (itemClass.EconomicValue == -1f)
		{
			return;
		}
		if (num > 1)
		{
			int num2 = countToSpawn % num;
			if (num2 > 0)
			{
				countToSpawn -= num2;
			}
			if (countToSpawn == 0)
			{
				countToSpawn = num;
			}
		}
		if (itemClass.CanStack())
		{
			int value = ItemClass.GetForId(item.type).Stacknumber.Value;
			for (int i = 0; i < spawnedItems.Count; i++)
			{
				ItemStack itemStack = spawnedItems[i];
				if (itemStack.itemValue.type == item.type)
				{
					if (itemStack.CanStack(countToSpawn))
					{
						itemStack.count += countToSpawn;
						spawnedItems[i] = itemStack;
						return;
					}
					int num3 = value - itemStack.count;
					itemStack.count = value;
					spawnedItems[i] = itemStack;
					countToSpawn -= num3;
				}
			}
		}
		int num4 = template.minQuality;
		int num5 = template.maxQuality;
		ItemValue itemValue;
		if (item.HasQuality)
		{
			if (num4 <= -1)
			{
				num4 = 1;
				num5 = 6;
			}
			if (num5 > TraderInfo.TraderMaxTier)
			{
				num5 = TraderInfo.TraderMaxTier;
			}
			if (num5 < num4)
			{
				return;
			}
			if (template != null && template.parentGroup != null && template.parentGroup.modsToInstall.Length != 0)
			{
				itemValue = new ItemValue(item.type, num4, num5, true, template.parentGroup.modsToInstall, template.parentGroup.modChance);
			}
			else
			{
				itemValue = new ItemValue(item.type, num4, num5, true, template.modsToInstall, template.modChance);
			}
		}
		else
		{
			itemValue = new ItemValue(item.type, true);
		}
		if (itemValue.ItemClass != null && itemValue.ItemClass.Actions != null && itemValue.ItemClass.Actions.Length != 0 && itemValue.ItemClass.Actions[0] != null)
		{
			itemValue.Meta = 0;
		}
		itemClass.AddGSStats(itemValue, -1, random);
		ItemStack itemStack2 = new ItemStack(itemValue, countToSpawn);
		spawnedItems.Add(itemStack2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int RandomSpawnCount(GameRandom random, int min, int max)
	{
		if (min < 0)
		{
			return -1;
		}
		float num = random.RandomRange((float)min - 0.49f, (float)max + 0.49f);
		if (num <= (float)min)
		{
			return min;
		}
		if (num > (float)max)
		{
			num = (float)max;
		}
		int num2 = (int)num;
		float num3 = num - (float)num2;
		if (random.RandomFloat < num3)
		{
			num2++;
		}
		return num2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnAllItemsFromList(GameRandom random, List<TraderInfo.TraderItemEntry> itemSet, List<ItemStack> spawnedItems)
	{
		float num = (this.IsVendingMachine ? TraderInfo.VendingItemAbundance : TraderInfo.TraderItemAbundance);
		for (int i = 0; i < itemSet.Count; i++)
		{
			TraderInfo.TraderItemEntry traderItemEntry = itemSet[i];
			int num2 = this.RandomSpawnCount(random, (int)((float)traderItemEntry.minCount * num), (int)((float)traderItemEntry.maxCount * num));
			if (traderItemEntry.group != null)
			{
				this.SpawnItemsFromGroup(random, traderItemEntry.group, num2, spawnedItems, traderItemEntry.uniqueOnly);
			}
			else
			{
				this.SpawnItem(random, traderItemEntry, traderItemEntry.item.itemValue, num2, spawnedItems);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnItemsFromGroup(GameRandom random, TraderInfo.TraderItemGroup group, int numToSpawn, List<ItemStack> spawnedItems, bool uniqueOnly)
	{
		List<int> list = null;
		if (group.uniqueOnly || uniqueOnly)
		{
			list = new List<int>();
		}
		for (int i = 0; i < numToSpawn; i++)
		{
			int num = this.RandomSpawnCount(random, group.minCount, group.maxCount);
			this.SpawnLootItemsFromList(random, group.items, num, spawnedItems, list);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnLootItemsFromList(GameRandom random, List<TraderInfo.TraderItemEntry> itemSet, int numToSpawn, List<ItemStack> spawnedItems, List<int> usedIndices)
	{
		if (numToSpawn < 1)
		{
			if (numToSpawn == -1)
			{
				this.SpawnAllItemsFromList(random, itemSet, spawnedItems);
			}
			return;
		}
		float num = 0f;
		for (int i = 0; i < itemSet.Count; i++)
		{
			TraderInfo.TraderItemEntry traderItemEntry = itemSet[i];
			if (usedIndices == null || !usedIndices.Contains(i))
			{
				num += traderItemEntry.prob;
			}
		}
		if (num == 0f)
		{
			return;
		}
		for (int j = 0; j < numToSpawn; j++)
		{
			float num2 = 0f;
			float randomFloat = random.RandomFloat;
			for (int k = 0; k < itemSet.Count; k++)
			{
				TraderInfo.TraderItemEntry traderItemEntry2 = itemSet[k];
				if (usedIndices == null || !usedIndices.Contains(k))
				{
					num2 += traderItemEntry2.prob / num;
					if (randomFloat <= num2)
					{
						int num3 = this.RandomSpawnCount(random, traderItemEntry2.minCount, traderItemEntry2.maxCount);
						if (usedIndices != null)
						{
							usedIndices.Add(k);
						}
						if (traderItemEntry2.group != null)
						{
							this.SpawnItemsFromGroup(random, traderItemEntry2.group, num3, spawnedItems, traderItemEntry2.uniqueOnly);
							break;
						}
						this.SpawnItem(random, traderItemEntry2, traderItemEntry2.item.itemValue, num3, spawnedItems);
						break;
					}
				}
			}
		}
	}

	public static TraderInfo[] traderInfoList;

	public static Dictionary<string, TraderInfo.TraderItemGroup> traderItemGroups;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float buyMarkup;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float sellMarkdown;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float qualityMinMod;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float qualityMaxMod;

	public int Id;

	public float SalesMarkup;

	public static bool TraderDialog = true;

	public static int GlobalResetInterval = -1;

	public static int GlobalResetIntervalInTicks = -1;

	public static int VendingResetInterval = -1;

	public static int VendingResetIntervalInTicks = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int resetInterval = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int resetIntervalInTicks = 24000;

	public int MaxItems = 50;

	public int minCount;

	public int maxCount;

	public bool AllowBuy = true;

	public bool AllowSell = true;

	public bool IsVendingMachine;

	public float OverrideBuyMarkup = -1f;

	public float OverrideSellMarkdown = -1f;

	public bool UseOpenHours;

	public ulong OpenTime;

	public ulong CloseTime;

	public ulong WarningTime;

	public bool PlayerOwned;

	public bool Rentable;

	public int RentCost;

	public int RentTimeInDays;

	public static int TraderMaxTier = 6;

	public static int TraderBuyLimit = 3;

	public static float TraderItemAbundance = 1f;

	public static float VendingItemAbundance = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastClosedBM;

	[PublicizedFrom(EAccessModifier.Private)]
	public static TraderInfo.TraderHourPresets traderHoursPreset;

	public static TraderInfo.TraderDaysPresets TraderDayPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ulong GlobalOpenTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ulong GlobalCloseTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ulong GlobalWarningTime;

	public List<TraderInfo.TierItemGroup> TierItemGroups = new List<TraderInfo.TierItemGroup>();

	public List<TraderInfo.TraderItemEntry> traderItems = new List<TraderInfo.TraderItemEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static float lastTime = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static TraderInfo.TraderDisplayInfo traderDisplayInfo = null;

	public enum TraderHourPresets
	{
		Default,
		MorningOnly,
		MidDayOnly,
		EveningOnly,
		NightOnly,
		OnlyClosedOnBM,
		AlwaysOpen
	}

	public enum TraderDaysPresets
	{
		Default,
		Everyday,
		EveryOtherDay,
		EveryThreeDays,
		EveryFourDays,
		EveryFiveDays
	}

	public class TraderItem
	{
		public ItemValue itemValue;
	}

	public class TraderItemGroup
	{
		public string name;

		public int minCount;

		public int maxCount;

		public int minQuality = -1;

		public int maxQuality = -1;

		public string[] modsToInstall;

		public float modChance = 1f;

		public bool uniqueOnly;

		public List<TraderInfo.TraderItemEntry> items = new List<TraderInfo.TraderItemEntry>();
	}

	public class TraderItemEntry
	{
		public int minCount;

		public int maxCount;

		public int minQuality;

		public int maxQuality;

		public float prob;

		public string[] modsToInstall;

		public float modChance = 1f;

		public bool uniqueOnly;

		public TraderInfo.TraderItem item;

		public TraderInfo.TraderItemGroup group;

		public TraderInfo.TraderItemGroup parentGroup;
	}

	public class TierItemGroup
	{
		public int minLevel;

		public int maxLevel;

		public int minCount;

		public int maxCount;

		public List<TraderInfo.TraderItemEntry> traderItems = new List<TraderInfo.TraderItemEntry>();
	}

	public class TraderDisplayInfo
	{
		public void Refresh()
		{
			if (GameManager.Instance.World == null)
			{
				return;
			}
			if (this.info == null)
			{
				this.info = TraderInfo.traderInfoList[1];
				if (this.info == null)
				{
					return;
				}
				this.OpenTime = this.info.GetOpenTime();
				this.CloseTime = this.info.GetCloseTime();
				this.OpenTimeText = GameUtils.WorldTimeToHourMinutesString(this.OpenTime);
				this.CloseTimeText = GameUtils.WorldTimeToHourMinutesString(this.CloseTime);
			}
			if (Time.time - this.lastUpdate > 3f)
			{
				ulong num = GameManager.Instance.World.worldTime % 24000UL;
				if (this.OpenTime < this.CloseTime)
				{
					this.IsOpen = this.OpenTime < num && num < this.CloseTime;
				}
				else
				{
					this.IsOpen = num > this.OpenTime || num < this.CloseTime;
				}
				this.lastUpdate = Time.time;
			}
		}

		public string GetTimeTitle()
		{
			if (!this.IsOpen)
			{
				return "Opens at";
			}
			return "Closes at";
		}

		public string GetTimeText()
		{
			if (!this.IsOpen)
			{
				return this.OpenTimeText;
			}
			return this.CloseTimeText;
		}

		public ulong OpenTime;

		public ulong CloseTime;

		public string OpenTimeText;

		public string CloseTimeText;

		public bool IsOpen;

		[PublicizedFrom(EAccessModifier.Private)]
		public float lastUpdate;

		[PublicizedFrom(EAccessModifier.Private)]
		public TraderInfo info;
	}
}
