using System;
using System.Collections.Generic;
using Steamworks;

namespace Platform.Steam
{
	public class AchievementManager : IAchievementManager
	{
		public AchievementManager()
		{
			this.m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(new Callback<UserStatsReceived_t>.DispatchDelegate(this.UserStatsReceived_Callback));
			this.m_UserStatsStored = Callback<UserStatsStored_t>.Create(new Callback<UserStatsStored_t>.DispatchDelegate(this.UserStatsStored_Callback));
			this.m_UserAchievementStored_t = Callback<UserAchievementStored_t>.Create(new Callback<UserAchievementStored_t>.DispatchDelegate(this.UserAchievementStored_Callback));
		}

		public void Init(IPlatform _owner)
		{
			_owner.User.UserLoggedIn += delegate(IPlatform _sender)
			{
				SteamUserStats.RequestCurrentStats();
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void UserStatsReceived_Callback(UserStatsReceived_t _result)
		{
			if (_result.m_nGameID != 251570UL)
			{
				return;
			}
			if (_result.m_eResult != 1)
			{
				Log.Error("AchievementManager: RequestStats failed: {0}", new object[] { _result.m_eResult.ToStringCached<EResult>() });
				return;
			}
			if (this.steamStatsCache.Count > 0)
			{
				return;
			}
			Log.Out("AchievementManager: Received stats and achievements from Steam");
			for (int i = 0; i < 19; i++)
			{
				EnumAchievementDataStat enumAchievementDataStat = (EnumAchievementDataStat)i;
				if (enumAchievementDataStat.IsSupported())
				{
					switch (AchievementData.GetStatType(enumAchievementDataStat))
					{
					case EnumStatType.Int:
					{
						int num;
						if (SteamUserStats.GetStat(enumAchievementDataStat.ToStringCached<EnumAchievementDataStat>(), ref num))
						{
							this.steamStatsCache.Add(enumAchievementDataStat, new AchievementManager.StatCacheEntry(enumAchievementDataStat.ToStringCached<EnumAchievementDataStat>(), num, 0f));
						}
						break;
					}
					case EnumStatType.Float:
					{
						float num2;
						if (SteamUserStats.GetStat(enumAchievementDataStat.ToStringCached<EnumAchievementDataStat>(), ref num2))
						{
							this.steamStatsCache.Add(enumAchievementDataStat, new AchievementManager.StatCacheEntry(enumAchievementDataStat.ToStringCached<EnumAchievementDataStat>(), 0, num2));
						}
						break;
					}
					}
				}
			}
			for (int j = 0; j < 48; j++)
			{
				EnumAchievementManagerAchievement enumAchievementManagerAchievement = (EnumAchievementManagerAchievement)j;
				bool flag;
				if (enumAchievementManagerAchievement.IsSupported() && SteamUserStats.GetAchievement(enumAchievementManagerAchievement.ToStringCached<EnumAchievementManagerAchievement>(), ref flag))
				{
					this.steamAchievementsCache.Add(enumAchievementManagerAchievement, new AchievementManager.AchievementCacheEntry(enumAchievementManagerAchievement.ToStringCached<EnumAchievementManagerAchievement>(), flag));
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void UserStatsStored_Callback(UserStatsStored_t _result)
		{
			Log.Out("AchievementManager.UserStatsStored_Callback, result={0}", new object[] { _result.m_eResult.ToStringCached<EResult>() });
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void UserAchievementStored_Callback(UserAchievementStored_t _result)
		{
			Log.Out("AchievementManager.UserAchievementStored_Callback, name={0}, cur={1}, max={2}", new object[] { _result.m_rgchAchievementName, _result.m_nCurProgress, _result.m_nMaxProgress });
		}

		public void ShowAchievementsUi()
		{
			Log.Out("AchievementManager.ShowAchievementsUI");
			SteamFriends.ActivateGameOverlay("Achievements");
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SendAchievementEvent(EnumAchievementManagerAchievement _achievement)
		{
			if (AchievementUtils.IsCreativeModeActive())
			{
				return;
			}
			Log.Out("AchievementManager.SendAchievementEvent (" + _achievement.ToStringCached<EnumAchievementManagerAchievement>() + ")");
			AchievementManager.AchievementCacheEntry achievementCacheEntry;
			if (this.steamAchievementsCache.TryGetValue(_achievement, out achievementCacheEntry))
			{
				SteamUserStats.SetAchievement(achievementCacheEntry.name);
				this.steamAchievementsCache[_achievement] = new AchievementManager.AchievementCacheEntry(achievementCacheEntry.name, true);
				SteamUserStats.StoreStats();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetAchievementStatValueFloat(EnumAchievementDataStat _stat, float _value)
		{
			AchievementManager.StatCacheEntry statCacheEntry;
			if (!this.steamStatsCache.TryGetValue(_stat, out statCacheEntry) || AchievementData.GetStatType(_stat) != EnumStatType.Float)
			{
				return;
			}
			this.steamStatsCache[_stat] = new AchievementManager.StatCacheEntry(statCacheEntry.name, 0, _value);
			SteamUserStats.SetStat(statCacheEntry.name, _value);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetAchievementStatValueInt(EnumAchievementDataStat _stat, int _value)
		{
			AchievementManager.StatCacheEntry statCacheEntry;
			if (!this.steamStatsCache.TryGetValue(_stat, out statCacheEntry) || AchievementData.GetStatType(_stat) != EnumStatType.Int)
			{
				return;
			}
			this.steamStatsCache[_stat] = new AchievementManager.StatCacheEntry(statCacheEntry.name, _value, 0f);
			SteamUserStats.SetStat(statCacheEntry.name, _value);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float GetAchievementStatValueFloat(EnumAchievementDataStat _stat)
		{
			AchievementManager.StatCacheEntry statCacheEntry;
			if (this.steamStatsCache.TryGetValue(_stat, out statCacheEntry) && AchievementData.GetStatType(_stat) == EnumStatType.Float)
			{
				return statCacheEntry.fValue;
			}
			return 0f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int GetAchievementStatValueInt(EnumAchievementDataStat _stat)
		{
			AchievementManager.StatCacheEntry statCacheEntry;
			if (this.steamStatsCache.TryGetValue(_stat, out statCacheEntry) && AchievementData.GetStatType(_stat) == EnumStatType.Int)
			{
				return statCacheEntry.iValue;
			}
			return 0;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool IsAchievementLocked(EnumAchievementManagerAchievement _achievement)
		{
			AchievementManager.AchievementCacheEntry achievementCacheEntry;
			return this.steamAchievementsCache.TryGetValue(_achievement, out achievementCacheEntry) && achievementCacheEntry.locked;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void UpdateAchievement(EnumAchievementDataStat _stat, float _newValue)
		{
			List<AchievementData.AchievementInfo> achievementInfos = AchievementData.GetAchievementInfos(_stat);
			for (int i = 0; i < achievementInfos.Count; i++)
			{
				EnumAchievementManagerAchievement achievement = achievementInfos[i].achievement;
				if (_newValue >= Convert.ToSingle(achievementInfos[i].triggerPoint) && !this.IsAchievementLocked(achievement))
				{
					this.SendAchievementEvent(achievement);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void UpdateAchievement(EnumAchievementDataStat _stat, int _newValue)
		{
			List<AchievementData.AchievementInfo> achievementInfos = AchievementData.GetAchievementInfos(_stat);
			for (int i = 0; i < achievementInfos.Count; i++)
			{
				EnumAchievementManagerAchievement achievement = achievementInfos[i].achievement;
				if (_newValue >= Convert.ToInt32(achievementInfos[i].triggerPoint) && !this.IsAchievementLocked(achievement))
				{
					this.SendAchievementEvent(achievement);
				}
			}
		}

		public bool IsAchievementStatSupported(EnumAchievementDataStat _stat)
		{
			return _stat != EnumAchievementDataStat.HighestGamestage;
		}

		public void SetAchievementStat(EnumAchievementDataStat _stat, int _value)
		{
			if (!_stat.IsSupported())
			{
				return;
			}
			if (AchievementUtils.IsCreativeModeActive())
			{
				return;
			}
			AchievementData.EnumUpdateType updateType = AchievementData.GetUpdateType(_stat);
			EnumStatType statType = AchievementData.GetStatType(_stat);
			if (!this.steamStatsCache.ContainsKey(_stat))
			{
				return;
			}
			if (statType != EnumStatType.Int)
			{
				Log.Warning("AchievementManager.SetAchievementStat, int given for float type stat {0}", new object[] { _stat.ToStringCached<EnumAchievementDataStat>() });
				return;
			}
			int achievementStatValueInt = this.GetAchievementStatValueInt(_stat);
			int num;
			switch (updateType)
			{
			case AchievementData.EnumUpdateType.Sum:
				num = achievementStatValueInt + _value;
				break;
			case AchievementData.EnumUpdateType.Replace:
				num = _value;
				break;
			case AchievementData.EnumUpdateType.Max:
				num = Math.Max(achievementStatValueInt, _value);
				break;
			default:
				num = 0;
				break;
			}
			int num2 = num;
			if (achievementStatValueInt != num2)
			{
				this.SetAchievementStatValueInt(_stat, num2);
				this.UpdateAchievement(_stat, num2);
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
				return;
			}
			AchievementData.EnumUpdateType updateType = AchievementData.GetUpdateType(_stat);
			EnumStatType statType = AchievementData.GetStatType(_stat);
			if (!this.steamStatsCache.ContainsKey(_stat))
			{
				return;
			}
			if (statType != EnumStatType.Float)
			{
				Log.Warning("AchievementManager.SetAchievementStat, float given for int type stat {0}", new object[] { _stat.ToStringCached<EnumAchievementDataStat>() });
				return;
			}
			float achievementStatValueFloat = this.GetAchievementStatValueFloat(_stat);
			float num;
			switch (updateType)
			{
			case AchievementData.EnumUpdateType.Sum:
				num = achievementStatValueFloat + _value;
				break;
			case AchievementData.EnumUpdateType.Replace:
				num = _value;
				break;
			case AchievementData.EnumUpdateType.Max:
				num = Math.Max(achievementStatValueFloat, _value);
				break;
			default:
				num = achievementStatValueFloat;
				break;
			}
			float num2 = num;
			if (achievementStatValueFloat != num2)
			{
				this.SetAchievementStatValueFloat(_stat, num2);
				this.UpdateAchievement(_stat, num2);
			}
		}

		public void ResetStats(bool _andAchievements)
		{
			SteamUserStats.ResetAllStats(_andAchievements);
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
			Log.Out("AchievementManager.Cleanup");
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Callback<UserStatsReceived_t> m_UserStatsReceived;

		[PublicizedFrom(EAccessModifier.Private)]
		public Callback<UserStatsStored_t> m_UserStatsStored;

		[PublicizedFrom(EAccessModifier.Private)]
		public Callback<UserAchievementStored_t> m_UserAchievementStored_t;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<EnumAchievementDataStat, AchievementManager.StatCacheEntry> steamStatsCache = new EnumDictionary<EnumAchievementDataStat, AchievementManager.StatCacheEntry>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<EnumAchievementManagerAchievement, AchievementManager.AchievementCacheEntry> steamAchievementsCache = new EnumDictionary<EnumAchievementManagerAchievement, AchievementManager.AchievementCacheEntry>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly struct StatCacheEntry
		{
			public StatCacheEntry(string _name, int _iValue, float _fValue)
			{
				this.name = _name;
				this.iValue = _iValue;
				this.fValue = _fValue;
			}

			public readonly string name;

			public readonly int iValue;

			public readonly float fValue;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly struct AchievementCacheEntry
		{
			public AchievementCacheEntry(string _name, bool _locked)
			{
				this.name = _name;
				this.locked = _locked;
			}

			public readonly string name;

			public readonly bool locked;
		}
	}
}
