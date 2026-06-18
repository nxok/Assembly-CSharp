using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SteelSeries.GameSense;
using SteelSeries.GameSense.DeviceZone;
using UnityEngine;

public class GameSenseManager : IEntityBuffsChanged
{
	public static GameSenseManager Instance
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static GameSenseManager()
	{
		if (GameManager.IsDedicatedServer || Application.isEditor)
		{
			return;
		}
		if (GameUtils.GetLaunchArgument("nogamesense") != null)
		{
			return;
		}
		GameSenseManager.Instance = new GameSenseManager();
	}

	public static bool GameSenseInstalled
	{
		get
		{
			return SdFile.Exists(GSClient._getPropsPath());
		}
	}

	public void Init()
	{
		if (!GameSenseManager.GameSenseInstalled)
		{
			Log.Out("GameSense server not found (no props file), disabling");
			GameSenseManager.Instance = null;
			return;
		}
		new GameObject("GameSense")
		{
			hideFlags = 61
		}.AddComponent<GSClient>();
		GSClient.Instance.RegisterGame("7dtd", "7 Days to Die", "The Fun Pimps");
		GameStats.OnChangedDelegates += this.GameStats_OnChanged;
		ThreadManager.StartCoroutine(this.BindEventsCo());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator BindEventsCo()
	{
		yield return new WaitForSeconds(0.1f);
		this.BindEventTime();
		this.BindEventAmmo();
		this.BindEventHealth();
		this.BindEventDurability();
		this.BindEventStealth();
		this.BindEventBleeding();
		this.BindEventBloodmoon();
		this.BindEventCompass();
		this.BindEventHit();
		this.BindEventDukes();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventTime()
	{
		ColorRanges colorRanges = ColorRanges.Create(new ColorRange[]
		{
			new ColorRange(0U, 16U, ColorStatic.Create(byte.MaxValue, 0, 0)),
			new ColorRange(17U, 32U, ColorStatic.Create(byte.MaxValue, 160, 0)),
			new ColorRange(33U, 49U, ColorStatic.Create(byte.MaxValue, byte.MaxValue, 0)),
			new ColorRange(50U, 66U, ColorStatic.Create(byte.MaxValue, byte.MaxValue, 0)),
			new ColorRange(67U, 74U, ColorStatic.Create(byte.MaxValue, 160, 0)),
			new ColorRange(75U, 82U, ColorStatic.Create(byte.MaxValue, 80, 0)),
			new ColorRange(83U, 91U, ColorStatic.Create(byte.MaxValue, 40, 0)),
			new ColorRange(92U, 100U, ColorStatic.Create(byte.MaxValue, 0, 0))
		});
		AbstractHandler abstractHandler = ColorHandler.Create(ScriptableObject.CreateInstance<RGBPerkeyZoneFunctionKeys>(), 1, colorRanges);
		this.BindEventWrapper("TIME", 0, 100, 15, false, new AbstractHandler[] { abstractHandler });
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventAmmo()
	{
		AbstractHandler abstractHandler = ColorHandler.Create(ScriptableObject.CreateInstance<RGBPerkeyZoneNumberKeys>(), 1, ColorGradient.Create(new RGB(160, 0, byte.MaxValue), new RGB(0, 0, byte.MaxValue)));
		this.BindEventWrapper("AMMO", 0, 100, 3, false, new AbstractHandler[] { abstractHandler });
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventHealth()
	{
		AbstractHandler abstractHandler = ColorHandler.Create(ScriptableObject.CreateInstance<RGBPerkeyZoneRowQ>(), 1, ColorGradient.Create(new RGB(byte.MaxValue, 0, 0), new RGB(0, byte.MaxValue, 0)), RateRange.Create(new FreqRepeatLimitPair[]
		{
			new FreqRepeatLimitPair(0U, 10U, 2U, 0U),
			new FreqRepeatLimitPair(11U, 20U, 1U, 0U)
		}));
		this.BindEventWrapper("HEALTH", 0, 100, 1, false, new AbstractHandler[] { abstractHandler });
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventDurability()
	{
		RGBPerkeyZoneCustom rgbperkeyZoneCustom = ScriptableObject.CreateInstance<RGBPerkeyZoneCustom>();
		rgbperkeyZoneCustom.zone = new byte[] { 83, 84, 85, 86 };
		AbstractHandler abstractHandler = ColorHandler.Create(rgbperkeyZoneCustom, 1, ColorGradient.Create(new RGB(130, 130, 0), new RGB(0, byte.MaxValue, 200)));
		this.BindEventWrapper("DURABILITY", 0, 100, 17, false, new AbstractHandler[] { abstractHandler });
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventStealth()
	{
		RGBPerkeyZoneCustom rgbperkeyZoneCustom = ScriptableObject.CreateInstance<RGBPerkeyZoneCustom>();
		rgbperkeyZoneCustom.zone = new byte[] { 224, 225, 57, 43, 53 };
		AbstractHandler abstractHandler = ColorHandler.Create(rgbperkeyZoneCustom, 1, ColorGradient.Create(new RGB(50, 120, 50), new RGB(0, 250, 250)));
		this.BindEventWrapper("STEALTH", 0, 100, 0, false, new AbstractHandler[] { abstractHandler });
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventBleeding()
	{
		AbstractHandler abstractHandler = ColorHandler.Create(ScriptableObject.CreateInstance<RGBPerkeyZoneNavCluster>(), 0, ColorStatic.Create(byte.MaxValue, 0, 0), RateStatic.Create(5U, 0U));
		this.BindEventWrapper("BLEEDING", 0, 1, 0, false, new AbstractHandler[] { abstractHandler });
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventBloodmoon()
	{
		ColorRanges colorRanges = ColorRanges.Create(new ColorRange[]
		{
			new ColorRange(0U, 0U, ColorStatic.Create(0, 0, 0)),
			new ColorRange(1U, 10U, ColorStatic.Create(60, 0, 0)),
			new ColorRange(11U, 20U, ColorStatic.Create(70, 0, 0)),
			new ColorRange(21U, 30U, ColorStatic.Create(80, 0, 0)),
			new ColorRange(31U, 40U, ColorStatic.Create(90, 0, 0)),
			new ColorRange(41U, 50U, ColorStatic.Create(105, 0, 0)),
			new ColorRange(51U, 60U, ColorStatic.Create(120, 0, 0)),
			new ColorRange(61U, 70U, ColorStatic.Create(135, 0, 0)),
			new ColorRange(71U, 80U, ColorStatic.Create(150, 0, 0)),
			new ColorRange(81U, 90U, ColorStatic.Create(190, 0, 0)),
			new ColorRange(91U, 100U, ColorStatic.Create(byte.MaxValue, 0, 0))
		});
		AbstractHandler abstractHandler = ColorHandler.Create(ScriptableObject.CreateInstance<RGBPerkeyZoneRowA>(), 0, colorRanges, RateRange.Create(new FreqRepeatLimitPair[]
		{
			new FreqRepeatLimitPair(81U, 90U, 1U, 0U)
		}));
		this.BindEventWrapper("BLOODMOON", 0, 100, 0, false, new AbstractHandler[] { abstractHandler });
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventCompass()
	{
		foreach (Tuple<string, byte> tuple in new Tuple<string, byte>[]
		{
			new Tuple<string, byte>("NORTH", 96),
			new Tuple<string, byte>("NORTHEAST", 97),
			new Tuple<string, byte>("EAST", 94),
			new Tuple<string, byte>("SOUTHEAST", 91),
			new Tuple<string, byte>("SOUTH", 90),
			new Tuple<string, byte>("SOUTHWEST", 89),
			new Tuple<string, byte>("WEST", 92),
			new Tuple<string, byte>("NORTHWEST", 95)
		})
		{
			RGBPerkeyZoneCustom rgbperkeyZoneCustom = ScriptableObject.CreateInstance<RGBPerkeyZoneCustom>();
			rgbperkeyZoneCustom.zone = new byte[] { tuple.Item2 };
			AbstractHandler abstractHandler = ColorHandler.Create(rgbperkeyZoneCustom, 0, ColorStatic.Create(50, byte.MaxValue, 0));
			this.BindEventWrapper(tuple.Item1, 0, 1, 12, false, new AbstractHandler[] { abstractHandler });
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventHit()
	{
		AbstractHandler abstractHandler = ColorHandler.Create(ScriptableObject.CreateInstance<MouseZoneAll>(), 0, ColorStatic.Create(byte.MaxValue, 0, 0), RateStatic.Create(8U, 0U));
		this.BindEventWrapper("HIT", 0, 1, 7, false, new AbstractHandler[] { abstractHandler });
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventDukes()
	{
		LineData lineData = LineData.Create(LineDataText.Create("Dukes: ", "", true, 0U), LineDataAccessor.ContextFrameKey("dukesstring"));
		FrameDataMultiLine frameDataMultiLine = FrameDataMultiLine.Create(new LineData[] { lineData }, new FrameModifiers(10000U, false), 4);
		AbstractScreenDevice_Zone abstractScreenDevice_Zone = ScriptableObject.CreateInstance<ScreenedZoneOne>();
		ScreenMode screenMode = 0;
		AbstractFrameData[] array = new FrameDataMultiLine[] { frameDataMultiLine };
		ScreenHandler screenHandler = ScreenHandler.Create(abstractScreenDevice_Zone, screenMode, array);
		this.BindEventWrapper("DUKES", 0, 1, 4, true, new AbstractHandler[] { screenHandler });
	}

	public void Cleanup()
	{
		GameStats.OnChangedDelegates -= this.GameStats_OnChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BindEventWrapper(string _eventName, int _minValue, int _maxValue, EventIconId _icon, bool _optionalValue, params AbstractHandler[] _handlers)
	{
		GSClient.Instance.BindEvent(_eventName, _minValue, _maxValue, _icon, _handlers, _optionalValue);
		this.registeredEvents.Add(_eventName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetEventValues()
	{
		foreach (string text in this.registeredEvents)
		{
			GSClient.Instance.SendEvent(text, 0, null);
		}
	}

	public void SessionStarted(EntityPlayerLocal _playerEntity)
	{
		this.ResetEventValues();
		this.entityPlayer = new WeakReference<EntityPlayerLocal>(_playerEntity);
		_playerEntity.PlayerStats.AddBuffChangedDelegate(this);
		this.lastTimePercent = -1;
		this.previousAmmoPercentage = -1;
		this.previousSlotIndex = -1;
		this.previousDurability = -1;
		this.previousStealth = -1;
		this.bloodMoonWarning = GameStats.GetInt(EnumGameStats.BloodMoonWarning);
		this.duskDawnTime = GameUtils.CalcDuskDawnHours(GameStats.GetInt(EnumGameStats.DayLightLength));
		this.lastBloodMoonValue = -1;
		this.lastDirection = GameSenseManager.EDirection45.None;
		this.dukesItem = ItemClass.GetItem(TraderInfo.CurrencyItem, false);
	}

	public void SessionEnded()
	{
		this.ResetEventValues();
		EntityPlayerLocal entityPlayerLocal;
		if (this.entityPlayer.TryGetTarget(out entityPlayerLocal))
		{
			entityPlayerLocal.PlayerStats.RemoveBuffChangedDelegate(this);
		}
		this.entityPlayer = null;
		this.dukesItem = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GameStats_OnChanged(EnumGameStats _gameStat, object _newValue)
	{
		if (_gameStat == EnumGameStats.BloodMoonDay)
		{
			this.bloodMoonDay = GameStats.GetInt(EnumGameStats.BloodMoonDay);
		}
	}

	public void Update()
	{
		if (!GSClient.Instance.isClientActive())
		{
			return;
		}
		this.UpdateEventAmmo();
		this.UpdateEventDurability();
		this.UpdateEventStealth();
		this.UpdateEventDukes();
	}

	public void UpdateEventTime(ulong _worldTime)
	{
		if (!GSClient.Instance.isClientActive())
		{
			return;
		}
		this.UpdateEventBloodmoon(_worldTime);
		int num = (int)(_worldTime % 24000UL) / 240;
		if (this.lastTimePercent == num)
		{
			return;
		}
		this.lastTimePercent = num;
		GSClient.Instance.SendEvent("TIME", num, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEventAmmo()
	{
		EntityPlayerLocal entityPlayerLocal;
		if (this.entityPlayer == null || !this.entityPlayer.TryGetTarget(out entityPlayerLocal) || entityPlayerLocal == null)
		{
			return;
		}
		int focusedItemIdx = entityPlayerLocal.inventory.GetFocusedItemIdx();
		ItemValue itemValue = entityPlayerLocal.inventory.GetItem(focusedItemIdx).itemValue;
		ItemClass itemClass;
		if (itemValue == null || itemValue.type == 0 || (itemClass = itemValue.ItemClass) == null)
		{
			this.previousAmmoPercentage = 0;
			GSClient.Instance.SendEvent("AMMO", 0, null);
			return;
		}
		ItemActionAttack itemActionAttack = itemClass.Actions[0] as ItemActionAttack;
		int num;
		if (itemActionAttack == null || itemActionAttack is ItemActionMelee || (num = (int)EffectManager.GetValue(PassiveEffects.MagazineSize, itemValue, 0f, entityPlayerLocal, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false)) <= 0)
		{
			this.previousAmmoPercentage = 0;
			GSClient.Instance.SendEvent("AMMO", 0, null);
			return;
		}
		int meta = itemValue.Meta;
		int num2 = 100 * meta / num;
		if (num2 == this.previousAmmoPercentage)
		{
			return;
		}
		this.previousAmmoPercentage = num2;
		GSClient.Instance.SendEvent("AMMO", num2, null);
	}

	public void UpdateEventHealth(int _value)
	{
		if (!GSClient.Instance.isClientActive())
		{
			return;
		}
		GSClient.Instance.SendEvent("HEALTH", _value, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEventDurability()
	{
		EntityPlayerLocal entityPlayerLocal;
		if (this.entityPlayer == null || !this.entityPlayer.TryGetTarget(out entityPlayerLocal) || entityPlayerLocal == null)
		{
			return;
		}
		int focusedItemIdx = entityPlayerLocal.inventory.GetFocusedItemIdx();
		ItemValue itemValue = entityPlayerLocal.inventory.GetItem(focusedItemIdx).itemValue;
		ItemClass itemClass;
		if (itemValue == null || itemValue.type == 0 || (itemClass = itemValue.ItemClass) == null || !itemClass.ShowQualityBar)
		{
			this.previousDurability = 0;
			GSClient.Instance.SendEvent("DURABILITY", 0, null);
			return;
		}
		int num = (int)(100f * itemValue.PercentUsesLeft);
		if (num == this.previousDurability)
		{
			return;
		}
		this.previousDurability = num;
		GSClient.Instance.SendEvent("DURABILITY", num, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEventStealth()
	{
		EntityPlayerLocal entityPlayerLocal;
		if (this.entityPlayer == null || !this.entityPlayer.TryGetTarget(out entityPlayerLocal) || entityPlayerLocal == null)
		{
			return;
		}
		int num = (int)(100f * entityPlayerLocal.Stealth.ValuePercentUI);
		if (num == this.previousStealth)
		{
			return;
		}
		this.previousStealth = num;
		GSClient.Instance.SendEvent("STEALTH", num, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEventBleeding(bool _isBleeding)
	{
		GSClient.Instance.SendEvent("BLEEDING", _isBleeding ? 1 : 0, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEventBloodmoon(ulong _worldTime)
	{
		int num = 0;
		ValueTuple<int, int, int> valueTuple = GameUtils.WorldTimeToElements(_worldTime);
		int item = valueTuple.Item1;
		int item2 = valueTuple.Item2;
		int item3 = valueTuple.Item3;
		if (GameUtils.IsBloodMoonTime(_worldTime, this.duskDawnTime, this.bloodMoonDay))
		{
			num = 100;
		}
		else if (this.bloodMoonDay == item && this.bloodMoonWarning >= 0 && this.bloodMoonWarning <= item2)
		{
			int num2 = this.duskDawnTime.Item1 - 2;
			if (item2 > num2)
			{
				num = 85;
			}
			else
			{
				float num3 = (float)item2 + (float)item3 / 60f - (float)this.bloodMoonWarning;
				float num4 = (float)(num2 - this.bloodMoonWarning);
				num = (int)(80f * num3 / num4);
			}
		}
		if (num == this.lastBloodMoonValue)
		{
			return;
		}
		this.lastBloodMoonValue = num;
		GSClient.Instance.SendEvent("BLOODMOON", num, null);
	}

	public void UpdateEventCompass(float _rotation)
	{
		if (!GSClient.Instance.isClientActive())
		{
			return;
		}
		GameSenseManager.EDirection45 edirection;
		if ((double)_rotation > 337.5 || (double)_rotation <= 22.5)
		{
			edirection = GameSenseManager.EDirection45.N;
		}
		else if ((double)_rotation <= 67.5)
		{
			edirection = GameSenseManager.EDirection45.NE;
		}
		else if ((double)_rotation <= 112.5)
		{
			edirection = GameSenseManager.EDirection45.E;
		}
		else if ((double)_rotation <= 157.5)
		{
			edirection = GameSenseManager.EDirection45.SE;
		}
		else if ((double)_rotation <= 202.5)
		{
			edirection = GameSenseManager.EDirection45.S;
		}
		else if ((double)_rotation <= 247.5)
		{
			edirection = GameSenseManager.EDirection45.SW;
		}
		else if ((double)_rotation <= 292.5)
		{
			edirection = GameSenseManager.EDirection45.W;
		}
		else
		{
			edirection = GameSenseManager.EDirection45.NW;
		}
		if (this.lastDirection == edirection)
		{
			return;
		}
		this.lastDirection = edirection;
		GSClient.Instance.SendEvent("NORTH", (edirection == GameSenseManager.EDirection45.N) ? 1 : 0, null);
		GSClient.Instance.SendEvent("NORTHEAST", (edirection == GameSenseManager.EDirection45.NE) ? 1 : 0, null);
		GSClient.Instance.SendEvent("EAST", (edirection == GameSenseManager.EDirection45.E) ? 1 : 0, null);
		GSClient.Instance.SendEvent("SOUTHEAST", (edirection == GameSenseManager.EDirection45.SE) ? 1 : 0, null);
		GSClient.Instance.SendEvent("SOUTH", (edirection == GameSenseManager.EDirection45.S) ? 1 : 0, null);
		GSClient.Instance.SendEvent("SOUTHWEST", (edirection == GameSenseManager.EDirection45.SW) ? 1 : 0, null);
		GSClient.Instance.SendEvent("WEST", (edirection == GameSenseManager.EDirection45.W) ? 1 : 0, null);
		GSClient.Instance.SendEvent("NORTHWEST", (edirection == GameSenseManager.EDirection45.NW) ? 1 : 0, null);
	}

	public void UpdateEventHit()
	{
		if (!GSClient.Instance.isClientActive())
		{
			return;
		}
		this.stopHitEventTime = Time.unscaledTime + 0.5f;
		if (this.stopHitEventCoroutine == null)
		{
			GSClient.Instance.SendEvent("HIT", 1, null);
			this.stopHitEventCoroutine = ThreadManager.StartCoroutine(this.StopHitCo());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator StopHitCo()
	{
		while (Time.unscaledTime < this.stopHitEventTime)
		{
			yield return null;
		}
		GSClient.Instance.SendEvent("HIT", 0, null);
		this.stopHitEventCoroutine = null;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateEventDukes()
	{
		float unscaledTime = Time.unscaledTime;
		if (unscaledTime < this.dukesNextTime)
		{
			return;
		}
		this.dukesNextTime = unscaledTime + 5f;
		EntityPlayerLocal entityPlayerLocal;
		if (this.entityPlayer == null || !this.entityPlayer.TryGetTarget(out entityPlayerLocal) || entityPlayerLocal == null)
		{
			return;
		}
		int num = entityPlayerLocal.inventory.GetItemCount(this.dukesItem, false, -1, -1, true);
		num += entityPlayerLocal.bag.GetItemCount(this.dukesItem, -1, -1, true);
		ContextFrameObject contextFrameObject = new ContextFrameObject();
		contextFrameObject["dukesstring"] = ((double)num).FormatNumberWithMetricPrefix(true, 2);
		ContextFrameObject contextFrameObject2 = contextFrameObject;
		GSClient.Instance.SendEvent("DUKES", num, contextFrameObject2);
	}

	public void EntityBuffAdded(BuffValue _buff)
	{
		if (_buff.BuffClass.Name.EqualsCaseInsensitive("buffInjuryBleeding"))
		{
			this.UpdateEventBleeding(true);
		}
	}

	public void EntityBuffRemoved(BuffValue _buff)
	{
		if (_buff.BuffClass.Name.EqualsCaseInsensitive("buffInjuryBleeding"))
		{
			this.UpdateEventBleeding(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<string> registeredEvents = new HashSet<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const string GameSenseGameName = "7dtd";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string GameSenseGameNameFull = "7 Days to Die";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string GameSenseGameDeveloper = "The Fun Pimps";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EventHealth = "HEALTH";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EventAmmo = "AMMO";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EventTime = "TIME";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EventDurability = "DURABILITY";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EventStealth = "STEALTH";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EventBleeding = "BLEEDING";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string BleedingBuffName = "buffInjuryBleeding";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EventBloodmoon = "BLOODMOON";

	[PublicizedFrom(EAccessModifier.Private)]
	public const int BloodmoonBlinkWarningHours = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EventHit = "HIT";

	[PublicizedFrom(EAccessModifier.Private)]
	public const float HitEventDuration = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string EventDukes = "DUKES";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string DukesStringValueKey = "dukesstring";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string DukesLabelLocKey = "gamesenseDukesLabel";

	[PublicizedFrom(EAccessModifier.Private)]
	public WeakReference<EntityPlayerLocal> entityPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastTimePercent = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int previousSlotIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int previousAmmoPercentage = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int previousDurability = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int previousStealth = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int bloodMoonDay;

	[PublicizedFrom(EAccessModifier.Private)]
	public int bloodMoonWarning;

	[TupleElementNames(new string[] { "duskHour", "dawnHour" })]
	[PublicizedFrom(EAccessModifier.Private)]
	public ValueTuple<int, int> duskDawnTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastBloodMoonValue = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameSenseManager.EDirection45 lastDirection = GameSenseManager.EDirection45.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public float stopHitEventTime = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine stopHitEventCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dukesNextTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue dukesItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EDirection45
	{
		N,
		NE,
		E,
		SE,
		S,
		SW,
		W,
		NW,
		None
	}
}
