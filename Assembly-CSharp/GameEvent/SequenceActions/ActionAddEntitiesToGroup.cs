using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionAddEntitiesToGroup : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			FastTags<TagGroup.Global> fastTags = ((this.tag == "") ? FastTags<TagGroup.Global>.none : FastTags<TagGroup.Global>.Parse(this.tag));
			World world = GameManager.Instance.World;
			Vector3 vector = ((this.yHeight == -1f) ? (Vector3.one * 2f * this.maxDistance) : new Vector3(2f * this.maxDistance, this.yHeight, 2f * this.maxDistance));
			Vector3 vector2 = ((base.Owner.Target != null) ? base.Owner.Target.position : base.Owner.TargetPosition);
			if (this.yHeight != -1f)
			{
				vector2 += Vector3.one * (this.yHeight * 0.5f);
			}
			List<Entity> entitiesInBounds = world.GetEntitiesInBounds(this.excludeTarget ? base.Owner.Target : null, new Bounds(vector2, vector), this.currentState == ActionAddEntitiesToGroup.EntityStates.Live);
			List<Entity> list = new List<Entity>();
			if (this.targetIsOwner && base.Owner.Target != null)
			{
				PersistentPlayerData playerDataFromEntityID = GameManager.Instance.GetPersistentPlayerList().GetPlayerDataFromEntityID(base.Owner.Target.entityId);
				for (int i = 0; i < entitiesInBounds.Count; i++)
				{
					if (entitiesInBounds[i].HasAnyTags(fastTags))
					{
						EntityVehicle entityVehicle = entitiesInBounds[i] as EntityVehicle;
						if (entityVehicle != null)
						{
							if (entityVehicle.GetOwner() == null)
							{
								goto IL_01E2;
							}
							if (this.targetIsOwner && !entityVehicle.IsOwner(playerDataFromEntityID.PrimaryId))
							{
								goto IL_01E2;
							}
						}
						else
						{
							EntityTurret entityTurret = entitiesInBounds[i] as EntityTurret;
							if (entityTurret == null || entityTurret.OwnerID == null || (this.targetIsOwner && !entityTurret.OwnerID.Equals(playerDataFromEntityID.PrimaryId)))
							{
								goto IL_01E2;
							}
						}
						list.Add(entitiesInBounds[i]);
					}
					IL_01E2:;
				}
			}
			else
			{
				for (int j = 0; j < entitiesInBounds.Count; j++)
				{
					Entity entity = entitiesInBounds[j];
					if (fastTags.IsEmpty)
					{
						if (entity is EntityEnemyAnimal || entity is EntityEnemy || (this.allowPlayers && entity is EntityPlayer))
						{
							list.Add(entity);
						}
					}
					else if (entity.HasAnyTags(fastTags))
					{
						list.Add(entity);
					}
				}
			}
			base.Owner.AddEntitiesToGroup(this.groupName, list, this.twitchNegative);
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionAddEntitiesToGroup.PropGroupName, ref this.groupName);
			properties.ParseString(ActionAddEntitiesToGroup.PropTag, ref this.tag);
			properties.ParseFloat(ActionAddEntitiesToGroup.PropMaxDistance, ref this.maxDistance);
			properties.ParseBool(ActionAddEntitiesToGroup.PropTwitchNegative, ref this.twitchNegative);
			properties.ParseBool(ActionAddEntitiesToGroup.PropTargetIsOwner, ref this.targetIsOwner);
			properties.ParseBool(ActionAddEntitiesToGroup.PropExcludeTarget, ref this.excludeTarget);
			properties.ParseBool(ActionAddEntitiesToGroup.PropAllowPlayers, ref this.allowPlayers);
			properties.ParseFloat(ActionAddEntitiesToGroup.PropYHeight, ref this.yHeight);
			properties.ParseEnum<ActionAddEntitiesToGroup.EntityStates>(ActionAddEntitiesToGroup.PropEntityState, ref this.currentState);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionAddEntitiesToGroup
			{
				maxDistance = this.maxDistance,
				tag = this.tag,
				groupName = this.groupName,
				twitchNegative = this.twitchNegative,
				targetIsOwner = this.targetIsOwner,
				yHeight = this.yHeight,
				currentState = this.currentState,
				excludeTarget = this.excludeTarget,
				allowPlayers = this.allowPlayers
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string groupName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string tag = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public float maxDistance = 10f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool twitchNegative = true;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool targetIsOwner;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float yHeight = -1f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool excludeTarget = true;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool allowPlayers;

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionAddEntitiesToGroup.EntityStates currentState;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropGroupName = "group_name";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTag = "entity_tags";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropEntityState = "entity_state";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropMaxDistance = "max_distance";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTwitchNegative = "twitch_negative";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTargetIsOwner = "target_is_owner";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropYHeight = "y_height";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropExcludeTarget = "exclude_target";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropAllowPlayers = "allow_players";

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum EntityStates
		{
			Live,
			Dead
		}
	}
}
