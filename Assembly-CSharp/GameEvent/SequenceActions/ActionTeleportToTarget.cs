using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionTeleportToTarget : ActionBaseTeleport
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			if (this.targetGroup != "")
			{
				List<Entity> entityGroup = base.Owner.GetEntityGroup(this.targetGroup);
				BaseAction.ActionCompleteStates actionCompleteStates = BaseAction.ActionCompleteStates.InComplete;
				for (int i = 0; i < entityGroup.Count; i++)
				{
					actionCompleteStates = this.HandleTeleportToTarget(entityGroup[i]);
					if (actionCompleteStates == BaseAction.ActionCompleteStates.InCompleteRefund)
					{
						return actionCompleteStates;
					}
				}
				return actionCompleteStates;
			}
			return this.HandleTeleportToTarget(base.Owner.Target);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public BaseAction.ActionCompleteStates HandleTeleportToTarget(Entity target)
		{
			Vector3 zero = Vector3.zero;
			Entity entity;
			if (this.teleportToGroup != "")
			{
				entity = base.Owner.GetEntityGroup(this.teleportToGroup).RandomObject<Entity>() as EntityAlive;
			}
			else
			{
				entity = base.Owner.Target;
			}
			if (entity == target)
			{
				return BaseAction.ActionCompleteStates.InComplete;
			}
			if (ActionBaseSpawn.FindValidPosition(out zero, entity, this.minDistance, this.maxDistance, this.safeSpawn, this.yOffset, this.airSpawn))
			{
				base.TeleportEntity(target, zero);
				return BaseAction.ActionCompleteStates.Complete;
			}
			return BaseAction.ActionCompleteStates.InComplete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseFloat(ActionTeleportToTarget.PropMinDistance, ref this.minDistance);
			properties.ParseFloat(ActionTeleportToTarget.PropMaxDistance, ref this.maxDistance);
			properties.ParseBool(ActionTeleportToTarget.PropSpawnInSafe, ref this.safeSpawn);
			properties.ParseBool(ActionTeleportToTarget.PropSpawnInAir, ref this.airSpawn);
			properties.ParseFloat(ActionTeleportToTarget.PropYOffset, ref this.yOffset);
			properties.ParseString(ActionBaseTargetAction.PropTargetGroup, ref this.targetGroup);
			properties.ParseString(ActionTeleportToTarget.PropTeleportToGroup, ref this.teleportToGroup);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionTeleportToTarget
			{
				minDistance = this.minDistance,
				maxDistance = this.maxDistance,
				safeSpawn = this.safeSpawn,
				airSpawn = this.airSpawn,
				yOffset = this.yOffset,
				targetGroup = this.targetGroup,
				teleportToGroup = this.teleportToGroup,
				teleportDelayText = this.teleportDelayText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public float minDistance = 8f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float maxDistance = 12f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool safeSpawn;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool airSpawn;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float yOffset;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string teleportToGroup = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionTeleportToTarget.TargetTypes targetType;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropMinDistance = "min_distance";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropMaxDistance = "max_distance";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropSpawnInSafe = "safe_spawn";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropSpawnInAir = "air_spawn";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropYOffset = "yoffset";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTeleportToGroup = "teleport_to_group";

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum TargetTypes
		{
			Target,
			TargetGroup_Random
		}
	}
}
