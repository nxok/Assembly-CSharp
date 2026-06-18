using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionRespawnEntities : BaseAction
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnInit()
		{
			base.OnInit();
			this.AddToGroups = this.addToGroup.Split(',', StringSplitOptions.None);
		}

		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			if (this.entityList == null)
			{
				this.entityList = new List<EntityAlive>();
				List<Entity> entityGroup = base.Owner.GetEntityGroup(this.targetGroup);
				if (entityGroup == null)
				{
					Debug.LogWarning("ActionReviveEntities: Target Group " + this.targetGroup + " Does not exist!");
					return BaseAction.ActionCompleteStates.InCompleteRefund;
				}
				for (int i = 0; i < entityGroup.Count; i++)
				{
					EntityAlive entityAlive = entityGroup[i] as EntityAlive;
					if (entityAlive != null)
					{
						this.entityList.Add(entityAlive);
					}
				}
			}
			else if (this.entityList.Count > 0)
			{
				this.checkTime -= Time.deltaTime;
				if (this.checkTime > 0f)
				{
					return BaseAction.ActionCompleteStates.Complete;
				}
				World world = GameManager.Instance.World;
				for (int j = 0; j < this.entityList.Count; j++)
				{
					if (this.entityList[j] != null && !this.entityList[j].IsAlive())
					{
						Entity entity = this.entityList[j];
						Entity entity2 = EntityFactory.CreateEntity(this.entityList[j].entityClass, entity.position, entity.rotation, base.Owner.Target.entityId, base.Owner.ExtraData);
						entity2.SetSpawnerSource(EnumSpawnerSource.Dynamic);
						world.SpawnEntityInWorld(entity2);
						world.RemoveEntity(entity.entityId, EnumRemoveEntityReason.Killed);
						EntityAlive entityAlive2 = entity2 as EntityAlive;
						GameEventManager.Current.RegisterSpawnedEntity(entity2 as EntityAlive, base.Owner.Target, base.Owner.Requester, base.Owner, true);
						if (base.Owner.Requester != null)
						{
							if (base.Owner.Requester is EntityPlayerLocal)
							{
								GameEventManager.Current.HandleGameEntitySpawned(base.Owner.Name, entityAlive2.entityId, base.Owner.Tag);
							}
							else
							{
								SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(base.Owner.Name, base.Owner.Target.entityId, base.Owner.ExtraData, base.Owner.Tag, NetPackageGameEventResponse.ResponseTypes.TwitchSetOwner, entityAlive2.entityId, -1, false, ""), false, base.Owner.Requester.entityId, -1, -1, null, 192, false);
							}
						}
						if (this.respawnSound != "")
						{
							Manager.BroadcastPlayByLocalPlayer(entity.position, this.respawnSound);
						}
						if (entity2 != null && this.AddToGroups != null)
						{
							for (int k = 0; k < this.AddToGroups.Length; k++)
							{
								if (this.AddToGroups[k] != "")
								{
									base.Owner.AddEntityToGroup(this.AddToGroups[k], entity2);
								}
							}
						}
						this.entityList.RemoveAt(j);
						return BaseAction.ActionCompleteStates.InComplete;
					}
				}
			}
			return BaseAction.ActionCompleteStates.InComplete;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnReset()
		{
			this.entityList = null;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionRespawnEntities.PropAddToGroup, ref this.addToGroup);
			properties.ParseString(ActionRespawnEntities.PropTargetGroup, ref this.targetGroup);
			properties.ParseString(ActionRespawnEntities.PropRespawnSound, ref this.respawnSound);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionRespawnEntities
			{
				targetGroup = this.targetGroup,
				addToGroup = this.addToGroup,
				respawnSound = this.respawnSound
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string targetGroup = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string addToGroup = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string[] AddToGroups;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string respawnSound = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTargetGroup = "target_group";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropAddToGroup = "add_to_group";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropIsMulti = "is_multi";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropRespawnSound = "respawn_sound";

		[PublicizedFrom(EAccessModifier.Private)]
		public List<EntityAlive> entityList;

		[PublicizedFrom(EAccessModifier.Private)]
		public float checkTime = 1f;
	}
}
