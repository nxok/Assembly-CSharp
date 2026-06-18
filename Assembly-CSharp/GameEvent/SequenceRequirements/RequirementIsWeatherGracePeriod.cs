using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class RequirementIsWeatherGracePeriod : BaseRequirement
	{
		public override bool CanPerform(Entity target)
		{
			bool flag = GameManager.Instance.World.GetWorldTime() <= 30000UL;
			if (!this.Invert)
			{
				return flag;
			}
			return !flag;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new RequirementIsWeatherGracePeriod
			{
				Invert = this.Invert
			};
		}
	}
}
