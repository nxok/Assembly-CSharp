using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class RequirementInVehicle : BaseRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
		}

		public override bool CanPerform(Entity target)
		{
			if (target == null)
			{
				return false;
			}
			if (target.AttachedToEntity)
			{
				return !this.Invert;
			}
			return this.Invert;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new RequirementInVehicle
			{
				Invert = this.Invert
			};
		}
	}
}
