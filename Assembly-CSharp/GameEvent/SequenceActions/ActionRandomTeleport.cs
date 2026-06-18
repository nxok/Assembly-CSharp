using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionRandomTeleport : ActionBaseTeleport
	{
		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			EntityPlayer entityPlayer = target as EntityPlayer;
			if (!(entityPlayer != null))
			{
				return BaseAction.ActionCompleteStates.Complete;
			}
			float num = GameManager.Instance.World.RandomRange(this.minDistance, this.maxDistance);
			this.position = ObjectiveRandomGoto.CalculateRandomPoint(entityPlayer.entityId, num, "", true, BiomeFilterTypes.SameBiome, "");
			if (this.position.y >= 0)
			{
				Vector3 vector = this.position.ToVector3();
				vector.y = -2000f;
				base.TeleportEntity(entityPlayer, vector);
				return BaseAction.ActionCompleteStates.Complete;
			}
			this.maxTries--;
			if (this.maxTries != 0)
			{
				return BaseAction.ActionCompleteStates.InComplete;
			}
			return BaseAction.ActionCompleteStates.InCompleteRefund;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseFloat(ActionRandomTeleport.PropMinDistance, ref this.minDistance);
			properties.ParseFloat(ActionRandomTeleport.PropMaxDistance, ref this.maxDistance);
			properties.ParseInt(ActionRandomTeleport.PropMaxTries, ref this.maxTries);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionRandomTeleport
			{
				targetGroup = this.targetGroup,
				minDistance = this.minDistance,
				maxDistance = this.maxDistance,
				maxTries = this.maxTries,
				teleportDelayText = this.teleportDelayText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public float minDistance = 100f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float maxDistance = 200f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int maxTries = 20;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropMinDistance = "min_distance";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropMaxDistance = "max_distance";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropMaxTries = "max_tries";

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3i position;
	}
}
