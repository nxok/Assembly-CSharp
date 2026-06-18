using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveEnterBiome : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.EnterBiome;
			}
		}

		public override string DescriptionText
		{
			get
			{
				return Localization.Get("challengeObjectiveEnter", false, null) + " " + Localization.Get("biome_" + this.biome, false, null) + ":";
			}
		}

		public override void Init()
		{
		}

		public override void HandleAddHooks()
		{
			QuestEventManager.Current.BiomeEnter += this.Current_BiomeEnter;
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.BiomeEnter -= this.Current_BiomeEnter;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_BiomeEnter(BiomeDefinition biomeDef)
		{
			if (biomeDef != null && biomeDef.m_sBiomeName == this.biome)
			{
				int num = base.Current;
				base.Current = num + 1;
				if (base.Current >= this.MaxCount)
				{
					base.Current = this.MaxCount;
					this.CheckObjectiveComplete(true);
				}
			}
		}

		public override void ParseElement(XElement e)
		{
			base.ParseElement(e);
			if (e.HasAttribute("biome"))
			{
				this.biome = e.GetAttribute("biome");
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveEnterBiome
			{
				biome = this.biome
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string biome;
	}
}
