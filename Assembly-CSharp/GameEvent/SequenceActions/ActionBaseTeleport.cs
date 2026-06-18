using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionBaseTeleport : ActionBaseTargetAction
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public void TeleportEntity(Entity entity, Vector3 position)
		{
			float floatValue = GameEventManager.GetFloatValue(entity as EntityAlive, this.teleportDelayText, 0.1f);
			GameManager.Instance.StartCoroutine(base.TeleportEntity(entity, position, floatValue));
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionBaseTeleport.PropTeleportDelay, ref this.teleportDelayText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return null;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string teleportDelayText = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTeleportDelay = "teleport_delay";
	}
}
