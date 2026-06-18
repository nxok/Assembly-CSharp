using System;
using System.Collections.Generic;

namespace Twitch
{
	public class UpdateMessage
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static string DifficultyValueLocalized()
		{
			int num = GameStats.GetInt(EnumGameStats.GameDifficulty) + 1;
			return Localization.Get(string.Format("goDifficulty{0}", num) + ((num == 2) ? "_nodefault" : ""), false, null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool IsGameModded()
		{
			GameServerInfo gameServerInfo = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
			return gameServerInfo != null && gameServerInfo.GetValue(GameInfoBool.ModdedConfig);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string ModdedValueLocalized()
		{
			GameServerInfo gameServerInfo = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo : SingletonMonoBehaviour<ConnectionManager>.Instance.LastGameServerInfo);
			if (gameServerInfo == null)
			{
				return "--";
			}
			if (!gameServerInfo.GetValue(GameInfoBool.ModdedConfig))
			{
				return Localization.Get("xuiComboYesNoOff", false, null);
			}
			return Localization.Get("xuiComboYesNoOn", false, null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string GetLocalizedPPRateValue()
		{
			if (TwitchManager.Current.ViewerData.PointRate == 1f)
			{
				return Localization.Get("xuiTwitchPointGenerationStandard", false, null);
			}
			if (TwitchManager.Current.ViewerData.PointRate == 2f)
			{
				return Localization.Get("xuiTwitchPointGenerationDouble", false, null);
			}
			if (TwitchManager.Current.ViewerData.PointRate == 3f)
			{
				return Localization.Get("xuiTwitchPointGenerationTriple", false, null);
			}
			if (TwitchManager.Current.ViewerData.PointRate == 0f)
			{
				return Localization.Get("goDisabled", false, null);
			}
			return Localization.Get("xuiTwitchPointGenerationStandard", false, null);
		}

		public UpdateMessage()
		{
			TwitchLeaderboardStats.StatEntry topKillerViewer = TwitchManager.LeaderboardStats.TopKillerViewer;
			this.TopKillerValue = ((topKillerViewer != null) ? topKillerViewer.Name : null) ?? "--";
			TwitchLeaderboardStats.StatEntry topGoodViewer = TwitchManager.LeaderboardStats.TopGoodViewer;
			this.TopGoodValue = ((topGoodViewer != null) ? topGoodViewer.Name : null) ?? "--";
			TwitchLeaderboardStats.StatEntry topBadViewer = TwitchManager.LeaderboardStats.TopBadViewer;
			this.TopBadValue = ((topBadViewer != null) ? topBadViewer.Name : null) ?? "--";
			TwitchLeaderboardStats.StatEntry currentGoodViewer = TwitchManager.LeaderboardStats.CurrentGoodViewer;
			this.BestHelperValue = ((currentGoodViewer != null) ? currentGoodViewer.Name : null) ?? "--";
			this.TotalGoodActionsValue = TwitchManager.LeaderboardStats.TotalGood.ToString();
			this.TotalBadActionsValue = TwitchManager.LeaderboardStats.TotalBad.ToString();
			this.LargestPimpPotValue = TwitchManager.LeaderboardStats.LargestPimpPot.ToString();
			this.DifficultyValue = UpdateMessage.DifficultyValueLocalized();
			this.DayCycleValue = string.Format(Localization.Get("goMinutes", false, null), GamePrefs.GetInt(EnumGamePrefs.DayNightLength));
			this.PPRateValue = UpdateMessage.GetLocalizedPPRateValue();
			this.ModdedValue = UpdateMessage.ModdedValueLocalized();
			base..ctor();
		}

		public string updateSignature;

		public string status;

		public int[] actionCooldowns;

		public Dictionary<string, int> bitBalances;

		public Dictionary<string, bool> hasChatted;

		public string ActionPresetKey = TwitchManager.Current.CurrentActionPreset.Name;

		public string VotePresetKey = TwitchManager.Current.CurrentVotePreset.Name;

		public string EventPresetKey = TwitchManager.Current.CurrentEventPreset.Name;

		public int Difficulty = GameStats.GetInt(EnumGameStats.GameDifficulty) + 1;

		public int DayMinutes = GamePrefs.GetInt(EnumGamePrefs.DayNightLength);

		public int PPRate = (int)TwitchManager.Current.ViewerData.PointRate;

		public bool IsModded = UpdateMessage.IsGameModded();

		public int GoodRewardTime = TwitchManager.LeaderboardStats.GoodRewardTime;

		public string ActionPresetValue = TwitchManager.Current.CurrentActionPreset.Title;

		public string VotePresetValue = TwitchManager.Current.CurrentVotePreset.Title;

		public string EventPresetValue = TwitchManager.Current.CurrentEventPreset.Title;

		public string TopKillerValue;

		public string TopGoodValue;

		public string TopBadValue;

		public string BestHelperValue;

		public string TotalGoodActionsValue;

		public string TotalBadActionsValue;

		public string LargestPimpPotValue;

		public string DifficultyValue;

		public string DayCycleValue;

		public string PPRateValue;

		public string ModdedValue;
	}
}
