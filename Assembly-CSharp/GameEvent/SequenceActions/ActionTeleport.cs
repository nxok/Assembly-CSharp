using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionTeleport : ActionBaseTeleport
	{
		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			World world = GameManager.Instance.World;
			Vector3 vector = Vector3.zero;
			switch (this.offsetType)
			{
			case ActionTeleport.OffsetTypes.None:
				vector = this.target_position;
				break;
			case ActionTeleport.OffsetTypes.Relative:
				vector = target.position + target.transform.TransformDirection(this.target_position);
				break;
			case ActionTeleport.OffsetTypes.World:
				vector = target.position + this.target_position;
				break;
			}
			if (vector.y > 0f)
			{
				base.TeleportEntity(target, vector);
				return BaseAction.ActionCompleteStates.Complete;
			}
			return BaseAction.ActionCompleteStates.InComplete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseVec(ActionTeleport.PropTargetPosition, ref this.target_position);
			properties.ParseEnum<ActionTeleport.OffsetTypes>(ActionTeleport.PropOffsetType, ref this.offsetType);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionTeleport
			{
				targetGroup = this.targetGroup,
				target_position = this.target_position,
				offsetType = this.offsetType,
				teleportDelayText = this.teleportDelayText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public Vector3 target_position;

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionTeleport.OffsetTypes offsetType;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTargetPosition = "target_position";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropOffsetType = "offset_type";

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum OffsetTypes
		{
			None,
			Relative,
			World
		}
	}
}
