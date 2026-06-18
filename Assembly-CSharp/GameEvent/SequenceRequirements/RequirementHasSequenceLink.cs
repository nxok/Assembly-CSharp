using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class RequirementHasSequenceLink : BaseRequirement
	{
		public override bool CanPerform(Entity target)
		{
			if (GameEventManager.Current.HasSequenceLink(this.Owner))
			{
				return !this.Invert;
			}
			return this.Invert;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new RequirementHasSequenceLink
			{
				Invert = this.Invert
			};
		}
	}
}
