using System;

namespace Twitch
{
	public class TwitchVoteRequirementHasProgression : BaseTwitchVoteOperationRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override float LeftSide(EntityPlayer player)
		{
			return (float)player.Progression.GetProgressionValue(this.SkillName).CalculatedLevel(player);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override float RightSide(EntityPlayer player)
		{
			return (float)this.Level;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool CheckPerk(EntityPlayer player, string buffName)
		{
			if (player.Progression.GetProgressionValue(buffName).CalculatedLevel(player) >= this.Level)
			{
				return !this.Invert;
			}
			return this.Invert;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(TwitchVoteRequirementHasProgression.PropSkillName, ref this.SkillName);
			properties.ParseInt(TwitchVoteRequirementHasProgression.PropLevel, ref this.Level);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string SkillName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public int Level = 1;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropSkillName = "skill_name";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropLevel = "level";
	}
}
