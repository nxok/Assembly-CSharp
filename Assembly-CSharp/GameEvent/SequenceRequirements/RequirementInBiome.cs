using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class RequirementInBiome : BaseRequirement
	{
		public override bool CanPerform(Entity target)
		{
			EntityAlive entityAlive = target as EntityAlive;
			if (entityAlive == null)
			{
				return false;
			}
			bool flag = this.biomeList.ContainsCaseInsensitive(entityAlive.biomeStandingOn.m_sBiomeName);
			if (!this.Invert)
			{
				return flag;
			}
			return !flag;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(RequirementInBiome.PropBiome, ref this.biomes);
			if (!string.IsNullOrEmpty(this.biomes))
			{
				this.biomeList = this.biomes.Split(',', StringSplitOptions.None);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new RequirementInBiome
			{
				Invert = this.Invert,
				biomeList = this.biomeList
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string biomes;

		[PublicizedFrom(EAccessModifier.Private)]
		public string[] biomeList;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropBiome = "biomes";
	}
}
