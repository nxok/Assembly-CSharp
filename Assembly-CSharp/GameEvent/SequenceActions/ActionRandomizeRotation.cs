using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionRandomizeRotation : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			if (target != null)
			{
				Entity attachedToEntity = target.AttachedToEntity;
				float num = (float)GameEventManager.Current.Random.RandomRange(45, 315);
				if (attachedToEntity)
				{
					Transform physicsTransform = attachedToEntity.PhysicsTransform;
					Quaternion quaternion = physicsTransform.rotation;
					quaternion = Quaternion.AngleAxis(num, physicsTransform.up) * quaternion;
					attachedToEntity.SetRotation(quaternion.eulerAngles);
					physicsTransform.rotation = quaternion;
					EntityVehicle entityVehicle = attachedToEntity as EntityVehicle;
					if (entityVehicle)
					{
						entityVehicle.CameraChangeRotation(num);
						return;
					}
				}
				else
				{
					Vector3 rotation = target.rotation;
					rotation.y += num;
					target.SetRotation(rotation);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionRandomizeRotation
			{
				targetGroup = this.targetGroup
			};
		}
	}
}
