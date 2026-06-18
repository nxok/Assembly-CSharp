using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using UniLinq;

namespace Challenges
{
	public class ChallengeGroup
	{
		public ChallengeGroup(string name)
		{
			this.Name = name;
		}

		public static ChallengeGroup NewClass(string id)
		{
			if (ChallengeGroup.s_ChallengeGroups.ContainsKey(id))
			{
				return null;
			}
			ChallengeGroup challengeGroup = new ChallengeGroup(id.ToLower());
			ChallengeGroup.s_ChallengeGroups[id] = challengeGroup;
			return challengeGroup;
		}

		public static ChallengeGroup GetGroup(string id)
		{
			if (!ChallengeGroup.s_ChallengeGroups.ContainsKey(id))
			{
				return null;
			}
			return ChallengeGroup.s_ChallengeGroups[id];
		}

		public void AddChallengeCount(string tag, int count)
		{
			if (this.ChallengeCounts == null)
			{
				this.ChallengeCounts = new List<ChallengeGroup.ChallengeCount>();
			}
			this.ChallengeCounts.Add(new ChallengeGroup.ChallengeCount
			{
				Tags = FastTags<TagGroup.Global>.Parse(tag),
				Count = count
			});
		}

		public void AddChallenge(ChallengeClass challenge)
		{
			this.ChallengeClasses.Add(challenge);
		}

		public void ParseElement(XElement e)
		{
			if (e.HasAttribute("title_key"))
			{
				this.Title = Localization.Get(e.GetAttribute("title_key"), false, null);
			}
			else if (e.HasAttribute("title"))
			{
				this.Title = e.GetAttribute("title");
			}
			else
			{
				this.Title = this.Name;
			}
			if (e.HasAttribute("category"))
			{
				this.Category = e.GetAttribute("category");
			}
			if (e.HasAttribute("reward_event"))
			{
				this.RewardEvent = e.GetAttribute("reward_event");
			}
			if (e.HasAttribute("reward_text_key"))
			{
				this.RewardText = Localization.Get(e.GetAttribute("reward_text_key"), false, null);
			}
			else if (e.HasAttribute("reward_text"))
			{
				this.RewardText = e.GetAttribute("reward_text");
			}
			if (e.HasAttribute("objective_text_key"))
			{
				this.ObjectiveText = Localization.Get(e.GetAttribute("objective_text_key"), false, null);
			}
			else if (e.HasAttribute("objective_text"))
			{
				this.ObjectiveText = e.GetAttribute("objective_text");
			}
			if (e.HasAttribute("active_challenge_count"))
			{
				this.ActiveChallengeCount = StringParsers.ParseSInt32(e.GetAttribute("active_challenge_count"), 0, -1, NumberStyles.Integer);
			}
			if (e.HasAttribute("day_reset"))
			{
				this.DayReset = StringParsers.ParseSInt32(e.GetAttribute("day_reset"), 0, -1, NumberStyles.Integer);
			}
			if (e.HasAttribute("is_random"))
			{
				this.IsRandom = StringParsers.ParseBool(e.GetAttribute("is_random"), 0, -1, true);
			}
			if (e.HasAttribute("link_challenges"))
			{
				this.LinkChallenges = StringParsers.ParseBool(e.GetAttribute("link_challenges"), 0, -1, true);
			}
			if (e.HasAttribute("hidden_by"))
			{
				this.HiddenBy = e.GetAttribute("hidden_by");
			}
			if (e.HasAttribute("is_intro"))
			{
				this.IsIntro = StringParsers.ParseBool(e.GetAttribute("is_intro"), 0, -1, true);
			}
		}

		public bool IsActive
		{
			get
			{
				return (!this.IsIntro || ChallengeJournal.IntroChallengesEnabled) && ChallengeCategory.s_ChallengeCategories[this.Category].IsActive;
			}
		}

		public bool IsVisible(EntityPlayer player)
		{
			if (!ChallengeJournal.IntroChallengesEnabled && this.IsIntro)
			{
				return false;
			}
			if (this.HiddenBy == "")
			{
				return true;
			}
			if (!ChallengeCategory.s_ChallengeCategories[this.Category].CanShow(player))
			{
				return false;
			}
			if (ChallengeGroup.s_ChallengeGroups.ContainsKey(this.HiddenBy))
			{
				ChallengeGroup challengeGroup = ChallengeGroup.s_ChallengeGroups[this.HiddenBy];
				if (challengeGroup.IsActive)
				{
					return challengeGroup.IsComplete;
				}
			}
			return true;
		}

		public bool HasEventsOrPassives()
		{
			return this.Effects != null;
		}

		public void ModifyValue(EntityAlive _ea, PassiveEffects _effect, ref float _base_value, ref float _perc_value, FastTags<TagGroup.Global> _tags = default(FastTags<TagGroup.Global>))
		{
			if (this.Effects == null || !this.IsComplete)
			{
				return;
			}
			this.Effects.ModifyValue(_ea, _effect, ref _base_value, ref _perc_value, 0f, _tags, 1);
		}

		public List<ChallengeClass> GetChallengeClassesForCreate()
		{
			List<ChallengeClass> list = new List<ChallengeClass>();
			for (int i = 0; i < this.ChallengeClasses.Count; i++)
			{
				list.Add(this.ChallengeClasses[i]);
			}
			GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
			for (int j = 0; j < list.Count * 2; j++)
			{
				int num = gameRandom.RandomRange(list.Count);
				int num2 = gameRandom.RandomRange(list.Count);
				ChallengeClass challengeClass = list[num];
				list[num] = list[num2];
				list[num2] = challengeClass;
			}
			if (this.ChallengeCounts != null)
			{
				for (int k = 0; k < this.ChallengeCounts.Count; k++)
				{
					ChallengeGroup.ChallengeCount challengeCount = this.ChallengeCounts[k];
					int num3 = challengeCount.Count;
					for (int l = list.Count - 1; l >= 0; l--)
					{
						if (list[l].Tags.Test_AnySet(challengeCount.Tags))
						{
							if (num3 == 0)
							{
								list.RemoveAt(l);
							}
							else
							{
								num3--;
							}
						}
					}
				}
				list = list.OrderBy((ChallengeClass c) => c.TagName).ToList<ChallengeClass>();
			}
			return list;
		}

		public static Dictionary<string, ChallengeGroup> s_ChallengeGroups = new CaseInsensitiveStringDictionary<ChallengeGroup>();

		public string Name;

		public string Title;

		public bool IsComplete;

		public string RewardEvent;

		public string RewardText;

		public string ObjectiveText;

		public bool IsRandom;

		public bool IsIntro;

		public int ActiveChallengeCount = 10;

		public int DayReset = -1;

		public bool LinkChallenges;

		public string Category;

		public string HiddenBy = "";

		public bool UIDirty;

		public List<ChallengeGroup.ChallengeCount> ChallengeCounts;

		public List<ChallengeClass> ChallengeClasses = new List<ChallengeClass>();

		public MinEffectController Effects;

		public class ChallengeCount
		{
			public FastTags<TagGroup.Global> Tags;

			public int Count;
		}
	}
}
