using System;
using System.Collections.Generic;

namespace Twitch
{
	public class TwitchActionManager
	{
		public static TwitchActionManager Current
		{
			get
			{
				if (TwitchActionManager.instance == null)
				{
					TwitchActionManager.instance = new TwitchActionManager();
				}
				return TwitchActionManager.instance;
			}
		}

		public static bool HasInstance
		{
			get
			{
				return TwitchActionManager.instance != null;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public TwitchActionManager()
		{
		}

		public void Cleanup()
		{
			if (TwitchActionManager.TwitchActions != null)
			{
				TwitchActionManager.TwitchActions.Clear();
			}
			if (TwitchActionManager.TwitchVotes != null)
			{
				TwitchActionManager.TwitchVotes.Clear();
			}
			this.CategoryList.Clear();
			if (TwitchManager.HasInstance)
			{
				TwitchManager.Current.CleanupData();
			}
		}

		public void AddAction(TwitchAction action)
		{
			if (!TwitchActionManager.TwitchActions.ContainsKey(action.Name))
			{
				TwitchActionManager.TwitchActions.Add(action.Name, action);
			}
		}

		public void AddVoteClass(TwitchVote vote)
		{
			TwitchActionManager.TwitchVotes.Add(vote.VoteName, vote);
		}

		public int GetCategoryIndex(string categoryName)
		{
			for (int i = 0; i < this.CategoryList.Count; i++)
			{
				if (categoryName.StartsWith(this.CategoryList[i].Name))
				{
					return i;
				}
			}
			return 9999;
		}

		public TwitchActionManager.ActionCategory GetCategory(string categoryName)
		{
			for (int i = 0; i < this.CategoryList.Count; i++)
			{
				if (this.CategoryList[i].Name == categoryName)
				{
					return this.CategoryList[i];
				}
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static TwitchActionManager instance = null;

		public List<TwitchActionManager.ActionCategory> CategoryList = new List<TwitchActionManager.ActionCategory>();

		public static Dictionary<string, TwitchAction> TwitchActions = new Dictionary<string, TwitchAction>();

		public static Dictionary<string, TwitchVote> TwitchVotes = new Dictionary<string, TwitchVote>();

		public class ActionCategory
		{
			public string Name;

			public string DisplayName;

			public string Icon;

			public bool ShowInCommandList = true;

			public bool AlwaysShowInMenu;
		}
	}
}
