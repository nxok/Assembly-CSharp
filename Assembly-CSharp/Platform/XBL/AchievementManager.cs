using System;
using System.Collections.Generic;
using Unity.XGamingRuntime;
using UnityEngine;

namespace Platform.XBL
{
	public class AchievementManager : IAchievementManager
	{
		[PublicizedFrom(EAccessModifier.Private)]
		static AchievementManager()
		{
			string launchArgument = GameUtils.GetLaunchArgument("debugachievements");
			if (launchArgument != null)
			{
				if (launchArgument == "verbose")
				{
					AchievementManager.debug = AchievementManager.EDebugLevel.Verbose;
					return;
				}
				AchievementManager.debug = AchievementManager.EDebugLevel.Normal;
			}
		}

		public void Init(IPlatform _owner)
		{
			_owner.User.UserLoggedIn += delegate(IPlatform _sender)
			{
				this.xblUser = (User)_owner.User;
			};
		}

		public void ShowAchievementsUi()
		{
			SDK.XGameUiShowAchievementsAsync(this.xblUser.UserHandle, 1745806870U, delegate(int _hresult)
			{
				XblHelpers.Succeeded(_hresult, "Open achievements UI", true, false);
			});
		}

		public bool IsAchievementStatSupported(EnumAchievementDataStat _stat)
		{
			return _stat != EnumAchievementDataStat.HighestPlayerLevel;
		}

		public void SetAchievementStat(EnumAchievementDataStat _stat, int _value)
		{
			if (!_stat.IsSupported())
			{
				return;
			}
			if (AchievementUtils.IsCreativeModeActive())
			{
				if (AchievementManager.debug != AchievementManager.EDebugLevel.Off && Time.unscaledTime - this.lastAchievementsDisabledWarningTime > 30f)
				{
					this.lastAchievementsDisabledWarningTime = Time.unscaledTime;
					Log.Warning("[XBL] Achievements disabled due to creative mode, creative menu or debug menu enabled");
				}
				return;
			}
			if (AchievementData.GetStatType(_stat) != EnumStatType.Int)
			{
				Log.Warning("AchievementManager.SetAchievementStat, int given for float type stat {0}", new object[] { _stat.ToStringCached<EnumAchievementDataStat>() });
				return;
			}
			AchievementManager.StatCacheEntry statCacheEntry;
			if (AchievementData.GetUpdateType(_stat) != AchievementData.EnumUpdateType.Sum && this.sentStatsCache.TryGetValue(_stat, out statCacheEntry) && statCacheEntry.iValue == _value)
			{
				if (AchievementManager.debug == AchievementManager.EDebugLevel.Verbose && Time.unscaledTime - statCacheEntry.lastSendTime > 30f)
				{
					this.sentStatsCache[_stat] = new AchievementManager.StatCacheEntry(_value, 0f, Time.unscaledTime);
					Log.Warning(string.Format("[XBL] Not sending achievement {0}, already sent with value {1}", _stat.ToStringCached<EnumAchievementDataStat>(), _value));
				}
				return;
			}
			if (XblHelpers.Succeeded(SDK.XBL.XblEventsWriteInGameEvent(this.xblUser.XblContextHandle, _stat.ToStringCached<EnumAchievementDataStat>(), string.Format("{{\"Value\":{0}}}", _value), "{}"), "Send int stat event '" + _stat.ToStringCached<EnumAchievementDataStat>() + "'", true, false))
			{
				this.sentStatsCache[_stat] = new AchievementManager.StatCacheEntry(_value, 0f, Time.unscaledTime);
				if (AchievementManager.debug == AchievementManager.EDebugLevel.Verbose)
				{
					Log.Out(string.Format("[XBL] Sent achievement update: {0} = {1}", _stat.ToStringCached<EnumAchievementDataStat>(), _value));
				}
			}
		}

		public void SetAchievementStat(EnumAchievementDataStat _stat, float _value)
		{
			if (!_stat.IsSupported())
			{
				return;
			}
			if (AchievementUtils.IsCreativeModeActive())
			{
				if (AchievementManager.debug != AchievementManager.EDebugLevel.Off && Time.unscaledTime - this.lastAchievementsDisabledWarningTime > 30f)
				{
					this.lastAchievementsDisabledWarningTime = Time.unscaledTime;
					Log.Warning("[XBL] Achievements disabled due to creative mode, creative menu or debug menu enabled");
				}
				return;
			}
			if (AchievementData.GetStatType(_stat) != EnumStatType.Float)
			{
				Log.Warning("AchievementManager.SetAchievementStat, float given for int type stat {0}", new object[] { _stat.ToStringCached<EnumAchievementDataStat>() });
				return;
			}
			AchievementManager.StatCacheEntry statCacheEntry;
			if (AchievementData.GetUpdateType(_stat) != AchievementData.EnumUpdateType.Sum && this.sentStatsCache.TryGetValue(_stat, out statCacheEntry) && statCacheEntry.fValue == _value)
			{
				if (AchievementManager.debug == AchievementManager.EDebugLevel.Verbose && Time.unscaledTime - statCacheEntry.lastSendTime > 30f)
				{
					this.sentStatsCache[_stat] = new AchievementManager.StatCacheEntry(0, _value, Time.unscaledTime);
					Log.Warning("[XBL] Not sending achievement " + _stat.ToStringCached<EnumAchievementDataStat>() + ", already sent with value " + _value.ToCultureInvariantString());
				}
				return;
			}
			if (XblHelpers.Succeeded(SDK.XBL.XblEventsWriteInGameEvent(this.xblUser.XblContextHandle, _stat.ToStringCached<EnumAchievementDataStat>(), "{\"Value\":" + _value.ToCultureInvariantString() + "}", "{}"), "Send float stat event '" + _stat.ToStringCached<EnumAchievementDataStat>() + "'", true, false))
			{
				this.sentStatsCache[_stat] = new AchievementManager.StatCacheEntry(0, _value, Time.unscaledTime);
				if (AchievementManager.debug == AchievementManager.EDebugLevel.Verbose)
				{
					Log.Out(string.Format("[XBL] Sent achievement update: {0} = {1}", _stat.ToStringCached<EnumAchievementDataStat>(), _value));
				}
			}
		}

		public void ResetStats(bool _andAchievements)
		{
		}

		public void UnlockAllAchievements()
		{
			for (int i = 0; i < 19; i++)
			{
				EnumAchievementDataStat enumAchievementDataStat = (EnumAchievementDataStat)i;
				if (enumAchievementDataStat.IsSupported())
				{
					List<AchievementData.AchievementInfo> achievementInfos = AchievementData.GetAchievementInfos(enumAchievementDataStat);
					AchievementData.AchievementInfo achievementInfo = achievementInfos[achievementInfos.Count - 1];
					switch (AchievementData.GetStatType(enumAchievementDataStat))
					{
					case EnumStatType.Int:
						this.SetAchievementStat(enumAchievementDataStat, Convert.ToInt32(achievementInfo.triggerPoint));
						break;
					case EnumStatType.Float:
						this.SetAchievementStat(enumAchievementDataStat, Convert.ToSingle(achievementInfo.triggerPoint));
						break;
					}
				}
			}
		}

		public void Destroy()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int suppressRepeatedNotSentWarningsTime = 30;

		[PublicizedFrom(EAccessModifier.Private)]
		public User xblUser;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly AchievementManager.EDebugLevel debug = AchievementManager.EDebugLevel.Off;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<EnumAchievementDataStat, AchievementManager.StatCacheEntry> sentStatsCache = new EnumDictionary<EnumAchievementDataStat, AchievementManager.StatCacheEntry>();

		[PublicizedFrom(EAccessModifier.Private)]
		public float lastAchievementsDisabledWarningTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public enum EDebugLevel
		{
			Off,
			Normal,
			Verbose
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly struct StatCacheEntry
		{
			public StatCacheEntry(int _iValue, float _fValue, float _lastSendTime)
			{
				this.iValue = _iValue;
				this.fValue = _fValue;
				this.lastSendTime = _lastSendTime;
			}

			public readonly int iValue;

			public readonly float fValue;

			public readonly float lastSendTime;
		}
	}
}
