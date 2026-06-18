using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class RequirementHasParty : BaseRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
		}

		public override bool CanPerform(Entity target)
		{
			EntityPlayer entityPlayer = target as EntityPlayer;
			if (entityPlayer != null && entityPlayer.Party != null)
			{
				return !this.Invert;
			}
			return this.Invert;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new RequirementHasParty();
		}
	}
}
