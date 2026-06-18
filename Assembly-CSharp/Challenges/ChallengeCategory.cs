using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Challenges
{
	public class ChallengeCategory
	{
		public ChallengeCategory(string name)
		{
			this.Name = name;
		}

		public bool IsActive
		{
			get
			{
				if (this.showType == ChallengeCategory.ShowTypes.BiomeProgression)
				{
					return World.BiomeProgressionEnabled;
				}
				return ChallengeJournal.AllowChallenges;
			}
		}

		public bool CanShow(EntityPlayer player)
		{
			if (this.showType == ChallengeCategory.ShowTypes.Twitch)
			{
				return player.TwitchEnabled;
			}
			if (this.showType == ChallengeCategory.ShowTypes.CVar)
			{
				return player.Buffs.GetCustomVar(this.showValue) > 0f;
			}
			if (this.showType == ChallengeCategory.ShowTypes.BiomeProgression)
			{
				return World.BiomeProgressionEnabled;
			}
			return ChallengeJournal.AllowChallenges;
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
			if (e.HasAttribute("icon"))
			{
				this.Icon = e.GetAttribute("icon");
			}
			if (e.HasAttribute("show_type"))
			{
				this.showType = (ChallengeCategory.ShowTypes)Enum.Parse(typeof(ChallengeCategory.ShowTypes), e.GetAttribute("show_type"), true);
			}
			if (e.HasAttribute("show_value"))
			{
				this.showValue = e.GetAttribute("show_value");
			}
			if (e.HasAttribute("display_key"))
			{
				this.DisplayKey = e.GetAttribute("display_key");
			}
		}

		public static Dictionary<string, ChallengeCategory> s_ChallengeCategories = new CaseInsensitiveStringDictionary<ChallengeCategory>();

		public string Name;

		public string Icon;

		public string Title;

		public string DisplayKey = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public ChallengeCategory.ShowTypes showType;

		[PublicizedFrom(EAccessModifier.Private)]
		public string showValue = "";

		public enum ShowTypes
		{
			Normal,
			CVar,
			Twitch,
			BiomeProgression
		}
	}
}
