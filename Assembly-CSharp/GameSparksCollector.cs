using System;
using System.Collections.Generic;

public static class GameSparksCollector
{
	public static bool CollectGamePlayData { get; set; }

	[PublicizedFrom(EAccessModifier.Private)]
	public static GSRequestData GetObject(string _keyString, GSRequestData _collection)
	{
		object obj = GameSparksCollector.lockObject;
		GSRequestData gsrequestData2;
		lock (obj)
		{
			GSRequestData gsrequestData = _collection.GetGSData(_keyString);
			if (gsrequestData != null)
			{
				gsrequestData2 = gsrequestData;
			}
			else
			{
				gsrequestData = new GSRequestData();
				_collection.Add(_keyString, gsrequestData);
				gsrequestData2 = gsrequestData;
			}
		}
		return gsrequestData2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static ValueTuple<GSRequestData, string> GetRequestDataAndKey(GameSparksCollector.GSDataCollection _collectionType, GameSparksCollector.GSDataKey _key, string _subKey = null)
	{
		string text = _key.ToStringCached<GameSparksCollector.GSDataKey>();
		GSRequestData gsrequestData = ((_collectionType == GameSparksCollector.GSDataCollection.SessionUpdates) ? GameSparksCollector.dataUpdates : GameSparksCollector.dataSessionTotal);
		if (_subKey == null)
		{
			return new ValueTuple<GSRequestData, string>(gsrequestData, text);
		}
		return new ValueTuple<GSRequestData, string>(GameSparksCollector.GetObject(text, gsrequestData), _subKey);
	}

	public static void SetValue(GameSparksCollector.GSDataKey _key, string _subKey, int _value, bool _isGamePlay = true, GameSparksCollector.GSDataCollection _collectionType = GameSparksCollector.GSDataCollection.SessionUpdates)
	{
		if (!_isGamePlay || GameSparksCollector.CollectGamePlayData)
		{
			object obj = GameSparksCollector.lockObject;
			lock (obj)
			{
				ValueTuple<GSRequestData, string> requestDataAndKey = GameSparksCollector.GetRequestDataAndKey(_collectionType, _key, _subKey);
				GSRequestData item = requestDataAndKey.Item1;
				string item2 = requestDataAndKey.Item2;
				item.AddNumber(item2, _value);
			}
		}
	}

	public static void SetValue(GameSparksCollector.GSDataKey _key, string _subKey, string _value, bool _isGamePlay = true, GameSparksCollector.GSDataCollection _collectionType = GameSparksCollector.GSDataCollection.SessionUpdates)
	{
		if (!_isGamePlay || GameSparksCollector.CollectGamePlayData)
		{
			object obj = GameSparksCollector.lockObject;
			lock (obj)
			{
				ValueTuple<GSRequestData, string> requestDataAndKey = GameSparksCollector.GetRequestDataAndKey(_collectionType, _key, _subKey);
				GSRequestData item = requestDataAndKey.Item1;
				string item2 = requestDataAndKey.Item2;
				item.AddString(item2, _value);
			}
		}
	}

	public static void IncrementCounter(GameSparksCollector.GSDataKey _key, string _subKey, int _increment, bool _isGamePlay = true, GameSparksCollector.GSDataCollection _collectionType = GameSparksCollector.GSDataCollection.SessionUpdates)
	{
		if (!_isGamePlay || GameSparksCollector.CollectGamePlayData)
		{
			object obj = GameSparksCollector.lockObject;
			lock (obj)
			{
				ValueTuple<GSRequestData, string> requestDataAndKey = GameSparksCollector.GetRequestDataAndKey(_collectionType, _key, _subKey);
				GSRequestData item = requestDataAndKey.Item1;
				string item2 = requestDataAndKey.Item2;
				int num = item.GetInt(item2).GetValueOrDefault();
				num += _increment;
				item.AddNumber(item2, num);
			}
		}
	}

	public static void IncrementCounter(GameSparksCollector.GSDataKey _key, string _subKey, float _increment, bool _isGamePlay = true, GameSparksCollector.GSDataCollection _collectionType = GameSparksCollector.GSDataCollection.SessionUpdates)
	{
		if (!_isGamePlay || GameSparksCollector.CollectGamePlayData)
		{
			object obj = GameSparksCollector.lockObject;
			lock (obj)
			{
				ValueTuple<GSRequestData, string> requestDataAndKey = GameSparksCollector.GetRequestDataAndKey(_collectionType, _key, _subKey);
				GSRequestData item = requestDataAndKey.Item1;
				string item2 = requestDataAndKey.Item2;
				float num = item.GetFloat(item2).GetValueOrDefault();
				num += _increment;
				item.AddNumber(item2, (double)num);
			}
		}
	}

	public static void SetMax(GameSparksCollector.GSDataKey _key, string _subKey, int _currentValue, bool _isGamePlay = true, GameSparksCollector.GSDataCollection _collectionType = GameSparksCollector.GSDataCollection.SessionUpdates)
	{
		if (!_isGamePlay || GameSparksCollector.CollectGamePlayData)
		{
			object obj = GameSparksCollector.lockObject;
			lock (obj)
			{
				ValueTuple<GSRequestData, string> requestDataAndKey = GameSparksCollector.GetRequestDataAndKey(_collectionType, _key, _subKey);
				GSRequestData item = requestDataAndKey.Item1;
				string item2 = requestDataAndKey.Item2;
				int num = item.GetInt(item2) ?? int.MinValue;
				num = Math.Max(num, _currentValue);
				item.AddNumber(item2, num);
			}
		}
	}

	public static GSRequestData GetSessionUpdateDataAndReset()
	{
		object obj = GameSparksCollector.lockObject;
		GSRequestData gsrequestData2;
		lock (obj)
		{
			GSRequestData gsrequestData = GameSparksCollector.dataUpdates;
			GameSparksCollector.dataUpdates = new GSRequestData();
			gsrequestData2 = gsrequestData;
		}
		return gsrequestData2;
	}

	public static GSRequestData GetSessionTotalData(bool _reset)
	{
		if (!_reset)
		{
			return GameSparksCollector.dataSessionTotal;
		}
		object obj = GameSparksCollector.lockObject;
		GSRequestData gsrequestData2;
		lock (obj)
		{
			GSRequestData gsrequestData = GameSparksCollector.dataSessionTotal;
			GameSparksCollector.dataSessionTotal = new GSRequestData();
			gsrequestData2 = gsrequestData;
		}
		return gsrequestData2;
	}

	public static void PlayerLevelUp(EntityPlayerLocal _localPlayer, int _level)
	{
		if (_level == 15)
		{
			GameSparksCollector.SendSaveTimePlayed(GameSparksCollector.GSDataKey.HoursPlayedAtLevel15, _localPlayer);
			GameSparksCollector.SendSkillStats(GameSparksCollector.GSDataKey.SkillsPurchasedAtLevel15, _localPlayer);
			return;
		}
		if (_level == 30)
		{
			GameSparksCollector.SendSaveTimePlayed(GameSparksCollector.GSDataKey.HoursPlayedAtLevel30, _localPlayer);
			GameSparksCollector.SendSkillStats(GameSparksCollector.GSDataKey.SkillsPurchasedAtLevel30, _localPlayer);
			return;
		}
		if (_level != 50)
		{
			return;
		}
		GameSparksCollector.SendSaveTimePlayed(GameSparksCollector.GSDataKey.HoursPlayedAtLevel50, _localPlayer);
		GameSparksCollector.SendSkillStats(GameSparksCollector.GSDataKey.SkillsPurchasedAtLevel50, _localPlayer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SendSkillStats(GameSparksCollector.GSDataKey _key, EntityPlayerLocal _localPlayer)
	{
		foreach (KeyValuePair<int, ProgressionValue> keyValuePair in _localPlayer.Progression.GetDict())
		{
			ProgressionClass progressionClass = keyValuePair.Value.ProgressionClass;
			for (int i = progressionClass.MinLevel + 1; i <= keyValuePair.Value.Level; i++)
			{
				string text;
				if (progressionClass.Parent == null || progressionClass.Parent == progressionClass)
				{
					text = string.Format("{0}_{1}", progressionClass.Name, i);
				}
				else
				{
					ProgressionClass progressionClass2 = progressionClass.Parent;
					while (progressionClass2.Parent != null && progressionClass2.Parent != progressionClass2)
					{
						progressionClass2 = progressionClass2.Parent;
					}
					text = string.Format("{0}_{1}_{2}", progressionClass2.Name, progressionClass.Name, i);
				}
				GameSparksCollector.SetValue(_key, text, 1, true, GameSparksCollector.GSDataCollection.SessionUpdates);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SendSaveTimePlayed(GameSparksCollector.GSDataKey _key, EntityPlayerLocal _localPlayer)
	{
		int num = (int)(_localPlayer.totalTimePlayed / 60f);
		if (num > 0)
		{
			GameSparksCollector.SetValue(_key, null, num, true, GameSparksCollector.GSDataCollection.SessionUpdates);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object lockObject = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static GSRequestData dataUpdates = new GSRequestData();

	[PublicizedFrom(EAccessModifier.Private)]
	public static GSRequestData dataSessionTotal = new GSRequestData();

	public enum GSDataKey
	{
		HoursPlayedAtLevel15,
		HoursPlayedAtLevel30,
		HoursPlayedAtLevel50,
		SkillsPurchasedAtLevel15,
		SkillsPurchasedAtLevel30,
		SkillsPurchasedAtLevel50,
		PlayerLevelAtHour,
		XpEarnedBy,
		PlayerDeathCauses,
		ZombiesKilledBy,
		CraftedItems,
		TraderItemsBought,
		VendingItemsBought,
		TraderMoneySpentOn,
		VendingMoneySpentOn,
		TotalMoneySpentOn,
		PeakConcurrentClients,
		PeakConcurrentPlayers,
		QuestTraderToTraderDistance,
		QuestAcceptedDistance,
		QuestOfferedDistance,
		QuestStarterTraderDistance,
		PlayerProfileIsCustom,
		PlayerArchetypeName,
		UsedTwitchIntegration
	}

	public enum GSDataCollection
	{
		SessionTotal,
		SessionUpdates
	}
}
