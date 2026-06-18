using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveKillByTag : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.KillByTag;
			}
		}

		public override string DescriptionText
		{
			get
			{
				if (this.Biome == "")
				{
					return Localization.Get("challengeObjectiveKill", false, null) + " " + Localization.Get(this.targetName, false, null) + ":";
				}
				return string.Format(Localization.Get("challengeObjectiveKillIn", false, null), Localization.Get(this.targetName, false, null), Localization.Get("biome_" + this.Biome, false, null));
			}
		}

		public override void Init()
		{
			this.entityTags = FastTags<TagGroup.Global>.Parse(this.entityTag);
		}

		public override void HandleAddHooks()
		{
			if (!EntityFactory.EnemySpawnMode && this.isEnemy)
			{
				base.Current = this.MaxCount;
				base.Complete = true;
				this.Owner.HandleComplete(false);
				if (this.Owner.ChallengeState == Challenge.ChallengeStates.Completed)
				{
					this.Owner.AutoCompleted = true;
					this.Owner.ChallengeState = Challenge.ChallengeStates.Redeemed;
					return;
				}
			}
			else
			{
				QuestEventManager.Current.EntityKill -= this.Current_EntityKill;
				QuestEventManager.Current.EntityKill += this.Current_EntityKill;
			}
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.EntityKill -= this.Current_EntityKill;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_EntityKill(EntityAlive killedBy, EntityAlive killedEntity)
		{
			if (!this.entityTags.Test_AnySet(killedEntity.EntityClass.Tags))
			{
				return;
			}
			if (this.CheckBaseRequirements())
			{
				return;
			}
			if (this.isTwitchSpawn > -1)
			{
				if (this.isTwitchSpawn == 0 && killedEntity.spawnById != -1)
				{
					return;
				}
				if (this.isTwitchSpawn == 1 && killedEntity.spawnById == -1)
				{
					return;
				}
			}
			if (!this.killerHasBuffTag.IsEmpty && !killedBy.Buffs.HasBuffByTag(this.killerHasBuffTag))
			{
				return;
			}
			if (!this.killedHasBuffTag.IsEmpty && !killedEntity.Buffs.HasBuffByTag(this.killedHasBuffTag))
			{
				return;
			}
			int num = base.Current;
			base.Current = num + 1;
			this.CheckObjectiveComplete(true);
		}

		public override void ParseElement(XElement e)
		{
			base.ParseElement(e);
			if (e.HasAttribute("entity_tags"))
			{
				this.entityTag = e.GetAttribute("entity_tags");
			}
			if (e.HasAttribute("target_name_key"))
			{
				this.targetName = Localization.Get(e.GetAttribute("target_name_key"), false, null);
			}
			else if (e.HasAttribute("target_name"))
			{
				this.targetName = e.GetAttribute("target_name");
			}
			if (e.HasAttribute("is_twitch_spawn"))
			{
				this.isTwitchSpawn = (StringParsers.ParseBool(e.GetAttribute("is_twitch_spawn"), 0, -1, true) ? 1 : 0);
			}
			if (e.HasAttribute("killer_has_bufftag"))
			{
				this.killerHasBuffTag = FastTags<TagGroup.Global>.Parse(e.GetAttribute("killer_has_bufftag"));
			}
			if (e.HasAttribute("killed_has_bufftag"))
			{
				this.killedHasBuffTag = FastTags<TagGroup.Global>.Parse(e.GetAttribute("killed_has_bufftag"));
			}
			e.ParseAttribute("is_enemy", ref this.isEnemy);
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveKillByTag
			{
				entityTag = this.entityTag,
				entityTags = this.entityTags,
				Biome = this.Biome,
				isEnemy = this.isEnemy,
				targetName = this.targetName,
				isTwitchSpawn = this.isTwitchSpawn,
				killerHasBuffTag = this.killerHasBuffTag,
				killedHasBuffTag = this.killedHasBuffTag
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string entityTag = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public FastTags<TagGroup.Global> entityTags;

		[PublicizedFrom(EAccessModifier.Private)]
		public string targetName = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public int isTwitchSpawn = -1;

		[PublicizedFrom(EAccessModifier.Private)]
		public FastTags<TagGroup.Global> killerHasBuffTag;

		[PublicizedFrom(EAccessModifier.Private)]
		public FastTags<TagGroup.Global> killedHasBuffTag;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isEnemy = true;
	}
}
