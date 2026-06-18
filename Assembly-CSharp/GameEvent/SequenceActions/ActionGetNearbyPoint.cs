using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionGetNearbyPoint : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			Vector3 zero = Vector3.zero;
			EntityAlive entityAlive = base.Owner.Target as EntityAlive;
			if (this.targetType == ActionGetNearbyPoint.TargetTypes.TargetGroup_Random && this.targetGroup != "")
			{
				entityAlive = base.Owner.GetEntityGroup(this.targetGroup).RandomObject<Entity>() as EntityAlive;
			}
			if (ActionBaseSpawn.FindValidPosition(out zero, entityAlive, this.minDistance, this.maxDistance, this.safeSpawn, this.yOffset, this.airSpawn))
			{
				base.Owner.TargetPosition = zero;
				return BaseAction.ActionCompleteStates.Complete;
			}
			return BaseAction.ActionCompleteStates.InComplete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseFloat(ActionGetNearbyPoint.PropMinDistance, ref this.minDistance);
			properties.ParseFloat(ActionGetNearbyPoint.PropMaxDistance, ref this.maxDistance);
			properties.ParseBool(ActionGetNearbyPoint.PropSpawnInSafe, ref this.safeSpawn);
			properties.ParseBool(ActionGetNearbyPoint.PropSpawnInAir, ref this.airSpawn);
			properties.ParseFloat(ActionGetNearbyPoint.PropYOffset, ref this.yOffset);
			properties.ParseString(ActionGetNearbyPoint.PropTargetGroup, ref this.targetGroup);
			properties.ParseEnum<ActionGetNearbyPoint.TargetTypes>(ActionGetNearbyPoint.PropTargetType, ref this.targetType);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionGetNearbyPoint
			{
				minDistance = this.minDistance,
				maxDistance = this.maxDistance,
				safeSpawn = this.safeSpawn,
				airSpawn = this.airSpawn,
				yOffset = this.yOffset,
				targetGroup = this.targetGroup,
				targetType = this.targetType
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
		public string targetGroup = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionGetNearbyPoint.TargetTypes targetType;

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
		public static string PropTargetGroup = "target_group";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTargetType = "target_type";

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum TargetTypes
		{
			Target,
			TargetGroup_Random
		}
	}
}
