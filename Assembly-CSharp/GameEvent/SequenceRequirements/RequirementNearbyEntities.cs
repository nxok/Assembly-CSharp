using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements
{
	[Preserve]
	public class RequirementNearbyEntities : BaseRequirement
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
		}

		public override bool CanPerform(Entity target)
		{
			FastTags<TagGroup.Global> fastTags = ((this.tag == "") ? FastTags<TagGroup.Global>.none : FastTags<TagGroup.Global>.Parse(this.tag));
			List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(target, new Bounds(target.position, Vector3.one * 2f * this.maxDistance), this.currentState == RequirementNearbyEntities.EntityStates.Live);
			if (this.targetIsOwner)
			{
				PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(this.Owner.Target.entityId);
				for (int i = 0; i < entitiesInBounds.Count; i++)
				{
					if (entitiesInBounds[i].HasAnyTags(fastTags))
					{
						EntityVehicle entityVehicle = entitiesInBounds[i] as EntityVehicle;
						if (entityVehicle != null)
						{
							if (this.targetIsOwner && !entityVehicle.IsOwner(playerDataFromEntityID.PrimaryId))
							{
								goto IL_0106;
							}
						}
						else
						{
							EntityTurret entityTurret = entitiesInBounds[i] as EntityTurret;
							if (entityTurret == null || (this.targetIsOwner && entityTurret.OwnerID != null && !entityTurret.OwnerID.Equals(playerDataFromEntityID.PrimaryId)))
							{
								goto IL_0106;
							}
						}
						return true;
					}
					IL_0106:;
				}
			}
			else
			{
				for (int j = 0; j < entitiesInBounds.Count; j++)
				{
					Entity entity = entitiesInBounds[j];
					if (fastTags.IsEmpty)
					{
						if (entity is EntityAnimal || entity is EntityEnemy)
						{
							return true;
						}
					}
					else if (entity.HasAnyTags(fastTags))
					{
						return true;
					}
				}
			}
			return false;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(RequirementNearbyEntities.PropTag, ref this.tag);
			properties.ParseFloat(RequirementNearbyEntities.PropMaxDistance, ref this.maxDistance);
			properties.ParseEnum<RequirementNearbyEntities.EntityStates>(RequirementNearbyEntities.PropEntityState, ref this.currentState);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseRequirement CloneChildSettings()
		{
			return new RequirementNearbyEntities
			{
				maxDistance = this.maxDistance,
				tag = this.tag,
				currentState = this.currentState
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string tag = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public float maxDistance = 10f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool targetIsOwner;

		[PublicizedFrom(EAccessModifier.Protected)]
		public RequirementNearbyEntities.EntityStates currentState;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTag = "entity_tags";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropEntityState = "entity_state";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropMaxDistance = "max_distance";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTargetIsOwner = "target_is_owner";

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum EntityStates
		{
			Live,
			Dead
		}
	}
}
