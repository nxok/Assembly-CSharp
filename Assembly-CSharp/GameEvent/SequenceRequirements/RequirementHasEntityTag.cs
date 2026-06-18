using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class RequirementHasEntityTag : BaseRequirement
	{
		public override bool CanPerform(Entity target)
		{
			FastTags<TagGroup.Global> fastTags = ((this.tag == "") ? FastTags<TagGroup.Global>.none : FastTags<TagGroup.Global>.Parse(this.tag));
			if (!(target is EntityAlive))
			{
				return false;
			}
			if (target.HasAnyTags(fastTags))
			{
				return !this.Invert;
			}
			return this.Invert;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(RequirementHasEntityTag.PropTag, ref this.tag);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new RequirementHasEntityTag
			{
				tag = this.tag,
				Invert = this.Invert
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string tag = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTag = "entity_tags";
	}
}
