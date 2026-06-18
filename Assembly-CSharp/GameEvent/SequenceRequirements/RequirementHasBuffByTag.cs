using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class RequirementHasBuffByTag : BaseRequirement
	{
		public override bool CanPerform(Entity target)
		{
			EntityAlive entityAlive = target as EntityAlive;
			if (entityAlive != null && entityAlive.Buffs.HasBuffByTag(this.buffTags))
			{
				return !this.Invert;
			}
			return this.Invert;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			if (properties.Values.ContainsKey(RequirementHasBuffByTag.PropBuffTags))
			{
				this.buffTags = FastTags<TagGroup.Global>.Parse(properties.Values[RequirementHasBuffByTag.PropBuffTags]);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new RequirementHasBuffByTag
			{
				buffTags = this.buffTags,
				Invert = this.Invert
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public FastTags<TagGroup.Global> buffTags = FastTags<TagGroup.Global>.none;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropBuffTags = "buff_tags";
	}
}
