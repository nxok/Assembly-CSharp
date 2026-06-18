using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class RequirementIsHomerunActive : BaseRequirement
	{
		public override bool CanPerform(Entity target)
		{
			EntityPlayer entityPlayer = target as EntityPlayer;
			if (entityPlayer == null)
			{
				return false;
			}
			bool flag = GameEventManager.Current.HomerunManager.HasHomerunActive(entityPlayer);
			if (!this.Invert)
			{
				return flag;
			}
			return !flag;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new RequirementIsHomerunActive
			{
				Invert = this.Invert
			};
		}
	}
}
