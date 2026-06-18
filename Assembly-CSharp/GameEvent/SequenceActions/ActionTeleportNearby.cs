using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionTeleportNearby : ActionBaseTeleport
	{
		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			World world = GameManager.Instance.World;
			if (base.Owner.TargetPosition != Vector3.zero)
			{
				Vector3 targetPosition = base.Owner.TargetPosition;
				base.TeleportEntity(target, targetPosition);
				return BaseAction.ActionCompleteStates.Complete;
			}
			return BaseAction.ActionCompleteStates.InComplete;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionTeleportNearby
			{
				targetGroup = this.targetGroup,
				teleportDelayText = this.teleportDelayText
			};
		}
	}
}
